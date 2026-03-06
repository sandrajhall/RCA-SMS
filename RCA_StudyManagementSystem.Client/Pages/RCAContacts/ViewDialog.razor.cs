using Microsoft.AspNetCore.Components;
using MudBlazor;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Drawing;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Client.Pages.RCAContacts
{
    public partial class ViewDialog : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private void Cancel() => MudDialog.Cancel();

        [Parameter]
        public RCAContact RCAContact { get; set; } = new RCAContact();

        [Parameter]
        public List<RCAContact> CarouselRecords { get; set; } = new List<RCAContact>(); // Receives the filtered items
        [Parameter]
        public int InitialSelectedIndex { get; set; }

        private Transition Transition = Transition.Fade; // Example transition


        private int Index = 0;


        private List<RCAContact> displayedRecords = new List<RCAContact>();
        private int currPage = 1; // Current page number
        private int pageSize = 1; // Number of records per page
        private int totalPages => (int)Math.Ceiling((double)CarouselRecords.Count / pageSize);

        protected override void OnInitialized()
        {
            UpdateDisplayedRecords();
        }

        private async Task UpdateDisplayedRecords()
        {
            int skip = (currPage -1) * pageSize;
            displayedRecords = CarouselRecords.Skip(skip).Take(pageSize).ToList();
            RCAContact = displayedRecords.FirstOrDefault() ?? new RCAContact();
 
            var contactRE =  await RCAContactData.ListReimbursementEntitiesAsync(RCAContact.RCAContactId);
            RCAContact.ReimbursementEntities = contactRE.ReimbursementEntities;
            InvokeAsync(StateHasChanged);
        }

        private void OnPageChanged(int newPage)
        {
            currPage = newPage;
            UpdateDisplayedRecords();
        }


        protected override async Task OnParametersSetAsync()
        {
            RCAContact = CarouselRecords[InitialSelectedIndex];
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
