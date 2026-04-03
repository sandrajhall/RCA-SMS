using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Drawing;
using System.Linq;

namespace RCA_StudyManagementSystem.Client.Pages.Cases
{
    public partial class PatientHistoryDialog : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private void Cancel() => MudDialog.Cancel();

        [Parameter]
        public Patient Patient { get; set; } = new Patient();

        [Parameter]
        public List<Patient> CarouselRecords { get; set; } = new List<Patient>(); // Receives the filtered items
        [Parameter]
        public int InitialSelectedIndex { get; set; } = 1;

        public string StudyPrefix { get; set; } = string.Empty;


        private int Index = 0;

        public string StudyColor;

        public List<Study> Studies = new List<Study>();

        public bool IsLoading = true;

        private List<Patient> displayedRecords = new List<Patient>();
        private int currPage = 1; // Current page number
        private int pageSize = 1; // Number of records per page
        private int totalPages => (int)Math.Ceiling((double)CarouselRecords.Count / pageSize);

        private Dictionary<string, string> UserLookup = new();


        protected override async void OnInitialized()
        {
            if (Patient == null || Patient.PatientId == Guid.Empty)
            {
                await UpdateDisplayedRecords();
            }

        }

        private async Task UpdateDisplayedRecords()
        {
            int skip = (currPage - 1) * pageSize;
            displayedRecords = CarouselRecords.Skip(skip).Take(pageSize).ToList();
            Patient = displayedRecords.FirstOrDefault() ?? new Patient();
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
            Patient = CarouselRecords[InitialSelectedIndex];
            OnPageChanged(InitialSelectedIndex + 1);
            StudyColor = await StudyData.GetStudyColorAsync(CarouselRecords.FirstOrDefault()?.StudyId ?? Guid.Empty) ?? "#FFFFFF";
            
            var response = await UserData.GetAllUsersAsync();
            if (response != null)
            {
                UserLookup = response;
            }

            await InvokeAsync(StateHasChanged);
        }

    }
}
