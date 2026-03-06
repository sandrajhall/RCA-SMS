using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Net.Http.Json;
using System.Text.Json;


namespace RCA_StudyManagementSystem.Client.Pages.RCAContacts
{
    public partial class Fields : Microsoft.AspNetCore.Components.ComponentBase
    {

        [Parameter]
        public RCAContact RCAContact { get; set; } = new RCAContact();

        [CascadingParameter]
        public EditContext? EditContext { get; set; }


        [Parameter]
        public bool IsSaved { get; set; }

        public CancellationToken CancellationToken { get; set; } = new CancellationToken();


        private CancellationToken cancellationToken { get; set; } = new CancellationToken();





        protected override async Task OnInitializedAsync()
        {

            EditContext!.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events

        }


        private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
        {

            IsSaved = false;
            // Logic to execute when a field changes
            // e.FieldIdentifier provides information about the changed field
            //Console.WriteLine($"Field in child '{e.FieldIdentifier.FieldName}' changed.");

        }

        private async Task HandleInternalNavigation(LocationChangingContext context)
        {
            if (EditContext!.IsModified())
            {
                var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "You have unsaved changes. Do you want to leave?");
                if (!confirmed)
                {
                    context.PreventNavigation();
                }
            }
        }





    }

}
