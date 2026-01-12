using PlaywrightNtlmDemo.Schemas;

namespace PlaywrightNtlmDemo.Handlers;

internal static class SelectorUtil
{
    public static string? BuildSelector(FormField f) => !string.IsNullOrEmpty(f.Id) ? $"#{f.Id}" : !string.IsNullOrEmpty(f.Name) ? $"[name='{f.Name}']" : null;
}