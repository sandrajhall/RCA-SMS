using RCA_StudyManagementSystem.Shared.Domain;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.AspNetCore.Components.Forms;
using RCA_StudyManagementSystem.Client.Services;

namespace RCA_StudyManagementSystem.Client.Pages.Cases
{
    public partial class Edit : Microsoft.AspNetCore.Components.ComponentBase
    {
        [Parameter]
        public Patient Patient { get; set; } = new Patient();

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
        public Guid StudyId
        {
            get => Patient.StudyId; // Expose PatientId for the form
            set
            {
                Patient.StudyId = value; // Set PatientId when the parameter is set
            }
        }

        [Parameter]
        public Guid PatientId
        {
            get => Patient.PatientId; // Expose PatientId for the form
            set
            {
                Patient.PatientId = value; // Set PatientId when the parameter is set
            }
        }

        [Parameter]
        public string? List { get; set; }

        public bool ShowFields = false;


        private IEnumerable<PatientPhoneNumber> PatientPhoneNumbers => Patient?.PatientPhoneNumbers ?? new List<PatientPhoneNumber>();
        private IEnumerable<PathReport> PathReports => Patient?.PathReports ?? new List<PathReport>();


        private MudDataGrid<PatientPhoneNumber> dataGrid;

        private Severity severity = Severity.Success;

        protected string SaveMessage = string.Empty;
        private string? apiResponse = string.Empty;

        protected bool HasErrors = false; // Flag to indicate if there are validation errors
        protected string slideSelectValue = "Yes"; // Default value for the slide select
        protected int studySelectValue = 1; // Default value for the study select
        public string StudyColor = "white"; // Default colour for the patient
        public string prefix = string.Empty; // Prefix for the case number

        private readonly DialogOptions _noHeader = new() { NoHeader = true };
        private MudDialog MudDialog { get; set; }

        protected override async Task OnInitializedAsync()
        {
            EditContext = new EditContext(new Patient()); // Initialize editContext with the Patient
            EditContext.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events
            try
            {
                var response = await PatientData.GetPatientAsync(PatientId);
                Patient = response ?? new Patient(); // Initialize Patient with the response or an empty Patient if null
                StudyId = Patient.StudyId;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching patient: {ex.Message}");
                Patient = new Patient(); // Initialize with an empty Patient on error
            }

            EditContext = new EditContext(Patient);
            EditContext.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events

            StudyColor = Patient.PathReports.FirstOrDefault()?.StudyColor ?? "white"; // Set the study color based on the first PathReport's StudyColor, or default to white if not available

            prefix = Patient.PathReports.FirstOrDefault()?.StudyPrefix ?? string.Empty; // Set the prefix based on the first PathReport's StudyPrefix, or default to an empty string if not available

            //StudyColor = await StudyData.GetStudyColorAsync(Patient.StudyId); // Set the study color based on the Patient's StudyId

            await InvokeAsync(StateHasChanged);
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
            ShowFields = true;

            if (Patient == null)
            {
                Patient = new Patient();
            }

            if (EditContext == null || EditContext.Model != Patient)
            {
                EditContext = new EditContext(Patient);
            }
        }

        private async Task OnCancel()
        {
            //EditContext?.MarkAsUnmodified(); // Mark the edit context as unmodified to prevent further validation errors
            SaveMessage = "Case not updated.";

            switch (List)
            {
                case "Cases":
                    NavigationManager.NavigateTo($"/cases/list");
                    break;
                case "CurrentExports":
                    NavigationManager.NavigateTo($"/exports/list/{StudyId}");
                    break;
                case "PastExports":
                    NavigationManager.NavigateTo($"/exports/pastlist/{StudyId}");
                    break;
                default:
                    NavigationManager.NavigateTo($"/cases/list");
                    break;
            }
        }

        private async Task OnDelete()
        {
            bool? result = await DialogService.ShowMessageBoxAsync(
              "Warning", // Dialog title
              $"Are you sure you want to delete this case and the related records?", // Message
              yesText: "Delete!", // Text for the confirmation button
              cancelText: "Cancel" // Text for the cancel button
          );
            if (result == true) // User clicked 'Delete!'
            {
                await PatientData.DeletePatientAsync(Patient.PatientId);

                await InvokeAsync(StateHasChanged);
                IsSaved = true;
                SaveMessage = "Case deleted.";
                severity = Severity.Warning;
                await InvokeAsync(StateHasChanged);

                NavigationManager.NavigateTo($"/cases/list");
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

                try
                {
                    Patient.PathReports.ToList().ForEach(pr => pr.CaseNumber = Patient.CaseNumber); // Set the case number for each path report
                    Patient.PathReports.ToList().ForEach(pr => pr.StudyPrefix = prefix); // Set the study prefix for each path report
                    Patient.PathReports.ToList().ForEach(pr => pr.StudyColor = StudyColor); // Set the study color for each path report
                    // Make a GET request to the SampleController
                    await PatientData.UpdatePatientAsync(Patient.PatientId, Patient);

                    //Logger.LogInformation("Patient updated. {Patient}", System.Text.Json.JsonSerializer.Serialize(Patient));


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
                SaveMessage = "Case not updated.  Please correct the errors."; // Set a failure message
            }
        }

        private async Task OnSubmit()
        {
            await Save();

        }

        private async Task OnSaveAndClose()
        {
            await Save();
            Task.Delay(1000); // Optional: Add a delay to allow the user to see the save message before navigating away

            if (IsSaved)
            {
                switch (List)
                {
                    case "Cases":
                        NavigationManager.NavigateTo($"/cases/list");
                        break;
                    case "CurrentExports":
                        NavigationManager.NavigateTo($"/exports/list/{StudyId}");
                        break;
                    case "PastExports":
                        NavigationManager.NavigateTo($"/exports/pastlist/{StudyId}");
                        break;
                    default:
                        NavigationManager.NavigateTo($"/cases/list");
                        break;
                }
            }
        }
    }
}