using RCA_StudyManagementSystem.Shared.Domain;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.AspNetCore.Components.Forms;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.ViewModels;

namespace RCA_StudyManagementSystem.Client.Pages.Studies
{
    public partial class Edit : Microsoft.AspNetCore.Components.ComponentBase
    {
        [Parameter]
        public Study Study { get; set; } = new Study();

        public StudyHeader StudyHeader { get; set; }

        [CascadingParameter]
        private EditContext? EditContext { get; set; }

        private bool _isSaved = false; // Backing field for IsSaved property

        private List<StudyLookupView> StudyLookupViewsRace = new List<StudyLookupView>();
        private List<StudyLookupView> StudyLookupViewsGender = new List<StudyLookupView>();
        private List<StudyLookupView> StudyLookupViewsEthnicity = new List<StudyLookupView>();
        private List<StudyLookupView> StudyLookupViewsProcedure = new List<StudyLookupView>();
        private List<StudyLookupView> StudyLookupViewsSite = new List<StudyLookupView>();
        private List<StudyLookupView> StudyLookupViewsCounty = new List<StudyLookupView>();
        private List<StudyHistologyView> StudyHistologyViews = new List<StudyHistologyView>();
        private List<StudyHeaderView> StudyHeaderViews = new List<StudyHeaderView>();
        private List<StudyReportHeaderView> StudyReportHeaderViews = new List<StudyReportHeaderView>();

        [Parameter]
        public bool IsSaved
        {
            get => _isSaved; // Expose IsSaved for the form
            set
            {
                if (value == null)
                {
                    _isSaved = false; // Default to false if value is null
                }
                else
                {
                    // Ensure that the value is a boolean
                    if (value.GetType() != typeof(bool))
                    {
                        throw new ArgumentException("IsSaved must be a boolean value.");
                    }
                }
                _isSaved = value; // Set IsSaved when the parameter is set
            }
        }


        [Parameter]
        public Guid StudyId
        {
            get => Study.StudyId; // Expose StudyId for the form
            set
            {
                Study.StudyId = value; // Set StudyId when the parameter is set
            }
        }

        private Severity severity = Severity.Success;

        protected string SaveMessage = string.Empty;
        private string? apiResponse = string.Empty;

        protected bool HasErrors = false; // Flag to indicate if there are validation errors
        protected string slideSelectValue = "Yes"; // Default value for the slide select
        protected int studySelectValue = 1; // Default value for the study select

        private readonly DialogOptions _noHeader = new() { NoHeader = true };
        private MudDialog MudDialog { get; set; }


