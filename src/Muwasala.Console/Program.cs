using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using Muwasala.Core.Services;
using Muwasala.Agents;
using Muwasala.KnowledgeBase.Services;
using Muwasala.KnowledgeBase.Extensions;
using Muwasala.Core.Models;

namespace Muwasala.Console;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Create the root command
        var rootCommand = new RootCommand("🕌 Muwasala: Islamic Knowledge Network")
        {
            Description = "A multi-agent system for Islamic learning, spiritual guidance, and religious practice using local Ollama models."
        };        // Create sub-commands for each agent        rootCommand.AddCommand(CreateQuranCommand());
        rootCommand.AddCommand(CreateHadithCommand());
        rootCommand.AddCommand(CreateFiqhCommand());
        rootCommand.AddCommand(CreateDuaCommand());
        rootCommand.AddCommand(CreateTajweedCommand());
        rootCommand.AddCommand(CreateSirahCommand());        rootCommand.AddCommand(CreateGlobalSearchCommand());
        rootCommand.AddCommand(CreateHybridSearchCommand());
        rootCommand.AddCommand(CreateInteractiveCommand());
        rootCommand.AddCommand(CreateSetupCommand());
        rootCommand.AddCommand(CreateTestCommand());

        return await rootCommand.InvokeAsync(args);
    }

    static Command CreateQuranCommand()
    {
        var contextOption = new Option<string>("--context", "The life situation or context for guidance") { IsRequired = true };
        var languageOption = new Option<string>("--language", () => "en", "Language for translation (en, ar, ur, etc.)");
        var tafsirOption = new Option<string>("--tafsir", () => "IbnKathir", "Tafsir source (IbnKathir, Jalalayn, etc.)");

        var command = new Command("quran", "Get contextual Quranic guidance")
        {
            contextOption,
            languageOption,
            tafsirOption
        };

        command.SetHandler(async (context, language, tafsir) =>
        {
            var serviceProvider = CreateServiceProvider();
            var agent = serviceProvider.GetRequiredService<QuranNavigatorAgent>();
            
            try
            {
                System.Console.WriteLine("🔍 Searching for relevant Quranic guidance...\n");
                
                var response = await agent.GetVerseAsync(context, language, tafsir);
                
                DisplayVerseResponse(response);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }, contextOption, languageOption, tafsirOption);

        return command;
    }

    static Command CreateHadithCommand()
    {
        var textOption = new Option<string>("--text", "Hadith text to verify") { IsRequired = true };
        var languageOption = new Option<string>("--language", () => "en", "Language for translation");

        var command = new Command("hadith", "Verify hadith authenticity")
        {
            textOption,
            languageOption
        };

        command.SetHandler(async (text, language) =>
        {
            var serviceProvider = CreateServiceProvider();
            var agent = serviceProvider.GetRequiredService<HadithVerifierAgent>();
            
            try
            {
                System.Console.WriteLine("🔍 Verifying hadith authenticity...\n");
                
                var response = await agent.VerifyHadithAsync(text, language);
                
                DisplayHadithResponse(response);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }, textOption, languageOption);

        return command;
    }

    static Command CreateFiqhCommand()
    {
        var questionOption = new Option<string>("--question", "Islamic legal question") { IsRequired = true };
        var madhabOption = new Option<Madhab>("--madhab", () => Madhab.Hanafi, "School of jurisprudence");
        var languageOption = new Option<string>("--language", () => "en", "Language for response");

        var command = new Command("fiqh", "Get Islamic legal ruling")
        {
            questionOption,
            madhabOption,
            languageOption
        };

        command.SetHandler(async (question, madhab, language) =>
        {
            var serviceProvider = CreateServiceProvider();
            var agent = serviceProvider.GetRequiredService<FiqhAdvisorAgent>();
            
            try
            {
                System.Console.WriteLine($"⚖️ Getting {madhab} ruling...\n");
                
                var response = await agent.GetRulingAsync(question, madhab, language);
                
                DisplayFiqhResponse(response);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }, questionOption, madhabOption, languageOption);

        return command;
    }

    static Command CreateDuaCommand()
    {
        var occasionOption = new Option<string>("--occasion", "Occasion or situation for du'a") { IsRequired = true };
        var languageOption = new Option<string>("--language", () => "en", "Language for translation");

        var command = new Command("dua", "Get appropriate du'as for an occasion")
        {
            occasionOption,
            languageOption
        };

        command.SetHandler(async (occasion, language) =>
        {
            var serviceProvider = CreateServiceProvider();
            var agent = serviceProvider.GetRequiredService<DuaCompanionAgent>();
            
            try
            {
                System.Console.WriteLine($"🤲 Finding du'as for: {occasion}...\n");
                
                var responses = await agent.GetDuasForOccasionAsync(occasion, language);
                
                foreach (var response in responses)
                {
                    DisplayDuaResponse(response);
                    System.Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }, occasionOption, languageOption);

        return command;
    }

    static Command CreateTajweedCommand()
    {
        var surahOption = new Option<int>("--surah", "Surah number") { IsRequired = true };
        var verseOption = new Option<int>("--verse", "Verse number") { IsRequired = true };
        var languageOption = new Option<string>("--language", () => "en", "Language for explanation");

        var command = new Command("tajweed", "Get tajweed rules for a verse")
        {
            surahOption,
            verseOption,
            languageOption
        };

        command.SetHandler(async (surah, verse, language) =>
        {
            var serviceProvider = CreateServiceProvider();
            var agent = serviceProvider.GetRequiredService<TajweedTutorAgent>();
            
            try
            {
                System.Console.WriteLine($"📖 Analyzing tajweed for {surah}:{verse}...\n");
                
                var response = await agent.AnalyzeVerseAsync(new VerseReference(surah, verse), language);
                
                DisplayTajweedResponse(response);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Error: {ex.Message}");
            }        }, surahOption, verseOption, languageOption);

        return command;
    }

    static Command CreateSirahCommand()
    {
        var topicOption = new Option<string>("--topic", "Topic for Prophetic guidance") { IsRequired = true };
        var periodOption = new Option<SirahPeriod?>("--period", "Specific period in Prophet's life");
        var languageOption = new Option<string>("--language", () => "en", "Language for response");

        var command = new Command("sirah", "Get Prophetic guidance and historical context")
        {
            topicOption,
            periodOption,
            languageOption
        };

        command.SetHandler(async (topic, period, language) =>
        {
            var serviceProvider = CreateServiceProvider();
            var agent = serviceProvider.GetRequiredService<SirahScholarAgent>();
            
            try
            {
                System.Console.WriteLine($"📜 Searching for Prophetic guidance on: {topic}...\n");
                
                var response = await agent.GetGuidanceFromSirahAsync(topic, language);
                
                DisplaySirahResponse(response);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }, topicOption, periodOption, languageOption);

        return command;
    }

    static Command CreateInteractiveCommand()
    {
        var command = new Command("interactive", "Start interactive session with all agents");

        command.SetHandler(async () =>
        {
            var serviceProvider = CreateServiceProvider();
            var session = new InteractiveSession(serviceProvider);
            await session.RunAsync();
        });

        return command;
    }

    static Command CreateSetupCommand()
    {
        var command = new Command("setup", "Setup and verify Ollama models");

        command.SetHandler(async () =>
        {
            var serviceProvider = CreateServiceProvider();
            var setup = new SystemSetup(serviceProvider);
            await setup.VerifySystemAsync();
        });

        return command;
    }

    static Command CreateTestCommand()
    {
        var command = new Command("test", "Test that different Surahs return different content (verify mock data fix)");

        command.SetHandler(async () =>
        {
            var serviceProvider = CreateServiceProvider();
            var agent = serviceProvider.GetRequiredService<QuranNavigatorAgent>();
            
            System.Console.WriteLine("🧪 Testing Surah Analysis - Verifying Different Surahs Return Different Content");
            System.Console.WriteLine("════════════════════════════════════════════════════════════════════════════════");

            // Test Surah 1 (Al-Fatiha)
            System.Console.WriteLine("\n📖 Testing Surah 1 (Al-Fatiha):");
            try 
            {
                var surah1 = await agent.GetSurahAnalysisAsync(1);
                System.Console.WriteLine($"   Verses found: {surah1.Count}");
                if (surah1.Any())
                {
                    System.Console.WriteLine($"   First verse: {surah1.First().Verse}");
                    System.Console.WriteLine($"   Arabic: {surah1.First().ArabicText}");
                    System.Console.WriteLine($"   Translation: {surah1.First().Translation[..Math.Min(50, surah1.First().Translation.Length)]}...");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"   ❌ Error: {ex.Message}");
            }

            // Test Surah 2 (Al-Baqarah)
            System.Console.WriteLine("\n📖 Testing Surah 2 (Al-Baqarah):");
            try 
            {
                var surah2 = await agent.GetSurahAnalysisAsync(2);
                System.Console.WriteLine($"   Verses found: {surah2.Count}");
                if (surah2.Any())
                {
                    System.Console.WriteLine($"   First verse: {surah2.First().Verse}");
                    System.Console.WriteLine($"   Arabic: {surah2.First().ArabicText}");
                    System.Console.WriteLine($"   Translation: {surah2.First().Translation[..Math.Min(50, surah2.First().Translation.Length)]}...");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"   ❌ Error: {ex.Message}");
            }

            // Test Surah 94 (Ash-Sharh)
            System.Console.WriteLine("\n📖 Testing Surah 94 (Ash-Sharh):");
            try 
            {
                var surah94 = await agent.GetSurahAnalysisAsync(94);
                System.Console.WriteLine($"   Verses found: {surah94.Count}");
                if (surah94.Any())
                {
                    System.Console.WriteLine($"   First verse: {surah94.First().Verse}");
                    System.Console.WriteLine($"   Arabic: {surah94.First().ArabicText}");
                    System.Console.WriteLine($"   Translation: {surah94.First().Translation[..Math.Min(50, surah94.First().Translation.Length)]}...");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"   ❌ Error: {ex.Message}");
            }

            // Test Surah 112 (Al-Ikhlas) - This was mentioned in the original issue
            System.Console.WriteLine("\n📖 Testing Surah 112 (Al-Ikhlas):");
            try 
            {
                var surah112 = await agent.GetSurahAnalysisAsync(112);
                System.Console.WriteLine($"   Verses found: {surah112.Count}");
                if (surah112.Any())
                {
                    System.Console.WriteLine($"   First verse: {surah112.First().Verse}");
                    System.Console.WriteLine($"   Arabic: {surah112.First().ArabicText}");
                    System.Console.WriteLine($"   Translation: {surah112.First().Translation[..Math.Min(50, surah112.First().Translation.Length)]}...");
                }
                else
                {
                    System.Console.WriteLine("   ℹ️ No verses found - this Surah is not in the sample data");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"   ❌ Error: {ex.Message}");
            }

            System.Console.WriteLine("\n✅ Test completed! If different Surahs show different content, the fix is working.");
            System.Console.WriteLine("✅ If all Surahs showed identical mock data, that issue has been resolved.");
        });

        return command;
    }

    static Command CreateGlobalSearchCommand()
    {
        var queryOption = new Option<string>("--query", "Search query to find across all Islamic sources") { IsRequired = true };
        var languageOption = new Option<string>("--language", () => "en", "Language for results (en, ar, ur, etc.)");
        var maxResultsOption = new Option<int>("--max-results", () => 20, "Maximum number of results to return");
        var typesOption = new Option<string[]>("--types", () => Array.Empty<string>(), "Specific content types to search (quran, hadith, fiqh, dua, sirah, tajweed)");

        var command = new Command("search", "Search across all Islamic knowledge sources")
        {
            queryOption,
            languageOption,
            maxResultsOption,
            typesOption
        };        command.SetHandler(async (query, language, maxResults, types) =>
        {
            var serviceProvider = CreateServiceProvider();
            var globalSearch = serviceProvider.GetRequiredService<IGlobalSearchService>();

            System.Console.WriteLine($"🔍 Searching for: \"{query}\"");
            System.Console.WriteLine($"Language: {language}, Max Results: {maxResults}");
            
            if (types.Length > 0)
            {
                System.Console.WriteLine($"Types: {string.Join(", ", types)}");
            }
            
            System.Console.WriteLine(new string('=', 60));

            try
            {
                GlobalSearchResponse results;
                
                if (types.Length > 0)
                {
                    // Parse types
                    var contentTypes = new List<IslamicContentType>();
                    foreach (var type in types)
                    {
                        switch (type.ToLower())
                        {
                            case "quran":
                                contentTypes.Add(IslamicContentType.Verse);
                                break;
                            case "hadith":
                                contentTypes.Add(IslamicContentType.Hadith);
                                break;
                            case "fiqh":
                                contentTypes.Add(IslamicContentType.FiqhRuling);
                                break;
                            case "dua":
                                contentTypes.Add(IslamicContentType.Dua);
                                break;
                            case "sirah":
                                contentTypes.Add(IslamicContentType.SirahEvent);
                                break;
                            case "tajweed":
                                contentTypes.Add(IslamicContentType.TajweedRule);
                                break;
                        }
                    }
                    
                    results = await globalSearch.SearchByTypeAsync(query, contentTypes.ToArray(), language, maxResults);
                }
                else
                {
                    results = await globalSearch.SearchAllAsync(query, language, maxResults);
                }

                // Display results summary
                System.Console.WriteLine($"📊 Found {results.TotalResults} results in {results.SearchDuration:F0}ms");
                System.Console.WriteLine();

                // Display results by type
                foreach (var typeGroup in results.Results.GroupBy(r => r.Type))
                {
                    var typeIcon = GetContentTypeIcon(typeGroup.Key);
                    System.Console.WriteLine($"{typeIcon} {typeGroup.Key} ({typeGroup.Count()} results)");
                    System.Console.WriteLine(new string('-', 40));                    foreach (var result in typeGroup.Take(5)) // Show top 5 per type
                    {
                        System.Console.WriteLine($"📌 {result.Title}");
                        System.Console.WriteLine($"   {result.Content}");
                        
                        if (!string.IsNullOrEmpty(result.ArabicText))
                        {
                            System.Console.WriteLine($"   📜 {result.ArabicText}");
                        }
                        
                        System.Console.WriteLine($"   📍 Source: {result.Source} | Reference: {result.Reference}");
                        System.Console.WriteLine($"   🎯 Relevance: {result.RelevanceScore:F1}");
                        System.Console.WriteLine();
                    }
                }

                // Display search suggestions
                System.Console.WriteLine("💡 Related search suggestions:");
                var suggestions = await globalSearch.GetSearchSuggestionsAsync(query, language);
                foreach (var suggestion in suggestions.Take(5))
                {
                    System.Console.WriteLine($"   • {suggestion}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Error performing global search: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Console.WriteLine($"   Inner: {ex.InnerException.Message}");
                }
            }
        }, queryOption, languageOption, maxResultsOption, typesOption);

        return command;
    }

    static Command CreateHybridSearchCommand()
    {
        var queryOption = new Option<string>("--query", "Search query for Islamic knowledge") { IsRequired = true };
        var languageOption = new Option<string>("--language", () => "en", "Language preference (en, ar)");
        var maxResultsOption = new Option<int>("--max-results", () => 20, "Maximum number of results to return");

        var command = new Command("hybrid-search", "🔍 Intelligent search using Local + AI + Web sources")
        {
            queryOption,
            languageOption,
            maxResultsOption
        };

        command.SetHandler(async (query, language, maxResults) =>
        {
            try
            {
                System.Console.WriteLine("🔍 Muwasala Hybrid Search");
                System.Console.WriteLine("═══════════════════════════");
                System.Console.WriteLine($"Query: {query}");
                System.Console.WriteLine($"Language: {language}");
                System.Console.WriteLine($"Max Results: {maxResults}");
                System.Console.WriteLine();

                var serviceProvider = CreateServiceProvider();
                var intelligentSearch = serviceProvider.GetRequiredService<IIntelligentSearchService>();

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await intelligentSearch.PerformHybridSearchAsync(query, language, maxResults);
                stopwatch.Stop();

                System.Console.WriteLine($"⚡ Search completed in {response.SearchDurationMs:F0}ms");
                System.Console.WriteLine($"📊 Total results found: {response.TotalResultsFound}");
                System.Console.WriteLine();

                // Display results by source
                System.Console.WriteLine("📋 Results by Source:");
                foreach (var sourceCount in response.ResultsBySource)
                {
                    System.Console.WriteLine($"   • {sourceCount.Key}: {sourceCount.Value} results");
                }
                System.Console.WriteLine();

                // Display local results
                if (response.LocalResults.Any())
                {
                    System.Console.WriteLine("🏠 Local Knowledge Base Results:");
                    System.Console.WriteLine("─────────────────────────────────");
                    foreach (var result in response.LocalResults.Take(5))
                    {
                        var icon = GetContentTypeIcon(result.Type);
                        System.Console.WriteLine($"{icon} {result.Title}");
                        System.Console.WriteLine($"   Source: {result.Source} | Reference: {result.Reference}");
                        System.Console.WriteLine($"   Relevance: {result.RelevanceScore:F1}/10");
                        
                        var preview = result.Content.Length > 150 
                            ? result.Content.Substring(0, 150) + "..." 
                            : result.Content;
                        System.Console.WriteLine($"   {preview}");
                        System.Console.WriteLine();
                    }
                }

                // Display AI response
                if (response.AIResponse != null)
                {
                    System.Console.WriteLine("🤖 AI Knowledge Response:");
                    System.Console.WriteLine("─────────────────────────");
                    System.Console.WriteLine($"Confidence: {response.AIResponse.ConfidenceScore:F1}/10");
                    System.Console.WriteLine($"Source: {response.AIResponse.Source}");
                    System.Console.WriteLine();
                    
                    var aiPreview = response.AIResponse.Response.Length > 300 
                        ? response.AIResponse.Response.Substring(0, 300) + "..." 
                        : response.AIResponse.Response;
                    System.Console.WriteLine(aiPreview);
                    
                    if (response.AIResponse.QuranReferences.Any())
                    {
                        System.Console.WriteLine();
                        System.Console.WriteLine("📖 Quran References:");
                        foreach (var qref in response.AIResponse.QuranReferences.Take(3))
                        {
                            System.Console.WriteLine($"   • {qref}");
                        }
                    }
                    
                    if (response.AIResponse.HadithReferences.Any())
                    {
                        System.Console.WriteLine();
                        System.Console.WriteLine("📚 Hadith References:");
                        foreach (var href in response.AIResponse.HadithReferences.Take(3))
                        {
                            System.Console.WriteLine($"   • {href}");
                        }
                    }
                    System.Console.WriteLine();
                }

                // Display web results
                if (response.WebResults.Any())
                {
                    System.Console.WriteLine("🌐 Web Source Results:");
                    System.Console.WriteLine("─────────────────────");
                    foreach (var webResult in response.WebResults.Take(3))
                    {
                        System.Console.WriteLine($"🔗 {webResult.Title}");
                        System.Console.WriteLine($"   Source: {webResult.Source} | Relevance: {webResult.RelevanceScore:F1}/10");
                        System.Console.WriteLine($"   URL: {webResult.Url}");
                        
                        var webPreview = webResult.Content.Length > 150 
                            ? webResult.Content.Substring(0, 150) + "..." 
                            : webResult.Content;
                        System.Console.WriteLine($"   {webPreview}");
                        System.Console.WriteLine();
                    }
                }

                // Display search suggestions
                if (response.SearchSuggestions.Any())
                {
                    System.Console.WriteLine("💡 Related Search Suggestions:");
                    foreach (var suggestion in response.SearchSuggestions)
                    {
                        System.Console.WriteLine($"   • {suggestion}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Error performing hybrid search: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Console.WriteLine($"   Inner: {ex.InnerException.Message}");
                }
            }
        }, queryOption, languageOption, maxResultsOption);

        return command;
    }

    static string GetContentTypeIcon(IslamicContentType type) => type switch
    {
        IslamicContentType.Verse => "📖",
        IslamicContentType.Hadith => "📚",
        IslamicContentType.FiqhRuling => "⚖️",
        IslamicContentType.Dua => "🤲",
        IslamicContentType.SirahEvent => "📜",
        IslamicContentType.TajweedRule => "🎵",
        _ => "📄"
    };    static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Add HTTP client
        services.AddHttpClient();        // Add configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:IslamicKnowledgeDb"] = @"Data Source=d:\Coding\VSCodeProject\Muwasala Islamic Knowledge Network\data\database\IslamicKnowledge.db"
            })
            .Build();
            
        services.AddSingleton<IConfiguration>(configuration);
        
        // Register core services
        services.AddSingleton<IOllamaService, OllamaService>();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        
        // Register Islamic Knowledge Base with database services
        services.AddIslamicKnowledgeBase(configuration, useDatabaseServices: true);
        
        // Register intelligent search service (AI + Web)
        services.AddSingleton<IIntelligentSearchService, IntelligentSearchService>();
        
        // Register agents
        services.AddSingleton<QuranNavigatorAgent>();
        services.AddSingleton<HadithVerifierAgent>();
        services.AddSingleton<FiqhAdvisorAgent>();
        services.AddSingleton<DuaCompanionAgent>();
        services.AddSingleton<TajweedTutorAgent>();
        services.AddSingleton<SirahScholarAgent>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Initialize the database
        Task.Run(async () =>
        {
            try
            {
                await serviceProvider.InitializeIslamicKnowledgeDatabaseAsync();
                System.Console.WriteLine("✅ Database initialized successfully");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"⚠️ Database initialization failed: {ex.Message}");
            }
        }).Wait();
        
        return serviceProvider;
    }

    static void DisplayVerseResponse(VerseResponse response)
    {
        System.Console.WriteLine("📖 Quranic Guidance");
        System.Console.WriteLine("═══════════════════");
        System.Console.WriteLine($"Verse: {response.Verse}");
        System.Console.WriteLine($"Arabic: {response.ArabicText}");
        System.Console.WriteLine($"Translation: {response.Translation}");
        
        if (!string.IsNullOrEmpty(response.Tafsir))
        {
            System.Console.WriteLine($"\n💡 Tafsir ({response.TafsirSource}):");
            System.Console.WriteLine(response.Tafsir);
        }
        
        if (response.RelatedDuas.Any())
        {
            System.Console.WriteLine($"\n🤲 Related Du'as: {string.Join(", ", response.RelatedDuas)}");
        }
        
        if (!string.IsNullOrEmpty(response.Warning))
        {
            System.Console.WriteLine($"\n⚠️ {response.Warning}");
        }
    }

    static void DisplayHadithResponse(HadithResponse response)
    {
        System.Console.WriteLine("📜 Hadith Verification");
        System.Console.WriteLine("════════════════════");
        System.Console.WriteLine($"Grade: {GetGradeEmoji(response.Grade)} {response.Grade}");
        System.Console.WriteLine($"Collection: {response.Collection}");
        
        if (!string.IsNullOrEmpty(response.HadithNumber))
        {
            System.Console.WriteLine($"Reference: {response.Collection} {response.HadithNumber}");
        }
        
        System.Console.WriteLine($"Arabic: {response.Text}");
        System.Console.WriteLine($"Translation: {response.Translation}");
        
        if (!string.IsNullOrEmpty(response.Explanation))
        {
            System.Console.WriteLine($"\n💡 Explanation:");
            System.Console.WriteLine(response.Explanation);
        }
        
        if (!string.IsNullOrEmpty(response.Warning))
        {
            System.Console.WriteLine($"\n⚠️ {response.Warning}");
        }
    }

    static void DisplayFiqhResponse(FiqhResponse response)
    {
        System.Console.WriteLine($"⚖️ {response.Madhab} Fiqh Ruling");
        System.Console.WriteLine("══════════════════════");
        System.Console.WriteLine($"Question: {response.Question}");
        System.Console.WriteLine($"Ruling: {response.Ruling}");
        
        if (!string.IsNullOrEmpty(response.Evidence))
        {
            System.Console.WriteLine($"\n📚 Evidence: {response.Evidence}");
        }
        
        if (response.OtherMadhabRulings.Any())
        {
            System.Console.WriteLine("\n🏛️ Other Madhab Views:");
            foreach (var ruling in response.OtherMadhabRulings)
            {
                System.Console.WriteLine($"  {ruling.Key}: {ruling.Value}");
            }
        }
        
        if (!string.IsNullOrEmpty(response.Warning))
        {
            System.Console.WriteLine($"\n⚠️ {response.Warning}");
        }
    }

    static void DisplayDuaResponse(DuaResponse response)
    {
        System.Console.WriteLine($"🤲 Du'a for {response.Occasion}");
        System.Console.WriteLine("═══════════════════════");
        System.Console.WriteLine($"Arabic: {response.ArabicText}");
        System.Console.WriteLine($"Translation: {response.Translation}");
        System.Console.WriteLine($"Transliteration: {response.Transliteration}");
        
        if (!string.IsNullOrEmpty(response.Source))
        {
            System.Console.WriteLine($"Source: {response.Source}");
        }
        
        if (!string.IsNullOrEmpty(response.Benefits))
        {
            System.Console.WriteLine($"\n💎 Benefits: {response.Benefits}");
        }
    }

    static void DisplayTajweedResponse(TajweedResponse response)
    {
        System.Console.WriteLine("📖 Tajweed Analysis");
        System.Console.WriteLine("═══════════════════");
        System.Console.WriteLine($"Verse: {response.VerseText}");
        
        if (response.Rules.Any())
        {
            System.Console.WriteLine("\n📋 Tajweed Rules:");
            foreach (var rule in response.Rules)
            {
                System.Console.WriteLine($"  • {rule.Name}: {rule.Description}");
            }
        }
          if (!string.IsNullOrEmpty(response.PronunciationGuide))
        {
            System.Console.WriteLine($"\n🗣️ Pronunciation: {response.PronunciationGuide}");
        }
    }

    static void DisplaySirahResponse(SirahResponse response)
    {        System.Console.WriteLine("📜 Prophetic Guidance");
        System.Console.WriteLine("════════════════════");
        System.Console.WriteLine($"Topic: {response.Topic}");
        System.Console.WriteLine($"Event: {response.Event}");
        System.Console.WriteLine($"Description: {response.Description}");
        System.Console.WriteLine($"Period: {response.Period}");
        
        if (!string.IsNullOrEmpty(response.KeyLessons))
        {
            System.Console.WriteLine($"\n🏛️ Key Lessons:");
            System.Console.WriteLine(response.KeyLessons);
        }
        
        if (response.RelatedEvents.Any())
        {
            System.Console.WriteLine($"\n📅 Related Events:");
            foreach (var eventItem in response.RelatedEvents)
            {
                System.Console.WriteLine($"  • {eventItem}");
            }
        }
        
        if (response.ModernApplication.Any())
        {
            System.Console.WriteLine($"\n🌟 Modern Applications:");
            foreach (var application in response.ModernApplication)
            {
                System.Console.WriteLine($"  • {application}");
            }
        }
        
        if (!string.IsNullOrEmpty(response.PropheticWisdom))
        {
            System.Console.WriteLine($"\n💎 Prophetic Wisdom:");
            System.Console.WriteLine(response.PropheticWisdom);
        }
        
        if (!string.IsNullOrEmpty(response.Warning))
        {
            System.Console.WriteLine($"\n⚠️ {response.Warning}");
        }
    }

    static string GetGradeEmoji(HadithGrade grade) => grade switch
    {
        HadithGrade.Sahih => "✅",
        HadithGrade.Hasan => "✅",
        HadithGrade.Daif => "⚠️",
        HadithGrade.Mawdu => "❌",
        _ => "❓"
    };
}
