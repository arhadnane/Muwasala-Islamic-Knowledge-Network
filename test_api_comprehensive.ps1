# Comprehensive API Test Script for Muwasala Islamic Knowledge Network
# Tests all available API endpoints to verify functionality

Write-Host "🕌 Muwasala API Comprehensive Test" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""

$apiBase = "http://localhost:5000"

# Function to test GET endpoint
function Test-GetEndpoint {
    param(
        [string]$Url,
        [string]$Description
    )
    
    Write-Host "Testing: $Description" -ForegroundColor Cyan
    Write-Host "URL: $Url" -ForegroundColor Gray
    
    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 10
        $statusCode = $response.StatusCode
        $contentLength = $response.Content.Length
          if ($statusCode -eq 200) {
            Write-Host "✅ SUCCESS - Status: $statusCode, Content Length: $contentLength" -ForegroundColor Green
            
            # Parse JSON and show sample data
            if ($response.Content -and $response.Content.Length -gt 2) {
                try {
                    $json = $response.Content | ConvertFrom-Json
                    if ($json -is [array] -and $json.Count -gt 0) {
                        Write-Host "📊 Sample Data (first item):" -ForegroundColor Yellow
                        Write-Host ($json[0] | ConvertTo-Json -Depth 2) -ForegroundColor White
                    } elseif ($json -and $json -isnot [array]) {
                        Write-Host "📊 Response Data:" -ForegroundColor Yellow
                        Write-Host ($json | ConvertTo-Json -Depth 2) -ForegroundColor White
                    } else {
                        Write-Host "📊 Empty response array" -ForegroundColor Yellow
                    }
                } catch {
                    Write-Host "📊 Raw response: $($response.Content.Substring(0, [Math]::Min(200, $response.Content.Length)))" -ForegroundColor White
                }
            }
        } else {
            Write-Host "⚠️  UNEXPECTED STATUS: $statusCode" -ForegroundColor Yellow
        }
    } catch {
        $errorMessage = $_.Exception.Message
        if ($errorMessage -like "*500*") {
            Write-Host "❌ INTERNAL SERVER ERROR (500)" -ForegroundColor Red
        } elseif ($errorMessage -like "*404*") {
            Write-Host "❌ NOT FOUND (404)" -ForegroundColor Red
        } else {
            Write-Host "❌ ERROR: $errorMessage" -ForegroundColor Red
        }
    }
    Write-Host ""
}

# Function to test POST endpoint
function Test-PostEndpoint {
    param(
        [string]$Url,
        [string]$Description,
        [hashtable]$Body
    )
    
    Write-Host "Testing: $Description" -ForegroundColor Cyan
    Write-Host "URL: $Url" -ForegroundColor Gray
    Write-Host "Body: $($Body | ConvertTo-Json -Compress)" -ForegroundColor Gray
    
    try {
        $jsonBody = $Body | ConvertTo-Json
        $response = Invoke-WebRequest -Uri $Url -Method POST -Body $jsonBody -ContentType "application/json" -UseBasicParsing -TimeoutSec 10
        $statusCode = $response.StatusCode
        $contentLength = $response.Content.Length
        
        if ($statusCode -eq 200) {
            Write-Host "✅ SUCCESS - Status: $statusCode, Content Length: $contentLength" -ForegroundColor Green
            
            # Parse JSON and show sample data
            if ($response.Content -and $response.Content.Length -gt 2) {
                try {
                    $json = $response.Content | ConvertFrom-Json
                    Write-Host "📊 Response Data:" -ForegroundColor Yellow
                    Write-Host ($json | ConvertTo-Json -Depth 2) -ForegroundColor White
                } catch {
                    Write-Host "📊 Raw response: $($response.Content.Substring(0, [Math]::Min(200, $response.Content.Length)))" -ForegroundColor White
                }
            }
        } else {
            Write-Host "⚠️  UNEXPECTED STATUS: $statusCode" -ForegroundColor Yellow
        }
    } catch {
        $errorMessage = $_.Exception.Message
        if ($errorMessage -like "*500*") {
            Write-Host "❌ INTERNAL SERVER ERROR (500)" -ForegroundColor Red
        } elseif ($errorMessage -like "*404*") {
            Write-Host "❌ NOT FOUND (404)" -ForegroundColor Red
        } else {
            Write-Host "❌ ERROR: $errorMessage" -ForegroundColor Red
        }
    }
    Write-Host ""
}

