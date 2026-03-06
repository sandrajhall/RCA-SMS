using RCA_StudyManager.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.Domain
{
    [Table("InvoiceItem")]
    public class InvoiceItem : ITrackable
    {
        public Guid InvoiceItemId { get; set; } = Guid.NewGuid();
        public Guid InvoiceId { get; set; } = Guid.NewGuid();

        public Guid HospitalId { get; set; } = Guid.Empty;
        public Guid StudyId { get; set; } = Guid.Empty;
        public int? NumPathReports { get; set; }

        public Hospital? Hospital { get; set; } = null;
        public Study? Study { get; set; } = null;

        public Guid CreatedUserId { get; set; } = Guid.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? ModifiedUserId { get; set; } = null;
        public DateTime? ModifiedDate { get; set; } = null;
    }

    public class GroupedInvoiceItems
    {
        public string HospitalName { get; set; }
        public IEnumerable<InvoiceItem> Items { get; set; }
        public decimal GroupTotal { get; set; }
    }
}
