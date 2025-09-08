    namespace WizardSchemaExtractor;

    public class FormField
    {
        public int Index { get; set; }
        public string Tag { get; set; } = "";
        public string Type { get; set; } = "";
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Placeholder { get; set; } = "";
        public string LabelText { get; set; } = "";
        public string? Value { get; set; }
    }