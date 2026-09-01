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
    public partial class UNCRexImportViewDialog : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private void Cancel() => MudDialog.Cancel();

        [Parameter]
        public UNCRexImportView Model { get; set; } = new UNCRexImportView();
        private MudForm? _form;

        private string? StatusMessage { get; set; }


        [Parameter]
        public List<UNCRexImportView> CarouselRecords { get; set; } = new List<UNCRexImportView>(); // Receives the filtered items
        [Parameter]
        public int InitialSelectedIndex { get; set; }

        [Parameter] public List<GroupedInvoiceItems> GroupedItems { get; set; } = new();
        [Parameter] public List<Study> Studies { get; set; } = new();

        private static string FormatCurrency(decimal value)
                                                            => value.ToString("C2");

        private const decimal PaymentPerPath = 15m;


        private Transition Transition = Transition.Fade; // Example transition


        private int Index = 0;


        private List<UNCRexImportView> displayedRecords = new List<UNCRexImportView>();
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
            Model = displayedRecords.FirstOrDefault() ?? new UNCRexImportView();



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



        private async Task SubmitAsync()
        {
            if (_form is null)
                return;

            await _form.Validate();

            if (!_form.IsValid)
            {
                StatusMessage = "Please correct the validation errors.";
                return;
            }

            // Replace this with your database or API call.
            StatusMessage = "The UNC Rex import was submitted successfully.";
        }

        private void ClearForm()
        {
            Model = new();
            StatusMessage = null;
        }

    }
}
