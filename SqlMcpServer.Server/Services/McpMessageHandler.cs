using Microsoft.Extensions.Logging;
using SqlMcpServer.Server.Models;
using SqlMcpServer.Server.Services.Interfaces;
using SqlMcpServer.Server.Utils;

namespace SqlMcpServer.Server.Services;

/// <summary>
/// Dispatches incoming JSON-RPC MCP requests to protocol handlers and database tools.
/// </summary>
/// <remarks>
/// <para>Protocol methods: <c>initialize</c>, <c>ping</c>, <c>tools/list</c>, <c>tools/call</c>.
/// Notifications (<c>notifications/*</c>) are ignored.</para>
/// <para>Tools delegate to <see cref="IDatabaseService"/>:</para>
/// <list type="bullet">
/// <item><description><c>list_tables</c>           - user base tables (<see cref="IDatabaseService.GetTablesAsync"/>)</description></item>
/// <item><description><c>list_views</c>            - views (<see cref="IDatabaseService.GetViewsAsync"/>)</description></item>
/// <item><description><c>list_procedures</c>       - stored procedures (<see cref="IDatabaseService.GetProceduresAsync"/>)</description></item>
/// <item><description><c>list_triggers</c>         - DML and DDL triggers (<see cref="IDatabaseService.GetTriggersAsync"/>)</description></item>
/// <item><description><c>describe_table</c>        - column metadata (<see cref="IDatabaseService.DescribeTableAsync"/>)</description></item>
/// <item><description><c>get_object_definition</c> - T-SQL source (<see cref="IDatabaseService.GetObjectDefinitionAsync"/>)</description></item>
/// <item><description><c>search_definitions</c>    - find objects by text in module body (<see cref="IDatabaseService.SearchObjectDefinitionsAsync"/>)</description></item>
/// <item><description><c>find_references</c>       - objects that reference a table or view (<see cref="IDatabaseService.GetObjectReferencesAsync"/>)</description></item>
/// <item><description><c>execute_read_query</c>    - execute read-only SQL (<see cref="IDatabaseService.ExecuteReadQueryAsync"/>)</description></item>
/// </list>
/// </remarks>
public sealed class McpMessageHandler
{
    // Service fields
    private readonly IDatabaseService           _database;
    private readonly ILogger<McpMessageHandler> _logger;

    /// <summary>
    /// Initializes the handler with dependencies to run MCP tools.
    /// </summary>
    /// <param name="database">The database service used to execute MCP tool queries.</param>
    /// <param name="logger">The logger used to record diagnostic information.</param>
    public McpMessageHandler(IDatabaseService database, ILogger<McpMessageHandler> logger)
    {
        _database = database;
        _logger   = logger;
    }

    /// <summary>
    /// Handles a single JSON-RPC request and returns a response, or <see langword="null"/> for notifications.
    /// </summary>
    /// <param name="request">The deserialized JSON-RPC request.</param>
    /// <returns>
    /// A JSON-RPC response, or <see langword="null"/> when the request is a notification.
    /// </returns>
    public async Task<JsonRpcResponse?> HandleAsync(JsonRpcRequest request)
    {
        if (request.Method.StartsWith("notifications/", StringComparison.Ordinal))
        {
            _logger.LogInformation($"Ignoring notification: {request.Method}");
            return null;
        }

        return request.Method switch
        {
            "initialize" => HandleInitialize(request),          // id: 1
            "ping"       => HandlePing(request),                // id: 2
            "tools/list" => HandleToolsList(request),           // id: 3
            "tools/call" => await HandleToolCallAsync(request), // id: 4+

            _            => Error(JsonHelper.ConvertId(request.Id), ErrorCodes.MethodNotFound, $"Method '{request.Method}' not found")
        };
    }

    /// <summary>
    /// Responds to the MCP <c>ping</c> method with an empty result.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>
    /// A JSON-RPC success response.
    /// </returns>
    private static JsonRpcResponse HandlePing(JsonRpcRequest request)
    {
        return new JsonRpcResponse
        {
            Id     = JsonHelper.ConvertId(request.Id),
            Result = new { }
        };
    }

    /// <summary>
    /// Responds to the MCP <c>initialize</c> handshake with protocol version, capabilities, and server info.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>
    /// A JSON-RPC success response.
    /// </returns>
    private static JsonRpcResponse HandleInitialize(JsonRpcRequest request)
    {
        return new JsonRpcResponse
        {
            Id     = JsonHelper.ConvertId(request.Id),
            Result = new
            {
                protocolVersion = "2025-11-25",
                capabilities    = new
                {
                    tools = new { }
                },
                serverInfo = new
                {
                    name    = "SqlMcpServer",
                    version = "1.0.0"
                }
            }
        };
    }

