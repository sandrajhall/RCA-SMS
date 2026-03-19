using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Pages.Doctors;
using RCA_StudyManagementSystem.Client.Pages.Hospitals;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;


namespace RCA_StudyManagementSystem.Client.Pages.Cases
{
    public partial class Fields : Microsoft.AspNetCore.Components.ComponentBase
    {

        [Parameter]
        public Patient Patient { get; set; } = new Patient();

        [CascadingParameter]
        public EditContext? EditContext { get; set; }

        [Parameter]
        public Guid StudyId { get; set; }

        [Parameter]
        public bool IsSaved { get; set; }

        public Study Study = new Study();

        public CancellationToken CancellationToken { get; set; } = new CancellationToken();
        private GeoapifySuggestion? SelectedValue { get; set; }

        private PathReport _activeReport;


        private IEnumerable<string> GenderList = new List<string>();
        private IEnumerable<string> RaceList = new List<string>();
        private IEnumerable<string> EthnicityList = new List<string>();
        private IEnumerable<string> ProcedureList = new List<string>();
        private IEnumerable<string> CountyList = new List<string>();
        private IEnumerable<string> HistologicDiffList = new List<string>();
        private IEnumerable<Doctor> DoctorList = new List<Doctor>();
        private IEnumerable<Doctor> PathologistList = new List<Doctor>();
        private IEnumerable<Hospital> HospitalList = new List<Hospital>();

        //private Doctor? SelectedDoctor = new Doctor();
        //private Doctor? SelectedPathologist = new Doctor();
        //private Hospital? SelectedHospital = new Hospital();


        // Using two separate lists for histology diagnoses to allow for two different "show all" toggles
        private IEnumerable<StudyHistologyView> HistologyList1 = new List<StudyHistologyView>();
        private IEnumerable<StudyHistologyView> HistologyList2 = new List<StudyHistologyView>();

        private IEnumerable<StudyLookupView> SiteList = new List<StudyLookupView>();

        private IEnumerable<Lookup> lookups { get; set; } = new List<Lookup>();

        private bool showAllDx1 = false;
        private bool showAllDx2 = false;

        private MudDataGrid<PatientPhoneNumber>? dataGrid;
        private MudSelect<string> exportSelect;

        private readonly DialogOptions _options = new() { CloseButton = false, MaxWidth = MaxWidth.Large, FullWidth = true };


        private CancellationToken cancellationToken { get; set; } = new CancellationToken();
        private List<GeoapifySuggestion> Suggestions = new();
        private string SelectedAddress = "";
        private string GeoapifyApiKey = String.Empty; // Securely obtain API key in production
        [Parameter]
        public string StudyColor { get; set; }
        [Parameter]
        public EventCallback<string> StudyColorChanged { get; set; }

        private Origin _anchor = Origin.BottomLeft;
        private Origin _transform = Origin.TopLeft;
        private bool _fixed = true;
        private OverflowBehavior _overflowBehavior = OverflowBehavior.FlipOnOpen;
        private DropdownWidth _dropdownWidth = DropdownWidth.Relative;
        private string selectedValue = string.Empty;

        //private DropdownSettings _dropdownSettings => new DropdownSettings() { Fixed = _fixed, OverflowBehavior = _overflowBehavior, };
        private string errorMsgAge;
        private string errorMsgPath;
        private string errorMsgCounty;
        private string errorMsgSSN;
        private string errorMsgDOB;
        private string errorMsgName;

        private MudAutocomplete<Hospital> hospAutocompleteRef;
        private MudAutocomplete<Hospital> origHospAutocompleteRef;
        private MudAutocomplete<Hospital> reimb1AutocompleteRef;
        private MudAutocomplete<Hospital> reimb2AutocompleteRef;
        private MudAutocomplete<Doctor> doc1AutocompleteRef;
        private MudAutocomplete<Doctor> doc2AutocompleteRef;
        private MudAutocomplete<Doctor> path1AutocompleteRef;
        private MudAutocomplete<Doctor> path2AutocompleteRef;


        private bool IsLoading = false;
        private bool hospSelectionMade = false;
        private bool origHospSelectionMade = false;
        private bool reimb1SelectionMade = false;
        private bool reimb2SelectionMade = false;
        private bool doctorSelectionMade = false;
        private bool pathologistSelectionMade = false;
        private bool doctor2SelectionMade = false;
        private bool pathologist2SelectionMade = false;
        private bool hospTextChanged = false;
        private bool origHospTextChanged = false;
        private bool reimb1TextChanged = false;
        private bool reimb2TextChanged = false;
        private bool doctorTextChanged = false;
        private bool pathologistTextChanged = false;
        private bool doctor2TextChanged = false;
        private bool pathologist2TextChanged = false;


        protected override async Task OnInitializedAsync()
        {

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                IsLoading = true;
                StudyColor = await StudyData.GetStudyColorAsync(StudyId); // Set the study color based on the Patient's StudyId

                await InvokeAsync(StateHasChanged); // Force a re-render after initialization


                GeoapifyApiKey = Configuration["Geoapify:ApiKey"]!;

                EditContext!.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events

                var taskGenderList = StudyLookupData.ListOptionsAsync("Gender", StudyId);
                var taskRaceList = StudyLookupData.ListOptionsAsync("Race", StudyId);
                var taskEthnicityList = StudyLookupData.ListOptionsAsync("Ethnicity", StudyId);
                var taskProcedureList = StudyLookupData.ListOptionsAsync("Procedure", StudyId);
                var taskCountyList = StudyLookupData.ListOptionsAsync("County", StudyId);
                var taskHistologicDiffList = LookupData.ListLookupsByTypeAsync("HistologicDiff");


                await Task.WhenAll(taskGenderList, taskRaceList, taskEthnicityList, taskProcedureList,
                    taskCountyList, taskHistologicDiffList);

                GenderList = await taskGenderList;
                RaceList = await taskRaceList;
                EthnicityList = await taskEthnicityList;
                ProcedureList = await taskProcedureList;
                CountyList = await taskCountyList;
                HistologicDiffList = await taskHistologicDiffList;

                await InvokeAsync(StateHasChanged); // Force a re-render after initialization


                var taskHistologyList1 = StudyHistologyData.ListStudyHistologiesByStudyIdAsync(StudyId);
                var taskHistologyList2 = StudyHistologyData.ListStudyHistologiesByStudyIdAsync(StudyId);
                var taskSiteList = StudyLookupData.ListStudyLookupsByStudyIdAsync(StudyId);
                //var taskDoctorList = DoctorData.ListDoctorsAsync(CancellationToken);
                //var taskPathologistList = DoctorData.ListPathologistsAsync(CancellationToken);
                //var taskHospitalList = HospitalData.ListHospitalsAsync(CancellationToken);
                var tasklookups = LookupData.ListLookupsAsync(); // Fetch lookups from the server

                await Task.WhenAll(taskHistologyList1, taskHistologyList2, taskSiteList, tasklookups);


                HistologyList1 = await taskHistologyList1;
                HistologyList2 = await taskHistologyList2;
                SiteList = await taskSiteList;
                //DoctorList = await taskDoctorList;
                //PathologistList = await taskPathologistList;
                //HospitalList = await taskHospitalList;
                lookups = await tasklookups;

                HistologyList1 = HistologyList1.Where(x => x.IsActive && x.IsPreferred).OrderBy(x => x.HistologyCode).ThenBy(x => x.HistologyName).ToList(); // Filter to only include active histologies
                HistologyList2 = HistologyList2.Where(x => x.IsActive && x.IsPreferred).OrderBy(x => x.HistologyCode).ThenBy(x => x.HistologyName).ToList(); // Filter to only include active histologies
                SiteList = SiteList.Where(x => x.LookupType == "Site").ToList(); // Filter to only include sites

                // Set the value of dropdown if only one option is available
                if (GenderList.Count() == 1 && Patient.Gender == string.Empty)
                {
                    Patient.Gender = GenderList.First();
                    Patient.GenderCode = lookups.Where(x => x.LookupName == Patient.Gender).FirstOrDefault()!.LookupCode; // Set GenderCode
                }
                if (RaceList.Count() == 1 && Patient.Race == string.Empty)
                {
                    Patient.Race = RaceList.First();
                    Patient.RaceCode = lookups.Where(x => x.LookupName == Patient.Race).FirstOrDefault()!.LookupCode; // Set RaceCode
                }
                if (EthnicityList.Count() == 1 && Patient.Ethnicity == string.Empty)
                {
                    Patient.Ethnicity = EthnicityList.First();
                    Patient.EthnicityCode = lookups.Where(x => x.LookupName == Patient.Ethnicity).FirstOrDefault()!.LookupCode; // Set EthnicityCode
                }
                if (CountyList.Count() == 1 && Patient.County == string.Empty)
                {
                    Patient.County = CountyList.First();
                    Patient.CountyCode = lookups.Where(x => x.LookupName == Patient.County).FirstOrDefault()!.LookupCode; // Set CountyCode
                }


                if (Patient.PathReports.Count > 0)
                {
                    if (ProcedureList.Count() == 1 && Patient.PathReports.First().PathProcedure == string.Empty)
                    {
                        Patient.PathReports.First().PathProcedure = ProcedureList.First();
                    }

                    if (SiteList.Count() == 1 && Patient.PathReports.First().Site == string.Empty)
                    {
                        Patient.PathReports.First().Site = SiteList.First().LookupName;
                        Patient.PathReports.First().SiteCode = SiteList.First().LookupCode; // Set SiteCode
                    }
                }

                foreach (var pathReport in Patient.PathReports)
                {
                    if (pathReport?.Site2?.Length > 0)
                    {
                        pathReport.IsShownSite2 = true;
                    }

                    if (pathReport?.ExportStatus == "Path too early")
                    {
                        PathMinAgeCheck(pathReport);
                    }

                    if (pathReport.HospitalEntity == null && !string.IsNullOrEmpty(pathReport.SubmittingHospital))
                    {
                        if (pathReport.HospitalId == null)
                        {
                            pathReport.HospitalId = await HospitalData.GetHospitalIdAsync(pathReport.SubmittingHospital);
                        }

                        if (pathReport.HospitalId != null)
                        {
                            pathReport.HospitalEntity = await HospitalData.GetHospitalAsync(pathReport.HospitalId.Value);
                        }
                    }

                    if (pathReport.OrigHospitalEntity == null && !string.IsNullOrEmpty(pathReport.OriginatingHospitalName))
                    {
                        if (pathReport.OrigHospitalId == null)
                        {
                            pathReport.OrigHospitalId = await HospitalData.GetHospitalIdAsync(pathReport.OriginatingHospitalName);
                        }

                        if (pathReport.OrigHospitalId != null)
                        {
                            pathReport.OrigHospitalEntity = await HospitalData.GetHospitalAsync(pathReport.OrigHospitalId.Value);
                        }
                    }

                    if (pathReport.Reimb1HospitalEntity == null && !string.IsNullOrEmpty(pathReport.Reimbursement1))
                    {    
                        var reimb1Id = await HospitalData.GetHospitalIdAsync(pathReport.Reimbursement1);      

                        if (reimb1Id != null)
                        {
                            pathReport.Reimb1HospitalEntity = await HospitalData.GetHospitalAsync(reimb1Id);
                        }
                    }

                    if (pathReport.Reimb2HospitalEntity == null && !string.IsNullOrEmpty(pathReport.Reimbursement2))
                    {
                        var reimb2Id = await HospitalData.GetHospitalIdAsync(pathReport.Reimbursement2);

                        if (reimb2Id != null)
                        {
                            pathReport.Reimb1HospitalEntity = await HospitalData.GetHospitalAsync(reimb2Id);
                        }
                    }

                    if (pathReport.DoctorEntity1 == null && pathReport.DoctorId != null)
                    {

                            pathReport.DoctorEntity1 = await DoctorData.GetDoctorAsync(pathReport.DoctorId.Value);
                    }
                    
                    if (pathReport.DoctorEntity2 == null && pathReport.Doctor2Id != null)
                    {

                        pathReport.DoctorEntity2 = await DoctorData.GetDoctorAsync(pathReport.Doctor2Id.Value);
                    }

                    if (pathReport.PathologistEntity1 == null && pathReport.PathologistId != null)
                    {

                        pathReport.PathologistEntity1 = await DoctorData.GetDoctorAsync(pathReport.PathologistId.Value);
                    }

                    if (pathReport.PathologistEntity2 == null && pathReport.Pathologist2Id != null)
                    {

                        pathReport.PathologistEntity2 = await DoctorData.GetDoctorAsync(pathReport.Pathologist2Id.Value);
                    }

                }

                await InvokeAsync(StateHasChanged); // Force a re-render after initialization

                Study = await StudyData.GetStudyAsync(Patient.StudyId);
                IsLoading = false;
            }
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

