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
    public partial class DukeImportList : Microsoft.AspNetCore.Components.ComponentBase
    {
        private readonly DialogOptions _options = new() { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        private readonly DialogOptions _maxWidth = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

        MudDataGrid<DukeImportView>? importGrid { get; set; }

        public CancellationToken CancellationToken { get; set; } = new CancellationToken();

        public IEnumerable<DukeImportView>? DukeImports { get; set; } = new List<DukeImportView>();

        private IEnumerable<DukeImportView>? _displayItems { get; set; } = new List<DukeImportView>();

        private int Index = 0;


        private string? StatusMessage { get; set; }

        private const string GridStateStorageKey = "ImportDataGridState"; // Key for local storage

        private List<DukeImportView> ImportedRecords { get; set; } = new();

        private string ImportMessage { get; set; } = string.Empty;

        private string? _searchString;
        private List<string> _events = new();
        private CancellationToken token;

        private Dictionary<string, string> PdfTokens { get; set; } = new();


        // quick filter - filter globally across multiple columns with the same input
        private Func<DukeImportView, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.LastName!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))

                return true;
            if (x.FirstName!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.CurrentCity!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        };


        protected override async Task OnInitializedAsync()
        {

            Console.WriteLine("OnInitializedAsync method called!");

            DukeImports = new List<DukeImportView>();

            _displayItems = DukeImports;

            return;

        }

        private async Task LoadGrid()
        {
            _displayItems = new List<DukeImportView>();

            _displayItems = DukeImports.ToList();

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {


            if (firstRender && importGrid != null)
            {
                //DukeImports = ;

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
                        .ToDictionary(dto => dto.SortBy, dto => new SortDefinition<DukeImportView>(
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
                        var sortedData = DukeImports.AsQueryable(); // Start with unsorted data



                        if (sortDefinitions != null && sortDefinitions.Any())
                        {
                            sortedData = DukeImports.AsQueryable(); // Start with unsorted data

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
                            _displayItems = DukeImports; // If no sorting, revert to original order
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
            try
            {
                ImportMessage = string.Empty;
                ImportedRecords.Clear();

                Encoding.RegisterProvider(
                    System.Text.CodePagesEncodingProvider.Instance);

                var file = e.File;

                if (file is null)
                {
                    ImportMessage = "Please select an Excel file.";
                    return;
                }

                if (file.Size == 0)
                {
                    ImportMessage = "The selected file is empty.";
                    return;
                }

                const long maxFileSize = 10 * 1024 * 1024;

                await using var stream =
                    file.OpenReadStream(maxFileSize);

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

                        var record = new DukeImportView
                        {
                            Study = studyName,

                            MedicalRecordNumber =
                                GetDukeExcelString(
                                    row,
                                    "Medical Record Number"),

                            LastName =
                                GetDukeExcelString(
                                    row,
                                    "Name--Last"),

                            FirstName =
                                GetDukeExcelString(
                                    row,
                                    "Name--First"),

                            MiddleName =
                                GetDukeExcelString(
                                    row,
                                    "Name--Middle"),

                            DateOfBirth =
                                GetDukeExcelDate(
                                    row,
                                    "Date Of Birth"),

                            Sex =
                                GetDukeExcelString(
                                    row,
                                    "Sex"),

                            SocialSecurityNumber =
                                GetDukeExcelString(
                                    row,
                                    "Social Security Number"),

                            CurrentAddress =
                                GetDukeExcelString(
                                    row,
                                    "Addr Current--No & Street"),

                            CurrentCity =
                                GetDukeExcelString(
                                    row,
                                    "Addr Current--City"),

                            DiagnosisState =
                                GetDukeExcelString(
                                    row,
                                    "Addr At Dx--State"),

                            CurrentPostalCode =
                                GetDukeExcelString(
                                    row,
                                    "Addr Current--Postal Code"),

                            Race1 =
                                GetDukeExcelString(
                                    row,
                                    "Race 1"),

                            MaritalStatusAtDiagnosis =
                                GetDukeExcelString(
                                    row,
                                    "Marital Status at Diagnosis"),

                            Telephone =
                                GetDukeExcelString(
                                    row,
                                    "Telephone"),

                            PathReportNumberId =
                                GetDukeExcelString(
                                    row,
                                    "PATH REPORT NUMBER/ID (ex RXS19-012345)"),

                            PathReportCollectedDate =
                                GetDukeExcelDate(
                                    row,
                                    "PATH REPORT COLLECTED DATE"),

                            PathReportAuthorizingProvider =
                                GetDukeExcelString(
                                    row,
                                    "* PATH REPORT: AUTHORIZING PROVIDER"),

                            PathReportOrderingLocation =
                                GetDukeExcelString(
                                    row,
                                    "PATH REPORT: ORDERING LOCATION"),

                            PathReportPathologist =
                                GetDukeExcelString(
                                    row,
                                    "PATH REPORT: PATHOLOGIST"),

                            PathReportFinalDiagnosis =
                                GetDukeExcelString(
                                    row,
                                    "PATH REPORT FINAL DIAGNOSIS"),

                            PathReportComment =
                                GetDukeExcelString(
                                    row,
                                    "PATH REPORT COMMENT"),

                            PathReportClinicalHistory =
                                GetDukeExcelString(
                                    row,
                                    "PATH REPORT CLIN HX"),

                            PathReportMicroscopicDescription =
                                GetDukeExcelString(
                                    row,
                                    "PATH REPORT MICROSCOPIC DESCRIPTION"),

                            Addendum1 =
                                GetDukeExcelString(
                                    row,
                                    "ADDENDUM_1"),

                            Addendum2 =
                                GetDukeExcelString(
                                    row,
                                    "ADDENDUM_2"),

                            Addendum3 =
                                GetDukeExcelString(
                                    row,
                                    "ADDENDUM_3"),

                            Addendum4 =
                                GetDukeExcelString(
                                    row,
                                    "ADDENDUM_4"),

                            Addendum5 =
                                GetDukeExcelString(
                                    row,
                                    "ADDENDUM_5")
                        };

                        ImportedRecords.Add(record);
                    }
                }

                _displayItems = ImportedRecords;
                CreatePdfTokens();

                if (ImportedRecords.Count == 0)
                {
                    ImportMessage =
                        "No valid records were found in the Excel file.";

                    return;
                }

                ImportMessage =
                    $"Successfully imported {ImportedRecords.Count} record(s).";

                // Save to the database here if needed:
                //
                // await DukeData.ImportAsync(ImportedRecords);
            }
            catch (InvalidDataException ex)
            {
                ImportedRecords.Clear();

                ImportMessage =
                    "The selected file is not a valid Excel workbook.";

                Console.Error.WriteLine(ex);
            }
            catch (Exception ex)
            {
                ImportedRecords.Clear();

                ImportMessage =
                    $"Import failed: {ex.Message}";

                Console.Error.WriteLine(ex);
            }

            await InvokeAsync(StateHasChanged);
        }

        private static string? GetDukeExcelString(
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

        private static DateTime? GetDukeExcelDate(
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
                if (string.IsNullOrWhiteSpace(record.MedicalRecordNumber))
                    continue;

                var token = PdfStore.Store(record);

                PdfTokens[record.MedicalRecordNumber] = token;
            }
        }

        // Helper to dynamically get property value for sorting
        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null) ?? string.Empty;
        }



        async Task<IDialogReference> ViewItem(DukeImportView args, DukeImportView pArgs)
        {

            var newDukeImports = new List<DukeImportView>();

            foreach (var import in importGrid.FilteredItems)
            {
                var newDukeImport = import;
                newDukeImports.Add(newDukeImport);
            }


            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<DukeImportViewDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, newDukeImports); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, importGrid.FilteredItems.ToList().IndexOf(pArgs)); // Set initial position

            var options = _options;

            return await DialogService.ShowAsync<DukeImportViewDialog>($"Duke Import View", parameters, options);

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
                    .ToDictionary(s => s.SortBy, s => new SortDefinition<DukeImportView>(
                        s.SortBy,
                        s.Descending,
                        s.Index,
                        (Func<DukeImportView, object>)s.SortFunc
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

