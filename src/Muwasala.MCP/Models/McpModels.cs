using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Muwasala.MCP.Models;

public class McpRequest
{
    public string jsonrpc { get; set; } = "2.0";
    public string method { get; set; } = string.Empty;
    public JsonElement @params { get; set; }
    public object? id { get; set; }
}

public class McpResponse
{
    public string jsonrpc { get; set; } = "2.0";
    public object? result { get; set; }
    public McpError? error { get; set; }
    public object? id { get; set; }
}

public class McpError
{
    public int code { get; set; }
    public string message { get; set; } = string.Empty;
    public object? data { get; set; }
}

public class McpTool
{
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public JsonElement inputSchema { get; set; }
}

public class McpResource
{
    public string uri { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string? description { get; set; }
    public string? mimeType { get; set; }
}

public class McpPrompt
{
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public List<McpPromptArgument>? arguments { get; set; }
}

public class McpPromptArgument
{
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public bool required { get; set; } = false;
}

public class DatabaseQueryResult
{
    public List<Dictionary<string, object?>> rows { get; set; } = new();
    public List<string> columns { get; set; } = new();
    public int rowCount { get; set; }
    public string? error { get; set; }
}

public class IslamicContent
{
    public string type { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
    public string? arabicText { get; set; }
    public string? reference { get; set; }
    public string? source { get; set; }
    public Dictionary<string, object?>? metadata { get; set; }
}
