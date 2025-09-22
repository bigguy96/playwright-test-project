using Microsoft.Playwright;
using WizardSchemaExtractor;

namespace PlaywrightNtlmDemo.Helpers;

public static class FormAutoFiller
{
    /// <summary>
    /// Fills fields from a JSON schema (PageSchema).
    /// Skips buttons automatically and logs labels for debugging.
    /// </summary>
    public static async Task FillFromSchemaAsync(IPage page, PageSchema? schema)
    {
        if (schema == null) return;

        var fieldTypes = new List<string> { "button", "text", "radio", "checkbox", "date", "textarea", "select" };

        foreach (var field in schema.Fields.Where(formField => fieldTypes.Contains(formField.Type) && !string.IsNullOrWhiteSpace(formField.Id) && formField.Placeholder != "--"))
        {
            // Determine selector
            var selector = !string.IsNullOrEmpty(field.Id)
                ? $"#{field.Id}"
                : !string.IsNullOrEmpty(field.Name)
                    ? $"[name='{field.Name}']"
                    : "";

            if (string.IsNullOrEmpty(selector))
            {
                Console.WriteLine($"⚠️ Skipping field {field.Index} (no selector found).");
                continue;
            }

            // Log field
            Console.WriteLine($"#{field.Index} {field.Id} ({field.Tag}) => Filling...");

            // Handle field type
            try
            {
                switch (field.Type.ToLowerInvariant())
                {
                    case "select":
                        {
                            if (field.Id.Equals("AircraftType"))
                            {
                                await HandleSelect2DropdownAsync(page);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(field.Value))
                                {
                                    await page.SelectOptionAsync(selector, new SelectOptionValue { Value = field.Value });
                                }
                                else
                                {
                                    await page.SelectOptionAsync(selector, new SelectOptionValue { Index = 1 });
                                }

                                Console.WriteLine($"Selected value '{field.Value}' for dropdown {field.Id}");
                            }
                        }
                        break;

                    case "checkbox":
                    case "radio":
                        {
                            var radioSelector = $"input[name='{field.Name}'][value='{field.Value}']";
                            await ClickRadioByValueAsync(page, radioSelector);

                        }
                        break;

                    case "text":
                    case "textarea":
                        {
                            await FillInputSafelyAsync(page, selector, field.Value ?? "TEST");
                        }
                        break;

                    case "date":
                        {
                            // Set the date value (format: YYYY-MM-DD)
                            var dateValue= DateTime.Today.AddDays(-10).ToString("yyyy-MM-dd");
                            await page.FillAsync(selector, dateValue);

                            // Verify the value was set
                            var setValue = await page.GetAttributeAsync(selector, "value");
                            Console.WriteLine($"Date set to: {setValue}");
                        }
                        break;

                    case "button":
                        {
                            await page.Locator(selector).ClickAsync();

                            // Wait for specific AJAX response
                            if (selector.ToLowerInvariant().Contains("search"))
                            {
                                var response = await page.WaitForResponseAsync(response => response.Url.Contains("/PFTR-ETVP") && response.Status == 200, new PageWaitForResponseOptions { Timeout = 10000 });
                                Console.WriteLine($"AJAX response received: {response.Url}");
                            }

                            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 10000 });
                            Console.WriteLine("Network idle, AJAX call completed.");
                        }
                        break;
                }
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"❌ Timeout exception: {ex.Message}");
            }
            catch (PlaywrightException ex)
            {
                Console.WriteLine($"❌ Playwright click failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error filling field {field.Index}: {ex.Message}");
            }
        }
    }

    private static async Task ClickRadioByValueAsync(IPage page, string selector)
    {
        // Ensure the radio button exists and is visible
        await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 5000
        });

        // Construct label selector for Bootstrap-styled radio button
        var labelSelector = $"label:has({selector})";
        var inputElement = page.Locator(selector);
        var labelElement = page.Locator(labelSelector);

        // Try clicking the label first (Bootstrap style), fall back to input
        if (await labelElement.CountAsync() > 0)
        {
            await labelElement.ClickAsync();
            Console.WriteLine($"Clicked label for radio button: {selector}");
        }
        else
        {
            await inputElement.ClickAsync();
            Console.WriteLine($"Clicked radio button: {selector}");
        }
    }

    public static async Task HandleSelect2DropdownAsync(IPage page)
    {
        try
        {
            // Wait for the select element
            await page.WaitForSelectorAsync("#AircraftType", new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
            Console.WriteLine("Select element found.");

            // Reinitialize Select2 with description mapping
            await page.EvaluateAsync(@"() => {
                try {
                    $('#AircraftType').select2('destroy');
                    $('#AircraftType').select2({
                        ajax: {
                            url: '/Saf-Sec-Sur/2/PFTR-ETVP-DEV/eng/20/GetAircraftTypes',
                            dataType: 'json',
                            delay: 250,
                            data: function(params) {
                                return { term: params.term, page: params.page || 1 };
                            },
                            processResults: function(data, params) {
                                params.page = params.page || 1;
                                const results = data.results.map(item => ({
                                    id: item.id,
                                    text: item.description
                                }));
                                return {
                                    results: results,
                                    pagination: { more: (params.page * 6) < data.count }
                                };
                            }
                        },
                        placeholder: 'Enter an aircraft type',
                        minimumInputLength: 0,
                        theme: 'bootstrap',
                        language: 'en'
                    });
                    $('.select2-container').removeClass('select2-hidden-accessible');
                    $('.select2-search').css('display', 'block');
                    $('.select2-results__options').css('display', 'block');
                } catch (e) {
                    console.error('Select2 error:', e);
                }
            }");

            // Click the dropdown
            var containerLocator = page.Locator("#select2-AircraftType-container");
            await containerLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 30000 });
            await containerLocator.ClickAsync(new LocatorClickOptions { Timeout = 5000 });
            Console.WriteLine("Clicked #select2-AircraftType-container.");

            // Type the search query
            var searchFieldLocator = page.Locator(".select2-search__field");
            await searchFieldLocator.FillAsync("B73C", new LocatorFillOptions { Force = true });
            Console.WriteLine("Typed search query: B73C");

            // Wait for AJAX response
            var response = await page.WaitForResponseAsync(response => response.Url.Contains("/Saf-Sec-Sur/2/PFTR-ETVP-DEV/eng/20/GetAircraftTypes"), new() { Timeout = 15000 });
            var responseBody = await response.TextAsync();
            Console.WriteLine($"AJAX response: {responseBody}");

            // Force visibility of dropdown options
            await page.EvaluateAsync(@"() => {
                const options = document.querySelector('.select2-results__options');
                if (options) options.style.display = 'block';
                const optionItems = document.querySelectorAll('.select2-results__option');
                optionItems.forEach(item => item.style.display = 'block');
            }");

            // Select the option
            const string targetText = "B73C B737 - 600/700/800 (NG) , B737-8 (MAX)";
            var optionLocator = page.Locator($".select2-results__option:has-text('{targetText}')");
            await optionLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
            await optionLocator.ClickAsync();
            Console.WriteLine($"Selected option: {targetText}");

            // Verify the selection
            var selectedText = await page.Locator("#select2-AircraftType-container").TextContentAsync();
            Console.WriteLine($"Selected text in UI: {selectedText}");
            var selectValue = await page.EvaluateAsync<string>("() => document.querySelector('#AircraftType').value");
            Console.WriteLine($"Underlying <select> value: {selectValue}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task FillInputSafelyAsync(IPage page, string selector, string value)
    {
        await page.FillAsync(selector, value);
    }
}