        private async Task CheckSSN(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && value.Length == 11)
            {
                var ssnExists = await PatientData.CheckSSNAsync(value);
                if (ssnExists)
                {
                    errorMsgSSN = "SSN already exists.";
                }
                else
                {
                    errorMsgSSN = null;
                    Patient.SocialSecurityNumber = value;
                }

                var doNotContacts = await DoNotContactData.ListDoNotContactsAsync(CancellationToken);
                var ssnDNC = doNotContacts.Where(p => p.SocialSecurityNumber == value).FirstOrDefault();
                if (ssnDNC != null)
                {
                    errorMsgSSN = "SSN is on Do Not Contact list.";
                }

            }

        }

        private async Task CheckDOB(DateTime? value)
        {
            if (value != null)
            {
                var dobExists = await PatientData.CheckDOBAsync(value);
                if (dobExists)
                {
                    errorMsgDOB = "DOB already exists.";
                }
                else
                {
                    errorMsgDOB = null;
                }

                Patient.DateOfBirth = value;

                var doNotContacts = await DoNotContactData.ListDoNotContactsAsync(CancellationToken);
                var dobDNC = doNotContacts.Where(p => p.DateOfBirth == value).FirstOrDefault();
                if (dobDNC != null)
                {
                    errorMsgDOB = "DOB is on Do Not Contact list.";
                }
            }

        }

        private async Task CheckName(string value)
        {
            if (value != null)
            {
                var nameExists = await PatientData.CheckNameAsync(value);
                if (nameExists)
                {
                    errorMsgName = "First Name and Last Name already exists.";
                }
                else
                {
                    errorMsgName = null;
                }

                var doNotContacts = await DoNotContactData.ListDoNotContactsAsync(CancellationToken);
                var nameDNC = doNotContacts.Where(p => p.FirstName == value.Split(" ")[0] &&  p.LastName == value.Split(" ")[1]).FirstOrDefault();
                if (nameDNC != null)
                {
                    errorMsgName = "First Name and Last Name are on the Do Not Contact list.";
                }
            }

        }

        private string MinAgeValidation(PathReport path)
        {
            if (string.IsNullOrWhiteSpace(path.AgeAtProcedure))
                return "Age is required.";

            // Optionally: check if Study or Patient is null
            if (Study == null || Patient == null)
                return "Patient or Study not found.";

            if (!Study.IsMinAgeValid(Patient))
            {
                if (Study.MinAge != null)
                    return $"Patient must be at least {Study.MinAge} years old at Dx.";
                return "Patient is too young.";
            }

            return null;
        }



        private string MaxAgeValidation(PathReport path)
        {
            if (string.IsNullOrWhiteSpace(path.AgeAtProcedure))
                return "Age is required.";

            // Optionally: check if Study or Patient is null
            if (Study == null || Patient == null)
                return "Patient or Study not found.";

            if (!Study.IsMaxAgeValid(Patient))
            {
                if (Study.MaxAge != null)
                    return $"Patient cannot be older than {Study.MaxAge} years old at Dx.";
                return "Patient is too old.";
            }

            return null;
        }

        private string PathMinAgeValidation(PathReport path)
        {
            var pathReport = Patient.PathReports.FirstOrDefault();

            if (Study == null || Patient == null)
                return "Patient or Study not found.";

            if (!Study.IsPathMinAgeValid(path))
            {
                if (Study.PathMinAge != null)
                {
                    path.ExportStatus = "Path too early";
                    StateHasChanged();
                    return $"Path Report must be at least {Study.PathMinAge} days old.";
                }
                path.ExportStatus = "Path too early";
                StateHasChanged(); // Refresh the UI to reflect the changes
                return "Path Report is too young.";
            }
            else
            {
                if (path.ExportStatus == "Path too early")
                {
                    path.ExportStatus = "Ready";
                    StateHasChanged(); // Refresh the UI to reflect the changes
                }
            }
                return null;
        }

        private void PathMinAgeCheck(PathReport path)
        {
            if (path.ExportStatus == "Path too early")
            {
                if (Study.IsPathMinAgeValid(path))
                {
                    path.ExportStatus = "Ready";
                    StateHasChanged(); // Refresh the UI to reflect the changes
                }
            }
        }

        private string PathMaxAgeValidation(PathReport path)
        {
            // Optionally: check if Study or Patient is null
            if (Study == null || Patient == null)
                return "Patient or Study not found.";
            if (!Study.IsPathMaxAgeValid(path))
            {
                if (Study.PathMaxAge != null)
                    return $"Path Report cannot be older than {Study.PathMaxAge} days.";
                return "Path Report is too old.";
            }
            else
            {
                var days = Study.CalculateAgeDays(path.DateOfProcedure.Value, DateTime.Now);
                
                if (Study.PathMaxAge == null) return null;

                if (days != null && days >= (Int32.Parse(Study.PathMaxAge) - 7) && days <= Int32.Parse(Study.PathMaxAge))
                {
                    return $"Path Report is {days} days old, which is less than a week from the cut-off date.";
                }
            }
            return null;
        }

        private string CountyValidation(string county)
        {
            if (string.IsNullOrWhiteSpace(county))
                return "County is required.";

            if (Study.IsCountyValid(county) == false)
            {
                return $"County '{county}' is not allowed for this study.";
            }
            return null;
        }

