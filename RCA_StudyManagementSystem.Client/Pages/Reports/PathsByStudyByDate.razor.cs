using ExcelDataReader; // NuGet package: EPPlus
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Client.Utilities;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using static MudBlazor.CategoryTypes;


namespace RCA_StudyManagementSystem.Client.Pages.Reports
{
    public partial class PathsByStudyByDate : Microsoft.AspNetCore.Components.ComponentBase
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public IEnumerable<PathCountByStudyByDate> PathReports { get; set; }

        private DateTime? startDate { get; set; }
        private DateTime? endDate { get; set; }

        private Guid studyId { get; set; }

        protected Study studySelectValue;
        protected string studySelectText;
        protected bool ShowFields = false;
        private IEnumerable<Study> StudyList = new List<Study>();
        public string StudyColor { get; set; } = string.Empty; // Color associated with the study, not mapped to the database


        protected override async Task OnInitializedAsync()
        {
            StudyList = await StudyData.ListStudiesAsync();
            if (studyId != Guid.Empty)
            {
                var study = await StudyData.GetStudyAsync(studyId);
                if (study != null)
                {
                    await OnStudySelectChanged(study);
                }
            }

        }

        private async Task OnStudySelectChanged(Study value)
        {
            ShowFields = true; // Show additional fields when a study is selected
            studySelectValue = value;
            studySelectText = value.Name;

            studyId = value.StudyId; // Update the StudyId for the form
            StudyColor = value.ColorLight; // Update the color based on the selected study

            await InvokeAsync(StateHasChanged);
        }

        public async Task Generate()
        {
            if (studyId == Guid.Empty || !startDate.HasValue || !endDate.HasValue)
            {
                Snackbar.Add("Please select a study and date range before generating a report.", Severity.Warning, options =>
                {
                    options.RequireInteraction = true; // User must manually dismiss
                });
                return;
            }
            PathReports = (List<PathCountByStudyByDate>)await ReportData.GetPathsByStudyByDateAsync(studyId, startDate.Value.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture), endDate.Value.AddDays(1).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture));
            StateHasChanged();
        }

        private async Task OnExport()
        {
            if (studyId == Guid.Empty || !startDate.HasValue || !endDate.HasValue)
            {
                Snackbar.Add("Please select a study and date range before downloading.", Severity.Warning, options =>
                {
                    options.RequireInteraction = true; // User must manually dismiss
                });
                return;
            }
            var Study = await StudyData.GetStudyAsync(studyId);
            // create a new batch

            var batchPrefix = "EXP-" + Study.Prefix;
            

            var exportData = await ReportData.GetPathsByStudyByDateCSVAsync(studyId, startDate.Value.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture), endDate.Value.AddDays(1).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture));

            var dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (exportData != null)
            {
                // Call JavaScript function to download CSV
                await JSRuntime.InvokeVoidAsync("downloadFile", $"{Study.Prefix}_PathCounts_{dateStr}.csv", exportData);
            }
            else
            {
                Snackbar.Add("No data available for export.", Severity.Warning, options =>
                {
                    options.RequireInteraction = true; // User must manually dismiss
                });
            }

           
        }


    }
}


