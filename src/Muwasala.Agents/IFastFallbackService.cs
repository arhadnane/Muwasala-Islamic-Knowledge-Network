using Muwasala.Core.Models;

namespace Muwasala.Agents;

public interface IFastFallbackService
{
    Task<List<WebSearchResult>> GetFastResultsAsync(QueryAnalysisResult queryAnalysis);
    Task<List<GlobalSearchResult>> GetFastSearchResultsAsync(string query, string language = "en");
}
