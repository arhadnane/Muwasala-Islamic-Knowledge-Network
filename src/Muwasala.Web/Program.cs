using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Http;
using Muwasala.Core.Services;
using Muwasala.Agents;
using Muwasala.KnowledgeBase.Services;
using Muwasala.KnowledgeBase.Extensions;
using Muwasala.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Add Muwasala services
builder.Services.AddHttpClient();
builder.Services.ConfigureAll<HttpClientFactoryOptions>(options =>
{
    options.HttpClientActions.Add(client => client.Timeout = TimeSpan.FromMinutes(5));
});

// Configure named HttpClient for Blazor pages with BaseAddress
builder.Services.AddHttpClient("BlazorClient", (serviceProvider, client) =>
{
    // Set base address for API calls from Blazor components
    var httpContext = serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
    if (httpContext != null)
    {
        var request = httpContext.Request;
        client.BaseAddress = new Uri($"{request.Scheme}://{request.Host}");
    }
    else
    {
        // Fallback for development
        client.BaseAddress = new Uri("https://localhost:5237");
    }
});

// Register scoped HttpClient for Blazor components
builder.Services.AddScoped(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return httpClientFactory.CreateClient("BlazorClient");
});
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Configure OllamaService with HttpClient - Extended timeout for DeepSeek-R1
builder.Services.AddHttpClient<IOllamaService, OllamaService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(15); // Extended timeout for heavy AI models
});

// Add Islamic Knowledge Base services with database backend (with file fallback)
builder.Services.AddIslamicKnowledgeBase(builder.Configuration, useDatabaseServices: true);

// Add Elasticsearch services for enhanced hadith search
builder.Services.AddElasticsearch(builder.Configuration);

// Add Intelligent Search Service (AI + Web) - Changed to Scoped to match database services
builder.Services.AddScoped<IIntelligentSearchService, IntelligentSearchService>();

// Add Advanced Hadith Search Service for "n'importe quels mots" search capability
builder.Services.AddScoped<IAdvancedHadithSearchService, AdvancedHadithSearchService>();

// Add Multi-Agent System with DeepSeek Brain
builder.Services.AddMultiAgentSystem();

// Register Islamic agent services explicitly for API endpoints
builder.Services.AddScoped<QuranNavigatorAgent>();
builder.Services.AddScoped<HadithVerifierAgent>();
builder.Services.AddScoped<FiqhAdvisorAgent>();
builder.Services.AddScoped<DuaCompanionAgent>();
builder.Services.AddScoped<TajweedTutorAgent>();
builder.Services.AddScoped<SirahScholarAgent>();

// Register enhanced Fiqh services
builder.Services.AddScoped<IEnhancedFiqhService, HybridFiqhService>();
builder.Services.AddScoped<EnhancedFiqhAdvisorAgent>();

var app = builder.Build();

// Initialize database
await app.Services.InitializeIslamicKnowledgeDatabaseAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
