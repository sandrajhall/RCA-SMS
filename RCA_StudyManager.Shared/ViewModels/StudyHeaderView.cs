using RCA_StudyManager.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.ViewModels
{
    public class StudyHeaderView
    {
        public Guid StudyHeaderViewId { get; set; } = Guid.NewGuid(); // Unique identifier for the StudyHeaderView

        public Guid StudyHeaderId { get; set; } // Foreign key to the StudyHeader

        public Guid StudyId { get; set; }

        public string HeaderCode { get; set; } = string.Empty; // Unique code for the header, if applicable


        public string HeaderName { get; set; } = string.Empty; // The name displayed in the UI for this header

        public string ExportTitle { get; set; } = string.Empty; // The title used for exporting data related to this header

        public string TableName { get; set; } = string.Empty; // The name of the table associated with this header

        public string HeaderType {  get; set; } = string.Empty;

        public int Order { get; set; }

        public bool IsSelected { get; set; } = false; // Indicates if this lookup is selected in the UI

        // Equals and GetHashCode methods to ensure correct comparison and hashing based on Id for MudBlazor DataGrid and other collections
        public override bool Equals(object? obj)
        {
            if (obj is not StudyHeaderView otherStudyHeaderView)
                return false;
            return StudyHeaderId == otherStudyHeaderView.StudyHeaderId;
        }

        public override int GetHashCode()
        {
            return StudyHeaderId.GetHashCode();
        }

    }
}
