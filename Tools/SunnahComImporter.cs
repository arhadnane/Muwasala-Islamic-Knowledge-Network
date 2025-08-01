using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Muwasala.Core.Models;
using Muwasala.KnowledgeBase.Services;

namespace Muwasala.Tools;

/// <summary>
/// Importer pour récupérer tous les hadiths depuis sunnah.com
/// Respecte les conditions d'utilisation et utilise des délais appropriés
/// </summary>
public class SunnahComImporter
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SunnahComImporter> _logger;
    private readonly string _baseUrl = "https://sunnah.com";
    
    // Collections principales de hadiths sur sunnah.com
    private readonly Dictionary<string, string> _collections = new()
    {
        // Collections Sahih (les plus authentiques)
        { "bukhari", "Sahih al-Bukhari" },
        { "muslim", "Sahih Muslim" },
        
        // Sunan Collections (Six Books of Hadith)
        { "abudawud", "Sunan Abi Dawud" },
        { "tirmidhi", "Jami` at-Tirmidhi" },
        { "nasai", "Sunan an-Nasa'i" },
        { "ibnmajah", "Sunan Ibn Majah" },
        
        // Collections Malikite
        { "malik", "Muwatta Malik" },
        
        // Collections Hanbalite
        { "ahmad", "Musnad Ahmad" },
        
        // Collections supplémentaires de Sunan
        { "darimi", "Sunan ad-Darimi" },
        { "hakim", "Al-Mustadrak 'ala al-Sahihain" },
        
        // Collections thématiques
        { "riyadussaliheen", "Riyad as-Salihin" },
        { "adab", "Al-Adab Al-Mufrad" },
        { "shamail", "Shamail Muhammadiyah" },
        { "bulugh", "Bulugh al-Maram" },
        
        // Collections de 40 Hadiths
        { "qudsi40", "40 Hadith Qudsi" },
        { "nawawi40", "40 Hadith Nawawi" },
        
        // Collections additionnelles
        { "mishkat", "Mishkat al-Masabih" },
        { "hisn", "Hisn al-Muslim" },
        { "adkar", "Al-Adhkar" },
        { "targheeb", "At-Targheeb wat-Tarheeb" },
        { "kanz", "Kanz al-Ummal" },
        { "majmauzzawaid", "Majma' az-Zawa'id" }
    };

    public SunnahComImporter(HttpClient httpClient, ILogger<SunnahComImporter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Configuration respectueuse du serveur
        _httpClient.DefaultRequestHeaders.Add("User-Agent", 
            "Muwasala Islamic Knowledge Network - Educational Research Tool");
        _httpClient.Timeout = TimeSpan.FromMinutes(2);
    }

    /// <summary>
    /// Importe tous les hadiths de toutes les collections
    /// </summary>
    public async Task<List<HadithRecord>> ImportAllHadithsAsync(bool authenticOnly = true)
    {
        var allHadiths = new List<HadithRecord>();
        
        _logger.LogInformation("Début de l'importation de tous les hadiths depuis sunnah.com");
        
        foreach (var collection in _collections)
        {
            try
            {
                _logger.LogInformation("Importation de la collection: {Collection}", collection.Value);
                
                var collectionHadiths = await ImportCollectionAsync(collection.Key, collection.Value);
                
                // Filtrer uniquement les hadiths authentiques si demandé
                if (authenticOnly)
                {
                    collectionHadiths = collectionHadiths
                        .Where(h => h.Grade == HadithGrade.Sahih || h.Grade == HadithGrade.Hasan)
                        .ToList();
                }
                
                allHadiths.AddRange(collectionHadiths);
                
                _logger.LogInformation("Importé {Count} hadiths de {Collection}", 
                    collectionHadiths.Count, collection.Value);
                
                // Délai respectueux entre les collections
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'importation de {Collection}", collection.Value);
            }
        }
        
        _logger.LogInformation("Importation terminée. Total: {Count} hadiths importés", allHadiths.Count);
        return allHadiths;
    }

    /// <summary>
    /// Importe une collection spécifique de hadiths
    /// </summary>
    public async Task<List<HadithRecord>> ImportCollectionAsync(string collectionKey, string collectionName)
    {
        var hadiths = new List<HadithRecord>();
        
        try
        {
            // Collections spéciales qui ont tous les hadiths sur une seule page
            var singlePageCollections = new[] { 
                "qudsi40", "nawawi40", "bulugh", "hisn", "adkar"
            };
            
            if (singlePageCollections.Contains(collectionKey))
            {
                return await ImportSinglePageCollectionAsync(collectionKey, collectionName);
            }
            
            // Récupérer la page principale de la collection
            var collectionUrl = $"{_baseUrl}/{collectionKey}";
            var collectionHtml = await GetPageContentAsync(collectionUrl);
            
            if (string.IsNullOrEmpty(collectionHtml))
            {
                _logger.LogWarning("Impossible de récupérer la page de collection: {Url}", collectionUrl);
                return hadiths;
            }

            // Parser la structure des livres/chapitres
            var books = ExtractBooks(collectionHtml, collectionKey);
            
            _logger.LogInformation("Trouvé {Count} livres dans {Collection}", books.Count, collectionName);

            foreach (var book in books)
            {
                try
                {
                    var bookHadiths = await ImportBookAsync(collectionKey, collectionName, book);
                    hadiths.AddRange(bookHadiths);
                    
                    // Délai entre les livres
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'importation du livre {Book}", book.Title);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'importation de la collection {Collection}", collectionName);
        }

        return hadiths;
    }

    /// <summary>
    /// Importe une collection qui a tous ses hadiths sur une seule page
    /// </summary>
    private async Task<List<HadithRecord>> ImportSinglePageCollectionAsync(string collectionKey, string collectionName)
    {
        var hadiths = new List<HadithRecord>();
        
        try
        {
            var collectionUrl = $"{_baseUrl}/{collectionKey}";
            var html = await GetPageContentAsync(collectionUrl);
            
            if (string.IsNullOrEmpty(html))
            {
                _logger.LogWarning("Impossible de récupérer la page: {Url}", collectionUrl);
                return hadiths;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Extraire tous les hadiths directement de la page
            var hadithNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'actualHadithContainer')]");
            
            if (hadithNodes != null)
            {
                _logger.LogInformation("Trouvé {Count} hadiths dans {Collection}", hadithNodes.Count, collectionName);
                
                foreach (var hadithNode in hadithNodes)
                {
                    try
                    {
                        var hadith = ExtractHadithFromSinglePageNode(hadithNode, collectionName);
                        if (hadith != null)
                        {
                            hadiths.Add(hadith);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erreur lors de l'extraction d'un hadith");
                    }
                }
            }
            else
            {
                _logger.LogWarning("Aucun nœud hadith trouvé avec le sélecteur 'actualHadithContainer'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'importation de la collection single-page {Collection}", collectionName);
        }
        
        return hadiths;
    }

    /// <summary>
    /// Importe tous les hadiths d'un livre spécifique
    /// </summary>
    private async Task<List<HadithRecord>> ImportBookAsync(string collectionKey, string collectionName, BookInfo book)
    {
        var hadiths = new List<HadithRecord>();
        
        try
        {
            var bookUrl = $"{_baseUrl}/{collectionKey}/{book.Number}";
            var bookHtml = await GetPageContentAsync(bookUrl);
            
            if (string.IsNullOrEmpty(bookHtml))
            {
                return hadiths;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(bookHtml);

            // Extraire tous les hadiths de ce livre
            var hadithNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'hadith')]");
            
            if (hadithNodes != null)
            {
                foreach (var hadithNode in hadithNodes)
                {
                    try
                    {
                        var hadith = ExtractHadithFromNode(hadithNode, collectionName, book);
                        if (hadith != null)
                        {
                            hadiths.Add(hadith);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erreur lors de l'extraction d'un hadith");
                    }
                }
            }

            _logger.LogInformation("Importé {Count} hadiths du livre {Book}", hadiths.Count, book.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'importation du livre {Book}", book.Title);
        }

        return hadiths;
    }

    /// <summary>
    /// Extrait un hadith depuis un nœud HTML
    /// </summary>
    private HadithRecord? ExtractHadithFromNode(HtmlNode hadithNode, string collection, BookInfo book)
    {
        try
        {
            // Extraire le numéro du hadith
            var hadithNumberNode = hadithNode.SelectSingleNode(".//span[contains(@class, 'hadith_number')]");
            var hadithNumber = hadithNumberNode?.InnerText?.Trim() ?? "0";

            // Extraire le texte arabe
            var arabicNode = hadithNode.SelectSingleNode(".//div[contains(@class, 'arabic')]");
            var arabicText = CleanText(arabicNode?.InnerText ?? "");

            // Extraire la traduction anglaise
            var englishNode = hadithNode.SelectSingleNode(".//div[contains(@class, 'english_hadith')]");
            var translation = CleanText(englishNode?.InnerText ?? "");

            // Extraire la chaîne de narration (Sanad)
            var sanadNode = hadithNode.SelectSingleNode(".//div[contains(@class, 'hadith_narrated')]");
            var sanadText = CleanText(sanadNode?.InnerText ?? "");

            // Extraire le grade (si disponible)
            var gradeNode = hadithNode.SelectSingleNode(".//span[contains(@class, 'grade')]");
            var gradeText = gradeNode?.InnerText?.Trim() ?? "";
            var grade = DetermineGrade(gradeText, collection);

            // Extraire les références
            var referenceNode = hadithNode.SelectSingleNode(".//div[contains(@class, 'hadith_reference')]");
            var reference = CleanText(referenceNode?.InnerText ?? "");

            if (string.IsNullOrEmpty(translation) && string.IsNullOrEmpty(arabicText))
            {
                return null;
            }

            return new HadithRecord(
                arabicText,
                translation,
                grade,
                collection,
                book.Number.ToString(),
                hadithNumber,
                ParseSanadChain(sanadText),
                book.Title,
                "en"
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur lors de l'extraction des détails du hadith");
            return null;
        }
    }

    /// <summary>
    /// Extrait la structure des livres depuis la page de collection
    /// </summary>
    private List<BookInfo> ExtractBooks(string html, string collectionKey)
    {
        var books = new List<BookInfo>();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Sélecteur pour les liens de livres
        var bookNodes = doc.DocumentNode.SelectNodes($"//a[contains(@href, '/{collectionKey}/')]");
        
        if (bookNodes != null)
        {
            foreach (var bookNode in bookNodes)
            {
                var href = bookNode.GetAttributeValue("href", "");
                var match = Regex.Match(href, $@"/{collectionKey}/(\d+)");
                
                if (match.Success && int.TryParse(match.Groups[1].Value, out int bookNumber))
                {
                    var title = CleanText(bookNode.InnerText);
                    
                    if (!string.IsNullOrEmpty(title) && !books.Any(b => b.Number == bookNumber))
                    {
                        books.Add(new BookInfo
                        {
                            Number = bookNumber,
                            Title = title,
                            Url = $"{_baseUrl}{href}"
                        });
                    }
                }
            }
        }

        return books.OrderBy(b => b.Number).ToList();
    }

    /// <summary>
    /// Récupère le contenu HTML d'une page avec gestion d'erreurs
    /// </summary>
    private async Task<string> GetPageContentAsync(string url)
    {
        try
        {
            _logger.LogDebug("Récupération de: {Url}", url);
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            
            // Délai respectueux entre les requêtes
            await Task.Delay(500);
            
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de {Url}", url);
            return string.Empty;
        }
    }

    /// <summary>
    /// Détermine le grade du hadith basé sur le texte et la collection
    /// </summary>
    private HadithGrade DetermineGrade(string gradeText, string collection)
    {
        gradeText = gradeText.ToLowerInvariant();

        // Collections considérées comme Sahih par défaut
        if (collection.Contains("Sahih al-Bukhari") || 
            collection.Contains("Sahih Muslim") ||
            collection.Contains("40 Hadith Qudsi") ||
            collection.Contains("40 Hadith Nawawi") ||
            collection.Contains("Bulugh al-Maram"))
        {
            return HadithGrade.Sahih;
        }

        // Collections avec grades mixtes mais généralement bonnes
        if (collection.Contains("Riyad as-Salihin") ||
            collection.Contains("Al-Adab Al-Mufrad") ||
            collection.Contains("Hisn al-Muslim"))
        {
            return HadithGrade.Hasan;
        }

        // Analyse du texte du grade
        if (gradeText.Contains("sahih") || gradeText.Contains("authentic"))
            return HadithGrade.Sahih;
        if (gradeText.Contains("hasan") || gradeText.Contains("good"))
            return HadithGrade.Hasan;
        if (gradeText.Contains("daif") || gradeText.Contains("weak"))
            return HadithGrade.Daif;
        if (gradeText.Contains("mawdu") || gradeText.Contains("fabricated"))
            return HadithGrade.Mawdu;

        // Grade par défaut basé sur la collection
        if (collection.Contains("Sunan") || collection.Contains("Musnad"))
            return HadithGrade.Hasan;
        
        return HadithGrade.Sahih;
    }

    /// <summary>
    /// Parse la chaîne de narration
    /// </summary>
    private List<string> ParseSanadChain(string sanadText)
    {
        if (string.IsNullOrEmpty(sanadText))
            return new List<string>();

        // Nettoyer et diviser la chaîne de narration
        var cleanSanad = sanadText.Replace("Narrated ", "")
                                  .Replace(":", "")
                                  .Replace(" that ", " → ");

        return cleanSanad.Split(" → ", StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => s.Trim())
                         .Where(s => !string.IsNullOrEmpty(s))
                         .ToList();
    }

    /// <summary>
    /// Nettoie le texte en supprimant les caractères indésirables
    /// </summary>
    private string CleanText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return Regex.Replace(text, @"\s+", " ")
                   .Replace("&nbsp;", " ")
                   .Replace("&quot;", "\"")
                   .Replace("&amp;", "&")
                   .Replace("&lt;", "<")
                   .Replace("&gt;", ">")
                   .Trim();
    }

    /// <summary>
    /// Extrait un hadith depuis un nœud HTML pour les collections single-page
    /// </summary>
    private HadithRecord? ExtractHadithFromSinglePageNode(HtmlNode hadithNode, string collectionName)
    {
        try
        {
            // Extraire le numéro depuis la référence
            var referenceNode = hadithNode.SelectSingleNode(".//span[contains(@class, 'hadith_reference')]");
            var referenceText = referenceNode?.InnerText?.Trim() ?? "";
            var hadithNumber = ExtractHadithNumber(referenceText);

            // Extraire le texte anglais
            var englishNodes = hadithNode.SelectNodes(".//div[contains(@class, 'englishcontainer')]//text()[normalize-space()]");
            var englishText = "";
            if (englishNodes != null)
            {
                englishText = string.Join(" ", englishNodes.Select(n => n.InnerText.Trim()));
            }

            // Extraire le texte arabe
            var arabicNodes = hadithNode.SelectNodes(".//div[contains(@class, 'arabiccontainer')]//text()[normalize-space()]");
            var arabicText = "";
            if (arabicNodes != null)
            {
                arabicText = string.Join(" ", arabicNodes.Select(n => n.InnerText.Trim()));
            }

            // Nettoyer les textes
            englishText = CleanText(englishText);
            arabicText = CleanText(arabicText);

            if (string.IsNullOrEmpty(englishText) && string.IsNullOrEmpty(arabicText))
            {
                return null;
            }

            // Pour les hadiths Qudsi, le grade est typiquement Sahih
            var grade = collectionName.Contains("Qudsi") ? HadithGrade.Sahih : HadithGrade.Hasan;

            return new HadithRecord(
                arabicText,
                englishText,
                grade,
                collectionName,
                "1", // Book number pour single-page collections
                hadithNumber,
                new List<string>(), // Pas de sanad explicite extrait
                collectionName,
                "en"
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur lors de l'extraction d'un hadith single-page");
            return null;
        }
    }

    /// <summary>
    /// Extrait le numéro de hadith depuis le texte de référence
    /// </summary>
    private string ExtractHadithNumber(string referenceText)
    {
        var match = Regex.Match(referenceText, @"Hadith\s+(\d+)");
        return match.Success ? match.Groups[1].Value : "1";
    }

    /// <summary>
    /// Sauvegarde les hadiths dans un fichier JSON
    /// </summary>
    public async Task SaveToJsonAsync(List<HadithRecord> hadiths, string filePath)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(hadiths, options);
            await File.WriteAllTextAsync(filePath, json, System.Text.Encoding.UTF8);
            
            _logger.LogInformation("Hadiths sauvegardés dans: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la sauvegarde des hadiths");
        }
    }
}

/// <summary>
/// Informations sur un livre de hadiths
/// </summary>
public class BookInfo
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
