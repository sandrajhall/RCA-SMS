using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.ViewModels
{
    public class GridStateView<TData>
    {
        public List<IFilterDefinition<TData>> FilterDefinitions { get; set; } = new List<IFilterDefinition<TData>>();
        public Dictionary<string, SortDefinition<TData>> SortDefinitions { get; set; } = new Dictionary<string, SortDefinition<TData>>();
        
        // Add pager properties
        public int CurrentPage { get; set; } = 0; // Current page index
        public int PageSize { get; set; } = 10; // Added PageSize property

        public string SortColumn { get; set; } = String.Empty; // Track the index of the sorted column


    }
}
