using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Muwasala.KnowledgeBase.Utils;

/// <summary>
/// Utilitaire pour analyser le format des fichiers sources du Coran
/// </summary>
public class QuranFileAnalyzer
{
    public static async Task AnalyzeFiles()
    {
        var basePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "..", "..", "..", "..", "..", 
            "data", "Quran", "SourceFiles"
        );

        var arabicFile = Path.Combine(basePath, "quran-uthmani.txt");
        var englishFile = Path.Combine(basePath, "en.sahih.txt");

        Console.WriteLine("📁 Analyse des fichiers sources du Coran");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine($"📂 Chemin de base: {basePath}");
        Console.WriteLine($"📄 Fichier arabe: {Path.GetFileName(arabicFile)}");
        Console.WriteLine($"📄 Fichier anglais: {Path.GetFileName(englishFile)}");
        Console.WriteLine();

        // Vérifier l'existence des fichiers
        if (!File.Exists(arabicFile))
        {
            Console.WriteLine($"❌ Fichier arabe non trouvé: {arabicFile}");
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
        var maxLines = Math.Min(5, Math.Min(arabicLines.Length, englishLines.Length));
        
        for (int i = 0; i < maxLines; i++)
        {
            Console.WriteLine($"\n   Ligne {i + 1}:");
            Console.WriteLine($"   🔤 AR: {arabicLines[i].Substring(0, Math.Min(60, arabicLines[i].Length))}...");
            Console.WriteLine($"   🇬🇧 EN: {englishLines[i].Substring(0, Math.Min(60, englishLines[i].Length))}...");
        }
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
            
            Console.WriteLine($"   🥇 Première ligne: {firstLine.Substring(0, Math.Min(50, firstLine.Length))}...");
            Console.WriteLine($"   🥉 Dernière ligne: {lastLine.Substring(0, Math.Min(50, lastLine.Length))}...");
            
            var avgLength = lines.Where(l => !string.IsNullOrWhiteSpace(l))
                                .Average(l => l.Length);
            Console.WriteLine($"   📐 Longueur moyenne: {avgLength:F1} caractères");
        }
    }
}

/// <summary>
/// Programme pour tester l'analyseur de fichiers
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            await QuranFileAnalyzer.AnalyzeFiles();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur: {ex.Message}");
        }
    }
}
