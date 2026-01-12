param(
    [switch]$GlobalTool
)

# Define projects
$projects = @(
    "PlaywrightNtlmDemo",
    "WizardSchemaExtractor"
)

Write-Host "🚀 Setting up Playwright solution..."

# Restore solution
dotnet restore

# Check and add project reference if missing
$projFile = Join-Path "PlaywrightNtlmDemo" "PlaywrightNtlmDemo.csproj"
$projContent = Get-Content $projFile

if ($projContent -notmatch "WizardSchemaExtractor.csproj") {
    Write-Host "🔗 Adding project reference (WizardSchemaExtractor -> PlaywrightNtlmDemo)..."
    dotnet add PlaywrightNtlmDemo reference WizardSchemaExtractor
} else {
    Write-Host "✅ Project reference already exists."
}

# Loop through projects to add/restore/update required NuGet packages
foreach ($project in $projects) {
    Write-Host "📦 Updating NuGet packages for $project..."

    dotnet add "$project" package Microsoft.Extensions.Configuration --version "*"
    dotnet add "$project" package Microsoft.Extensions.Configuration.FileExtensions --version "*"
    dotnet add "$project" package Microsoft.Extensions.Configuration.Json --version "*"
    dotnet add "$project" package Microsoft.Extensions.Configuration.UserSecrets --version "*"
    dotnet add "$project" package Microsoft.Playwright --version "*"
    dotnet add "$project" package HtmlAgilityPack --version "*"

    dotnet restore "$project"
    dotnet build "$project"
}

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
    } else {
        Write-Host "✅ Local tool manifest already exists."
    }
    Write-Host "📦 Restoring Playwright CLI locally..."
    dotnet tool restore
}

# Install Playwright browsers (shared install)
Write-Host "🌐 Installing Playwright browsers..."
if ($GlobalTool) { playwright install } else { dotnet playwright install }

Write-Host "🎉 Setup complete!"

