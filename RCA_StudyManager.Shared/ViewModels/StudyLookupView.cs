using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.ViewModels
{
    public class StudyLookupView
    {
        public Guid StudyLookupViewId { get; set; } = Guid.NewGuid(); // Unique identifier for the StudyLookupView
        
        public Guid StudyLookupId { get; set; } // Unique identifier for the StudyLookup
        public Guid StudyId { get; set; }

        public Guid LookupId { get; set; }

        public string LookupCode { get; set; } = string.Empty; // Unique code for the lookup, if applicable

        public string LookupName { get; set; } = string.Empty; // The name displayed in the UI for this lookup

        public string LookupType {  get; set; } = string.Empty;

        public int Order { get; set; }

        public bool IsSelected { get; set; } = false; // Indicates if this lookup is selected in the UI

        // Equals and GetHashCode methods to ensure correct comparison and hashing based on Id for MudBlazor DataGrid and other collections
        public override bool Equals(object? obj)
        {
            if (obj is not StudyLookupView otherStudyLookupView)
                return false;
            return StudyLookupId == otherStudyLookupView.StudyLookupId;
        }

        public override int GetHashCode()
        {
            return StudyLookupId.GetHashCode();
        }

    }
}
