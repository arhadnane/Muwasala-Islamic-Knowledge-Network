using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Muwasala.Core.Services;
using Muwasala.KnowledgeBase.Services;
using Muwasala.KnowledgeBase.Extensions;
using Muwasala.KnowledgeBase.Data;
using Microsoft.EntityFrameworkCore;

// Debug script to test search functionality
var builder = Host.CreateDefaultBuilder();

builder.ConfigureServices((context, services) =>
{
    // Add basic services
    services.AddHttpClient();
    services.AddSingleton<IOllamaService, OllamaService>();
    services.AddSingleton<ICacheService, MemoryCacheService>();
    
    // Add Islamic Knowledge Base services with database backend
    services.AddIslamicKnowledgeBase(context.Configuration, useDatabaseServices: true);
    
    // Add Intelligent Search Service
    services.AddScoped<IIntelligentSearchService, IntelligentSearchService>();
});

var host = builder.Build();

// Initialize database
Console.WriteLine("Initializing database...");
await host.Services.InitializeIslamicKnowledgeDatabaseAsync();

using var scope = host.Services.CreateScope();
var serviceProvider = scope.ServiceProvider;

try
{
    // Test database connection
    Console.WriteLine("Testing database connection...");
    var dbContext = serviceProvider.GetRequiredService<IslamicKnowledgeDbContext>();
    var canConnect = await dbContext.Database.CanConnectAsync();
    Console.WriteLine($"Database connection: {(canConnect ? "SUCCESS" : "FAILED")}");
    
    if (canConnect)
    {
        // Check if tables have data
        var quranCount = await dbContext.QuranVerses.CountAsync();
        var hadithCount = await dbContext.Hadiths.CountAsync();
        var fiqhCount = await dbContext.FiqhRulings.CountAsync();
        
        Console.WriteLine($"Database contents:");
        Console.WriteLine($"  Quran verses: {quranCount}");
        Console.WriteLine($"  Hadiths: {hadithCount}");
        Console.WriteLine($"  Fiqh rulings: {fiqhCount}");
    }
    
    // Test GlobalSearchService directly
    Console.WriteLine("\nTesting GlobalSearchService...");
    var globalSearchService = serviceProvider.GetRequiredService<IGlobalSearchService>();
    Console.WriteLine($"GlobalSearchService type: {globalSearchService.GetType().Name}");
    
    var searchResult = await globalSearchService.SearchAllAsync("prayer", "en", 5);
    Console.WriteLine($"Search results count: {searchResult.Results.Count}");
    
    if (searchResult.Results.Any())
    {
        Console.WriteLine("Sample results:");
        foreach (var result in searchResult.Results.Take(3))
        {
            Console.WriteLine($"  - {result.Source}: {result.Title}");
        }
    }
    else
    {
        Console.WriteLine("No results found - investigating...");
        
        // Check individual services
        var quranService = serviceProvider.GetRequiredService<IQuranService>();
        var hadithService = serviceProvider.GetRequiredService<IHadithService>();
        var fiqhService = serviceProvider.GetRequiredService<IFiqhService>();
        
        Console.WriteLine($"QuranService type: {quranService.GetType().Name}");
        Console.WriteLine($"HadithService type: {hadithService.GetType().Name}");
        Console.WriteLine($"FiqhService type: {fiqhService.GetType().Name}");
        
        // Test individual service searches
        var quranResults = await quranService.SearchVersesAsync("prayer", "en");
        var hadithResults = await hadithService.SearchHadithsAsync("prayer", "en");
        var fiqhResults = await fiqhService.SearchRulingsAsync("prayer", "en");
        
        Console.WriteLine($"Individual service results:");
        Console.WriteLine($"  Quran: {quranResults.Count()}");
        Console.WriteLine($"  Hadith: {hadithResults.Count()}");
        Console.WriteLine($"  Fiqh: {fiqhResults.Count()}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

Console.WriteLine("\nTest completed. Press any key to exit...");
Console.ReadKey();
