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
    [Table("Batch")]
    public class Batch : ITrackable
    {
        public Guid BatchId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public Guid StudyId { get; set; }

        public string? NumberOfCases { get; set; } = string.Empty;
        [Required]
        public DateTime? ExportDate { get; set; } = null;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid CreatedUserId { get; set; } = Guid.Empty;

        public DateTime? ModifiedDate { get; set; } = null;
        public Guid? ModifiedUserId { get; set; } = null;

        public ICollection<PathReportExport>? PathReportExports { get; set; }
    }
}
