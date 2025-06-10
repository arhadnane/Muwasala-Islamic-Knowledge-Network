using Muwasala.Core.Models;
using Microsoft.Extensions.Logging;

namespace Muwasala.Agents;

public class FastFallbackService : IFastFallbackService
{
    private readonly ILogger<FastFallbackService> _logger;
    private readonly Dictionary<string, List<WebSearchResult>> _cachedResults;
    private readonly Dictionary<string, List<GlobalSearchResult>> _fallbackSearchResults;

    public FastFallbackService(ILogger<FastFallbackService> logger)
    {
        _logger = logger;
        _cachedResults = InitializeCachedResults();
        _fallbackSearchResults = InitializeFallbackSearchResults();
    }

    public async Task<List<WebSearchResult>> GetFastResultsAsync(QueryAnalysisResult queryAnalysis)
    {
        var results = new List<WebSearchResult>();

        // Get results based on query type
        foreach (var queryType in queryAnalysis.QueryTypes)
        {
            var key = queryType.ToString().ToLowerInvariant();
            if (_cachedResults.TryGetValue(key, out var cachedResults))
            {
                results.AddRange(cachedResults);
            }
        }

        // Get results based on keywords
        foreach (var keyword in queryAnalysis.SearchKeywords.Take(3))
        {
            var key = keyword.ToLowerInvariant();
            if (_cachedResults.TryGetValue(key, out var keywordResults))
            {
                results.AddRange(keywordResults.Take(2));
            }
        }

        // If no specific matches, provide general Islamic guidance
        if (!results.Any())
        {
            results.AddRange(_cachedResults["general"]);
        }

        _logger.LogInformation("🚀 Fast fallback service provided {Count} results for query: {Query}", 
            results.Count, queryAnalysis.OriginalQuery);

        return await Task.FromResult(results.Take(10).ToList());
    }

    public async Task<List<GlobalSearchResult>> GetFastSearchResultsAsync(string query, string language = "en")
    {
        var queryLower = query.ToLowerInvariant();
        var results = new List<GlobalSearchResult>();

        // Check for common queries
        foreach (var kvp in _fallbackSearchResults)
        {
            if (queryLower.Contains(kvp.Key))
            {
                results.AddRange(kvp.Value);
            }
        }

        // If no matches, return general results
        if (!results.Any() && _fallbackSearchResults.TryGetValue("general", out var generalResults))
        {
            results.AddRange(generalResults);
        }

        _logger.LogInformation("🚀 Fast fallback provided {Count} search results for: {Query}", 
            results.Count, query);

        return await Task.FromResult(results.Take(5).ToList());
    }

