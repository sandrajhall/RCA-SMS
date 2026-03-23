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
    [Table("StudyContact")]
    public class StudyContact : ITrackable
    {
        public Guid StudyContactId { get; set; } // Unique identifier for the StudyContact
        public Guid StudyId { get; set; }

        public Study? Study { get; set; } = null!; // Navigation property to the Study

        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;

        public string? Role { get; set; } = string.Empty;

        public string? Email { get; set; } = string.Empty;
        public string? PhoneNumber {  get; set; } = string.Empty;

        public string? Comments { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public bool IsOnTeamsChannel { get; set; } = false;
        public bool IsExportEmailReceived { get; set; } = false;

        public bool IsConfidentialAgreementSigned { get; set; } = false;


        public DateTime CreatedDate { get; set; }
        public Guid CreatedUserId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? ModifiedUserId { get; set; }
    }
}
