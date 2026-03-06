using RCA_StudyManager.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.Domain
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


        public Guid CreatedUserId { get; set; } = Guid.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? ModifiedUserId { get; set; } = null;
        public DateTime? ModifiedDate { get; set; } = null;

        public Patient? Patient { get; set; }
        public Study? Study { get; set; }
    }
}
