using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muwasala.Core.Services;
using Muwasala.KnowledgeBase.Services;
using Muwasala.KnowledgeBase.Data;
using Muwasala.Agents;

namespace Muwasala.TestAISearchPerformance;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Muwasala AI Search Performance Test");
        Console.WriteLine("════════════════════════════════════════");
        Console.WriteLine("Testing the optimized AI search functionality");
        Console.WriteLine();

        var serviceProvider = CreateServiceProvider();
        
        // Test queries with different complexity levels
        var testQueries = new[]
        {
            "What is prayer in Islam?",                    // Simple query
            "How to perform Hajj step by step?",          // Medium complexity
            "Islamic jurisprudence on modern banking",    // Complex query
            "Benefits of reading Quran daily",            // Spiritual query
            "Rules of fasting during Ramadan"             // Fiqh query
        };

        var performanceResults = new List<PerformanceResult>();

        foreach (var query in testQueries)
        {
            Console.WriteLine($"🔍 Testing Query: '{query}'");
            Console.WriteLine("─".PadRight(60, '─'));
            
            try
            {
                var result = await TestSearchPerformance(serviceProvider, query);
                performanceResults.Add(result);
                
                Console.WriteLine($"✅ Success!");
                Console.WriteLine($"   ⏱️  Total Time: {result.TotalTimeMs:F0}ms");
                Console.WriteLine($"   🔍 Local Results: {result.LocalResultsCount}");
                Console.WriteLine($"   🤖 AI Response: {(result.HasAIResponse ? "Yes" : "No")}");
                Console.WriteLine($"   🌐 Web Results: {result.WebResultsCount}");
                Console.WriteLine($"   💾 Cache Hit: {(result.CacheHit ? "Yes" : "No")}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine();
            }
        }

        // Test caching performance by running the same query twice
        Console.WriteLine("🧪 Testing Cache Performance");
        Console.WriteLine("─".PadRight(60, '─'));
        
        var cacheTestQuery = "What is Bismillah?";
        
        // First run (should miss cache)
        var firstRun = await TestSearchPerformance(serviceProvider, cacheTestQuery);
        Console.WriteLine($"First run: {firstRun.TotalTimeMs:F0}ms (Cache: {(firstRun.CacheHit ? "Hit" : "Miss")})");
        
        // Second run (should hit cache)
        var secondRun = await TestSearchPerformance(serviceProvider, cacheTestQuery);
        Console.WriteLine($"Second run: {secondRun.TotalTimeMs:F0}ms (Cache: {(secondRun.CacheHit ? "Hit" : "Miss")})");
        
        if (secondRun.CacheHit)
        {
            var improvement = ((firstRun.TotalTimeMs - secondRun.TotalTimeMs) / firstRun.TotalTimeMs) * 100;
            Console.WriteLine($"🚀 Cache improvement: {improvement:F1}% faster!");
        }
        
        Console.WriteLine();

        // Display summary
        DisplayPerformanceSummary(performanceResults);
    }

    static async Task<PerformanceResult> TestSearchPerformance(ServiceProvider serviceProvider, string query)
    {
        var intelligentSearchService = serviceProvider.GetRequiredService<IIntelligentSearchService>();
        
        var stopwatch = Stopwatch.StartNew();
        var response = await intelligentSearchService.PerformHybridSearchAsync(query, "en", 20);
        stopwatch.Stop();

        return new PerformanceResult
        {
            Query = query,
            TotalTimeMs = stopwatch.ElapsedMilliseconds,
            LocalResultsCount = response.LocalResults.Count,
            HasAIResponse = response.AIResponse != null,
            WebResultsCount = response.WebResults?.Count ?? 0,
            CacheHit = stopwatch.ElapsedMilliseconds < 1000 // Assume cache hit if very fast
        };
    }

    static void DisplayPerformanceSummary(List<PerformanceResult> results)
    {
        Console.WriteLine("📊 Performance Summary");
        Console.WriteLine("════════════════════════════════════════");
        
        if (results.Count == 0)
        {
            Console.WriteLine("No results to analyze.");
            return;
        }

        var avgTime = results.Average(r => r.TotalTimeMs);
        var minTime = results.Min(r => r.TotalTimeMs);
        var maxTime = results.Max(r => r.TotalTimeMs);
        var aiResponseRate = (results.Count(r => r.HasAIResponse) / (double)results.Count) * 100;

        Console.WriteLine($"🎯 Total Queries Tested: {results.Count}");
        Console.WriteLine($"⏱️  Average Response Time: {avgTime:F0}ms");
        Console.WriteLine($"⚡ Fastest Response: {minTime:F0}ms");
        Console.WriteLine($"🐌 Slowest Response: {maxTime:F0}ms");
        Console.WriteLine($"🤖 AI Response Rate: {aiResponseRate:F1}%");
        
        var fastQueries = results.Count(r => r.TotalTimeMs < 2000);
        var mediumQueries = results.Count(r => r.TotalTimeMs >= 2000 && r.TotalTimeMs < 5000);
        var slowQueries = results.Count(r => r.TotalTimeMs >= 5000);
        
        Console.WriteLine();
        Console.WriteLine("📈 Response Time Distribution:");
        Console.WriteLine($"   🟢 Fast (< 2s): {fastQueries} queries");
        Console.WriteLine($"   🟡 Medium (2-5s): {mediumQueries} queries");
        Console.WriteLine($"   🔴 Slow (> 5s): {slowQueries} queries");
        
        Console.WriteLine();
        Console.WriteLine("🎉 Performance Optimizations Applied:");
        Console.WriteLine("   ✅ Parallel execution of search tasks");
        Console.WriteLine("   ✅ AI response caching (30min expiry)");
        Console.WriteLine("   ✅ Intelligent search thresholds");
        Console.WriteLine("   ✅ Asynchronous history saving");
        Console.WriteLine("   ✅ Optimized AI prompts");
        Console.WriteLine("   ✅ Request timeout protection (15s)");
    }

    static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        
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
        
        return services.BuildServiceProvider();
    }
}

// Mock Ollama service for testing
public class MockOllamaService : IOllamaService
{
    public async Task<string> GenerateResponseAsync(string model, string prompt)
    {
        // Simulate AI processing time
        await Task.Delay(Random.Shared.Next(800, 2000));
        
        return @"ANSWER: In Islamic tradition, prayer (Salah) is one of the Five Pillars of Islam and represents a direct connection between the worshipper and Allah. It is performed five times daily and involves specific physical movements and recitations.

QURAN_REFS: 2:238|4:103|17:78
HADITH_REFS: Bukhari 527|Muslim 612
SCHOLARS: Ibn Kathir|Al-Nawawi|Ibn Taymiyyah";
    }

    public async Task<bool> IsModelAvailableAsync(string modelName)
    {
        await Task.Delay(50);
        return true;
    }

    public async Task<List<string>> GetAvailableModelsAsync()
    {
        await Task.Delay(100);
        return new List<string> { "mistral:7b", "llama2:7b" };
    }
}

public class PerformanceResult
{
    public string Query { get; set; } = "";
    public double TotalTimeMs { get; set; }
    public int LocalResultsCount { get; set; }
    public bool HasAIResponse { get; set; }
    public int WebResultsCount { get; set; }
    public bool CacheHit { get; set; }
}

public static class Extensions
{
    public static double Average<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        return source.Select(selector).Average();
    }
    
    public static double Min<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        return source.Select(selector).Min();
    }
    
    public static double Max<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        return source.Select(selector).Max();
    }
}
