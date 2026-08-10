using MudBlazor;
using RCA_StudyManagementSystem.Client.Pages.Archives;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using static MudBlazor.CategoryTypes;

namespace RCA_StudyManagementSystem.Client.Pages.Reports
{
    public partial class PathSubmissions : Microsoft.AspNetCore.Components.ComponentBase
    {
        private List<MonthlyPathSubmissionView> Rows = new();
        private List<DateTime> DaysInMonth = new();
        private string _searchString = string.Empty;
        private List<string> months = new();
        private int year;
        private int month;

        protected string StudySelectText;
        private bool _isLoading;

        private IEnumerable<Study> StudyList = new List<Study>();
        private CancellationToken token;

        public Guid StudyId { get; set; }
        public string StudyColor { get; set; }

        public string SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
        private string SearchString
        {
            get => _searchString;
            set
            {
                if (_searchString != value)
                {
                    _searchString = value;
                }
            }
        }
        private MonthlyPathSubmissionView _selectedItem;
        private void SelectRow(MonthlyPathSubmissionView item) => _selectedItem = item;
        private string SelectedStudyIdString { get; set; } = string.Empty;
        private Study? _studySelectValue;

        private Study? StudySelectValue
        {
            get => _studySelectValue;
            set
            {
                if (Equals(_studySelectValue, value))
                    return;

                _studySelectValue = value;

                if (value is not null)
                {
                    _ = OnStudyChanged(value.StudyId.ToString());
                }
            }
        }
        // quick filter - filter globally across multiple columns with the same input
        private Func<MonthlyPathSubmissionView, bool> _quickFilter => x =>
        {
            //CheckDNC(_searchString);
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.HospitalName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.HospitalShortName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        };

        protected override async Task OnInitializedAsync()
        {
            SelectedMonth = DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture);
            SelectedYear = DateTime.Now.Year;

            StudyList = await StudyData.ListStudiesAsync();
            if (StudyId != Guid.Empty)
            {
                var study = await StudyData.GetStudyAsync(StudyId);
                if (study != null)
                {
                    await OnStudyChanged(study.StudyId.ToString());
                }
            }

            year = SelectedYear;
            month = GetMonthNumber(SelectedMonth);

            // Generates list of month names (January - December)
            months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                .Where(m => !string.IsNullOrEmpty(m))
                .ToList();

            // Generate list of all days in the current month
            DaysInMonth = Enumerable.Range(1, DateTime.DaysInMonth(year, month))
                .Select(day => new DateTime(year, month, day))
                .ToList();
        }

        private async Task LoadGrid()
        {
            if (StudyId == Guid.Empty || _isLoading)
                return;

            try
            {
                _isLoading = true;

                Rows.Clear();
                year = SelectedYear;
                month = GetMonthNumber(SelectedMonth);
                StudyColor = StudyList.FirstOrDefault(s => s.StudyId == StudyId)?.ColorLight ?? "#FFFFFF"; // Default to white if not found

                // Generates list of month names (January - December)
                months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                    .Where(m => !string.IsNullOrEmpty(m))
                    .ToList();

                // Generate list of all days in the current month
                DaysInMonth = Enumerable.Range(1, DateTime.DaysInMonth(year, month))
                    .Select(day => new DateTime(year, month, day))
                    .ToList();

                var submissions = await DailyPathSubmissionData.ListMonthlyPathSubmissionAsync(year, month, StudyId);

                foreach (var submission in submissions)
                {
                    var row = new MonthlyPathSubmissionView
                    {
                        HospitalId = submission.HospitalId,
                        HospitalName = submission.HospitalName,
                        HospitalShortName = submission.HospitalShortName,
                        StudyId = submission.StudyId,
                        DailyValues = submission.DailyValues
                    };
                    Rows.Add(row);
                }

                var hospitals = await HospitalData.ListHospitalsAsync(token);

                foreach (var hospital in hospitals)
                {
                    if (!Rows.Any(r => r.HospitalId == hospital.HospitalId))
                    {
                        var emptyRow = new MonthlyPathSubmissionView
                        {
                            HospitalId = hospital.HospitalId,
                            HospitalName = hospital.HospitalName,
                            HospitalShortName = hospital.HospitalShortName,
                            StudyId = StudyId,
                            DailyValues = new Dictionary<int, string>() // Start with an empty dictionary
                        };

                        Rows.Add(emptyRow);
                    }
                }


                // Ensure all days of the month are represented in the dictionary for each row
                foreach (var row in Rows)
                {
                    foreach (var day in DaysInMonth)
                    {
                        if (!row.DailyValues.ContainsKey(day.Day))
                        {
                            row.DailyValues[day.Day] = string.Empty; // Prevent "null" rendering issues
                        }
                    }
                }
            }
            finally
            {
                _isLoading = false;
            }
        }


