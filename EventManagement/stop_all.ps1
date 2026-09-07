Write-Host "Stopping all local dotnet processes..." -ForegroundColor Yellow

Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force

Write-Host "All dotnet services stopped successfully!" -ForegroundColor Green