        private void CalculateAgeAtProcedure(Patient patient, PathReport pathReport, DateTime? dateOfProcedure, DateTime? dateOfBirth)
        {
            if (dateOfProcedure.HasValue && dateOfBirth.HasValue)
            {
                var age = dateOfProcedure.Value.Year - dateOfBirth.Value.Year;
                if (dateOfProcedure.Value < dateOfBirth.Value.AddYears(age)) age--;
                pathReport.AgeAtProcedure = age.ToString();
                pathReport.DateOfProcedure = dateOfProcedure.Value; // Ensure the date is set in the PathReport
            }
            else
            {
                pathReport.AgeAtProcedure = string.Empty; // Reset if no date is provided
                pathReport.DateOfProcedure = dateOfProcedure!.Value;
            }

            var fieldIdentifierAge = new FieldIdentifier(pathReport, nameof(PathReport.AgeAtProcedure));
            EditContext!.NotifyFieldChanged(fieldIdentifierAge); // Refresh the UI to reflect the changes
            StateHasChanged(); // Refresh the UI to reflect the changes

            errorMsgAge = MinAgeValidation(pathReport);
            if (string.IsNullOrEmpty(errorMsgAge))
            {
                errorMsgAge = MaxAgeValidation(pathReport);
            }

            errorMsgPath = PathMinAgeValidation(pathReport);
            if (string.IsNullOrEmpty(errorMsgPath))
            {
                errorMsgPath = PathMaxAgeValidation(pathReport);
            }
        }

        private void OnFirstNameChanged(string newValue)
        {
            Patient.FirstName = newValue;
            if (!string.IsNullOrEmpty(Patient.LastName))
            {
                Patient.DisplayName = $"{Patient.FirstName} {Patient.LastName}".Trim();
                if (!string.IsNullOrEmpty(Patient.Suffix))
                {
                    Patient.DisplayName += $", {Patient.Suffix}";
                }
            }
        }

        private async Task OnLastNameChanged(string newValue)
        {
            Patient.LastName = newValue;
            if (!string.IsNullOrEmpty(Patient.FirstName))
            {
                Patient.DisplayName = $"{Patient.FirstName} {Patient.LastName}".Trim();
                if (!string.IsNullOrEmpty(Patient.Suffix))
                {
                    Patient.DisplayName += $", {Patient.Suffix}";
                }
            }
            await CheckName(Patient.DisplayName);
        }

        private void OnSuffixChanged(string newValue)
        {
            Patient.Suffix = newValue;
            if (!string.IsNullOrEmpty(Patient.FirstName) && !string.IsNullOrEmpty(Patient.LastName) && !string.IsNullOrEmpty(Patient.Suffix))
            {
                Patient.DisplayName = $"{Patient.FirstName} {Patient.LastName}, {Patient.Suffix}".Trim();
            }
        }

        private void OnSuffixCleared()
        {
            Patient.Suffix = string.Empty;
            if (!string.IsNullOrEmpty(Patient.FirstName) && !string.IsNullOrEmpty(Patient.LastName))
            {
                Patient.DisplayName = $"{Patient.FirstName} {Patient.LastName}".Trim();
            }
        }

        private async Task OnShowAllDx1Async(bool newValue)
        {
            if (newValue)
            {
                showAllDx1 = true;
                HistologyList1 = await StudyHistologyData.ListStudyHistologiesByStudyIdAsync(StudyId);
            }
            else
            {
                showAllDx1 = false;
                HistologyList1 = HistologyList1.Where(x => x.IsPreferred).OrderBy(x => x.HistologyCode).ThenBy(x => x.HistologyName).ToList();
            }

            await InvokeAsync(StateHasChanged); // Refresh the UI to reflect the changes

        }

        private async Task OnShowAllDx2Async(bool newValue)
        {
            if (newValue)
            {
                showAllDx2 = true;
                HistologyList2 = await StudyHistologyData.ListStudyHistologiesByStudyIdAsync(StudyId);
            }
            else
            {
                showAllDx2 = false;
                HistologyList2 = HistologyList2.Where(x => x.IsPreferred).OrderBy(x => x.HistologyCode).ThenBy(x => x.HistologyName).ToList();
            }

            await InvokeAsync(StateHasChanged); // Refresh the UI to reflect the changes

        }

        private void OnCountyChanged(string newValue)
        {
            if (!string.IsNullOrEmpty(newValue) && Patient != null && lookups.Count() > 0)
            {
                Patient.County = newValue; // Update the county name in the Patient
                Patient.CountyCode = lookups.FirstOrDefault(x => x.LookupName == newValue)?.LookupCode ?? ""; // Reset CountyCode when County changes

                var fieldIdentifierCounty = new FieldIdentifier(Patient, nameof(Patient.County));
                EditContext!.NotifyFieldChanged(fieldIdentifierCounty); // Refresh the UI to reflect the changes

                errorMsgCounty = CountyValidation(newValue);
            }
        }


        private void OnRaceChanged(string newValue)
        {
            Patient.Race = newValue;
            Patient.RaceCode = lookups.Where(x => x.LookupName == newValue).FirstOrDefault()!.LookupCode; // Reset RaceCode
        }
        private void OnEthnicityChanged(string newValue)
        {
            Patient.Ethnicity = newValue;
            Patient.EthnicityCode = lookups.Where(x => x.LookupName == newValue).FirstOrDefault()!.LookupCode; // Reset EthnicityCode
        }
        private void OnGenderChanged(string newValue)
        {
            Patient.Gender = newValue; // Update
            Patient.GenderCode = lookups.Where(x => x.LookupName == newValue).FirstOrDefault()!.LookupCode; // Reset GenderCode
        }

        private void OnSiteChanged(string newValue, PathReport pathReport)
        {
            pathReport.Site = newValue;
            pathReport.SiteCode = lookups.Where(x => x.LookupName == newValue).FirstOrDefault()!.LookupCode; // Reset SiteCode
        }

        private void OnSite2Changed(string newValue, PathReport pathReport)
        {
            pathReport.Site2 = newValue;
            pathReport.SiteCode2 = lookups.Where(x => x.LookupName == newValue).FirstOrDefault()!.LookupCode; // Reset SiteCode2
        }

        private void OnHistologyDiagnosis1Changed(string newValue, PathReport pathReport)
        {
            pathReport.HistologyDiagnosis1 = newValue;
            pathReport.HistologyCode1 = HistologyList1.Where(x => x.HistologyName == newValue).FirstOrDefault()?.HistologyCode ?? ""; // Reset HistologyCode1
            pathReport.HistologyBehavior1 = HistologyList1.Where(x => x.HistologyName == newValue).FirstOrDefault()?.HistologyBehavior ?? ""; // Reset HistologyBehavior1
        }

        private void OnHistologyDiagnosis2Changed(string newValue, PathReport pathReport)
        {
            pathReport.HistologyDiagnosis2 = newValue;
            pathReport.HistologyCode2 = HistologyList2.Where(x => x.HistologyName == newValue).FirstOrDefault()?.HistologyCode ?? ""; // Reset HistologyCode2
            pathReport.HistologyBehavior2 = HistologyList2.Where(x => x.HistologyName == newValue).FirstOrDefault()?.HistologyBehavior ?? ""; // Reset HistologyBehavior2
        }

        private async Task<IDialogReference> OnIsOnHoldChangedAsync(PathReport pathReport, bool newValue)
        {
            if (pathReport.RcaExportDate != null)
            {

                var result = await DialogService.ShowMessageBoxAsync(
             "Can't Put On Hold",
             "Path Report has already been downloaded.",
             yesText: "OK",
             cancelText: "Cancel"
         );

                // You can use the nullable bool result to check the user's choice
                if (result == true)
                {
                    // The user clicked "OK"
                    Console.WriteLine("User confirmed.");
                }
                else
                {
                    // The user clicked "Cancel" or closed the dialog
                    Console.WriteLine("User cancelled.");
                }
            }
            else
            {
                pathReport.IsOnHold = newValue;
            }
            return null;

        }

        private async Task OnDoctorSelectionChanged(Doctor doctor, PathReport pathReport)
        {
            doctorSelectionMade = true;

            if (pathReport.RcaExportDate == null)
            {
                pathReport.AuthorizingProvider = doctor.DisplayName;
                pathReport.DoctorId = doctor.DoctorId;

                var id = doctor.DoctorId;
                var updatedDoctor = await DoctorData.GetDoctorAsync(id);

                if (updatedDoctor != null)
                {
                    pathReport.MDAddress1 = updatedDoctor.Address1;
                    pathReport.MDAddress2 = updatedDoctor?.Address2;
                    pathReport.MDAddress3 = updatedDoctor?.Address3;
                    pathReport.MDCity = updatedDoctor?.City;
                    pathReport.MDState = updatedDoctor?.State;
                    pathReport.MDZipCode = updatedDoctor?.ZipCode;
                    pathReport.MDCounty = updatedDoctor.County;
                    pathReport.MDPhoneNumber1 = updatedDoctor.PhoneNumber1;
                    pathReport.MDPhoneNumber2 = updatedDoctor?.PhoneNumber2;
                    pathReport.MDFaxNumber = updatedDoctor.FaxNumber;
                    pathReport.MDEmail = updatedDoctor.Email;
                }
            }
        }

