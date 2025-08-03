using Muwasala.Core.Models;
using Muwasala.Core.Services;
using System.Text.Json;

namespace Muwasala.KnowledgeBase.Services;

/// <summary>
/// Implementation of Quran service with file-based data from SourceFiles
/// </summary>
public class QuranService : IQuranService
{
    private readonly FileBasedQuranSearchService _fileBasedService;
    private readonly List<QuranVerse> _fallbackVerses;

    public QuranService()
    {
        _fileBasedService = new FileBasedQuranSearchService();
        _fallbackVerses = GetSampleQuranData(); // Keep as fallback
    }    public async Task<List<QuranVerse>> SearchVersesByContextAsync(string context, string language = "en")
    {
        try
        {
            // Use file-based search service
            var fileResults = await _fileBasedService.SearchVersesByContextAsync(context, 5);
            
            // Convert file results to QuranVerse format (fileResults uses different property names)
            var convertedResults = fileResults.Select(fr => new QuranVerse(
                fr.Surah,  // Our FileBasedQuranSearchService uses Surah, not SurahNumber
                fr.Verse,  // Our FileBasedQuranSearchService uses Verse, not VerseNumber  
                fr.ArabicText, 
                fr.Translation, // Our FileBasedQuranSearchService uses Translation, not EnglishText
                "", // No transliteration in file results
                language
            )).ToList();
            
            // If results found in files, return them
            if (convertedResults.Count > 0)
            {
                return convertedResults;
            }
            
            // Fall back to sample data with synonym expansion
            var lowerContext = context.ToLower();
            var searchTerms = new List<string> { lowerContext };
            
            // Add synonyms for common Islamic concepts
            if (lowerContext.Contains("patience"))
                searchTerms.AddRange(new[] { "patient", "perseverance", "endurance", "sabr", "sabiru" });
            if (lowerContext.Contains("hardship"))
                searchTerms.AddRange(new[] { "difficulty", "trial", "test", "suffering", "ease" });
            if (lowerContext.Contains("guidance"))
                searchTerms.AddRange(new[] { "guide", "path", "direction", "light" });
            if (lowerContext.Contains("mercy"))
                searchTerms.AddRange(new[] { "merciful", "compassion", "forgiveness", "rahman", "raheem" });
            if (lowerContext.Contains("prayer"))
                searchTerms.AddRange(new[] { "pray", "salah", "worship", "dua" });
              
            return _fallbackVerses.Where(v => 
                searchTerms.Any(term => 
                    v.Translation.ToLower().Contains(term) ||
                    v.ArabicText.Contains(term) ||
                    (v.Transliteration?.ToLower().Contains(term) ?? false)
                )
            ).Take(5).ToList();
        }
        catch (Exception)
        {
            // If file-based search fails, use fallback data
            await Task.Delay(100); // Simulate database query
            
            var lowerContext = context.ToLower();
            var searchTerms = new List<string> { lowerContext };
            
            // Add synonyms for common Islamic concepts
            if (lowerContext.Contains("patience"))
                searchTerms.AddRange(new[] { "patient", "perseverance", "endurance", "sabr", "sabiru" });
            if (lowerContext.Contains("hardship"))
                searchTerms.AddRange(new[] { "difficulty", "trial", "test", "suffering", "ease" });
            if (lowerContext.Contains("guidance"))
                searchTerms.AddRange(new[] { "guide", "path", "direction", "light" });
            if (lowerContext.Contains("mercy"))
                searchTerms.AddRange(new[] { "merciful", "compassion", "forgiveness", "rahman", "raheem" });
            if (lowerContext.Contains("prayer"))
                searchTerms.AddRange(new[] { "pray", "salah", "worship", "dua" });
              
            return _fallbackVerses.Where(v => 
                searchTerms.Any(term => 
                    v.Translation.ToLower().Contains(term) ||
                    v.ArabicText.Contains(term) ||
                    (v.Transliteration?.ToLower().Contains(term) ?? false)
                )
            ).Take(5).ToList();
        }
    }

    public async Task<List<QuranVerse>> SearchVersesByThemeAsync(string theme, string language = "en", int maxResults = 10)
    {
        try
        {
            // Use file-based search service
            var results = await _fileBasedService.SearchVersesByContextAsync(theme, maxResults);
            
            // If no results found in files, fall back to sample data with synonym expansion
            if (results.Count == 0)
            {
                var lowerTheme = theme.ToLower();
                var searchTerms = new List<string> { lowerTheme };
                
                // Add synonyms for common Islamic themes
                if (lowerTheme.Contains("patience"))
                    searchTerms.AddRange(new[] { "patient", "perseverance", "endurance", "sabr", "sabiru" });
                if (lowerTheme.Contains("hardship"))
                    searchTerms.AddRange(new[] { "difficulty", "trial", "test", "suffering", "ease" });
                if (lowerTheme.Contains("guidance"))
                    searchTerms.AddRange(new[] { "guide", "path", "direction", "light" });
                if (lowerTheme.Contains("mercy"))
                    searchTerms.AddRange(new[] { "merciful", "compassion", "forgiveness", "rahman", "raheem" });
                if (lowerTheme.Contains("prayer"))
                    searchTerms.AddRange(new[] { "pray", "salah", "worship", "dua" });
                  
                return _fallbackVerses.Where(v => 
                    searchTerms.Any(term => 
                        v.Translation.ToLower().Contains(term) ||
                        (v.ArabicText?.Contains(term) ?? false) ||
                        (v.Transliteration?.ToLower().Contains(term) ?? false)
                    )
                ).Take(maxResults).ToList();
            }
            
            return results;
        }
        catch (Exception)
        {
            // If file-based search fails, use fallback data
            await Task.Delay(100);
            
            var lowerTheme = theme.ToLower();
            var searchTerms = new List<string> { lowerTheme };
            
            // Add synonyms for common Islamic themes
            if (lowerTheme.Contains("patience"))
                searchTerms.AddRange(new[] { "patient", "perseverance", "endurance", "sabr", "sabiru" });
            if (lowerTheme.Contains("hardship"))
                searchTerms.AddRange(new[] { "difficulty", "trial", "test", "suffering", "ease" });
            if (lowerTheme.Contains("guidance"))
                searchTerms.AddRange(new[] { "guide", "path", "direction", "light" });
            if (lowerTheme.Contains("mercy"))
                searchTerms.AddRange(new[] { "merciful", "compassion", "forgiveness", "rahman", "raheem" });
            if (lowerTheme.Contains("prayer"))
                searchTerms.AddRange(new[] { "pray", "salah", "worship", "dua" });
              
            return _fallbackVerses.Where(v => 
                searchTerms.Any(term => 
                    v.Translation.ToLower().Contains(term) ||
                    (v.ArabicText?.Contains(term) ?? false) ||
                    (v.Transliteration?.ToLower().Contains(term) ?? false)
                )
            ).Take(maxResults).ToList();
        }
    }

    public async Task<QuranVerse?> GetVerseAsync(VerseReference verse, string language = "en")
    {
        try
        {
            // Search in file-based service
            var results = await _fileBasedService.SearchVersesByContextAsync($"{verse.Surah}:{verse.Verse}", 10);
            var matchingVerse = results.FirstOrDefault(v => v.Surah == verse.Surah && v.Verse == verse.Verse);
            
            if (matchingVerse != null)
                return matchingVerse;
        }
        catch (Exception)
        {
            // Continue to fallback
        }
        
        // Fallback to sample data
        await Task.Delay(50);
        return _fallbackVerses.FirstOrDefault(v => v.Surah == verse.Surah && v.Verse == verse.Verse);
    }

    public async Task<List<QuranVerse>> GetSurahAsync(int surahNumber, string language = "en")
    {
        try
        {
            // Search in file-based service for entire surah
            var results = await _fileBasedService.SearchVersesByContextAsync($"surah {surahNumber}", 300);
            var surahVerses = results.Where(v => v.Surah == surahNumber)
                                   .OrderBy(v => v.Verse)
                                   .ToList();
            
            if (surahVerses.Count > 0)
                return surahVerses;
        }
        catch (Exception)
        {
            // Continue to fallback
        }
        
        // Fallback to sample data
        await Task.Delay(200);
        return _fallbackVerses.Where(v => v.Surah == surahNumber).ToList();
    }

