using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Pages.Cases;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Client.Utilities;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.DTOs;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Threading.Tasks;
using System.Xml.Linq;
using static MudBlazor.CategoryTypes;


namespace RCA_StudyManagementSystem.Client.Pages.Exports
{
    public partial class PastList : Microsoft.AspNetCore.Components.ComponentBase
    {
        private readonly DialogOptions _options = new() { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        private readonly DialogOptions _maxWidth = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

        MudDataGrid<PathReportView>? pathGrid { get; set; }

        public IEnumerable<PathReportView>? PathReports { get; set; }
        public IEnumerable<Patient>? Patients { get; set; }

        private IEnumerable<PathReportView>? _displayItems { get; set; }
        private List<PathReportView>? checkedPathReports { get; set; } = new List<PathReportView>();

        private HashSet<PathReportView> SelectedItems = new();

        private int Index = 0;

        private const string GridStateStorageKey = "PathDataGridState"; // Key for local storage


        private string? _searchString;
        private List<string> _events = new();

        [Parameter]
        public Guid StudyId { get; set; }


        protected bool IsSaved = false;
        protected bool IsExported = false;
        protected bool ShowFields = false;
        protected bool HasErrors = false; // Flag to indicate if there are validation errors

        protected string slideSelectValue = "Yes"; // Default value for the slide select
        protected Study studySelectValue;
        protected string studySelectText;
        protected string color = "white"; // Default colour for the patient
        private Study _study;

        private IEnumerable<Study> StudyList = new List<Study>();
        private string prefix = string.Empty; // Prefix for the case number

        [Parameter]
        public string StudyColor { get; set; } = string.Empty; // Color associated with the study, not mapped to the database

        [Parameter]
        public EventCallback<string> StudyColorChanged { get; set; }

     


        private Func<PathReportView, object> _sortByHold => x =>
        {
            if (x.IsOnHold)
                return "true";
            else
                return "false";
        };

        // quick filter - filter globally across multiple columns with the same input
        private Func<PathReportView, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.LastName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.FirstName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.ExportStatus != null && x.ExportStatus.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.CaseNumber != null && x.CaseNumber.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.BatchNumber != null && x.BatchNumber.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.StudyPrefix.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.StudyName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.DateOfBirth.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.RcaExportDate.Value.ToShortDateString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.SocialSecurityNumber.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.DisplayName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.SubmittingHospitalPathReportNumber.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.SubmittingHospital.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.HospShortName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        };

        public Func<PathReportView, object> SortBySelected = x => x.IsSelected;

        protected override async Task OnInitializedAsync()
        {
            StudyList = await StudyData.ListUnarchivedStudiesAsync();

            if (StudyId != Guid.Empty)
            {
                _study = await StudyData.GetStudyAsync(StudyId);
                if (_study != null)
                {
                    await OnStudySelectChanged(_study);
                }
            }
        }

