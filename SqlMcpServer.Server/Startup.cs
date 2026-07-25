using Microsoft.Extensions.Logging;
using SqlMcpServer.Server.Models;
using SqlMcpServer.Server.Services;
using SqlMcpServer.Server.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlMcpServer.Server
{
    /// <summary>
    /// The Startup class is responsible for initializing services and running the main loop 
    /// of the MCP server, which processes JSON-RPC messages from stdin and writes responses to stdout.
    /// </summary>
    public class Startup
    {
        // Services
        private readonly ILogger<Startup>  _logger;
        private readonly IDatabaseService  _database;
        private readonly McpMessageHandler _handler;

        /// <summary>
        /// The constructor for the Startup class, which initializes the logger service.
        /// </summary>
        /// <param name="logger">The logger service used to record diagnostic information.</param>
        /// <param name="database">The database service used to execute MCP tool queries.</param>
        /// <param name="handler">The MCP message handler used to process incoming requests.</param>
        public Startup(ILogger<Startup> logger, IDatabaseService database, McpMessageHandler handler) 
        {
            _logger   = logger;
            _database = database;
            _handler  = handler;
        }

        /// <summary>
        /// Starts the MCP server by entering a loop that reads JSON-RPC messages from 
        /// stdin, processes them, and writes responses to stdout.
        /// </summary>
        public async Task Run()
        {
            // MCP servers communicate over stdio: the host (e.g. Cursor) spawns this process and
            // sends one JSON-RPC message per line on stdin; we write one JSON-RPC response per line on stdout.
            // Fatal startup errors exit with code 1 so the host can detect a failed launch.
            try
            {
                using var cts = new CancellationTokenSource();

                _logger.LogInformation("Application starting...");

                // Fail fast before entering the loop if SQL Server is unreachable or misconfigured.
                var con = await _database.ValidateConnectionAsync(cts.Token);
                if(!con)
                {
                    _logger.LogError("Database connection validation failed.");
                    Environment.Exit(1);
                }

                _logger.LogInformation("Database connected; entering MCP stdio loop.");

                // Omit null properties from responses (e.g. no "error" field on success) per JSON-RPC conventions.
                var serializerOptions = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                // Process messages until the host closes stdin or kills the process.
                while (true)
                {
                    var line = Console.ReadLine();

                    // Blank lines are ignored; the host should send valid JSON-RPC payloads only.
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        // Deserialize the incoming JSON-RPC request
                        var request = JsonSerializer.Deserialize<JsonRpcRequest>(line);

                        // Malformed JSON or empty object: skip without replying (no id to correlate an error to).
                        if (request is null)
                            continue;

                        // Handle the request and get a response
                        var response = await _handler.HandleAsync(request);

                        // Notifications (methods starting with "notifications/") do not get a response.
                        if (response is null)
                            continue;

                        var json = JsonSerializer.Serialize(response, serializerOptions);

                        // Each response must be a single line so the host can frame messages on stdout.
                        Console.WriteLine(json);
                    }
                    catch (Exception ex)
                    {
                        // Log and continue: one bad message must not tear down the whole server session.
                        _logger.LogError(ex, "Error processing message.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Startup failures (missing env var, DB unreachable, etc.) are logged and reported via exit code.
                _logger.LogError(ex, "Startup failure.");
                Environment.Exit(1);
            }
        }
    }
}
