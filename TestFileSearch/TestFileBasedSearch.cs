using System;
using System.Threading.Tasks;
using Muwasala.KnowledgeBase.Services;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🔍 Test du service de recherche basé sur les fichiers");
        Console.WriteLine(new string('=', 60));
        
        var searchService = new FileBasedQuranSearchService();
        
        // Test 1: Recherche par mot-clé
        Console.WriteLine("\n📖 Test 1: Recherche de 'Allah'");
        var results1 = await searchService.SearchVersesByContextAsync("Allah", 5);
        Console.WriteLine($"Résultats trouvés: {results1.Count}");
        
        foreach (var verse in results1)
        {
            Console.WriteLine($"Sourate {verse.Surah}, Verset {verse.Verse}:");
            Console.WriteLine($"🔤 Arabe: {verse.ArabicText}");
            Console.WriteLine($"🌍 Traduction: {verse.Translation}");
            Console.WriteLine();
        }
        
        // Test 2: Recherche par expression
        Console.WriteLine("\n📖 Test 2: Recherche de 'five pillars'");
        var results2 = await searchService.SearchVersesByContextAsync("five pillars", 3);
        Console.WriteLine($"Résultats trouvés: {results2.Count}");
        
        foreach (var verse in results2)
        {
            Console.WriteLine($"Sourate {verse.Surah}, Verset {verse.Verse}:");
            Console.WriteLine($"🔤 Arabe: {verse.ArabicText}");
            Console.WriteLine($"🌍 Traduction: {verse.Translation}");
            Console.WriteLine();
        }
        
        // Test 3: Recherche en français (si disponible)
        Console.WriteLine("\n📖 Test 3: Recherche de 'prayer'");
        var results3 = await searchService.SearchVersesByContextAsync("prayer", 3);
        Console.WriteLine($"Résultats trouvés: {results3.Count}");
        
        foreach (var verse in results3)
        {
            Console.WriteLine($"Sourate {verse.Surah}, Verset {verse.Verse}:");
            Console.WriteLine($"🔤 Arabe: {verse.ArabicText}");
            Console.WriteLine($"🌍 Traduction: {verse.Translation}");
            Console.WriteLine();
        }
        
        Console.WriteLine("✅ Tests terminés!");
    }
}
