using RCA_StudyManagementSystem.Shared.Domain;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.AspNetCore.Components.Forms;
using RCA_StudyManagementSystem.Client.Services;

namespace RCA_StudyManagementSystem.Client.Pages.Doctors
{
    public partial class Create : Microsoft.AspNetCore.Components.ComponentBase
    {

        [Parameter]
        public Doctor Doctor { get; set; }

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

        protected string color = "white"; // Default colour for the doctor

        private IEnumerable<Doctor> DoctorList = new List<Doctor>();



        protected override async Task OnInitializedAsync()
        {
            Doctor ??= new();
            EditContext = new EditContext(Doctor);


            //DoctorList = await DoctorData.ListDoctorsAsync(CancellationToken);

        }

        protected override void OnParametersSet()
        {
            if (Doctor == null)
            {
                Doctor = new Doctor();
            }

            if (EditContext == null || EditContext.Model != Doctor)
            {
                EditContext = new EditContext(Doctor);
            }
        }

        private async Task OnCancel()
        {
            //EditContext?.MarkAsUnmodified(); // Mark the edit context as unmodified to prevent further validation errors
            SaveMessage = "Doctor not added.";
            if (!IsDialog)
            {
                NavigationManager.NavigateTo($"/app/doctors/list");
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

                Doctor.DisplayName = $"{Doctor.FirstName} {Doctor.LastName}, {Doctor.LicenseType}";

                try
                {
                    var auth = await AuthStateProvider.GetAuthenticationStateAsync();
                    var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    id = await DoctorData.CreateDoctorAsync(userId, Doctor);

                    IsSaved = true; // Set IsSaved to true to indicate the form was submitted successfully

                    //Logger.LogInformation("Doctor created. {Doctor}", System.Text.Json.JsonSerializer.Serialize(Doctor));

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
                SaveMessage = "Doctor not added.  Please correct the errors."; // Set a failure message
            }

            return id;
        }

        private async Task OnSubmit()
        {
            var id =  await Save();

            if (!IsDialog && IsSaved)
            {
                NavigationManager.NavigateTo($"/app/doctors/edit/{id}/{IsSaved}");
            }
        }

        private async Task OnSaveAndAddNew()
        {
            var id = await Save();

            if (IsSaved)
            {
                Doctor = new Doctor();
                EditContext = new EditContext(Doctor);
                EditContext.MarkAsUnmodified();

                SaveMessage = "Saved successfully. You can now add another.";
                StateHasChanged();
            }
        }

        private async Task OnSaveAndClose()
        {
            var id = await Save();

            if (!IsDialog && IsSaved)
            {
                NavigationManager.NavigateTo($"/app/doctors/list");
            }
            else
            {
                if (IsSaved)
                {
                    // Set the dialog's result with the saved data
                    MudDialog.Close(DialogResult.Ok(Doctor));
                }
            }
        }
    }
}