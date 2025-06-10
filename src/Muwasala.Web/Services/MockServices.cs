using Muwasala.Core.Models;
using Muwasala.KnowledgeBase.Services;

namespace Muwasala.Web.Services;

// Mock implementations for Blazor web application
public class MockQuranService : IQuranService
{
    public async Task<List<QuranVerse>> SearchVersesByContextAsync(string context, string language = "en")
    {
        await Task.Delay(50);
        return new List<QuranVerse>
        {
            new QuranVerse(2, 286, "لَا يُكَلِّفُ اللَّهُ نَفْسًا إِلَّا وُسْعَهَا", "Allah does not charge a soul except [with that within] its capacity.", null, language),
            new QuranVerse(65, 2, "وَمَن يَتَّقِ اللَّهَ يَجْعَل لَّهُ مَخْرَجًا", "And whoever fears Allah - He will make for him a way out", null, language),
            new QuranVerse(3, 159, "فَبِمَا رَحْمَةٍ مِّنَ اللَّهِ لِنتَ لَهُمْ", "So by mercy from Allah, [O Muhammad], you were lenient with them", null, language)
        };
    }

    public async Task<List<QuranVerse>> SearchVersesByThemeAsync(string theme, string language = "en", int maxResults = 10)
    {
        await Task.Delay(50);
        
        var verses = new Dictionary<string, List<QuranVerse>>
        {
            ["patience"] = new List<QuranVerse>
            {
                new QuranVerse(2, 155, "وَلَنَبْلُوَنَّكُم بِشَيْءٍ مِّنَ الْخَوْفِ وَالْجُوعِ", "And We will surely test you with something of fear and hunger", null, language),
                new QuranVerse(94, 5, "فَإِنَّ مَعَ الْعُسْرِ يُسْرًا", "For indeed, with hardship [will be] ease.", null, language)
            },
            ["prayer"] = new List<QuranVerse>
            {
                new QuranVerse(2, 45, "وَاسْتَعِينُوا بِالصَّبْرِ وَالصَّلَاةِ", "And seek help through patience and prayer", null, language),
                new QuranVerse(20, 14, "وَأَقِمِ الصَّلَاةَ لِذِكْرِي", "And establish prayer for My remembrance", null, language)
            },
            ["forgiveness"] = new List<QuranVerse>
            {
                new QuranVerse(39, 53, "لَا تَقْنَطُوا مِن رَّحْمَةِ اللَّهِ", "Do not despair of the mercy of Allah", null, language),
                new QuranVerse(42, 40, "وَجَزَاءُ سَيِّئَةٍ سَيِّئَةٌ مِّثْلُهَا", "The recompense for an evil is an evil like thereof", null, language)
            }
        };

        var key = verses.Keys.FirstOrDefault(k => theme.ToLower().Contains(k)) ?? "patience";
        return verses[key].Take(maxResults).ToList();
    }