        protected override async Task OnInitializedAsync()
        {
            EditContext = new EditContext(new Study()); // Initialize editContext with the Study
            EditContext.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events
            try
            {
                var response = await StudyData.GetStudyAsync(StudyId);
                Study = response ?? new Study(); // Initialize Study with the response or an empty Study if null
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching study: {ex.Message}");
                Study = new Study(); // Initialize with an empty Study on error
            }

            EditContext = new EditContext(Study);
            EditContext.OnFieldChanged += HandleFieldChanged; // Subscribe to field change events

            // Load StudyLookupViews based on the current Study
            StudyLookupViewsRace = Study.StudyLookups!
                .Where(sl => sl.Lookup.LookupId == sl.LookupId && sl.Lookup.LookupType == "Race" && sl.Lookup.IsActive==true)
                .Select(sl => new StudyLookupView
                {
                    StudyLookupId = sl.StudyLookupId,
                    StudyId = sl.StudyId,
                    LookupId = sl.LookupId,
                    LookupCode = sl.Lookup?.LookupCode ?? string.Empty, // Ensure LookupCode is not null
                    LookupName = sl.Lookup?.LookupName ?? string.Empty, // Ensure LookupName is not null
                    LookupType = sl.Lookup?.LookupType ?? string.Empty, // Ensure LookupType is not null
                    Order = sl.Order
                })
                .ToList();

            StudyLookupViewsGender = Study.StudyLookups!
               .Where(sl => sl.Lookup.LookupId == sl.LookupId && sl.Lookup.LookupType == "Gender" && sl.Lookup.IsActive == true)
               .Select(sl => new StudyLookupView
               {
                   StudyLookupId = sl.StudyLookupId,
                   StudyId = sl.StudyId,
                   LookupId = sl.LookupId,
                   LookupCode = sl.Lookup?.LookupCode ?? string.Empty, // Ensure LookupCode is not null
                   LookupName = sl.Lookup?.LookupName ?? string.Empty, // Ensure LookupName is not null
                   LookupType = sl.Lookup?.LookupType ?? string.Empty, // Ensure LookupType is not null
                   Order = sl.Order
               })
               .ToList();

            StudyLookupViewsEthnicity = Study.StudyLookups!
               .Where(sl => sl.Lookup.LookupId == sl.LookupId && sl.Lookup.LookupType == "Ethnicity" && sl.Lookup.IsActive == true)
               .Select(sl => new StudyLookupView
               {
                   StudyLookupId = sl.StudyLookupId,
                   StudyId = sl.StudyId,
                   LookupId = sl.LookupId,
                   LookupCode = sl.Lookup?.LookupCode ?? string.Empty, // Ensure LookupCode is not null
                   LookupName = sl.Lookup?.LookupName ?? string.Empty, // Ensure LookupName is not null
                   LookupType = sl.Lookup?.LookupType ?? string.Empty, // Ensure LookupType is not null
                   Order = sl.Order
               })
               .ToList();

            StudyLookupViewsProcedure = Study.StudyLookups!
               .Where(sl => sl.Lookup.LookupId == sl.LookupId && sl.Lookup.LookupType == "Procedure" && sl.Lookup.IsActive == true)
               .Select(sl => new StudyLookupView
               {
                   StudyLookupId = sl.StudyLookupId,
                   StudyId = sl.StudyId,
                   LookupId = sl.LookupId,
                   LookupCode = sl.Lookup?.LookupCode ?? string.Empty, // Ensure LookupCode is not null
                   LookupName = sl.Lookup?.LookupName ?? string.Empty, // Ensure LookupName is not null
                   LookupType = sl.Lookup?.LookupType ?? string.Empty, // Ensure LookupType is not null
                   Order = sl.Order
               })
               .ToList();

            StudyLookupViewsSite= Study.StudyLookups!
               .Where(sl => sl.Lookup.LookupId == sl.LookupId && sl.Lookup.LookupType == "Site" && sl.Lookup.IsActive == true)
               .Select(sl => new StudyLookupView
               {
                   StudyLookupId = sl.StudyLookupId,
                   StudyId = sl.StudyId,
                   LookupId = sl.LookupId,
                   LookupCode = sl.Lookup?.LookupCode ?? string.Empty, // Ensure LookupCode is not null
                   LookupName = sl.Lookup?.LookupName ?? string.Empty, // Ensure LookupName is not null
                   LookupType = sl.Lookup?.LookupType ?? string.Empty, // Ensure LookupType is not null
                   Order = sl.Order
               })
               .ToList();

            StudyLookupViewsCounty = Study.StudyLookups!
               .Where(sl => sl.Lookup.LookupId == sl.LookupId && sl.Lookup.LookupType == "County" && sl.Lookup.IsActive == true)
               .Select(sl => new StudyLookupView
               {
                   StudyLookupId = sl.StudyLookupId,
                   StudyId = sl.StudyId,
                   LookupId = sl.LookupId,
                   LookupCode = sl.Lookup?.LookupCode ?? string.Empty, // Ensure LookupCode is not null
                   LookupName = sl.Lookup?.LookupName ?? string.Empty, // Ensure LookupName is not null
                   LookupType = sl.Lookup?.LookupType ?? string.Empty, // Ensure LookupType is not null
                   Order = sl.Order
               })
               .ToList();

            // Load StudyHistologyViews based on the current Study
            StudyHistologyViews = Study.StudyHistologies!
                .Where(sh => sh.Histology.IsActive == true)
                .Select(sh => new StudyHistologyView
                {
                    StudyHistologyId = sh.StudyHistologyId,
                    StudyId = sh.StudyId,
                    HistologyId = sh.HistologyId,
                    HistologyCode = sh.Histology.HistologyCode,
                    HistologyBehavior = sh.Histology.HistologyBehavior,
                    HistologyName = sh.Histology.HistologyName ?? string.Empty, // Ensure HistologyName is not null
                    IsPreferred = sh.Histology.IsPreferred,
                    IsActive = sh.IsActive,
                    Order = sh.Order
                })
                .ToList();

            // Load StudyHeaders based on the current Study
            var StudyHeaders = await StudyHeaderData.ListStudyHeadersByStudyIdAsync(Study.StudyId);
            StudyHeaderViews = StudyHeaders!
                .Where(sh => sh.IsActive == true)
                .Select(sh => new StudyHeaderView
                {
                    StudyHeaderId = sh.StudyHeaderId,
                    StudyId = sh.StudyId,
                    HeaderName = sh.HeaderName ?? string.Empty, // Ensure HeaderName is not null
                    ExportTitle = sh.ExportTitle ?? string.Empty, // Ensure ExportTitle is not null
                    Order = sh.Order,
                })
                .ToList();

            // Load StudyReportHeaders based on the current Study
            var StudyReportHeaders = await StudyReportHeaderData.ListStudyReportHeadersByStudyIdAsync(Study.StudyId);
            StudyReportHeaderViews = StudyReportHeaders!
                .Where(sh => sh.IsActive == true)
                .Select(sh => new StudyReportHeaderView
                {
                    StudyReportHeaderId = sh.StudyReportHeaderId,
                    StudyId = sh.StudyId,
                    HeaderName = sh.HeaderName ?? string.Empty, // Ensure HeaderName is not null
                    ExportTitle = sh.ExportTitle ?? string.Empty, // Ensure ExportTitle is not null
                    Order = sh.Order,
                })
                .ToList();
        }

