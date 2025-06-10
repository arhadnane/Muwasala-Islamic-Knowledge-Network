using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Muwasala.Core.Models;

namespace Muwasala.KnowledgeBase.Data.Models;

/// <summary>
/// Database entity for Quran verses
/// </summary>
[Table("QuranVerses")]
public class QuranVerseEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int SurahNumber { get; set; }
    
    [Required]
    public int VerseNumber { get; set; }
    
    [Required]
    [MaxLength(2000)]
    public string ArabicText { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(3000)]
    public string Translation { get; set; } = string.Empty;
    
    [MaxLength(3000)]
    public string? Transliteration { get; set; }
    
    [MaxLength(50)]
    public string Language { get; set; } = "en";
    
    [MaxLength(500)]
    public string? Keywords { get; set; }
    
    [MaxLength(1000)]
    public string? Context { get; set; }
    
    [MaxLength(500)]
    public string? Theme { get; set; }
    
    // Navigation properties
    public virtual ICollection<TafsirEntity> Tafsirs { get; set; } = new List<TafsirEntity>();
    
    // Index for faster searching
    [NotMapped]
    public VerseReference VerseReference => new(SurahNumber, VerseNumber);
}

/// <summary>
/// Database entity for Tafsir (Quran commentary)
/// </summary>
[Table("Tafsirs")]
public class TafsirEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int QuranVerseId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Source { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(5000)]
    public string Commentary { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Language { get; set; } = "en";
    
    [MaxLength(100)]
    public string? Scholar { get; set; }
    
    // Navigation properties
    public virtual QuranVerseEntity QuranVerse { get; set; } = null!;
}

