using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Http;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Utilities;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.DTOs;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using static MudBlazor.CategoryTypes;
using static MudBlazor.Icons;
using static System.Net.WebRequestMethods;




namespace RCA_StudyManagementSystem.Client.Pages.Studies
{
    public partial class Fields : Microsoft.AspNetCore.Components.ComponentBase
    {

        [Parameter]
        public Study Study { get; set; } = new Study();

        [CascadingParameter]
        public EditContext? EditContext { get; set; }

        [Parameter]
        public bool IsSaved { get; set; } = false;

        private bool IsLoading { get; set; } = true;

        [Parameter]
        public string FormType { get; set; } = string.Empty;

        public MudColor DefaultColor { get; set; } = new MudColor("#FF0000"); // Initialize with a color

        public MudDataGrid<StudyHistologyView> dataHistology;
        public MudDataGrid<StudyContact> contactGrid;

        public CancellationToken CancellationToken { get; set; } = new CancellationToken();

        private IEnumerable<Lookup> lookups = new List<Lookup>();
        private IEnumerable<Histology> histologies = new List<Histology>();
        private IEnumerable<StudyHeader> studyHeaderOptions = new List<StudyHeader>();

        public IEnumerable<string> StudySiteList = new List<string>();

        private List<StudyLookupView> luRace = new List<StudyLookupView>();
        private List<StudyLookupView> luGender = new List<StudyLookupView>();
        private List<StudyLookupView> luEthnicity = new List<StudyLookupView>();
        private List<StudyLookupView> luProcedure = new List<StudyLookupView>();
        private List<StudyLookupView> luSite = new List<StudyLookupView>();
        private List<StudyLookupView> luCounty = new List<StudyLookupView>();
        private List<StudyHistologyView> luHistology = new List<StudyHistologyView>();
        private List<StudyHeaderView> luHeaderOption = new List<StudyHeaderView>();
        private List<StudyReportHeaderView> luReportHeaderOption = new List<StudyReportHeaderView>();



        public HashSet<StudyLookupView> SelectedRace = new HashSet<StudyLookupView>();
        public HashSet<StudyLookupView> SelectedGender = new HashSet<StudyLookupView>();
        public HashSet<StudyLookupView> SelectedEthnicity = new HashSet<StudyLookupView>();
        public HashSet<StudyLookupView> SelectedProcedure = new HashSet<StudyLookupView>();
        public HashSet<StudyLookupView> SelectedSite = new HashSet<StudyLookupView>();
        public HashSet<StudyLookupView> SelectedCounty = new HashSet<StudyLookupView>();
        public HashSet<StudyHistologyView> SelectedHistology = new HashSet<StudyHistologyView>();
        public HashSet<StudyHeaderView> SelectedHeaderOption = new HashSet<StudyHeaderView>();
        public HashSet<StudyReportHeaderView> SelectedReportHeaderOption = new HashSet<StudyReportHeaderView>();

        private List<string> _events = new();
        private string? _searchString;


        [Parameter]
        public List<StudyLookupView> StudyLookupViewsRace { get; set; } = new List<StudyLookupView>();
        [Parameter]
        public List<StudyLookupView> StudyLookupViewsGender { get; set; } = new List<StudyLookupView>();
        [Parameter]
        public List<StudyLookupView> StudyLookupViewsEthnicity { get; set; } = new List<StudyLookupView>();
        [Parameter]
        public List<StudyLookupView> StudyLookupViewsProcedure { get; set; } = new List<StudyLookupView>();
        [Parameter]
        public List<StudyLookupView> StudyLookupViewsSite { get; set; } = new List<StudyLookupView>();
        [Parameter]
        public List<StudyLookupView> StudyLookupViewsCounty { get; set; } = new List<StudyLookupView>();
        [Parameter]
        public List<StudyHistologyView> StudyHistologyViews { get; set; } = new List<StudyHistologyView>();
        [Parameter]
        public List<StudyHeaderView> StudyHeaderViews { get; set; } = new List<StudyHeaderView>();
        [Parameter]
        public List<StudyReportHeaderView> StudyReportHeaderViews { get; set; } = new List<StudyReportHeaderView>();

        [Parameter]
        public EventCallback<List<StudyLookupView>> StudyLookupViewsRaceChanged { get; set; }
        [Parameter]
        public EventCallback<List<StudyLookupView>> StudyLookupViewsGenderChanged { get; set; }
        [Parameter]
        public EventCallback<List<StudyLookupView>> StudyLookupViewsEthnicityChanged { get; set; }
        [Parameter]
        public EventCallback<List<StudyLookupView>> StudyLookupViewsProcedureChanged { get; set; }
        [Parameter]
        public EventCallback<List<StudyLookupView>> StudyLookupViewsSiteChanged { get; set; }
        [Parameter]
        public EventCallback<List<StudyLookupView>> StudyLookupViewsCountyChanged { get; set; }
        [Parameter]
        public EventCallback<List<StudyHistologyView>> StudyHistologyViewsChanged { get; set; }
        [Parameter]
        public EventCallback<List<StudyHeaderView>> StudyHeaderViewsChanged { get; set; }
        [Parameter]
        public EventCallback<List<StudyReportHeaderView>> StudyReportHeaderViewsChanged { get; set; }

