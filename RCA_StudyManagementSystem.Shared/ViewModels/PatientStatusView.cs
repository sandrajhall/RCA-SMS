using RCA_StudyManagementSystem.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.ViewModels
{
    public class PatientStatusView
    {
        public DateTime Date { get; set; }
        public string? CaseNumber { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public bool NoContact { get; set; }
        public string? Status { get; set; }
        public DateTime? DateOfDeath { get; set; }
        public DateTime? DateLastContact { get; set; }
        public bool InformedOfCancerDiagnosis { get; set; }
        public bool StatedNoCancerDiagnosis { get; set; }

        public string? Comments { get; set; }
    }
}
