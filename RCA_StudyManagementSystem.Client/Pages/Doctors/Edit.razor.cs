using RCA_StudyManagementSystem.Shared.Domain;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.AspNetCore.Components.Forms;
using RCA_StudyManagementSystem.Client.Services;

namespace RCA_StudyManagementSystem.Client.Pages.Doctors
{
    public partial class Edit : Microsoft.AspNetCore.Components.ComponentBase
    {
        //[SupplyParameterFromForm]
        [Parameter]
        public Doctor Doctor { get; set; } = new Doctor();

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
        public Guid DoctorId
        {
            get => Doctor.DoctorId; // Expose DoctorId for the form
            set
            {
                Doctor.DoctorId = value; // Set DoctorId when the parameter is set
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
            EditContext = new EditContext(new Doctor()); // Initialize editContext with the Doctor
            EditContext.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events
            try
            {
                var response = await DoctorData.GetDoctorAsync(DoctorId);
                Doctor = response ?? new Doctor(); // Initialize Doctor with the response or an empty Doctor if null

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching doctor: {ex.Message}");
                Doctor = new Doctor(); // Initialize with an empty Doctor on error
            }

            EditContext = new EditContext(Doctor);
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
            SaveMessage = "Doctor not updated.";

            if (!IsDialog)
            {
                NavigationManager.NavigateTo($"/app/doctors/list");
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

                HasErrors = false;

                severity = Severity.Success; // Set severity to Success if the form is valid
                SaveMessage = "Doctor updated successfully!"; // Set a success message

                Doctor.DisplayName = $"{Doctor.FirstName} {Doctor.LastName}, {Doctor.LicenseType}";

                try
                {
                    var auth = await AuthStateProvider.GetAuthenticationStateAsync();
                    var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    // Make a GET request to the SampleController
                    await DoctorData.UpdateDoctorAsync(Doctor.DoctorId, userId, Doctor);

                    SaveMessage = "Doctor updated.";
                    severity = Severity.Success; // Set severity to Success if the form is valid
                    IsSaved = true; // Set IsSaved to true to indicate the form was submitted successfully
                    Snackbar.Add(SaveMessage, severity);

                    //Logger.LogInformation("Doctor updated. {Doctor}", System.Text.Json.JsonSerializer.Serialize(Doctor));

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
                SaveMessage = "Doctor not updated.  Please correct the errors."; // Set a failure message
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