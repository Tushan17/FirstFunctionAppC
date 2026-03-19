# Diagnostic script to check if Azure Function is running
# Usage: Run this script to verify the function is accessible

$functionPort = 7222
$functionUrl = "http://localhost:$functionPort"
$uploadEndpoint = "$functionUrl/api/UploadXml"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Azure Function Diagnostics" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check 1: Is anything running on the port?
Write-Host "Check 1: Testing port $functionPort..." -ForegroundColor Yellow
try {
    $tcpConnection = Test-NetConnection -ComputerName localhost -Port $functionPort -WarningAction SilentlyContinue
    if ($tcpConnection.TcpTestSucceeded) {
        Write-Host "✓ Port $functionPort is OPEN and accessible" -ForegroundColor Green
    } else {
        Write-Host "✗ Port $functionPort is CLOSED" -ForegroundColor Red
        Write-Host "  → Make sure to start the Azure Function (F5 in Visual Studio)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Unable to test port" -ForegroundColor Red
    Write-Host "  Error: $_" -ForegroundColor Yellow
}

Write-Host ""

# Check 2: Can we reach the base URL?
Write-Host "Check 2: Testing base URL ($functionUrl)..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $functionUrl -Method Get -TimeoutSec 3 -ErrorAction Stop
    Write-Host "✓ Azure Function is responding" -ForegroundColor Green
    Write-Host "  Status: $($response.StatusCode)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Cannot reach Azure Function" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""

# Check 3: Can we reach the UploadXml endpoint?
Write-Host "Check 3: Testing UploadXml endpoint..." -ForegroundColor Yellow
try {
    # Try a GET request (should fail but confirms endpoint exists)
    $response = Invoke-WebRequest -Uri $uploadEndpoint -Method Get -TimeoutSec 3 -ErrorAction Stop
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 404) {
        Write-Host "✗ Endpoint not found (404)" -ForegroundColor Red
        Write-Host "  → The UploadXml function might not be loaded" -ForegroundColor Yellow
    } elseif ($_.Exception.Response.StatusCode.value__ -eq 401) {
        Write-Host "✓ Endpoint exists but requires authentication (401)" -ForegroundColor Green
        Write-Host "  → This is expected - the endpoint is working" -ForegroundColor Cyan
    } else {
        Write-Host "✓ Endpoint is accessible" -ForegroundColor Green
        Write-Host "  Response: $($_.Exception.Response.StatusCode)" -ForegroundColor Cyan
    }
}

Write-Host ""

# Check 4: List processes using the port
Write-Host "Check 4: Checking what's using port $functionPort..." -ForegroundColor Yellow
try {
    $netstat = netstat -ano | Select-String ":$functionPort"
    if ($netstat) {
        Write-Host "✓ Process found using port $functionPort" -ForegroundColor Green
        $netstat | ForEach-Object { Write-Host "  $_" -ForegroundColor Cyan }
    } else {
        Write-Host "✗ No process is using port $functionPort" -ForegroundColor Red
        Write-Host "  → The Azure Function is NOT running" -ForegroundColor Yellow
    }
} catch {
    Write-Host "! Unable to check port usage" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Diagnostic Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "If all checks passed:" -ForegroundColor Yellow
Write-Host "  → Your function is running correctly" -ForegroundColor White
Write-Host "  → Try the Postman request again" -ForegroundColor White
Write-Host ""
Write-Host "If checks failed:" -ForegroundColor Yellow
Write-Host "  1. Open Visual Studio" -ForegroundColor White
Write-Host "  2. Press F5 to start debugging" -ForegroundColor White
Write-Host "  3. Wait for console window showing 'Functions: UploadXml...'" -ForegroundColor White
Write-Host "  4. Run this diagnostic script again" -ForegroundColor White
Write-Host ""
Write-Host "Alternative ports to try if 7222 is in use:" -ForegroundColor Yellow
Write-Host "  → 7071 (default Azure Functions port)" -ForegroundColor White
Write-Host "  → 7223, 7224, etc." -ForegroundColor White
Write-Host ""
