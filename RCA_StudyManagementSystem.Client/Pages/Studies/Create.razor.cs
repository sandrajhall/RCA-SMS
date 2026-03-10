using RCA_StudyManagementSystem.Shared.Domain;
using Microsoft.AspNetCore.Components;
using static MudBlazor.CategoryTypes;
using MudBlazor;
using System.Xml;
using static MudBlazor.Icons;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor.Interfaces;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Client.Services;

namespace RCA_StudyManagementSystem.Client.Pages.Studies
{
    public partial class Create : Microsoft.AspNetCore.Components.ComponentBase
    {

        [Parameter]
        public Study Study { get; set; }

        EditContext EditContext;


        private Severity severity = Severity.Success;

        protected string SaveMessage = string.Empty;
        private string? apiResponse = string.Empty;
        protected bool IsSaved = false;
        protected bool HasErrors = false; // Flag to indicate if there are validation errors


        protected override void OnInitialized()
        {
            Study ??= new();
            EditContext = new EditContext(Study);
            Study.ColorLight = "#fcfcfdff"; // Set the default color for the study
        }

        private async Task OnCancel()
        {
            //EditContext?.MarkAsUnmodified(); // Mark the edit context as unmodified to prevent further validation errors
            SaveMessage = "Study not added.";
            NavigationManager.NavigateTo($"/studies/list");
        }

        protected override void OnParametersSet()
        {
            if (Study == null)
            {
                Study = new Study();
            }

            if (EditContext == null || EditContext.Model != Study)
            {
                EditContext = new EditContext(Study);
            }
        }

        private async Task<Guid> Save()
        {
            var id = Guid.Empty;
            
            // Manually trigger validation
            bool isValid = EditContext.Validate();

            if (isValid)
            {
                HasErrors = false;

                try
                {
                    id = await StudyData.CreateStudyAsync(Study);

                    //Logger.LogInformation("Study created. {Study}", System.Text.Json.JsonSerializer.Serialize(Study));

                    IsSaved = true; // Set IsSaved to true to indicate the form was submitted successfully

                    EditContext.MarkAsUnmodified(); // Mark the edit context as unmodified to prevent further validation errors
                }
                catch (HttpRequestException ex)
                {
                    apiResponse = $"Error: {ex.Message}"; // Handle potential errors during the API call
                    SaveMessage = apiResponse; // Set the save message to the error message
                    severity = Severity.Error; // Set severity to Error if the form is invalid
                    IsSaved = false; // Set IsSaved to false if there was an error
                    HasErrors = true; // Set HasErrors to true to indicate there are errors
                }
            }
            else
            {
                IsSaved = false;
                HasErrors = true; // Set HasErrors to true to indicate there are validation errors
                severity = Severity.Error; // Set severity to Error if the form is invalid
                SaveMessage = "Study not added.  Please correct the errors."; // Set a failure message
            }
            return id;
        }

        private async Task OnSubmit()
        {
            var id = await Save();

            if (IsSaved)
            {
                NavigationManager.NavigateTo($"/studies/edit/{id}/{IsSaved}");
            }
        }

        private async Task OnSaveAndAddNew()
        {
            var id = await Save();

            if (IsSaved)
            {
                NavigationManager.NavigateTo($"/studies/create", forceLoad: true);
            }
        }

        private async Task OnSaveAndClose()
        {
            var id = await Save();

            if (IsSaved)
            {
                NavigationManager.NavigateTo($"/studies/list");
            }
        }
    }
}