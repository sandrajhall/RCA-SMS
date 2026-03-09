using RCA_StudyManagementSystem.Shared.Domain;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.AspNetCore.Components.Forms;
using RCA_StudyManagementSystem.Client.Services;

namespace RCA_StudyManagementSystem.Client.Pages.DoNotContacts
{
    public partial class Edit : ComponentBase
    {
        //[SupplyParameterFromForm]
        [Parameter]
        public DoNotContact DoNotContact { get; set; } = new DoNotContact();

        [CascadingParameter]
        IMudDialogInstance MudDialog { get; set; }

        [CascadingParameter]
        private EditContext? EditContext { get; set; }

        private bool _isSaved = false; // Backing field for IsSaved property

        [Parameter]
        public bool IsSaved
        {
            get => _isSaved; // Expose IsSaved for the form
            set
            {
                if (value == null)
                {
                    _isSaved = false; // Default to false if value is null
                }
                else
                {
                    // Ensure that the value is a boolean
                    if (value.GetType() != typeof(bool))
                    {
                        throw new ArgumentException("IsSaved must be a boolean value.");
                    }
                }
                _isSaved = value; // Set IsSaved when the parameter is set
            }
        }


        [Parameter]
        public Guid DoNotContactId
        {
            get => DoNotContact.DoNotContactId; // Expose DoNotContactId for the form
            set
            {
                DoNotContact.DoNotContactId = value; // Set DoNotContactId when the parameter is set
            }
        }

        [Parameter]
        public bool IsDialog { get; set; } = false; 



        private Severity severity = Severity.Success;

        protected string SaveMessage = string.Empty;
        private string? apiResponse = string.Empty;

        protected bool HasErrors = false; // Flag to indicate if there are validation errors

        private readonly DialogOptions _noHeader = new() { NoHeader = true };

        protected override async Task OnInitializedAsync()
        {
            // pause for 1 second to simulate loading
            EditContext = new EditContext(new DoNotContact()); // Initialize editContext with the DoNotContact
            EditContext.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events
            try
            {
                var response = await DoNotContactData.GetDoNotContactAsync(DoNotContactId);
                DoNotContact = response ?? new DoNotContact(); // Initialize DoNotContact with the response or an empty DoNotContact if null

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching Do Not Contact: {ex.Message}");
                DoNotContact = new DoNotContact(); // Initialize with an empty DoNotContact on error
            }

            EditContext = new EditContext(DoNotContact);
            EditContext.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events
        }



        private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
        {
            // Logic to execute when a field changes
            // e.FieldIdentifier provides information about the changed field
            //Console.WriteLine($"Field '{e.FieldIdentifier.FieldName}' changed.");

            IsSaved = false;
        }

        protected override void OnParametersSet()
        {
            if (DoNotContact == null)
            {
                DoNotContact = new DoNotContact();
            }

            if (EditContext == null || EditContext.Model != DoNotContact)
            {
                EditContext = new EditContext(DoNotContact);
            }
        }

        private async Task OnCancel()
        {
            //EditContext?.MarkAsUnmodified(); // Mark the edit context as unmodified to prevent further validation errors
            SaveMessage = "Do Not Contact not updated.";

            if (!IsDialog)
            {
                NavigationManager.NavigateTo($"/donotcontact/list");
            }
            else
            {
                MudDialog.Cancel();
            }
        }

        private async Task Save()
        {
            // Manually trigger validation
            bool isValid = EditContext.Validate();

            if (isValid)
            {

                IsSaved = true; // Set IsSaved to true to indicate the form was submitted successfully
                HasErrors = false;

                severity = Severity.Success; // Set severity to Success if the form is valid
                SaveMessage = "Do Not Contact updated successfully!"; // Set a success message

                DoNotContact.DisplayName = $"{DoNotContact.FirstName} {DoNotContact.LastName}".Trim();


                try
                {
                    // Make a GET request to the SampleController
                    await DoNotContactData.UpdateDoNotContactAsync(DoNotContact.DoNotContactId, DoNotContact);

                    //Logger.LogInformation("DoNotContact updated. {DoNotContact}", System.Text.Json.JsonSerializer.Serialize(DoNotContact));

                    EditContext.MarkAsUnmodified(); // Mark the edit context as unmodified to prevent further validation errors

                }
                catch (HttpRequestException ex)
                {
                    apiResponse = $"Error: {ex.Message}"; // Handle potential errors during the API call
                    SaveMessage = apiResponse; // Set the save message to the error message
                }
            }
            else
            {
                IsSaved = false;
                HasErrors = true; // Set HasErrors to true to indicate there are validation errors
                severity = Severity.Error; // Set severity to Error if the form is invalid
                SaveMessage = "Do Not Contact not updated.  Please correct the errors."; // Set a failure message
            }
        }


        private async Task OnSubmit()
        {
            await Save();
        }

        private async Task OnSaveAndClose()
        {
            await Save();

            if (!IsDialog && IsSaved)
            {
                NavigationManager.NavigateTo($"/donotcontact/list");
            }
            else
            {
                if (IsSaved)
                {
                    // Set the dialog's result with the saved data
                    MudDialog.Close(DialogResult.Ok(DoNotContact));
                }
            }
        }
    }
}