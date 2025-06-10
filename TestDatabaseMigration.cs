using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muwasala.KnowledgeBase.Extensions;
using Muwasala.KnowledgeBase.Data;
using Muwasala.KnowledgeBase.Services;

/// <summary>
/// Test application for database migration and functionality
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Muwasala Islamic Knowledge Base - Database Migration Test ===\n");        // Setup configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:IslamicKnowledgeDb", "Data Source=d:\\Coding\\VSCodeProject\\Muwasala Islamic Knowledge Network\\data\\database\\IslamicKnowledge.db"}
            })
            .Build();

        // Setup dependency injection
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add Islamic Knowledge Base services
        services.AddIslamicKnowledgeBase(configuration);

        var serviceProvider = services.BuildServiceProvider();

        try
        {
            // Initialize database
            Console.WriteLine("1. Initializing database...");
            await serviceProvider.InitializeIslamicKnowledgeDatabaseAsync();
            Console.WriteLine("✓ Database initialized successfully!\n");

            // Get database statistics
            Console.WriteLine("2. Getting database statistics...");
            var initializer = serviceProvider.GetRequiredService<DatabaseInitializer>();
            var stats = await initializer.GetStatisticsAsync();
            
            Console.WriteLine($"   Database Statistics:");
            Console.WriteLine($"   - Quran Verses: {stats.QuranVersesCount}");
            Console.WriteLine($"   - Tafsir Entries: {stats.TafsirEntriesCount}");
            Console.WriteLine($"   - Hadith Records: {stats.HadithRecordsCount}");
            Console.WriteLine($"   - Fiqh Rulings: {stats.FiqhRulingsCount}");
            Console.WriteLine($"   - Du'a Records: {stats.DuaRecordsCount}");
            Console.WriteLine($"   - Sirah Events: {stats.SirahEventsCount}");
            Console.WriteLine($"   - Tajweed Rules: {stats.TajweedRulesCount}");
            Console.WriteLine($"   - Common Mistakes: {stats.CommonMistakesCount}");
            Console.WriteLine($"   - Verse Tajweed: {stats.VerseTajweedCount}");
            Console.WriteLine($"   - Total Records: {stats.TotalRecords}");
            Console.WriteLine($"   - Database Size: {stats.DatabaseSizeMB} MB\n");

            // Test services
            await TestQuranService(serviceProvider);
            await TestHadithService(serviceProvider);
            await TestGlobalSearch(serviceProvider);

            Console.WriteLine("🎉 All tests completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during testing: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            await serviceProvider.DisposeAsync();
        }
    }

    static async Task TestQuranService(ServiceProvider serviceProvider)
    {
        Console.WriteLine("3. Testing Quran Service...");
        var quranService = serviceProvider.GetRequiredService<IQuranService>();

        // Test verse search
        var verses = await quranService.SearchVersesByContextAsync("patience");
        Console.WriteLine($"   - Found {verses.Count} verses about patience");
          if (verses.Any())
        {
            var firstVerse = verses.First();
            Console.WriteLine($"   - Sample: {firstVerse.Surah} {firstVerse.Verse}: {firstVerse.Translation[..Math.Min(100, firstVerse.Translation.Length)]}...");
        }

        // Test theme search
        var mercyVerses = await quranService.SearchVersesByThemeAsync("mercy", maxResults: 5);
        Console.WriteLine($"   - Found {mercyVerses.Count} verses about mercy");
        
        Console.WriteLine("✓ Quran Service test completed\n");
    }

    static async Task TestHadithService(ServiceProvider serviceProvider)
    {
        Console.WriteLine("4. Testing Hadith Service...");
        var hadithService = serviceProvider.GetRequiredService<IHadithService>();

        // Test hadith search
        var hadiths = await hadithService.SearchHadithAsync("prayer");
        Console.WriteLine($"   - Found {hadiths.Count} hadiths about prayer");
        
        if (hadiths.Any())
        {
            var firstHadith = hadiths.First();
            Console.WriteLine($"   - Sample: {firstHadith.Collection} {firstHadith.HadithNumber} ({firstHadith.Grade})");
            Console.WriteLine($"     {firstHadith.Translation[..Math.Min(100, firstHadith.Translation.Length)]}...");
        }

        // Test authentic hadith
        var authenticHadiths = await hadithService.GetAuthenticHadithAsync("charity", 3);
        Console.WriteLine($"   - Found {authenticHadiths.Count} authentic hadiths about charity");
        
        Console.WriteLine("✓ Hadith Service test completed\n");
    }

    static async Task TestGlobalSearch(ServiceProvider serviceProvider)
    {
        Console.WriteLine("5. Testing Global Search Service...");
        var globalSearchService = serviceProvider.GetRequiredService<IGlobalSearchService>();        // Test global search
        var searchResults = await globalSearchService.SearchAllAsync("prayer", maxResults: 10);
        Console.WriteLine($"   - Global search for 'prayer' returned {searchResults.TotalResults} results");
        Console.WriteLine($"   - Categories found: {string.Join(", ", searchResults.ResultsByType.Keys)}");

        foreach (var category in searchResults.ResultsByType)
        {
            Console.WriteLine($"     * {category.Key}: {category.Value} results");
        }        if (searchResults.Results.Any())
        {
            var firstResult = searchResults.Results.First();
            Console.WriteLine($"   - Sample result: [{firstResult.Type}] {firstResult.Title}");
        }

        // Test search suggestions
        var suggestions = await globalSearchService.GetSearchSuggestionsAsync("char");
        Console.WriteLine($"   - Suggestions for 'char': {string.Join(", ", suggestions)}");
        
        Console.WriteLine("✓ Global Search Service test completed\n");
    }
}
