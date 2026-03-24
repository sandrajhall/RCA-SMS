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


namespace RCA_StudyManagementSystem.Client.Pages.ReimbursementEntities
{
    public partial class Fields : Microsoft.AspNetCore.Components.ComponentBase
    {

        [Parameter]
        public ReimbursementEntity ReimbursementEntity { get; set; } = new ReimbursementEntity();

        [CascadingParameter]
        public EditContext? EditContext { get; set; }


        [Parameter]
        public bool IsSaved { get; set; }

        public CancellationToken CancellationToken { get; set; } = new CancellationToken();
        private GeoapifySuggestion? SelectedValue { get; set; }


        private MudDataGrid<ReimbursementEntityRCAContact>? dataGrid;


        private CancellationToken cancellationToken { get; set; } = new CancellationToken();
        private List<GeoapifySuggestion> Suggestions = new();
        private string SelectedAddress = "";
        private string GeoapifyApiKey = String.Empty; // Securely obtain API key in production

        private List<RCAContact> rcaContacts = new();
        private CancellationToken token;

        private DateTime? modDate;
        private string? modUser;

        protected override async Task OnInitializedAsync()
        {

            GeoapifyApiKey = Configuration["Geoapify:ApiKey"]!;

            EditContext!.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events

            rcaContacts = (List<RCAContact>)await RCAContactData.ListRCAContactsAsync(token);
            StateHasChanged();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (ReimbursementEntity != null && ReimbursementEntity.ModifiedDate.HasValue && ReimbursementEntity.ModifiedUserId.HasValue)
            {
                modDate = ReimbursementEntity.ModifiedDate.Value.ToLocalTime();
                modUser = await UserData.GetDisplayNameAsync(ReimbursementEntity.ModifiedUserId.ToString());
            }
            else
            {
                modDate = default; // Or DateTime.MinValue, or null if modDate is nullable
                modUser = null;    // Or string.Empty, as appropriate
            }

            await InvokeAsync(StateHasChanged); // Optional; may not be needed if you're already in lifecycle
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


        private string GetDisplayText(Guid id)
        {
            var contact = rcaContacts.Where(x => x.RCAContactId == id).FirstOrDefault();
            var name = contact != null ? $"{contact.LastName}, {contact.FirstName}" : "Unknown";
            return name;
        }



        private async Task AddNewContact()
        {
            var newItem = new ReimbursementEntityRCAContact();
            if (ReimbursementEntity.ReimbursementEntityRCAContacts.Count == 0)
            {
                newItem.IsPrimaryContact = true; // Set the first phone number as primary
            }
            else
            {
                newItem.IsPrimaryContact = false; // Subsequent items are not primary by default
            }
            newItem.ReimbursementEntityId = ReimbursementEntity.ReimbursementEntityId;
            newItem.ReimbursementEntityRCAContactId = Guid.NewGuid();

            ReimbursementEntity.ReimbursementEntityRCAContacts.Add(newItem); // Add to your data source
            await InvokeAsync(StateHasChanged);
            await Task.Delay(100); // Give the UI a moment to update
            //await dataGrid.SetEditingItemAsync(newItem); // Start inline editing - shows modal
        }

        private async Task DeleteContact(ReimbursementEntityRCAContact item)
        {
            if (item.RCAContactId == Guid.Empty)
            {
                ReimbursementEntity.ReimbursementEntityRCAContacts.Remove(item); // Remove from data source
                await InvokeAsync(StateHasChanged);
                return; // Exit if the phone number is empty
            }

            bool? result = await DialogService.ShowMessageBoxAsync(
                          "Warning", // Dialog title
                          $"Are you sure you want to delete this contact? Save the record to complete deletion.", // Message
                          yesText: "Delete!", // Text for the confirmation button
                          cancelText: "Cancel" // Text for the cancel button
                      );

            if (result == true) // User clicked 'Delete!'
            {
                ReimbursementEntity.ReimbursementEntityRCAContacts.Remove(item); // Remove from data source
                await InvokeAsync(StateHasChanged);
            }

        }

        private void CheckPrimaryValue(bool isChecked, ReimbursementEntityRCAContact item)
        {
            if (!isChecked)
            {
                item.IsPrimaryContact = false; // If unchecked, just set IsPrimary to false
                return;
            }

            item.IsPrimaryContact = isChecked;
            if (item.IsPrimaryContact)
            {
                // Uncheck all other items
                foreach (var contact in ReimbursementEntity.ReimbursementEntityRCAContacts)
                {
                    if (contact != item)
                    {
                        contact.IsPrimaryContact = false;
                    }
                }
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
                                              County = f.Properties.County
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
            ReimbursementEntity.City = suggestion.City;
            ReimbursementEntity.State = suggestion.State_Code;
            ReimbursementEntity.ZipCode = suggestion.Postcode;
            ReimbursementEntity.AddressLine1 = suggestion.Address_line1;
            //Hospital.County = suggestion.County;
            //Hospital.Address2 = suggestion.Address_line2; // Assuming Address2 is part of the suggestion
            Suggestions.Clear(); // Clear the suggestions after selection

            var fieldIdentifierCity = new FieldIdentifier(ReimbursementEntity, nameof(ReimbursementEntity.City));
            var fieldIdentifierState = new FieldIdentifier(ReimbursementEntity, nameof(ReimbursementEntity.State));
            var fieldIdentifierZip = new FieldIdentifier(ReimbursementEntity, nameof(ReimbursementEntity.ZipCode));
            var fieldIdentifierAddress1 = new FieldIdentifier(ReimbursementEntity, nameof(ReimbursementEntity.AddressLine1));
            //var fieldIdentifierCounty = new FieldIdentifier(Hospital, nameof(Hospital.County));
            //var fieldIdentifierAddress2 = new FieldIdentifier(Hospital, nameof(Hospital.Address2));

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
