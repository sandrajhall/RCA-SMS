using ExcelDataReader; // NuGet package: EPPlus
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;
using RCA_StudyManagementSystem.Shared.Domain;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Text;
using System.Xml.Linq;
using static MudBlazor.CategoryTypes;


namespace RCA_StudyManagementSystem.Client.Pages.Lookups
{
    public partial class List : Microsoft.AspNetCore.Components.ComponentBase
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public List<Lookup> Lookups { get; set; }

        private string? _searchString;
        private List<string> _events = new();

        private bool HasChanges = false;


        // quick filter - filter globally across multiple columns with the same input
        private Func<Lookup, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.LookupName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.LookupType.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        private string GetRowClass(Lookup item, int index)
        {
            return item.IsModified ? "mud-datagrid-modified-row" : "";
        }


        protected override async Task OnInitializedAsync()
        {
            Lookups = (List<Lookup>)await LookupData.ListLookupsAsync();

            if (Lookups == null || !Lookups.Any())
            {
                // If no lookups exist, initialize with an empty list
                Lookups = new List<Lookup>();
                var lookup = new Lookup
                {
                    LookupId = Guid.NewGuid(),
                    LookupName = "",
                    LookupType = ""
                };
                Lookups.Add(lookup); // Add an empty lookup to the list
                await InvokeAsync(StateHasChanged);

            }

            return;

        }

        private void OnSortOrderChanged(int newValue, Lookup item)
        {
            item.SortOrder = newValue;
            item.IsModified = true; // Set IsModified to true when any field is changed
            HasChanges = true; // Set HasChanges to true to indicate that there are unsaved changes
        }

        private void OnCodeChanged(string newValue, Lookup item)
        {
            item.LookupCode = newValue;
            item.IsModified = true; // Set IsModified to true when any field is changed
            HasChanges = true; // Set HasChanges to true to indicate that there are unsaved changes
        }

        private void OnLookupNameChanged(string newValue, Lookup item)
        {
            item.LookupName = newValue;
            item.IsModified = true; // Set IsModified to true when any field is changed
            HasChanges = true; // Set HasChanges to true to indicate that there are unsaved changes
        }

        private void OnLookupTypeChanged(string newValue, Lookup item)
        {
            item.LookupType = newValue;
            item.IsModified = true; // Set IsModified to true when any field is changed
            HasChanges = true; // Set HasChanges to true to indicate that there are unsaved changes
        }

        private void OnCommentsChanged(string newValue, Lookup item)
        {
            item.Comments = newValue;
            item.IsModified = true; // Set IsModified to true when any field is changed
            HasChanges = true; // Set HasChanges to true to indicate that there are unsaved changes
        }

        private async void AddNewLookup()
        {
            var lookup = new Lookup
            {
                LookupId = Guid.NewGuid(),
                LookupName = "",
                LookupType = "",
                IsModified = true,
                IsNew = true // Mark as new for inline editing
            };

            Lookups.Insert(0, lookup); // Insert at the top of the list
            await InvokeAsync(StateHasChanged);
            await Task.Delay(100); // Give the UI a moment to update
            HasChanges = true; // Set HasChanges to true to indicate that there are unsaved changes
            //await dataGrid.SetEditingItemAsync(newItem); // Start inline editing - shows modal
        }

        // This method is called when a lookup is modified on a per row basis - not in use currently
        private async Task<DataGridEditFormAction> HandleCommittedChanges(Lookup item)
        {
            var auth = await AuthStateProvider.GetAuthenticationStateAsync();
            var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            await LookupData.UpdateLookupAsync(item.LookupId, userId, item);


            Lookups = (List<Lookup>)await LookupData.ListLookupsAsync(); // Refresh the list
            await InvokeAsync(StateHasChanged);

            return DataGridEditFormAction.KeepOpen;
        }

        private async Task HandleInternalNavigation(LocationChangingContext context)
        {
            if (HasChanges)
            {
                var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "You have unsaved changes. Do you want to leave?");
                if (!confirmed)
                {
                    context.PreventNavigation();
                }
            }
        }

        private async Task OnSubmitAsync()
        {
            var auth = await AuthStateProvider.GetAuthenticationStateAsync();
            var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            foreach (var lookup in Lookups)
            {
                if (lookup.IsNew)
                {
                    lookup.LookupId = Guid.NewGuid(); // Ensure a new ID is generated for new lookups
                    if (string.IsNullOrWhiteSpace(lookup.LookupName) || string.IsNullOrWhiteSpace(lookup.LookupType))
                    {
                        // Skip saving if required fields are empty
                        continue;
                    }
                    await LookupData.CreateLookupAsync(userId, lookup);
                    lookup.IsNew = false; // Reset IsNew after creation
                    lookup.IsModified = false; // Reset IsModified after saving
                }
                else if (lookup.IsModified)
                {
                    await LookupData.UpdateLookupAsync(lookup.LookupId, userId, lookup);
                    lookup.IsModified = false; // Reset IsModified after saving
                }
            }
            Lookups = (List<Lookup>)await LookupData.ListLookupsAsync(); // Refresh the list
            HasChanges = false; // Reset HasChanges after saving
        }


        private async void OnCancel()
        {
            Lookups = (List<Lookup>)await LookupData.ListLookupsAsync(); // Refresh the list from the database
            await InvokeAsync(StateHasChanged);
            HasChanges = false; // Reset HasChanges after canceling

        }

        private async Task OnImport()
        {
            //string excelFilePath = @"C:\RCA\repos\RCA_StudyManagementSystem\RCA_StudyManagementSystem.Client\wwwroot\Lookups.xlsx";
            string fileUrl = "https://localhost:7190/Counties.xlsx";
            string fileContent = string.Empty;

            string excelUrl = fileUrl; // Replace with your URL

            using (HttpClient client = new HttpClient())
            {
                using (Stream stream = await client.GetStreamAsync(excelUrl))
                {
                    // Register encoding provider for older Excel formats if needed
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        // Example: Convert to DataSet
                        DataSet result = reader.AsDataSet();

                        // You can now access the data in 'result.Tables'
                        // For example, to iterate through the first sheet:
                        var LookupList = new List<Lookup>();

                        if (result.Tables.Count > 0)
                        {

                            DataTable firstSheet = result.Tables[0];
                            foreach (DataRow row in firstSheet.Rows)
                            {
                                var newLookup = new Lookup();
                                newLookup.LookupId = Guid.NewGuid();
                                newLookup.LookupName = row[2].ToString() ?? string.Empty;
                                newLookup.LookupType = row[1].ToString() ?? string.Empty;
                                newLookup.ParentCategory = string.Empty;
                                newLookup.Comments = string.Empty;
                                newLookup.IsActive = true;
                                newLookup.SortOrder = row[0] != null && int.TryParse(row[0].ToString(), out int sortOrder) ? sortOrder : 0;
                                LookupList.Add(newLookup);
                            }
                        }
                        // Add the new lookups to the database
                        foreach (var lookup in LookupList)
                        {
                            var userId = await UserData.GetIdByEmailAsync("system_user@system.user");
                            await LookupData.CreateLookupAsync(userId, lookup);
                        }

                        // Refresh the list of lookups
                        Lookups = (List<Lookup>)await LookupData.ListLookupsAsync();
                    }
                }
            }
        }

       
    }
}


