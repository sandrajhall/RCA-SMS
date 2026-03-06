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


        private Transition Transition = Transition.Fade; // Example transition


        private int Index = 0;


        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        private IEnumerable<GroupedInvoiceItems> GroupedItems { get; set; } = new List<GroupedInvoiceItems>();


        protected override async Task OnInitializedAsync()
        {
            Invoice = await InvoiceData.GetInvoiceAsync(InvoiceId);
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
            await InvokeAsync(StateHasChanged);
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
