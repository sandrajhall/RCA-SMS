using RCA_StudyManagementSystem.Shared.Domain;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.AspNetCore.Components.Forms;
using RCA_StudyManagementSystem.Client.Services;

namespace RCA_StudyManagementSystem.Client.Pages.Cases
{
    public partial class Create : Microsoft.AspNetCore.Components.ComponentBase
    {

        [Parameter]
        public Patient Patient { get; set; }

        EditContext EditContext;


        public Guid StudyId { get; set; }

        private IEnumerable<PatientPhoneNumber> PatientPhoneNumbers => Patient?.PatientPhoneNumbers ?? new List<PatientPhoneNumber>();
        private IEnumerable<PathReport> PathReports => Patient?.PathReports ?? new List<PathReport>();


        private MudDataGrid<PatientPhoneNumber> dataGrid;

        private Severity severity = Severity.Success;

        protected string SaveMessage = string.Empty;
        private string? apiResponse = string.Empty;
        protected bool IsSaved = false;
        protected bool ShowFields = false;
        protected bool HasErrors = false; // Flag to indicate if there are validation errors

        protected string slideSelectValue = "Yes"; // Default value for the slide select
        protected Study studySelectValue;
        protected string color = "white"; // Default colour for the patient

        private IEnumerable<Study> StudyList = new List<Study>();
        private string prefix = string.Empty; // Prefix for the case number

        [Parameter]
        public string StudyColor { get; set; } = string.Empty; // Color associated with the study, not mapped to the database

        [Parameter]
        public EventCallback<string> StudyColorChanged { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Patient ??= new();
            EditContext = new EditContext(Patient);
            PatientPhoneNumber emptyPhone = new PatientPhoneNumber();
            emptyPhone.IsPrimary = true; // Set the first phone number as primary
            Patient.PatientPhoneNumbers.Add(emptyPhone);
            PathReport emptyReport = new PathReport();
            emptyReport.SlidesResideAtSubmittingHospital = "Yes"; // Set the default value for SlidesResideAtSubmittingHospital
            emptyReport.PathIndex = Patient.PathReports.Count + 1; // Set the index for the new PathReport
            Patient.PathReports.Add(emptyReport);

            StudyList = await StudyData.ListStudiesAsync();

        }

        protected override void OnParametersSet()
        {
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
            SaveMessage = "Case not added.";
            NavigationManager.NavigateTo($"/cases/list");
        }

        private void SetPatientHistoricalData()
        {
            var phoneNumberList = new List<string>();
            var phoneNumbers = string.Empty;

            foreach (var phone in Patient.PatientPhoneNumbers.OrderBy(x => x.IsPrimary))
            {
                var phoneStr = phone.PhoneNumber;
                phoneStr += "("+phone.PhoneType+")";
                if (phone.IsPrimary)
                    phoneStr += "(primary)";
                if (phone.PhoneNumberComments?.Length > 0)
                    phoneStr += "(" + phone.PhoneNumberComments + ")";
                phoneNumbers += phoneStr + ", ";
            }
            if (phoneNumbers.Length > 2)
            {
                phoneNumbers = phoneNumbers.Remove(phoneNumbers.Length - 2, 2);
            }
            foreach (var path in Patient.PathReports)
            {
                path.DxAddress1 = Patient.Address1;
                path.DxAddress2 = Patient.Address2;
                path.DxCity = Patient.City;
                path.DxState = Patient.State;
                path.DxZipCode = Patient.ZipCode;
                path.DxCountyCode = Patient.CountyCode;
                path.DxCounty = Patient.County;
                path.DxPhoneNumber = phoneNumbers;
            }
        }

        private async Task<Guid> Save()
        {
            var id = Guid.Empty;
            SetPatientHistoricalData();

            // Manually trigger validation
            bool isValid = EditContext.Validate();

            if (isValid)
            {
                HasErrors = false;
                Patient.CaseNumber = await GenerateCaseNumber?.Generate(prefix); // Get the last case number for the selected study
                Patient.PathReports.ToList().ForEach(pr => pr.CaseNumber = Patient.CaseNumber); // Set the case number for each path report
                Patient.PathReports.ToList().ForEach(pr => pr.StudyPrefix = prefix); // Set the study prefix for each path report
                Patient.PathReports.ToList().ForEach(pr => pr.StudyColor = StudyColor); // Set the study color for each path report

                try
                {
                    id = await PatientData.CreatePatientAsync(Patient);

                    IsSaved = true; // Set IsSaved to true to indicate the form was submitted successfully

                    //Logger.LogInformation("Patient created. {Patient}", System.Text.Json.JsonSerializer.Serialize(Patient));

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
                SaveMessage = "Case not created.  Please correct the errors."; // Set a failure message
            }

            return id;

        }

        private async Task OnSubmit()
        {
            var id = await Save();

            if (IsSaved)
            {
                NavigationManager.NavigateTo($"/cases/edit/{id}/{IsSaved}");
            }

        }

        private async Task OnSaveAndAddNew()
        {
            var id = await Save();

            if (IsSaved)
            {
                NavigationManager.NavigateTo($"/cases/create", forceLoad: true);
            }
        }

        private async Task OnSaveAndClose()
        {
            var id = await Save();

            if (IsSaved)
            {
                NavigationManager.NavigateTo($"/cases/list");
            }
        }

        private async Task OnStudySelectChanged(Study value)
        {
            ShowFields = true; // Show additional fields when a study is selected
            studySelectValue = value;

            Patient.StudyId = value.StudyId; // Update the Patient's StudyId based on the selected study
            StudyId = value.StudyId; // Update the StudyId for the form

            StudyColor = value.ColorLight; // Update the color based on the selected study
            InvokeAsync(StateHasChanged); // Refresh the UI to reflect the changes

            // TODO: Add logic to generate the case number based on the selected study
            prefix = value.Prefix;

            StudyColorChanged.InvokeAsync(StudyColor);

        }




    }
}