        private async Task OnDoctor2SelectionChanged(Doctor doctor, PathReport pathReport)
        {
            doctor2SelectionMade = true;

            if (pathReport.RcaExportDate == null)
            {
                pathReport.AuthorizingProvider2 = doctor.DisplayName;
                pathReport.Doctor2Id = doctor.DoctorId;

                var id = doctor.DoctorId;
                var updatedDoctor = await DoctorData.GetDoctorAsync(id);

                if (updatedDoctor != null)
                {
                    pathReport.MD2Address1 = updatedDoctor.Address1;
                    pathReport.MD2Address2 = updatedDoctor?.Address2;
                    pathReport.MD2Address3 = updatedDoctor?.Address3;
                    pathReport.MD2City = updatedDoctor?.City;
                    pathReport.MD2State = updatedDoctor?.State;
                    pathReport.MD2ZipCode = updatedDoctor?.ZipCode;
                    pathReport.MD2County = updatedDoctor.County;
                    pathReport.MD2PhoneNumber1 = updatedDoctor.PhoneNumber1;
                    pathReport.MD2PhoneNumber2 = updatedDoctor?.PhoneNumber2;
                    pathReport.MD2FaxNumber = updatedDoctor.FaxNumber;
                    pathReport.MD2Email = updatedDoctor.Email;
                }
            }
        }

        private async Task OnPathologistSelectionChanged(Doctor doctor, PathReport pathReport)
        {
            pathologistSelectionMade = true;

            if (pathReport.RcaExportDate == null)
            {
                pathReport.Pathologist = doctor.DisplayName;
                pathReport.PathologistId = doctor.DoctorId;

                var id = doctor.DoctorId;
                var updatedDoctor = await DoctorData.GetDoctorAsync(id);

                if (updatedDoctor != null)
                {
                    pathReport.PathAddress1 = updatedDoctor.Address1;
                    pathReport.PathAddress2 = updatedDoctor?.Address2;
                    pathReport.PathAddress3 = updatedDoctor?.Address3;
                    pathReport.PathCity = updatedDoctor?.City;
                    pathReport.PathState = updatedDoctor?.State;
                    pathReport.PathZipCode = updatedDoctor?.ZipCode;
                    pathReport.PathCounty = updatedDoctor.County;
                    pathReport.PathPhoneNumber1 = updatedDoctor.PhoneNumber1;
                    pathReport.PathPhoneNumber2 = updatedDoctor?.PhoneNumber2;
                    pathReport.PathFaxNumber = updatedDoctor.FaxNumber;
                    pathReport.PathEmail = updatedDoctor.Email;
                }
            }
        }

        private async Task OnPathologist2SelectionChanged(Doctor doctor, PathReport pathReport)
        {
            pathologist2SelectionMade = true;

            if (pathReport.RcaExportDate == null)
            {
                pathReport.Pathologist2 = doctor.DisplayName;
                pathReport.Pathologist2Id = doctor.DoctorId;

                var id = doctor.DoctorId;
                var updatedDoctor = await DoctorData.GetDoctorAsync(id);

                if (updatedDoctor != null)
                {
                    pathReport.Path2Address1 = updatedDoctor.Address1;
                    pathReport.Path2Address2 = updatedDoctor?.Address2;
                    pathReport.Path2Address3 = updatedDoctor?.Address3;
                    pathReport.Path2City = updatedDoctor?.City;
                    pathReport.Path2State = updatedDoctor?.State;
                    pathReport.Path2ZipCode = updatedDoctor?.ZipCode;
                    pathReport.Path2County = updatedDoctor.County;
                    pathReport.Path2PhoneNumber1 = updatedDoctor.PhoneNumber1;
                    pathReport.Path2PhoneNumber2 = updatedDoctor?.PhoneNumber2;
                    pathReport.Path2FaxNumber = updatedDoctor.FaxNumber;
                    pathReport.Path2Email = updatedDoctor.Email;
                }
            }
        }

        private async Task OnHospitalSelectionChanged(Hospital hospital, PathReport pathReport)
        {
            hospSelectionMade = true;

            if (hospital == null || hospital.HospitalId == Guid.Empty)
            {
                return; // Exit if no hospital is selected
            }

            if (pathReport.RcaExportDate == null)
            {
                //pathReport.SubmittingHospital = hospital.Split("(").FirstOrDefault().Trim();
                //pathReport.Reimbursement1 = hospital.Split("(").FirstOrDefault().Trim();

                //var id = hospital.Split('(').Last().Split(")").First();


                pathReport.SubmittingHospital = hospital.HospitalName;
                pathReport.HospitalId = hospital.HospitalId;
                pathReport.Reimbursement1 = hospital.HospitalName;
                pathReport.HospitalEntity = hospital;
                pathReport.Reimb1HospitalEntity = hospital;


                var updatedHospital = await HospitalData.GetHospitalAsync(hospital.HospitalId);

                if (updatedHospital != null)
                {
                    pathReport.HospAddress1 = updatedHospital.Address1;
                    pathReport.HospAddress2 = updatedHospital?.Address2;
                    pathReport.HospCity = updatedHospital?.City;
                    pathReport.HospState = updatedHospital?.State;
                    pathReport.HospZipCode = updatedHospital?.ZipCode;
                    pathReport.HospPhoneNumber = updatedHospital.PhoneNumber;
                    pathReport.HospFaxNumber = updatedHospital.FaxNumber;
                }
            }
        }
        private async Task OnOrigHospitalSelectionChanged(Hospital hospital, PathReport pathReport)
        {
            origHospSelectionMade = true;


            if (hospital == null || hospital.HospitalId == Guid.Empty)
            {
                return; // Exit if no hospital is selected
            }


            if (pathReport.RcaExportDate == null)
            {
                pathReport.OriginatingHospitalName = hospital.HospitalName;
                pathReport.OrigHospitalId = hospital.HospitalId;
                pathReport.OrigHospitalEntity = hospital;

                var updatedHospital = await HospitalData.GetHospitalAsync(hospital.HospitalId);

                if (updatedHospital != null)
                {
                    pathReport.OrigHospAddress1 = updatedHospital.Address1;
                    pathReport.OrigHospAddress2 = updatedHospital?.Address2;
                    pathReport.OrigHospCity = updatedHospital?.City;
                    pathReport.OrigHospState = updatedHospital?.State;
                    pathReport.OrigHospZipCode = updatedHospital?.ZipCode;
                    pathReport.OrigHospPhoneNumber = updatedHospital?.PhoneNumber;
                    pathReport.OrigHospFaxNumber = updatedHospital?.FaxNumber;
                }
            }
        }

        private async Task OnReimbursement1SelectionChanged(Hospital hospital, PathReport pathReport)
        {
            reimb1SelectionMade = true;


            if (hospital == null || hospital.HospitalId == Guid.Empty)
            {
                return; // Exit if no hospital is selected
            }
            if (pathReport.RcaExportDate == null)
            {
                pathReport.Reimbursement1 = hospital.HospitalName;
                pathReport.Reimb1HospitalEntity = hospital;
            }
        }

        private async Task OnReimbursement2SelectionChanged(Hospital hospital, PathReport pathReport)
        {
            reimb2SelectionMade = true;


            if (hospital == null || hospital.HospitalId == Guid.Empty)
            {
                return; // Exit if no hospital is selected
            }
            if (pathReport.RcaExportDate == null)
            {
                pathReport.Reimbursement2 = hospital.HospitalName;
                pathReport.Reimb2HospitalEntity = hospital;
            }
        }

        private async Task OnDoctor1TextChanged(string value, PathReport pathReport)
        {
            if (string.IsNullOrEmpty(value) && pathReport.IsDoctor1Cleared)
            {
                pathReport.AuthorizingProvider = string.Empty;
                pathReport.DoctorId = null;
                pathReport.IsDoctor1Cleared = false;
            }
            else if (string.IsNullOrEmpty(value))
            {
                return; // Do nothing if the user clears the text but hasn't used the clear button
            }
            else
            {
                // If the user types, update the display text.
                pathReport.AuthorizingProvider = value.Split("(").FirstOrDefault();
            }


            if (doctorSelectionMade)
            {
                await doc1AutocompleteRef.CloseMenuAsync();
                await Task.Delay(25);
                await doc1AutocompleteRef.BlurAsync();
            }
        }

        private void OnDoctor1Cleared(PathReport pathReport)
        {
            pathReport.IsDoctor1Cleared = true;
            pathReport.AuthorizingProvider = string.Empty;
            pathReport.DoctorId = null;
        }

