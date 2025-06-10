using Muwasala.Core.Models;
using Muwasala.KnowledgeBase.Services;

namespace Muwasala.Console;

// Mock implementations for console application
public class MockQuranService : IQuranService
{
    public async Task<List<QuranVerse>> SearchVersesByContextAsync(string context, string language = "en")
    {
        await Task.Delay(50);
        return new List<QuranVerse>
        {
            new QuranVerse(2, 286, "لَا يُكَلِّفُ اللَّهُ نَفْسًا إِلَّا وُسْعَهَا", "Allah does not charge a soul except [with that within] its capacity.")
        };
    }

    public async Task<List<QuranVerse>> SearchVersesByThemeAsync(string theme, string language = "en", int maxResults = 10)
    {
        await Task.Delay(50);
        return new List<QuranVerse>
        {
            new QuranVerse(94, 5, "فَإِنَّ مَعَ الْعُسْرِ يُسْرًا", "For indeed, with hardship [will be] ease.")
        };
    }

    public async Task<QuranVerse?> GetVerseAsync(VerseReference verse, string language = "en")
    {
        await Task.Delay(50);
        return new QuranVerse(verse.Surah, verse.Verse, "Sample Arabic text", "Sample translation");
    }

    public async Task<List<QuranVerse>> GetSurahAsync(int surahNumber, string language = "en")
    {
        await Task.Delay(50);
        return new List<QuranVerse>();
    }

    public async Task<List<TafsirEntry>> GetTafsirAsync(VerseReference verse, string source = "IbnKathir")
    {
        await Task.Delay(50);
        return new List<TafsirEntry>
        {
            new TafsirEntry(verse, source, "Sample tafsir commentary", "Ibn Kathir")
        };
    }
}

public class MockHadithService : IHadithService
{
    public async Task<List<HadithRecord>> SearchHadithAsync(string text, string language = "en")
    {
        await Task.Delay(50);
        return new List<HadithRecord>
        {
            new HadithRecord(
                "إِنَّمَا الْأَعْمَالُ بِالنِّيَّاتِ",
                "Actions are but by intention",
                HadithGrade.Sahih,
                "Bukhari",
                "1",
                "1"
            )
        };
    }

    public async Task<List<HadithRecord>> GetHadithByTopicAsync(string topic, string language = "en", int maxResults = 10)
    {
        await Task.Delay(50);
        return new List<HadithRecord>();
    }

    public async Task<HadithRecord?> GetHadithByReferenceAsync(string collection, string hadithNumber)
    {
        await Task.Delay(50);
        return null;
    }

    public async Task<List<HadithRecord>> GetAuthenticHadithAsync(string topic, int maxResults = 5)
    {
        await Task.Delay(50);
        return new List<HadithRecord>();
    }
}

public class MockFiqhService : IFiqhService
{
    public async Task<List<FiqhRuling>> SearchRulingsAsync(string question, Madhab madhab, string language = "en")
    {
        await Task.Delay(50);
        return new List<FiqhRuling>
        {
            new FiqhRuling(
                question,
                $"Sample ruling according to {madhab} madhab",
                madhab,
                "Quran and Sunnah",
                "Classical texts"
            )
        };
    }

    public async Task<List<FiqhRuling>> GetRulingsByTopicAsync(string topic, Madhab madhab, string language = "en")
    {
        await Task.Delay(50);
        return new List<FiqhRuling>();
    }

    public async Task<List<FiqhComparison>> CompareMadhabRulingsAsync(string topic, string language = "en")
    {
        await Task.Delay(50);
        return new List<FiqhComparison>();
    }
}

