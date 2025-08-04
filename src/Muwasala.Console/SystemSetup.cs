using Microsoft.Extensions.DependencyInjection;
using Muwasala.Core.Services;
using System.Text.Json;

namespace Muwasala.Console;

public class SystemSetup
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOllamaService _ollamaService;

    public SystemSetup(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _ollamaService = serviceProvider.GetRequiredService<IOllamaService>();
    }

    public async Task VerifySystemAsync()
    {
        System.Console.WriteLine("🔧 Muwasala System Setup & Verification");
        System.Console.WriteLine("═══════════════════════════════════════");
        System.Console.WriteLine();

        // Check Ollama connection
        await VerifyOllamaConnectionAsync();

        // Check required models
        await VerifyRequiredModelsAsync();

        // Display setup instructions
        DisplaySetupInstructions();
    }

    private async Task VerifyOllamaConnectionAsync()
    {
        System.Console.WriteLine("🔍 Checking Ollama connection...");
        
        try
        {
            var models = await _ollamaService.GetAvailableModelsAsync();
            System.Console.WriteLine("✅ Ollama is running and accessible");
            System.Console.WriteLine($"📊 Found {models.Count} available models");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine("❌ Failed to connect to Ollama");
            System.Console.WriteLine($"   Error: {ex.Message}");
            System.Console.WriteLine("   Make sure Ollama is installed and running on http://localhost:11434");
        }
        
        System.Console.WriteLine();
    }

    private async Task VerifyRequiredModelsAsync()
    {
        System.Console.WriteLine("📋 Checking required models...");        var requiredModels = new Dictionary<string, string>
        {
            { "mistral:7b-instruct", "Quran Navigator & Hadith Verifier Agent" },
            { "mistral:7b", "Du'a Companion Agent" },
            { "deepseek-r1", "Fiqh Advisor, Tajweed Tutor & Sirah Scholar Agent" }
        };

        try
        {            var availableModels = await _ollamaService.GetAvailableModelsAsync();
            var availableNames = availableModels.Select(m => m.ToLower()).ToList();            foreach (var required in requiredModels)
            {
                var isAvailable = availableNames.Any(name => name.Equals(required.Key.ToLower()));
                var status = isAvailable ? "✅" : "❌";
                System.Console.WriteLine($"  {status} {required.Key} - {required.Value}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Failed to check models: {ex.Message}");
        }

        System.Console.WriteLine();
    }

    private void DisplaySetupInstructions()
    {
        System.Console.WriteLine("📖 Setup Instructions");
        System.Console.WriteLine("════════════════════");
        System.Console.WriteLine();

        System.Console.WriteLine("1. Install Ollama:");
        System.Console.WriteLine("   • Download from: https://ollama.ai");
        System.Console.WriteLine("   • Follow installation instructions for your OS");
        System.Console.WriteLine();        System.Console.WriteLine("2. Pull required models:");
        System.Console.WriteLine("   ollama pull mistral:7b-instruct   # For Quran Navigator & Hadith Verifier");
        System.Console.WriteLine("   ollama pull mistral:7b             # For Du'a Companion");
        System.Console.WriteLine("   ollama pull deepseek-r1            # For Fiqh, Tajweed & Sirah Scholar");
        System.Console.WriteLine();

        System.Console.WriteLine("3. Start Ollama service:");
        System.Console.WriteLine("   ollama serve");
        System.Console.WriteLine();

        System.Console.WriteLine("4. Verify installation:");
        System.Console.WriteLine("   muwasala setup");
        System.Console.WriteLine();

        System.Console.WriteLine("5. Start using the system:");
        System.Console.WriteLine("   muwasala interactive           # Interactive mode");
        System.Console.WriteLine("   muwasala quran --context \"guidance for difficult times\"");
        System.Console.WriteLine("   muwasala hadith --text \"Actions are by intention\"");
        System.Console.WriteLine("   muwasala fiqh --question \"Prayer in travel\" --madhab Hanafi");
        System.Console.WriteLine("   muwasala dua --occasion \"morning\"");
        System.Console.WriteLine("   muwasala tajweed --surah 1 --verse 1");
        System.Console.WriteLine("   muwasala sirah --topic \"leadership\"");
        System.Console.WriteLine();

        System.Console.WriteLine("💡 For best results, ensure you have a stable internet connection");
        System.Console.WriteLine("   when first pulling models. They are typically 3-7GB each.");
    }
}
