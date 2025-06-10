using System.Text.Json;
using Microsoft.Extensions.Logging;
using Muwasala.MCP.Models;
using Muwasala.MCP.Services;

namespace Muwasala.MCP.Services;

public interface IIslamicContentService
{
    Task<List<IslamicContent>> SearchQuranAsync(string searchTerm, int limit = 10);
    Task<List<IslamicContent>> SearchHadithAsync(string searchTerm, int limit = 10);
    Task<List<IslamicContent>> SearchFiqhAsync(string searchTerm, int limit = 10);
    Task<List<IslamicContent>> SearchDuaAsync(string searchTerm, int limit = 10);
    Task<List<IslamicContent>> SearchAllContentAsync(string searchTerm, int limit = 5);
    Task<IslamicContent?> GetQuranVerseAsync(int surah, int verse);
    Task<List<IslamicContent>> GetSurahVersesAsync(int surah);
    Task<List<IslamicContent>> GetHadithByCollectionAsync(string collection, int limit = 10);
    Task<List<IslamicContent>> GetFiqhByMadhabAsync(string madhab, int limit = 10);
}

public class IslamicContentService : IIslamicContentService
{
    private readonly ISqliteService _sqliteService;
    private readonly ILogger<IslamicContentService> _logger;

    public IslamicContentService(ISqliteService sqliteService, ILogger<IslamicContentService> logger)
    {
        _sqliteService = sqliteService;
        _logger = logger;
    }

    public async Task<List<IslamicContent>> SearchQuranAsync(string searchTerm, int limit = 10)
    {
        var query = @"
            SELECT SurahNumber, VerseNumber, ArabicText, Translation, Transliteration, Theme, Context, Keywords
            FROM QuranVerses 
            WHERE Translation LIKE @searchTerm OR ArabicText LIKE @searchTerm OR Keywords LIKE @searchTerm OR Theme LIKE @searchTerm
            ORDER BY SurahNumber, VerseNumber
            LIMIT @limit";

        var parameters = new Dictionary<string, object>
        {
            { "searchTerm", $"%{searchTerm}%" },
            { "limit", limit }
        };

        var result = await _sqliteService.ExecuteQueryAsync(query, parameters);
        return ConvertToQuranContent(result);
    }

    public async Task<List<IslamicContent>> SearchHadithAsync(string searchTerm, int limit = 10)
    {
        var query = @"
            SELECT Collection, BookNumber, HadithNumber, ArabicText, Translation, Grade, Topic, Explanation
            FROM HadithRecords 
            WHERE Translation LIKE @searchTerm OR ArabicText LIKE @searchTerm OR Topic LIKE @searchTerm OR Explanation LIKE @searchTerm
            ORDER BY Collection, BookNumber, HadithNumber
            LIMIT @limit";

        var parameters = new Dictionary<string, object>
        {
            { "searchTerm", $"%{searchTerm}%" },
            { "limit", limit }
        };

        var result = await _sqliteService.ExecuteQueryAsync(query, parameters);
        return ConvertToHadithContent(result);
    }

    public async Task<List<IslamicContent>> SearchFiqhAsync(string searchTerm, int limit = 10)
    {
        var query = @"
            SELECT Question, Ruling, Madhab, Evidence, Topic, ScholarlyReferences, ModernApplication
            FROM FiqhRulings 
            WHERE Question LIKE @searchTerm OR Ruling LIKE @searchTerm OR Topic LIKE @searchTerm OR Evidence LIKE @searchTerm
            ORDER BY Topic
            LIMIT @limit";

        var parameters = new Dictionary<string, object>
        {
            { "searchTerm", $"%{searchTerm}%" },
            { "limit", limit }
        };

        var result = await _sqliteService.ExecuteQueryAsync(query, parameters);
        return ConvertToFiqhContent(result);
    }

    public async Task<List<IslamicContent>> SearchDuaAsync(string searchTerm, int limit = 10)
    {
        var query = @"
            SELECT ArabicText, Translation, Transliteration, Occasion, Source, Benefits, TimeOfDay
            FROM DuaRecords 
            WHERE Translation LIKE @searchTerm OR Occasion LIKE @searchTerm OR Benefits LIKE @searchTerm
            ORDER BY Occasion
            LIMIT @limit";

        var parameters = new Dictionary<string, object>
        {
            { "searchTerm", $"%{searchTerm}%" },
            { "limit", limit }
        };

        var result = await _sqliteService.ExecuteQueryAsync(query, parameters);
        return ConvertToDuaContent(result);
    }

    public async Task<List<IslamicContent>> SearchAllContentAsync(string searchTerm, int limit = 5)
    {
        var allContent = new List<IslamicContent>();

        // Search in parallel for better performance
        var tasks = new[]
        {
            SearchQuranAsync(searchTerm, limit),
            SearchHadithAsync(searchTerm, limit),
            SearchFiqhAsync(searchTerm, limit),
            SearchDuaAsync(searchTerm, limit)
        };

        var results = await Task.WhenAll(tasks);
        
        foreach (var contentList in results)
        {
            allContent.AddRange(contentList);
        }

        return allContent.Take(limit * 4).ToList();
    }

