using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightNtlmDemo.Schemas;

namespace PlaywrightNtlmDemo.Handlers;

internal class Select2Handler : IFieldHandler
{
    public bool CanHandle(FormField f) => f.Tag.Equals("select", StringComparison.OrdinalIgnoreCase) && f.IsSelect2;

    public async Task HandleAsync(IPage page, FormField f, ILogger log)
    {
        var selector = SelectorUtil.BuildSelector(f);
        if (selector is null) { log.LogWarning("Select2: no selector for #{idx}", f.Index); return; }

        var desiredText = f.Value ?? string.Empty;
        var selectId = await page.EvalOnSelectorAsync<string?>(selector, "(el) => el ? el.id : null");
        if (string.IsNullOrWhiteSpace(selectId))
        {
            log.LogWarning("Select2: select needs an id. {sel}", selector);
            return;
        }

        // Get the select2 container
        var containerSpan = $"#select2-{selectId}-container";
        var containerLocator = page.Locator(containerSpan);

        // Click and display the select2 dropdown
        await containerLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 30000 });
        await containerLocator.ClickAsync(new LocatorClickOptions { Timeout = 5000 });
        await page.WaitForSelectorAsync(".select2-dropdown, .select2-results__options");

        // Type the desired text
        var searchInput = page.Locator(".select2-search__field");
        await searchInput.FillAsync(desiredText, new LocatorFillOptions { Force = true });

        // Wait for AJAX response
        var response = await page.WaitForResponseAsync(response => response.Url.Contains("/PFTR-ETVP") && response.Status == 200, new PageWaitForResponseOptions { Timeout = 15000 });
        var responseBody = await response.TextAsync();
        Console.WriteLine($"AJAX response: {responseBody}");

        // Select the desired text
        var optionLocator = page.Locator(".select2-results__option");
        var count = await optionLocator.CountAsync();
        for (var i = 0; i < count; i++)
        {
            var loc = optionLocator.Nth(i);
            var txt = (await loc.InnerTextAsync())?.Trim() ?? "";

            if (!string.IsNullOrEmpty(txt) && txt.Contains(desiredText, StringComparison.OrdinalIgnoreCase))
            {
                await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
                await loc.ClickAsync();

                return;
            }
        }
    }
}