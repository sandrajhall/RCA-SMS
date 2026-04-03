using Microsoft.AspNetCore.Components;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Drawing;

namespace RCA_StudyManagementSystem.Client.Pages.Hospitals
{
    public partial class HospitalHistoryDialog : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private void Cancel() => MudDialog.Cancel();

        [Parameter]
        public Hospital Hospital { get; set; } = new Hospital();

        [Parameter]
        public List<Hospital> CarouselRecords { get; set; } = new List<Hospital>(); // Receives the filtered items
        [Parameter]
        public int InitialSelectedIndex { get; set; }

        private Transition Transition = Transition.Fade; // Example transition


        private int Index = 0;


        private List<Hospital> displayedRecords = new List<Hospital>();
        private int currPage = 1; // Current page number
        private int pageSize = 1; // Number of records per page
        private int totalPages => (int)Math.Ceiling((double)CarouselRecords.Count / pageSize);

        private Dictionary<string, string> UserLookup = new();


        protected override async Task OnInitializedAsync()
        {
            UpdateDisplayedRecords();

            var response = await UserData.GetAllUsersAsync();
            if (response != null)
            {
                UserLookup = response;
            }
        }

        private void UpdateDisplayedRecords()
        {
            int skip = (currPage -1) * pageSize;
            displayedRecords = CarouselRecords.Skip(skip).Take(pageSize).ToList();
            Hospital = displayedRecords.FirstOrDefault() ?? new Hospital();
            InvokeAsync(StateHasChanged);
        }

        private void OnPageChanged(int newPage)
        {
            currPage = newPage;
            UpdateDisplayedRecords();
        }


        protected override async Task OnParametersSetAsync()
        {
            Hospital = CarouselRecords[InitialSelectedIndex];
            OnPageChanged(InitialSelectedIndex + 1);

        }

    }
}
