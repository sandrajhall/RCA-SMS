using RCA_StudyManagementSystem.Shared.Domain;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.AspNetCore.Components.Forms;
using RCA_StudyManagementSystem.Client.Services;

namespace RCA_StudyManagementSystem.Client.Pages.DoNotContacts
{
    public partial class Create : ComponentBase
    {

        [Parameter]
        public DoNotContact DoNotContact { get; set; }

        [CascadingParameter]
        IMudDialogInstance MudDialog { get; set; }

        public CancellationToken CancellationToken { get; set; } = new CancellationToken();

        EditContext EditContext;

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
        public bool IsDialog { get; set; }

        private Severity severity = Severity.Success;

        protected string SaveMessage = string.Empty;
        private string? apiResponse = string.Empty;
        protected bool ShowFields = false;
        protected bool HasErrors = false; // Flag to indicate if there are validation errors

        protected string color = "white"; // Default colour for do not contact

        private IEnumerable<DoNotContact> DoNotContactList = new List<DoNotContact>();



        protected override async Task OnInitializedAsync()
        {
            DoNotContact ??= new();
            EditContext = new EditContext(DoNotContact);


            DoNotContactList = await DoNotContactData.ListDoNotContactsAsync(CancellationToken);

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
            SaveMessage = "Do Not Contact not added.";
            if (!IsDialog)
            {
                NavigationManager.NavigateTo($"/app/donotcontact/list");
            }
            else
            {
                MudDialog.Cancel();
            }
        }

        private async Task<Guid> Save()
        {
            var id = Guid.Empty;
            // Manually trigger validation
            bool isValid = EditContext.Validate();

            if (isValid)
            {
                HasErrors = false;

                DoNotContact.DisplayName = $"{DoNotContact.FirstName} {DoNotContact.LastName}".Trim();

                try
                {
                    id = await DoNotContactData.CreateDoNotContactAsync(DoNotContact);

                    IsSaved = true; // Set IsSaved to true to indicate the form was submitted successfully

                    //Logger.LogInformation("DoNotContact created. {DoNotContact}", System.Text.Json.JsonSerializer.Serialize(DoNotContact));

                    EditContext.MarkAsUnmodified(); // Mark the edit context as unmodified to prevent further validation errors

                }
                catch (HttpRequestException ex)
                {
                    apiResponse = $"Error: {ex.Message}"; // Handle potential errors during the API call
                    SaveMessage = apiResponse; // Set the save message to the error message
                    severity = Severity.Error; // Set severity to Error if the form is invalid
                    IsSaved = false; // Set IsSaved to false if there was an error
                    HasErrors = true; // Set HasErrors to true to indicate there are errors
                }
            }
            else
            {
                IsSaved = false;
                HasErrors = true; // Set HasErrors to true to indicate there are validation errors
                severity = Severity.Error; // Set severity to Error if the form is invalid
                SaveMessage = "Do Not Contact not added.  Please correct the errors."; // Set a failure message
            }

            return id;
        }

        private async Task OnSubmit()
        {
            var id =  await Save();

            if (!IsDialog && IsSaved)
            {
                NavigationManager.NavigateTo($"/app/donotcontact/edit/{id}/{IsSaved}");
            }
        }

        private async Task OnSaveAndAddNew()
        {
            var id = await Save();

            if (IsSaved)
            {
                NavigationManager.NavigateTo($"/app/donotcontact/create", forceLoad: true);
            }
        }

        private async Task OnSaveAndClose()
        {
            var id = await Save();

            if (!IsDialog && IsSaved)
            {
                NavigationManager.NavigateTo($"/app/donotcontact/list");
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