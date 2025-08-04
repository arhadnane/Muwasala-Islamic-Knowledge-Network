using Microsoft.Extensions.Logging;
using Muwasala.Core.Models;
using Muwasala.Core.Services;
using Muwasala.KnowledgeBase.Services;

namespace Muwasala.Agents;

/// <summary>
/// Tajweed Tutor Agent - Quranic recitation correction and guidance
/// Uses Phi-3 model for precise pronunciation and tajweed rule instruction
/// </summary>
public class TajweedTutorAgent
{
    private readonly IOllamaService _ollama;
    private readonly ITajweedService _tajweedService;
    private readonly ILogger<TajweedTutorAgent> _logger;
    private const string MODEL_NAME = "deepseek-r1";

    public TajweedTutorAgent(
        IOllamaService ollama,
        ITajweedService tajweedService,
        ILogger<TajweedTutorAgent> logger)
    {
        _ollama = ollama;
        _tajweedService = tajweedService;
        _logger = logger;
    }

    /// <summary>
    /// Analyze tajweed rules for a specific verse
    /// </summary>
    public async Task<TajweedResponse> AnalyzeVerseAsync(
        VerseReference verse, 
        string language = "en")
    {
        _logger.LogInformation("TajweedTutor analyzing verse: {Verse}", verse);        try
        {
            // Get verse text and tajweed rules
            var verseData = await _tajweedService.GetVerseWithTajweedAsync(verse);
            if (verseData == null)
            {
                throw new ArgumentException($"Verse {verse} not found");
            }

            // Analyze tajweed rules with AI - using simplified text response
            var prompt = BuildTajweedAnalysisPrompt(verseData, language);
            var analysis = await _ollama.GenerateResponseAsync(
                MODEL_NAME, prompt, temperature: 0.3);

            // Create simplified response with basic tajweed rules
            var response = new TajweedResponse
            {
                VerseText = verseData.ArabicText,
                Rules = new List<TajweedRule>
                {
                    new TajweedRule(
                        "General Tajweed Guidance",
                        analysis,
                        0,
                        verseData.ArabicText.Length,
                        verseData.ArabicText
                    )
                },
                PronunciationGuide = "Please listen to qualified Quranic recitation for proper pronunciation"
            };

            response.Sources.AddRange(new[] { "Tajweed Rules Database", MODEL_NAME, "Quranic Audio Library" });

            _logger.LogInformation("TajweedTutor completed analysis for verse {Verse}", verse);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TajweedTutor for verse: {Verse}", verse);
            throw;
        }
    }

    /// <summary>
    /// Get step-by-step pronunciation guide for difficult words
    /// </summary>
    public async Task<PronunciationGuide> GetPronunciationGuideAsync(
        string arabicWord, 
        string language = "en")
    {
        _logger.LogInformation("TajweedTutor creating pronunciation guide for: {Word}", arabicWord);

        var prompt = BuildPronunciationPrompt(arabicWord, language);
        var guide = await _ollama.GenerateStructuredResponseAsync<PronunciationGuide>(
            MODEL_NAME, prompt, temperature: 0.1);

        return guide;
    }

