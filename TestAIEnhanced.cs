using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muwasala.Core.Services;
using Muwasala.KnowledgeBase.Services;
using Muwasala.KnowledgeBase.Data;
using Muwasala.Agents;

namespace Muwasala.TestAIEnhanced;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🤖 Testing AI Enhanced Search Functionality");
        Console.WriteLine("===========================================");

        // Create host builder with dependencies
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register services
                services.AddSingleton<IOllamaService, OllamaService>();
                services.AddHttpClient<IIntelligentSearchService, IntelligentSearchService>();
                services.AddTransient<IIntelligentSearchService, IntelligentSearchService>();
                
                // Mock global search service for this test
                services.AddTransient<IGlobalSearchService, MockGlobalSearchService>();
                
                // Logging
                services.AddLogging(builder => builder.AddConsole());
            })
            .Build();

        var serviceProvider = host.Services;
        var intelligentSearchService = serviceProvider.GetRequiredService<IIntelligentSearchService>();
        
        // Test queries
        var testQueries = new[]
        {
            "What are the five pillars of Islam?",
            "Explain the importance of prayer in Islam",
            "What does the Quran say about charity?",
            "Tell me about the Prophet Muhammad's character"
        };

        foreach (var query in testQueries)
        {
            Console.WriteLine($"\n🔍 Testing Query: '{query}'");
            Console.WriteLine("---");
            
            try
            {
                var aiResponse = await intelligentSearchService.GetAIKnowledgeAsync(query, "en");
                
                if (aiResponse != null)
                {
                    Console.WriteLine("✅ AI Enhanced Search Response:");
                    Console.WriteLine($"📝 Response: {aiResponse.Response}");
                    Console.WriteLine($"📖 Quran References: {string.Join(", ", aiResponse.QuranReferences)}");
                    Console.WriteLine($"📚 Hadith References: {string.Join(", ", aiResponse.HadithReferences)}");
                    Console.WriteLine($"👨‍🎓 Scholarly Opinions: {string.Join(", ", aiResponse.ScholarlyOpinions)}");
                    Console.WriteLine($"🎯 Confidence Score: {aiResponse.ConfidenceScore:F2}");
                    Console.WriteLine($"🔗 Source: {aiResponse.Source}");
                }
                else
                {
                    Console.WriteLine("❌ No AI response generated");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"   Stack trace: {ex.StackTrace}");
            }
            
            await Task.Delay(2000); // Wait between requests
        }

        Console.WriteLine("\n🎯 AI Enhanced Search Test completed!");
    }
}

// Mock implementation for testing
public class MockGlobalSearchService : IGlobalSearchService
{
    public async Task<GlobalSearchResponse> SearchAllAsync(string query, string language = "en", int maxResults = 20)
    {
        await Task.Delay(100);
        return new GlobalSearchResponse(query, new List<GlobalSearchResult>(), 0, 100, new Dictionary<IslamicContentType, int>());
    }

    public async Task<GlobalSearchResponse> SearchByTypeAsync(string query, IslamicContentType[] types, string language = "en", int maxResults = 20)
    {
        await Task.Delay(100);
        return new GlobalSearchResponse(query, new List<GlobalSearchResult>(), 0, 100, new Dictionary<IslamicContentType, int>());
    }

    public async Task<List<string>> GetSearchSuggestionsAsync(string partialQuery, string language = "en")
    {
        await Task.Delay(50);
        return new List<string> { "suggestion1", "suggestion2" };
    }
}
