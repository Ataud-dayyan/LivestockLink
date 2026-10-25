<#
.SYNOPSIS
  LivestockLink Nigeria - 1-Click Windows Startup Script
#>

Write-Host "==========================================================" -ForegroundColor Green
Write-Host "       LIVESTOCKLINK NIGERIA — MVP DEMONSTRATION          " -ForegroundColor Yellow
Write-Host "  B2B Livestock & Animal Protein Supply Brokerage Platform" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Green
Write-Host ""

# 1. Check .NET SDK
Write-Host "[1/4] Checking .NET SDK..." -ForegroundColor Cyan
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: .NET SDK not detected. Please install .NET 9.0 SDK." -ForegroundColor Red
    exit 1
}
Write-Host "Found .NET SDK version: $dotnetVersion" -ForegroundColor Green

# 2. Check PostgreSQL service
Write-Host ""
Write-Host "[2/4] Verifying PostgreSQL service..." -ForegroundColor Cyan
$pgService = Get-Service *postgres* -ErrorAction SilentlyContinue | Where-Object { $_.Status -eq 'Running' }
if ($pgService) {
    Write-Host "PostgreSQL service is running: $($pgService.Name)" -ForegroundColor Green
} else {
    Write-Host "Notice: PostgreSQL service not detected as running service. Ensure PostgreSQL is active on port 5432." -ForegroundColor Yellow
}

# 3. Run Automated Tests
Write-Host ""
Write-Host "[3/4] Running automated test suite..." -ForegroundColor Cyan
dotnet test --nologo -v q
if ($LASTEXITCODE -ne 0) {
    Write-Host "Automated tests encountered an error. Please inspect test results." -ForegroundColor Red
} else {
    Write-Host "All automated tests passed successfully!" -ForegroundColor Green
}

# 4. Start LivestockLink Application
Write-Host ""
Write-Host "[4/4] Starting LivestockLink Web API & Marketplace Portal..." -ForegroundColor Cyan
Write-Host "Application starting at: http://localhost:5000" -ForegroundColor Yellow
Write-Host "API Documentation at:    http://localhost:5000/swagger" -ForegroundColor Yellow
Write-Host ""
Write-Host "Demo Credentials:" -ForegroundColor Green
Write-Host " - Admin:              admin@livestocklink.ng / Password123!" -ForegroundColor White
Write-Host " - Restaurant (Lagos): mamacass@livestocklink.ng / Password123!" -ForegroundColor White
Write-Host " - Restaurant (Abuja): calabarkitchen@livestocklink.ng / Password123!" -ForegroundColor White
Write-Host " - Supplier (Poultry): albarakah@livestocklink.ng / Password123!" -ForegroundColor White
Write-Host " - Supplier (Catfish): nigervalley@livestocklink.ng / Password123!" -ForegroundColor White
Write-Host " - Supplier (Beef):    plateauprime@livestocklink.ng / Password123!" -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C to stop the application." -ForegroundColor Gray
Write-Host "==========================================================" -ForegroundColor Green

# Launch browser after 2 seconds
Start-Job -ScriptBlock {
    Start-Sleep -Seconds 2
    Start-Process "http://localhost:5000"
} | Out-Null

dotnet run --project src/LivestockLink.Web --urls "http://localhost:5000"
