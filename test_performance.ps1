# Script de Test de Performance - Muwasala Islamic Knowledge Network
# Usage: .\test_performance.ps1

Write-Host "🚀 Lancement des tests de performance..." -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

# Vérifier si l'application est en cours d'exécution
$processRunning = Get-NetTCPConnection -LocalPort 5237 -ErrorAction SilentlyContinue
if (-not $processRunning) {
    Write-Host "❌ L'application n'est pas en cours d'exécution sur le port 5237" -ForegroundColor Red
    Write-Host "   Démarrez l'application avec: dotnet run --project src/Muwasala.Web" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Application détectée sur le port 5237" -ForegroundColor Green

# Naviguer vers le dossier des tests de performance
$testPath = "tests\Muwasala.PerformanceTests"
if (-not (Test-Path $testPath)) {
    Write-Host "❌ Dossier des tests de performance non trouvé: $testPath" -ForegroundColor Red
    exit 1
}

Set-Location $testPath

try {
    # Compiler le projet de test
    Write-Host "🔨 Compilation du projet de test..." -ForegroundColor Yellow
    dotnet build --verbosity quiet
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Échec de la compilation" -ForegroundColor Red
        exit 1
    }

    Write-Host "✅ Compilation réussie" -ForegroundColor Green

    # Exécuter les tests de performance
    Write-Host "`n🎯 Exécution des tests de performance..." -ForegroundColor Yellow
    dotnet run

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n🎉 Tests de performance terminés avec succès!" -ForegroundColor Green
        Write-Host "📊 Consultez les résultats ci-dessus pour analyser les performances" -ForegroundColor Cyan
    } else {
        Write-Host "`n❌ Les tests de performance ont échoué" -ForegroundColor Red
    }

} catch {
    Write-Host "❌ Erreur lors de l'exécution: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    # Retourner au répertoire racine
    Set-Location "..\..\"
}
