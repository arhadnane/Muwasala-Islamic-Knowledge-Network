using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muwasala.Core.Services;
using Muwasala.KnowledgeBase.Services;
using Muwasala.Agents;

namespace Muwasala.TestSearch;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🔍 Muwasala Search Results Test");
        Console.WriteLine("═══════════════════════════════════");
        
        var serviceProvider = CreateServiceProvider();
        var intelligentSearchService = serviceProvider.GetRequiredService<IIntelligentSearchService>();
        
        // Test queries
        var testQueries = new[]
        {
            "What is prayer in Islam?",
            "How to perform Hajj?",
            "Islamic charity and Zakat",
            "Prophet Muhammad teachings"
        };
        
        foreach (var query in testQueries)
        {
            Console.WriteLine($"\n🔍 Testing Query: {query}");
            Console.WriteLine("─────────────────────────────────────");
            
            try
            {
                var response = await intelligentSearchService.PerformHybridSearchAsync(query, "en", 20);
                
                Console.WriteLine($"✅ Search completed successfully!");
                Console.WriteLine($"   📊 Total results: {response.TotalResultsFound}");
                Console.WriteLine($"   📚 Local results: {response.LocalResults.Count}");
                Console.WriteLine($"   🤖 AI response: {(response.AIResponse != null ? "Available" : "None")}");
                Console.WriteLine($"   🌐 Web results: {response.WebResults.Count}");
                Console.WriteLine($"   ⚡ Duration: {response.SearchDurationMs:F0}ms");
                Console.WriteLine($"   💡 Suggestions: {response.SearchSuggestions.Count}");
                
                // Display results by source
                if (response.ResultsBySource.Any())
                {
                    Console.WriteLine("   📋 Results by source:");
                    foreach (var sourceCount in response.ResultsBySource)
                    {
                        Console.WriteLine($"      • {sourceCount.Key}: {sourceCount.Value}");
                    }
                }
                
                // Check if we're getting 20+ results as expected
                if (response.TotalResultsFound >= 20)
                {
                    Console.WriteLine("   ✅ SUCCESS: Got 20+ results as expected!");
                }
                else if (response.TotalResultsFound >= 10)
                {
                    Console.WriteLine("   ⚠️  PARTIAL: Got 10+ results but less than 20");
                }
                else
                {
                    Console.WriteLine($"   ❌ ISSUE: Only got {response.TotalResultsFound} results (expected 20+)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }
        
        Console.WriteLine("\n🏁 Test completed!");
    }
    
    static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Add HttpClient
        services.AddHttpClient();
        
        // Add core services
        services.AddSingleton<IQuranService, QuranService>();
        services.AddSingleton<IHadithService, HadithService>();
        services.AddSingleton<IFiqhService, FiqhService>();
        services.AddSingleton<IDuaService, DuaService>();
        services.AddSingleton<ISirahService, SirahService>();
        services.AddSingleton<ITajweedService, TajweedService>();
        services.AddSingleton<IOllamaService, MockOllamaService>();
        
        // Add search services
        services.AddSingleton<IGlobalSearchService, GlobalSearchService>();
        services.AddSingleton<IIntelligentSearchService, IntelligentSearchService>();
        
        // Add agent services
        services.AddSingleton<WebCrawlerAgent>();
        
        return services.BuildServiceProvider();
    }
}

// Mock Ollama service for testing
public class MockOllamaService : IOllamaService
{
    public async Task<string> GenerateResponseAsync(string model, string prompt)
    {
        await Task.Delay(100); // Simulate processing
        return "ANSWER: This is a mock AI response for testing purposes.\nQURAN_REFS: 2:183|2:184\nHADITH_REFS: Bukhari 1|Muslim 1\nSCHOLARS: Ibn Taymiyyah|Al-Nawawi";
    }
    
    public async Task<bool> IsAvailableAsync() => true;
}