        private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
        {
            // Logic to execute when a field changes
            // e.FieldIdentifier provides information about the changed field
            //Console.WriteLine($"Field '{e.FieldIdentifier.FieldName}' changed.");

            IsSaved = false;
        }

        protected override void OnParametersSet()
        {
            if (Study == null)
            {
                Study = new Study();
            }

            if (EditContext == null || EditContext.Model != Study)
            {
                EditContext = new EditContext(Study);
            }
        }

        private async Task OnCancel()
        {
            NavigationManager.NavigateTo($"/studies/list");
        }

        private async Task OnDeactivate()
        {
            bool? result = await DialogService.ShowMessageBoxAsync(
              "Warning", // Dialog title
              $"Are you sure you want to deactivate this study?", // Message
              yesText: "Deactivate!", // Text for the confirmation button
              cancelText: "Cancel" // Text for the cancel button
          );
            if (result == true) // User clicked 'Deactivate!'
            {
                await StudyData.DeleteStudyAsync(Study.StudyId);

                await InvokeAsync(StateHasChanged);
                IsSaved = true;
                SaveMessage = "Study deactivated.";
                severity = Severity.Warning;
                await InvokeAsync(StateHasChanged);

                NavigationManager.NavigateTo($"/studies/list");
            }
        }

        private async Task OnArchive()
        {
            bool? result = await DialogService.ShowMessageBoxAsync(
              "Warning", // Dialog title
              $"Are you sure you want to archive this study?", // Message
              yesText: "Archive!", // Text for the confirmation button
              cancelText: "Cancel" // Text for the cancel button
          );
            if (result == true) // User clicked 'Archive!'
            {
                await StudyData.ArchiveStudyAsync(Study.StudyId);

                await InvokeAsync(StateHasChanged);
                IsSaved = true;
                SaveMessage = "Study archived.";
                severity = Severity.Warning;
                await InvokeAsync(StateHasChanged);

                NavigationManager.NavigateTo($"/studies/list");
            }
        }