        // quick filter - filter globally across multiple columns with the same input
        private Func<StudyHistologyView, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.HistologyCode.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.HistologyName != null && x.HistologyName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        protected override async Task OnInitializedAsync()
        {
            //StudySiteList = await LookupData.ListLookupsByTypeAsync("Study Site");

            EditContext.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events

            //lookups = await LookupData.ListLookupsAsync();
            //histologies = await HistologyData.ListActiveHistologiesAsync();
            //studyHeaderOptions = await PathReportData.GetPathReportHeaderOptionsAsync();

            var taskStudy = LookupData.ListLookupsByTypeAsync("Study Site");
            var taskLookup = LookupData.ListLookupsAsync();
            var taskHistology = HistologyData.ListActiveHistologiesAsync();
            var taskStudyHeader = PathReportData.GetPathReportHeaderOptionsAsync();

            await Task.WhenAll(taskStudy, taskLookup, taskHistology, taskStudyHeader);

            StudySiteList = await taskStudy;
            lookups = await taskLookup;
            histologies = await taskHistology;
            studyHeaderOptions = await taskStudyHeader;


            luRace = lookups.Where(t => t.LookupType == "Race")
                .OrderBy(t => t.SortOrder)
                .Select(t => new StudyLookupView
                {
                    StudyLookupId = Guid.NewGuid(), // Generate a new ID for the view model
                    StudyId = Study.StudyId,
                    LookupId = t.LookupId,
                    LookupName = t.LookupName,
                    Order = t.SortOrder,
                }).ToList();

            SelectItemsRace(StudyLookupViewsRace.Where(x => x.StudyId == Study.StudyId && x.LookupType == "Race").ToList(), luRace);

            luGender = lookups.Where(t => t.LookupType == "Gender")
                .OrderBy(t => t.SortOrder)
                .Select(t => new StudyLookupView
                {
                    StudyLookupId = Guid.NewGuid(), // Generate a new ID for the view model
                    StudyId = Study.StudyId,
                    LookupId = t.LookupId,
                    LookupName = t.LookupName,
                    Order = t.SortOrder,
                }).ToList();

            SelectItemsGender(StudyLookupViewsGender.Where(x => x.StudyId == Study.StudyId && x.LookupType == "Gender").ToList(), luGender);

            luEthnicity = lookups.Where(t => t.LookupType == "Ethnicity")
                .OrderBy(t => t.SortOrder)
                .Select(t => new StudyLookupView
                {
                    StudyLookupId = Guid.NewGuid(), // Generate a new ID for the view model
                    StudyId = Study.StudyId,
                    LookupId = t.LookupId,
                    LookupName = t.LookupName,
                    Order = t.SortOrder,
                }).ToList();

            SelectItemsEthnicity(StudyLookupViewsEthnicity.Where(x => x.StudyId == Study.StudyId && x.LookupType == "Ethnicity").ToList(), luEthnicity);

            luProcedure = lookups.Where(t => t.LookupType == "Procedure")
                .OrderBy(t => t.SortOrder)
                .Select(t => new StudyLookupView
                {
                    StudyLookupId = Guid.NewGuid(), // Generate a new ID for the view model
                    StudyId = Study.StudyId,
                    LookupId = t.LookupId,
                    LookupName = t.LookupName,
                    Order = t.SortOrder,
                }).ToList();

            SelectItemsProcedure(StudyLookupViewsProcedure.Where(x => x.StudyId == Study.StudyId && x.LookupType == "Procedure").ToList(), luProcedure);

            luSite = lookups.Where(t => t.LookupType == "Site")
                .OrderBy(t => t.SortOrder)
                .Select(t => new StudyLookupView
                {
                    StudyLookupId = Guid.NewGuid(), // Generate a new ID for the view model
                    StudyId = Study.StudyId,
                    LookupId = t.LookupId,
                    LookupName = t.LookupName,
                    LookupCode = t.LookupCode,
                    Order = t.SortOrder,
                }).ToList();

            SelectItemsSite(StudyLookupViewsSite.Where(x => x.StudyId == Study.StudyId && x.LookupType == "Site").ToList(), luSite);


            luCounty = lookups.Where(t => t.LookupType == "County")
                .OrderBy(t => t.SortOrder)
                .Select(t => new StudyLookupView
                {
                    StudyLookupId = Guid.NewGuid(), // Generate a new ID for the view model
                    StudyId = Study.StudyId,
                    LookupId = t.LookupId,
                    LookupName = t.LookupName,
                    Order = t.SortOrder,
                }).ToList();

            SelectItemsCounty(StudyLookupViewsCounty.Where(x => x.StudyId == Study.StudyId && x.LookupType == "County").ToList(), luCounty);

            luHistology = histologies
                .Select(t => new StudyHistologyView
                {
                    StudyHistologyId = Guid.NewGuid(), // Generate a new ID for the view model
                    StudyId = Study.StudyId,
                    HistologyId = t.HistologyId,
                    HistologyCode = t.HistologyCode,
                    HistologyBehavior = t.HistologyBehavior,
                    HistologyName = t.HistologyName,
                    IsPreferred = t.IsPreferred,
                    IsActive = t.IsActive,
                    Order = t.SortOrder,
                }).ToList();

            SelectItemsHistology(StudyHistologyViews.Where(x => x.StudyId == Study.StudyId).ToList(), luHistology);

            luHeaderOption = studyHeaderOptions
                .Where(t => t.IsActive) // Only include active header options
                .Select(t => new StudyHeaderView
                {
                    StudyHeaderId = Guid.NewGuid(), // Generate a new ID for the view model
                    StudyId = Study.StudyId,
                    HeaderName = t.HeaderName,
                    TableName = t.TableName,
                    Order = t.Order,
                }).ToList();

            SelectItemsHeaderOption(StudyHeaderViews.Where(x => x.StudyId == Study.StudyId).ToList(), luHeaderOption);

            luReportHeaderOption = studyHeaderOptions
                .Where(t => t.IsActive) // Only include active header options
                .Select(t => new StudyReportHeaderView
                {
                    StudyReportHeaderId = Guid.NewGuid(), // Generate a new ID for the view model
                    StudyId = Study.StudyId,
                    HeaderName = t.HeaderName,
                    TableName = t.TableName,
                    Order = t.Order,
                }).ToList();

            SelectItemsReportHeaderOption(StudyReportHeaderViews.Where(x => x.StudyId == Study.StudyId).ToList(), luReportHeaderOption);

            IsLoading = false;

            await InvokeAsync(StateHasChanged);
        }

