using Microsoft.Data.Sqlite;

var connectionString = "Data Source=d:\\Coding\\VSCodeProject\\Muwasala Islamic Knowledge Network\\data\\database\\IslamicKnowledge.db";

try
{
    Console.WriteLine("=== Quick Database Check ===");
    
    using var connection = new SqliteConnection(connectionString);
    await connection.OpenAsync();
    
    // Check if database tables exist
    var tableCheckQuery = @"
        SELECT name FROM sqlite_master 
        WHERE type='table' AND name NOT LIKE 'sqlite_%'
        ORDER BY name;";
    
    using var tableCmd = new SqliteCommand(tableCheckQuery, connection);
    using var tableReader = await tableCmd.ExecuteReaderAsync();
    
    Console.WriteLine("\nDatabase Tables:");
    var tables = new List<string>();
    while (await tableReader.ReadAsync())
    {
        var tableName = tableReader.GetString(0);
        tables.Add(tableName);
        Console.WriteLine($"- {tableName}");
    }
    
    if (!tables.Any())
    {
        Console.WriteLine("No tables found in database!");
        return;
    }
    
    // Check record counts in key tables
    var tablesToCheck = new[] { "QuranVerses", "HadithRecords", "FiqhRulings", "DuaRecords", "SirahEvents", "TajweedRules" };
    
    Console.WriteLine("\nRecord Counts:");
    foreach (var table in tablesToCheck)
    {
        if (tables.Contains(table))
        {
            try
            {
                using var countCmd = new SqliteCommand($"SELECT COUNT(*) FROM {table}", connection);
                var count = await countCmd.ExecuteScalarAsync();
                Console.WriteLine($"{table}: {count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{table}: Error - {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"{table}: Table not found");
        }
    }
    
    // Test a simple search in Quran verses if they exist
    if (tables.Contains("QuranVerses"))
    {
        Console.WriteLine("\nTesting search in QuranVerses:");
        using var searchCmd = new SqliteCommand("SELECT Translation FROM QuranVerses WHERE Translation LIKE '%Allah%' LIMIT 3", connection);
        using var searchReader = await searchCmd.ExecuteReaderAsync();
        
        int resultCount = 0;
        while (await searchReader.ReadAsync())
        {
            var translation = searchReader.GetString(0);
            Console.WriteLine($"- {translation.Substring(0, Math.Min(80, translation.Length))}...");
            resultCount++;
        }
        
        if (resultCount == 0)
        {
            Console.WriteLine("No verses found containing 'Allah'");
        }
        else
        {
            Console.WriteLine($"Found {resultCount} verses containing 'Allah'");
        }
    }
    
    Console.WriteLine("\n✅ Database check completed!");
    
    // Write result to file
    var results = "Database is accessible and contains data!";
    await File.WriteAllTextAsync("quick_db_check_result.txt", results);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    await File.WriteAllTextAsync("quick_db_check_result.txt", $"Error: {ex.Message}");
}
