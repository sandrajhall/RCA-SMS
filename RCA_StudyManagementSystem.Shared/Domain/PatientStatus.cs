using RCA_StudyManagementSystem.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.Domain
{
    [Table("PatientStatus")]
    public class PatientStatus : ITrackable
    {
        public Guid PatientStatusId { get; set; }
        public DateTime Date { get; set; }

        public Guid StudyId { get; set; }

        public Guid PatientId { get; set; }
        public string? CaseNumber { get; set; }

        public bool NoContact { get; set; } 
        public string? Status { get; set; }
        public DateTime? DateOfDeath { get; set; }
        public DateTime? DateLastContact { get; set; }  
        public bool InformedOfCancerDiagnosis { get; set; }
        public bool StatedNoCancerDiagnosis { get; set; }

        public string? Comments { get; set; }


        public DateTime CreatedDate { get; set; }
        public Guid CreatedUserId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? ModifiedUserId { get; set; }

        public Patient? Patient { get; set; }
        public Study? Study { get; set; }
    }
}
