using System;
using System.Text.RegularExpressions;
using Muwasala.Core.Models;

namespace TestVerseSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing Verse Parsing...");
            
            TestParseSpecificVerseRequest("Verse 1:1");
            TestParseSpecificVerseRequest("Verse 2:255");
            TestParseSpecificVerseRequest("1:1");
            TestParseSpecificVerseRequest("114:6");
            TestParseSpecificVerseRequest("just some text");
            TestParseSpecificVerseRequest("Verse 0:1"); // Invalid
            TestParseSpecificVerseRequest("Verse 115:1"); // Invalid
        }

        static void TestParseSpecificVerseRequest(string context)
        {
            Console.WriteLine($"\nTesting: '{context}'");
            var result = ParseSpecificVerseRequest(context);
            if (result != null)
            {
                Console.WriteLine($"Result: Surah {result.Surah}, Verse {result.Verse}");
            }
            else
            {
                Console.WriteLine("Result: Not a specific verse request");
            }
        }

        static VerseReference? ParseSpecificVerseRequest(string context)
        {
            if (string.IsNullOrWhiteSpace(context))
                return null;

            // Try to match "Verse X:Y" pattern
            var versePattern = @"^Verse\s+(\d+):(\d+)$";
            var match = Regex.Match(context.Trim(), versePattern, RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int surah) && 
                    int.TryParse(match.Groups[2].Value, out int verse))
                {
                    // Validate surah and verse numbers
                    if (surah >= 1 && surah <= 114 && verse >= 1)
                    {
                        return new VerseReference(surah, verse);
                    }
                }
            }

            // Try to match simple "X:Y" pattern
            var simplePattern = @"^(\d+):(\d+)$";
            match = Regex.Match(context.Trim(), simplePattern);
            
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int surah) && 
                    int.TryParse(match.Groups[2].Value, out int verse))
                {
                    // Validate surah and verse numbers
                    if (surah >= 1 && surah <= 114 && verse >= 1)
                    {
                        return new VerseReference(surah, verse);
                    }
                }
            }

            return null;
        }
    }
}
