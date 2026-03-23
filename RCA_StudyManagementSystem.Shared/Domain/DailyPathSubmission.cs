using RCA_StudyManagementSystem.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.Domain
{
    [Table("DailyPathSubmission")]
    public class DailyPathSubmission : ITrackable
    {
        public Guid DailyPathSubmissionId { get; set; }
        public Guid HospitalId { get; set; }
        public Guid StudyId { get; set; }
        public DateTime Date { get; set; }
        public string Value { get; set; } // e.g., "8" number of paths submitted

        public DateTime CreatedDate { get; set; }
        public Guid CreatedUserId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? ModifiedUserId { get; set; }

        public Hospital? Hospitals { get; set; }
        public Study? Studies { get; set; }
    }
}
