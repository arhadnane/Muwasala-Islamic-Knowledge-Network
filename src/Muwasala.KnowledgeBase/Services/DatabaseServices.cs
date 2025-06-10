using Muwasala.Core.Models;
using Muwasala.KnowledgeBase.Data.Repositories;

namespace Muwasala.KnowledgeBase.Services;

/// <summary>
/// Database-backed implementation of Quran service
/// </summary>
public class DatabaseQuranService : IQuranService
{
    private readonly IQuranRepository _quranRepository;
    private readonly ITafsirRepository _tafsirRepository;

    public DatabaseQuranService(IQuranRepository quranRepository, ITafsirRepository tafsirRepository)
    {
        _quranRepository = quranRepository;
        _tafsirRepository = tafsirRepository;
    }

    public async Task<List<QuranVerse>> SearchVersesByContextAsync(string context, string language = "en")
    {
        var entities = await _quranRepository.GetVersesByContextAsync(context, language, 20);
        return entities.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToQuranVerse).ToList();
    }

    public async Task<List<QuranVerse>> SearchVersesByThemeAsync(string theme, string language = "en", int maxResults = 10)
    {
        var entities = await _quranRepository.SearchByThemeAsync(theme, language, maxResults);
        return entities.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToQuranVerse).ToList();
    }

    public async Task<QuranVerse?> GetVerseAsync(VerseReference verse, string language = "en")
    {
        var entity = await _quranRepository.GetVerseAsync(verse.Surah, verse.Verse, language);
        return entity != null ? Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToQuranVerse(entity) : null;
    }

    public async Task<List<QuranVerse>> GetSurahAsync(int surahNumber, string language = "en")
    {
        var entities = await _quranRepository.GetSurahAsync(surahNumber, language);
        return entities.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToQuranVerse).ToList();
    }

    public async Task<List<TafsirEntry>> GetTafsirAsync(VerseReference verse, string source = "IbnKathir")
    {
        var entities = await _tafsirRepository.GetTafsirForVerseAsync(verse.Surah, verse.Verse, source);
        return entities.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToTafsirEntry).ToList();
    }
}

/// <summary>
/// Database-backed implementation of Hadith service
/// </summary>
public class DatabaseHadithService : IHadithService
{
    private readonly IHadithRepository _hadithRepository;

    public DatabaseHadithService(IHadithRepository hadithRepository)
    {
        _hadithRepository = hadithRepository;
    }

    public async Task<List<HadithRecord>> SearchHadithAsync(string text, string language = "en")
    {
        var entities = await _hadithRepository.SearchByTextAsync(text, language, 20);
        return entities.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToHadithRecord).ToList();
    }

    public async Task<List<HadithRecord>> GetHadithByTopicAsync(string topic, string language = "en", int maxResults = 10)
    {
        var entities = await _hadithRepository.GetByTopicAsync(topic, language, maxResults);
        return entities.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToHadithRecord).ToList();
    }

    public async Task<HadithRecord?> GetHadithByReferenceAsync(string collection, string hadithNumber)
    {
        var entity = await _hadithRepository.GetByReferenceAsync(collection, hadithNumber);
        return entity != null ? Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToHadithRecord(entity) : null;
    }

    public async Task<List<HadithRecord>> GetAuthenticHadithAsync(string topic, int maxResults = 5)
    {
        var entities = await _hadithRepository.GetAuthenticHadithAsync(topic, HadithGrade.Sahih, maxResults);
        return entities.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToHadithRecord).ToList();
    }
}

/// <summary>
/// Database-backed implementation of Fiqh service
/// </summary>
public class DatabaseFiqhService : IFiqhService
{
    private readonly IFiqhRepository _fiqhRepository;

    public DatabaseFiqhService(IFiqhRepository fiqhRepository)
    {
        _fiqhRepository = fiqhRepository;
    }

