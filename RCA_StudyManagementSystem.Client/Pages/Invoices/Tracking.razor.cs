using ExcelDataReader;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.DTOs;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static MudBlazor.CategoryTypes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RCA_StudyManagementSystem.Client.Pages.Invoices
{
    public partial class Tracking : Microsoft.AspNetCore.Components.ComponentBase
    {
        private readonly DialogOptions _options = new() { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        private readonly DialogOptions _maxWidth = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

        MudDataGrid<Invoice>? invGrid { get; set; }

        public CancellationToken CancellationToken { get; set; } = new CancellationToken();

        public IEnumerable<Invoice>? Invoices { get; set; }

        private IEnumerable<Invoice>? _displayItems { get; set; }

        private int Index = 0;



        private const string GridStateStorageKey = "InvTrackDataGridState"; // Key for local storage


        private string? _searchString;
        private List<string> _events = new();
        private CancellationToken token;

        public bool IsNewQuarter = false;
        public int LastQuarter = 0;
        public int CurrentQuarter = 0;


        // quick filter - filter globally across multiple columns with the same input
        private Func<Invoice, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.InvoiceDate.ToShortDateString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.InvoiceNumber!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.ReimbursementEntity!.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        };


        protected override async Task OnInitializedAsync()
        {

            Console.WriteLine("OnInitializedAsync method called!");

            Invoices = await InvoiceData.ListInvoicesSentAsync(CancellationToken);

            _displayItems = Invoices;




            invGrid.FilterDefinitions = InvGridStateView.FilterDefinitions;
            invGrid.SortDefinitions = new Dictionary<string, SortDefinition<Invoice>>(InvGridStateView.SortDefinitions);

            invGrid.CurrentPage = InvGridStateView.CurrentPage;

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {


            if (firstRender && invGrid != null)
            {
                Invoices = await InvoiceData.ListInvoicesSentAsync(CancellationToken);

                var storedStateDto = await LocalStorage.GetItemAsync<GridStateDto>(GridStateStorageKey);

                if (storedStateDto != null)
                {
                    _searchString = storedStateDto.SearchString ?? ""; // Restore the search string

                    // Map DTOs back to MudBlazor types and update the ViewModel
                    var newFilterDefs = storedStateDto.Filters
                        .Select(dto =>
                        {
                            // Find the column by property name
                            var column = invGrid.RenderedColumns.FirstOrDefault(c => c.PropertyName == dto.Field);
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
                        .ToDictionary(dto => dto.SortBy, dto => new SortDefinition<Invoice>(
                            dto.SortBy,
                            dto.Descending,
                            0,
                            null
                        ));

                    var newSortColumn = InvGridStateView.SortColumn;

                    InvGridStateView.FilterDefinitions = newFilterDefs!;
                    InvGridStateView.SortDefinitions = newSortDefs;

                    invGrid.FilterDefinitions = InvGridStateView.FilterDefinitions;
                    invGrid.SortDefinitions = InvGridStateView.SortDefinitions;

                    // Set the sort definitions
                    if (InvGridStateView.SortDefinitions.Any())
                    {
                        // Apply the first sort definition if available
                        var firstSort = InvGridStateView.SortDefinitions.First();
                        SortDirection direction = firstSort.Value.Descending ? SortDirection.Descending : SortDirection.Ascending;
                        await invGrid.SetSortAsync(firstSort.Key, direction, firstSort.Value.SortFunc);

                        var sortDefinitions = invGrid.SortDefinitions; // Get current sort settings

                        string sortByProperty = "";
                        var sortedData = Invoices.AsQueryable(); // Start with unsorted data



                        if (sortDefinitions != null && sortDefinitions.Any())
                        {
                            sortedData = Invoices.AsQueryable(); // Start with unsorted data

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
                            _displayItems = Invoices; // If no sorting, revert to original order
                        }
                    }
                    // Load CurrentPage and PageSize
                    // Check that the dataGrid reference is not null

                    InvGridStateView.CurrentPage = storedStateDto.CurrentPage;
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

        private async void OnCurrentPageChanged(int page)
        {
            InvGridStateView.CurrentPage = page;

            var storedStateDto = await LocalStorage.GetItemAsync<GridStateDto>(GridStateStorageKey);
            if (storedStateDto == null)
            {
                storedStateDto = new GridStateDto();
            }

            storedStateDto.CurrentPage = page;
            StateHasChanged(); // Update the UI to show the new page number
        }



        // Helper to dynamically get property value for sorting
        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null) ?? string.Empty;
        }

        private async Task OnSubmit()
        {
            foreach (var item in _displayItems)
            {
                await InvoiceData.UpdateInvoiceAsync(item.InvoiceId, item);
            }
            NavigationManager.NavigateTo("/invoices/tracking", forceLoad: true);
        }

        private void OnCancel()
        {
            NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
        }



        async Task<IDialogReference> ViewItem(Invoice args, Invoice pArgs)
        {

            var newInvoices = new List<Invoice>();

            foreach (var inv in invGrid.FilteredItems)
            {
                var newInvoice = await InvoiceData.GetInvoiceAsync(inv.InvoiceId);
                newInvoices.Add(newInvoice);
            }


            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<InvoiceTemplateDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, newInvoices); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, invGrid.FilteredItems.ToList().IndexOf(pArgs)); // Set initial position

            var options = _options;

            return await DialogService.ShowAsync<InvoiceTemplateDialog>("Invoice View", parameters, options);

        }


        public async void Dispose()
        {
            Console.WriteLine("Dispose method called for DataGrid component.");

            if (invGrid != null)
            {
                // Map MudBlazor types to DTOs for serialization
                var filtersToSave = invGrid.FilterDefinitions
                    .Select(f => new FilterDefinitionDto
                    {
                        // Get the Field name from the Column property of the FilterDefinition
                        Field = f.Column?.PropertyName ?? string.Empty,
                        Operator = f.Operator!,
                        Value = f.Value!.ToString(), // Convert Value to string for serialization
                        BoolValue = f.Value is bool boolValue ? boolValue : null // Handle boolean values
                    })
                    .ToList();

                var sortsToSave = invGrid.SortDefinitions.Values // Iterate over values of the Dictionary
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
                    var sortColum = invGrid.RenderedColumns.FirstOrDefault(c => c.PropertyName == sortsToSave.FirstOrDefault()!.SortBy)?.Title ?? string.Empty;
                    InvGridStateView.SortColumn = sortColum;
                }

                // Create a DTO to hold the state
                var stateDto = new GridStateDto
                {
                    Filters = filtersToSave,
                    Sorts = sortsToSave,
                    CurrentPage = invGrid.CurrentPage, // Save current page
                    SearchString = _searchString,
                };

                // Save the DTO to local storage
                await LocalStorage.SetItemAsync(GridStateStorageKey, stateDto);

                InvGridStateView.SortDefinitions = stateDto.Sorts
                    .ToDictionary(s => s.SortBy, s => new SortDefinition<Invoice>(
                        s.SortBy,
                        s.Descending,
                        s.Index,
                        (Func<Invoice, object>)s.SortFunc
                    ));



                Console.WriteLine($"State saved to LocalStorage. Filters Count: {filtersToSave.Count}, Sorts Count: {sortsToSave.Count}, Page: {invGrid.CurrentPage}");
            }
            else
            {
                Console.WriteLine("mudDataGrid was null during Dispose.");
            }
        }

        //private async Task OnImport()
        //{
        //    string fileUrl = "https://localhost:7190/Hospitals.xlsx";
        //    string fileContent = string.Empty;

        //    string excelUrl = fileUrl; // Replace with your URL

        //    using (HttpClient client = new HttpClient())
        //    {
        //        using (Stream stream = await client.GetStreamAsync(excelUrl))
        //        {
        //            // Register encoding provider for older Excel formats if needed
        //            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        //            using (var reader = ExcelReaderFactory.CreateReader(stream))
        //            {
        //                // Example: Convert to DataSet
        //                DataSet result = reader.AsDataSet();

        //                // You can now access the data in 'result.Tables'
        //                // For example, to iterate through the first sheet:
        //                var HospitalList = new List<Hospital>();

        //                if (result.Tables.Count > 0)
        //                {

        //                    DataTable firstSheet = result.Tables[0];
        //                    foreach (DataRow row in firstSheet.Rows)
        //                    {
        //                        Console.WriteLine(string.Join(", ", row.ItemArray));
        //                        if (row[0]?.ToString() == "ShortHospName") // Skip header row
        //                            continue;
        //                        var newHospital = new Hospital();
        //                        newHospital.HospitalShortName = row[0]?.ToString().Trim() ?? string.Empty;
        //                        newHospital.HospitalCode = row[1]?.ToString().Trim() ?? string.Empty;
        //                        newHospital.HospitalName = row[2]?.ToString().Trim() ?? string.Empty;
        //                        newHospital.Address1 = row[3]?.ToString().Trim() ?? string.Empty;
        //                        newHospital.Address2 = row[4]?.ToString().Trim() ?? string.Empty;
        //                        newHospital.City = row[7]?.ToString().Trim() ?? string.Empty;
        //                        newHospital.State = row[8]?.ToString().Trim() ?? string.Empty;
        //                        newHospital.ZipCode = row[9]?.ToString().Trim() ?? string.Empty;
        //                        newHospital.County = row[17]?.ToString().Trim() ?? string.Empty;
        //                        newHospital.PhoneNumber = row[5]?.ToString().Trim() ?? string.Empty;
        //                        newHospital.FaxNumber = row[6]?.ToString().Trim() ?? string.Empty;
        //                        newHospital.ModifiedDate = DateTime.TryParse(row[13]?.ToString().Trim(), out DateTime tempDate) ? tempDate : (DateTime?)null;
        //                        newHospital.HospitalComments = row[11]?.ToString().Trim() ?? string.Empty;
        //                        newHospital.IsActive = true;


        //                        HospitalList.Add(newHospital);

        //                    }
        //                }
        //                // Add the new hosptial to the database
        //                foreach (var hospital in HospitalList)
        //                {
        //                    var id = await HospitalData.CreateHospitalAsync(hospital);
        //                }

        //                // Refresh the list of lookups
        //                Hospitals = (List<Hospital>)await HospitalData.ListHospitalsAsync(token);
        //                await InvokeAsync(StateHasChanged);
        //            }
        //        }
        //    }
        //}



    }
}
