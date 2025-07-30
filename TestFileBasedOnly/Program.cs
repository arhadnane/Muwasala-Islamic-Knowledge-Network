using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

// Minimal version pour tester directement
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Test FileBasedQuranSearch - Version Directe");
        Console.WriteLine("==========================================");

        string sourceFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "data", "Quran", "SourceFiles");
        Console.WriteLine($"Chemin source: {sourceFilesPath}");
        
        var service = new FileBasedQuranSearchService(sourceFilesPath);
        
        try
        {
            Console.WriteLine("\n1. Test recherche 'patience':");
            var patienceResults = await service.SearchVersesByContextAsync("patience", 5);
            Console.WriteLine($"   Résultats trouvés: {patienceResults.Count()}");
            foreach (var verse in patienceResults.Take(3))
            {
                Console.WriteLine($"   - {verse.SurahNumber}:{verse.VerseNumber} - {verse.ArabicText.Substring(0, Math.Min(50, verse.ArabicText.Length))}...");
            }

            Console.WriteLine("\n2. Test recherche 'Land':");
            var landResults = await service.SearchVersesByContextAsync("Land", 5);
            Console.WriteLine($"   Résultats trouvés: {landResults.Count()}");
            foreach (var verse in landResults.Take(3))
            {
                Console.WriteLine($"   - {verse.SurahNumber}:{verse.VerseNumber} - {verse.EnglishText.Substring(0, Math.Min(50, verse.EnglishText.Length))}...");
            }

            Console.WriteLine("\n3. Test recherche 'earth':");
            var earthResults = await service.SearchVersesByContextAsync("earth", 5);
            Console.WriteLine($"   Résultats trouvés: {earthResults.Count()}");
            foreach (var verse in earthResults.Take(3))
            {
                Console.WriteLine($"   - {verse.SurahNumber}:{verse.VerseNumber} - {verse.EnglishText.Substring(0, Math.Min(50, verse.EnglishText.Length))}...");
            }

            Console.WriteLine("\n4. Test recherche 'mercy':");
            var mercyResults = await service.SearchVersesByContextAsync("mercy", 5);
            Console.WriteLine($"   Résultats trouvés: {mercyResults.Count()}");
            foreach (var verse in mercyResults.Take(3))
            {
                Console.WriteLine($"   - {verse.SurahNumber}:{verse.VerseNumber} - {verse.EnglishText.Substring(0, Math.Min(50, verse.EnglishText.Length))}...");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }

        Console.WriteLine("\nTest terminé. Appuyez sur une touche pour continuer...");
        Console.ReadKey();
    }
}

// Copie simple des classes nécessaires
public class QuranVerse
{
    public int SurahNumber { get; set; }
    public int VerseNumber { get; set; }
    public string ArabicText { get; set; } = "";
    public string EnglishText { get; set; } = "";
    public double RelevanceScore { get; set; }
    public string Context { get; set; } = "";
}

public class FileBasedQuranSearchService
{
    private readonly string _sourceFilesPath;
    private List<QuranVerse> _verses = new List<QuranVerse>();
    private bool _isLoaded = false;

    public FileBasedQuranSearchService(string sourceFilesPath)
    {
        _sourceFilesPath = sourceFilesPath;
    }

    public async Task<IEnumerable<QuranVerse>> SearchVersesByContextAsync(string query, int maxResults = 10)
    {
        await EnsureVersesLoadedAsync();
        
        var searchTerms = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var expandedTerms = new HashSet<string>(searchTerms);
        
        // Expansion des synonymes
        var synonyms = GetIslamicSynonyms();
        foreach (var term in searchTerms)
        {
            if (synonyms.ContainsKey(term))
            {
                foreach (var synonym in synonyms[term])
                {
                    expandedTerms.Add(synonym);
                }
            }
        }

        var results = _verses
            .Where(verse => expandedTerms.Any(term => 
                verse.EnglishText.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                verse.ArabicText.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .Select(verse => new QuranVerse
            {
                SurahNumber = verse.SurahNumber,
                VerseNumber = verse.VerseNumber,
                ArabicText = verse.ArabicText,
                EnglishText = verse.EnglishText,
                RelevanceScore = CalculateRelevanceScore(verse, expandedTerms),
                Context = $"Surah {verse.SurahNumber}, Verse {verse.VerseNumber}"
            })
            .OrderByDescending(v => v.RelevanceScore)
            .Take(maxResults);

        return results;
    }

    private async Task EnsureVersesLoadedAsync()
    {
        if (_isLoaded) return;

        var arabicFile = Path.Combine(_sourceFilesPath, "quran-uthmani.txt");
        var englishFile = Path.Combine(_sourceFilesPath, "en.sahih.txt");

        if (!File.Exists(arabicFile) || !File.Exists(englishFile))
        {
            throw new FileNotFoundException($"Source files not found in {_sourceFilesPath}");
        }

        var arabicLines = await File.ReadAllLinesAsync(arabicFile);
        var englishLines = await File.ReadAllLinesAsync(englishFile);

        _verses.Clear();

        for (int i = 0; i < Math.Min(arabicLines.Length, englishLines.Length); i++)
        {
            var arabicParts = arabicLines[i].Split('|');
            var englishParts = englishLines[i].Split('|');

            if (arabicParts.Length >= 3 && englishParts.Length >= 3 &&
                int.TryParse(arabicParts[0], out int surahNumber) &&
                int.TryParse(arabicParts[1], out int verseNumber))
            {
                _verses.Add(new QuranVerse
                {
                    SurahNumber = surahNumber,
                    VerseNumber = verseNumber,
                    ArabicText = arabicParts[2],
                    EnglishText = englishParts[2]
                });
            }
        }

        _isLoaded = true;
        Console.WriteLine($"Loaded {_verses.Count} verses from source files");
    }

    private double CalculateRelevanceScore(QuranVerse verse, HashSet<string> searchTerms)
    {
        double score = 0;
        var text = (verse.EnglishText + " " + verse.ArabicText).ToLower();
        
        foreach (var term in searchTerms)
        {
            int count = CountOccurrences(text, term.ToLower());
            score += count * (term.Length > 3 ? 2.0 : 1.0);
        }
        
        return score;
    }

    private int CountOccurrences(string text, string term)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(term, index, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            index += term.Length;
        }
        return count;
    }

    private Dictionary<string, List<string>> GetIslamicSynonyms()
    {
        return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["patience"] = new List<string> { "sabr", "perseverance", "endurance", "steadfastness" },
            ["guidance"] = new List<string> { "hidayah", "direction", "path", "way" },
            ["mercy"] = new List<string> { "rahman", "raheem", "compassion", "forgiveness" },
            ["land"] = new List<string> { "earth", "ground", "soil", "territory", "country" },
            ["earth"] = new List<string> { "land", "ground", "world", "planet" },
            ["prayer"] = new List<string> { "salah", "dua", "worship", "supplication" },
            ["faith"] = new List<string> { "iman", "belief", "trust", "conviction" },
            ["god"] = new List<string> { "allah", "lord", "creator", "almighty" },
            ["prophet"] = new List<string> { "messenger", "rasul", "nabi" },
            ["book"] = new List<string> { "kitab", "scripture", "quran", "revelation" }
        };
    }
}
