using ExcelDataReader;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Pages.Archives;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Client.Utilities;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.DTOs;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace RCA_StudyManagementSystem.Client.Pages.Cases
{
    public partial class List : Microsoft.AspNetCore.Components.ComponentBase
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

        private IEnumerable<Study>? _studyList { get; set; } = new List<Study>();

        private int Index = 0;

        private const string GridStateStorageKey = "PathDataGridState"; // Key for local storage


        private List<string> _events = new();
        private CancellationToken token;

        private bool ShowDNC = false;
        private int _recordLimit = 1; // Default to "Past Week"
        private int? storedView = 1;
        GridStateDto storedStateDto = new GridStateDto();



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
            //CheckDNC(_searchString);
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
            if (x.StudyPrefix != null && x.StudyPrefix.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.StudyName != null && x.StudyName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.MigratedCCRNumber != null && x.MigratedCCRNumber.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
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
            await Task.Delay(300); // Add delay so recordLimit gets set to stored value, not sure why this is needed but it works

            _recordLimit = (int)(storedView != 0 ? storedView : 1);


            switch (_recordLimit)
            {
                case 1:
                    PathReports = await PathReportData.ListPathReportsAsync("Week");
                    break;
                case 2:
                    PathReports = await PathReportData.ListPathReportsAsync("ThreeMonths");
                    break;
                case 3:
                    PathReports = await PathReportData.ListPathReportsAsync("Year");
                    break;
                case 4:
                    PathReports = await PathReportData.ListPathReportsAsync("All");
                    break;
            }

            //SetPathId(); // Do not use for now

            _displayItems = PathReports;

            Patients = await PatientData.ListPatientsAsync();

            await InvokeAsync(StateHasChanged);

            _studyList = await StudyData.ListStudiesAsync();

            foreach (var item in Patients)
            {
                //item.StudyColor = await StudyData.GetStudyColorAsync(item.StudyId);  // Not used for now
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

            storedStateDto = await LocalStorage.GetItemAsync<GridStateDto>(GridStateStorageKey);

            if (firstRender && pathGrid != null)
            {
                if (storedStateDto is not null)
                {
                    storedView = storedStateDto.ViewLimit;
                }
                else
                {
                    storedView = 1;
                }



                _recordLimit = (int)(storedView != 0 ? storedView : _recordLimit); // Use stored view limit if available, otherwise default


                switch (_recordLimit)
                {
                    case 1:
                        PathReports = await PathReportData.ListPathReportsAsync("Week");
                        break;
                    case 2:
                        PathReports = await PathReportData.ListPathReportsAsync("ThreeMonths");
                        break;
                    case 3:
                        PathReports = await PathReportData.ListPathReportsAsync("Year");
                        break;
                    case 4:
                        PathReports = await PathReportData.ListPathReportsAsync("All");
                        break;
                }
                //SetPathId(); // Do not use for now

                StateHasChanged();

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
                                sortedData = PathReports.AsQueryable(); // Start with unsorted data

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
                                _displayItems = PathReports; // If no sorting, revert to original order
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

        private async Task OnRecordLimitChange(int newValue)
        {
            _recordLimit = newValue;

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

            var stateDto = new GridStateDto
            {
                Filters = filtersToSave,
                Sorts = sortsToSave,
                CurrentPage = pathGrid.CurrentPage, // Save current page
                SearchString = _searchString,
                ViewLimit = newValue // Save the current view limit
            };

            // Save the DTO to local storage
            await LocalStorage.SetItemAsync(GridStateStorageKey, stateDto);

            switch (_recordLimit)
            {
                case 1:
                    PathReports = await PathReportData.ListPathReportsAsync("Week");
                    break;
                case 2:
                    PathReports = await PathReportData.ListPathReportsAsync("ThreeMonths");
                    break;
                case 3:
                    PathReports = await PathReportData.ListPathReportsAsync("Year");
                    break;
                case 4:
                    PathReports = await PathReportData.ListPathReportsAsync("All");
                    break;
            }

            _displayItems = PathReports;
            await InvokeAsync(StateHasChanged);
        }

        private async Task ClearStateItems()
        {
            await LocalStorage.ClearAsync();
            //await InvokeAsync(StateHasChanged);
            NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
        }

        private void QuickFilterTextChanged()
        {
            if (string.IsNullOrEmpty(SearchString))
            {
                _displayDNC = null;
                ShowDNC = false;
            }
        }

        private async void CheckDNC(string? searchString)
        {
            _displayDNC = null;
            ShowDNC = false;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var dncList = await DoNotContactData.ListDoNotContactsAsync(token);
                var dncListFiltered = dncList.Where(d => d.MiddleName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                                        || d.FirstName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                                        || d.LastName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                                        || d.DisplayName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                                        || d.DateOfBirth.Value.ToShortDateString().Contains(searchString, StringComparison.OrdinalIgnoreCase)
                                        || d.SocialSecurityNumber.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                _displayDNC = dncListFiltered;
                if (_displayDNC.Any())
                {
                    ShowDNC = true;
                }
                else
                {
                    ShowDNC = false;
                }
            }
            else
            {
                _displayDNC = null;
                ShowDNC = false;
            }
            await InvokeAsync(StateHasChanged);
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

            var parameters = new DialogParameters<ViewDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, newPatients); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, pathGrid.FilteredItems.ToList().IndexOf(pArgs)); // Set initial position

            var options = _options;

            return await DialogService.ShowAsync<ViewDialog>("Case View", parameters, options);

        }

        private async Task PathMinAgeCheck(Patient patient)
        {
            var Study = _studyList.Where(x => x.StudyId == patient.StudyId).FirstOrDefault();

            foreach (var path in patient.PathReports)
            {
                if (path.ExportStatus == "Path too early")
                {
                    if (Study.IsPathMinAgeValid(path))
                    {
                        path.ExportStatus = "Ready";
                        await PatientData.UpdatePatientAsync(patient.PatientId, patient);
                    }
                }
            }
        }

        private async Task<PathReportView> PathMaxAgeCheck(Patient patient, PathReportView pathReportView)
        {
            var Study = _studyList.Where(x => x.StudyId == patient.StudyId).FirstOrDefault();
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
                    ViewLimit = _recordLimit // Save the current view limit
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


        private async Task OnCBCS4Import()
        {
            string fileUrl = "https://localhost:7150/cbcs4-cases.xlsx";
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
                        var PatientList = new List<Patient>();
                        var PatientPhoneList = new List<PatientPhoneNumber>();
                        var PatientPathReportList = new List<PathReport>();

                        if (result.Tables.Count > 0)
                        {
                            var index = 1;

                            DataTable firstSheet = result.Tables[0];
                            foreach (DataRow row in firstSheet.Rows)
                            {
                                Console.WriteLine(string.Join(", ", row.ItemArray));
                                if (row[0]?.ToString() == "CCRNo") // Skip header row
                                    continue;
                                var newPatient = new Patient();
                                newPatient.StudyId = Guid.Parse("889CD103-A5BA-48EA-1168-08DDD04D0C1E");  // CBCS4 StudyId
                                newPatient.MigratedCCRNo = row[0]?.ToString().Trim() ?? string.Empty;
                                newPatient.FirstName = row[2]?.ToString().Trim() ?? string.Empty;
                                newPatient.MiddleName = row[3]?.ToString().Trim() ?? string.Empty;
                                newPatient.LastName = row[1]?.ToString().Trim() ?? string.Empty;
                                newPatient.Suffix = row[4]?.ToString().Trim() ?? string.Empty;
                                newPatient.Address1 = row[5]?.ToString().Trim() ?? string.Empty;
                                newPatient.Address2 = row[6]?.ToString().Trim() ?? string.Empty;
                                newPatient.PatientComments = row[7]?.ToString().Trim() + " ";
                                newPatient.City = row[8]?.ToString().Trim() ?? string.Empty;
                                newPatient.State = row[9]?.ToString().Trim() ?? string.Empty;
                                newPatient.County = await LookupData.GetCountyByFIPSAsync(row[11]?.ToString().Trim()) ?? string.Empty;
                                newPatient.CountyCode = row[11]?.ToString().Trim().Remove(0, 2) ?? string.Empty;
                                newPatient.ZipCode = row[10]?.ToString().Trim() ?? string.Empty;
                                newPatient.DateOfBirth = DateTime.TryParse(row[14]?.ToString().Trim(), out DateTime dob) ? dob : (DateTime?)null;
                                Console.WriteLine("RACECODE: " + row[15]?.ToString().Trim().PadLeft(2, '0'));

                                newPatient.Race = await LookupData.GetTypeByCodeAsync("Race", row[15]?.ToString().Trim().PadLeft(2, '0')) ?? string.Empty;
                                newPatient.RaceCode = row[15]?.ToString().Trim().PadLeft(2, '0') ?? string.Empty;
                                newPatient.Gender = await LookupData.GetTypeByCodeAsync("Gender", row[16]?.ToString().Trim()) ?? string.Empty;
                                newPatient.GenderCode = row[16]?.ToString().Trim() ?? string.Empty;

                                var ethnicity = row[17]?.ToString().Trim();
                                var ethnicityCode = "9";
                                switch (ethnicity)
                                {
                                    case "Hispanic":
                                        ethnicityCode = "6";
                                        break;
                                    case "Non-Hispanic":
                                        ethnicityCode = "0";
                                        break;
                                    case "Unknown":
                                        ethnicityCode = "9";
                                        break;
                                    default:
                                        ethnicityCode = "9";
                                        break;
                                }

                                newPatient.Ethnicity = await LookupData.GetTypeByCodeAsync("Ethnicity", ethnicityCode);
                                newPatient.EthnicityCode = ethnicityCode ?? string.Empty;
                                newPatient.SocialSecurityNumber = row[18]?.ToString().Trim() ?? string.Empty;
                                newPatient.PatientComments += row[19]?.ToString().Trim() ?? string.Empty;
                                newPatient.Email = row[34]?.ToString().Trim() ?? string.Empty;
                                newPatient.PatientComments += row[35]?.ToString().Trim() ?? string.Empty;
                                newPatient.PreferredName = row[36]?.ToString().Trim() ?? string.Empty;
                                newPatient.ModifiedDate = DateTime.TryParse(row[22]?.ToString().Trim(), out DateTime tempDate) ? tempDate : (DateTime?)null;
                                newPatient.DisplayName = $"{newPatient.LastName}, {newPatient.FirstName}";
                                newPatient.IsActive = true;

                                // Initialize the list so it's not null
                                newPatient.PatientPhoneNumbers = new List<PatientPhoneNumber>();

                                if (!string.IsNullOrWhiteSpace(row[12]?.ToString()))
                                {
                                    newPatient.PatientPhoneNumbers.Add(new PatientPhoneNumber
                                    {
                                        PhoneNumber = row[12].ToString().Trim(),
                                        PhoneType = "Other",
                                        IsPrimary = true,
                                        CreatedDate = DateTime.UtcNow
                                    });
                                }

                                if (!string.IsNullOrWhiteSpace(row[13]?.ToString()))
                                {
                                    newPatient.PatientPhoneNumbers.Add(new PatientPhoneNumber
                                    {
                                        PhoneNumber = row[13].ToString().Trim(),
                                        PhoneType = "Other",
                                        IsPrimary = false,
                                        CreatedDate = DateTime.UtcNow
                                    });
                                }

                                //newPatient.PatientPhoneNumbers = PatientPhoneList;


                                PatientPhoneList = new List<PatientPhoneNumber>();

                                var newPath = new PathReport();
                                newPath.MigratedCCRNumber = newPatient.MigratedCCRNo;
                                newPath.AgeAtProcedure = "0";
                                newPath.AuthorizingProvider = "Placeholder";
                                newPath.SubmittingHospital = "UNC Health Care";
                                newPath.HospCity = "Chapel Hill";
                                newPath.SubmittingHospitalPathReportNumber = "Placeholder" + index.ToString();
                                newPath.DateOfProcedure = DateTime.Now;
                                newPath.Site = "Placeholder";
                                newPath.PathProcedure = "Placeholder";
                                newPath.HistologyDiagnosis1 = "Placeholder";
                                newPath.HistologyCode1 = "0";
                                newPath.HistologyBehavior1 = "0";
                                newPath.HospAddress1 = "Placeholder";
                                newPath.PathIndex = 1;

                                PatientPathReportList.Add(newPath);

                                newPatient.PathReports = PatientPathReportList;

                                PatientPathReportList = new List<PathReport>();


                                PatientList.Add(newPatient);
                                index++;
                            }

                            // Add the new patients to the database
                            foreach (var patient in PatientList)
                            {
                                patient.CaseNumber = await GenerateCaseNumber.Generate("CBCS4");
                                var id = await PatientData.CreatePatientAsync(patient);
                            }
                        }
                    }
                }

            }
        }

        private async Task OnCBCS4PathImport()
        {
            string fileUrl = "https://localhost:7150/cbcs4-pathreports.xlsx";
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

                        var PathReportList = new List<PathReport>();

                        if (result.Tables.Count > 0)
                        {
                            var index = 1;

                            DataTable firstSheet = result.Tables[0];
                            foreach (DataRow row in firstSheet.Rows)
                            {
                                Console.WriteLine(string.Join(", ", row.ItemArray));
                                if (row[0]?.ToString() == "CCRPathRptKey") // Skip header row
                                    continue;
                                var newPath = new PathReport();
                                newPath.PathIndex = 1;
                                newPath.MigratedCCRNumber = row[1]?.ToString().Trim() ?? string.Empty;
                                newPath.PatientId = await PatientData.GetPatientIdByCCRNoAsync(newPath.MigratedCCRNumber);
                                await Task.Delay(50);

                                var study = await StudyData.GetStudyAsync(Guid.Parse("889CD103-A5BA-48EA-1168-08DDD04D0C1E"));  // CBCS4 StudyId
                                newPath.StudyPrefix = study.Prefix;
                                newPath.StudyColor = study.ColorLight;

                                var patient = await PatientData.GetPatientAsync(newPath.PatientId);
                                await Task.Delay(50);
                                newPath.CaseNumber = patient.CaseNumber;
                                newPath.DxAddress1 = patient != null ? patient.Address1 : "Unknown";
                                newPath.DxAddress2 = patient != null ? patient.Address2 : "Unknown";
                                newPath.DxCity = patient != null ? patient.City : "Unknown";
                                newPath.DxState = patient != null ? patient.State : "Unknown";
                                newPath.DxZipCode = patient != null ? patient.ZipCode : "Unknown";
                                newPath.DxPhoneNumber = await PatientData.GetPatientPrimaryPhoneAsync(newPath.PatientId);
                                newPath.DxCounty = patient != null ? patient.County : "Unknown";
                                newPath.DxCountyCode = patient != null ? patient.CountyCode : "Unknown";

                                newPath.DateOfProcedure = DateTime.TryParse(row[4]?.ToString().Trim(), out DateTime dop) ? dop : (DateTime?)null;
                                newPath.AgeAtProcedure = row[5]?.ToString().Trim() ?? string.Empty;

                                var hospital = await HospitalData.GetHospitalByMigratedIdAsync(row[2]?.ToString().Trim().TrimStart('0') ?? string.Empty);
                                await Task.Delay(50);
                                if(hospital.IsDuplicate)
                                {
                                    hospital = await HospitalData.GetHospitalByMigratedIdAsync(hospital.DuplicateOfHospitalId);
                                }
                                newPath.HospitalId = hospital != null ? hospital.HospitalId : Guid.Empty;
                                newPath.SubmittingHospital = hospital != null ? hospital.HospitalName : "Unknown Hospital";
                                newPath.HospAddress1 = hospital != null ? hospital.Address1 : "Unknown";
                                newPath.HospAddress2 = hospital != null ? hospital.Address2 : "Unknown";
                                newPath.HospCity = hospital != null ? hospital.City : "Unknown";
                                newPath.HospState = hospital != null ? hospital.State : "Unknown";
                                newPath.HospZipCode = hospital != null ? hospital.ZipCode : "Unknown";
                                newPath.HospPhoneNumber = hospital != null ? hospital.PhoneNumber : "Unknown";
                                newPath.HospFaxNumber = hospital != null ? hospital.FaxNumber : "Unknown";

                                newPath.SubmittingHospitalPathReportNumber = row[6]?.ToString().Trim() ?? string.Empty;

                                var doctor = await DoctorData.GetDoctorByMigratedIdAsync(row[7]?.ToString().Trim());
                                await Task.Delay(50);
                                if(doctor.IsDuplicate)
                                {
                                    doctor = await DoctorData.GetDoctorByMigratedIdAsync(doctor.DuplicateOfDoctorId);
                                }
                                newPath.AuthorizingProvider = doctor != null ? doctor.DisplayName : "Unknown Provider";
                                newPath.DoctorId = doctor != null ? doctor.DoctorId : Guid.Empty;
                                newPath.MDAddress1 = doctor != null ? doctor.Address1 : "Unknown";
                                newPath.MDAddress2 = doctor != null ? doctor.Address2 : "Unknown";
                                newPath.MD2Address3 = doctor != null ? doctor.Address2 : "Unknown";
                                newPath.MDCity = doctor != null ? doctor.City : "Unknown";
                                newPath.MDState = doctor != null ? doctor.State : "Unknown";
                                newPath.MDZipCode = doctor != null ? doctor.ZipCode : "Unknown";
                                newPath.MDCounty = doctor != null ? doctor.County : "Unknown";
                                newPath.MDPhoneNumber1 = doctor != null ? doctor.PhoneNumber1 : "Unknown";
                                newPath.MDPhoneNumber2 = doctor != null ? doctor.PhoneNumber2 : "Unknown";
                                newPath.MDFaxNumber = doctor != null ? doctor.FaxNumber : "Unknown";
                                newPath.MDEmail = doctor != null ? doctor.Email : "Unknown";

                                newPath.AuthorizingProviderComments = row[55]?.ToString().Trim() ?? string.Empty;

                                var studyId = Guid.Parse("889CD103-A5BA-48EA-1168-08DDD04D0C1E");  // CBCS4 StudyId
                                if (!string.IsNullOrEmpty(row[10]?.ToString().Trim()))
                                {
                                    var site1 = await StudyLookupData.GetValueByOldCodeAsync(studyId, "Site", Int32.Parse(row[10]?.ToString().Trim()));
                                    await Task.Delay(50);
                                    newPath.Site = site1 ?? "Unknown Site";
                                    newPath.SiteCode = await StudyLookupData.GetCodeByValueAsync(studyId, "Site", site1);
                                    await Task.Delay(50);
                                }
                                else
                                {
                                    newPath.Site = "Unknown Site";
                                    newPath.SiteCode = string.Empty;
                                }
                                if (!string.IsNullOrEmpty(row[13]?.ToString().Trim()))
                                {
                                    var site2 = await StudyLookupData.GetValueByOldCodeAsync(studyId, "Site", Int32.Parse(row[13]?.ToString().Trim()));
                                    await Task.Delay(50);
                                    newPath.Site2 = site2 ?? "Unknown Site";
                                    newPath.SiteCode2 = await StudyLookupData.GetCodeByValueAsync(studyId, "Site", site2);
                                    await Task.Delay(50);

                                    newPath.AuthorizingProvider2 = newPath.AuthorizingProvider;
                                    newPath.Doctor2Id = newPath.DoctorId;
                                    newPath.MD2Address1 = newPath.MDAddress1;
                                    newPath.MD2Address2 = newPath.MDAddress2;
                                    newPath.MD2Address3 = newPath.MD2Address2;
                                    newPath.MD2City = newPath.MDCity;
                                    newPath.MD2State = newPath.MDState;
                                    newPath.MD2ZipCode = newPath.MDZipCode;
                                    newPath.MD2County = newPath.MDCounty;
                                    newPath.MD2PhoneNumber1 = newPath.MDPhoneNumber1;
                                    newPath.MD2PhoneNumber2 = newPath.MDPhoneNumber2;
                                    newPath.MD2FaxNumber = newPath.MDFaxNumber;
                                    newPath.MD2Email = newPath.MDEmail;
                                }
                                else
                                {
                                    newPath.Site2 = string.Empty;
                                    newPath.SiteCode2 = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(row[8]?.ToString().Trim()))
                                {
                                    var proc1 = await StudyLookupData.GetValueByOldCodeAsync(studyId, "Procedure", Int32.Parse(row[8]?.ToString().Trim()));
                                    await Task.Delay(50);
                                    newPath.PathProcedure = proc1 ?? "Unknown Procedure";
                                }
                                else
                                {
                                    newPath.PathProcedure = "Unknown Procedure";
                                }
                                if (!string.IsNullOrEmpty(row[12]?.ToString().Trim()))
                                {
                                    var proc2 = await StudyLookupData.GetValueByOldCodeAsync(studyId, "Procedure", Int32.Parse(row[12]?.ToString().Trim()));
                                    await Task.Delay(50);
                                    newPath.PathProcedure2 = proc2 ?? "Unknown Procedure";
                                }
                                else
                                {
                                    newPath.PathProcedure2 = string.Empty;
                                }

                                newPath.PathComments = row[9]?.ToString().Trim() ?? string.Empty;
                                newPath.PathComments2 = row[11]?.ToString().Trim() ?? string.Empty;
                                newPath.IsOutsidePathReport = row[15]?.ToString().Trim().ToLower() == "y" ? false : true;
                                newPath.SlidesResideAtSubmittingHospital = row[15]?.ToString().Trim().ToLower() == "y" ? "Yes" : "No";

                                if (!string.IsNullOrEmpty(row[16].ToString()))
                                {
                                    var outsideHospital = await HospitalData.GetHospitalByMigratedIdAsync(row[16].ToString().Trim().TrimStart('0'));
                                    await Task.Delay(50);
                                    if (outsideHospital.IsDuplicate)
                                    {
                                        outsideHospital = await HospitalData.GetHospitalByMigratedIdAsync(outsideHospital.DuplicateOfHospitalId);
                                    }
                                    newPath.OriginatingHospitalName = outsideHospital.HospitalName ?? "Unknown";
                                    newPath.OrigHospitalId = outsideHospital.HospitalId;
                                    newPath.OrigHospAddress1 = outsideHospital.Address1;
                                    newPath.OrigHospAddress2 = outsideHospital.Address2;
                                    newPath.OrigHospCity = outsideHospital.City;
                                    newPath.OrigHospState = outsideHospital.State;
                                    newPath.OrigHospZipCode = outsideHospital.ZipCode;
                                    newPath.OrigHospPhoneNumber = outsideHospital.PhoneNumber;
                                    newPath.OrigHospFaxNumber = outsideHospital.FaxNumber;

                                }

                                newPath.OriginatingHospitalPathReportNumber = row[18]?.ToString().Trim() ?? string.Empty;
                                newPath.OriginatingHospitalComments = row[19]?.ToString().Trim() ?? string.Empty;
                                newPath.IsOnHold = row[34]?.ToString().Trim().ToLower() == "true" ? true : false;
                                newPath.CreatedDate = row[35]?.ToString().Trim() != null ? DateTime.Parse(row[35]?.ToString().Trim()) : DateTime.UtcNow;
                                if (string.IsNullOrEmpty(row[38]?.ToString().Trim()))
                                {
                                    newPath.RcaExportDate = null;
                                    newPath.ExportStatus = "Unknown";
                                }
                                else
                                {
                                    newPath.RcaExportDate = DateTime.Parse(row[38]?.ToString().Trim());
                                    newPath.ExportStatus = "Exported";
                                }

                                if (!string.IsNullOrEmpty(row[49].ToString().Trim()))
                                {
                                    var histCode = Int32.Parse(row[49].ToString().Trim());
                                    if (histCode == 111)
                                    {
                                        histCode = 107; // map code due to duplicate entries in lookup
                                    }
                                    if (histCode == 110)
                                    {
                                        histCode = 109; // map code due to duplicate entries in lookup
                                    }
                                    var histology = await StudyHistologyData.GetValueByOldCodeAsync(studyId, histCode);
                                    await Task.Delay(50);
                                    newPath.HistologyDiagnosis1 = histology.HistologyName;
                                    newPath.HistologyCode1 = histology.HistologyCode;
                                    newPath.HistologyBehavior1 = histology.HistologyBehavior;
                                }

                                newPath.HistologyDiagnosisComments1 = row[50]?.ToString().Trim() ?? string.Empty;

                                if (!string.IsNullOrEmpty(row[51].ToString().Trim()))
                                {
                                    var histCode = Int32.Parse(row[51].ToString().Trim());
                                    if (histCode == 111)
                                    {
                                        histCode = 107; // map code due to duplicate entries in lookup
                                    }
                                    if (histCode == 110)
                                    {
                                        histCode = 109; // map code due to duplicate entries in lookup
                                    }
                                    var histology = await StudyHistologyData.GetValueByOldCodeAsync(studyId, histCode);
                                    await Task.Delay(50);
                                    newPath.HistologyDiagnosis2 = histology.HistologyName;
                                    newPath.HistologyCode2 = histology.HistologyCode;
                                    newPath.HistologyBehavior2 = histology.HistologyBehavior;
                                }

                                newPath.HistologyDiagnosisComments2 = row[52]?.ToString().Trim() ?? string.Empty;

                                if (!string.IsNullOrEmpty(row[53]?.ToString().Trim()))
                                {
                                    var ReimbursementHospital1 = await HospitalData.GetHospitalByMigratedIdAsync(row[53]?.ToString().Trim().TrimStart('0'));
                                    await Task.Delay(50);
                                    if (ReimbursementHospital1.IsDuplicate)
                                    {
                                        ReimbursementHospital1 = await HospitalData.GetHospitalByMigratedIdAsync(ReimbursementHospital1.DuplicateOfHospitalId);
                                    }
                                    newPath.Reimbursement1 = ReimbursementHospital1.HospitalName;
                                }

                                if (!string.IsNullOrEmpty(row[54]?.ToString().Trim()))
                                {
                                    var ReimbursementHospital2 = await HospitalData.GetHospitalByMigratedIdAsync(row[54]?.ToString().Trim().TrimStart('0'));
                                    await Task.Delay(50);
                                    if (ReimbursementHospital2.IsDuplicate)
                                    {
                                        ReimbursementHospital2 = await HospitalData.GetHospitalByMigratedIdAsync(ReimbursementHospital2.DuplicateOfHospitalId);
                                    }
                                    newPath.Reimbursement2 = ReimbursementHospital2.HospitalName;
                                }


                                PathReportList.Add(newPath);
                            }

                            // Add the new paths to the database
                            foreach (var path in PathReportList)
                            {
                                var id = await PathReportData.CreatePathReportAsync(path);
                            }

                            // Delete the placeholder pathreports
                            await PathReportData.DeletePlaceholderPathReportsAsync();
                            await PatientData.ClearCCRNosAsync();

                        }
                    }

                }
            }

        }

        private async Task OnCECSImport()
        {
            string fileUrl = "https://localhost:7150/cecs-cases.xlsx";
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
                        var PatientList = new List<Patient>();
                        var PatientPhoneList = new List<PatientPhoneNumber>();
                        var PatientPathReportList = new List<PathReport>();

                        if (result.Tables.Count > 0)
                        {
                            var index = 1;

                            DataTable firstSheet = result.Tables[0];
                            foreach (DataRow row in firstSheet.Rows)
                            {
                                Console.WriteLine(string.Join(", ", row.ItemArray));
                                if (row[0]?.ToString() == "CCRNo") // Skip header row
                                    continue;
                                var newPatient = new Patient();
                                newPatient.StudyId = Guid.Parse("631B71B4-B451-4FD7-8B82-08DDD0513E23");  // CECS StudyId
                                newPatient.MigratedCCRNo = row[0]?.ToString().Trim() ?? string.Empty;
                                newPatient.FirstName = row[2]?.ToString().Trim() ?? string.Empty;
                                newPatient.MiddleName = row[3]?.ToString().Trim() ?? string.Empty;
                                newPatient.LastName = row[1]?.ToString().Trim() ?? string.Empty;
                                newPatient.Suffix = row[4]?.ToString().Trim() ?? string.Empty;
                                newPatient.Address1 = row[5]?.ToString().Trim() ?? string.Empty;
                                newPatient.Address2 = row[6]?.ToString().Trim() ?? string.Empty;
                                newPatient.PatientComments = row[7]?.ToString().Trim() + " ";
                                newPatient.City = row[8]?.ToString().Trim() ?? string.Empty;
                                newPatient.State = row[9]?.ToString().Trim() ?? string.Empty;
                                newPatient.County = await LookupData.GetCountyByFIPSAsync(row[11]?.ToString().Trim()) ?? string.Empty;
                                newPatient.CountyCode = row[11]?.ToString().Trim().Remove(0, 2) ?? string.Empty;
                                newPatient.ZipCode = row[10]?.ToString().Trim() ?? string.Empty;
                                newPatient.DateOfBirth = DateTime.TryParse(row[14]?.ToString().Trim(), out DateTime dob) ? dob : (DateTime?)null;
                                Console.WriteLine("RACECODE: " + row[15]?.ToString().Trim().PadLeft(2, '0'));

                                var race = row[15]?.ToString().Trim().PadLeft(2, '0');
                                var raceCode = "";
                                switch (race)
                                {
                                    case "01":
                                        raceCode = "01";
                                        break;
                                    case "02":
                                        raceCode = "02";
                                        break;
                                    case "03":
                                        raceCode = "03";
                                        break;
                                    case "04":
                                        raceCode = "96";
                                        break;
                                    case "05":
                                        raceCode = "98";
                                        break;
                                    case "06":
                                        raceCode = "07";
                                        break;
                                    case "08":
                                        raceCode = "98";
                                        break;
                                    case "09":
                                        raceCode = "99";
                                        break;
                                    case "10":
                                        raceCode = "98";
                                        break;
                                    default:
                                        raceCode = "99";
                                        break;
                                }

                                newPatient.Race = await LookupData.GetTypeByCodeAsync("Race", raceCode) ?? string.Empty;
                                newPatient.RaceCode = raceCode ?? string.Empty;
                                newPatient.Gender = await LookupData.GetTypeByCodeAsync("Gender", row[16]?.ToString().Trim()) ?? string.Empty;
                                newPatient.GenderCode = row[16]?.ToString().Trim() ?? string.Empty;

                                var ethnicity = row[29]?.ToString().Trim();
                                var ethnicityCode = "9";
                                switch (ethnicity)
                                {
                                    case "Hispanic":
                                        ethnicityCode = "6";
                                        break;
                                    case "Non-Hispanic":
                                        ethnicityCode = "0";
                                        break;
                                    case "Unknown":
                                        ethnicityCode = "9";
                                        break;
                                    default:
                                        ethnicityCode = "9";
                                        break;
                                }

                                newPatient.Ethnicity = await LookupData.GetTypeByCodeAsync("Ethnicity", ethnicityCode);
                                newPatient.EthnicityCode = ethnicityCode ?? string.Empty;
                                newPatient.SocialSecurityNumber = row[17]?.ToString().Trim() ?? string.Empty;
                                newPatient.PatientComments += row[18]?.ToString().Trim() ?? string.Empty;
                                newPatient.Email = row[38]?.ToString().Trim() ?? string.Empty;
                                newPatient.PatientComments += row[36]?.ToString().Trim() ?? string.Empty;
                                newPatient.PreferredName = row[35]?.ToString().Trim() ?? string.Empty;
                                newPatient.ModifiedDate = DateTime.TryParse(row[21]?.ToString().Trim(), out DateTime tempDate) ? tempDate : (DateTime?)null;
                                newPatient.DisplayName = $"{newPatient.LastName}, {newPatient.FirstName}";
                                newPatient.IsActive = true;

                                // Initialize the list so it's not null
                                newPatient.PatientPhoneNumbers = new List<PatientPhoneNumber>();

                                if (!string.IsNullOrWhiteSpace(row[12]?.ToString()))
                                {
                                    newPatient.PatientPhoneNumbers.Add(new PatientPhoneNumber
                                    {
                                        PhoneNumber = row[12].ToString().Trim(),
                                        PhoneType = "Other",
                                        IsPrimary = true,
                                        CreatedDate = DateTime.UtcNow
                                    });
                                }

                                if (!string.IsNullOrWhiteSpace(row[13]?.ToString()))
                                {
                                    newPatient.PatientPhoneNumbers.Add(new PatientPhoneNumber
                                    {
                                        PhoneNumber = row[13].ToString().Trim(),
                                        PhoneType = "Other",
                                        IsPrimary = false,
                                        CreatedDate = DateTime.UtcNow
                                    });
                                }

                                //newPatient.PatientPhoneNumbers = PatientPhoneList;


                                PatientPhoneList = new List<PatientPhoneNumber>();

                                var newPath = new PathReport();
                                newPath.MigratedCCRNumber = newPatient.MigratedCCRNo;
                                newPath.AgeAtProcedure = "0";
                                newPath.AuthorizingProvider = "Placeholder";
                                newPath.SubmittingHospital = "UNC Health Care";
                                newPath.HospCity = "Chapel Hill";
                                newPath.SubmittingHospitalPathReportNumber = "Placeholder" + index.ToString();
                                newPath.DateOfProcedure = DateTime.Now;
                                newPath.Site = "Placeholder";
                                newPath.PathProcedure = "Placeholder";
                                newPath.HistologyDiagnosis1 = "Placeholder";
                                newPath.HistologyCode1 = "0";
                                newPath.HistologyBehavior1 = "0";
                                newPath.HospAddress1 = "Placeholder";
                                newPath.PathIndex = 1;

                                PatientPathReportList.Add(newPath);

                                newPatient.PathReports = PatientPathReportList;

                                PatientPathReportList = new List<PathReport>();


                                PatientList.Add(newPatient);
                                index++;
                            }

                            // Add the new patients to the database
                            foreach (var patient in PatientList)
                            {
                                patient.CaseNumber = await GenerateCaseNumber.Generate("CECS");
                                var id = await PatientData.CreatePatientAsync(patient);
                            }
                        }
                    }
                }

            }
        }

        private async Task OnCECSPathImport()
        {
            string fileUrl = "https://localhost:7150/cecs-pathreports.xlsx";
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

                        var PathReportList = new List<PathReport>();

                        if (result.Tables.Count > 0)
                        {
                            var index = 1;

                            DataTable firstSheet = result.Tables[0];
                            foreach (DataRow row in firstSheet.Rows)
                            {
                                Console.WriteLine(string.Join(", ", row.ItemArray));
                                if (row[0]?.ToString() == "CCRPathRptKey") // Skip header row
                                    continue;
                                var newPath = new PathReport();
                                newPath.PathIndex = 1;
                                newPath.MigratedCCRNumber = row[1]?.ToString().Trim() ?? string.Empty;
                                newPath.PatientId = await PatientData.GetPatientIdByCCRNoAsync(newPath.MigratedCCRNumber);
                                await Task.Delay(50);

                                var study = await StudyData.GetStudyAsync(Guid.Parse("631B71B4-B451-4FD7-8B82-08DDD0513E23"));  // CECS StudyId
                                newPath.StudyPrefix = study.Prefix;
                                newPath.StudyColor = study.ColorLight;

                                var patient = await PatientData.GetPatientAsync(newPath.PatientId);
                                await Task.Delay(50);
                                newPath.CaseNumber = patient.CaseNumber;
                                newPath.DxAddress1 = patient != null ? patient.Address1 : "Unknown";
                                newPath.DxAddress2 = patient != null ? patient.Address2 : "Unknown";
                                newPath.DxCity = patient != null ? patient.City : "Unknown";
                                newPath.DxState = patient != null ? patient.State : "Unknown";
                                newPath.DxZipCode = patient != null ? patient.ZipCode : "Unknown";
                                newPath.DxPhoneNumber = await PatientData.GetPatientPrimaryPhoneAsync(newPath.PatientId);
                                newPath.DxCounty = patient != null ? patient.County : "Unknown";
                                newPath.DxCountyCode = patient != null ? patient.CountyCode : "Unknown";

                                newPath.DateOfProcedure = DateTime.TryParse(row[4]?.ToString().Trim(), out DateTime dop) ? dop : (DateTime?)null;
                                newPath.AgeAtProcedure = row[5]?.ToString().Trim() ?? string.Empty;

                                var hospital = await HospitalData.GetHospitalByMigratedIdAsync(row[2]?.ToString().Trim().TrimStart('0') ?? string.Empty);
                                await Task.Delay(50);
                                if (hospital.IsDuplicate)
                                {
                                    hospital = await HospitalData.GetHospitalByMigratedIdAsync(hospital.DuplicateOfHospitalId);
                                }
                                newPath.HospitalId = hospital != null ? hospital.HospitalId : Guid.Empty;
                                newPath.SubmittingHospital = hospital != null ? hospital.HospitalName : "Unknown Hospital";
                                newPath.HospAddress1 = hospital != null ? hospital.Address1 : "Unknown";
                                newPath.HospAddress2 = hospital != null ? hospital.Address2 : "Unknown";
                                newPath.HospCity = hospital != null ? hospital.City : "Unknown";
                                newPath.HospState = hospital != null ? hospital.State : "Unknown";
                                newPath.HospZipCode = hospital != null ? hospital.ZipCode : "Unknown";
                                newPath.HospPhoneNumber = hospital != null ? hospital.PhoneNumber : "Unknown";
                                newPath.HospFaxNumber = hospital != null ? hospital.FaxNumber : "Unknown";

                                newPath.SubmittingHospitalPathReportNumber = row[6]?.ToString().Trim() ?? string.Empty;
                                var dup = await PathReportData.CheckPathReportNumberAsync(newPath.SubmittingHospitalPathReportNumber);
                                if (dup == "true")
                                    newPath.SubmittingHospitalPathReportNumber = newPath.SubmittingHospitalPathReportNumber + "-1";

                                var doctor = await DoctorData.GetDoctorByMigratedIdAsync(row[7]?.ToString().Trim());
                                await Task.Delay(50);
                                if (doctor.IsDuplicate)
                                {
                                    doctor = await DoctorData.GetDoctorByMigratedIdAsync(doctor.DuplicateOfDoctorId);
                                }
                                newPath.AuthorizingProvider = doctor != null ? doctor.DisplayName : "Unknown Provider";
                                newPath.DoctorId = doctor != null ? doctor.DoctorId : Guid.Empty;
                                newPath.MDAddress1 = doctor != null ? doctor.Address1 : "Unknown";
                                newPath.MDAddress2 = doctor != null ? doctor.Address2 : "Unknown";
                                newPath.MD2Address3 = doctor != null ? doctor.Address2 : "Unknown";
                                newPath.MDCity = doctor != null ? doctor.City : "Unknown";
                                newPath.MDState = doctor != null ? doctor.State : "Unknown";
                                newPath.MDZipCode = doctor != null ? doctor.ZipCode : "Unknown";
                                newPath.MDCounty = doctor != null ? doctor.County : "Unknown";
                                newPath.MDPhoneNumber1 = doctor != null ? doctor.PhoneNumber1 : "Unknown";
                                newPath.MDPhoneNumber2 = doctor != null ? doctor.PhoneNumber2 : "Unknown";
                                newPath.MDFaxNumber = doctor != null ? doctor.FaxNumber : "Unknown";
                                newPath.MDEmail = doctor != null ? doctor.Email : "Unknown";

                                newPath.AuthorizingProviderComments = row[37]?.ToString().Trim() ?? string.Empty;

                                var pathologist = await DoctorData.GetDoctorByMigratedIdAsync(row[8]?.ToString().Trim());
                                await Task.Delay(50);
                                if (pathologist.IsDuplicate)
                                {
                                    pathologist = await DoctorData.GetDoctorByMigratedIdAsync(pathologist.DuplicateOfDoctorId);
                                }
                                newPath.Pathologist = pathologist != null ? pathologist.DisplayName : "Unknown Pathologist";
                                newPath.PathologistId = pathologist != null ? pathologist.DoctorId : Guid.Empty;
                                newPath.PathAddress1 = pathologist != null ? pathologist.Address1 : "Unknown";
                                newPath.PathAddress2 = pathologist != null ? pathologist.Address2 : "Unknown";
                                newPath.PathAddress3 = pathologist != null ? pathologist.Address2 : "Unknown";
                                newPath.PathCity = pathologist != null ? pathologist.City : "Unknown";
                                newPath.PathState = pathologist != null ? pathologist.State : "Unknown";
                                newPath.PathZipCode = pathologist != null ? pathologist.ZipCode : "Unknown";
                                newPath.PathCounty = pathologist != null ? pathologist.County : "Unknown";
                                newPath.PathPhoneNumber1 = pathologist != null ? pathologist.PhoneNumber1 : "Unknown";
                                newPath.PathPhoneNumber2 = pathologist != null ? pathologist.PhoneNumber2 : "Unknown";
                                newPath.PathFaxNumber = pathologist != null ? pathologist.FaxNumber : "Unknown";
                                newPath.PathEmail = pathologist != null ? pathologist.Email : "Unknown";

                                newPath.PathologistComments = row[38]?.ToString().Trim() ?? string.Empty;


                                var studyId = Guid.Parse("631B71B4-B451-4FD7-8B82-08DDD0513E23");  // CECS StudyId
                                if (!string.IsNullOrEmpty(row[11]?.ToString().Trim()))
                                {
                                    var site1 = await StudyLookupData.GetValueByOldCodeAsync(studyId, "Site", Int32.Parse(row[11]?.ToString().Trim()));
                                    await Task.Delay(50);
                                    newPath.Site = site1 ?? "Unknown Site";
                                    newPath.SiteCode = await StudyLookupData.GetCodeByValueAsync(studyId, "Site", site1);
                                    await Task.Delay(50);
                                }
                                else
                                {
                                    newPath.Site = "Unknown Site";
                                    newPath.SiteCode = string.Empty;
                                }


                                if (!string.IsNullOrEmpty(row[9]?.ToString().Trim()))
                                {
                                    var proc1 = await StudyLookupData.GetValueByOldCodeAsync(studyId, "Procedure", Int32.Parse(row[9]?.ToString().Trim()));
                                    await Task.Delay(50);
                                    newPath.PathProcedure = proc1 ?? "Unknown Procedure";
                                }
                                else
                                {
                                    newPath.PathProcedure = "Unknown Procedure";
                                }


                                newPath.PathComments = row[10]?.ToString().Trim() ?? string.Empty;
                                newPath.IsOutsidePathReport = row[13]?.ToString().Trim().ToLower() == "n" ? false : true;
                                newPath.SlidesResideAtSubmittingHospital = row[13]?.ToString().Trim().ToLower() == "n" ? "Yes" : "No";

                                if (!string.IsNullOrEmpty(row[14].ToString()))
                                {
                                    var outsideHospital = await HospitalData.GetHospitalByMigratedIdAsync(row[14].ToString().Trim().TrimStart('0'));
                                    await Task.Delay(50);
                                    if (outsideHospital.IsDuplicate)
                                    {
                                        outsideHospital = await HospitalData.GetHospitalByMigratedIdAsync(outsideHospital.DuplicateOfHospitalId);
                                    }
                                    newPath.OriginatingHospitalName = outsideHospital.HospitalName ?? "Unknown";
                                    newPath.OrigHospitalId = outsideHospital.HospitalId;
                                    newPath.OrigHospAddress1 = outsideHospital.Address1;
                                    newPath.OrigHospAddress2 = outsideHospital.Address2;
                                    newPath.OrigHospCity = outsideHospital.City;
                                    newPath.OrigHospState = outsideHospital.State;
                                    newPath.OrigHospZipCode = outsideHospital.ZipCode;
                                    newPath.OrigHospPhoneNumber = outsideHospital.PhoneNumber;
                                    newPath.OrigHospFaxNumber = outsideHospital.FaxNumber;

                                }

                                newPath.OriginatingHospitalPathReportNumber = row[16]?.ToString().Trim() ?? string.Empty;
                                newPath.PathComments += " " + row[22]?.ToString().Trim() ?? string.Empty;
                                newPath.OriginatingHospitalComments = row[28]?.ToString().Trim() ?? string.Empty;
                                newPath.IsOnHold = row[23]?.ToString().Trim().ToLower() == "true" ? true : false;
                                newPath.CreatedDate = row[24]?.ToString().Trim() != null ? DateTime.Parse(row[24]?.ToString().Trim()) : DateTime.UtcNow;
                                if (string.IsNullOrEmpty(row[27]?.ToString().Trim()))
                                {
                                    newPath.RcaExportDate = null;
                                    newPath.ExportStatus = "Unknown";
                                }
                                else
                                {
                                    newPath.RcaExportDate = DateTime.Parse(row[27]?.ToString().Trim());
                                    newPath.ExportStatus = "Exported";
                                }

                                if (!string.IsNullOrEmpty(row[29].ToString().Trim()))
                                {
                                    var histCode = Int32.Parse(row[29].ToString().Trim());
                                    if (histCode == 111)
                                    {
                                        histCode = 107; // map code due to duplicate entries in lookup
                                    }
                                    if (histCode == 110)
                                    {
                                        histCode = 109; // map code due to duplicate entries in lookup
                                    }
                                    var histology = await StudyHistologyData.GetValueByOldCodeAsync(studyId, histCode);
                                    await Task.Delay(50);
                                    newPath.HistologyDiagnosis1 = histology.HistologyName;
                                    newPath.HistologyCode1 = histology.HistologyCode;
                                    newPath.HistologyBehavior1 = histology.HistologyBehavior;
                                }

                                newPath.HistologyDiagnosisComments1 = row[30]?.ToString().Trim() ?? string.Empty;

                                if (!string.IsNullOrEmpty(row[31].ToString().Trim()))
                                {
                                    var histCode = Int32.Parse(row[31].ToString().Trim());
                                    if (histCode == 111)
                                    {
                                        histCode = 107; // map code due to duplicate entries in lookup
                                    }
                                    if (histCode == 110)
                                    {
                                        histCode = 109; // map code due to duplicate entries in lookup
                                    }
                                    var histology = await StudyHistologyData.GetValueByOldCodeAsync(studyId, histCode);
                                    await Task.Delay(50);
                                    newPath.HistologyDiagnosis2 = histology.HistologyName;
                                    newPath.HistologyCode2 = histology.HistologyCode;
                                    newPath.HistologyBehavior2 = histology.HistologyBehavior;
                                }

                                newPath.HistologyDiagnosisComments2 = row[32]?.ToString().Trim() ?? string.Empty;

                                newPath.IsAddendum = row[33]?.ToString().Trim().ToLower() == "n" ? false : true;

                                newPath.PathComments += " " + row[34]?.ToString().Trim() ?? string.Empty;

                                if (!string.IsNullOrEmpty(row[35]?.ToString().Trim()))
                                {
                                    var ReimbursementHospital1 = await HospitalData.GetHospitalByMigratedIdAsync(row[35]?.ToString().Trim().TrimStart('0'));
                                    await Task.Delay(50);
                                    if (ReimbursementHospital1.IsDuplicate)
                                    {
                                        ReimbursementHospital1 = await HospitalData.GetHospitalByMigratedIdAsync(ReimbursementHospital1.DuplicateOfHospitalId);
                                    }
                                    newPath.Reimbursement1 = ReimbursementHospital1.HospitalName;
                                }

                                if (!string.IsNullOrEmpty(row[36]?.ToString().Trim()))
                                {
                                    var ReimbursementHospital2 = await HospitalData.GetHospitalByMigratedIdAsync(row[36]?.ToString().Trim().TrimStart('0'));
                                    await Task.Delay(50);
                                    if (ReimbursementHospital2.IsDuplicate)
                                    {
                                        ReimbursementHospital2 = await HospitalData.GetHospitalByMigratedIdAsync(ReimbursementHospital2.DuplicateOfHospitalId);
                                    }
                                    newPath.Reimbursement2 = ReimbursementHospital2.HospitalName;
                                }


                                var id = await PathReportData.CreatePathReportAsync(newPath);
                            }

                            // Delete the placeholder pathreports
                            await PathReportData.DeletePlaceholderPathReportsAsync();
                            await PatientData.ClearCCRNosAsync();

                        }
                    }

                }
            }

        }

        private async Task OnKIDCOMMImport()
        {
            string fileUrl = "https://localhost:7150/kid-comm-cases.xlsx";
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
                        var PatientList = new List<Patient>();
                        var PatientPhoneList = new List<PatientPhoneNumber>();
                        var PatientPathReportList = new List<PathReport>();

                        if (result.Tables.Count > 0)
                        {
                            var index = 1;

                            DataTable firstSheet = result.Tables[0];
                            foreach (DataRow row in firstSheet.Rows)
                            {
                                Console.WriteLine(string.Join(", ", row.ItemArray));
                                if (row[0]?.ToString() == "CCRNo") // Skip header row
                                    continue;
                                var newPatient = new Patient();
                                newPatient.StudyId = Guid.Parse("251CE4DC-F01D-43D8-D47D-08DDD05DE341");  // KID-COMM StudyId
                                newPatient.MigratedCCRNo = row[0]?.ToString().Trim() ?? string.Empty;
                                newPatient.FirstName = row[2]?.ToString().Trim() ?? string.Empty;
                                newPatient.MiddleName = row[3]?.ToString().Trim() ?? string.Empty;
                                newPatient.LastName = row[1]?.ToString().Trim() ?? string.Empty;
                                newPatient.Suffix = row[4]?.ToString().Trim() ?? string.Empty;
                                newPatient.Address1 = row[5]?.ToString().Trim() ?? string.Empty;
                                newPatient.Address2 = row[6]?.ToString().Trim() ?? string.Empty;
                                newPatient.PatientComments = row[7]?.ToString().Trim() + " ";
                                newPatient.City = row[8]?.ToString().Trim() ?? string.Empty;
                                newPatient.State = row[9]?.ToString().Trim() ?? string.Empty;
                                newPatient.County = await LookupData.GetCountyByFIPSAsync(row[11]?.ToString().Trim()) ?? string.Empty;
                                newPatient.CountyCode = row[11]?.ToString().Trim().Remove(0, 2) ?? string.Empty;
                                newPatient.ZipCode = row[10]?.ToString().Trim() ?? string.Empty;
                                newPatient.DateOfBirth = DateTime.TryParse(row[14]?.ToString().Trim(), out DateTime dob) ? dob : (DateTime?)null;
                                Console.WriteLine("RACECODE: " + row[15]?.ToString().Trim().PadLeft(2, '0'));

                                newPatient.Race = await LookupData.GetTypeByCodeAsync("Race", row[15]?.ToString().Trim().PadLeft(2, '0')) ?? string.Empty;
                                newPatient.RaceCode = row[15]?.ToString().Trim().PadLeft(2, '0') ?? string.Empty;
                                newPatient.Gender = await LookupData.GetTypeByCodeAsync("Gender", row[16]?.ToString().Trim()) ?? string.Empty;
                                newPatient.GenderCode = row[16]?.ToString().Trim() ?? string.Empty;

                                var ethnicityCode = row[17]?.ToString().Trim();

                                newPatient.Ethnicity = await LookupData.GetTypeByCodeAsync("Ethnicity", ethnicityCode);
                                newPatient.EthnicityCode = ethnicityCode ?? string.Empty;
                                newPatient.SocialSecurityNumber = row[18]?.ToString().Trim() ?? string.Empty;
                                newPatient.PatientComments += row[19]?.ToString().Trim() ?? string.Empty;
                                newPatient.Email = row[34]?.ToString().Trim() ?? string.Empty;
                                newPatient.PatientComments += row[35]?.ToString().Trim() ?? string.Empty;
                                newPatient.PreferredName = row[36]?.ToString().Trim() ?? string.Empty;
                                newPatient.ModifiedDate = DateTime.TryParse(row[22]?.ToString().Trim(), out DateTime tempDate) ? tempDate : (DateTime?)null;
                                newPatient.DisplayName = $"{newPatient.LastName}, {newPatient.FirstName}";
                                newPatient.IsActive = true;

                                // Initialize the list so it's not null
                                newPatient.PatientPhoneNumbers = new List<PatientPhoneNumber>();

                                if (!string.IsNullOrWhiteSpace(row[12]?.ToString()))
                                {
                                    newPatient.PatientPhoneNumbers.Add(new PatientPhoneNumber
                                    {
                                        PhoneNumber = row[12].ToString().Trim(),
                                        PhoneType = "Other",
                                        IsPrimary = true,
                                        CreatedDate = DateTime.UtcNow
                                    });
                                }

                                if (!string.IsNullOrWhiteSpace(row[13]?.ToString()))
                                {
                                    newPatient.PatientPhoneNumbers.Add(new PatientPhoneNumber
                                    {
                                        PhoneNumber = row[13].ToString().Trim(),
                                        PhoneType = "Other",
                                        IsPrimary = false,
                                        CreatedDate = DateTime.UtcNow
                                    });
                                }

                                //newPatient.PatientPhoneNumbers = PatientPhoneList;


                                PatientPhoneList = new List<PatientPhoneNumber>();

                                var newPath = new PathReport();
                                newPath.MigratedCCRNumber = newPatient.MigratedCCRNo;
                                newPath.AgeAtProcedure = "0";
                                newPath.AuthorizingProvider = "Placeholder";
                                newPath.SubmittingHospital = "UNC Health Care";
                                newPath.HospCity = "Chapel Hill";
                                newPath.SubmittingHospitalPathReportNumber = "Placeholder" + index.ToString();
                                newPath.DateOfProcedure = DateTime.Now;
                                newPath.Site = "Placeholder";
                                newPath.PathProcedure = "Placeholder";
                                newPath.HistologyDiagnosis1 = "Placeholder";
                                newPath.HistologyCode1 = "0";
                                newPath.HistologyBehavior1 = "0";
                                newPath.HospAddress1 = "Placeholder";
                                newPath.PathIndex = 1;

                                PatientPathReportList.Add(newPath);

                                newPatient.PathReports = PatientPathReportList;

                                PatientPathReportList = new List<PathReport>();


                                PatientList.Add(newPatient);
                                index++;
                            }

                            // Add the new patients to the database
                            foreach (var patient in PatientList)
                            {
                                patient.CaseNumber = await GenerateCaseNumber.Generate("KID-COMM");
                                var id = await PatientData.CreatePatientAsync(patient);
                            }
                        }
                    }
                }

            }
        }

        private async Task OnKIDCOMMPathImport()
        {
            string fileUrl = "https://localhost:7150/kid-comm-pathreports.xlsx";
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

                        var PathReportList = new List<PathReport>();

                        if (result.Tables.Count > 0)
                        {
                            var index = 1;

                            DataTable firstSheet = result.Tables[0];
                            foreach (DataRow row in firstSheet.Rows)
                            {
                                Console.WriteLine(string.Join(", ", row.ItemArray));
                                if (row[0]?.ToString() == "CCRPathRptKey") // Skip header row
                                    continue;
                                var newPath = new PathReport();
                                newPath.PathIndex = 1;
                                newPath.MigratedCCRNumber = row[1]?.ToString().Trim() ?? string.Empty;
                                newPath.PatientId = await PatientData.GetPatientIdByCCRNoAsync(newPath.MigratedCCRNumber);
                                await Task.Delay(50);

                                var study = await StudyData.GetStudyAsync(Guid.Parse("251CE4DC-F01D-43D8-D47D-08DDD05DE341"));  // KID-COMM StudyId
                                newPath.StudyPrefix = study.Prefix;
                                newPath.StudyColor = study.ColorLight;

                                var patient = await PatientData.GetPatientAsync(newPath.PatientId);
                                await Task.Delay(50);
                                newPath.CaseNumber = patient.CaseNumber;
                                newPath.DxAddress1 = patient != null ? patient.Address1 : "Unknown";
                                newPath.DxAddress2 = patient != null ? patient.Address2 : "Unknown";
                                newPath.DxCity = patient != null ? patient.City : "Unknown";
                                newPath.DxState = patient != null ? patient.State : "Unknown";
                                newPath.DxZipCode = patient != null ? patient.ZipCode : "Unknown";
                                newPath.DxPhoneNumber = await PatientData.GetPatientPrimaryPhoneAsync(newPath.PatientId);
                                newPath.DxCounty = patient != null ? patient.County : "Unknown";
                                newPath.DxCountyCode = patient != null ? patient.CountyCode : "Unknown";

                                newPath.DateOfProcedure = DateTime.TryParse(row[4]?.ToString().Trim(), out DateTime dop) ? dop : (DateTime?)null;
                                newPath.AgeAtProcedure = row[5]?.ToString().Trim() ?? string.Empty;

                                var hospital = await HospitalData.GetHospitalByMigratedIdAsync(row[2]?.ToString().Trim().TrimStart('0') ?? string.Empty);
                                await Task.Delay(50);
                                if (hospital.IsDuplicate)
                                {
                                    hospital = await HospitalData.GetHospitalByMigratedIdAsync(hospital.DuplicateOfHospitalId);
                                }
                                newPath.HospitalId = hospital != null ? hospital.HospitalId : Guid.Empty;
                                newPath.SubmittingHospital = hospital != null ? hospital.HospitalName : "Unknown Hospital";
                                newPath.HospAddress1 = hospital != null ? hospital.Address1 : "Unknown";
                                newPath.HospAddress2 = hospital != null ? hospital.Address2 : "Unknown";
                                newPath.HospCity = hospital != null ? hospital.City : "Unknown";
                                newPath.HospState = hospital != null ? hospital.State : "Unknown";
                                newPath.HospZipCode = hospital != null ? hospital.ZipCode : "Unknown";
                                newPath.HospPhoneNumber = hospital != null ? hospital.PhoneNumber : "Unknown";
                                newPath.HospFaxNumber = hospital != null ? hospital.FaxNumber : "Unknown";

                                newPath.SubmittingHospitalPathReportNumber = row[6]?.ToString().Trim() ?? string.Empty;

                                var doctor = await DoctorData.GetDoctorByMigratedIdAsync(row[7]?.ToString().Trim());
                                await Task.Delay(50);
                                if (doctor.IsDuplicate)
                                {
                                    doctor = await DoctorData.GetDoctorByMigratedIdAsync(doctor.DuplicateOfDoctorId);
                                }
                                newPath.AuthorizingProvider = doctor != null ? doctor.DisplayName : "Unknown Provider";
                                newPath.DoctorId = doctor != null ? doctor.DoctorId : Guid.Empty;
                                newPath.MDAddress1 = doctor != null ? doctor.Address1 : "Unknown";
                                newPath.MDAddress2 = doctor != null ? doctor.Address2 : "Unknown";
                                newPath.MD2Address3 = doctor != null ? doctor.Address2 : "Unknown";
                                newPath.MDCity = doctor != null ? doctor.City : "Unknown";
                                newPath.MDState = doctor != null ? doctor.State : "Unknown";
                                newPath.MDZipCode = doctor != null ? doctor.ZipCode : "Unknown";
                                newPath.MDCounty = doctor != null ? doctor.County : "Unknown";
                                newPath.MDPhoneNumber1 = doctor != null ? doctor.PhoneNumber1 : "Unknown";
                                newPath.MDPhoneNumber2 = doctor != null ? doctor.PhoneNumber2 : "Unknown";
                                newPath.MDFaxNumber = doctor != null ? doctor.FaxNumber : "Unknown";
                                newPath.MDEmail = doctor != null ? doctor.Email : "Unknown";

                                newPath.AuthorizingProviderComments = row[55]?.ToString().Trim() ?? string.Empty;

                                var studyId = Guid.Parse("251CE4DC-F01D-43D8-D47D-08DDD05DE341");  // KID-COMM StudyId
                                if (!string.IsNullOrEmpty(row[10]?.ToString().Trim()))
                                {
                                    var site1 = await StudyLookupData.GetValueByOldCodeAsync(studyId, "Site", Int32.Parse(row[10]?.ToString().Trim()));
                                    await Task.Delay(50);
                                    newPath.Site = site1 ?? "Unknown Site";
                                    newPath.SiteCode = await StudyLookupData.GetCodeByValueAsync(studyId, "Site", site1);
                                    await Task.Delay(50);
                                }
                                else
                                {
                                    newPath.Site = "Unknown Site";
                                    newPath.SiteCode = string.Empty;
                                }
                                if (!string.IsNullOrEmpty(row[13]?.ToString().Trim()))
                                {
                                    var site2 = await StudyLookupData.GetValueByOldCodeAsync(studyId, "Site", Int32.Parse(row[13]?.ToString().Trim()));
                                    await Task.Delay(50);
                                    newPath.Site2 = site2 ?? "Unknown Site";
                                    newPath.SiteCode2 = await StudyLookupData.GetCodeByValueAsync(studyId, "Site", site2);
                                    await Task.Delay(50);

                                    newPath.AuthorizingProvider2 = newPath.AuthorizingProvider;
                                    newPath.Doctor2Id = newPath.DoctorId;
                                    newPath.MD2Address1 = newPath.MDAddress1;
                                    newPath.MD2Address2 = newPath.MDAddress2;
                                    newPath.MD2Address3 = newPath.MD2Address2;
                                    newPath.MD2City = newPath.MDCity;
                                    newPath.MD2State = newPath.MDState;
                                    newPath.MD2ZipCode = newPath.MDZipCode;
                                    newPath.MD2County = newPath.MDCounty;
                                    newPath.MD2PhoneNumber1 = newPath.MDPhoneNumber1;
                                    newPath.MD2PhoneNumber2 = newPath.MDPhoneNumber2;
                                    newPath.MD2FaxNumber = newPath.MDFaxNumber;
                                    newPath.MD2Email = newPath.MDEmail;
                                }
                                else
                                {
                                    newPath.Site2 = string.Empty;
                                    newPath.SiteCode2 = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(row[8]?.ToString().Trim()))
                                {
                                    var proc1 = await StudyLookupData.GetValueByOldCodeAsync(studyId, "Procedure", Int32.Parse(row[8]?.ToString().Trim()));
                                    await Task.Delay(50);
                                    newPath.PathProcedure = proc1 ?? "Unknown Procedure";
                                }
                                else
                                {
                                    newPath.PathProcedure = "Unknown Procedure";
                                }
                                if (!string.IsNullOrEmpty(row[12]?.ToString().Trim()))
                                {
                                    var proc2 = await StudyLookupData.GetValueByOldCodeAsync(studyId, "Procedure", Int32.Parse(row[12]?.ToString().Trim()));
                                    await Task.Delay(50);
                                    newPath.PathProcedure2 = proc2 ?? "Unknown Procedure";
                                }
                                else
                                {
                                    newPath.PathProcedure2 = string.Empty;
                                }

                                newPath.PathComments = row[9]?.ToString().Trim() ?? string.Empty;
                                newPath.PathComments2 = row[11]?.ToString().Trim() ?? string.Empty;
                                newPath.IsOutsidePathReport = row[15]?.ToString().Trim().ToLower() == "y" ? false : true;
                                newPath.SlidesResideAtSubmittingHospital = row[15]?.ToString().Trim().ToLower() == "y" ? "Yes" : "No";

                                if (!string.IsNullOrEmpty(row[16].ToString()))
                                {
                                    var outsideHospital = await HospitalData.GetHospitalByMigratedIdAsync(row[16].ToString().Trim().TrimStart('0'));
                                    await Task.Delay(50);
                                    if (outsideHospital.IsDuplicate)
                                    {
                                        outsideHospital = await HospitalData.GetHospitalByMigratedIdAsync(outsideHospital.DuplicateOfHospitalId);
                                    }
                                    newPath.OriginatingHospitalName = outsideHospital.HospitalName ?? "Unknown";
                                    newPath.OrigHospitalId = outsideHospital.HospitalId;
                                    newPath.OrigHospAddress1 = outsideHospital.Address1;
                                    newPath.OrigHospAddress2 = outsideHospital.Address2;
                                    newPath.OrigHospCity = outsideHospital.City;
                                    newPath.OrigHospState = outsideHospital.State;
                                    newPath.OrigHospZipCode = outsideHospital.ZipCode;
                                    newPath.OrigHospPhoneNumber = outsideHospital.PhoneNumber;
                                    newPath.OrigHospFaxNumber = outsideHospital.FaxNumber;

                                }

                                newPath.OriginatingHospitalPathReportNumber = row[18]?.ToString().Trim() ?? string.Empty;
                                newPath.OriginatingHospitalComments = row[19]?.ToString().Trim() ?? string.Empty;
                                newPath.IsOnHold = row[34]?.ToString().Trim().ToLower() == "true" ? true : false;
                                newPath.CreatedDate = row[35]?.ToString().Trim() != null ? DateTime.Parse(row[35]?.ToString().Trim()) : DateTime.UtcNow;
                                if (string.IsNullOrEmpty(row[38]?.ToString().Trim()))
                                {
                                    newPath.RcaExportDate = null;
                                    newPath.ExportStatus = "Unknown";
                                }
                                else
                                {
                                    newPath.RcaExportDate = DateTime.Parse(row[38]?.ToString().Trim());
                                    newPath.ExportStatus = "Exported";
                                }

                                if (!string.IsNullOrEmpty(row[49].ToString().Trim()))
                                {
                                    var codeList = new List<int> { 32, 33, 52, 30, 29, 28, 34 };
                                    var histCode = Int32.Parse(row[49].ToString().Trim());
                                    if (codeList.Contains(histCode))
                                    {
                                        histCode = 31; // map code due to duplicate entries in lookup
                                    }
                                    if (histCode == 35)
                                    {
                                        histCode = 53; // map code due to duplicate entries in lookup
                                    }
                                    if (histCode == 54)
                                    {
                                        histCode = 37; // map code due to duplicate entries in lookup
                                    }
                                    if (histCode == 45)
                                    {
                                        histCode = 43; // map code due to duplicate entries in lookup
                                    }
                                    var histology = await StudyHistologyData.GetValueByOldCodeAsync(studyId, histCode);
                                    await Task.Delay(50);
                                    newPath.HistologyDiagnosis1 = histology.HistologyName;
                                    newPath.HistologyCode1 = histology.HistologyCode;
                                    newPath.HistologyBehavior1 = histology.HistologyBehavior;
                                }

                                newPath.HistologyDiagnosisComments1 = row[50]?.ToString().Trim() ?? string.Empty;

                                if (!string.IsNullOrEmpty(row[51].ToString().Trim()))
                                {
                                    var codeList = new List<int> { 32, 33, 52, 30, 29, 28, 34 };
                                    var histCode = Int32.Parse(row[49].ToString().Trim());
                                    if (codeList.Contains(histCode))
                                    {
                                        histCode = 31; // map code due to duplicate entries in lookup
                                    }
                                    if (histCode == 35)
                                    {
                                        histCode = 53; // map code due to duplicate entries in lookup
                                    }
                                    if (histCode == 54)
                                    {
                                        histCode = 37; // map code due to duplicate entries in lookup
                                    }
                                    if (histCode == 45)
                                    {
                                        histCode = 43; // map code due to duplicate entries in lookup
                                    }
                                    var histology = await StudyHistologyData.GetValueByOldCodeAsync(studyId, histCode);
                                    await Task.Delay(50);
                                    newPath.HistologyDiagnosis2 = histology.HistologyName;
                                    newPath.HistologyCode2 = histology.HistologyCode;
                                    newPath.HistologyBehavior2 = histology.HistologyBehavior;
                                }

                                newPath.HistologyDiagnosisComments2 = row[52]?.ToString().Trim() ?? string.Empty;

                                if (!string.IsNullOrEmpty(row[53]?.ToString().Trim()))
                                {
                                    var ReimbursementHospital1 = await HospitalData.GetHospitalByMigratedIdAsync(row[53]?.ToString().Trim().TrimStart('0'));
                                    await Task.Delay(50);
                                    if (ReimbursementHospital1.IsDuplicate)
                                    {
                                        ReimbursementHospital1 = await HospitalData.GetHospitalByMigratedIdAsync(ReimbursementHospital1.DuplicateOfHospitalId);
                                    }
                                    newPath.Reimbursement1 = ReimbursementHospital1.HospitalName;
                                }

                                if (!string.IsNullOrEmpty(row[54]?.ToString().Trim()))
                                {
                                    var ReimbursementHospital2 = await HospitalData.GetHospitalByMigratedIdAsync(row[54]?.ToString().Trim().TrimStart('0'));
                                    await Task.Delay(50);
                                    if (ReimbursementHospital2.IsDuplicate)
                                    {
                                        ReimbursementHospital2 = await HospitalData.GetHospitalByMigratedIdAsync(ReimbursementHospital2.DuplicateOfHospitalId);
                                    }
                                    newPath.Reimbursement2 = ReimbursementHospital2.HospitalName;
                                }

                                if (string.IsNullOrEmpty(row[63]?.ToString().Trim()))
                                {
                                    newPath.TumorSize = 0.0;
                                }
                                else
                                {
                                    newPath.TumorSize = Double.Parse(row[63]?.ToString().Trim());
                                }


                                if (string.IsNullOrEmpty(row[64]?.ToString().Trim()))
                                {
                                    newPath.MarginStatus = "unknown";
                                }
                                else
                                {
                                    newPath.MarginStatus = row[64]?.ToString().Trim() ?? string.Empty;
                                }



                                PathReportList.Add(newPath);
                            }

                            // Add the new paths to the database
                            foreach (var path in PathReportList)
                            {
                                var id = await PathReportData.CreatePathReportAsync(path);
                            }

                            // Delete the placeholder pathreports
                            await PathReportData.DeletePlaceholderPathReportsAsync();
                            await PatientData.ClearCCRNosAsync();

                        }
                    }

                }
            }

        }

        private async Task OnMTCSSImport()
        {
            string fileUrl = "https://localhost:7150/mtcss-cases.xlsx";
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
                        var PatientList = new List<Patient>();
                        var PatientPhoneList = new List<PatientPhoneNumber>();
                        var PatientPathReportList = new List<PathReport>();

                        if (result.Tables.Count > 0)
                        {
                            var index = 1;

                            DataTable firstSheet = result.Tables[0];
                            foreach (DataRow row in firstSheet.Rows)
                            {
                                Console.WriteLine(string.Join(", ", row.ItemArray));
                                if (row[0]?.ToString() == "CCRNo") // Skip header row
                                    continue;
                                var newPatient = new Patient();
                                newPatient.StudyId = Guid.Parse("B4C7A525-42CD-4931-75CA-08DDEF2BF95E");  // MTCSS StudyId
                                newPatient.MigratedCCRNo = row[0]?.ToString().Trim() ?? string.Empty;
                                newPatient.FirstName = row[2]?.ToString().Trim() ?? string.Empty;
                                newPatient.MiddleName = row[3]?.ToString().Trim() ?? string.Empty;
                                newPatient.LastName = row[1]?.ToString().Trim() ?? string.Empty;
                                newPatient.Suffix = row[4]?.ToString().Trim() ?? string.Empty;
                                newPatient.Address1 = row[6]?.ToString().Trim() ?? string.Empty;
                                newPatient.Address2 = row[7]?.ToString().Trim() ?? string.Empty;
                                newPatient.PatientComments = row[8]?.ToString().Trim() + " ";
                                newPatient.City = row[9]?.ToString().Trim() ?? string.Empty;
                                newPatient.State = row[10]?.ToString().Trim() ?? string.Empty;
                                newPatient.County = await LookupData.GetCountyByFIPSAsync(row[12]?.ToString().Trim()) ?? string.Empty;
                                newPatient.CountyCode = row[12]?.ToString().Trim().Remove(0, 2) ?? string.Empty;
                                newPatient.ZipCode = row[11]?.ToString().Trim() ?? string.Empty;
                                newPatient.DateOfBirth = DateTime.TryParse(row[15]?.ToString().Trim(), out DateTime dob) ? dob : (DateTime?)null;
                                Console.WriteLine("RACECODE: " + row[17]?.ToString().Trim().PadLeft(2, '0'));

                                newPatient.Race = await LookupData.GetTypeByCodeAsync("Race", row[17]?.ToString().Trim().PadLeft(2, '0')) ?? string.Empty;
                                newPatient.RaceCode = row[17]?.ToString().Trim().PadLeft(2, '0') ?? string.Empty;
                                newPatient.Gender = await LookupData.GetTypeByCodeAsync("Gender", row[19]?.ToString().Trim()) ?? string.Empty;
                                newPatient.GenderCode = row[19]?.ToString().Trim() ?? string.Empty;

                                var ethnicity = row[16]?.ToString().Trim();
                                var ethnicityCode = "9";
                                switch (ethnicity)
                                {
                                    case "Hispanic":
                                        ethnicityCode = "6";
                                        break;
                                    case "Non-Hispanic":
                                        ethnicityCode = "0";
                                        break;
                                    case "Unknown":
                                        ethnicityCode = "9";
                                        break;
                                    default:
                                        ethnicityCode = "9";
                                        break;
                                }

                                newPatient.Ethnicity = await LookupData.GetTypeByCodeAsync("Ethnicity", ethnicityCode);
                                newPatient.EthnicityCode = ethnicityCode ?? string.Empty;
                                newPatient.SocialSecurityNumber = row[20]?.ToString().Trim() ?? string.Empty;
                                newPatient.PatientComments += row[18]?.ToString().Trim() ?? string.Empty;
                                newPatient.Email = string.Empty;
                                newPatient.PatientComments += row[24]?.ToString().Trim() ?? string.Empty;
                                newPatient.PreferredName = string.Empty;
                                newPatient.ModifiedDate = DateTime.TryParse(row[27]?.ToString().Trim(), out DateTime tempDate) ? tempDate : (DateTime?)null;
                                newPatient.DisplayName = $"{newPatient.LastName}, {newPatient.FirstName}";
                                newPatient.IsActive = true;

                                // Initialize the list so it's not null
                                newPatient.PatientPhoneNumbers = new List<PatientPhoneNumber>();

                                if (!string.IsNullOrWhiteSpace(row[13]?.ToString()))
                                {
                                    newPatient.PatientPhoneNumbers.Add(new PatientPhoneNumber
                                    {
                                        PhoneNumber = row[12].ToString().Trim(),
                                        PhoneType = "Other",
                                        IsPrimary = true,
                                        CreatedDate = DateTime.UtcNow
                                    });
                                }

                                if (!string.IsNullOrWhiteSpace(row[14]?.ToString()))
                                {
                                    newPatient.PatientPhoneNumbers.Add(new PatientPhoneNumber
                                    {
                                        PhoneNumber = row[13].ToString().Trim(),
                                        PhoneType = "Other",
                                        IsPrimary = false,
                                        CreatedDate = DateTime.UtcNow
                                    });
                                }

                                //newPatient.PatientPhoneNumbers = PatientPhoneList;


                                PatientPhoneList = new List<PatientPhoneNumber>();

                                var newPath = new PathReport();
                                newPath.MigratedCCRNumber = newPatient.MigratedCCRNo;
                                newPath.AgeAtProcedure = "0";
                                newPath.AuthorizingProvider = "Placeholder";
                                newPath.SubmittingHospital = "UNC Health Care";
                                newPath.HospCity = "Chapel Hill";
                                newPath.SubmittingHospitalPathReportNumber = "Placeholder" + index.ToString();
                                newPath.DateOfProcedure = DateTime.Now;
                                newPath.Site = "Placeholder";
                                newPath.PathProcedure = "Placeholder";
                                newPath.HistologyDiagnosis1 = "Placeholder";
                                newPath.HistologyCode1 = "0";
                                newPath.HistologyBehavior1 = "0";
                                newPath.HospAddress1 = "Placeholder";
                                newPath.PathIndex = 1;

                                PatientPathReportList.Add(newPath);

                                newPatient.PathReports = PatientPathReportList;

                                PatientPathReportList = new List<PathReport>();


                                PatientList.Add(newPatient);
                                index++;
                            }

                            // Add the new patients to the database
                            foreach (var patient in PatientList)
                            {
                                patient.CaseNumber = await GenerateCaseNumber.Generate("MTCSS");
                                var id = await PatientData.CreatePatientAsync(patient);
                            }
                        }
                    }
                }

            }
        }

        private async Task OnMTCSSPathImport()
        {
            string fileUrl = "https://localhost:7150/mtcss-pathreports.xlsx";
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

                        var PathReportList = new List<PathReport>();

                        if (result.Tables.Count > 0)
                        {
                            var index = 1;

                            DataTable firstSheet = result.Tables[0];
                            foreach (DataRow row in firstSheet.Rows)
                            {
                                Console.WriteLine(string.Join(", ", row.ItemArray));
                                if (row[0]?.ToString() == "CCRPathRptKey") // Skip header row
                                    continue;
                                var newPath = new PathReport();
                                newPath.PathIndex = 1;
                                newPath.MigratedCCRNumber = row[1]?.ToString().Trim() ?? string.Empty;
                                newPath.PatientId = await PatientData.GetPatientIdByCCRNoAsync(newPath.MigratedCCRNumber);
                                await Task.Delay(50);

                                var study = await StudyData.GetStudyAsync(Guid.Parse("B4C7A525-42CD-4931-75CA-08DDEF2BF95E"));  // MTCSS StudyId
                                newPath.StudyPrefix = study.Prefix;
                                newPath.StudyColor = study.ColorLight;

                                var patient = await PatientData.GetPatientAsync(newPath.PatientId);
                                await Task.Delay(50);
                                newPath.CaseNumber = patient.CaseNumber;
                                newPath.DxAddress1 = patient != null ? patient.Address1 : "Unknown";
                                newPath.DxAddress2 = patient != null ? patient.Address2 : "Unknown";
                                newPath.DxCity = patient != null ? patient.City : "Unknown";
                                newPath.DxState = patient != null ? patient.State : "Unknown";
                                newPath.DxZipCode = patient != null ? patient.ZipCode : "Unknown";
                                newPath.DxPhoneNumber = await PatientData.GetPatientPrimaryPhoneAsync(newPath.PatientId);
                                newPath.DxCounty = patient != null ? patient.County : "Unknown";
                                newPath.DxCountyCode = patient != null ? patient.CountyCode : "Unknown";

                                newPath.DateOfProcedure = DateTime.TryParse(row[4]?.ToString().Trim(), out DateTime dop) ? dop : (DateTime?)null;
                                newPath.AgeAtProcedure = row[5]?.ToString().Trim() ?? string.Empty;

                                var hospital = await HospitalData.GetHospitalByMigratedIdAsync(row[2]?.ToString().Trim().TrimStart('0') ?? string.Empty);
                                await Task.Delay(50);
                                if (hospital.IsDuplicate)
                                {
                                    hospital = await HospitalData.GetHospitalByMigratedIdAsync(hospital.DuplicateOfHospitalId);
                                }
                                newPath.HospitalId = hospital != null ? hospital.HospitalId : Guid.Empty;
                                newPath.SubmittingHospital = hospital != null ? hospital.HospitalName : "Unknown Hospital";
                                newPath.HospAddress1 = hospital != null ? hospital.Address1 : "Unknown";
                                newPath.HospAddress2 = hospital != null ? hospital.Address2 : "Unknown";
                                newPath.HospCity = hospital != null ? hospital.City : "Unknown";
                                newPath.HospState = hospital != null ? hospital.State : "Unknown";
                                newPath.HospZipCode = hospital != null ? hospital.ZipCode : "Unknown";
                                newPath.HospPhoneNumber = hospital != null ? hospital.PhoneNumber : "Unknown";
                                newPath.HospFaxNumber = hospital != null ? hospital.FaxNumber : "Unknown";

                                newPath.SubmittingHospitalPathReportNumber = row[6]?.ToString().Trim() ?? string.Empty;

                                var doctor = await DoctorData.GetDoctorByMigratedIdAsync(row[7]?.ToString().Trim());
                                await Task.Delay(50);
                                if (doctor.IsDuplicate)
                                {
                                    doctor = await DoctorData.GetDoctorByMigratedIdAsync(doctor.DuplicateOfDoctorId);
                                }
                                newPath.AuthorizingProvider = doctor != null ? doctor.DisplayName : "Unknown Provider";
                                newPath.DoctorId = doctor != null ? doctor.DoctorId : Guid.Empty;
                                newPath.MDAddress1 = doctor != null ? doctor.Address1 : "Unknown";
                                newPath.MDAddress2 = doctor != null ? doctor.Address2 : "Unknown";
                                newPath.MD2Address3 = doctor != null ? doctor.Address2 : "Unknown";
                                newPath.MDCity = doctor != null ? doctor.City : "Unknown";
                                newPath.MDState = doctor != null ? doctor.State : "Unknown";
                                newPath.MDZipCode = doctor != null ? doctor.ZipCode : "Unknown";
                                newPath.MDCounty = doctor != null ? doctor.County : "Unknown";
                                newPath.MDPhoneNumber1 = doctor != null ? doctor.PhoneNumber1 : "Unknown";
                                newPath.MDPhoneNumber2 = doctor != null ? doctor.PhoneNumber2 : "Unknown";
                                newPath.MDFaxNumber = doctor != null ? doctor.FaxNumber : "Unknown";
                                newPath.MDEmail = doctor != null ? doctor.Email : "Unknown";

                                newPath.AuthorizingProviderComments = string.Empty;

                                var pathologist = await DoctorData.GetDoctorByMigratedIdAsync(row[8]?.ToString().Trim());
                                await Task.Delay(50);
                                if (pathologist.IsDuplicate)
                                {
                                    pathologist = await DoctorData.GetDoctorByMigratedIdAsync(pathologist.DuplicateOfDoctorId);
                                }
                                newPath.Pathologist = pathologist != null ? pathologist.DisplayName : "Unknown Pathologist";
                                newPath.PathologistId = pathologist != null ? pathologist.DoctorId : Guid.Empty;
                                newPath.PathAddress1 = pathologist != null ? pathologist.Address1 : "Unknown";
                                newPath.PathAddress2 = pathologist != null ? pathologist.Address2 : "Unknown";
                                newPath.PathAddress3 = pathologist != null ? pathologist.Address2 : "Unknown";
                                newPath.PathCity = pathologist != null ? pathologist.City : "Unknown";
                                newPath.PathState = pathologist != null ? pathologist.State : "Unknown";
                                newPath.PathZipCode = pathologist != null ? pathologist.ZipCode : "Unknown";
                                newPath.PathCounty = pathologist != null ? pathologist.County : "Unknown";
                                newPath.PathPhoneNumber1 = pathologist != null ? pathologist.PhoneNumber1 : "Unknown";
                                newPath.PathPhoneNumber2 = pathologist != null ? pathologist.PhoneNumber2 : "Unknown";
                                newPath.PathFaxNumber = pathologist != null ? pathologist.FaxNumber : "Unknown";
                                newPath.PathEmail = pathologist != null ? pathologist.Email : "Unknown";

                                newPath.PathologistComments = string.Empty;


                                var studyId = Guid.Parse("B4C7A525-42CD-4931-75CA-08DDEF2BF95E");  // MTCSS StudyId

                                newPath.Site = "None";
                                newPath.SiteCode = "1";


                                if (!string.IsNullOrEmpty(row[9]?.ToString().Trim()))
                                {
                                    var proc1 = await StudyLookupData.GetValueByOldCodeAsync(studyId, "Procedure", Int32.Parse(row[9]?.ToString().Trim()));
                                    await Task.Delay(50);
                                    newPath.PathProcedure = proc1 ?? "Unknown Procedure";
                                }
                                else
                                {
                                    newPath.PathProcedure = "Unknown Procedure";
                                }


                                newPath.PathComments = row[10]?.ToString().Trim() ?? string.Empty;
                                newPath.IsOutsidePathReport = row[11]?.ToString().Trim().ToLower() == "n" ? false : true;
                                newPath.SlidesResideAtSubmittingHospital = row[11]?.ToString().Trim().ToLower() == "n" ? "Yes" : "No";

                                if (!string.IsNullOrEmpty(row[12].ToString()))
                                {
                                    var outsideHospital = await HospitalData.GetHospitalByMigratedIdAsync(row[12].ToString().Trim().TrimStart('0'));
                                    await Task.Delay(50);
                                    if (outsideHospital.IsDuplicate)
                                    {
                                        outsideHospital = await HospitalData.GetHospitalByMigratedIdAsync(outsideHospital.DuplicateOfHospitalId);
                                    }
                                    newPath.OriginatingHospitalName = outsideHospital.HospitalName ?? "Unknown";
                                    newPath.OrigHospitalId = outsideHospital.HospitalId;
                                    newPath.OrigHospAddress1 = outsideHospital.Address1;
                                    newPath.OrigHospAddress2 = outsideHospital.Address2;
                                    newPath.OrigHospCity = outsideHospital.City;
                                    newPath.OrigHospState = outsideHospital.State;
                                    newPath.OrigHospZipCode = outsideHospital.ZipCode;
                                    newPath.OrigHospPhoneNumber = outsideHospital.PhoneNumber;
                                    newPath.OrigHospFaxNumber = outsideHospital.FaxNumber;

                                }

                                newPath.OriginatingHospitalPathReportNumber = row[14]?.ToString().Trim() ?? string.Empty;
                                newPath.OriginatingHospitalComments = row[15]?.ToString().Trim() ?? string.Empty;
                                newPath.IsOnHold = row[37]?.ToString().Trim().ToLower() == "true" ? true : false;
                                newPath.CreatedDate = row[38]?.ToString().Trim() != null ? DateTime.Parse(row[38]?.ToString().Trim()) : DateTime.UtcNow;
                                if (string.IsNullOrEmpty(row[41]?.ToString().Trim()))
                                {
                                    newPath.RcaExportDate = null;
                                    newPath.ExportStatus = "Unknown";
                                }
                                else
                                {
                                    newPath.RcaExportDate = DateTime.Parse(row[41]?.ToString().Trim());
                                    newPath.ExportStatus = "Exported";
                                }

                                if (!string.IsNullOrEmpty(row[16].ToString().Trim()))
                                {
                                    var histCode = Int32.Parse(row[16].ToString().Trim());
                                    var histology = await StudyHistologyData.GetValueByOldCodeAsync(studyId, histCode);
                                    await Task.Delay(50);
                                    newPath.HistologyDiagnosis1 = histology.HistologyName;
                                    newPath.HistologyCode1 = histology.HistologyCode;
                                    newPath.HistologyBehavior1 = histology.HistologyBehavior;
                                }

                                newPath.HistologyDiagnosisComments1 = row[17]?.ToString().Trim() ?? string.Empty;

                                if (!string.IsNullOrEmpty(row[18].ToString().Trim()))
                                {
                                    var histCode = Int32.Parse(row[18].ToString().Trim());
                                    var histology = await StudyHistologyData.GetValueByOldCodeAsync(studyId, histCode);
                                    await Task.Delay(50);
                                    newPath.HistologyDiagnosis2 = histology.HistologyName;
                                    newPath.HistologyCode2 = histology.HistologyCode;
                                    newPath.HistologyBehavior2 = histology.HistologyBehavior;
                                }

                                newPath.HistologyDiagnosisComments2 = row[19]?.ToString().Trim() ?? string.Empty;

                                if (!string.IsNullOrEmpty(row[2]?.ToString().Trim()))
                                {
                                    var ReimbursementHospital1 = await HospitalData.GetHospitalByMigratedIdAsync(row[2]?.ToString().Trim().TrimStart('0'));
                                    await Task.Delay(50);
                                    if (ReimbursementHospital1.IsDuplicate)
                                    {
                                        ReimbursementHospital1 = await HospitalData.GetHospitalByMigratedIdAsync(ReimbursementHospital1.DuplicateOfHospitalId);
                                    }
                                    newPath.Reimbursement1 = ReimbursementHospital1.HospitalName;
                                }

                                if (string.IsNullOrEmpty(row[21]?.ToString().Trim()))
                                {
                                    newPath.TumorSize = 0.0;
                                }
                                else
                                {
                                    newPath.TumorSize = Double.Parse(row[21]?.ToString().Trim());
                                }

                                newPath.ALNL_PositiveNodes = row[28]?.ToString().Trim() ?? string.Empty;
                                newPath.ALNL_NodesExamined = row[29]?.ToString().Trim() ?? string.Empty;

                                var histologicDiffCode = row[32]?.ToString().Trim();
                                var histologicDiffValue = string.Empty;
                                var histologicGradeValue = string.Empty;
                                switch (histologicDiffCode)
                                {
                                    case "1":
                                        histologicDiffValue = "Well differentiated";
                                        histologicGradeValue = "Grade I";
                                        break;
                                    case "2":
                                        histologicDiffValue = "Differentiated, NOS";
                                        histologicGradeValue = "Grade I";
                                        break;
                                    case "3":
                                        histologicDiffValue = "Moderately Differentiated";
                                        histologicGradeValue = "Grade II";
                                        break;
                                    case "4":
                                        histologicDiffValue = "Moderately Well Differentiated";
                                        histologicGradeValue = "Grade II";
                                        break;
                                    case "5":
                                        histologicDiffValue = "Intermediate Differentiated";
                                        histologicGradeValue = "Grade II";
                                        break;
                                    case "6":
                                        histologicDiffValue = "Poorly Differentiated";
                                        histologicGradeValue = "Grade III";
                                        break;
                                    case "7":
                                        histologicDiffValue = "Undifferentiated";
                                        histologicGradeValue = "Grade IV";
                                        break;
                                    case "8":
                                        histologicDiffValue = "Anaplastic Differentiated";
                                        histologicGradeValue = "Grade IV";
                                        break;
                                    case "9":
                                        histologicDiffValue = "Grade or differentiation not determined (not stated or NA)";
                                        histologicGradeValue = "Unknown";
                                        break;
                                    default:
                                        histologicDiffValue = "Grade or differentiation not determined (not stated or NA)";
                                        histologicGradeValue = "Unknown";
                                        break;
                                }
                                newPath.HistologicDiff = histologicDiffValue;
                                newPath.HistologicGrade = histologicGradeValue;


                                PathReportList.Add(newPath);
                            }

                            // Add the new paths to the database
                            foreach (var path in PathReportList)
                            {
                                var id = await PathReportData.CreatePathReportAsync(path);
                            }

                            // Delete the placeholder pathreports
                            await PathReportData.DeletePlaceholderPathReportsAsync();
                            await PatientData.ClearCCRNosAsync();

                        }
                    }

                }
            }

        }



    }
}
