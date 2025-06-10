using Muwasala.Core.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Muwasala.Agents;

/// <summary>
/// Quality assurance agent that validates, enhances, and ensures the authenticity 
/// of Islamic knowledge responses
/// </summary>
public class ResponseQualityAgent
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ResponseQualityAgent> _logger;
    private readonly Dictionary<string, QualityRule> _qualityRules;
    private readonly HashSet<string> _authenticSourceKeywords;
    private readonly HashSet<string> _prohibitedContent;

    public ResponseQualityAgent(HttpClient httpClient, ILogger<ResponseQualityAgent> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _qualityRules = InitializeQualityRules();
        _authenticSourceKeywords = InitializeAuthenticSourceKeywords();
        _prohibitedContent = InitializeProhibitedContent();
    }

    public async Task<QualityAssessmentResult> ValidateResponseAsync(
        string response, 
        QueryAnalysisResult queryAnalysis, 
        List<WebSearchResult> webResults)
    {
        try
        {
            var assessment = new QualityAssessmentResult
            {
                OriginalResponse = response,
                QueryContext = queryAnalysis,
                WebSources = webResults,
                ValidationTimestamp = DateTime.UtcNow
            };

            // Run parallel quality checks
            await Task.WhenAll(
                ValidateIslamicAccuracyAsync(response, assessment),
                CheckSourceAuthenticityAsync(response, webResults, assessment),
                ValidateLanguageQualityAsync(response, queryAnalysis.Language, assessment),
                CheckContentAppropriatenessAsync(response, assessment),
                ValidateStructureAndClarityAsync(response, assessment),
                CheckFactualConsistencyAsync(response, webResults, assessment),
                ValidateIslamicEthicsAsync(response, assessment)
            );

            // Calculate overall quality score
            assessment.OverallQualityScore = CalculateOverallScore(assessment);

            // Generate improvement suggestions if needed
            if (assessment.OverallQualityScore < 0.8f)
            {
                assessment.ImprovementSuggestions = await GenerateImprovementSuggestionsAsync(response, assessment);
            }

            // Enhance response if possible
            if (assessment.OverallQualityScore > 0.6f && assessment.OverallQualityScore < 0.9f)
            {
                assessment.EnhancedResponse = await EnhanceResponseAsync(response, queryAnalysis, webResults);
            }

            _logger.LogInformation("Response quality validation completed with score: {Score}", 
                assessment.OverallQualityScore);

            return assessment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during response quality validation");
            return CreateFallbackAssessment(response, queryAnalysis);
        }
    }

    private async Task ValidateIslamicAccuracyAsync(string response, QualityAssessmentResult assessment)
    {
        var accuracy = new IslamicAccuracyCheck
        {
            HasQuranReferences = ContainsQuranReferences(response),
            HasHadithReferences = ContainsHadithReferences(response),
            HasScholarlyReferences = ContainsScholarlyReferences(response),
            UsesIslamicTerminology = ContainsIslamicTerminology(response),
            FollowsIslamicEtiquette = FollowsIslamicEtiquette(response)
        };

        // Check for common misconceptions
        accuracy.HasMisconceptions = await CheckForMisconceptionsAsync(response);
        
        // Validate theological soundness
        accuracy.IsTheologicallySound = ValidateTheologicalSoundness(response);

        // Calculate accuracy score
        var score = 0f;
        score += accuracy.HasQuranReferences ? 0.2f : 0f;
        score += accuracy.HasHadithReferences ? 0.15f : 0f;
        score += accuracy.HasScholarlyReferences ? 0.1f : 0f;
        score += accuracy.UsesIslamicTerminology ? 0.15f : 0f;
        score += accuracy.FollowsIslamicEtiquette ? 0.2f : 0f;
        score += accuracy.IsTheologicallySound ? 0.3f : 0f;
        score -= accuracy.HasMisconceptions ? 0.3f : 0f;

        accuracy.AccuracyScore = Math.Max(0f, Math.Min(1f, score));
        assessment.IslamicAccuracy = accuracy;
    }

    private async Task CheckSourceAuthenticityAsync(string response, List<WebSearchResult> webResults, QualityAssessmentResult assessment)
    {        var authenticity = new SourceAuthenticityCheck
        {
            TrustedSourceCount = webResults.Count(r => r.AuthenticityScore > 0.8),
            AverageSourceAuthenticity = webResults.Any() ? (float)webResults.Average(r => r.AuthenticityScore) : 0f,
            HasPrimarySourceReferences = HasPrimarySourceReferences(response, webResults),
            SourceDiversityScore = CalculateSourceDiversity(webResults)
        };

        // Check if response aligns with trusted sources
        authenticity.AlignmentWithSources = await CheckAlignmentWithSourcesAsync(response, webResults);
        
        // Validate citation quality
        authenticity.CitationQuality = ValidateCitationQuality(response);

        authenticity.AuthenticityScore = CalculateAuthenticityScore(authenticity);
        assessment.SourceAuthenticity = authenticity;
    }

    private async Task ValidateLanguageQualityAsync(string response, string language, QualityAssessmentResult assessment)
    {
        var languageCheck = new LanguageQualityCheck
        {
            Language = language,
            GrammarScore = await ValidateGrammarAsync(response, language),
            ClarityScore = ValidateClarity(response),
            ReadabilityScore = CalculateReadability(response),
            ToneAppropriateness = ValidateTone(response),
            TerminologyConsistency = ValidateTerminologyConsistency(response)
        };

        languageCheck.OverallLanguageScore = (
            languageCheck.GrammarScore +
            languageCheck.ClarityScore +
            languageCheck.ReadabilityScore +
            languageCheck.ToneAppropriateness +
            languageCheck.TerminologyConsistency
        ) / 5f;

        assessment.LanguageQuality = languageCheck;
    }

    private async Task CheckContentAppropriatenessAsync(string response, QualityAssessmentResult assessment)
    {
        var appropriateness = new ContentAppropriatenessCheck
        {
            IsHalal = !ContainsProhibitedContent(response),
            IsRespectful = ValidateRespectfulness(response),
            IsEducational = ValidateEducationalValue(response),
            AvoidsSectarianBias = ValidateNonSectarian(response),
            PromotesUnity = ValidateUnityPromotion(response)
        };

        appropriateness.AppropriatenessScore = CalculateAppropriatenessScore(appropriateness);
        assessment.ContentAppropriateness = appropriateness;
    }

    private async Task ValidateStructureAndClarityAsync(string response, QualityAssessmentResult assessment)
    {
        var structure = new StructuralQualityCheck
        {
            HasClearIntroduction = HasClearIntroduction(response),
            HasLogicalFlow = HasLogicalFlow(response),
            HasClearConclusion = HasClearConclusion(response),
            UsesProperFormatting = UsesProperFormatting(response),
            AppropriateLength = ValidateLength(response)
        };

        structure.StructuralScore = CalculateStructuralScore(structure);
        assessment.StructuralQuality = structure;
    }

    private async Task CheckFactualConsistencyAsync(string response, List<WebSearchResult> webResults, QualityAssessmentResult assessment)
    {
        var consistency = new FactualConsistencyCheck
        {
            ConsistentWithSources = await ValidateConsistencyWithSourcesAsync(response, webResults),
            NoContradictions = !ContainsContradictions(response),
            FactuallyAccurate = await ValidateFactualAccuracyAsync(response),
            CurrentAndRelevant = ValidateCurrentRelevance(response)
        };

        consistency.ConsistencyScore = CalculateConsistencyScore(consistency);
        assessment.FactualConsistency = consistency;
    }

    private async Task ValidateIslamicEthicsAsync(string response, QualityAssessmentResult assessment)
    {
        var ethics = new IslamicEthicsCheck
        {
            PromotesGoodConduct = ValidateGoodConduct(response),
            EncouragesVirtue = ValidateVirtueEncouragement(response),
            DiscouragsVice = ValidateViceDiscouragement(response),
            PromotesJustice = ValidateJusticePromotion(response),
            EncouragesKnowledge = ValidateKnowledgeEncouragement(response)
        };

        ethics.EthicsScore = CalculateEthicsScore(ethics);
        assessment.IslamicEthics = ethics;
    }

    private async Task<string> EnhanceResponseAsync(string originalResponse, QueryAnalysisResult queryAnalysis, List<WebSearchResult> webResults)
    {
        try
        {
            // Build enhancement prompt for DeepSeek
            var enhancementPrompt = $@"
Please enhance this Islamic knowledge response to make it more comprehensive and authentic:

Original Query: {queryAnalysis.OriginalQuery}
Original Response: {originalResponse}

Enhancement Guidelines:
1. Add relevant Quranic verses if applicable
2. Include authentic hadith references where appropriate
3. Improve clarity and structure
4. Ensure proper Islamic etiquette (saying peace be upon him, etc.)
5. Add practical guidance if relevant
6. Maintain authenticity and avoid innovations

Sources to consider:
{string.Join("\n", webResults.Take(3).Select(r => $"- {r.Title}: {r.Snippet}"))}

Enhanced Response:";

            var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/generate", new
            {
                model = "deepseek-r1:7b",
                prompt = enhancementPrompt,
                stream = false,
                options = new
                {
                    temperature = 0.3,
                    max_tokens = 1000
                }
            });

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(jsonResponse);
                
                if (!string.IsNullOrEmpty(ollamaResponse?.Response))
                {
                    return ollamaResponse.Response.Trim();
                }
            }

            return originalResponse; // Return original if enhancement fails
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enhance response");
            return originalResponse;
        }
    }

    private async Task<List<string>> GenerateImprovementSuggestionsAsync(string response, QualityAssessmentResult assessment)
    {
        var suggestions = new List<string>();

        if (assessment.IslamicAccuracy?.AccuracyScore < 0.7f)
        {
            suggestions.Add("Add authentic Quranic verses or hadith references to support the answer");
            suggestions.Add("Ensure theological accuracy by consulting authentic Islamic sources");
        }

        if (assessment.SourceAuthenticity?.AuthenticityScore < 0.7f)
        {
            suggestions.Add("Include references to trusted Islamic scholars or classical texts");
            suggestions.Add("Verify information against multiple authentic sources");
        }

        if (assessment.LanguageQuality?.OverallLanguageScore < 0.7f)
        {
            suggestions.Add("Improve clarity and readability of the response");
            suggestions.Add("Use appropriate Islamic terminology consistently");
        }

        if (assessment.StructuralQuality?.StructuralScore < 0.7f)
        {
            suggestions.Add("Organize the response with clear introduction, body, and conclusion");
            suggestions.Add("Use proper formatting and logical flow");
        }

        return suggestions;
    }

    // Helper methods for validation
    private bool ContainsQuranReferences(string response)
    {
        var quranPattern = @"\b(quran|qur'an|verse|ayah|surah|\d+:\d+)\b";
        return Regex.IsMatch(response, quranPattern, RegexOptions.IgnoreCase);
    }

    private bool ContainsHadithReferences(string response)
    {
        var hadithPattern = @"\b(hadith|hadeeth|sunnah|bukhari|muslim|tirmidhi|abu dawud|nasai|ibn majah)\b";
        return Regex.IsMatch(response, hadithPattern, RegexOptions.IgnoreCase);
    }

    private bool ContainsScholarlyReferences(string response)
    {
        var scholarPattern = @"\b(imam|scholar|sheikh|mufti|according to|as stated by)\b";
        return Regex.IsMatch(response, scholarPattern, RegexOptions.IgnoreCase);
    }

    private bool ContainsIslamicTerminology(string response)
    {
        return _authenticSourceKeywords.Any(keyword => 
            response.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private bool FollowsIslamicEtiquette(string response)
    {
        var etiquettePatterns = new[]
        {
            @"\b(peace be upon him|pbuh|sallallahu alayhi wasallam)\b",
            @"\b(allah|god)\b",
            @"\b(inshallah|mashallah|subhanallah|alhamdulillah)\b"
        };

        return etiquettePatterns.Any(pattern => 
            Regex.IsMatch(response, pattern, RegexOptions.IgnoreCase));
    }

    private bool ContainsProhibitedContent(string response)
    {
        return _prohibitedContent.Any(content => 
            response.Contains(content, StringComparison.OrdinalIgnoreCase));
    }

    private float CalculateOverallScore(QualityAssessmentResult assessment)
    {
        var scores = new List<float>();

        if (assessment.IslamicAccuracy != null)
            scores.Add(assessment.IslamicAccuracy.AccuracyScore * 0.25f);

        if (assessment.SourceAuthenticity != null)
            scores.Add(assessment.SourceAuthenticity.AuthenticityScore * 0.2f);

        if (assessment.LanguageQuality != null)
            scores.Add(assessment.LanguageQuality.OverallLanguageScore * 0.15f);

        if (assessment.ContentAppropriateness != null)
            scores.Add(assessment.ContentAppropriateness.AppropriatenessScore * 0.15f);

        if (assessment.StructuralQuality != null)
            scores.Add(assessment.StructuralQuality.StructuralScore * 0.1f);

        if (assessment.FactualConsistency != null)
            scores.Add(assessment.FactualConsistency.ConsistencyScore * 0.1f);

        if (assessment.IslamicEthics != null)
            scores.Add(assessment.IslamicEthics.EthicsScore * 0.05f);

        return scores.Any() ? scores.Sum() : 0f;
    }

    // Initialize methods
    private Dictionary<string, QualityRule> InitializeQualityRules()
    {
        return new Dictionary<string, QualityRule>
        {
            ["quran_reference"] = new QualityRule { Weight = 0.2f, Required = false, Description = "Contains Quranic references" },
            ["hadith_reference"] = new QualityRule { Weight = 0.15f, Required = false, Description = "Contains hadith references" },
            ["islamic_etiquette"] = new QualityRule { Weight = 0.2f, Required = true, Description = "Follows Islamic etiquette" },
            ["no_prohibited"] = new QualityRule { Weight = 0.3f, Required = true, Description = "Contains no prohibited content" },
            ["clear_structure"] = new QualityRule { Weight = 0.15f, Required = true, Description = "Has clear structure" }
        };
    }

    private HashSet<string> InitializeAuthenticSourceKeywords()
    {
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "allah", "prophet", "muhammad", "quran", "hadith", "sunnah", "islam", "muslim",
            "salah", "prayer", "zakat", "hajj", "ramadan", "fasting", "halal", "haram",
            "imam", "scholar", "fiqh", "sharia", "ummah", "jihad", "tawhid", "iman"
        };
    }

    private HashSet<string> InitializeProhibitedContent()
    {
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "innovation", "bidah", "shirk", "extremism", "terrorism", "violence",
            "hatred", "discrimination", "sectarian", "blasphemy"
        };
    }

    private QualityAssessmentResult CreateFallbackAssessment(string response, QueryAnalysisResult queryAnalysis)
    {
        return new QualityAssessmentResult
        {
            OriginalResponse = response,
            QueryContext = queryAnalysis,
            OverallQualityScore = 0.5f,
            ValidationTimestamp = DateTime.UtcNow,
            ImprovementSuggestions = ["Response quality validation failed - manual review recommended"]
        };
    }

    // Placeholder implementations for complex validation methods
    private async Task<bool> CheckForMisconceptionsAsync(string response) => false;
    private bool ValidateTheologicalSoundness(string response) => true;
    private bool HasPrimarySourceReferences(string response, List<WebSearchResult> sources) => sources.Any();
    private float CalculateSourceDiversity(List<WebSearchResult> sources) => 0.8f;
    private async Task<float> CheckAlignmentWithSourcesAsync(string response, List<WebSearchResult> sources) => 0.8f;
    private float ValidateCitationQuality(string response) => 0.7f;
    private float CalculateAuthenticityScore(SourceAuthenticityCheck check) => check.AverageSourceAuthenticity;
    private async Task<float> ValidateGrammarAsync(string response, string language) => 0.8f;
    private float ValidateClarity(string response) => 0.8f;
    private float CalculateReadability(string response) => 0.8f;
    private float ValidateTone(string response) => 0.8f;
    private float ValidateTerminologyConsistency(string response) => 0.8f;
    private bool ValidateRespectfulness(string response) => true;
    private bool ValidateEducationalValue(string response) => true;
    private bool ValidateNonSectarian(string response) => true;
    private bool ValidateUnityPromotion(string response) => true;
    private float CalculateAppropriatenessScore(ContentAppropriatenessCheck check) => 0.8f;
    private bool HasClearIntroduction(string response) => true;
    private bool HasLogicalFlow(string response) => true;
    private bool HasClearConclusion(string response) => true;
    private bool UsesProperFormatting(string response) => true;
    private bool ValidateLength(string response) => response.Length > 50 && response.Length < 2000;
    private float CalculateStructuralScore(StructuralQualityCheck check) => 0.8f;
    private async Task<bool> ValidateConsistencyWithSourcesAsync(string response, List<WebSearchResult> sources) => true;
    private bool ContainsContradictions(string response) => false;
    private async Task<bool> ValidateFactualAccuracyAsync(string response) => true;
    private bool ValidateCurrentRelevance(string response) => true;
    private float CalculateConsistencyScore(FactualConsistencyCheck check) => 0.8f;
    private bool ValidateGoodConduct(string response) => true;
    private bool ValidateVirtueEncouragement(string response) => true;
    private bool ValidateViceDiscouragement(string response) => true;
    private bool ValidateJusticePromotion(string response) => true;
    private bool ValidateKnowledgeEncouragement(string response) => true;
    private float CalculateEthicsScore(IslamicEthicsCheck check) => 0.8f;
}

