using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Muwasala.KnowledgeBase.Extensions;
using Muwasala.KnowledgeBase.Services;
using Muwasala.Core.Models;

namespace Muwasala.TestLocalSearch;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🔍 Testing Local Search Functionality");
        Console.WriteLine("═══════════════════════════════════════");
        
        var serviceProvider = CreateServiceProvider();
        var globalSearchService = serviceProvider.GetRequiredService<IGlobalSearchService>();
        
        // Test queries
        var testQueries = new[]
        {
            "prayer",
            "Allah", 
            "faith",
            "patience",
            "charity"
        };
        
        foreach (var query in testQueries)
        {
            Console.WriteLine($"\n🔍 Testing Query: '{query}'");
            Console.WriteLine("─────────────────────────────────────");
            
            try
            {
                // Test local search with all content types
                var contentTypes = new[]
                {
                    IslamicContentType.Verse,
                    IslamicContentType.Hadith,
                    IslamicContentType.FiqhRuling,
                    IslamicContentType.Dua,
                    IslamicContentType.SirahEvent
                };
                
                var results = await globalSearchService.SearchByTypeAsync(query, contentTypes, "en", 20);
                  Console.WriteLine($"✅ Search completed!");
                Console.WriteLine($"   📊 Total results: {results.Results.Count}");
                Console.WriteLine($"   ⏱️  Duration: {results.SearchDuration:F2}ms");
                
                if (results.Results.Any())
                {
                    Console.WriteLine($"   📋 Results by type:");
                    foreach (var typeGroup in results.ResultsByType)
                    {
                        Console.WriteLine($"      - {typeGroup.Key}: {typeGroup.Value} results");
                    }
                    
                    Console.WriteLine($"   📄 Sample results:");
                    foreach (var result in results.Results.Take(3))
                    {
                        Console.WriteLine($"      • [{result.Type}] {result.Title}");
                        if (!string.IsNullOrEmpty(result.Content))
                        {
                            var preview = result.Content.Length > 100 ? 
                                result.Content.Substring(0, 100) + "..." : 
                                result.Content;
                            Console.WriteLine($"        {preview}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"   ❌ No results found!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error: {ex.Message}");
                Console.WriteLine($"   Stack trace: {ex.StackTrace}");
            }
        }
        
        // Test database connectivity
        Console.WriteLine($"\n🗄️ Testing Database Connectivity");
        Console.WriteLine("─────────────────────────────────────");
        
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Muwasala.KnowledgeBase.Data.IslamicKnowledgeDbContext>();
            
            var canConnect = await dbContext.Database.CanConnectAsync();
            Console.WriteLine($"Database connection: {(canConnect ? "✅ Connected" : "❌ Failed")}");
              // Check table contents
            var quranCount = await dbContext.QuranVerses.CountAsync();
            var hadithCount = await dbContext.HadithRecords.CountAsync();
            var fiqhCount = await dbContext.FiqhRulings.CountAsync();
            var duaCount = await dbContext.DuaRecords.CountAsync();
            var sirahCount = await dbContext.SirahEvents.CountAsync();
            
            Console.WriteLine($"📊 Database contents:");
            Console.WriteLine($"   - Quran verses: {quranCount}");
            Console.WriteLine($"   - Hadiths: {hadithCount}");
            Console.WriteLine($"   - Fiqh rulings: {fiqhCount}");
            Console.WriteLine($"   - Duas: {duaCount}");
            Console.WriteLine($"   - Sirah events: {sirahCount}");
            
            var totalRecords = quranCount + hadithCount + fiqhCount + duaCount + sirahCount;
            Console.WriteLine($"   📈 Total records: {totalRecords}");
            
            if (totalRecords == 0)
            {
                Console.WriteLine($"   ⚠️  WARNING: Database appears to be empty!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Database error: {ex.Message}");
        }
        
        Console.WriteLine("\n🏁 Test completed!");
    }
    
    static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Add configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:IslamicKnowledgeDb"] = "Data Source=islamic_knowledge.db"
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        
        // Add Islamic Knowledge Base services with database backend
        services.AddIslamicKnowledgeBase(configuration, useDatabaseServices: true);
        
        return services.BuildServiceProvider();
    }
}