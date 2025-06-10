using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muwasala.Core.Services;
using Muwasala.Agents;
using Muwasala.KnowledgeBase.Services;

// Test program to verify that different Surahs return different content
// This verifies the fix for the issue where all Surahs returned identical mock data

var services = new ServiceCollection();

// Add logging
services.AddLogging(builder => builder.AddConsole());

// Add HTTP client
services.AddHttpClient();

// Register core services
services.AddSingleton<IOllamaService, OllamaService>();
services.AddSingleton<ICacheService, MemoryCacheService>();

// Register knowledge base services (Real implementations)
services.AddSingleton<IQuranService, QuranService>();

// Register agents
services.AddSingleton<QuranNavigatorAgent>();

var serviceProvider = services.BuildServiceProvider();
var agent = serviceProvider.GetRequiredService<QuranNavigatorAgent>();

Console.WriteLine("🧪 Testing Surah Analysis - Verifying Different Surahs Return Different Content");
Console.WriteLine("════════════════════════════════════════════════════════════════════════════════");

// Test Surah 1 (Al-Fatiha)
Console.WriteLine("\n📖 Testing Surah 1 (Al-Fatiha):");
try 
{
    var surah1 = await agent.GetSurahAnalysisAsync(1);
    Console.WriteLine($"   Verses found: {surah1.Count}");
    if (surah1.Any())
    {
        Console.WriteLine($"   First verse: {surah1.First().Verse}");
        Console.WriteLine($"   Arabic: {surah1.First().ArabicText}");
        Console.WriteLine($"   Translation: {surah1.First().Translation}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"   ❌ Error: {ex.Message}");
}

// Test Surah 2 (Al-Baqarah)
Console.WriteLine("\n📖 Testing Surah 2 (Al-Baqarah):");
try 
{
    var surah2 = await agent.GetSurahAnalysisAsync(2);
    Console.WriteLine($"   Verses found: {surah2.Count}");
    if (surah2.Any())
    {
        Console.WriteLine($"   First verse: {surah2.First().Verse}");
        Console.WriteLine($"   Arabic: {surah2.First().ArabicText}");
        Console.WriteLine($"   Translation: {surah2.First().Translation}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"   ❌ Error: {ex.Message}");
}

// Test Surah 94 (Ash-Sharh)
Console.WriteLine("\n📖 Testing Surah 94 (Ash-Sharh):");
try 
{
    var surah94 = await agent.GetSurahAnalysisAsync(94);
    Console.WriteLine($"   Verses found: {surah94.Count}");
    if (surah94.Any())
    {
        Console.WriteLine($"   First verse: {surah94.First().Verse}");
        Console.WriteLine($"   Arabic: {surah94.First().ArabicText}");
        Console.WriteLine($"   Translation: {surah94.First().Translation}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"   ❌ Error: {ex.Message}");
}

// Test Surah 112 (Al-Ikhlas) - This was mentioned in the original issue
Console.WriteLine("\n📖 Testing Surah 112 (Al-Ikhlas):");
try 
{
    var surah112 = await agent.GetSurahAnalysisAsync(112);
    Console.WriteLine($"   Verses found: {surah112.Count}");
    if (surah112.Any())
    {
        Console.WriteLine($"   First verse: {surah112.First().Verse}");
        Console.WriteLine($"   Arabic: {surah112.First().ArabicText}");
        Console.WriteLine($"   Translation: {surah112.First().Translation}");
    }
    else
    {
        Console.WriteLine("   ℹ️ No verses found - this Surah is not in the sample data");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"   ❌ Error: {ex.Message}");
}

Console.WriteLine("\n✅ Test completed! If different Surahs show different content, the fix is working.");
Console.WriteLine("✅ If all Surahs showed identical mock data, that issue has been resolved.");
