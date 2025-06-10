using Muwasala.Core.Models;
using Muwasala.KnowledgeBase.Data.Models;
using Muwasala.KnowledgeBase.Services;

namespace Muwasala.KnowledgeBase.Data.Mappers;

/// <summary>
/// Static mapper class for converting between database entities and domain models
/// </summary>
public static class EntityMapper
{
    #region QuranVerse Mapping
      public static QuranVerse ToQuranVerse(this QuranVerseEntity entity)
    {
        return new QuranVerse(
            entity.SurahNumber,
            entity.VerseNumber,
            entity.ArabicText,
            entity.Translation,
            entity.Transliteration,
            entity.Language
        );
    }
      public static QuranVerseEntity ToEntity(this QuranVerse verse)
    {
        return new QuranVerseEntity
        {
            SurahNumber = verse.Surah,
            VerseNumber = verse.Verse,
            ArabicText = verse.ArabicText,
            Translation = verse.Translation,
            Transliteration = verse.Transliteration,
            Language = verse.Language
        };
    }
    
    #endregion
      #region TafsirEntry Mapping
    
    public static TafsirEntry ToTafsirEntry(this TafsirEntity entity)
    {
        return new TafsirEntry(
            new VerseReference(entity.QuranVerse.SurahNumber, entity.QuranVerse.VerseNumber),
            entity.Source,
            entity.Commentary,
            entity.Scholar ?? "Unknown",
            entity.Language
        );
    }
    
    public static TafsirEntity ToEntity(this TafsirEntry tafsir)
    {
        return new TafsirEntity
        {
            Source = tafsir.Source,
            Commentary = tafsir.Commentary,
            Scholar = tafsir.Scholar,
            Language = tafsir.Language
        };
    }
    
    #endregion
      #region HadithRecord Mapping
    
    public static HadithRecord ToHadithRecord(this HadithEntity entity)
    {
        return new HadithRecord(
            entity.ArabicText,
            entity.Translation,
            (HadithGrade)entity.Grade,
            entity.Collection,
            entity.BookNumber,
            entity.HadithNumber,
            entity.SanadChain?.Split(',').ToList() ?? new List<string>(),
            entity.Topic,
            entity.Language
        );
    }
    
    public static HadithEntity ToEntity(this HadithRecord hadith)
    {
        return new HadithEntity
        {
            ArabicText = hadith.ArabicText,
            Translation = hadith.Translation,
            Grade = (int)hadith.Grade,
            Collection = hadith.Collection,
            BookNumber = hadith.BookNumber,
            HadithNumber = hadith.HadithNumber,
            SanadChain = string.Join(",", hadith.SanadChain),
            Topic = hadith.Topic,
            Language = hadith.Language
        };
    }
    
    #endregion
    
    #region FiqhRuling Mapping
      public static FiqhRuling ToFiqhRuling(this FiqhRulingEntity entity)
    {
        return new FiqhRuling(
            entity.Question,
            entity.Ruling,
            (Madhab)entity.Madhab,
            entity.Evidence ?? "No evidence provided",
            entity.ScholarlyReferences ?? "Classical sources",
            new List<string>(), // ScholarReferences - empty list since entity doesn't have this
            entity.Language
        );
    }
    
    public static FiqhRulingEntity ToEntity(this FiqhRuling ruling)
    {
        return new FiqhRulingEntity
        {
            Question = ruling.Question,
            Ruling = ruling.Ruling,
            Madhab = (int)ruling.Madhab,
            Evidence = ruling.Evidence,
            ScholarlyReferences = ruling.Source,
            Language = ruling.Language,
            Topic = ruling.Question, // Use question as topic fallback
            Keywords = ruling.Question // Use question as keywords fallback
        };
    }
    
    #endregion
      #region DuaRecord Mapping
      public static DuaRecord ToDuaRecord(this DuaEntity entity)
    {
        return new DuaRecord(
            entity.ArabicText,
            entity.Translation,
            entity.Transliteration,
            entity.Occasion,
            entity.Source,
            entity.Benefits,
            entity.Language
        );
    }
    
    public static DuaEntity ToEntity(this DuaRecord dua)
    {
        return new DuaEntity
        {
            ArabicText = dua.ArabicText,
            Translation = dua.Translation,
            Transliteration = dua.Transliteration,
            Occasion = dua.Occasion,
            Source = dua.Source,
            Benefits = dua.Benefits,
            Language = dua.Language
        };
    }
    
    #endregion
    
    #region TajweedRule Mapping
      public static TajweedRule ToTajweedRule(this TajweedRuleEntity entity)
    {
        return new TajweedRule(
            entity.Name,
            entity.Description,
            0, // StartPosition - not stored in entity
            0, // EndPosition - not stored in entity
            entity.Example ?? ""
        );
    }
    
    public static TajweedRuleEntity ToEntity(this TajweedRule rule)
    {
        return new TajweedRuleEntity
        {
            Name = rule.Name,
            Description = rule.Description,
            Example = rule.Example,
            Category = "", // Would need to be passed separately
            Language = "en" // Default language
        };
    }
      #endregion
    
