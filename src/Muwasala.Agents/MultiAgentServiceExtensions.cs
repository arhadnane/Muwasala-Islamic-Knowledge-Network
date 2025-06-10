using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muwasala.Core.Services;
using Muwasala.Core.Models;
using Muwasala.KnowledgeBase.Services;

namespace Muwasala.Agents;

/// <summary>
/// Extension methods for registering the multi-agent system services
/// </summary>
public static class MultiAgentServiceExtensions
{
    /// <summary>
    /// Register all multi-agent system services
    /// </summary>
    public static IServiceCollection AddMultiAgentSystem(this IServiceCollection services)
    {        // Register core infrastructure services
        services.AddSingleton<ICircuitBreakerService, CircuitBreakerService>();
        // Note: FastFallbackService temporarily disabled due to compilation issues
        // Will be re-enabled after resolving the type recognition problem
        
        // Register HttpClient for web crawling
        services.AddHttpClient<WebCrawlerAgent>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", 
                "Muwasala Islamic Knowledge Network Bot 1.0 (+https://muwasala.com)");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register HttpClient for RealTimeSearchAgent
        services.AddHttpClient<RealTimeSearchAgent>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", 
                "Muwasala Islamic Knowledge Network Bot 1.0 (+https://muwasala.com)");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // Register specialized agents
        services.AddScoped<WebCrawlerAgent>();
        services.AddScoped<HadithVerifierAgent>();
        services.AddScoped<FiqhAdvisorAgent>();
        services.AddScoped<QuranNavigatorAgent>();
        services.AddScoped<DuaCompanionAgent>();
        services.AddScoped<SirahScholarAgent>();
        services.AddScoped<TajweedTutorAgent>();

        // Register advanced AI agents
        services.AddScoped<QueryAnalysisAgent>();
        services.AddScoped<RealTimeSearchAgent>();
        services.AddScoped<ResponseQualityAgent>();

        // Register the central Enhanced DeepSeek Brain
        services.AddScoped<EnhancedDeepSeekBrainAgent>();
        services.AddScoped<DeepSeekBrainAgent>(); // Keep original for fallback compatibility        // Register the enhanced hybrid search service
        services.AddScoped<IEnhancedHybridSearchService, EnhancedHybridSearchService>();

        return services;
    }
}

/// <summary>
/// Enhanced hybrid search service implementation
/// </summary>
public class EnhancedHybridSearchService : IEnhancedHybridSearchService
{
    private readonly EnhancedDeepSeekBrainAgent _brainAgent;
    private readonly IIntelligentSearchService _fallbackService;
    private readonly IFastFallbackService? _fastFallbackService;
    private readonly ICircuitBreakerService _circuitBreaker;
    private readonly ILogger<EnhancedHybridSearchService> _logger;

    public EnhancedHybridSearchService(
        EnhancedDeepSeekBrainAgent brainAgent,
        IIntelligentSearchService fallbackService,
        ICircuitBreakerService circuitBreaker,
        ILogger<EnhancedHybridSearchService> logger,
        IFastFallbackService? fastFallbackService = null)
    {
        _brainAgent = brainAgent;
        _fallbackService = fallbackService;
        _fastFallbackService = fastFallbackService;
        _circuitBreaker = circuitBreaker;
        _logger = logger;
    }    public async Task<HybridSearchResponse> SearchAsync(string query, string language = "en")
    {
        _logger.LogInformation("🚀 Enhanced hybrid search with performance optimization: {Query}", query);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Use circuit breaker for the enhanced brain agent
            var aiResponse = await _circuitBreaker.ExecuteAsync(
                "HybridSearch",
                async () => await _brainAgent.ProcessQueryAsync(query, language),
                new IntelligentResponse 
                { 
                    Answer = "Service temporarily unavailable",
                    IsSuccessful = false,
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
                });

            if (aiResponse.IsSuccessful)
            {
                return new HybridSearchResponse
                {
                    AIResponse = aiResponse,
                    WebResults = aiResponse.WebSearchResults ?? new List<WebSearchResult>(),
                    SearchSuggestions = aiResponse.RelatedQuestions ?? new List<string>(),
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
                };
            }
            else
            {
                _logger.LogWarning("AI search failed, using fallback search");
                return await GetFastFallbackResponse(query, language, stopwatch);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in hybrid search");
            return await GetFastFallbackResponse(query, language, stopwatch);
        }
    }    private async Task<HybridSearchResponse> GetFastFallbackResponse(string query, string language, System.Diagnostics.Stopwatch stopwatch)
    {
        try
        {
            // Try fallback service first using PerformHybridSearchAsync (correct method from interface)
            var fallbackResponse = await _fallbackService.PerformHybridSearchAsync(query, language);            // Convert the response types properly
            var webSearchResults = fallbackResponse.WebResults?.Select(w => new WebSearchResult
            {
                Title = w.Title,
                Snippet = w.Content,
                Url = w.Url,
                Source = w.Source,
                RelevanceScore = w.RelevanceScore,
                AuthenticityScore = 0.8f,
                Language = w.Language,
                PublishedDate = w.LastUpdated
            }).ToList() ?? new List<WebSearchResult>();

            // Create the AI response properly
            IntelligentResponse aiResponse;
            if (fallbackResponse.AIResponse != null)
            {
                aiResponse = new IntelligentResponse
                {
                    Answer = fallbackResponse.AIResponse.Response,
                    IsSuccessful = true,
                    Sources = new List<string> { fallbackResponse.AIResponse.Source },
                    WebSearchResults = webSearchResults,
                    RelatedQuestions = fallbackResponse.SearchSuggestions?.ToList() ?? new List<string>(),
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
                };
            }
            else
            {
                aiResponse = new IntelligentResponse
                {
                    Answer = $"I understand you're asking about '{query}'. Please try rephrasing your question.",
                    IsSuccessful = false,
                    Sources = new List<string>(),
                    WebSearchResults = new List<WebSearchResult>(),
                    RelatedQuestions = new List<string>(),
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
                };
            }

            return new HybridSearchResponse
            {
                AIResponse = aiResponse,
                WebResults = webSearchResults,
                SearchSuggestions = fallbackResponse.SearchSuggestions ?? new List<string>(),
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
        }        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallback service also failed");
            
            // Return fast fallback results if available
            var fastResults = new List<GlobalSearchResult>();
            if (_fastFallbackService != null)
            {
                fastResults = await _fastFallbackService.GetFastSearchResultsAsync(query, language);
            }
            
            return new HybridSearchResponse
            {
                AIResponse = new IntelligentResponse
                {
                    Answer = $"I understand you're asking about '{query}'. {fastResults.FirstOrDefault()?.Content ?? "Please try rephrasing your question."}",
                    IsSuccessful = false,
                    Sources = new List<string>(),
                    WebSearchResults = new List<WebSearchResult>(),
                    RelatedQuestions = new List<string>(),
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
                },
                WebResults = new List<WebSearchResult>(),
                SearchSuggestions = new List<string> { "Try being more specific", "Search for Quran verses", "Look for Hadith" },
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
    }
}
