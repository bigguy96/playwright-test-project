using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightNtlmDemo.Schemas;

namespace PlaywrightNtlmDemo.Handlers;

internal class RadioHandler : IFieldHandler
{
    public bool CanHandle(FormField f) => f.Tag.Equals("input", StringComparison.OrdinalIgnoreCase) && f.Type.Equals("radio", StringComparison.OrdinalIgnoreCase);

    public async Task HandleAsync(IPage page, FormField f, ILogger log)
    {
        if (!string.IsNullOrEmpty(f.Name))
        {
            var radioSelector = $"input[name='{f.Name}'][value='{f.Value}']";

            // Ensure the radio button exists and is visible
            await page.WaitForSelectorAsync(radioSelector, new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });

            // Construct label selector for Bootstrap-styled radio button
            var labelSelector = $"label:has({radioSelector})";
            var inputElement = page.Locator(radioSelector);
            var labelElement = page.Locator(labelSelector);

            // Try clicking the label first (Bootstrap style), fall back to input
            if (await labelElement.CountAsync() > 0)
            {
                await labelElement.ClickAsync();
                Console.WriteLine($"Clicked label for radio button: {radioSelector}");
            }
            else
            {
                await inputElement.ClickAsync();
                Console.WriteLine($"Clicked radio button: {radioSelector}");
            }

            return;
        }

        var selector = SelectorUtil.BuildSelector(f);
        if (selector is null) { log.LogWarning("Radio: no selector for #{idx}", f.Index); return; }
        await page.CheckAsync(selector);
    }
}