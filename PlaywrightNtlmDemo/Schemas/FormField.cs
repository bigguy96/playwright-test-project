namespace PlaywrightNtlmDemo.Schemas;

internal class FormField
{
    public int Index { get; set; }
    public string Tag { get; set; } = "";
    public string Type { get; set; } = "";
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string LabelText { get; set; } = "";
    public bool IsSelect2 { get; set; }
    public bool IsAjax { get; set; }
    public bool Exclude { get; set; }
    public string? Value { get; set; }
}