    public async Task<List<FiqhRuling>> SearchRulingsAsync(string question, Madhab madhab, string language = "en")
    {
        var entities = await _fiqhRepository.SearchRulingsAsync(question, madhab, language, 20);
        return entities.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToFiqhRuling).ToList();
    }

    public async Task<List<FiqhRuling>> GetRulingsByTopicAsync(string topic, Madhab madhab, string language = "en")
    {
        var entities = await _fiqhRepository.GetRulingsByTopicAsync(topic, madhab, language, 20);
        return entities.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToFiqhRuling).ToList();
    }    public async Task<List<FiqhComparison>> CompareMadhabRulingsAsync(string topic, string language = "en")
    {
        var entities = await _fiqhRepository.GetRulingsByTopicAllMadhabsAsync(topic, language, 20);
          // Group by madhab and create comparisons
        var groupedByMadhab = entities.GroupBy(e => e.Madhab);
        var madhabRulings = new Dictionary<Madhab, string>();

        foreach (var group in groupedByMadhab)
        {
            var madhab = (Madhab)group.Key;
            var rulings = group.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToFiqhRuling).ToList();
            
            if (rulings.Any())
            {
                madhabRulings[madhab] = rulings.First().Ruling;
            }
        }

        var comparison = new FiqhComparison(
            topic,
            madhabRulings,
            "Common principles based on Quran and Sunnah",
            madhabRulings.Select(kvp => $"{kvp.Key} emphasizes different methodological approaches").ToList(),
            language
        );

        return new List<FiqhComparison> { comparison };
    }
}

/// <summary>
/// Database-backed implementation of Du'a service
/// </summary>
public class DatabaseDuaService : IDuaService
{
    private readonly IDuaRepository _duaRepository;

    public DatabaseDuaService(IDuaRepository duaRepository)
    {
        _duaRepository = duaRepository;
    }

    public async Task<List<DuaRecord>> SearchDuasByOccasionAsync(string occasion, string language = "en", int maxResults = 10)
    {
        var entities = await _duaRepository.SearchByOccasionAsync(occasion, language, maxResults);
        return entities.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToDuaRecord).ToList();
    }

    public async Task<DuaRecord?> GetSpecificPrayerAsync(SpecificPrayer prayerType, string language = "en")
    {
        var entity = await _duaRepository.GetSpecificPrayerAsync(prayerType, language);
        return entity != null ? Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToDuaRecord(entity) : null;
    }

    public async Task<List<DuaRecord>> GetDailyDuasAsync(string timeOfDay, string language = "en")
    {
        var entities = await _duaRepository.GetDailyDuasAsync(timeOfDay, language, 20);
        return entities.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToDuaRecord).ToList();
    }
}

/// <summary>
/// Database-backed implementation of Tajweed service
/// </summary>
public class DatabaseTajweedService : ITajweedService
{
    private readonly ITajweedRepository _tajweedRepository;
    private readonly IQuranRepository _quranRepository;

    public DatabaseTajweedService(ITajweedRepository tajweedRepository, IQuranRepository quranRepository)
    {
        _tajweedRepository = tajweedRepository;
        _quranRepository = quranRepository;
    }    public async Task<VerseData?> GetVerseWithTajweedAsync(VerseReference verseRef)
    {
        var quranVerse = await _quranRepository.GetVerseAsync(verseRef.Surah, verseRef.Verse);
        if (quranVerse == null) return null;

        var tajweedRules = await _tajweedRepository.GetVerseRulesAsync(verseRef.Surah, verseRef.Verse);
        var commonMistakes = await _tajweedRepository.GetCommonMistakesAsync(verseRef.Surah, verseRef.Verse);

        var verse = Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToQuranVerse(quranVerse);
        var tajweedMarkers = tajweedRules.Select(vt => new TajweedMarker(
            vt.TajweedRule.Name,
            vt.StartPosition,
            vt.EndPosition,
            vt.TajweedRule.Description
        )).ToList();        return new VerseData(
            verse.Surah,
            verse.Verse,
            verse.ArabicText,
            verse.Translation,
            tajweedMarkers
        );
    }public async Task<List<CommonMistake>> GetCommonMistakesAsync(VerseReference verse)
    {
        var mistakes = await _tajweedRepository.GetCommonMistakesAsync(verse.Surah, verse.Verse);
        return mistakes.Select(cm => new CommonMistake(
            cm.MistakeType,
            cm.Description,
            "Common error", // TypicalError - not available in entity
            cm.Correction ?? "See description"
        )).ToList();
    }