/// <summary>
/// Database entity for Hadith records
/// </summary>
[Table("HadithRecords")]
public class HadithEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(5000)]
    public string ArabicText { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(5000)]
    public string Translation { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Collection { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? BookNumber { get; set; }
    
    [MaxLength(50)]
    public string? HadithNumber { get; set; }
    
    [Required]
    public int Grade { get; set; } // Maps to HadithGrade enum
    
    [MaxLength(50)]
    public string Language { get; set; } = "en";
    
    [MaxLength(1000)]
    public string? Topic { get; set; }
    
    [MaxLength(1000)]
    public string? Keywords { get; set; }
    
    [MaxLength(2000)]
    public string? SanadChain { get; set; }
    
    [MaxLength(3000)]
    public string? Explanation { get; set; }
    
    [NotMapped]
    public HadithGrade HadithGrade 
    { 
        get => (HadithGrade)Grade; 
        set => Grade = (int)value; 
    }
}

/// <summary>
/// Database entity for Fiqh rulings
/// </summary>
[Table("FiqhRulings")]
public class FiqhRulingEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Question { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Ruling { get; set; } = string.Empty;
    
    [Required]
    public int Madhab { get; set; } // Maps to Madhab enum
    
    [MaxLength(2000)]
    public string? Evidence { get; set; }
    
    [MaxLength(50)]
    public string Language { get; set; } = "en";
    
    [MaxLength(500)]
    public string? Topic { get; set; }
    
    [MaxLength(1000)]
    public string? Keywords { get; set; }
    
    [MaxLength(1000)]
    public string? ScholarlyReferences { get; set; }
    
    [MaxLength(1000)]
    public string? ModernApplication { get; set; }
    
    [NotMapped]
    public Madhab MadhabEnum 
    { 
        get => (Madhab)Madhab; 
        set => Madhab = (int)value; 
    }
}

/// <summary>
/// Database entity for Du'a records
/// </summary>
[Table("DuaRecords")]
public class DuaEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(2000)]
    public string ArabicText { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Translation { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Transliteration { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Occasion { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Source { get; set; }
    
    [MaxLength(50)]
    public string Language { get; set; } = "en";
    
    [MaxLength(500)]
    public string? Benefits { get; set; }
    
    [MaxLength(500)]
    public string? Keywords { get; set; }
    
    [MaxLength(100)]
    public string? TimeOfDay { get; set; }
    
    public int? SpecificPrayerType { get; set; } // Maps to SpecificPrayer enum
    
    [NotMapped]
    public SpecificPrayer? SpecificPrayer 
    { 
        get => SpecificPrayerType.HasValue ? (SpecificPrayer)SpecificPrayerType.Value : null; 
        set => SpecificPrayerType = value.HasValue ? (int)value.Value : null; 
    }
}

/// <summary>
/// Database entity for Sirah events
/// </summary>
[Table("SirahEvents")]
public class SirahEventEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string EventName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(3000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public int Period { get; set; } // Maps to SirahPeriod enum
    
    [MaxLength(50)]
    public string Language { get; set; } = "en";
    
    [MaxLength(1000)]
    public string? KeyLessons { get; set; }
    
    [MaxLength(1000)]
    public string? ModernApplication { get; set; }
    
    [MaxLength(1000)]
    public string? PropheticWisdom { get; set; }
    
    [MaxLength(500)]
    public string? Keywords { get; set; }
    
    [MaxLength(500)]
    public string? Context { get; set; }
    
    public DateTime? EventDate { get; set; }
    
    [NotMapped]
    public SirahPeriod SirahPeriod 
    { 
        get => (SirahPeriod)Period; 
        set => Period = (int)value; 
    }
}

/// <summary>
/// Database entity for Tajweed rules
/// </summary>
[Table("TajweedRules")]
public class TajweedRuleEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Example { get; set; }
    
    [MaxLength(50)]
    public string Language { get; set; } = "en";
    
    [MaxLength(200)]
    public string? Category { get; set; }
    
    [MaxLength(500)]
    public string? Keywords { get; set; }
}

/// <summary>
/// Database entity for verse-specific Tajweed data
/// </summary>
[Table("VerseTajweedData")]
public class VerseTajweedEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int SurahNumber { get; set; }
    
    [Required]
    public int VerseNumber { get; set; }
    
    [Required]
    public int TajweedRuleId { get; set; }
    
    public int StartPosition { get; set; }
    
    public int EndPosition { get; set; }
    
    [MaxLength(200)]
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual TajweedRuleEntity TajweedRule { get; set; } = null!;
}

/// <summary>
/// Database entity for common Tajweed mistakes
/// </summary>
[Table("CommonMistakes")]
public class CommonMistakeEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int SurahNumber { get; set; }
    
    [Required]
    public int VerseNumber { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string MistakeType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Correction { get; set; }
    
    [MaxLength(50)]
    public string Language { get; set; } = "en";
    
    public int StartPosition { get; set; }
    
    public int EndPosition { get; set; }
}

/// <summary>
/// Database entity for search history
/// </summary>
[Table("SearchHistory")]
public class SearchHistoryEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string SearchQuery { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string SearchMode { get; set; } = string.Empty; // "local", "ai", "hybrid"
    
    [Required]
    [MaxLength(50)]
    public string Language { get; set; } = "en";
    
    [Required]
    public DateTime SearchDateTime { get; set; }
    
    [MaxLength(500)]
    public string? SelectedContentTypes { get; set; } // JSON array of selected types
      public int ResultsCount { get; set; }
    
    public double SearchDurationMs { get; set; }
    
    [MaxLength(100)]
    public string? UserIdentifier { get; set; } // For future user tracking
    
    [MaxLength(2000)]
    public string? SearchContext { get; set; } // Additional context about the search
    
    // AI-related properties
    public bool HasAIResponse { get; set; }
    
    [MaxLength(3000)]
    public string? AIResponseSummary { get; set; }
    
    public double? AIConfidenceScore { get; set; }
    
    [MaxLength(2000)]
    public string? QuranReferencesFromAI { get; set; } // JSON array of verse references
    
    [MaxLength(2000)]
    public string? HadithReferencesFromAI { get; set; } // JSON array of hadith references
    
    // Navigation properties
    public virtual ICollection<SearchResultHistoryEntity> SearchResults { get; set; } = new List<SearchResultHistoryEntity>();
}

/// <summary>
/// Database entity for individual search results in history
/// </summary>
[Table("SearchResultHistory")]
public class SearchResultHistoryEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int SearchHistoryId { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;
      [Required]
    [MaxLength(3000)]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(3000)]
    public string? ArabicText { get; set; } // For Quran verses and Arabic content
    
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty; // "Verse", "Hadith", "FiqhRuling", etc.
    
    [MaxLength(1000)]
    public string? Source { get; set; }
    
    [MaxLength(500)]
    public string? Reference { get; set; }
    
    public double RelevanceScore { get; set; }
    
    public int ResultPosition { get; set; } // Position in search results (1, 2, 3, etc.)
    
    [MaxLength(50)]
    public string Language { get; set; } = "en";
    
    [MaxLength(1000)]
    public string? AdditionalMetadata { get; set; } // JSON for extra data
    
    // Navigation properties
    public virtual SearchHistoryEntity SearchHistory { get; set; } = null!;
}

/// <summary>
/// Database entity for search analytics and insights
/// </summary>
[Table("SearchAnalytics")]
public class SearchAnalyticsEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string SearchTerm { get; set; } = string.Empty;
      [Required]
    public DateTime Date { get; set; }
    
    public int SearchCount { get; set; } = 1;
    
    [MaxLength(50)]
    public string Language { get; set; } = "en";
    
    [MaxLength(50)]
    public string SearchMode { get; set; } = string.Empty; // "local", "ai", "hybrid"
    
    [MaxLength(100)]
    public string MostCommonContentType { get; set; } = string.Empty;
    
    public int TotalSearches { get; set; }
    
    public int TotalResults { get; set; }
    
    public double TotalDurationMs { get; set; }
    
    public double AverageResultsPerSearch { get; set; }
    
    public double AverageDurationMs { get; set; }
    
    public double AverageResultsCount { get; set; }
    
    public double AverageSearchDuration { get; set; }
    
    [MaxLength(50)]
    public string PreferredSearchMode { get; set; } = string.Empty;
}
