using Microsoft.EntityFrameworkCore;
using Muwasala.KnowledgeBase.Data;
using Muwasala.KnowledgeBase.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = "Data Source=d:\\Coding\\VSCodeProject\\Muwasala Islamic Knowledge Network\\data\\database\\IslamicKnowledge.db";
        
        var services = new ServiceCollection();
        services.AddDbContext<IslamicKnowledgeDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
        services.AddScoped<IGlobalSearchService, GlobalSearchService>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IslamicKnowledgeDbContext>();
        var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        var searchService = scope.ServiceProvider.GetRequiredService<IGlobalSearchService>();
        
        try
        {
            // Ensure database exists
            await context.Database.EnsureCreatedAsync();
            
            // Check current counts
            var quranCount = await context.QuranVerses.CountAsync();
            var hadithCount = await context.HadithTexts.CountAsync();
            var fiqhCount = await context.FiqhRulings.CountAsync();
            var duaCount = await context.Duas.CountAsync();
            var sirahCount = await context.SirahEvents.CountAsync();
            var tajweedCount = await context.TajweedRules.CountAsync();
            
            var totalRecords = quranCount + hadithCount + fiqhCount + duaCount + sirahCount + tajweedCount;
            
            // Write results to a file so we can see them
            var results = $"""
                === Database Status ===
                Quran verses: {quranCount}
                Hadith texts: {hadithCount}
                Fiqh rulings: {fiqhCount}
                Duas: {duaCount}
                Sirah events: {sirahCount}
                Tajweed rules: {tajweedCount}
                Total records: {totalRecords}
                
                """;
            
            if (totalRecords == 0)
            {
                results += "Database is empty! Initializing...\n";
                await initializer.InitializeAsync();
                
                // Recount after initialization
                quranCount = await context.QuranVerses.CountAsync();
                hadithCount = await context.HadithTexts.CountAsync();
                fiqhCount = await context.FiqhRulings.CountAsync();
                duaCount = await context.Duas.CountAsync();
                sirahCount = await context.SirahEvents.CountAsync();
                tajweedCount = await context.TajweedRules.CountAsync();
                
                results += $"""
                    
                    === After Initialization ===
                    Quran verses: {quranCount}
                    Hadith texts: {hadithCount}
                    Fiqh rulings: {fiqhCount}
                    Duas: {duaCount}
                    Sirah events: {sirahCount}
                    Tajweed rules: {tajweedCount}
                    
                    """;
            }
            
            // Test search
            results += "=== Testing Search ===\n";
            var searchResults = await searchService.SearchAsync("Allah", 3);
            results += $"Search for 'Allah' returned {searchResults.Count()} results:\n";
            
            foreach (var result in searchResults)
            {
                results += $"- {result.Source}: {result.Title}\n";
                results += $"  {result.Content.Substring(0, Math.Min(100, result.Content.Length))}...\n\n";
            }
            
            await File.WriteAllTextAsync("database_check_results.txt", results);
        }
        catch (Exception ex)
        {
            var errorResults = $"Error: {ex.Message}\nStack trace: {ex.StackTrace}";
            await File.WriteAllTextAsync("database_check_results.txt", errorResults);
        }
    }
}
