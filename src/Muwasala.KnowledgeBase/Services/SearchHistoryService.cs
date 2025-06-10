using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muwasala.Core.Models;
using Muwasala.KnowledgeBase.Data;
using Muwasala.KnowledgeBase.Data.Models;
using Muwasala.KnowledgeBase.Services;

namespace Muwasala.KnowledgeBase.Services
{
    /// <summary>
    /// Implementation of search history service for tracking and analyzing search patterns
    /// </summary>
    public class SearchHistoryService : ISearchHistoryService
    {
        private readonly IslamicKnowledgeDbContext _dbContext;
        private readonly ILogger<SearchHistoryService> _logger;

        public SearchHistoryService(
            IslamicKnowledgeDbContext dbContext,
            ILogger<SearchHistoryService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Saves a search and its results to the database
        /// </summary>
        public async Task<int> SaveSearchAsync(string query, string searchMode, string language,
            List<GlobalSearchResult> results, TimeSpan searchDuration,
            string[]? selectedContentTypes = null, string? userIdentifier = null,
            string? searchContext = null)
        {
            try
            {
                var searchHistory = new SearchHistoryEntity
                {
                    SearchQuery = query,
                    SearchMode = searchMode,
                    Language = language,
                    SearchDateTime = DateTime.UtcNow,
                    SelectedContentTypes = selectedContentTypes != null ? string.Join(",", selectedContentTypes) : null,
                    ResultsCount = results.Count,
                    SearchDurationMs = searchDuration.TotalMilliseconds,
                    UserIdentifier = userIdentifier,
                    SearchContext = searchContext,
                    HasAIResponse = false
                };

                _dbContext.SearchHistory.Add(searchHistory);
                await _dbContext.SaveChangesAsync();

                // Save individual results
                await SaveSearchResults(searchHistory.Id, results);

                // Update analytics
                await UpdateSearchAnalytics(searchMode, language, searchDuration, results.Count);

                _logger.LogInformation("Search saved successfully: Query='{Query}', Mode='{Mode}', Results={Count}",
                    query, searchMode, results.Count);

                return searchHistory.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving search history for query: {Query}", query);
                throw;
            }
        }

        /// <summary>
        /// Saves a search with AI response to the database
        /// </summary>
        public async Task<int> SaveSearchWithAIAsync(string query, string searchMode, string language,
            List<GlobalSearchResult> localResults, IslamicKnowledgeResponse? aiResponse,
            List<WebSearchResult>? webResults, TimeSpan searchDuration,
            string[]? selectedContentTypes = null, string? userIdentifier = null,
            string? searchContext = null)
        {
            try
            {
                var searchHistory = new SearchHistoryEntity
                {
                    SearchQuery = query,
                    SearchMode = searchMode,
                    Language = language,
                    SearchDateTime = DateTime.UtcNow,
                    SelectedContentTypes = selectedContentTypes != null ? string.Join(",", selectedContentTypes) : null,
                    ResultsCount = localResults.Count + (webResults?.Count ?? 0),
                    SearchDurationMs = searchDuration.TotalMilliseconds,
                    UserIdentifier = userIdentifier,
                    SearchContext = searchContext,
                    HasAIResponse = aiResponse != null,
                    AIResponseSummary = aiResponse?.Response,
                    AIConfidenceScore = aiResponse?.ConfidenceScore,
                    QuranReferencesFromAI = aiResponse?.QuranReferences != null ? string.Join(";", aiResponse.QuranReferences) : null,
                    HadithReferencesFromAI = aiResponse?.HadithReferences != null ? string.Join(";", aiResponse.HadithReferences) : null
                };

                _dbContext.SearchHistory.Add(searchHistory);
                await _dbContext.SaveChangesAsync();

                // Save local search results
                await SaveSearchResults(searchHistory.Id, localResults);

                // Save web search results if available
                if (webResults != null && webResults.Any())
                {
                    await SaveWebSearchResults(searchHistory.Id, webResults);
                }

                // Update analytics
                await UpdateSearchAnalytics(searchMode, language, searchDuration, searchHistory.ResultsCount);

                _logger.LogInformation("Search with AI saved successfully: Query='{Query}', Mode='{Mode}', LocalResults={LocalCount}, WebResults={WebCount}, HasAI={HasAI}",
                    query, searchMode, localResults.Count, webResults?.Count ?? 0, aiResponse != null);

                return searchHistory.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving search with AI for query: {Query}", query);
                throw;
            }
        }

        /// <summary>
        /// Gets recent search history
        /// </summary>
        public async Task<List<SearchHistoryEntry>> GetRecentSearchesAsync(int maxResults = 50, string? userIdentifier = null)
        {
            try
            {
                var query = _dbContext.SearchHistory.AsQueryable();

                if (!string.IsNullOrEmpty(userIdentifier))
                {
                    query = query.Where(h => h.UserIdentifier == userIdentifier);
                }

                var histories = await query
                    .OrderByDescending(h => h.SearchDateTime)
                    .Take(maxResults)
                    .Select(h => new SearchHistoryEntry
                    {
                        Id = h.Id,
                        SearchQuery = h.SearchQuery,
                        SearchMode = h.SearchMode,
                        Language = h.Language,
                        SearchDateTime = h.SearchDateTime,
                        SelectedContentTypes = !string.IsNullOrEmpty(h.SelectedContentTypes) 
                            ? h.SelectedContentTypes.Split(',', StringSplitOptions.RemoveEmptyEntries) 
                            : null,
                        ResultsCount = h.ResultsCount,
                        SearchDurationMs = h.SearchDurationMs,
                        UserIdentifier = h.UserIdentifier,                        SearchContext = h.SearchContext
                    })
                    .ToListAsync();

                return histories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent searches");
                return new List<SearchHistoryEntry>();
            }
        }

        /// <summary>
        /// Gets search history for a specific query
        /// </summary>
        public async Task<List<SearchHistoryEntry>> GetSearchHistoryForQueryAsync(string query, int maxResults = 10)
        {
            try
            {
                var histories = await _dbContext.SearchHistory
                    .Where(h => h.SearchQuery.ToLower().Contains(query.ToLower()))
                    .OrderByDescending(h => h.SearchDateTime)
                    .Take(maxResults)
                    .Select(h => new SearchHistoryEntry
                    {
                        Id = h.Id,
                        SearchQuery = h.SearchQuery,
                        SearchMode = h.SearchMode,
                        Language = h.Language,
                        SearchDateTime = h.SearchDateTime,
                        SelectedContentTypes = !string.IsNullOrEmpty(h.SelectedContentTypes) 
                            ? h.SelectedContentTypes.Split(',', StringSplitOptions.RemoveEmptyEntries) 
                            : null,
                        ResultsCount = h.ResultsCount,
                        SearchDurationMs = h.SearchDurationMs,
                        UserIdentifier = h.UserIdentifier,                        SearchContext = h.SearchContext
                    })
                    .ToListAsync();

                return histories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving search history for query: {Query}", query);
                return new List<SearchHistoryEntry>();
            }
        }

        /// <summary>
        /// Gets most popular search terms
        /// </summary>
        public async Task<List<PopularSearchTerm>> GetPopularSearchTermsAsync(int maxResults = 20, TimeSpan? timeRange = null)
        {
            try
            {
                var query = _dbContext.SearchHistory.AsQueryable();

                if (timeRange.HasValue)
                {
                    var cutoffDate = DateTime.UtcNow.Subtract(timeRange.Value);
                    query = query.Where(h => h.SearchDateTime >= cutoffDate);
                }

                var popularTerms = await query
                    .GroupBy(h => h.SearchQuery.ToLower())
                    .Select(g => new PopularSearchTerm
                    {
                        SearchTerm = g.Key,
                        SearchCount = g.Count(),
                        LastSearched = g.Max(h => h.SearchDateTime),
                        AverageResultsCount = g.Average(h => h.ResultsCount),
                        PreferredSearchMode = g.GroupBy(h => h.SearchMode)
                            .OrderByDescending(mg => mg.Count())
                            .First().Key
                    })
                    .OrderByDescending(t => t.SearchCount)
                    .Take(maxResults)
                    .ToListAsync();

                return popularTerms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving popular search terms");
                return new List<PopularSearchTerm>();
            }
        }

        /// <summary>
        /// Gets search analytics for a specific period
        /// </summary>
        public async Task<SearchAnalytics> GetSearchAnalyticsAsync(DateTime fromDate, DateTime toDate, string? language = null)
        {
            try
            {
                var query = _dbContext.SearchHistory
                    .Where(h => h.SearchDateTime >= fromDate && h.SearchDateTime <= toDate);

                if (!string.IsNullOrEmpty(language))
                {
                    query = query.Where(h => h.Language == language);
                }

                var searches = await query.ToListAsync();

                if (!searches.Any())
                {
                    return new SearchAnalytics
                    {
                        PeriodStart = fromDate,
                        PeriodEnd = toDate
                    };
                }

                var uniqueQueries = searches.Select(s => s.SearchQuery.ToLower()).Distinct().Count();
                var searchModeDistribution = searches.GroupBy(s => s.SearchMode)
                    .ToDictionary(g => g.Key, g => g.Count());
                var languageDistribution = searches.GroupBy(s => s.Language)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Get content type distribution from results
                var contentTypeDistribution = new Dictionary<string, int>();
                var searchResults = await _dbContext.SearchResultHistory
                    .Where(r => searches.Select(s => s.Id).Contains(r.SearchHistoryId))
                    .ToListAsync();

                foreach (var result in searchResults)
                {
                    if (!string.IsNullOrEmpty(result.ContentType))
                    {
                        contentTypeDistribution[result.ContentType] = 
                            contentTypeDistribution.GetValueOrDefault(result.ContentType, 0) + 1;
                    }
                }

                var topQueries = searches.GroupBy(s => s.SearchQuery.ToLower())
                    .Select(g => new PopularSearchTerm
                    {
                        SearchTerm = g.Key,
                        SearchCount = g.Count(),
                        LastSearched = g.Max(s => s.SearchDateTime),
                        AverageResultsCount = g.Average(s => s.ResultsCount),
                        PreferredSearchMode = g.GroupBy(s => s.SearchMode)
                            .OrderByDescending(mg => mg.Count())
                            .First().Key
                    })
                    .OrderByDescending(t => t.SearchCount)
                    .Take(10)
                    .ToList();

                return new SearchAnalytics
                {
                    TotalSearches = searches.Count,
                    UniqueQueries = uniqueQueries,
                    AverageResultsCount = searches.Average(s => s.ResultsCount),
                    AverageSearchDuration = searches.Average(s => s.SearchDurationMs),
                    SearchModeDistribution = searchModeDistribution,
                    ContentTypeDistribution = contentTypeDistribution,
                    LanguageDistribution = languageDistribution,
                    TopQueries = topQueries,
                    PeriodStart = fromDate,
                    PeriodEnd = toDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating search analytics");
                return new SearchAnalytics
                {
                    PeriodStart = fromDate,
                    PeriodEnd = toDate
                };
            }
        }

        /// <summary>
        /// Deletes old search history
        /// </summary>
        public async Task<int> CleanupOldSearchHistoryAsync(TimeSpan retentionPeriod)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.Subtract(retentionPeriod);
                
                var oldSearches = await _dbContext.SearchHistory
                    .Where(h => h.SearchDateTime < cutoffDate)
                    .ToListAsync();

                if (oldSearches.Any())
                {
                    // Delete related search results first
                    var searchIds = oldSearches.Select(s => s.Id).ToList();
                    var oldResults = await _dbContext.SearchResultHistory
                        .Where(r => searchIds.Contains(r.SearchHistoryId))
                        .ToListAsync();

                    _dbContext.SearchResultHistory.RemoveRange(oldResults);
                    _dbContext.SearchHistory.RemoveRange(oldSearches);

                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Cleaned up {Count} old search records older than {Date}",
                        oldSearches.Count, cutoffDate);

                    return oldSearches.Count;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old search history");
                return 0;
            }
        }

