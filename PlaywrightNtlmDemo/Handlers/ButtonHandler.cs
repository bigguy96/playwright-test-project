using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightNtlmDemo.Schemas;

namespace PlaywrightNtlmDemo.Handlers;

internal class ButtonHandler : IFieldHandler
{
    public bool CanHandle(FormField field)
    {
        return field.Tag.Equals("button", StringComparison.OrdinalIgnoreCase);
    }

    public async Task HandleAsync(IPage page, FormField field, ILogger log)
    {
        var selector = SelectorUtil.BuildSelector(field);
        if (selector is null) { log.LogWarning("Button: no selector for #{idx}", field.Index); return; }

        await page.Locator(selector).ClickAsync();

        // Wait for specific AJAX response
        if (field.IsAjax)
        {
            var response = await page.WaitForResponseAsync(response => response.Url.Contains("/PFTR-ETVP") && response.Status == 200, new PageWaitForResponseOptions());
            Console.WriteLine($"AJAX response received: {response.Url}");
        }

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions());
        Console.WriteLine($"Button {field.Id} is clicked.");
    }
}