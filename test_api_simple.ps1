# Simple API Test Script for Muwasala Islamic Knowledge Network
# Tests the search functionality through direct API calls

$baseUrl = "http://localhost:5237"
$testQuery = "What are the five pillars of Islam?"

Write-Host "Testing AI Enhanced Search Functionality" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

Write-Host "Testing Query: '$testQuery'" -ForegroundColor Cyan
Write-Host "---" -ForegroundColor Gray

try {
    # Create the search request body
    $body = @{
        query = $testQuery
        searchMode = "ai"
        includeQuran = $true
        includeHadith = $true
        includeFiqh = $true
    } | ConvertTo-Json
    
    Write-Host "Sending request to: $baseUrl/api/search" -ForegroundColor Yellow
    
    # Make the API call
    $response = Invoke-RestMethod -Uri "$baseUrl/api/search" -Method Post -Body $body -ContentType "application/json" -TimeoutSec 60
    
    Write-Host "Response received:" -ForegroundColor Green
    Write-Host $response -ForegroundColor White
    
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
}

Write-Host "Test completed!" -ForegroundColor Green
