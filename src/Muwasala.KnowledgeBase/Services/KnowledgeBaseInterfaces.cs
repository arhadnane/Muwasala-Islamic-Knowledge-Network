using Muwasala.Core.Models;

namespace Muwasala.KnowledgeBase.Services;

/// <summary>
/// Interface for Quran data access and search
/// </summary>
public interface IQuranService
{
    Task<List<QuranVerse>> SearchVersesByContextAsync(string context, string language = "en");
    Task<List<QuranVerse>> SearchVersesByThemeAsync(string theme, string language = "en", int maxResults = 10);
    Task<QuranVerse?> GetVerseAsync(VerseReference verse, string language = "en");
    Task<List<QuranVerse>> GetSurahAsync(int surahNumber, string language = "en");
    Task<List<TafsirEntry>> GetTafsirAsync(VerseReference verse, string source = "IbnKathir");
}

/// <summary>
/// Interface for Hadith data access and verification
/// </summary>
public interface IHadithService
{
    Task<List<HadithRecord>> SearchHadithAsync(string text, string language = "en");
    Task<List<HadithRecord>> GetHadithByTopicAsync(string topic, string language = "en", int maxResults = 10);
    Task<HadithRecord?> GetHadithByReferenceAsync(string collection, string hadithNumber);
    Task<List<HadithRecord>> GetAuthenticHadithAsync(string topic, int maxResults = 5);
}

/// <summary>
/// Interface for Fiqh (Islamic jurisprudence) data access
/// </summary>
public interface IFiqhService
{
    Task<List<FiqhRuling>> SearchRulingsAsync(string question, Madhab madhab, string language = "en");
    Task<List<FiqhRuling>> GetRulingsByTopicAsync(string topic, Madhab madhab, string language = "en");
    Task<List<FiqhComparison>> CompareMadhabRulingsAsync(string topic, string language = "en");
}

/// <summary>
/// Interface for Du'a (Islamic prayers) data access
/// </summary>
public interface IDuaService
{
    Task<List<DuaRecord>> SearchDuasByOccasionAsync(string occasion, string language = "en", int maxResults = 10);
    Task<DuaRecord?> GetSpecificPrayerAsync(SpecificPrayer prayerType, string language = "en");
    Task<List<DuaRecord>> GetDailyDuasAsync(string timeOfDay, string language = "en");
}

/// <summary>
/// Interface for Tajweed (Quranic recitation) data access
/// </summary>
public interface ITajweedService
{
    Task<VerseData?> GetVerseWithTajweedAsync(VerseReference verse);
    Task<List<CommonMistake>> GetCommonMistakesAsync(VerseReference verse);
    Task<SurahData?> GetSurahForLessonAsync(int surahNumber, RecitationLevel level);
    Task<QiraatData?> GetQiraatDataAsync(VerseReference verse, QiraatType qiraatType);
}

/// <summary>
/// Interface for Sirah (Prophetic biography) data access
/// </summary>
public interface ISirahService
{
    Task<List<SirahEvent>> SearchEventsByContextAsync(string context, string language = "en");
    Task<List<SirahEvent>> GetEventsByPeriodAsync(SirahPeriod period, string language = "en");
    Task<SirahEvent?> GetEventByNameAsync(string eventName, string language = "en");
    Task<List<PropheticCharacteristic>> GetCharacteristicsAsync(string aspect, string language = "en");
    Task<List<PropheticGuidance>> GetGuidanceByTopicAsync(string topic, string language = "en");
    Task<ChronologicalTimeline> GetTimelineAsync(string language = "en");
}

/// <summary>
/// Interface for global search across all Islamic knowledge sources
/// </summary>
public interface IGlobalSearchService
{
    Task<GlobalSearchResponse> SearchAllAsync(string query, string language = "en", int maxResults = 20);
    Task<GlobalSearchResponse> SearchByTypeAsync(string query, IslamicContentType[] types, string language = "en", int maxResults = 20);
    Task<List<string>> GetSearchSuggestionsAsync(string partialQuery, string language = "en");
}

/// <summary>
/// Intelligent search service that uses AI when local knowledge base has insufficient results
/// </summary>
public interface IIntelligentSearchService
{
    /// <summary>
    /// Performs intelligent search using AI when local results are insufficient
    /// </summary>
    Task<List<GlobalSearchResult>> SearchWithAIAsync(string query, string language = "en", int maxResults = 10);
      /// <summary>
    /// Gets AI-generated Islamic knowledge response for queries not found in local database
    /// </summary>
    Task<IslamicKnowledgeResponse?> GetAIKnowledgeAsync(string query, string language = "en");
    
    /// <summary>
    /// Searches authentic Islamic web sources as fallback
    /// </summary>
    Task<List<WebSourceResult>> SearchWebSourcesAsync(string query, string language = "en", int maxResults = 5);
    
    /// <summary>
    /// Combines local, AI, and web search results with relevance ranking
    /// </summary>
    Task<EnhancedSearchResponse> PerformHybridSearchAsync(string query, string language = "en", int maxResults = 20);
}