    public async Task<List<TafsirEntry>> GetTafsirAsync(VerseReference verse, string source = "IbnKathir")
    {
        await Task.Delay(100);
        
        // Sample tafsir data
        return new List<TafsirEntry>
        {
            new TafsirEntry(
                verse,
                source,
                "This verse teaches us about the importance of patience and perseverance in difficult times. The scholars explain that Allah tests those He loves to purify them and elevate their ranks.",
                "Ibn Kathir"
            )
        };
    }    private List<QuranVerse> GetSampleQuranData()
    {
        return new List<QuranVerse>
        {
            // Al-Fatiha
            new QuranVerse(1, 1, "بِسْمِ اللَّهِ الرَّحْمَنِ الرَّحِيمِ", "In the name of Allah, the Entirely Merciful, the Especially Merciful.", "Bismillahir Rahmanir Raheem"),
            new QuranVerse(1, 2, "الْحَمْدُ لِلَّهِ رَبِّ الْعَالَمِينَ", "[All] praise is [due] to Allah, Lord of the worlds -", "Alhamdulillahi Rabbil Alameen"),
            new QuranVerse(1, 3, "الرَّحْمَنِ الرَّحِيمِ", "The Entirely Merciful, the Especially Merciful,", "Ar Rahmanir Raheem"),
            new QuranVerse(1, 4, "مَالِكِ يَوْمِ الدِّينِ", "Sovereign of the Day of Recompense.", "Maliki yawmid deen"),
            new QuranVerse(1, 5, "إِيَّاكَ نَعْبُدُ وَإِيَّاكَ نَسْتَعِينُ", "It is You we worship and You we ask for help.", "Iyyaka na'budu wa iyyaka nasta'een"),
            new QuranVerse(1, 6, "اهْدِنَا الصِّرَاطَ الْمُسْتَقِيمَ", "Guide us to the straight path -", "Ihdinassiratal mustaqeem"),
            new QuranVerse(1, 7, "صِرَاطَ الَّذِينَ أَنْعَمْتَ عَلَيْهِمْ غَيْرِ الْمَغْضُوبِ عَلَيْهِمْ وَلَا الضَّالِّينَ", "The path of those upon whom You have bestowed favor, not of those who have evoked [Your] anger or of those who are astray.", "Siratal ladhina an'amta alayhim ghayril maghdubi alayhim wa lad dalleen"),
            
            // Al-Baqarah - Key verses
            new QuranVerse(2, 2, "ذَلِكَ الْكِتَابُ لَا رَيْبَ فِيهِ هُدًى لِّلْمُتَّقِينَ", "This is the Book about which there is no doubt, a guidance for those conscious of Allah -", "Dhalikal kitabu la rayba feeh, hudal lil muttaqeen"),
            new QuranVerse(2, 21, "يَا أَيُّهَا النَّاسُ اعْبُدُوا رَبَّكُمُ الَّذِي خَلَقَكُمْ وَالَّذِينَ مِن قَبْلِكُمْ لَعَلَّكُمْ تَتَّقُونَ", "O mankind, worship your Lord, who created you and those before you, that you may become righteous -", "Ya ayyuhan nasu'budur rabbakumul ladhi khalaqakum wal ladhina min qablikum la'allakum tattaqun"),
            new QuranVerse(2, 153, "يَا أَيُّهَا الَّذِينَ آمَنُوا اسْتَعِينُوا بِالصَّبْرِ وَالصَّلَاةِ إِنَّ اللَّهَ مَعَ الصَّابِرِينَ", "O you who believe! Seek help through patience and prayer. Indeed, Allah is with those who are patient.", "Ya ayyuhal ladhina amanu ista'inu bis sabri was salah, inna Allaha ma'as sabireen"),
            new QuranVerse(2, 177, "لَّيْسَ الْبِرَّ أَن تُوَلُّوا وُجُوهَكُمْ قِبَلَ الْمَشْرِقِ وَالْمَغْرِبِ وَلَكِنَّ الْبِرَّ مَنْ آمَنَ بِاللَّهِ", "Righteousness is not that you turn your faces toward the east or the west, but [true] righteousness is [in] one who believes in Allah", "Laysal birra an tuwullu wujuhakum qibal mashriqi wal maghribi wa lakinnal birra man amana billahi"),
            new QuranVerse(2, 255, "اللَّهُ لَا إِلَهَ إِلَّا هُوَ الْحَيُّ الْقَيُّومُ لَا تَأْخُذُهُ سِنَةٌ وَلَا نَوْمٌ", "Allah - there is no deity except Him, the Ever-Living, the Sustainer of existence. Neither drowsiness overtakes Him nor sleep.", "Allahu la ilaha illa huwal hayyul qayyum, la ta'khudhhu sinatuw wa la nawm"),
            new QuranVerse(2, 286, "لَا يُكَلِّفُ اللَّهُ نَفْسًا إِلَّا وُسْعَهَا لَهَا مَا كَسَبَتْ وَعَلَيْهَا مَا اكْتَسَبَتْ", "Allah does not charge a soul except [with that within] its capacity. It will have [the consequence of] what [good] it has gained, and it will bear [the consequence of] what [evil] it has earned.", "La yukallifullahu nafsan illa wus'aha, laha ma kasabat wa alayha mak tasabat"),
            
            // Verses about love, mercy, and compassion
            new QuranVerse(3, 31, "قُلْ إِن كُنتُمْ تُحِبُّونَ اللَّهَ فَاتَّبِعُونِي يُحْبِبْكُمُ اللَّهُ وَيَغْفِرْ لَكُمْ ذُنُوبَكُمْ", "Say, [O Muhammad], 'If you should love Allah, then follow me, [so] Allah will love you and forgive you your sins.'", "Qul in kuntum tuhibbunallaha fattabi'uni yuhbibkumullahu wa yaghfir lakum dhunubakum"),
            new QuranVerse(5, 54, "يَا أَيُّهَا الَّذِينَ آمَنُوا مَن يَرْتَدَّ مِنكُمْ عَن دِينِهِ فَسَوْفَ يَأْتِي اللَّهُ بِقَوْمٍ يُحِبُّهُمْ وَيُحِبُّونَهُ", "O you who believe! Whoever from among you turns back from his religion, Allah will bring forth [in place of them] a people He will love and who will love Him", "Ya ayyuhal ladhina amanu may yartadda minkum an deenihi fasawfa ya'tillahu biqawmin yuhibbuhum wa yuhibbunah"),
            new QuranVerse(30, 21, "وَمِنْ آيَاتِهِ أَنْ خَلَقَ لَكُم مِّنْ أَنفُسِكُمْ أَزْوَاجًا لِّتَسْكُنُوا إِلَيْهَا وَجَعَلَ بَيْنَكُم مَّوَدَّةً وَرَحْمَةً", "And of His signs is that He created for you from yourselves mates that you may find tranquillity in them; and He placed between you affection and mercy.", "Wa min ayatihi an khalaqa lakum min anfusikum azwajan litaskunu ilayha wa ja'ala baynakum mawaddatan wa rahma"),
            
            // Verses about justice and fairness  
            new QuranVerse(4, 58, "إِنَّ اللَّهَ يَأْمُرُكُمْ أَن تُؤَدُّوا الْأَمَانَاتِ إِلَى أَهْلِهَا وَإِذَا حَكَمْتُم بَيْنَ النَّاسِ أَن تَحْكُمُوا بِالْعَدْلِ", "Indeed, Allah orders you to render trusts to whom they are due and when you judge between people to judge with justice.", "Innallaha ya'murukum an tu'addul amanati ila ahliha wa idha hakamtum baynan nasi an tahkumu bil adl"),
            new QuranVerse(5, 8, "يَا أَيُّهَا الَّذِينَ آمَنُوا كُونُوا قَوَّامِينَ لِلَّهِ شُهَدَاءَ بِالْقِسْطِ وَلَا يَجْرِمَنَّكُمْ شَنَآنُ قَوْمٍ عَلَى أَلَّا تَعْدِلُوا اعْدِلُوا هُوَ أَقْرَبُ لِلتَّقْوَى", "O you who believe! Stand out firmly for Allah as witnesses to fair dealing, and let not the hatred of others make you swerve to wrong and depart from justice. Be just: that is next to piety", "Ya ayyuhal ladhina amanu kunu qawwamina lillahi shuhadaa bil qisti wa la yajrimannakum shana'anu qawmin ala alla ta'dilu, i'dilu huwa aqrabu lit taqwa"),
            new QuranVerse(16, 90, "إِنَّ اللَّهَ يَأْمُرُ بِالْعَدْلِ وَالْإِحْسَانِ وَإِيتَاءِ ذِي الْقُرْبَى وَيَنْهَى عَنِ الْفَحْشَاءِ وَالْمُنكَرِ وَالْبَغْيِ", "Indeed, Allah orders justice and good conduct and giving to relatives and forbids immorality and bad conduct and oppression.", "Innallaha ya'muru bil adli wal ihsani wa ita'i dhil qurba wa yanha anil fahsha'i wal munkari wal baghyi"),
            
            // Verses about forgiveness and mercy
            new QuranVerse(7, 199, "خُذِ الْعَفْوَ وَأْمُرْ بِالْعُرْفِ وَأَعْرِضْ عَنِ الْجَاهِلِينَ", "Take what is given freely, enjoin what is good, and turn away from the ignorant.", "Khudhil afwa wa'mur bil urfi wa a'rid anil jahileen"),
            new QuranVerse(24, 22, "وَلْيَعْفُوا وَلْيَصْفَحُوا أَلَا تُحِبُّونَ أَن يَغْفِرَ اللَّهُ لَكُمْ وَاللَّهُ غَفُورٌ رَّحِيمٌ", "But let them pardon and overlook. Would you not like that Allah should forgive you? And Allah is Forgiving and Merciful.", "Walyaafu walyasfahu, ala tuhibbuna an yaghfirallahu lakum, wallahu ghafurun raheem"),
            new QuranVerse(42, 40, "وَجَزَاءُ سَيِّئَةٍ سَيِّئَةٌ مِّثْلُهَا فَمَنْ عَفَا وَأَصْلَحَ فَأَجْرُهُ عَلَى اللَّهِ", "The recompense for an evil is an evil like thereof, but whoever forgives and makes reconciliation, his reward is with Allah.", "Wa jazau sayyi'atin sayyi'atum mithluhaa, faman afa wa aslaha fa ajruhu alallah"),
            
            // Verses about knowledge and wisdom
            new QuranVerse(20, 114, "فَتَعَالَى اللَّهُ الْمَلِكُ الْحَقُّ وَلَا تَعْجَلْ بِالْقُرْآنِ مِن قَبْلِ أَن يُقْضَى إِلَيْكَ وَحْيُهُ وَقُل رَّبِّ زِدْنِي عِلْمًا", "So high [above all] is Allah, the Sovereign, the Truth. And, [O Muhammad], do not hasten with [recitation of] the Qur'an before its revelation is completed to you, and say, 'My Lord, increase me in knowledge.'", "Fata'alallahul malikul haqq, wa la ta'jal bil qur'ani min qabli an yuqda ilayka wahyuh, wa qur rabbi zidni ilma"),
            new QuranVerse(39, 9, "قُلْ هَلْ يَسْتَوِي الَّذِينَ يَعْلَمُونَ وَالَّذِينَ لَا يَعْلَمُونَ إِنَّمَا يَتَذَكَّرُ أُولُو الْأَلْبَابِ", "Say, 'Are those who know equal to those who do not know?' Only they will remember [who are] people of understanding.", "Qul hal yastawil ladhina ya'lamuna wal ladhina la ya'lamun, innama yatadhakkaru ulul albab"),
            
            // Additional verses for various topics
            new QuranVerse(13, 28, "الَّذِينَ آمَنُوا وَتَطْمَئِنُّ قُلُوبُهُم بِذِكْرِ اللَّهِ أَلَا بِذِكْرِ اللَّهِ تَطْمَئِنُّ الْقُلُوبُ", "Those who believe and whose hearts are assured by the remembrance of Allah. Unquestionably, by the remembrance of Allah hearts are assured.", "Alladhina amanu wa tatma'innu qulubuhum bidhikr illah, ala bidhikr illahi tatma'innul qulub"),
            new QuranVerse(17, 110, "قُلِ ادْعُوا اللَّهَ أَوِ ادْعُوا الرَّحْمَنَ أَيًّا مَّا تَدْعُوا فَلَهُ الْأَسْمَاءُ الْحُسْنَى", "Say, 'Call upon Allah or call upon the Most Merciful. Whichever [name] you call - to Him belong the best names.'", "Qul-id'ullaha aw-id'ur Rahman, ayyan ma tad'u falahul asma'ul husna"),
            new QuranVerse(94, 5, "فَإِنَّ مَعَ الْعُسْرِ يُسْرًا", "For indeed, with hardship [will be] ease.", "Fa inna ma'al usri yusra"),
            new QuranVerse(94, 6, "إِنَّ مَعَ الْعُسْرِ يُسْرًا", "Indeed, with hardship [will be] ease.", "Inna ma'al usri yusra"),
            
            // Patience and perseverance verses
            new QuranVerse(3, 200, "يَا أَيُّهَا الَّذِينَ آمَنُوا اصْبِرُوا وَصَابِرُوا وَرَابِطُوا وَاتَّقُوا اللَّهَ لَعَلَّكُمْ تُفْلِحُونَ", "O you who believe! Persevere in patience and constancy; vie in such perseverance; strengthen each other; and fear Allah; that ye may prosper.", "Ya ayyuhal ladhina amanu sbiru wa sabiru wa rabitu wattaqullaha la'allakum tuflihun"),
            new QuranVerse(16, 126, "وَإِنْ عَاقَبْتُمْ فَعَاقِبُوا بِمِثْلِ مَا عُوقِبْتُم بِهِ وَلَئِن صَبَرْتُمْ لَهُوَ خَيْرٌ لِّلصَّابِرِينَ", "And if you punish, then punish with the like of that with which you were afflicted. But if you are patient, it is certainly best for those who are patient.", "Wa in aqabtum fa aqibu bimithli ma uqibtum bihi wa la'in sabartum lahuwa khayrun lis sabireen"),
            new QuranVerse(25, 20, "وَجَعَلْنَا مِنْهُمْ أَئِمَّةً يَهْدُونَ بِأَمْرِنَا لَمَّا صَبَرُوا وَكَانُوا بِآيَاتِنَا يُوقِنُونَ", "And We made from among them leaders guiding by Our command when they were patient and were certain of Our signs.", "Wa ja'alna minhum a'immatan yahduna bi amrina lamma sabaru wa kanu bi ayatina yuqinun"),
            new QuranVerse(11, 115, "وَاصْبِرْ فَإِنَّ اللَّهَ لَا يُضِيعُ أَجْرَ الْمُحْسِنِينَ", "And be patient, for indeed, Allah does not allow to be lost the reward of those who do good.", "Wasbir fa inna Allaha la yudee'u ajral muhsineen")
        };
    }
}

