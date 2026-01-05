using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightNtlmDemo.Schemas;

namespace PlaywrightNtlmDemo.Handlers;

internal sealed class FormElementHandler(IFieldHandler inner) : IFieldHandler
{
    public bool CanHandle(FormField field) => inner.CanHandle(field);

    public async Task HandleAsync(IPage page, FormField field, ILogger log)
    {
        await inner.HandleAsync(page, field, log);

        log.LogDebug("Field {FieldId}", field.Id);

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}