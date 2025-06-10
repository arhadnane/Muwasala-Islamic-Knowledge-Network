using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Muwasala.KnowledgeBase.Data.Models;
using Muwasala.Core.Models;

namespace Muwasala.KnowledgeBase.Data;

/// <summary>
/// Entity Framework DbContext for Islamic Knowledge Database
/// </summary>
public class IslamicKnowledgeDbContext : DbContext
{
    private readonly string _connectionString;

    public IslamicKnowledgeDbContext() : this("Data Source=islamic_knowledge.db")
    {
    }

    public IslamicKnowledgeDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IslamicKnowledgeDbContext(DbContextOptions<IslamicKnowledgeDbContext> options) : base(options)
    {
        _connectionString = "Data Source=islamic_knowledge.db";
    }    // DbSets for all entities
    public DbSet<QuranVerseEntity> QuranVerses { get; set; }
    public DbSet<TafsirEntity> Tafsirs { get; set; }
    public DbSet<HadithEntity> HadithRecords { get; set; }
    public DbSet<FiqhRulingEntity> FiqhRulings { get; set; }
    public DbSet<DuaEntity> DuaRecords { get; set; }
    public DbSet<SirahEventEntity> SirahEvents { get; set; }
    public DbSet<TajweedRuleEntity> TajweedRules { get; set; }
    public DbSet<VerseTajweedEntity> VerseTajweedData { get; set; }
    public DbSet<CommonMistakeEntity> CommonMistakes { get; set; }
    
    // Search History DbSets
    public DbSet<SearchHistoryEntity> SearchHistory { get; set; }
    public DbSet<SearchResultHistoryEntity> SearchResultHistory { get; set; }
    public DbSet<SearchAnalyticsEntity> SearchAnalytics { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure QuranVerse entity
        modelBuilder.Entity<QuranVerseEntity>(entity =>
        {
            entity.HasIndex(e => new { e.SurahNumber, e.VerseNumber })
                  .IsUnique()
                  .HasDatabaseName("IX_QuranVerses_SurahVerse");
            
            entity.HasIndex(e => e.Language)
                  .HasDatabaseName("IX_QuranVerses_Language");
            
            entity.HasIndex(e => e.Theme)
                  .HasDatabaseName("IX_QuranVerses_Theme");
            
            entity.HasIndex(e => e.Keywords)
                  .HasDatabaseName("IX_QuranVerses_Keywords");
        });

        // Configure Tafsir entity
        modelBuilder.Entity<TafsirEntity>(entity =>
        {
            entity.HasOne(t => t.QuranVerse)
                  .WithMany(q => q.Tafsirs)
                  .HasForeignKey(t => t.QuranVerseId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.Source)
                  .HasDatabaseName("IX_Tafsirs_Source");
            
            entity.HasIndex(e => e.Language)
                  .HasDatabaseName("IX_Tafsirs_Language");
        });

        // Configure Hadith entity
        modelBuilder.Entity<HadithEntity>(entity =>
        {
            entity.HasIndex(e => e.Collection)
                  .HasDatabaseName("IX_HadithRecords_Collection");
            
            entity.HasIndex(e => new { e.Collection, e.BookNumber, e.HadithNumber })
                  .HasDatabaseName("IX_HadithRecords_Reference");
            
            entity.HasIndex(e => e.Grade)
                  .HasDatabaseName("IX_HadithRecords_Grade");
            
            entity.HasIndex(e => e.Topic)
                  .HasDatabaseName("IX_HadithRecords_Topic");
            
            entity.HasIndex(e => e.Keywords)
                  .HasDatabaseName("IX_HadithRecords_Keywords");
        });

        // Configure FiqhRuling entity
        modelBuilder.Entity<FiqhRulingEntity>(entity =>
        {
            entity.HasIndex(e => e.Madhab)
                  .HasDatabaseName("IX_FiqhRulings_Madhab");
            
            entity.HasIndex(e => e.Topic)
                  .HasDatabaseName("IX_FiqhRulings_Topic");
            
            entity.HasIndex(e => e.Keywords)
                  .HasDatabaseName("IX_FiqhRulings_Keywords");
        });

        // Configure Dua entity
        modelBuilder.Entity<DuaEntity>(entity =>
        {
            entity.HasIndex(e => e.Occasion)
                  .HasDatabaseName("IX_DuaRecords_Occasion");
            
            entity.HasIndex(e => e.TimeOfDay)
                  .HasDatabaseName("IX_DuaRecords_TimeOfDay");
            
            entity.HasIndex(e => e.SpecificPrayerType)
                  .HasDatabaseName("IX_DuaRecords_SpecificPrayerType");
            
            entity.HasIndex(e => e.Keywords)
                  .HasDatabaseName("IX_DuaRecords_Keywords");
        });

        // Configure SirahEvent entity
        modelBuilder.Entity<SirahEventEntity>(entity =>
        {
            entity.HasIndex(e => e.Period)
                  .HasDatabaseName("IX_SirahEvents_Period");
            
            entity.HasIndex(e => e.EventName)
                  .HasDatabaseName("IX_SirahEvents_EventName");
            
            entity.HasIndex(e => e.Keywords)
                  .HasDatabaseName("IX_SirahEvents_Keywords");
            
            entity.HasIndex(e => e.EventDate)
                  .HasDatabaseName("IX_SirahEvents_EventDate");
        });

        // Configure TajweedRule entity
        modelBuilder.Entity<TajweedRuleEntity>(entity =>
        {
            entity.HasIndex(e => e.Name)
                  .HasDatabaseName("IX_TajweedRules_Name");
            
            entity.HasIndex(e => e.Category)
                  .HasDatabaseName("IX_TajweedRules_Category");
            
            entity.HasIndex(e => e.Keywords)
                  .HasDatabaseName("IX_TajweedRules_Keywords");
        });

        // Configure VerseTajweed entity
        modelBuilder.Entity<VerseTajweedEntity>(entity =>
        {
            entity.HasOne(vt => vt.TajweedRule)
                  .WithMany()
                  .HasForeignKey(vt => vt.TajweedRuleId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.SurahNumber, e.VerseNumber })
                  .HasDatabaseName("IX_VerseTajweedData_SurahVerse");
            
            entity.HasIndex(e => e.TajweedRuleId)
                  .HasDatabaseName("IX_VerseTajweedData_TajweedRuleId");
        });        // Configure CommonMistake entity
        modelBuilder.Entity<CommonMistakeEntity>(entity =>
        {
            entity.HasIndex(e => new { e.SurahNumber, e.VerseNumber })
                  .HasDatabaseName("IX_CommonMistakes_SurahVerse");
            
            entity.HasIndex(e => e.MistakeType)
                  .HasDatabaseName("IX_CommonMistakes_MistakeType");
        });

