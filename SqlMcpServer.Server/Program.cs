using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SqlMcpServer.Server;
using SqlMcpServer.Server.Models;
using SqlMcpServer.Server.Services;
using SqlMcpServer.Server.Services.Interfaces;

class Program
{
    static async Task Main(string[] args)
    {
        
        // adding services
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                       .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
            })
            .UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services);
            })
            .ConfigureServices((context, services) =>
            {
                // Binds sections of appsettings.json
                services.Configure<AppSettings>(context.Configuration);

                // Singleton services for dependency injection
                services.AddSingleton<ISqlExecutor, SqlExecutor>();
                services.AddSingleton<IDatabaseService, DatabaseService>();
                services.AddSingleton<McpMessageHandler>();
            })
            .Build();

        // Automatically resolve and inject any dependencies required by the Startup class's constructor.
        // This helps in creating instances of classes that have dependencies registered in the DI container.

        var service = ActivatorUtilities.CreateInstance<Startup>(host.Services);
        await service.Run();
    }
}