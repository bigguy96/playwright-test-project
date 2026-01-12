using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightNtlmDemo.Helpers;
using PlaywrightNtlmDemo.Schemas;

namespace PlaywrightNtlmDemo;

internal class Program
{
    public static async Task Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(b =>
        {
            b.AddSimpleConsole(o => { o.SingleLine = true; o.TimestampFormat = "HH:mm:ss "; });
            b.SetMinimumLevel(LogLevel.Information);
        });
        var log = loggerFactory.CreateLogger("Playwright");
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>(optional: true)
            .Build();
        var whitelist = config["PlaywrightSettings:DomainWhitelist"];
        var dashboardUrl = config["PlaywrightSettings:DashboardUrl"] ?? "dashboard";
        var language = config["PlaywrightSettings:Language"];
        var reportType = config["PlaywrightSettings:ReportType"];
        var reportTypeSelector = config["PlaywrightSettings:ReportTypeSelector"] ?? "selector";
        var saveButtonSelector = config["PlaywrightSettings:SaveButtonSelector"];
        var totalSteps = int.Parse(config["PlaywrightSettings:TotalSteps"] ?? "6");
        var newReport = config["PlaywrightSettings:NewReport"] ?? "report";
        var flightTestType = config["PlaywrightSettings:FlightTestType"] ?? "test type";
        var createReport = config["PlaywrightSettings:CreateReport"] ?? "create report";
        var agreement = config["PlaywrightSettings:Agreement"] ?? "I agree to these terms and conditions";
        var outputDirectory = PathHelper.ResolveWizardFormsFolder();

        // Initiate Playwright
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            Args =
            [
                $"--auth-server-whitelist={whitelist}",
                $"--auth-negotiate-delegate-whitelist={whitelist}"
            ]
        });

        // Create page
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { IgnoreHTTPSErrors = true });
        var page = await context.NewPageAsync();

        page.Request += (_, r) => log.LogInformation("➡️ {m} {u}", r.Method, r.Url);
        page.Response += (_, r) => log.LogInformation("⬅️ {s} {u}", r.Status, r.Url);

        // 1. Navigate to dashboard
        await page.GotoAsync(dashboardUrl);
        await page.ClickAsync(newReport);
        
        // 1.1 Accept terms and conditions
        await page.ClickAsync(agreement);

        // 2. Select a report type
        Console.WriteLine($"Clicking report type selector: {reportTypeSelector}");
        await page.SelectOptionAsync(flightTestType, reportTypeSelector);
        await page.ClickAsync(createReport);

        // 3. Extract wizard ID from URL
        var currentUrl = page.Url;
        var wizardId = currentUrl.Split('/').LastOrDefault(s => int.TryParse(s, out _));
        if (wizardId == null)
        {
            Console.WriteLine("Could not extract wizard ID from URL!");
            return;
        }
        Console.WriteLine($"Wizard started with ID: {wizardId}");

        // 4. Iterate through steps
        for (var step = 2; step <= totalSteps; step++)
        {
            var stepUrl = $"{dashboardUrl}/{language}/{reportType}/Step{step}/{wizardId}";
            var outputFile = Path.Combine(outputDirectory, $"Step{step}.json");

            if (File.Exists(outputFile))
            {
                // Get the step details and deserialize
                var json = await File.ReadAllTextAsync(outputFile);
                var pageSchema = JsonSerializer.Deserialize<PageSchema>(json);

                if (pageSchema is not null)
                {
                    // Go to step and fill the form
                    await page.GotoAsync(stepUrl);
                    await FormAutoFiller.FillFromSchemaAsync(page, pageSchema, log);

                    if (step == 5)
                    {
                        await page.ClickAsync("text=Save & Continue");
                    }
                    else
                    {
                        await page.Locator($"button[value='{saveButtonSelector}']").ClickAsync();
                    }
                }
                else
                {
                    log.LogWarning("Page schema is null, continuing");
                    return;
                }
            }
            else
            {
                log.LogWarning("Schema not found at {path}. Run the extractor to generate it.", outputFile);
                return;
            }
        }

        // 5. Return to the dashboard
        await page.ClickAsync("text=Close");

        log.LogInformation("Done. Press any key to close...");
        Console.ReadKey();
    }
}