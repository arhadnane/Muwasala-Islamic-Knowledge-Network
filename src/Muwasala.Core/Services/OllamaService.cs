using System.Text.Json;

namespace Muwasala.Core.Services;

/// <summary>
/// Interface for Ollama AI model interaction
/// </summary>
public interface IOllamaService
{
    Task<string> GenerateResponseAsync(string model, string prompt, double temperature = 0.1);
    Task<T> GenerateStructuredResponseAsync<T>(string model, string prompt, double temperature = 0.1);
    Task<bool> IsModelAvailableAsync(string model);
    Task<List<string>> GetAvailableModelsAsync();
}

/// <summary>
/// Ollama service implementation for local AI models
/// </summary>
public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _baseUrl;

    public OllamaService(HttpClient httpClient, string baseUrl = "http://localhost:11434")
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<string> GenerateResponseAsync(string model, string prompt, double temperature = 0.1)
    {
        var request = new
        {
            model = model,
            prompt = prompt,
            stream = false,
            options = new
            {
                temperature = temperature,
                top_p = 0.9,
                top_k = 40
            }
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OllamaResponse>(responseJson, _jsonOptions);
        
        return result?.Response ?? string.Empty;
    }    public async Task<T> GenerateStructuredResponseAsync<T>(string model, string prompt, double temperature = 0.1)
    {
        var enhancedPrompt = $"{prompt}\n\nPlease respond with valid JSON that matches this structure. Do not include any text outside of the JSON response.";
        var response = await GenerateResponseAsync(model, enhancedPrompt, temperature);
        
        // Clean the response - remove markdown code blocks if present
        var cleanedResponse = response.Trim();
        if (cleanedResponse.StartsWith("```json"))
        {
            cleanedResponse = cleanedResponse.Substring(7); // Remove ```json
        }
        if (cleanedResponse.StartsWith("```"))
        {
            cleanedResponse = cleanedResponse.Substring(3); // Remove ```
        }        if (cleanedResponse.EndsWith("```"))
        {
            cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3); // Remove ending ```
        }
        cleanedResponse = cleanedResponse.Trim();
        
        // Fix common JSON formatting issues from AI models
        cleanedResponse = FixJsonFormatting(cleanedResponse);
        
        try
        {
            return JsonSerializer.Deserialize<T>(cleanedResponse, _jsonOptions) 
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON response: {cleanedResponse}", ex);
        }
    }

    public async Task<bool> IsModelAvailableAsync(string model)
    {
        try
        {
            var models = await GetAvailableModelsAsync();
            return models.Contains(model);
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<string>> GetAvailableModelsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ModelsResponse>(json, _jsonOptions);
            
            return result?.Models?.Select(m => m.Name).ToList() ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private record OllamaResponse(string Response, bool Done);    private record ModelsResponse(List<ModelInfo> Models);
    private record ModelInfo(string Name, string Model, string Modified_at, long Size, string Digest);

    /// <summary>
    /// Fix common JSON formatting issues from AI model responses
    /// </summary>
    private static string FixJsonFormatting(string json)
    {
        if (string.IsNullOrEmpty(json)) return json;
        
        // Fix unescaped newlines in JSON strings
        var lines = json.Split('\n');
        var fixedLines = new List<string>();
        bool inStringValue = false;
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            
            // Simple heuristic: if line starts with spaces and doesn't contain : or {,},[,]
            // and we're inside a string value, it's likely a continuation
            if (i > 0 && inStringValue && 
                line.Trim().Length > 0 && 
                !line.Contains(':') && 
                !line.Trim().StartsWith('{') && 
                !line.Trim().StartsWith('}') &&
                !line.Trim().StartsWith('[') && 
                !line.Trim().StartsWith(']') &&
                !line.Trim().EndsWith(','))
            {
                // This is likely a continuation of a string value - escape it
                var lastLineIndex = fixedLines.Count - 1;
                if (lastLineIndex >= 0)
                {
                    // Remove the closing quote from previous line and add continuation
                    var lastLine = fixedLines[lastLineIndex];
                    if (lastLine.TrimEnd().EndsWith('"'))
                    {
                        fixedLines[lastLineIndex] = lastLine.TrimEnd().Substring(0, lastLine.TrimEnd().Length - 1);
                        fixedLines[lastLineIndex] += "\\n" + line.Trim() + '"';
                        continue;
                    }
                }
            }
            
            fixedLines.Add(line);
            
            // Track if we're in a string value (simple heuristic)
            if (line.Contains(':') && line.Contains('"'))
            {
                inStringValue = true;
            }
            if (line.TrimEnd().EndsWith(',') || line.TrimEnd().EndsWith('}'))
            {
                inStringValue = false;
            }
        }
        
        return string.Join('\n', fixedLines);
    }
}