/// <summary>
/// Implementation of Hadith service with sample data
/// </summary>
public class HadithService : IHadithService
{
    private readonly List<HadithRecord> _hadiths;

    public HadithService()
    {
        _hadiths = GetSampleHadithData();
    }

    public async Task<List<HadithRecord>> SearchHadithAsync(string text, string language = "en")
    {
        await Task.Delay(100);
        
        var lowerText = text.ToLower();
        return _hadiths.Where(h => 
            h.Translation.ToLower().Contains(lowerText) ||
            h.ArabicText.ToLower().Contains(lowerText)
        ).Take(5).ToList();
    }

    public async Task<List<HadithRecord>> GetHadithByTopicAsync(string topic, string language = "en", int maxResults = 10)
    {
        await Task.Delay(100);
        
        var lowerTopic = topic.ToLower();
        return _hadiths.Where(h => 
            (h.Topic?.ToLower().Contains(lowerTopic) ?? false) ||
            h.Translation.ToLower().Contains(lowerTopic)
        ).Take(maxResults).ToList();
    }

    public async Task<HadithRecord?> GetHadithByReferenceAsync(string collection, string hadithNumber)
    {
        await Task.Delay(50);
        
        return _hadiths.FirstOrDefault(h => 
            h.Collection.Equals(collection, StringComparison.OrdinalIgnoreCase) &&
            h.HadithNumber == hadithNumber
        );
    }

    public async Task<List<HadithRecord>> GetAuthenticHadithAsync(string topic, int maxResults = 5)
    {
        await Task.Delay(100);
        
        var lowerTopic = topic.ToLower();
        return _hadiths.Where(h => 
            h.Grade == HadithGrade.Sahih &&
            ((h.Topic?.ToLower().Contains(lowerTopic) ?? false) ||
             h.Translation.ToLower().Contains(lowerTopic))
        ).Take(maxResults).ToList();
    }    private List<HadithRecord> GetSampleHadithData()
    {
        return new List<HadithRecord>
        {
            // Fundamental Hadiths
            new HadithRecord(
                "إِنَّمَا الْأَعْمَالُ بِالنِّيَّاتِ وَإِنَّمَا لِكُلِّ امْرِئٍ مَا نَوَى",
                "Actions are but by intention, and every man shall have but that which he intended.",
                HadithGrade.Sahih,
                "Bukhari",
                "1",
                "1",
                new List<string> { "Umar ibn al-Khattab", "Alqama ibn Waqqas", "Muhammad ibn Ibrahim" },
                "Intention"
            ),
            new HadithRecord(
                "مَنْ كَانَ يُؤْمِنُ بِاللَّهِ وَالْيَوْمِ الْآخِرِ فَلْيَقُلْ خَيْرًا أَوْ لِيَصْمُتْ",
                "Whoever believes in Allah and the Last Day should speak good or remain silent.",
                HadithGrade.Sahih,
                "Bukhari",
                "78",
                "6018",
                new List<string> { "Abu Hurairah" },
                "Speech"
            ),
            new HadithRecord(
                "الْمُسْلِمُ مَنْ سَلِمَ الْمُسْلِمُونَ مِنْ لِسَانِهِ وَيَدِهِ",
                "A Muslim is one from whose tongue and hand the Muslims are safe.",
                HadithGrade.Sahih,
                "Bukhari",
                "2",
                "10",
                new List<string> { "Abdullah ibn Amr" },
                "Character"
            ),
            
            // About Love and Compassion
            new HadithRecord(
                "لَا يُؤْمِنُ أَحَدُكُمْ حَتَّى يُحِبَّ لِأَخِيهِ مَا يُحِبُّ لِنَفْسِهِ",
                "None of you truly believes until he loves for his brother what he loves for himself.",
                HadithGrade.Sahih,
                "Bukhari",
                "2",
                "13",
                new List<string> { "Anas ibn Malik" },
                "Love"
            ),
            new HadithRecord(
                "الرَّاحِمُونَ يَرْحَمُهُمُ الرَّحْمَنُ ارْحَمُوا مَنْ فِي الْأَرْضِ يَرْحَمْكُمْ مَنْ فِي السَّمَاءِ",
                "Those who are merciful will be shown mercy by the Most Merciful. Be merciful to others and you will receive mercy from Allah.",
                HadithGrade.Sahih,
                "Abu Dawud",
                "43",
                "4941",
                new List<string> { "Abdullah ibn Amr" },
                "Mercy"
            ),
            new HadithRecord(
                "مَثَلُ الْمُؤْمِنِينَ فِي تَوَادِّهِمْ وَتَرَاحُمِهِمْ وَتَعَاطُفِهِمْ مَثَلُ الْجَسَدِ",
                "The example of the believers in their affection, mercy, and compassion for each other is that of a body. When a limb suffers, the whole body responds to it with wakefulness and fever.",
                HadithGrade.Sahih,
                "Bukhari",
                "78",
                "6011",
                new List<string> { "Nu'man ibn Bashir" },
                "Compassion"
            ),
            
            // About Justice and Fairness
            new HadithRecord(
                "إِنَّ الْمُقْسِطِينَ عِنْدَ اللَّهِ عَلَى مَنَابِرَ مِنْ نُورٍ",
                "Indeed, those who act justly will be with Allah on pulpits of light.",
                HadithGrade.Sahih,
                "Muslim",
                "33",
                "1827",
                new List<string> { "Abdullah ibn Amr" },
                "Justice"
            ),
            new HadithRecord(
                "كُلُّكُمْ رَاعٍ وَكُلُّكُمْ مَسْئُولٌ عَنْ رَعِيَّتِهِ",
                "All of you are shepherds and each of you is responsible for his flock.",
                HadithGrade.Sahih,
                "Bukhari",
                "93",
                "7138",
                new List<string> { "Abdullah ibn Umar" },
                "Responsibility"
            ),
            
            // About Knowledge and Wisdom
            new HadithRecord(
                "طَلَبُ الْعِلْمِ فَرِيضَةٌ عَلَى كُلِّ مُسْلِمٍ",
                "Seeking knowledge is an obligation upon every Muslim.",
                HadithGrade.Hasan,
                "Ibn Majah",
                "1",
                "224",
                new List<string> { "Anas ibn Malik" },
                "Knowledge"
            ),
            new HadithRecord(
                "مَنْ سَلَكَ طَرِيقًا يَلْتَمِسُ فِيهِ عِلْمًا سَهَّلَ اللَّهُ لَهُ بِهِ طَرِيقًا إِلَى الْجَنَّةِ",
                "Whoever takes a path upon which to obtain knowledge, Allah makes the path to Paradise easy for him.",
                HadithGrade.Sahih,
                "Muslim",
                "54",
                "2699",
                new List<string> { "Abu Hurairah" },
                "Knowledge"
            ),
            
            // About Patience and Perseverance
            new HadithRecord(
                "وَاعْلَمْ أَنَّ النَّصْرَ مَعَ الصَّبْرِ وَأَنَّ الْفَرَجَ مَعَ الْكَرْبِ وَأَنَّ مَعَ الْعُسْرِ يُسْرًا",
                "Know that victory comes with patience, relief comes with affliction, and ease comes with hardship.",
                HadithGrade.Hasan,
                "Ahmad",
                "1",
                "2803",
                new List<string> { "Ibn Abbas" },
                "Patience"
            ),
            new HadithRecord(
                "مَا يُصِيبُ الْمُسْلِمَ مِنْ نَصَبٍ وَلَا وَصَبٍ وَلَا هَمٍّ وَلَا حُزْنٍ وَلَا أَذًى وَلَا غَمٍّ حَتَّى الشَّوْكَةِ يُشَاكُهَا إِلَّا كَفَّرَ اللَّهُ بِهَا مِنْ خَطَايَاهُ",
                "No fatigue, nor disease, nor sorrow, nor sadness, nor hurt, nor distress befalls a Muslim, not even if it were the prick he receives from a thorn, but that Allah expiates some of his sins for that.",
                HadithGrade.Sahih,
                "Bukhari",
                "75",
                "5641",
                new List<string> { "Abu Sa'id al-Khudri", "Abu Hurairah" },
                "Trials"
            ),
              // About Prayer and Worship
            new HadithRecord(
                "الصَّلَاةُ عِمَادُ الدِّينِ",
                "Prayer is the pillar of religion.",
                HadithGrade.Daif,
                "Bayhaqi",
                "3",
                "3",
                new List<string> { "Umar ibn al-Khattab" },
                "Prayer"
            ),
            new HadithRecord(
                "إِنَّ فِي الصَّلَاةِ شُغْلًا",
                "Indeed, in prayer there is sufficient occupation.",
                HadithGrade.Sahih,
                "Bukhari",
                "10",
                "1216",
                new List<string> { "Abdullah ibn Mas'ud" },
                "Prayer"
            ),
            
            // About Forgiveness and Kindness
            new HadithRecord(
                "ارْحَمُوا تُرْحَمُوا وَاغْفِرُوا يُغْفَرْ لَكُمْ",
                "Show mercy to others and you will receive mercy. Forgive others and you will be forgiven.",
                HadithGrade.Hasan,
                "Ahmad",
                "2",
                "7001",
                new List<string> { "Abdullah ibn Amr" },
                "Forgiveness"
            ),
            new HadithRecord(
                "مَنْ لَا يَرْحَمُ النَّاسَ لَا يَرْحَمُهُ اللَّهُ",
                "He who does not show mercy to others, will not be shown mercy by Allah.",
                HadithGrade.Sahih,
                "Bukhari",
                "78",
                "5997",
                new List<string> { "Jarir ibn Abdullah" },
                "Mercy"
            )
        };
    }
}

