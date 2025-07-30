using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuranFileAnalyzer;

/// <summary>
/// Test simple pour analyser les fichiers sources du Coran
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🕌 Test d'analyse des fichiers sources du Coran");
        Console.WriteLine("=".PadRight(60, '='));
        
        // Chemin vers les fichiers sources
        var basePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "..", "..", "..", "..", "..", 
            "data", "Quran", "SourceFiles"
        );

        var arabicFile = Path.Combine(basePath, "quran-uthmani.txt");
        var englishFile = Path.Combine(basePath, "en.sahih.txt");

        Console.WriteLine($"📂 Chemin de base: {basePath}");
        Console.WriteLine($"📄 Fichier arabe: {Path.GetFileName(arabicFile)}");
        Console.WriteLine($"📄 Fichier anglais: {Path.GetFileName(englishFile)}");
        Console.WriteLine();

        // Vérifier l'existence des fichiers
        if (!File.Exists(arabicFile))
        {
            Console.WriteLine($"❌ Fichier arabe non trouvé: {arabicFile}");
            // Essayons un autre chemin
            var altPath = "D:\\Data Perso Adnane\\Coding\\VSCodeProject\\Muwasala Islamic Knowledge Network V2\\Muwasala-Islamic-Knowledge-Network\\data\\Quran\\SourceFiles";
            arabicFile = Path.Combine(altPath, "quran-uthmani.txt");
            englishFile = Path.Combine(altPath, "en.sahih.txt");
            Console.WriteLine($"🔄 Essai du chemin alternatif: {altPath}");
        }

        if (!File.Exists(arabicFile))
        {
            Console.WriteLine($"❌ Fichier arabe non trouvé: {arabicFile}");
            // Lister les fichiers disponibles dans le répertoire data
            var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "data");
            if (Directory.Exists(dataPath))
            {
                Console.WriteLine($"📁 Contenu du répertoire data:");
                foreach (var item in Directory.GetFileSystemEntries(dataPath))
                {
                    Console.WriteLine($"   - {Path.GetFileName(item)}");
                }
            }
            return;
        }

        if (!File.Exists(englishFile))
        {
            Console.WriteLine($"❌ Fichier anglais non trouvé: {englishFile}");
            return;
        }

        // Analyser les fichiers
        await AnalyzeFile(arabicFile, "Arabe");
        Console.WriteLine();
        await AnalyzeFile(englishFile, "Anglais");

        // Comparer les fichiers
        Console.WriteLine("\n🔄 Comparaison des fichiers:");
        var arabicLines = await File.ReadAllLinesAsync(arabicFile);
        var englishLines = await File.ReadAllLinesAsync(englishFile);
        
        Console.WriteLine($"   📊 Lignes en arabe: {arabicLines.Length}");
        Console.WriteLine($"   📊 Lignes en anglais: {englishLines.Length}");
        Console.WriteLine($"   ⚖️  Différence: {Math.Abs(arabicLines.Length - englishLines.Length)}");

        // Montrer quelques exemples
        Console.WriteLine("\n📖 Exemples de contenu:");
        var maxLines = Math.Min(10, Math.Min(arabicLines.Length, englishLines.Length));
        
        for (int i = 0; i < maxLines; i++)
        {
            if (!string.IsNullOrWhiteSpace(arabicLines[i]) && !string.IsNullOrWhiteSpace(englishLines[i]))
            {
                Console.WriteLine($"\n   Ligne {i + 1}:");
                Console.WriteLine($"   🔤 AR: {arabicLines[i].Substring(0, Math.Min(80, arabicLines[i].Length))}...");
                Console.WriteLine($"   🇬🇧 EN: {englishLines[i].Substring(0, Math.Min(80, englishLines[i].Length))}...");
            }
        }

        // Rechercher des patterns de numérotation
        Console.WriteLine("\n🔍 Analyse des patterns de numérotation:");
        AnalyzeNumberingPatterns(arabicLines.Take(20).ToArray(), "Arabe");
        AnalyzeNumberingPatterns(englishLines.Take(20).ToArray(), "Anglais");
        
        // Test de parsing d'une référence
        Console.WriteLine("\n🔍 Test de parsing de références:");
        TestReferenceParsing();
        
        Console.WriteLine("\n✅ Analyse terminée. Appuyez sur une touche pour continuer...");
        Console.ReadKey();
    }

    private static async Task AnalyzeFile(string filePath, string language)
    {
        Console.WriteLine($"🔍 Analyse du fichier {language}:");
        
        var fileInfo = new FileInfo(filePath);
        Console.WriteLine($"   📏 Taille: {fileInfo.Length:N0} octets");
        
        var lines = await File.ReadAllLinesAsync(filePath);
        Console.WriteLine($"   📄 Lignes: {lines.Length:N0}");
        
        var nonEmptyLines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).Count();
        Console.WriteLine($"   📝 Lignes non-vides: {nonEmptyLines:N0}");
        
        if (lines.Length > 0)
        {
            var firstLine = lines[0];
            var lastLine = lines[^1];
            
            Console.WriteLine($"   🥇 Première ligne: {firstLine.Substring(0, Math.Min(60, firstLine.Length))}...");
            Console.WriteLine($"   🥉 Dernière ligne: {lastLine.Substring(0, Math.Min(60, lastLine.Length))}...");
            
            var avgLength = lines.Where(l => !string.IsNullOrWhiteSpace(l))
                                .Average(l => l.Length);
            Console.WriteLine($"   📐 Longueur moyenne: {avgLength:F1} caractères");
        }
    }

    private static void AnalyzeNumberingPatterns(string[] lines, string language)
    {
        Console.WriteLine($"   🔢 Patterns dans {language}:");
        
        for (int i = 0; i < Math.Min(10, lines.Length); i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            // Rechercher des chiffres au début
            var match = System.Text.RegularExpressions.Regex.Match(line, @"^(\d+)[:|.]");
            if (match.Success)
            {
                Console.WriteLine($"      Ligne {i+1}: Numéro {match.Groups[1].Value} - {line.Substring(0, Math.Min(50, line.Length))}...");
            }
            else
            {
                Console.WriteLine($"      Ligne {i+1}: {line.Substring(0, Math.Min(50, line.Length))}...");
            }
        }
    }

    private static void TestReferenceParsing()
    {
        var testReferences = new[]
        {
            "1:1", "2:255", "112:1-4", "18:1-10", "114:6"
        };

        foreach (var reference in testReferences)
        {
            var result = ParseVerseReference(reference);
            Console.WriteLine($"   Référence '{reference}' -> Sourate: {result.surah}, Verset: {result.verseStart}-{result.verseEnd}");
        }
    }

    private static (int surah, int verseStart, int verseEnd) ParseVerseReference(string reference)
    {
        try
        {
            var parts = reference.Split(':');
            if (parts.Length != 2) return (0, 0, 0);

            var surah = int.Parse(parts[0]);
            var versePart = parts[1];

            if (versePart.Contains('-'))
            {
                var verseRange = versePart.Split('-');
                var verseStart = int.Parse(verseRange[0]);
                var verseEnd = int.Parse(verseRange[1]);
                return (surah, verseStart, verseEnd);
            }
            else
            {
                var verse = int.Parse(versePart);
                return (surah, verse, verse);
            }
        }
        catch
        {
            return (0, 0, 0);
        }
    }
}