        private async Task SetPathReports()
        {
            Console.WriteLine("SetPathReports method called!");
            PathReports = new List<PathReportView>();
            _displayItems = new List<PathReportView>();
            checkedPathReports = new List<PathReportView>();

            var batches = await BatchData.ListBatchesByStudyAsync(_study.StudyId);

            foreach (var batch in batches)
            {
                var pathsInBatch = await PathReportData.ListPathReportsByBatchAsync(batch.BatchNumber);

                foreach (var path in pathsInBatch)
                {

                    ((List<PathReportView>)PathReports).Add(path);

                }
            }

            //SetPathId(); // Also sets StudyName

            _displayItems = PathReports;

            Patients = await PatientData.ListPatientsAsync();

            await InvokeAsync(StateHasChanged);

            foreach (var item in Patients)
            {
                //item.StudyColor = await StudyData.GetStudyColorAsync(item.StudyId);
                await PathMinAgeCheck(item);

            }
            foreach (var pathReport in PathReports)
            {
                var patient = Patients.FirstOrDefault(p => p.PatientId == pathReport.PatientId);
                if (patient != null)
                {
                    PathReportView newView = await PathMaxAgeCheck(patient, pathReport);
                    checkedPathReports.Add(newView);
                }
            }

            _displayItems = checkedPathReports;

            pathGrid.FilterDefinitions = PathGridStateView.FilterDefinitions;
            pathGrid.SortDefinitions = new Dictionary<string, SortDefinition<PathReportView>>(PathGridStateView.SortDefinitions);

            pathGrid.CurrentPage = PathGridStateView.CurrentPage;

            return;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {


            if (firstRender && pathGrid != null)
            {
                SetPathReports();
                //PathReports = await PathReportData.ListPathReportsAsync("All");
                //SetPathId();

                var storedStateDto = await LocalStorage.GetItemAsync<GridStateDto>(GridStateStorageKey);

                if (storedStateDto != null)
                {
                    // Map DTOs back to MudBlazor types and update the ViewModel
                    var newFilterDefs = storedStateDto.Filters
                        .Select(dto =>
                        {
                            // Find the column by property name
                            var column = pathGrid.RenderedColumns.FirstOrDefault(c => c.PropertyName == dto.Field);
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
                        .ToDictionary(dto => dto.SortBy, dto => new SortDefinition<PathReportView>(
                            dto.SortBy,
                            dto.Descending,
                            0,
                            null
                        ));

                    var newSortColumn = PathGridStateView.SortColumn;

                    PathGridStateView.FilterDefinitions = newFilterDefs!;
                    PathGridStateView.SortDefinitions = newSortDefs;

                    pathGrid.FilterDefinitions = PathGridStateView.FilterDefinitions;
                    pathGrid.SortDefinitions = PathGridStateView.SortDefinitions;

                    // Set the sort definitions
                    if (PathGridStateView.SortDefinitions.Any())
                    {
                        // Apply the first sort definition if available
                        var firstSort = PathGridStateView.SortDefinitions.First();
                        SortDirection direction = firstSort.Value.Descending ? SortDirection.Descending : SortDirection.Ascending;
                        await pathGrid.SetSortAsync(firstSort.Key, direction, firstSort.Value.SortFunc);

                        var sortDefinitions = pathGrid.SortDefinitions; // Get current sort settings

                        string sortByProperty = "";
                        var sortedData = PathReports.AsQueryable(); // Start with unsorted data


                        if (Guid.TryParse(firstSort.Value.SortBy, out _)) // Check if sortby is a Guid
                        {
                            sortByProperty = newSortColumn; // Use the column title

                            if (sortByProperty == "Export Date")
                            {
                                if (firstSort.Value.Descending)
                                {
                                    sortedData = sortedData.OrderByDescending(x => x.RcaExportDate);
                                }
                                else
                                {
                                    sortedData = sortedData.OrderBy(x => x.RcaExportDate);
                                }

                                _displayItems = sortedData.ToList();

                            }
                        }
                        else
                        {
                            if (sortDefinitions != null && sortDefinitions.Any())
                            {
                                sortedData = checkedPathReports.AsQueryable(); // Start with unsorted data

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
                                _displayItems = checkedPathReports; // If no sorting, revert to original order
                            }
                        }
                    }
                    // Load CurrentPage and PageSize
                    // Check that the dataGrid reference is not null

                    PathGridStateView.CurrentPage = storedStateDto.CurrentPage;
                    //CaseGridStateView.PageSize = storedStateDto.PageSize;

                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        // Helper to dynamically get property value for sorting
        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null) ?? string.Empty;
        }

        private async Task OnBatchExport()
        {
            var Study = await StudyData.GetStudyAsync(StudyId);
            var selectedItems = _displayItems?.Where(p => p.IsSelected).ToList();

            var batchNumber = selectedItems.FirstOrDefault()?.BatchNumber;

            var batchId = await BatchData.GetBatchIdAsync(batchNumber);

            var pathIds = "Not applicable";

            var isReport = false;   

            var exportData = await PathReportData.ExportPathReportDataAsync(StudyId, "Past", batchId, pathIds, isReport);

            var dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (exportData != null)
            {
                // Call JavaScript function to download CSV
                await JSRuntime.InvokeVoidAsync("downloadFile", $"{Study.Prefix}_{dateStr}.csv", exportData);
            }
            else
            {
                Snackbar.Add("No data available for export.", Severity.Warning);
            }

        }

        private async Task OnSelectedExport()
        {
            var selectedItems = _displayItems.Where(x => x.IsSelected);
            var selectedIds = selectedItems.Select(p => p.PathReportId).ToList();
            var Study = await StudyData.GetStudyAsync(StudyId);

            // create a new Ad Hoc batch

            var batchPrefix = "EXP-SELECTED-" + Study.Prefix;
            var batchNumber = await GenerateBatchNumber?.Generate(batchPrefix);

            var newBatch = new Batch
            {
                BatchId = Guid.NewGuid(),
                BatchNumber = batchNumber,
                CreatedDate = DateTime.Now,
                StudyId = StudyId,
                NumberOfCases = selectedIds.Count.ToString(),
                ExportDate = DateTime.Now,
            };

            string pathIdsString = string.Join(",", selectedIds.Select(g => g.ToString()));

            var isReport = false;

            var exportData = await PathReportData.ExportPathReportDataAsync(Study.StudyId, "Selected", Guid.Empty, pathIdsString, isReport);

            var dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (exportData != null)
            {
                // Call JavaScript function to download CSV
                await JSRuntime.InvokeVoidAsync("downloadFile", $"{Study.Prefix}_{dateStr}.csv", exportData);
                IsExported = true;
            }
            else
            {
                IsExported = false;
                Snackbar.Add("No data available for export.", Severity.Warning);
            }

            if (IsExported)
            {
                var auth = await AuthStateProvider.GetAuthenticationStateAsync();
                var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                // save the batch
                var batchId = await BatchData.CreateBatchAsync(userId, newBatch);

                // set export status to exported, set export date to now, set batchnumber
                foreach (var id in selectedIds)
                {
                    var path = await PathReportData.GetPathReportAsync(id);

                    // create a new PathReportExport record
                    var pathReportExport = new PathReportExport
                    {
                        PathReportId = path.PathReportId,
                        BatchId = newBatch.BatchId,
                    };

                    await PathReportExportData.CreatePathReportExportAsync(userId, pathReportExport);
                }

                // update the UI
                StateHasChanged();
            }
        }
        private async Task OnBatchReport(Guid StudyId)
        {
            var Study = await StudyData.GetStudyAsync(StudyId);

            var selectedItems = _displayItems?.Where(p => p.IsSelected).ToList();

            var batchNumber = selectedItems.FirstOrDefault()?.BatchNumber;

            var batchId = await BatchData.GetBatchIdAsync(batchNumber);

            var pathIds = "Not applicable";

            var isReport = true;

            var exportData = await PathReportData.ExportPathReportDataAsync(StudyId, "Past", batchId, pathIds, isReport);

            var dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (exportData != null)
            {
                // Call JavaScript function to download CSV
                await JSRuntime.InvokeVoidAsync("downloadFile", $"{Study.Prefix}_export_report_{dateStr}.csv", exportData);
            }
            else
            {
                Snackbar.Add("No data available for export.", Severity.Warning);
            }
        }

        private async Task OnSelectedReport()
        {
            var selectedItems = _displayItems.Where(x => x.IsSelected);
            var selectedIds = selectedItems.Select(p => p.PathReportId).ToList();
            //var Study = await StudyData.GetStudyAsync(StudyId);
            var Study = _study;

            string pathIdsString = string.Join(",", selectedIds.Select(g => g.ToString()));

            var isReport = true;

            var exportData = await PathReportData.ExportPathReportDataAsync(Study.StudyId, "Selected", Guid.Empty, pathIdsString, isReport);

            var dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (exportData != null)
            {
                // Call JavaScript function to download CSV
                await JSRuntime.InvokeVoidAsync("downloadFile", $"{Study.Prefix}_export_report_{dateStr}.csv", exportData);
                IsExported = true;
            }
            else
            {
                IsExported = false;
                Snackbar.Add("No data available for export.", Severity.Warning);
            }
        }


        private async void OnCurrentPageChanged(int page)
        {
            PathGridStateView.CurrentPage = page;

            var storedStateDto = await LocalStorage.GetItemAsync<GridStateDto>(GridStateStorageKey);
            if (storedStateDto == null)
            {
                storedStateDto = new GridStateDto();
            }

            storedStateDto.CurrentPage = page;
            StateHasChanged(); // Update the UI to show the new page number
        }

        private async Task OnStudySelectChanged(Study value)
        {
            _study = value; // Store the selected study for later use
            ShowFields = true; // Show additional fields when a study is selected
            studySelectValue = value;
            studySelectText = value.Name;

            StudyId = value.StudyId; // Update the StudyId for the form

            StudyColor = value.ColorLight; // Update the color based on the selected study
            await InvokeAsync(StateHasChanged);

            // TODO: Add logic to generate the case number based on the selected study
            prefix = value.Prefix;

            StudyColorChanged.InvokeAsync(StudyColor);

            await SetPathReports();

        }

        private async Task SetPathId()
        {
            if (PathReports != null)
            {
                foreach (var item in PathReports)
                {
                    var Study = await StudyData.GetStudyAsync(item.StudyId);
                    item.StudyName = Study.Name;
                    item.StudyPrefix = Study.Prefix;

                    // at their request, only append the PathIndex if greater than 1
                    if (item.PathIndex > 1)
                    {
                        item.PathId = item.CaseNumber + "-" + item.PathIndex;
                    }
                    else
                    {
                        item.PathId = item.CaseNumber;
                    }
                }
            }
        }

        private void OnSelectedItemsChanged(HashSet<PathReportView> selectedItems)
        {
            foreach (var item in checkedPathReports)
            {
                item.IsSelected = selectedItems.Contains(item);
            }
            // You might need to call StateHasChanged() here if the grid doesn't re-render automatically
        }

        async Task<IDialogReference> ViewItem(Patient args, PathReportView pArgs)
        {

            var newPatients = new List<Patient>();

            foreach (var path in pathGrid.FilteredItems)
            {
                var newPatient = await PatientData.GetPatientAsync(path.PatientId);
                newPatients.Add(newPatient);
            }


            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<ViewDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, newPatients); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, pathGrid.FilteredItems.ToList().IndexOf(pArgs)); // Set initial position

            var options = _options;

            return await DialogService.ShowAsync<ViewDialog>("Case View", parameters, options);

        }

        private async Task PathMinAgeCheck(Patient patient)
        {
            //var Study = await StudyData.GetStudyAsync(patient.StudyId);
            var Study = _study; // Use the already loaded study to avoid multiple calls to the database

            foreach (var path in patient.PathReports)
            {
                if (path.ExportStatus == "Path too early")
                {
                    if (Study.IsPathMinAgeValid(path))
                    {
                        path.ExportStatus = "Ready";
                        var userId = await UserData.GetIdByEmailAsync("system_user@system.user"); // System User Id
                        await PatientData.UpdatePatientAsync(patient.PatientId, userId, patient);
                    }
                }
            }
        }

        private async Task<PathReportView> PathMaxAgeCheck(Patient patient, PathReportView pathReportView)
        {
            //var Study = await StudyData.GetStudyAsync(patient.StudyId);
            var Study = _study; // Use the already loaded study to avoid multiple calls to the database

            foreach (var path in patient.PathReports)
            {
                if (path.ExportStatus == "Ready")
                {
                    if (Study.IsPathMaxAgeValid(path))
                    {
                        if (Study.IsPathMaxAgeWeekOut(path))
                        {
                            if (!string.IsNullOrEmpty(Study.PathMaxAge))
                            {
                                pathReportView.MaxAgeDiff = CalculateAgeDays(path.DateOfProcedure.Value, DateTime.Now);
                            }
                            else
                            {
                                pathReportView.MaxAgeDiff = 0;
                            }
                        }
                    }
                }
            }
            return pathReportView;
        }

        private int CalculateAgeDays(DateTime procedureDate, DateTime currentDate)
        {
            TimeSpan difference = currentDate - procedureDate;
            int age = (int)difference.TotalDays;
            return age;
        }

        public async void Dispose()
        {
            Console.WriteLine("Dispose method called for DataGrid component.");

            if (pathGrid != null)
            {
                // Map MudBlazor types to DTOs for serialization
                var filtersToSave = pathGrid.FilterDefinitions
                    .Select(f => new FilterDefinitionDto
                    {
                        // Get the Field name from the Column property of the FilterDefinition
                        Field = f.Column?.PropertyName ?? string.Empty,
                        Operator = f.Operator!,
                        Value = f.Value!.ToString(), // Convert Value to string for serialization
                        BoolValue = f.Value is bool boolValue ? boolValue : null // Handle boolean values
                    })
                    .ToList();

                var sortsToSave = pathGrid.SortDefinitions.Values // Iterate over values of the Dictionary
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
                    var sortColum = pathGrid.RenderedColumns.FirstOrDefault(c => c.PropertyName == sortsToSave.FirstOrDefault()!.SortBy)?.Title ?? string.Empty;
                    PathGridStateView.SortColumn = sortColum;
                }

                // Create a DTO to hold the state
                var stateDto = new GridStateDto
                {
                    Filters = filtersToSave,
                    Sorts = sortsToSave,
                    CurrentPage = pathGrid.CurrentPage, // Save current page
                };

                // Save the DTO to local storage
                await LocalStorage.SetItemAsync(GridStateStorageKey, stateDto);

                PathGridStateView.SortDefinitions = stateDto.Sorts
                    .ToDictionary(s => s.SortBy, s => new SortDefinition<PathReportView>(
                        s.SortBy,
                        s.Descending,
                        s.Index,
                        (Func<PathReportView, object>)s.SortFunc
                    ));



                Console.WriteLine($"State saved to LocalStorage. Filters Count: {filtersToSave.Count}, Sorts Count: {sortsToSave.Count}, Page: {pathGrid.CurrentPage}");
            }
            else
            {
                Console.WriteLine("mudDataGrid was null during Dispose.");
            }
        }

    }
}