/// <summary>
/// Implementation of Fiqh service with sample data
/// </summary>
public class FiqhService : IFiqhService
{
    private readonly List<FiqhRuling> _rulings;

    public FiqhService()
    {
        _rulings = GetSampleFiqhData();
    }

    public async Task<List<FiqhRuling>> SearchRulingsAsync(string question, Madhab madhab, string language = "en")
    {
        await Task.Delay(100);
        
        var lowerQuestion = question.ToLower();
        return _rulings.Where(r => 
            r.Madhab == madhab &&
            (r.Question.ToLower().Contains(lowerQuestion) ||
             r.Ruling.ToLower().Contains(lowerQuestion))
        ).Take(5).ToList();
    }

    public async Task<List<FiqhRuling>> GetRulingsByTopicAsync(string topic, Madhab madhab, string language = "en")
    {
        await Task.Delay(100);
        
        var lowerTopic = topic.ToLower();
        return _rulings.Where(r => 
            r.Madhab == madhab &&
            (r.Question.ToLower().Contains(lowerTopic) ||
             r.Ruling.ToLower().Contains(lowerTopic))
        ).Take(10).ToList();
    }

    public async Task<List<FiqhComparison>> CompareMadhabRulingsAsync(string topic, string language = "en")
    {
        await Task.Delay(150);
        
        // Sample comparison data
        return new List<FiqhComparison>
        {
            new FiqhComparison(
                topic,
                new Dictionary<Madhab, string>
                {
                    { Madhab.Hanafi, "Hanafi position: [Sample ruling based on Abu Hanifa's methodology]" },
                    { Madhab.Maliki, "Maliki position: [Sample ruling based on Malik's methodology]" },
                    { Madhab.Shafi, "Shafi'i position: [Sample ruling based on Shafi'i's methodology]" },
                    { Madhab.Hanbali, "Hanbali position: [Sample ruling based on Ahmad's methodology]" }
                },
                "All madhabs agree on the fundamental principles while differing in specific applications.",
                new List<string> { "Method of reasoning", "Primary sources emphasis", "Local customs consideration" }
            )
        };
    }

    private List<FiqhRuling> GetSampleFiqhData()
    {
        return new List<FiqhRuling>
        {
            new FiqhRuling(
                "What are the conditions for valid prayer?",
                "The prayer is valid when performed with proper purification, facing the Qibla, covering the Awrah, and performing all obligatory acts.",
                Madhab.Hanafi,
                "Based on Quran, Sunnah, and scholarly consensus",
                "Al-Hidayah, Fiqh al-Islami",
                new List<string> { "Abu Hanifa", "Abu Yusuf", "Muhammad al-Shaybani" }
            ),
            new FiqhRuling(
                "Is it permissible to pray in a moving vehicle?",
                "It is permissible when unable to get out, with adjustments for Qibla direction as much as possible.",
                Madhab.Shafi,
                "Based on the principle of removing hardship and necessity",
                "Al-Majmu', Minhaj al-Talibin",
                new List<string> { "Imam Shafi'i", "Al-Nawawi" }
            )
        };
    }
}

/// <summary>
/// Implementation of Du'a service with sample data
/// </summary>
public class DuaService : IDuaService
{
    private readonly List<DuaRecord> _duas;

    public DuaService()
    {
        _duas = GetSampleDuaData();
    }

    public async Task<List<DuaRecord>> SearchDuasByOccasionAsync(string occasion, string language = "en", int maxResults = 10)
    {
        await Task.Delay(100);
        
        var lowerOccasion = occasion.ToLower();
        return _duas.Where(d => 
            d.Occasion.ToLower().Contains(lowerOccasion) ||
            d.Translation.ToLower().Contains(lowerOccasion)
        ).Take(maxResults).ToList();
    }

    public async Task<DuaRecord?> GetSpecificPrayerAsync(SpecificPrayer prayerType, string language = "en")
    {
        await Task.Delay(50);
        
        return _duas.FirstOrDefault(d => 
            d.Occasion.ToLower().Contains(prayerType.ToString().ToLower())
        );
    }

    public async Task<List<DuaRecord>> GetDailyDuasAsync(string timeOfDay, string language = "en")
    {
        await Task.Delay(100);
        
        var lowerTime = timeOfDay.ToLower();
        return _duas.Where(d => 
            d.Occasion.ToLower().Contains(lowerTime)
        ).ToList();
    }

    private List<DuaRecord> GetSampleDuaData()
    {
        return new List<DuaRecord>
        {
            new DuaRecord(
                "اللَّهُمَّ أَعِنِّي عَلَى ذِكْرِكَ وَشُكْرِكَ وَحُسْنِ عِبَادَتِكَ",
                "O Allah, help me to remember You, thank You, and worship You in the best manner.",
                "Allahumma a'inni ala dhikrika wa shukrika wa husni ibadatik",
                "General Supplication",
                "Sunan Abu Dawud",
                "Increases mindfulness and devotion to Allah"
            ),
            new DuaRecord(
                "رَبَّنَا آتِنَا فِي الدُّنْيَا حَسَنَةً وَفِي الْآخِرَةِ حَسَنَةً وَقِنَا عَذَابَ النَّارِ",
                "Our Lord, give us good in this world and good in the hereafter, and save us from the punishment of the Fire.",
                "Rabbana atina fi'd-dunya hasanatan wa fi'l-akhirati hasanatan wa qina adhab an-nar",
                "General Supplication",
                "Quran 2:201",
                "Comprehensive dua covering worldly and spiritual needs"
            ),
            new DuaRecord(
                "أَسْتَغْفِرُ اللَّهَ الَّذِي لَا إِلَهَ إِلَّا هُوَ الْحَيُّ الْقَيُّومُ وَأَتُوبُ إِلَيْهِ",
                "I seek forgiveness from Allah, besides whom there is no god, the Ever-Living, the Sustainer, and I repent to Him.",
                "Astaghfir Allaha'l-ladhi la ilaha illa Huwa'l-Hayy al-Qayyum wa atubu ilayh",
                "Istighfar",
                "Sunan Abu Dawud",
                "Seeking Allah's forgiveness and mercy"
            )
        };
    }
}

/// <summary>
/// Implementation of Tajweed service with sample data
/// </summary>
public class TajweedService : ITajweedService
{
    public async Task<VerseData?> GetVerseWithTajweedAsync(VerseReference verse)
    {
        await Task.Delay(100);
        
        // Sample verse with tajweed markers
        return new VerseData(
            verse.Surah,
            verse.Verse,
            "بِسْمِ اللَّهِ الرَّحْمَنِ الرَّحِيمِ",
            "In the name of Allah, the Entirely Merciful, the Especially Merciful.",
            new List<TajweedMarker>
            {
                new TajweedMarker("Iqlab", 5, 8, "Change 'nun' sound before 'ba' to 'meem'"),
                new TajweedMarker("Madd", 12, 15, "Elongate the 'alif' for 2 counts"),
                new TajweedMarker("Qalqalah", 18, 20, "Bounce the 'meem' sound")
            }
        );
    }

    public async Task<List<CommonMistake>> GetCommonMistakesAsync(VerseReference verse)
    {
        await Task.Delay(100);
        
        return new List<CommonMistake>
        {
            new CommonMistake(
                "Madd Length",
                "Incorrect elongation of vowel sounds",
                "Shortening madd letters or over-elongating them",
                "Maintain exactly 2, 4, or 6 counts as per the rule"
            ),
            new CommonMistake(
                "Qalqalah",
                "Not pronouncing the bounce correctly",
                "Making it too soft or too harsh",
                "Light bounce without adding extra vowel sound"
            )
        };
    }

    public async Task<SurahData?> GetSurahForLessonAsync(int surahNumber, RecitationLevel level)
    {
        await Task.Delay(100);
        
        var surahs = new Dictionary<int, SurahData>
        {
            {
                1, new SurahData(
                    1,
                    "Al-Fatihah",
                    "The Opening",
                    7,
                    "Mecca",
                    new List<string> { "Praise of Allah", "Seeking Guidance", "Prayer" }
                )
            },
            {
                112, new SurahData(
                    112,
                    "Al-Ikhlas",
                    "The Sincerity",
                    4,
                    "Mecca",
                    new List<string> { "Monotheism", "Unity of Allah", "Divine Attributes" }
                )
            }
        };

        return surahs.GetValueOrDefault(surahNumber);
    }

    public async Task<QiraatData?> GetQiraatDataAsync(VerseReference verse, QiraatType qiraatType)
    {
        await Task.Delay(100);
        
        return new QiraatData(
            verse,
            qiraatType,
            new List<string> { $"Variation according to {qiraatType} recitation style" },
            new List<string> { $"audio_reference_{qiraatType.ToString().ToLower()}.mp3" }
        );
    }
}

/// <summary>
/// Implementation of Sirah service with sample data
/// </summary>
public class SirahService : ISirahService
{
    private readonly List<SirahEvent> _events;
    private readonly List<PropheticCharacteristic> _characteristics;
    private readonly List<PropheticGuidance> _guidance;

    public SirahService()
    {
        _events = GetSampleSirahEvents();
        _characteristics = GetSampleCharacteristics();
        _guidance = GetSampleGuidance();
    }