    public async Task<QuranVerse?> GetVerseAsync(VerseReference verse, string language = "en")
    {
        await Task.Delay(50);
        return new QuranVerse(verse.Surah, verse.Verse, "نَصُّ الآية العربية", "English translation of the verse", null, language);
    }    public async Task<List<QuranVerse>> GetSurahAsync(int surahNumber, string language = "en")
    {
        await Task.Delay(100);
        
        // Sample data for specific surahs
        var surahData = new Dictionary<int, List<QuranVerse>>
        {
            [1] = new List<QuranVerse> // Al-Fatiha
            {
                new QuranVerse(1, 1, "بِسْمِ اللَّهِ الرَّحْمَـٰنِ الرَّحِيمِ", "In the name of Allah, the Entirely Merciful, the Especially Merciful", "Bismillahir Rahmanir Raheem", language),
                new QuranVerse(1, 2, "الْحَمْدُ لِلَّهِ رَبِّ الْعَالَمِينَ", "All praise is due to Allah, Lord of the worlds", "Alhamdulillahi Rabbil Alameen", language),
                new QuranVerse(1, 3, "الرَّحْمَـٰنِ الرَّحِيمِ", "The Entirely Merciful, the Especially Merciful", "Ar-Rahmanir Raheem", language),
                new QuranVerse(1, 4, "مَالِكِ يَوْمِ الدِّينِ", "Sovereign of the Day of Recompense", "Maliki Yawmid Deen", language),
                new QuranVerse(1, 5, "إِيَّاكَ نَعْبُدُ وَإِيَّاكَ نَسْتَعِينُ", "It is You we worship and You we ask for help", "Iyyaka Na'budu wa Iyyaka Nasta'een", language),
                new QuranVerse(1, 6, "اهْدِنَا الصِّرَاطَ الْمُسْتَقِيمَ", "Guide us to the straight path", "Ihdinassiratal Mustaqeem", language),
                new QuranVerse(1, 7, "صِرَاطَ الَّذِينَ أَنْعَمْتَ عَلَيْهِمْ غَيْرِ الْمَغْضُوبِ عَلَيْهِمْ وَلَا الضَّالِّينَ", "The path of those upon whom You have bestowed favor, not of those who have evoked anger or of those who are astray", "Siratal lazeena an'amta alayhim ghayril maghdubi alayhim wa lad daalleen", language)
            },
            [112] = new List<QuranVerse> // Al-Ikhlas
            {
                new QuranVerse(112, 1, "قُلْ هُوَ اللَّهُ أَحَدٌ", "Say, He is Allah, [who is] One", "Qul huwa Allahu ahad", language),
                new QuranVerse(112, 2, "اللَّهُ الصَّمَدُ", "Allah, the Eternal Refuge", "Allahus samad", language),
                new QuranVerse(112, 3, "لَمْ يَلِدْ وَلَمْ يُولَدْ", "He neither begets nor is born", "Lam yalid wa lam yulad", language),
                new QuranVerse(112, 4, "وَلَمْ يَكُنْ لَهُ كُفُوًا أَحَدٌ", "Nor is there to Him any equivalent", "Wa lam yakun lahu kufuwan ahad", language)
            },
            [113] = new List<QuranVerse> // Al-Falaq
            {
                new QuranVerse(113, 1, "قُلْ أَعُوذُ بِرَبِّ الْفَلَقِ", "Say, I seek refuge in the Lord of daybreak", "Qul a'udhu bi rabbil falaq", language),
                new QuranVerse(113, 2, "مِنْ شَرِّ مَا خَلَقَ", "From the evil of that which He created", "Min sharri ma khalaq", language),
                new QuranVerse(113, 3, "وَمِنْ شَرِّ غَاسِقٍ إِذَا وَقَبَ", "And from the evil of darkness when it settles", "Wa min sharri ghasiqin idha waqab", language),
                new QuranVerse(113, 4, "وَمِنْ شَرِّ النَّفَّاثَاتِ فِي الْعُقَدِ", "And from the evil of the blowers in knots", "Wa min sharrin naffathati fil uqad", language),
                new QuranVerse(113, 5, "وَمِنْ شَرِّ حَاسِدٍ إِذَا حَسَدَ", "And from the evil of an envier when he envies", "Wa min sharri hasidin idha hasad", language)
            },
            [114] = new List<QuranVerse> // An-Nas
            {
                new QuranVerse(114, 1, "قُلْ أَعُوذُ بِرَبِّ النَّاسِ", "Say, I seek refuge in the Lord of mankind", "Qul a'udhu bi rabbin nas", language),
                new QuranVerse(114, 2, "مَلِكِ النَّاسِ", "The Sovereign of mankind", "Malikin nas", language),
                new QuranVerse(114, 3, "إِلَـٰهِ النَّاسِ", "The God of mankind", "Ilahin nas", language),
                new QuranVerse(114, 4, "مِنْ شَرِّ الْوَسْوَاسِ الْخَنَّاسِ", "From the evil of the retreating whisperer", "Min sharril waswasil khannas", language),
                new QuranVerse(114, 5, "الَّذِي يُوَسْوِسُ فِي صُدُورِ النَّاسِ", "Who whispers [evil] into the breasts of mankind", "Allazhi yuwaswisu fi sudoorin nas", language),
                new QuranVerse(114, 6, "مِنَ الْجِنَّةِ وَالنَّاسِ", "From among the jinn and mankind", "Minal jinnati wan nas", language)
            }
        };

        // Return specific surah data if available, otherwise return a generic verse
        if (surahData.ContainsKey(surahNumber))
        {
            return surahData[surahNumber];
        }
        
        // Fallback for other surahs
        return new List<QuranVerse>
        {
            new QuranVerse(surahNumber, 1, "بِسْمِ اللَّهِ الرَّحْمَـٰنِ الرَّحِيمِ", "In the name of Allah, the Entirely Merciful, the Especially Merciful", "Bismillahir Rahmanir Raheem", language),
            new QuranVerse(surahNumber, 2, $"نص تجريبي للسورة {surahNumber}", $"Sample text for Surah {surahNumber}", null, language)
        };
    }

