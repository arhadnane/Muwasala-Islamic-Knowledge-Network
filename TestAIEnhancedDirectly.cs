using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muwasala.Core.Services;
using Muwasala.KnowledgeBase.Services;
using Muwasala.KnowledgeBase.Extensions;

namespace Muwasala.TestAIEnhanced;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🔍 Testing AI Enhanced Search Functionality Directly...\n");
        
        var serviceProvider = CreateServiceProvider();
        
        try
        {
            // Test 1: Direct Ollama Service Test
            await TestOllamaService(serviceProvider);
            
            // Test 2: Intelligent Search Service Test  
            await TestIntelligentSearchService(serviceProvider);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fatal error: {ex.Message}");
            Console.WriteLine($"   Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("\n✅ Test completed. Press any key to exit...");
        Console.ReadKey();
    }
    
    static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Add HTTP client for Ollama
        services.AddHttpClient<IOllamaService, OllamaService>();
        
        // Register core services
        services.AddSingleton<IOllamaService, OllamaService>();
        
        // Register Islamic Knowledge Base services (minimal config for testing)
        services.AddScoped<IGlobalSearchService, GlobalSearchService>();
        services.AddScoped<IIntelligentSearchService, IntelligentSearchService>();
        
        // Register Islamic services with minimal dependencies
        services.AddScoped<IQuranService, QuranService>();
        services.AddScoped<IHadithService, HadithService>();
        services.AddScoped<IFiqhService, FiqhService>();
        services.AddScoped<IDuaService, DuaService>();
        services.AddScoped<ITajweedService, TajweedService>();
        services.AddScoped<ISirahService, SirahService>();
        
        return services.BuildServiceProvider();
    }
    
    static async Task TestOllamaService(ServiceProvider serviceProvider)
    {
        Console.WriteLine("🧪 Testing Ollama Service...");
        
        try
        {
            var ollamaService = serviceProvider.GetRequiredService<IOllamaService>();
            Console.WriteLine($"   ✅ Ollama service resolved: {ollamaService.GetType().Name}");
            
            // Test simple prompt
            var testPrompt = "What is Islam?";
            Console.WriteLine($"   📝 Testing prompt: '{testPrompt}'");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await ollamaService.GenerateResponseAsync("mistral:7b", testPrompt);
            stopwatch.Stop();
            
            Console.WriteLine($"   ⏱️  Response time: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"   📄 Response length: {response?.Length ?? 0} characters");
            
            if (!string.IsNullOrEmpty(response))
            {
                var preview = response.Length > 100 ? response.Substring(0, 100) + "..." : response;
                Console.WriteLine($"   📋 Response preview: {preview}");
                Console.WriteLine("   ✅ Ollama service working correctly!");
            }
            else
            {
                Console.WriteLine("   ❌ Empty response from Ollama service");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Ollama service test failed: {ex.Message}");
            Console.WriteLine($"   📋 Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine();
    }
    
    static async Task TestIntelligentSearchService(ServiceProvider serviceProvider)
    {
        Console.WriteLine("🧪 Testing IntelligentSearchService...");
        
        try
        {
            var intelligentSearchService = serviceProvider.GetRequiredService<IIntelligentSearchService>();
            Console.WriteLine($"   ✅ IntelligentSearchService resolved: {intelligentSearchService.GetType().Name}");
            
            // Test AI Enhanced search
            var testQuery = "What is the meaning of Bismillah?";
            Console.WriteLine($"   📝 Testing AI Enhanced search with query: '{testQuery}'");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var aiResponse = await intelligentSearchService.GetAIKnowledgeAsync(testQuery, "en");
            stopwatch.Stop();
            
            Console.WriteLine($"   ⏱️  Response time: {stopwatch.ElapsedMilliseconds}ms");
            
            if (aiResponse != null)
            {
                Console.WriteLine($"   ✅ AI Enhanced search successful!");
                Console.WriteLine($"   📋 Response: {aiResponse.Response.Substring(0, Math.Min(200, aiResponse.Response.Length))}...");
                Console.WriteLine($"   📖 Quran refs: {string.Join(", ", aiResponse.QuranReferences)}");
                Console.WriteLine($"   📚 Hadith refs: {string.Join(", ", aiResponse.HadithReferences)}");
                Console.WriteLine($"   👨‍🎓 Scholars: {string.Join(", ", aiResponse.ScholarlyOpinions)}");
                Console.WriteLine($"   🎯 Confidence: {aiResponse.ConfidenceScore:F2}");
            }
            else
            {
                Console.WriteLine("   ❌ AI Enhanced search returned null");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ IntelligentSearchService test failed: {ex.Message}");
            Console.WriteLine($"   📋 Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine();
    }
}
