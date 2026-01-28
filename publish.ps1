# AumovioNadSelectorVRG8 - Build and Publish Script
# This script builds the application and creates a Velopack release package

param(
    [Parameter(Mandatory=$false)]
    [string]$Version = "1.0.0",

    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild = $false
)

$ErrorActionPreference = "Stop"

# Configuration
$ProjectName = "AumovioNadSelectorVRG8"
$ProjectPath = "src\NadMatcher.UI\NadMatcher.UI.csproj"
$OutputDir = "publish"
$ReleasesDir = "releases"
$Runtime = "win-x64"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " $ProjectName Build Script" -ForegroundColor Cyan
Write-Host " Version: $Version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Ensure vpk tool is installed
Write-Host "Checking for Velopack CLI (vpk)..." -ForegroundColor Yellow
$vpkInstalled = Get-Command vpk -ErrorAction SilentlyContinue
if (-not $vpkInstalled) {
    Write-Host "Installing Velopack CLI tool..." -ForegroundColor Yellow
    dotnet tool install -g vpk
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to install vpk tool" -ForegroundColor Red
        exit 1
    }
}

# Clean output directories
Write-Host "Cleaning output directories..." -ForegroundColor Yellow
if (Test-Path $OutputDir) {
    Remove-Item -Recurse -Force $OutputDir
}
if (Test-Path $ReleasesDir) {
    Remove-Item -Recurse -Force $ReleasesDir
}

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
New-Item -ItemType Directory -Path $ReleasesDir -Force | Out-Null

if (-not $SkipBuild) {
    # Build and publish the application
    Write-Host ""
    Write-Host "Building application..." -ForegroundColor Yellow
    dotnet publish $ProjectPath `
        -c Release `
        -r $Runtime `
        --self-contained true `
        -p:PublishSingleFile=false `
        -p:Version=$Version `
        -p:FileVersion=$Version.0 `
        -p:InformationalVersion=$Version `
        -o $OutputDir

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed!" -ForegroundColor Red
        exit 1
    }
    Write-Host "Build completed successfully!" -ForegroundColor Green
}

# Create Velopack release package
Write-Host ""
Write-Host "Creating Velopack release package..." -ForegroundColor Yellow

vpk pack `
    --packId $ProjectName `
    --packVersion $Version `
    --packDir $OutputDir `
    --mainExe "$ProjectName.exe" `
    --outputDir $ReleasesDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "Velopack packaging failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host " Release package created successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output files in: $ReleasesDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "Files created:" -ForegroundColor Yellow
Get-ChildItem $ReleasesDir | ForEach-Object { Write-Host "  - $($_.Name)" }
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Create a new release on GitHub" -ForegroundColor White
Write-Host "  2. Upload all files from '$ReleasesDir' folder" -ForegroundColor White
Write-Host "  3. Tag the release with version 'v$Version'" -ForegroundColor White
Write-Host ""
