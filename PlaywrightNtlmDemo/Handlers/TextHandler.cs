using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightNtlmDemo.Schemas;

namespace PlaywrightNtlmDemo.Handlers;

internal class TextHandler : IFieldHandler
{
    public bool CanHandle(FormField f)
    {
        if (f.Tag.Equals("textarea", StringComparison.OrdinalIgnoreCase) || f.Tag.Equals("input", StringComparison.OrdinalIgnoreCase))
        {
            return f.Type.Equals("text", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public async Task HandleAsync(IPage page, FormField f, ILogger log)
    {
        var selector = SelectorUtil.BuildSelector(f);
        if (selector is null) { log.LogWarning("Text: no selector for #{idx}", f.Index); return; }

        await page.FillAsync(selector, f.Value ?? string.Empty);
        await page.DispatchEventAsync(selector, "blur");
    }
}