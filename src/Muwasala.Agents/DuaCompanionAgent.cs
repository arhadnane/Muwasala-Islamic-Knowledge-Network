using Microsoft.Extensions.Logging;
using Muwasala.Core.Models;
using Muwasala.Core.Services;
using Muwasala.KnowledgeBase.Services;

namespace Muwasala.Agents;

/// <summary>
/// Du'a Companion Agent - Prayer recommendation engine
/// Uses Mistral 7B for contextual Islamic prayer suggestions
/// </summary>
public class DuaCompanionAgent
{
    private readonly IOllamaService _ollama;
    private readonly IDuaService _duaService;
    private readonly ILogger<DuaCompanionAgent> _logger;
    private const string MODEL_NAME = "deepseek-r1";

    public DuaCompanionAgent(
        IOllamaService ollama,
        IDuaService duaService,
        ILogger<DuaCompanionAgent> logger)
    {
        _ollama = ollama;
        _duaService = duaService;
        _logger = logger;
    }

    /// <summary>
    /// Get appropriate du'as for a specific occasion or need
    /// </summary>
    public async Task<List<DuaResponse>> GetDuasForOccasionAsync(
        string occasion, 
        string language = "en", 
        int maxResults = 3,
        bool fastMode = false) // 🚀 Mode rapide ajouté
    {
        _logger.LogInformation("DuaCompanion finding duas for occasion: {Occasion} (FastMode: {FastMode})", occasion, fastMode);

        try
        {
            // Search authenticated du'a database
            var candidateDuas = await _duaService.SearchDuasByOccasionAsync(occasion, language, maxResults);
            
            if (!candidateDuas.Any())
            {
                return await GetAlternativeDuasAsync(occasion, language, fastMode);
            }

            // 🚀 MODE RAPIDE: Skip AI enhancement pour des réponses instantanées
            if (fastMode)
            {
                return candidateDuas.Select(dua => new DuaResponse
                {
                    ArabicText = dua.ArabicText,
                    Translation = dua.Translation,
                    Transliteration = dua.Transliteration,
                    Occasion = occasion,
                    Source = dua.Source,
                    Benefits = dua.Benefits ?? "Authentic Islamic supplication with spiritual benefits",
                    RelatedDuas = new List<string>()
                }).ToList();
            }

            // 🚀 OPTIMISATION: Parallélisation des appels AI
            var enhancementTasks = candidateDuas.Select(async dua =>
            {
                try
                {
                    var prompt = BuildDuaExplanationPrompt(dua, occasion, language);
                    var aiEnhancement = await _ollama.GenerateStructuredResponseAsync<DuaEnhancement>(
                        MODEL_NAME, prompt, temperature: 0.1);
                    
                    return new { dua, aiEnhancement, success = true };
                }
                catch (Exception ex)
                {
                    string errorType = ex is TaskCanceledException || ex is TimeoutException || ex.Message.Contains("timeout") 
                        ? "timeout" : "error";
                    _logger.LogWarning(ex, "Failed to get AI enhancement for dua due to {ErrorType}: {DuaText}", errorType, dua.ArabicText);
                    
                    // Fallback avec benefits de base
                    return new { 
                        dua, 
                        aiEnhancement = new DuaEnhancement(
                            errorType == "timeout" 
                                ? "AI enhancement timed out - this is a blessed authentic supplication" 
                                : "Recitation brings spiritual benefits", 
                            "Anytime", 
                            new List<string>(), 
                            "Authentic Islamic supplication"),
                        success = false 
                    };
                }
            }).ToArray();

            var enhancementResults = await Task.WhenAll(enhancementTasks);
            
            var responses = enhancementResults.Select(result =>
            {
                var response = new DuaResponse
                {
                    ArabicText = result.dua.ArabicText,
                    Translation = result.dua.Translation,
                    Transliteration = result.dua.Transliteration,
                    Occasion = occasion,
                    Source = result.dua.Source,
                    Benefits = result.aiEnhancement.Benefits,
                    RelatedDuas = result.aiEnhancement.RelatedDuas
                };

                response.Sources.AddRange(new[] { result.dua.Source, MODEL_NAME });
                return response;
            }).ToList();

            _logger.LogInformation("DuaCompanion found {Count} duas for {Occasion}", responses.Count, occasion);
            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DuaCompanion for occasion: {Occasion}", occasion);
            
            // Si c'est un timeout et que ce n'est pas déjà en mode rapide, essayons en mode rapide
            if ((ex is TaskCanceledException || ex is TimeoutException || ex.Message.Contains("timeout")) && !fastMode)
            {
                _logger.LogWarning("Timeout detected, falling back to fast mode for occasion: {Occasion}", occasion);
                return await GetDuasForOccasionAsync(occasion, language, maxResults, fastMode: true);
            }
            
            throw;
        }
    }