    /// <summary>
    /// Provide interactive recitation lesson for a surah
    /// </summary>
    public async Task<RecitationLesson> CreateRecitationLessonAsync(
        int surahNumber, 
        RecitationLevel level = RecitationLevel.Beginner,
        string language = "en")
    {
        _logger.LogInformation("Creating recitation lesson for Surah {Surah}, level: {Level}", 
            surahNumber, level);

        var surahData = await _tajweedService.GetSurahForLessonAsync(surahNumber, level);
        if (surahData == null)
        {
            throw new ArgumentException($"Surah {surahNumber} not found");
        }

        var prompt = BuildLessonPrompt(surahData, level, language);
        
        try 
        {
            var lessonPlan = await _ollama.GenerateStructuredResponseAsync<LessonPlan>(
                MODEL_NAME, prompt, temperature: 0.1);
            
            // Create step-by-step lesson
            var lessonSteps = await CreateLessonStepsAsync(surahData, lessonPlan, language);
            var lesson = new RecitationLesson(
                surahNumber,
                surahData.Name,
                level,
                surahData.VerseCount,
                CalculateLessonDuration(surahData.VerseCount, level),
                lessonSteps,
                lessonPlan.Prerequisites,
                lessonPlan.LearningObjectives
            );

            return lesson;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate structured lesson plan, falling back to simple lesson");
            
            // Fallback to simple lesson plan
            var simpleLessonPlan = new LessonPlan(
                Prerequisites: new List<string> 
                { 
                    language == "ar" ? "معرفة أساسية بالحروف العربية" : "Basic knowledge of Arabic letters",
                    language == "ar" ? "فهم أهمية التجويد" : "Understanding the importance of Tajweed"
                },
                LearningObjectives: new List<string> 
                { 
                    language == "ar" ? "إتقان قواعد التجويد الأساسية" : "Master basic Tajweed rules",
                    language == "ar" ? "تحسين النطق" : "Improve pronunciation"
                },
                StepCount: 5
            );
            
            var simpleSteps = new List<LessonStep>();
            for (int i = 1; i <= 5; i++)
            {
                simpleSteps.Add(new LessonStep(
                    StepNumber: i,
                    Title: language == "ar" ? $"الخطوة {i}" : $"Step {i}",
                    Objective: language == "ar" ? "تعلم قواعد التجويد" : "Learn Tajweed rules",
                    VerseReferences: new List<string> { $"{surahData.Name} - {(language == "ar" ? "آية" : "Verse")} {i}" },
                    TajweedFocus: new List<string> { language == "ar" ? "قواعد أساسية" : "Basic rules" },
                    PracticeExercises: new List<string> { language == "ar" ? "تمارين النطق" : "Pronunciation exercises" },
                    AssessmentCriteria: language == "ar" ? "دقة النطق" : "Pronunciation accuracy"
                ));
            }
            
            return new RecitationLesson(
                surahNumber,
                surahData.Name,
                level,
                surahData.VerseCount,
                CalculateLessonDuration(surahData.VerseCount, level),
                simpleSteps,
                simpleLessonPlan.Prerequisites,
                simpleLessonPlan.LearningObjectives
            );
        }
    }

    /// <summary>
    /// Check common tajweed mistakes and provide corrections
    /// </summary>
    public async Task<List<TajweedCorrection>> CheckCommonMistakesAsync(
        VerseReference verse,
        string language = "en")
    {
        _logger.LogInformation("Checking common tajweed mistakes for verse: {Verse}", verse);

        var verseData = await _tajweedService.GetVerseWithTajweedAsync(verse);
        var commonMistakes = await _tajweedService.GetCommonMistakesAsync(verse);

        var corrections = new List<TajweedCorrection>();

        foreach (var mistake in commonMistakes)
        {
            try 
            {
                var prompt = BuildCorrectionPrompt(mistake, verseData?.ArabicText ?? "", language);
                var correction = await _ollama.GenerateStructuredResponseAsync<TajweedCorrection>(
                    MODEL_NAME, prompt, temperature: 0.1);

                corrections.Add(correction);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate correction for mistake {Type}, using fallback", mistake.Type);
                
                // Fallback simple correction
                var simpleCorrection = new TajweedCorrection(
                    MistakeType: mistake.Type,
                    IncorrectPronunciation: mistake.TypicalError,
                    CorrectPronunciation: language == "ar" ? "النطق الصحيح" : "Correct pronunciation",
                    Explanation: language == "ar" ? "تصحيح الخطأ" : mistake.Description,
                    PracticeTips: new List<string> { language == "ar" ? "تمرن مع معلم" : "Practice with a teacher" }
                );
                
                corrections.Add(simpleCorrection);
            }
        }

        return corrections;
    }

