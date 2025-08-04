using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Muwasala.Core.Services;
using Muwasala.Agents;
using Muwasala.KnowledgeBase.Services;
using Muwasala.KnowledgeBase.Extensions;
using Muwasala.Core.Models;

// Additional using statements for service interfaces
using Muwasala.KnowledgeBase.Services;

namespace TestAllAgents;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🕌 Testing All Muwasala Agents");
        Console.WriteLine("================================");

        var serviceProvider = CreateServiceProvider();

        try
        {
            await TestQuranNavigatorAgent(serviceProvider);
            await TestHadithVerifierAgent(serviceProvider);
            await TestFiqhAdvisorAgent(serviceProvider);
            await TestDuaCompanionAgent(serviceProvider);
            await TestTajweedTutorAgent(serviceProvider);
            await TestSirahScholarAgent(serviceProvider);

            Console.WriteLine("\n✅ All agent tests completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        services.AddSingleton<IConfiguration>(configuration);
        
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add core services with mock implementations
        services.AddTransient<IOllamaService, MockOllamaService>();
        
        // Add knowledge base services with mock implementations
        services.AddTransient<IQuranService, MockQuranService>();
        services.AddTransient<IHadithService, MockHadithService>();
        services.AddTransient<IFiqhService, MockFiqhService>();
        services.AddTransient<IDuaService, MockDuaService>();
        services.AddTransient<ITajweedService, MockTajweedService>();
        services.AddTransient<ISirahService, MockSirahService>();
        
        // Add agents
        services.AddTransient<QuranNavigatorAgent>();
        services.AddTransient<HadithVerifierAgent>();
        services.AddTransient<FiqhAdvisorAgent>();
        services.AddTransient<DuaCompanionAgent>();
        services.AddTransient<TajweedTutorAgent>();
        services.AddTransient<SirahScholarAgent>();

        return services.BuildServiceProvider();
    }

    static async Task TestQuranNavigatorAgent(ServiceProvider serviceProvider)
    {
        Console.WriteLine("\n📖 Testing QuranNavigatorAgent...");
        var agent = serviceProvider.GetRequiredService<QuranNavigatorAgent>();
        var response = await agent.GetVerseAsync("guidance in difficulty");
        Console.WriteLine($"✅ QuranNavigator: {response.Verse} - {response.Translation?[..50]}...");
    }

    static async Task TestHadithVerifierAgent(ServiceProvider serviceProvider)
    {
        Console.WriteLine("\n📜 Testing HadithVerifierAgent...");
        var agent = serviceProvider.GetRequiredService<HadithVerifierAgent>();
        var response = await agent.VerifyHadithAsync("The world is green and beautiful");
        Console.WriteLine($"✅ HadithVerifier: Grade {response.Grade} - {response.Collection}");
    }

    static async Task TestFiqhAdvisorAgent(ServiceProvider serviceProvider)
    {
        Console.WriteLine("\n⚖️ Testing FiqhAdvisorAgent...");
        var agent = serviceProvider.GetRequiredService<FiqhAdvisorAgent>();
        var response = await agent.GetRulingAsync("Can I pray while traveling?", Madhab.Hanafi);
        Console.WriteLine($"✅ FiqhAdvisor: {response.Madhab} ruling - {response.Ruling?[..50]}...");
    }

    static async Task TestDuaCompanionAgent(ServiceProvider serviceProvider)
    {
        Console.WriteLine("\n🤲 Testing DuaCompanionAgent...");
        var agent = serviceProvider.GetRequiredService<DuaCompanionAgent>();
        var responses = await agent.GetDuasForOccasionAsync("morning");
        Console.WriteLine($"✅ DuaCompanion: Found {responses.Count} du'as for morning");
    }

    static async Task TestTajweedTutorAgent(ServiceProvider serviceProvider)
    {
        Console.WriteLine("\n📚 Testing TajweedTutorAgent...");
        var agent = serviceProvider.GetRequiredService<TajweedTutorAgent>();
        var response = await agent.AnalyzeVerseAsync(new VerseReference(1, 1));
        Console.WriteLine($"✅ TajweedTutor: Found {response.Rules.Count} rules for Al-Fatiha verse 1");
    }

    static async Task TestSirahScholarAgent(ServiceProvider serviceProvider)
    {
        Console.WriteLine("\n🏛️ Testing SirahScholarAgent...");
        var agent = serviceProvider.GetRequiredService<SirahScholarAgent>();
        var response = await agent.GetGuidanceFromSirahAsync("leadership");
        Console.WriteLine($"✅ SirahScholar: Guidance on {response.Topic} from {response.Event}");
    }
}

