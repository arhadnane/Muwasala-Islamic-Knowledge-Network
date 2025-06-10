using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Muwasala.KnowledgeBase.Data;
using Muwasala.KnowledgeBase.Extensions;

Console.WriteLine("Starting database reset...");

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string>
    {
        ["ConnectionStrings:IslamicKnowledgeDb"] = "Data Source=d:\\Coding\\VSCodeProject\\Muwasala Islamic Knowledge Network\\data\\database\\IslamicKnowledge.db"
    })
    .Build();

services.AddSingleton<IConfiguration>(configuration);
services.AddIslamicKnowledgeBase(configuration, useDatabaseServices: true);

var serviceProvider = services.BuildServiceProvider();

try
{
    using var scope = serviceProvider.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    
    Console.WriteLine("Resetting database...");
    await initializer.ResetDatabaseAsync();
    
    var stats = await initializer.GetStatisticsAsync();
    Console.WriteLine($"Database reset complete! Total records: {stats.TotalRecords}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("Done!");
