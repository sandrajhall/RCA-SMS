using RCA_StudyManagementSystem.Shared.Domain;
using Microsoft.AspNetCore.Components;
using static MudBlazor.CategoryTypes;
using MudBlazor;
using System.Xml;
using static MudBlazor.Icons;
using Microsoft.AspNetCore.Components.Forms;
using static System.Net.WebRequestMethods;
using System.Net.Http.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using RCA_StudyManagementSystem.Client.Services;

namespace RCA_StudyManagementSystem.Client.Pages.ReimbursementEntities
{
    public partial class Edit : Microsoft.AspNetCore.Components.ComponentBase
    {
        //[SupplyParameterFromForm]
        [Parameter]
        public ReimbursementEntity ReimbursementEntity { get; set; } = new ReimbursementEntity();

        [CascadingParameter]
        private EditContext? EditContext { get; set; }

        [CascadingParameter]
        IMudDialogInstance MudDialog { get; set; }


        [Parameter]
        public bool IsDialog { get; set; } = false;

        private bool _isSaved = false; // Backing field for IsSaved property

        [Parameter]
        public bool IsSaved
        {
            get => _isSaved; // Expose IsSaved for the form
            set
            {
                if (value == null)
                {
                    _isSaved = false; // Default to false if value is null
                }
                else
                {
                    // Ensure that the value is a boolean
                    if (value.GetType() != typeof(bool))
                    {
                        throw new ArgumentException("IsSaved must be a boolean value.");
                    }
                }
                _isSaved = value; // Set IsSaved when the parameter is set
            }
        }


        [Parameter]
        public Guid ReimbursementEntityId
        {
            get => ReimbursementEntity.ReimbursementEntityId; // Expose ReimbursementEntityId for the form
            set
            {
                ReimbursementEntity.ReimbursementEntityId = value; // Set ReimbursementEntityId when the parameter is set
            }
        }



        private Severity severity = Severity.Success;

        protected string SaveMessage = string.Empty;
        private string? apiResponse = string.Empty;

        protected bool HasErrors = false; // Flag to indicate if there are validation errors

        private readonly DialogOptions _noHeader = new() { NoHeader = true };

        protected override async Task OnInitializedAsync()
        {
            // pause for 1 second to simulate loading
            EditContext = new EditContext(new ReimbursementEntity()); // Initialize editContext with the ReimbursementEntity
            EditContext.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events
            try
            {
                var response = await ReimbursementEntityData.GetReimbursementEntityAsync(ReimbursementEntityId);
                ReimbursementEntity = response ?? new ReimbursementEntity(); // Initialize ReimbursementEntity with the response or an empty ReimbursementEntity if null

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching ReimbursementEntity: {ex.Message}");
                ReimbursementEntity = new ReimbursementEntity(); // Initialize with an empty ReimbursementEntity on error
            }

            EditContext = new EditContext(ReimbursementEntity);
            EditContext.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events

            //if (ReimbursementEntity.ReimbursementEntityRCAContacts.Count < 1)
            //{
            //    ReimbursementEntityRCAContact emptyContact = new ReimbursementEntityRCAContact();
            //    emptyContact.IsPrimaryContact = true; // Set the first contact as primary
            //    emptyContact.ReimbursementEntityId = ReimbursementEntity.ReimbursementEntityId;
            //    emptyContact.RCAContactId = Guid.Empty;
            //    ReimbursementEntity.ReimbursementEntityRCAContacts.Add(emptyContact);
            //}
 
        }



        private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
        {
            // Logic to execute when a field changes
            // e.FieldIdentifier provides information about the changed field
            //Console.WriteLine($"Field '{e.FieldIdentifier.FieldName}' changed.");

            IsSaved = false;
        }

        protected override void OnParametersSet()
        {
            if (ReimbursementEntity == null)
            {
                ReimbursementEntity = new ReimbursementEntity();
            }

            if (EditContext == null || EditContext.Model != ReimbursementEntity)
            {
                EditContext = new EditContext(ReimbursementEntity);
            }
        }

        private async Task OnCancel()
        {
            //EditContext?.MarkAsUnmodified(); // Mark the edit context as unmodified to prevent further validation errors
            SaveMessage = "Reimbursement Entity not updated.";

            if (!IsDialog)
            {
                NavigationManager.NavigateTo($"/reimbursemententities/list");
            }
            else
            {
                MudDialog.Cancel();
            }
        }

        private async Task OnDeactivate()
        {
            bool? result = await DialogService.ShowMessageBoxAsync(
              "Warning", // Dialog title
              $"Are you sure you want to deactivate this Reimbursement Entity?", // Message
              yesText: "Deactivate!", // Text for the confirmation button
              cancelText: "Cancel" // Text for the cancel button
          );
            if (result == true) // User clicked 'Deactivate!'
            {
                await ReimbursementEntityData.DeleteReimbursementEntityAsync(ReimbursementEntity.ReimbursementEntityId);

                await InvokeAsync(StateHasChanged);
                IsSaved = true;
                SaveMessage = "Reimbursement Entity deactivated.";
                severity = Severity.Warning;
                await InvokeAsync(StateHasChanged);

                NavigationManager.NavigateTo($"/reimbursemententities/list");
            }
        }


        private async Task Save()
        {
            // Manually trigger validation
            bool isValid = EditContext.Validate();

            if (isValid)
            {

                IsSaved = true; // Set IsSaved to true to indicate the form was submitted successfully
                HasErrors = false;

                severity = Severity.Success; // Set severity to Success if the form is valid
                SaveMessage = "Reimbursement Entity updated successfully!"; // Set a success message
                await InvokeAsync(StateHasChanged);

                try
                {
                    // Make a GET request to the SampleController
                    await ReimbursementEntityData.UpdateReimbursementEntityAsync(ReimbursementEntity.ReimbursementEntityId, ReimbursementEntity);

                    //Logger.LogInformation("ReimbursementEntity updated. {ReimbursementEntity}", System.Text.Json.JsonSerializer.Serialize(ReimbursementEntity));


                    EditContext.MarkAsUnmodified(); // Mark the edit context as unmodified to prevent further validation errors

                }
                catch (HttpRequestException ex)
                {
                    apiResponse = $"Error: {ex.Message}"; // Handle potential errors during the API call
                    SaveMessage = apiResponse; // Set the save message to the error message
                }

            }
            else
            {
                IsSaved = false;
                HasErrors = true; // Set HasErrors to true to indicate there are validation errors
                severity = Severity.Error; // Set severity to Error if the form is invalid
                SaveMessage = "Reimbursement Entity not updated.  Please correct the errors."; // Set a failure message
            }
        }


        private async Task OnSubmit()
        {
            await Save();

        }

        private async Task OnSaveAndClose()
        {
            await Save();

            if (!IsDialog && IsSaved)
            {
                NavigationManager.NavigateTo($"/reimbursemententities/list");
            }
            else
            {
                if (IsSaved)
                {
                    // Set the dialog's result with the saved data
                    MudDialog.Close(DialogResult.Ok(ReimbursementEntity));
                }
            }
        }
    }
}