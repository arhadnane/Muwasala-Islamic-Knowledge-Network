# AI Enhanced Search Functionality Test Script
# Tests the AI Enhanced search through direct API calls

$baseUrl = "http://localhost:5237"
$testQueries = @(
    "What are the five pillars of Islam?",
    "Explain the importance of prayer in Islam",
    "What does the Quran say about charity?",
    "Tell me about the Prophet Muhammad's character"
)

Write-Host "🤖 Testing AI Enhanced Search Functionality" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

foreach ($query in $testQueries) {
    Write-Host "`n🔍 Testing Query: '$query'" -ForegroundColor Cyan
    Write-Host "---" -ForegroundColor Gray
    
    try {
        # Create the search request body
        $body = @{
            query = $query
            searchMode = "ai"
            includeQuran = $true
            includeHadith = $true
            includeFiqh = $true
        } | ConvertTo-Json
        
        # Make the API call
        $response = Invoke-RestMethod -Uri "$baseUrl/api/search" -Method Post -Body $body -ContentType "application/json" -TimeoutSec 60
        
        Write-Host "✅ Response received:" -ForegroundColor Green
        Write-Host $response -ForegroundColor White
        Write-Host "`n" -ForegroundColor Gray
        
    } catch {
        Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Start-Sleep -Seconds 2
}

Write-Host "`n🎯 Test completed!" -ForegroundColor Green
