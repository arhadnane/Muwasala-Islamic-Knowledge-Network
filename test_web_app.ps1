# Test the web application search functionality
Write-Host "Testing Muwasala Search Functionality" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

# Test endpoints
$baseUrl = "https://localhost:7002"

# Test if web app is running
try {
    Write-Host "`nTesting if web application is accessible..." -ForegroundColor Yellow
    $response = Invoke-WebRequest -Uri $baseUrl -UseBasicParsing -SkipCertificateCheck
    Write-Host "✓ Web application is running" -ForegroundColor Green
} catch {
    Write-Host "✗ Web application is not accessible: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test different search pages
$pages = @(
    "/",
    "/search", 
    "/globalsearch",
    "/quran",
    "/hadith"
)

Write-Host "`nTesting page accessibility..." -ForegroundColor Yellow
foreach ($page in $pages) {
    try {
        $url = "$baseUrl$page"
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing -SkipCertificateCheck
        Write-Host "✓ $page - Status: $($response.StatusCode)" -ForegroundColor Green
    } catch {
        Write-Host "✗ $page - Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n" -ForegroundColor Green
Write-Host "Open the following URLs in your browser to test search functionality:" -ForegroundColor Cyan
Write-Host "• Global Search: $baseUrl/globalsearch" -ForegroundColor White
Write-Host "• Regular Search: $baseUrl/search" -ForegroundColor White
Write-Host "• Quran Search: $baseUrl/quran" -ForegroundColor White
Write-Host "• Hadith Search: $baseUrl/hadith" -ForegroundColor White

Write-Host "`nTesting completed!" -ForegroundColor Green
