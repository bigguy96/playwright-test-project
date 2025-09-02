param(
    [switch]$GlobalTool
)

$projectDir = "PlaywrightNtlmDemo"
$browserPath = Join-Path $projectDir "bin\Debug\net9.0\playwright\.local-browsers"

Write-Host "🚀 Setting up Playwright project in $projectDir ..."

# Restore solution/project packages
dotnet restore

# Always update NuGet packages to the latest versions
Write-Host "📦 Updating NuGet packages to latest..."
dotnet add "$projectDir" package Microsoft.Extensions.Configuration --version "*"
dotnet add "$projectDir" package Microsoft.Extensions.Configuration.Json --version "*"
dotnet add "$projectDir" package Microsoft.Extensions.Configuration.FileExtensions --version "*"

# Self-heal: re-run restore and build
Write-Host "🔄 Running dotnet restore after updates..."
dotnet restore
Write-Host "🔨 Building project..."
dotnet build "$projectDir"

Push-Location $projectDir

# Install Playwright CLI
if ($GlobalTool) {
    if (-not (Get-Command playwright -ErrorAction SilentlyContinue)) {
        Write-Host "📦 Installing Playwright CLI globally..."
        dotnet tool install --global Microsoft.Playwright.CLI --version "*"
    } else {
        Write-Host "✅ Playwright CLI is already installed globally."
    }
} else {
    if (-not (Test-Path ".config/dotnet-tools.json")) {
        Write-Host "📦 Creating local tool manifest..."
        dotnet new tool-manifest
    }
    Write-Host "📦 Restoring Playwright CLI locally..."
    dotnet tool restore
}

# Install Playwright browsers
if (-not (Test-Path $browserPath)) {
    Write-Host "🌐 Installing Playwright browsers..."
    if ($GlobalTool) { playwright install } else { dotnet playwright install }
} else {
    Write-Host "✅ Browsers already installed."
}

Pop-Location

Write-Host "🎉 Setup complete!"