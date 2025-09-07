using Microsoft.Extensions.Configuration;

namespace WizardSchemaExtractor;

public class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>(optional: true)
            .Build();

        var env = args.Length > 0 ? args[0] : "Test";
        var dashboardUrl = config[$"PlaywrightSettings:Environments:{env}:DashboardUrl"];
        var saveButtonSelector = config["PlaywrightSettings:Wizard:SaveButtonSelector"];
        var totalSteps = int.Parse(config["PlaywrightSettings:Wizard:TotalSteps"] ?? "1");

        if (string.IsNullOrEmpty(dashboardUrl))
        {
            Console.WriteLine($"❌ Environment '{env}' not found.");
            return;
        }

        var wizardId = "12345"; // Normally extracted dynamically after clicking a report type
        var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "WizardForms");
        Directory.CreateDirectory(outputDir);

        for (var i = 1; i <= totalSteps; i++)
        {
            var stepUrl = $"{dashboardUrl}/../wizard/{wizardId}/step{i}";
            var outputFile = Path.Combine(outputDir, $"Step{i}.json");
            await FormExtractor.ExtractAsync(stepUrl, outputFile, saveButtonSelector);
        }

        Console.WriteLine("✅ Extraction complete.");
    }
}