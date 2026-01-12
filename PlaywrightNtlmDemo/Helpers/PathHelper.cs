namespace PlaywrightNtlmDemo.Helpers;

internal static class PathHelper
{
    public static string ResolveWizardFormsFolder()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var root = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\"));
        var folder = Path.Combine(root, "Steps");

        return folder;
    }
}