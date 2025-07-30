using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Muwasala.Core.Models;

namespace Muwasala.KnowledgeBase.Services;

/// <summary>
/// Service de recherche dans le Coran basé sur les fichiers sources
/// Format des fichiers : SOURATE|VERSET|TEXTE
/// </summary>
public class FileBasedQuranSearchService
{
    private readonly string _basePath;
    private static readonly Dictionary<string, List<QuranVerse>> _cachedVerses = new();
    private static readonly object _cacheLock = new();
    
    public FileBasedQuranSearchService()
    {
        // Déterminer le chemin vers les fichiers sources
        _basePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "..",
            "data", "Quran", "SourceFiles"
        );
        
        // Si ce chemin n'existe pas, utiliser le chemin absolu
        if (!Directory.Exists(_basePath))
        {
            _basePath = "D:\\Data Perso Adnane\\Coding\\VSCodeProject\\Muwasala Islamic Knowledge Network V2\\Muwasala-Islamic-Knowledge-Network\\data\\Quran\\SourceFiles";
        }
    }

    /// <summary>
    /// Recherche des versets par mots-clés avec support multilingue
    /// </summary>
    public async Task<List<QuranVerse>> SearchVersesByContextAsync(string query, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<QuranVerse>();

        var queryTerms = query.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Charger les versets en arabe et anglais
        var arabicVerses = await LoadVersesFromFileAsync("quran-uthmani.txt", isArabic: true);
        var englishVerses = await LoadVersesFromFileAsync("en.sahih.txt", isArabic: false);

        // Créer un dictionnaire pour les traductions
        var translationDict = englishVerses.ToDictionary(v => (v.Surah, v.Verse), v => v.Translation);

        // Combiner les résultats avec un score de pertinence
        var scoredResults = new List<(QuranVerse verse, double score)>();

        // Rechercher dans les versets arabes
        foreach (var verse in arabicVerses)
        {
            var score = CalculateRelevanceScore(verse.ArabicText, queryTerms, isArabic: true);
            if (score > 0)
            {
                // Trouver la traduction correspondante
                var translation = translationDict.GetValueOrDefault((verse.Surah, verse.Verse), "");
                
                var combinedVerse = new QuranVerse(
                    verse.Surah,
                    verse.Verse,
                    verse.ArabicText,
                    translation,
                    verse.Transliteration,
                    verse.Language
                );
                
                scoredResults.Add((combinedVerse, score));
            }
        }

        // Rechercher dans les traductions anglaises pour les versets non trouvés
        var arabicDict = arabicVerses.ToDictionary(v => (v.Surah, v.Verse), v => v.ArabicText);
        
        foreach (var verse in englishVerses)
        {
            var score = CalculateRelevanceScore(verse.Translation, queryTerms, isArabic: false);
            if (score > 0)
            {
                // Vérifier si ce verset n'est pas déjà dans les résultats
                var existingResult = scoredResults.FirstOrDefault(r => 
                    r.verse.Surah == verse.Surah && r.verse.Verse == verse.Verse);
                
                if (existingResult.verse == null)
                {
                    // Trouver le texte arabe correspondant
                    var arabicText = arabicDict.GetValueOrDefault((verse.Surah, verse.Verse), "");
                    
                    var combinedVerse = new QuranVerse(
                        verse.Surah,
                        verse.Verse,
                        arabicText,
                        verse.Translation,
                        verse.Transliteration,
                        verse.Language
                    );
                    
                    scoredResults.Add((combinedVerse, score));
                }
                else
                {
                    // Améliorer le score existant
                    var index = scoredResults.IndexOf(existingResult);
                    scoredResults[index] = (existingResult.verse, existingResult.score + score * 0.5);
                }
            }
        }

        // Trier par score de pertinence et retourner les meilleurs résultats
        return scoredResults
            .OrderByDescending(r => r.score)
            .Take(maxResults)
            .Select(r => r.verse)
            .ToList();
    }

    /// <summary>
    /// Charge les versets depuis un fichier avec mise en cache
    /// </summary>
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
            return new List<QuranVerse>();
        }

        var verses = new List<QuranVerse>();
        var lines = await File.ReadAllLinesAsync(filePath);

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

        lock (_cacheLock)
        {
            _cachedVerses[fileName] = verses;
        }

        return verses;
    }

    /// <summary>
    /// Calcule un score de pertinence pour un texte donné
    /// </summary>
    private double CalculateRelevanceScore(string text, string[] queryTerms, bool isArabic)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;

        var normalizedText = text.ToLowerInvariant();
        double score = 0;

        foreach (var term in queryTerms)
        {
            if (string.IsNullOrWhiteSpace(term)) continue;

            // Recherche exacte (score le plus élevé)
            if (normalizedText.Contains(term))
            {
                score += 3.0;

                // Bonus si le terme apparaît au début
                if (normalizedText.StartsWith(term))
                    score += 2.0;

                // Compter les occurrences multiples
                var occurrences = Regex.Matches(normalizedText, Regex.Escape(term), RegexOptions.IgnoreCase).Count;
                if (occurrences > 1)
                    score += (occurrences - 1) * 0.5;
            }
            
            // Recherche partielle pour les mots longs
            else if (term.Length >= 4)
            {
                var words = normalizedText.Split(new char[] { ' ', '.', ',', ';', ':', '!', '?' }, 
                    StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var word in words)
                {
                    if (word.Contains(term))
                        score += 1.0;
                    else if (word.StartsWith(term))
                        score += 1.5;
                    else if (word.EndsWith(term))
                        score += 1.2;
                }
            }
        }

        // Bonus pour les textes plus courts (généralement plus pertinents)
        if (text.Length < 200)
            score *= 1.2;

        // Bonus léger pour l'arabe (texte original)
        if (isArabic && score > 0)
            score *= 1.1;

        return score;
    }

    /// <summary>
    /// Vide le cache (utile pour les tests)
    /// </summary>
    public static void ClearCache()
    {
        lock (_cacheLock)
        {
            _cachedVerses.Clear();
        }
    }
}
