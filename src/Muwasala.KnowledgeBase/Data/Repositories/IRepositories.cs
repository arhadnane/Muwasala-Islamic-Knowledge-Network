using Muwasala.Core.Models;
using Muwasala.KnowledgeBase.Data.Models;

namespace Muwasala.KnowledgeBase.Data.Repositories;

/// <summary>
/// Generic repository interface for common database operations
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    Task<int> CountAsync();
    Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
}

/// <summary>
/// Repository interface for Quran verses with specialized search methods
/// </summary>
public interface IQuranRepository : IRepository<QuranVerseEntity>
{
    Task<IEnumerable<QuranVerseEntity>> SearchByTextAsync(string searchText, string language = "en", int maxResults = 10);
    Task<IEnumerable<QuranVerseEntity>> SearchByThemeAsync(string theme, string language = "en", int maxResults = 10);
    Task<QuranVerseEntity?> GetVerseAsync(int surahNumber, int verseNumber, string language = "en");
    Task<IEnumerable<QuranVerseEntity>> GetSurahAsync(int surahNumber, string language = "en");
    Task<IEnumerable<QuranVerseEntity>> GetVersesByContextAsync(string context, string language = "en", int maxResults = 10);
}

/// <summary>
/// Repository interface for Tafsir entries
/// </summary>
public interface ITafsirRepository : IRepository<TafsirEntity>
{
    Task<IEnumerable<TafsirEntity>> GetTafsirForVerseAsync(int surahNumber, int verseNumber, string source = "IbnKathir");
    Task<IEnumerable<TafsirEntity>> SearchTafsirAsync(string searchText, string source = "", int maxResults = 10);
}

/// <summary>
/// Repository interface for Hadith records with authentication and classification
/// </summary>
public interface IHadithRepository : IRepository<HadithEntity>
{
    Task<IEnumerable<HadithEntity>> SearchByTextAsync(string searchText, string language = "en", int maxResults = 10);
    Task<IEnumerable<HadithEntity>> GetByTopicAsync(string topic, string language = "en", int maxResults = 10);
    Task<HadithEntity?> GetByReferenceAsync(string collection, string hadithNumber);
    Task<IEnumerable<HadithEntity>> GetAuthenticHadithAsync(string topic, HadithGrade minGrade = HadithGrade.Sahih, int maxResults = 10);
    Task<IEnumerable<HadithEntity>> GetByCollectionAsync(string collection, int maxResults = 50);
}

/// <summary>
/// Repository interface for Fiqh rulings with Madhab-specific queries
/// </summary>
public interface IFiqhRepository : IRepository<FiqhRulingEntity>
{
    Task<IEnumerable<FiqhRulingEntity>> SearchRulingsAsync(string question, Madhab madhab, string language = "en", int maxResults = 10);
    Task<IEnumerable<FiqhRulingEntity>> GetRulingsByTopicAsync(string topic, Madhab madhab, string language = "en", int maxResults = 10);
    Task<IEnumerable<FiqhRulingEntity>> GetRulingsByTopicAllMadhabsAsync(string topic, string language = "en", int maxResults = 20);
}

/// <summary>
/// Repository interface for Du'a records with occasion-based queries
/// </summary>
public interface IDuaRepository : IRepository<DuaEntity>
{
    Task<IEnumerable<DuaEntity>> SearchByOccasionAsync(string occasion, string language = "en", int maxResults = 10);
    Task<DuaEntity?> GetSpecificPrayerAsync(SpecificPrayer prayerType, string language = "en");
    Task<IEnumerable<DuaEntity>> GetDailyDuasAsync(string timeOfDay, string language = "en", int maxResults = 10);
    Task<IEnumerable<DuaEntity>> SearchByTextAsync(string searchText, string language = "en", int maxResults = 10);
}

/// <summary>
/// Repository interface for Sirah events with period-based queries
/// </summary>
public interface ISirahRepository : IRepository<SirahEventEntity>
{
    Task<IEnumerable<SirahEventEntity>> SearchByContextAsync(string context, string language = "en", int maxResults = 10);
    Task<IEnumerable<SirahEventEntity>> GetEventsByPeriodAsync(SirahPeriod period, string language = "en", int maxResults = 20);
    Task<SirahEventEntity?> GetEventByNameAsync(string eventName, string language = "en");
    Task<IEnumerable<SirahEventEntity>> GetChronologicalEventsAsync(string language = "en");
}

/// <summary>
/// Repository interface for Tajweed rules and verse-specific Tajweed data
/// </summary>
public interface ITajweedRepository : IRepository<TajweedRuleEntity>
{
    Task<IEnumerable<VerseTajweedEntity>> GetVerseRulesAsync(int surahNumber, int verseNumber);
    Task<IEnumerable<CommonMistakeEntity>> GetCommonMistakesAsync(int surahNumber, int verseNumber);
    Task<IEnumerable<TajweedRuleEntity>> GetRulesByTypeAsync(string ruleType);
    Task<IEnumerable<CommonMistakeEntity>> GetAllCommonMistakesAsync();
}

/// <summary>
/// Repository interface for Common Mistakes in Quranic recitation
/// </summary>
public interface ICommonMistakeRepository : IRepository<CommonMistakeEntity>
{
    Task<IEnumerable<CommonMistakeEntity>> GetMistakesForVerseAsync(int surahNumber, int verseNumber);
    Task<IEnumerable<CommonMistakeEntity>> SearchMistakesAsync(string searchText, int maxResults = 10);
    Task<IEnumerable<CommonMistakeEntity>> GetMistakesByTypeAsync(string mistakeType, int maxResults = 10);
}

/// <summary>
/// Repository interface for Verse-specific Tajweed data
/// </summary>
public interface IVerseTajweedRepository : IRepository<VerseTajweedEntity>
{
    Task<IEnumerable<VerseTajweedEntity>> GetTajweedForVerseAsync(int surahNumber, int verseNumber);
    Task<IEnumerable<VerseTajweedEntity>> GetTajweedBySurahAsync(int surahNumber);
}