/// <summary>
/// Interface for search history management
/// </summary>
public interface ISearchHistoryService
{
    /// <summary>
    /// Saves a search and its results to the database
    /// </summary>
    Task<int> SaveSearchAsync(string query, string searchMode, string language, 
        List<GlobalSearchResult> results, TimeSpan searchDuration, 
        string[]? selectedContentTypes = null, string? userIdentifier = null, 
        string? searchContext = null);

    /// <summary>
    /// Saves a search with AI response to the database
    /// </summary>
    Task<int> SaveSearchWithAIAsync(string query, string searchMode, string language,
        List<GlobalSearchResult> localResults, IslamicKnowledgeResponse? aiResponse,
        List<WebSearchResult>? webResults, TimeSpan searchDuration,
        string[]? selectedContentTypes = null, string? userIdentifier = null,
        string? searchContext = null);

    /// <summary>
    /// Gets recent search history
    /// </summary>
    Task<List<SearchHistoryEntry>> GetRecentSearchesAsync(int maxResults = 50, string? userIdentifier = null);

    /// <summary>
    /// Gets search history for a specific query
    /// </summary>
    Task<List<SearchHistoryEntry>> GetSearchHistoryForQueryAsync(string query, int maxResults = 10);

    /// <summary>
    /// Gets most popular search terms
    /// </summary>
    Task<List<PopularSearchTerm>> GetPopularSearchTermsAsync(int maxResults = 20, TimeSpan? timeRange = null);

    /// <summary>
    /// Gets search analytics for a specific period
    /// </summary>
    Task<SearchAnalytics> GetSearchAnalyticsAsync(DateTime fromDate, DateTime toDate, string? language = null);

    /// <summary>
    /// Deletes old search history
    /// </summary>
    Task<int> CleanupOldSearchHistoryAsync(TimeSpan retentionPeriod);

    /// <summary>
    /// Gets search suggestions based on history
    /// </summary>
    Task<List<string>> GetHistoryBasedSuggestionsAsync(string partialQuery, int maxResults = 10);
}

/// <summary>
/// AI-generated Islamic knowledge response
/// </summary>
public record IslamicKnowledgeResponse(
    string Query,
    string Response,
    List<string> QuranReferences,
    List<string> HadithReferences,
    List<string> ScholarlyOpinions,
    double ConfidenceScore,
    string Source,
    string Language = "en"
)
{
    public List<string> QuranReferences { get; init; } = QuranReferences ?? new List<string>();
    public List<string> HadithReferences { get; init; } = HadithReferences ?? new List<string>();
    public List<string> ScholarlyOpinions { get; init; } = ScholarlyOpinions ?? new List<string>();
    public List<WebSourceResult> WebSearchResults { get; init; } = new List<WebSourceResult>();
}

/// <summary>
/// Web source search result
/// </summary>
public record WebSourceResult(
    string Title,
    string Content,
    string Url,
    string Source,
    double RelevanceScore,
    DateTime LastUpdated,
    string Language = "en"
);

/// <summary>
/// Enhanced search response combining multiple sources
/// </summary>
public record EnhancedSearchResponse(
    string Query,
    List<GlobalSearchResult> LocalResults,
    IslamicKnowledgeResponse? AIResponse,
    List<WebSourceResult> WebResults,
    int TotalResultsFound,
    double SearchDurationMs,
    Dictionary<string, int> ResultsBySource,
    List<string> SearchSuggestions,
    string Language = "en"
)
{
    public List<GlobalSearchResult> LocalResults { get; init; } = LocalResults ?? new List<GlobalSearchResult>();
    public List<WebSourceResult> WebResults { get; init; } = WebResults ?? new List<WebSourceResult>();
    public Dictionary<string, int> ResultsBySource { get; init; } = ResultsBySource ?? new Dictionary<string, int>();
    public List<string> SearchSuggestions { get; init; } = SearchSuggestions ?? new List<string>();
}

// Data models for the knowledge base
public record QuranVerse(
    int Surah,
    int Verse,
    string ArabicText,
    string Translation,
    string? Transliteration = null,
    string Language = "en"
);

public record TafsirEntry(
    VerseReference Verse,
    string Source,
    string Commentary,
    string Scholar,
    string Language = "en"
);

public record HadithRecord(
    string ArabicText,
    string Translation,
    HadithGrade Grade,
    string Collection,
    string? BookNumber = null,
    string? HadithNumber = null,
    List<string> SanadChain = default!,
    string? Topic = null,
    string Language = "en"
)
{
    public List<string> SanadChain { get; init; } = SanadChain ?? new List<string>();
}

public record FiqhRuling(
    string Question,
    string Ruling,
    Madhab Madhab,
    string Evidence,
    string Source,
    List<string> ScholarReferences = default!,
    string Language = "en"
)
{
    public List<string> ScholarReferences { get; init; } = ScholarReferences ?? new List<string>();
}

public record FiqhComparison(
    string Topic,
    Dictionary<Madhab, string> Rulings,
    string CommonGround,
    List<string> KeyDifferences,
    string Language = "en"
);

