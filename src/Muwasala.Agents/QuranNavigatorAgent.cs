using Microsoft.Extensions.Logging;
using Muwasala.Core.Models;
using Muwasala.Core.Services;
using Muwasala.KnowledgeBase.Services;
using System.Security.Cryptography;
using System.Text;

namespace Muwasala.Agents;

/// <summary>
/// Qur'an Navigator Agent - Contextual verse retrieval & thematic analysis
/// Uses DeepSeek-Coder 1.3B model for accurate Quranic guidance
/// </summary>
public class QuranNavigatorAgent
{
    private readonly IOllamaService _ollama;
    private readonly IQuranService _quranService;
    private readonly ICacheService _cache;
    private readonly ILogger<QuranNavigatorAgent> _logger;
    private const string MODEL_NAME = "phi3:mini";

    public QuranNavigatorAgent(
        IOllamaService ollama, 
        IQuranService quranService,
        ICacheService cache,
        ILogger<QuranNavigatorAgent> logger)
    {
        _ollama = ollama;
        _quranService = quranService;
        _cache = cache;
        _logger = logger;
    }    /// <summary>
    /// Get contextually relevant Quranic verses based on user's situation
    /// </summary>
    public async Task<VerseResponse> GetVerseAsync(
        string context, 
        string language = "en", 
        string tafsir = "IbnKathir")
    {
        _logger.LogInformation("QuranNavigator processing context: {Context}", context);

        try
        {
            // Check cache first
            var cacheKey = GenerateCacheKey(context, language, tafsir);
            var cachedResponse = await _cache.GetAsync<VerseResponse>(cacheKey);
            if (cachedResponse != null)
            {
                _logger.LogInformation("Returning cached response for context: {Context}", context);
                return cachedResponse;
            }

            // Check if this is a specific verse request (format: "Verse X:Y")
            var specificVerse = ParseSpecificVerseRequest(context);
            List<QuranVerse> candidateVerses;
            
            if (specificVerse != null)
            {
                // Get the specific verse
                var verse = await _quranService.GetVerseAsync(specificVerse, language);
                if (verse == null)
                {
                    _logger.LogWarning("Specific verse not found: {Context}", context);
                    throw new InvalidOperationException($"Verse not found: {context}");
                }
                candidateVerses = new List<QuranVerse> { verse };
            }
            else
            {
                // First, search knowledge base for relevant verses
                candidateVerses = await _quranService.SearchVersesByContextAsync(context, language);
                
                if (!candidateVerses.Any())
                {
                    _logger.LogWarning("No verses found for context: {Context}", context);
                    throw new InvalidOperationException($"No relevant verses found for context: {context}");
                }
            }            // Use AI to select the most relevant verse and provide analysis
            var prompt = BuildContextualPrompt(context, candidateVerses, language, tafsir);
            var aiText = await _ollama.GenerateResponseAsync(
                MODEL_NAME, prompt, temperature: 0.3);

            // Parse the text response to extract key information
            var aiResponse = ParseTextResponse(aiText, candidateVerses);// Get the selected verse details - fallback to first candidate if AI suggestion not found
            var selectedVerse = candidateVerses.FirstOrDefault(v => 
                v.Surah == aiResponse.SelectedVerse.Surah && 
                v.Verse == aiResponse.SelectedVerse.Verse) ?? candidateVerses.First();
            
            // Update the response with the actual selected verse reference
            if (selectedVerse.Surah != aiResponse.SelectedVerse.Surah || selectedVerse.Verse != aiResponse.SelectedVerse.Verse)
            {
                aiResponse = aiResponse with { SelectedVerse = new VerseReference(selectedVerse.Surah, selectedVerse.Verse) };
            }

            // Build comprehensive response
            var response = new VerseResponse
            {
                Verse = aiResponse.SelectedVerse,
                ArabicText = selectedVerse.ArabicText,
                Translation = selectedVerse.Translation,
                Transliteration = selectedVerse.Transliteration,
                Tafsir = aiResponse.TafsirExplanation,
                TafsirSource = tafsir,
                Context = context,
                RelatedVerses = aiResponse.RelatedVerses,
                RelatedDuas = aiResponse.RelatedDuas
            };

            response.Sources.AddRange(new[] { "Quran", tafsir, MODEL_NAME });

            _logger.LogInformation("QuranNavigator found verse: {Verse}", response.Verse);
            
            // Cache the response for 1 hour
            await _cache.SetAsync(cacheKey, response, TimeSpan.FromHours(1));
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in QuranNavigator for context: {Context}", context);
            throw;
        }
    }    /// <summary>
    /// Get thematic analysis of multiple verses on a topic
    /// </summary>
    public async Task<List<VerseResponse>> GetThematicAnalysisAsync(
        string theme, 
        string language = "en",
        int maxVerses = 5)
    {
        _logger.LogInformation("QuranNavigator performing thematic analysis: {Theme}", theme);

        var verses = await _quranService.SearchVersesByThemeAsync(theme, language, maxVerses);
        var responses = new List<VerseResponse>();

        foreach (var verse in verses)
        {
            // Create a specific response for each verse found
            var response = new VerseResponse
            {
                Verse = new VerseReference(verse.Surah, verse.Verse),
                ArabicText = verse.ArabicText,
                Translation = verse.Translation,
                Transliteration = verse.Transliteration,
                Tafsir = $"This verse is relevant to the theme '{theme}' and provides guidance on this topic.",
                TafsirSource = "Thematic Analysis",
                Context = $"Thematic study: {theme}",
                RelatedVerses = new List<VerseReference>(),
                RelatedDuas = new List<string>()
            };
            
            response.Sources.AddRange(new[] { "Quran", "Thematic Analysis" });
            responses.Add(response);
        }

        return responses;
    }

