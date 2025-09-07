using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using PlaywrightNtlmDemo.Helpers;
using WizardSchemaExtractor;

// Reference schema classes

namespace PlaywrightNtlmDemo;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>(optional: true)
            .Build();

        var defaultEnv = config["PlaywrightSettings:DefaultEnvironment"] ?? "Test";
        var env = args.Length > 0 ? args[0] : defaultEnv;

        var domainWhitelist = config[$"PlaywrightSettings:Environments:{env}:DomainWhitelist"];
        var dashboardUrl = config[$"PlaywrightSettings:Environments:{env}:DashboardUrl"];

        if (string.IsNullOrEmpty(domainWhitelist) || string.IsNullOrEmpty(dashboardUrl))
        {
            Console.WriteLine($"❌ Environment '{env}' not found in configuration.");
            return;
        }

        if (env.Equals("Prod", StringComparison.OrdinalIgnoreCase) ||
            dashboardUrl.Contains("www.myapp.ca", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("🚨 Tests cannot run against Production!");
        }

        Console.WriteLine($"✅ Running tests against environment: {env}");
        Console.WriteLine($"➡️ URL: {dashboardUrl}");

        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            Args =
            [
                $"--auth-server-whitelist=\"{domainWhitelist}\"",
                $"--auth-negotiate-delegate-whitelist=\"{domainWhitelist}\""
            ]
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });

        var page = await context.NewPageAsync();

        // Log network activity
        page.Request += (_, request) => Console.WriteLine($"➡️ {request.Method} {request.Url}");
        page.Response += (_, response) => Console.WriteLine($"⬅️ {response.Status} {response.Url}");

        await page.GotoAsync(dashboardUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Load schema file (Step1.json as example)
        var schemaPath = Path.Combine("WizardForms", "Step1.json");
        if (!File.Exists(schemaPath))
        {
            Console.WriteLine($"❌ Schema file not found: {schemaPath}");
            return;
        }

        var schemaJson = await File.ReadAllTextAsync(schemaPath);
        var schema = JsonSerializer.Deserialize<PageSchema>(schemaJson);

        Console.WriteLine($"📄 Page Title: {schema?.PageTitle}");
        Console.WriteLine($"🔖 Heading: {schema?.Heading}");

        if (schema != null)
        {
            Console.WriteLine("📝 Filling form using schema...");
            await FormAutoFiller.FillFromSchemaAsync(page, schema);
        }

        Console.WriteLine("✅ Form completed.");
        Console.WriteLine("Press any key to close...");
        Console.ReadKey();
    }
}