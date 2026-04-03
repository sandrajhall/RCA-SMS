using ExcelDataReader;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.DTOs;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml.Linq;
using static MudBlazor.CategoryTypes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RCA_StudyManagementSystem.Client.Pages.DoNotContacts
{
    public partial class List : Microsoft.AspNetCore.Components.ComponentBase
    {
        private readonly DialogOptions _options = new() { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        private readonly DialogOptions _maxWidth = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

        MudDataGrid<DoNotContact>? dncGrid { get; set; }


        public IEnumerable<DoNotContact>? DoNotContacts { get; set; }

        private IEnumerable<DoNotContact>? _displayItems { get; set; }

        private int Index = 0;

        private CancellationToken CancellationToken { get; set; } = new CancellationToken();



        private const string GridStateStorageKey = "DncDataGridState"; // Key for local storage


        private string? _searchString;
        private List<string> _events = new();
        private CancellationToken token;




        // quick filter - filter globally across multiple columns with the same input
        private Func<DoNotContact, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.LastName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            
            if (x.FirstName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.MiddleName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.DateOfBirth.Value.ToShortDateString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        };





        protected override async Task OnInitializedAsync()
        {

            Console.WriteLine("OnInitializedAsync method called!");

            DoNotContacts = await DoNotContactData.ListDoNotContactsAsync(token);

  

            _displayItems = DoNotContacts;




            dncGrid.FilterDefinitions = DncGridStateView.FilterDefinitions;
            dncGrid.SortDefinitions = new Dictionary<string, SortDefinition<DoNotContact>>(DncGridStateView.SortDefinitions);

            dncGrid.CurrentPage = DncGridStateView.CurrentPage;



            return;

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {


            if (firstRender && dncGrid != null)
            {
                DoNotContacts = await DoNotContactData.ListDoNotContactsAsync(token);

                var storedStateDto = await LocalStorage.GetItemAsync<GridStateDto>(GridStateStorageKey);

                if (storedStateDto != null)
                {
                    _searchString = storedStateDto.SearchString ?? ""; // Restore the search string

                    // Map DTOs back to MudBlazor types and update the ViewModel
                    var newFilterDefs = storedStateDto.Filters
                        .Select(dto =>
                        {
                            // Find the column by property name
                            var column = dncGrid.RenderedColumns.FirstOrDefault(c => c.PropertyName == dto.Field);
                            if (column == null)
                                return null;

                            // Use the column to create a new filter definition instance
                            var filterDef = column.FilterContext?.FilterDefinition;
                            if (filterDef == null)
                                return null;

                            filterDef.Column = column;
                            filterDef.Operator = dto.Operator;
                            filterDef.Value = dto.Value;
                            if (dto.BoolValue.HasValue)
                            {
                                filterDef.Value = dto.BoolValue.Value;
                            }
                            return filterDef;
                        })
                        .Where(fd => fd != null)
                        .ToList();


                    var newSortDefs = storedStateDto.Sorts
                        .ToDictionary(dto => dto.SortBy, dto => new SortDefinition<DoNotContact>(
                            dto.SortBy,
                            dto.Descending,
                            0,
                            null
                        ));

                    var newSortColumn = DncGridStateView.SortColumn;

                    DncGridStateView.FilterDefinitions = newFilterDefs!;
                    DncGridStateView.SortDefinitions = newSortDefs;

                    dncGrid.FilterDefinitions = DncGridStateView.FilterDefinitions;
                    dncGrid.SortDefinitions = DncGridStateView.SortDefinitions;

                    // Set the sort definitions
                    if (DncGridStateView.SortDefinitions.Any())
                    {
                        // Apply the first sort definition if available
                        var firstSort = DncGridStateView.SortDefinitions.First();
                        SortDirection direction = firstSort.Value.Descending ? SortDirection.Descending : SortDirection.Ascending;
                        await dncGrid.SetSortAsync(firstSort.Key, direction, firstSort.Value.SortFunc);

                        var sortDefinitions = dncGrid.SortDefinitions; // Get current sort settings

                        string sortByProperty = "";
                        var sortedData = DoNotContacts.AsQueryable(); // Start with unsorted data



                        if (sortDefinitions != null && sortDefinitions.Any())
                        {
                            sortedData = DoNotContacts.AsQueryable(); // Start with unsorted data

                            foreach (var sortDefinition in sortDefinitions)
                            {
                                var propertyName = sortDefinition.Value.SortBy; // Extract property name

                                // Dynamically apply sorting
                                if (sortDefinition.Value.Descending == false)
                                {
                                    sortedData = sortedData.OrderBy(x => GetPropertyValue(x, propertyName));
                                }
                                else
                                {
                                    sortedData = sortedData.OrderByDescending(x => GetPropertyValue(x, propertyName));
                                }
                            }

                            _displayItems = sortedData.ToList();

                        }
                        else
                        {
                            _displayItems = DoNotContacts; // If no sorting, revert to original order
                        }
                    }
                    // Load CurrentPage and PageSize
                    // Check that the dataGrid reference is not null

                    DncGridStateView.CurrentPage = storedStateDto.CurrentPage;
                    //CaseGridStateView.PageSize = storedStateDto.PageSize;

                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        private async Task ClearStateItems()
        {
            await LocalStorage.ClearAsync();
            await InvokeAsync(StateHasChanged);
            NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
        }
        private async Task OnCurrentPageChanged(int page) // Changed to Task
        {
            if (DncGridStateView == null) return;

            DncGridStateView.CurrentPage = page;

            try
            {
                var storedStateDto = await LocalStorage.GetItemAsync<GridStateDto>(GridStateStorageKey);

                if (storedStateDto == null)
                {
                    storedStateDto = new GridStateDto();
                }

                storedStateDto.CurrentPage = page;

                await LocalStorage.SetItemAsync(GridStateStorageKey, storedStateDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Storage error: {ex.Message}");
            }

            StateHasChanged();
        }




        // Helper to dynamically get property value for sorting
        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null) ?? string.Empty;
        }



        async Task<IDialogReference> ViewItem(DoNotContact args, DoNotContact pArgs)
        {

            var newDoNotContacts = new List<DoNotContact>();

            foreach (var doc in dncGrid.FilteredItems)
            {
                var newDoNotContact = await DoNotContactData.GetDoNotContactAsync(doc.DoNotContactId);
                newDoNotContacts.Add(newDoNotContact);
            }


            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<ViewDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, newDoNotContacts); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, dncGrid.FilteredItems.ToList().IndexOf(pArgs)); // Set initial position

            var options = _options;

            return await DialogService.ShowAsync<ViewDialog>("Do Not Contact View", parameters, options);

        }


        private string FormatPhoneNumber(string number)
        {
            if (string.IsNullOrEmpty(number) || number.Length != 10)
            {
                return number; // Return as is if not a valid 10-digit number
            }
            return $"({number.Substring(0, 3)}) {number.Substring(3, 3)}-{number.Substring(6, 4)}";
        }

        public async void Dispose()
        {
            Console.WriteLine("Dispose method called for DataGrid component.");

            if (dncGrid != null)
            {
                // Map MudBlazor types to DTOs for serialization
                var filtersToSave = dncGrid.FilterDefinitions
                    .Select(f => new FilterDefinitionDto
                    {
                        // Get the Field name from the Column property of the FilterDefinition
                        Field = f.Column?.PropertyName ?? string.Empty,
                        Operator = f.Operator!,
                        Value = f.Value!.ToString(), // Convert Value to string for serialization
                        BoolValue = f.Value is bool boolValue ? boolValue : null // Handle boolean values
                    })
                    .ToList();

                var sortsToSave = dncGrid.SortDefinitions.Values // Iterate over values of the Dictionary
                    .Select(s => new SortDefinitionDto
                    {
                        SortBy = s.SortBy,
                        Descending = s.Descending ? true : false,
                        //Index = s.Index, // Save the index of the sort
                        //SortFunc = s.SortFunc // Save the sort function
                    })
                    .ToList(); // Convert to List for serialization

                if (sortsToSave.Count > 0)
                {
                    var sortColum = dncGrid.RenderedColumns.FirstOrDefault(c => c.PropertyName == sortsToSave.FirstOrDefault()!.SortBy)?.Title ?? string.Empty;
                    DncGridStateView.SortColumn = sortColum;
                }

                // Create a DTO to hold the state
                var stateDto = new GridStateDto
                {
                    Filters = filtersToSave,
                    Sorts = sortsToSave,
                    CurrentPage = dncGrid.CurrentPage, // Save current page
                    SearchString = _searchString,
                };

                // Save the DTO to local storage
                await LocalStorage.SetItemAsync(GridStateStorageKey, stateDto);

                DncGridStateView.SortDefinitions = stateDto.Sorts
                    .ToDictionary(s => s.SortBy, s => new SortDefinition<DoNotContact>(
                        s.SortBy,
                        s.Descending,
                        s.Index,
                        (Func<DoNotContact, object>)s.SortFunc
                    ));



                Console.WriteLine($"State saved to LocalStorage. Filters Count: {filtersToSave.Count}, Sorts Count: {sortsToSave.Count}, Page: {dncGrid.CurrentPage}");
            }
            else
            {
                Console.WriteLine("mudDataGrid was null during Dispose.");
            }
        }


        private async Task OnImport()
        {
            string fileUrl = "https://localhost:7150/donotcontact.xlsx";
            using (var client = new HttpClient())
            {
                using (var stream = await client.GetStreamAsync(fileUrl))
                {
                    // Copy to a seekable stream
                    using (var ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);
                        ms.Position = 0;

                        // Register encoding provider for Excel files, if not already done
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                        using (var reader = ExcelReaderFactory.CreateReader(ms))
                        {
                            // Example: Convert to DataSet
                            DataSet result = reader.AsDataSet();

                            // You can now access the data in 'result.Tables'
                            var DNCList = new List<DoNotContact>();

                            if (result.Tables.Count > 0)
                            {

                                DataTable firstSheet = result.Tables[0];
                                foreach (DataRow row in firstSheet.Rows)
                                {
                                    Console.WriteLine(string.Join(", ", row.ItemArray));
                                    if (row[0]?.ToString() == "ID") // Skip header row
                                        continue;
                                    var newDNC = new DoNotContact();
                                    newDNC.FirstName = row[2]?.ToString().Trim() ?? string.Empty;
                                    newDNC.MiddleName = row[3]?.ToString().Trim() ?? string.Empty;
                                    newDNC.LastName = row[1]?.ToString().Trim() ?? string.Empty;
                                    newDNC.DateOfBirth = DateTime.TryParse(row[6]?.ToString().Trim(), out DateTime dob) ? dob : (DateTime?)null;
                                    newDNC.SocialSecurityNumber = row[5]?.ToString().Trim() ?? string.Empty;
                                    newDNC.StudyName = row[8]?.ToString().Trim() ?? string.Empty;
                                    newDNC.DateLastContact = DateTime.TryParse(row[12]?.ToString().Trim(), out DateTime dlc) ? dlc : (DateTime?)null;

                                    newDNC.PhoneNumber = row[4]?.ToString().Trim() ?? string.Empty;


                                    newDNC.DisplayName = $"{newDNC.FirstName} {newDNC.LastName}";

                                    newDNC.CreatedDate = DateTime.UtcNow;
                                    newDNC.CreatedUserId = Guid.Empty;



                                    DNCList.Add(newDNC);

                                }
                            }
                            // Add the new records to the database
                            foreach (var dnc in DNCList)
                            {
                                var userId = await UserData.GetIdByEmailAsync("system_user@system.user"); // System User Id
                                dnc.CreatedUserId = Guid.Parse(userId);
                                var id = await DoNotContactData.CreateDoNotContactAsync(userId, dnc);
                            }

                        }
                    }
                }
            }
        }
    }
}