    /// <summary>
    /// Handles <c>tools/list</c> and returns metadata for every MCP tool this server exposes.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>
    /// A JSON-RPC success response whose <c>result.tools</c> array describes each tool name, description, and input schema.
    /// </returns>
    private static JsonRpcResponse HandleToolsList(JsonRpcRequest request)
    {
        return new JsonRpcResponse
        {
            Id     = JsonHelper.ConvertId(request.Id),
            Result = new
            {
                tools = new object[]
                {
                    new
                    {
                        name        = "list_tables",
                        description = "Lists user base tables as schema.table (excludes system and temporal history tables).",
                        inputSchema = new
                        {
                            type       = "object",
                            properties = new { }
                        }
                    },
                    new
                    {
                        name        = "describe_table",
                        description = "Returns column name, data type, nullability, and max length for a table (schema.table or table name).",
                        inputSchema = new
                        {
                            type       = "object",
                            properties = new
                            {
                                tableName = new
                                {
                                    type = "string"
                                }
                            },
                            required = new[] { "tableName" }
                        }
                    },
                    new
                    {
                        name        = "list_views",
                        description = "Lists views as schema.view",
                        inputSchema = new
                        {
                            type       = "object",
                            properties = new { }
                        }
                    },
                    new
                    {
                        name        = "list_procedures",
                        description = "Lists user stored procedures as schema.procedure (excludes system and numbered variants).",
                        inputSchema = new
                        {
                            type       = "object",
                            properties = new { }
                        }
                    },
                    new
                    {
                        name        = "list_triggers",
                        description = "Lists triggers as parent.trigger.isDisabled (parent is schema.table or (database)).",
                        inputSchema = new
                        {
                            type       = "object",
                            properties = new { }
                        }
                    },
                    new
                    {
                        name        = "list_functions",
                        description = "Lists user functions as schema.function (excludes system and numbered variants).",
                        inputSchema = new
                        {
                            type       = "object",
                            properties = new { }
                        }
                    },
                    new
                    {
                        name        = "get_object_definition",
                        description = "Get SQL definition for procedure, view, trigger or function.",
                        inputSchema = new
                        {
                            type       = "object",
                            properties = new
                            {
                                objectName = new
                                {
                                    type = "string"
                                }
                            },
                            required = new[] 
                            { 
                                "objectName" 
                            }
                        }
                    },
                    new
                    {
                        name        = "search_definitions",
                        description = "Find procedures, views, functions, and triggers whose definition contains the given text.",
                        inputSchema = new
                        {
                            type       = "object",
                            properties = new
                            {
                                text = new
                                {
                                    type = "string"
                                }
                            },
                            required = new[] 
                            { 
                                "text" 
                            }
                        }
                    },
                    new
                    {
                        name        = "find_references",
                        description = "List procedures, views, and functions that reference a table or view by name.",
                        inputSchema = new
                        {
                            type       = "object",
                            properties = new
                            {
                                objectName = new
                                {
                                    type = "string"
                                }
                            },
                            required = new[] 
                            { 
                                "objectName" 
                            }
                        }
                    },
                    new
                    {
                        name        = "execute_read_query",
                        description = "Execute read-only SQL queries.",
                        inputSchema = new
                        {
                            type       = "object",
                            properties = new
                            {
                                sql = new
                                {
                                    type = "string"
                                }
                            },
                            required = new[] 
                            { 
                                "sql" 
                            }
                        }
                    }
                }
            }
        };
    }

