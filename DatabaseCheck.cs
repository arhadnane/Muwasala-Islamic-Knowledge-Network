using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Muwasala.KnowledgeBase.Data;
using Muwasala.KnowledgeBase.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration.AddJsonFile("src/Muwasala.Web/appsettings.json", optional: false);

// Add services
builder.Services.AddDbContext<IslamicKnowledgeDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                     "Data Source=muwasala.db"));

builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
builder.Services.AddScoped<IGlobalSearchService, GlobalSearchService>();

var app = builder.Build();

// Check database status
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<IslamicKnowledgeDbContext>();
var searchService = scope.ServiceProvider.GetRequiredService<IGlobalSearchService>();

Console.WriteLine("=== Database Status Check ===");

try
{
    // Check if database exists and has tables
    await context.Database.EnsureCreatedAsync();
    
    // Count records in each table
    var quranCount = await context.QuranVerses.CountAsync();
    var hadithCount = await context.HadithTexts.CountAsync();
    var fiqhCount = await context.FiqhRulings.CountAsync();
    var duaCount = await context.Duas.CountAsync();
    var sirahCount = await context.SirahEvents.CountAsync();
    var tajweedCount = await context.TajweedRules.CountAsync();
    
    Console.WriteLine($"Quran verses: {quranCount}");
    Console.WriteLine($"Hadith texts: {hadithCount}");
    Console.WriteLine($"Fiqh rulings: {fiqhCount}");
    Console.WriteLine($"Duas: {duaCount}");
    Console.WriteLine($"Sirah events: {sirahCount}");
    Console.WriteLine($"Tajweed rules: {tajweedCount}");
    
    var totalRecords = quranCount + hadithCount + fiqhCount + duaCount + sirahCount + tajweedCount;
    Console.WriteLine($"Total records: {totalRecords}");
    
    if (totalRecords == 0)
    {
        Console.WriteLine("\n=== Database is empty! Initializing... ===");
        var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        await initializer.InitializeAsync();
        Console.WriteLine("Database initialization completed.");
        
        // Recount after initialization
        quranCount = await context.QuranVerses.CountAsync();
        hadithCount = await context.HadithTexts.CountAsync();
        fiqhCount = await context.FiqhRulings.CountAsync();
        duaCount = await context.Duas.CountAsync();
        sirahCount = await context.SirahEvents.CountAsync();
        tajweedCount = await context.TajweedRules.CountAsync();
        
        Console.WriteLine("\n=== After Initialization ===");
        Console.WriteLine($"Quran verses: {quranCount}");
        Console.WriteLine($"Hadith texts: {hadithCount}");
        Console.WriteLine($"Fiqh rulings: {fiqhCount}");
        Console.WriteLine($"Duas: {duaCount}");
        Console.WriteLine($"Sirah events: {sirahCount}");
        Console.WriteLine($"Tajweed rules: {tajweedCount}");
    }
    
    // Test search functionality
    Console.WriteLine("\n=== Testing Search Functionality ===");
    var searchResults = await searchService.SearchAsync("Allah", 5);
    Console.WriteLine($"Search for 'Allah' returned {searchResults.Count()} results");
    
    foreach (var result in searchResults.Take(3))
    {
        Console.WriteLine($"- {result.Source}: {result.Title}");
        Console.WriteLine($"  {result.Content.Substring(0, Math.Min(100, result.Content.Length))}...");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

Console.WriteLine("\n=== Check completed ===");
