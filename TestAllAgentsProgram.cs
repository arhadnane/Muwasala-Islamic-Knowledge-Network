using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Muwasala.Core.Services;
using Muwasala.Agents;
using Muwasala.KnowledgeBase.Services;
using Muwasala.Core.Models;

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
        Console.WriteLine($"✅ QuranNavigator: {response.Verse} - {response.Translation?[..Math.Min(50, response.Translation.Length)]}...");
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
        Console.WriteLine($"✅ FiqhAdvisor: {response.Madhab} ruling - {response.Ruling?[..Math.Min(50, response.Ruling.Length)]}...");
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
        return Task.FromResult("Mock AI response with helpful Islamic guidance based on the request.");
    }

    public Task<T> GenerateStructuredResponseAsync<T>(string model, string prompt, double temperature = 0.7)
    {
        // Return default instances for various types
        if (typeof(T) == typeof(List<VerseContext>))
            return Task.FromResult((T)(object)new List<VerseContext>());
        
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
            return Task.FromResult((T)Activator.CreateInstance(typeof(T))!);
        
        // Try to create default instances for records/classes
        try
        {
            return Task.FromResult(Activator.CreateInstance<T>());
        }
        catch
        {
            return Task.FromResult(default(T)!);
        }
    }

    public Task<bool> IsModelAvailableAsync(string modelName)
    {
        return Task.FromResult(true);
    }

    public Task<List<string>> GetAvailableModelsAsync()
    {
        return Task.FromResult(new List<string> { "phi3:mini" });
    }
}

public class MockQuranService : IQuranService
{
    public Task<List<VerseRecord>> SearchVersesByContextAsync(string context, string language = "en", int maxResults = 5)
    {
        return Task.FromResult(new List<VerseRecord>
        {
            new VerseRecord("Al-Fatiha", 1, 1, "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ", 
                "In the name of Allah, the Most Gracious, the Most Merciful", "IbnKathir tafsir", "en")
        });
    }

    public Task<VerseRecord?> GetVerseAsync(int surah, int verse, string language = "en")
    {
        return Task.FromResult<VerseRecord?>(
            new VerseRecord("Al-Fatiha", 1, 1, "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ", 
                "In the name of Allah, the Most Gracious, the Most Merciful", "IbnKathir tafsir", "en"));
    }

    public Task<List<VerseRecord>> GetSurahAsync(int surahNumber, string language = "en")
    {
        return Task.FromResult(new List<VerseRecord>());
    }
}

public class MockHadithService : IHadithService
{
    public Task<List<HadithRecord>> SearchHadithAsync(string searchText, string language = "en", int maxResults = 5)
    {
        return Task.FromResult(new List<HadithRecord>
        {
            new HadithRecord("The world is green and beautiful", "The world is green and beautiful hadith translation", "Bukhari", "1", HadithGrade.Sahih, "Abu Huraira", "en")
        });
    }

    public Task<HadithRecord?> VerifyHadithAsync(string hadithText, string language = "en")
    {
        return Task.FromResult<HadithRecord?>(
            new HadithRecord("The world is green and beautiful", "The world is green and beautiful hadith translation", "Bukhari", "1", HadithGrade.Sahih, "Abu Huraira", "en"));
    }
}

public class MockFiqhService : IFiqhService
{
    public Task<List<FiqhRuling>> SearchRulingsAsync(string question, Madhab madhab = Madhab.Hanafi, string language = "en", int maxResults = 3)
    {
        return Task.FromResult(new List<FiqhRuling>
        {
            new FiqhRuling("Can I pray while traveling?", "Yes, travelers may shorten and combine prayers according to Hanafi fiqh", madhab, "Quran and Sunnah evidence", new Dictionary<Madhab, string>(), "en")
        });
    }

    public Task<FiqhRuling?> GetRulingByTopicAsync(string topic, Madhab madhab = Madhab.Hanafi, string language = "en")
    {
        return Task.FromResult<FiqhRuling?>(
            new FiqhRuling("Travel prayer question", "Travelers have concessions in Islamic law", madhab, "Quranic and Prophetic guidance", new Dictionary<Madhab, string>(), "en"));
    }
}

public class MockDuaService : IDuaService
{
    public Task<List<DuaRecord>> SearchDuasByOccasionAsync(string occasion, string language = "en", int maxResults = 3)
    {
        return Task.FromResult(new List<DuaRecord>
        {
            new DuaRecord("أَصْبَحْنَا وَأَصْبَحَ الْمُلْكُ لِلَّهِ", "We have reached the morning and with it the dominion of Allah", "Asbahna wa asbaha al-mulku lillah", occasion, "Muslim", "en")
        });
    }