public record DuaRecord(
    string ArabicText,
    string Translation,
    string Transliteration,
    string Occasion,
    string Source,
    string? Benefits = null,
    string Language = "en"
);

public record VerseData(
    int Surah,
    int Verse,
    string ArabicText,
    string Translation,
    List<TajweedMarker> TajweedMarkers = default!
)
{
    public List<TajweedMarker> TajweedMarkers { get; init; } = TajweedMarkers ?? new List<TajweedMarker>();
}

public record TajweedMarker(
    string RuleName,
    int StartPosition,
    int EndPosition,
    string Description
);

public record CommonMistake(
    string Type,
    string Description,
    string TypicalError,
    string Correction
);

public record SurahData(
    int Number,
    string Name,
    string NameTranslation,
    int VerseCount,
    string PlaceOfRevelation,
    List<string> MainThemes = default!
)
{
    public List<string> MainThemes { get; init; } = MainThemes ?? new List<string>();
}

public record QiraatData(
    VerseReference Verse,
    QiraatType Type,
    List<string> Variations = default!,
    List<string> AudioReferences = default!
)
{
    public List<string> Variations { get; init; } = Variations ?? new List<string>();
    public List<string> AudioReferences { get; init; } = AudioReferences ?? new List<string>();
}

// Forward declarations for agent-specific enums moved to Core.Models

// Sirah-specific data models
public record SirahEvent(
    string Name,
    string Description,
    SirahPeriod Period,
    DateTime? ApproximateDate,
    string Location,
    List<string> KeyLessons = default!,
    List<string> ParticipantsInvolved = default!,
    string Language = "en"
)
{
    public List<string> KeyLessons { get; init; } = KeyLessons ?? new List<string>();
    public List<string> ParticipantsInvolved { get; init; } = ParticipantsInvolved ?? new List<string>();
}

public record PropheticCharacteristic(
    string Aspect,
    string Description,
    List<string> Examples = default!,
    List<string> RelatedHadiths = default!,
    string Language = "en"
)
{
    public List<string> Examples { get; init; } = Examples ?? new List<string>();
    public List<string> RelatedHadiths { get; init; } = RelatedHadiths ?? new List<string>();
}

public record PropheticGuidance(
    string Topic,
    string Guidance,
    string Context,
    List<string> RelatedEvents = default!,
    List<string> ModernApplication = default!,
    string Language = "en"
)
{
    public List<string> RelatedEvents { get; init; } = RelatedEvents ?? new List<string>();
    public List<string> ModernApplication { get; init; } = ModernApplication ?? new List<string>();
}

public record ChronologicalTimeline(
    List<SirahEvent> MeccanPeriod = default!,
    List<SirahEvent> MedinanPeriod = default!,
    List<SirahEvent> MajorEvents = default!,
    string Language = "en"
)
{
    public List<SirahEvent> MeccanPeriod { get; init; } = MeccanPeriod ?? new List<SirahEvent>();
    public List<SirahEvent> MedinanPeriod { get; init; } = MedinanPeriod ?? new List<SirahEvent>();    public List<SirahEvent> MajorEvents { get; init; } = MajorEvents ?? new List<SirahEvent>();
}

/// <summary>
/// Represents a search history entry
/// </summary>
public record SearchHistoryEntry
{
    public int Id { get; init; }
    public string SearchQuery { get; init; } = string.Empty;
    public string SearchMode { get; init; } = string.Empty;
    public string Language { get; init; } = "en";
    public DateTime SearchDateTime { get; init; }
    public string[]? SelectedContentTypes { get; init; }
    public int ResultsCount { get; init; }
    public double SearchDurationMs { get; init; }
    public string? UserIdentifier { get; init; }
    public string? SearchContext { get; init; }
    public List<SearchResultSummary> Results { get; init; } = new();
}

/// <summary>
/// Represents a summary of a search result
/// </summary>
public record SearchResultSummary
{
    public string Title { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public double RelevanceScore { get; init; }
    public int Position { get; init; }
}

/// <summary>
/// Represents a popular search term
/// </summary>
public record PopularSearchTerm
{
    public string SearchTerm { get; init; } = string.Empty;
    public int SearchCount { get; init; }
    public string PreferredLanguage { get; init; } = "en";
    public string MostCommonContentType { get; init; } = string.Empty;
    public string PreferredSearchMode { get; init; } = string.Empty;
    public DateTime LastSearched { get; init; }
    public double AverageResultsCount { get; init; }
}

/// <summary>
/// Represents search analytics data
/// </summary>
public record SearchAnalytics
{
    public int TotalSearches { get; init; }
    public int UniqueQueries { get; init; }
    public double AverageResultsCount { get; init; }
    public double AverageSearchDuration { get; init; }
    public Dictionary<string, int> SearchModeDistribution { get; init; } = new();
    public Dictionary<string, int> ContentTypeDistribution { get; init; } = new();
    public Dictionary<string, int> LanguageDistribution { get; init; } = new();
    public List<PopularSearchTerm> TopQueries { get; init; } = new();
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
}