public class MockDuaService : IDuaService
{
    public async Task<List<DuaRecord>> SearchDuasByOccasionAsync(string occasion, string language = "en", int maxResults = 10)
    {
        await Task.Delay(50);
        return new List<DuaRecord>
        {
            new DuaRecord(
                "رَبَّنَا آتِنَا فِي الدُّنْيَا حَسَنَةً وَفِي الْآخِرَةِ حَسَنَةً وَقِنَا عَذَابَ النَّارِ",
                "Our Lord, give us good in this world and good in the hereafter",
                "Rabbana atina fi'd-dunya hasanatan",
                occasion,
                "Quran 2:201"
            )
        };
    }

    public async Task<DuaRecord?> GetSpecificPrayerAsync(SpecificPrayer prayerType, string language = "en")
    {
        await Task.Delay(50);
        return null;
    }

    public async Task<List<DuaRecord>> GetDailyDuasAsync(string timeOfDay, string language = "en")
    {
        await Task.Delay(50);
        return new List<DuaRecord>();
    }
}

public class MockTajweedService : ITajweedService
{
    public async Task<VerseData?> GetVerseWithTajweedAsync(VerseReference verse)
    {
        await Task.Delay(50);
        return new VerseData(
            verse.Surah,
            verse.Verse,
            "Sample Arabic text",
            "Sample translation",
            new List<TajweedMarker>
            {
                new TajweedMarker("Madd", 5, 8, "Elongate for 2 counts")
            }
        );
    }

    public async Task<List<CommonMistake>> GetCommonMistakesAsync(VerseReference verse)
    {
        await Task.Delay(50);
        return new List<CommonMistake>
        {
            new CommonMistake(
                "Madd Length",
                "Incorrect elongation",
                "Too short or too long",
                "Maintain exact count"
            )
        };
    }

    public async Task<SurahData?> GetSurahForLessonAsync(int surahNumber, RecitationLevel level)
    {
        await Task.Delay(50);
        return new SurahData(surahNumber, "Sample Surah", "Translation", 10, "Mecca");
    }

    public async Task<QiraatData?> GetQiraatDataAsync(VerseReference verse, QiraatType qiraatType)
    {
        await Task.Delay(50);
        return new QiraatData(verse, qiraatType);
    }
}

public class MockSirahService : ISirahService
{
    public async Task<List<SirahEvent>> SearchEventsByContextAsync(string context, string language = "en")
    {
        await Task.Delay(50);
        return new List<SirahEvent>
        {
            new SirahEvent(
                "Sample Event",
                $"This event relates to {context} in the Prophet's life",
                SirahPeriod.EarlyMecca,
                DateTime.Now,
                "Mecca",
                new List<string> { "Patience", "Trust in Allah" },
                new List<string> { "Prophet Muhammad" }
            )
        };
    }

    public async Task<List<SirahEvent>> GetEventsByPeriodAsync(SirahPeriod period, string language = "en")
    {
        await Task.Delay(50);
        return new List<SirahEvent>();
    }

    public async Task<SirahEvent?> GetEventByNameAsync(string eventName, string language = "en")
    {
        await Task.Delay(50);
        return null;
    }

    public async Task<List<PropheticCharacteristic>> GetCharacteristicsAsync(string aspect, string language = "en")
    {
        await Task.Delay(50);
        return new List<PropheticCharacteristic>
        {
            new PropheticCharacteristic(
                aspect,
                $"The Prophet exemplified {aspect} in all his dealings",
                new List<string> { "Example 1", "Example 2" },
                new List<string> { "Related hadith reference" }
            )
        };
    }

    public async Task<List<PropheticGuidance>> GetGuidanceByTopicAsync(string topic, string language = "en")
    {
        await Task.Delay(50);
        return new List<PropheticGuidance>
        {
            new PropheticGuidance(
                topic,
                $"The Prophetic guidance regarding {topic} teaches us to...",
                "Historical context",
                new List<string> { "Related event" },
                new List<string> { "Modern application" }
            )
        };
    }

    public async Task<ChronologicalTimeline> GetTimelineAsync(string language = "en")
    {
        await Task.Delay(50);
        return new ChronologicalTimeline();
    }
}