// Quality assessment models
public record QualityAssessmentResult
{
    public required string OriginalResponse { get; init; }
    public QueryAnalysisResult? QueryContext { get; init; }
    public List<WebSearchResult> WebSources { get; init; } = new();
    public IslamicAccuracyCheck? IslamicAccuracy { get; set; }
    public SourceAuthenticityCheck? SourceAuthenticity { get; set; }
    public LanguageQualityCheck? LanguageQuality { get; set; }
    public ContentAppropriatenessCheck? ContentAppropriateness { get; set; }
    public StructuralQualityCheck? StructuralQuality { get; set; }
    public FactualConsistencyCheck? FactualConsistency { get; set; }
    public IslamicEthicsCheck? IslamicEthics { get; set; }
    public float OverallQualityScore { get; set; }
    public string? EnhancedResponse { get; set; }
    public List<string> ImprovementSuggestions { get; set; } = new();
    public DateTime ValidationTimestamp { get; init; }
}

public record QualityRule
{
    public float Weight { get; init; }
    public bool Required { get; init; }
    public string Description { get; init; } = "";
}

public record IslamicAccuracyCheck
{
    public bool HasQuranReferences { get; set; }
    public bool HasHadithReferences { get; set; }
    public bool HasScholarlyReferences { get; set; }
    public bool UsesIslamicTerminology { get; set; }
    public bool FollowsIslamicEtiquette { get; set; }
    public bool HasMisconceptions { get; set; }
    public bool IsTheologicallySound { get; set; }
    public float AccuracyScore { get; set; }
}

