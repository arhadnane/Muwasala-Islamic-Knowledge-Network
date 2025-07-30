# PowerShell script to run the Muwasala Web Application
Write-Host "Starting Muwasala Web Application..." -ForegroundColor Green

# Navigate to the web project directory
Set-Location "d:\Data Perso Adnane\Coding\VSCodeProject\Muwasala Islamic Knowledge Network V2\Muwasala-Islamic-Knowledge-Network\src\Muwasala.Web"

# Check if the project file exists
if (Test-Path "Muwasala.Web.csproj") {
    Write-Host "Found project file. Building and running..." -ForegroundColor Yellow
    
    # Build the project first
    Write-Host "Building project..." -ForegroundColor Cyan
    dotnet build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Build successful. Starting web server..." -ForegroundColor Green
        
        # Run the project
        Write-Host "Starting web application. Press Ctrl+C to stop." -ForegroundColor Yellow
        Write-Host "The application will be available at:" -ForegroundColor Cyan
        Write-Host "  - https://localhost:5001" -ForegroundColor White
        Write-Host "  - http://localhost:5000" -ForegroundColor White
        Write-Host ""
        
        dotnet run
    } else {
        Write-Host "Build failed. Please check the error messages above." -ForegroundColor Red
    }
} else {
    Write-Host "Project file not found in current directory." -ForegroundColor Red
    Write-Host "Current directory: $(Get-Location)" -ForegroundColor Yellow
}