        private async Task OnDoctor2TextChanged(string value, PathReport pathReport)
        {
            if (string.IsNullOrEmpty(value) && pathReport.IsDoctor2Cleared)
            {
                pathReport.AuthorizingProvider2 = string.Empty;
                pathReport.Doctor2Id = null;
                pathReport.IsDoctor2Cleared = false;
            }
            else if (string.IsNullOrEmpty(value))
            {
                return; // Do nothing if the user clears the text but hasn't used the clear button
            }
            else
            {
                // If the user types, update the display text.
                pathReport.AuthorizingProvider2 = value.Split("(").FirstOrDefault();
            }
            if (doctor2SelectionMade)
            {
                await doc2AutocompleteRef.CloseMenuAsync();
                await Task.Delay(25);
                await doc2AutocompleteRef.BlurAsync();
            }
        }

        private void OnDoctor2Cleared(PathReport pathReport)
        {
            pathReport.IsDoctor2Cleared = true;
            pathReport.AuthorizingProvider2 = string.Empty;
            pathReport.Doctor2Id = null;
        }

        private async Task OnPathologist1TextChanged(string value, PathReport pathReport)
        {
            if (string.IsNullOrEmpty(value) && pathReport.IsPathologist1Cleared)
            {
                pathReport.Pathologist = string.Empty;
                pathReport.PathologistId = null;
                pathReport.IsPathologist1Cleared = false;
            }
            else if (string.IsNullOrEmpty(value))
            {
                return; // Do nothing if the user clears the text but hasn't used the clear button
            }
            else
            {
                // If the user types, update the display text.
                pathReport.Pathologist = value.Split("(").FirstOrDefault();
            }
            if (pathologistSelectionMade)
            {
                await path1AutocompleteRef.CloseMenuAsync();
                await Task.Delay(25);
                await path1AutocompleteRef.BlurAsync();
            }
        }

        private void OnPathologist1Cleared(PathReport pathReport)
        {
            pathReport.IsPathologist1Cleared = true;
            pathReport.PathologistId = null;
            pathReport.Pathologist = string.Empty;
        }

        private async Task OnPathologist2TextChanged(string value, PathReport pathReport)
        {
            if (string.IsNullOrEmpty(value) && pathReport.IsPathologist2Cleared)
            {
                pathReport.Pathologist2 = string.Empty;
                pathReport.Pathologist2Id = null;
                pathReport.IsPathologist2Cleared = false;
            }
            else if (string.IsNullOrEmpty(value))
            {
                return; // Do nothing if the user clears the text but hasn't used the clear button
            }
            else
            {
                // If the user types, update the display text.
                pathReport.Pathologist2 = value.Split("(").FirstOrDefault();
            }
            if (pathologist2SelectionMade)
            {
                await path2AutocompleteRef.CloseMenuAsync();
                await Task.Delay(25);
                await path2AutocompleteRef.BlurAsync();
            }
        }

        private void OnPathologist2Cleared(PathReport pathReport)
        {
            pathReport.IsPathologist2Cleared = true;
            pathReport.Pathologist2 = string.Empty;
            pathReport.Pathologist2Id = null;
        }

        private async Task OnHospitalTextChanged(string value, PathReport pathReport)
        {
            if (string.IsNullOrEmpty(value) && pathReport.IsHospitalCleared)
            {
                pathReport.SubmittingHospital = string.Empty;
                pathReport.IsHospitalCleared = false;
                pathReport.HospitalId = null;
                pathReport.Reimbursement1 = string.Empty;
            }
            else if (string.IsNullOrEmpty(value))
            {
                return; // Do nothing if the user clears the text but hasn't used the clear button
            }
            else
            {
                // If the user types, update the display text.
                pathReport.SubmittingHospital = value.Split("[").FirstOrDefault();
                pathReport.Reimbursement1 = value.Split("[").FirstOrDefault();
            }

            if (hospSelectionMade)
            {
                await hospAutocompleteRef.CloseMenuAsync();
                await Task.Delay(25);
                await hospAutocompleteRef.BlurAsync();
            }
        }

        private void OnHospitalCleared(PathReport pathReport)
        {
            pathReport.IsHospitalCleared = true;
            pathReport.SubmittingHospital = string.Empty;
            pathReport.HospitalId = null;
            pathReport.Reimbursement1 = string.Empty;
            pathReport.HospitalEntity = null;
            pathReport.Reimb1HospitalEntity = null;
        }

        private async Task OnOrigHospitalTextChanged(string value, PathReport pathReport)
        {
            if (string.IsNullOrEmpty(value) && pathReport.IsOrigHospitalCleared)
            {
                pathReport.OriginatingHospitalName = string.Empty;
                pathReport.IsOrigHospitalCleared = false;

            }
            else if (string.IsNullOrEmpty(value))
            {
                return; // Do nothing if the user clears the text but hasn't used the clear button
            }
            else
            {
                // If the user types, update the display text.
                pathReport.OriginatingHospitalName = value.Split("[").FirstOrDefault();
            }

            if (origHospSelectionMade)
            {
                await origHospAutocompleteRef.CloseMenuAsync();
                await Task.Delay(25);
                await origHospAutocompleteRef.BlurAsync();
            }
        }

        private void OnOrigHospitalCleared(PathReport pathReport)
        {
            pathReport.IsOrigHospitalCleared = true;
            pathReport.OriginatingHospitalName = string.Empty;
            pathReport.OrigHospitalId = null;
            pathReport.OrigHospitalEntity = null;

        }

        private async Task OnReimbursement1TextChanged(string value, PathReport pathReport)
        {
            if (string.IsNullOrEmpty(value) && pathReport.IsReimbursement1Cleared)
            {
                pathReport.Reimbursement1 = string.Empty;
                pathReport.IsReimbursement1Cleared = false;

            }
            else if (string.IsNullOrEmpty(value))
            {
                return; // Do nothing if the user clears the text but hasn't used the clear button
            }
            else
            {
                // If the user types, update the display text.
                pathReport.Reimbursement1 = value.Split("[").FirstOrDefault();
            }

            if (reimb1SelectionMade)
            {
                await reimb1AutocompleteRef.CloseMenuAsync();
                await Task.Delay(25);
                await reimb1AutocompleteRef.BlurAsync();
            }
        }

        private void OnReimbursement1Cleared(PathReport pathReport)
        {
            pathReport.IsReimbursement1Cleared = true;
            pathReport.Reimbursement1 = string.Empty;
            pathReport.Reimb1HospitalEntity = null;
        }

        private async Task OnReimbursement2TextChanged(string value, PathReport pathReport)
        {
            if (string.IsNullOrEmpty(value) && pathReport.IsReimbursement2Cleared)
            {
                pathReport.Reimbursement2 = string.Empty;
                pathReport.IsReimbursement2Cleared = false;

            }
            else if (string.IsNullOrEmpty(value))
            {
                return; // Do nothing if the user clears the text but hasn't used the clear button
            }
            else
            {
                // If the user types, update the display text.
                pathReport.Reimbursement2 = value.Split("[").FirstOrDefault();
            }

            if (reimb2SelectionMade)
            {
                await reimb2AutocompleteRef.CloseMenuAsync();
                await Task.Delay(25);
                await reimb2AutocompleteRef.BlurAsync();
            }
        }

        private void OnReimbursement2Cleared(PathReport pathReport)
        {
            pathReport.IsReimbursement2Cleared = true;
            pathReport.Reimbursement2 = string.Empty;
            pathReport.Reimb2HospitalEntity = null;
        }


        private async Task<IEnumerable<Hospital>> HospitalSearch(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                hospSelectionMade = false;

                // Return the saved entity ONLY if we aren't explicitly clearing it
                return _activeReport?.HospitalEntity != null
                    ? new[] { _activeReport.HospitalEntity }
                    : Enumerable.Empty<Hospital>();
            }

            if (value != _activeReport?.HospitalEntity?.HospitalName)
            {
                hospSelectionMade = false;
            }

            // Normal flag check
            if (hospSelectionMade) return Enumerable.Empty<Hospital>();