        private void SetColor(string color)
        {
            // Set the color for the study
            Study.MudColorLight = new MudColor(color);
            DefaultColor = new MudColor(color); // Update the DefaultColor property
            Study.ColorLight = Study.MudColorLight.Value;

        }

        private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
        {
            IsSaved = false;

        }

        private async Task HandleInternalNavigation(LocationChangingContext context)
        {
            if (EditContext.IsModified())
            {
                var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "You have unsaved changes. Do you want to leave?");
                if (!confirmed)
                {
                    context.PreventNavigation();
                }
            }
        }

        public void OnStartDateChanged(DateTime? date)
        {
            Study.StartDate = date;
        }

        public void OnEndDateChanged(DateTime? date)
        {
            Study.EndDate = date;
        }


        private async Task AddNewContact()
        {
            var newItem = new StudyContact();

            newItem.StudyId = Study.StudyId;

            Study.StudyContacts.Add(newItem); // Add to your data source
            await InvokeAsync(StateHasChanged);
            await Task.Delay(100); // Give the UI a moment to update
            //await dataGrid.SetEditingItemAsync(newItem); // Start inline editing - shows modal
        }

        private async Task DeleteContact(StudyContact item)
        {
            if (item.LastName == "" && item.FirstName == "")
            {
                Study.StudyContacts.Remove(item); // Remove from data source
                await InvokeAsync(StateHasChanged);
                return; // Exit if the phone number is empty
            }

            bool? result = await DialogService.ShowMessageBoxAsync(
                          "Warning", // Dialog title
                          $"Are you sure you want to delete this contact? Save the record to complete deletion.", // Message
                          yesText: "Delete!", // Text for the confirmation button
                          cancelText: "Cancel" // Text for the cancel button
                      );

            if (result == true) // User clicked 'Delete!'
            {
                Study.StudyContacts.Remove(item); // Remove from data source
                await InvokeAsync(StateHasChanged);
            }

        }

        public async Task OnOrderRaceChanged(StudyLookupView item)
        {
            await StudyLookupViewsRaceChanged.InvokeAsync(SelectedRace.ToList());
        }

        public async Task SelectItemsRace(List<StudyLookupView> items, List<StudyLookupView> uiItems)
        {
            if (items.Count == 0)
            {
                return;
            }
            foreach (var item in items)
            {
                var uiItem = uiItems.Where(x => x.LookupName == item.LookupName).FirstOrDefault();
                if (!SelectedRace.Contains(uiItem))
                {
                    uiItem.IsSelected = true; // Mark the item as selected
                    uiItem.Order = item.Order; // Set the order from the original item
                    SelectedRace.Add(uiItem);
                }

            }
            await InvokeAsync(StateHasChanged); // Refresh the UI to reflect the change
        }

        public async Task SelectedItemRaceChanged()
        {

            await StudyLookupViewsRaceChanged.InvokeAsync(SelectedRace.ToList());

        }

        public async Task OnSelectAllRaceChanged()
        {
            await StudyLookupViewsRaceChanged.InvokeAsync(SelectedRace.ToList());

        }

