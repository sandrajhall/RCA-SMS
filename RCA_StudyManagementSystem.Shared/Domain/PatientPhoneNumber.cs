using RCA_StudyManagementSystem.Shared.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RCA_StudyManagementSystem.Shared.Domain
{
    [Table("PatientPhoneNumber")]
    public class PatientPhoneNumber : ITrackable
    {
        [Key]
        public Guid PatientPhoneNumberId { get; set; }

        //[ForeignKey("Patient")]
        public Guid PatientId { get; set; }
        [Required(ErrorMessage = "Phone Number is required")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone Type is required")]
        public string PhoneType { get; set; } = string.Empty; // e.g. "Mobile", "Home", "Other"
        public string? PhoneNumberComments { get; set; } = string.Empty; // Optional comments about the phone number
        public bool IsPrimary { get; set; } = false;

        public Patient? Patient { get; set; } // Navigation property to the Patient entity

        public DateTime CreatedDate { get; set; }
        public Guid CreatedUserId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? ModifiedUserId { get; set; }
    }
}
