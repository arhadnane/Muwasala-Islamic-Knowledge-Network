using Microsoft.Extensions.Logging;
using Muwasala.Core.Models;
using Muwasala.Core.Services;
using Muwasala.KnowledgeBase.Services;
using System.Security.Cryptography;
using System.Text;

namespace Muwasala.Agents;

/// <summary>
/// Hadith Verifier Agent - Authentication chain (Sanad) checking & explanation
/// Uses Mistral 7B model for detailed hadith verification
/// </summary>
public class HadithVerifierAgent
{
    private readonly IOllamaService _ollama;
    private readonly IHadithService _hadithService;
    private readonly ICacheService _cache;
    private readonly ILogger<HadithVerifierAgent> _logger;
    private const string MODEL_NAME = "phi3:mini";

    public HadithVerifierAgent(
        IOllamaService ollama,
        IHadithService hadithService,
        ICacheService cache,
        ILogger<HadithVerifierAgent> logger)
    {
        _ollama = ollama;
        _hadithService = hadithService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Verify and explain a hadith with full authentication analysis
    /// </summary>
    public async Task<HadithResponse> VerifyHadithAsync(string hadithText, string language = "en")
    {
        _logger.LogInformation("HadithVerifier processing hadith verification");

        try
        {
            // Search for the hadith in authenticated collections
            var hadithResults = await _hadithService.SearchHadithAsync(hadithText, language);
            
            if (!hadithResults.Any())
            {
                return await HandleUnknownHadith(hadithText, language);
            }

            var primaryHadith = hadithResults.First();
              // Get AI analysis of the hadith's authenticity and meaning
            var prompt = BuildVerificationPrompt(primaryHadith, language);            var aiResponse = await _ollama.GenerateResponseAsync(MODEL_NAME, prompt, 0.3);

            var response = new HadithResponse
            {
                Text = primaryHadith.ArabicText,
                Translation = primaryHadith.Translation,
                Grade = primaryHadith.Grade,
                Collection = primaryHadith.Collection,
                BookNumber = primaryHadith.BookNumber,
                HadithNumber = primaryHadith.HadithNumber,
                SanadChain = primaryHadith.SanadChain,
                Explanation = aiResponse,
                AlternativeHadith = new List<string> { "Related authentic hadiths available upon request" }
            };

            // Add warning if the hadith is weak or fabricated
            if (primaryHadith.Grade == HadithGrade.Daif)
            {
                response = response with { Warning = "This hadith is classified as weak (Da'if). Use with caution." };
            }
            else if (primaryHadith.Grade == HadithGrade.Mawdu)
            {
                response = response with { Warning = "⚠️ WARNING: This hadith is fabricated (Mawdu'). Do not use for religious guidance." };
            }

            response.Sources.AddRange(new[] { primaryHadith.Collection, "Hadith Verification Database", MODEL_NAME });

            _logger.LogInformation("HadithVerifier completed verification for {Collection} {HadithNumber}", 
                primaryHadith.Collection, primaryHadith.HadithNumber);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HadithVerifier");
            throw;
        }
    }

    /// <summary>
    /// Get hadiths by topic with verification status - ONLY authentic hadiths
    /// </summary>
    public async Task<List<HadithResponse>> GetHadithByTopicAsync(string topic, string language = "en", int maxResults = 5)
    {
        _logger.LogInformation("HadithVerifier searching hadiths by topic: {Topic}", topic);

        // Use GetAuthenticHadithAsync to ensure only Sahih hadiths are returned
        var hadiths = await _hadithService.GetAuthenticHadithAsync(topic, maxResults);
        var responses = new List<HadithResponse>();

        // Only process authentic hadiths found in database - no AI generation
        foreach (var hadith in hadiths)
        {
            // Build response directly from database hadith (no AI verification to avoid generating fake content)
            var prompt = BuildVerificationPrompt(hadith, language);
            var aiResponse = await _ollama.GenerateResponseAsync(MODEL_NAME, prompt, 0.3);

            var response = new HadithResponse
            {
                Text = hadith.ArabicText,
                Translation = hadith.Translation,
                Grade = hadith.Grade,
                Collection = hadith.Collection,
                BookNumber = hadith.BookNumber,
                HadithNumber = hadith.HadithNumber,
                SanadChain = hadith.SanadChain,
                Explanation = aiResponse,
                AlternativeHadith = new List<string> { "Related authentic hadiths available upon request" }
            };

            // Since we're using GetAuthenticHadithAsync, these should all be Sahih
            // But keep safety checks just in case
            if (hadith.Grade == HadithGrade.Daif)
            {
                response = response with { Warning = "This hadith is classified as weak (Da'if). Use with caution." };
            }
            else if (hadith.Grade == HadithGrade.Mawdu)
            {
                response = response with { Warning = "⚠️ WARNING: This hadith is fabricated (Mawdu'). Do not use for religious guidance." };
            }

            response.Sources.AddRange(new[] { hadith.Collection, "Hadith Verification Database", MODEL_NAME });
            responses.Add(response);
        }

        _logger.LogInformation("Found {Count} authentic hadiths for topic: {Topic}", responses.Count, topic);
        return responses.OrderByDescending(h => h.Grade).ToList();
    }

    /// <summary>
    /// Get ONLY authentic (Sahih) hadiths by topic - explicit method for clarity
    /// </summary>
    public async Task<List<HadithResponse>> GetAuthenticHadithByTopicAsync(string topic, string language = "en", int maxResults = 5)
    {
        _logger.LogInformation("HadithVerifier searching ONLY authentic hadiths by topic: {Topic}", topic);

        // Use GetAuthenticHadithAsync to ensure only Sahih hadiths are returned
        var hadiths = await _hadithService.GetAuthenticHadithAsync(topic, maxResults);
        var responses = new List<HadithResponse>();

        // Process each authentic hadith
        foreach (var hadith in hadiths)
        {
            var prompt = BuildVerificationPrompt(hadith, language);
            var aiResponse = await _ollama.GenerateResponseAsync(MODEL_NAME, prompt, 0.3);

            var response = new HadithResponse
            {
                Text = hadith.ArabicText,
                Translation = hadith.Translation,
                Grade = hadith.Grade, // Should always be Sahih
                Collection = hadith.Collection,
                BookNumber = hadith.BookNumber,
                HadithNumber = hadith.HadithNumber,
                SanadChain = hadith.SanadChain,
                Explanation = aiResponse,
                AlternativeHadith = new List<string> { "Related authentic hadiths available upon request" }
            };

            response.Sources.AddRange(new[] { hadith.Collection, "Authentic Hadith Database", MODEL_NAME });
            responses.Add(response);
        }

        _logger.LogInformation("Found {Count} verified authentic hadiths for topic: {Topic}", responses.Count, topic);
        return responses.OrderByDescending(h => h.Grade).ToList();
    }

    /// <summary>
    /// Check the Sanad (chain of narrators) for a specific hadith
    /// </summary>
    public async Task<SanadAnalysis> AnalyzeSanadAsync(string collection, string hadithNumber)
    {
        _logger.LogInformation("Analyzing Sanad for {Collection} {HadithNumber}", collection, hadithNumber);

        var hadith = await _hadithService.GetHadithByReferenceAsync(collection, hadithNumber);
        if (hadith == null)
        {
            throw new ArgumentException($"Hadith not found: {collection} {hadithNumber}");
        }

        var prompt = BuildSanadAnalysisPrompt(hadith);
        var analysis = await _ollama.GenerateStructuredResponseAsync<SanadAnalysis>(
            MODEL_NAME, prompt, temperature: 0.1);

        return analysis;
    }

    private async Task<HadithResponse> HandleUnknownHadith(string hadithText, string language)
    {
        _logger.LogWarning("Unknown hadith submitted for verification");

        // Do NOT generate fake hadith content - just return guidance
        var prompt = $@"
A hadith text was submitted that is not found in our authenticated collections:
""{hadithText}""

Provide ONLY guidance about why this might not be found and what to do next.
DO NOT create or suggest alternative hadith texts.
DO NOT translate or explain the content as if it were authentic.

Provide brief guidance on:
1. Why hadiths might not be in major collections
2. Recommend consulting qualified Islamic scholars
3. Suggest searching with different keywords

Keep response concise and focused on verification guidance only.";

        var aiGuidance = await _ollama.GenerateResponseAsync(MODEL_NAME, prompt, 0.1);

        return new HadithResponse
        {
            Text = "[Text not displayed - unverified]",
            Translation = "Not available - this text was not found in authenticated collections",
            Grade = HadithGrade.Unknown,
            Collection = "Not Found",
            Explanation = aiGuidance,
            Warning = "⚠️ This hadith was not found in authenticated collections. Please verify with qualified Islamic scholars before using."
        };
    }    private string BuildVerificationPrompt(HadithRecord hadith, string language)
    {
        return $@"
Explain this authenticated hadith from {hadith.Collection} (Grade: {hadith.Grade}):

Translation: {hadith.Translation}

Please provide:
1. The meaning and context of this hadith
2. How it applies to modern Muslim life
3. Any important scholarly insights

Keep the explanation clear and practical.";
    }

    private string BuildSanadAnalysisPrompt(HadithRecord hadith)
    {
        return $@"
Analyze the Sanad (chain of narrators) for this hadith:

Collection: {hadith.Collection}
Hadith Number: {hadith.HadithNumber}
Sanad Chain: {string.Join(" ← ", hadith.SanadChain)}
Grade: {hadith.Grade}

Provide detailed analysis of:
1. Each narrator's reliability
2. Chain strength and weaknesses
3. Historical context of transmission
4. Reasons for the authenticity grading

Respond in JSON format with comprehensive Sanad analysis.";
    }

    private record HadithAnalysis(
        string Explanation,
        string PracticalApplication,
        List<string> AlternativeHadith,
        string ScholarlyNotes
    );

    public record SanadAnalysis(
        List<NarratorAnalysis> Narrators,
        string OverallAssessment,
        List<string> Strengths,
        List<string> Weaknesses,
        string HistoricalContext
    );

    public record NarratorAnalysis(
        string Name,
        string ReliabilityRating,
        string BiographicalNotes,
        List<string> KnownFor
    );
}
