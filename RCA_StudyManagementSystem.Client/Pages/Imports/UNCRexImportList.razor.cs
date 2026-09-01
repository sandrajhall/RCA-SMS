using ExcelDataReader;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.DTOs;
using RCA_StudyManagementSystem.Shared.ImportViews;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static MudBlazor.CategoryTypes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RCA_StudyManagementSystem.Client.Pages.Imports
{
    public partial class UNCRexImportList : Microsoft.AspNetCore.Components.ComponentBase
    {
        private readonly DialogOptions _options = new() { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        private readonly DialogOptions _maxWidth = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

        MudDataGrid<UNCRexImportView>? importGrid { get; set; }

        public CancellationToken CancellationToken { get; set; } = new CancellationToken();

        public IEnumerable<UNCRexImportView>? UNCRexImports { get; set; } = new List<UNCRexImportView>();

        private IEnumerable<UNCRexImportView>? _displayItems { get; set; } = new List<UNCRexImportView>();

        private int Index = 0;


        private string? StatusMessage { get; set; }

        private const string GridStateStorageKey = "ImportDataGridState"; // Key for local storage


        private string? _searchString;
        private List<string> _events = new();
        private CancellationToken token;

        private Dictionary<string, string> PdfTokens { get; set; } = new();


        // quick filter - filter globally across multiple columns with the same input
        private Func<UNCRexImportView, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.PAT_LAST_NAME!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))

                return true;
            if (x.PAT_FIRST_NAME!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.CITY!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        };


        protected override async Task OnInitializedAsync()
        {

            Console.WriteLine("OnInitializedAsync method called!");

            UNCRexImports = new List<UNCRexImportView>();

            _displayItems = UNCRexImports;

            return;

        }

        private async Task LoadGrid()
        {
            _displayItems = new List<UNCRexImportView>();

            _displayItems = UNCRexImports.ToList();

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {


            if (firstRender && importGrid != null)
            {
                //UNCRexImports = ;

                var storedStateDto = await LocalStorage.GetItemAsync<GridStateDto>(GridStateStorageKey);

                if (storedStateDto != null)
                {
                    _searchString = storedStateDto.SearchString ?? ""; // Restore the search string

                    // Map DTOs back to MudBlazor types and update the ViewModel
                    var newFilterDefs = storedStateDto.Filters
                        .Select(dto =>
                        {
                            // Find the column by property name
                            var column = importGrid.RenderedColumns.FirstOrDefault(c => c.PropertyName == dto.Field);
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
                        .ToDictionary(dto => dto.SortBy, dto => new SortDefinition<UNCRexImportView>(
                            dto.SortBy,
                            dto.Descending,
                            0,
                            null
                        ));

                    var newSortColumn = ImportGridStateView.SortColumn;

                    ImportGridStateView.FilterDefinitions = newFilterDefs!;
                    ImportGridStateView.SortDefinitions = newSortDefs;

                    importGrid.FilterDefinitions = ImportGridStateView.FilterDefinitions;
                    importGrid.SortDefinitions = ImportGridStateView.SortDefinitions;

                    // Set the sort definitions
                    if (ImportGridStateView.SortDefinitions.Any())
                    {
                        // Apply the first sort definition if available
                        var firstSort = ImportGridStateView.SortDefinitions.First();
                        SortDirection direction = firstSort.Value.Descending ? SortDirection.Descending : SortDirection.Ascending;
                        await importGrid.SetSortAsync(firstSort.Key, direction, firstSort.Value.SortFunc);

                        var sortDefinitions = importGrid.SortDefinitions; // Get current sort settings

                        string sortByProperty = "";
                        var sortedData = UNCRexImports.AsQueryable(); // Start with unsorted data



                        if (sortDefinitions != null && sortDefinitions.Any())
                        {
                            sortedData = UNCRexImports.AsQueryable(); // Start with unsorted data

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
                            _displayItems = UNCRexImports; // If no sorting, revert to original order
                        }
                    }
                    // Load CurrentPage and PageSize
                    // Check that the dataGrid reference is not null

                    ImportGridStateView.CurrentPage = storedStateDto.CurrentPage;
                    //CaseGridStateView.PageSize = storedStateDto.PageSize;

                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        private async Task Import(InputFileChangeEventArgs e)
        {
            Encoding.RegisterProvider(
                System.Text.CodePagesEncodingProvider.Instance);

            var file = e.File;

            await using var stream = file.OpenReadStream(
                maxAllowedSize: 10 * 1024 * 1024);

            await using var memoryStream = new MemoryStream();

            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var reader = ExcelReaderFactory.CreateReader(memoryStream);

            var result = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            });

            var importedRecords = new List<UNCRexImportView>();

            foreach (DataTable table in result.Tables)
            {
                var studyName = table.TableName?.Trim();

                foreach (DataRow row in table.Rows)
                {
                    if (IsBlankRow(row))
                        continue;

                    var record = new UNCRexImportView
                    {
                        Study = studyName,

                        PAT_MRN_ID = GetString(row, "PAT_MRN_ID"),
                        PAT_ID = GetString(row, "PAT_ID"),
                        PAT_LAST_NAME = GetString(row, "PAT_LAST_NAME"),
                        PAT_FIRST_NAME = GetString(row, "PAT_FIRST_NAME"),
                        PAT_MIDDLE_NAME = GetString(row, "PAT_MIDDLE_NAME"),
                        PreferredLang = GetString(row, "PreferredLang"),
                        DOB = GetNullableDateTime(row, "DOB"),
                        Gender = GetString(row, "Gender"),
                        Race = GetString(row, "Race"),
                        Marital_Status = GetString(row, "Marital_Status"),
                        SSN = GetString(row, "SSN"),
                        PhoneNumber = GetString(row, "PhoneNumber"),
                        EMAIL_ADDRESS = GetString(row, "EMAIL_ADDRESS"),
                        ADD_LINE_1 = GetString(row, "ADD_LINE_1"),
                        CITY = GetString(row, "CITY"),
                        State = GetString(row, "State"),
                        ZIP = GetString(row, "ZIP"),

                        LAB_NAME = GetString(row, "LAB_NAME"),
                        CASE_ID = GetString(row, "CASE_ID"),
                        RESULT_ID = GetString(row, "RESULT_ID"),
                        SPEC_NUMBER_LN1 = GetString(row, "SPEC_NUMBER_LN1"),
                        SPECIMEN_ID = GetString(row, "SPECIMEN_ID"),
                        SpecSource_Name = GetString(row, "SpecSource_Name"),
                        SpecSource_Code = GetString(row, "SpecSource_Code"),
                        SpecType_Name = GetString(row, "SpecType_Name"),
                        CaseStatus = GetString(row, "CaseStatus"),
                        CASE_TYPE_ID = GetString(row, "CASE_TYPE_ID"),
                        Submitter = GetString(row, "Submitter"),
                        SignedOutDate = GetNullableDateTime(row, "SignedOutDate"),
                        CollectedDate = GetNullableDateTime(row, "CollectedDate"),

                        FinalDiagnosis = GetString(row, "FinalDiagnosis"),
                        Addendum_1 = GetString(row, "Addendum_1"),
                        Addendum_2 = GetString(row, "Addendum_2"),
                        Addendum_3 = GetString(row, "Addendum_3"),
                        Addendum_4 = GetString(row, "Addendum_4"),
                        Addendum_5 = GetString(row, "Addendum_5"),
                        SynopticTerms = GetString(row, "SynopticTerms"),
                        PosTerm = GetString(row, "PosTerm"),
                        NegTerm = GetString(row, "NegTerm"),
                        ICD_10 = GetString(row, "ICD_10"),
                        Order_Comments = GetString(row, "Order_Comments"),
                        SNOMED = GetString(row, "SNOMED"),
                        SNOMED_Rslt = GetString(row, "SNOMED_Rslt"),
                        DX_NAME = GetString(row, "DX_NAME"),
                        Diag_Comment = GetString(row, "Diag_Comment"),
                        Clinical_History = GetString(row, "Clinical_History"),
                        AuthorizingProvider = GetString(row, "AuthorizingProvider"),
                        Pathologist = GetString(row, "Pathologist"),
                        Synoptic_Report = GetString(row, "Synoptic_Report"),
                        Gross_Description = GetString(row, "Gross_Description")
                    };

                    importedRecords.Add(record);
                }
            }

            UNCRexImports = importedRecords;
            _displayItems = UNCRexImports;
            CreatePdfTokens();

            StatusMessage =
                $"{UNCRexImports.Count()} records imported from {result.Tables.Count} worksheet(s).";

            await InvokeAsync(StateHasChanged);
        }

        private async void OnCurrentPageChanged(int page)
        {
            ImportGridStateView.CurrentPage = page;

            var storedStateDto = await LocalStorage.GetItemAsync<GridStateDto>(GridStateStorageKey);
            if (storedStateDto == null)
            {
                storedStateDto = new GridStateDto();
            }

            storedStateDto.CurrentPage = page;
            StateHasChanged(); // Update the UI to show the new page number
        }


        private void CreatePdfTokens()
        {
            PdfTokens.Clear();

            foreach (var record in _displayItems)
            {
                if (string.IsNullOrWhiteSpace(record.PAT_ID))
                    continue;

                var token = PdfStore.Store(record);

                PdfTokens[record.PAT_ID] = token;
            }
        }

        // Helper to dynamically get property value for sorting
        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null) ?? string.Empty;
        }



        async Task<IDialogReference> ViewItem(UNCRexImportView args, UNCRexImportView pArgs)
        {

            var newUNCRexImports = new List<UNCRexImportView>();

            foreach (var import in importGrid.FilteredItems)
            {
                var newUNCRexImport = import;
                newUNCRexImports.Add(newUNCRexImport);
            }


            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<UNCRexImportViewDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, newUNCRexImports); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, importGrid.FilteredItems.ToList().IndexOf(pArgs)); // Set initial position

            var options = _options;

            return await DialogService.ShowAsync<UNCRexImportViewDialog>("UNCRex Import View", parameters, options);

        }


        private static bool IsBlankRow(DataRow row)
        {
            if (row == null)
                return true;

            return row.ItemArray.All(value =>
                value == null ||
                value == DBNull.Value ||
                string.IsNullOrWhiteSpace(value.ToString()));
        }

        private static string? GetString(DataRow row, string columnName)
        {
            if (row == null ||
                string.IsNullOrWhiteSpace(columnName) ||
                !row.Table.Columns.Contains(columnName))
            {
                return null;
            }

            object value = row[columnName];

            if (value == null || value == DBNull.Value)
                return null;

            return Convert.ToString(value)?.Trim();
        }

        private static DateTime? GetNullableDateTime(
            DataRow row,
            string columnName)
        {
            if (row == null ||
                string.IsNullOrWhiteSpace(columnName) ||
                !row.Table.Columns.Contains(columnName))
            {
                return null;
            }

            object value = row[columnName];

            if (value == null || value == DBNull.Value)
                return null;

            if (value is DateTime dateTime)
                return dateTime;

            // Excel may store dates as OLE Automation numbers.
            if (value is double doubleValue)
            {
                try
                {
                    return DateTime.FromOADate(doubleValue);
                }
                catch
                {
                    return null;
                }
            }

            if (value is decimal decimalValue)
            {
                try
                {
                    return DateTime.FromOADate((double)decimalValue);
                }
                catch
                {
                    return null;
                }
            }

            if (value is int intValue)
            {
                try
                {
                    return DateTime.FromOADate(intValue);
                }
                catch
                {
                    return null;
                }
            }

            if (DateTime.TryParse(
                    Convert.ToString(value),
                    out DateTime parsedDate))
            {
                return parsedDate;
            }

            return null;
        }

        public async void Dispose()
        {
            Console.WriteLine("Dispose method called for DataGrid component.");

            if (importGrid != null)
            {
                // Map MudBlazor types to DTOs for serialization
                var filtersToSave = importGrid.FilterDefinitions
                    .Select(f => new FilterDefinitionDto
                    {
                        // Get the Field name from the Column property of the FilterDefinition
                        Field = f.Column?.PropertyName ?? string.Empty,
                        Operator = f.Operator!,
                        Value = f.Value!.ToString(), // Convert Value to string for serialization
                        BoolValue = f.Value is bool boolValue ? boolValue : null // Handle boolean values
                    })
                    .ToList();

                var sortsToSave = importGrid.SortDefinitions.Values // Iterate over values of the Dictionary
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
                    var sortColum = importGrid.RenderedColumns.FirstOrDefault(c => c.PropertyName == sortsToSave.FirstOrDefault()!.SortBy)?.Title ?? string.Empty;
                    ImportGridStateView.SortColumn = sortColum;
                }

                // Create a DTO to hold the state
                var stateDto = new GridStateDto
                {
                    Filters = filtersToSave,
                    Sorts = sortsToSave,
                    CurrentPage = importGrid.CurrentPage, // Save current page
                    SearchString = _searchString,
                };

                // Save the DTO to local storage
                await LocalStorage.SetItemAsync(GridStateStorageKey, stateDto);

                ImportGridStateView.SortDefinitions = stateDto.Sorts
                    .ToDictionary(s => s.SortBy, s => new SortDefinition<UNCRexImportView>(
                        s.SortBy,
                        s.Descending,
                        s.Index,
                        (Func<UNCRexImportView, object>)s.SortFunc
                    ));



                Console.WriteLine($"State saved to LocalStorage. Filters Count: {filtersToSave.Count}, Sorts Count: {sortsToSave.Count}, Page: {importGrid.CurrentPage}");
            }
            else
            {
                Console.WriteLine("mudDataGrid was null during Dispose.");
            }
        }

    }
}

