using RCA_StudyManagementSystem.Shared.Domain;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.AspNetCore.Components.Forms;
using RCA_StudyManagementSystem.Client.Services;

namespace RCA_StudyManagementSystem.Client.Pages.RCAContacts
{
    public partial class Create : Microsoft.AspNetCore.Components.ComponentBase
    {

        [Parameter]
        public RCAContact RCAContact { get; set; }

        EditContext EditContext;

        public CancellationToken CancellationToken { get; set; } = new CancellationToken();

        [CascadingParameter]
        IMudDialogInstance MudDialog { get; set; }

        [Parameter]
        public bool IsDialog { get; set; }


        private Severity severity = Severity.Success;

        protected string SaveMessage = string.Empty;
        private string? apiResponse = string.Empty;
        protected bool IsSaved = false;
        protected bool ShowFields = false;
        protected bool HasErrors = false; // Flag to indicate if there are validation errors

        protected string color = "white"; // Default colour for the hospital

        private IEnumerable<RCAContact> RCAContactList = new List<RCAContact>();



        protected override async Task OnInitializedAsync()
        {
            RCAContact ??= new();
            EditContext = new EditContext(RCAContact);


            RCAContactList = await RCAContactData.ListRCAContactsAsync(CancellationToken);

        }

        protected override void OnParametersSet()
        {
            if (RCAContact == null)
            {
                RCAContact = new RCAContact();
            }

            if (EditContext == null || EditContext.Model != RCAContact)
            {
                EditContext = new EditContext(RCAContact);
            }
        }

        private async Task OnCancel()
        {
            //EditContext?.MarkAsUnmodified(); // Mark the edit context as unmodified to prevent further validation errors
            SaveMessage = "RCAContact not added.";

            if (!IsDialog)
            {
                NavigationManager.NavigateTo($"/rcacontacts/list");
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

                IsSaved = true; // Set IsSaved to true to indicate the form was submitted successfully
                HasErrors = false;


                //These two are no longer needed if we are setting the message in the edit form
                severity = Severity.Success; // Set severity to Success if the form is valid
                SaveMessage = "RCAContact has been saved."; // Set a success message

                try
                {
                    id = await RCAContactData.CreateRCAContactAsync(RCAContact);

                    //Logger.LogInformation("RCAContact created. {RCAContact}", System.Text.Json.JsonSerializer.Serialize(RCAContact));

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
                SaveMessage = "RCAContact not added.  Please correct the errors."; // Set a failure message
            }

            return id;
        }

        private async Task OnSubmit()
        {
            var id = await Save();

            if (!IsDialog)
            {
                NavigationManager.NavigateTo($"/rcacontacts/edit/{id}/{IsSaved}");
            }
        }

        private async Task OnSaveAndAddNew()
        {
            var id = await Save();

            NavigationManager.NavigateTo($"/rcacontacts/create", forceLoad: true);

        }

        private async Task OnSaveAndClose()
        {
            var id = await Save();

            if (!IsDialog)
            {
                NavigationManager.NavigateTo($"/rcacontacts/list");
            }
            else
            {
                // Set the dialog's result with the saved data
                MudDialog.Close(DialogResult.Ok(RCAContact));
            }
        }
    }
}