        public async Task OnOrderGenderChanged(StudyLookupView item)
        {
            await StudyLookupViewsGenderChanged.InvokeAsync(SelectedGender.ToList());
        }

        public async Task SelectItemsGender(List<StudyLookupView> items, List<StudyLookupView> uiItems)
        {
            if (items.Count == 0)
            {
                return;
            }
            foreach (var item in items)
            {
                var uiItem = uiItems.Where(x => x.LookupName == item.LookupName).FirstOrDefault();
                if (!SelectedGender.Contains(uiItem))
                {
                    uiItem.IsSelected = true; // Mark the item as selected
                    uiItem.Order = item.Order; // Set the order from the original item
                    SelectedGender.Add(uiItem);
                }

            }
            await InvokeAsync(StateHasChanged); // Refresh the UI to reflect the change
        }

        public async Task SelectedItemGenderChanged()
        {

            await StudyLookupViewsGenderChanged.InvokeAsync(SelectedGender.ToList());

        }

        public async Task OnSelectAllGenderChanged()
        {
            await StudyLookupViewsGenderChanged.InvokeAsync(SelectedGender.ToList());

        }

        public async Task OnOrderEthnicityChanged(StudyLookupView item)
        {
            await StudyLookupViewsEthnicityChanged.InvokeAsync(SelectedEthnicity.ToList());
        }

        public async Task SelectItemsEthnicity(List<StudyLookupView> items, List<StudyLookupView> uiItems)
        {
            if (items.Count == 0)
            {
                return;
            }
            foreach (var item in items)
            {
                var uiItem = uiItems.Where(x => x.LookupName == item.LookupName).FirstOrDefault();
                if (!SelectedEthnicity.Contains(uiItem))
                {
                    uiItem.IsSelected = true; // Mark the item as selected
                    uiItem.Order = item.Order; // Set the order from the original item
                    SelectedEthnicity.Add(uiItem);
                }

            }
            await InvokeAsync(StateHasChanged); // Refresh the UI to reflect the change
        }

        public async Task SelectedItemEthnicityChanged()
        {

            await StudyLookupViewsEthnicityChanged.InvokeAsync(SelectedEthnicity.ToList());

        }

        public async Task OnSelectAllEthnicityChanged()
        {
            await StudyLookupViewsEthnicityChanged.InvokeAsync(SelectedEthnicity.ToList());

        }

        public async Task OnOrderProcedureChanged(StudyLookupView item)
        {
            await StudyLookupViewsProcedureChanged.InvokeAsync(SelectedProcedure.ToList());
        }

        public async Task SelectItemsProcedure(List<StudyLookupView> items, List<StudyLookupView> uiItems)
        {
            if (items.Count == 0)
            {
                return;
            }
            foreach (var item in items)
            {
                var uiItem = uiItems.Where(x => x.LookupName == item.LookupName).FirstOrDefault();
                if (!SelectedProcedure.Contains(uiItem))
                {
                    uiItem.IsSelected = true; // Mark the item as selected
                    uiItem.Order = item.Order; // Set the order from the original item
                    SelectedProcedure.Add(uiItem);
                }

            }
            await InvokeAsync(StateHasChanged); // Refresh the UI to reflect the change
        }

        public async Task SelectedItemProcedureChanged()
        {

            await StudyLookupViewsProcedureChanged.InvokeAsync(SelectedProcedure.ToList());

        }

        public async Task OnSelectAllProcedureChanged()
        {
            await StudyLookupViewsProcedureChanged.InvokeAsync(SelectedProcedure.ToList());

        }

        public async Task OnOrderSiteChanged(StudyLookupView item)
        {
            await StudyLookupViewsSiteChanged.InvokeAsync(SelectedSite.ToList());
        }

        public async Task SelectItemsSite(List<StudyLookupView> items, List<StudyLookupView> uiItems)
        {
            if (items.Count == 0)
            {
                return;
            }
            foreach (var item in items)
            {
                var uiItem = uiItems.Where(x => x.LookupName == item.LookupName).FirstOrDefault();
                if (!SelectedSite.Contains(uiItem))
                {
                    uiItem.IsSelected = true; // Mark the item as selected
                    uiItem.Order = item.Order; // Set the order from the original item
                    SelectedSite.Add(uiItem);
                }

            }
            await InvokeAsync(StateHasChanged); // Refresh the UI to reflect the change
        }

        public async Task SelectedItemSiteChanged()
        {

            await StudyLookupViewsSiteChanged.InvokeAsync(SelectedSite.ToList());

        }

        public async Task OnSelectAllSiteChanged()
        {
            await StudyLookupViewsSiteChanged.InvokeAsync(SelectedSite.ToList());

        }

        public async Task OnOrderCountyChanged(StudyLookupView item)
        {
            await StudyLookupViewsCountyChanged.InvokeAsync(SelectedCounty.ToList());
        }

