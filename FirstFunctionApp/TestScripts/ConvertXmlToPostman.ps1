# Helper script to convert XML file to Postman-ready JSON format
# This will output JSON that you can copy/paste directly into Postman

param(
    [Parameter(Mandatory=$false)]
    [string]$XmlFilePath = (Join-Path $PSScriptRoot "..\SampleData\SampleData.xml"),
    
    [Parameter(Mandatory=$false)]
    [string]$OutputFile = (Join-Path $PSScriptRoot "PostmanRequest.json")
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "XML to Postman JSON Converter" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $XmlFilePath)) {
    Write-Host "✗ XML file not found: $XmlFilePath" -ForegroundColor Red
    exit 1
}

Write-Host "Reading XML file: $XmlFilePath" -ForegroundColor Yellow

# Read the XML content
$xmlContent = Get-Content $XmlFilePath -Raw

# Create the JSON structure
$postmanBody = @{
    xmlContent = $xmlContent
} | ConvertTo-Json -Depth 10

# Save to file
$postmanBody | Out-File $OutputFile -Encoding UTF8

Write-Host "✓ Conversion complete!" -ForegroundColor Green
Write-Host ""
Write-Host "JSON file saved to: $OutputFile" -ForegroundColor Cyan
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Ready to use in Postman!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Instructions:" -ForegroundColor Yellow
Write-Host "1. Open Postman" -ForegroundColor White
Write-Host "2. Create a POST request to: http://localhost:7222/api/UploadXml" -ForegroundColor White
Write-Host "3. Set Header: Content-Type = application/json" -ForegroundColor White
Write-Host "4. In Body tab, select 'raw' and 'JSON'" -ForegroundColor White
Write-Host "5. Copy the content from: $OutputFile" -ForegroundColor White
Write-Host "6. Paste into Postman Body" -ForegroundColor White
Write-Host "7. Click Send!" -ForegroundColor White
Write-Host ""

# Also display the content on screen
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copy this JSON to Postman Body:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host $postmanBody -ForegroundColor Green
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
