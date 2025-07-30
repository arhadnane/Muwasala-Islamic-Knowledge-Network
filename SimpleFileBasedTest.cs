using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TestFileBasedSearch
{
    // Local copy of QuranVerse to avoid conflicts
    public class QuranVerse
    {
        public int Surah { get; set; }
        public int Verse { get; set; }
        public string ArabicText { get; set; } = "";
        public string Translation { get; set; } = "";
        public string? Transliteration { get; set; }
        public string Language { get; set; } = "en";

        public QuranVerse(int surah, int verse, string arabicText, string translation, string? transliteration = null, string language = "en")
        {
            Surah = surah;
            Verse = verse;
            ArabicText = arabicText;
            Translation = translation;
            Transliteration = transliteration;
            Language = language;
        }
    }

    // Simplified FileBasedQuranSearchService for testing
    public class SimpleFileSearchService
    {
        private readonly string _basePath;
        private static readonly Dictionary<string, List<QuranVerse>> _cachedVerses = new();
        private static readonly object _cacheLock = new();
        
        public SimpleFileSearchService()
        {
            _basePath = "D:\\Data Perso Adnane\\Coding\\VSCodeProject\\Muwasala Islamic Knowledge Network V2\\Muwasala-Islamic-Knowledge-Network\\data\\Quran\\SourceFiles";
        }

        public async Task<List<QuranVerse>> SearchVersesByContextAsync(string query, int maxResults = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<QuranVerse>();

            var queryTerms = query.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Load verses from files
            var englishVerses = await LoadVersesFromFileAsync("en.sahih.txt", isArabic: false);
            var arabicVerses = await LoadVersesFromFileAsync("quran-uthmani.txt", isArabic: true);

            var results = new List<QuranVerse>();

            // Search in English translations
            foreach (var verse in englishVerses)
            {
                var score = CalculateRelevanceScore(verse.Translation, queryTerms);
                if (score > 0)
                {
                    // Find corresponding Arabic text
                    var arabicVerse = arabicVerses.FirstOrDefault(v => v.Surah == verse.Surah && v.Verse == verse.Verse);
                    var combinedVerse = new QuranVerse(
                        verse.Surah,
                        verse.Verse,
                        arabicVerse?.ArabicText ?? "",
                        verse.Translation,
                        verse.Transliteration,
                        verse.Language
                    );
                    results.Add(combinedVerse);
                }
            }

            return results.Take(maxResults).ToList();
        }

        private async Task<List<QuranVerse>> LoadVersesFromFileAsync(string fileName, bool isArabic)
        {
            lock (_cacheLock)
            {
                if (_cachedVerses.ContainsKey(fileName))
                {
                    return _cachedVerses[fileName];
                }
            }

            var filePath = Path.Combine(_basePath, fileName);
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return new List<QuranVerse>();
            }

            var verses = new List<QuranVerse>();
            var lines = await File.ReadAllLinesAsync(filePath);

            Console.WriteLine($"Loading {lines.Length} lines from {fileName}");

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3 && 
                    int.TryParse(parts[0], out int surah) && 
                    int.TryParse(parts[1], out int verse))
                {
                    var text = string.Join("|", parts.Skip(2));

                    var quranVerse = new QuranVerse(
                        surah,
                        verse,
                        isArabic ? text : "",
                        isArabic ? "" : text,
                        null,
                        isArabic ? "ar" : "en"
                    );

                    verses.Add(quranVerse);
                }
            }

            Console.WriteLine($"Loaded {verses.Count} verses from {fileName}");

            lock (_cacheLock)
            {
                _cachedVerses[fileName] = verses;
            }

            return verses;
        }

        private double CalculateRelevanceScore(string text, string[] queryTerms)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;

            var normalizedText = text.ToLowerInvariant();
            double score = 0;

            foreach (var term in queryTerms)
            {
                if (string.IsNullOrWhiteSpace(term)) continue;

                if (normalizedText.Contains(term))
                {
                    score += 3.0;
                    if (normalizedText.StartsWith(term))
                        score += 2.0;
                }
            }

            return score;
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Test du service de recherche basé sur les fichiers sources");
            Console.WriteLine("==========================================================");
            
            var searchService = new SimpleFileSearchService();
            
            // Test avec "Land" - terme qui ne donnait pas de résultats avant
            Console.WriteLine("\n1. Test avec 'Land':");
            try
            {
                var landResults = await searchService.SearchVersesByContextAsync("Land", 5);
                Console.WriteLine($"Résultats pour 'Land': {landResults.Count} versets trouvés");
                
                foreach (var verse in landResults.Take(3))
                {
                    Console.WriteLine($"  - Sourate {verse.Surah}:{verse.Verse}");
                    Console.WriteLine($"    EN: {verse.Translation.Substring(0, Math.Min(100, verse.Translation.Length))}...");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur avec 'Land': {ex.Message}");
            }
            
            // Test avec "earth" - nouveau terme à tester
            Console.WriteLine("\n2. Test avec 'earth':");
            try
            {
                var earthResults = await searchService.SearchVersesByContextAsync("earth", 5);
                Console.WriteLine($"Résultats pour 'earth': {earthResults.Count} versets trouvés");
                
                foreach (var verse in earthResults.Take(3))
                {
                    Console.WriteLine($"  - Sourate {verse.Surah}:{verse.Verse}");
                    Console.WriteLine($"    EN: {verse.Translation.Substring(0, Math.Min(100, verse.Translation.Length))}...");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur avec 'earth': {ex.Message}");
            }
            
            // Test avec "mercy" - terme existant avec synonymes
            Console.WriteLine("\n3. Test avec 'mercy':");
            try
            {
                var mercyResults = await searchService.SearchVersesByContextAsync("mercy", 5);
                Console.WriteLine($"Résultats pour 'mercy': {mercyResults.Count} versets trouvés");
                
                foreach (var verse in mercyResults.Take(3))
                {
                    Console.WriteLine($"  - Sourate {verse.Surah}:{verse.Verse}");
                    Console.WriteLine($"    EN: {verse.Translation.Substring(0, Math.Min(100, verse.Translation.Length))}...");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur avec 'mercy': {ex.Message}");
            }
            
            Console.WriteLine("\nTest terminé. Appuyez sur une touche pour continuer...");
            Console.ReadKey();
        }
    }
}
