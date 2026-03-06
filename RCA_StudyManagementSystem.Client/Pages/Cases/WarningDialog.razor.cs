using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace RCA_StudyManagementSystem.Client.Pages.Cases
{
    public partial class WarningDialog : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private void Submit() => MudDialog.Close(DialogResult.Ok(true));

        private void Cancel() => MudDialog.Cancel();
    }
}