    /// <summary>
    /// Handles <c>tools/call</c> by running the tool named in <c>params.name</c> and returning MCP text content.
    /// </summary>
    /// <param name="request">The incoming request. Tool arguments are in <c>params.arguments</c> (for example, <c>tableName</c> for <c>describe_table</c>).</param>
    /// <returns>
    /// A JSON-RPC success response with a text content block, or <see cref="ErrorCodes.InvalidParams"/> when the tool or required arguments are missing.
    /// </returns>
    private async Task<JsonRpcResponse> HandleToolCallAsync(JsonRpcRequest request)
    {
        using var cts = new CancellationTokenSource();

        var tool = request.Params.TryGetProperty("name", out var nameProp)
            ? nameProp.GetString()
            : null;

        if (string.IsNullOrEmpty(tool))
            return Error(JsonHelper.ConvertId(request.Id), ErrorCodes.InvalidParams, "Missing tool name");

        switch (tool)
        {
            case "list_tables"          :
                return Success(JsonHelper.ConvertId(request.Id), await _database.GetTablesAsync(cts.Token));

            case "list_views"           :
                return Success(JsonHelper.ConvertId(request.Id), await _database.GetViewsAsync(cts.Token));

            case "list_procedures"      :
                return Success(JsonHelper.ConvertId(request.Id), await _database.GetProceduresAsync(cts.Token));

            case "list_triggers"        :
                return Success(JsonHelper.ConvertId(request.Id), await _database.GetTriggersAsync(cts.Token));

            case "list_functions":
                return Success(JsonHelper.ConvertId(request.Id), await _database.GetFunctionsAsync(cts.Token));

            case "describe_table"       :
                {
                    if (!request.Params.TryGetProperty("arguments", out var arguments) || !arguments.TryGetProperty("tableName", out var tableNameProp))
                    {
                        return Error(JsonHelper.ConvertId(request.Id), ErrorCodes.InvalidParams, "Missing required argument 'tableName'");
                    }

                    var tableName = tableNameProp.GetString();
                    if (string.IsNullOrWhiteSpace(tableName))
                        return Error(JsonHelper.ConvertId(request.Id), ErrorCodes.InvalidParams, "Missing required argument 'tableName'");

                    return Success(JsonHelper.ConvertId(request.Id), await _database.DescribeTableAsync(tableName, cts.Token));
                }

            case "get_object_definition":
                {
                    if (!request.Params.TryGetProperty("arguments", out var arguments) || !arguments.TryGetProperty("objectName", out var objectNameProp))
                    {
                        return Error(JsonHelper.ConvertId(request.Id), ErrorCodes.InvalidParams, "Missing required argument 'objectName'");
                    }

                    var objectName = objectNameProp.GetString();
                    if (string.IsNullOrWhiteSpace(objectName))
                        return Error(JsonHelper.ConvertId(request.Id), ErrorCodes.InvalidParams, "Missing required argument 'objectName'");

                    return Success(JsonHelper.ConvertId(request.Id), await _database.GetObjectDefinitionAsync(objectName, cts.Token));
                }

            case "search_definitions"   :
                {
                    if (!request.Params.TryGetProperty("arguments", out var arguments) || !arguments.TryGetProperty("text", out var textProp))
                    {
                        return Error(JsonHelper.ConvertId(request.Id), ErrorCodes.InvalidParams, "Missing required argument 'text'");
                    }

                    var text = textProp.GetString();
                    if (string.IsNullOrWhiteSpace(text))
                        return Error(JsonHelper.ConvertId(request.Id), ErrorCodes.InvalidParams, "Missing required argument 'text'");

                    return Success(JsonHelper.ConvertId(request.Id), await _database.SearchObjectDefinitionsAsync(text, cts.Token));
                }

            case "find_references"      :
                {
                    if (!request.Params.TryGetProperty("arguments", out var arguments) || !arguments.TryGetProperty("objectName", out var objectNameProp))
                    {
                        return Error(JsonHelper.ConvertId(request.Id), ErrorCodes.InvalidParams, "Missing required argument 'objectName'");
                    }

                    var objectName = objectNameProp.GetString();
                    if (string.IsNullOrWhiteSpace(objectName))
                        return Error(JsonHelper.ConvertId(request.Id), ErrorCodes.InvalidParams, "Missing required argument 'objectName'");

                    return Success(JsonHelper.ConvertId(request.Id), await _database.GetObjectReferencesAsync(objectName, cts.Token));
                }

            case "execute_read_query"   :
                {
                    if (!request.Params.TryGetProperty("arguments", out var arguments) || !arguments.TryGetProperty("sql", out var sqlProp))
                    {
                        return Error(JsonHelper.ConvertId(request.Id), ErrorCodes.InvalidParams, "Missing required argument 'sql'");
                    }

                    var sql = sqlProp.GetString();
                    if (string.IsNullOrWhiteSpace(sql))
                        return Error(JsonHelper.ConvertId(request.Id), ErrorCodes.InvalidParams, "Missing required argument 'sql'");

                    return Success(JsonHelper.ConvertId(request.Id), await _database.ExecuteReadQueryAsync(sql, cts.Token));
                }

            default                     :
                return Error(JsonHelper.ConvertId(request.Id), ErrorCodes.InvalidParams, $"Unknown tool '{tool}'");
        }
    }

    /// <summary>
    /// Builds a successful tool result with a single text content block.
    /// </summary>
    /// <param name="id">The request identifier to echo.</param>
    /// <param name="result">The type of <see cref="QueryResult"/> containing the tool output.</param>
    /// <returns>
    /// A JSON-RPC success response.
    /// </returns>
    private static JsonRpcResponse Success(object? id, QueryResult result)
    {
        return new JsonRpcResponse
        {
            Id     = id,
            Result = result
        };
    }

    /// <summary>
    /// Builds a JSON-RPC error response.
    /// </summary>
    /// <param name="id">The request identifier to echo.</param>
    /// <param name="code">The JSON-RPC error code.</param>
    /// <param name="message">A human-readable error description.</param>
    /// <returns>
    /// A JSON-RPC error response.
    /// </returns>
    private static JsonRpcResponse Error(object? id, ErrorCodes code, string message)
    {
        return new JsonRpcResponse
        {
            Id    = id,
            Error = new JsonRpcError
            {
                Code    = (int)code,
                Message = message
            }
        };
    }
}