    /// <summary>
    /// Get daily du'as for different times of day
    /// </summary>
    public async Task<DailyDuaSchedule> GetDailyDuaScheduleAsync(string language = "en")
    {
        _logger.LogInformation("Generating daily dua schedule");

        var schedule = new DailyDuaSchedule();

        // Morning duas
        schedule.MorningDuas = await GetDuasForOccasionAsync("morning", language, 2);
        
        // Evening duas  
        schedule.EveningDuas = await GetDuasForOccasionAsync("evening", language, 2);
        
        // Before eating
        schedule.MealDuas = await GetDuasForOccasionAsync("before eating", language, 1);
        
        // Before sleeping
        schedule.SleepDuas = await GetDuasForOccasionAsync("before sleep", language, 2);
        
        // Travel duas
        schedule.TravelDuas = await GetDuasForOccasionAsync("travel", language, 2);

        return schedule;
    }

    /// <summary>
    /// Get personalized du'a recommendations based on user's situation
    /// </summary>
    public async Task<List<DuaResponse>> GetPersonalizedDuasAsync(
        PersonalContext context, 
        string language = "en")
    {
        _logger.LogInformation("Getting personalized duas for user context");

        var contextDescription = BuildContextDescription(context);
        var situations = AnalyzeSituations(context);
        
        var allDuas = new List<DuaResponse>();

        foreach (var situation in situations)
        {
            var duas = await GetDuasForOccasionAsync(situation, language, 2);
            allDuas.AddRange(duas);
        }

        // Remove duplicates and prioritize by relevance
        var uniqueDuas = allDuas
            .GroupBy(d => d.ArabicText)
            .Select(g => g.First())
            .Take(5)
            .ToList();

        return uniqueDuas;
    }

    /// <summary>
    /// Get du'a for specific prayers (Istighfar, Istikhara, etc.)
    /// </summary>
    public async Task<DuaResponse> GetSpecificPrayerAsync(
        SpecificPrayer prayerType, 
        string language = "en")
    {
        _logger.LogInformation("Getting specific prayer: {PrayerType}", prayerType);

        var dua = await _duaService.GetSpecificPrayerAsync(prayerType, language);
        if (dua == null)
        {
            throw new ArgumentException($"Prayer type {prayerType} not found");
        }

        var prompt = BuildSpecificPrayerPrompt(dua, prayerType, language);
        var enhancement = await _ollama.GenerateStructuredResponseAsync<DuaEnhancement>(
            MODEL_NAME, prompt, temperature: 0.1);

        var response = new DuaResponse
        {
            ArabicText = dua.ArabicText,
            Translation = dua.Translation,
            Transliteration = dua.Transliteration,
            Occasion = prayerType.ToString(),
            Source = dua.Source,
            Benefits = enhancement.Benefits,
            RelatedDuas = enhancement.RelatedDuas
        };

        response.Sources.AddRange(new[] { dua.Source, MODEL_NAME });
        return response;
    }

    private async Task<List<DuaResponse>> GetAlternativeDuasAsync(string occasion, string language, bool fastMode = false)
    {
        _logger.LogInformation("Searching for alternative duas for: {Occasion} (fastMode: {FastMode})", occasion, fastMode);

        string suggestions;
        
        if (fastMode)
        {
            // Mode rapide : pas d'appel IA
            suggestions = language == "ar" 
                ? "دعاء عام مناسب للمناسبة المطلوبة. يُنصح بالرجوع إلى المصادر الإسلامية للحصول على أدعية محددة."
                : "General supplication suitable for the requested occasion. Please consult Islamic resources for specific duas.";
        }
        else
        {
            // Mode amélioré : avec IA
            var prompt = $@"
A Muslim is seeking du'as for this situation: ""{occasion}""

Suggest appropriate authentic du'as from Quran and Sunnah that would be suitable.
Include:
1. General du'as that apply to this situation
2. Specific verses from Quran that can be used as du'a
3. Authentic du'as from Hadith collections

Language: {language}

Respond with suggestions for finding appropriate authentic du'as.";

            suggestions = await _ollama.GenerateResponseAsync(MODEL_NAME, prompt, 0.1);
        }

        // Return a single guidance response
        return new List<DuaResponse>
        {
            new DuaResponse
            {
                ArabicText = "رَبَّنَا آتِنَا فِي الدُّنْيَا حَسَنَةً وَفِي الْآخِرَةِ حَسَنَةً وَقِنَا عَذَابَ النَّارِ",
                Translation = "Our Lord, give us good in this world and good in the hereafter, and protect us from the punishment of the Fire.",
                Transliteration = "Rabbana atina fi'd-dunya hasanatan wa fi'l-akhirati hasanatan wa qina 'adhab an-nar",
                Occasion = occasion,
                Source = "Quran 2:201",
                Benefits = suggestions,
                Warning = "This is a general du'a. Consult Islamic resources for specific duas for this occasion."
            }
        };
    }

