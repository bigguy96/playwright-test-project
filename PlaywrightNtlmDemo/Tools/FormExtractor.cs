using System.Text.Json;
using HtmlAgilityPack;
using Microsoft.Playwright;

namespace PlaywrightNtlmDemo.Tools
{
    public class FormExtractor
    {
        /// <summary>
        /// Extracts form fields from a given URL and saves them as a JSON file.
        /// Optionally clicks a "Save" or "Next" button to move to the next wizard step.
        /// </summary>
        /// <param name="url">The full URL of the wizard step, including dynamic ID.</param>
        /// <param name="outputFile">Where to save the JSON file.</param>
        /// <param name="saveButtonSelector">CSS selector for the Save/Next button (optional).</param>
        public static async Task ExtractFormAsync(string url, string outputFile, string? saveButtonSelector = null)
        {
            Console.WriteLine($"🌐 Navigating to: {url}");

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true
            });

            var page = await context.NewPageAsync();
            await page.GotoAsync(url);

            // Grab page HTML
            var html = await page.ContentAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Extract form fields
            var nodes = doc.DocumentNode.SelectNodes("//input | //select | //textarea") ?? new HtmlNodeCollection(null);
            var fields = new List<Dictionary<string, string>>();

            foreach (var node in nodes)
            {
                var field = new Dictionary<string, string>
                {
                    { "tag", node.Name },
                    { "type", node.GetAttributeValue("type", "text") },
                    { "id", node.GetAttributeValue("id", "") },
                    { "name", node.GetAttributeValue("name", "") },
                    { "placeholder", node.GetAttributeValue("placeholder", "") }
                };
                fields.Add(field);
            }

            // Save JSON
            var json = JsonSerializer.Serialize(fields, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(outputFile, json);
            Console.WriteLine($"✅ Fields extracted and saved to {outputFile}");

            // Optional: Click Save/Next
            if (!string.IsNullOrEmpty(saveButtonSelector))
            {
                Console.WriteLine($"➡️ Clicking save/next button: {saveButtonSelector}");
                await page.ClickAsync(saveButtonSelector);
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                Console.WriteLine("✅ Moved to the next wizard step.");
            }
        }
    }
}