        public async Task SelectItemsCounty(List<StudyLookupView> items, List<StudyLookupView> uiItems)
        {
            if (items.Count == 0)
            {
                return;
            }
            foreach (var item in items)
            {
                var uiItem = uiItems.Where(x => x.LookupName == item.LookupName).FirstOrDefault();
                if (!SelectedCounty.Contains(uiItem))
                {
                    uiItem.IsSelected = true; // Mark the item as selected
                    uiItem.Order = item.Order; // Set the order from the original item
                    SelectedCounty.Add(uiItem);
                }

            }
            await InvokeAsync(StateHasChanged); // Refresh the UI to reflect the change
        }

        public async Task SelectedItemCountyChanged()
        {

            await StudyLookupViewsCountyChanged.InvokeAsync(SelectedCounty.ToList());

        }

        public async Task OnSelectAllCountyChanged()
        {
            await StudyLookupViewsCountyChanged.InvokeAsync(SelectedCounty.ToList());

        }

        public async Task OnOrderHistologyChanged(StudyHistologyView item)
        {
            await StudyHistologyViewsChanged.InvokeAsync(SelectedHistology.ToList());
        }

        public async Task SelectItemsHistology(List<StudyHistologyView> items, List<StudyHistologyView> uiItems)
        {
            if (items.Count == 0)
            {
                return;
            }
            foreach (var item in items)
            {
                var uiItem = uiItems.Where(x => x.HistologyName == item.HistologyName).FirstOrDefault();
                if (!SelectedHistology.Contains(uiItem))
                {
                    uiItem.IsSelected = true; // Mark the item as selected
                    uiItem.Order = item.Order; // Set the order from the original item
                    SelectedHistology.Add(uiItem);
                }

            }
            var SelectedHistologyList = SelectedHistology.OrderBy(x => x.HistologyCode).ThenBy(x => x.HistologyBehavior).ThenBy(x => x.HistologyName).ToList(); // Ensure the selected items are ordered by HistologyCode
            SelectedHistology = new HashSet<StudyHistologyView>(SelectedHistologyList); // Update the SelectedHistology with the ordered list
            await InvokeAsync(StateHasChanged); // Refresh the UI to reflect the change
        }

        public async Task SelectedItemHistologyChanged()
        {

            await StudyHistologyViewsChanged.InvokeAsync(SelectedHistology.ToList());

        }

        public async Task OnSelectAllHistologyChanged()
        {
            await StudyHistologyViewsChanged.InvokeAsync(SelectedHistology.ToList());

        }

        public async Task OnOrderHeaderOptionChanged(StudyHeaderView item)
        {
            await StudyHeaderViewsChanged.InvokeAsync(SelectedHeaderOption.ToList());
        }

