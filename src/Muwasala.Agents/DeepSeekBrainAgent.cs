using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Muwasala.Core.Models;
using Muwasala.KnowledgeBase.Services;
using System.Text;
using System.Text.Json;

namespace Muwasala.Agents;

public class DeepSeekBrainAgent
{
    private readonly IConfiguration _configuration;    private readonly ILogger<DeepSeekBrainAgent> _logger;
    private readonly HttpClient _httpClient;

    public DeepSeekBrainAgent(
        IConfiguration configuration,
        ILogger<DeepSeekBrainAgent> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }    public async Task<IslamicKnowledgeResponse> ProcessQueryAsync(string query, string? language = "en")
    {
        try
        {
            _logger.LogInformation("Processing query with DeepSeek Brain Agent: {Query}", query);

            // Create a simple analysis object (mock for now)
            var mockAnalysis = new { Query = query, Language = language ?? "en" };
            
            // Create empty search results for now
            var searchResults = new List<WebSearchResult>();

            // Step 1: Synthesize response using DeepSeek
            var response = await SynthesizeResponse(query, mockAnalysis, searchResults, language ?? "en");
            _logger.LogInformation("Response synthesis completed");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query with DeepSeek Brain Agent");
            return CreateErrorResponse(ex.Message, language ?? "en");
        }
    }private async Task<IslamicKnowledgeResponse> SynthesizeResponse(
        string query, 
        object analysis, 
        List<WebSearchResult> searchResults, 
        string language)
    {
        try
        {
            var prompt = BuildSynthesisPrompt(query, analysis, searchResults, language);
            var deepSeekResponse = await CallDeepSeekAPI(prompt);

            // Extract sources from search results and analysis
            var quranRefs = ExtractQuranReferences(searchResults, analysis);
            var hadithRefs = ExtractHadithReferences(searchResults, analysis);
            var scholarlyOpinions = ExtractScholarlyOpinions(searchResults);            return new IslamicKnowledgeResponse(
                query,
                deepSeekResponse,
                quranRefs,
                hadithRefs,
                scholarlyOpinions,
                CalculateConfidence(analysis, searchResults),
                "DeepSeek AI",
                language
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synthesizing response");
            throw;
        }
    }    private string BuildSynthesisPrompt(string query, object analysis, List<WebSearchResult> searchResults, string language)
    {
        var promptBuilder = new StringBuilder();
        
        promptBuilder.AppendLine($"You are an expert Islamic scholar providing comprehensive answers in {language}.");
        promptBuilder.AppendLine($"Query: {query}");
        promptBuilder.AppendLine($"Please provide a detailed Islamic response based on the search results.");
        
        if (searchResults.Any())
        {
            promptBuilder.AppendLine("\nRelevant Sources:");
            foreach (var result in searchResults.Take(5))
            {
                promptBuilder.AppendLine($"- {result.Title}: {result.Snippet}");
                if (!string.IsNullOrEmpty(result.Source))
                {
                    promptBuilder.AppendLine($"  Source: {result.Source}");
                }
            }
        }

        promptBuilder.AppendLine("\nProvide a comprehensive, accurate, and well-structured Islamic response.");
        promptBuilder.AppendLine("Include relevant Quranic verses and authentic Hadith when applicable.");
        promptBuilder.AppendLine("Ensure the response is scholarly and appropriate.");

        return promptBuilder.ToString();
    }

    private async Task<string> CallDeepSeekAPI(string prompt)
    {
        try
        {
            var apiKey = _configuration["DeepSeek:ApiKey"];
            var apiUrl = _configuration["DeepSeek:ApiUrl"] ?? "https://api.deepseek.com/v1/chat/completions";

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("DeepSeek API key not configured, using fallback response");
                return GenerateFallbackResponse(prompt);
            }

            var requestBody = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new { role = "system", content = "You are an expert Islamic scholar providing comprehensive and accurate Islamic knowledge." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 2000
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return responseData.GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Unable to generate response";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling DeepSeek API");
            return GenerateFallbackResponse(prompt);
        }
    }

