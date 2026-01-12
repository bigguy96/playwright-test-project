using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightNtlmDemo.Schemas;

namespace PlaywrightNtlmDemo.Handlers;

internal class SelectHandler : IFieldHandler
{
    public bool CanHandle(FormField f) => f.Tag.Equals("select", StringComparison.OrdinalIgnoreCase) && !f.IsSelect2;

    public async Task HandleAsync(IPage page, FormField f, ILogger log)
    {
        var selector = SelectorUtil.BuildSelector(f);

        if (selector is null)
        {
            log.LogWarning("Select: no selector for #{idx}", f.Index); return;
        }

        if (!string.IsNullOrEmpty(f.Value))
        {
            await page.SelectOptionAsync(selector, new SelectOptionValue { Value = f.Value });
        }
        else
        {
            await page.SelectOptionAsync(selector, new SelectOptionValue { Index = 1 });
        }
    }
}