    /// <summary>
    /// Get melodies (Qira'at) information for advanced learners
    /// </summary>
    public async Task<QiraatInfo> GetQiraatInfoAsync(
        VerseReference verse,
        QiraatType qiraatType = QiraatType.Hafs,
        string language = "en")
    {
        _logger.LogInformation("Getting Qiraat info for verse {Verse}, type: {QiraatType}", verse, qiraatType);

        var qiraatData = await _tajweedService.GetQiraatDataAsync(verse, qiraatType);
        
        if (qiraatData == null)
        {
            throw new ArgumentException($"Qiraat data not found for verse {verse} and type {qiraatType}");
        }
        
        var prompt = BuildQiraatPrompt(qiraatData, qiraatType, language);
        var analysis = await _ollama.GenerateStructuredResponseAsync<QiraatAnalysis>(
            MODEL_NAME, prompt, temperature: 0.1);        return new QiraatInfo(
            verse,
            qiraatType,
            analysis.Variations,
            analysis.HistoricalContext,
            qiraatData.AudioReferences,
            analysis.ScholarlyNotes
        );
    }

    private async Task<TajweedRule> GetDetailedRuleExplanationAsync(
        BasicTajweedRule basicRule, 
        string verseText, 
        string language)
    {
        var languageInstruction = language switch
        {
            "ar" => "يرجى الرد باللغة العربية فقط. استخدم المصطلحات الإسلامية بالعربية وقدم جميع الشروحات باللغة العربية. لا تستخدم الإنجليزية في الرد.",
            "en" => "Please respond in English language. Use clear English explanations.",
            _ => "Please respond in English language. Use clear English explanations."
        };

        var prompt = $@"You are a Tajweed expert providing pronunciation guidance.

{languageInstruction}

Provide detailed explanation for this tajweed rule:

Rule: {basicRule.Name}
Context: {verseText}
Position: Characters {basicRule.StartPosition}-{basicRule.EndPosition}

Explain:
1. What this rule means
2. How to pronounce it correctly
3. Common mistakes students make
4. Practice tips

Respond in JSON format with detailed explanation in the requested language.";

        var explanation = await _ollama.GenerateStructuredResponseAsync<RuleExplanation>(
            MODEL_NAME, prompt, temperature: 0.1);

        return new TajweedRule(
            Name: basicRule.Name,
            Description: explanation.Description,
            StartPosition: basicRule.StartPosition,
            EndPosition: basicRule.EndPosition,
            Example: explanation.Example
        );
    }

    private async Task<List<LessonStep>> CreateLessonStepsAsync(
        SurahData surahData, 
        LessonPlan plan, 
        string language)
    {
        var steps = new List<LessonStep>();
        
        // Create progressive steps based on lesson plan
        for (int i = 0; i < plan.StepCount; i++)
        {
            try 
            {
                var stepPrompt = BuildStepPrompt(surahData, i + 1, plan.StepCount, language);
                var step = await _ollama.GenerateStructuredResponseAsync<LessonStep>(
                    MODEL_NAME, stepPrompt, temperature: 0.1);
                
                steps.Add(step);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate step {Step}, using fallback", i + 1);
                
                // Fallback simple step
                var simpleStep = new LessonStep(
                    StepNumber: i + 1,
                    Title: language == "ar" ? $"الخطوة {i + 1}" : $"Step {i + 1}",
                    Objective: language == "ar" ? "تعلم قواعد التجويد" : "Learn Tajweed rules",
                    VerseReferences: new List<string> { $"{surahData.Name}" },
                    TajweedFocus: new List<string> { language == "ar" ? "قواعد أساسية" : "Basic rules" },
                    PracticeExercises: new List<string> { language == "ar" ? "تمارين النطق" : "Pronunciation exercises" },
                    AssessmentCriteria: language == "ar" ? "دقة النطق" : "Pronunciation accuracy"
                );
                
                steps.Add(simpleStep);
            }
        }

        return steps;
    }

