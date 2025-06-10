using Muwasala.Core.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Muwasala.Agents;

/// <summary>
/// Advanced query analysis agent that provides detailed intent analysis and classification
/// for Islamic knowledge queries using sophisticated NLP techniques
/// </summary>
public class QueryAnalysisAgent
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QueryAnalysisAgent> _logger;
    private readonly Dictionary<string, string[]> _islamicTopicKeywords;
    private readonly Dictionary<string, string[]> _languagePatterns;

    public QueryAnalysisAgent(HttpClient httpClient, ILogger<QueryAnalysisAgent> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _islamicTopicKeywords = InitializeIslamicKeywords();
        _languagePatterns = InitializeLanguagePatterns();
    }

    public async Task<QueryAnalysisResult> AnalyzeQueryAsync(string query, string language = "en")
    {
        try
        {
            var analysis = new QueryAnalysisResult
            {
                OriginalQuery = query,
                Language = language,
                Timestamp = DateTime.UtcNow
            };

            // Perform multiple analysis techniques in parallel
            var analysisTask = Task.WhenAll(
                ClassifyQueryTypeAsync(query, analysis),
                ExtractIslamicEntitiesAsync(query, analysis),
                DetermineComplexityLevelAsync(query, analysis),
                AnalyzeIntentAndContextAsync(query, analysis),
                GenerateSearchKeywordsAsync(query, analysis)
            );

            await analysisTask;

            // Calculate confidence score based on all analysis results
            analysis.ConfidenceScore = CalculateOverallConfidence(analysis);

            _logger.LogInformation("Query analysis completed for: {Query} with confidence: {Confidence}", 
                query, analysis.ConfidenceScore);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing query: {Query}", query);
            return CreateFallbackAnalysis(query, language);
        }
    }

    private async Task ClassifyQueryTypeAsync(string query, QueryAnalysisResult analysis)
    {
        var queryLower = query.ToLowerInvariant();
        
        // Religious practice queries
        if (ContainsAny(queryLower, ["prayer", "salah", "namaz", "wudu", "ablution", "fast", "sawm", "hajj", "umrah"]))
        {
            analysis.QueryTypes.Add(QueryType.ReligiousPractice);
        }

        // Quran related queries
        if (ContainsAny(queryLower, ["quran", "qur'an", "verse", "ayah", "surah", "chapter", "recitation"]))
        {
            analysis.QueryTypes.Add(QueryType.QuranStudy);
        }

        // Hadith related queries
        if (ContainsAny(queryLower, ["hadith", "hadeeth", "sunnah", "prophet", "bukhari", "muslim", "tirmidhi"]))
        {
            analysis.QueryTypes.Add(QueryType.HadithStudy);
        }

        // Islamic law queries
        if (ContainsAny(queryLower, ["halal", "haram", "fiqh", "sharia", "ruling", "permissible", "forbidden", "law"]))
        {
            analysis.QueryTypes.Add(QueryType.IslamicLaw);
        }

        // Theological queries
        if (ContainsAny(queryLower, ["allah", "tawhid", "belief", "faith", "iman", "theology", "creed", "aqidah"]))
        {
            analysis.QueryTypes.Add(QueryType.Theology);
        }

        // Historical queries
        if (ContainsAny(queryLower, ["history", "historical", "companions", "sahaba", "caliphate", "battle"]))
        {
            analysis.QueryTypes.Add(QueryType.IslamicHistory);
        }

        // If no specific type found, classify as general
        if (!analysis.QueryTypes.Any())
        {
            analysis.QueryTypes.Add(QueryType.GeneralInquiry);
        }
    }

    private async Task ExtractIslamicEntitiesAsync(string query, QueryAnalysisResult analysis)
    {
        // Extract Islamic entities using pattern matching and keyword recognition
        var entities = new List<IslamicEntity>();

        // Prophet names
        var prophetPattern = @"\b(muhammad|prophet|rasulullah|nabi|moses|musa|jesus|isa|abraham|ibrahim|noah|nuh)\b";
        var prophetMatches = Regex.Matches(query, prophetPattern, RegexOptions.IgnoreCase);
        foreach (Match match in prophetMatches)
        {
            entities.Add(new IslamicEntity 
            { 
                Text = match.Value, 
                Type = EntityType.Prophet,
                Confidence = 0.95f
            });
        }

        // Quran references
        var quranPattern = @"\b(surah|chapter)\s+(\w+|\d+)|(\d+):(\d+)\b";
        var quranMatches = Regex.Matches(query, quranPattern, RegexOptions.IgnoreCase);
        foreach (Match match in quranMatches)
        {
            entities.Add(new IslamicEntity 
            { 
                Text = match.Value, 
                Type = EntityType.QuranReference,
                Confidence = 0.90f
            });
        }

        // Islamic terms
        foreach (var (topic, keywords) in _islamicTopicKeywords)
        {
            foreach (var keyword in keywords)
            {
                if (query.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    entities.Add(new IslamicEntity 
                    { 
                        Text = keyword, 
                        Type = EntityType.IslamicTerm,
                        Category = topic,
                        Confidence = 0.80f
                    });
                }
            }
        }

        analysis.ExtractedEntities = entities;
    }

    private async Task DetermineComplexityLevelAsync(string query, QueryAnalysisResult analysis)
    {
        var wordCount = query.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var questionWords = new[] { "what", "how", "why", "when", "where", "which", "who", "explain", "describe" };
        var hasQuestionWords = questionWords.Any(q => query.Contains(q, StringComparison.OrdinalIgnoreCase));
        var hasMultipleClauses = query.Contains("and") || query.Contains("or") || query.Contains("but");

        if (wordCount < 5 && !hasQuestionWords)
        {
            analysis.ComplexityLevel = ComplexityLevel.Simple;
        }
        else if (wordCount <= 15 && hasQuestionWords && !hasMultipleClauses)
        {
            analysis.ComplexityLevel = ComplexityLevel.Moderate;
        }
        else
        {
            analysis.ComplexityLevel = ComplexityLevel.Complex;
        }
    }

    private async Task AnalyzeIntentAndContextAsync(string query, QueryAnalysisResult analysis)
    {
        var queryLower = query.ToLowerInvariant();

        // Determine primary intent
        if (ContainsAny(queryLower, ["what is", "define", "meaning", "explain"]))
        {
            analysis.PrimaryIntent = QueryIntent.SeekDefinition;
        }
        else if (ContainsAny(queryLower, ["how to", "how do", "steps", "procedure"]))
        {
            analysis.PrimaryIntent = QueryIntent.SeekGuidance;
        }
        else if (ContainsAny(queryLower, ["is it allowed", "can i", "permissible", "halal", "haram"]))
        {
            analysis.PrimaryIntent = QueryIntent.SeekRuling;
        }
        else if (ContainsAny(queryLower, ["verse", "quote", "reference", "citation"]))
        {
            analysis.PrimaryIntent = QueryIntent.SeekReference;
        }
        else if (ContainsAny(queryLower, ["compare", "difference", "versus", "vs"]))
        {
            analysis.PrimaryIntent = QueryIntent.SeekComparison;
        }
        else
        {
            analysis.PrimaryIntent = QueryIntent.GeneralInquiry;
        }

        // Determine emotional context
        if (ContainsAny(queryLower, ["worried", "concerned", "confused", "doubt", "anxiety"]))
        {
            analysis.EmotionalContext = EmotionalContext.Concerned;
        }
        else if (ContainsAny(queryLower, ["grateful", "thank", "blessed", "alhamdulillah"]))
        {
            analysis.EmotionalContext = EmotionalContext.Grateful;
        }
        else if (ContainsAny(queryLower, ["urgent", "immediately", "quick", "fast"]))
        {
            analysis.EmotionalContext = EmotionalContext.Urgent;
        }
        else
        {
            analysis.EmotionalContext = EmotionalContext.Neutral;
        }
    }

    private async Task GenerateSearchKeywordsAsync(string query, QueryAnalysisResult analysis)
    {
        var keywords = new List<string>();
        
        // Extract important nouns and adjectives
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var stopWords = new HashSet<string> { "the", "is", "are", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by" };
        
        foreach (var word in words)
        {
            var cleanWord = word.Trim(',', '.', '?', '!', ';', ':').ToLowerInvariant();
            if (!stopWords.Contains(cleanWord) && cleanWord.Length > 2)
            {
                keywords.Add(cleanWord);
            }
        }

        // Add related Islamic terms based on extracted entities
        foreach (var entity in analysis.ExtractedEntities)
        {
            if (entity.Type == EntityType.IslamicTerm && !string.IsNullOrEmpty(entity.Category))
            {
                if (_islamicTopicKeywords.TryGetValue(entity.Category, out var relatedTerms))
                {
                    keywords.AddRange(relatedTerms.Take(3)); // Add top 3 related terms
                }
            }
        }

        analysis.SearchKeywords = keywords.Distinct().Take(10).ToList();
    }

    private float CalculateOverallConfidence(QueryAnalysisResult analysis)
    {
        var confidenceFactors = new List<float>();

        // Query type confidence
        confidenceFactors.Add(analysis.QueryTypes.Any() ? 0.8f : 0.3f);

        // Entity extraction confidence
        if (analysis.ExtractedEntities.Any())
        {
            confidenceFactors.Add(analysis.ExtractedEntities.Average(e => e.Confidence));
        }
        else
        {
            confidenceFactors.Add(0.4f);
        }

        // Complexity assessment confidence
        confidenceFactors.Add(0.9f); // Always high as it's rule-based

        // Intent recognition confidence
        confidenceFactors.Add(analysis.PrimaryIntent != QueryIntent.GeneralInquiry ? 0.85f : 0.5f);

        return confidenceFactors.Average();
    }

    private QueryAnalysisResult CreateFallbackAnalysis(string query, string language)
    {
        return new QueryAnalysisResult
        {
            OriginalQuery = query,
            Language = language,
            QueryTypes = [QueryType.GeneralInquiry],
            PrimaryIntent = QueryIntent.GeneralInquiry,
            ComplexityLevel = ComplexityLevel.Moderate,
            EmotionalContext = EmotionalContext.Neutral,
            ExtractedEntities = [],
            SearchKeywords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(5).ToList(),
            ConfidenceScore = 0.3f,
            Timestamp = DateTime.UtcNow
        };
    }

    private bool ContainsAny(string text, string[] keywords)
    {
        return keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private Dictionary<string, string[]> InitializeIslamicKeywords()
    {
        return new Dictionary<string, string[]>
        {
            ["Prayer"] = ["salah", "prayer", "namaz", "fajr", "dhuhr", "asr", "maghrib", "isha", "wudu", "ablution", "qibla", "imam", "congregation"],
            ["Quran"] = ["quran", "qur'an", "verse", "ayah", "surah", "chapter", "recitation", "tafsir", "revelation", "mushaf"],
            ["Hadith"] = ["hadith", "hadeeth", "sunnah", "bukhari", "muslim", "tirmidhi", "narrator", "chain", "isnad"],
            ["Fiqh"] = ["halal", "haram", "fiqh", "ruling", "jurisprudence", "madhab", "school", "opinion", "fatwa"],
            ["Beliefs"] = ["allah", "tawhid", "iman", "faith", "belief", "monotheism", "angels", "prophets", "day of judgment"],
            ["Pilgrimage"] = ["hajj", "umrah", "pilgrimage", "mecca", "kaaba", "tawaf", "sai", "arafat", "mina"],
            ["Charity"] = ["zakat", "charity", "sadaqah", "donation", "poor", "needy", "wealth", "purification"],
            ["Fasting"] = ["sawm", "fast", "fasting", "ramadan", "iftar", "suhur", "breaking fast"]
        };
    }

    private Dictionary<string, string[]> InitializeLanguagePatterns()
    {
        return new Dictionary<string, string[]>
        {
            ["en"] = ["what", "how", "when", "where", "why", "which", "who", "is", "are", "can", "should", "would"],
            ["ar"] = ["ما", "كيف", "متى", "أين", "لماذا", "أي", "من", "هل", "يجب", "ينبغي"],
            ["ur"] = ["کیا", "کیسے", "کب", "کہاں", "کیوں", "کون سا", "کون", "ہے", "ہیں", "کر سکتے"],
            ["fr"] = ["quoi", "comment", "quand", "où", "pourquoi", "quel", "qui", "est", "sont", "peut", "devrait"]
        };
    }
}

// Enhanced models for query analysis
public record QueryAnalysisResult
{
    public required string OriginalQuery { get; init; }
    public required string Language { get; init; }
    public List<QueryType> QueryTypes { get; init; } = new();
    public QueryIntent PrimaryIntent { get; set; }
    public ComplexityLevel ComplexityLevel { get; set; }
    public EmotionalContext EmotionalContext { get; set; }
    public List<IslamicEntity> ExtractedEntities { get; set; } = new();
    public List<string> SearchKeywords { get; set; } = new();
    public float ConfidenceScore { get; set; }
    public DateTime Timestamp { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

public record IslamicEntity
{
    public required string Text { get; init; }
    public EntityType Type { get; init; }
    public string? Category { get; init; }
    public float Confidence { get; init; }
    public Dictionary<string, string> Properties { get; init; } = new();
}

public enum QueryType
{
    GeneralInquiry,
    ReligiousPractice,
    QuranStudy,
    HadithStudy,
    IslamicLaw,
    Theology,
    IslamicHistory,
    PersonalGuidance,
    Comparative,
    Academic
}

public enum QueryIntent
{
    GeneralInquiry,
    SeekDefinition,
    SeekGuidance,
    SeekRuling,
    SeekReference,
    SeekComparison,
    SeekValidation,
    SeekClarification
}

public enum ComplexityLevel
{
    Simple,
    Moderate,
    Complex,
    Expert
}

public enum EmotionalContext
{
    Neutral,
    Concerned,
    Grateful,
    Urgent,
    Confused,
    Seeking
}

public enum EntityType
{
    Prophet,
    QuranReference,
    HadithReference,
    IslamicTerm,
    ScholarName,
    GeographicLocation,
    HistoricalEvent,
    ReligiousPractice
}