    public async Task<List<TafsirEntry>> GetTafsirAsync(VerseReference verse, string source = "IbnKathir")
    {
        await Task.Delay(50);
        return new List<TafsirEntry>
        {
            new TafsirEntry(verse, source, "This verse emphasizes the mercy and wisdom of Allah in His guidance to humanity.", "Ibn Kathir", "en")
        };
    }
}

public class MockHadithService : IHadithService
{
    public async Task<List<HadithRecord>> SearchHadithAsync(string text, string language = "en")
    {
        await Task.Delay(50);
        return new List<HadithRecord>
        {
            new HadithRecord("إِنَّمَا الْأَعْمَالُ بِالنِّيَّاتِ", "Actions are by intentions", HadithGrade.Sahih, "Bukhari", "1", "1", new List<string>{"Umar ibn al-Khattab"}, "Intentions", language),
            new HadithRecord("لَيْسَ الْمُؤْمِنُ الَّذِي يَشْبَعُ وَجَارُهُ جَائِعٌ", "The believer is not one who eats his fill while his neighbor goes hungry", HadithGrade.Sahih, "Al-Adab Al-Mufrad", "2", "112", new List<string>{"Anas ibn Malik"}, "Neighbors", language)
        };
    }

    public async Task<List<HadithRecord>> GetHadithByTopicAsync(string topic, string language = "en", int maxResults = 10)
    {
        await Task.Delay(50);
        return new List<HadithRecord>
        {
            new HadithRecord($"حديث عن {topic}", $"Hadith about {topic}", HadithGrade.Sahih, "Muslim", "1", "101", new List<string>{"Abu Hurairah"}, topic, language)
        };
    }

    public async Task<HadithRecord?> GetHadithByReferenceAsync(string collection, string hadithNumber)
    {
        await Task.Delay(50);
        return new HadithRecord("نص الحديث بالعربية", "Sample hadith text", HadithGrade.Sahih, collection, "1", hadithNumber, new List<string>{"Companion name"}, "General", "en");
    }

    public async Task<List<HadithRecord>> GetAuthenticHadithAsync(string topic, int maxResults = 5)
    {
        await Task.Delay(100);
        return new List<HadithRecord>
        {
            new HadithRecord("حديث صحيح", "Authentic hadith text", HadithGrade.Sahih, "Bukhari", "1", "1", new List<string>{"Authentic chain"}, topic, "en")
        };
    }
}

public class MockFiqhService : IFiqhService
{
    public async Task<List<FiqhRuling>> SearchRulingsAsync(string question, Madhab madhab, string language = "en")
    {
        await Task.Delay(50);
        return new List<FiqhRuling>
        {
            new FiqhRuling(question, $"The ruling regarding {question} is permissible with conditions...", madhab, "Quran and Sunnah evidence", "Classical fiqh texts", new List<string>{"Scholar 1", "Scholar 2"}, language)
        };
    }

