using Microsoft.Extensions.Configuration;

namespace WizardSchemaExtractor;

public class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>(optional: true)
            .Build();
        var appUrl = config["PlaywrightSettings:Environments:Test:AppUrl"];
        var language = config["PlaywrightSettings:Environments:Test:Language"];
        var reportType = config["PlaywrightSettings:Environments:Test:ReportType"];
        var id = config["PlaywrightSettings:Environments:Test:Id"];
        var saveButtonSelector = config["PlaywrightSettings:Wizard:SaveButtonSelector"];
        var totalSteps = int.Parse(config["PlaywrightSettings:Wizard:TotalSteps"] ?? "1");
        var outputDir = CreateOutPutDirectory();
        
        Directory.CreateDirectory(outputDir);

        for (var i = 2; i <= totalSteps; i++)
        {
            var stepUrl = $"{appUrl}/{language}/{reportType}/step{i}/{id}";
            var outputFile = Path.Combine(outputDir, $"Step{i}.json");

            await FormExtractor.ExtractAsync(stepUrl, outputFile, saveButtonSelector);
        }

        Console.WriteLine("✅ Extraction complete.");
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