    public async Task<SurahData?> GetSurahForLessonAsync(int surahNumber, RecitationLevel level)
    {
        var verses = await _quranRepository.GetSurahAsync(surahNumber);
        if (!verses.Any()) return null;

        var surahTajweed = new List<VerseData>();        foreach (var verse in verses.Take(5)) // Limit for lesson purposes
        {
            var verseData = await GetVerseWithTajweedAsync(new VerseReference(verse.SurahNumber, verse.VerseNumber));
            if (verseData != null)
                surahTajweed.Add(verseData);
        }        return new SurahData(
            surahNumber,
            $"Surah {surahNumber}", // Use generic surah name since SurahNameArabic is not available
            $"Surah {surahNumber} Translation",
            verses.Count(),
            "Unknown", // PlaceOfRevelation - not available in entity
            new List<string>() // MainThemes - not available in entity
        );
    }

    public async Task<QiraatData?> GetQiraatDataAsync(VerseReference verse, QiraatType qiraatType)
    {
        // For now, return basic data - would need additional tables for full Qiraat support
        var verseData = await GetVerseWithTajweedAsync(verse);
        if (verseData == null) return null;        return new QiraatData(
            verse,
            qiraatType,
            new List<string> { "Standard recitation" }, // Variations
            new List<string> { "audio_reference.mp3" } // AudioReferences
        );
    }
}

/// <summary>
/// Database-backed implementation of Sirah service
/// </summary>
public class DatabaseSirahService : ISirahService
{
    private readonly ISirahRepository _sirahRepository;

    public DatabaseSirahService(ISirahRepository sirahRepository)
    {
        _sirahRepository = sirahRepository;
    }

    public async Task<List<SirahEvent>> SearchEventsByContextAsync(string context, string language = "en")
    {
        var entities = await _sirahRepository.SearchByContextAsync(context, language, 20);
        return entities.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToSirahEvent).ToList();
    }

    public async Task<List<SirahEvent>> GetEventsByPeriodAsync(SirahPeriod period, string language = "en")
    {
        var entities = await _sirahRepository.GetEventsByPeriodAsync(period, language, 20);
        return entities.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToSirahEvent).ToList();
    }

    public async Task<SirahEvent?> GetEventByNameAsync(string eventName, string language = "en")
    {
        var entity = await _sirahRepository.GetEventByNameAsync(eventName, language);
        return entity != null ? Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToSirahEvent(entity) : null;
    }    public async Task<List<PropheticCharacteristic>> GetCharacteristicsAsync(string aspect, string language = "en")
    {
        // For now, derive from events - could be expanded with dedicated table
        var events = await _sirahRepository.SearchByContextAsync(aspect, language, 10);
        return events.Select(e => {
            var sirahEvent = Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToSirahEvent(e);
            return new PropheticCharacteristic(
                aspect,
                sirahEvent.Description,
                new List<string> { sirahEvent.Name },
                new List<string>(), // RelatedHadiths
                language
            );
        }).ToList();
    }

    public async Task<List<PropheticGuidance>> GetGuidanceByTopicAsync(string topic, string language = "en")
    {
        // Derive from events that contain lessons
        var events = await _sirahRepository.SearchByContextAsync(topic, language, 10);        return events.Where(e => !string.IsNullOrEmpty(e.KeyLessons))
            .Select(e => new PropheticGuidance(
                topic,
                e.KeyLessons ?? string.Empty,
                e.Description,
                new List<string> { e.EventName },
                new List<string> { $"Modern application of lessons from {e.EventName}" },
                language
            )).ToList();
    }

    public async Task<ChronologicalTimeline> GetTimelineAsync(string language = "en")
    {
        var allEvents = await _sirahRepository.GetChronologicalEventsAsync(language);
        var timelineEvents = allEvents.Select(Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToSirahEvent).ToList();        var meccanEvents = timelineEvents.Where(e => e.Period == SirahPeriod.EarlyMecca || 
                                                   e.Period == SirahPeriod.MiddleMecca || 
                                                   e.Period == SirahPeriod.LateMecca).ToList();
        var medinanEvents = timelineEvents.Where(e => e.Period == SirahPeriod.EarlyMedina || 
                                                    e.Period == SirahPeriod.MiddleMedina || 
                                                    e.Period == SirahPeriod.LateMedina).ToList();
        var majorEvents = timelineEvents.Take(10).ToList(); // Select first 10 as major events

        return new ChronologicalTimeline(
            meccanEvents,
            medinanEvents,
            majorEvents,
            language
        );
    }
}

