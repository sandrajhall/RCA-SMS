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
    [Table("Doctor")]
    public class Doctor : ITrackable
    {
        public Guid DoctorId { get; set; }
        public string? MigratedDoctorId { get; set; } = string.Empty; // For doctors migrated from legacy systems
        public string DisplayName { get; set; } = string.Empty; // e.g. "Dr. John A. Smith, MD"
        [Required (ErrorMessage = "First Name is required")]
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; } = string.Empty;
        public string? Suffix { get; set; } = string.Empty;
        [Required(ErrorMessage = "Primary Specialty is required")]
        public string PrimarySpecialty { get; set; } = string.Empty;
        public string? SecondarySpecialty { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone Number is required")]
        public string PhoneNumber1 { get; set; } = string.Empty;
        public string? PhoneNumber2 { get; set; } = string.Empty;
        public string? FaxNumber { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? DoctorComments { get; set; } = string.Empty;
        [Required(ErrorMessage = "Address1 is required")]
        public string Address1 { get; set; } = string.Empty;
        public string? Address2 { get; set; } = string.Empty;
        public string? Address3 { get; set; } = string.Empty;
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;
        [Required(ErrorMessage = "State is required")]
        public string State { get; set; } = string.Empty;
        [Required(ErrorMessage = "Zip Code is required")]
        public string ZipCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "County is required")]
        public string County { get; set; } = string.Empty;
        [Required(ErrorMessage = "License Type is required")]
        public string LicenseType { get; set; } = string.Empty; // MD, DO, MBBS, etc.
        public bool IsPathologist { get; set; } = false;

        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedDate { get; set; } = null;

        public bool IsActive { get; set; } = true;

        public bool IsDuplicate { get; set; } = false; // Flag to indicate if this doctor is a potential duplicate of another record
        public string? DuplicateOfDoctorId { get; set; } = null; // If IsDuplicate is true, this field can store the DoctorId of the record it is a duplicate of

        public Guid CreatedUserId { get; set; } = Guid.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? ModifiedUserId { get; set; } = null;
        public DateTime? ModifiedDate { get; set; } = null;
    }
}
