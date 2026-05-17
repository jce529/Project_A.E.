# Unity Editor Setup Script for Phase 1
$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe"
$projectPath = (Get-Item .).FullName

Write-Host "Running Unity Setup for Phase 1: Build Assets & Place Monster..." -ForegroundColor Cyan

# Run Unity in batch mode to execute Phase1CLI.ExecuteAll
Start-Process -FilePath $unityPath -ArgumentList "-batchmode -nographics -projectPath `"$projectPath`" -executeMethod Phase1CLI.ExecuteAll -quit -logFile unity_setup.log" -Wait

if ($LASTEXITCODE -eq 0) {
    Write-Host "Success: Phase 1 Editor Setup complete." -ForegroundColor Green
} else {
    Write-Host "Error: Unity failed with exit code $LASTEXITCODE. Check unity_setup.log for details." -ForegroundColor Red
}