        private string SelectedRowStyleFunc(MonthlyPathSubmissionView view, int index)
        {
            if (_selectedItem != null && _selectedItem.Equals(view))
            {
                return $"background-color: {StudyColor} !important;";
            }
            return string.Empty;
        }

        //private async Task OnStudySelectChanged(Study value)
        //{
        //    StudySelectValue = value;
        //    StudySelectText = value.Name;

        //    StudyId = value.StudyId; // Update the StudyId for the form

        //    StudyColor = value.ColorLight; // Update the color based on the selected study

        //    await LoadGrid();
        //}

        private async Task OnStudyChanged(string value)
        {
            SelectedStudyIdString = value ?? string.Empty;

            if (!Guid.TryParse(SelectedStudyIdString, out var selectedStudyId))
            {
                StudyId = Guid.Empty;
                StudyColor = "#FFFFFF";
                Rows.Clear();
                await InvokeAsync(StateHasChanged);
                return;
            }

            StudyId = selectedStudyId;

            var study = StudyList.FirstOrDefault(s => s.StudyId == StudyId);
            if (study != null)
            {
                StudyColor = study.ColorLight;
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task YearChanged(int value)
        {
            SelectedYear = value;
            await InvokeAsync(StateHasChanged);
        }

        private async Task MonthChanged(string value)
        {
            SelectedMonth = value;
            await InvokeAsync(StateHasChanged);
        }

        private async Task OnCellValueChanged(MonthlyPathSubmissionView item, int day, string newValue)
        {
            var auth = await AuthStateProvider.GetAuthenticationStateAsync();
            var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            item.DailyValues[day] = newValue;

            var submissions = await DailyPathSubmissionData.ListDailyPathSubmissionsAsync(token);

            foreach (var d in item.DailyValues)
            {
                var targetDate = new DateTime(year, month, d.Key);

                // 1. Check if the entry already exists
                var existingEntry = submissions
                   .FirstOrDefault(x => x.HospitalId == item.HospitalId && x.Date == targetDate);

                if (existingEntry != null)
                {
                    existingEntry.Value = d.Value; // Update
                    if (string.IsNullOrEmpty(d.Value))
                    {
                        await DailyPathSubmissionData.DeleteDailyPathSubmissionAsync(existingEntry.DailyPathSubmissionId); // Delete if value is empty
                    }
                    else
                    {
                        await DailyPathSubmissionData.UpdateDailyPathSubmissionAsync(existingEntry.DailyPathSubmissionId, userId, existingEntry);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(d.Value))
                    {
                        continue; // Skip creating empty entries
                    }
                    var newEntry = new DailyPathSubmission() // Insert
                    {
                        HospitalId = item.HospitalId,
                        StudyId = item.StudyId,
                        Date = targetDate,
                        Value = d.Value
                    };

                    await DailyPathSubmissionData.CreateDailyPathSubmissionAsync(userId, newEntry);
                }
            }
            StateHasChanged();
        }

        public static int GetMonthNumber(string monthName)
        {
            // Use CultureInfo.InvariantCulture if the month names are always English,
            // or CultureInfo.CurrentCulture if they depend on the user's regional settings.
            // The "MMMM" format specifier is for the full month name.
            DateTime date = DateTime.ParseExact(
                monthName,
                "MMMM",
                CultureInfo.InvariantCulture
            );

            return date.Month;
        }

    }

}