        /// <summary>
        /// Gets search suggestions based on history
        /// </summary>
        public async Task<List<string>> GetHistoryBasedSuggestionsAsync(string partialQuery, int maxResults = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(partialQuery) || partialQuery.Length < 2)
                {
                    return new List<string>();
                }

                var lowerPartial = partialQuery.ToLower();

                var suggestions = await _dbContext.SearchHistory
                    .Where(h => h.SearchQuery.ToLower().Contains(lowerPartial))
                    .GroupBy(h => h.SearchQuery.ToLower())
                    .Select(g => new
                    {
                        Query = g.Key,
                        Count = g.Count(),
                        LastSearch = g.Max(h => h.SearchDateTime)
                    })
                    .OrderByDescending(s => s.Count)
                    .ThenByDescending(s => s.LastSearch)
                    .Take(maxResults)
                    .Select(s => s.Query)
                    .ToListAsync();

                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history-based suggestions for: {PartialQuery}", partialQuery);
                return new List<string>();
            }
        }

        #region Private Helper Methods

        private async Task SaveSearchResults(int searchHistoryId, List<GlobalSearchResult> results)
        {            var resultEntities = results.Select(result => new SearchResultHistoryEntity            {
                SearchHistoryId = searchHistoryId,
                ContentType = result.Type.ToString(),
                Title = result.Title,
                Content = result.Content,
                ArabicText = result.ArabicText,
                Source = result.Source,
                Reference = result.Reference,
                RelevanceScore = result.RelevanceScore,
                ResultPosition = results.IndexOf(result) + 1,
                AdditionalMetadata = result.Metadata != null ? System.Text.Json.JsonSerializer.Serialize(result.Metadata) : null
            }).ToList();

            _dbContext.SearchResultHistory.AddRange(resultEntities);
            await _dbContext.SaveChangesAsync();
        }

        private async Task SaveWebSearchResults(int searchHistoryId, List<WebSearchResult> webResults)
        {
            var webResultEntities = webResults.Select(result => new SearchResultHistoryEntity
            {
                SearchHistoryId = searchHistoryId,                ContentType = "WebResult",
                Title = result.Title,
                Content = result.Snippet,
                Source = result.Source,
                Reference = result.Url,                RelevanceScore = result.RelevanceScore,
                ResultPosition = webResults.IndexOf(result) + 1,
                AdditionalMetadata = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Url = result.Url,
                    PublishedDate = result.PublishedDate,
                    Language = result.Language
                })
            }).ToList();

            _dbContext.SearchResultHistory.AddRange(webResultEntities);
            await _dbContext.SaveChangesAsync();
        }

        private async Task UpdateSearchAnalytics(string searchMode, string language, TimeSpan duration, int resultsCount)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var analytics = await _dbContext.SearchAnalytics
                    .FirstOrDefaultAsync(a => a.Date == today && a.SearchMode == searchMode && a.Language == language);

                if (analytics == null)
                {
                    analytics = new SearchAnalyticsEntity
                    {
                        Date = today,
                        SearchMode = searchMode,
                        Language = language,
                        TotalSearches = 1,
                        TotalResults = resultsCount,
                        TotalDurationMs = duration.TotalMilliseconds,
                        AverageResultsPerSearch = resultsCount,
                        AverageDurationMs = duration.TotalMilliseconds
                    };
                    _dbContext.SearchAnalytics.Add(analytics);
                }
                else
                {
                    analytics.TotalSearches++;
                    analytics.TotalResults += resultsCount;
                    analytics.TotalDurationMs += duration.TotalMilliseconds;
                    analytics.AverageResultsPerSearch = (double)analytics.TotalResults / analytics.TotalSearches;
                    analytics.AverageDurationMs = analytics.TotalDurationMs / analytics.TotalSearches;
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error updating search analytics");
                // Don't throw here as this is not critical
            }
        }

        #endregion
    }
}
