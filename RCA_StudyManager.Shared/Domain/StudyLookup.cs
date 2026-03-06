using RCA_StudyManager.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.Domain
{
    [Table("StudyLookup")]
    public class StudyLookup : ITrackable
    {
        public Guid StudyLookupId { get; set; } = Guid.NewGuid(); // Unique identifier for the StudyLookup
        public Guid StudyId { get; set; }

        public Study? Study { get; set; } = null!; // Navigation property to the Study this lookup belongs to

        public Guid LookupId { get; set; }

        public Lookup? Lookup { get; set; } = null!; // Navigation property to the Lookup this study lookup refers to

        public int Order { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid CreatedUserId { get; set; } = Guid.Empty;

        public DateTime? ModifiedDate { get; set; } = null;
        public Guid? ModifiedUserId { get; set; } = null;
    }
}