        // Configure SearchHistory entity
        modelBuilder.Entity<SearchHistoryEntity>(entity =>
        {
            entity.HasIndex(e => e.SearchDateTime)
                  .HasDatabaseName("IX_SearchHistory_SearchDateTime");
            
            entity.HasIndex(e => e.SearchQuery)
                  .HasDatabaseName("IX_SearchHistory_SearchQuery");
            
            entity.HasIndex(e => e.SearchMode)
                  .HasDatabaseName("IX_SearchHistory_SearchMode");
            
            entity.HasIndex(e => e.Language)
                  .HasDatabaseName("IX_SearchHistory_Language");
            
            entity.HasIndex(e => e.UserIdentifier)
                  .HasDatabaseName("IX_SearchHistory_UserIdentifier");
        });

        // Configure SearchResultHistory entity
        modelBuilder.Entity<SearchResultHistoryEntity>(entity =>
        {
            entity.HasOne(sr => sr.SearchHistory)
                  .WithMany(sh => sh.SearchResults)
                  .HasForeignKey(sr => sr.SearchHistoryId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.SearchHistoryId)
                  .HasDatabaseName("IX_SearchResultHistory_SearchHistoryId");
            
            entity.HasIndex(e => e.ContentType)
                  .HasDatabaseName("IX_SearchResultHistory_ContentType");
            
            entity.HasIndex(e => e.RelevanceScore)
                  .HasDatabaseName("IX_SearchResultHistory_RelevanceScore");
        });

