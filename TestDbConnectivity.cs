using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Muwasala.KnowledgeBase.Data;
using Muwasala.KnowledgeBase.Extensions;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing database connectivity...");
        
        var services = new ServiceCollection();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:IslamicKnowledgeDb"] = "Data Source=data/database/IslamicKnowledge.db"
            })
            .Build();
            
        services.AddSingleton<IConfiguration>(configuration);
        services.AddIslamicKnowledgeDatabase(configuration);
        
        var serviceProvider = services.BuildServiceProvider();
        
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IslamicKnowledgeDbContext>();
        
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync();
            Console.WriteLine($"Database connection: {(canConnect ? "SUCCESS" : "FAILED")}");
            
            if (canConnect)
            {
                var quranCount = await dbContext.QuranVerses.CountAsync();
                var hadithCount = await dbContext.Hadiths.CountAsync();
                var fiqhCount = await dbContext.FiqhRulings.CountAsync();
                
                Console.WriteLine($"Quran verses: {quranCount}");
                Console.WriteLine($"Hadiths: {hadithCount}");
                Console.WriteLine($"Fiqh rulings: {fiqhCount}");
                
                if (quranCount == 0 && hadithCount == 0 && fiqhCount == 0)
                {
                    Console.WriteLine("Database is empty - this explains why search returns no results!");
                }
                else
                {
                    Console.WriteLine("Database has content - search should work.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
