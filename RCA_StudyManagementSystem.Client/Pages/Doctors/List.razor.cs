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

namespace RCA_StudyManagementSystem.Client.Pages.Doctors
{
    public partial class List : Microsoft.AspNetCore.Components.ComponentBase
    {
        private readonly DialogOptions _options = new() { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        private readonly DialogOptions _maxWidth = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

        MudDataGrid<Doctor>? docGrid { get; set; }


        public IEnumerable<Doctor>? Doctors { get; set; }

        private IEnumerable<Doctor>? _displayItems { get; set; }

        private int Index = 0;



        private const string GridStateStorageKey = "DocDataGridState"; // Key for local storage


        private string? _searchString;
        private List<string> _events = new();
        private CancellationToken token;




        // quick filter - filter globally across multiple columns with the same input
        private Func<Doctor, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.LastName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            
            if (x.FirstName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if(x.MigratedDoctorId.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.DisplayName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.PrimarySpecialty.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.City.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.Address1.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        };

        private Func<Doctor, object> _sortByPathologist => x =>
        {
            if (x.IsPathologist)
                return "true";
            else
                return "false";
        };

        private Func<Doctor, object> _sortByVerified => x =>
        {
            if (x.IsVerified)
                return "true";
            else
                return "false";
        };


        protected override async Task OnInitializedAsync()
        {
            var auth = await AuthStateProvider.GetAuthenticationStateAsync();
            var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            Console.WriteLine("OnInitializedAsync method called!");

            Doctors = await DoctorData.ListAllDoctorsAsync();

            if (Doctors == null)
            {
                foreach (var doc in Doctors)
                {
                    if (doc.VerifiedDate < DateTime.Now.AddMonths(-6) && doc.IsVerified)
                    {
                        doc.IsVerified = false;
                        doc.VerifiedDate = null;
                        await DoctorData.UpdateDoctorAsync(doc.DoctorId, userId, doc);
                    }
                }
            }

            _displayItems = Doctors;

            docGrid.FilterDefinitions = DocGridStateView.FilterDefinitions;
            docGrid.SortDefinitions = new Dictionary<string, SortDefinition<Doctor>>(DocGridStateView.SortDefinitions);

            docGrid.CurrentPage = DocGridStateView.CurrentPage;

            return;

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && docGrid != null)
            {
                Doctors = await DoctorData.ListAllDoctorsAsync();

                var storedStateDto = await LocalStorage.GetItemAsync<GridStateDto>(GridStateStorageKey);

                if (storedStateDto != null)
                {
                    _searchString = storedStateDto.SearchString ?? ""; // Restore the search string

                    // Map DTOs back to MudBlazor types and update the ViewModel
                    var newFilterDefs = storedStateDto.Filters
                        .Select(dto =>
                        {
                            // Find the column by property name
                            var column = docGrid.RenderedColumns.FirstOrDefault(c => c.PropertyName == dto.Field);
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
                        .ToDictionary(dto => dto.SortBy, dto => new SortDefinition<Doctor>(
                            dto.SortBy,
                            dto.Descending,
                            0,
                            null
                        ));

                    var newSortColumn = DocGridStateView.SortColumn;

                    DocGridStateView.FilterDefinitions = newFilterDefs!;
                    DocGridStateView.SortDefinitions = newSortDefs;

                    docGrid.FilterDefinitions = DocGridStateView.FilterDefinitions;
                    docGrid.SortDefinitions = DocGridStateView.SortDefinitions;

                    // Set the sort definitions
                    if (DocGridStateView.SortDefinitions.Any())
                    {
                        // Apply the first sort definition if available
                        var firstSort = DocGridStateView.SortDefinitions.First();
                        SortDirection direction = firstSort.Value.Descending ? SortDirection.Descending : SortDirection.Ascending;
                        await docGrid.SetSortAsync(firstSort.Key, direction, firstSort.Value.SortFunc);

                        var sortDefinitions = docGrid.SortDefinitions; // Get current sort settings

                        string sortByProperty = "";
                        var sortedData = Doctors.AsQueryable(); // Start with unsorted data



                        if (sortDefinitions != null && sortDefinitions.Any())
                        {
                            sortedData = Doctors.AsQueryable(); // Start with unsorted data

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
                            _displayItems = Doctors; // If no sorting, revert to original order
                        }
                    }
                    // Load CurrentPage and PageSize
                    // Check that the dataGrid reference is not null

                    DocGridStateView.CurrentPage = storedStateDto.CurrentPage;
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
            DocGridStateView.CurrentPage = page;

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



        async Task<IDialogReference> ViewItem(Doctor args, Doctor pArgs)
        {

            var newDoctors = new List<Doctor>();

            foreach (var doc in docGrid.FilteredItems)
            {
                var newDoctor = await DoctorData.GetDoctorAsync(doc.DoctorId);
                newDoctors.Add(newDoctor);
            }


            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {System.Text.Json.JsonSerializer.Serialize(args.Item)}");

            var parameters = new DialogParameters<ViewDialog>();
            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, newDoctors); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, docGrid.FilteredItems.ToList().IndexOf(pArgs)); // Set initial position

            var options = _options;

            return await DialogService.ShowAsync<ViewDialog>("Doctor View", parameters, options);

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

            if (docGrid != null)
            {
                // Map MudBlazor types to DTOs for serialization
                var filtersToSave = docGrid.FilterDefinitions
                    .Select(f => new FilterDefinitionDto
                    {
                        // Get the Field name from the Column property of the FilterDefinition
                        Field = f.Column?.PropertyName ?? string.Empty,
                        Operator = f.Operator!,
                        Value = f.Value!.ToString(), // Convert Value to string for serialization
                        BoolValue = f.Value is bool boolValue ? boolValue : null // Handle boolean values
                    })
                    .ToList();

                var sortsToSave = docGrid.SortDefinitions.Values // Iterate over values of the Dictionary
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
                    var sortColum = docGrid.RenderedColumns.FirstOrDefault(c => c.PropertyName == sortsToSave.FirstOrDefault()!.SortBy)?.Title ?? string.Empty;
                    DocGridStateView.SortColumn = sortColum;
                }

                // Create a DTO to hold the state
                var stateDto = new GridStateDto
                {
                    Filters = filtersToSave,
                    Sorts = sortsToSave,
                    CurrentPage = docGrid.CurrentPage, // Save current page
                    SearchString = _searchString,
                };

                // Save the DTO to local storage
                await LocalStorage.SetItemAsync(GridStateStorageKey, stateDto);

                DocGridStateView.SortDefinitions = stateDto.Sorts
                    .ToDictionary(s => s.SortBy, s => new SortDefinition<Doctor>(
                        s.SortBy,
                        s.Descending,
                        s.Index,
                        (Func<Doctor, object>)s.SortFunc
                    ));



                Console.WriteLine($"State saved to LocalStorage. Filters Count: {filtersToSave.Count}, Sorts Count: {sortsToSave.Count}, Page: {docGrid.CurrentPage}");
            }
            else
            {
                Console.WriteLine("mudDataGrid was null during Dispose.");
            }
        }


        private async Task OnImport()
        {
            string fileUrl = "https://localhost:7190/Doctors.xlsx";
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
                        var DoctorList = new List<Doctor>();

                        if (result.Tables.Count > 0)
                        {

                            DataTable firstSheet = result.Tables[0];
                            foreach (DataRow row in firstSheet.Rows)
                            {
                                Console.WriteLine(string.Join(", ", row.ItemArray));
                                if (row[0]?.ToString() == "MDNo") // Skip header row
                                    continue;
                                var newDoctor = new Doctor();
                                //newDoctor.DoctorId = Guid.NewGuid();
                                newDoctor.MigratedDoctorId = row[0]?.ToString().Trim() ?? string.Empty;
                                if (row[2] == null || string.IsNullOrWhiteSpace(row[2].ToString()))
                                {
                                    newDoctor.FirstName = "Not Entered";
                                }
                                else
                                {
                                    newDoctor.FirstName = row[2]?.ToString().Trim();
                                }
                                newDoctor.MiddleName = row[3]?.ToString().Trim() ?? string.Empty;
                                if (row[1] == null || string.IsNullOrWhiteSpace(row[1].ToString()))
                                {
                                    newDoctor.LastName = "Not Entered";
                                }
                                else
                                {
                                    newDoctor.LastName = row[1]?.ToString().Trim();
                                }
                                newDoctor.Suffix = row[4]?.ToString().Trim() ?? string.Empty;
                                if(row[9] == null || string.IsNullOrWhiteSpace(row[9].ToString()))
                                {
                                    newDoctor.Address1 = "Not Entered";
                                }
                                else
                                {
                                    newDoctor.Address1 = row[9]?.ToString().Trim();
                                }
                                newDoctor.Address2 = row[10]?.ToString().Trim() ?? string.Empty;
                                newDoctor.Address3 = row[11]?.ToString().Trim() ?? string.Empty;
                                if(row[12] == null || string.IsNullOrWhiteSpace(row[12].ToString()))
                                {
                                    newDoctor.City = "Not Entered";
                                }
                                else
                                {
                                    newDoctor.City = row[12]?.ToString().Trim();
                                }
                                if(row[13] == null || string.IsNullOrWhiteSpace(row[13].ToString()))
                                {
                                    newDoctor.State = "Not Entered";
                                }
                                else
                                {
                                    newDoctor.State = row[13]?.ToString().Trim();
                                }
                                if(row[14] == null || string.IsNullOrWhiteSpace(row[14].ToString()))
                                {
                                    newDoctor.County = "Not Entered";
                                }
                                else
                                {
                                    newDoctor.County = row[14]?.ToString().Trim();
                                }
                                if(row[15] == null || string.IsNullOrWhiteSpace(row[15].ToString()))
                                {
                                    newDoctor.ZipCode = "Not Entered";
                                }
                                else
                                {
                                    newDoctor.ZipCode = row[15]?.ToString().Trim();
                                }
                                if(row[16] == null || string.IsNullOrWhiteSpace(row[16].ToString()))
                                {
                                    newDoctor.PhoneNumber1 = "555-555-5555";
                                }
                                else
                                {
                                    newDoctor.PhoneNumber1 = row[16]?.ToString().Trim();
                                }
                                newDoctor.PhoneNumber2 = row[17]?.ToString().Trim() ?? string.Empty;
                                newDoctor.FaxNumber = row[18]?.ToString().Trim() ?? string.Empty;
                                newDoctor.Email = row[23]?.ToString().Trim() ?? string.Empty;
                                newDoctor.ModifiedDate = DateTime.TryParse(row[20]?.ToString().Trim(), out DateTime tempDate) ? tempDate : (DateTime?)null;
                                if (row[7] == null || string.IsNullOrWhiteSpace(row[7].ToString()))
                                {
                                    newDoctor.PrimarySpecialty = "Not Entered";
                                }
                                else
                                {
                                    newDoctor.PrimarySpecialty = row[7]?.ToString().Trim();
                                }
                                newDoctor.SecondarySpecialty = row[8]?.ToString().Trim() ?? string.Empty;
                                if (row[5] == null || string.IsNullOrWhiteSpace(row[5].ToString()))
                                {
                                    newDoctor.LicenseType = "Not Entered";
                                }
                                else
                                {
                                    newDoctor.LicenseType = row[5]?.ToString().Trim();
                                }
                                newDoctor.IsPathologist = (row[6]?.ToString().Trim() ?? string.Empty).ToLower() == "y" ? true : false;
                                newDoctor.DisplayName = $"{newDoctor.FirstName} {newDoctor.LastName}, {newDoctor.LicenseType}";

                                newDoctor.IsActive = true;
                                newDoctor.IsVerified = false;

                                if (newDoctor.County.StartsWith("37"))
                                {
                                    newDoctor.County = await LookupData.GetCountyByFIPSAsync(newDoctor.County);
                                }
                                DoctorList.Add(newDoctor);

                            }
                        }
                        // Add the new doctors to the database
                        foreach (var doctor in DoctorList)
                        {
                           var userId = await UserData.GetIdByEmailAsync("system_user@system.user"); // System User Id
                           var id =  await DoctorData.CreateDoctorAsync(userId, doctor);
                        }

                        // Refresh the list of lookups
                        Doctors = (List<Doctor>)await DoctorData.ListDoctorsAsync(token);
                        await InvokeAsync(StateHasChanged);
                    }
                }
            }
        }

    
    
    
    
    }
}
