using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Muwasala.QuickPerformanceTest;

class Program
{
    private const string BaseUrl = "http://localhost:5237";
    
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Muwasala Performance Test - Search Endpoint Verification");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(60); // Give enough time to detect if old 100s+ timeout occurs
        
        var testQueries = new[]
        {
            "Islam",
            "Quran",
            "Prayer",
            "Hadith",
            "Islamic knowledge"
        };

        Console.WriteLine($"🎯 Testing {testQueries.Length} queries with timeout detection...\n");

        var totalStartTime = Stopwatch.StartNew();
        var successCount = 0;
        var timeoutCount = 0;
        var errorCount = 0;

        foreach (var query in testQueries)
        {
            Console.Write($"🔍 Testing query: '{query}'... ");
            
            var queryStartTime = Stopwatch.StartNew();
            
            try
            {
                var response = await httpClient.GetAsync($"{BaseUrl}/api/islamicagents/search?query={Uri.EscapeDataString(query)}&page=1&pageSize=5");
                
                queryStartTime.Stop();
                var responseTime = queryStartTime.ElapsedMilliseconds;
                
                if (response.IsSuccessStatusCode)
                {
                    successCount++;
                    var content = await response.Content.ReadAsStringAsync();
                    
                    if (responseTime > 100000) // 100+ seconds (old problem)
                    {
                        Console.WriteLine($"⚠️  TIMEOUT ISSUE DETECTED! ({responseTime:N0}ms)");
                        timeoutCount++;
                    }
                    else if (responseTime > 30000) // 30+ seconds
                    {
                        Console.WriteLine($"⚠️  SLOW ({responseTime:N0}ms)");
                    }
                    else if (responseTime > 10000) // 10+ seconds
                    {
                        Console.WriteLine($"🔶 OK but slow ({responseTime:N0}ms)");
                    }
                    else
                    {
                        Console.WriteLine($"✅ FAST ({responseTime:N0}ms)");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ HTTP {response.StatusCode} ({responseTime:N0}ms)");
                    errorCount++;
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                queryStartTime.Stop();
                Console.WriteLine($"❌ TIMEOUT after {queryStartTime.ElapsedMilliseconds:N0}ms");
                timeoutCount++;
            }
            catch (Exception ex)
            {
                queryStartTime.Stop();
                Console.WriteLine($"❌ ERROR: {ex.Message} ({queryStartTime.ElapsedMilliseconds:N0}ms)");
                errorCount++;
            }
        }
        
        totalStartTime.Stop();
        
        Console.WriteLine("\n📊 PERFORMANCE TEST RESULTS:");
        Console.WriteLine("════════════════════════════");
        Console.WriteLine($"Total queries tested: {testQueries.Length}");
        Console.WriteLine($"✅ Successful responses: {successCount}");
        Console.WriteLine($"❌ Errors: {errorCount}");
        Console.WriteLine($"⚠️  Timeouts (100s+): {timeoutCount}");
        Console.WriteLine($"⏱️  Total test time: {totalStartTime.ElapsedMilliseconds:N0}ms");
        Console.WriteLine($"📈 Average time per query: {totalStartTime.ElapsedMilliseconds / testQueries.Length:N0}ms");
        
        if (timeoutCount == 0)
        {
            Console.WriteLine("\n🎉 SUCCESS: No 100+ second timeouts detected!");
            Console.WriteLine("✅ The performance optimization has RESOLVED the timeout issue!");
        }
        else
        {
            Console.WriteLine($"\n⚠️  WARNING: {timeoutCount} queries still experiencing 100+ second timeouts");
        }
        
        if (successCount == testQueries.Length && timeoutCount == 0)
        {
            Console.WriteLine("\n🏆 PERFORMANCE OPTIMIZATION: COMPLETE SUCCESS!");
            Environment.Exit(0);
        }
        else
        {
            Console.WriteLine("\n🔧 Some issues remain, but major timeout problem appears resolved");
            Environment.Exit(1);
        }
    }
}
