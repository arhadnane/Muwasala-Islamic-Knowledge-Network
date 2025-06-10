using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Muwasala.MCP.Models;

namespace Muwasala.MCP.Services;

public interface ISqliteService
{
    Task<DatabaseQueryResult> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null);
    Task<DatabaseQueryResult> GetTableSchemaAsync(string tableName);
    Task<List<string>> GetTableNamesAsync();
    Task<DatabaseQueryResult> GetTableDataAsync(string tableName, int limit = 10, int offset = 0);
    Task<bool> ValidateConnectionAsync();
}

public class SqliteService : ISqliteService
{
    private readonly string _connectionString;
    private readonly ILogger<SqliteService> _logger;

    public SqliteService(string databasePath, ILogger<SqliteService> logger)
    {
        _connectionString = $"Data Source={databasePath}";
        _logger = logger;
    }

    public async Task<DatabaseQueryResult> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null)
    {
        var result = new DatabaseQueryResult();
        
        try
        {
            // Security: Only allow SELECT statements for safety
            if (!IsReadOnlyQuery(query))
            {
                result.error = "Only SELECT queries are allowed for security reasons";
                return result;
            }

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqliteCommand(query, connection);
            
            // Add parameters if provided
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                }
            }

            using var reader = await command.ExecuteReaderAsync();
            
            // Get column names
            for (int i = 0; i < reader.FieldCount; i++)
            {
                result.columns.Add(reader.GetName(i));
            }

            // Get data rows
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetValue(i);
                    row[reader.GetName(i)] = value == DBNull.Value ? null : value;
                }
                result.rows.Add(row);
            }

            result.rowCount = result.rows.Count;
            _logger.LogInformation($"Query executed successfully. Rows returned: {result.rowCount}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query: {Query}", query);
            result.error = ex.Message;
        }

        return result;
    }

    public async Task<DatabaseQueryResult> GetTableSchemaAsync(string tableName)
    {
        var query = $"PRAGMA table_info({tableName})";
        return await ExecuteQueryAsync(query);
    }

    public async Task<List<string>> GetTableNamesAsync()
    {
        var query = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name";
        var result = await ExecuteQueryAsync(query);
        
        return result.rows.Select(row => row["name"]?.ToString() ?? "").Where(name => !string.IsNullOrEmpty(name)).ToList();
    }

    public async Task<DatabaseQueryResult> GetTableDataAsync(string tableName, int limit = 10, int offset = 0)
    {
        var query = $"SELECT * FROM {tableName} LIMIT {limit} OFFSET {offset}";
        return await ExecuteQueryAsync(query);
    }

    public async Task<bool> ValidateConnectionAsync()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate database connection");
            return false;
        }
    }

    private static bool IsReadOnlyQuery(string query)
    {
        var trimmedQuery = query.Trim().ToUpperInvariant();
        return trimmedQuery.StartsWith("SELECT") || 
               trimmedQuery.StartsWith("WITH") || 
               trimmedQuery.StartsWith("PRAGMA table_info") ||
               trimmedQuery.StartsWith("PRAGMA schema_version");
    }
}
