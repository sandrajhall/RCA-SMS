using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;


namespace RCA_StudyManagementSystem.Client.Pages.DoNotContacts
{
    public partial class Fields : ComponentBase
    {

        [Parameter]
        public DoNotContact DoNotContact { get; set; } = new DoNotContact();

        [CascadingParameter]
        public EditContext? EditContext { get; set; }


        [Parameter]
        public bool IsSaved { get; set; }

        private IEnumerable<Study> StudyList = new List<Study>();
        protected Study studySelectValue;



        protected override async Task OnInitializedAsync()
        {

            EditContext!.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events
            StudyList = await StudyData.ListStudiesAsync();
            if (DoNotContact.StudyName != null)
            {
                studySelectValue = StudyList.FirstOrDefault(s => s.Prefix == DoNotContact.StudyName)!;
            }

            await InvokeAsync(StateHasChanged); // Refresh the UI to reflect the changes


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

        private async Task OnStudySelectChanged(Study value)
        {
            DoNotContact.StudyName = value.Prefix;
            studySelectValue = value;
        }
    }
}
