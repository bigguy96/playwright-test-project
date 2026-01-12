using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightNtlmDemo.Schemas;

namespace PlaywrightNtlmDemo.Handlers;

internal class DateHandler : IFieldHandler
{
    public bool CanHandle(FormField field)
    {
        return field.Tag.Equals("input", StringComparison.OrdinalIgnoreCase) && field.Type.Equals("date", StringComparison.OrdinalIgnoreCase);
    }

    public async Task HandleAsync(IPage page, FormField field, ILogger log)
    {
        var selector = SelectorUtil.BuildSelector(field);
        if (selector is null) { log.LogWarning("Text: no selector for #{idx}", field.Index); return; }

        // Set the date value (format: YYYY-MM-DD)
        var dateValue = DateTime.Today.AddDays(-10).ToString("yyyy-MM-dd");
        await page.FillAsync(selector, dateValue);

        // Verify the value was set
        var setValue = await page.GetAttributeAsync(selector, "value");
        Console.WriteLine($"Date set to: {setValue}");
    }
}