using Microsoft.Extensions.Logging;
using Muwasala.Core.Models;
using Muwasala.Core.Services;
using Muwasala.KnowledgeBase.Services;

namespace Muwasala.Agents;

/// <summary>
/// Sirah Scholar Agent - Prophetic biography and historical guidance
/// Uses Phi-3 model for detailed historical context and life lessons from Prophet Muhammad's life
/// </summary>
public class SirahScholarAgent
{
    private readonly IOllamaService _ollama;
    private readonly ISirahService _sirahService;
    private readonly ILogger<SirahScholarAgent> _logger;
    private const string MODEL_NAME = "deepseek-r1";

    public SirahScholarAgent(
        IOllamaService ollama,
        ISirahService sirahService,
        ILogger<SirahScholarAgent> logger)
    {
        _ollama = ollama;
        _sirahService = sirahService;
        _logger = logger;
    }

    /// <summary>
    /// Get Prophetic guidance for a specific life situation based on Sirah
    /// </summary>
    public async Task<SirahResponse> GetGuidanceFromSirahAsync(
        string situation, 
        string language = "en")
    {
        _logger.LogInformation("SirahScholar providing guidance for situation: {Situation}", situation);

        try
        {
            // Search for relevant events in Prophet's life
            var relevantEvents = await _sirahService.SearchEventsByContextAsync(situation, language);
            var guidance = await _sirahService.GetGuidanceByTopicAsync(situation, language);

            // Find the most relevant event
            var primaryEvent = relevantEvents.FirstOrDefault() ?? new SirahEvent(
                "General Prophetic Guidance",
                "Guidance from the life and teachings of Prophet Muhammad (peace be upon him)",
                SirahPeriod.EarlyMedina,
                null,
                "General",
                Language: language
            );            // Get AI analysis of how this applies to the user's situation
            var prompt = BuildSirahGuidancePrompt(primaryEvent, guidance, situation, language);
            var analysis = await _ollama.GenerateResponseAsync(
                MODEL_NAME, prompt, temperature: 0.3);

            var response = new SirahResponse
            {
                Topic = situation,
                Event = primaryEvent.Name,
                Description = analysis,
                Period = primaryEvent.Period,
                KeyLessons = "Key lessons from the Prophet's life applicable to this situation",
                RelatedEvents = relevantEvents.Take(3).Select(e => e.Name).ToList(),
                ModernApplication = new List<string> { "Apply Prophetic wisdom in daily life" },
                PropheticWisdom = "Guidance from the Sunnah and Sirah"
            };

            response.Sources.AddRange(new[] { "Sirah An-Nabawiyyah", "Al-Raheeq Al-Makhtum", MODEL_NAME });
            
            _logger.LogInformation("SirahScholar completed guidance for {Situation}", situation);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SirahScholar for situation: {Situation}", situation);
            throw;
        }
    }

    /// <summary>
    /// Get chronological events for a specific period in Prophet's life
    /// </summary>
    public async Task<List<SirahResponse>> GetEventsByPeriodAsync(
        SirahPeriod period, 
        string language = "en")
    {
        _logger.LogInformation("Getting Sirah events for period: {Period}", period);

        var events = await _sirahService.GetEventsByPeriodAsync(period, language);
        var responses = new List<SirahResponse>();

        foreach (var sirahEvent in events)
        {
            var prompt = BuildEventAnalysisPrompt(sirahEvent, language);
            var analysis = await _ollama.GenerateStructuredResponseAsync<SirahAnalysis>(
                MODEL_NAME, prompt, temperature: 0.1);

            var response = new SirahResponse
            {
                Topic = $"{period} Period",
                Event = sirahEvent.Name,
                Description = sirahEvent.Description,
                Period = sirahEvent.Period,
                KeyLessons = analysis.KeyLessons,
                RelatedEvents = analysis.RelatedEvents,
                ModernApplication = analysis.ModernApplication,
                PropheticWisdom = analysis.PropheticWisdom
            };

            response.Sources.Add("As-Sirah An-Nabawiyyah");
            responses.Add(response);
        }

        return responses;
    }

    /// <summary>
    /// Explore Prophet's character traits and how to emulate them
    /// </summary>
    public async Task<SirahResponse> GetPropheticCharacteristicsAsync(
        string characterAspect, 
        string language = "en")
    {
        _logger.LogInformation("Exploring Prophetic characteristic: {Aspect}", characterAspect);

        var characteristics = await _sirahService.GetCharacteristicsAsync(characterAspect, language);
        var primaryCharacteristic = characteristics.FirstOrDefault() ?? new PropheticCharacteristic(
            characterAspect,
            "A noble characteristic of Prophet Muhammad (peace be upon him)",
            Language: language
        );

        var prompt = BuildCharacterPrompt(primaryCharacteristic, language);
        var analysis = await _ollama.GenerateStructuredResponseAsync<SirahAnalysis>(
            MODEL_NAME, prompt, temperature: 0.1);

        var response = new SirahResponse
        {
            Topic = characterAspect,
            Event = $"Prophetic Character: {characterAspect}",
            Description = primaryCharacteristic.Description,
            Period = SirahPeriod.EarlyMedina, // Default period
            KeyLessons = analysis.KeyLessons,
            RelatedEvents = analysis.RelatedEvents,
            ModernApplication = analysis.ModernApplication,
            PropheticWisdom = analysis.PropheticWisdom
        };

        response.Sources.AddRange(new[] { "Ash-Shama'il Al-Muhammadiyyah", "Character of Prophet", MODEL_NAME });
        return response;
    }

