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
    [Table("StudyHeader")]
    public class StudyHeader : ITrackable
    {
        public Guid StudyHeaderId { get; set; } = Guid.NewGuid(); // Unique identifier for the StudyHeader
        public Guid StudyId { get; set; }

        public string HeaderName { get; set; } = string.Empty;

        public string ExportTitle { get; set; } = string.Empty;

        public string TableName { get; set; } = string.Empty;

        public int Order { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public Guid CreatedUserId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? ModifiedUserId { get; set; }
    }
}
