using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Muwasala.KnowledgeBase.Data;
using Muwasala.KnowledgeBase.Data.Repositories;
using Muwasala.KnowledgeBase.Services;

namespace Muwasala.KnowledgeBase.Extensions;

/// <summary>
/// Extension methods for configuring KnowledgeBase services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Islamic Knowledge Base services with database backend
    /// </summary>
    public static IServiceCollection AddIslamicKnowledgeBase(
        this IServiceCollection services, 
        IConfiguration configuration,
        bool useDatabaseServices = true)
    {
        // Add DbContext
        var connectionString = configuration.GetConnectionString("IslamicKnowledgeDb") 
            ?? "Data Source=IslamicKnowledge.db";
            
        services.AddDbContext<IslamicKnowledgeDbContext>(options =>
            options.UseSqlite(connectionString));

        // Add Database Initializer
        services.AddScoped<DatabaseInitializer>();

        if (useDatabaseServices)
        {
            // Register Database Services
            RegisterDatabaseServices(services);
        }
        else
        {
            // Register In-Memory Services (for testing or legacy support)
            RegisterInMemoryServices(services);
        }

        return services;
    }

    /// <summary>
    /// Add only database infrastructure without services (for custom service registration)
    /// </summary>
    public static IServiceCollection AddIslamicKnowledgeDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IslamicKnowledgeDb") 
            ?? "Data Source=IslamicKnowledge.db";
            
        services.AddDbContext<IslamicKnowledgeDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<DatabaseInitializer>();

        // Register only repositories
        RegisterRepositories(services);

        return services;
    }

    /// <summary>
    /// Register database-backed services
    /// </summary>
    private static void RegisterDatabaseServices(IServiceCollection services)
    {
        // Register Repositories
        RegisterRepositories(services);        // Register Database-backed Services
        services.AddScoped<IQuranService, DatabaseQuranService>();
        services.AddScoped<IHadithService, DatabaseHadithService>();
        services.AddScoped<IFiqhService, DatabaseFiqhService>();
        services.AddScoped<IDuaService, DatabaseDuaService>();
        services.AddScoped<ITajweedService, DatabaseTajweedService>();
        services.AddScoped<ISirahService, DatabaseSirahService>();
        services.AddScoped<IGlobalSearchService, DatabaseGlobalSearchService>();
        services.AddScoped<ISearchHistoryService, SearchHistoryService>();
    }

    /// <summary>
    /// Register in-memory services (original implementation)
    /// </summary>
    private static void RegisterInMemoryServices(IServiceCollection services)
    {
        // Register original in-memory services
        services.AddScoped<IQuranService, QuranService>();
        services.AddScoped<IHadithService, HadithService>();
        services.AddScoped<IFiqhService, FiqhService>();
        services.AddScoped<IDuaService, DuaService>();
        services.AddScoped<ITajweedService, TajweedService>();
        services.AddScoped<ISirahService, SirahService>();
        services.AddScoped<IGlobalSearchService, GlobalSearchService>();
    }

    /// <summary>
    /// Register repository interfaces and implementations
    /// </summary>
    private static void RegisterRepositories(IServiceCollection services)
    {
        // Generic Repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Specific Repositories
        services.AddScoped<IQuranRepository, QuranRepository>();
        services.AddScoped<ITafsirRepository, TafsirRepository>();
        services.AddScoped<IHadithRepository, HadithRepository>();
        services.AddScoped<IFiqhRepository, FiqhRepository>();
        services.AddScoped<IDuaRepository, DuaRepository>();
        services.AddScoped<ISirahRepository, SirahRepository>();
        services.AddScoped<ITajweedRepository, TajweedRepository>();
        services.AddScoped<ICommonMistakeRepository, CommonMistakeRepository>();
        services.AddScoped<IVerseTajweedRepository, VerseTajweedRepository>();
    }

    /// <summary>
    /// Configure Islamic Knowledge Base with custom database options
    /// </summary>
    public static IServiceCollection AddIslamicKnowledgeBase(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext,
        bool useDatabaseServices = true)
    {
        // Add DbContext with custom configuration
        services.AddDbContext<IslamicKnowledgeDbContext>(configureDbContext);

        // Add Database Initializer
        services.AddScoped<DatabaseInitializer>();

        if (useDatabaseServices)
        {
            RegisterDatabaseServices(services);
        }
        else
        {
            RegisterInMemoryServices(services);
        }

        return services;
    }
}

/// <summary>
/// Extension methods for database initialization
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Initialize the Islamic Knowledge database
    /// </summary>
    public static async Task InitializeIslamicKnowledgeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync();
    }

    /// <summary>
    /// Migrate the Islamic Knowledge database
    /// </summary>
    public static async Task MigrateIslamicKnowledgeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.MigrateAsync();
    }
}
