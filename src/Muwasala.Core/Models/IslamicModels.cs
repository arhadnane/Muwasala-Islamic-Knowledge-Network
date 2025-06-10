namespace Muwasala.Core.Models;

/// <summary>
/// Schools of Islamic jurisprudence
/// </summary>
public enum Madhab
{
    Hanafi,
    Shafi,
    Maliki,
    Hanbali
}

/// <summary>
/// Hadith authenticity grades
/// </summary>
public enum HadithGrade
{
    Sahih,          // Authentic
    Hasan,          // Good
    Daif,           // Weak
    Mawdu,          // Fabricated
    Unknown
}

/// <summary>
/// Prayer times
/// </summary>
public enum PrayerName
{
    Fajr,
    Dhuhr,
    Asr,
    Maghrib,
    Isha,
    Tahajjud,
    Witr
}

/// <summary>
/// Periods in Prophet Muhammad's life for Sirah studies
/// </summary>
public enum SirahPeriod
{
    PreProphethood,
    EarlyMecca,
    MiddleMecca,
    LateMecca,
    EarlyMedina,
    MiddleMedina,
    LateMedina
}

/// <summary>
/// Quranic verse reference
/// </summary>
public record VerseReference(int Surah, int Verse)
{
    public override string ToString() => $"{Surah}:{Verse}";
}

/// <summary>
/// Geographic location for prayer calculations
/// </summary>
public record Location(
    double Latitude,
    double Longitude,
    string City,
    string Country,
    TimeZoneInfo TimeZone,
    Madhab Madhab = Madhab.Hanafi
);

/// <summary>
/// Base response model for all agent interactions
/// </summary>
public abstract record BaseResponse
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string RequestId { get; init; } = Guid.NewGuid().ToString();
    public List<string> Sources { get; init; } = new();
    public string? Warning { get; init; }
}

/// <summary>
/// Response from Quran Navigator agent
/// </summary>
public record VerseResponse : BaseResponse
{
    public required VerseReference Verse { get; init; }
    public required string ArabicText { get; init; }
    public required string Translation { get; init; }
    public string? Transliteration { get; init; }
    public string? Tafsir { get; init; }
    public string? TafsirSource { get; init; }
    public List<VerseReference> RelatedVerses { get; init; } = new();
    public List<string> RelatedDuas { get; init; } = new();
    public string Context { get; init; } = string.Empty;
}

/// <summary>
/// Response from Hadith Verifier agent
/// </summary>
public record HadithResponse : BaseResponse
{
    public required string Text { get; init; }
    public required string Translation { get; init; }
    public required HadithGrade Grade { get; init; }
    public required string Collection { get; init; }
    public string? BookNumber { get; init; }
    public string? HadithNumber { get; init; }
    public List<string> SanadChain { get; init; } = new();
    public string? Explanation { get; init; }
    public List<string> AlternativeHadith { get; init; } = new();
}

/// <summary>
/// Response from Fiqh Advisor agent
/// </summary>
public record FiqhResponse : BaseResponse
{
    public required string Question { get; init; }
    public required string Ruling { get; init; }
    public required Madhab Madhab { get; init; }
    public string? Evidence { get; init; }
    public Dictionary<Madhab, string> OtherMadhabRulings { get; init; } = new();
    public List<string> ScholarlyReferences { get; init; } = new();
    public string? ModernApplication { get; init; }
}

/// <summary>
/// Prayer calculation solution
/// </summary>
public record PrayerSolution : BaseResponse
{
    public required Location Location { get; init; }
    public bool ShorteningPermission { get; init; }
    public double QiblaDirection { get; init; }
    public bool CombinedPrayers { get; init; }
    public Dictionary<PrayerName, DateTime> PrayerTimes { get; init; } = new();
    public string? TravelRuling { get; init; }
}

/// <summary>
/// Du'a recommendation from Du'a Companion
/// </summary>
public record DuaResponse : BaseResponse
{
    public required string ArabicText { get; init; }
    public required string Translation { get; init; }
    public required string Transliteration { get; init; }
    public required string Occasion { get; init; }
    public string? Source { get; init; }
    public string? Benefits { get; init; }
    public List<string> RelatedDuas { get; init; } = new();
}

