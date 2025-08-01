using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace TirmidhiImporter;

/// <summary>
/// Importer dédié pour Jami` at-Tirmidhi depuis sunnah.com
/// </summary>
public class Program
{
    private static readonly HttpClient _httpClient = new();
    private static readonly string _baseUrl = "https://sunnah.com";
    
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🕌 JAMI` AT-TIRMIDHI DIRECT IMPORT");
        Console.WriteLine("===================================");
        
        try
        {
            // Configuration respectueuse
            _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                "Muwasala Islamic Knowledge Network - Educational Research Tool");
            _httpClient.Timeout = TimeSpan.FromMinutes(3);
            
            Console.WriteLine("📚 Starting Jami` at-Tirmidhi import...");
            
            var hadiths = await ImportTirmidhiAsync();
            
            // Sauvegarder en JSON
            var outputPath = "../Tools/jami_tirmidhi_complete.json";
            await SaveToJsonAsync(hadiths, outputPath);
            
            Console.WriteLine("🎉 IMPORT COMPLETED!");
            Console.WriteLine($"📊 Total hadiths extracted: {hadiths.Count:N0}");
            Console.WriteLine($"💾 Saved to: {outputPath}");
            Console.WriteLine("🚀 Ready for database integration!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"📋 Details: {ex}");
        }
        finally
        {
            _httpClient.Dispose();
        }
    }
    
    private static async Task<List<HadithRecord>> ImportTirmidhiAsync()
    {
        var allHadiths = new List<HadithRecord>();
        
        // Récupérer la page principale
        Console.WriteLine("🔗 Fetching: https://sunnah.com/tirmidhi");
        var mainHtml = await GetPageContentAsync("https://sunnah.com/tirmidhi");
        
        if (string.IsNullOrEmpty(mainHtml))
        {
            throw new Exception("Failed to fetch main page");
        }
        
        // Extraire les livres
        var books = ExtractBooks(mainHtml);
        Console.WriteLine($"📖 Found {books.Count} books in Jami` at-Tirmidhi");
        
        for (int i = 0; i < books.Count; i++)
        {
            var book = books[i];
            try
            {
                Console.WriteLine($"📚 Book {i + 1}/{books.Count}: {book.Title} (#{book.Number})");
                
                var bookHadiths = await ImportBookAsync(book);
                allHadiths.AddRange(bookHadiths);
                
                Console.WriteLine($"   ✅ Extracted {bookHadiths.Count} hadiths");
                
                // Délai respectueux entre les livres
                await Task.Delay(1500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error in book {book.Title}: {ex.Message}");
            }
        }
        
        return allHadiths;
    }
    
    private static List<BookInfo> ExtractBooks(string html)
    {
        var books = new List<BookInfo>();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        // Sélecteur pour les liens de livres
        var bookNodes = doc.DocumentNode.SelectNodes("//a[contains(@href, '/tirmidhi/')]");
        
        if (bookNodes != null)
        {
            foreach (var bookNode in bookNodes)
            {
                var href = bookNode.GetAttributeValue("href", "");
                var match = Regex.Match(href, @"/tirmidhi/(\d+)");
                
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
    
    private static async Task<List<HadithRecord>> ImportBookAsync(BookInfo book)
    {
        var hadiths = new List<HadithRecord>();
        
        var bookUrl = $"{_baseUrl}/tirmidhi/{book.Number}";
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
                    var hadith = ExtractHadithFromNode(hadithNode, book);
                    if (hadith != null)
                    {
                        hadiths.Add(hadith);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ⚠️ Warning: Failed to extract hadith: {ex.Message}");
                }
            }
        }
        
        return hadiths;
    }
    
    private static HadithRecord? ExtractHadithFromNode(HtmlNode hadithNode, BookInfo book)
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
            
            // Extraire le grade (si disponible) - Tirmidhi est célèbre pour ses classifications détaillées
            var gradeNode = hadithNode.SelectSingleNode(".//span[contains(@class, 'grade')]");
            var gradeText = gradeNode?.InnerText?.Trim() ?? "";
            var grade = DetermineGrade(gradeText);
            
            if (string.IsNullOrEmpty(translation) && string.IsNullOrEmpty(arabicText))
            {
                return null;
            }
            
            return new HadithRecord
            {
                ArabicText = arabicText,
                Translation = translation,
                Grade = grade,
                Collection = "Jami` at-Tirmidhi",
                BookNumber = book.Number.ToString(),
                HadithNumber = hadithNumber,
                SanadChain = ParseSanadChain(sanadText),
                BookTitle = book.Title,
                Language = "en"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ⚠️ Error extracting hadith details: {ex.Message}");
            return null;
        }
    }
    
    private static string DetermineGrade(string gradeText)
    {
        gradeText = gradeText.ToLowerInvariant();
        
        // Tirmidhi utilise des classifications spécifiques
        if (gradeText.Contains("sahih") || gradeText.Contains("authentic"))
            return "Sahih";
        if (gradeText.Contains("hasan") || gradeText.Contains("good"))
            return "Hasan";
        if (gradeText.Contains("daif") || gradeText.Contains("weak"))
            return "Daif";
        if (gradeText.Contains("gharib") || gradeText.Contains("rare"))
            return "Hasan"; // Les hadiths gharib de Tirmidhi sont généralement acceptables
        if (gradeText.Contains("mawdu") || gradeText.Contains("fabricated"))
            return "Mawdu";
        
        // Pour Jami` at-Tirmidhi, par défaut Hasan (collection très respectée)
        return "Hasan";
    }
    
    private static List<string> ParseSanadChain(string sanadText)
    {
        if (string.IsNullOrEmpty(sanadText))
            return new List<string>();
        
        var cleanSanad = sanadText.Replace("Narrated ", "")
                                  .Replace(":", "")
                                  .Replace(" that ", " → ");
        
        return cleanSanad.Split(" → ", StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => s.Trim())
                         .Where(s => !string.IsNullOrEmpty(s))
                         .ToList();
    }
    
    private static string CleanText(string text)
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
    
    private static async Task<string> GetPageContentAsync(string url)
    {
        try
        {
            Console.WriteLine($"   📥 Fetching: {url}");
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            
            // Délai respectueux
            await Task.Delay(800);
            
            return content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error fetching {url}: {ex.Message}");
            return string.Empty;
        }
    }
    
    private static async Task SaveToJsonAsync(List<HadithRecord> hadiths, string filePath)
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
            
            Console.WriteLine($"💾 Hadiths saved to: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error saving hadiths: {ex.Message}");
        }
    }
}

public class HadithRecord
{
    public string ArabicText { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string Collection { get; set; } = string.Empty;
    public string BookNumber { get; set; } = string.Empty;
    public string HadithNumber { get; set; } = string.Empty;
    public List<string> SanadChain { get; set; } = new();
    public string BookTitle { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}

public class BookInfo
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
