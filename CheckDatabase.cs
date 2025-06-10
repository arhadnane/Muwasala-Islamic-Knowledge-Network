using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Muwasala.KnowledgeBase.Data;
using Muwasala.KnowledgeBase.Extensions;
using Microsoft.Extensions.Logging;

// Quick database check and initialization
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder => builder.AddConsole());

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
    Console.WriteLine("🔍 Checking database status...");
    
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IslamicKnowledgeDbContext>();
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    
    // Check connection
    var canConnect = await dbContext.Database.CanConnectAsync();
    Console.WriteLine($"Database connection: {(canConnect ? "✅ SUCCESS" : "❌ FAILED")}");
    
    if (canConnect)
    {
        // Get current statistics
        var stats = await initializer.GetStatisticsAsync();
        Console.WriteLine($"\n📊 Current Database Contents:");
        Console.WriteLine($"   Quran verses: {stats.QuranVersesCount}");
        Console.WriteLine($"   Hadiths: {stats.HadithRecordsCount}");
        Console.WriteLine($"   Fiqh rulings: {stats.FiqhRulingsCount}");
        Console.WriteLine($"   Duas: {stats.DuaRecordsCount}");
        Console.WriteLine($"   Sirah events: {stats.SirahEventsCount}");
        Console.WriteLine($"   Total records: {stats.TotalRecords}");
        Console.WriteLine($"   Database size: {stats.DatabaseSizeMB} MB");
        
        if (stats.TotalRecords == 0)
        {
            Console.WriteLine("\n⚠️  Database is empty! Initializing with seed data...");
            await initializer.InitializeAsync();
            
            // Check again after initialization
            var newStats = await initializer.GetStatisticsAsync();
            Console.WriteLine($"\n📊 After Initialization:");
            Console.WriteLine($"   Total records: {newStats.TotalRecords}");
            
            if (newStats.TotalRecords > 0)
            {
                Console.WriteLine("✅ Database successfully seeded!");
            }
            else
            {
                Console.WriteLine("❌ Failed to seed database - check seed data methods");
            }
        }
        else
        {
            Console.WriteLine("✅ Database has content - search should work!");
        }
        
        // Test a quick search
        Console.WriteLine("\n🔍 Testing search functionality...");
        var globalSearchService = scope.ServiceProvider.GetRequiredService<Muwasala.KnowledgeBase.Services.IGlobalSearchService>();
        var searchResult = await globalSearchService.SearchAllAsync("prayer", "en", 5);
        
        Console.WriteLine($"Search for 'prayer' returned {searchResult.Results.Count} results");
        if (searchResult.Results.Any())
        {
            Console.WriteLine("Sample results:");
            foreach (var result in searchResult.Results.Take(3))
            {
                Console.WriteLine($"  - [{result.Type}] {result.Title}");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
