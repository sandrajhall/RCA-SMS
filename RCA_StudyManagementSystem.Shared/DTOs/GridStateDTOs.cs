using RCA_StudyManager.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.DTOs
{
    // DTO for FilterDefinition
    public class FilterDefinitionDto
    {
        public string Field { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string? Value { get; set; }
        public bool? BoolValue { get; set; } = null; // Nullable boolean value for filters that require it
    }

    // DTO for SortDefinition
    public class SortDefinitionDto
    {
        public string SortBy { get; set; } = string.Empty;
        public bool Descending { get; set; } = false;

        public int Index { get; set; } = 0; // Added Index property to track the order of sorts

        public object SortFunc { get; set; } = null!; // Placeholder for the sort function, can be used for custom sorting logic
        }

    // DTO for the overall Grid State
    public class GridStateDto
    {
        public List<FilterDefinitionDto> Filters { get; set; } = new();
        public List<SortDefinitionDto> Sorts { get; set; } = new();
        public int CurrentPage { get; set; } = 0; 
        public int PageSize { get; set; } = 10; 
        public string? SearchString { get; set; }
        public int? ViewLimit { get; set; } = 0;  // Optional property to specify a view limit, can be used for different grid configurations

    }
}
