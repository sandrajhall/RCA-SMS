using Microsoft.AspNetCore.Components;
using MudBlazor;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Drawing;
using RCA_StudyManagementSystem.Client.Utilities;
using RCA_StudyManagementSystem.Shared.ImportViews;
using System.Threading.Tasks;
using RCA_StudyManagementSystem.Client.Services;


namespace RCA_StudyManagementSystem.Client.Pages.Imports
{
    public partial class AtriumImportViewDialog : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private void Cancel() => MudDialog.Cancel();

        [Parameter]
        public AtriumImportView Model { get; set; } = new AtriumImportView();
        private MudForm? _form;

        private string? StatusMessage { get; set; }


        [Parameter]
        public List<AtriumImportView> CarouselRecords { get; set; } = new List<AtriumImportView>(); // Receives the filtered items
        [Parameter]
        public int InitialSelectedIndex { get; set; }

        [Parameter] public List<GroupedInvoiceItems> GroupedItems { get; set; } = new();
        [Parameter] public List<Study> Studies { get; set; } = new();

        private static string FormatCurrency(decimal value)
                                                            => value.ToString("C2");

        private const decimal PaymentPerPath = 15m;


        private Transition Transition = Transition.Fade; // Example transition


        private int Index = 0;


        private List<AtriumImportView> displayedRecords = new List<AtriumImportView>();
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
            Model = displayedRecords.FirstOrDefault() ?? new AtriumImportView();



            await InvokeAsync(StateHasChanged);
        }

        private void OnPageChanged(int newPage)
        {
            currPage = newPage;
            UpdateDisplayedRecords();
        }


        protected override async Task OnParametersSetAsync()
        {
            Model = CarouselRecords[InitialSelectedIndex];
            OnPageChanged(InitialSelectedIndex + 1);

        }



      
    private static string Display(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value;
    }

    private static string FormatDate(DateTime? value)
    {
        return value?.ToString("MM/dd/yyyy") ?? string.Empty;
    }

    }
}