    private string GenerateFallbackResponse(string prompt)
    {
        return "I apologize, but I'm currently unable to connect to the advanced AI system. However, I can provide basic Islamic guidance. Please consult with a qualified Islamic scholar for detailed religious matters.";
    }    private List<string> ExtractQuranReferences(List<WebSearchResult> searchResults, object analysis)
    {
        var quranRefs = new List<string>();

        // Extract from search results
        foreach (var result in searchResults)
        {
            if (result.Source.Contains("quran", StringComparison.OrdinalIgnoreCase) ||
                result.Title.Contains("quran", StringComparison.OrdinalIgnoreCase))
            {
                quranRefs.Add($"{result.Title} - {result.Source}");
            }
        }

        // Add some generic Quran references if none found
        if (!quranRefs.Any())
        {
            quranRefs.Add("Quran - General Reference");
        }

        return quranRefs.Distinct().Take(5).ToList();
    }    private List<string> ExtractHadithReferences(List<WebSearchResult> searchResults, object analysis)
    {
        var hadithRefs = new List<string>();

        // Extract from search results
        foreach (var result in searchResults)
        {
            if (result.Source.Contains("hadith", StringComparison.OrdinalIgnoreCase) ||
                result.Source.Contains("sunnah", StringComparison.OrdinalIgnoreCase) ||
                result.Title.Contains("hadith", StringComparison.OrdinalIgnoreCase))
            {
                hadithRefs.Add($"{result.Title} - {result.Source}");
            }
        }

        // Add some generic Hadith references if none found
        if (!hadithRefs.Any())
        {
            hadithRefs.Add("Hadith - General Reference");
        }

        return hadithRefs.Distinct().Take(5).ToList();
    }

    private List<string> ExtractScholarlyOpinions(List<WebSearchResult> searchResults)
    {
        var opinions = new List<string>();

        foreach (var result in searchResults)
        {
            if (result.Source.Contains("scholar", StringComparison.OrdinalIgnoreCase) ||
                result.Source.Contains("imam", StringComparison.OrdinalIgnoreCase) ||
                result.Author != null)
            {
                var opinion = result.Author != null 
                    ? $"{result.Author}: {result.Snippet}" 
                    : $"{result.Source}: {result.Snippet}";
                opinions.Add(opinion);
            }
        }

        return opinions.Distinct().Take(3).ToList();
    }    private List<string> GenerateRelatedQuestions(object analysis, string language)
    {
        // Simplified implementation - return basic related questions
        var questions = new List<string>();
        
        if (language == "fr")
        {
            questions.Add("Qu'est-ce que l'Islam enseigne à ce sujet?");
            questions.Add("Y a-t-il des preuves dans le Coran?");
            questions.Add("Quelle est la perspective des érudits?");
        }
        else if (language == "ar")
        {
            questions.Add("ما يعلمه الإسلام حول هذا الموضوع؟");
            questions.Add("هل هناك أدلة في القرآن؟");
            questions.Add("ما هو رأي العلماء؟");
        }
        else
        {
            questions.Add("What does Islam teach about this topic?");
            questions.Add("Are there evidences in the Quran?");
            questions.Add("What is the scholarly perspective?");
        }
        
        return questions;
    }    private double CalculateConfidence(object analysis, List<WebSearchResult> searchResults)
    {
        double confidence = 0.5; // Base confidence

        // Increase confidence based on search results
        if (searchResults.Any())
        {
            confidence += Math.Min(searchResults.Count * 0.1, 0.3);
            confidence += searchResults.Average(r => r.RelevanceScore) * 0.2;
        }

        // Simple confidence adjustment
        confidence += 0.1; // Small boost for having analysis

        return Math.Max(0.0, Math.Min(1.0, confidence));
    }private IslamicKnowledgeResponse CreateErrorResponse(string errorMessage, string language)
    {
        string message = language switch
        {
            "fr" => "Je m'excuse, mais j'ai rencontré une erreur lors du traitement de votre question. Veuillez réessayer.",
            "ar" => "أعتذر، لقد واجهت خطأ أثناء معالجة سؤالك. يرجى المحاولة مرة أخرى.",
            _ => "I apologize, but I encountered an error while processing your question. Please try again."
        };

        return new IslamicKnowledgeResponse(
            "Error Query",
            message,
            new List<string>(),
            new List<string>(),
            new List<string>(),
            0.0,
            "DeepSeek AI",
            language
        );
    }
}