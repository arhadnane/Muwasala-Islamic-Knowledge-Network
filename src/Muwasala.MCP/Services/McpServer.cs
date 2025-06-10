using System.Text.Json;
using Microsoft.Extensions.Logging;
using Muwasala.MCP.Models;
using Muwasala.MCP.Services;

namespace Muwasala.MCP.Services;

public interface IMcpServer
{
    Task<McpResponse> HandleRequestAsync(McpRequest request);
    Task StartAsync();
}

public class McpServer : IMcpServer
{
    private readonly ISqliteService _sqliteService;
    private readonly IIslamicContentService _islamicContentService;
    private readonly ILogger<McpServer> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public McpServer(
        ISqliteService sqliteService, 
        IIslamicContentService islamicContentService,
        ILogger<McpServer> logger)
    {
        _sqliteService = sqliteService;
        _islamicContentService = islamicContentService;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public async Task<McpResponse> HandleRequestAsync(McpRequest request)
    {
        try
        {
            _logger.LogInformation("Handling MCP request: {Method}", request.method);

            return request.method switch
            {
                "initialize" => await HandleInitializeAsync(request),
                "tools/list" => await HandleToolsListAsync(),
                "tools/call" => await HandleToolCallAsync(request),
                "resources/list" => await HandleResourcesListAsync(),
                "resources/read" => await HandleResourceReadAsync(request),
                "prompts/list" => await HandlePromptsListAsync(),
                "prompts/get" => await HandlePromptGetAsync(request),
                _ => new McpResponse
                {
                    id = request.id,
                    error = new McpError
                    {
                        code = -32601,
                        message = $"Method not found: {request.method}"
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MCP request: {Method}", request.method);
            return new McpResponse
            {
                id = request.id,
                error = new McpError
                {
                    code = -32603,
                    message = "Internal error",
                    data = ex.Message
                }
            };
        }
    }    private Task<McpResponse> HandleInitializeAsync(McpRequest request)
    {
        var result = new
        {
            protocolVersion = "2024-11-05",
            capabilities = new
            {
                tools = new { },
                resources = new { subscribe = false, listChanged = false },
                prompts = new { }
            },
            serverInfo = new
            {
                name = "Muwasala Islamic Knowledge MCP Server",
                version = "1.0.0"
            }
        };

        return Task.FromResult(new McpResponse { id = request.id, result = result });
    }

    private Task<McpResponse> HandleToolsListAsync()
    {
        var tools = new List<McpTool>
        {
            new()
            {
                name = "query_database",
                description = "Execute SQL queries on the Islamic Knowledge database",
                inputSchema = JsonDocument.Parse(@"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""query"": {
                            ""type"": ""string"",
                            ""description"": ""SQL query to execute (SELECT statements only)""
                        },
                        ""parameters"": {
                            ""type"": ""object"",
                            ""description"": ""Optional parameters for the query""
                        }
                    },
                    ""required"": [""query""]
                }").RootElement
            },
            new()
            {
                name = "search_quran",
                description = "Search for Quran verses by content or theme",
                inputSchema = JsonDocument.Parse(@"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""searchTerm"": {
                            ""type"": ""string"",
                            ""description"": ""Term to search for in Quran verses""
                        },
                        ""limit"": {
                            ""type"": ""integer"",
                            ""description"": ""Maximum number of results to return"",
                            ""default"": 10
                        }
                    },
                    ""required"": [""searchTerm""]
                }").RootElement
            },
            new()
            {
                name = "search_hadith",
                description = "Search for Hadith by content or topic",
                inputSchema = JsonDocument.Parse(@"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""searchTerm"": {
                            ""type"": ""string"",
                            ""description"": ""Term to search for in Hadith""
                        },
                        ""limit"": {
                            ""type"": ""integer"",
                            ""description"": ""Maximum number of results to return"",
                            ""default"": 10
                        }
                    },
                    ""required"": [""searchTerm""]
                }").RootElement
            },
            new()
            {
                name = "search_fiqh",
                description = "Search for Islamic jurisprudence rulings",
                inputSchema = JsonDocument.Parse(@"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""searchTerm"": {
                            ""type"": ""string"",
                            ""description"": ""Term to search for in Fiqh rulings""
                        },
                        ""limit"": {
                            ""type"": ""integer"",
                            ""description"": ""Maximum number of results to return"",
                            ""default"": 10
                        }
                    },
                    ""required"": [""searchTerm""]
                }").RootElement
            },
            new()
            {
                name = "search_dua",
                description = "Search for Islamic prayers and supplications",
                inputSchema = JsonDocument.Parse(@"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""searchTerm"": {
                            ""type"": ""string"",
                            ""description"": ""Term to search for in Du'a""
                        },
                        ""limit"": {
                            ""type"": ""integer"",
                            ""description"": ""Maximum number of results to return"",
                            ""default"": 10
                        }
                    },
                    ""required"": [""searchTerm""]
                }").RootElement
            },
            new()
            {
                name = "get_quran_verse",
                description = "Get a specific Quran verse by Surah and verse number",
                inputSchema = JsonDocument.Parse(@"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""surah"": {
                            ""type"": ""integer"",
                            ""description"": ""Surah number (1-114)""
                        },
                        ""verse"": {
                            ""type"": ""integer"",
                            ""description"": ""Verse number""
                        }
                    },
                    ""required"": [""surah"", ""verse""]
                }").RootElement
            },
            new()
            {
                name = "get_table_schema",
                description = "Get the schema information for a database table",
                inputSchema = JsonDocument.Parse(@"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""tableName"": {
                            ""type"": ""string"",
                            ""description"": ""Name of the table to get schema for""
                        }
                    },
                    ""required"": [""tableName""]
                }").RootElement
            },
            new()
            {
                name = "list_tables",
                description = "List all available tables in the database",
                inputSchema = JsonDocument.Parse(@"{
                    ""type"": ""object"",
                    ""properties"": {}
                }").RootElement
            }        };

        return Task.FromResult(new McpResponse { result = new { tools } });
    }

    private async Task<McpResponse> HandleToolCallAsync(McpRequest request)
    {
        if (!request.@params.TryGetProperty("name", out var nameElement))
        {
            return new McpResponse
            {
                id = request.id,
                error = new McpError { code = -32602, message = "Missing tool name" }
            };
        }

        var toolName = nameElement.GetString();
        var arguments = request.@params.TryGetProperty("arguments", out var argsElement) ? argsElement : new JsonElement();

        var result = toolName switch
        {
            "query_database" => await HandleQueryDatabaseTool(arguments),
            "search_quran" => await HandleSearchQuranTool(arguments),
            "search_hadith" => await HandleSearchHadithTool(arguments),
            "search_fiqh" => await HandleSearchFiqhTool(arguments),
            "search_dua" => await HandleSearchDuaTool(arguments),
            "get_quran_verse" => await HandleGetQuranVerseTool(arguments),
            "get_table_schema" => await HandleGetTableSchemaTool(arguments),
            "list_tables" => await HandleListTablesTool(),
            _ => new { error = $"Unknown tool: {toolName}" }
        };

        return new McpResponse { id = request.id, result = new { content = new[] { new { type = "text", text = JsonSerializer.Serialize(result, _jsonOptions) } } } };
    }

    private async Task<object> HandleQueryDatabaseTool(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("query", out var queryElement))
        {
            return new { error = "Missing query parameter" };
        }

        var query = queryElement.GetString() ?? "";
        var parameters = new Dictionary<string, object>();

        if (arguments.TryGetProperty("parameters", out var paramsElement))
        {
            foreach (var prop in paramsElement.EnumerateObject())
            {
                parameters[prop.Name] = prop.Value.ToString() ?? "";
            }
        }

        var result = await _sqliteService.ExecuteQueryAsync(query, parameters);
        return result;
    }

    private async Task<object> HandleSearchQuranTool(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("searchTerm", out var searchTermElement))
        {
            return new { error = "Missing searchTerm parameter" };
        }

        var searchTerm = searchTermElement.GetString() ?? "";
        var limit = arguments.TryGetProperty("limit", out var limitElement) ? limitElement.GetInt32() : 10;

        var results = await _islamicContentService.SearchQuranAsync(searchTerm, limit);
        return new { searchTerm, results, count = results.Count };
    }

    private async Task<object> HandleSearchHadithTool(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("searchTerm", out var searchTermElement))
        {
            return new { error = "Missing searchTerm parameter" };
        }

        var searchTerm = searchTermElement.GetString() ?? "";
        var limit = arguments.TryGetProperty("limit", out var limitElement) ? limitElement.GetInt32() : 10;

        var results = await _islamicContentService.SearchHadithAsync(searchTerm, limit);
        return new { searchTerm, results, count = results.Count };
    }

    private async Task<object> HandleSearchFiqhTool(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("searchTerm", out var searchTermElement))
        {
            return new { error = "Missing searchTerm parameter" };
        }

        var searchTerm = searchTermElement.GetString() ?? "";
        var limit = arguments.TryGetProperty("limit", out var limitElement) ? limitElement.GetInt32() : 10;

        var results = await _islamicContentService.SearchFiqhAsync(searchTerm, limit);
        return new { searchTerm, results, count = results.Count };
    }

    private async Task<object> HandleSearchDuaTool(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("searchTerm", out var searchTermElement))
        {
            return new { error = "Missing searchTerm parameter" };
        }

        var searchTerm = searchTermElement.GetString() ?? "";
        var limit = arguments.TryGetProperty("limit", out var limitElement) ? limitElement.GetInt32() : 10;

        var results = await _islamicContentService.SearchDuaAsync(searchTerm, limit);
        return new { searchTerm, results, count = results.Count };
    }

    private async Task<object> HandleGetQuranVerseTool(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("surah", out var surahElement) ||
            !arguments.TryGetProperty("verse", out var verseElement))
        {
            return new { error = "Missing surah or verse parameter" };
        }

        var surah = surahElement.GetInt32();
        var verse = verseElement.GetInt32();

        var result = await _islamicContentService.GetQuranVerseAsync(surah, verse);
        return new { surah, verse, result };
    }

    private async Task<object> HandleGetTableSchemaTool(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("tableName", out var tableNameElement))
        {
            return new { error = "Missing tableName parameter" };
        }

        var tableName = tableNameElement.GetString() ?? "";
        var result = await _sqliteService.GetTableSchemaAsync(tableName);
        return new { tableName, schema = result };
    }

    private async Task<object> HandleListTablesTool()
    {
        var tables = await _sqliteService.GetTableNamesAsync();
        return new { tables, count = tables.Count };
    }    private Task<McpResponse> HandleResourcesListAsync()
    {
        var resources = new List<McpResource>
        {
            new()
            {
                uri = "sqlite://islamic-knowledge",
                name = "Islamic Knowledge Database",
                description = "SQLite database containing Quran, Hadith, Fiqh, and Du'a content",
                mimeType = "application/x-sqlite3"
            }        };

        return Task.FromResult(new McpResponse { result = new { resources } });
    }    private async Task<McpResponse> HandleResourceReadAsync(McpRequest request)
    {
        if (!request.@params.TryGetProperty("uri", out var uriElement))
        {
            return new McpResponse
            {
                id = request.id,
                error = new McpError { code = -32602, message = "Missing URI parameter" }
            };
        }

        var uri = uriElement.GetString();
        
        if (uri == "sqlite://islamic-knowledge")
        {
            var tables = await _sqliteService.GetTableNamesAsync();
            var content = JsonSerializer.Serialize(new { 
                database = "IslamicKnowledge.db",
                tables,
                description = "Islamic Knowledge database with Quran, Hadith, Fiqh, and Du'a content"
            }, _jsonOptions);

            return new McpResponse
            {
                id = request.id,
                result = new
                {
                    contents = new[]
                    {
                        new { uri, mimeType = "application/json", text = content }
                    }
                }
            };
        }

        return new McpResponse
        {
            id = request.id,
            error = new McpError { code = -32602, message = "Resource not found" }
        };
    }private Task<McpResponse> HandlePromptsListAsync()
    {
        var prompts = new List<McpPrompt>
        {
            new()
            {
                name = "islamic_search",
                description = "Search across all Islamic content types",
                arguments = new List<McpPromptArgument>
                {
                    new() { name = "query", description = "Search query", required = true },
                    new() { name = "content_types", description = "Types of content to search (quran,hadith,fiqh,dua)", required = false }
                }
            },
            new()
            {
                name = "verse_analysis",
                description = "Analyze a specific Quran verse with related content",
                arguments = new List<McpPromptArgument>
                {
                    new() { name = "surah", description = "Surah number", required = true },
                    new() { name = "verse", description = "Verse number", required = true }
                }
            }
        };        return Task.FromResult(new McpResponse { result = new { prompts } });
    }

    private async Task<McpResponse> HandlePromptGetAsync(McpRequest request)
    {
        if (!request.@params.TryGetProperty("name", out var nameElement))
        {
            return new McpResponse
            {
                id = request.id,
                error = new McpError { code = -32602, message = "Missing prompt name" }
            };
        }

        var promptName = nameElement.GetString();
        var arguments = request.@params.TryGetProperty("arguments", out var argsElement) ? argsElement : new JsonElement();

        var content = promptName switch
        {
            "islamic_search" => await GenerateIslamicSearchPrompt(arguments),
            "verse_analysis" => await GenerateVerseAnalysisPrompt(arguments),
            _ => "Unknown prompt"
        };

        return new McpResponse
        {
            id = request.id,
            result = new
            {
                description = $"Generated prompt for {promptName}",
                messages = new[]
                {
                    new { role = "user", content = new { type = "text", text = content } }
                }
            }
        };
    }

    private async Task<string> GenerateIslamicSearchPrompt(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("query", out var queryElement))
        {
            return "Please provide a search query.";
        }

        var query = queryElement.GetString() ?? "";
        var results = await _islamicContentService.SearchAllContentAsync(query, 5);

        var prompt = $"Search results for '{query}' in Islamic knowledge:\n\n";
        
        foreach (var result in results)
        {
            prompt += $"**{result.type} - {result.title}**\n";
            if (!string.IsNullOrEmpty(result.arabicText))
            {
                prompt += $"Arabic: {result.arabicText}\n";
            }
            prompt += $"Translation: {result.content}\n";
            prompt += $"Reference: {result.reference}\n\n";
        }

        return prompt;
    }

    private async Task<string> GenerateVerseAnalysisPrompt(JsonElement arguments)
    {
        if (!arguments.TryGetProperty("surah", out var surahElement) ||
            !arguments.TryGetProperty("verse", out var verseElement))
        {
            return "Please provide both surah and verse numbers.";
        }

        var surah = surahElement.GetInt32();
        var verse = verseElement.GetInt32();

        var verseContent = await _islamicContentService.GetQuranVerseAsync(surah, verse);
        
        if (verseContent == null)
        {
            return $"Verse {surah}:{verse} not found.";
        }

        var prompt = $"Analysis of Quran {surah}:{verse}\n\n";
        prompt += $"**Arabic Text:** {verseContent.arabicText}\n\n";
        prompt += $"**Translation:** {verseContent.content}\n\n";
        
        if (verseContent.metadata != null)
        {
            if (verseContent.metadata.ContainsKey("theme"))
                prompt += $"**Theme:** {verseContent.metadata["theme"]}\n\n";
            if (verseContent.metadata.ContainsKey("context"))
                prompt += $"**Context:** {verseContent.metadata["context"]}\n\n";
        }

        return prompt;
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("MCP Server started. Waiting for requests...");
        
        // Validate database connection
        if (!await _sqliteService.ValidateConnectionAsync())
        {
            _logger.LogError("Failed to connect to SQLite database");
            throw new InvalidOperationException("Database connection failed");
        }

        _logger.LogInformation("Database connection validated successfully");
    }
}
