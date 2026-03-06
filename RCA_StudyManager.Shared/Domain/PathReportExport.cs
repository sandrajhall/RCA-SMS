using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.Domain
{
    [Table("PathReportExport")]
    public class PathReportExport
    {
        public Guid PathReportExportId { get; set; }
        public Guid PathReportId { get; set; }
        public Guid BatchId { get; set; }

        public Batch? Batch { get; set; }
    }
}