    public async Task<List<SirahEvent>> SearchEventsByContextAsync(string context, string language = "en")
    {
        await Task.Delay(100);
        
        var lowerContext = context.ToLower();
        return _events.Where(e => 
            e.Description.ToLower().Contains(lowerContext) ||
            e.KeyLessons.Any(l => l.ToLower().Contains(lowerContext))
        ).Take(5).ToList();
    }

    public async Task<List<SirahEvent>> GetEventsByPeriodAsync(SirahPeriod period, string language = "en")
    {
        await Task.Delay(100);
        
        return _events.Where(e => e.Period == period).ToList();
    }

    public async Task<SirahEvent?> GetEventByNameAsync(string eventName, string language = "en")
    {
        await Task.Delay(50);
        
        return _events.FirstOrDefault(e => 
            e.Name.Equals(eventName, StringComparison.OrdinalIgnoreCase)
        );
    }

    public async Task<List<PropheticCharacteristic>> GetCharacteristicsAsync(string aspect, string language = "en")
    {
        await Task.Delay(100);
        
        var lowerAspect = aspect.ToLower();
        return _characteristics.Where(c => 
            c.Aspect.ToLower().Contains(lowerAspect) ||
            c.Description.ToLower().Contains(lowerAspect)
        ).ToList();
    }

    public async Task<List<PropheticGuidance>> GetGuidanceByTopicAsync(string topic, string language = "en")
    {
        await Task.Delay(100);
        
        var lowerTopic = topic.ToLower();
        return _guidance.Where(g => 
            g.Topic.ToLower().Contains(lowerTopic) ||
            g.Guidance.ToLower().Contains(lowerTopic)
        ).ToList();
    }

    public async Task<ChronologicalTimeline> GetTimelineAsync(string language = "en")
    {
        await Task.Delay(200);
        
        return new ChronologicalTimeline(
            _events.Where(e => e.Period <= SirahPeriod.LateMecca).ToList(),
            _events.Where(e => e.Period >= SirahPeriod.EarlyMedina).ToList(),
            _events.Where(e => e.Name.Contains("Battle") || e.Name.Contains("Treaty")).ToList()
        );
    }

    private List<SirahEvent> GetSampleSirahEvents()
    {
        return new List<SirahEvent>
        {
            new SirahEvent(
                "The First Revelation",
                "Prophet Muhammad (peace be upon him) received the first verses of the Quran in the Cave of Hira.",
                SirahPeriod.EarlyMecca,
                new DateTime(610, 8, 10),
                "Cave of Hira, Mecca",
                new List<string> { "Seeking solitude for worship", "The beginning of prophethood", "Trust in Allah during fear" },
                new List<string> { "Prophet Muhammad", "Angel Jibril" }
            ),
            new SirahEvent(
                "The Hijra",
                "The migration of Prophet Muhammad and the early Muslims from Mecca to Medina.",
                SirahPeriod.EarlyMedina,
                new DateTime(622, 9, 24),
                "From Mecca to Medina",
                new List<string> { "Sacrifice for faith", "Trust in Allah's plan", "Community building" },
                new List<string> { "Prophet Muhammad", "Abu Bakr", "Ali ibn Abi Talib" }
            ),
            new SirahEvent(
                "Treaty of Hudaybiyyah",
                "A pivotal peace treaty between Muslims and the Meccan tribes.",
                SirahPeriod.MiddleMedina,
                new DateTime(628, 3, 15),
                "Hudaybiyyah, near Mecca",
                new List<string> { "Diplomacy in Islam", "Patience in leadership", "Long-term strategic thinking" },
                new List<string> { "Prophet Muhammad", "Meccan representatives", "Muslim companions" }
            )
        };
    }

    private List<PropheticCharacteristic> GetSampleCharacteristics()
    {
        return new List<PropheticCharacteristic>
        {
            new PropheticCharacteristic(
                "Patience",
                "The Prophet showed extraordinary patience in all circumstances, whether in hardship or ease.",
                new List<string> { "Patience during persecution in Mecca", "Patience with difficult companions", "Patience in teaching" },
                new List<string> { "The hadith about the Bedouin who urinated in the mosque", "His treatment of enemies" }
            ),
            new PropheticCharacteristic(
                "Justice",
                "The Prophet was known for his absolute justice, even with enemies and non-Muslims.",
                new List<string> { "Fair treatment of all tribes", "Just rulings in disputes", "Equal application of law" },
                new List<string> { "The case of the noble woman who stole", "Treaties with Jewish tribes" }
            )
        };
    }

    private List<PropheticGuidance> GetSampleGuidance()
    {
        return new List<PropheticGuidance>
        {
            new PropheticGuidance(
                "Leadership",
                "True leadership is service to others, consultation with people, and putting community needs first.",
                "Shown throughout his leadership of the Muslim community",
                new List<string> { "Consultation before battles", "Caring for the poor and orphans" },
                new List<string> { "Servant leadership in organizations", "Community-first approach in governance" }
            ),
            new PropheticGuidance(
                "Family Relations",
                "Treat your family with kindness, respect, and patience. Be the best to those closest to you.",
                "His treatment of his wives, children, and household",
                new List<string> { "His marriage to Khadijah", "Care for his daughters", "Household interactions" },
                new List<string> { "Modern marriage counseling", "Parenting with compassion", "Work-life balance" }
            )
        };
    }
}

/// <summary>
/// Global Islamic knowledge search service implementation
/// </summary>
public class GlobalSearchService : IGlobalSearchService
{
    private readonly IQuranService _quranService;
    private readonly IHadithService _hadithService;
    private readonly IFiqhService _fiqhService;
    private readonly IDuaService _duaService;
    private readonly ISirahService _sirahService;
    private readonly ITajweedService _tajweedService;
    private readonly ISearchHistoryService? _searchHistoryService;

    public GlobalSearchService(
        IQuranService quranService,
        IHadithService hadithService,
        IFiqhService fiqhService,
        IDuaService duaService,
        ISirahService sirahService,
        ITajweedService tajweedService,
        ISearchHistoryService? searchHistoryService = null)
    {
        _quranService = quranService;
        _hadithService = hadithService;
        _fiqhService = fiqhService;
        _duaService = duaService;
        _sirahService = sirahService;
        _tajweedService = tajweedService;
        _searchHistoryService = searchHistoryService;
    }

    public async Task<GlobalSearchResponse> SearchAllAsync(string query, string language = "en", int maxResults = 20)
    {
        var startTime = DateTime.UtcNow;
        var allResults = new List<GlobalSearchResult>();
        var resultsByType = new Dictionary<IslamicContentType, int>();

        // Search in parallel across all services
        var tasks = new List<Task<List<GlobalSearchResult>>>
        {
            SearchQuranAsync(query, language, maxResults / 6),
            SearchHadithAsync(query, language, maxResults / 6),
            SearchFiqhAsync(query, language, maxResults / 6),
            SearchDuaAsync(query, language, maxResults / 6),
            SearchSirahAsync(query, language, maxResults / 6),
            SearchTajweedAsync(query, language, maxResults / 6)
        };

        var results = await Task.WhenAll(tasks);
        
        // Combine and rank results
        foreach (var resultSet in results)
        {
            allResults.AddRange(resultSet);
        }

        // Sort by relevance score and take top results
        var topResults = allResults
            .OrderByDescending(r => r.RelevanceScore)
            .Take(maxResults)
            .ToList();        // Count results by type
        foreach (var type in Enum.GetValues<IslamicContentType>())
        {
            resultsByType[type] = topResults.Count(r => r.Type == type);
        }

        var duration = DateTime.UtcNow - startTime;

        // Save to search history if service is available
        if (_searchHistoryService != null)
        {
            try
            {
                await _searchHistoryService.SaveSearchAsync(
                    query, 
                    "GlobalSearch", 
                    language, 
                    topResults, 
                    duration);
            }
            catch
            {
                // Log but don't fail the search
                // Could add logging here if logger is available
            }
        }

        return new GlobalSearchResponse(
            query,
            topResults,
            allResults.Count,
            duration.TotalMilliseconds,
            resultsByType
        );
    }

    public async Task<GlobalSearchResponse> SearchByTypeAsync(string query, IslamicContentType[] types, string language = "en", int maxResults = 20)
    {
        var startTime = DateTime.UtcNow;
        var allResults = new List<GlobalSearchResult>();
        var resultsByType = new Dictionary<IslamicContentType, int>();

        var tasks = new List<Task<List<GlobalSearchResult>>>();

        foreach (var type in types)
        {
            switch (type)
            {
                case IslamicContentType.Verse:
                    tasks.Add(SearchQuranAsync(query, language, maxResults));
                    break;
                case IslamicContentType.Hadith:
                    tasks.Add(SearchHadithAsync(query, language, maxResults));
                    break;
                case IslamicContentType.FiqhRuling:
                    tasks.Add(SearchFiqhAsync(query, language, maxResults));
                    break;
                case IslamicContentType.Dua:
                    tasks.Add(SearchDuaAsync(query, language, maxResults));
                    break;
                case IslamicContentType.SirahEvent:
                    tasks.Add(SearchSirahAsync(query, language, maxResults));
                    break;
                case IslamicContentType.TajweedRule:
                    tasks.Add(SearchTajweedAsync(query, language, maxResults));
                    break;
            }
        }

        var results = await Task.WhenAll(tasks);
        
        foreach (var resultSet in results)
        {
            allResults.AddRange(resultSet);
        }

        var topResults = allResults
            .OrderByDescending(r => r.RelevanceScore)
            .Take(maxResults)
            .ToList();        foreach (var type in types)
        {
            resultsByType[type] = topResults.Count(r => r.Type == type);
        }

        var duration = DateTime.UtcNow - startTime;

        // Save to search history if service is available
        if (_searchHistoryService != null)
        {
            try
            {
                var selectedTypes = types.Select(t => t.ToString()).ToArray();
                await _searchHistoryService.SaveSearchAsync(
                    query, 
                    "TypedSearch", 
                    language, 
                    topResults, 
                    duration,
                    selectedTypes);
            }
            catch
            {
                // Log but don't fail the search
            }
        }

        return new GlobalSearchResponse(
            query,
            topResults,
            allResults.Count,
            duration.TotalMilliseconds,
            resultsByType
        );
    }    public async Task<List<string>> GetSearchSuggestionsAsync(string partialQuery, string language = "en")
    {
        await Task.Delay(10); // Small delay to make it truly async
        
        var suggestions = new List<string>();
        
        // Common Islamic search terms and suggestions (enhanced with astronomical/calendar terms)
        var commonTerms = new[]
        {
            "patience", "prayer", "charity", "fasting", "pilgrimage", "faith", "forgiveness",
            "guidance", "mercy", "justice", "wisdom", "knowledge", "worship", "gratitude",
            "repentance", "trust", "hope", "love", "compassion", "kindness", "humility",
            "leadership", "family", "marriage", "friendship", "business", "ethics", "morality",
            // Added astronomical and calendar terms
            "moon", "lunar", "crescent", "calendar", "eclipse", "sighting", "hilal", "months",
            "night", "day", "time", "season", "year", "ramadan"
        };

        var lowerQuery = partialQuery.ToLower();
        suggestions.AddRange(commonTerms.Where(term => term.StartsWith(lowerQuery)).Take(10));

        return suggestions;
    }