    private TimeSpan CalculateLessonDuration(int verseCount, RecitationLevel level)
    {
        var baseMinutes = level switch
        {
            RecitationLevel.Beginner => verseCount * 5,
            RecitationLevel.Intermediate => verseCount * 3,
            RecitationLevel.Advanced => verseCount * 2,
            _ => verseCount * 4
        };

        return TimeSpan.FromMinutes(Math.Max(15, Math.Min(120, baseMinutes)));
    }    private string BuildTajweedAnalysisPrompt(VerseData verseData, string language)
    {
        var languageInstruction = language switch
        {
            "ar" => "يرجى الرد باللغة العربية فقط. استخدم المصطلحات الإسلامية بالعربية وقدم جميع الشروحات باللغة العربية. لا تستخدم الإنجليزية في الرد.",
            "en" => "Please respond in English language. Use clear English explanations.",
            _ => "Please respond in English language. Use clear English explanations."
        };

        return $@"You are a Tajweed expert providing verse analysis.

{languageInstruction}

Analyze the tajweed rules for this Quranic verse:

Arabic Text: {verseData.ArabicText}
Translation: {verseData.Translation}
Verse Reference: {verseData.Surah}:{verseData.Verse}

Please provide clear guidance on:
1. Key tajweed rules that apply to this verse
2. Important pronunciation points
3. Common mistakes to avoid
4. Any special recitation notes

Respond with clear, practical guidance in the requested language without complex JSON formatting.";
    }

    private string BuildPronunciationPrompt(string arabicWord, string language)
    {
        var languageInstruction = language switch
        {
            "ar" => "يرجى الرد باللغة العربية فقط. استخدم المصطلحات الإسلامية بالعربية وقدم جميع الشروحات باللغة العربية. لا تستخدم الإنجليزية في الرد.",
            "en" => "Please respond in English language. Use clear English explanations.",
            _ => "Please respond in English language. Use clear English explanations."
        };

        return $@"You are a Tajweed expert providing pronunciation guidance.

{languageInstruction}

Create a detailed pronunciation guide for this Arabic word from the Quran:

Arabic Word: {arabicWord}

Provide:
1. Letter-by-letter breakdown
2. Phonetic transcription
3. Common pronunciation mistakes
4. Audio description (how it should sound)
5. Practice exercises

Respond in JSON format with step-by-step pronunciation guide in the requested language.";
    }

