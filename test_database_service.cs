using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Muwasala.KnowledgeBase.Extensions;
using Muwasala.KnowledgeBase.Services;
using Muwasala.KnowledgeBase.Data.Repositories;
using Muwasala.Core.Models;

namespace Muwasala.TestDatabaseService;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🔧 Testing Database Service Directly");
        Console.WriteLine("═══════════════════════════════════════");
        
        var serviceProvider = CreateServiceProvider();
        
        // Test repository connections directly
        Console.WriteLine("\n🔍 Testing Repository Connections:");
        Console.WriteLine("───────────────────────────────────────");
        
        try
        {
            // Test Quran Repository
            var quranRepo = serviceProvider.GetRequiredService<IQuranRepository>();
            var quranResults = await quranRepo.SearchByTextAsync("prayer", "en", 5);
            Console.WriteLine($"✅ Quran Repository: {quranResults.Count()} results");
            
            // Test Hadith Repository  
            var hadithRepo = serviceProvider.GetRequiredService<IHadithRepository>();
            var hadithResults = await hadithRepo.SearchByTextAsync("prayer", "en", 5);
            Console.WriteLine($"✅ Hadith Repository: {hadithResults.Count()} results");
            
            // Test Fiqh Repository
            var fiqhRepo = serviceProvider.GetRequiredService<IFiqhRepository>();
            var fiqhResults = await fiqhRepo.GetRulingsByTopicAllMadhabsAsync("prayer", "en", 5);
            Console.WriteLine($"✅ Fiqh Repository: {fiqhResults.Count()} results");
            
            // Test Dua Repository
            var duaRepo = serviceProvider.GetRequiredService<IDuaRepository>();
            var duaResults = await duaRepo.SearchByTextAsync("prayer", "en", 5);
            Console.WriteLine($"✅ Dua Repository: {duaResults.Count()} results");
            
            // Test Sirah Repository
            var sirahRepo = serviceProvider.GetRequiredService<ISirahRepository>();
            var sirahResults = await sirahRepo.SearchByContextAsync("prayer", "en", 5);
            Console.WriteLine($"✅ Sirah Repository: {sirahResults.Count()} results");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Repository test failed: {ex.Message}");
            Console.WriteLine($"   Stack trace: {ex.StackTrace}");
        }
        
        // Test DatabaseGlobalSearchService directly
        Console.WriteLine("\n🔍 Testing DatabaseGlobalSearchService:");
        Console.WriteLine("─────────────────────────────────────");
        
        try
        {
            var globalSearchService = serviceProvider.GetRequiredService<IGlobalSearchService>();
            Console.WriteLine($"✅ Service type: {globalSearchService.GetType().Name}");
            
            var contentTypes = new[]
            {
                IslamicContentType.Verse,
                IslamicContentType.Hadith,
                IslamicContentType.FiqhRuling,
                IslamicContentType.Dua,
                IslamicContentType.SirahEvent
            };
            
            var results = await globalSearchService.SearchByTypeAsync("prayer", contentTypes, "en", 20);
            
            Console.WriteLine($"✅ Search completed!");
            Console.WriteLine($"   📊 Total results: {results.Results.Count}");
            Console.WriteLine($"   ⏱️  Duration: {results.SearchDurationMs:F2}ms");
            
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
                Console.WriteLine("   ❌ No results found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database search failed: {ex.Message}");
            Console.WriteLine($"   Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("\n🏁 Test completed");
    }
    
    static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => 
        {
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug);
        });
        
        // Configure to use database services
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        // Add Islamic Knowledge Base with database services
        services.AddIslamicKnowledgeBase(configuration, useDatabaseServices: true);
        
        return services.BuildServiceProvider();
    }
}
