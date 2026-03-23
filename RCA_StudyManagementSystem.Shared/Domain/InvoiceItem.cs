using RCA_StudyManagementSystem.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.Domain
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

        public DateTime CreatedDate { get; set; }
        public Guid CreatedUserId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? ModifiedUserId { get; set; }
    }

    public class GroupedInvoiceItems
    {
        public string HospitalName { get; set; }
        public IEnumerable<InvoiceItem> Items { get; set; }
        public decimal GroupTotal { get; set; }
    }
}
