     namespace WizardSchemaExtractor;

     public class PageSchema
     {
         public string PageTitle { get; set; } = "";
         public string Heading { get; set; } = "";
         public List<FormField> Fields { get; set; } = new();
     }