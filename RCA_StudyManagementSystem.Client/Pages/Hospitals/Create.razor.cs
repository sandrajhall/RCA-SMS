using RCA_StudyManagementSystem.Shared.Domain;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.AspNetCore.Components.Forms;
using RCA_StudyManagementSystem.Client.Services;

namespace RCA_StudyManagementSystem.Client.Pages.Hospitals
{
    public partial class Create : Microsoft.AspNetCore.Components.ComponentBase
    {

        [Parameter]
        public Hospital Hospital { get; set; }

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

        private IEnumerable<Hospital> HospitalList = new List<Hospital>();



        protected override async Task OnInitializedAsync()
        {
            Hospital ??= new();
            EditContext = new EditContext(Hospital);


            HospitalList = await HospitalData.ListHospitalsAsync(CancellationToken);

        }

        protected override void OnParametersSet()
        {
            if (Hospital == null)
            {
                Hospital = new Hospital();
            }

            if (EditContext == null || EditContext.Model != Hospital)
            {
                EditContext = new EditContext(Hospital);
            }
        }

        private async Task OnCancel()
        {
            //EditContext?.MarkAsUnmodified(); // Mark the edit context as unmodified to prevent further validation errors
            SaveMessage = "Hospital not added.";

            if (!IsDialog)
            {
                NavigationManager.NavigateTo($"/hospitals/list");
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
                SaveMessage = "Hospital has been saved."; // Set a success message

                try
                {
                    id = await HospitalData.CreateHospitalAsync(Hospital);

                    IsSaved = true; // Set IsSaved to true to indicate the form was submitted successfully

                    //Logger.LogInformation("Hospital created. {Hospital}", System.Text.Json.JsonSerializer.Serialize(Hospital));

                    EditContext.MarkAsUnmodified(); // Mark the edit context as unmodified to prevent further validation errors



                }
                catch (HttpRequestException ex)
                {
                    apiResponse = $"Error: {ex.Message}"; // Handle potential errors during the API call
                    SaveMessage = apiResponse; // Set the save message to the error message
                    severity = Severity.Error; // Set severity to Error if the form is invalid
                    IsSaved = false; // Set IsSaved to false if there was an error
                }

            }
            else
            {
                IsSaved = false;
                HasErrors = true; // Set HasErrors to true to indicate there are validation errors
                severity = Severity.Error; // Set severity to Error if the form is invalid
                SaveMessage = "Hospital not added.  Please correct the errors."; // Set a failure message
            }

            return id;
        }

        private async Task OnSubmit()
        {
            var id = await Save();

            if (!IsDialog && IsSaved)
            {
                NavigationManager.NavigateTo($"/hospitals/edit/{id}/{IsSaved}");
            }
        }

        private async Task OnSaveAndAddNew()
        {
            var id = await Save();

            if (IsSaved)
            {
                NavigationManager.NavigateTo($"/hospitals/create", forceLoad: true);
            }
        }

        private async Task OnSaveAndClose()
        {
            var id = await Save();

            if (!IsDialog && IsSaved)
            {
                NavigationManager.NavigateTo($"/hospitals/list");
            }
            else
            {
                if (IsSaved)
                {
                    // Set the dialog's result with the saved data
                    MudDialog.Close(DialogResult.Ok(Hospital));
                }
            }
        }
    }
}