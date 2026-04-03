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
    public partial class PathReportHistoryDialog : Microsoft.AspNetCore.Components.ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private void Cancel() => MudDialog.Cancel();

        private readonly DialogOptions _options = new() { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true };

        [Parameter]
        public PathReport PathReport { get; set; } = new PathReport();

        [Parameter]
        public List<PathReport> PathCarouselRecords { get; set; } = new List<PathReport>(); // Receives the filtered items
        [Parameter]
        public int PathInitialSelectedIndex { get; set; }

        public string StudyPrefix { get; set; } = string.Empty;

        private Transition Transition = Transition.Fade; // Example transition



        private int Index = 0;

        private string studyColor;

        public List<Study> Studies = new List<Study>();

        public bool IsLoading = true;

        private List<PathReport> displayedRecords = new List<PathReport>();
        private int currPage = 1; // Current page number
        private int pageSize = 1; // Number of records per page
        private int totalPages => (int)Math.Ceiling((double)PathCarouselRecords.Count / pageSize);


        private Dictionary<string, string> UserLookup = new();


        protected override async void OnInitialized()
        {
            if (PathReport == null)
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
            displayedRecords = PathCarouselRecords.Skip(skip).Take(pageSize).ToList();
            PathReport = displayedRecords.FirstOrDefault() ?? new PathReport();
            await InvokeAsync(StateHasChanged);

            foreach (var item in displayedRecords)
            {

                PathReportView prv = new PathReportView
                {
                    PatientId = PathReport.PatientId,
                    PathReportId = PathReport.PathReportId,
                    MigratedCCRNumber = PathReport.MigratedCCRNumber,
                    DateOfProcedure = PathReport.DateOfProcedure,

                    PathId = PathReport.CaseNumber + "-" + PathReport.PathIndex,

                };

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




        protected override async Task OnParametersSetAsync()
        {
            PathReport = PathCarouselRecords[PathInitialSelectedIndex];
            OnPageChanged(PathInitialSelectedIndex + 1);


            await InvokeAsync(StateHasChanged);
        }


    }
}
