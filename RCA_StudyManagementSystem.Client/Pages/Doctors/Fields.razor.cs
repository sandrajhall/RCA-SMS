using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Net.Http.Json;
using System.Text.Json;


namespace RCA_StudyManagementSystem.Client.Pages.Doctors
{
    public partial class Fields : Microsoft.AspNetCore.Components.ComponentBase
    {

        [Parameter]
        public Doctor Doctor { get; set; } = new Doctor();

        [CascadingParameter]
        public EditContext? EditContext { get; set; }


        [Parameter]
        public bool IsSaved { get; set; }

        public CancellationToken CancellationToken { get; set; } = new CancellationToken();
        private GeoapifySuggestion? SelectedValue { get; set; }




        private CancellationToken cancellationToken { get; set; } = new CancellationToken();
        private List<GeoapifySuggestion> Suggestions = new();
        private string SelectedAddress = "";
        private string GeoapifyApiKey = String.Empty; // Securely obtain API key in production


        protected override async Task OnInitializedAsync()
        {
            GeoapifyApiKey = Configuration["Geoapify:ApiKey"]!;

            EditContext!.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events
           
            if (Doctor.VerifiedDate < DateTime.Now.AddMonths(-6) && Doctor.IsVerified)
            {
                Doctor.IsVerified = false;
                Doctor.VerifiedDate = null;
                await DoctorData.UpdateDoctorAsync(Doctor.DoctorId, Doctor);
            }
        }


        private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
        {

            IsSaved = false;
            // Logic to execute when a field changes
            // e.FieldIdentifier provides information about the changed field
            //Console.WriteLine($"Field in child '{e.FieldIdentifier.FieldName}' changed.");

        }

        private async Task HandleInternalNavigation(LocationChangingContext context)
        {
            if (EditContext!.IsModified())
            {
                var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "You have unsaved changes. Do you want to leave?");
                if (!confirmed)
                {
                    context.PreventNavigation();
                }
            }
        }

        private void OnVerifiedChanged(bool verified)
        {
            Doctor.IsVerified = verified;
            if (verified)
            {
                Doctor.VerifiedDate = DateTime.UtcNow;
            }
            else
            {
                Doctor.VerifiedDate = null;
            }
        }


        private async Task<IEnumerable<GeoapifySuggestion>> Search(string text, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < 3)
            {
                Suggestions.Clear();
                return null!;
            }

            try
            {
                var url = $"https://api.geoapify.com/v1/geocode/autocomplete?text={text}&apiKey={GeoapifyApiKey}";
                var client = ClientFactory.CreateClient("Geoapify");
                var response = await client.GetFromJsonAsync<GeoapifyAutocompleteResponse>(url);
                if (response == null)
                {
                    Console.WriteLine("No response received.");
                    Suggestions.Clear();
                    return null!;
                }
                if (response?.Features != null)
                {
                    Suggestions = response.Features
                                          .Select(f => new GeoapifySuggestion
                                          {
                                              FormattedAddress = f.Properties.Formatted,
                                              City = f.Properties.City,
                                              State_Code = f.Properties.State_Code,
                                              Postcode = f.Properties.Postcode,
                                              Address_line1 = f.Properties.Address_line1,
                                              Address_line2 = f.Properties.Address_line2,
                                              County = f.Properties.County,
                                          })
                                          .ToList();
                    return Suggestions;

                }
                else
                {
                    Suggestions.Clear();
                    return null!;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching suggestions: {ex.Message}");
                Suggestions.Clear();
                return null!;

            }
            catch (NotSupportedException) // When content type is not valid
            {
                Console.WriteLine("The content type is not supported.");
                Suggestions.Clear();
                return null!;

            }
            catch (JsonException) // Invalid JSON
            {
                Console.WriteLine("Invalid JSON.");
                Suggestions.Clear();
                return null!;

            }
        }

        private async Task HandleSelection(GeoapifySuggestion newValue)
        {
            SelectedValue = newValue; // Update the component's value
            await SelectAddress(newValue);
        }

        private async Task SelectAddress(GeoapifySuggestion suggestion)
        {

            //SelectedAddress = suggestion.FormattedAddress;
            //SearchText = suggestion.FormattedAddress; // Update the input field with the selected address
            Doctor.City = suggestion.City;
            Doctor.State = suggestion.State_Code;
            Doctor.ZipCode = suggestion.Postcode;
            Doctor.Address1 = suggestion.Address_line1;
            Doctor.Address2 = suggestion.Address_line2.Split(',').FirstOrDefault()?.Trim() ?? "";
            Doctor.County = suggestion.County.Remove(suggestion.County.Length - 7, 7); ;
            Suggestions.Clear(); // Clear the suggestions after selection

            var fieldIdentifierCity = new FieldIdentifier(Doctor, nameof(Doctor.City));
            var fieldIdentifierState = new FieldIdentifier(Doctor, nameof(Doctor.State));
            var fieldIdentifierZip = new FieldIdentifier(Doctor, nameof(Doctor.ZipCode));
            var fieldIdentifierAddress1 = new FieldIdentifier(Doctor, nameof(Doctor.Address1));
            var fieldIdentifierAddress2 = new FieldIdentifier(Doctor, nameof(Doctor.Address2));

            EditContext.NotifyFieldChanged(fieldIdentifierCity); // Refresh the UI to reflect the changes
            EditContext.NotifyFieldChanged(fieldIdentifierState); // Refresh the UI to reflect the changes
            EditContext.NotifyFieldChanged(fieldIdentifierZip); // Refresh the UI to reflect the changes
            EditContext.NotifyFieldChanged(fieldIdentifierAddress1); // Refresh the UI to reflect the changes
            //EditContext.NotifyFieldChanged(fieldIdentifierAddress2); // Refresh the UI to reflect the changes
            await InvokeAsync(StateHasChanged);
        }

        // You would define these classes based on the Geoapify API response structure
        public class GeoapifyAutocompleteResponse
        {
            public List<Feature> Features { get; set; } = new();
        }

        public class Feature
        {
            public Properties Properties { get; set; } = new();
        }

        public class Properties
        {
            public string Formatted { get; set; } = "";

            // Add other properties like street, city, postcode, etc. as needed
            public string City { get; set; } = "";
            public string Postcode { get; set; } = "";
            public string State_Code { get; set; } = "";
            public string Address_line1 { get; set; } = "";
            public string Address_line2 { get; set; } = "";
            public string County { get; set; } = "";
        }

        public class GeoapifySuggestion
        {
            public string FormattedAddress { get; set; } = "";


            // You might want to store other properties from the Geoapify response here

            public string City { get; set; } = "";
            public string Postcode { get; set; } = "";
            public string State_Code { get; set; } = "";
            public string Address_line1 { get; set; } = "";
            public string Address_line2 { get; set; } = "";
            public string County { get; set; } = "";
        }
    }

}
