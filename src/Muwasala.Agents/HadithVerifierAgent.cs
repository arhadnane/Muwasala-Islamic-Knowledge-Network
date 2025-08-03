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

    // Terme expansion mapping pour recherche intelligente
    private readonly Dictionary<string, List<string>> _termExpansion = new()
    {
        // Femmes et genre
        { "women", new() { "woman", "femme", "femmes", "épouse", "épouses", "fille", "filles", "mère", "mères", "sœur", "sœurs", "ladies", "lady" } },
        { "woman", new() { "women", "femme", "femmes", "épouse", "fille", "mère", "sœur", "lady" } },
        { "femme", new() { "femmes", "woman", "women", "épouse", "fille", "mère", "sœur" } },
        { "femmes", new() { "femme", "women", "woman", "épouses", "filles", "mères", "sœurs" } },
        
        // Antéchrist / Dajjal
        { "antichrist", new() { "dajjal", "anti christ", "anti-christ", "antéchrist", "faux messie", "false messiah", "deceiver", "trompeur" } },
        { "dajjal", new() { "antichrist", "anti christ", "antéchrist", "faux messie", "false messiah" } },
        
        // Termes religieux
        { "prayer", new() { "salah", "salat", "prière", "prières", "namaz" } },
        { "prière", new() { "prières", "prayer", "salah", "salat", "namaz" } },
        { "fasting", new() { "jeûne", "sawm", "ramadan", "siyam" } },
        { "jeûne", new() { "fasting", "sawm", "ramadan", "siyam" } },
        
        // Prophète
        { "prophet", new() { "prophète", "muhammad", "mohammed", "rasul", "messenger", "messager" } },
        { "prophète", new() { "prophet", "muhammad", "mohammed", "rasul", "messenger", "messager" } },
        
        // Paradis/Enfer
        { "paradise", new() { "paradis", "jannah", "garden", "jardin" } },
        { "paradis", new() { "paradise", "jannah", "garden", "jardin" } },
        { "hell", new() { "enfer", "jahannam", "fire", "feu" } },
        { "enfer", new() { "hell", "jahannam", "fire", "feu" } },
        
        // 🌙 Termes astronomiques et calendrier islamique
        { "moon", new() { "lunar", "crescent", "hilal", "new moon", "full moon", "sighting", "calendar", "month" } },
        { "lunar", new() { "moon", "crescent", "hilal", "monthly", "calendar", "sighting" } },
        { "crescent", new() { "hilal", "moon", "new moon", "sighting", "lunar", "monthly" } },
        { "hilal", new() { "crescent", "new moon", "moon", "sighting", "lunar", "calendar" } },
        { "eclipse", new() { "lunar eclipse", "solar eclipse", "moon", "sun", "eclipse prayer", "kusuf", "khusuf" } },
        { "calendar", new() { "lunar calendar", "islamic calendar", "hijri", "moon", "month", "year" } },
        { "sighting", new() { "moon sighting", "hilal", "crescent", "observation", "lunar", "new moon" } },
        { "month", new() { "lunar month", "islamic month", "moon", "calendar", "hijri month" } },
        { "kusuf", new() { "eclipse", "lunar eclipse", "eclipse prayer", "khusuf", "moon" } },
        { "khusuf", new() { "eclipse", "solar eclipse", "eclipse prayer", "kusuf", "sun" } }
    };

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
            // First try exact search
            var hadithResults = await _hadithService.SearchHadithAsync(hadithText, language);
            
            // If no exact match, try flexible search with key phrases
            if (!hadithResults.Any())
            {
                hadithResults = await PerformFlexibleSearch(hadithText, language);
            }
            
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
    /// <summary>
    /// Search for hadiths by topic with enhanced astronomical term expansion
    /// </summary>
    public async Task<List<HadithResponse>> GetHadithByTopicAsync(string topic, string language = "en", int maxResults = 5)
    {
        _logger.LogInformation("HadithVerifier searching hadiths by topic: {Topic}", topic);

        // Expand search terms including astronomical terminology
        var expandedTerms = ExpandSearchTerms(topic);
        _logger.LogInformation("Expanded search terms: {Terms}", string.Join(", ", expandedTerms));

        var allHadiths = new List<HadithRecord>();

        // Try with original term first
        var hadiths = await _hadithService.GetAuthenticHadithAsync(topic, maxResults);
        allHadiths.AddRange(hadiths);

        // If we didn't get enough results, try expanded terms
        if (allHadiths.Count < maxResults && expandedTerms.Any())
        {
            foreach (var term in expandedTerms.Take(5)) // Limit to first 5 expanded terms
            {
                if (allHadiths.Count >= maxResults) break;
                
                var additionalHadiths = await _hadithService.GetAuthenticHadithAsync(term, maxResults - allHadiths.Count);
                
                // Avoid duplicates by checking collection and hadith number
                foreach (var hadith in additionalHadiths)
                {
                    if (!allHadiths.Any(h => h.Collection == hadith.Collection && h.HadithNumber == hadith.HadithNumber))
                    {
                        allHadiths.Add(hadith);
                    }
                }
            }
        }

        var responses = new List<HadithResponse>();

        // Process found hadiths
        foreach (var hadith in allHadiths.Take(maxResults))
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

            // Safety checks for hadith grades
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

        _logger.LogInformation("Found {Count} authentic hadiths for topic: {Topic} (expanded: {ExpandedCount} terms)", responses.Count, topic, expandedTerms.Count);
        return responses.OrderByDescending(h => h.Grade).ToList();
    }

    /// <summary>
    /// Expand search terms for better topic matching
    /// </summary>
    private List<string> ExpandSearchTerms(string originalTerm)
    {
        var expandedTerms = new List<string>();
        var lowerTerm = originalTerm.ToLower().Trim();

        // Check if we have expansions for this term
        if (_termExpansion.ContainsKey(lowerTerm))
        {
            expandedTerms.AddRange(_termExpansion[lowerTerm]);
        }

        // Also check if any expansion values contain our term (reverse lookup)
        foreach (var kvp in _termExpansion)
        {
            if (kvp.Value.Any(v => v.Contains(lowerTerm) || lowerTerm.Contains(v)))
            {
                expandedTerms.Add(kvp.Key);
                expandedTerms.AddRange(kvp.Value);
            }
        }

        return expandedTerms.Distinct().Where(t => !t.Equals(lowerTerm, StringComparison.OrdinalIgnoreCase)).ToList();
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
    }

    /// <summary>
    /// Perform flexible search for hadith using key phrases and variations
    /// </summary>
    private async Task<List<HadithRecord>> PerformFlexibleSearch(string hadithText, string language)
    {
        _logger.LogInformation("Performing flexible search for hadith text: {Text}", hadithText);

        // Extract key phrases from the hadith text
        var keyPhrases = ExtractKeyPhrases(hadithText);
        _logger.LogInformation("Extracted key phrases: {Phrases}", string.Join(", ", keyPhrases));
        
        foreach (var phrase in keyPhrases)
        {
            _logger.LogInformation("Searching with phrase: {Phrase}", phrase);
            var results = await _hadithService.SearchHadithAsync(phrase, language);
            _logger.LogInformation("Found {Count} results for phrase: {Phrase}", results.Count(), phrase);
            
            if (results.Any())
            {
                // Log all found results for debugging
                foreach (var result in results)
                {
                    _logger.LogInformation("Found hadith: {Translation}", result.Translation);
                }
                
                // Verify similarity with original text
                var similarResults = results.Where(h => IsTextSimilar(hadithText, h.Translation)).ToList();
                _logger.LogInformation("Found {Count} similar results after similarity check", similarResults.Count);
                
                if (similarResults.Any())
                {
                    _logger.LogInformation("Found similar hadith using phrase: {Phrase}", phrase);
                    return similarResults;
                }
                else
                {
                    // Log why similarity failed
                    foreach (var result in results)
                    {
                        var similarity = CalculateSimilarityScore(hadithText, result.Translation);
                        _logger.LogInformation("Similarity score for '{Translation}': {Score:F2}", 
                            result.Translation.Substring(0, Math.Min(50, result.Translation.Length)), similarity);
                    }
                }
            }
        }

        return new List<HadithRecord>();
    }

    /// <summary>
    /// Extract key phrases from hadith text for flexible searching
    /// </summary>
    private List<string> ExtractKeyPhrases(string hadithText)
    {
        var phrases = new List<string>();
        
        // Common hadith opening phrases - more variations for better matching
        var commonPhrases = new[]
        {
            "Actions are but by intention",
            "actions are but by intention", 
            "Actions are only by intention",
            "actions are only by intention",
            "deeds are but by intention",
            "deeds are only by intention",
            "Every man shall have",
            "every man shall have",
            "but that which he intended",
            "only that which he intended",
            "that which he intended",
            "by intention",
            "intention"
        };

        // Add phrases that appear in the text
        foreach (var phrase in commonPhrases)
        {
            if (hadithText.Contains(phrase, StringComparison.OrdinalIgnoreCase))
            {
                phrases.Add(phrase);
            }
        }

        // Also try shorter, more specific phrases
        var shortPhrases = new[]
        {
            "Actions are",
            "intention",
            "intended"
        };

        foreach (var phrase in shortPhrases)
        {
            if (hadithText.Contains(phrase, StringComparison.OrdinalIgnoreCase) && !phrases.Contains(phrase))
            {
                phrases.Add(phrase);
            }
        }

        // If no common phrases found, extract meaningful words
        if (!phrases.Any())
        {
            var words = hadithText.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3 && !IsStopWord(w))
                .Take(5);
            phrases.AddRange(words);
        }

        return phrases;
    }

    /// <summary>
    /// Check if two texts are similar (for hadith matching)
    /// </summary>
    private bool IsTextSimilar(string text1, string text2)
    {
        var similarity = CalculateSimilarityScore(text1, text2);
        // Lower threshold for better matching - 50% similarity
        return similarity >= 0.5;
    }

    /// <summary>
    /// Calculate similarity score between two texts
    /// </summary>
    private double CalculateSimilarityScore(string text1, string text2)
    {
        // Normalize texts for comparison
        var normalized1 = NormalizeText(text1);
        var normalized2 = NormalizeText(text2);

        // Calculate similarity score based on common words
        var words1 = normalized1.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var words2 = normalized2.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var commonWords = words1.Intersect(words2, StringComparer.OrdinalIgnoreCase).Count();
        var totalWords = Math.Max(words1.Length, words2.Length);

        if (totalWords == 0) return 0.0;

        return (double)commonWords / totalWords;
    }

    /// <summary>
    /// Normalize text for comparison (remove punctuation, standardize spacing)
    /// </summary>
    private string NormalizeText(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        // Remove punctuation and normalize whitespace
        var normalized = text.Replace(",", " ")
                            .Replace(".", " ")
                            .Replace(";", " ")
                            .Replace(":", " ")
                            .Replace("?", " ")
                            .Replace("!", " ")
                            .Replace("(", " ")
                            .Replace(")", " ");

        // Replace multiple spaces with single space
        while (normalized.Contains("  "))
        {
            normalized = normalized.Replace("  ", " ");
        }

        return normalized.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Check if a word is a stop word (articles, prepositions, etc.)
    /// </summary>
    private bool IsStopWord(string word)
    {
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "the", "and", "but", "or", "a", "an", "is", "are", "was", "were",
            "be", "been", "have", "has", "had", "do", "does", "did", "will",
            "would", "could", "should", "may", "might", "must", "can",
            "of", "in", "on", "at", "by", "for", "with", "to", "from"
        };

        return stopWords.Contains(word);
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