    private string BuildLessonPrompt(SurahData surahData, RecitationLevel level, string language)
    {
        var languageInstruction = language switch
        {
            "ar" => "يرجى الرد باللغة العربية فقط. استخدم المصطلحات الإسلامية بالعربية وقدم جميع الشروحات باللغة العربية. لا تستخدم الإنجليزية في الرد.",
            "en" => "Please respond in English language. Use clear English explanations.",
            _ => "Please respond in English language. Use clear English explanations."
        };

        return $@"You are a Tajweed expert creating educational content.

{languageInstruction}

Create a structured recitation lesson plan for:

Surah: {surahData.Name} (#{surahData.Number})
Verses: {surahData.VerseCount}
Student Level: {level}

Design lesson with:
1. Prerequisites students should know
2. Learning objectives
3. Number of recommended steps/sessions
4. Progressive difficulty structure

Respond in JSON format with educational lesson plan in the requested language.";
    }

    private string BuildCorrectionPrompt(CommonMistake mistake, string verseText, string language)
    {
        var languageInstruction = language switch
        {
            "ar" => "يرجى الرد باللغة العربية فقط. استخدم المصطلحات الإسلامية بالعربية وقدم جميع الشروحات باللغة العربية. لا تستخدم الإنجليزية في الرد.",
            "en" => "Please respond in English language. Use clear English explanations.",
            _ => "Please respond in English language. Use clear English explanations."
        };

        return $@"You are a Tajweed expert providing mistake correction guidance.

{languageInstruction}

Explain how to correct this common tajweed mistake:

Mistake Type: {mistake.Type}
Description: {mistake.Description}
Context Verse: {verseText}
Common Error: {mistake.TypicalError}

Provide:
1. Clear explanation of the correct pronunciation
2. Why this mistake happens
3. Step-by-step correction method
4. Practice tips to avoid this mistake

Respond in JSON format with correction guidance in the requested language.";
    }

    private string BuildQiraatPrompt(QiraatData qiraatData, QiraatType qiraatType, string language)
    {
        var languageInstruction = language switch
        {
            "ar" => "يرجى الرد باللغة العربية فقط. استخدم المصطلحات الإسلامية بالعربية وقدم جميع الشروحات باللغة العربية. لا تستخدم الإنجليزية في الرد.",
            "en" => "Please respond in English language. Use clear English explanations.",
            _ => "Please respond in English language. Use clear English explanations."
        };

        return $@"You are a Qira'at scholar providing recitation guidance.

{languageInstruction}

Analyze the Qira'at (recitation style) information:

Qira'at Type: {qiraatType}
Verse Reference: {qiraatData.Verse}
Variations: {string.Join(", ", qiraatData.Variations)}

Provide:
1. Historical background of this Qira'at
2. How variations affect pronunciation
3. Scholarly opinions and authenticity
4. Practical guidance for reciters

Respond in JSON format with Qira'at analysis in the requested language.";
    }

    private string BuildStepPrompt(SurahData surahData, int stepNumber, int totalSteps, string language)
    {
        var languageInstruction = language switch
        {
            "ar" => "يرجى الرد باللغة العربية فقط. استخدم المصطلحات الإسلامية بالعربية وقدم جميع الشروحات باللغة العربية. لا تستخدم الإنجليزية في الرد.",
            "en" => "Please respond in English language. Use clear English explanations.",
            _ => "Please respond in English language. Use clear English explanations."
        };

        return $@"You are a Tajweed expert creating lesson steps.

{languageInstruction}

Create lesson step {stepNumber} of {totalSteps} for Surah {surahData.Name}:

Focus on progressive learning with:
1. Specific verses or sections to practice
2. Key tajweed rules to emphasize
3. Practice exercises for this step
4. Assessment criteria

Respond in JSON format with detailed lesson step in the requested language.";
    }    // Supporting records - enums moved to Core.Models
    private record TajweedAnalysis(
        List<BasicTajweedRule> Rules,
        string PronunciationGuide,
        List<string> KeyPoints
    );

    private record BasicTajweedRule(
        string Name,
        int StartPosition,
        int EndPosition
    );

    public record PronunciationGuide(
        string Word,
        List<LetterGuide> Letters,
        string PhoneticTranscription,
        List<string> CommonMistakes,
        List<string> PracticeExercises
    );

    public record LetterGuide(
        string Letter,
        string Pronunciation,
        string Description
    );

    public record RecitationLesson(
        int SurahNumber,
        string SurahName,
        RecitationLevel Level,
        int TotalVerses,
        TimeSpan EstimatedDuration,
        List<LessonStep> Steps,
        List<string> Prerequisites,
        List<string> LearningObjectives
    );

    private record LessonPlan(
        List<string> Prerequisites,
        List<string> LearningObjectives,
        int StepCount
    );

    public record LessonStep(
        int StepNumber,
        string Title,
        string Objective,
        List<string> VerseReferences,
        List<string> TajweedFocus,
        List<string> PracticeExercises,
        string AssessmentCriteria
    );

    public record TajweedCorrection(
        string MistakeType,
        string IncorrectPronunciation,
        string CorrectPronunciation,
        string Explanation,
        List<string> PracticeTips
    );

    public record QiraatInfo(
        VerseReference Verse,
        QiraatType QiraatType,
        List<string> Variations,
        string HistoricalContext,
        List<string> AudioExamples,
        List<string> ScholarlyNotes
    );

    private record QiraatAnalysis(
        List<string> Variations,
        string HistoricalContext,
        List<string> ScholarlyNotes
    );

    private record RuleExplanation(
        string Description,
        string Example
    );
}