        // Configure SearchAnalytics entity
        modelBuilder.Entity<SearchAnalyticsEntity>(entity =>
        {
            entity.HasIndex(e => e.SearchTerm)
                  .HasDatabaseName("IX_SearchAnalytics_SearchTerm");
            
            entity.HasIndex(e => e.Date)
                  .HasDatabaseName("IX_SearchAnalytics_Date");
            
            entity.HasIndex(e => e.Language)
                  .HasDatabaseName("IX_SearchAnalytics_Language");
        });
    }

    /// <summary>
    /// Initializes the database with schema creation
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        await Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Seeds the database with initial data if empty
    /// </summary>
    public async Task SeedDataAsync()
    {
        if (!await QuranVerses.AnyAsync())
        {
            await SeedQuranDataAsync();
        }

        if (!await HadithRecords.AnyAsync())
        {
            await SeedHadithDataAsync();
        }

        if (!await FiqhRulings.AnyAsync())
        {
            await SeedFiqhDataAsync();
        }

        if (!await DuaRecords.AnyAsync())
        {
            await SeedDuaDataAsync();
        }

        if (!await SirahEvents.AnyAsync())
        {
            await SeedSirahDataAsync();
        }

        if (!await TajweedRules.AnyAsync())
        {
            await SeedTajweedDataAsync();
        }

        await SaveChangesAsync();
    }

    private async Task SeedQuranDataAsync()
    {
        var verses = new List<QuranVerseEntity>
        {
            new QuranVerseEntity
            {
                SurahNumber = 2,
                VerseNumber = 155,
                ArabicText = "وَلَنَبْلُوَنَّكُم بِشَيْءٍ مِّنَ الْخَوْفِ وَالْجُوعِ وَنَقْصٍ مِّنَ الْأَمْوَالِ وَالْأَنفُسِ وَالثَّمَرَاتِ ۗ وَبَشِّرِ الصَّابِرِينَ",
                Translation = "And We will surely test you with something of fear and hunger and a loss of wealth and lives and fruits, but give good tidings to the patient.",
                Transliteration = "Wa lanabluwannakum bishay'in minal-khawfi wal-ju'i wa naqsin minal-amwali wal-anfusi wal-thamarati wa bashshiris-sabirin",
                Keywords = "patience,trials,hardship,test,sabr,perseverance",
                Theme = "patience,trials",
                Context = "Trials and patience in adversity"
            },
            new QuranVerseEntity
            {
                SurahNumber = 2,
                VerseNumber = 156,
                ArabicText = "الَّذِينَ إِذَا أَصَابَتْهُم مُّصِيبَةٌ قَالُوا إِنَّا لِلَّهِ وَإِنَّا إِلَيْهِ رَاجِعُونَ",
                Translation = "Who, when disaster strikes them, say, 'Indeed we belong to Allah, and indeed to Him we will return.'",
                Transliteration = "Alladhina idha asabat-hum musibatun qalu inna lillahi wa inna ilayhi raji'un",
                Keywords = "patience,disaster,return to Allah,inna lillahi",
                Theme = "patience,trials",
                Context = "Response to trials and difficulties"
            },
            new QuranVerseEntity
            {
                SurahNumber = 1,
                VerseNumber = 1,
                ArabicText = "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ",
                Translation = "In the name of Allah, the Entirely Merciful, the Especially Merciful.",
                Transliteration = "Bismillahir-Rahmanir-Rahim",
                Keywords = "mercy,compassion,rahman,rahim,bismillah",
                Theme = "mercy,compassion",
                Context = "Opening of Quran and prayers"
            },
            new QuranVerseEntity
            {
                SurahNumber = 2,
                VerseNumber = 2,
                ArabicText = "ذَٰلِكَ الْكِتَابُ لَا رَيْبَ ۛ فِيهِ ۛ هُدًى لِّلْمُتَّقِينَ",
                Translation = "This is the Book about which there is no doubt, a guidance for those conscious of Allah.",
                Transliteration = "Dhalika al-kitabu la rayba fihi hudan lil-muttaqin",
                Keywords = "guidance,book,doubt,muttaqin,huda",
                Theme = "guidance",
                Context = "Description of the Quran as guidance"
            },
            new QuranVerseEntity
            {
                SurahNumber = 5,
                VerseNumber = 3,
                ArabicText = "الْيَوْمَ أَكْمَلْتُ لَكُمْ دِينَكُمْ وَأَتْمَمْتُ عَلَيْكُمْ نِعْمَتِي وَرَضِيتُ لَكُمُ الْإِسْلَامَ دِينًا",
                Translation = "This day I have perfected for you your religion and completed My favor upon you and have approved for you Islam as religion.",
                Transliteration = "Al-yawma akmaltu lakum dinakum wa atmamtu alaykum ni'mati wa raditu lakumu al-islama dinan",
                Keywords = "perfection,completion,Islam,religion,favor",
                Theme = "completion of religion",
                Context = "Completion of Islam as a religion"
            }
        };

        await QuranVerses.AddRangeAsync(verses);
    }

    private async Task SeedHadithDataAsync()
    {
        var hadiths = new List<HadithEntity>
        {
            new HadithEntity
            {
                ArabicText = "إِنَّمَا الْأَعْمَالُ بِالنِّيَّاتِ",
                Translation = "Actions are but by intention.",
                Collection = "Sahih al-Bukhari",
                BookNumber = "1",
                HadithNumber = "1",
                Grade = (int)HadithGrade.Sahih,
                Topic = "intention,actions,niyyah",
                Keywords = "intention,niyyah,actions,deeds",
                Explanation = "This hadith emphasizes that the value of actions depends on the intention behind them."
            },
            new HadithEntity
            {
                ArabicText = "مَنْ صَبَرَ ظَفِرَ",
                Translation = "Whoever is patient will be victorious.",
                Collection = "Various Collections",
                Grade = (int)HadithGrade.Hasan,
                Topic = "patience,perseverance,victory",
                Keywords = "patience,sabr,victory,perseverance",
                Explanation = "This hadith teaches that patience leads to success and victory."
            }
        };

        await HadithRecords.AddRangeAsync(hadiths);
    }

    private async Task SeedFiqhDataAsync()
    {
        var fiqhRulings = new List<FiqhRulingEntity>
        {
            new FiqhRulingEntity
            {
                Question = "What is the ruling on prayer during travel?",
                Ruling = "It is permissible to shorten and combine prayers during travel according to the Sunnah.",
                Madhab = (int)Core.Models.Madhab.Hanafi,
                Evidence = "Based on the practice of Prophet Muhammad (peace be upon him) during his travels.",
                Topic = "prayer,travel,qasr",
                Keywords = "prayer,travel,qasr,shortening,combining",
                ModernApplication = "Applies to modern travel by car, plane, or other means of transportation."
            }
        };

        await FiqhRulings.AddRangeAsync(fiqhRulings);
    }

    private async Task SeedDuaDataAsync()
    {
        var duas = new List<DuaEntity>
        {
            new DuaEntity
            {
                ArabicText = "رَبَّنَا آتِنَا فِي الدُّنْيَا حَسَنَةً وَفِي الْآخِرَةِ حَسَنَةً وَقِنَا عَذَابَ النَّارِ",
                Translation = "Our Lord, give us good in this world and good in the next world, and save us from the punishment of the Fire.",
                Transliteration = "Rabbana atina fi'd-dunya hasanatan wa fi'l-akhirati hasanatan wa qina 'adhab an-nar",
                Occasion = "General supplication",
                Source = "Quran 2:201",
                Benefits = "Comprehensive dua for this world and the hereafter",
                Keywords = "general,comprehensive,dunya,akhirah,hasanah"
            }
        };

        await DuaRecords.AddRangeAsync(duas);
    }

    private async Task SeedSirahDataAsync()
    {
        var sirahEvents = new List<SirahEventEntity>
        {
            new SirahEventEntity
            {
                EventName = "The First Revelation",
                Description = "The Prophet Muhammad (peace be upon him) received his first revelation in the Cave of Hira through the Angel Gabriel.",
                Period = (int)SirahPeriod.EarlyMecca,
                KeyLessons = "The importance of seeking solitude for reflection and the beginning of the prophetic mission.",
                ModernApplication = "The value of meditation, reflection, and spiritual seeking in our daily lives.",
                PropheticWisdom = "Seek knowledge and spiritual growth through contemplation and connection with Allah.",
                Keywords = "revelation,cave,hira,gabriel,beginning",
                Context = "Beginning of prophethood"
            }
        };

        await SirahEvents.AddRangeAsync(sirahEvents);
    }

    private async Task SeedTajweedDataAsync()
    {
        var tajweedRules = new List<TajweedRuleEntity>
        {
            new TajweedRuleEntity
            {
                Name = "Noon Sakinah and Tanween",
                Description = "Rules for pronouncing noon sakinah and tanween before different letters.",
                Example = "مِن كُلِّ - Idgham without ghunnah",
                Category = "Basic Rules",
                Keywords = "noon,sakinah,tanween,idgham,ikhfa"
            }
        };

        await TajweedRules.AddRangeAsync(tajweedRules);
    }
}