    #region GlobalSearchResult Mapping
      public static GlobalSearchResult ToGlobalSearchResult(this QuranVerseEntity entity)
    {
        return new GlobalSearchResult(
            IslamicContentType.Verse,
            $"Surah {entity.SurahNumber}:{entity.VerseNumber}",
            entity.Translation,
            entity.ArabicText,
            "Quran",
            $"{entity.SurahNumber}:{entity.VerseNumber}",
            1.0,
            new Dictionary<string, object> { ["language"] = entity.Language }
        );
    }
    
    public static GlobalSearchResult ToGlobalSearchResult(this HadithEntity entity)
    {
        return new GlobalSearchResult(
            IslamicContentType.Hadith,
            entity.Collection,
            entity.Translation,
            entity.ArabicText,
            entity.Collection,
            entity.HadithNumber,
            1.0,
            new Dictionary<string, object> { ["language"] = entity.Language, ["grade"] = entity.Grade }
        );
    }
    
    public static GlobalSearchResult ToGlobalSearchResult(this FiqhRulingEntity entity)
    {
        return new GlobalSearchResult(
            IslamicContentType.FiqhRuling,
            entity.Topic,
            entity.Ruling,
            string.Empty, // No Arabic text for Fiqh rulings
            $"{entity.Madhab} School",
            entity.Topic,
            1.0,
            new Dictionary<string, object> { ["language"] = entity.Language, ["madhab"] = entity.Madhab }
        );
    }
    
    public static GlobalSearchResult ToGlobalSearchResult(this DuaEntity entity)
    {
        return new GlobalSearchResult(
            IslamicContentType.Dua,
            entity.Occasion,
            entity.Translation,
            entity.ArabicText,
            entity.Source ?? "Islamic Tradition",
            entity.Occasion,
            1.0,
            new Dictionary<string, object> { ["language"] = entity.Language, ["occasion"] = entity.Occasion }
        );
    }
    
    public static GlobalSearchResult ToGlobalSearchResult(this SirahEventEntity entity)
    {
        return new GlobalSearchResult(
            IslamicContentType.SirahEvent,
            entity.EventName,
            entity.Description,
            string.Empty, // No Arabic text for Sirah events
            "Sirah",
            entity.EventName,
            1.0,
            new Dictionary<string, object> { ["language"] = entity.Language, ["period"] = entity.Period }
        );
    }
    
    #endregion

    // Note: VerseTajweedEntity is primarily used internally for database operations
    // and doesn't need to be mapped to a domain model record type.
    // The tajweed functionality uses VerseData, TajweedMarker, and other types instead.
    
    /*
    #region VerseTajweedInfo Mapping
    
    public static VerseTajweedInfo ToVerseTajweedInfo(this VerseTajweedEntity entity)
    {
        return new VerseTajweedInfo(
            entity.SurahNumber,
            entity.VerseNumber,
            entity.WordIndex,
            entity.RuleName,
            entity.RuleCategory,
            entity.Explanation,
            entity.Language
        );
    }
    
    public static VerseTajweedEntity ToEntity(this VerseTajweedInfo info)
    {
        return new VerseTajweedEntity
        {
            SurahNumber = info.SurahNumber,
            VerseNumber = info.VerseNumber,
            WordIndex = info.WordIndex,
            RuleName = info.RuleName,
            RuleCategory = info.RuleCategory,
            Explanation = info.Explanation,  
            Language = info.Language
        };
    }
    
    #endregion
    */
    
    #region SirahEvent Mapping
      public static SirahEvent ToSirahEvent(this SirahEventEntity entity)
    {
        return new SirahEvent(
            entity.EventName,
            entity.Description,
            (SirahPeriod)entity.Period,
            entity.EventDate,
            entity.Context ?? "",
            entity.KeyLessons?.Split(',').ToList() ?? new List<string>(),
            new List<string>(), // ParticipantsInvolved - not stored in entity
            entity.Language
        );
    }
    
    public static SirahEventEntity ToEntity(this SirahEvent sirahEvent)
    {
        return new SirahEventEntity
        {
            EventName = sirahEvent.Name,
            Description = sirahEvent.Description,
            Period = (int)sirahEvent.Period,
            EventDate = sirahEvent.ApproximateDate,
            Context = sirahEvent.Location,
            KeyLessons = string.Join(",", sirahEvent.KeyLessons),
            Language = sirahEvent.Language
        };
    }
    
    #endregion
    
    #region CommonMistake Mapping
      public static CommonMistake ToCommonMistake(this CommonMistakeEntity entity)
    {
        return new CommonMistake(
            entity.MistakeType,
            entity.Description,
            "", // TypicalError - not stored separately in entity
            entity.Correction ?? ""
        );
    }
    
    public static CommonMistakeEntity ToEntity(this CommonMistake mistake)
    {
        return new CommonMistakeEntity
        {
            MistakeType = mistake.Type,
            Description = mistake.Description,
            Correction = mistake.Correction,
            Language = "en" // Default language
        };
    }
    
    #endregion
}