            return await HospitalData.GetHospitalsAsync(value);
        }

        private async Task<IEnumerable<Hospital>> OrigHospitalSearch(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                origHospSelectionMade = false;

                // Return the saved entity ONLY if we aren't explicitly clearing it
                return _activeReport?.OrigHospitalEntity != null
                    ? new[] { _activeReport.OrigHospitalEntity }
                    : Enumerable.Empty<Hospital>();
            }

            if (value != _activeReport?.OrigHospitalEntity?.HospitalName)
            {
                origHospSelectionMade = false;
            }

            // Normal flag check
            if (origHospSelectionMade) return Enumerable.Empty<Hospital>();

            return await HospitalData.GetHospitalsAsync(value);
        }

        private async Task<IEnumerable<Hospital>> Reimb1HospitalSearch(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                reimb1SelectionMade = false;

                // Return the saved entity ONLY if we aren't explicitly clearing it
                return _activeReport?.Reimb1HospitalEntity != null
                    ? new[] { _activeReport.Reimb1HospitalEntity }
                    : Enumerable.Empty<Hospital>();
            }

            if (value != _activeReport?.Reimb1HospitalEntity?.HospitalName)
            {
                reimb1SelectionMade = false;
            }

            // Normal flag check
            if (reimb1SelectionMade) return Enumerable.Empty<Hospital>();

            return await HospitalData.GetHospitalsAsync(value);
        }

        private async Task<IEnumerable<Hospital>> Reimb2HospitalSearch(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                reimb2SelectionMade = false;

                // Return the saved entity ONLY if we aren't explicitly clearing it
                return _activeReport?.Reimb2HospitalEntity != null
                    ? new[] { _activeReport.Reimb2HospitalEntity }
                    : Enumerable.Empty<Hospital>();
            }

            if (value != _activeReport?.Reimb2HospitalEntity?.HospitalName)
            {
                reimb2SelectionMade = false;
            }

            // Normal flag check
            if (reimb2SelectionMade) return Enumerable.Empty<Hospital>();

            return await HospitalData.GetHospitalsAsync(value);
        }

        private async Task<IEnumerable<Doctor>> DoctorSearch(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                doctorSelectionMade = false;

                // Return the saved entity ONLY if we aren't explicitly clearing it
                return _activeReport?.DoctorEntity1 != null
                    ? new[] { _activeReport.DoctorEntity1 }
                    : Enumerable.Empty<Doctor>();
            }

            if (value != _activeReport?.DoctorEntity1?.DisplayName)
            {
                doctorSelectionMade = false;
            }

            // Normal flag check
            if (doctorSelectionMade) return Enumerable.Empty<Doctor>();

            var doctors = await DoctorData.GetDoctorsAsync(value);
            return doctors;
        }

        private async Task<IEnumerable<Doctor>> Doctor2Search(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                doctor2SelectionMade = false;

                // Return the saved entity ONLY if we aren't explicitly clearing it
                return _activeReport?.DoctorEntity2 != null
                    ? new[] { _activeReport.DoctorEntity2 }
                    : Enumerable.Empty<Doctor>();
            }

            if (value != _activeReport?.DoctorEntity2?.DisplayName)
            {
                doctor2SelectionMade = false;
            }

            // Normal flag check
            if (doctor2SelectionMade) return Enumerable.Empty<Doctor>();

            var doctors = await DoctorData.GetDoctorsAsync(value);
            return doctors;
        }

        private async Task<IEnumerable<Doctor>> PathologistSearch(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                pathologistSelectionMade = false;

                // Return the saved entity ONLY if we aren't explicitly clearing it
                return _activeReport?.PathologistEntity1 != null
                    ? new[] { _activeReport.PathologistEntity1 }
                    : Enumerable.Empty<Doctor>();
            }

            if (value != _activeReport?.PathologistEntity1?.DisplayName)
            {
                pathologistSelectionMade = false;
            }

            // Normal flag check
            if (pathologistSelectionMade) return Enumerable.Empty<Doctor>();

            var pathologists = await DoctorData.GetPathologistsAsync(value);
            return pathologists;
        }

        private async Task<IEnumerable<Doctor>> Pathologist2Search(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                pathologist2SelectionMade = false;

                // Return the saved entity ONLY if we aren't explicitly clearing it
                return _activeReport?.PathologistEntity2 != null
                    ? new[] { _activeReport.PathologistEntity2 }
                    : Enumerable.Empty<Doctor>();
            }

            if (value != _activeReport?.PathologistEntity2?.DisplayName)
            {
                pathologist2SelectionMade = false;
            }

            // Normal flag check
            if (pathologist2SelectionMade) return Enumerable.Empty<Doctor>();

            var pathologists = await DoctorData.GetPathologistsAsync(value);
            return pathologists;
        }

        async Task EditHospital(PathReport pathReport)
        {
            pathReport.SubmittingHospital = pathReport.SubmittingHospital?.Trim();
            IEnumerable<Hospital> hospitals = new List<Hospital>();

            Hospital SelectedHospital = null;

            if (pathReport.HospitalId != null)
            {
                SelectedHospital = await HospitalData.GetHospitalAsync((Guid)pathReport.HospitalId);


                //var hospitals = await HospitalData.ListHospitalsAsync(CancellationToken);

                hospitals.Append(SelectedHospital);
            }

            var index = 0;
            var titleStr = "Edit Hospital";

            if (pathReport.HospitalId == null)
            {
                index = -1;
                titleStr = "Create Hospital";
            }
            else
            {
                //index = hospitals.ToList().FindIndex(d => d.HospitalName == pathReport.SubmittingHospital);
                index = 0; // Since we are only passing the selected hospital, the index is always 0
            }

            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<EditHospitalDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, hospitals); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, index); // Set initial position
            parameters.Add(p => p.Hospital, SelectedHospital);

            var options = _options;

            var dialog = await DialogService.ShowAsync<EditHospitalDialog>(titleStr, parameters, options);
            var result = await dialog.Result; // Execution pauses here

            // This code only runs after the dialog closes.
            if (!result.Canceled && result.Data is Hospital updatedHospital && pathReport.RcaExportDate is null)
            {
                //HospitalList = await HospitalData.ListHospitalsAsync(CancellationToken); // Refresh the hospital list in case of any changes
                pathReport.HospitalId = updatedHospital.HospitalId;
                pathReport.SubmittingHospital = updatedHospital.HospitalName;
                pathReport.HospAddress1 = updatedHospital.Address1;
                pathReport.HospAddress2 = updatedHospital.Address2;
                pathReport.HospCity = updatedHospital.City;
                pathReport.HospState = updatedHospital.State;
                pathReport.HospZipCode = updatedHospital.ZipCode;
                pathReport.HospPhoneNumber = updatedHospital.PhoneNumber;
                pathReport.HospFaxNumber = updatedHospital.FaxNumber;
                pathReport.HospitalEntity = updatedHospital;
                StateHasChanged(); // Tell Blazor to re-render the component
            }
        }

        async Task EditOrigHospital(PathReport pathReport)
        {
            pathReport.OriginatingHospitalName = pathReport.OriginatingHospitalName?.Trim();
            IEnumerable<Hospital> hospitals = new List<Hospital>();

            Hospital SelectedOrigHospital = null;

            if (pathReport?.OriginatingHospitalName?.Length > 1)
            {
                hospitals = await HospitalData.GetHospitalsAsync(pathReport.OriginatingHospitalName);

                SelectedOrigHospital = hospitals.FirstOrDefault()!;
            }

            var index = 0;
            var titleStr = "Edit Hospital";

            if (pathReport?.OriginatingHospitalName?.Length < 1)
            {
                index = -1;
                titleStr = "Create Hospital";
            }


            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<EditHospitalDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, hospitals); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, index); // Set initial position
            parameters.Add(p => p.Hospital, SelectedOrigHospital);

            var options = _options;

            var dialog = await DialogService.ShowAsync<EditHospitalDialog>(titleStr, parameters, options);
            var result = await dialog.Result; // Execution pauses here

            // This code only runs after the dialog closes.
            if (!result.Canceled && result.Data is Hospital updatedHospital && pathReport.RcaExportDate is null)
            {
                //HospitalList = await HospitalData.ListHospitalsAsync(CancellationToken); // Refresh the hospital list in case of any changes
                pathReport.OriginatingHospitalName = updatedHospital.HospitalName;
                pathReport.OrigHospitalEntity = updatedHospital;

                StateHasChanged(); // Tell Blazor to re-render the component
            }
        }



        async Task EditDoctor1(PathReport pathReport)
        {
            pathReport.AuthorizingProvider = pathReport.AuthorizingProvider?.Trim();

            IEnumerable<Doctor> doctors = new List<Doctor>();

            Doctor SelectedDoctor = null;

            if (pathReport.DoctorId != null)
            {
                SelectedDoctor = await DoctorData.GetDoctorAsync((Guid)pathReport.DoctorId);


                //var doctors = await DoctorData.ListDoctorsAsync(CancellationToken);

                doctors.Append(SelectedDoctor);
            }

            var index = 0;
            var titleStr = "Edit Doctor";

            if (pathReport.DoctorId == null)
            {
                index = -1;
                titleStr = "Create Doctor";
            }
            else
            {
                //index = doctors.ToList().FindIndex(d => d.DisplayName == pathReport.AuthorizingProvider);
                index = 0; // Since we are only passing the selected doctor, the index is always 0
            }

            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<EditDoctorDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, doctors); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, index); // Set initial position
            parameters.Add(p => p.Doctor, SelectedDoctor);

            var options = _options;

            var dialog = await DialogService.ShowAsync<EditDoctorDialog>(titleStr, parameters, options);
            var result = await dialog.Result; // Execution pauses here

            // This code only runs after the dialog closes.
            if (!result.Canceled && result.Data is Doctor updatedDoctor && pathReport.RcaExportDate is null)
            {
                //DoctorList = await DoctorData.ListDoctorsAsync(CancellationToken); // Refresh the doctor list in case of any changes
                pathReport.DoctorId = updatedDoctor.DoctorId;
                pathReport.AuthorizingProvider = updatedDoctor.DisplayName;
                pathReport.MDAddress1 = updatedDoctor.Address1;
                pathReport.MDAddress2 = updatedDoctor?.Address2;
                pathReport.MDAddress3 = updatedDoctor?.Address3;
                pathReport.MDCity = updatedDoctor?.City;
                pathReport.MDState = updatedDoctor?.State;
                pathReport.MDZipCode = updatedDoctor?.ZipCode;
                pathReport.MDCounty = updatedDoctor.County;
                pathReport.MDPhoneNumber1 = updatedDoctor.PhoneNumber1;
                pathReport.MDPhoneNumber2 = updatedDoctor?.PhoneNumber2;
                pathReport.MDFaxNumber = updatedDoctor.FaxNumber;
                pathReport.MDEmail = updatedDoctor.Email;
                pathReport.DoctorEntity1 = updatedDoctor;
                StateHasChanged(); // Tell Blazor to re-render the component
            }
        }

        async Task EditPathologist1(PathReport pathReport)
        {
            pathReport.Pathologist = pathReport.Pathologist?.Trim();

            IEnumerable<Doctor> pathologists = new List<Doctor>();

            Doctor SelectedPathologist = null;

            if (pathReport.PathologistId != null)
            {
                SelectedPathologist = await DoctorData.GetDoctorAsync((Guid)pathReport.PathologistId);


                //var doctors = await DoctorData.ListDoctorsAsync(CancellationToken);

                pathologists.Append(SelectedPathologist);
            }

            var index = 0;
            var titleStr = "Edit Doctor";

            if (pathReport.PathologistId == null)
            {
                index = -1;
                titleStr = "Create Doctor";
            }
            else
            {
                //index = pathologists.ToList().FindIndex(d => d.DisplayName == pathReport.Pathologist);
                index = 0; // Since we are only passing the selected pathologist, the index is always 0
            }

            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<EditDoctorDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, pathologists); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, index); // Set initial position
            parameters.Add(p => p.Doctor, SelectedPathologist);

            var options = _options;

            var dialog = await DialogService.ShowAsync<EditDoctorDialog>(titleStr, parameters, options);
            var result = await dialog.Result; // Execution pauses here

            // This code only runs after the dialog closes.
            if (!result.Canceled && result.Data is Doctor updatedDoctor && pathReport.RcaExportDate is null)
            {
                //PathologistList = await DoctorData.ListPathologistsAsync(CancellationToken); // Refresh the doctor list in case of any changes
                pathReport.PathologistId = updatedDoctor.DoctorId;
                pathReport.Pathologist = updatedDoctor.DisplayName;
                pathReport.PathAddress1 = updatedDoctor.Address1;
                pathReport.PathAddress2 = updatedDoctor?.Address2;
                pathReport.PathAddress3 = updatedDoctor?.Address3;
                pathReport.PathCity = updatedDoctor?.City;
                pathReport.PathState = updatedDoctor?.State;
                pathReport.PathZipCode = updatedDoctor?.ZipCode;
                pathReport.PathCounty = updatedDoctor.County;
                pathReport.PathPhoneNumber1 = updatedDoctor.PhoneNumber1;
                pathReport.PathPhoneNumber2 = updatedDoctor?.PhoneNumber2;
                pathReport.PathFaxNumber = updatedDoctor.FaxNumber;
                pathReport.PathEmail = updatedDoctor.Email;
                pathReport.PathologistEntity1 = updatedDoctor;
                StateHasChanged(); // Tell Blazor to re-render the component
            }
        }

        async Task EditDoctor2(PathReport pathReport)
        {
            pathReport.AuthorizingProvider2 = pathReport.AuthorizingProvider2?.Trim();

            IEnumerable<Doctor> doctors = new List<Doctor>();

            Doctor SelectedDoctor = null;

            if (pathReport.Doctor2Id != null)
            {
                SelectedDoctor = await DoctorData.GetDoctorAsync((Guid)pathReport.Doctor2Id);


                //var doctors = await DoctorData.ListDoctorsAsync(CancellationToken);

                doctors.Append(SelectedDoctor);
            }

            var index = 0;
            var titleStr = "Edit Doctor";

            if (pathReport.Doctor2Id == null)
            {
                index = -1;
                titleStr = "Create Doctor";
            }
            else
            {
                //index = doctors.ToList().FindIndex(d => d.DisplayName == pathReport.AuthorizingProvider2);
                index = 0; // Since we are only passing the selected doctor, the index is always 0
            }

            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<EditDoctorDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, doctors); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, index); // Set initial position
            parameters.Add(p => p.Doctor, SelectedDoctor);


            var options = _options;

            var dialog = await DialogService.ShowAsync<EditDoctorDialog>(titleStr, parameters, options);
            var result = await dialog.Result; // Execution pauses here

            // This code only runs after the dialog closes.
            if (!result.Canceled && result.Data is Doctor updatedDoctor && pathReport.RcaExportDate is null)
            {
                //DoctorList = await DoctorData.ListDoctorsAsync(CancellationToken); // Refresh the doctor list in case of any changes
                pathReport.Doctor2Id = updatedDoctor.DoctorId;
                pathReport.AuthorizingProvider2 = updatedDoctor.DisplayName;
                pathReport.MD2Address1 = updatedDoctor.Address1;
                pathReport.MD2Address2 = updatedDoctor?.Address2;
                pathReport.MD2Address3 = updatedDoctor?.Address3;
                pathReport.MD2City = updatedDoctor?.City;
                pathReport.MD2State = updatedDoctor?.State;
                pathReport.MD2ZipCode = updatedDoctor?.ZipCode;
                pathReport.MD2County = updatedDoctor.County;
                pathReport.MD2PhoneNumber1 = updatedDoctor.PhoneNumber1;
                pathReport.MD2PhoneNumber2 = updatedDoctor?.PhoneNumber2;
                pathReport.MD2FaxNumber = updatedDoctor.FaxNumber;
                pathReport.MD2Email = updatedDoctor.Email;
                pathReport.DoctorEntity2 = updatedDoctor;
                StateHasChanged(); // Tell Blazor to re-render the component
            }
        }

        async Task EditPathologist2(PathReport pathReport)
        {
            pathReport.Pathologist2 = pathReport.Pathologist2?.Trim();

            IEnumerable<Doctor> pathologists = new List<Doctor>();

            Doctor SelectedPathologist = null;

            if (pathReport.Pathologist2Id != null)
            {
                SelectedPathologist = await DoctorData.GetDoctorAsync((Guid)pathReport.Pathologist2Id);


                //var doctors = await DoctorData.ListDoctorsAsync(CancellationToken);

                pathologists.Append(SelectedPathologist);
            }

            var index = 0;
            var titleStr = "Edit Doctor";

            if (pathReport.Pathologist2Id == null)
            {
                index = -1;
                titleStr = "Create Doctor";
            }
            else
            {
                //index = pathologists.ToList().FindIndex(d => d.DisplayName == pathReport.Pathologist2);
                index = 0; // Since we are only passing the selected pathologist, the index is always 0
            }

            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<EditDoctorDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, pathologists); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, index); // Set initial position
            parameters.Add(p => p.Doctor, SelectedPathologist);

            var options = _options;

            var dialog = await DialogService.ShowAsync<EditDoctorDialog>(titleStr, parameters, options);
            var result = await dialog.Result; // Execution pauses here

            // This code only runs after the dialog closes.
            if (!result.Canceled && result.Data is Doctor updatedDoctor && pathReport.RcaExportDate is null)
            {
                //PathologistList = await DoctorData.ListPathologistsAsync(CancellationToken); // Refresh the doctor list in case of any changes
                pathReport.Pathologist2Id = updatedDoctor.DoctorId;
                pathReport.Pathologist2 = updatedDoctor.DisplayName;
                pathReport.Path2Address1 = updatedDoctor.Address1;
                pathReport.Path2Address2 = updatedDoctor?.Address2;
                pathReport.Path2Address3 = updatedDoctor?.Address3;
                pathReport.Path2City = updatedDoctor?.City;
                pathReport.Path2State = updatedDoctor?.State;
                pathReport.Path2ZipCode = updatedDoctor?.ZipCode;
                pathReport.Path2County = updatedDoctor.County;
                pathReport.Path2PhoneNumber1 = updatedDoctor.PhoneNumber1;
                pathReport.Path2PhoneNumber2 = updatedDoctor?.PhoneNumber2;
                pathReport.Path2FaxNumber = updatedDoctor.FaxNumber;
                pathReport.Path2Email = updatedDoctor.Email;
                pathReport.PathologistEntity2 = updatedDoctor;
                StateHasChanged(); // Tell Blazor to re-render the component
            }
        }


        private async Task AddNewPhoneNumber()
        {
            var newItem = new PatientPhoneNumber();
            if (Patient.PatientPhoneNumbers.Count == 0)
            {
                newItem.IsPrimary = true; // Set the first item as primary
            }
            else
            {
                newItem.IsPrimary = false; // Subsequent items are not primary by default
            }
            newItem.PatientPhoneNumberId = Guid.NewGuid(); // Assign a new unique ID
            Patient.PatientPhoneNumbers.Add(newItem); // Add to your data source
            await InvokeAsync(StateHasChanged);
            await Task.Delay(100); // Give the UI a moment to update
            //await dataGrid.SetEditingItemAsync(newItem); // Start inline editing - shows modal
        }

        private async Task DeletePhoneNumber(PatientPhoneNumber item)
        {
            if (item.PhoneNumber == "")
            {
                Patient.PatientPhoneNumbers.Remove(item); // Remove from data source
                await InvokeAsync(StateHasChanged);
                return; // Exit if the phone number is empty
            }

            bool? result = await DialogService.ShowMessageBoxAsync(
                          "Warning", // Dialog title
                          $"Are you sure you want to delete '{item.PhoneNumber}'? Save the record to complete deletion.", // Message
                          yesText: "Delete!", // Text for the confirmation button
                          cancelText: "Cancel" // Text for the cancel button
                      );

            if (result == true) // User clicked 'Delete!'
            {
                Patient.PatientPhoneNumbers.Remove(item); // Remove from data source
                await InvokeAsync(StateHasChanged);
            }

        }

        private void SetPatientHistoricalData(PathReport path)
        {
            var phoneNumberList = new List<string>();
            var phoneNumbers = string.Empty;

            foreach (var phone in Patient.PatientPhoneNumbers.OrderBy(x => x.IsPrimary))
            {
                var phoneStr = phone.PhoneNumber;
                phoneStr += "(" + phone.PhoneType + ")";
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

            path.DxAddress1 = Patient.Address1;
            path.DxAddress2 = Patient.Address2;
            path.DxCity = Patient.City;
            path.DxState = Patient.State;
            path.DxZipCode = Patient.ZipCode;
            path.DxCounty = Patient.County;
            path.DxPhoneNumber = phoneNumbers;

        }

        private void AddNewPathReport()
        {
            Patient.PathReports.Add(new PathReport());
            var newReport = Patient.PathReports.Last();
            newReport.PathIndex = Patient.PathReports.Count; // Set the index for the new PathReport
            newReport.SlidesResideAtSubmittingHospital = "Yes"; // Set the default value for SlidesResideAtSubmittingHospital
            if (ProcedureList.Count() == 1)
            {
                newReport.PathProcedure = ProcedureList.First();
            }
            if (SiteList.Count() == 1)
            {
                newReport.Site = SiteList.First().LookupName;
                newReport.SiteCode = SiteList.First().LookupCode; // Set SiteCode
            }

            SetPatientHistoricalData(newReport);
        }

        private async Task DeletePathReport(PathReport pathReport)
        {
            if (pathReport.SubmittingHospital == "")
            {
                Patient.PathReports.Remove(pathReport);

                var index = 0;
                foreach (var report in Patient.PathReports)
                {
                    index++;
                    // Update the PathIndex for each remaining PathReport
                    report.PathIndex = index;
                }

                await InvokeAsync(StateHasChanged);
                return; // Exit if the path report is empty
            }

            bool? result = await DialogService.ShowMessageBoxAsync(
                          "Warning", // Dialog title
                          $"Are you sure you want to delete this path report? Save the record to complete deletion.", // Message
                          yesText: "Delete!", // Text for the confirmation button
                          cancelText: "Cancel" // Text for the cancel button
                      );
            if (result == true) // User clicked 'Delete!'
            {
                Patient.PathReports.Remove(pathReport);

                var index = 0;
                foreach (var report in Patient.PathReports)
                {
                    index++;
                    // Update the PathIndex for each remaining PathReport
                    report.PathIndex = index;
                }

                await InvokeAsync(StateHasChanged);
            }

        }





        private void CheckPrimaryValue(bool isChecked, PatientPhoneNumber item)
        {
            if (!isChecked)
            {
                item.IsPrimary = false; // If unchecked, just set IsPrimary to false
                return;
            }

            item.IsPrimary = isChecked;
            if (item.IsPrimary)
            {
                // Uncheck all other items
                foreach (var phoneNumber in Patient.PatientPhoneNumbers)
                {
                    if (phoneNumber != item)
                    {
                        phoneNumber.IsPrimary = false;
                    }
                }
            }
        }


        private async Task<IEnumerable<GeoapifySuggestion>> Search(string text, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < 3)
            {
                return Enumerable.Empty<GeoapifySuggestion>();
            }

            var q = text.Trim();

            // IMPORTANT: encode user input
            var encoded = Uri.EscapeDataString(q);

            // Force GeoJSON since you're deserializing Features/Properties
            var url = $"https://api.geoapify.com/v1/geocode/autocomplete?text={encoded}&format=geojson&apiKey={GeoapifyApiKey}";

            try
            {
                var client = ClientFactory.CreateClient("Geoapify");

                using var resp = await client.GetAsync(url, cancellationToken);
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);

                Console.WriteLine($"Geoapify status={(int)resp.StatusCode} {resp.StatusCode}");
                Console.WriteLine(body.Length > 500 ? body[..500] : body);

                if (!resp.IsSuccessStatusCode)
                {
                    Suggestions.Clear();
                    return Enumerable.Empty<GeoapifySuggestion>();
                }

                var response = System.Text.Json.JsonSerializer.Deserialize<GeoapifyAutocompleteResponse>(
                    body,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var features = response?.Features ?? new List<Feature>();

                Suggestions = features
                    .Select(f => new GeoapifySuggestion
                    {
                        FormattedAddress = f.Properties?.Formatted ?? "",
                        City = f.Properties?.City,
                        State_Code = f.Properties?.State_Code,
                        Postcode = f.Properties?.Postcode,
                        County = f.Properties?.County,
                        Address_line1 = f.Properties?.Address_line1,
                        Address_line2 = f.Properties?.Address_line2,
                    })
                    .Where(s => !string.IsNullOrWhiteSpace(s.FormattedAddress))
                    .ToList();

                return Suggestions;
            }
            catch (OperationCanceledException)
            {
                // MudAutocomplete cancels as you type; cancellation is normal.
                return Enumerable.Empty<GeoapifySuggestion>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Geoapify exception: {ex}");
                Suggestions.Clear();
                return Enumerable.Empty<GeoapifySuggestion>();
            }
        }

        private async Task HandleSelection(GeoapifySuggestion newValue)
        {
            SelectedValue = newValue; // Update the component's value
            await SelectAddress(newValue);
        }

        private Task SelectAddress(GeoapifySuggestion? suggestion)
        {
            if (suggestion is null)
            {
                // X was clicked (clear)
                Patient.City = "";
                Patient.State = "";
                Patient.ZipCode = "";
                Patient.County = "";
                Patient.Address1 = "";
                // Patient.Address2 = "";

                Suggestions.Clear();

                EditContext.NotifyFieldChanged(new FieldIdentifier(Patient, nameof(Patient.City)));
                EditContext.NotifyFieldChanged(new FieldIdentifier(Patient, nameof(Patient.State)));
                EditContext.NotifyFieldChanged(new FieldIdentifier(Patient, nameof(Patient.ZipCode)));
                EditContext.NotifyFieldChanged(new FieldIdentifier(Patient, nameof(Patient.County)));
                EditContext.NotifyFieldChanged(new FieldIdentifier(Patient, nameof(Patient.Address1)));

                return Task.CompletedTask;
            }

            Patient.City = suggestion.City ?? "";
            Patient.State = suggestion.State_Code ?? "";
            Patient.ZipCode = suggestion.Postcode ?? "";
            Patient.Address1 = suggestion.Address_line1 ?? "";

            Patient.County = NormalizeCounty(suggestion.County);

            Suggestions.Clear();

            EditContext.NotifyFieldChanged(new FieldIdentifier(Patient, nameof(Patient.City)));
            EditContext.NotifyFieldChanged(new FieldIdentifier(Patient, nameof(Patient.State)));
            EditContext.NotifyFieldChanged(new FieldIdentifier(Patient, nameof(Patient.ZipCode)));
            EditContext.NotifyFieldChanged(new FieldIdentifier(Patient, nameof(Patient.County)));
            EditContext.NotifyFieldChanged(new FieldIdentifier(Patient, nameof(Patient.Address1)));

            return Task.CompletedTask;
        }

        private static string NormalizeCounty(string? county)
        {
            if (string.IsNullOrWhiteSpace(county))
                return "";

            const string suffix = " County";
            return county.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                ? county[..^suffix.Length]
                : county;
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
            public string County { get; set; } = "";
            public string Postcode { get; set; } = "";
            public string State_Code { get; set; } = "";
            public string Address_line1 { get; set; } = "";
            public string Address_line2 { get; set; } = "";
        }

        public class GeoapifySuggestion
        {
            public string FormattedAddress { get; set; } = "";


            // You might want to store other properties from the Geoapify response here

            public string City { get; set; } = "";
            public string County { get; set; } = "";
            public string Postcode { get; set; } = "";
            public string State_Code { get; set; } = "";
            public string Address_line1 { get; set; } = "";
            public string Address_line2 { get; set; } = "";
        }
    }

}
