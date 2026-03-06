using RCA_StudyManagementSystem.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.Domain
{
    [Table("StudyHistology")]
    public class StudyHistology : ITrackable
    {
        public Guid StudyHistologyId { get; set; } = Guid.NewGuid(); // Unique identifier for the StudyHistology
        public Guid StudyId { get; set; }

        public Study? Study { get; set; } = null!; // Navigation property to the Study

        public Guid HistologyId { get; set; }

        public Histology? Histology { get; set; } = null!; // Navigation property to the Histology this study histology refers to

        public int Order { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid CreatedUserId { get; set; } = Guid.Empty;

        public DateTime? ModifiedDate { get; set; } = null;
        public Guid? ModifiedUserId { get; set; } = null;
    }
}
