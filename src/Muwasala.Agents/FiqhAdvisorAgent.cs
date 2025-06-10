using Microsoft.Extensions.Logging;
using Muwasala.Core.Models;
using Muwasala.Core.Services;
using Muwasala.KnowledgeBase.Services;

namespace Muwasala.Agents;

/// <summary>
/// Fiqh Advisor Agent - Jurisprudential guidance across all four Sunni madhabs
/// Uses Phi-3 model for nuanced Islamic legal analysis
/// </summary>
public class FiqhAdvisorAgent
{
    private readonly IOllamaService _ollama;
    private readonly IFiqhService _fiqhService;
    private readonly ILogger<FiqhAdvisorAgent> _logger;
    private const string MODEL_NAME = "phi3:mini";

    public FiqhAdvisorAgent(
        IOllamaService ollama,
        IFiqhService fiqhService,
        ILogger<FiqhAdvisorAgent> logger)
    {
        _ollama = ollama;
        _fiqhService = fiqhService;
        _logger = logger;
    }

    /// <summary>
    /// Get Islamic legal ruling on a specific question
    /// </summary>
    public async Task<FiqhResponse> GetRulingAsync(
        string question, 
        Madhab userMadhab = Madhab.Hanafi, 
        string language = "en")
    {
        _logger.LogInformation("FiqhAdvisor processing question for {Madhab} madhab: {Question}", 
            userMadhab, question);

        try
        {
            // Search existing fiqh database for similar rulings
            var existingRulings = await _fiqhService.SearchRulingsAsync(question, userMadhab, language);
            
            // Get rulings from all madhabs for comparison
            var allMadhabRulings = new Dictionary<Madhab, string>();
            foreach (Madhab madhab in Enum.GetValues<Madhab>())
            {
                var madhabRulings = await _fiqhService.SearchRulingsAsync(question, madhab, language);
                if (madhabRulings.Any())
                {
                    allMadhabRulings[madhab] = madhabRulings.First().Ruling;
                }
            }            // Generate comprehensive AI analysis using simplified text response
            var prompt = BuildFiqhPrompt(question, userMadhab, existingRulings, allMadhabRulings, language);
            var aiRuling = await _ollama.GenerateResponseAsync(
                MODEL_NAME, prompt, temperature: 0.3);

            var response = new FiqhResponse
            {
                Question = question,
                Ruling = aiRuling,
                Madhab = userMadhab,
                Evidence = "Based on classical Islamic jurisprudence texts and " + userMadhab + " School methodology",
                OtherMadhabRulings = allMadhabRulings,
                ScholarlyReferences = new List<string> { $"{userMadhab} classical scholars", "Contemporary Islamic jurists" },
                ModernApplication = "Please consult with qualified scholars for specific personal situations."
            };

            // Add standard warning for complex issues
            response = response with 
            { 
                Warning = "This is jurisprudential guidance. Consult qualified scholars for personal situations." 
            };

            response.Sources.Add($"{userMadhab} jurisprudence");
            response.Sources.Add(MODEL_NAME);

            _logger.LogInformation("FiqhAdvisor provided {Madhab} ruling for question", userMadhab);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FiqhAdvisor for question: {Question}", question);
            throw;
        }
    }

    /// <summary>
    /// Get travel prayer rulings based on distance and madhab
    /// </summary>
    public async Task<TravelPrayerRuling> GetTravelPrayerRulesAsync(
        Madhab madhab, 
        double distanceKm, 
        string language = "en")
    {
        _logger.LogInformation("Getting travel prayer rules for {Madhab}, distance: {Distance}km", 
            madhab, distanceKm);

        var prompt = BuildTravelPrayerPrompt(madhab, distanceKm, language);
        var ruling = await _ollama.GenerateStructuredResponseAsync<TravelPrayerRuling>(
            MODEL_NAME, prompt, temperature: 0.1);

        return ruling;
    }

    /// <summary>
    /// Calculate Islamic inheritance shares
    /// </summary>
    public async Task<InheritanceResult> CalculateInheritanceAsync(
        InheritanceCase inheritanceCase,
        Madhab madhab = Madhab.Hanafi)
    {
        _logger.LogInformation("Calculating inheritance for {Madhab} madhab", madhab);

        var prompt = BuildInheritancePrompt(inheritanceCase, madhab);
        var calculation = await _ollama.GenerateStructuredResponseAsync<InheritanceCalculation>(
            MODEL_NAME, prompt, temperature: 0.1);

        // Perform actual mathematical calculations
        var finalAmounts = CalculateFinalAmounts(inheritanceCase.TotalEstate, calculation.Shares);

        var result = new InheritanceResult
        {
            Shares = calculation.Shares,
            TotalEstate = inheritanceCase.TotalEstate,
            FinalAmounts = finalAmounts,
            CalculationMethod = $"{madhab} School of Jurisprudence",
            SpecialCases = calculation.SpecialCases
        };

        result.Sources.AddRange(new[] { "Kitab al-Fara'id", $"{madhab} Inheritance Rules", MODEL_NAME });

        return result;
    }

