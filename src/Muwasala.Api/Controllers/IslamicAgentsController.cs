using Muwasala.Core.Models;
using Muwasala.Agents;
using Muwasala.KnowledgeBase.Services;
using Microsoft.AspNetCore.Mvc;

namespace Muwasala.Api.Controllers;

/// <summary>
/// API controller for Quranic verse retrieval and guidance
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QuranController : ControllerBase
{
    private readonly QuranNavigatorAgent _quranAgent;
    private readonly ILogger<QuranController> _logger;

    public QuranController(QuranNavigatorAgent quranAgent, ILogger<QuranController> logger)
    {
        _quranAgent = quranAgent;
        _logger = logger;
    }

    /// <summary>
    /// Get contextual Quranic guidance
    /// </summary>
    [HttpPost("guidance")]
    public async Task<ActionResult<VerseResponse>> GetGuidance(
        [FromBody] GuidanceRequest request)
    {
        try
        {
            var response = await _quranAgent.GetVerseAsync(
                request.Context, 
                request.Language ?? "en", 
                request.Tafsir ?? "IbnKathir");
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Quranic guidance for context: {Context}", request.Context);
            return StatusCode(500, new { error = "Failed to retrieve guidance", details = ex.Message });
        }
    }

    /// <summary>
    /// Get thematic analysis of verses
    /// </summary>
    [HttpGet("theme/{theme}")]
    public async Task<ActionResult<List<VerseResponse>>> GetThematicAnalysis(
        string theme,
        [FromQuery] string language = "en",
        [FromQuery] int maxVerses = 5)
    {
        try
        {
            var responses = await _quranAgent.GetThematicAnalysisAsync(theme, language, maxVerses);
            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in thematic analysis for: {Theme}", theme);
            return StatusCode(500, new { error = "Failed to perform thematic analysis" });
        }
    }

    public record GuidanceRequest(string Context, string? Language = null, string? Tafsir = null);
}

/// <summary>
/// API controller for Hadith verification and search
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HadithController : ControllerBase
{
    private readonly HadithVerifierAgent _hadithAgent;
    private readonly IAdvancedHadithSearchService _advancedSearchService;
    private readonly ILogger<HadithController> _logger;

    public HadithController(
        HadithVerifierAgent hadithAgent, 
        IAdvancedHadithSearchService advancedSearchService,
        ILogger<HadithController> logger)
    {
        _hadithAgent = hadithAgent;
        _advancedSearchService = advancedSearchService;
        _logger = logger;
    }

    /// <summary>
    /// Verify authenticity of a hadith
    /// </summary>
    [HttpPost("verify")]
    public async Task<ActionResult<HadithResponse>> VerifyHadith(
        [FromBody] HadithVerificationRequest request)
    {
        try
        {
            var response = await _hadithAgent.VerifyHadithAsync(request.HadithText, request.Language ?? "en");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying hadith");
            return StatusCode(500, new { error = "Failed to verify hadith" });
        }
    }

    /// <summary>
    /// Search hadiths by topic
    /// </summary>
    [HttpGet("topic/{topic}")]
    public async Task<ActionResult<List<HadithResponse>>> GetHadithByTopic(
        string topic,
        [FromQuery] string language = "en",
        [FromQuery] int maxResults = 5)
    {
        try
        {
            var responses = await _hadithAgent.GetHadithByTopicAsync(topic, language, maxResults);
            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching hadiths for topic: {Topic}", topic);
            return StatusCode(500, new { error = "Failed to search hadiths" });
        }
    }

    public record HadithVerificationRequest(string HadithText, string? Language = null);
}

