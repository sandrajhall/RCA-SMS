using Microsoft.AspNetCore.Components;
using MudBlazor;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Drawing;
using RCA_StudyManagementSystem.Client.Utilities;
using System.Threading.Tasks;
using RCA_StudyManagementSystem.Client.Services;


namespace RCA_StudyManagementSystem.Client.Pages.Invoices
{
    public partial class InvoiceMissingReimbEntitiesDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        public MudDataGrid<Hospital> MissingGrid { get; set; } = new MudDataGrid<Hospital>();

        [Parameter]
        public IEnumerable<Hospital>? MissingHospitals { get; set; } = new List<Hospital>();

        protected override void OnInitialized()
        {

        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}
