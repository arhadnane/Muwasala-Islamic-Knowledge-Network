# API Endpoints Test Script for Muwasala Islamic Knowledge Network
Write-Host "🕌 Muwasala API Endpoints Test" -ForegroundColor Green
Write-Host "===============================" -ForegroundColor Green

$apiBase = "http://localhost:5000"

function Test-Endpoint {
    param([string]$Url, [string]$Description)
    
    Write-Host "`nTesting: $Description" -ForegroundColor Cyan
    Write-Host "URL: $Url" -ForegroundColor Gray
    
    try {
        $response = Invoke-RestMethod -Uri $Url -Method GET -TimeoutSec 10
        Write-Host "✅ SUCCESS" -ForegroundColor Green
        if ($response) {
            if ($response -is [array]) {
                Write-Host "📊 Response: Array with $($response.Count) items" -ForegroundColor Yellow
                if ($response.Count -gt 0) {
                    Write-Host "Sample: $($response[0] | ConvertTo-Json -Compress)" -ForegroundColor White
                }
            } else {
                Write-Host "📊 Response: $($response | ConvertTo-Json -Compress)" -ForegroundColor White
            }
        }
    } catch {
        if ($_.Exception.Response.StatusCode -eq 500) {
            Write-Host "❌ INTERNAL SERVER ERROR (500)" -ForegroundColor Red
        } elseif ($_.Exception.Response.StatusCode -eq 404) {
            Write-Host "❌ NOT FOUND (404)" -ForegroundColor Red
        } else {
            Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

function Test-PostEndpoint {
    param([string]$Url, [string]$Description, [hashtable]$Body)
    
    Write-Host "`nTesting: $Description" -ForegroundColor Cyan
    Write-Host "URL: $Url" -ForegroundColor Gray
    
    try {
        $response = Invoke-RestMethod -Uri $Url -Method POST -Body ($Body | ConvertTo-Json) -ContentType "application/json" -TimeoutSec 10
        Write-Host "✅ SUCCESS" -ForegroundColor Green
        Write-Host "📊 Response: $($response | ConvertTo-Json -Compress)" -ForegroundColor White
    } catch {
        if ($_.Exception.Response.StatusCode -eq 500) {
            Write-Host "❌ INTERNAL SERVER ERROR (500)" -ForegroundColor Red
        } elseif ($_.Exception.Response.StatusCode -eq 404) {
            Write-Host "❌ NOT FOUND (404)" -ForegroundColor Red
        } else {
            Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

# Basic endpoints
Write-Host "`n🔍 BASIC ENDPOINTS" -ForegroundColor Magenta
Test-Endpoint "$apiBase/health" "Health Check"
Test-Endpoint "$apiBase/" "API Information"

# Quran endpoints
Write-Host "`n📖 QURAN ENDPOINTS" -ForegroundColor Magenta
Test-Endpoint "$apiBase/api/Quran/theme/Allah" "Quran Theme: Allah"
Test-Endpoint "$apiBase/api/Quran/theme/mercy" "Quran Theme: Mercy"
Test-PostEndpoint "$apiBase/api/Quran/guidance" "Quran Guidance" @{ Context = "prayer"; Language = "en" }

# Hadith endpoints
Write-Host "`n📚 HADITH ENDPOINTS" -ForegroundColor Magenta
Test-Endpoint "$apiBase/api/Hadith/topic/prayer" "Hadith Topic: Prayer"
Test-PostEndpoint "$apiBase/api/Hadith/verify" "Hadith Verification" @{ HadithText = "Actions are by intentions"; Language = "en" }

# Other endpoints
Write-Host "`n⚖️ FIQH ENDPOINTS" -ForegroundColor Magenta
Test-PostEndpoint "$apiBase/api/Fiqh/ruling" "Fiqh Ruling" @{ Question = "Can I pray in shoes?"; Madhab = "Hanafi"; Language = "en" }

Write-Host "`n🤲 DUA ENDPOINTS" -ForegroundColor Magenta
Test-Endpoint "$apiBase/api/Dua/occasion/morning" "Morning Dua"
Test-Endpoint "$apiBase/api/Dua/daily" "Daily Duas"

Write-Host "`n🎵 TAJWEED ENDPOINTS" -ForegroundColor Magenta
Test-Endpoint "$apiBase/api/Tajweed/analyze/1/1" "Tajweed Analysis"
Test-PostEndpoint "$apiBase/api/Tajweed/pronunciation" "Pronunciation" @{ ArabicWord = "Allah"; Language = "en" }

Write-Host "`n📜 SIRAH ENDPOINTS" -ForegroundColor Magenta
Test-Endpoint "$apiBase/api/Sirah/timeline" "Sirah Timeline"
Test-PostEndpoint "$apiBase/api/Sirah/guidance" "Sirah Guidance" @{ Situation = "hardship"; Language = "en" }

Write-Host "`nTest completed!" -ForegroundColor Green