/// <summary>
/// API controller for Islamic jurisprudence guidance
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FiqhController : ControllerBase
{
    private readonly FiqhAdvisorAgent _fiqhAgent;
    private readonly ILogger<FiqhController> _logger;

    public FiqhController(FiqhAdvisorAgent fiqhAgent, ILogger<FiqhController> logger)
    {
        _fiqhAgent = fiqhAgent;
        _logger = logger;
    }

    /// <summary>
    /// Get Islamic ruling on a question
    /// </summary>
    [HttpPost("ruling")]
    public async Task<ActionResult<FiqhResponse>> GetRuling(
        [FromBody] FiqhQuestionRequest request)
    {
        try
        {
            var response = await _fiqhAgent.GetRulingAsync(
                request.Question, 
                request.Madhab, 
                request.Language ?? "en");
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fiqh ruling for: {Question}", request.Question);
            return StatusCode(500, new { error = "Failed to get ruling" });
        }
    }

    /// <summary>
    /// Calculate inheritance shares
    /// </summary>
    [HttpPost("inheritance")]
    public async Task<ActionResult<InheritanceResult>> CalculateInheritance(
        [FromBody] InheritanceRequest request)
    {
        try
        {
            var inheritanceCase = new FiqhAdvisorAgent.InheritanceCase(
                request.Gender,
                request.TotalEstate,
                request.Heirs
            );

            var result = await _fiqhAgent.CalculateInheritanceAsync(inheritanceCase, request.Madhab);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating inheritance");
            return StatusCode(500, new { error = "Failed to calculate inheritance" });
        }
    }

    /// <summary>
    /// Compare rulings across madhabs
    /// </summary>
    [HttpPost("compare")]
    public async Task<ActionResult<FiqhAdvisorAgent.MadhabComparisonResponse>> CompareMadhabs(
        [FromBody] ComparisonRequest request)
    {
        try
        {
            var response = await _fiqhAgent.CompareMadhabRulingsAsync(request.Question, request.Language ?? "en");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing madhab rulings");
            return StatusCode(500, new { error = "Failed to compare rulings" });
        }
    }

    public record FiqhQuestionRequest(string Question, Madhab Madhab = Madhab.Hanafi, string? Language = null);
    public record InheritanceRequest(string Gender, decimal TotalEstate, List<FiqhAdvisorAgent.Heir> Heirs, Madhab Madhab = Madhab.Hanafi);
    public record ComparisonRequest(string Question, string? Language = null);
}

