using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using PlaywrightNtlmDemo.Helpers;
using WizardSchemaExtractor;

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
        var language = config[$"PlaywrightSettings:Environments:{env}:Language"];
        var reportType = config[$"PlaywrightSettings:Environments:{env}:ReportType"];
        var reportTypeSelector = config["PlaywrightSettings:Wizard:ReportTypeSelector"];
        var saveButtonSelector = config["PlaywrightSettings:Wizard:SaveButtonSelector"];
        var totalSteps = int.Parse(config["PlaywrightSettings:Wizard:TotalSteps"] ?? "6");
        var outputDirectory = CreateOutPutDirectory();

        if (string.IsNullOrEmpty(domainWhitelist) || string.IsNullOrEmpty(dashboardUrl))
        {
            Console.WriteLine($"❌ Environment '{env}' not found in configuration.");
            return;
        }

        if (env.Equals("Prod", StringComparison.OrdinalIgnoreCase))
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
        Console.WriteLine("📄 On Dashboard...");
        await page.GotoAsync(dashboardUrl);
        //await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        //TODO: To fix
        await page.ClickAsync("text=Create a new PFTR");

        // 2. Select a report type
        Console.WriteLine($"📑 Clicking report type selector: {reportTypeSelector}");
        await page.SelectOptionAsync("select#FlightTestType", reportTypeSelector ?? string.Empty);
        await page.ClickAsync("button[name='NavigationAction']");

        //await page.ClickAsync(reportTypeSelector ?? string.Empty);
        //await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // 3. Extract wizard ID from URL
        var currentUrl = page.Url;
        var wizardId = currentUrl.Split('/').LastOrDefault(s => int.TryParse(s, out _));
        if (wizardId == null)
        {
            Console.WriteLine("❌ Could not extract wizard ID from URL!");
            return;
        }
        Console.WriteLine($"🔍 Wizard started with ID: {wizardId}");

        // 4. Iterate through steps
        for (var i = 2; i <= totalSteps; i++)
        {
            //temporary
            if (i == 5) return;

            var stepUrl = $"{dashboardUrl}{language}/{reportType}/Step{i}/{wizardId}";
            var outputFile = Path.Combine(outputDirectory, $"Step{i}.json");

            if (!File.Exists(outputFile))
            {
                Console.WriteLine($"❌ Schema file not found: {outputFile}");
                return;
            }

            var excludes = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "excludes.txt"));
            var excluded = excludes.Split([','], StringSplitOptions.RemoveEmptyEntries);

            var schemaJson = await File.ReadAllTextAsync(outputFile);
            var schema = JsonSerializer.Deserialize<PageSchema>(schemaJson);

            if (schema != null)
            {
                //schema.Fields = schema.Fields.Where(field => !excluded.Contains(field.Id)).ToList();
                schema.Fields = schema.Fields.Where(field => !excluded.Contains(field.Id)).GroupBy(field => field.Id).Select(field => field.First()).ToList();

                Console.WriteLine($"➡️ Navigating to Step {i}: {stepUrl}");
                Console.WriteLine($"📄 Page Title: {schema?.PageTitle}");
                Console.WriteLine($"🔖 Heading: {schema?.Heading}");

                // Fill fields immediately (if you want live testing instead)
                await page.GotoAsync(stepUrl);
                await FormAutoFiller.FillFromSchemaAsync(page, schema);
            }

            await page.Locator($"button[value='{saveButtonSelector}']").ClickAsync();
        }

        Console.WriteLine("✅ Form completed.");
        Console.WriteLine("Press any key to close...");
        Console.ReadKey();
    }

    private static string CreateOutPutDirectory()
    {
        // Get the base directory of the running app (usually /bin/Debug/netX.X/)
        var baseDirectory = AppContext.BaseDirectory;

        // Traverse up to reach the solution/project root (adjust based on depth)
        var projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\..\"));

        // Define the shared folder path (or just the root)
        var sharedFolderPath = Path.Combine(projectRoot, "WizardForms");

        // Make sure the directory exists
        Directory.CreateDirectory(sharedFolderPath);

        Console.WriteLine($"File saved to directory: {sharedFolderPath}");

        return sharedFolderPath;
    }
}