        public async Task SelectItemsHeaderOption(List<StudyHeaderView> items, List<StudyHeaderView> uiItems)
        {
            if (items.Count == 0)
            {
                // Get all public instance properties of the specified type T
                var propPatient = typeof(Patient).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var propPathReport = typeof(PathReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                // Filter the properties to find those with the [Required] attribute
                var reqPatient = propPatient.Where(p => p.IsDefined(typeof(RequiredAttribute), false));
                var reqPathReport = propPathReport.Where(p => p.IsDefined(typeof(RequiredAttribute), false));

                // Ensure all required fields are selected by default
                foreach (var req in reqPatient)
                {
                    var requiredItem = uiItems.Where(x => x.HeaderName == req.Name).FirstOrDefault();
                    if (requiredItem != null && !SelectedHeaderOption.Contains(requiredItem))
                    {
                        requiredItem.IsSelected = true;
                        SelectedHeaderOption.Add(requiredItem);
                    }
                }
                foreach (var req in reqPathReport)
                {
                    var requiredItem = uiItems.Where(x => x.HeaderName == req.Name).FirstOrDefault();
                    if (requiredItem != null && !SelectedHeaderOption.Contains(requiredItem))
                    {
                        requiredItem.IsSelected = true;
                        SelectedHeaderOption.Add(requiredItem);
                    }
                }

                // Ensure all phone number fields are selected by default
                var phoneNumberItem = uiItems.Where(x => x.HeaderName == "PhoneNumber").FirstOrDefault();
                if (phoneNumberItem != null && !SelectedHeaderOption.Contains(phoneNumberItem))
                {
                    phoneNumberItem.IsSelected = true; // Ensure PhoneNumber is always selected
                    SelectedHeaderOption.Add(phoneNumberItem);
                }
                var phoneTypeItem = uiItems.Where(x => x.HeaderName == "PhoneType").FirstOrDefault();
                if (phoneTypeItem != null && !SelectedHeaderOption.Contains(phoneTypeItem))
                {
                    phoneTypeItem.IsSelected = true; // Ensure PhoneNumberType is always selected
                    SelectedHeaderOption.Add(phoneTypeItem);
                }
                var phoneNumberCommentsItem = uiItems.Where(x => x.HeaderName == "PhoneNumberComments").FirstOrDefault();
                if (phoneNumberCommentsItem != null && !SelectedHeaderOption.Contains(phoneNumberCommentsItem))
                {
                    phoneNumberCommentsItem.IsSelected = true; // Ensure PhoneNumberComments is always selected
                    SelectedHeaderOption.Add(phoneNumberCommentsItem);
                }
                var phoneNumberIsPrimaryItem = uiItems.Where(x => x.HeaderName == "IsPrimary").FirstOrDefault();
                if (phoneNumberIsPrimaryItem != null && !SelectedHeaderOption.Contains(phoneNumberIsPrimaryItem))
                {
                    phoneNumberIsPrimaryItem.IsSelected = true; // Ensure IsPrimary is always selected
                    SelectedHeaderOption.Add(phoneNumberIsPrimaryItem);
                }
                return;
            }

            foreach (var item in items)
            {
                var uiItem = uiItems.Where(x => x.HeaderName == item.HeaderName).FirstOrDefault();
                if (!SelectedHeaderOption.Contains(uiItem))
                {
                    uiItem.IsSelected = true; // Mark the item as selected
                    uiItem.Order = item.Order; // Set the order from the original item
                    uiItem.ExportTitle = item.ExportTitle; // Set the ExportTitle from the original item
                    uiItem.TableName = item.TableName; // Set the TableName from the original item
                    SelectedHeaderOption.Add(uiItem);
                }
            }

            await InvokeAsync(StateHasChanged); // Refresh the UI to reflect the change
        }

        public async Task SelectedItemHeaderOptionChanged()
        {
            var phoneNumberItem = luHeaderOption.Where(x => x.HeaderName == "PhoneNumber").FirstOrDefault();
            if (phoneNumberItem != null && !SelectedHeaderOption.Contains(phoneNumberItem))
            {
                phoneNumberItem.IsSelected = true; // Ensure PhoneNumber is always selected
                SelectedHeaderOption.Add(phoneNumberItem);
            }
            var phoneTypeItem = luHeaderOption.Where(x => x.HeaderName == "PhoneType").FirstOrDefault();
            if (phoneTypeItem != null && !SelectedHeaderOption.Contains(phoneTypeItem))
            {
                phoneTypeItem.IsSelected = true; // Ensure PhoneNumberType is always selected
                SelectedHeaderOption.Add(phoneTypeItem);
            }
            var phoneNumberCommentsItem = luHeaderOption.Where(x => x.HeaderName == "PhoneNumberComments").FirstOrDefault();
            if (phoneNumberCommentsItem != null && !SelectedHeaderOption.Contains(phoneNumberCommentsItem))
            {
                phoneNumberCommentsItem.IsSelected = true; // Ensure PhoneNumberComments is always selected
                SelectedHeaderOption.Add(phoneNumberCommentsItem);
            }
            var phoneNumberIsPrimaryItem = luHeaderOption.Where(x => x.HeaderName == "IsPrimary").FirstOrDefault();
            if (phoneNumberIsPrimaryItem != null && !SelectedHeaderOption.Contains(phoneNumberIsPrimaryItem))
            {
                phoneNumberIsPrimaryItem.IsSelected = true; // Ensure IsPrimary is always selected
                SelectedHeaderOption.Add(phoneNumberIsPrimaryItem);
            }

            await StudyHeaderViewsChanged.InvokeAsync(SelectedHeaderOption.ToList());

        }

        public async Task OnSelectAllHeaderOptionChanged()
        {
            var phoneNumberItem = luHeaderOption.Where(x => x.HeaderName == "PhoneNumber").FirstOrDefault();
            if (phoneNumberItem != null && !SelectedHeaderOption.Contains(phoneNumberItem))
            {
                phoneNumberItem.IsSelected = true; // Ensure PhoneNumber is always selected
                SelectedHeaderOption.Add(phoneNumberItem);
            }
            var phoneTypeItem = luHeaderOption.Where(x => x.HeaderName == "PhoneType").FirstOrDefault();
            if (phoneTypeItem != null && !SelectedHeaderOption.Contains(phoneTypeItem))
            {
                phoneTypeItem.IsSelected = true; // Ensure PhoneNumberType is always selected
                SelectedHeaderOption.Add(phoneTypeItem);
            }
            var phoneNumberCommentsItem = luHeaderOption.Where(x => x.HeaderName == "PhoneNumberComments").FirstOrDefault();
            if (phoneNumberCommentsItem != null && !SelectedHeaderOption.Contains(phoneNumberCommentsItem))
            {
                phoneNumberCommentsItem.IsSelected = true; // Ensure PhoneNumberComments is always selected
                SelectedHeaderOption.Add(phoneNumberCommentsItem);
            }
            var phoneNumberIsPrimaryItem = luHeaderOption.Where(x => x.HeaderName == "IsPrimary").FirstOrDefault();
            if (phoneNumberIsPrimaryItem != null && !SelectedHeaderOption.Contains(phoneNumberIsPrimaryItem))
            {
                phoneNumberIsPrimaryItem.IsSelected = true; // Ensure IsPrimary is always selected
                SelectedHeaderOption.Add(phoneNumberIsPrimaryItem);
            }
            await StudyHeaderViewsChanged.InvokeAsync(SelectedHeaderOption.ToList());

        }

        public async Task OnOrderReportHeaderOptionChanged(StudyReportHeaderView item)
        {
            await StudyReportHeaderViewsChanged.InvokeAsync(SelectedReportHeaderOption.ToList());
        }

        public async Task SelectItemsReportHeaderOption(List<StudyReportHeaderView> items, List<StudyReportHeaderView> uiItems)
        {
            if (items.Count == 0)
            {
                // Get all public instance properties of the specified type T
                var propPatient = typeof(Patient).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var propPathReport = typeof(PathReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                // Filter the properties to find those with the [Required] attribute
                var reqPatient = propPatient.Where(p => p.IsDefined(typeof(RequiredAttribute), false));
                var reqPathReport = propPathReport.Where(p => p.IsDefined(typeof(RequiredAttribute), false));

                // Ensure all required fields are selected by default
                foreach (var req in reqPatient)
                {
                    var requiredItem = uiItems.Where(x => x.HeaderName == req.Name).FirstOrDefault();
                    if (requiredItem != null && !SelectedReportHeaderOption.Contains(requiredItem))
                    {
                        requiredItem.IsSelected = true;
                        SelectedReportHeaderOption.Add(requiredItem);
                    }
                }
                foreach (var req in reqPathReport)
                {
                    var requiredItem = uiItems.Where(x => x.HeaderName == req.Name).FirstOrDefault();
                    if (requiredItem != null && !SelectedReportHeaderOption.Contains(requiredItem))
                    {
                        requiredItem.IsSelected = true;
                        SelectedReportHeaderOption.Add(requiredItem);
                    }
                }

                // Ensure all phone number fields are selected by default
                var phoneNumberItem = uiItems.Where(x => x.HeaderName == "PhoneNumber").FirstOrDefault();
                if (phoneNumberItem != null && !SelectedReportHeaderOption.Contains(phoneNumberItem))
                {
                    phoneNumberItem.IsSelected = true; // Ensure PhoneNumber is always selected
                    SelectedReportHeaderOption.Add(phoneNumberItem);
                }
                var phoneTypeItem = uiItems.Where(x => x.HeaderName == "PhoneType").FirstOrDefault();
                if (phoneTypeItem != null && !SelectedReportHeaderOption.Contains(phoneTypeItem))
                {
                    phoneTypeItem.IsSelected = true; // Ensure PhoneNumberType is always selected
                    SelectedReportHeaderOption.Add(phoneTypeItem);
                }
                var phoneNumberCommentsItem = uiItems.Where(x => x.HeaderName == "PhoneNumberComments").FirstOrDefault();
                if (phoneNumberCommentsItem != null && !SelectedReportHeaderOption.Contains(phoneNumberCommentsItem))
                {
                    phoneNumberCommentsItem.IsSelected = true; // Ensure PhoneNumberComments is always selected
                    SelectedReportHeaderOption.Add(phoneNumberCommentsItem);
                }
                var phoneNumberIsPrimaryItem = uiItems.Where(x => x.HeaderName == "IsPrimary").FirstOrDefault();
                if (phoneNumberIsPrimaryItem != null && !SelectedReportHeaderOption.Contains(phoneNumberIsPrimaryItem))
                {
                    phoneNumberIsPrimaryItem.IsSelected = true; // Ensure IsPrimary is always selected
                    SelectedReportHeaderOption.Add(phoneNumberIsPrimaryItem);
                }
                return;
            }

            foreach (var item in items)
            {
                var uiItem = uiItems.Where(x => x.HeaderName == item.HeaderName).FirstOrDefault();
                if (!SelectedReportHeaderOption.Contains(uiItem))
                {
                    uiItem.IsSelected = true; // Mark the item as selected
                    uiItem.Order = item.Order; // Set the order from the original item
                    uiItem.ExportTitle = item.ExportTitle; // Set the ExportTitle from the original item
                    uiItem.TableName = item.TableName; // Set the TableName from the original item
                    SelectedReportHeaderOption.Add(uiItem);
                }
            }

            await InvokeAsync(StateHasChanged); // Refresh the UI to reflect the change
        }

        public async Task SelectedItemReportHeaderOptionChanged()
        {
            var phoneNumberItem = luReportHeaderOption.Where(x => x.HeaderName == "PhoneNumber").FirstOrDefault();
            if (phoneNumberItem != null && !SelectedReportHeaderOption.Contains(phoneNumberItem))
            {
                phoneNumberItem.IsSelected = true; // Ensure PhoneNumber is always selected
                SelectedReportHeaderOption.Add(phoneNumberItem);
            }
            var phoneTypeItem = luReportHeaderOption.Where(x => x.HeaderName == "PhoneType").FirstOrDefault();
            if (phoneTypeItem != null && !SelectedReportHeaderOption.Contains(phoneTypeItem))
            {
                phoneTypeItem.IsSelected = true; // Ensure PhoneNumberType is always selected
                SelectedReportHeaderOption.Add(phoneTypeItem);
            }
            var phoneNumberCommentsItem = luReportHeaderOption.Where(x => x.HeaderName == "PhoneNumberComments").FirstOrDefault();
            if (phoneNumberCommentsItem != null && !SelectedReportHeaderOption.Contains(phoneNumberCommentsItem))
            {
                phoneNumberCommentsItem.IsSelected = true; // Ensure PhoneNumberComments is always selected
                SelectedReportHeaderOption.Add(phoneNumberCommentsItem);
            }
            var phoneNumberIsPrimaryItem = luReportHeaderOption.Where(x => x.HeaderName == "IsPrimary").FirstOrDefault();
            if (phoneNumberIsPrimaryItem != null && !SelectedReportHeaderOption.Contains(phoneNumberIsPrimaryItem))
            {
                phoneNumberIsPrimaryItem.IsSelected = true; // Ensure IsPrimary is always selected
                SelectedReportHeaderOption.Add(phoneNumberIsPrimaryItem);
            }

            await StudyReportHeaderViewsChanged.InvokeAsync(SelectedReportHeaderOption.ToList());

        }

        public async Task OnSelectAllReportHeaderOptionChanged()
        {
            var phoneNumberItem = luReportHeaderOption.Where(x => x.HeaderName == "PhoneNumber").FirstOrDefault();
            if (phoneNumberItem != null && !SelectedReportHeaderOption.Contains(phoneNumberItem))
            {
                phoneNumberItem.IsSelected = true; // Ensure PhoneNumber is always selected
                SelectedReportHeaderOption.Add(phoneNumberItem);
            }
            var phoneTypeItem = luReportHeaderOption.Where(x => x.HeaderName == "PhoneType").FirstOrDefault();
            if (phoneTypeItem != null && !SelectedReportHeaderOption.Contains(phoneTypeItem))
            {
                phoneTypeItem.IsSelected = true; // Ensure PhoneNumberType is always selected
                SelectedReportHeaderOption.Add(phoneTypeItem);
            }
            var phoneNumberCommentsItem = luReportHeaderOption.Where(x => x.HeaderName == "PhoneNumberComments").FirstOrDefault();
            if (phoneNumberCommentsItem != null && !SelectedReportHeaderOption.Contains(phoneNumberCommentsItem))
            {
                phoneNumberCommentsItem.IsSelected = true; // Ensure PhoneNumberComments is always selected
                SelectedReportHeaderOption.Add(phoneNumberCommentsItem);
            }
            var phoneNumberIsPrimaryItem = luReportHeaderOption.Where(x => x.HeaderName == "IsPrimary").FirstOrDefault();
            if (phoneNumberIsPrimaryItem != null && !SelectedReportHeaderOption.Contains(phoneNumberIsPrimaryItem))
            {
                phoneNumberIsPrimaryItem.IsSelected = true; // Ensure IsPrimary is always selected
                SelectedReportHeaderOption.Add(phoneNumberIsPrimaryItem);
            }
            await StudyReportHeaderViewsChanged.InvokeAsync(SelectedReportHeaderOption.ToList());

        }

        private async Task OnIsSelectedChanged(bool newValue, StudyHistologyView item)
        {
            var exists = SelectedHistology.Any(x => x.HistologyId == item.HistologyId);
            if (newValue == true && !exists)
            {
                SelectedHistology.Add(item);
            }
            else if (newValue == false && exists)
            {
                var toRemove = SelectedHistology.Where(x => x.HistologyId == item.HistologyId).FirstOrDefault();
                SelectedHistology.Remove(toRemove);
            }
            if (newValue == true)
            {
                item.IsSelected = true;
            }
            else
            {
                item.IsSelected = false;
            }
            await StudyHistologyViewsChanged.InvokeAsync(SelectedHistology.ToList());
            var SelectedHistologyList = SelectedHistology.OrderBy(x => x.HistologyCode).ThenBy(x => x.HistologyBehavior).ThenBy(x => x.HistologyName).ToList(); // Ensure the selected items are ordered by HistologyCode
            SelectedHistology = new HashSet<StudyHistologyView>(SelectedHistologyList); // Update the SelectedHistology with the ordered list
        }

        private async Task OnExportFields(Study Study)
        {
            var exportData = await PathReportData.ExportPathReportDataAsync(Study.StudyId, "Current", Guid.Empty, null, false);


            if (exportData != null)
            {
                // Call JavaScript function to download CSV
                await JSRuntime.InvokeVoidAsync("downloadFile", $"{Study.Prefix}_{DateTime.Now.ToString("yyyy-MM-dd")}.csv", exportData);
            }
            else
            {
                Snackbar.Add("No data available for export.", Severity.Warning);
            }
        }
    }
}