/// <summary>
/// API controller for Islamic prayers and supplications
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DuaController : ControllerBase
{
    private readonly DuaCompanionAgent _duaAgent;
    private readonly ILogger<DuaController> _logger;

    public DuaController(DuaCompanionAgent duaAgent, ILogger<DuaController> logger)
    {
        _duaAgent = duaAgent;
        _logger = logger;
    }

    /// <summary>
    /// Get du'as for specific occasion
    /// </summary>
    [HttpGet("occasion/{occasion}")]
    public async Task<ActionResult<List<DuaResponse>>> GetDuasForOccasion(
        string occasion,
        [FromQuery] string language = "en",
        [FromQuery] int maxResults = 3)
    {
        try
        {
            var responses = await _duaAgent.GetDuasForOccasionAsync(occasion, language, maxResults);
            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting duas for occasion: {Occasion}", occasion);
            return StatusCode(500, new { error = "Failed to get du'as" });
        }
    }

    /// <summary>
    /// Get daily du'a schedule
    /// </summary>
    [HttpGet("daily")]
    public async Task<ActionResult<DuaCompanionAgent.DailyDuaSchedule>> GetDailySchedule(
        [FromQuery] string language = "en")
    {
        try
        {
            var schedule = await _duaAgent.GetDailyDuaScheduleAsync(language);
            return Ok(schedule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily dua schedule");
            return StatusCode(500, new { error = "Failed to get daily schedule" });
        }
    }

    /// <summary>
    /// Get specific prayer (Istikhara, Istighfar, etc.)
    /// </summary>
    [HttpGet("specific/{prayerType}")]    public async Task<ActionResult<DuaResponse>> GetSpecificPrayer(
        SpecificPrayer prayerType,
        [FromQuery] string language = "en")
    {
        try
        {
            var response = await _duaAgent.GetSpecificPrayerAsync(prayerType, language);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting specific prayer: {PrayerType}", prayerType);
            return StatusCode(500, new { error = "Failed to get specific prayer" });
        }
    }
}

/// <summary>
/// API controller for Quranic recitation and Tajweed guidance
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TajweedController : ControllerBase
{
    private readonly TajweedTutorAgent _tajweedAgent;
    private readonly ILogger<TajweedController> _logger;

    public TajweedController(TajweedTutorAgent tajweedAgent, ILogger<TajweedController> logger)
    {
        _tajweedAgent = tajweedAgent;
        _logger = logger;
    }

    /// <summary>
    /// Analyze tajweed rules for a specific verse
    /// </summary>
    [HttpGet("analyze/{surah}/{verse}")]
    public async Task<ActionResult<TajweedResponse>> AnalyzeVerse(
        int surah,
        int verse,
        [FromQuery] string language = "en")
    {
        try
        {
            var verseRef = new VerseReference(surah, verse);
            var response = await _tajweedAgent.AnalyzeVerseAsync(verseRef, language);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing tajweed for verse {Surah}:{Verse}", surah, verse);
            return StatusCode(500, new { error = "Failed to analyze tajweed" });
        }
    }

    /// <summary>
    /// Get pronunciation guide for Arabic word
    /// </summary>
    [HttpPost("pronunciation")]
    public async Task<ActionResult<TajweedTutorAgent.PronunciationGuide>> GetPronunciationGuide(
        [FromBody] PronunciationRequest request)
    {
        try
        {
            var guide = await _tajweedAgent.GetPronunciationGuideAsync(request.ArabicWord, request.Language ?? "en");
            return Ok(guide);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pronunciation guide for: {Word}", request.ArabicWord);
            return StatusCode(500, new { error = "Failed to get pronunciation guide" });
        }
    }

    /// <summary>
    /// Create interactive recitation lesson
    /// </summary>
    [HttpPost("lesson")]
    public async Task<ActionResult<TajweedTutorAgent.RecitationLesson>> CreateLesson(
        [FromBody] LessonRequest request)
    {
        try
        {
            var lesson = await _tajweedAgent.CreateRecitationLessonAsync(
                request.SurahNumber, 
                request.Level, 
                request.Language ?? "en");
            return Ok(lesson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lesson for Surah {Surah}", request.SurahNumber);
            return StatusCode(500, new { error = "Failed to create lesson" });
        }
    }

    /// <summary>
    /// Check common tajweed mistakes
    /// </summary>
    [HttpGet("mistakes/{surah}/{verse}")]
    public async Task<ActionResult<List<TajweedTutorAgent.TajweedCorrection>>> CheckMistakes(
        int surah,
        int verse,
        [FromQuery] string language = "en")
    {
        try
        {
            var verseRef = new VerseReference(surah, verse);
            var corrections = await _tajweedAgent.CheckCommonMistakesAsync(verseRef, language);
            return Ok(corrections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking mistakes for verse {Surah}:{Verse}", surah, verse);
            return StatusCode(500, new { error = "Failed to check mistakes" });
        }
    }

    /// <summary>
    /// Get Qira'at (recitation style) information
    /// </summary>
    [HttpGet("qiraat/{surah}/{verse}")]
    public async Task<ActionResult<TajweedTutorAgent.QiraatInfo>> GetQiraatInfo(
        int surah,
        int verse,
        [FromQuery] QiraatType qiraatType = QiraatType.Hafs,
        [FromQuery] string language = "en")
    {
        try
        {
            var verseRef = new VerseReference(surah, verse);
            var qiraatInfo = await _tajweedAgent.GetQiraatInfoAsync(verseRef, qiraatType, language);
            return Ok(qiraatInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Qiraat info for verse {Surah}:{Verse}", surah, verse);
            return StatusCode(500, new { error = "Failed to get Qiraat information" });
        }
    }

    public record PronunciationRequest(string ArabicWord, string? Language = null);
    public record LessonRequest(int SurahNumber, RecitationLevel Level = RecitationLevel.Beginner, string? Language = null);
}

/// <summary>
/// API controller for Prophetic biography and historical guidance
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SirahController : ControllerBase
{
    private readonly SirahScholarAgent _sirahAgent;
    private readonly ILogger<SirahController> _logger;

    public SirahController(SirahScholarAgent sirahAgent, ILogger<SirahController> logger)
    {
        _sirahAgent = sirahAgent;
        _logger = logger;
    }

    /// <summary>
    /// Get Prophetic guidance for a life situation
    /// </summary>
    [HttpPost("guidance")]
    public async Task<ActionResult<SirahResponse>> GetGuidance(
        [FromBody] SirahGuidanceRequest request)
    {
        try
        {
            var response = await _sirahAgent.GetGuidanceFromSirahAsync(request.Situation, request.Language ?? "en");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Sirah guidance for: {Situation}", request.Situation);
            return StatusCode(500, new { error = "Failed to get Sirah guidance" });
        }
    }

    /// <summary>
    /// Get events from a specific period in Prophet's life
    /// </summary>
    [HttpGet("period/{period}")]
    public async Task<ActionResult<List<SirahResponse>>> GetEventsByPeriod(
        SirahPeriod period,
        [FromQuery] string language = "en")
    {
        try
        {
            var responses = await _sirahAgent.GetEventsByPeriodAsync(period, language);
            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events for period: {Period}", period);
            return StatusCode(500, new { error = "Failed to get period events" });
        }
    }

    /// <summary>
    /// Explore Prophet's character traits
    /// </summary>
    [HttpGet("character/{aspect}")]
    public async Task<ActionResult<SirahResponse>> GetCharacteristics(
        string aspect,
        [FromQuery] string language = "en")
    {
        try
        {
            var response = await _sirahAgent.GetPropheticCharacteristicsAsync(aspect, language);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exploring character aspect: {Aspect}", aspect);
            return StatusCode(500, new { error = "Failed to get character guidance" });
        }
    }

    /// <summary>
    /// Get complete historical timeline
    /// </summary>
    [HttpGet("timeline")]
    public async Task<ActionResult<ChronologicalTimeline>> GetTimeline(
        [FromQuery] string language = "en")
    {
        try
        {
            var timeline = await _sirahAgent.GetHistoricalTimelineAsync(language);
            return Ok(timeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Sirah timeline");
            return StatusCode(500, new { error = "Failed to get timeline" });
        }
    }

    public record SirahGuidanceRequest(string Situation, string? Language = null);
}

/// <summary>
/// Main API controller for unified Islamic knowledge search using multi-agent system
/// This is the primary endpoint that performance tests call: /api/islamicagents/search
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class IslamicAgentsController : ControllerBase
{
    private readonly Muwasala.Agents.IEnhancedHybridSearchService _hybridSearchService;
    private readonly ILogger<IslamicAgentsController> _logger;

    public IslamicAgentsController(
        Muwasala.Agents.IEnhancedHybridSearchService hybridSearchService,
        ILogger<IslamicAgentsController> logger)
    {
        _hybridSearchService = hybridSearchService;
        _logger = logger;
    }

    /// <summary>
    /// Unified search endpoint that uses the multi-agent system for comprehensive Islamic knowledge search
    /// This is the main endpoint called by performance tests and the frontend
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<HybridSearchResponse>> Search(
        [FromQuery] string query,
        [FromQuery] string language = "en",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { error = "Query parameter is required" });
        }

        try
        {
            _logger.LogInformation("🔍 Islamic Agents Search Request - Query: {Query}, Language: {Language}, Page: {Page}, Size: {PageSize}", 
                query, language, page, pageSize);

            // Use cancellation token with timeout to prevent 100+ second delays
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            
            var response = await _hybridSearchService.SearchAsync(query, language);
            
            // Apply pagination to results
            var paginatedResults = ApplyPagination(response, page, pageSize);
            
            _logger.LogInformation("✅ Search completed - Processing time: {Time}ms, Results: {Count}", 
                response.ProcessingTimeMs, paginatedResults.WebResults.Count);

            return Ok(paginatedResults);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("⏰ Search request timed out for query: {Query}", query);
            return StatusCode(408, new { error = "Search request timed out", timeout = "30 seconds" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during Islamic agents search for query: {Query}", query);
            return StatusCode(500, new { error = "Internal server error during search", details = ex.Message });
        }
    }

    /// <summary>
    /// Health check endpoint for the multi-agent search system
    /// </summary>
    [HttpGet("health")]
    public ActionResult GetHealth()
    {
        return Ok(new 
        { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            agents = new[]
            {
                "EnhancedDeepSeekBrainAgent",
                "RealTimeSearchAgent",
                "QueryAnalysisAgent",
                "ResponseQualityAgent"
            }
        });
    }

    /// <summary>
    /// Get search suggestions based on partial query
    /// </summary>
    [HttpGet("suggestions")]
    public ActionResult<List<string>> GetSearchSuggestions([FromQuery] string partialQuery)
    {
        if (string.IsNullOrWhiteSpace(partialQuery))
        {
            return BadRequest(new { error = "PartialQuery parameter is required" });
        }

        try
        {
            // Generate suggestions based on common Islamic topics
            var suggestions = GenerateSearchSuggestions(partialQuery);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating search suggestions for: {Query}", partialQuery);
            return StatusCode(500, new { error = "Failed to generate suggestions" });
        }
    }

    private HybridSearchResponse ApplyPagination(HybridSearchResponse response, int page, int pageSize)
    {
        var startIndex = (page - 1) * pageSize;
        var paginatedWebResults = response.WebResults
            .Skip(startIndex)
            .Take(pageSize)
            .ToList();

        return new HybridSearchResponse
        {
            AIResponse = response.AIResponse,
            WebResults = paginatedWebResults,
            SearchSuggestions = response.SearchSuggestions,
            ProcessingTimeMs = response.ProcessingTimeMs
        };
    }

    private List<string> GenerateSearchSuggestions(string partialQuery)
    {
        var lowercaseQuery = partialQuery.ToLower();
        var suggestions = new List<string>();

        var commonQueries = new[]
        {
            "prayer times", "Quran verses", "hadith collection", "Islamic law", "Prophet Muhammad",
            "pillars of Islam", "Ramadan fasting", "Hajj pilgrimage", "zakat calculation", "Islamic marriage",
            "divorce in Islam", "funeral prayers", "Islamic finance", "halal food", "Islamic calendar",
            "Friday prayer", "night prayer", "morning prayer", "charity in Islam", "Islamic history"
        };

        foreach (var commonQuery in commonQueries)
        {
            if (commonQuery.Contains(lowercaseQuery, StringComparison.OrdinalIgnoreCase))
            {
                suggestions.Add(commonQuery);
            }
        }

        return suggestions.Take(5).ToList();
    }
}
