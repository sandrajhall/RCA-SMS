using Microsoft.AspNetCore.Components;
using MudBlazor;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Drawing;

namespace RCA_StudyManagementSystem.Client.Pages.Hospitals
{
    public partial class EditHospitalDialog : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private void Cancel() => MudDialog.Cancel();

        [Parameter]
        public Hospital? Hospital { get; set; }

        [Parameter]
        public List<Hospital> CarouselRecords { get; set; } = new List<Hospital>(); // Receives the filtered items
        [Parameter]
        public int InitialSelectedIndex { get; set; }

        private Transition Transition = Transition.Fade; // Example transition


        private int Index = 0;

        private bool _isSaved = false; // Backing field for IsSaved property

        [Parameter]
        public bool IsSaved
        {
            get => _isSaved; // Expose IsSaved for the form
            set
            {
                if (value == null)
                {
                    _isSaved = false; // Default to false if value is null
                }
                else
                {
                    // Ensure that the value is a boolean
                    if (value.GetType() != typeof(bool))
                    {
                        throw new ArgumentException("IsSaved must be a boolean value.");
                    }
                }
                _isSaved = value; // Set IsSaved when the parameter is set
            }
        }


        private List<Hospital> displayedRecords = new List<Hospital>();
        private int currPage = 1; // Current page number
        private int pageSize = 1; // Number of records per page
        private int totalPages => (int)Math.Ceiling((double)CarouselRecords.Count / pageSize);

        protected override void OnInitialized()
        {
            //UpdateDisplayedRecords();
        }

        private void UpdateDisplayedRecords()
        {
            int skip = (currPage - 1) * pageSize;
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
            if (Hospital != null && CarouselRecords.Count > 0)
            {
                Hospital = CarouselRecords[InitialSelectedIndex];
                OnPageChanged(InitialSelectedIndex + 1);
            }


        }
    }
}