        private async Task SetStudyLookupViewsRaceAsync(List<StudyLookupView> studyLookupViewsRace)
        {
            // All lookups for Race
            var allLookups = await StudyLookupData.ListStudyLookupsAllByTypeAsync("Race");

            // Filter lookups for the current StudyId
            var allRaceLookups = allLookups.Where(sl => sl.StudyId == StudyId).ToList();

            foreach(var lookup in allRaceLookups)
            {
                // Check if the lookup exists in the new list
                var existingLookup = studyLookupViewsRace.FirstOrDefault(sl => sl.LookupName == lookup.LookupName);
                // Remove existing lookups that are not in the new list
                if (existingLookup == null)
                {
                    await StudyLookupData.DeleteStudyLookupAsync(lookup.StudyLookupId); // Delete the lookup if it doesn't exist in the new list
                }
            }

            foreach (var lookupView in studyLookupViewsRace)
            {
                // check to see if the lookupView already exists in the StudyLookupViews list
                var existingLookup = allRaceLookups.Where(sl => sl.LookupName == lookupView.LookupName).FirstOrDefault();

                if (existingLookup != null)
                {
                    var lookupRecord = await StudyLookupData.GetStudyLookupAsync(existingLookup.StudyLookupId);

                    // If it exists, update the existing lookupView record with the new values
                    lookupRecord.Order = lookupView.Order;
                    lookupRecord.IsActive = true;

                    await StudyLookupData.UpdateStudyLookupAsync(lookupRecord.StudyLookupId, lookupRecord); // Update the existing lookupView
                    continue; // Skip adding a new lookupView since it already exists
                }

                if (existingLookup == null)
                {
                    var newLookup = new StudyLookup
                    {
                        StudyLookupId = Guid.NewGuid(), // Generate a new Guid for the StudyLookupId
                        StudyId = lookupView.StudyId,
                        LookupId = lookupView.LookupId,
                        Order = lookupView.Order,
                        IsActive = true,
                    };

                    await StudyLookupData.CreateStudyLookupAsync(newLookup); // Create a new lookupView
                }
            }
        }

        private async Task SetStudyLookupViewsGenderAsync(List<StudyLookupView> studyLookupViewsGender)
        {
            // All lookups for Gender
            var allLookups = await StudyLookupData.ListStudyLookupsAllByTypeAsync("Gender");

            // Filter lookups for the current StudyId
            var allGenderLookups = allLookups.Where(sl => sl.StudyId == StudyId).ToList();

            foreach (var lookup in allGenderLookups)
            {
                // Check if the lookup exists in the new list
                var existingLookup = studyLookupViewsGender.FirstOrDefault(sl => sl.LookupName == lookup.LookupName);
                // Remove existing lookups that are not in the new list
                if (existingLookup == null)
                {
                    await StudyLookupData.DeleteStudyLookupAsync(lookup.StudyLookupId); // Delete the lookup if it doesn't exist in the new list
                }
            }

            foreach (var lookupView in studyLookupViewsGender)
            {
                // check to see if the lookupView already exists in the StudyLookupViews list
                var existingLookup = allGenderLookups.Where(sl => sl.LookupName == lookupView.LookupName).FirstOrDefault();

                if (existingLookup != null)
                {
                    var lookupRecord = await StudyLookupData.GetStudyLookupAsync(existingLookup.StudyLookupId);

                    // If it exists, update the existing lookupView record with the new values
                    lookupRecord.Order = lookupView.Order;
                    lookupRecord.IsActive = true;

                    await StudyLookupData.UpdateStudyLookupAsync(lookupRecord.StudyLookupId, lookupRecord); // Update the existing lookupView
                    continue; // Skip adding a new lookupView since it already exists
                }

                if (existingLookup == null)
                {
                    var newLookup = new StudyLookup
                    {
                        StudyLookupId = Guid.NewGuid(), // Generate a new Guid for the StudyLookupId
                        StudyId = lookupView.StudyId,
                        LookupId = lookupView.LookupId,
                        Order = lookupView.Order,
                        IsActive = true,
                    };

                    await StudyLookupData.CreateStudyLookupAsync(newLookup); // Create a new lookupView
                }
            }
        }

