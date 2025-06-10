using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muwasala.MCP.Models;
using Muwasala.MCP.Services;

namespace Muwasala.MCP;

class Program
{
    static async Task Main(string[] args)
    {
        // Get database path from command line or use default
        var databasePath = args.Length > 0 ? args[0] : 
            Path.Combine(Directory.GetCurrentDirectory(), "data", "database", "IslamicKnowledge.db");

        if (!File.Exists(databasePath))
        {
            Console.Error.WriteLine($"Database file not found: {databasePath}");
            Environment.Exit(1);
        }

        var host = CreateHostBuilder(args, databasePath).Build();
        
        try
        {
            var mcpServer = host.Services.GetRequiredService<IMcpServer>();
            await mcpServer.StartAsync();

            // MCP communication over stdin/stdout
            await HandleMcpCommunication(mcpServer);
        }
        catch (Exception ex)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Fatal error in MCP server");
            Environment.Exit(1);
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args, string databasePath) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                services.AddSingleton<ISqliteService>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<SqliteService>>();
                    return new SqliteService(databasePath, logger);
                });

                services.AddSingleton<IIslamicContentService, IslamicContentService>();
                services.AddSingleton<IMcpServer, McpServer>();
            });

    private static async Task HandleMcpCommunication(IMcpServer mcpServer)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        string? line;
        while ((line = await Console.In.ReadLineAsync()) != null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var request = JsonSerializer.Deserialize<McpRequest>(line, jsonOptions);
                if (request == null)
                    continue;

                var response = await mcpServer.HandleRequestAsync(request);
                var responseJson = JsonSerializer.Serialize(response, jsonOptions);
                
                await Console.Out.WriteLineAsync(responseJson);
                await Console.Out.FlushAsync();
            }
            catch (JsonException ex)
            {
                var errorResponse = new McpResponse
                {
                    jsonrpc = "2.0",
                    error = new McpError
                    {
                        code = -32700,
                        message = "Parse error",
                        data = ex.Message
                    }
                };

                var errorJson = JsonSerializer.Serialize(errorResponse, jsonOptions);
                await Console.Out.WriteLineAsync(errorJson);
                await Console.Out.FlushAsync();
            }
            catch (Exception ex)
            {
                var errorResponse = new McpResponse
                {
                    jsonrpc = "2.0",
                    error = new McpError
                    {
                        code = -32603,
                        message = "Internal error",
                        data = ex.Message
                    }
                };

                var errorJson = JsonSerializer.Serialize(errorResponse, jsonOptions);
                await Console.Out.WriteLineAsync(errorJson);
                await Console.Out.FlushAsync();
            }
        }
    }
}
