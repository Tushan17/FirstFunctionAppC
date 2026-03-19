# Test script for UploadXml Azure Function
# Usage: Run this script after starting the Azure Function locally (F5 in Visual Studio)

$functionUrl = "http://localhost:7222/api/UploadXml"
$sampleXmlPath = Join-Path $PSScriptRoot "..\SampleData\SampleData.xml"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing UploadXml Azure Function" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if the function is running
Write-Host "Checking if Azure Function is running..." -ForegroundColor Yellow
try {
    $null = Invoke-WebRequest -Uri "http://localhost:7222" -Method Get -TimeoutSec 2 -ErrorAction Stop
    Write-Host "✓ Azure Function is running" -ForegroundColor Green
} catch {
    Write-Host "✗ Azure Function is not running!" -ForegroundColor Red
    Write-Host "Please start the function by pressing F5 in Visual Studio" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Test 1: Valid XML
Write-Host "Test 1: Uploading valid XML..." -ForegroundColor Yellow

if (Test-Path $sampleXmlPath) {
    $xmlContent = Get-Content $sampleXmlPath -Raw
    
    $body = @{
        xmlContent = $xmlContent
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri $functionUrl `
            -Method Post `
            -Body $body `
            -ContentType "application/json"
        
        Write-Host "✓ Test 1 PASSED" -ForegroundColor Green
        Write-Host "Response:" -ForegroundColor Cyan
        $response | ConvertTo-Json -Depth 10
    } catch {
        Write-Host "✗ Test 1 FAILED" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
    }
} else {
    Write-Host "✗ Sample XML file not found at: $sampleXmlPath" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

# Test 2: Invalid XML (missing required element)
Write-Host "Test 2: Uploading invalid XML (missing required element)..." -ForegroundColor Yellow

$invalidXml = @"
<?xml version="1.0" encoding="utf-8"?>
<DataRoot xmlns="http://example.com/data" version="1.0">
  <Company>
    <CompanyId>1</CompanyId>
    <!-- Missing CompanyName -->
    <Address>
      <Street>123 Main</Street>
      <City>Seattle</City>
      <State>WA</State>
      <ZipCode>98101</ZipCode>
      <Country>USA</Country>
    </Address>
    <Employees></Employees>
  </Company>
</DataRoot>
"@

$body = @{
    xmlContent = $invalidXml
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $functionUrl `
        -Method Post `
        -Body $body `
        -ContentType "application/json"
    
    Write-Host "✗ Test 2 FAILED - Should have rejected invalid XML" -ForegroundColor Red
    $response | ConvertTo-Json -Depth 10
} catch {
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
    if ($errorResponse.success -eq $false) {
        Write-Host "✓ Test 2 PASSED - Invalid XML was correctly rejected" -ForegroundColor Green
        Write-Host "Validation Errors:" -ForegroundColor Cyan
        $errorResponse.validationErrors | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
    } else {
        Write-Host "✗ Test 2 FAILED" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

# Test 3: Empty request body
Write-Host "Test 3: Testing empty request body..." -ForegroundColor Yellow

$body = @{
    xmlContent = ""
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $functionUrl `
        -Method Post `
        -Body $body `
        -ContentType "application/json"
    
    Write-Host "✗ Test 3 FAILED - Should have rejected empty content" -ForegroundColor Red
} catch {
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
    if ($errorResponse.success -eq $false) {
        Write-Host "✓ Test 3 PASSED - Empty content was correctly rejected" -ForegroundColor Green
        Write-Host "Message: $($errorResponse.message)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "All tests completed!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
