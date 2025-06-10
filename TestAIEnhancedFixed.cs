using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Muwasala.KnowledgeBase.Extensions;
using Muwasala.KnowledgeBase.Services;
using Muwasala.Core.Services;

namespace Muwasala.TestAIEnhancedFixed;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🤖 Testing AI Enhanced Search (Fixed Version)");
        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine();

        var serviceProvider = CreateServiceProvider();
        
        // Test direct Ollama connection first
        Console.WriteLine("🔗 Testing Direct Ollama Connection...");
        await TestOllamaConnection(serviceProvider);
        Console.WriteLine();
        
        // Test IntelligentSearchService
        Console.WriteLine("🧠 Testing IntelligentSearchService...");
        await TestIntelligentSearchService(serviceProvider);
        Console.WriteLine();
        
        Console.WriteLine("✅ AI Enhanced Search Test Complete!");
        Console.ReadKey();
    }
    
    static async Task TestOllamaConnection(ServiceProvider serviceProvider)
    {
        try
        {
            var ollamaService = serviceProvider.GetRequiredService<IOllamaService>();
            Console.WriteLine($"   OllamaService Type: {ollamaService.GetType().Name}");
            
            var testPrompt = "What is the meaning of 'Bismillah'?";
            Console.WriteLine($"   Testing prompt: '{testPrompt}'");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await ollamaService.GenerateResponseAsync("mistral:7b", testPrompt);
            stopwatch.Stop();
            
            Console.WriteLine($"   ✅ Direct Ollama Response (in {stopwatch.ElapsedMilliseconds}ms):");
            Console.WriteLine($"   📝 Response length: {response?.Length ?? 0} characters");
            
            if (!string.IsNullOrEmpty(response))
            {
                var preview = response.Length > 200 ? response.Substring(0, 200) + "..." : response;
                Console.WriteLine($"   📄 Preview: {preview}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error testing Ollama: {ex.Message}");
        }
    }
    
    static async Task TestIntelligentSearchService(ServiceProvider serviceProvider)
    {
        try
        {
            var intelligentSearch = serviceProvider.GetRequiredService<IIntelligentSearchService>();
            Console.WriteLine($"   IntelligentSearchService Type: {intelligentSearch.GetType().Name}");
            
            var testQueries = new[]
            {
                "What is prayer in Islam?",
                "Tell me about the Five Pillars of Islam",
                "What is the importance of charity in Islam?"
            };
            
            foreach (var query in testQueries)
            {
                Console.WriteLine($"   🔍 Testing query: '{query}'");
                
                try
                {
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    var aiResponse = await intelligentSearch.GetAIKnowledgeAsync(query, "en");
                    stopwatch.Stop();
                    
                    if (aiResponse != null)
                    {
                        Console.WriteLine($"   ✅ AI Knowledge Response (in {stopwatch.ElapsedMilliseconds}ms):");
                        Console.WriteLine($"      📝 Response: {aiResponse.Response?.Length ?? 0} chars");
                        Console.WriteLine($"      📖 Quran Refs: {aiResponse.QuranReferences?.Count ?? 0}");
                        Console.WriteLine($"      📚 Hadith Refs: {aiResponse.HadithReferences?.Count ?? 0}");
                        Console.WriteLine($"      👨‍🎓 Scholar Opinions: {aiResponse.ScholarlyOpinions?.Count ?? 0}");
                        Console.WriteLine($"      🎯 Confidence: {aiResponse.ConfidenceScore:F2}");
                        
                        if (!string.IsNullOrEmpty(aiResponse.Response))
                        {
                            var preview = aiResponse.Response.Length > 150 ? 
                                aiResponse.Response.Substring(0, 150) + "..." : 
                                aiResponse.Response;
                            Console.WriteLine($"      📄 Preview: {preview}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"   ⚠️ No AI response received for query: '{query}'");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ Error with query '{query}': {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"      Inner: {ex.InnerException.Message}");
                    }
                }
                
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error testing IntelligentSearchService: {ex.Message}");
        }
    }
    
    static ServiceProvider CreateServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        
        // Add configuration
        services.AddSingleton<IConfiguration>(configuration);
        
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        // Add Islamic Knowledge Base services
        services.AddIslamicKnowledgeBase(configuration);
        
        return services.BuildServiceProvider();
    }
}
