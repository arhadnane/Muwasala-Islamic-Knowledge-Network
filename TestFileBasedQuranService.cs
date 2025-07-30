using System;
using System.Threading.Tasks;
using Muwasala.KnowledgeBase.Services;

class TestFileBasedQuranService
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Test du QuranService modifié avec FileBasedQuranSearchService");
        Console.WriteLine("================================================================");
        
        var quranService = new QuranService();
        
        // Test avec "Land" - terme qui ne donnait pas de résultats avant
        Console.WriteLine("\n1. Test avec 'Land':");
        try
        {
            var landResults = await quranService.SearchVersesByThemeAsync("Land", "en", 5);
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
        
        // Test avec "patience" - terme qui fonctionnait avant
        Console.WriteLine("\n2. Test avec 'patience':");
        try
        {
            var patienceResults = await quranService.SearchVersesByThemeAsync("patience", "en", 5);
            Console.WriteLine($"Résultats pour 'patience': {patienceResults.Count} versets trouvés");
            
            foreach (var verse in patienceResults.Take(3))
            {
                Console.WriteLine($"  - Sourate {verse.Surah}:{verse.Verse}");
                Console.WriteLine($"    EN: {verse.Translation.Substring(0, Math.Min(100, verse.Translation.Length))}...");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur avec 'patience': {ex.Message}");
        }
        
        // Test avec "earth" - nouveau terme à tester
        Console.WriteLine("\n3. Test avec 'earth':");
        try
        {
            var earthResults = await quranService.SearchVersesByThemeAsync("earth", "en", 5);
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
        Console.WriteLine("\n4. Test avec 'mercy':");
        try
        {
            var mercyResults = await quranService.SearchVersesByThemeAsync("mercy", "en", 5);
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
