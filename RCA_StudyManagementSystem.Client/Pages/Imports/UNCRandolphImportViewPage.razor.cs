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
    public partial class UNCRandolphImportViewPage : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private void Cancel() => MudDialog.Cancel();

        [Parameter]
        public UNCRandolphImportView Model { get; set; } = new UNCRandolphImportView();
        private MudForm? _form;

        private string? StatusMessage { get; set; }

        [Parameter]
        public string? Token { get; set; }

        private int Index = 0;

        private bool _loading = true;


        private List<UNCRandolphImportView> displayedRecords = new List<UNCRandolphImportView>();



        protected override void OnParametersSet()
        {
            _loading = true;
            Model = null;

            if (!string.IsNullOrWhiteSpace(Token))
            {
                Model = PdfStore.Get(Token);
            }

            _loading = false;
        }

        private static string Display(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "—"
                : value;
        }

        private static string FormatDate(DateTime? value)
        {
            return value.HasValue
                ? value.Value.ToString("MM/dd/yyyy HH:mm")
                : "—";
        }


        private static string Text(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value;
        }

    }
}
