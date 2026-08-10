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

        [Parameter] public List<GroupedInvoiceItems> GroupedItems { get; set; } = new();
        [Parameter] public List<Study> Studies { get; set; } = new();

        private static string FormatCurrency(decimal value)
                                                            => value.ToString("C2");

        private const decimal PaymentPerPath = 15m;


        private Transition Transition = Transition.Fade; // Example transition


        private int Index = 0;


        private List<Invoice> displayedRecords = new List<Invoice>();
        private int currPage = 1; // Current page number
        private int pageSize = 1; // Number of records per page
        private int totalPages => (int)Math.Ceiling((double)CarouselRecords.Count / pageSize);

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


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
            // Build the master study list from all invoice items
            Studies = Invoice.InvoiceItems
                .Where(x => x.Study != null)
                .GroupBy(x => x.StudyId)
                .Select(g => g.First().Study!)
                .OrderBy(s => s.InvoiceDesignation)   // or whatever display field you want
                .ToList();

            // Group by hospital and calculate totals
            GroupedItems = Invoice.InvoiceItems
                .GroupBy(item => item.Hospital?.HospitalName ?? "Unknown")
                .Select(group => new GroupedInvoiceItems
                {
                    HospitalName = group.Key,
                    Items = group.ToList(),
                    GroupTotal = group.Sum(x => x.NumPathReports ?? 0)
                })
                .OrderBy(x => x.HospitalName)
                .ToList();

            await InvokeAsync(StateHasChanged);
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


        private static int GetStudyPathReportCount(IEnumerable<InvoiceItem> items, Guid studyId)
        {
            return items
                .Where(x => x.StudyId == studyId)
                .Sum(x => x.NumPathReports ?? 0);
        }

        private static int GetGroupPathReportCount(IEnumerable<InvoiceItem> items)
        {
            return items.Sum(x => x.NumPathReports ?? 0);
        }

        private int GetTotalStudyPathReportCount(Guid studyId)
        {
            return GroupedItems.Sum(g => GetStudyPathReportCount(g.Items, studyId));
        }

        private int GetGrandTotalPathReports()
        {
            return GroupedItems.Sum(g => GetGroupPathReportCount(g.Items));
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
