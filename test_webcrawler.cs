using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muwasala.Agents;

namespace Muwasala.WebCrawlerTest;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🕷️ WebCrawler Agent Results Test");
        Console.WriteLine("═══════════════════════════════════");
        
        var serviceProvider = CreateServiceProvider();
        var webCrawlerAgent = serviceProvider.GetRequiredService<WebCrawlerAgent>();
        
        // Test queries
        var testQueries = new[]
        {
            "What is prayer in Islam?",
            "How to perform Hajj?",
            "Islamic charity and Zakat"
        };
        
        foreach (var query in testQueries)
        {
            Console.WriteLine($"\n🔍 Testing Query: {query}");
            Console.WriteLine("─────────────────────────────────────");
            
            try
            {
                var results = await webCrawlerAgent.SearchIslamicSourcesAsync(query, "en");
                
                Console.WriteLine($"✅ WebCrawler search completed!");
                Console.WriteLine($"   📊 Total results: {results.Count}");
                
                // Check if we're getting the expected 20+ results
                if (results.Count >= 20)
                {
                    Console.WriteLine("   ✅ SUCCESS: Got 20+ results as expected!");
                }
                else if (results.Count >= 10)
                {
                    Console.WriteLine("   ⚠️  PARTIAL: Got 10+ results but less than 20");
                }
                else
                {
                    Console.WriteLine($"   ❌ ISSUE: Only got {results.Count} results (expected 20+)");
                }
                
                // Display first few results
                Console.WriteLine("   📋 Sample results:");
                for (int i = 0; i < Math.Min(5, results.Count); i++)
                {
                    var result = results[i];
                    Console.WriteLine($"      {i + 1}. {result.Title}");
                    Console.WriteLine($"         Source: {result.Source}");
                    Console.WriteLine($"         URL: {result.Url}");
                    Console.WriteLine($"         Snippet: {result.Snippet[..Math.Min(80, result.Snippet.Length)]}...");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }
        
        Console.WriteLine("\n🏁 WebCrawler Test completed!");
    }
    
    static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Add HttpClient
        services.AddHttpClient();
        
        // Add WebCrawlerAgent
        services.AddSingleton<WebCrawlerAgent>();
        
        return services.BuildServiceProvider();
    }
}