    public async Task<List<FiqhRuling>> GetRulingsByTopicAsync(string topic, Madhab madhab, string language = "en")
    {
        await Task.Delay(50);
        return new List<FiqhRuling>
        {
            new FiqhRuling($"Question about {topic}", $"According to the {madhab} school, {topic} requires careful consideration...", madhab, "Quranic and Hadith evidence", "Madhab texts", new List<string>{"Classical scholars"}, language)
        };
    }

    public async Task<List<FiqhComparison>> CompareMadhabRulingsAsync(string topic, string language = "en")
    {
        await Task.Delay(100);
        var rulings = new Dictionary<Madhab, string>
        {
            [Madhab.Hanafi] = $"Hanafi view on {topic}",
            [Madhab.Shafi] = $"Shafi view on {topic}",
            [Madhab.Maliki] = $"Maliki view on {topic}",
            [Madhab.Hanbali] = $"Hanbali view on {topic}"
        };
        
        return new List<FiqhComparison>
        {
            new FiqhComparison(topic, rulings, "Common principles shared by all schools", new List<string>{"Difference 1", "Difference 2"}, language)
        };
    }
}

public class MockDuaService : IDuaService
{
    public async Task<List<DuaRecord>> SearchDuasByOccasionAsync(string occasion, string language = "en", int maxResults = 10)
    {
        await Task.Delay(50);
        return new List<DuaRecord>
        {
            new DuaRecord("أَعُوذُ بِاللَّهِ مِنَ الشَّيْطَانِ الرَّجِيمِ", "I seek refuge in Allah from Satan the accursed", "A'udhu billahi min ash-shaytani'r-rajim", occasion, "Quran and Sunnah", "Protection from evil", language),
            new DuaRecord("بِاسْمِكَ اللَّهُمَّ أَمُوتُ وَأَحْيَا", "In Your name, O Allah, I die and I live", "Bismika Allahumma amutu wa ahya", occasion, "Hadith collection", "Sleep and rest", language)
        };
    }

    public async Task<DuaRecord?> GetSpecificPrayerAsync(SpecificPrayer prayerType, string language = "en")
    {
        await Task.Delay(50);
        return new DuaRecord("اللَّهُمَّ أَعِنِّي عَلَى ذِكْرِكَ وَشُكْرِكَ وَحُسْنِ عِبَادَتِكَ", "O Allah, help me to remember You, thank You, and worship You in the best manner", "Allahumma a'inni 'ala dhikrika wa shukrika wa husni 'ibadatika", prayerType.ToString(), "Prophetic tradition", "Spiritual enhancement", language);
    }

    public async Task<List<DuaRecord>> GetDailyDuasAsync(string timeOfDay, string language = "en")
    {
        await Task.Delay(50);
        return new List<DuaRecord>
        {
            new DuaRecord($"دعاء {timeOfDay}", $"Dua for {timeOfDay}", "Transliteration", timeOfDay, "Daily supplications", "Daily spiritual practice", language)
        };
    }
}

public class MockTajweedService : ITajweedService
{
    public async Task<VerseData?> GetVerseWithTajweedAsync(VerseReference verse)
    {
        await Task.Delay(100);
        return new VerseData(verse.Surah, verse.Verse, "نَصُّ الآية العربية", "English translation", new List<TajweedMarker>
        {
            new TajweedMarker("Idgham", 5, 10, "Merge the noon sakinah with the following letter"),
            new TajweedMarker("Qalqalah", 12, 15, "Pronounced with a slight bounce")
        });
    }

    public async Task<List<CommonMistake>> GetCommonMistakesAsync(VerseReference verse)
    {
        await Task.Delay(50);
        return new List<CommonMistake>
        {
            new CommonMistake("Pronunciation", "Incorrect articulation of heavy letters", "Wrong pronunciation", "Correct pronunciation with emphasis"),
            new CommonMistake("Timing", "Rushing through elongation", "Too fast", "Proper timing for madd rules")
        };
    }

