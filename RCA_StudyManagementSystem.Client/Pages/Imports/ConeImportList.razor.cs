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
    public partial class ConeImportList : Microsoft.AspNetCore.Components.ComponentBase
    {
        private readonly DialogOptions _options = new() { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        private readonly DialogOptions _maxWidth = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

        MudDataGrid<ConeImportView>? importGrid { get; set; }

        public CancellationToken CancellationToken { get; set; } = new CancellationToken();

        public IEnumerable<ConeImportView>? ConeImports { get; set; } = new List<ConeImportView>();

        private IEnumerable<ConeImportView>? _displayItems { get; set; } = new List<ConeImportView>();

        private int Index = 0;


        private string? StatusMessage { get; set; }

        private const string GridStateStorageKey = "ImportDataGridState"; // Key for local storage

        public string SelectedHospital { get; set; } = string.Empty;

        private string? _searchString;
        private List<string> _events = new();
        private CancellationToken token;

        private Dictionary<string, string> PdfTokens { get; set; } = new();


        // quick filter - filter globally across multiple columns with the same input
        private Func<ConeImportView, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.LAST_NAME!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))

                return true;
            if (x.FIRST_NAME!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.CITY!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        };


        protected override async Task OnInitializedAsync()
        {

            Console.WriteLine("OnInitializedAsync method called!");

            ConeImports = new List<ConeImportView>();

            _displayItems = ConeImports;

            return;

        }

        private async Task LoadGrid()
        {
            _displayItems = new List<ConeImportView>();

            _displayItems = ConeImports.ToList();

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {


            if (firstRender && importGrid != null)
            {
                //ConeImports = ;

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
                        .ToDictionary(dto => dto.SortBy, dto => new SortDefinition<ConeImportView>(
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
                        var sortedData = ConeImports.AsQueryable(); // Start with unsorted data



                        if (sortDefinitions != null && sortDefinitions.Any())
                        {
                            sortedData = ConeImports.AsQueryable(); // Start with unsorted data

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
                            _displayItems = ConeImports; // If no sorting, revert to original order
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

        private List<ConeImportView> ImportedRecords { get; set; } = new();

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
                    var studyName = table.TableName?.Trim();

                    foreach (DataRow row in table.Rows)
                    {
                        if (IsBlankRow(row))
                        {
                            continue;
                        }

                        var record = new ConeImportView
                        {
                            Study = studyName,
                            HospitalName = SelectedHospital,

                            FACILITY =
                                GetConeExcelString(row, "FACILITY"),

                            MRN =
                                GetConeExcelString(row, "MRN"),

                            LAST_NAME =
                                GetConeExcelString(row, "LAST_NAME"),

                            FIRST_NAME =
                                GetConeExcelString(row, "FIRST_NAME"),

                            MIDDLE_INITIAL =
                                GetConeExcelString(row, "MIDDLE_INITIAL"),

                            ADDRESS =
                                GetConeExcelString(row, "ADDRESS"),

                            CITY =
                                GetConeExcelString(row, "CITY"),

                            STATE =
                                GetConeExcelString(row, "STATE"),

                            ZIP =
                                GetConeExcelString(row, "ZIP"),

                            SSN =
                                GetConeExcelString(row, "SSN"),

                            HOME_PHONE =
                                GetConeExcelString(row, "HOME_PHONE"),

                            BIRTH_DATE =
                                GetConeExcelDate(row, "BIRTH_DATE"),

                            RACE =
                                GetConeExcelString(row, "RACE"),

                            SEX =
                                GetConeExcelString(row, "SEX"),

                            MARITAL_STATUS =
                                GetConeExcelString(row, "MARITAL_STATUS"),

                            DISCHARGE_DISPOSITION =
                                GetConeExcelString(
                                    row,
                                    "DISCHARGE_DISPOSITION"),

                            C_Specimen_Specnum_Formatted =
                                GetConeExcelString(
                                    row,
                                    "C_Specimen_Specnum_Formatted"),

                            C_Specimen_Accession_Date =
                                GetConeExcelDate(
                                    row,
                                    "C_Specimen_Accession_Date"),

                            C_Specimen_Accession_Time =
                                GetConeExcelString(
                                    row,
                                    "C_Specimen_Accession_Time"),

                            AUTHRZING_Last_Name =
                                GetConeExcelString(
                                    row,
                                    "AUTHRZING_Last_Name"),

                            AUTHRZING_First_Name =
                                GetConeExcelString(
                                    row,
                                    "AUTHRZING_First_Name"),

                            AUTHRZING_Middle_Name =
                                GetConeExcelString(
                                    row,
                                    "AUTHRZING_Middle_Name"),

                            C_D_Person_Phy_Street =
                                GetConeExcelString(
                                    row,
                                    "C_D_Person_Phy_Street"),

                            C_D_Person_Phy_City =
                                GetConeExcelString(
                                    row,
                                    "C_D_Person_Phy_City"),

                            C_D_Person_Phy_State =
                                GetConeExcelString(
                                    row,
                                    "C_D_Person_Phy_State"),

                            C_D_Person_Phy_Zip =
                                GetConeExcelString(
                                    row,
                                    "C_D_Person_Phy_Zip"),

                            Clinical_History =
                                GetConeExcelString(
                                    row,
                                    "Clinical_History"),

                            Gross_Description =
                                GetConeExcelString(
                                    row,
                                    "Gross_Description"),

                            Final_Microscopic_Diagnosis =
                                GetConeExcelString(
                                    row,
                                    "Final_Microscopic_Diagnosis"),

                            Addendum =
                                GetConeExcelString(row, "Addendum"),

                            Comments =
                                GetConeExcelString(row, "Comments"),

                            Pathologist =
                                GetConeExcelString(row, "Pathologist"),

                            Language =
                                GetConeExcelString(row, "Language")
                        };

                        ImportedRecords.Add(record);
                    }
                }

                _displayItems = ImportedRecords;
                CreatePdfTokens();


                if (ImportedRecords.Count == 0)
                {
                    Snackbar.Add(
                        "No valid records were found in the Excel file.",
                        Severity.Warning);

                    return;
                }

                /*
                 * Save the records here if needed.
                 *
                 * Example:
                 *
                 * await ConeData.ImportAsync(ImportedRecords);
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



        async Task<IDialogReference> ViewItem(ConeImportView args, ConeImportView pArgs)
        {

            var newConeImports = new List<ConeImportView>();

            foreach (var import in importGrid.FilteredItems)
            {
                var newConeImport = import;
                newConeImports.Add(newConeImport);
            }


            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<ConeImportViewDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, newConeImports); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, importGrid.FilteredItems.ToList().IndexOf(pArgs)); // Set initial position

            var options = _options;

            return await DialogService.ShowAsync<ConeImportViewDialog>($"{SelectedHospital} Import View", parameters, options);

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

        private static string? GetConeExcelString(
    DataRow row,
    string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
            {
                return null;
            }

            var value = row[columnName];

            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            var text = value.ToString()?.Trim();

            return string.IsNullOrWhiteSpace(text)
                ? null
                : text;
        }

        private static DateTime? GetConeExcelDate(
    DataRow row,
    string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
            {
                return null;
            }

            var value = row[columnName];

            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            if (value is DateTime dateTime)
            {
                return dateTime;
            }

            if (value is double excelSerialDate)
            {
                try
                {
                    return DateTime.FromOADate(excelSerialDate);
                }
                catch
                {
                    return null;
                }
            }

            if (value is decimal decimalDate)
            {
                try
                {
                    return DateTime.FromOADate(
                        Convert.ToDouble(decimalDate));
                }
                catch
                {
                    return null;
                }
            }

            var text = value.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            if (DateTime.TryParse(text, out var parsedDate))
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
                    .ToDictionary(s => s.SortBy, s => new SortDefinition<ConeImportView>(
                        s.SortBy,
                        s.Descending,
                        s.Index,
                        (Func<ConeImportView, object>)s.SortFunc
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