    public async Task<IslamicContent?> GetQuranVerseAsync(int surah, int verse)
    {
        var query = @"
            SELECT SurahNumber, VerseNumber, ArabicText, Translation, Transliteration, Theme, Context, Keywords
            FROM QuranVerses 
            WHERE SurahNumber = @surah AND VerseNumber = @verse";

        var parameters = new Dictionary<string, object>
        {
            { "surah", surah },
            { "verse", verse }
        };

        var result = await _sqliteService.ExecuteQueryAsync(query, parameters);
        var content = ConvertToQuranContent(result);
        return content.FirstOrDefault();
    }

    public async Task<List<IslamicContent>> GetSurahVersesAsync(int surah)
    {
        var query = @"
            SELECT SurahNumber, VerseNumber, ArabicText, Translation, Transliteration, Theme, Context, Keywords
            FROM QuranVerses 
            WHERE SurahNumber = @surah
            ORDER BY VerseNumber";

        var parameters = new Dictionary<string, object>
        {
            { "surah", surah }
        };

        var result = await _sqliteService.ExecuteQueryAsync(query, parameters);
        return ConvertToQuranContent(result);
    }

    public async Task<List<IslamicContent>> GetHadithByCollectionAsync(string collection, int limit = 10)
    {
        var query = @"
            SELECT Collection, BookNumber, HadithNumber, ArabicText, Translation, Grade, Topic, Explanation
            FROM HadithRecords 
            WHERE Collection LIKE @collection
            ORDER BY BookNumber, HadithNumber
            LIMIT @limit";

        var parameters = new Dictionary<string, object>
        {
            { "collection", $"%{collection}%" },
            { "limit", limit }
        };

        var result = await _sqliteService.ExecuteQueryAsync(query, parameters);
        return ConvertToHadithContent(result);
    }

    public async Task<List<IslamicContent>> GetFiqhByMadhabAsync(string madhab, int limit = 10)
    {
        var query = @"
            SELECT Question, Ruling, Madhab, Evidence, Topic, ScholarlyReferences, ModernApplication
            FROM FiqhRulings 
            WHERE Madhab = @madhab
            ORDER BY Topic
            LIMIT @limit";

        var parameters = new Dictionary<string, object>
        {
            { "madhab", madhab },
            { "limit", limit }
        };

        var result = await _sqliteService.ExecuteQueryAsync(query, parameters);
        return ConvertToFiqhContent(result);
    }

    private List<IslamicContent> ConvertToQuranContent(DatabaseQueryResult result)
    {
        return result.rows.Select(row => new IslamicContent
        {
            type = "Quran",
            title = $"Surah {row["SurahNumber"]}:{row["VerseNumber"]}",
            content = row["Translation"]?.ToString() ?? "",
            arabicText = row["ArabicText"]?.ToString(),
            reference = $"{row["SurahNumber"]}:{row["VerseNumber"]}",
            source = "Quran",
            metadata = new Dictionary<string, object?>
            {
                { "surah", row["SurahNumber"] },
                { "verse", row["VerseNumber"] },
                { "transliteration", row["Transliteration"] },
                { "theme", row["Theme"] },
                { "context", row["Context"] },
                { "keywords", row["Keywords"] }
            }
        }).ToList();
    }

    private List<IslamicContent> ConvertToHadithContent(DatabaseQueryResult result)
    {
        return result.rows.Select(row => new IslamicContent
        {
            type = "Hadith",
            title = $"{row["Collection"]} - Book {row["BookNumber"]}, Hadith {row["HadithNumber"]}",
            content = row["Translation"]?.ToString() ?? "",
            arabicText = row["ArabicText"]?.ToString(),
            reference = $"{row["Collection"]} {row["BookNumber"]}:{row["HadithNumber"]}",
            source = row["Collection"]?.ToString(),
            metadata = new Dictionary<string, object?>
            {
                { "collection", row["Collection"] },
                { "bookNumber", row["BookNumber"] },
                { "hadithNumber", row["HadithNumber"] },
                { "grade", row["Grade"] },
                { "topic", row["Topic"] },
                { "explanation", row["Explanation"] }
            }
        }).ToList();
    }

    private List<IslamicContent> ConvertToFiqhContent(DatabaseQueryResult result)
    {
        return result.rows.Select(row => new IslamicContent
        {
            type = "Fiqh",
            title = row["Question"]?.ToString() ?? "",
            content = row["Ruling"]?.ToString() ?? "",
            reference = $"Madhab: {row["Madhab"]}",
            source = "Fiqh Ruling",
            metadata = new Dictionary<string, object?>
            {
                { "madhab", row["Madhab"] },
                { "evidence", row["Evidence"] },
                { "topic", row["Topic"] },
                { "scholarlyReferences", row["ScholarlyReferences"] },
                { "modernApplication", row["ModernApplication"] }
            }
        }).ToList();
    }

    private List<IslamicContent> ConvertToDuaContent(DatabaseQueryResult result)
    {
        return result.rows.Select(row => new IslamicContent
        {
            type = "Dua",
            title = row["Occasion"]?.ToString() ?? "",
            content = row["Translation"]?.ToString() ?? "",
            arabicText = row["ArabicText"]?.ToString(),
            reference = row["Source"]?.ToString(),
            source = "Dua Collection",
            metadata = new Dictionary<string, object?>
            {
                { "occasion", row["Occasion"] },
                { "transliteration", row["Transliteration"] },
                { "benefits", row["Benefits"] },
                { "timeOfDay", row["TimeOfDay"] }
            }
        }).ToList();
    }
}
