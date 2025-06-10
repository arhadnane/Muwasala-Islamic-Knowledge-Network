Write-Host "Muwasala API Test" -ForegroundColor Green
Write-Host "=================" -ForegroundColor Green

$apiBase = "http://localhost:5000"

Write-Host "Testing Health endpoint..." -ForegroundColor Cyan
try {
    $health = Invoke-RestMethod -Uri "$apiBase/health" -Method GET
    Write-Host "SUCCESS: Health endpoint working" -ForegroundColor Green
    Write-Host $($health | ConvertTo-Json) -ForegroundColor White
} catch {
    Write-Host "ERROR: Health endpoint failed - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTesting API root..." -ForegroundColor Cyan
try {
    $root = Invoke-RestMethod -Uri "$apiBase/" -Method GET
    Write-Host "SUCCESS: Root endpoint working" -ForegroundColor Green
    Write-Host $($root | ConvertTo-Json) -ForegroundColor White
} catch {
    Write-Host "ERROR: Root endpoint failed - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTesting Quran theme: Allah..." -ForegroundColor Cyan
try {
    $quran = Invoke-RestMethod -Uri "$apiBase/api/Quran/theme/Allah" -Method GET
    Write-Host "SUCCESS: Quran theme endpoint working" -ForegroundColor Green
    Write-Host "Results count: $($quran.Count)" -ForegroundColor Yellow
    if ($quran.Count -gt 0) {
        Write-Host "Sample: $($quran[0] | ConvertTo-Json -Compress)" -ForegroundColor White
    }
} catch {
    Write-Host "ERROR: Quran theme failed - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTesting Hadith topic: prayer..." -ForegroundColor Cyan
try {
    $hadith = Invoke-RestMethod -Uri "$apiBase/api/Hadith/topic/prayer" -Method GET
    Write-Host "SUCCESS: Hadith topic endpoint working" -ForegroundColor Green
    Write-Host "Results count: $($hadith.Count)" -ForegroundColor Yellow
    if ($hadith.Count -gt 0) {
        Write-Host "Sample: $($hadith[0] | ConvertTo-Json -Compress)" -ForegroundColor White
    }
} catch {
    Write-Host "ERROR: Hadith topic failed - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTesting Dua daily..." -ForegroundColor Cyan
try {
    $dua = Invoke-RestMethod -Uri "$apiBase/api/Dua/daily" -Method GET
    Write-Host "SUCCESS: Dua daily endpoint working" -ForegroundColor Green
    Write-Host "Results count: $($dua.Count)" -ForegroundColor Yellow
    if ($dua.Count -gt 0) {
        Write-Host "Sample: $($dua[0] | ConvertTo-Json -Compress)" -ForegroundColor White
    }
} catch {
    Write-Host "ERROR: Dua daily failed - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTesting POST Quran guidance..." -ForegroundColor Cyan
try {
    $body = @{ Context = "prayer"; Language = "en" } | ConvertTo-Json
    $guidance = Invoke-RestMethod -Uri "$apiBase/api/Quran/guidance" -Method POST -Body $body -ContentType "application/json"
    Write-Host "SUCCESS: Quran guidance endpoint working" -ForegroundColor Green
    Write-Host $($guidance | ConvertTo-Json -Compress) -ForegroundColor White
} catch {
    Write-Host "ERROR: Quran guidance failed - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nAPI Test completed!" -ForegroundColor Green
