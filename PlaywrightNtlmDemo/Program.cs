using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;

class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var defaultEnv = config["PlaywrightSettings:DefaultEnvironment"] ?? "Test";
        var env = args.Length > 0 ? args[0] : defaultEnv;

        var domainWhitelist = config[$"PlaywrightSettings:Environments:{env}:DomainWhitelist"];
        var appUrl = config[$"PlaywrightSettings:Environments:{env}:AppUrl"];

        if (string.IsNullOrEmpty(domainWhitelist) || string.IsNullOrEmpty(appUrl))
        {
            Console.WriteLine($"❌ Environment '{env}' not found in configuration.");
            return;
        }

        if (env.Equals("Prod", StringComparison.OrdinalIgnoreCase) ||
            appUrl.Contains("www.myapp.ca", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("🚨 Tests cannot run against Production!");
        }

        Console.WriteLine($"✅ Running tests against environment: {env}");
        Console.WriteLine($"➡️ URL: {appUrl}");

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

        // 🔎 Hook into request/response events
        page.Request += (_, request) =>
        {
            Console.WriteLine($"➡️ {request.Method} {request.Url}");
        };

        page.Response += (_, response) =>
        {
            Console.WriteLine($"⬅️ {response.Status} {response.Url}");
        };

        await page.GotoAsync(appUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Console.WriteLine("Page title: " + await page.TitleAsync());
        Console.WriteLine("Press any key to close...");
        Console.ReadKey();
    }
}