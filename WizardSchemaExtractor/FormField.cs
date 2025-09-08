using System.Text.Json.Serialization;

namespace WizardSchemaExtractor;

public class FormField
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
    [JsonPropertyName("tag")]
    public string Tag { get; set; } = "";
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    [JsonPropertyName("placeholder")]
    public string Placeholder { get; set; } = "";
    [JsonPropertyName("labelText")]
    public string LabelText { get; set; } = "";
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}