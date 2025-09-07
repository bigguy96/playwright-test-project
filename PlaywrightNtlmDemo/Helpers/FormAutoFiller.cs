using Microsoft.Playwright;
using WizardSchemaExtractor;

// Use the PageSchema and FormField models

namespace PlaywrightNtlmDemo.Helpers;
public static class FormAutoFiller
{
    /// <summary>
    /// Fills fields from a JSON schema (PageSchema).
    /// Skips buttons automatically and logs labels for debugging.
    /// </summary>
    public static async Task FillFromSchemaAsync(IPage page, PageSchema schema)
    {
        foreach (var field in schema.Fields)
        {
            // Skip buttons
            if (field.Tag.Equals("button", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"🔘 Skipping button: {field.Id ?? field.Name}");
                continue;
            }

            // Determine selector
            var selector = !string.IsNullOrEmpty(field.Id)
                ? $"#{field.Id}"
                : !string.IsNullOrEmpty(field.Name)
                    ? $"[name='{field.Name}']"
                    : "";

            if (string.IsNullOrEmpty(selector))
            {
                Console.WriteLine($"⚠️ Skipping field {field.Index} (no selector found).");
                continue;
            }

            // Log field
            Console.WriteLine($"#{field.Index} {field.LabelText ?? field.Name} ({field.Tag}) => Filling...");

            // Handle field type
            try
            {
                if (field.Tag.Equals("select", StringComparison.OrdinalIgnoreCase))
                {
                    await page.SelectOptionAsync(selector, ["Option1"]); // Replace with dynamic test data if needed
                }
                else if (field.Type.Equals("checkbox", StringComparison.OrdinalIgnoreCase) || field.Type.Equals("radio", StringComparison.OrdinalIgnoreCase))
                {
                    await page.CheckAsync(selector);
                }
                else
                {
                    await page.FillAsync(selector, "TestValue");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error filling field {field.Index}: {ex.Message}");
            }
        }
    }
}