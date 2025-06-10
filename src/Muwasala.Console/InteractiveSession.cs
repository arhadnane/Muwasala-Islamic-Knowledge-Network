using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muwasala.Core.Services;
using Muwasala.Agents;
using Muwasala.Core.Models;

namespace Muwasala.Console;

/// <summary>
/// Interactive session for conversational Islamic guidance
/// </summary>
public class InteractiveSession
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InteractiveSession> _logger;

    public InteractiveSession(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<InteractiveSession>>();
    }

    public async Task RunAsync()
    {
        DisplayWelcome();
        
        while (true)
        {
            System.Console.Write("\n🕌 Muwasala> ");
            var input = System.Console.ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(input))
                continue;
                
            if (input.ToLower() is "exit" or "quit" or "q")
            {
                System.Console.WriteLine("بارك الله فيك (May Allah bless you). Assalamu alaikum!");
                break;
            }
            
            if (input.ToLower() == "help")
            {
                DisplayHelp();
                continue;
            }
            
            await ProcessUserInput(input);
        }
    }

    private void DisplayWelcome()
    {
        System.Console.Clear();
        System.Console.WriteLine("═══════════════════════════════════════════");
        System.Console.WriteLine("🕌 Muwasala: Islamic Knowledge Network");
        System.Console.WriteLine("═══════════════════════════════════════════");
        System.Console.WriteLine("Welcome to your AI-powered Islamic learning companion!");
        System.Console.WriteLine("Ask me about Quran, Hadith, Fiqh, Du'as, or Tajweed.");
        System.Console.WriteLine("\nExamples:");
        System.Console.WriteLine("• \"I'm feeling anxious, what does Islam say?\"");
        System.Console.WriteLine("• \"Is this hadith authentic: [hadith text]\"");
        System.Console.WriteLine("• \"What's the Hanafi ruling on combining prayers?\"");
        System.Console.WriteLine("• \"Teach me du'a for before sleep\"");
        System.Console.WriteLine("• \"Show me tajweed rules for Surah Al-Fatiha\"");
        System.Console.WriteLine("\nType 'help' for more commands or 'exit' to quit.");
    }

    private void DisplayHelp()
    {
        System.Console.WriteLine("\n📚 Available Commands:");
        System.Console.WriteLine("• quran [context] - Get Quranic guidance");
        System.Console.WriteLine("• hadith [text] - Verify hadith authenticity");
        System.Console.WriteLine("• fiqh [question] [madhab] - Get Islamic ruling");
        System.Console.WriteLine("• dua [occasion] - Find appropriate du'as");
        System.Console.WriteLine("• tajweed [surah:verse] - Learn tajweed rules");
        System.Console.WriteLine("• inheritance - Calculate Islamic inheritance");
        System.Console.WriteLine("• help - Show this help");
        System.Console.WriteLine("• exit - Quit the session");
        System.Console.WriteLine("\nOr just describe your situation naturally!");
    }

    private async Task ProcessUserInput(string input)
    {
        try
        {
            // Simple command parsing
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLower();
            var args = parts.Skip(1).ToArray();

            switch (command)
            {
                case "quran":
                    await HandleQuranQuery(string.Join(" ", args));
                    break;
                case "hadith":
                    await HandleHadithQuery(string.Join(" ", args));
                    break;
                case "fiqh":
                    await HandleFiqhQuery(string.Join(" ", args));
                    break;
                case "dua":
                    await HandleDuaQuery(string.Join(" ", args));
                    break;
                case "tajweed":
                    await HandleTajweedQuery(string.Join(" ", args));
                    break;
                case "inheritance":
                    await HandleInheritanceCalculator();
                    break;
                default:
                    // Natural language processing - route to most appropriate agent
                    await HandleNaturalLanguageQuery(input);
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error: {ex.Message}");
            _logger.LogError(ex, "Error processing user input: {Input}", input);
        }
    }

    private async Task HandleQuranQuery(string context)
    {
        if (string.IsNullOrEmpty(context))
        {
            System.Console.WriteLine("Please provide a context. Example: quran I'm feeling sad");
            return;
        }

        var agent = _serviceProvider.GetRequiredService<QuranNavigatorAgent>();
        System.Console.WriteLine("🔍 Searching for relevant Quranic guidance...");
        
        var response = await agent.GetVerseAsync(context);
        DisplayQuranGuidance(response);
    }

    private async Task HandleHadithQuery(string hadithText)
    {
        if (string.IsNullOrEmpty(hadithText))
        {
            System.Console.WriteLine("Please provide hadith text to verify.");
            return;
        }

        var agent = _serviceProvider.GetRequiredService<HadithVerifierAgent>();
        System.Console.WriteLine("🔍 Verifying hadith authenticity...");
        
        var response = await agent.VerifyHadithAsync(hadithText);
        DisplayHadithVerification(response);
    }

    private async Task HandleFiqhQuery(string question)
    {
        if (string.IsNullOrEmpty(question))
        {
            System.Console.WriteLine("Please provide a fiqh question.");
            return;
        }

        // Extract madhab if specified
        var madhab = Madhab.Hanafi; // default
        if (question.Contains("shafi", StringComparison.OrdinalIgnoreCase))
            madhab = Madhab.Shafi;
        else if (question.Contains("maliki", StringComparison.OrdinalIgnoreCase))
            madhab = Madhab.Maliki;
        else if (question.Contains("hanbali", StringComparison.OrdinalIgnoreCase))
            madhab = Madhab.Hanbali;

        var agent = _serviceProvider.GetRequiredService<FiqhAdvisorAgent>();
        System.Console.WriteLine($"⚖️ Getting {madhab} ruling...");
        
        var response = await agent.GetRulingAsync(question, madhab);
        DisplayFiqhRuling(response);
    }

    private async Task HandleDuaQuery(string occasion)
    {
        if (string.IsNullOrEmpty(occasion))
        {
            System.Console.WriteLine("Please specify an occasion. Example: dua before sleep");
            return;
        }

        var agent = _serviceProvider.GetRequiredService<DuaCompanionAgent>();
        System.Console.WriteLine($"🤲 Finding du'as for: {occasion}...");
        
        var responses = await agent.GetDuasForOccasionAsync(occasion);
        
        if (responses.Any())
        {
            foreach (var response in responses)
            {
                DisplayDuaRecommendation(response);
                System.Console.WriteLine();
            }
        }
        else
        {
            System.Console.WriteLine("No specific du'as found. Here's a general du'a:");
            DisplayGeneralDua();
        }
    }

    private async Task HandleTajweedQuery(string verseRef)
    {
        // Parse verse reference (e.g., "1:1" or "Al-Fatiha:1")
        var parts = verseRef.Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int surah) || !int.TryParse(parts[1], out int verse))
        {
            System.Console.WriteLine("Please provide verse reference in format 'surah:verse' (e.g., '1:1')");
            return;
        }

        var agent = _serviceProvider.GetRequiredService<TajweedTutorAgent>();
        System.Console.WriteLine($"📖 Analyzing tajweed for {surah}:{verse}...");
        
        var response = await agent.AnalyzeVerseAsync(new VerseReference(surah, verse));
        DisplayTajweedGuidance(response);
    }

    private async Task HandleInheritanceCalculator()
    {
        System.Console.WriteLine("🧮 Islamic Inheritance Calculator");
        System.Console.WriteLine("This is a simplified example. For real cases, consult qualified scholars.");
        
        // This would be a more interactive process in a real implementation
        var agent = _serviceProvider.GetRequiredService<FiqhAdvisorAgent>();
        
        // Example case
        var inheritanceCase = new FiqhAdvisorAgent.InheritanceCase(
            "Male",
            100000m,
            new List<FiqhAdvisorAgent.Heir>
            {
                new("Wife", "Female"),
                new("Son", "Male"),
                new("Daughter", "Female"),
                new("Father", "Male")
            }
        );

        var result = await agent.CalculateInheritanceAsync(inheritanceCase);
        DisplayInheritanceResult(result);
    }

    private async Task HandleNaturalLanguageQuery(string input)
    {
        // Simple keyword-based routing (in a real system, you'd use NLP)
        var keywords = input.ToLower();
        
        if (keywords.Contains("anxious") || keywords.Contains("sad") || keywords.Contains("difficulty") ||
            keywords.Contains("stress") || keywords.Contains("worry") || keywords.Contains("guidance"))
        {
            await HandleQuranQuery(input);
        }
        else if (keywords.Contains("authentic") || keywords.Contains("hadith") || keywords.Contains("narration"))
        {
            await HandleHadithQuery(input);
        }
        else if (keywords.Contains("ruling") || keywords.Contains("allowed") || keywords.Contains("haram") ||
                 keywords.Contains("halal") || keywords.Contains("fiqh") || keywords.Contains("prayer time"))
        {
            await HandleFiqhQuery(input);
        }
        else if (keywords.Contains("dua") || keywords.Contains("prayer") || keywords.Contains("supplication"))
        {
            await HandleDuaQuery(input);
        }
        else
        {
            // Default to Quranic guidance
            System.Console.WriteLine("🤔 I'll search for relevant Quranic guidance for your situation...");
            await HandleQuranQuery(input);
        }
    }

    private void DisplayQuranGuidance(VerseResponse response)
    {
        System.Console.WriteLine("\n📖 Quranic Guidance");
        System.Console.WriteLine("═══════════════════");
        System.Console.WriteLine($"📍 {response.Verse}");
        System.Console.WriteLine($"🔤 {response.ArabicText}");
        System.Console.WriteLine($"📝 {response.Translation}");
        
        if (!string.IsNullOrEmpty(response.Tafsir))
        {
            System.Console.WriteLine($"\n💡 Tafsir: {response.Tafsir}");
        }
        
        if (response.RelatedDuas.Any())
        {
            System.Console.WriteLine($"\n🤲 Related Du'as: {string.Join(", ", response.RelatedDuas)}");
        }
    }

    private void DisplayHadithVerification(HadithResponse response)
    {
        var gradeEmoji = response.Grade switch
        {
            HadithGrade.Sahih => "✅ Authentic",
            HadithGrade.Hasan => "✅ Good",
            HadithGrade.Daif => "⚠️ Weak",
            HadithGrade.Mawdu => "❌ Fabricated",
            _ => "❓ Unknown"
        };

        System.Console.WriteLine("\n📜 Hadith Verification");
        System.Console.WriteLine("════════════════════");
        System.Console.WriteLine($"🎯 Grade: {gradeEmoji}");
        System.Console.WriteLine($"📚 Collection: {response.Collection}");
        System.Console.WriteLine($"🔤 Arabic: {response.Text}");
        System.Console.WriteLine($"📝 Translation: {response.Translation}");
        
        if (!string.IsNullOrEmpty(response.Explanation))
        {
            System.Console.WriteLine($"\n💡 Explanation: {response.Explanation}");
        }
    }

    private void DisplayFiqhRuling(FiqhResponse response)
    {
        System.Console.WriteLine($"\n⚖️ {response.Madhab} Fiqh Ruling");
        System.Console.WriteLine("══════════════════════");
        System.Console.WriteLine($"❓ Question: {response.Question}");
        System.Console.WriteLine($"✅ Ruling: {response.Ruling}");
        
        if (!string.IsNullOrEmpty(response.Evidence))
        {
            System.Console.WriteLine($"📚 Evidence: {response.Evidence}");
        }
        
        if (response.OtherMadhabRulings.Any())
        {
            System.Console.WriteLine("\n🏛️ Other Madhab Views:");
            foreach (var ruling in response.OtherMadhabRulings)
            {
                System.Console.WriteLine($"  • {ruling.Key}: {ruling.Value}");
            }
        }
    }

    private void DisplayDuaRecommendation(DuaResponse response)
    {
        System.Console.WriteLine($"🤲 Du'a for {response.Occasion}");
        System.Console.WriteLine("═══════════════════════");
        System.Console.WriteLine($"🔤 {response.ArabicText}");
        System.Console.WriteLine($"📝 {response.Translation}");
        System.Console.WriteLine($"🗣️ {response.Transliteration}");
        
        if (!string.IsNullOrEmpty(response.Source))
        {
            System.Console.WriteLine($"📚 Source: {response.Source}");
        }
    }

    private void DisplayTajweedGuidance(TajweedResponse response)
    {
        System.Console.WriteLine("\n📖 Tajweed Analysis");
        System.Console.WriteLine("═══════════════════");
        System.Console.WriteLine($"🔤 {response.VerseText}");
        
        if (response.Rules.Any())
        {
            System.Console.WriteLine("\n📋 Tajweed Rules Applied:");
            foreach (var rule in response.Rules)
            {
                System.Console.WriteLine($"  • {rule.Name}: {rule.Description}");
            }
        }
    }

    private void DisplayInheritanceResult(InheritanceResult result)
    {
        System.Console.WriteLine("\n🧮 Inheritance Calculation");
        System.Console.WriteLine("═══════════════════════════");
        System.Console.WriteLine($"💰 Total Estate: {result.TotalEstate:C}");
        System.Console.WriteLine("\n📊 Shares:");
        
        foreach (var share in result.Shares)
        {
            var amount = result.FinalAmounts.ContainsKey(share.Key) 
                ? result.FinalAmounts[share.Key].ToString("C")
                : "N/A";
            System.Console.WriteLine($"  • {share.Key}: {share.Value} = {amount}");
        }
        
        System.Console.WriteLine($"\n📖 Method: {result.CalculationMethod}");
        System.Console.WriteLine("⚠️ This is a simplified calculation. Consult qualified scholars for actual inheritance matters.");
    }

    private void DisplayGeneralDua()
    {
        System.Console.WriteLine("🤲 General Du'a (Quran 2:201)");
        System.Console.WriteLine("════════════════════════════");
        System.Console.WriteLine("🔤 رَبَّنَا آتِنَا فِي الدُّنْيَا حَسَنَةً وَفِي الْآخِرَةِ حَسَنَةً وَقِنَا عَذَابَ النَّارِ");
        System.Console.WriteLine("📝 Our Lord, give us good in this world and good in the hereafter, and protect us from the punishment of the Fire.");
        System.Console.WriteLine("🗣️ Rabbana atina fi'd-dunya hasanatan wa fi'l-akhirati hasanatan wa qina 'adhab an-nar");
    }
}