    private Dictionary<string, List<WebSearchResult>> InitializeCachedResults()
    {
        return new Dictionary<string, List<WebSearchResult>>
        {
            ["prayer"] = new()
            {
                new WebSearchResult
                {
                    Title = "The Five Daily Prayers in Islam",
                    Snippet = "The five daily prayers (Salat) are fundamental acts of worship in Islam: Fajr (dawn), Dhuhr (midday), Asr (afternoon), Maghrib (sunset), and Isha (night).",
                    Url = "https://islamqa.info/prayers",
                    Source = "IslamQA",
                    RelevanceScore = 0.95,
                    PublishedDate = DateTime.UtcNow.AddDays(-30)
                },
                new WebSearchResult
                {
                    Title = "How to Perform Prayer (Salat)",
                    Snippet = "Learn the proper way to perform Islamic prayer including ablution (wudu), facing the Qibla, and the correct recitations.",
                    Url = "https://sunnah.com/prayer-guide",
                    Source = "Sunnah.com",
                    RelevanceScore = 0.92,
                    PublishedDate = DateTime.UtcNow.AddDays(-15)
                }
            },
            ["quran"] = new()
            {
                new WebSearchResult
                {
                    Title = "Understanding the Holy Quran",
                    Snippet = "The Quran is the holy book of Islam, revealed to Prophet Muhammad (peace be upon him) over 23 years. It contains 114 chapters (Surahs) and is the final revelation from Allah.",
                    Url = "https://quran.com/about",
                    Source = "Quran.com",
                    RelevanceScore = 0.98,
                    PublishedDate = DateTime.UtcNow.AddDays(-10)
                }
            },
            ["hadith"] = new()
            {
                new WebSearchResult
                {
                    Title = "Introduction to Hadith Literature",
                    Snippet = "Hadith are the recorded sayings, actions, and approvals of Prophet Muhammad (peace be upon him). They provide guidance for Muslim life and help interpret the Quran.",
                    Url = "https://sunnah.com/about-hadith",
                    Source = "Sunnah.com",
                    RelevanceScore = 0.94,
                    PublishedDate = DateTime.UtcNow.AddDays(-20)
                }
            },
            ["fasting"] = new()
            {
                new WebSearchResult
                {
                    Title = "Ramadan and Fasting in Islam",
                    Snippet = "Fasting (Sawm) during Ramadan is one of the Five Pillars of Islam. Muslims abstain from food, drink, and other physical needs from dawn to sunset.",
                    Url = "https://islamqa.info/ramadan",
                    Source = "IslamQA",
                    RelevanceScore = 0.96,
                    PublishedDate = DateTime.UtcNow.AddDays(-45)
                }
            },
            ["charity"] = new()
            {
                new WebSearchResult
                {
                    Title = "Zakat: The Third Pillar of Islam",
                    Snippet = "Zakat is obligatory charity in Islam, representing 2.5% of a Muslim's savings. It purifies wealth and helps those in need.",
                    Url = "https://islamqa.info/zakat",
                    Source = "IslamQA",
                    RelevanceScore = 0.93,
                    PublishedDate = DateTime.UtcNow.AddDays(-25)
                }
            },
            ["general"] = new()
            {
                new WebSearchResult
                {
                    Title = "Introduction to Islam",
                    Snippet = "Islam is a monotheistic religion based on the belief in one God (Allah) and Muhammad as His final messenger. It emphasizes peace, submission to Allah, and righteous conduct.",
                    Url = "https://islamqa.info/introduction",
                    Source = "IslamQA",
                    RelevanceScore = 0.85,
                    PublishedDate = DateTime.UtcNow.AddDays(-60)
                }
            }
        };
    }

    private Dictionary<string, List<GlobalSearchResult>> InitializeFallbackSearchResults()
    {
        return new Dictionary<string, List<GlobalSearchResult>>
        {
            ["prayer"] = new()
            {
                new GlobalSearchResult(
                    IslamicContentType.Verse,
                    "Verse about Prayer",
                    "And establish prayer and give zakah and bow with those who bow. (Quran 2:43)",
                    "وَأَقِيمُوا الصَّلَاةَ وَآتُوا الزَّكَاةَ وَارْكَعُوا مَعَ الرَّاكِعِينَ",
                    "Quran",
                    "Quran 2:43",
                    0.95,
                    new Dictionary<string, object> { ["context"] = "Prayer is fundamental in Islam" }
                )
            },
            ["quran"] = new()
            {
                new GlobalSearchResult(
                    IslamicContentType.Verse,
                    "About the Quran",
                    "This is the Book about which there is no doubt, a guidance for those conscious of Allah. (Quran 2:2)",
                    "ذَٰلِكَ الْكِتَابُ لَا رَيْبَ فِيهِ هُدًى لِّلْمُتَّقِينَ",
                    "Quran",
                    "Quran 2:2",
                    0.98,
                    new Dictionary<string, object> { ["context"] = "The Quran as guidance" }
                )
            },
            ["general"] = new()
            {
                new GlobalSearchResult(
                    IslamicContentType.Verse,
                    "General Islamic Guidance",
                    "And whoever relies upon Allah - then He is sufficient for him. Indeed, Allah will accomplish His purpose. (Quran 65:3)",
                    "وَمَن يَتَوَكَّلْ عَلَى اللَّهِ فَهُوَ حَسْبُهُ إِنَّ اللَّهَ بَالِغُ أَمْرِهِ",
                    "Quran",
                    "Quran 65:3",
                    0.85,
                    new Dictionary<string, object> { ["context"] = "Trust in Allah" }
                )
            }
        };
    }
}
