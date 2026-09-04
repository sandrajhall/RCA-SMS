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
    public partial class AtriumImportList : Microsoft.AspNetCore.Components.ComponentBase
    {
        private readonly DialogOptions _options = new() { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        private readonly DialogOptions _maxWidth = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

        MudDataGrid<AtriumImportView>? importGrid { get; set; }

        public CancellationToken CancellationToken { get; set; } = new CancellationToken();

        public IEnumerable<AtriumImportView>? AtriumImports { get; set; } = new List<AtriumImportView>();

        private IEnumerable<AtriumImportView>? _displayItems { get; set; } = new List<AtriumImportView>();

        private int Index = 0;


        private string? StatusMessage { get; set; }

        private const string GridStateStorageKey = "ImportDataGridState"; // Key for local storage

        public string SelectedHospital { get; set; } = string.Empty;

        private string? _searchString;
        private List<string> _events = new();
        private CancellationToken token;

        private Dictionary<string, string> PdfTokens { get; set; } = new();


        // quick filter - filter globally across multiple columns with the same input
        private Func<AtriumImportView, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Last_Name!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))

                return true;
            if (x.First_Name!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.City!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        };


        protected override async Task OnInitializedAsync()
        {

            Console.WriteLine("OnInitializedAsync method called!");

            AtriumImports = new List<AtriumImportView>();

            _displayItems = AtriumImports;

            return;

        }

        private async Task LoadGrid()
        {
            _displayItems = new List<AtriumImportView>();

            _displayItems = AtriumImports.ToList();

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {


            if (firstRender && importGrid != null)
            {
                //AtriumImports = ;

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
                        .ToDictionary(dto => dto.SortBy, dto => new SortDefinition<AtriumImportView>(
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
                        var sortedData = AtriumImports.AsQueryable(); // Start with unsorted data



                        if (sortDefinitions != null && sortDefinitions.Any())
                        {
                            sortedData = AtriumImports.AsQueryable(); // Start with unsorted data

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
                            _displayItems = AtriumImports; // If no sorting, revert to original order
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

        private List<AtriumImportView> ImportedRecords { get; set; } = new();

        private async Task Import(InputFileChangeEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SelectedHospital))
            {
                Snackbar.Add(
                    "Please select a hospital.",
                    Severity.Error);

                return;
            }

            try
            {
                ImportedRecords.Clear();

                Encoding.RegisterProvider(
                    System.Text.CodePagesEncodingProvider.Instance);

                var file = e.File;

                if (file is null)
                {
                    Snackbar.Add(
                        "Please select an Excel file.",
                        Severity.Error);

                    return;
                }

                if (file.Size == 0)
                {
                    Snackbar.Add(
                        "The selected file is empty.",
                        Severity.Error);

                    return;
                }

                const long maxAllowedSize = 10 * 1024 * 1024;

                await using var stream =
                    file.OpenReadStream(maxAllowedSize);

                await using var memoryStream = new MemoryStream();

                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var reader =
                    ExcelReaderFactory.CreateReader(memoryStream);

                var result = reader.AsDataSet(
                    new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ =>
                            new ExcelDataTableConfiguration
                            {
                                UseHeaderRow = true
                            }
                    });

                foreach (DataTable table in result.Tables)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        if (IsBlankRow(row))
                        {
                            continue;
                        }

                        var facilityName =
                            GetExcelString(row, "Facility_Name");

                        var record = new AtriumImportView
                        {
                            Study = GetExcelString(row, "Study"),
                            SpecimenSource =
                                GetExcelString(row, "SpecimenSource"),

                            Final_Diagnosis =
                                GetExcelString(row, "Final_Diagnosis"),

                            Dx_Comments =
                                GetExcelString(row, "Dx_Comments"),

                            Addendums =
                                GetExcelString(row, "Addendums"),

                            Requisition_ID =
                                GetExcelString(row, "Requisition_ID"),

                            Facility_Name =
                                string.IsNullOrWhiteSpace(facilityName)
                                    ? SelectedHospital
                                    : facilityName,

                            MRN =
                                GetExcelString(row, "MRN"),

                            Last_Name =
                                GetExcelString(row, "Last_Name"),

                            First_Name =
                                GetExcelString(row, "First_Name"),

                            Middle_Name =
                                GetExcelString(row, "Middle_Name"),

                            DOB =
                                GetExcelDate(row, "DOB"),

                            SSN =
                                GetExcelString(row, "SSN"),

                            Gender =
                                GetExcelString(row, "Gender"),

                            PatientRace =
                                GetExcelString(row, "PatientRace"),

                            Ethnicity =
                                GetExcelString(row, "Ethnicity"),

                            Language =
                                GetExcelString(row, "Language"),

                            MaritalStatus =
                                GetExcelString(row, "MaritalStatus"),

                            Address_Line1 =
                                GetExcelString(row, "Address_Line1"),

                            Address_Line2 =
                                GetExcelString(row, "Address_Line2"),

                            City =
                                GetExcelString(row, "City"),

                            State =
                                GetExcelString(row, "State"),

                            Zip =
                                GetExcelString(row, "Zip"),

                            Phone_Number_Home =
                                GetExcelString(row, "Phone_Number_Home"),

                            Phone_Number_Mobile =
                                GetExcelString(row, "Phone_Number_Mobile"),

                            Email =
                                GetExcelString(row, "Email"),

                            Path_Report_Number =
                                GetExcelString(row, "Path_Report_Number"),

                            Date_Collected =
                                GetExcelDate(row, "Date_Collected"),

                            Lab_Name =
                                GetExcelString(row, "Lab_Name"),

                            Authorizing_Provider =
                                GetExcelString(row, "Authorizing_Provider"),

                            Pathologist =
                                GetExcelString(row, "Pathologist"),

                            Clinical_History =
                                GetExcelString(row, "Clinical_History"),

                            Gross_Description =
                                GetExcelString(row, "Gross_Description"),

                            SynopticReport =
                                GetExcelString(row, "SynopticReport")
                        };

                        ImportedRecords.Add(record);
                    }
                }

                if (ImportedRecords.Count == 0)
                {
                    Snackbar.Add(
                        "No valid records were found in the Excel file.",
                        Severity.Warning);

                    return;
                }

                /*
                 * Save the imported records here.
                 *
                 * For example:
                 *
                 * await AtriumData.ImportAsync(ImportedRecords);
                 */

                Snackbar.Add(
                    $"Successfully imported {ImportedRecords.Count} record(s).",
                    Severity.Success);

                await InvokeAsync(StateHasChanged);
            }
            catch (InvalidDataException ex)
            {
                Console.Error.WriteLine(ex);

                Snackbar.Add(
                    "The selected file is not a valid Excel workbook.",
                    Severity.Error);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

                Snackbar.Add(
                    $"Import failed: {ex.Message}",
                    Severity.Error);
            }
       
        AtriumImports = ImportedRecords;
            _displayItems = AtriumImports;
            CreatePdfTokens();

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
                if (string.IsNullOrWhiteSpace(record.MRN))
                    continue;

                var token = PdfStore.Store(record);

                PdfTokens[record.MRN] = token;
            }
        }

        // Helper to dynamically get property value for sorting
        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null) ?? string.Empty;
        }



        async Task<IDialogReference> ViewItem(AtriumImportView args, AtriumImportView pArgs)
        {

            var newAtriumImports = new List<AtriumImportView>();

            foreach (var import in importGrid.FilteredItems)
            {
                var newAtriumImport = import;
                newAtriumImports.Add(newAtriumImport);
            }


            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<AtriumImportViewDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, newAtriumImports); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, importGrid.FilteredItems.ToList().IndexOf(pArgs)); // Set initial position

            var options = _options;

            return await DialogService.ShowAsync<AtriumImportViewDialog>($"{SelectedHospital} Import View", parameters, options);

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

        private static string? GetExcelString(DataRow row, string columnName)
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

        private static DateTime? GetExcelDate(
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

        private void NameChanged(string value)
        {
            SelectedHospital = value;
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
                    .ToDictionary(s => s.SortBy, s => new SortDefinition<AtriumImportView>(
                        s.SortBy,
                        s.Descending,
                        s.Index,
                        (Func<AtriumImportView, object>)s.SortFunc
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

