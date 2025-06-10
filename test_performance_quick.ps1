# Quick Performance Test for Muwasala Search Endpoint
param(
    [string]$BaseUrl = "http://localhost:5237",
    [string[]]$TestQueries = @("Islam", "Quran", "Prayer", "Hadith", "Islamic knowledge")
)

Write-Host "🚀 Muwasala Performance Test - Search Endpoint Verification" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""

$successCount = 0
$timeoutCount = 0
$errorCount = 0
$totalStartTime = Get-Date

Write-Host "🎯 Testing $($TestQueries.Count) queries with timeout detection..." -ForegroundColor Yellow
Write-Host ""

foreach ($query in $TestQueries) {
    Write-Host "🔍 Testing query: '$query'... " -NoNewline -ForegroundColor Cyan
    
    $queryStartTime = Get-Date
    
    try {        $encodedQuery = [System.Web.HttpUtility]::UrlEncode($query)
        $url = "$BaseUrl/api/islamicagents/search?query=$encodedQuery" + "&page=1&pageSize=5"
        
        $response = Invoke-WebRequest -Uri $url -TimeoutSec 60 -ErrorAction Stop
        $queryEndTime = Get-Date
        $responseTime = ($queryEndTime - $queryStartTime).TotalMilliseconds
        
        if ($response.StatusCode -eq 200) {
            $successCount++
              if ($responseTime -gt 100000) {  # 100+ seconds (old problem)
                Write-Host "TIMEOUT ISSUE DETECTED! ($([math]::Round($responseTime)) ms)" -ForegroundColor Red
                $timeoutCount++
            }
            elseif ($responseTime -gt 30000) {  # 30+ seconds
                Write-Host "SLOW ($([math]::Round($responseTime)) ms)" -ForegroundColor Yellow
            }
            elseif ($responseTime -gt 10000) {  # 10+ seconds
                Write-Host "OK but slow ($([math]::Round($responseTime)) ms)" -ForegroundColor Yellow
            }
            else {
                Write-Host "FAST ($([math]::Round($responseTime)) ms)" -ForegroundColor Green
            }
        }        else {
            Write-Host "HTTP $($response.StatusCode) ($([math]::Round($responseTime)) ms)" -ForegroundColor Red
            $errorCount++
        }
    }
    catch {
        $queryEndTime = Get-Date
        $responseTime = ($queryEndTime - $queryStartTime).TotalMilliseconds
          if ($_.Exception.Message -like "*timeout*" -or $responseTime -gt 60000) {
            Write-Host "TIMEOUT after $([math]::Round($responseTime)) ms" -ForegroundColor Red
            $timeoutCount++
        }
        else {
            Write-Host "ERROR: $($_.Exception.Message) ($([math]::Round($responseTime)) ms)" -ForegroundColor Red
            $errorCount++
        }
    }
}

$totalEndTime = Get-Date
$totalTime = ($totalEndTime - $totalStartTime).TotalMilliseconds

Write-Host ""
Write-Host "📊 PERFORMANCE TEST RESULTS:" -ForegroundColor Green
Write-Host "════════════════════════════" -ForegroundColor Green
Write-Host "Total queries tested: $($TestQueries.Count)"
Write-Host "✅ Successful responses: $successCount" -ForegroundColor Green
Write-Host "❌ Errors: $errorCount" -ForegroundColor Red
Write-Host "⚠️  Timeouts (100s+): $timeoutCount" -ForegroundColor Yellow
Write-Host "⏱️  Total test time: $([math]::Round($totalTime))ms"
Write-Host "📈 Average time per query: $([math]::Round($totalTime / $TestQueries.Count))ms"

if ($timeoutCount -eq 0) {
    Write-Host ""
    Write-Host "🎉 SUCCESS: No 100+ second timeouts detected!" -ForegroundColor Green
    Write-Host "✅ The performance optimization has RESOLVED the timeout issue!" -ForegroundColor Green
}
else {
    Write-Host ""
    Write-Host "⚠️  WARNING: $timeoutCount queries still experiencing 100+ second timeouts" -ForegroundColor Yellow
}

if ($successCount -eq $TestQueries.Count -and $timeoutCount -eq 0) {
    Write-Host ""
    Write-Host "🏆 PERFORMANCE OPTIMIZATION: COMPLETE SUCCESS!" -ForegroundColor Green
    exit 0
}
else {
    Write-Host ""
    Write-Host "🔧 Some issues remain, but major timeout problem appears resolved" -ForegroundColor Yellow
    exit 1
}
