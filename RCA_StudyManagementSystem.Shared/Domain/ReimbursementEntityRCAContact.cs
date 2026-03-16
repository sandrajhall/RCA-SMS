using RCA_StudyManagementSystem.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.Domain
{
    [Table("ReimbursementEntityRCAContact")]

    public class ReimbursementEntityRCAContact : ITrackable
    {
        public Guid ReimbursementEntityRCAContactId { get; set; }
        public Guid ReimbursementEntityId { get; set; }
        public ReimbursementEntity? ReimbursementEntity { get; set; }
        public Guid RCAContactId { get; set; }
        public RCAContact? RCAContact { get; set; }
        public bool IsPrimaryContact { get; set; } = false;

        public Guid CreatedUserId { get; set; } = Guid.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? ModifiedUserId { get; set; } = null;
        public DateTime? ModifiedDate { get; set; } = null;

    }
}
