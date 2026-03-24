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


namespace RCA_StudyManagementSystem.Client.Pages.Histologies
{
    public partial class List : Microsoft.AspNetCore.Components.ComponentBase
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public List<Histology> Histologies { get; set; }

        private string? _searchString;
        private List<string> _events = new();

        private bool HasChanges = false;


        // quick filter - filter globally across multiple columns with the same input
        private Func<Histology, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.HistologyCode.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.HistologyName != null && x.HistologyName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.Comments != null && x.Comments.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;



            return false;
        };

        private string GetRowClass(Histology item, int index)
        {
            return item.IsModified ? "mud-datagrid-modified-row" : "";
        }

        private Func<Histology, object> _sortByPreferred => x =>
        {
            if (x.IsPreferred)
                return "true";
            else
                return "false";
        };

        private Func<Histology, object> _sortByActive => x =>
        {
            if (x.IsActive)
                return "true";
            else
                return "false";
        };


        protected override async Task OnInitializedAsync()
        {
            Histologies = (List<Histology>)await HistologyData.ListHistologiesAsync();

            if (Histologies == null || !Histologies.Any())
            {
                // If no lookups exist, initialize with an empty list
                Histologies = new List<Histology>();
                var histology = new Histology
                {
                    HistologyId = Guid.NewGuid(),
                    HistologyCode = "",
                    HistologyBehavior = "",
                    HistologyName = "",
                    IsPreferred = false,
                    Comments = "",
                    IsActive = true,
                    SortOrder = 0,
                    IsModified = true,
                    IsNew = true // Mark as new for inline editing
                };
                Histologies.Add(histology); // Add an empty histology to the list
                await InvokeAsync(StateHasChanged);

            }

            return;

        }

        private void OnSortOrderChanged(int newValue, Histology item)
        {
            item.SortOrder = newValue;
            item.IsModified = true; // Set IsModified to true when any field is changed
            HasChanges = true; // Set HasChanges to true to indicate that there are unsaved changes
        }

        private void OnHistologyCodeChanged(string newValue, Histology item)
        {
            item.HistologyCode = newValue;
            item.IsModified = true; // Set IsModified to true when any field is changed
            HasChanges = true; // Set HasChanges to true to indicate that there are unsaved changes
        }

        private void OnHistologyBehaviorChanged(string newValue, Histology item)
        {
            item.HistologyBehavior = newValue;
            item.IsModified = true; // Set IsModified to true when any field is changed
            HasChanges = true; // Set HasChanges to true to indicate that there are unsaved changes
        }

        private void OnHistologyNameChanged(string newValue, Histology item)
        {
            item.HistologyName = newValue;
            item.IsModified = true; // Set IsModified to true when any field is changed
            HasChanges = true; // Set HasChanges to true to indicate that there are unsaved changes
        }

        private void OnCommentsChanged(string newValue, Histology item)
        {
            item.Comments = newValue;
            item.IsModified = true; // Set IsModified to true when any field is changed
            HasChanges = true; // Set HasChanges to true to indicate that there are unsaved changes
        }

        private void OnIsPreferredChanged(bool newValue, Histology item)
        {
            item.IsPreferred = newValue;
            item.IsModified = true; // Set IsModified to true when any field is changed
            HasChanges = true; // Set HasChanges to true to indicate that there are unsaved changes
        }

        private void OnIsActiveChanged(bool newValue, Histology item)
        {
            item.IsActive = newValue;
            item.IsModified = true; // Set IsModified to true when any field is changed
            HasChanges = true; // Set HasChanges to true to indicate that there are unsaved changes
        }

        private async Task AddNewHistology()
        {
            var histology = new Histology
            {
                HistologyId = Guid.NewGuid(),
                HistologyCode = "",
                HistologyBehavior = "",
                HistologyName = "",
                IsPreferred = false,
                Comments = "",
                IsActive = true,
                SortOrder = 0,
                IsModified = true,
                IsNew = true // Mark as new for inline editing
            };

            Histologies.Insert(0, histology); // Insert at the top of the list
            //Lookups.Add(lookup); // Add an empty lookup to the list
            await InvokeAsync(StateHasChanged);
            await Task.Delay(100); // Give the UI a moment to update
            HasChanges = true; // Set HasChanges to true to indicate that there are unsaved changes
            //await dataGrid.SetEditingItemAsync(newItem); // Start inline editing - shows modal
        }

        // This method is called when a histology is modified on a per row basis - not in use currently
        private async Task<DataGridEditFormAction> HandleCommittedChanges(Histology item)
        {
            var auth = await AuthStateProvider.GetAuthenticationStateAsync();
            var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            await HistologyData.UpdateHistologyAsync(item.HistologyId, userId, item);


            Histologies = (List<Histology>)await HistologyData.ListHistologiesAsync(); // Refresh the list
            await InvokeAsync(StateHasChanged);

            return DataGridEditFormAction.KeepOpen; // Keep the edit form open after saving changes
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

            foreach (var histology in Histologies)
            {
                if (histology.IsNew)
                {
                    histology.HistologyId = Guid.NewGuid(); // Ensure a new ID is generated for new histologies
                    if (string.IsNullOrWhiteSpace(histology.HistologyCode) || string.IsNullOrWhiteSpace(histology.HistologyName))
                    {
                        // Skip saving if required fields are empty
                        continue;
                    }
                    await HistologyData.CreateHistologyAsync(userId, histology);
                    histology.IsNew = false; // Reset IsNew after creation
                    histology.IsModified = false; // Reset IsModified after saving
                }
                else if (histology.IsModified)
                {
                    await HistologyData.UpdateHistologyAsync(histology.HistologyId, userId, histology);
                    histology.IsModified = false; // Reset IsModified after saving
                }
            }
            Histologies = (List<Histology>)await HistologyData.ListHistologiesAsync(); // Refresh the list
            HasChanges = false; // Reset HasChanges after saving
        }


        private async void OnCancel()
        {
            Histologies = (List<Histology>)await HistologyData.ListHistologiesAsync(); // Refresh the list from the database
            await InvokeAsync(StateHasChanged);
            HasChanges = false; // Reset HasChanges after canceling

        }

        private async Task OnImportHistologies()
        {
            //string excelFilePath = @"C:\RCA\repos\RCA_StudyManagementSystem\RCA_StudyManagementSystem.Client\wwwroot\Lookups.xlsx";
            string fileUrl = "https://localhost:7190/Histologies.xlsx";
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
                        var HistologyList = new List<Histology>();

                        if (result.Tables.Count > 0)
                        {

                            DataTable firstSheet = result.Tables[0];
                            foreach (DataRow row in firstSheet.Rows)
                            {
                                var newHistology = new Histology();
                                newHistology.HistologyId = Guid.NewGuid();
                                newHistology.HistologyCode = row[0].ToString() ?? string.Empty;
                                newHistology.HistologyBehavior = row[1].ToString() ?? string.Empty;

                                if (string.Equals(row[2]?.ToString(), "TRUE", StringComparison.OrdinalIgnoreCase))
                                {
                                    newHistology.IsPreferred = true;
                                }
                                else
                                {
                                    newHistology.IsPreferred = false;
                                }
                                newHistology.HistologyName = row[3].ToString() ?? string.Empty; ;
                                newHistology.IsActive = true;
                                HistologyList.Add(newHistology);
                            }
                        }
                        // Add the new histologies to the database
                        foreach (var histology in HistologyList)
                        {
                            var userid = await UserData.GetIdByEmailAsync("system_user@system.user");
                            await HistologyData.CreateHistologyAsync(userid, histology);
                        }

                        // Refresh the list of lookups
                        Histologies = (List<Histology>)await HistologyData.ListHistologiesAsync();
                    }
                }
            }
        }
    }
}


