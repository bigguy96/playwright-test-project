using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;
using PlaywrightNtlmDemo.Tools;
using PlaywrightNtlmDemo.Helpers;

class Program
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
        var reportTypeSelector = config["PlaywrightSettings:Wizard:ReportTypeSelector"];
        var saveButtonSelector = config["PlaywrightSettings:Wizard:SaveButtonSelector"];
        var totalSteps = int.Parse(config["PlaywrightSettings:Wizard:TotalSteps"] ?? "1");

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

        // 🔎 Hook into network events
        page.Request += (_, request) => Console.WriteLine($"➡️ {request.Method} {request.Url}");
        page.Response += (_, response) => Console.WriteLine($"⬅️ {response.Status} {response.Url}");

        // 1. Navigate to Dashboard
        await page.GotoAsync(dashboardUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Console.WriteLine("📄 On Dashboard...");

        // 2. Select a report type
        Console.WriteLine($"📑 Clicking report type selector: {reportTypeSelector}");
        await page.ClickAsync(reportTypeSelector);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 3. Extract wizard ID from URL
        var currentUrl = page.Url;
        var wizardId = currentUrl.Split('/').FirstOrDefault(s => int.TryParse(s, out _));
        if (wizardId == null)
        {
            Console.WriteLine("❌ Could not extract wizard ID from URL!");
            return;
        }
        Console.WriteLine($"🔍 Wizard started with ID: {wizardId}");

        // 4. Create output folder for JSON files
        var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "WizardForms");
        Directory.CreateDirectory(outputDir);

        // 5. Iterate through steps
        for (int i = 1; i <= totalSteps; i++)
        {
            var stepUrl = $"{dashboardUrl}/../wizard/{wizardId}/step{i}";
            var outputFile = Path.Combine(outputDir, $"Step{i}.json");
            Console.WriteLine($"➡️ Navigating to Step {i}: {stepUrl}");

            // Option A: Extract fields to JSON (schema-driven approach)
            //await FormExtractor.ExtractFormAsync(stepUrl, outputFile, saveButtonSelector);

            // Option B: Fill fields immediately (if you want live testing instead)
            await page.GotoAsync(stepUrl);
            await FormAutoFiller.FillAllFormFieldsAsync(page);
            await page.ClickAsync(saveButtonSelector);
        }

        Console.WriteLine("✅ Wizard flow complete! JSON field schemas saved.");
        Console.ReadKey();
    }
}