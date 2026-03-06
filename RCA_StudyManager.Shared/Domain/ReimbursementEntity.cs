using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.Domain
{
    [Table("ReimbursementEntity")]
    public class ReimbursementEntity
    {
        public Guid ReimbursementEntityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PayableTo { get; set; } = string.Empty;
        public string? AttentionTo { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; } = string.Empty;
        public string? AddressLine3 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;

        public string? Comments { get; set; } = string.Empty;

        public string InvoicePrefix { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [ValidateComplexType]
        public ICollection<ReimbursementEntityRCAContact>? ReimbursementEntityRCAContacts { get; set; }

        public ICollection<RCAContact>? RCAContacts { get; set; }


    }
}
