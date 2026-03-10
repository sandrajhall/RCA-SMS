using RCA_StudyManagementSystem.Shared.Domain;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.AspNetCore.Components.Forms;
using RCA_StudyManagementSystem.Client.Services;

namespace RCA_StudyManagementSystem.Client.Pages.ReimbursementEntities
{
    public partial class Create : Microsoft.AspNetCore.Components.ComponentBase
    {

        [Parameter]
        public ReimbursementEntity ReimbursementEntity { get; set; }

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

        private IEnumerable<ReimbursementEntity> ReimbursementEntityList = new List<ReimbursementEntity>();



        protected override async Task OnInitializedAsync()
        {
            ReimbursementEntity ??= new();
            EditContext = new EditContext(ReimbursementEntity);

            ReimbursementEntityRCAContact emptyContact = new ReimbursementEntityRCAContact();
            emptyContact.IsPrimaryContact = true; // Set the first contact as primary
            emptyContact.ReimbursementEntityId = ReimbursementEntity.ReimbursementEntityId;
            emptyContact.RCAContactId = Guid.Empty;
            //ReimbursementEntity.ReimbursementEntityRCAContacts.Add(emptyContact);  Don't add the empty contact here, let the user add it

            ReimbursementEntityList = await ReimbursementEntityData.ListReimbursementEntitiesAsync(CancellationToken);
        }

        protected override void OnParametersSet()
        {
            if (ReimbursementEntity == null)
            {
                ReimbursementEntity = new ReimbursementEntity();
            }

            if (EditContext == null || EditContext.Model != ReimbursementEntity)
            {
                EditContext = new EditContext(ReimbursementEntity);
            }
        }

        private async Task OnCancel()
        {
            //EditContext?.MarkAsUnmodified(); // Mark the edit context as unmodified to prevent further validation errors
            SaveMessage = "Reimbursement Entity not added.";

            if (!IsDialog)
            {
                NavigationManager.NavigateTo($"/reimbursemententities/list");
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


                //These two are no longer needed if we are setting the message in the edit form
                severity = Severity.Success; // Set severity to Success if the form is valid
                SaveMessage = "Reimbursement Entity has been saved."; // Set a success message

                try
                {
                    id = await ReimbursementEntityData.CreateReimbursementEntityAsync(ReimbursementEntity);
                    
                    IsSaved = true; // Set IsSaved to true to indicate the form was submitted successfully

                    //Logger.LogInformation("ReimbursementEntity created. {ReimbursementEntity}", System.Text.Json.JsonSerializer.Serialize(ReimbursementEntity));

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
                SaveMessage = "Reimbursement Entity not added.  Please correct the errors."; // Set a failure message
            }

            return id;
        }

        private async Task OnSubmit()
        {
            var id = await Save();

            if (!IsDialog && IsSaved)
            {
                NavigationManager.NavigateTo($"/reimbursemententities/edit/{id}/{IsSaved}");
            }
        }

        private async Task OnSaveAndAddNew()
        {
            var id = await Save();

            if (IsSaved)
            {
                NavigationManager.NavigateTo($"/reimbursemententities/create", forceLoad: true);
            }
        }

        private async Task OnSaveAndClose()
        {
            var id = await Save();

            if (!IsDialog && IsSaved)
            {
                NavigationManager.NavigateTo($"/reimbursemententities/list");
            }
            else
            {
                if (IsSaved)
                {
                    // Set the dialog's result with the saved data
                    MudDialog.Close(DialogResult.Ok(ReimbursementEntity));
                }
            }
        }
    }
}