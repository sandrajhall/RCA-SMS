using Microsoft.AspNetCore.Components;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Drawing;

namespace RCA_StudyManagementSystem.Client.Pages.ReimbursementEntities
{
    public partial class ViewDialog : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private void Cancel() => MudDialog.Cancel();

        [Parameter]
        public ReimbursementEntity ReimbursementEntity { get; set; } = new ReimbursementEntity();

        [Parameter]
        public List<ReimbursementEntity> CarouselRecords { get; set; } = new List<ReimbursementEntity>(); // Receives the filtered items
        [Parameter]
        public int InitialSelectedIndex { get; set; }

        private Transition Transition = Transition.Fade; // Example transition


        private int Index = 0;

        MudDataGrid<Invoice>? invGrid { get; set; }

        public CancellationToken CancellationToken { get; set; } = new CancellationToken();

        public IEnumerable<Invoice>? Invoices { get; set; }

        private IEnumerable<Invoice>? _displayItems { get; set; }


        private List<ReimbursementEntity> displayedRecords = new List<ReimbursementEntity>();
        private List<Hospital> associatedHospitals = new List<Hospital>();
        private List<RCAContact> RCAContacts = new List<RCAContact>();
        private int currPage = 1; // Current page number
        private int pageSize = 1; // Number of records per page
        private int totalPages => (int)Math.Ceiling((double)CarouselRecords.Count / pageSize);

        protected override async Task OnInitializedAsync()
        {
            await UpdateDisplayedRecords();
        }

        private async Task UpdateDisplayedRecords()
        {
            int skip = (currPage -1) * pageSize;
            displayedRecords = CarouselRecords.Skip(skip).Take(pageSize).ToList();
            ReimbursementEntity = displayedRecords.FirstOrDefault() ?? new ReimbursementEntity();

            // Load associated hospitals and contacts
            associatedHospitals = (List<Hospital>)await HospitalData.ListHospitalsForReimbursementEntityAsync(ReimbursementEntity.ReimbursementEntityId);
            RCAContacts = (List<RCAContact>)await RCAContactData.ListRCAContactsByReimbursementEntityIdAsync(ReimbursementEntity.ReimbursementEntityId);

            Invoices = await InvoiceData.ListInvoicesSentAsync(CancellationToken);

            _displayItems = Invoices.Where(re => re.ReimbursementEntityId == ReimbursementEntity.ReimbursementEntityId);

            await InvokeAsync(StateHasChanged);
        }

        private async Task OnPageChanged(int newPage)
        {
            currPage = newPage;
            await UpdateDisplayedRecords();
        }


        protected override async Task OnParametersSetAsync()
        {
            ReimbursementEntity = CarouselRecords[InitialSelectedIndex];
            await OnPageChanged(InitialSelectedIndex + 1);

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
