using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using PlaywrightNtlmDemo.Schemas;

namespace PlaywrightNtlmDemo.Handlers;

internal interface IFieldHandler
{
    bool CanHandle(FormField field);
    Task HandleAsync(IPage page, FormField field, ILogger log);
}