        private async Task SetStudyLookupViewsEthnicityAsync(List<StudyLookupView> studyLookupViewsEthnicity)
        {
            // All lookups for Ethnicity
            var allLookups = await StudyLookupData.ListStudyLookupsAllByTypeAsync("Ethnicity");

            // Filter lookups for the current StudyId
            var allEthnicityLookups = allLookups.Where(sl => sl.StudyId == StudyId).ToList();

            foreach (var lookup in allEthnicityLookups)
            {
                // Check if the lookup exists in the new list
                var existingLookup = studyLookupViewsEthnicity.FirstOrDefault(sl => sl.LookupName == lookup.LookupName);
                // Remove existing lookups that are not in the new list
                if (existingLookup == null)
                {
                    await StudyLookupData.DeleteStudyLookupAsync(lookup.StudyLookupId); // Delete the lookup if it doesn't exist in the new list
                }
            }

            foreach (var lookupView in studyLookupViewsEthnicity)
            {
                // check to see if the lookupView already exists in the StudyLookupViews list
                var existingLookup = allEthnicityLookups.Where(sl => sl.LookupName == lookupView.LookupName).FirstOrDefault();

                if (existingLookup != null)
                {
                    var lookupRecord = await StudyLookupData.GetStudyLookupAsync(existingLookup.StudyLookupId);

                    // If it exists, update the existing lookupView record with the new values
                    lookupRecord.Order = lookupView.Order;
                    lookupRecord.IsActive = true;

                    await StudyLookupData.UpdateStudyLookupAsync(lookupRecord.StudyLookupId, lookupRecord); // Update the existing lookupView
                    continue; // Skip adding a new lookupView since it already exists
                }

                if (existingLookup == null)
                {
                    var newLookup = new StudyLookup
                    {
                        StudyLookupId = Guid.NewGuid(), // Generate a new Guid for the StudyLookupId
                        StudyId = lookupView.StudyId,
                        LookupId = lookupView.LookupId,
                        Order = lookupView.Order,
                        IsActive = true,
                    };

                    await StudyLookupData.CreateStudyLookupAsync(newLookup); // Create a new lookupView
                }
            }
        }

        private async Task SetStudyLookupViewsProcedureAsync(List<StudyLookupView> studyLookupViewsProcedure)
        {
            // All lookups for Procedure
            var allLookups = await StudyLookupData.ListStudyLookupsAllByTypeAsync("Procedure");

            // Filter lookups for the current StudyId
            var allProcedureLookups = allLookups.Where(sl => sl.StudyId == StudyId).ToList();

            foreach (var lookup in allProcedureLookups)
            {
                // Check if the lookup exists in the new list
                var existingLookup = studyLookupViewsProcedure.FirstOrDefault(sl => sl.LookupName == lookup.LookupName);
                // Remove existing lookups that are not in the new list
                if (existingLookup == null)
                {
                    await StudyLookupData.DeleteStudyLookupAsync(lookup.StudyLookupId); // Delete the lookup if it doesn't exist in the new list
                }
            }

            foreach (var lookupView in studyLookupViewsProcedure)
            {
                // check to see if the lookupView already exists in the StudyLookupViews list
                var existingLookup = allProcedureLookups.Where(sl => sl.LookupName == lookupView.LookupName).FirstOrDefault();

                if (existingLookup != null)
                {
                    var lookupRecord = await StudyLookupData.GetStudyLookupAsync(existingLookup.StudyLookupId);

                    // If it exists, update the existing lookupView record with the new values
                    lookupRecord.Order = lookupView.Order;
                    lookupRecord.IsActive = true;

                    await StudyLookupData.UpdateStudyLookupAsync(lookupRecord.StudyLookupId, lookupRecord); // Update the existing lookupView
                    continue; // Skip adding a new lookupView since it already exists
                }

                if (existingLookup == null)
                {
                    var newLookup = new StudyLookup
                    {
                        StudyLookupId = Guid.NewGuid(), // Generate a new Guid for the StudyLookupId
                        StudyId = lookupView.StudyId,
                        LookupId = lookupView.LookupId,
                        Order = lookupView.Order,
                        IsActive = true,
                    };

                    await StudyLookupData.CreateStudyLookupAsync(newLookup); // Create a new lookupView
                }
            }
        }