/// <summary>
/// Database-backed global search service
/// </summary>
public class DatabaseGlobalSearchService : IGlobalSearchService
{    private readonly IQuranRepository _quranRepository;
    private readonly IHadithRepository _hadithRepository;
    private readonly IFiqhRepository _fiqhRepository;
    private readonly IDuaRepository _duaRepository;
    private readonly ISirahRepository _sirahRepository;
    private readonly ISearchHistoryService? _searchHistoryService;

    public DatabaseGlobalSearchService(
        IQuranRepository quranRepository,
        IHadithRepository hadithRepository,
        IFiqhRepository fiqhRepository,
        IDuaRepository duaRepository,
        ISirahRepository sirahRepository,
        ISearchHistoryService? searchHistoryService = null)
    {
        _quranRepository = quranRepository;
        _hadithRepository = hadithRepository;
        _fiqhRepository = fiqhRepository;
        _duaRepository = duaRepository;
        _sirahRepository = sirahRepository;
        _searchHistoryService = searchHistoryService;
    }    public async Task<GlobalSearchResponse> SearchAllAsync(string query, string language = "en", int maxResults = 20)
    {
        var startTime = DateTime.UtcNow;
        var resultsPerType = Math.Max(1, maxResults / 5); // Distribute across content types

        var tasks = new List<Task<IEnumerable<GlobalSearchResult>>>
        {
            SearchQuranAsync(query, language, resultsPerType),
            SearchHadithAsync(query, language, resultsPerType),
            SearchFiqhAsync(query, language, resultsPerType),
            SearchDuaAsync(query, language, resultsPerType),
            SearchSirahAsync(query, language, resultsPerType)
        };

        var results = await Task.WhenAll(tasks);
        var allResults = results.SelectMany(r => r).Take(maxResults).ToList();
        
        var duration = DateTime.UtcNow - startTime;

        // Save to search history if service is available
        if (_searchHistoryService != null)
        {
            try
            {
                await _searchHistoryService.SaveSearchAsync(
                    query, 
                    "DatabaseGlobalSearch", 
                    language, 
                    allResults, 
                    duration);
            }
            catch
            {
                // Log but don't fail the search
            }
        }

        return new GlobalSearchResponse(
            query,
            allResults,
            allResults.Count,
            duration.TotalMilliseconds,
            allResults.GroupBy(r => r.Type).ToDictionary(g => g.Key, g => g.Count())
        );
    }    public async Task<GlobalSearchResponse> SearchByTypeAsync(string query, IslamicContentType[] types, string language = "en", int maxResults = 20)
    {
        var startTime = DateTime.UtcNow;
        var tasks = new List<Task<IEnumerable<GlobalSearchResult>>>();
        var resultsPerType = Math.Max(1, maxResults / types.Length);foreach (var type in types)
        {
            switch (type)
            {
                case IslamicContentType.Verse:
                    tasks.Add(SearchQuranAsync(query, language, resultsPerType));
                    break;
                case IslamicContentType.Hadith:
                    tasks.Add(SearchHadithAsync(query, language, resultsPerType));
                    break;
                case IslamicContentType.FiqhRuling:
                    tasks.Add(SearchFiqhAsync(query, language, resultsPerType));
                    break;
                case IslamicContentType.Dua:
                    tasks.Add(SearchDuaAsync(query, language, resultsPerType));
                    break;
                case IslamicContentType.SirahEvent:
                    tasks.Add(SearchSirahAsync(query, language, resultsPerType));
                    break;
            }        }

        var results = await Task.WhenAll(tasks);
        var allResults = results.SelectMany(r => r).Take(maxResults).ToList();
        
        var duration = DateTime.UtcNow - startTime;

        // Save to search history if service is available
        if (_searchHistoryService != null)
        {
            try
            {
                var selectedTypes = types.Select(t => t.ToString()).ToArray();
                await _searchHistoryService.SaveSearchAsync(
                    query, 
                    "DatabaseTypedSearch", 
                    language, 
                    allResults, 
                    duration,
                    selectedTypes);
            }
            catch
            {
                // Log but don't fail the search
            }
        }

        return new GlobalSearchResponse(
            query,
            allResults,
            allResults.Count,
            duration.TotalMilliseconds,
            allResults.GroupBy(r => r.Type).ToDictionary(g => g.Key, g => g.Count())
        );
    }    public async Task<List<string>> GetSearchSuggestionsAsync(string partialQuery, string language = "en")
    {
        var suggestions = new List<string>();

        // Try to get history-based suggestions first
        if (_searchHistoryService != null)
        {
            try
            {
                var historySuggestions = await _searchHistoryService.GetHistoryBasedSuggestionsAsync(partialQuery);
                suggestions.AddRange(historySuggestions);
            }
            catch
            {
                // Fallback to static suggestions if history service fails
            }
        }

        // Add common Islamic topics as fallback
        var commonSuggestions = new List<string>
        {
            "prayer", "patience", "charity", "fasting", "pilgrimage",
            "mercy", "forgiveness", "guidance", "knowledge", "wisdom",
            "family", "marriage", "business", "justice", "kindness"
        };

        suggestions.AddRange(commonSuggestions
            .Where(s => s.StartsWith(partialQuery.ToLower()) || s.Contains(partialQuery.ToLower()))
            .Where(s => !suggestions.Contains(s, StringComparer.OrdinalIgnoreCase))
        );

        return suggestions.Take(10).ToList();
    }

