using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Muwasala.KnowledgeBase.Extensions;
using Muwasala.KnowledgeBase.Services;

var services = new ServiceCollection();

var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string>
    {
        ["ConnectionStrings:IslamicKnowledgeDb"] = "Data Source=d:\\Coding\\VSCodeProject\\Muwasala Islamic Knowledge Network\\data\\database\\IslamicKnowledge.db"
    })
    .Build();

services.AddSingleton<IConfiguration>(configuration);
services.AddIslamicKnowledgeBase(configuration, useDatabaseServices: true);

var serviceProvider = services.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();
var searchService = scope.ServiceProvider.GetRequiredService<IGlobalSearchService>();

Console.WriteLine("Testing search functionality...");

var result = await searchService.SearchAllAsync("prayer", "en", 5);
Console.WriteLine($"Search results for 'prayer': {result.Results.Count} found");

foreach (var item in result.Results)
{
    Console.WriteLine($"- [{item.Type}] {item.Title}");
}

Console.WriteLine("Done!");
