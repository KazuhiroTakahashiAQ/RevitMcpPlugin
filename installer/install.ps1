param(
    [string]$Configuration = "Release",
    [string]$RevitVersion = "2026"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$addinProject = Join-Path $repoRoot "src\RevitMcp.Addin\RevitMcp.Addin.csproj"
$serverProject = Join-Path $repoRoot "src\RevitMcp.Server\RevitMcp.Server.csproj"
$installRoot = Join-Path $env:LOCALAPPDATA "RevitMcp\$RevitVersion"
$addinOutput = Join-Path $installRoot "addin"
$serverOutput = Join-Path $installRoot "server"
$manifestDir = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$RevitVersion"
$manifestPath = Join-Path $manifestDir "RevitMcp.addin"
$templatePath = Join-Path $PSScriptRoot "RevitMcp.addin.template"

New-Item -ItemType Directory -Force -Path $addinOutput | Out-Null
New-Item -ItemType Directory -Force -Path $serverOutput | Out-Null
New-Item -ItemType Directory -Force -Path $manifestDir | Out-Null

dotnet publish $addinProject -c $Configuration -o $addinOutput
dotnet publish $serverProject -c $Configuration -o $serverOutput

$assemblyPath = Join-Path $addinOutput "RevitMcp.Addin.dll"
$manifestTemplate = Get-Content -Raw -Path $templatePath
$manifestContent = $manifestTemplate.Replace("{{ASSEMBLY_PATH}}", $assemblyPath)

Set-Content -Path $manifestPath -Value $manifestContent -Encoding UTF8

Write-Host "Installed Revit MCP add-in." -ForegroundColor Green
Write-Host "Add-in assembly: $assemblyPath"
Write-Host "Claude Desktop bridge: $(Join-Path $serverOutput 'RevitMcp.Server.dll')"
Write-Host "Manifest: $manifestPath"
