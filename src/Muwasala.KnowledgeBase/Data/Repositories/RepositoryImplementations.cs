using Microsoft.EntityFrameworkCore;
using Muwasala.Core.Models;
using Muwasala.KnowledgeBase.Data.Models;
using System.Linq.Expressions;

namespace Muwasala.KnowledgeBase.Data.Repositories;

/// <summary>
/// Generic repository implementation for common database operations
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly IslamicKnowledgeDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(IslamicKnowledgeDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
        return entities;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }
}

/// <summary>
/// Repository implementation for Quran verses with specialized search methods
/// </summary>
public class QuranRepository : Repository<QuranVerseEntity>, IQuranRepository
{
    public QuranRepository(IslamicKnowledgeDbContext context) : base(context) { }

    public async Task<IEnumerable<QuranVerseEntity>> SearchByTextAsync(string searchText, string language = "en", int maxResults = 10)
    {
        var query = searchText.ToLower();        return await _dbSet
            .Include(v => v.Tafsirs)
            .Where(v => v.Translation.ToLower().Contains(query) ||
                       v.ArabicText.Contains(query) ||
                       (v.Transliteration != null && v.Transliteration.ToLower().Contains(query)))
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<IEnumerable<QuranVerseEntity>> SearchByThemeAsync(string theme, string language = "en", int maxResults = 10)
    {
        var query = theme.ToLower();
        
        // Enhanced theme search with synonyms
        var searchTerms = new List<string> { query };
        
        // Add Islamic concept synonyms
        if (query.Contains("patience")) searchTerms.AddRange(new[] { "sabr", "perseverance", "endurance" });
        if (query.Contains("mercy")) searchTerms.AddRange(new[] { "rahman", "raheem", "compassion" });
        if (query.Contains("prayer")) searchTerms.AddRange(new[] { "salah", "worship", "dua" });
        if (query.Contains("guidance")) searchTerms.AddRange(new[] { "hidayah", "direction", "path" });
        if (query.Contains("knowledge")) searchTerms.AddRange(new[] { "ilm", "wisdom", "understanding" });
          return await _dbSet
            .Include(v => v.Tafsirs)
            .Where(v => searchTerms.Any(term =>
                v.Translation.ToLower().Contains(term) ||
                v.ArabicText.Contains(term) ||
                (v.Transliteration != null && v.Transliteration.ToLower().Contains(term)) ||
                v.Tafsirs.Any(t => t.Commentary.ToLower().Contains(term))))
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<QuranVerseEntity?> GetVerseAsync(int surahNumber, int verseNumber, string language = "en")
    {        return await _dbSet
            .Include(v => v.Tafsirs)
            .FirstOrDefaultAsync(v => v.SurahNumber == surahNumber && v.VerseNumber == verseNumber);
    }

    public async Task<IEnumerable<QuranVerseEntity>> GetSurahAsync(int surahNumber, string language = "en")
    {        return await _dbSet
            .Include(v => v.Tafsirs)
            .Where(v => v.SurahNumber == surahNumber)
            .OrderBy(v => v.VerseNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<QuranVerseEntity>> GetVersesByContextAsync(string context, string language = "en", int maxResults = 10)
    {
        return await SearchByThemeAsync(context, language, maxResults);
    }
}

/// <summary>
/// Repository implementation for Tafsir entries
/// </summary>
public class TafsirRepository : Repository<TafsirEntity>, ITafsirRepository
{
    public TafsirRepository(IslamicKnowledgeDbContext context) : base(context) { }

    public async Task<IEnumerable<TafsirEntity>> GetTafsirForVerseAsync(int surahNumber, int verseNumber, string source = "IbnKathir")
    {
        var query = _dbSet.Include(t => t.QuranVerse)
            .Where(t => t.QuranVerse.SurahNumber == surahNumber && t.QuranVerse.VerseNumber == verseNumber);

        if (!string.IsNullOrEmpty(source))
        {
            query = query.Where(t => t.Source.Contains(source));
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<TafsirEntity>> SearchTafsirAsync(string searchText, string source = "", int maxResults = 10)
    {
        var query = searchText.ToLower();
        var dbQuery = _dbSet.Include(t => t.QuranVerse)
            .Where(t => t.Commentary.ToLower().Contains(query) || t.Source.ToLower().Contains(query));

        if (!string.IsNullOrEmpty(source))
        {
            dbQuery = dbQuery.Where(t => t.Source.ToLower().Contains(source.ToLower()));
        }

        return await dbQuery.Take(maxResults).ToListAsync();
    }
}

/// <summary>
/// Repository implementation for Hadith records
/// </summary>
public class HadithRepository : Repository<HadithEntity>, IHadithRepository
{
    public HadithRepository(IslamicKnowledgeDbContext context) : base(context) { }

    public async Task<IEnumerable<HadithEntity>> SearchByTextAsync(string searchText, string language = "en", int maxResults = 10)
    {
        var query = searchText.ToLower();
        return await _dbSet
            .Where(h => (h.Translation.ToLower().Contains(query) || 
                        h.ArabicText.Contains(query)) &&
                       h.Grade <= (int)HadithGrade.Hasan) // Only Sahih and Hasan (authentic) hadiths
            .OrderBy(h => h.Grade) // Sahih first, then Hasan
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<IEnumerable<HadithEntity>> GetByTopicAsync(string topic, string language = "en", int maxResults = 10)
    {
        var query = topic.ToLower();
        return await _dbSet
            .Where(h => h.Topic.ToLower().Contains(query) || h.Translation.ToLower().Contains(query))
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<HadithEntity?> GetByReferenceAsync(string collection, string hadithNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(h => h.Collection == collection && h.HadithNumber == hadithNumber);
    }

    public async Task<IEnumerable<HadithEntity>> GetAuthenticHadithAsync(string topic, HadithGrade minGrade = HadithGrade.Sahih, int maxResults = 10)
    {
        var query = topic.ToLower();
        return await _dbSet
            .Where(h => (h.Topic.ToLower().Contains(query) || h.Translation.ToLower().Contains(query)) &&
                       h.Grade >= (int)minGrade)
            .OrderBy(h => h.Grade) // Sahih first, then Hasan, etc.
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<IEnumerable<HadithEntity>> GetByCollectionAsync(string collection, int maxResults = 50)
    {
        return await _dbSet
            .Where(h => h.Collection == collection)
            .Take(maxResults)
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for Fiqh rulings
/// </summary>
public class FiqhRepository : Repository<FiqhRulingEntity>, IFiqhRepository
{
    public FiqhRepository(IslamicKnowledgeDbContext context) : base(context) { }

    public async Task<IEnumerable<FiqhRulingEntity>> SearchRulingsAsync(string question, Madhab madhab, string language = "en", int maxResults = 10)
    {
        var query = question.ToLower();
        return await _dbSet
            .Where(f => (f.Question.ToLower().Contains(query) || f.Ruling.ToLower().Contains(query)) &&
                       f.Madhab == (int)madhab)
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<IEnumerable<FiqhRulingEntity>> GetRulingsByTopicAsync(string topic, Madhab madhab, string language = "en", int maxResults = 10)
    {
        var query = topic.ToLower();
        return await _dbSet
            .Where(f => f.Topic.ToLower().Contains(query) && f.Madhab == (int)madhab)
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<IEnumerable<FiqhRulingEntity>> GetRulingsByTopicAllMadhabsAsync(string topic, string language = "en", int maxResults = 20)
    {
        var query = topic.ToLower();
        return await _dbSet
            .Where(f => f.Topic.ToLower().Contains(query))
            .OrderBy(f => f.Madhab)
            .Take(maxResults)
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for Du'a records
/// </summary>
public class DuaRepository : Repository<DuaEntity>, IDuaRepository
{
    public DuaRepository(IslamicKnowledgeDbContext context) : base(context) { }

    public async Task<IEnumerable<DuaEntity>> SearchByOccasionAsync(string occasion, string language = "en", int maxResults = 10)
    {
        var query = occasion.ToLower();
        return await _dbSet
            .Where(d => d.Occasion.ToLower().Contains(query) || d.Translation.ToLower().Contains(query))
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<DuaEntity?> GetSpecificPrayerAsync(SpecificPrayer prayerType, string language = "en")
    {
        return await _dbSet
            .FirstOrDefaultAsync(d => d.SpecificPrayerType == (int)prayerType);
    }

    public async Task<IEnumerable<DuaEntity>> GetDailyDuasAsync(string timeOfDay, string language = "en", int maxResults = 10)
    {
        var query = timeOfDay.ToLower();
        return await _dbSet
            .Where(d => d.Occasion.ToLower().Contains(query) || d.Translation.ToLower().Contains(query))
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<IEnumerable<DuaEntity>> SearchByTextAsync(string searchText, string language = "en", int maxResults = 10)
    {
        var query = searchText.ToLower();
        return await _dbSet
            .Where(d => d.Translation.ToLower().Contains(query) || 
                       d.ArabicText.Contains(query) ||
                       (d.Transliteration != null && d.Transliteration.ToLower().Contains(query)))
            .Take(maxResults)
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for Sirah events
/// </summary>
public class SirahRepository : Repository<SirahEventEntity>, ISirahRepository
{
    public SirahRepository(IslamicKnowledgeDbContext context) : base(context) { }

    public async Task<IEnumerable<SirahEventEntity>> SearchByContextAsync(string context, string language = "en", int maxResults = 10)
    {
        var query = context.ToLower();
        return await _dbSet
            .Where(s => s.Description.ToLower().Contains(query) || 
                       s.EventName.ToLower().Contains(query) ||
                       s.KeyLessons.ToLower().Contains(query))
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<IEnumerable<SirahEventEntity>> GetEventsByPeriodAsync(SirahPeriod period, string language = "en", int maxResults = 20)
    {        return await _dbSet
            .Where(s => s.Period == (int)period)
            .OrderBy(s => s.EventDate)
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<SirahEventEntity?> GetEventByNameAsync(string eventName, string language = "en")
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.EventName.ToLower() == eventName.ToLower());
    }    public async Task<IEnumerable<SirahEventEntity>> GetChronologicalEventsAsync(string language = "en")
    {
        return await _dbSet
            .OrderBy(s => s.EventDate)
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for Tajweed rules
/// </summary>
public class TajweedRepository : Repository<TajweedRuleEntity>, ITajweedRepository
{
    public TajweedRepository(IslamicKnowledgeDbContext context) : base(context) { }

    public async Task<IEnumerable<VerseTajweedEntity>> GetVerseRulesAsync(int surahNumber, int verseNumber)
    {
        return await _context.VerseTajweedData
            .Include(vt => vt.TajweedRule)
            .Where(vt => vt.SurahNumber == surahNumber && vt.VerseNumber == verseNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<CommonMistakeEntity>> GetCommonMistakesAsync(int surahNumber, int verseNumber)
    {
        return await _context.CommonMistakes
            .Where(cm => cm.SurahNumber == surahNumber && cm.VerseNumber == verseNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<TajweedRuleEntity>> GetRulesByTypeAsync(string ruleType)
    {
        return await _dbSet
            .Where(tr => tr.Category.ToLower().Contains(ruleType.ToLower()))
            .ToListAsync();
    }

    public async Task<IEnumerable<CommonMistakeEntity>> GetAllCommonMistakesAsync()
    {
        return await _context.CommonMistakes.ToListAsync();
    }
}

/// <summary>
/// Repository implementation for Common Mistakes
/// </summary>
public class CommonMistakeRepository : Repository<CommonMistakeEntity>, ICommonMistakeRepository
{
    public CommonMistakeRepository(IslamicKnowledgeDbContext context) : base(context) { }

    public async Task<IEnumerable<CommonMistakeEntity>> GetMistakesForVerseAsync(int surahNumber, int verseNumber)
    {
        return await _dbSet
            .Where(cm => cm.SurahNumber == surahNumber && cm.VerseNumber == verseNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<CommonMistakeEntity>> SearchMistakesAsync(string searchText, int maxResults = 10)
    {
        var query = searchText.ToLower();
        return await _dbSet            .Where(cm => cm.Description.ToLower().Contains(query) || 
                        cm.Correction.ToLower().Contains(query))
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<IEnumerable<CommonMistakeEntity>> GetMistakesByTypeAsync(string mistakeType, int maxResults = 10)
    {
        return await _dbSet
            .Where(cm => cm.MistakeType.ToLower().Contains(mistakeType.ToLower()))
            .Take(maxResults)
            .ToListAsync();
    }
}

/// <summary>
/// Repository implementation for Verse Tajweed data
/// </summary>
public class VerseTajweedRepository : Repository<VerseTajweedEntity>, IVerseTajweedRepository
{
    public VerseTajweedRepository(IslamicKnowledgeDbContext context) : base(context) { }

    public async Task<IEnumerable<VerseTajweedEntity>> GetTajweedForVerseAsync(int surahNumber, int verseNumber)
    {
        return await _dbSet
            .Include(vt => vt.TajweedRule)
            .Where(vt => vt.SurahNumber == surahNumber && vt.VerseNumber == verseNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<VerseTajweedEntity>> GetTajweedBySurahAsync(int surahNumber)
    {
        return await _dbSet
            .Include(vt => vt.TajweedRule)
            .Where(vt => vt.SurahNumber == surahNumber)
            .OrderBy(vt => vt.VerseNumber)
            .ToListAsync();
    }
}
