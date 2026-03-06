using ExcelDataReader; // NuGet package: EPPlus
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Services;
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
    public partial class CECSPathCaseRace : Microsoft.AspNetCore.Components.ComponentBase
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public IEnumerable<PathCountByStudyByDate> PathReports { get; set; }
        public int CECSCaseTotal { get; set; } = 0;
        public IEnumerable<RaceCountByDate> RaceCounts { get; set; } = new List<RaceCountByDate>();
        public IEnumerable<EthnicityCountByDate> EthnicityCounts { get; set; } = new List<EthnicityCountByDate>();

        private DateTime? startDate { get; set; }
        private DateTime? endDate { get; set; }

        public string StudyColor { get; set; } = "#ffa07aa1"; // Color associated with the study, not mapped to the database


        protected override async Task OnInitializedAsync()
        {


        }

        public async Task Generate()
        {
            PathReports = (List<PathCountByStudyByDate>)await ReportData.GetCECSPathsByDateAsync(startDate.Value.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture), endDate.Value.AddDays(1).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture));
            StateHasChanged();

            CECSCaseTotal = await ReportData.GetCESCCasesByDateAsync(startDate.Value.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture), endDate.Value.AddDays(1).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture));

            RaceCounts = await ReportData.GetCECSRaceCountByDateAsync(startDate.Value.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture), endDate.Value.AddDays(1).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture));

            EthnicityCounts = await ReportData.GetCECSEthnicityCountByDateAsync(startDate.Value.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture), endDate.Value.AddDays(1).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture));

        }


    }
}