        private async Task SetStudyLookupViewsSiteAsync(List<StudyLookupView> studyLookupViewsSite)
        {
            // All lookups for Site
            var allLookups = await StudyLookupData.ListStudyLookupsAllByTypeAsync("Site");

            // Filter lookups for the current StudyId
            var allSiteLookups = allLookups.Where(sl => sl.StudyId == StudyId).ToList();

            foreach (var lookup in allSiteLookups)
            {
                // Check if the lookup exists in the new list
                var existingLookup = studyLookupViewsSite.FirstOrDefault(sl => sl.LookupName == lookup.LookupName);
                // Remove existing lookups that are not in the new list
                if (existingLookup == null)
                {
                    await StudyLookupData.DeleteStudyLookupAsync(lookup.StudyLookupId); // Delete the lookup if it doesn't exist in the new list
                }
            }

            foreach (var lookupView in studyLookupViewsSite)
            {
                // check to see if the lookupView already exists in the StudyLookupViews list
                var existingLookup = allSiteLookups.Where(sl => sl.LookupName == lookupView.LookupName).FirstOrDefault();

                if (existingLookup != null)
                {
                    var lookupRecord = await StudyLookupData.GetStudyLookupAsync(existingLookup.StudyLookupId);

                    // If it exists, update the existing lookupView record with the new values
                    lookupRecord.Order = lookupView.Order;
                    lookupRecord.IsActive = true;

                    await StudyLookupData.UpdateStudyLookupAsync(lookupRecord.StudyLookupId, lookupRecord); // Update the existing lookupView
                    continue; // Skip adding a new lookupView since it already exists
                }

                if (existingLookup == null)
                {
                    var newLookup = new StudyLookup
                    {
                        StudyLookupId = Guid.NewGuid(), // Generate a new Guid for the StudyLookupId
                        StudyId = lookupView.StudyId,
                        LookupId = lookupView.LookupId,
                        Order = lookupView.Order,
                        IsActive = true,
                    };

                    await StudyLookupData.CreateStudyLookupAsync(newLookup); // Create a new lookupView
                }
            }
        }

        private async Task SetStudyLookupViewsCountyAsync(List<StudyLookupView> studyLookupViewsCounty)
        {
            // All lookups for County
            var allLookups = await StudyLookupData.ListStudyLookupsAllByTypeAsync("County");

            // Filter lookups for the current StudyId
            var allCountyLookups = allLookups.Where(sl => sl.StudyId == StudyId).ToList();

            foreach (var lookup in allCountyLookups)
            {
                // Check if the lookup exists in the new list
                var existingLookup = studyLookupViewsCounty.FirstOrDefault(sl => sl.LookupName == lookup.LookupName);
                // Remove existing lookups that are not in the new list
                if (existingLookup == null)
                {
                    await StudyLookupData.DeleteStudyLookupAsync(lookup.StudyLookupId); // Delete the lookup if it doesn't exist in the new list
                }
            }

            foreach (var lookupView in studyLookupViewsCounty)
            {
                // check to see if the lookupView already exists in the StudyLookupViews list
                var existingLookup = allCountyLookups.Where(sl => sl.LookupName == lookupView.LookupName).FirstOrDefault();

                if (existingLookup != null)
                {
                    var lookupRecord = await StudyLookupData.GetStudyLookupAsync(existingLookup.StudyLookupId);

                    // If it exists, update the existing lookupView record with the new values
                    lookupRecord.Order = lookupView.Order;
                    lookupRecord.IsActive = true;

                    await StudyLookupData.UpdateStudyLookupAsync(lookupRecord.StudyLookupId, lookupRecord); // Update the existing lookupView
                    continue; // Skip adding a new lookupView since it already exists
                }

                if (existingLookup == null)
                {
                    var newLookup = new StudyLookup
                    {
                        StudyLookupId = Guid.NewGuid(), // Generate a new Guid for the StudyLookupId
                        StudyId = lookupView.StudyId,
                        LookupId = lookupView.LookupId,
                        Order = lookupView.Order,
                        IsActive = true,
                    };

                    await StudyLookupData.CreateStudyLookupAsync(newLookup); // Create a new lookupView
                }
            }
        }