    public Task<DuaRecord?> GetSpecificPrayerAsync(SpecificPrayer prayerType, string language = "en")
    {
        return Task.FromResult<DuaRecord?>(
            new DuaRecord("رَبِّ اغْفِرْ لِي", "My Lord, forgive me", "Rabbi ghfir li", prayerType.ToString(), "Quran", "en"));
    }
}

public class MockTajweedService : ITajweedService
{
    public Task<VerseData?> GetVerseWithTajweedAsync(VerseReference verse)
    {
        return Task.FromResult<VerseData?>(
            new VerseData(verse.Surah, verse.Verse, "Al-Fatiha", "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ", 
                "In the name of Allah", new List<BasicTajweedRule>
                {
                    new BasicTajweedRule("Basmalah Pronunciation", 0, 10)
                }));
    }

    public Task<SurahData?> GetSurahForLessonAsync(int surahNumber, RecitationLevel level)
    {
        return Task.FromResult<SurahData?>(
            new SurahData(1, "Al-Fatiha", 7));
    }

    public Task<List<CommonMistake>> GetCommonMistakesAsync(VerseReference verse)
    {
        return Task.FromResult(new List<CommonMistake>());
    }

    public Task<QiraatData> GetQiraatDataAsync(VerseReference verse, QiraatType qiraatType)
    {
        return Task.FromResult(new QiraatData(verse, new List<string>(), new List<string>()));
    }
}

public class MockSirahService : ISirahService
{
    public Task<List<SirahEvent>> SearchEventsByContextAsync(string context, string language = "en")
    {
        return Task.FromResult(new List<SirahEvent>
        {
            new SirahEvent("Leadership in Medina", "Prophet's exemplary leadership during the establishment of the first Islamic state", SirahPeriod.EarlyMedina, "Medina", "Leadership", "en")
        });
    }

    public Task<List<PropheticGuidance>> GetGuidanceByTopicAsync(string topic, string language = "en")
    {
        return Task.FromResult(new List<PropheticGuidance>
        {
            new PropheticGuidance()
        });
    }

    public Task<List<SirahEvent>> GetEventsByPeriodAsync(SirahPeriod period, string language = "en")
    {
        return Task.FromResult(new List<SirahEvent>());
    }

    public Task<List<PropheticCharacteristic>> GetCharacteristicsAsync(string aspect, string language = "en")
    {
        return Task.FromResult(new List<PropheticCharacteristic>());
    }

    public Task<ChronologicalTimeline> GetTimelineAsync(string language = "en")
    {
        return Task.FromResult(new ChronologicalTimeline(new List<SirahEvent>(), new List<SirahEvent>(), new List<SirahEvent>()));
    }
}

// Additional record types needed for mock implementations
public record VerseRecord(
    string SurahName,
    int SurahNumber,
    int VerseNumber,
    string ArabicText,
    string Translation,
    string? Tafsir = null,
    string Language = "en"
);

public record HadithRecord(
    string ArabicText,
    string Translation,
    string Collection,
    string? HadithNumber = null,
    HadithGrade Grade = HadithGrade.Sahih,
    string? Narrator = null,
    string Language = "en"
);

public record FiqhRuling(
    string Question,
    string Ruling,
    Madhab Madhab,
    string Evidence,
    Dictionary<Madhab, string> OtherMadhabRulings,
    string Language = "en"
);

public record DuaRecord(
    string ArabicText,
    string Translation,
    string Transliteration,
    string Occasion,
    string Source,
    string Language = "en"
);

public record VerseData(
    int Surah,
    int Verse,
    string SurahName,
    string ArabicText,
    string Translation,
    List<BasicTajweedRule> TajweedRules
);

public record BasicTajweedRule(
    string Name,
    int StartPosition,
    int EndPosition
);

public record SurahData(
    int Number,
    string Name,
    int VerseCount
);

public record CommonMistake(
    string Type,
    string Description,
    string TypicalError
);

public record QiraatData(
    VerseReference Verse,
    List<string> Variations,
    List<string> AudioReferences
);

public record SirahEvent(
    string Name,
    string Description,
    SirahPeriod Period,
    string? Location,
    string Category,
    string Language = "en"
);

public record PropheticCharacteristic(
    string Aspect,
    string Description,
    List<string> Examples = default!,
    string Language = "en"
)
{
    public List<string> Examples { get; init; } = Examples ?? new List<string>();
}

public record PropheticGuidance();

public record ChronologicalTimeline(
    List<SirahEvent> MeccanPeriod,
    List<SirahEvent> MedinanPeriod,
    List<SirahEvent> MajorEvents
);
