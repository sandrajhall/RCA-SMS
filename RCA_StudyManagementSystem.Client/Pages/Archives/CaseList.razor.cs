using MudBlazor;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.DTOs;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Threading.Tasks;


namespace RCA_StudyManagementSystem.Client.Pages.Archives
{
    public partial class CaseList : Microsoft.AspNetCore.Components.ComponentBase
    {
        private readonly DialogOptions _options = new() { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        private readonly DialogOptions _maxWidth = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

        MudDataGrid<PathReportView>? pathGrid { get; set; }
        MudDataGrid<DoNotContact>? dncGrid { get; set; }

        public IEnumerable<PathReportView>? PathReports { get; set; }
        public IEnumerable<Patient>? Patients { get; set; }

        private IEnumerable<PathReportView>? _displayItems { get; set; }
        private IEnumerable<DoNotContact>? _displayDNC { get; set; }
        private List<PathReportView>? checkedPathReports { get; set; } = new List<PathReportView>();


        private int Index = 0;

        private const string GridStateStorageKey = "ArchivedPathDataGridState"; // Key for local storage


        private List<string> _events = new();
        private CancellationToken token;


        private Func<PathReportView, object> _sortByHold => x =>
        {
            if (x.IsOnHold)
                return "true";
            else
                return "false";
        };

        private string _searchString = string.Empty;
        private string SearchString
        {
            get => _searchString;
            set
            {
                if (_searchString != value)
                {
                    _searchString = value;
                    QuickFilterTextChanged();
                }
            }
        }


        // quick filter - filter globally across multiple columns with the same input
        private Func<PathReportView, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.LastName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.FirstName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true; 
            if (x.MiddleName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.ExportStatus != null && x.ExportStatus.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.CaseNumber != null && x.CaseNumber.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.StudyPrefix.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.StudyName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.DateOfBirth.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
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


        protected override async Task OnInitializedAsync()
        {

            Console.WriteLine("OnInitializedAsync method called!");
            PathReports = await PathReportData.ListArchivedPathReportsAsync();
            //SetPathId(); // Also sets StudyName

            _displayItems = PathReports;

            Patients = await PatientData.ListPatientsAsync();

            await InvokeAsync(StateHasChanged);

            foreach (var item in Patients)
            {
                item.StudyColor = await StudyData.GetStudyColorAsync(item.StudyId);
                //await PathMinAgeCheck(item);  Don't do this in Archive

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
                PathReports = await PathReportData.ListArchivedPathReportsAsync();
                //SetPathId();

                var storedStateDto = await LocalStorage.GetItemAsync<GridStateDto>(GridStateStorageKey);

                if (storedStateDto != null)
                {
                    _searchString = storedStateDto.SearchString ?? ""; // Restore the search string

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

        private async Task ClearStateItems()
        {
            await LocalStorage.ClearAsync();
            await InvokeAsync(StateHasChanged);
            NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
        }

        private void QuickFilterTextChanged()
        {
            if (string.IsNullOrEmpty(SearchString))
            {
                _displayDNC = null;
                //ShowDNC = false;
            }
        }

        // Helper to dynamically get property value for sorting
        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null) ?? string.Empty;
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

        async Task<IDialogReference> ViewItem(Patient args, PathReportView pArgs)
        {

            var newPatients = new List<Patient>();

            foreach (var path in pathGrid.FilteredItems)
            {
                var newPatient = await PatientData.GetPatientAsync(path.PatientId);
                newPatients.Add(newPatient);
            }


            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<CaseViewDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, newPatients); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, pathGrid.FilteredItems.ToList().IndexOf(pArgs)); // Set initial position

            var options = _options;

            return await DialogService.ShowAsync<CaseViewDialog>("Case View", parameters, options);

        }

        private async Task PathMinAgeCheck(Patient patient)
        {
            var Study = await StudyData.GetStudyAsync(patient.StudyId);

            foreach (var path in patient.PathReports)
            {
                if (path.ExportStatus == "Path too early")
                {
                    if (Study.IsPathMinAgeValid(path))
                    {
                        path.ExportStatus = "Ready";
                        var userId = Guid.Empty.ToString();
                        await PatientData.UpdatePatientAsync(patient.PatientId, userId, patient);
                    }
                }
            }
        }

        private async Task<PathReportView> PathMaxAgeCheck(Patient patient, PathReportView pathReportView)
        {
            var Study = await StudyData.GetStudyAsync(patient.StudyId);
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
                    SearchString = _searchString,
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