// Mock service implementations for testing
public class MockOllamaService : IOllamaService
{
    public Task<string> GenerateResponseAsync(string model, string prompt, double temperature = 0.7)
    {
        return Task.FromResult("Mock AI response");
    }

    public Task<T> GenerateStructuredResponseAsync<T>(string model, string prompt, double temperature = 0.7)
    {
        // Return default instances for various types
        if (typeof(T) == typeof(List<VerseContext>))
            return Task.FromResult((T)(object)new List<VerseContext>());
        
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
            return Task.FromResult((T)Activator.CreateInstance(typeof(T)));
        
        // Try to create default instances for records/classes
        try
        {
            return Task.FromResult(Activator.CreateInstance<T>());
        }
        catch
        {
            return Task.FromResult(default(T));
        }
    }

    public Task<bool> IsModelAvailableAsync(string modelName)
    {
        return Task.FromResult(true);
    }

    public Task<List<string>> GetAvailableModelsAsync()
    {
        return Task.FromResult(new List<string> { "deepseek-r1" });
    }
}

public class MockQuranService : IQuranService
{
    public Task<List<QuranVerse>> SearchVersesByContextAsync(string context, string language = "en")
    {
        return Task.FromResult(new List<QuranVerse>
        {
            new QuranVerse(1, 1, "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ", 
                "In the name of Allah, the Most Gracious, the Most Merciful", 
                "Bismillahir Rahmanir Raheem", language)
        });
    }

    public Task<List<QuranVerse>> SearchVersesByThemeAsync(string theme, string language = "en", int maxResults = 10)
    {
        return Task.FromResult(new List<QuranVerse>
        {
            new QuranVerse(1, 1, "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ", 
                "In the name of Allah, the Most Gracious, the Most Merciful", 
                "Bismillahir Rahmanir Raheem", language)
        });
    }

    public Task<QuranVerse?> GetVerseAsync(VerseReference verse, string language = "en")
    {
        return Task.FromResult<QuranVerse?>(
            new QuranVerse(verse.Surah, verse.Verse, "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ", 
                "In the name of Allah, the Most Gracious, the Most Merciful", 
                "Bismillahir Rahmanir Raheem", language));
    }

    public Task<List<QuranVerse>> GetSurahAsync(int surahNumber, string language = "en")
    {
        return Task.FromResult(new List<QuranVerse>());
    }

    public Task<List<TafsirEntry>> GetTafsirAsync(VerseReference verse, string source = "IbnKathir")
    {
        return Task.FromResult(new List<TafsirEntry>
        {
            new TafsirEntry(verse, source, "Sample tafsir commentary", "Ibn Kathir")
        });
    }
}

public class MockHadithService : IHadithService
{
    public Task<List<HadithRecord>> SearchHadithAsync(string text, string language = "en")
    {
        return Task.FromResult(new List<HadithRecord>
        {
            new HadithRecord("Test hadith", "Test hadith translation", HadithGrade.Sahih, "Bukhari", 
                "1", "1", new List<string> { "Test narrator" }, "Test topic", language)
        });
    }

    public Task<List<HadithRecord>> GetHadithByTopicAsync(string topic, string language = "en", int maxResults = 10)
    {
        return Task.FromResult(new List<HadithRecord>
        {
            new HadithRecord("Test hadith", "Test hadith translation", HadithGrade.Sahih, "Bukhari", 
                "1", "1", new List<string> { "Test narrator" }, topic, language)
        });
    }

    public Task<HadithRecord?> GetHadithByReferenceAsync(string collection, string hadithNumber)
    {
        return Task.FromResult<HadithRecord?>(
            new HadithRecord("Test hadith", "Test hadith translation", HadithGrade.Sahih, collection, 
                "1", hadithNumber, new List<string> { "Test narrator" }, "Test topic", "en"));
    }

    public Task<List<HadithRecord>> GetAuthenticHadithAsync(string topic, int maxResults = 5)
    {
        return Task.FromResult(new List<HadithRecord>
        {
            new HadithRecord("Test hadith", "Test hadith translation", HadithGrade.Sahih, "Bukhari", 
                "1", "1", new List<string> { "Test narrator" }, topic, "en")
        });
    }
}

public class MockFiqhService : IFiqhService
{
    public Task<List<FiqhRuling>> SearchRulingsAsync(string question, Madhab madhab, string language = "en")
    {
        return Task.FromResult(new List<FiqhRuling>
        {
            new FiqhRuling(question, "Test ruling", madhab, "Test evidence", "Test source", 
                new List<string> { "Test scholar" }, language)
        });
    }

