using RCA_StudyManagementSystem.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.Domain
{
    [Table("Invoice")]
    public class Invoice : ITrackable
    {
        public Guid InvoiceId { get; set; } = Guid.NewGuid();
        public Guid ReimbursementEntityId { get; set; } = Guid.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public string InvoiceQuarter { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; } = 0.0M;

        public DateTime? DateEmailed { get; set; }
        public DateTime? DateReminderSent { get; set; }
        public DateTime? DateSignedInvoiceReturned { get; set; }
        public DateTime? RASRSubmitDate { get; set; }
        public string? RASRId { get; set; } = string.Empty;
        public DateTime? RASRCompleteDate { get; set; }
        public bool IsComplete { get; set; } = false;

        public ICollection<InvoiceItem> InvoiceItems { get; set; }

        public ReimbursementEntity? ReimbursementEntity { get; set; }


        public DateTime CreatedDate { get; set; }
        public Guid CreatedUserId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? ModifiedUserId { get; set; }
    }
}