    /// <summary>
    /// Get historical timeline with key events and lessons
    /// </summary>
    public async Task<ChronologicalTimeline> GetHistoricalTimelineAsync(string language = "en")
    {
        _logger.LogInformation("Generating complete Sirah timeline");

        var timeline = await _sirahService.GetTimelineAsync(language);
        
        // Enhance timeline with AI analysis of interconnections
        var prompt = BuildTimelineAnalysisPrompt(timeline, language);
        var timelineAnalysis = await _ollama.GenerateStructuredResponseAsync<TimelineAnalysis>(
            MODEL_NAME, prompt, temperature: 0.1);

        // Add AI insights to the timeline
        foreach (var period in new[] { timeline.MeccanPeriod, timeline.MedinanPeriod, timeline.MajorEvents })
        {
            foreach (var sirahEvent in period)
            {
                // Enhance each event with contextual analysis
                if (timelineAnalysis.EventConnections.ContainsKey(sirahEvent.Name))
                {
                    var connection = timelineAnalysis.EventConnections[sirahEvent.Name];
                    sirahEvent.KeyLessons.AddRange(connection.Split(", "));
                }
            }
        }

        return timeline;
    }

    private string BuildSirahGuidancePrompt(
        SirahEvent sirahEvent, 
        List<PropheticGuidance> guidance, 
        string situation, 
        string language)
    {
        var languageInstruction = language switch
        {
            "ar" => "Please respond in Arabic language (العربية). Use Islamic terminology in Arabic and provide explanations in Arabic.",
            "en" => "Please respond in English language. Use clear English explanations.",
            _ => "Please respond in English language. Use clear English explanations."
        };

        var guidanceText = guidance.Any() 
            ? string.Join("\n", guidance.Select(g => $"- {g.Guidance} (Context: {g.Context})"))
            : "General guidance from Prophet's life";        return $@"You are an Islamic scholar specializing in the Sirah (biography) of Prophet Muhammad (peace be upon him).

{languageInstruction}

User's Situation: ""{situation}""

Relevant Event from Sirah:
Event: {sirahEvent.Name}
Description: {sirahEvent.Description}
Period: {sirahEvent.Period}
Location: {sirahEvent.Location}

Existing Guidance:
{guidanceText}

Please provide clear guidance on how this relates to the user's situation. Include:
1. Key lessons from this event that apply to the situation
2. Practical applications of the Prophetic example
3. Any relevant wisdom or sayings from Prophet Muhammad (PBUH)

Respond with clear, practical guidance in the requested language.";
    }

    private string BuildEventAnalysisPrompt(SirahEvent sirahEvent, string language)
    {
        return $@"
Analyze this event from the life of Prophet Muhammad (peace be upon him):

Event: {sirahEvent.Name}
Description: {sirahEvent.Description}
Period: {sirahEvent.Period}
Location: {sirahEvent.Location}

Provide deep analysis including:
1. Historical context and significance
2. Key lessons and wisdom gained
3. How this event connects to other events in his life
4. Modern applications for Muslims today

Language: {language}

Respond in JSON format with scholarly analysis.";
    }

    private string BuildCharacterPrompt(PropheticCharacteristic characteristic, string language)
    {
        var examplesText = characteristic.Examples.Any() 
            ? string.Join(", ", characteristic.Examples)
            : "Various examples from his life";

        return $@"
Explore this aspect of Prophet Muhammad's (PBUH) character:

Characteristic: {characteristic.Aspect}
Description: {characteristic.Description}
Examples: {examplesText}

Provide guidance on:
1. How this characteristic was manifested in his life
2. Related events that demonstrate this quality
3. Practical ways Muslims can develop this characteristic
4. Relevant prophetic sayings about this trait

Language: {language}

Respond in JSON format with character development guidance.";
    }

    private string BuildTimelineAnalysisPrompt(ChronologicalTimeline timeline, string language)
    {
        var majorEvents = string.Join(", ", timeline.MajorEvents.Select(e => e.Name));
        
        return $@"
Analyze the connections and progression in Prophet Muhammad's life:

Major Events: {majorEvents}
Meccan Period Events: {timeline.MeccanPeriod.Count} events
Medinan Period Events: {timeline.MedinanPeriod.Count} events

Identify:
1. How events built upon each other
2. Lessons that emerge from the chronological progression  
3. Patterns in prophetic methodology
4. Key turning points and their significance

Language: {language}

Respond in JSON format mapping event names to their broader significance.";
    }

    // Supporting records for AI analysis
    private record SirahAnalysis(
        string KeyLessons,
        List<string> RelatedEvents,
        List<string> ModernApplication,
        string PropheticWisdom
    );

    private record TimelineAnalysis(
        Dictionary<string, string> EventConnections,
        List<string> KeyTurningPoints,
        string OverallProgression
    );
}