    /// <summary>
    /// Compare rulings across all four madhabs
    /// </summary>
    public async Task<MadhabComparisonResponse> CompareMadhabRulingsAsync(
        string question, 
        string language = "en")
    {
        _logger.LogInformation("Comparing madhab rulings for: {Question}", question);

        var comparisons = new Dictionary<Madhab, FiqhResponse>();
        
        foreach (Madhab madhab in Enum.GetValues<Madhab>())
        {
            comparisons[madhab] = await GetRulingAsync(question, madhab, language);
        }

        var prompt = BuildComparisonPrompt(question, comparisons, language);
        var analysis = await _ollama.GenerateStructuredResponseAsync<MadhabComparisonAnalysis>(
            MODEL_NAME, prompt, temperature: 0.1);

        return new MadhabComparisonResponse
        {
            Question = question,
            MadhabRulings = comparisons,
            CommonGround = analysis.CommonGround,
            KeyDifferences = analysis.KeyDifferences,
            RecommendedApproach = analysis.RecommendedApproach
        };
    }

    private string BuildFiqhPrompt(
        string question, 
        Madhab madhab, 
        List<FiqhRuling> existingRulings,
        Dictionary<Madhab, string> allMadhabRulings,
        string language)
    {
        var existingRulingsText = existingRulings.Any() 
            ? string.Join("\n", existingRulings.Select(r => $"- {r.Ruling} (Source: {r.Source})"))
            : "No specific rulings found in database";

        var madhabComparisons = string.Join("\n", allMadhabRulings.Select(kvp => 
            $"- {kvp.Key}: {kvp.Value}"));        return $@"
You are a qualified Islamic jurisprudence scholar specializing in the {madhab} school of thought.

Question: ""{question}""

Existing {madhab} rulings in our database:
{existingRulingsText}

Other madhab positions:
{madhabComparisons}

Please provide a clear and concise {madhab} ruling for this question. Include:
1. The primary ruling according to {madhab} methodology
2. Brief evidence from Quran and Hadith
3. Any practical considerations for modern application

Respond with a clear, direct answer without complex formatting or JSON structure.";
    }

    private string BuildTravelPrayerPrompt(Madhab madhab, double distanceKm, string language)
    {
        return $@"
Determine travel prayer rulings according to {madhab} jurisprudence:

Travel Distance: {distanceKm} km
Madhab: {madhab}
Language: {language}

Provide rulings for:
1. Prayer shortening (Qasr) permission
2. Prayer combining (Jam') permission  
3. Minimum distance requirements
4. Duration of travel concessions

Respond in JSON format with specific {madhab} rulings.";
    }

    private string BuildInheritancePrompt(InheritanceCase inheritanceCase, Madhab madhab)
    {
        return $@"
Calculate Islamic inheritance according to {madhab} jurisprudence:

Deceased: {inheritanceCase.Gender}
Total Estate: {inheritanceCase.TotalEstate:C}
Heirs: {string.Join(", ", inheritanceCase.Heirs.Select(h => $"{h.Relationship} ({h.Gender})"))}

Apply {madhab} inheritance rules and calculate shares for each heir.
Include any special cases or adjustments.

Respond in JSON format with detailed breakdown.";
    }

    private string BuildComparisonPrompt(
        string question, 
        Dictionary<Madhab, FiqhResponse> comparisons, 
        string language)
    {
        var rulingsText = string.Join("\n\n", comparisons.Select(kvp => 
            $"{kvp.Key} Position: {kvp.Value.Ruling}\nEvidence: {kvp.Value.Evidence}"));

        return $@"
Analyze the differences and similarities in madhab rulings for this question:
""{question}""

Madhab Positions:
{rulingsText}

Provide:
1. What all madhabs agree on (if anything)
2. Key differences and their underlying reasons
3. Balanced approach for a Muslim to follow

Language: {language}

Respond in JSON format with scholarly analysis.";
    }

    private Dictionary<string, decimal> CalculateFinalAmounts(decimal totalEstate, Dictionary<string, string> shares)
    {
        var finalAmounts = new Dictionary<string, decimal>();
        
        // This is a simplified calculation - in reality, Islamic inheritance 
        // calculations are quite complex and require sophisticated algorithms
        foreach (var share in shares)
        {
            if (share.Value.Contains("/"))
            {
                var parts = share.Value.Split('/');
                if (parts.Length == 2 && 
                    int.TryParse(parts[0], out int numerator) && 
                    int.TryParse(parts[1], out int denominator))
                {
                    finalAmounts[share.Key] = totalEstate * numerator / denominator;
                }
            }
        }

        return finalAmounts;
    }

    // Supporting records and classes
    private record FiqhAnalysis(
        string PrimaryRuling,
        string Evidence,
        string ModernApplication,
        List<string> ScholarlyReferences,
        List<string> Sources,
        bool RequiresScholarConsultation,
        List<string> Conditions
    );

    public record TravelPrayerRuling(
        bool AllowsQasr,
        bool AllowsJam,
        double MinimumDistanceKm,
        int MaxDurationDays,
        List<string> Conditions
    );

    public record InheritanceCase(
        string Gender,
        decimal TotalEstate,
        List<Heir> Heirs
    );

    public record Heir(
        string Relationship,
        string Gender,
        bool IsAlive = true
    );

    private record InheritanceCalculation(
        Dictionary<string, string> Shares,
        List<string> SpecialCases
    );

    public record MadhabComparisonResponse : BaseResponse
    {
        public required string Question { get; init; }
        public required Dictionary<Madhab, FiqhResponse> MadhabRulings { get; init; }
        public required List<string> CommonGround { get; init; }
        public required List<string> KeyDifferences { get; init; }
        public required string RecommendedApproach { get; init; }
    }

    private record MadhabComparisonAnalysis(
        List<string> CommonGround,
        List<string> KeyDifferences,
        string RecommendedApproach
    );
}
