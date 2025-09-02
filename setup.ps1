param(
    [switch]$GlobalTool
)

$projectDir = "PlaywrightNtlmDemo"
$projectFile = Join-Path $projectDir "PlaywrightNtlmDemo.csproj"
$browserPath = Join-Path $projectDir "bin\Debug\net9.0\playwright\.local-browsers"

Write-Host "🚀 Setting up Playwright project in $projectDir ..."

dotnet restore

function Add-PackageIfMissing {
    param([string]$PackageName, [string]$Version)
    $csprojContent = Get-Content $projectFile
    if ($csprojContent -notmatch $PackageName) {
        Write-Host "📦 Installing package: $PackageName"
        dotnet add "$projectDir" package $PackageName --version $Version
    } else {
        Write-Host "✅ Package already installed: $PackageName"
    }
}

Add-PackageIfMissing "Microsoft.Extensions.Configuration" "9.*"
Add-PackageIfMissing "Microsoft.Extensions.Configuration.Json" "9.*"
Add-PackageIfMissing "Microsoft.Extensions.Configuration.FileExtensions" "9.*"

dotnet build "$projectDir"

Push-Location $projectDir

if ($GlobalTool) {
    if (-not (Get-Command playwright -ErrorAction SilentlyContinue)) {
        Write-Host "📦 Installing Playwright CLI globally..."
        dotnet tool install --global Microsoft.Playwright.CLI --version "1.*"
    } else {
        Write-Host "✅ Playwright CLI is already installed globally."
    }
} else {
    if (-not (Test-Path "../.config/dotnet-tools.json")) {
        Write-Host "📦 Creating local tool manifest..."
        dotnet new tool-manifest
    }
    Write-Host "📦 Restoring Playwright CLI locally..."
    dotnet tool restore
}

if (-not (Test-Path $browserPath)) {
    Write-Host "🌐 Installing Playwright browsers..."
    if ($GlobalTool) {
        playwright install
    } else {
        dotnet playwright install
    }
} else {
    Write-Host "✅ Browsers already installed."
}

Pop-Location

Write-Host "🎉 Setup complete!"
