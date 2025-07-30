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

// Add Muwasala services
builder.Services.AddHttpClient();
builder.Services.ConfigureAll<HttpClientFactoryOptions>(options =>
{
    options.HttpClientActions.Add(client => client.Timeout = TimeSpan.FromMinutes(5));
});
builder.Services.AddSingleton<IOllamaService, OllamaService>();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Add Islamic Knowledge Base services with database backend (with file fallback)
builder.Services.AddIslamicKnowledgeBase(builder.Configuration, useDatabaseServices: true);

// Add Intelligent Search Service (AI + Web) - Changed to Scoped to match database services
builder.Services.AddScoped<IIntelligentSearchService, IntelligentSearchService>();

// Add Multi-Agent System with DeepSeek Brain
builder.Services.AddMultiAgentSystem();

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
