using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Drawing;
using System.Linq;
using static MudBlazor.CategoryTypes;

namespace RCA_StudyManagementSystem.Client.Pages.Cases
{
    public partial class ViewDialog : Microsoft.AspNetCore.Components.ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private void Cancel() => MudDialog.Cancel();

        private readonly DialogOptions _options = new() { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true };

        [Parameter]
        public Patient Patient { get; set; } = new Patient();


        [Parameter]
        public PathReport PathReport { get; set; } = new PathReport();
        [Parameter]
        public List<Patient> CarouselRecords { get; set; } = new List<Patient>(); // Receives the filtered items
        [Parameter]
        public int InitialSelectedIndex { get; set; }

        [Parameter]
        public List<PathReport> PathCarouselRecords { get; set; } = new List<PathReport>(); // Receives the filtered items
        [Parameter]
        public int PathInitialSelectedIndex { get; set; }

        public string StudyPrefix { get; set; } = string.Empty;

        private Transition Transition = Transition.Fade; // Example transition


        private IEnumerable<PatientPhoneNumber> PatientPhoneNumbers => Patient?.PatientPhoneNumbers ?? new List<PatientPhoneNumber>();
        private IEnumerable<PathReport> PathReports => Patient?.PathReports ?? new List<PathReport>();

        private IEnumerable<PathReportView> PathReportViews { get; set; } = new List<PathReportView>();
        private IEnumerable<ExportView> exportItems { get; set; } = new List<ExportView>();
        private IEnumerable<Patient> patientHistory { get; set; } = new List<Patient>();
        private IEnumerable<PatientPhoneNumber> patientPhoneNumberHistory { get; set; } = new List<PatientPhoneNumber>();
        private IEnumerable<PathReport> pathReportHistory { get; set; } = new List<PathReport>();


        private int Index = 0;

        private string studyColor;

        public List<Study> Studies = new List<Study>();

        public bool IsLoading = true;

        private List<Patient> displayedRecords = new List<Patient>();
        private int currPage = 1; // Current page number
        private int pageSize = 1; // Number of records per page
        private int totalPages => (int)Math.Ceiling((double)CarouselRecords.Count / pageSize);
        private DateTime? doctor1VerifiedDate = null;
        private DateTime? doctor2VerifiedDate = null;
        private DateTime? path1VerifiedDate = null;
        private DateTime? path2VerifiedDate = null;

        private Dictionary<string, string> UserLookup = new();


        protected override async void OnInitialized()
        {
            if (Patient == null)
            {
                await UpdateDisplayedRecords();
            }

            var response = await UserData.GetAllUsersAsync();
            if (response != null)
            {
                UserLookup = response;
            }
        }

        private async Task UpdateDisplayedRecords()
        {
            int skip = (currPage - 1) * pageSize;
            displayedRecords = CarouselRecords.Skip(skip).Take(pageSize).ToList();
            Patient = displayedRecords.FirstOrDefault() ?? new Patient();
            exportItems = new List<ExportView>();
            await InvokeAsync(StateHasChanged);

            foreach (var item in displayedRecords)
            {

                foreach (var path in Patient.PathReports)
                {
                    var StudyTask = StudyData.GetStudyAsync(Patient.StudyId);
                    var exportViewTask = PathReportData.GetPathReportExportHistoryAsync(path.PathReportId);

                    await Task.WhenAll(StudyTask, exportViewTask);

                    var Study = StudyTask.Result;
                    var exportView = exportViewTask.Result;

                    PathReportView prv = new PathReportView
                    {
                        StudyName = Study.Name,
                        StudyPrefix = Study.Prefix,
                        PatientId = Patient.PatientId,
                        PathReportId = path.PathReportId,
                        MigratedCCRNumber = path.MigratedCCRNumber,
                        DateOfProcedure = path.DateOfProcedure,
                        IsPathMinAgeValid = Study.IsPathMinAgeValid(path),
                        IsPathMaxAgeValid = Study.IsPathMaxAgeValid(path),
                        IsPathMaxAgeWeekOut = Study.IsPathMaxAgeWeekOut(path),
                        IsPathMaxNumValid = Study.IsPathMaxNumValid(Patient.PathReports.Count),
                        IsCountyValid = Study.IsCountyValid(Patient.County),
                        PathId = Patient.CaseNumber + "-" + path.PathIndex,

                    };

                    if (prv.IsPathMaxAgeWeekOut)
                    {
                        if (Study.PathMaxAge != null && prv.DateOfProcedure != null)
                            prv.MaxAgeDiff = Study.CalculateAgeDays(prv.DateOfProcedure!.Value, DateTime.Now);
                    }
                    if (!PathReportViews.Contains(prv))
                        PathReportViews = PathReportViews.Append(prv);

                    foreach (var export in exportView)
                    {
                        if (!exportItems.Contains(export))
                            exportItems = exportItems.Append(export);
                    }

                    patientHistory = await PatientData.GetPatientHistoryAsync(Patient.PatientId);
                    patientPhoneNumberHistory = await PatientData.GetPatientPhoneNumberHistoryAsync(Patient.PatientId);
                    pathReportHistory = await PatientData.GetPathReportHistoryAsync(Patient.PatientId);
                    
                }

            }
            await InvokeAsync(StateHasChanged);
            IsLoading = false;

        }

        private async Task OnPageChanged(int newPage)
        {
            currPage = newPage;

            await UpdateDisplayedRecords();

            await InvokeAsync(StateHasChanged);
        }

        async Task<IDialogReference> ViewPatientHistory(Patient args, int index)
        {
            //int index = patientHistory
            //    .Select((item, idx) => new { item, idx })
            //    .FirstOrDefault(x => x.item.PatientId == args.PatientId)?.idx ?? 0;

            var parameters = new DialogParameters<PatientHistoryDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, patientHistory.ToList()); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, index); // Set initial position

            var options = _options;

            return await DialogService.ShowAsync<PatientHistoryDialog>("Patient History View", parameters, options);

        }

        async Task<IDialogReference> ViewPathReportHistory(PathReport args, int index)
        {
            //int index = patientHistory
            //    .Select((item, idx) => new { item, idx })
            //    .FirstOrDefault(x => x.item.PatientId == args.PatientId)?.idx ?? 0;

            var parameters = new DialogParameters<PathReportHistoryDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.PathCarouselRecords, pathReportHistory.ToList()); // Pass filtered items
            parameters.Add(p => p.PathInitialSelectedIndex, index); // Set initial position

            var options = _options;

            return await DialogService.ShowAsync<PathReportHistoryDialog>("PathReport History View", parameters, options);

        }


        protected override async Task OnParametersSetAsync()
        {
            Patient = CarouselRecords[InitialSelectedIndex];
            OnPageChanged(InitialSelectedIndex + 1);


            await InvokeAsync(StateHasChanged);
        }


        private string FormatPhoneNumber(string number)
        {
            if (string.IsNullOrEmpty(number) || number.Length != 10)
            {
                return number; // Return as is if not a valid 10-digit number
            }
            return $"({number.Substring(0, 3)}) {number.Substring(3, 3)}-{number.Substring(6, 4)}";
        }
    }
}
