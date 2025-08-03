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
        var searchTerms = GetExpandedSearchTerms(query);
        
        return await _dbSet
            .Where(h => searchTerms.Any(term =>
                h.Translation.ToLower().Contains(term) || 
                h.ArabicText.Contains(term) ||
                (h.Topic != null && h.Topic.ToLower().Contains(term)) ||
                (h.Explanation != null && h.Explanation.ToLower().Contains(term))) &&
                h.Grade <= (int)HadithGrade.Hasan) // Only Sahih and Hasan (authentic) hadiths
            .OrderBy(h => h.Grade) // Sahih first, then Hasan
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<IEnumerable<HadithEntity>> GetByTopicAsync(string topic, string language = "en", int maxResults = 10)
    {
        var query = topic.ToLower();
        var searchTerms = GetExpandedSearchTerms(query);
        
        return await _dbSet
            .Where(h => searchTerms.Any(term => 
                (h.Topic != null && h.Topic.ToLower().Contains(term)) || 
                h.Translation.ToLower().Contains(term) ||
                (h.Explanation != null && h.Explanation.ToLower().Contains(term))))
            .Take(maxResults)
            .ToListAsync();
    }

    private List<string> GetExpandedSearchTerms(string originalTerm)
    {
        var terms = new List<string> { originalTerm };
        var termLower = originalTerm.ToLower().Trim();
        
        // Add generic linguistic variations
        terms.AddRange(GetLinguisticVariations(termLower));
        
        // Add specific Islamic and topical terms
        terms.AddRange(GetTopicalTerms(termLower));
        
        return terms.Distinct().ToList();
    }

    private List<string> GetLinguisticVariations(string term)
    {
        var variations = new List<string>();
        
        // 1. Plural/Singular variations
        if (term.EndsWith("s") && term.Length > 3)
        {
            // Plural to singular: "women" -> "woman", "prayers" -> "prayer"
            var singular = term.TrimEnd('s');
            variations.Add(singular);
            
            // Handle irregular plurals
            if (singular.EndsWith("e"))
                singular = singular.TrimEnd('e');
            variations.Add(singular);
        }
        else
        {
            // Singular to plural: "woman" -> "women", "prayer" -> "prayers"
            variations.Add(term + "s");
            if (term.EndsWith("y"))
            {
                variations.Add(term.TrimEnd('y') + "ies"); // "family" -> "families"
            }
            if (term.EndsWith("man"))
            {
                variations.Add(term.Replace("man", "men")); // "woman" -> "women"
            }
        }
        
        // 2. Verb forms variations
        if (term.EndsWith("ing"))
        {
            var root = term.Substring(0, term.Length - 3);
            variations.AddRange(new[] { root, root + "ed", root + "s" });
        }
        else if (term.EndsWith("ed"))
        {
            var root = term.Substring(0, term.Length - 2);
            variations.AddRange(new[] { root, root + "ing", root + "s" });
        }
        else
        {
            // Add common verb forms
            variations.AddRange(new[] { term + "ing", term + "ed", term + "s" });
            if (term.EndsWith("e"))
            {
                var root = term.TrimEnd('e');
                variations.AddRange(new[] { root + "ing", root + "ed" });
            }
        }
        
        // 3. Adjective/Adverb variations
        if (term.EndsWith("ly"))
        {
            variations.Add(term.Substring(0, term.Length - 2)); // "peacefully" -> "peaceful"
        }
        else
        {
            variations.Add(term + "ly"); // "peaceful" -> "peacefully"
        }
        
        // 4. Common prefixes and suffixes
        var prefixes = new[] { "un", "in", "dis", "re", "pre", "over", "under" };
        var suffixes = new[] { "ness", "ment", "tion", "sion", "able", "ible", "ful", "less" };
        
        foreach (var prefix in prefixes)
        {
            if (term.StartsWith(prefix) && term.Length > prefix.Length + 2)
            {
                variations.Add(term.Substring(prefix.Length)); // "unhappy" -> "happy"
            }
            else
            {
                variations.Add(prefix + term); // "happy" -> "unhappy"
            }
        }
        
        foreach (var suffix in suffixes)
        {
            if (term.EndsWith(suffix) && term.Length > suffix.Length + 2)
            {
                variations.Add(term.Substring(0, term.Length - suffix.Length)); // "happiness" -> "happy"
            }
            else
            {
                variations.Add(term + suffix); // "happy" -> "happiness"
            }
        }
        
        return variations.Where(v => v.Length > 2).ToList(); // Filter out very short variations
    }

    private List<string> GetTopicalTerms(string term)
    {
        var topicalTerms = new List<string>();
        
        // Islamic-specific terms mapping
        var islamicTerms = new Dictionary<string, string[]>
        {
            // Family and relationships
            ["women"] = new[] { "woman", "female", "wife", "mother", "daughter", "sister", "ladies", "girl", "females" },
            ["woman"] = new[] { "women", "female", "wife", "mother", "lady", "girl", "females" },
            ["marriage"] = new[] { "marry", "married", "wedding", "husband", "wife", "spouse", "nikah", "matrimony" },
            ["family"] = new[] { "relatives", "kin", "household", "children", "parents", "siblings" },
            
            // Worship and spirituality
            ["prayer"] = new[] { "pray", "salah", "salat", "worship", "dua", "supplication", "prayers" },
            ["worship"] = new[] { "prayer", "praise", "devotion", "reverence", "ibadah", "adoration" },
            ["faith"] = new[] { "belief", "believe", "trust", "iman", "religion", "conviction" },
            ["god"] = new[] { "allah", "lord", "creator", "almighty", "divine", "deity" },
            
            // Ethics and virtues
            ["charity"] = new[] { "zakat", "sadaqah", "alms", "giving", "poor", "needy", "donation" },
            ["knowledge"] = new[] { "learn", "study", "education", "wisdom", "ilm", "scholar", "learning" },
            ["peace"] = new[] { "peaceful", "harmony", "tranquil", "salam", "calm", "serenity" },
            ["justice"] = new[] { "just", "fair", "fairness", "equity", "right", "adl", "righteousness" },
            ["patience"] = new[] { "patient", "perseverance", "endurance", "sabr", "steadfast", "tolerance" },
            ["forgiveness"] = new[] { "forgive", "pardon", "mercy", "compassion", "clemency" },
            ["humility"] = new[] { "humble", "modest", "meek", "unpretentious", "lowly" },
            
            // Actions and behaviors
            ["help"] = new[] { "assist", "aid", "support", "helping", "assistance", "service" },
            ["love"] = new[] { "affection", "care", "compassion", "kindness", "mercy", "loving" },
            ["respect"] = new[] { "honor", "esteem", "reverence", "dignity", "regard" },
            ["obey"] = new[] { "obedience", "follow", "comply", "submit", "adherence" },
            
            // Time and life
            ["death"] = new[] { "die", "dying", "mortality", "demise", "passing", "afterlife" },
            ["life"] = new[] { "living", "existence", "lifetime", "world", "dunya" },
            ["future"] = new[] { "tomorrow", "hereafter", "akhirah", "eternity", "afterlife" },
            
            // Social concepts
            ["community"] = new[] { "society", "ummah", "people", "nation", "group", "collective" },
            ["leader"] = new[] { "leadership", "guide", "imam", "ruler", "authority", "chief" },
            ["friend"] = new[] { "friendship", "companion", "ally", "buddy", "comrade" },
            
            // Eschatology and End Times (very important Islamic concepts)
            ["antichrist"] = new[] { "dajjal", "masih ad-dajjal", "deceiver", "false messiah", "one-eyed", "impostor" },
            ["anti christ"] = new[] { "dajjal", "masih ad-dajjal", "deceiver", "false messiah", "one-eyed", "impostor" },
            ["dajjal"] = new[] { "antichrist", "anti christ", "deceiver", "false messiah", "one-eyed", "impostor", "masih ad-dajjal" },
            ["end times"] = new[] { "last days", "final hour", "qiyamah", "resurrection", "judgment day", "apocalypse", "signs" },
            ["judgment day"] = new[] { "qiyamah", "resurrection", "final judgment", "last day", "reckoning", "hereafter" },
            ["resurrection"] = new[] { "qiyamah", "judgment day", "rising", "afterlife", "hereafter", "final judgment" },
            ["signs"] = new[] { "portents", "omens", "indicators", "warnings", "prophecy", "predictions" },
            
            // Prophets and messengers
            ["jesus"] = new[] { "isa", "christ", "messiah", "prophet isa", "son of mary", "maryam" },
            ["prophet"] = new[] { "messenger", "rasul", "nabi", "apostle", "envoy" },
            ["muhammad"] = new[] { "prophet muhammad", "messenger", "rasulullah", "ahmad", "final prophet" },
            ["moses"] = new[] { "musa", "prophet musa", "staff", "pharaoh", "exodus" },
            
            // Angels and spiritual beings
            ["angel"] = new[] { "malak", "gabriel", "jibril", "michael", "mikail", "messenger" },
            ["satan"] = new[] { "shaytan", "devil", "iblis", "evil", "tempter", "whisper" },
            ["devil"] = new[] { "shaytan", "satan", "iblis", "evil", "tempter", "demon" },
            
            // Paradise and Hell
            ["paradise"] = new[] { "jannah", "heaven", "garden", "bliss", "eternal reward" },
            ["hell"] = new[] { "jahannam", "hellfire", "punishment", "torment", "damnation" },
            ["heaven"] = new[] { "jannah", "paradise", "sky", "celestial", "divine realm" }
        };
        
        // Check for exact matches first
        if (islamicTerms.ContainsKey(term))
        {
            topicalTerms.AddRange(islamicTerms[term]);
        }
        
        // Check for partial matches (if the term contains or is contained in a key)
        foreach (var kvp in islamicTerms)
        {
            if (term.Contains(kvp.Key) || kvp.Key.Contains(term))
            {
                topicalTerms.AddRange(kvp.Value);
            }
        }
        
        // Add contextual terms based on semantic similarity
        var contextualMappings = new Dictionary<string, string[]>
        {
            // Add terms that might appear in similar contexts
            ["good"] = new[] { "righteous", "virtuous", "moral", "ethical", "beneficial" },
            ["bad"] = new[] { "evil", "wrong", "sinful", "harmful", "wicked" },
            ["reward"] = new[] { "blessing", "benefit", "gift", "prize", "recompense" },
            ["punishment"] = new[] { "penalty", "consequence", "retribution", "discipline" },
            ["truth"] = new[] { "honest", "truthful", "sincere", "genuine", "real" },
            ["false"] = new[] { "lie", "deception", "falsehood", "untrue", "dishonest" }
        };
        
        foreach (var kvp in contextualMappings)
        {
            if (term.Contains(kvp.Key) || kvp.Key.Contains(term))
            {
                topicalTerms.AddRange(kvp.Value);
            }
        }
        
        return topicalTerms.Distinct().ToList();
    }

    public async Task<HadithEntity?> GetByReferenceAsync(string collection, string hadithNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(h => h.Collection == collection && h.HadithNumber == hadithNumber);
    }

    public async Task<IEnumerable<HadithEntity>> GetAuthenticHadithAsync(string topic, HadithGrade minGrade = HadithGrade.Sahih, int maxResults = 10)
    {
        var searchTerms = GetExpandedSearchTerms(topic);
        
        // Use a single LINQ query with multiple OR conditions
        return await _dbSet
            .Where(h => searchTerms.Any(term => 
                (h.Topic != null && h.Topic.ToLower().Contains(term.ToLower())) ||
                h.Translation.ToLower().Contains(term.ToLower()) ||
                (h.Explanation != null && h.Explanation.ToLower().Contains(term.ToLower()))) &&
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
