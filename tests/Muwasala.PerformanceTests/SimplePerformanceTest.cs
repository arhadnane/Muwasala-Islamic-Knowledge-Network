using System.Diagnostics;

namespace Muwasala.PerformanceTests;

public class Program
{
    private static readonly HttpClient httpClient = new HttpClient();
    private const string BaseUrl = "http://localhost:5237";

    public static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Démarrage des tests de performance pour Muwasala Islamic Knowledge Network");
        Console.WriteLine("================================================================================");

        await TestSearchEndpointPerformance();
        await TestCacheEffectiveness();
        await TestPaginationPerformance();
        
        Console.WriteLine("\n✅ Tests de performance terminés!");
    }

    private static async Task TestSearchEndpointPerformance()
    {
        Console.WriteLine("\n📊 Test 1: Performance de l'endpoint de recherche");
        Console.WriteLine("-".PadRight(50, '-'));

        var queries = new[] { "Islam", "Quran", "Prophet", "Prayer", "Hadith" };
        var results = new List<long>();

        foreach (var query in queries)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var response = await httpClient.GetAsync($"{BaseUrl}/api/islamicagents/search?query={Uri.EscapeDataString(query)}&page=1&pageSize=10");
                sw.Stop();
                
                if (response.IsSuccessStatusCode)
                {
                    results.Add(sw.ElapsedMilliseconds);
                    Console.WriteLine($"  ✓ Query '{query}': {sw.ElapsedMilliseconds}ms");
                }
                else
                {
                    Console.WriteLine($"  ❌ Query '{query}': Failed ({response.StatusCode})");
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                Console.WriteLine($"  ❌ Query '{query}': Error - {ex.Message}");
            }

            await Task.Delay(100); // Small delay between requests
        }

        if (results.Any())
        {
            var avgTime = results.Average();
            var minTime = results.Min();
            var maxTime = results.Max();
            
            Console.WriteLine($"\n📈 Résultats:");
            Console.WriteLine($"  • Temps moyen: {avgTime:F2}ms");
            Console.WriteLine($"  • Temps minimum: {minTime}ms");
            Console.WriteLine($"  • Temps maximum: {maxTime}ms");
            Console.WriteLine($"  • Requêtes réussies: {results.Count}/{queries.Length}");
        }
    }

    private static async Task TestCacheEffectiveness()
    {
        Console.WriteLine("\n💾 Test 2: Efficacité du cache");
        Console.WriteLine("-".PadRight(50, '-'));

        var query = "Islamic knowledge";
        var times = new List<long>();

        // Premier appel (sans cache)
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await httpClient.GetAsync($"{BaseUrl}/api/islamicagents/search?query={Uri.EscapeDataString(query)}&page=1&pageSize=10");
            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
            Console.WriteLine($"  1er appel (sans cache): {sw.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Premier appel échoué: {ex.Message}");
            return;
        }

        await Task.Delay(100);

        // Appels suivants (avec cache potentiel)
        for (int i = 2; i <= 5; i++)
        {
            sw = Stopwatch.StartNew();
            try
            {
                var response = await httpClient.GetAsync($"{BaseUrl}/api/islamicagents/search?query={Uri.EscapeDataString(query)}&page=1&pageSize=10");
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
                Console.WriteLine($"  {i}e appel (avec cache): {sw.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ {i}e appel échoué: {ex.Message}");
            }
            
            await Task.Delay(100);
        }

        if (times.Count >= 2)
        {
            var firstCall = times[0];
            var cachedCalls = times.Skip(1).Average();
            var improvement = ((firstCall - cachedCalls) / firstCall) * 100;
            
            Console.WriteLine($"\n📈 Résultats du cache:");
            Console.WriteLine($"  • Premier appel: {firstCall}ms");
            Console.WriteLine($"  • Appels avec cache (moyenne): {cachedCalls:F2}ms");
            Console.WriteLine($"  • Amélioration: {improvement:F1}%");
        }
    }

    private static async Task TestPaginationPerformance()
    {
        Console.WriteLine("\n📄 Test 3: Performance de la pagination");
        Console.WriteLine("-".PadRight(50, '-'));

        var query = "Quran";
        var pageSizes = new[] { 5, 10, 20, 50 };
        
        foreach (var pageSize in pageSizes)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var response = await httpClient.GetAsync($"{BaseUrl}/api/islamicagents/search?query={Uri.EscapeDataString(query)}&page=1&pageSize={pageSize}");
                sw.Stop();
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"  ✓ Page size {pageSize}: {sw.ElapsedMilliseconds}ms");
                }
                else
                {
                    Console.WriteLine($"  ❌ Page size {pageSize}: Failed ({response.StatusCode})");
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                Console.WriteLine($"  ❌ Page size {pageSize}: Error - {ex.Message}");
            }

            await Task.Delay(100);
        }
    }
}
