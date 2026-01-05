using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightNtlmDemo.Schemas;

namespace PlaywrightNtlmDemo.Handlers;

internal class CheckboxHandler : IFieldHandler
{
    public bool CanHandle(FormField f) => f.Tag.Equals("input", StringComparison.OrdinalIgnoreCase) && f.Type.Equals("checkbox", StringComparison.OrdinalIgnoreCase);

    public async Task HandleAsync(IPage page, FormField f, ILogger log)
    {
        var selector = SelectorUtil.BuildSelector(f);
        if (selector is null) { log.LogWarning("Checkbox: no selector for #{idx}", f.Index); return; }
        await page.CheckAsync(selector);
    }
}