public record SourceAuthenticityCheck
{
    public int TrustedSourceCount { get; set; }
    public float AverageSourceAuthenticity { get; set; }
    public bool HasPrimarySourceReferences { get; set; }
    public float SourceDiversityScore { get; set; }
    public float AlignmentWithSources { get; set; }
    public float CitationQuality { get; set; }
    public float AuthenticityScore { get; set; }
}

public record LanguageQualityCheck
{
    public string Language { get; set; } = "en";
    public float GrammarScore { get; set; }
    public float ClarityScore { get; set; }
    public float ReadabilityScore { get; set; }
    public float ToneAppropriateness { get; set; }
    public float TerminologyConsistency { get; set; }
    public float OverallLanguageScore { get; set; }
}

public record ContentAppropriatenessCheck
{
    public bool IsHalal { get; set; }
    public bool IsRespectful { get; set; }
    public bool IsEducational { get; set; }
    public bool AvoidsSectarianBias { get; set; }
    public bool PromotesUnity { get; set; }
    public float AppropriatenessScore { get; set; }
}

public record StructuralQualityCheck
{
    public bool HasClearIntroduction { get; set; }
    public bool HasLogicalFlow { get; set; }
    public bool HasClearConclusion { get; set; }
    public bool UsesProperFormatting { get; set; }
    public bool AppropriateLength { get; set; }
    public float StructuralScore { get; set; }
}

public record FactualConsistencyCheck
{
    public bool ConsistentWithSources { get; set; }
    public bool NoContradictions { get; set; }
    public bool FactuallyAccurate { get; set; }
    public bool CurrentAndRelevant { get; set; }
    public float ConsistencyScore { get; set; }
}

public record IslamicEthicsCheck
{
    public bool PromotesGoodConduct { get; set; }
    public bool EncouragesVirtue { get; set; }
    public bool DiscouragsVice { get; set; }
    public bool PromotesJustice { get; set; }
    public bool EncouragesKnowledge { get; set; }
    public float EthicsScore { get; set; }
}

public record OllamaResponse
{
    public string Response { get; init; } = "";
    public bool Done { get; init; }
}
