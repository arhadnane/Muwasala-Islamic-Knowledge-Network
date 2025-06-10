using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Muwasala.KnowledgeBase.Data;
using Muwasala.KnowledgeBase.Extensions;

Console.WriteLine("🔄 Resetting and Reseeding Islamic Knowledge Database...");
Console.WriteLine("=========================================================");

var services = new ServiceCollection();

// Add logging
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
    var dbContext = scope.ServiceProvider.GetRequiredService<IslamicKnowledgeDbContext>();
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    
    Console.WriteLine("📊 Checking current database status...");
    var initialStats = await initializer.GetStatisticsAsync();
    Console.WriteLine($"Current records: {initialStats.TotalRecords}");
    
    Console.WriteLine("\n🗑️ Resetting database (this will delete all existing data)...");
    await initializer.ResetDatabaseAsync();
    
    Console.WriteLine("\n📈 Database has been reset and reseeded!");
    var finalStats = await initializer.GetStatisticsAsync();
    
    Console.WriteLine($"\n📊 Final Database Contents:");
    Console.WriteLine($"   Quran verses: {finalStats.QuranVersesCount}");
    Console.WriteLine($"   Hadiths: {finalStats.HadithRecordsCount}");
    Console.WriteLine($"   Fiqh rulings: {finalStats.FiqhRulingsCount}");
    Console.WriteLine($"   Duas: {finalStats.DuaRecordsCount}");
    Console.WriteLine($"   Sirah events: {finalStats.SirahEventsCount}");
    Console.WriteLine($"   Tajweed rules: {finalStats.TajweedRulesCount}");
    Console.WriteLine($"   Total records: {finalStats.TotalRecords}");
    Console.WriteLine($"   Database size: {finalStats.DatabaseSizeMB} MB");
    
    if (finalStats.TotalRecords > 0)
    {
        Console.WriteLine("\n✅ Database successfully reset and seeded!");
        
        // Test search functionality
        Console.WriteLine("\n🔍 Testing search functionality...");
        var globalSearchService = scope.ServiceProvider.GetRequiredService<Muwasala.KnowledgeBase.Services.IGlobalSearchService>();
        
        var testQueries = new[] { "prayer", "patience", "Allah", "knowledge" };
        
        foreach (var query in testQueries)
        {
            var result = await globalSearchService.SearchAllAsync(query, "en", 3);
            Console.WriteLine($"   '{query}': {result.Results.Count} results");
            
            if (result.Results.Any())
            {
                foreach (var searchResult in result.Results.Take(2))
                {
                    Console.WriteLine($"      - [{searchResult.Type}] {searchResult.Title}");
                }
            }
        }
        
        Console.WriteLine("\n🎉 Database reset complete! The web application search should now work.");
    }
    else
    {
        Console.WriteLine("\n❌ Failed to seed database - please check the seeding methods");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
    }
    Console.WriteLine($"\nStack trace: {ex.StackTrace}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