    /// <summary>
    /// Get all verses from a specific Surah
    /// </summary>
    public async Task<List<VerseResponse>> GetSurahAnalysisAsync(
        int surahNumber, 
        string language = "en")
    {
        _logger.LogInformation("QuranNavigator getting Surah analysis: {SurahNumber}", surahNumber);

        var verses = await _quranService.GetSurahAsync(surahNumber, language);
        var responses = new List<VerseResponse>();

        foreach (var verse in verses)
        {
            var response = new VerseResponse
            {
                Verse = new VerseReference(verse.Surah, verse.Verse),
                ArabicText = verse.ArabicText,
                Translation = verse.Translation,
                Transliteration = verse.Transliteration,
                Tafsir = $"This is verse {verse.Verse} from Surah {verse.Surah}.",
                TafsirSource = "Surah Analysis",
                Context = $"Surah {surahNumber}",
                RelatedVerses = new List<VerseReference>(),
                RelatedDuas = new List<string>()
            };
            
            response.Sources.AddRange(new[] { "Quran", "Surah Analysis" });
            responses.Add(response);
        }

        return responses;
    }private string BuildContextualPrompt(
        string context, 
        List<QuranVerse> candidateVerses, 
        string language, 
        string tafsir)
    {
        var versesText = string.Join("\n", candidateVerses.Take(3).Select(v => 
            $"{v.Surah}:{v.Verse} - {v.Translation.Substring(0, Math.Min(v.Translation.Length, 100))}..."));

        return $@"Context: {context}

Verses:
{versesText}

Select best verse and provide brief {tafsir} explanation. JSON format:
{{
    ""selectedVerse"": {{ ""surah"": {candidateVerses[0].Surah}, ""verse"": {candidateVerses[0].Verse} }},
    ""tafsirExplanation"": ""brief explanation"",
    ""relatedVerses"": [{{ ""surah"": 1, ""verse"": 1 }}],
    ""relatedDuas"": [""Dua for Guidance""]
}}";
    }