# Test basic health and info endpoints
Write-Host "🔍 BASIC ENDPOINTS" -ForegroundColor Magenta
Write-Host "==================" -ForegroundColor Magenta
Test-GetEndpoint "$apiBase/health" "Health Check"
Test-GetEndpoint "$apiBase/" "API Information"

# Test Quran endpoints
Write-Host "📖 QURAN ENDPOINTS" -ForegroundColor Magenta
Write-Host "==================" -ForegroundColor Magenta
Test-GetEndpoint "$apiBase/api/Quran/theme/Allah" "Quran Theme: Allah"
Test-GetEndpoint "$apiBase/api/Quran/theme/mercy" "Quran Theme: Mercy"
Test-GetEndpoint "$apiBase/api/Quran/theme/guidance" "Quran Theme: Guidance"
Test-GetEndpoint "$apiBase/api/Quran/theme/peace" "Quran Theme: Peace"
Test-PostEndpoint "$apiBase/api/Quran/guidance" "Quran Guidance Request" @{ Context = "Allah"; Language = "en" }

# Test Hadith endpoints  
Write-Host "📚 HADITH ENDPOINTS" -ForegroundColor Magenta
Write-Host "===================" -ForegroundColor Magenta
Test-GetEndpoint "$apiBase/api/Hadith/topic/prayer" "Hadith Topic: Prayer"
Test-GetEndpoint "$apiBase/api/Hadith/topic/charity" "Hadith Topic: Charity"
Test-PostEndpoint "$apiBase/api/Hadith/verify" "Hadith Verification" @{ HadithText = "Actions are by intentions"; Language = "en" }

# Test Fiqh endpoints
Write-Host "⚖️ FIQH ENDPOINTS" -ForegroundColor Magenta
Write-Host "=================" -ForegroundColor Magenta
Test-PostEndpoint "$apiBase/api/Fiqh/ruling" "Fiqh Ruling Request" @{ Question = "Is it permissible to pray in shoes?"; Madhab = "Hanafi"; Language = "en" }

# Test Dua endpoints
Write-Host "🤲 DUA ENDPOINTS" -ForegroundColor Magenta
Write-Host "================" -ForegroundColor Magenta
Test-GetEndpoint "$apiBase/api/Dua/occasion/morning" "Dua for Morning"
Test-GetEndpoint "$apiBase/api/Dua/daily" "Daily Dua Schedule"
Test-GetEndpoint "$apiBase/api/Dua/specific/Istikhara" "Specific Prayer: Istikhara"

# Test Tajweed endpoints
Write-Host "🎵 TAJWEED ENDPOINTS" -ForegroundColor Magenta
Write-Host "====================" -ForegroundColor Magenta
Test-GetEndpoint "$apiBase/api/Tajweed/analyze/1/1" "Tajweed Analysis: Surah 1, Verse 1"
Test-PostEndpoint "$apiBase/api/Tajweed/pronunciation" "Pronunciation Guide" @{ ArabicWord = "الله"; Language = "en" }

# Test Sirah endpoints
Write-Host "📜 SIRAH ENDPOINTS" -ForegroundColor Magenta
Write-Host "==================" -ForegroundColor Magenta
Test-GetEndpoint "$apiBase/api/Sirah/timeline" "Sirah Timeline"
Test-GetEndpoint "$apiBase/api/Sirah/character/patience" "Character Aspect: Patience"
Test-PostEndpoint "$apiBase/api/Sirah/guidance" "Sirah Guidance Request" @{ Situation = "dealing with hardship"; Language = "en" }

Write-Host "🏁 Test completed!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Summary:" -ForegroundColor Yellow
Write-Host "- API is running on: $apiBase" -ForegroundColor White
Write-Host "- Web interface is running on: http://localhost:5237" -ForegroundColor White
Write-Host "- Check the results above to see which endpoints are working" -ForegroundColor White
Write-Host ""
Write-Host "💡 If you see empty arrays [], it means the endpoint works but no data matches the search terms." -ForegroundColor Cyan
Write-Host "💡 If you see 500 errors, there might be issues with the AI agents or database connections." -ForegroundColor Cyan
