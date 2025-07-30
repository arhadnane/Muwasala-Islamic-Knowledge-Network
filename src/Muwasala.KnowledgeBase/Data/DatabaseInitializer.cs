using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muwasala.KnowledgeBase.Data.Models; // Ensure this using statement is present
using System.IO; // For File and Path operations
using System.Collections.Generic; // For Dictionary
using System.Linq; // For LINQ operations like Any

namespace Muwasala.KnowledgeBase.Data;

/// <summary>
/// Service for initializing and migrating the Islamic Knowledge database
/// </summary>
public class DatabaseInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Initialize the database with schema and seed data
    /// </summary>
    public async Task InitializeAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IslamicKnowledgeDbContext>();

        try
        {
            _logger.LogInformation("Starting database initialization...");

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            _logger.LogInformation("Database schema created/verified.");

            // Always try to load hadith data from files first (the loader has duplicate protection)
            _logger.LogInformation("Attempting to load hadith data from files...");
            await context.LoadHadithFromFilesAsync(_logger);

            // Check if Quran data exists after potential file loading
            var hasQuranData = await context.QuranVerses.AnyAsync();
            if (!hasQuranData)
            {
                _logger.LogInformation("Seeding database with Quran verses...");
                await SeedQuranVersesAsync(context);
                
                // Call SeedDataAsync for other basic data (FiqhRulings, DuaRecords, etc.)
                // but note that SeedHadithDataAsync is now disabled in the context
                await context.SeedDataAsync();
            }
            else
            {
                _logger.LogInformation("Quran data already exists. Skipping Quran seeding.");
            }

            _logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    /// <summary>
    /// Migrate database to latest schema version
    /// </summary>
    public async Task MigrateAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IslamicKnowledgeDbContext>();

        try
        {
            _logger.LogInformation("Starting database migration...");
            await context.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database migration.");
            throw;
        }
    }

    /// <summary>
    /// Reset database - drop and recreate with fresh data
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IslamicKnowledgeDbContext>();

        try
        {
            _logger.LogWarning("Resetting database - all data will be lost!");
            
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            
            _logger.LogInformation("Database reset. Seeding with fresh data...");
            await SeedDataAsync(context);
            
            _logger.LogInformation("Database reset and seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database reset.");
            throw;
        }
    }

    /// <summary>
    /// Backup database to specified file
    /// </summary>
    public async Task BackupDatabaseAsync(string backupPath)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IslamicKnowledgeDbContext>();

        try
        {
            _logger.LogInformation($"Creating database backup at: {backupPath}");
            
            // For SQLite, we can simply copy the database file
            var connectionString = context.Database.GetConnectionString();
            if (connectionString?.Contains("Data Source=") == true)
            {
                var dataSource = connectionString.Split("Data Source=")[1].Split(";")[0];
                if (File.Exists(dataSource))
                {
                    // File.Copy is synchronous. To make this method truly async if needed,
                    // one might consider reading and writing streams asynchronously,
                    // but for a simple file copy, this is often acceptable.
                    // For now, to satisfy the async warning if no other await is present:
                    File.Copy(dataSource, backupPath, overwrite: true);
                    _logger.LogInformation("Database backup completed successfully.");
                    await Task.CompletedTask; // Added to satisfy async warning if no other await is used
                }
                else
                {
                    throw new FileNotFoundException($"Database file not found: {dataSource}");
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type for backup operation.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database backup.");
            throw;
        }
    }

    /// <summary>
    /// Restore database from backup file
    /// </summary>
    public async Task RestoreDatabaseAsync(string backupPath)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IslamicKnowledgeDbContext>();

        try
        {
            _logger.LogInformation($"Restoring database from backup: {backupPath}");
            
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException($"Backup file not found: {backupPath}");
            }

            var connectionString = context.Database.GetConnectionString();
            if (connectionString?.Contains("Data Source=") == true)
            {
                var dataSource = connectionString.Split("Data Source=")[1].Split(";")[0];
                
                // Close any existing connections
                await context.Database.CloseConnectionAsync();
                
                // Copy backup over current database
                File.Copy(backupPath, dataSource, overwrite: true);
                
                _logger.LogInformation("Database restore completed successfully.");
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type for restore operation.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database restore.");
            throw;
        }
    }

    /// <summary>
    /// Get database statistics
    /// </summary>
    public async Task<DatabaseStatistics> GetStatisticsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IslamicKnowledgeDbContext>();

        return new DatabaseStatistics
        {
            QuranVersesCount = await context.QuranVerses.CountAsync(),
            TafsirEntriesCount = await context.Tafsirs.CountAsync(),
            HadithRecordsCount = await context.HadithRecords.CountAsync(),
            FiqhRulingsCount = await context.FiqhRulings.CountAsync(),
            DuaRecordsCount = await context.DuaRecords.CountAsync(),
            SirahEventsCount = await context.SirahEvents.CountAsync(),
            TajweedRulesCount = await context.TajweedRules.CountAsync(),
            CommonMistakesCount = await context.CommonMistakes.CountAsync(),
            VerseTajweedCount = await context.VerseTajweedData.CountAsync(),
            DatabaseSizeMB = GetDatabaseSizeInMB(context)
        };
    }    private async Task SeedDataAsync(IslamicKnowledgeDbContext context)
    {
        // Use the existing public seed method from the DbContext
        // This might already seed some initial data or can be kept for other types of seeding.
        await context.SeedDataAsync();

        // Add our new Quran verse seeding
        await SeedQuranVersesAsync(context);

        // Add hadith loading from files
        await context.LoadHadithFromFilesAsync(_logger);
    }

    private async Task SeedQuranVersesAsync(IslamicKnowledgeDbContext context)
    {
        _logger.LogInformation("Starting to seed Quran verses...");

        // Path resolution: Assumes the executable runs from a path like:
        // d:\Coding\VSCodeProject\Muwasala Islamic Knowledge Network\src\PROJECT_NAME\bin\Debug\net8.0
        // We need to go up 5 levels to reach the solution root.
        string solutionRootPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", ".."));
        var baseDataPath = Path.Combine(solutionRootPath, "data", "Quran", "SourceFiles");

        var arabicTextFile = Path.Combine(baseDataPath, "quran-uthmani.txt");
        var englishTranslationFile = Path.Combine(baseDataPath, "en.sahih.txt");

        if (!File.Exists(arabicTextFile))
        {
            _logger.LogError($"Arabic Quran text file not found: {arabicTextFile}. Please ensure it exists at the correct path.");
            return;
        }
        if (!File.Exists(englishTranslationFile))
        {
            _logger.LogError($"English Quran translation file not found: {englishTranslationFile}. Please ensure it exists at the correct path.");
            return;
        }

        try
        {
            var arabicVerses = await File.ReadAllLinesAsync(arabicTextFile);
            var englishTranslations = await File.ReadAllLinesAsync(englishTranslationFile);

            if (arabicVerses.Length == 0)
            {
                _logger.LogWarning($"Arabic Quran text file is empty: {arabicTextFile}");
                return;
            }
            if (englishTranslations.Length == 0)
            {
                _logger.LogWarning($"English Quran translation file is empty: {englishTranslationFile}");
                return;
            }
            
            // Basic check for mismatched file lengths
            if (arabicVerses.Length != englishTranslations.Length)
            {
                _logger.LogWarning($"Warning: Arabic text file ({arabicVerses.Length} lines) and English translation file ({englishTranslations.Length} lines) have different number of lines. Verses might not align correctly for all entries. Processing up to the minimum length: {Math.Min(arabicVerses.Length, englishTranslations.Length)} lines.");
            }

            var quranVersesToAdd = new List<QuranVerseEntity>();
            // Load existing verses into a dictionary for quick lookup to prevent duplicates.
            // This assumes SurahNumber and VerseNumber form a unique key.
            var existingVerses = new HashSet<string>();
            if (await context.QuranVerses.AnyAsync())
            {
                 existingVerses = new HashSet<string>(
                    await context.QuranVerses.Select(v => $"{v.SurahNumber}:{v.VerseNumber}").ToListAsync()
                );
                _logger.LogInformation($"Found {existingVerses.Count} existing verses in the database.");
            }
            else
            {
                _logger.LogInformation("No existing Quran verses found in the database. Seeding all new verses.");
            }


            _logger.LogInformation($"Processing up to {Math.Min(arabicVerses.Length, englishTranslations.Length)} lines from source files.");

            for (int i = 0; i < Math.Min(arabicVerses.Length, englishTranslations.Length); i++)
            {
                var arabicLine = arabicVerses[i];
                var englishLine = englishTranslations[i];

                // Skip comments or empty lines, common in Tanzil files
                if (string.IsNullOrWhiteSpace(arabicLine) || arabicLine.StartsWith("#")) continue;
                if (string.IsNullOrWhiteSpace(englishLine) || englishLine.StartsWith("#")) continue;

                var arabicParts = arabicLine.Split('|');
                var englishParts = englishLine.Split('|');

                if (arabicParts.Length == 3 && englishParts.Length == 3)
                {
                    if (int.TryParse(arabicParts[0], out int surahNumberArabic) &&
                        int.TryParse(arabicParts[1], out int verseNumberArabic) &&
                        int.TryParse(englishParts[0], out int surahNumberEnglish) &&
                        int.TryParse(englishParts[1], out int verseNumberEnglish))
                    {
                        if (surahNumberArabic != surahNumberEnglish || verseNumberArabic != verseNumberEnglish)
                        {
                            _logger.LogWarning($"Mismatch in Surah/Verse numbers at line {i + 1}. Arabic: S{surahNumberArabic}V{verseNumberArabic}, English: S{surahNumberEnglish}V{verseNumberEnglish}. Skipping this verse.");
                            continue;
                        }

                        var verseKey = $"{surahNumberArabic}:{verseNumberArabic}";
                        if (!existingVerses.Contains(verseKey))
                        {
                            quranVersesToAdd.Add(new QuranVerseEntity
                            {
                                SurahNumber = surahNumberArabic,
                                VerseNumber = verseNumberArabic,
                                ArabicText = arabicParts[2].Trim(),
                                Translation = englishParts[2].Trim(),
                                Language = "en", // For the English translation
                                // Transliteration, Keywords, Context, Theme will be null or default
                            });
                            existingVerses.Add(verseKey); // Add to HashSet to prevent adding duplicates from the file itself if any
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Could not parse Surah/Verse numbers at line {i + 1}. Arabic: '{arabicLine.Substring(0, Math.Min(arabicLine.Length, 50))}', English: '{englishLine.Substring(0, Math.Min(englishLine.Length, 50))}'. Skipping.");
                    }
                }
                else
                {
                    _logger.LogWarning($"Incorrect format at line {i + 1}. Arabic: '{arabicLine.Substring(0, Math.Min(arabicLine.Length, 50))}', English: '{englishLine.Substring(0, Math.Min(englishLine.Length, 50))}'. Expected 3 parts separated by '|'. Skipping.");
                }
            }

            if (quranVersesToAdd.Any())
            {
                _logger.LogInformation($"Adding {quranVersesToAdd.Count} new Quran verses to the database.");
                await context.QuranVerses.AddRangeAsync(quranVersesToAdd);
                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully added new Quran verses.");
            }
            else
            {
                _logger.LogInformation("No new Quran verses to add. Database might be up-to-date with the provided files, files were empty/corrupt, or all verses in files already exist.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding Quran verses.");
            // Optionally rethrow or handle more gracefully depending on application requirements
        }
    }

    private double GetDatabaseSizeInMB(IslamicKnowledgeDbContext context)
    {
        var connectionString = context.Database.GetConnectionString();
        if (connectionString?.Contains("Data Source=") == true)
        {
            var dataSource = connectionString.Split("Data Source=")[1].Split(";")[0];
            if (File.Exists(dataSource))
            {
                var fileInfo = new FileInfo(dataSource);
                return Math.Round(fileInfo.Length / (1024.0 * 1024.0), 2);
            }
        }
        return 0;
    }
}

/// <summary>
/// Database statistics model
/// </summary>
public class DatabaseStatistics
{
    public int QuranVersesCount { get; set; }
    public int TafsirEntriesCount { get; set; }
    public int HadithRecordsCount { get; set; }
    public int FiqhRulingsCount { get; set; }
    public int DuaRecordsCount { get; set; }
    public int SirahEventsCount { get; set; }
    public int TajweedRulesCount { get; set; }
    public int CommonMistakesCount { get; set; }
    public int VerseTajweedCount { get; set; }
    public double DatabaseSizeMB { get; set; }
    public int TotalRecords => QuranVersesCount + TafsirEntriesCount + HadithRecordsCount + 
                              FiqhRulingsCount + DuaRecordsCount + SirahEventsCount + 
                              TajweedRulesCount + CommonMistakesCount + VerseTajweedCount;
}