    private string GenerateCacheKey(string context, string language, string tafsir)
    {
        var input = $"quran:{context}:{language}:{tafsir}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));        return Convert.ToHexString(hash)[..16]; // Use first 16 characters
    }    /// <summary>
    /// Parse text response from AI to extract structured information
    /// </summary>
    private VerseAnalysisResponse ParseTextResponse(string aiText, List<QuranVerse> candidateVerses)
    {
        // Default to first verse if parsing fails
        var selectedVerse = new VerseReference(candidateVerses.First().Surah, candidateVerses.First().Verse);
        var tafsirExplanation = aiText;
        var relatedVerses = new List<VerseReference>();
        var relatedDuas = new List<string>();
        
        try
        {
            // Try to extract verse reference (format: "2:286" or "Surah 2, Verse 286")
            var verseMatch = System.Text.RegularExpressions.Regex.Match(aiText, @"(?:Surah\s+)?(\d+)(?:[,:]\s*(?:Verse\s+)?(\d+))?");
            if (verseMatch.Success && int.TryParse(verseMatch.Groups[1].Value, out int surah))
            {
                var verse = 1; // default
                if (verseMatch.Groups[2].Success && int.TryParse(verseMatch.Groups[2].Value, out verse))
                {
                    // Check if this verse is in our candidates
                    var foundVerse = candidateVerses.FirstOrDefault(v => v.Surah == surah && v.Verse == verse);
                    if (foundVerse != null)
                    {
                        selectedVerse = new VerseReference(surah, verse);
                    }
                }
            }

            // Extract tafsir explanation - look for patterns that indicate explanation
            var lines = aiText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var explanationLines = lines.Where(line => 
                !line.Contains("Surah") || 
                !line.Contains("Verse") ||
                line.Length > 50 // Longer lines likely contain explanation
            ).ToList();
            
            if (explanationLines.Any())
            {
                tafsirExplanation = string.Join(" ", explanationLines).Trim();
            }
        }
        catch (Exception)
        {
            // If parsing fails, use defaults
        }

        return new VerseAnalysisResponse(
            selectedVerse,
            tafsirExplanation,
            relatedVerses,
            relatedDuas,
            "AI analysis of contextual relevance"
        );
    }

    /// <summary>
    /// Parse a specific verse request in the format "Verse X:Y" or "X:Y"
    /// </summary>
    private VerseReference? ParseSpecificVerseRequest(string context)
    {
        if (string.IsNullOrWhiteSpace(context))
            return null;

        // Try to match "Verse X:Y" pattern
        var versePattern = @"^Verse\s+(\d+):(\d+)$";
        var match = System.Text.RegularExpressions.Regex.Match(context.Trim(), versePattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        if (match.Success)
        {
            if (int.TryParse(match.Groups[1].Value, out int surah) && 
                int.TryParse(match.Groups[2].Value, out int verse))
            {
                // Validate surah and verse numbers
                if (surah >= 1 && surah <= 114 && verse >= 1)
                {
                    return new VerseReference(surah, verse);
                }
            }
        }

        // Try to match simple "X:Y" pattern
        var simplePattern = @"^(\d+):(\d+)$";
        match = System.Text.RegularExpressions.Regex.Match(context.Trim(), simplePattern);
        
        if (match.Success)
        {
            if (int.TryParse(match.Groups[1].Value, out int surah) && 
                int.TryParse(match.Groups[2].Value, out int verse))
            {
                // Validate surah and verse numbers
                if (surah >= 1 && surah <= 114 && verse >= 1)
                {
                    return new VerseReference(surah, verse);
                }
            }
        }

        return null;
    }

    private record VerseAnalysisResponse(
        VerseReference SelectedVerse,
        string TafsirExplanation,
        List<VerseReference> RelatedVerses,
        List<string> RelatedDuas,
        string ContextualRelevance
    );
}
