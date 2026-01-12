using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightNtlmDemo.Handlers;
using PlaywrightNtlmDemo.Schemas;

namespace PlaywrightNtlmDemo.Helpers;

internal static class FormAutoFiller
{
    private static readonly IFieldHandler[] Handlers =
    [
        new FormElementHandler(new Select2Handler()),
        new FormElementHandler(new SelectHandler()),
        new FormElementHandler(new CheckboxHandler()),
        new FormElementHandler(new RadioHandler()),
        new FormElementHandler(new TextHandler()),
        new FormElementHandler(new ButtonHandler()),
        new FormElementHandler(new DateHandler())
    ];

    public static async Task FillFromSchemaAsync(IPage page, PageSchema schema, ILogger? log = null)
    {
        foreach (var field in schema.Fields)
        {
            var handler = Handlers.FirstOrDefault(h => h.CanHandle(field));
            if (handler is null)
            {
                log?.LogWarning("No handler for field #{idx} Tag={tag} Type={type}", field.Index, field.Tag, field.Type);
                continue;
            }

            log?.LogInformation("#{idx} {label} ({tag}/{type}) select2={s2} ajax={ajax}",
                field.Index,
                string.IsNullOrWhiteSpace(field.LabelText) ? field.Name : field.LabelText,
                field.Tag,
                field.Type,
                field.IsSelect2,
                field.IsAjax);

            try
            {
                await handler.HandleAsync(page, field, log ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
            }
            catch (Exception ex)
            {
                log?.LogError(ex, "Error handling field: #{idx}", field.Index);
            }
        }
    }
}