        private async Task SetStudyHistologyViewsAsync(List<StudyHistologyView> studyHistologyViews)
        {
            // All histologies
            var allHistologies = await StudyHistologyData.ListStudyHistologiesAllAsync();

            // Filter lookups for the current StudyId
            var allStudyHistologies = allHistologies.Where(sl => sl.StudyId == StudyId).ToList();

            foreach (var histology in allStudyHistologies)
            {
                // Check if the histology exists in the new list
                var existingHistology = studyHistologyViews.FirstOrDefault(sl => sl.HistologyName == histology.HistologyName);
                // Remove existing histologies that are not in the new list
                if (existingHistology == null)
                {
                    await StudyHistologyData.DeleteStudyHistologyAsync(histology.StudyHistologyId); // Delete the histology if it doesn't exist in the new list
                }
            }

            foreach (var histologyView in studyHistologyViews)
            {
                // check to see if the histologyView already exists in the StudyHistologyViews list
                var existingHistology = allStudyHistologies.Where(sl => sl.HistologyName == histologyView.HistologyName).FirstOrDefault();

                if (existingHistology != null)
                {
                    var histologyRecord = await StudyHistologyData.GetStudyHistologyAsync(existingHistology.StudyHistologyId);

                    // If it exists, update the existing histologyView record with the new values
                    histologyRecord.Order = histologyView.Order;
                    histologyRecord.IsActive = true;

                    await StudyHistologyData.UpdateStudyHistologyAsync(histologyRecord.StudyHistologyId, histologyRecord); // Update the existing histologyView
                    continue; // Skip adding a new histologyView since it already exists
                }

                if (existingHistology == null)
                {
                    var newHistology = new StudyHistology
                    {
                        StudyHistologyId = Guid.NewGuid(), // Generate a new Guid for the StudyLookupId
                        StudyId = histologyView.StudyId,
                        HistologyId = histologyView.HistologyId,
                        Order = histologyView.Order,
                        IsActive = true,
                    };

                    await StudyHistologyData.CreateStudyHistologyAsync(newHistology); // Create a new histologyView
                }
            }
        }

        private async Task SetStudyHeaderViewsAsync(List<StudyHeaderView> studyHeaderViews)
        {
            // All headers
            var allHeaders = await StudyHeaderData.ListStudyHeadersAsync();

            // Filter headers for the current StudyId
            var allStudyHeaders = allHeaders.Where(sl => sl.StudyId == StudyId).ToList();

            foreach (var header in allStudyHeaders)
            {
                // Check if the header exists in the new list
                var existingHeader = studyHeaderViews.FirstOrDefault(sl => sl.HeaderName == header.HeaderName);
                // Remove existing headers that are not in the new list
                if (existingHeader == null)
                {
                    await StudyHeaderData.DeleteStudyHeaderAsync(header.StudyHeaderId); // Delete the header if it doesn't exist in the new list
                }
            }

            foreach (var headerView in studyHeaderViews)
            {
                // check to see if the headerView already exists in the StudyHeaderViews list
                var existingHeader = allStudyHeaders.Where(sl => sl.HeaderName == headerView.HeaderName).FirstOrDefault();

                if (existingHeader != null)
                {
                    var headerRecord = await StudyHeaderData.GetStudyHeaderAsync(existingHeader.StudyHeaderId);

                    // If it exists, update the existing lookupView record with the new values
                    headerRecord.Order = headerRecord.Order;
                    headerRecord.ExportTitle = headerRecord.ExportTitle ?? string.Empty; // Ensure ExportTitle is not null
                    headerRecord.IsActive = true;

                    await StudyHeaderData.UpdateStudyHeaderAsync(headerRecord.StudyHeaderId, headerRecord); // Update the existing headerView
                    continue; // Skip adding a new headerView since it already exists
                }

                if (existingHeader == null)
                {
                    var newHeader = new StudyHeader
                    {
                        StudyHeaderId = Guid.NewGuid(), // Generate a new Guid for the StudyHeaderId
                        StudyId = headerView.StudyId,
                        HeaderName = headerView.HeaderName ?? string.Empty, // Ensure HeaderName is not null
                        ExportTitle = headerView.ExportTitle ?? string.Empty, // Ensure ExportTitle is not null
                        TableName = headerView.TableName ?? string.Empty, // Ensure TableName is not null
                        Order = headerView.Order,
                        IsActive = true,
                    };

                    await StudyHeaderData.CreateStudyHeaderAsync(newHeader); // Create a new header
                }
            }
        }

