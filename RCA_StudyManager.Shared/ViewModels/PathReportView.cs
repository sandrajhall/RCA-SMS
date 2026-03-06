using RCA_StudyManager.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.ViewModels
{
    public class PathReportView : PathReport
    {
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string DateOfBirth { get; set; }
        public string SocialSecurityNumber { get; set; }

        public Guid StudyId { get; set; }
        public string StudyName { get; set; }
        public string StudyPrefix { get; set; }
        public string StudyColor { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public string MigratedCCRNumber { get; set; } = string.Empty;

        public string HospShortName { get; set; } = string.Empty;

        public bool IsSelected { get; set; } = false; // Indicates if this path report is selected in the UI


        public bool IsMinAgeValid { get; set; } = false;
        public bool IsMaxAgeValid { get; set; } = false;
        public bool IsPathMinAgeValid { get; set; } = false;
        public int MinAgeDiff { get; set; }
        public bool IsPathMaxAgeValid { get; set; } = false;
        public bool IsPathMaxAgeWeekOut { get; set; } = false;
        public int MaxAgeDiff { get; set; }
        public bool IsPathMaxNumValid { get; set; } = false;
        public bool IsCountyValid { get; set; } = false;
        public bool IsCaseMatchValid { get; set; } = false;
        public bool IsDNCValid { get; set; } = false;

    }
}
