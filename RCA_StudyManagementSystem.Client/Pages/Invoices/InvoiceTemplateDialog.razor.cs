using Microsoft.AspNetCore.Components;
using MudBlazor;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Drawing;
using RCA_StudyManagementSystem.Client.Utilities;
using System.Threading.Tasks;
using RCA_StudyManagementSystem.Client.Services;


namespace RCA_StudyManagementSystem.Client.Pages.Invoices
{
    public partial class InvoiceTemplateDialog : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private void Cancel() => MudDialog.Cancel();

        [Parameter]
        public Invoice Invoice { get; set; } = new Invoice();

        [Parameter]
        public List<Invoice> CarouselRecords { get; set; } = new List<Invoice>(); // Receives the filtered items
        [Parameter]
        public int InitialSelectedIndex { get; set; }

        private Transition Transition = Transition.Fade; // Example transition


        private int Index = 0;


        private List<Invoice> displayedRecords = new List<Invoice>();
        private int currPage = 1; // Current page number
        private int pageSize = 1; // Number of records per page
        private int totalPages => (int)Math.Ceiling((double)CarouselRecords.Count / pageSize);

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        private IEnumerable<GroupedInvoiceItems> GroupedItems { get; set; } = new List<GroupedInvoiceItems>();


        protected override void OnInitialized()
        {
            UpdateDisplayedRecords();

        }

        private async Task UpdateDisplayedRecords()
        {
            int skip = (currPage -1) * pageSize;
            displayedRecords = CarouselRecords.Skip(skip).Take(pageSize).ToList();
            Invoice = displayedRecords.FirstOrDefault() ?? new Invoice();
            Quarter quarter = new Quarter(Int32.Parse(Invoice.InvoiceQuarter.Split("Quarter")[0]), Int32.Parse(Invoice.InvoiceQuarter.Split("Quarter")[1]));
            StartDate = quarter.StartDate;
            EndDate = quarter.EndDate;
            GroupedItems = Invoice.InvoiceItems
                .GroupBy(item => item.Hospital.HospitalName) // Group by the Category property
                .Select(group => new GroupedInvoiceItems
                {
                    HospitalName = group.Key,
                    Items = group.ToList()
                })
                .ToList();
            InvokeAsync(StateHasChanged);
        }

        private void OnPageChanged(int newPage)
        {
            currPage = newPage;
            UpdateDisplayedRecords();
        }


        protected override async Task OnParametersSetAsync()
        {
            Invoice = CarouselRecords[InitialSelectedIndex];
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