    private async Task<IEnumerable<GlobalSearchResult>> SearchQuranAsync(string query, string language, int maxResults)
    {
        var verses = await _quranRepository.SearchByTextAsync(query, language, maxResults);
        return verses.Select(v => Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToGlobalSearchResult(v));
    }

    private async Task<IEnumerable<GlobalSearchResult>> SearchHadithAsync(string query, string language, int maxResults)
    {
        var hadiths = await _hadithRepository.SearchByTextAsync(query, language, maxResults);
        return hadiths.Select(h => Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToGlobalSearchResult(h));
    }

    private async Task<IEnumerable<GlobalSearchResult>> SearchFiqhAsync(string query, string language, int maxResults)
    {
        var rulings = await _fiqhRepository.GetRulingsByTopicAllMadhabsAsync(query, language, maxResults);
        return rulings.Select(f => Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToGlobalSearchResult(f));
    }

    private async Task<IEnumerable<GlobalSearchResult>> SearchDuaAsync(string query, string language, int maxResults)
    {
        var duas = await _duaRepository.SearchByTextAsync(query, language, maxResults);
        return duas.Select(d => Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToGlobalSearchResult(d));
    }

    private async Task<IEnumerable<GlobalSearchResult>> SearchSirahAsync(string query, string language, int maxResults)
    {
        var events = await _sirahRepository.SearchByContextAsync(query, language, maxResults);
        return events.Select(s => Muwasala.KnowledgeBase.Data.Mappers.EntityMapper.ToGlobalSearchResult(s));
    }
}
