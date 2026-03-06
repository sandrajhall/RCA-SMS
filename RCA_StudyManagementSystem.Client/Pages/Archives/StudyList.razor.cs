using Microsoft.AspNetCore.Components;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Xml.Linq;
using static MudBlazor.CategoryTypes;

namespace RCA_StudyManagementSystem.Client.Pages.Archives
{
    public partial class StudyList : Microsoft.AspNetCore.Components.ComponentBase
    {
        private readonly DialogOptions _options = new() { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        private readonly DialogOptions _maxWidth = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

        public IEnumerable<Study> Studies { get; set; }

        MudDataGrid<Study> studyGrid;

        private int Index = 0;

        private string? _searchString;
        private List<string> _events = new();

        private Func<Study, string> _cellStyleFunc => item =>
        {
            // Assuming 'ColorField' is the property in YourModel that holds the color string (e.g., "red", "#FF0000")
            return $"background-color:{item.MudColorLight}";
        };

        // quick filter - filter globally across multiple columns with the same input
        private Func<Study, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true; 
            if (x.Prefix.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.Comments!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (x.Design!.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };


        protected override async Task OnInitializedAsync()
        {
            Studies = await StudyData.ListArchivedStudiesAsync();

            return;

        }


        async Task<IDialogReference> ViewItem(Study study)
        {
            var newStudies = new List<Study>();

            foreach (var item in studyGrid.FilteredItems)
            {
                var newStudy = await StudyData.GetStudyAsync(item.StudyId);
                newStudies.Add(newStudy);
            }

            var parameters = new DialogParameters<StudyViewDialog> { { x => x.Study, study } };

            var options = _options;

            // Pass the filtered items and the index of the clicked item
            parameters.Add(p => p.CarouselRecords, newStudies); // Pass filtered items
            parameters.Add(p => p.InitialSelectedIndex, studyGrid.FilteredItems.ToList().IndexOf(study)); // Set initial position


            return await DialogService.ShowAsync<StudyViewDialog>("Study View", parameters, options);

        }

    }
}