    public async Task<SurahData?> GetSurahForLessonAsync(int surahNumber, RecitationLevel level)
    {
        await Task.Delay(50);
        return new SurahData(surahNumber, "Al-Fatiha", "The Opening", 7, "Mecca", new List<string>{"Praise", "Guidance", "Prayer"});
    }

    public async Task<QiraatData?> GetQiraatDataAsync(VerseReference verse, QiraatType qiraatType)
    {
        await Task.Delay(50);
        return new QiraatData(verse, qiraatType, new List<string>{"Variation 1", "Variation 2"}, new List<string>{"Audio reference 1"});
    }
}

public class MockSirahService : ISirahService
{
    public async Task<List<SirahEvent>> SearchEventsByContextAsync(string context, string language = "en")
    {
        await Task.Delay(50);
        return new List<SirahEvent>
        {
            new SirahEvent("Migration to Medina", "The Prophet's historic migration marked a new chapter in Islamic history", SirahPeriod.EarlyMedina, new DateTime(622, 9, 24), "Medina", new List<string>{"Patience in adversity", "Trust in Allah"}, new List<string>{"Prophet Muhammad", "Abu Bakr"}, language),
            new SirahEvent("Treaty of Hudaybiyyah", "A pivotal peace agreement that demonstrated the Prophet's wisdom in diplomacy", SirahPeriod.MiddleMedina, new DateTime(628, 3, 1), "Hudaybiyyah", new List<string>{"Diplomacy", "Patience"}, new List<string>{"Prophet Muhammad", "Quraysh leaders"}, language)
        };
    }

    public async Task<List<SirahEvent>> GetEventsByPeriodAsync(SirahPeriod period, string language = "en")
    {
        await Task.Delay(50);
        return new List<SirahEvent>
        {
            new SirahEvent($"Event from {period}", $"Significant events that occurred during {period}", period, DateTime.Now.AddYears(-1400), "Various", new List<string>{"Historical lesson"}, new List<string>{"Historical figures"}, language)
        };
    }

    public async Task<SirahEvent?> GetEventByNameAsync(string eventName, string language = "en")
    {
        await Task.Delay(50);
        return new SirahEvent(eventName, $"Detailed description of {eventName}", SirahPeriod.EarlyMecca, DateTime.Now.AddYears(-1400), "Mecca", new List<string>{"Key lesson"}, new List<string>{"Key figures"}, language);
    }

    public async Task<List<PropheticCharacteristic>> GetCharacteristicsAsync(string aspect, string language = "en")
    {
        await Task.Delay(50);
        return new List<PropheticCharacteristic>
        {
            new PropheticCharacteristic(aspect, $"The Prophet exemplified {aspect} in all his dealings", new List<string>{"Example 1", "Example 2"}, new List<string>{"Related hadith 1"}, language)
        };
    }

    public async Task<List<PropheticGuidance>> GetGuidanceByTopicAsync(string topic, string language = "en")
    {
        await Task.Delay(100);
        return new List<PropheticGuidance>
        {
            new PropheticGuidance(topic, $"The Prophet's guidance regarding {topic} emphasizes wisdom and compassion", "Historical context", new List<string>{"Related event"}, new List<string>{"Modern application"}, language)
        };
    }

    public async Task<ChronologicalTimeline> GetTimelineAsync(string language = "en")
    {
        await Task.Delay(100);
        var meccanEvents = new List<SirahEvent>
        {
            new SirahEvent("Birth", "Birth of Prophet Muhammad", SirahPeriod.PreProphethood, new DateTime(570, 4, 22), "Mecca", new List<string>{"Divine mercy"}, new List<string>{"Abdullah", "Aminah"}, language)
        };
        
        var medinanEvents = new List<SirahEvent>
        {
            new SirahEvent("Hijra", "Migration to Medina", SirahPeriod.EarlyMedina, new DateTime(622, 9, 24), "Medina", new List<string>{"New beginning"}, new List<string>{"Prophet", "Abu Bakr"}, language)
        };

        return new ChronologicalTimeline(meccanEvents, medinanEvents, meccanEvents.Concat(medinanEvents).ToList(), language);
    }
}
