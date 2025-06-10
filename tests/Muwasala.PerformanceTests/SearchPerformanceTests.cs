using NBomber.CSharp;
using NBomber.Http.CSharp;
using System.Text.Json;

namespace Muwasala.PerformanceTests;

public class SearchPerformanceTests
{
    private const string BaseUrl = "http://localhost:5237";
    
    public static void RunSearchPerformanceTest()
    {
        var httpClient = new HttpClient();
        
        // Test various search queries
        var searchQueries = new[]
        {
            "Islam",
            "Quran",
            "Prophet Muhammad",
            "Prayer",
            "Hajj",
            "Ramadan",
            "Sunnah",
            "Hadith",
            "Islamic knowledge",
            "Arabic language"
        };

        var scenario = Scenario.Create("search_performance", async context =>
        {
            var query = searchQueries[Random.Shared.Next(searchQueries.Length)];
            
            // Test the AI-enhanced search endpoint
            var response = await httpClient.GetAsync($"{BaseUrl}/api/islamicagents/search?query={Uri.EscapeDataString(query)}&page=1&pageSize=10");
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(2))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("performance-reports")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Csv)
            .Run();
    }

    public static void RunConcurrentSearchTest()
    {
        var httpClient = new HttpClient();
        
        var scenario = Scenario.Create("concurrent_search", async context =>
        {
            var query = "Islamic knowledge";
            var response = await httpClient.GetAsync($"{BaseUrl}/api/islamicagents/search?query={Uri.EscapeDataString(query)}&page=1&pageSize=20");
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 50, during: TimeSpan.FromMinutes(1))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("performance-reports")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Csv)
            .Run();
    }

    public static void RunPaginationTest()
    {
        var httpClient = new HttpClient();
        
        var scenario = Scenario.Create("pagination_test", async context =>
        {
            var page = Random.Shared.Next(1, 10);
            var pageSize = Random.Shared.Next(5, 50);
            var query = "Quran";
            
            var response = await httpClient.GetAsync($"{BaseUrl}/api/islamicagents/search?query={Uri.EscapeDataString(query)}&page={page}&pageSize={pageSize}");
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 15, during: TimeSpan.FromMinutes(1))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("performance-reports")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Csv)
            .Run();
    }
}
