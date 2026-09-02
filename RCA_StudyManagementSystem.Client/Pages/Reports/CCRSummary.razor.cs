using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Pages.Archives;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Net;
using static MudBlazor.CategoryTypes;

namespace RCA_StudyManagementSystem.Client.Pages.Reports
{
    public partial class CCRSummary : Microsoft.AspNetCore.Components.ComponentBase
    {
        private List<CCRSummaryView> Rows = new();

        private string _searchString = string.Empty;


        private IEnumerable<Study> StudyList = new List<Study>();
        private CancellationToken token;

        public Guid StudyId { get; set; }
        public string StudyColor { get; set; }

        protected Study studySelectValue;
        protected string studySelectText;
        protected string studyPrefix;

        public int SelectedYear { get; set; }


        private DateTime? startDate { get; set; }
        private DateTime? endDate { get; set; }

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



        protected override async Task OnInitializedAsync()
        {
            StudyList = await StudyData.ListStudiesAsync();

            SelectedYear = DateTime.UtcNow.Year;

            if (StudyId != Guid.Empty)
            {
                var study = await StudyData.GetStudyAsync(StudyId);

                if (study != null)
                {
                    OnStudySelectChanged(study);

                    // Load the report automatically on initial load
                    await LoadGrid();
                }
            }
        }

        private async Task LoadGrid()
        {
            try
            {
                Rows = new List<CCRSummaryView>();

                if (StudyId == Guid.Empty || SelectedYear == 0)
                {
                    return;
                }

                Console.WriteLine($"StudyId: {StudyId}");
                Console.WriteLine($"SelectedYear: {SelectedYear}");

                Rows = await PatientData.GetCCRSummaryAsync(
                    StudyId,
                    SelectedYear);

                if (Rows.Count == 0)
                {
                    Snackbar.Add(
                        $"No report rows were returned for {SelectedYear}.",
                        Severity.Info);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

                Snackbar.Add(
                    $"No report rows were returned for {SelectedYear}.",
                    Severity.Error,
                    options => options.RequireInteraction = true);
            }
        }



        private void OnStudySelectChanged(Study value)
        {
            studySelectValue = value;
            studySelectText = value.Name;
            studyPrefix = value.Prefix;

            StudyId = value.StudyId;
            StudyColor = value.ColorLight;
        }

        private void YearChanged(int value)
        {
            SelectedYear = value;
        }



        public async Task Generate()
        {
            if (StudyId == Guid.Empty)
            {
                ShowError();
                return;
            }

            if (SelectedYear == 0)
            {
                Snackbar.Add(
                    "Please select a valid year.",
                    Severity.Error,
                    options => options.RequireInteraction = true);

                return;
            }

            await LoadGrid();
        }

        private async Task OnExport()
        {
            if (StudyId == Guid.Empty || SelectedYear == 0 )
            {
                Snackbar.Add("Please select a study and year before downloading.", Severity.Warning, options =>
                {
                    options.RequireInteraction = true; // User must manually dismiss
                });
                return;
            }
            var Study = await StudyData.GetStudyAsync(StudyId);

            var fileName = Study.Prefix + "_CCR_Report up to " + SelectedYear;


            var exportData = await PatientData.GetCCRSummaryCSVAsync(StudyId, SelectedYear);

            if (exportData != null)
            {
                // Call JavaScript function to download CSV
                await JSRuntime.InvokeVoidAsync("downloadFile", $"{fileName}.csv", exportData);
            }
            else
            {
                Snackbar.Add("No data available for export.", Severity.Warning, options =>
                {
                    options.RequireInteraction = true; // User must manually dismiss
                });
            }


        }

        private void ShowError()
        {
            Snackbar.Add("You must select a study before uploading.", Severity.Error, options =>
            {
                options.RequireInteraction = true; // User must manually dismiss
            });
        }

    }

}