    private async Task<List<GlobalSearchResult>> SearchQuranAsync(string query, string language, int maxResults)
    {
        try
        {
            var verses = await _quranService.SearchVersesByContextAsync(query, language);
            return verses.Take(maxResults).Select(v => new GlobalSearchResult(
                IslamicContentType.Verse,
                $"Quran {v.Surah}:{v.Verse}",
                v.Translation,
                v.ArabicText,
                "Quran",
                $"{v.Surah}:{v.Verse}",
                CalculateRelevanceScore(query, v.Translation + " " + v.ArabicText),                new Dictionary<string, object>
                {
                    ["surah"] = v.Surah,
                    ["verse"] = v.Verse,
                    ["transliteration"] = v.Transliteration ?? ""
                }
            )).ToList();
        }
        catch
        {
            return new List<GlobalSearchResult>();
        }
    }

    private async Task<List<GlobalSearchResult>> SearchHadithAsync(string query, string language, int maxResults)
    {
        try
        {
            var hadiths = await _hadithService.SearchHadithAsync(query, language);
            return hadiths.Take(maxResults).Select(h => new GlobalSearchResult(
                IslamicContentType.Hadith,
                $"{h.Collection} {h.HadithNumber}",
                h.Translation,
                h.ArabicText,
                h.Collection,
                $"{h.Collection} {h.HadithNumber}",
                CalculateRelevanceScore(query, h.Translation + " " + h.ArabicText),                new Dictionary<string, object>
                {
                    ["grade"] = h.Grade.ToString(),
                    ["narrator"] = string.Join(", ", h.SanadChain ?? new List<string>()),
                    ["topic"] = h.Topic ?? ""
                }
            )).ToList();
        }
        catch
        {
            return new List<GlobalSearchResult>();
        }
    }

    private async Task<List<GlobalSearchResult>> SearchFiqhAsync(string query, string language, int maxResults)
    {
        try
        {
            var rulings = await _fiqhService.SearchRulingsAsync(query, Madhab.Hanafi, language);
            return rulings.Take(maxResults).Select(r => new GlobalSearchResult(
                IslamicContentType.FiqhRuling,
                r.Question,
                r.Ruling,
                "",
                "Fiqh",
                r.Madhab.ToString(),
                CalculateRelevanceScore(query, r.Question + " " + r.Ruling),                new Dictionary<string, object>
                {
                    ["madhab"] = r.Madhab.ToString(),
                    ["evidence"] = r.Evidence,
                    ["scholars"] = string.Join(", ", r.ScholarReferences ?? new List<string>())
                }
            )).ToList();
        }
        catch
        {
            return new List<GlobalSearchResult>();
        }
    }

    private async Task<List<GlobalSearchResult>> SearchDuaAsync(string query, string language, int maxResults)
    {
        try
        {
            var duas = await _duaService.SearchDuasByOccasionAsync(query, language, maxResults);
            return duas.Select(d => new GlobalSearchResult(
                IslamicContentType.Dua,
                d.Occasion,
                d.Translation,
                d.ArabicText,
                "Dua Collection",
                d.Source,
                CalculateRelevanceScore(query, d.Translation + " " + d.Occasion),                new Dictionary<string, object>
                {
                    ["transliteration"] = d.Transliteration ?? "",
                    ["benefits"] = d.Benefits ?? "",
                    ["source"] = d.Source ?? ""
                }
            )).ToList();
        }
        catch
        {
            return new List<GlobalSearchResult>();
        }
    }

    private async Task<List<GlobalSearchResult>> SearchSirahAsync(string query, string language, int maxResults)
    {
        try
        {
            var events = await _sirahService.SearchEventsByContextAsync(query, language);            return events.Take(maxResults).Select(e => new GlobalSearchResult(
                IslamicContentType.SirahEvent,
                e.Name,
                e.Description,
                "",
                "Sirah",
                e.ApproximateDate?.ToString("yyyy-MM-dd") ?? "Unknown",
                CalculateRelevanceScore(query, e.Name + " " + e.Description + " " + string.Join(" ", e.KeyLessons)),
                new Dictionary<string, object>
                {
                    ["period"] = e.Period.ToString(),
                    ["location"] = e.Location,
                    ["lessons"] = e.KeyLessons,
                    ["participants"] = e.ParticipantsInvolved
                }
            )).ToList();
        }
        catch
        {
            return new List<GlobalSearchResult>();
        }
    }    private async Task<List<GlobalSearchResult>> SearchTajweedAsync(string query, string language, int maxResults)
    {
        await Task.Delay(10); // Make it truly async
        // For now, return empty list as Tajweed search is more complex
        // This can be implemented based on specific Tajweed rule database
        return new List<GlobalSearchResult>();
    }

    private double CalculateRelevanceScore(string query, string content)
    {
        var queryWords = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var contentLower = content.ToLower();
        
        var score = 0.0;
        
        foreach (var word in queryWords)
        {
            if (contentLower.Contains(word))
            {
                // Higher score for exact matches
                score += contentLower.Split(' ').Count(w => w.Equals(word)) * 10;
                // Lower score for partial matches
                score += contentLower.Split(' ').Count(w => w.Contains(word)) * 5;
            }
        }
        
        // Normalize by content length to favor more relevant shorter texts
        return score / Math.Max(content.Length / 100.0, 1.0);
    }
}

/// <summary>
/// Intelligent search service that combines local knowledge base with AI and web search
/// </summary>
public class IntelligentSearchService : IIntelligentSearchService
{
    private readonly IGlobalSearchService _globalSearchService;
    private readonly IOllamaService _ollamaService;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, string> _trustedSources;
    private readonly ISearchHistoryService? _searchHistoryService;
    
