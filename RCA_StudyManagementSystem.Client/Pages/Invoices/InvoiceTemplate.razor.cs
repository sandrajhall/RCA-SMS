using Microsoft.AspNetCore.Components;
using MudBlazor;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Drawing;
using RCA_StudyManagementSystem.Client.Utilities;
using System.Threading.Tasks;
using RCA_StudyManagementSystem.Client.Services;


namespace RCA_StudyManagementSystem.Client.Pages.Invoices
{
    public partial class InvoiceTemplate : ComponentBase
    {

        [Parameter]
        public Invoice Invoice { get; set; } = new Invoice();

        [Parameter]
        public Guid InvoiceId { get; set; }

        [Parameter] public List<GroupedInvoiceItems> GroupedItems { get; set; } = new();
        [Parameter] public List<Study> Studies { get; set; } = new();


        private Transition Transition = Transition.Fade; // Example transition


        private int Index = 0;


        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        private static string FormatCurrency(decimal value)
                                                            => value.ToString("C2");

        private const decimal PaymentPerPath = 15m;



        protected override async Task OnInitializedAsync()
        {
            Invoice = await InvoiceData.GetInvoiceAsync(InvoiceId);

            Quarter quarter = new Quarter(
                int.Parse(Invoice.InvoiceQuarter.Split("Quarter")[0]),
                int.Parse(Invoice.InvoiceQuarter.Split("Quarter")[1]));

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

        protected override async Task OnParametersSetAsync()
        {


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