/// <summary>
/// Tajweed correction from Tajweed Tutor
/// </summary>
public record TajweedResponse : BaseResponse
{
    public required string VerseText { get; init; }
    public required List<TajweedRule> Rules { get; init; }
    public string? AudioExample { get; init; }
    public string? PronunciationGuide { get; init; }
}

/// <summary>
/// Sirah guidance from Sirah Scholar agent
/// </summary>
public record SirahResponse : BaseResponse
{
    public required string Topic { get; init; }
    public required string Event { get; init; }
    public required string Description { get; init; }
    public required SirahPeriod Period { get; init; }
    public string? KeyLessons { get; init; }
    public List<string> RelatedEvents { get; init; } = new();
    public List<string> ModernApplication { get; init; } = new();
    public string? PropheticWisdom { get; init; }
}

public record TajweedRule(
    string Name,
    string Description,
    int StartPosition,
    int EndPosition,
    string Example
);

/// <summary>
/// Travel schedule for prayer calculations
/// </summary>
public record TravelSchedule(
    Location CurrentLocation,
    Location NextDestination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    double TotalDistance
);

/// <summary>
/// Inheritance calculation result
/// </summary>
public record InheritanceResult : BaseResponse
{
    public required Dictionary<string, string> Shares { get; init; }
    public required decimal TotalEstate { get; init; }
    public Dictionary<string, decimal> FinalAmounts { get; init; } = new();
    public string? CalculationMethod { get; init; }
    public List<string> SpecialCases { get; init; } = new();
}

/// <summary>
/// Specific types of Islamic prayers/supplications
/// </summary>
public enum SpecificPrayer
{
    Istighfar,
    Istikhara,
    Tahajjud,
    Qunut,
    Witr,
    DuaKhatm,
    RuqyaShariyya
}

/// <summary>
/// Levels of Quranic recitation proficiency
/// </summary>
public enum RecitationLevel
{
    Beginner,
    Intermediate,
    Advanced,
    Master
}

/// <summary>
/// Different Quranic recitation styles (Qira'at)
/// </summary>
public enum QiraatType
{
    Hafs,
    Warsh,
    Qalun,
    Duri,
    Susi,
    IbnKathir,
    AbuAmr
}

/// <summary>
/// Types of search results for global Islamic search
/// </summary>
public enum IslamicContentType
{
    Verse,
    Hadith,
    FiqhRuling,
    Dua,
    SirahEvent,
    TajweedRule
}

/// <summary>
/// Global search result item
/// </summary>
public record GlobalSearchResult(
    IslamicContentType Type,
    string Title,
    string Content,
    string ArabicText,
    string Source,
    string Reference,
    double RelevanceScore,
    Dictionary<string, object> Metadata
);

/// <summary>
/// Global search response
/// </summary>
public record GlobalSearchResponse(
    string SearchQuery,
    List<GlobalSearchResult> Results,
    int TotalResults,
    double SearchDuration,    Dictionary<IslamicContentType, int> ResultsByType
);

/// <summary>
/// Response from the multi-agent hybrid search system
/// </summary>
public record HybridSearchResponse : BaseResponse
{
    public required IntelligentResponse AIResponse { get; init; }
    public List<WebSearchResult> WebResults { get; init; } = new();
    public List<string> SearchSuggestions { get; init; } = new();
    public int ProcessingTimeMs { get; init; }
}

/// <summary>
/// Intelligent response from DeepSeek brain agent
/// </summary>
public record IntelligentResponse : BaseResponse
{
    public required string Answer { get; init; }
    public List<WebSearchResult> WebSearchResults { get; init; } = new();
    public List<string> RelatedQuestions { get; init; } = new();
    public bool IsSuccessful { get; init; }
    public int ProcessingTimeMs { get; init; }
}

/// <summary>
/// Web search result from Islamic sources
/// </summary>
public record WebSearchResult
{
    public required string Title { get; init; }
    public required string Url { get; init; }
    public required string Snippet { get; init; }
    public required string Source { get; init; }
    public DateTime? PublishedDate { get; init; }
    public double RelevanceScore { get; init; }
    public List<string> Tags { get; init; } = new();
    
    // Enhanced properties for advanced AI agents
    public double AuthenticityScore { get; init; } = 0.8;
    public string Language { get; init; } = "en";
    public string? Author { get; init; }
    public string? Category { get; init; }
    public string? ContentType { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}
