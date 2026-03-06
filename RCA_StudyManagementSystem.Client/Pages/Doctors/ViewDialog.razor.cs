using Microsoft.AspNetCore.Components;
using MudBlazor;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Drawing;

namespace RCA_StudyManagementSystem.Client.Pages.Doctors
{
    public partial class ViewDialog : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private void Cancel() => MudDialog.Cancel();

        [Parameter]
        public Doctor Doctor { get; set; } = new Doctor();

        [Parameter]
        public List<Doctor> CarouselRecords { get; set; } = new List<Doctor>(); // Receives the filtered items
        [Parameter]
        public int InitialSelectedIndex { get; set; }

        private Transition Transition = Transition.Fade; // Example transition


        private int Index = 0;


        private List<Doctor> displayedRecords = new List<Doctor>();
        private int currPage = 1; // Current page number
        private int pageSize = 1; // Number of records per page
        private int totalPages => (int)Math.Ceiling((double)CarouselRecords.Count / pageSize);

        protected override void OnInitialized()
        {
            UpdateDisplayedRecords();
        }

        private void UpdateDisplayedRecords()
        {
            int skip = (currPage -1) * pageSize;
            displayedRecords = CarouselRecords.Skip(skip).Take(pageSize).ToList();
            Doctor = displayedRecords.FirstOrDefault() ?? new Doctor();
            InvokeAsync(StateHasChanged);
        }

        private void OnPageChanged(int newPage)
        {
            currPage = newPage;
            UpdateDisplayedRecords();
        }


        protected override async Task OnParametersSetAsync()
        {
            Doctor = CarouselRecords[InitialSelectedIndex];
            OnPageChanged(InitialSelectedIndex + 1);

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
