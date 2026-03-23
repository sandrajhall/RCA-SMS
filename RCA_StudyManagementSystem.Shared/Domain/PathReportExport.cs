using RCA_StudyManagementSystem.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.Domain
{
    [Table("PathReportExport")]
    public class PathReportExport : ITrackable
    {
        public Guid PathReportExportId { get; set; }
        public Guid PathReportId { get; set; }
        public Guid BatchId { get; set; }

        public Batch? Batch { get; set; }

        public DateTime CreatedDate { get; set; }
        public Guid CreatedUserId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? ModifiedUserId { get; set; }
    }
}