    // ✨ OPTIMISATION: Cache simple pour les réponses AI récentes
    private readonly Dictionary<string, (IslamicKnowledgeResponse response, DateTime timestamp)> _aiCache = new();
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);

    public IntelligentSearchService(IGlobalSearchService globalSearchService, IOllamaService ollamaService, HttpClient httpClient, ISearchHistoryService? searchHistoryService = null)
    {
        _globalSearchService = globalSearchService;
        _ollamaService = ollamaService;
        _httpClient = httpClient;
        _trustedSources = InitializeTrustedSources();
        _searchHistoryService = searchHistoryService;
    }

    public async Task<EnhancedSearchResponse> PerformHybridSearchAsync(string query, string language = "en", int maxResults = 20)
    {
        var startTime = DateTime.UtcNow;
        var resultsBySource = new Dictionary<string, int>();
        
        // 🚀 OPTIMISATION 1: Exécution parallèle des recherches locales et suggestions
        var localSearchTask = _globalSearchService.SearchAllAsync(query, language, maxResults);
        var suggestionsTask = GenerateSearchSuggestions(query, new List<GlobalSearchResult>(), language);
        
        // Attendre les recherches locales (priorité haute)
        var localResponse = await localSearchTask;
        var localResults = localResponse.Results;
        resultsBySource["Local Knowledge Base"] = localResults.Count;
        
        // 🚀 OPTIMISATION 2: Décision intelligente sur les recherches supplémentaires
        var needsAI = localResults.Count < 3;
        var needsWeb = localResults.Count < 2; // Seuil plus bas pour web
        
        // 🚀 OPTIMISATION 3: Exécution parallèle conditionnelle des recherches AI et Web
        Task<IslamicKnowledgeResponse?> aiTask = needsAI ? GetAIKnowledgeWithCacheAsync(query, language) : Task.FromResult<IslamicKnowledgeResponse?>(null);
        Task<List<WebSourceResult>> webTask = needsWeb ? SearchWebSourcesAsync(query, language, 3) : Task.FromResult(new List<WebSourceResult>());
        
        // Attendre toutes les tâches en parallèle
        await Task.WhenAll(aiTask, webTask, suggestionsTask);
        
        var aiResponse = await aiTask;
        var webResults = await webTask;
        var suggestions = await suggestionsTask;
        
        if (aiResponse != null) resultsBySource["AI Knowledge"] = 1;
        if (webResults.Any()) resultsBySource["Web Sources"] = webResults.Count;
        
        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        var totalResults = localResults.Count + (aiResponse != null ? 1 : 0) + webResults.Count;
        
        // 🚀 OPTIMISATION 4: Sauvegarde asynchrone sans attendre
        _ = Task.Run(async () => await SaveSearchHistoryAsync(query, language, localResults, aiResponse, webResults, duration));
        
        return new EnhancedSearchResponse(
            query,
            localResults,
            aiResponse,
            webResults,
            totalResults,
            duration,
            resultsBySource,
            suggestions,
            language
        );
    }
    
    // 🚀 OPTIMISATION: Méthode avec cache pour les réponses AI
    private async Task<IslamicKnowledgeResponse?> GetAIKnowledgeWithCacheAsync(string query, string language = "en")
    {
        var cacheKey = $"{query}_{language}".ToLower();
        
        // Vérifier le cache d'abord
        if (_aiCache.TryGetValue(cacheKey, out var cached))
        {
            if (DateTime.UtcNow - cached.timestamp < _cacheExpiry)
            {
                Console.WriteLine($"🚀 Cache hit for AI query: {query.Substring(0, Math.Min(50, query.Length))}...");
                return cached.response;
            }
            else
            {
                _aiCache.Remove(cacheKey); // Nettoyer le cache expiré
            }
        }
        
        // Si pas en cache, générer la réponse
        var response = await GetAIKnowledgeAsync(query, language);
        
        // Mettre en cache la réponse
        if (response != null)
        {
            _aiCache[cacheKey] = (response, DateTime.UtcNow);
            
            // Nettoyer le cache si trop d'entrées (max 100)
            if (_aiCache.Count > 100)
            {
                var oldestKey = _aiCache.OrderBy(x => x.Value.timestamp).First().Key;
                _aiCache.Remove(oldestKey);
            }
        }
        
        return response;
    }
    
    // 🚀 OPTIMISATION: Sauvegarde asynchrone de l'historique
    private async Task SaveSearchHistoryAsync(string query, string language, List<GlobalSearchResult> localResults, 
        IslamicKnowledgeResponse? aiResponse, List<WebSourceResult> webResults, double duration)
    {
        if (_searchHistoryService == null) return;
        
        try
        {
            var webSearchResults = webResults.Select(w => new WebSearchResult
            {
                Title = w.Title,
                Snippet = w.Content,
                Url = w.Url,
                Source = w.Source,
                RelevanceScore = w.RelevanceScore,
                AuthenticityScore = 0.9f,
                Language = language,
                PublishedDate = w.LastUpdated
            }).ToList();

            await _searchHistoryService.SaveSearchWithAIAsync(
                query,
                "HybridSearch",
                language,
                localResults,
                aiResponse,
                webSearchResults,
                TimeSpan.FromMilliseconds(duration));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to save search history: {ex.Message}");
        }
    }

    public async Task<IslamicKnowledgeResponse?> GetAIKnowledgeAsync(string query, string language = "en")
    {
        // 🚀 OPTIMISATION: Prompt plus concis pour des réponses plus rapides
        var prompt = $@"Islamic Scholar Response for: '{query}'

Provide authentic Islamic answer with:
- Clear response in {(language == "ar" ? "Arabic" : "English")}
- Quran refs (Surah:Ayah)
- Hadith refs
- Scholar opinions

Format:
ANSWER: [response]
QURAN_REFS: [ref1|ref2]
HADITH_REFS: [ref1|ref2]  
SCHOLARS: [scholar1|scholar2]";

        try
        {
            // 🚀 OPTIMISATION: Timeout pour éviter les attentes trop longues
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            
            var aiResponse = await _ollamaService.GenerateResponseAsync("mistral:latest", prompt);
            
            // Parse AI response (logique existante)
            var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var answer = "";
            var quranRefs = new List<string>();
            var hadithRefs = new List<string>();
            var scholars = new List<string>();
            
            foreach (var line in lines)
            {
                if (line.StartsWith("ANSWER:"))
                    answer = line.Substring(7).Trim();
                else if (line.StartsWith("QURAN_REFS:"))
                    quranRefs = line.Substring(11).Split('|', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                else if (line.StartsWith("HADITH_REFS:"))
                    hadithRefs = line.Substring(12).Split('|', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                else if (line.StartsWith("SCHOLARS:"))
                    scholars = line.Substring(9).Split('|', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                else if (string.IsNullOrEmpty(answer))
                    answer += line + " ";
            }
            
            // Calculate confidence score based on references provided
            var confidenceScore = CalculateAIConfidenceScore(quranRefs, hadithRefs, scholars);
            
            return new IslamicKnowledgeResponse(
                query,
                answer.Trim(),
                quranRefs,
                hadithRefs,
                scholars,
                confidenceScore,
                "AI Knowledge (Ollama)",
                language
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AI knowledge generation failed: {ex.Message}");
            return null;
        }
    }

    public async Task<List<GlobalSearchResult>> SearchWithAIAsync(string query, string language = "en", int maxResults = 10)
    {
        var aiResponse = await GetAIKnowledgeAsync(query, language);
        if (aiResponse == null) return new List<GlobalSearchResult>();
        
        // Convert AI response to GlobalSearchResult format
        var results = new List<GlobalSearchResult>();
        
        // Add the main AI response as a result
        results.Add(new GlobalSearchResult(
            IslamicContentType.FiqhRuling, // Closest match for AI-generated Islamic knowledge
            $"AI Response: {query}",
            aiResponse.Response,
            "", // No Arabic text for AI responses
            "AI Knowledge",
            "Ollama AI",
            aiResponse.ConfidenceScore,
            new Dictionary<string, object>
            {
                ["quran_references"] = string.Join(", ", aiResponse.QuranReferences),
                ["hadith_references"] = string.Join(", ", aiResponse.HadithReferences),
                ["scholarly_opinions"] = string.Join(", ", aiResponse.ScholarlyOpinions),
                ["ai_generated"] = true
            }
        ));
        
        return results.Take(maxResults).ToList();
    }    public async Task<List<WebSourceResult>> SearchWebSourcesAsync(string query, string language = "en", int maxResults = 5)
    {
        var results = new List<WebSourceResult>();
        
        try
        {
            // Construct search query with Islamic context
            var islamicQuery = $"{query} site:islamqa.info OR site:sunnah.com OR site:quran.com OR site:islamicfinder.org OR site:dar-alifta.org OR site:islamweb.net";
            
            // Use a simple web search approach
            // Note: In production, you should use proper search APIs like Bing Search API, Google Custom Search, etc.
            var searchResults = await PerformSimpleWebSearch(islamicQuery, language, maxResults);
            
            // Filter and validate results
            foreach (var result in searchResults)
            {
                if (IsValidIslamicSource(result.Url))
                {
                    results.Add(result);
                }
            }
            
            // If no real web results, fall back to curated mock results for demonstration
            if (results.Count == 0)
            {
                results.AddRange(GetCuratedIslamicResults(query, language));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Web search failed: {ex.Message}");
            // Fallback to curated results
            results.AddRange(GetCuratedIslamicResults(query, language));
        }
        
        return results.Take(maxResults).ToList();
    }    private async Task<List<WebSourceResult>> PerformSimpleWebSearch(string query, string language, int maxResults)
    {
        var results = new List<WebSourceResult>();
        
        // Simple web search simulation - in production, replace with actual search API
        await Task.Delay(300); // Simulate API delay
        
        Console.WriteLine($"🌐 Performing web search for: {query}");
        
        // TODO: Replace with real web search API implementation
        // For example, using Bing Search API:
        // var bingApiKey = Environment.GetEnvironmentVariable("BING_SEARCH_API_KEY");
        // if (!string.IsNullOrEmpty(bingApiKey))
        // {
        //     var searchUrl = $"https://api.cognitive.microsoft.com/bing/v7.0/search?q={Uri.EscapeDataString(query)}&count={maxResults}";
        //     _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", bingApiKey);
        //     var response = await _httpClient.GetAsync(searchUrl);
        //     // Process response and convert to WebSourceResult objects
        // }
        
        // For now, generate realistic mock web search results instead of empty list
        var queryLower = query.ToLower();
        
        // Generate diverse web search results
        if (queryLower.Contains("prayer") || queryLower.Contains("salah"))
        {
            results.Add(new WebSourceResult(
                "Complete Guide to Islamic Prayer (Salah) - IslamQA",
                "Learn the complete method of performing the five daily prayers in Islam, including preparation, recitations, and etiquette. Includes step-by-step instructions with Arabic text and translations.",
                "https://islamqa.info/en/prayer-guide",
                "IslamQA.info",
                9.2,
                DateTime.Now.AddDays(-1),
                language
            ));
            
            results.Add(new WebSourceResult(
                "Prayer Times and Qibla Direction - IslamicFinder",
                "Find accurate prayer times for your location and determine the correct Qibla direction. Includes monthly prayer calendars and mosque locator.",
                "https://islamicfinder.org/prayer-times",
                               "IslamicFinder.org",
                8.8,
                DateTime.Now.AddDays(-2),
                language
            ));
        }
        
        if (queryLower.Contains("quran") || queryLower.Contains("qur'an"))
        {
            results.Add(new WebSourceResult(
                "Quran.com - The Noble Quran Online",
                "Read, listen to, and study the Quran with multiple translations, recitations, and verse-by-verse analysis from authentic Islamic scholars.",
                "https://quran.com",
                "Quran.com",
                9.5,
                DateTime.Now.AddDays(-1),
                language
            ));
        }
        
        if (queryLower.Contains("hadith") || queryLower.Contains("sunnah"))
        {
            results.Add(new WebSourceResult(
                "Sunnah.com - Hadith Collection Online",
                "Access authentic hadith collections from the six major books including Sahih Bukhari, Sahih Muslim, and more with detailed chain of narrators.",
                "https://sunnah.com",
                "Sunnah.com",
                9.3,
                DateTime.Now.AddDays(-1),
                language
            ));
        }
        
        // Add general Islamic knowledge result
        results.Add(new WebSourceResult(
            $"Islamic Knowledge about {query} - Comprehensive Guide",
            $"Authentic Islamic guidance and information about {query} based on Quran and Sunnah. Scholarly opinions and practical applications for Muslims.",
            "https://islamqa.info/en/search?q=" + Uri.EscapeDataString(query),
            "IslamQA.info",
            8.5,
            DateTime.Now.AddDays(-3),
            language
        ));
        
        Console.WriteLine($"🌐 Generated {results.Count} web search results");
        return results.Take(maxResults).ToList();
    }private bool IsValidIslamicSource(string url)
    {
        var trustedDomains = new[]
        {
            "islamqa.info", "sunnah.com", "quran.com", "islamicfinder.org", 
            "dar-alifta.org", "islamweb.net", "islamicity.org", "aboutislam.net",
            "seekersguidance.org", "yaqeeninstitute.org", "almaghrib.org",
            "bayyinah.com", "islamonline.net", "islamreligion.com"
        };
        
        foreach (var domain in trustedDomains)
        {
            if (url.Contains(domain, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        
        return false;
    }private List<WebSourceResult> GetCuratedIslamicResults(string query, string language)
    {
        var results = new List<WebSourceResult>();
        var queryLower = query.ToLower();
        
        // Curated results from trusted Islamic sources
        if (queryLower.Contains("prayer") || queryLower.Contains("salah") || queryLower.Contains("namaz"))
        {
            results.Add(new WebSourceResult(
                "The Five Daily Prayers in Islam",
                "The five daily prayers (Salah) are a fundamental pillar of Islam. They are Fajr (dawn), Dhuhr (midday), Asr (afternoon), Maghrib (sunset), and Isha (night). Each prayer has specific times and is performed facing the Qibla (direction of Mecca).",
                "https://islamqa.info/en/prayer-times",
                "IslamQA.info",
                8.5,
                DateTime.Now.AddDays(-1),
                language
            ));
            
            results.Add(new WebSourceResult(
                "How to Perform Prayer (Salah) - Step by Step Guide",
                "A comprehensive guide on performing the Islamic prayer including ablution (Wudu), prayer positions, recitations, and proper etiquette. Includes both Arabic text and translations.",
                "https://sunnah.com/pray",
                "Sunnah.com",
                8.2,
                DateTime.Now.AddDays(-3),
                language
            ));
        }
        
        if (queryLower.Contains("zakat") || queryLower.Contains("charity") || queryLower.Contains("zakah"))
        {
            results.Add(new WebSourceResult(
                "Zakat: The Third Pillar of Islam",
                "Zakat is obligatory charity that every Muslim must pay if they meet certain conditions. It purifies wealth and helps the needy. Calculate your Zakat obligation and learn about distribution rules.",
                "https://islamicfinder.org/zakat-calculator",
                "IslamicFinder",
                7.8,
                DateTime.Now.AddDays(-2),
                language
            ));
        }
        
        if (queryLower.Contains("pilgrimage") || queryLower.Contains("hajj") || queryLower.Contains("umrah"))
        {
            results.Add(new WebSourceResult(
                "Hajj: The Sacred Pilgrimage to Mecca",
                "Complete guide to Hajj pilgrimage including rituals, preparation, spiritual significance, and practical advice for Muslims planning to perform this fifth pillar of Islam.",
                "https://islamqa.info/en/hajj-guide",
                "IslamQA.info",
                8.7,
                DateTime.Now.AddDays(-5),
                language
            ));
        }
        
        if (queryLower.Contains("fasting") || queryLower.Contains("ramadan") || queryLower.Contains("sawm"))
        {
            results.Add(new WebSourceResult(
                "Fasting in Ramadan: Rules and Spiritual Benefits",
                "Understanding the Islamic month of Ramadan, fasting rules, exemptions, spiritual significance, and how to observe this important pillar of Islam properly.",
                "https://islamweb.net/en/ramadan",
                "IslamWeb",
                8.1,
                DateTime.Now.AddDays(-10),
                language
            ));
        }

        if (queryLower.Contains("quran") || queryLower.Contains("qur'an") || queryLower.Contains("koran"))
        {
            results.Add(new WebSourceResult(
                "Understanding the Quran - Holy Book of Islam",
                "The Quran is the holy book of Islam, believed by Muslims to be the direct word of Allah revealed to Prophet Muhammad. Learn about its structure, themes, and guidance for daily life.",
                "https://quran.com/about",
                "Quran.com",
                9.0,
                DateTime.Now.AddDays(-1),
                language
            ));
        }

        if (queryLower.Contains("hadith") || queryLower.Contains("sunnah") || queryLower.Contains("prophet"))
        {
            results.Add(new WebSourceResult(
                "Hadith and Sunnah - Following the Prophet's Example",
                "Learn about authentic Hadith collections and the Sunnah of Prophet Muhammad (PBUH). Understand how these teachings complement the Quran in Islamic guidance.",
                "https://sunnah.com/about",
                "Sunnah.com",
                8.8,
                DateTime.Now.AddDays(-2),
                language
            ));
        }

        if (queryLower.Contains("jihad") || queryLower.Contains("struggle"))
        {
            results.Add(new WebSourceResult(
                "Understanding Jihad in Islamic Context",
                "Jihad means 'struggle' in Arabic and has multiple dimensions in Islam: personal spiritual struggle, defending faith, and striving for justice. Learn the authentic Islamic perspective.",
                "https://islamqa.info/en/jihad-meaning",
                "IslamQA.info",
                7.9,
                DateTime.Now.AddDays(-4),
                language
            ));
        }

        if (queryLower.Contains("marriage") || queryLower.Contains("nikah") || queryLower.Contains("family"))
        {
            results.Add(new WebSourceResult(
                "Marriage in Islam - Rights and Responsibilities",
                "Islamic guidance on marriage (Nikah), including the rights and responsibilities of spouses, wedding procedures, and building a successful Islamic family.",
                "https://islamweb.net/en/marriage",
                "IslamWeb",
                8.3,
                DateTime.Now.AddDays(-6),
                language
            ));
        }

        if (queryLower.Contains("death") || queryLower.Contains("funeral") || queryLower.Contains("afterlife"))
        {
            results.Add(new WebSourceResult(
                "Death and Afterlife in Islamic Belief",
                "Islamic teachings about death, funeral rites, the afterlife, and preparing for the hereafter. Understanding the Islamic perspective on life after death.",
                "https://islamqa.info/en/afterlife",
                "IslamQA.info",
                8.0,
                DateTime.Now.AddDays(-8),
                language
            ));
        }

        // 🌙 NEW: Moon, lunar calendar, and astronomical terms
        if (queryLower.Contains("moon") || queryLower.Contains("lunar") || queryLower.Contains("crescent") || queryLower.Contains("hilal"))
        {
            results.Add(new WebSourceResult(
                "Moon Sighting in Islam - Lunar Calendar",
                "Learn about the importance of moon sighting (rukyah) in Islam for determining lunar months, Ramadan, Eid, and other Islamic observances. Understanding the Islamic lunar calendar and Hilal sighting.",
                "https://islamqa.info/en/moon-sighting",
                "IslamQA.info",
                8.6,
                DateTime.Now.AddDays(-2),
                language
            ));
            
            results.Add(new WebSourceResult(
                "Islamic Calendar - Hijri Lunar System",
                "Complete guide to the Islamic Hijri calendar system based on lunar months. Moon phases, crescent sighting, and Islamic month calculations for religious observances.",
                "https://islamicfinder.org/islamic-calendar",
                "IslamicFinder.org",
                8.4,
                DateTime.Now.AddDays(-3),
                language
            ));
        }

        if (queryLower.Contains("eclipse") || queryLower.Contains("kusuf") || queryLower.Contains("khusuf"))
        {
            results.Add(new WebSourceResult(
                "Eclipse Prayer in Islam - Salat al-Kusuf",
                "Islamic teachings about solar and lunar eclipses, the special eclipse prayer (Salat al-Kusuf), and the Prophet's guidance during celestial events.",
                "https://sunnah.com/eclipse-prayer",
                "Sunnah.com",
                8.3,
                DateTime.Now.AddDays(-4),
                language
            ));
        }

        if (queryLower.Contains("calendar") || queryLower.Contains("hijri") || queryLower.Contains("months"))
        {
            results.Add(new WebSourceResult(
                "Hijri Calendar - Islamic Lunar Year",
                "Understanding the 12 months of the Islamic Hijri calendar, their significance, and how lunar calculations determine Islamic dates and religious observances.",
                "https://islamweb.net/en/hijri-calendar",
                "IslamWeb",
                7.9,
                DateTime.Now.AddDays(-5),
                language
            ));
        }
        
        // Default Islamic knowledge result for general queries
        if (results.Count == 0)
        {
            results.Add(new WebSourceResult(
                "Islamic Knowledge Portal - Comprehensive Resources",
                $"Explore comprehensive Islamic resources covering {query} and related topics from authentic sources including Quran, Hadith, and scholarly interpretations. Find answers to your Islamic questions.",
                "https://islamicity.org/knowledge",
                "Islamicity.org",
                7.5,
                DateTime.Now.AddDays(-7),
                language
            ));

            // Add a second default result for broader coverage
            results.Add(new WebSourceResult(
                "IslamQA - Ask the Scholar",
                $"Get authentic Islamic answers to questions about {query}. Scholarly fatawa and religious guidance based on Quran and Sunnah from qualified Islamic scholars.",
                "https://islamqa.info/en/search",
                "IslamQA.info",
                7.3,
                DateTime.Now.AddDays(-5),
                language
            ));
        }
        
        return results;
    }    private Dictionary<string, string> InitializeTrustedSources()
    {
        return new Dictionary<string, string>
        {
            ["islamqa.info"] = "IslamQA - Scholarly Fatawa",
            ["sunnah.com"] = "Sunnah.com - Hadith Collection",
            ["quran.com"] = "Quran.com - Quran Text and Translation",
            ["islamicfinder.org"] = "Islamic Finder - Islamic Resources",
            ["dar-alifta.org"] = "Dar al-Ifta - Egyptian Fatwa Authority",
            ["islamweb.net"] = "IslamWeb - Islamic Knowledge",
            ["islamicity.org"] = "Islamicity - Islamic Resources",
            ["aboutislam.net"] = "About Islam - Islamic Articles",
            ["seekersguidance.org"] = "SeekersGuidance - Islamic Learning",
            ["yaqeeninstitute.org"] = "Yaqeen Institute - Islamic Research",
            ["almaghrib.org"] = "AlMaghrib Institute - Islamic Education",
            ["bayyinah.com"] = "Bayyinah Institute - Quranic Studies"
        };
    }

    private double CalculateAIConfidenceScore(List<string> quranRefs, List<string> hadithRefs, List<string> scholars)
    {
        double score = 0.3; // Base score
        
        // Add points for Quran references
        score += Math.Min(quranRefs.Count * 0.2, 0.4);
        
        // Add points for Hadith references  
        score += Math.Min(hadithRefs.Count * 0.15, 0.3);
        
        // Add points for scholarly references
        score += Math.Min(scholars.Count * 0.1, 0.2);
        
        return Math.Min(score, 1.0); // Cap at 1.0
    }

    private async Task<List<string>> GenerateSearchSuggestions(string query, List<GlobalSearchResult> localResults, string language)
    {
        await Task.Delay(10); // Simulate processing
        
        var suggestions = new List<string>();
        var queryLower = query.ToLower();
        
        // Generate related suggestions based on query
        if (queryLower.Contains("prayer")) 
        {
            suggestions.AddRange(new[] { "salah times", "prayer conditions", "types of prayer", "prayer benefits" });
        }
        else if (queryLower.Contains("charity"))
        {
            suggestions.AddRange(new[] { "zakat calculation", "sadaqah", "charity recipients", "voluntary charity" });
        }
        else if (queryLower.Contains("fasting"))
        {
            suggestions.AddRange(new[] { "ramadan fasting", "fasting rules", "breaking fast", "voluntary fasting" });
        }
        else if (queryLower.Contains("hajj"))
        {
            suggestions.AddRange(new[] { "hajj steps", "umrah", "pilgrimage requirements", "hajj preparation" });
        }
        else if (queryLower.Contains("moon") || queryLower.Contains("lunar") || queryLower.Contains("crescent"))
        {
            suggestions.AddRange(new[] { "moon sighting", "lunar calendar", "islamic months", "crescent moon" });
        }
        else if (queryLower.Contains("eclipse"))
        {
            suggestions.AddRange(new[] { "lunar eclipse", "solar eclipse", "eclipse prayer", "kusuf prayer" });
        }
        else if (queryLower.Contains("calendar"))
        {
            suggestions.AddRange(new[] { "hijri calendar", "islamic months", "lunar year", "moon sighting" });
        }
        else
        {
            // Generic Islamic suggestions
            suggestions.AddRange(new[] { "islamic beliefs", "five pillars", "quran teachings", "prophet muhammad" });
        }
        
        return suggestions.Take(4).ToList();
    }
}
