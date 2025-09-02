using Microsoft.Playwright;

namespace PlaywrightNtlmDemo.Helpers
{
    public static class FormAutoFiller
    {
        /// <summary>
        /// Fills out all visible form elements on the page with default test data.
        /// Supports text inputs, textareas, checkboxes, radios, and select dropdowns.
        /// </summary>
        /// <param name="page">The Playwright IPage instance.</param>
        public static async Task FillAllFormFieldsAsync(IPage page)
        {
            // Fill text inputs and textareas
            var textFields = await page.QuerySelectorAllAsync("input[type='text'], input[type='email'], input[type='number'], textarea");
            foreach (var field in textFields)
            {
                var name = await field.GetAttributeAsync("name") ?? await field.GetAttributeAsync("id") ?? "field";
                Console.WriteLine($"📝 Filling text field: {name}");
                await field.FillAsync("TestValue");
            }

            // Fill password fields
            var passwordFields = await page.QuerySelectorAllAsync("input[type='password']");
            foreach (var field in passwordFields)
            {
                var name = await field.GetAttributeAsync("name") ?? await field.GetAttributeAsync("id") ?? "password";
                Console.WriteLine($"🔒 Filling password field: {name}");
                await field.FillAsync("P@ssword123");
            }

            // Handle checkboxes
            var checkboxes = await page.QuerySelectorAllAsync("input[type='checkbox']");
            foreach (var checkbox in checkboxes)
            {
                var isChecked = await checkbox.IsCheckedAsync();
                if (!isChecked)
                {
                    var name = await checkbox.GetAttributeAsync("name") ?? await checkbox.GetAttributeAsync("id") ?? "checkbox";
                    Console.WriteLine($"☑️ Checking: {name}");
                    await checkbox.CheckAsync();
                }
            }

            // Handle radio buttons (select first option in each group)
            var radios = await page.QuerySelectorAllAsync("input[type='radio']");
            var groupedRadios = radios.GroupBy(async r => await r.GetAttributeAsync("name")).ToList();
            foreach (var group in groupedRadios)
            {
                var firstRadio = group.FirstOrDefault();
                if (firstRadio != null && !await firstRadio.IsCheckedAsync())
                {
                    var name = await firstRadio.GetAttributeAsync("name") ?? "radio";
                    Console.WriteLine($"🔘 Selecting radio option: {name}");
                    await firstRadio.CheckAsync();
                }
            }

            // Handle dropdown selects
            var selects = await page.QuerySelectorAllAsync("select");
            foreach (var select in selects)
            {
                var options = await select.QuerySelectorAllAsync("option");
                if (options.Any())
                {
                    var value = await options.Last().GetAttributeAsync("value");
                    var name = await select.GetAttributeAsync("name") ?? await select.GetAttributeAsync("id") ?? "select";
                    Console.WriteLine($"⬇️ Selecting last option for: {name}");
                    await select.SelectOptionAsync(new[] { value });
                }
            }
        }
    }
}