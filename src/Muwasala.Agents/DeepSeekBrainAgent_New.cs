using Microsoft.Extensions.Logging;
using Muwasala.Core.Models;
using Muwasala.Core.Services;
using System.Text.Json;

namespace Muwasala.Agents;

/// <summary>
/// Enhanced DeepSeek Brain Agent - Central intelligence coordinator for Islamic knowledge processing
/// Orchestrates specialized agents with advanced query analysis and quality assurance
/// </summary>
public class EnhancedDeepSeekBrainAgent
{
    private readonly IOllamaService _ollama;
    private readonly ILogger<EnhancedDeepSeekBrainAgent> _logger;
    private readonly WebCrawlerAgent _webCrawler;
    private readonly HadithVerifierAgent _hadithVerifier;
    private readonly FiqhAdvisorAgent _fiqhAdvisor;
    private readonly QuranNavigatorAgent _quranNavigator;
    private readonly QueryAnalysisAgent _queryAnalysisAgent;
    private readonly RealTimeSearchAgent _realTimeSearchAgent;
    private readonly ResponseQualityAgent _responseQualityAgent;    
    private const string DEEPSEEK_MODEL = "deepseek-r1:7b"; // Using DeepSeek model via Ollama
    
    public EnhancedDeepSeekBrainAgent(
        IOllamaService ollama,
        ILogger<EnhancedDeepSeekBrainAgent> logger,
        WebCrawlerAgent webCrawler,
        HadithVerifierAgent hadithVerifier,
        FiqhAdvisorAgent fiqhAdvisor,
        QuranNavigatorAgent quranNavigator,
        QueryAnalysisAgent queryAnalysisAgent,
        RealTimeSearchAgent realTimeSearchAgent,
        ResponseQualityAgent responseQualityAgent)
    {
        _ollama = ollama;
        _logger = logger;
        _webCrawler = webCrawler;
        _hadithVerifier = hadithVerifier;
        _fiqhAdvisor = fiqhAdvisor;
        _quranNavigator = quranNavigator;
        _queryAnalysisAgent = queryAnalysisAgent;
        _realTimeSearchAgent = realTimeSearchAgent;
        _responseQualityAgent = responseQualityAgent;
    }

    /// <summary>
    /// Process a query using enhanced multi-agent intelligence with DeepSeek coordination
    /// </summary>
    public async Task<IntelligentResponse> ProcessQueryAsync(string query, string language = "en")
    {
        _logger.LogInformation("🧠 Enhanced DeepSeek Brain processing query: {Query}", query);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Step 1: Advanced Query Analysis using our sophisticated NLP agent
            _logger.LogInformation("🔍 Starting advanced query analysis...");
            var queryAnalysis = await _queryAnalysisAgent.AnalyzeQueryAsync(query, language);
            _logger.LogInformation("📊 Query Analysis Complete - Types: {Types}, Complexity: {Complexity}", 
                string.Join(", ", queryAnalysis.QueryTypes), queryAnalysis.ComplexityLevel);

            // Step 2: Real-time search for latest Islamic knowledge
            _logger.LogInformation("🌐 Initiating real-time search across Islamic sources...");
            var searchResults = await _realTimeSearchAgent.SearchTrustedSourcesAsync(queryAnalysis);
            _logger.LogInformation("🔍 Found {Count} relevant sources", searchResults.Count);

            // Step 3: Coordinate specialized agents based on query analysis
            var agentTasks = new List<Task<object>>();
            
            if (queryAnalysis.QueryTypes.Contains(QueryType.HadithStudy))
            {
                _logger.LogInformation("📖 Activating Hadith Verifier Agent");
                agentTasks.Add(Task.Run<object>(async () => await _hadithVerifier.VerifyHadithAsync(query, language)));
            }

            if (queryAnalysis.QueryTypes.Contains(QueryType.IslamicLaw) || queryAnalysis.QueryTypes.Contains(QueryType.ReligiousPractice))
            {
                _logger.LogInformation("⚖️ Activating Fiqh Advisor Agent");
                agentTasks.Add(Task.Run<object>(async () => await _fiqhAdvisor.GetRulingAsync(query, Madhab.Hanafi, language)));
            }

            if (queryAnalysis.QueryTypes.Contains(QueryType.QuranStudy) || queryAnalysis.ExtractedEntities.Any(e => e.Type == EntityType.QuranReference))
            {
                _logger.LogInformation("📚 Activating Quran Navigator Agent");
                agentTasks.Add(Task.Run<object>(async () => await _quranNavigator.GetVerseAsync(query, language)));
            }

            // Step 4: Wait for all agents to complete
            var agentResults = await Task.WhenAll(agentTasks);
            
            // Step 5: Synthesize results with DeepSeek
            var response = await SynthesizeResponse(query, queryAnalysis, agentResults, searchResults, language);            // Step 6: Quality assurance check
            _logger.LogInformation("🔍 Running quality assurance checks...");
            var qualityReport = await _responseQualityAgent.ValidateResponseAsync(response.Answer, queryAnalysis, searchResults);
            _logger.LogInformation("📊 Quality Score: {Score}, Passed: {Passed}", qualityReport.OverallQualityScore, qualityReport.OverallQualityScore > 0.7f);

            // Step 7: Enhance response if needed
            if (qualityReport.OverallQualityScore < 0.8f && !string.IsNullOrEmpty(qualityReport.EnhancedResponse))
            {
                _logger.LogInformation("🔧 Using enhanced response based on quality feedback...");
                response = new IntelligentResponse
                {
                    Answer = qualityReport.EnhancedResponse,
                    Sources = response.Sources,
                    WebSearchResults = response.WebSearchResults,
                    RelatedQuestions = response.RelatedQuestions,
                    IsSuccessful = true
                };
            }

            stopwatch.Stop();
            
            // Create final response with timing
            return new IntelligentResponse
            {
                Answer = response.Answer,
                Sources = response.Sources,
                WebSearchResults = response.WebSearchResults,
                RelatedQuestions = response.RelatedQuestions,
                IsSuccessful = response.IsSuccessful,
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in Enhanced DeepSeek Brain processing");
            stopwatch.Stop();
            return CreateErrorResponse(ex.Message, language);
        }
    }

