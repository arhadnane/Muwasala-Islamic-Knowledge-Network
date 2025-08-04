using Muwasala.Core.Services;
using Muwasala.Core.Models;
using Muwasala.Agents;
using Muwasala.KnowledgeBase.Services;
using Muwasala.KnowledgeBase.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Muwasala Islamic Knowledge Network API", 
        Version = "v1",
        Description = "Multi-agent system for Islamic learning, spiritual guidance, and religious practice"
    });
});

// Configure HTTP client for Ollama with extended timeout
builder.Services.AddHttpClient<IOllamaService, OllamaService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5); // 5 minutes timeout for AI operations
});

// Register core services
builder.Services.AddScoped<IOllamaService, OllamaService>();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Add Islamic Knowledge Base services with database backend
builder.Services.AddIslamicKnowledgeBase(builder.Configuration, useDatabaseServices: true);

// Add Elasticsearch services for enhanced search capabilities
builder.Services.AddElasticsearch(builder.Configuration);

// Register search services (now provided by AddIslamicKnowledgeBase)
builder.Services.AddScoped<IIntelligentSearchService, IntelligentSearchService>();

// Register advanced hadith search service for enhanced search capabilities
builder.Services.AddScoped<IAdvancedHadithSearchService, AdvancedHadithSearchService>();

// Register multi-agent system with enhanced search capabilities
Muwasala.Agents.MultiAgentServiceExtensions.AddMultiAgentSystem(builder.Services);

// Register Islamic agent services
builder.Services.AddScoped<QuranNavigatorAgent>();
builder.Services.AddScoped<HadithVerifierAgent>();
builder.Services.AddScoped<FiqhAdvisorAgent>();
builder.Services.AddScoped<DuaCompanionAgent>();
builder.Services.AddScoped<TajweedTutorAgent>();
builder.Services.AddScoped<SirahScholarAgent>();

// Register enhanced Fiqh services
builder.Services.AddScoped<IEnhancedFiqhService, HybridFiqhService>();
builder.Services.AddScoped<EnhancedFiqhAdvisorAgent>();

// Configure CORS for frontend integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize database
await app.Services.InitializeIslamicKnowledgeDatabaseAsync();

// Initialize Elasticsearch (optional - will create index if needed)
try
{
    await app.Services.InitializeElasticsearchAsync();
}
catch (Exception ex)
{
    // Elasticsearch is optional, continue without it
    app.Logger.LogWarning(ex, "Elasticsearch initialization failed - continuing without Elasticsearch");
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Muwasala API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    service = "Muwasala Islamic Knowledge Network"
});

// Welcome endpoint
app.MapGet("/", () => new {
    service = "🕌 Muwasala: Islamic Knowledge Network",
    description = "Multi-agent system for Islamic learning and spiritual guidance",
    version = "1.0.0",
    agents = new[] {
        "QuranNavigator - Contextual verse retrieval",
        "HadithVerifier - Authentication chain checking",
        "FiqhAdvisor - Jurisprudential guidance",
        "DuaCompanion - Prayer recommendations",
        "TajweedTutor - Quranic recitation guidance"
    },
    endpoints = new {
        swagger = "/swagger",
        health = "/health",
        quran = "/api/quran",
        hadith = "/api/hadith",
        fiqh = "/api/fiqh",
        dua = "/api/dua"
    }
});

// Note: Elasticsearch endpoints are available via the ElasticsearchController
// Use /api/elasticsearch/search?query=your_search for Elasticsearch search
// Use /api/elasticsearch/health for Elasticsearch health status

app.Run();