    public Task<List<FiqhRuling>> GetRulingsByTopicAsync(string topic, Madhab madhab, string language = "en")
    {
        return Task.FromResult(new List<FiqhRuling>
        {
            new FiqhRuling(topic, "Test ruling", madhab, "Test evidence", "Test source", 
                new List<string> { "Test scholar" }, language)
        });
    }

    public Task<List<FiqhComparison>> CompareMadhabRulingsAsync(string topic, string language = "en")
    {
        return Task.FromResult(new List<FiqhComparison>
        {
            new FiqhComparison(topic, new Dictionary<Madhab, string> { { Madhab.Hanafi, "Test ruling" } }, 
                "Common ground", new List<string> { "Key difference" }, language)
        });
    }
}

public class MockDuaService : IDuaService
{
    public Task<List<DuaRecord>> SearchDuasByOccasionAsync(string occasion, string language = "en", int maxResults = 10)
    {
        return Task.FromResult(new List<DuaRecord>
        {
            new DuaRecord("Test dua", "Test translation", "Test transliteration", occasion, "Quran", "Test benefits", language)
        });
    }

    public Task<DuaRecord?> GetSpecificPrayerAsync(SpecificPrayer prayerType, string language = "en")
    {
        return Task.FromResult<DuaRecord?>(
            new DuaRecord("Test prayer", "Test translation", "Test transliteration", prayerType.ToString(), "Quran", "Test benefits", language));
    }

    public Task<List<DuaRecord>> GetDailyDuasAsync(string timeOfDay, string language = "en")
    {
        return Task.FromResult(new List<DuaRecord>
        {
            new DuaRecord("Daily dua", "Daily translation", "Daily transliteration", timeOfDay, "Sunnah", "Daily benefits", language)
        });
    }
}

public class MockTajweedService : ITajweedService
{
    public Task<VerseData?> GetVerseWithTajweedAsync(VerseReference verse)
    {
        return Task.FromResult<VerseData?>(
            new VerseData(verse.Surah, verse.Verse, "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ", 
                "In the name of Allah, the Most Gracious, the Most Merciful", 
                new List<TajweedMarker>
                {
                    new TajweedMarker("Iqlab", 5, 8, "Change 'nun' sound before 'ba' to 'meem'")
                }));
    }

    public Task<List<CommonMistake>> GetCommonMistakesAsync(VerseReference verse)
    {
        return Task.FromResult(new List<CommonMistake>
        {
            new CommonMistake("Pronunciation", "Mispronouncing letters", "Typical error", "Correct pronunciation")
        });
    }

    public Task<SurahData?> GetSurahForLessonAsync(int surahNumber, RecitationLevel level)
    {
        return Task.FromResult<SurahData?>(
            new SurahData(surahNumber, "Al-Fatiha", "The Opening", 7, "Mecca", new List<string> { "Opening", "Prayer" }));
    }

    public Task<QiraatData?> GetQiraatDataAsync(VerseReference verse, QiraatType qiraatType)
    {
        return Task.FromResult<QiraatData?>(
            new QiraatData(verse, qiraatType, new List<string> { "Standard variation" }, new List<string> { "audio.mp3" }));
    }
}

public class MockSirahService : ISirahService
{
    public Task<List<SirahEvent>> SearchEventsByContextAsync(string context, string language = "en")
    {
        return Task.FromResult(new List<SirahEvent>
        {
            new SirahEvent("Test Event", "Test Description", SirahPeriod.EarlyMedina, "Medina", "Leadership", "en")
        });
    }

    public Task<List<PropheticGuidance>> GetGuidanceByTopicAsync(string topic, string language = "en")
    {
        return Task.FromResult(new List<PropheticGuidance>());
    }

    public Task<List<SirahEvent>> GetEventsByPeriodAsync(SirahPeriod period, string language = "en")
    {
        return Task.FromResult(new List<SirahEvent>());
    }

    public Task<List<PropheticCharacteristic>> GetCharacteristicsAsync(string aspect, string language = "en")
    {
        return Task.FromResult(new List<PropheticCharacteristic>());
    }    public Task<ChronologicalTimeline> GetTimelineAsync(string language = "en")
    {
        return Task.FromResult(new ChronologicalTimeline(new List<SirahEvent>(), new List<SirahEvent>(), new List<SirahEvent>()));
    }
}

// Additional record types needed for mock implementations that are not defined elsewhere

public record BasicTajweedRule(
    string Name,
    int StartPosition,
    int EndPosition
);

public record PropheticGuidance();

public record ChronologicalTimeline(
    List<SirahEvent> MeccanPeriod,
    List<SirahEvent> MedinanPeriod,
    List<SirahEvent> MajorEvents
);
