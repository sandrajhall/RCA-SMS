using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.ViewModels
{
    public class StudyHistologyView
    {
        public Guid StudyHistologyViewId { get; set; } = Guid.NewGuid(); // Unique identifier for the StudyHistologyView
        
        public Guid StudyHistologyId { get; set; } // Unique identifier for the StudyHistology
        public Guid StudyId { get; set; }

        public Guid HistologyId { get; set; }

        public string HistologyCode { get; set; } = string.Empty; // The name displayed in the UI for this histology
        public string HistologyBehavior { get; set; } = string.Empty; // The name displayed in the UI for this histology

        public string HistologyName { get; set; } = string.Empty; // The name displayed in the UI for this histology

        public bool IsPreferred { get; set; } = false; // Indicates if this histology is preferred for the study
        public bool IsActive { get; set; } = true; // Indicates if the histology is currently active or not

        public int Order { get; set; }

        public bool IsSelected { get; set; } = false; // Indicates if this histology is selected in the UI

        // Equals and GetHashCode methods to ensure correct comparison and hashing based on Id for MudBlazor DataGrid and other collections
        public override bool Equals(object? obj)
        {
            if (obj is not StudyHistologyView otherStudyHistologyView)
                return false;
            return StudyHistologyId == otherStudyHistologyView.StudyHistologyId;
        }

        public override int GetHashCode()
        {
            return StudyHistologyId.GetHashCode();
        }

    }
}
