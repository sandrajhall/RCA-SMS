using RCA_StudyManager.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.ViewModels
{
    public class PatientStatusCSVView
    {
        public string Date { get; set; }
        public string? CaseNumber { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        public string? DateOfBirth { get; set; }

        public string NoContact { get; set; }
        public string? Status { get; set; }
        public string? DateOfDeath { get; set; }
        public string? DateLastContact { get; set; }
        public string InformedOfCancerDiagnosis { get; set; }
        public string StatedNoCancerDiagnosis { get; set; }

        public string? Comments { get; set; }
    }
}
