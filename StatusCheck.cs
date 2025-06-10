using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Muwasala.KnowledgeBase.Data;
using Muwasala.KnowledgeBase.Services;
using Muwasala.KnowledgeBase.Extensions;
using System.Text;

Console.WriteLine("🔍 Muwasala Islamic Knowledge Database - Status Check & Search Test");
Console.WriteLine("===================================================================");

// Setup services
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string>
    {
        ["ConnectionStrings:IslamicKnowledgeDb"] = "Data Source=d:\\Coding\\VSCodeProject\\Muwasala Islamic Knowledge Network\\data\\database\\IslamicKnowledge.db"
    })
    .Build();

services.AddSingleton<IConfiguration>(configuration);
services.AddIslamicKnowledgeBase(configuration, useDatabaseServices: true);

var serviceProvider = services.BuildServiceProvider();

try
{
    using var scope = serviceProvider.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    var searchService = scope.ServiceProvider.GetRequiredService<IGlobalSearchService>();
    
    Console.WriteLine("\n📊 Getting database statistics...");
    var stats = await initializer.GetStatisticsAsync();
    
    var report = new StringBuilder();
    report.AppendLine("Database Status Report");
    report.AppendLine("=====================");
    report.AppendLine($"Quran verses: {stats.QuranVersesCount:N0}");
    report.AppendLine($"Hadith records: {stats.HadithRecordsCount:N0}");
    report.AppendLine($"Fiqh rulings: {stats.FiqhRulingsCount:N0}");
    report.AppendLine($"Duas: {stats.DuaRecordsCount:N0}");
    report.AppendLine($"Sirah events: {stats.SirahEventsCount:N0}");
    report.AppendLine($"Tajweed rules: {stats.TajweedRulesCount:N0}");
    report.AppendLine($"Tafsir entries: {stats.TafsirEntriesCount:N0}");
    report.AppendLine($"Common mistakes: {stats.CommonMistakesCount:N0}");
    report.AppendLine($"Verse Tajweed data: {stats.VerseTajweedCount:N0}");
    report.AppendLine($"Total records: {stats.TotalRecords:N0}");
    report.AppendLine($"Database size: {stats.DatabaseSizeMB:F2} MB");
    
    Console.WriteLine(report.ToString());
    
    if (stats.TotalRecords == 0)
    {
        Console.WriteLine("⚠️  Database is empty! Initializing...");
        await initializer.InitializeAsync();
        
        // Get updated stats
        stats = await initializer.GetStatisticsAsync();
        Console.WriteLine($"✅ Database initialized! Total records: {stats.TotalRecords:N0}");
    }
    else
    {
        Console.WriteLine("✅ Database contains data!");
    }
    
    // Test search functionality
    Console.WriteLine("\n🔍 Testing search functionality...");
    
    var searchQueries = new[] { "Allah", "prayer", "patience", "knowledge", "faith" };
    
    foreach (var query in searchQueries)
    {
        try
        {
            var results = await searchService.SearchAsync(query, 3);
            Console.WriteLine($"\n🔍 Search for '{query}': {results.Count()} results");
            
            foreach (var result in results.Take(2))
            {
                Console.WriteLine($"   📖 {result.Source}: {result.Title}");
                var preview = result.Content.Length > 80 
                    ? result.Content.Substring(0, 80) + "..." 
                    : result.Content;
                Console.WriteLine($"      {preview}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Search for '{query}' failed: {ex.Message}");
        }
    }
    
    // Write detailed report to file
    var detailedReport = new StringBuilder();
    detailedReport.AppendLine("MUWASALA ISLAMIC KNOWLEDGE DATABASE - DETAILED REPORT");
    detailedReport.AppendLine("====================================================");
    detailedReport.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    detailedReport.AppendLine();
    detailedReport.Append(report.ToString());
    detailedReport.AppendLine();
    detailedReport.AppendLine("SEARCH TEST RESULTS");
    detailedReport.AppendLine("==================");
    
    foreach (var query in searchQueries)
    {
        try
        {
            var results = await searchService.SearchAsync(query, 5);
            detailedReport.AppendLine($"\nSearch Query: '{query}' - {results.Count()} results found");
            
            foreach (var result in results)
            {
                detailedReport.AppendLine($"- {result.Source}: {result.Title}");
                detailedReport.AppendLine($"  Score: {result.RelevanceScore:F2}");
                detailedReport.AppendLine($"  Preview: {(result.Content.Length > 100 ? result.Content.Substring(0, 100) + "..." : result.Content)}");
                detailedReport.AppendLine();
            }
        }
        catch (Exception ex)
        {
            detailedReport.AppendLine($"Search for '{query}' failed: {ex.Message}");
        }
    }
    
    await File.WriteAllTextAsync("database_status_report.txt", detailedReport.ToString());
    Console.WriteLine("\n📄 Detailed report saved to: database_status_report.txt");
    
    Console.WriteLine("\n✅ Database check and search test completed successfully!");
    
    if (stats.TotalRecords > 0)
    {
        Console.WriteLine("\n🌐 The web application search should now be working properly!");
        Console.WriteLine("   Visit: https://localhost:7002/globalsearch to test the search interface");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    
    await File.WriteAllTextAsync("database_error_log.txt", $"Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
    Console.WriteLine("Error details saved to: database_error_log.txt");
}