        private async Task SetStudyReportHeaderViewsAsync(List<StudyReportHeaderView> studyReportHeaderViews)
        {
            // All Report headers
            var allReportHeaders = await StudyReportHeaderData.ListStudyReportHeadersAsync();

            // Filter headers for the current StudyId
            var allStudyReportHeaders = allReportHeaders.Where(sl => sl.StudyId == StudyId).ToList();

            foreach (var header in allStudyReportHeaders)
            {
                // Check if the header exists in the new list
                var existingHeader = studyReportHeaderViews.FirstOrDefault(sl => sl.HeaderName == header.HeaderName);
                // Remove existing headers that are not in the new list
                if (existingHeader == null)
                {
                    await StudyReportHeaderData.DeleteStudyReportHeaderAsync(header.StudyReportHeaderId); // Delete the header if it doesn't exist in the new list
                }
            }

            foreach (var headerView in studyReportHeaderViews)
            {
                // check to see if the headerView already exists in the StudyHeaderViews list
                var existingHeader = allStudyReportHeaders.Where(sl => sl.HeaderName == headerView.HeaderName).FirstOrDefault();

                if (existingHeader != null)
                {
                    var headerRecord = await StudyReportHeaderData.GetStudyReportHeaderAsync(existingHeader.StudyReportHeaderId);

                    // If it exists, update the existing lookupView record with the new values
                    headerRecord.Order = headerRecord.Order;
                    headerRecord.ExportTitle = headerRecord.ExportTitle ?? string.Empty; // Ensure ExportTitle is not null
                    headerRecord.IsActive = true;

                    await StudyReportHeaderData.UpdateStudyReportHeaderAsync(headerRecord.StudyReportHeaderId, headerRecord); // Update the existing headerView
                    continue; // Skip adding a new headerView since it already exists
                }

                if (existingHeader == null)
                {
                    var newHeader = new StudyReportHeader
                    {
                        StudyReportHeaderId = Guid.NewGuid(), // Generate a new Guid for the StudyHeaderId
                        StudyId = headerView.StudyId,
                        HeaderName = headerView.HeaderName ?? string.Empty, // Ensure HeaderName is not null
                        ExportTitle = headerView.ExportTitle ?? string.Empty, // Ensure ExportTitle is not null
                        TableName = headerView.TableName ?? string.Empty, // Ensure TableName is not null
                        Order = headerView.Order,
                        IsActive = true,
                    };

                    await StudyReportHeaderData.CreateStudyReportHeaderAsync(newHeader); // Create a new header
                }
            }
        }

        private async Task Save()
        {
            // Manually trigger validation
            bool isValid = EditContext.Validate();

            if (isValid)
            {
                HasErrors = false;
                severity = Severity.Success; // Set severity to Success if the form is valid
                SaveMessage = "Study updated."; // Set a success message
                await InvokeAsync(StateHasChanged);

                try
                {
                    await StudyData.UpdateStudyAsync(Study.StudyId, Study);

                    // Set the StudyLookupViews based on the current Study
                    await SetStudyLookupViewsRaceAsync(StudyLookupViewsRace);
                    await SetStudyLookupViewsGenderAsync(StudyLookupViewsGender);
                    await SetStudyLookupViewsEthnicityAsync(StudyLookupViewsEthnicity);
                    await SetStudyLookupViewsProcedureAsync(StudyLookupViewsProcedure);
                    await SetStudyLookupViewsSiteAsync(StudyLookupViewsSite);
                    await SetStudyLookupViewsCountyAsync(StudyLookupViewsCounty);
                    await SetStudyHistologyViewsAsync(StudyHistologyViews);
                    await SetStudyHeaderViewsAsync(StudyHeaderViews);
                    await SetStudyReportHeaderViewsAsync(StudyReportHeaderViews);

                    IsSaved = true; // Set IsSaved to true to indicate the form was submitted successfully

                    //Logger.LogInformation("Study updated. {Study}", System.Text.Json.JsonSerializer.Serialize(Study));

                    EditContext.MarkAsUnmodified(); // Mark the edit context as unmodified to prevent further validation errors
                }
                catch (HttpRequestException ex)
                {
                    apiResponse = $"Error: {ex.Message}"; // Handle potential errors during the API call
                    SaveMessage = apiResponse; // Set the save message to the error message
                    severity = Severity.Error; // Set severity to Error if the form is invalid
                    IsSaved = false; // Set IsSaved to false if there was an error
                    HasErrors = true; // Set HasErrors to true to indicate there are errors
                }
            }
            else
            {
                IsSaved = false;
                HasErrors = true; // Set HasErrors to true to indicate there are validation errors
                severity = Severity.Error; // Set severity to Error if the form is invalid
                SaveMessage = "Study not updated.  Please correct the errors."; // Set a failure message
                await InvokeAsync(StateHasChanged);

            }
        }

        private async Task OnSubmit()
        {
            await Save();
        }

        private async Task OnSaveAndClose()
        {
            await Save();

            if (IsSaved)
            {
                NavigationManager.NavigateTo($"/studies/list");
            }
        }
    }
}