Write-Host "🚀 Setting up Playwright project..."

$projectDir = "PlaywrightNtlmDemo"

# Always restore and update packages
dotnet restore
Write-Host "📦 Updating NuGet packages..."
dotnet add "$projectDir" package Microsoft.Extensions.Configuration --version "*"
dotnet add "$projectDir" package Microsoft.Extensions.Configuration.Json --version "*"
dotnet add "$projectDir" package Microsoft.Extensions.Configuration.FileExtensions --version "*"

# Rebuild
dotnet restore
dotnet build "$projectDir"

# Ensure Playwright browsers are installed
Write-Host "🌐 Installing Playwright browsers..."
playwright install

Write-Host "🎉 Setup complete!"