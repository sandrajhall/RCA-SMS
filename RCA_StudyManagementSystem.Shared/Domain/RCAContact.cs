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
    [Table("RCAContact")]
    public class RCAContact : ITrackable
    {
        public Guid RCAContactId { get; set; }
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;
        public string? Credentials { get; set; } = string.Empty;
        public string? Title { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public string? PhoneExtension { get; set; } = string.Empty;
        public string? Comments { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public Guid CreatedUserId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? ModifiedUserId { get; set; }

        public ICollection<ReimbursementEntityRCAContact>? ReimbursementEntityRCAContacts { get; set; }

        public ICollection<ReimbursementEntity>? ReimbursementEntities { get; set; }


    }
}
