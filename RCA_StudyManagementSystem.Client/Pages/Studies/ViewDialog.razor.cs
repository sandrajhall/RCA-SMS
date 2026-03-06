using Microsoft.AspNetCore.Components;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;

namespace RCA_StudyManagementSystem.Client.Pages.Studies
{
    public partial class ViewDialog : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private void Cancel() => MudDialog.Cancel();

        [Parameter]
        public List<Study> CarouselRecords { get; set; } = new List<Study>(); // Receives the filtered items
        [Parameter]
        public int InitialSelectedIndex { get; set; }

        [Parameter]
        public Study Study { get; set; } = new Study();

        public IEnumerable<StudyLookupView> StudyLookups { get; set; } = new List<StudyLookupView>();
        public IEnumerable<StudyHistologyView> StudyHistologies { get; set; } = new List<StudyHistologyView>();

        public IEnumerable<StudyLookupView> StudyRace { get; set; } = new List<StudyLookupView>();
        public IEnumerable<StudyLookupView> StudyGender { get; set; } = new List<StudyLookupView>();
        public IEnumerable<StudyLookupView> StudyEthnicity { get; set; } = new List<StudyLookupView>();
        public IEnumerable<StudyLookupView> StudyProcedure { get; set; } = new List<StudyLookupView>();
        public IEnumerable<StudyLookupView> StudyCounty { get; set; } = new List<StudyLookupView>();
        public IEnumerable<StudyLookupView> StudySite { get; set; } = new List<StudyLookupView>();



        private int Index = 0;
        private List<Study> displayedRecords = new List<Study>();
        private int currPage = 1; // Current page number
        private int pageSize = 1; // Number of records per page
        private int totalPages => (int)Math.Ceiling((double)CarouselRecords.Count / pageSize);

        protected override async Task  OnInitializedAsync()
        {
            await UpdateDisplayedRecordsAsync();
        }

        private async Task UpdateDisplayedRecordsAsync()
        {
            int skip = (currPage - 1) * pageSize;
            displayedRecords = CarouselRecords.Skip(skip).Take(pageSize).ToList();
            Study = displayedRecords.FirstOrDefault() ?? new Study();

            StudyLookups = await StudyLookupData.ListStudyLookupsByStudyIdAsync(Study.StudyId);
            StudyHistologies = await StudyHistologyData.ListStudyHistologiesByStudyIdAsync(Study.StudyId);
            StudyHistologies = StudyHistologies.OrderBy(sh => sh.HistologyCode).ThenBy(sh => sh.HistologyName);

            StudyRace = StudyLookups
                .Where(sl => sl.LookupType == "Race").ToList();
            StudyGender = StudyLookups
                .Where(sl => sl.LookupType == "Gender").ToList();
            StudyEthnicity = StudyLookups
                .Where(sl => sl.LookupType == "Ethnicity").ToList();
            StudyProcedure = StudyLookups
                .Where(sl => sl.LookupType == "Procedure").ToList();
            StudyCounty = StudyLookups
                .Where(sl => sl.LookupType == "County").ToList();
            StudySite = StudyLookups
                .Where(sl => sl.LookupType == "Site").ToList();

            await InvokeAsync(StateHasChanged);
        }

        private async Task OnPageChanged(int newPage)
        {
            currPage = newPage;
            await UpdateDisplayedRecordsAsync();
        }


        protected override async Task OnParametersSetAsync()
        {
            Study = CarouselRecords[InitialSelectedIndex];
            OnPageChanged(InitialSelectedIndex + 1);
        }

    }
}
