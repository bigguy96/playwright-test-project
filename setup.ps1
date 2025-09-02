param(
    [switch]$GlobalTool  # use -GlobalTool if you want global CLI install
)

$projectDir = "PlaywrightNtlmDemo"

Write-Host "🚀 Setting up Playwright project in $projectDir ..."

# Restore NuGet packages
dotnet restore

# Ensure required NuGet packages are installed
Write-Host "📦 Ensuring required NuGet packages are installed..."
dotnet add "$projectDir" package Microsoft.Extensions.Configuration --version "7.*" | Out-Null
dotnet add "$projectDir" package Microsoft.Extensions.Configuration.Json --version "7.*" | Out-Null
dotnet add "$projectDir" package Microsoft.Extensions.Configuration.FileExtensions --version "7.*" | Out-Null

# Build project
dotnet build "$projectDir"

Push-Location $projectDir
try {
    # Check if Playwright CLI is installed
    if ($GlobalTool) {
        if (-not (Get-Command playwright -ErrorAction SilentlyContinue)) {
            Write-Host "📦 Installing Microsoft.Playwright.CLI globally..."
            dotnet tool install --global Microsoft.Playwright.CLI --version "1.*"
        } else {
            Write-Host "✅ Playwright CLI is already installed globally."
        }

        # Check if browsers are already installed
        if (-not (Test-Path ".\bin\Debug\net*playwright\.local-browsers")) {
            Write-Host "🌐 Installing Playwright browsers..."
            playwright install
        } else {
            Write-Host "✅ Browsers are already installed."
        }

    } else {
        # Local tool install
        if (-not (Test-Path "../.config/dotnet-tools.json")) {
            Write-Host "📦 Setting up local Playwright tool manifest..."
            dotnet new tool-manifest -n | Out-Null
        }

        Write-Host "📦 Restoring Playwright CLI locally..."
        dotnet tool restore

        # Check browsers
        if (-not (Test-Path ".\bin\Debug\net*playwright\.local-browsers")) {
            Write-Host "🌐 Installing Playwright browsers..."
            dotnet playwright install
        } else {
            Write-Host "✅ Browsers are already installed."
        }
    }
}
finally {
    Pop-Location
}

Write-Host "🎉 Setup complete!"
