param(
    [string]$RevitVersion = "2026"
)

$ErrorActionPreference = "Stop"

$installRoot = Join-Path $env:LOCALAPPDATA "RevitMcp\$RevitVersion"
$manifestPath = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$RevitVersion\RevitMcp.addin"

if (Test-Path $manifestPath) {
    Remove-Item -Force $manifestPath
}

if (Test-Path $installRoot) {
    Remove-Item -Recurse -Force $installRoot
}

Write-Host "Removed Revit MCP add-in files for Revit $RevitVersion." -ForegroundColor Green