    private async Task<IntelligentResponse> SynthesizeResponse(
        string originalQuery,
        QueryAnalysisResult analysis, 
        object[] agentResults,
        List<WebSearchResult> searchResults,
        string language)
    {
        _logger.LogInformation("🧩 Synthesizing response from {Count} agent results and {SearchCount} search results", 
            agentResults.Length, searchResults.Count);

        var synthesisPrompt = $@"
As an Islamic scholar AI powered by DeepSeek, synthesize this information into a comprehensive response:

Original Query: ""{originalQuery}""
User Intent: {analysis.PrimaryIntent}
Query Complexity: {analysis.ComplexityLevel}
Emotional Context: {analysis.EmotionalContext}

Agent Results:
{string.Join("\n\n", agentResults.Select((r, i) => $"Agent {i + 1}: {JsonSerializer.Serialize(r)}"))}

Real-time Search Results:
{string.Join("\n\n", searchResults.Take(5).Select((r, i) => $"Source {i + 1}: {r.Title} - {r.Snippet}"))}

Extracted Islamic Entities:
{string.Join(", ", analysis.ExtractedEntities.Select(e => $"{e.Text} ({e.Type})"))}

Provide a scholarly Islamic response in {language} that:
1. Directly answers the user's question with appropriate depth for {analysis.ComplexityLevel} complexity
2. Includes relevant Quranic verses if applicable
3. References authentic Hadiths when relevant
4. Provides scholarly context and different madhab views when appropriate
5. Maintains Islamic etiquette and respect, especially considering {analysis.EmotionalContext} emotional context
6. Cites sources when possible
7. Uses appropriate tone based on user's emotional state

Format as a comprehensive but well-structured response.";

        try
        {
            var synthesizedAnswer = await _ollama.GenerateResponseAsync(synthesisPrompt, DEEPSEEK_MODEL);
            
            return new IntelligentResponse
            {
                Answer = synthesizedAnswer,
                Sources = ExtractSourcesFromResults(agentResults, searchResults),
                WebSearchResults = searchResults,
                RelatedQuestions = await GenerateRelatedQuestions(originalQuery, analysis, language),
                IsSuccessful = true,
                ProcessingTimeMs = 0 // Will be set by caller
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Response synthesis failed");
            return CreateErrorResponse("Failed to synthesize response", language);
        }
    }

    private List<string> ExtractSourcesFromResults(object[] agentResults, List<WebSearchResult> searchResults)
    {
        var sources = new List<string>();
        
        // Extract sources from agent results
        foreach (var result in agentResults)
        {            if (result is HadithResponse hadithResp && hadithResp.Sources != null)
                sources.AddRange(hadithResp.Sources);
            if (result is VerseResponse verseResp && verseResp.Sources != null)
                sources.AddRange(verseResp.Sources);
            if (result is FiqhResponse fiqhResp && fiqhResp.Sources != null)
                sources.AddRange(fiqhResp.Sources);
        }
        
        // Extract sources from search results
        sources.AddRange(searchResults.Select(r => $"{r.Source}: {r.Title}"));
        
        return sources.Distinct().ToList();
    }

    private async Task<List<string>> GenerateRelatedQuestions(string originalQuery, QueryAnalysisResult analysis, string language)
    {
        var questionsPrompt = $@"
Based on this Islamic query: ""{originalQuery}""
Query type: {string.Join(", ", analysis.QueryTypes)}
User intent: {analysis.PrimaryIntent}
Complexity: {analysis.ComplexityLevel}

Generate 3 related questions a Muslim might ask next that build upon this query.
Consider the user's apparent level of knowledge based on complexity: {analysis.ComplexityLevel}
Respond with JSON array only: [""question1"", ""question2"", ""question3""]
Questions should be in {language} and appropriate for the user's knowledge level.";

        try
        {
            var response = await _ollama.GenerateResponseAsync(questionsPrompt, DEEPSEEK_MODEL);
            return JsonSerializer.Deserialize<List<string>>(response) ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("⚠️ Failed to generate related questions: {Error}", ex.Message);
            return new List<string>();
        }
    }

    private IntelligentResponse CreateErrorResponse(string error, string language)
    {
        var message = language.StartsWith("fr") ? 
            "Désolé, une erreur s'est produite lors du traitement de votre question." :
            language.StartsWith("ar") ?
            "عذراً، حدث خطأ أثناء معالجة استفسارك." :
            "Sorry, an error occurred while processing your question.";
            
        return new IntelligentResponse
        {
            Answer = $"{message} Error: {error}",
            IsSuccessful = false,
            Sources = new List<string>(),
            WebSearchResults = new List<WebSearchResult>(),
            RelatedQuestions = new List<string>()
        };
    }
}