    private string BuildDuaExplanationPrompt(DuaRecord dua, string occasion, string language)
    {
        var languageInstruction = language switch
        {
            "ar" => "Please respond in Arabic language (العربية). Use Islamic terminology in Arabic and provide explanations in Arabic.",
            "en" => "Please respond in English language. Use clear English explanations.",
            _ => "Please respond in English language. Use clear English explanations."
        };

        return $@"You are an Islamic scholar providing du'a guidance.

{languageInstruction}

Explain this authentic Islamic du'a:

Arabic: {dua.ArabicText}
Translation: {dua.Translation}
Source: {dua.Source}
Occasion: {occasion}

Provide:
1. Spiritual benefits of reciting this du'a
2. Best times and ways to recite it
3. Related du'as on similar themes
4. Historical context or Prophet's guidance about it

Respond in JSON format:
{{
    ""benefits"": ""spiritual and practical benefits in the requested language"",
    ""bestTimes"": ""when to recite this du'a"",
    ""relatedDuas"": [""names of related duas""],
    ""historicalContext"": ""background information""
}}";
    }

    private string BuildSpecificPrayerPrompt(DuaRecord dua, SpecificPrayer prayerType, string language)
    {
        var languageInstruction = language switch
        {
            "ar" => "Please respond in Arabic language (العربية). Use Islamic terminology in Arabic and provide explanations in Arabic.",
            "en" => "Please respond in English language. Use clear English explanations.",
            _ => "Please respond in English language. Use clear English explanations."
        };

        return $@"You are an Islamic scholar providing prayer guidance.

{languageInstruction}

Provide comprehensive guidance for this specific Islamic prayer:

Prayer Type: {prayerType}
Arabic: {dua.ArabicText}
Translation: {dua.Translation}
Source: {dua.Source}

Explain:
1. When and how to perform this prayer
2. Specific etiquettes and requirements
3. What to expect and how Allah responds
4. Related supplications

Respond in JSON format with detailed guidance in the requested language.";
    }

    private string BuildContextDescription(PersonalContext context)
    {
        var descriptions = new List<string>();
        
        if (context.IsRamadan) descriptions.Add("during Ramadan");
        if (context.IsTraveling) descriptions.Add("while traveling");
        if (context.HasDifficulty) descriptions.Add("facing difficulties");
        if (context.IsStudying) descriptions.Add("while studying");
        if (context.IsWorking) descriptions.Add("during work");
        
        return string.Join(", ", descriptions);
    }

    private List<string> AnalyzeSituations(PersonalContext context)
    {
        var situations = new List<string>();
        
        if (context.IsRamadan) situations.AddRange(new[] { "ramadan", "fasting", "iftar" });
        if (context.IsTraveling) situations.AddRange(new[] { "travel", "journey safety" });
        if (context.HasDifficulty) situations.AddRange(new[] { "hardship", "patience", "relief" });
        if (context.IsStudying) situations.AddRange(new[] { "knowledge", "learning", "memory" });
        if (context.IsWorking) situations.AddRange(new[] { "work", "livelihood", "success" });
        
        // Always include general categories
        situations.AddRange(new[] { "morning", "evening", "gratitude" });
        
        return situations.Distinct().ToList();
    }

    // Supporting records and enums
    public record PersonalContext(
        bool IsRamadan = false,
        bool IsTraveling = false,
        bool HasDifficulty = false,
        bool IsStudying = false,
        bool IsWorking = false,
        List<string>? CustomSituations = null
    );    // SpecificPrayer enum moved to Core.Models

    public record DailyDuaSchedule
    {
        public List<DuaResponse> MorningDuas { get; set; } = new();
        public List<DuaResponse> EveningDuas { get; set; } = new();
        public List<DuaResponse> MealDuas { get; set; } = new();
        public List<DuaResponse> SleepDuas { get; set; } = new();
        public List<DuaResponse> TravelDuas { get; set; } = new();
    }

    private record DuaEnhancement(
        string Benefits,
        string BestTimes,
        List<string> RelatedDuas,
        string HistoricalContext
    );
}
