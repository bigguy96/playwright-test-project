using System.Text.Json;
using HtmlAgilityPack;
using Microsoft.Playwright;

namespace WizardSchemaExtractor;

public class FormExtractor
{
    public static async Task ExtractAsync(string url, string outputFile, string? saveButtonSelector = null)
    {
        Console.WriteLine($"🌐 Extracting form from: {url}");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });

        var page = await context.NewPageAsync();
        await page.GotoAsync(url);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Get page title and heading
        var title = await page.TitleAsync();
        var headingElement = await page.QuerySelectorAsync("h1");
        var heading = headingElement != null ? await headingElement.InnerTextAsync() : "";

        // Parse HTML
        var html = await page.ContentAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Collect labels for easy lookup
        var labels = doc.DocumentNode.SelectNodes("//label").DistinctBy(d => d.GetAttributeValue("for", ""));
        var labelMap = labels
            .Select(l => new
            {
                For = l.GetAttributeValue("for", ""),
                Text = l.InnerText.Trim()
            })
            .Where(l => !string.IsNullOrEmpty(l.For))
            .ToDictionary(l => l.For, l => l.Text);

        // Collect inputs, selects, textareas, and buttons
        var nodes = doc.DocumentNode
            .SelectNodes("//input | //select | //textarea | //button")
            ?.OrderBy(n => n.StreamPosition);

        var fields = new List<FormField>();
        var index = 1;

        fields.AddRange(from node in nodes
                        let id = node.GetAttributeValue("id", "")
                        let labelText = !string.IsNullOrEmpty(id) && labelMap.TryGetValue(id, out var value)
                            ? value
                            : ""
                        select new FormField
                        {
                            Index = index++,
                            Tag = node.Name,
                            Type = node.GetAttributeValue("type", node.Name == "button" ? "button" : "text"),
                            Id = id,
                            Name = node.GetAttributeValue("name", ""),
                            Placeholder = node.GetAttributeValue("placeholder", ""),
                            LabelText = labelText,
                            Value = node.GetAttributeValue("value", "")
                        });

        fields = fields.Where(field => field.Type != "hidden" && field.Placeholder != "--" && !string.IsNullOrWhiteSpace(field.Id)).Distinct().ToList();
        var schema = new PageSchema
        {
            PageTitle = title,
            Heading = heading,
            Fields = fields
        };

        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(outputFile, json);

        Console.WriteLine($"✅ Saved schema to: {outputFile}");

        // Optionally click "Save/Next"
        if (!string.IsNullOrEmpty(saveButtonSelector))
        {
            if (url.ToLower().Contains("step6"))
            {
                return;
            }

            if (url.ToLower().Contains("step5"))
            {
                await page.GetByText("Save & Continue").ClickAsync();
            }
            else
            {
                await page.Locator($"button[value='{saveButtonSelector}']").ClickAsync();
            }

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            Console.WriteLine("➡️ Proceeded to next step.");
        }
    }
}