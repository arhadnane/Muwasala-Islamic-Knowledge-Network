# PowerShell script to run the Muwasala API
Write-Host "Starting Muwasala API..." -ForegroundColor Green

# Navigate to the API project directory
Set-Location "d:\Data Perso Adnane\Coding\VSCodeProject\Muwasala Islamic Knowledge Network V2\Muwasala-Islamic-Knowledge-Network\src\Muwasala.Api"

# Check if the project file exists
if (Test-Path "Muwasala.Api.csproj") {
    Write-Host "Found API project file. Building and running..." -ForegroundColor Yellow
    
    # Build the project first
    Write-Host "Building API project..." -ForegroundColor Cyan
    dotnet build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Build successful. Starting API server..." -ForegroundColor Green
        
        # Run the project
        Write-Host "Starting API. Press Ctrl+C to stop." -ForegroundColor Yellow
        Write-Host "The API will be available at:" -ForegroundColor Cyan
        Write-Host "  - https://localhost:5001/swagger" -ForegroundColor White
        Write-Host "  - http://localhost:5000/swagger" -ForegroundColor White
        Write-Host ""
        
        dotnet run
    } else {
        Write-Host "Build failed. Please check the error messages above." -ForegroundColor Red
    }
} else {
    Write-Host "API project file not found in current directory." -ForegroundColor Red
    Write-Host "Current directory: $(Get-Location)" -ForegroundColor Yellow
}
