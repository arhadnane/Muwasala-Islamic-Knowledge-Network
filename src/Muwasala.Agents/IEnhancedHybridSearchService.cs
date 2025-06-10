using Muwasala.Core.Models;

namespace Muwasala.Agents;

/// <summary>
/// Enhanced hybrid search service that uses the multi-agent system
/// </summary>
public interface IEnhancedHybridSearchService
{
    Task<HybridSearchResponse> SearchAsync(string query, string language = "en");
}
