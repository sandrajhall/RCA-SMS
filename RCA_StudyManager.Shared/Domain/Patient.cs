using Microsoft.AspNetCore.Components;
using RCA_StudyManager.Shared.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace RCA_StudyManager.Shared.Domain
{

    [Table("Patient")]
    public class Patient : ITrackable
    {
        [Key]
        public Guid PatientId { get; set; }

        [Required(ErrorMessage = "Study ID is required")]
        //[ForeignKey("Study")]
        public Guid StudyId { get; set; } = Guid.Empty; // Foreign key to the Study this patient belongs to
        public string? MigratedCCRNo { get; set; } = string.Empty; // Previous number associated with the patient
        public string CaseNumber { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty; // Computed property for displaying the patient's first and last name

        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }
        public string? MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }
        public string? Suffix { get; set; } = string.Empty;
        public string? PreferredName { get; set; } = string.Empty;

        //[EmailAddress(ErrorMessage = "Invalid email address format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime? DateOfBirth { get; set; }  // set nullable so field can be blank on creation, annotate to do validation
        [Required(ErrorMessage = "SSN is required")]
        public string SocialSecurityNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Address1 is required")]
        public string Address1 { get; set; } = string.Empty;
        public string? Address2 { get; set; } = string.Empty;
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;
        [Required(ErrorMessage = "State is required")]
        public string State { get; set; } = string.Empty;
        [Required(ErrorMessage = "Zip code is required")]
        public string ZipCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "County is required")]
        public string County { get; set; } = string.Empty;
        public string? CountyCode { get; set; } = string.Empty; // Code representing the county, if applicable
        public string? PatientComments { get; set; } = string.Empty;

        [Required(ErrorMessage = "Race is required")]
        public string Race { get; set; } = string.Empty;
        public string? RaceCode { get; set; } = string.Empty; // Code representing the race, if applicable
        [Required(ErrorMessage = "Ethnicity is required")]
        public string Ethnicity { get; set; } = string.Empty;
        public string? EthnicityCode { get; set; } = string.Empty; // Code representing the ethnicity, if applicable
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;
        public string? GenderCode { get; set; } = string.Empty; // Code representing the gender, if applicable


        public bool IsActive { get; set; } = true; // Indicates if the patient is currently active

        public Guid CreatedUserId { get; set; } = Guid.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? ModifiedUserId { get; set; } = null;
        public DateTime? ModifiedDate { get; set; } = null;

        public Study? Study { get; set; } // Navigation property to the associated Study

        [NotMapped]
        public string StudyColor { get; set; } = string.Empty; // Color associated with the study, not mapped to the database


        [ValidateComplexType]
        public ICollection<PathReport> PathReports { get; set; } = new List<PathReport>();
        [ValidateComplexType]
        public virtual ICollection<PatientPhoneNumber> PatientPhoneNumbers { get; set; } = new List<PatientPhoneNumber>();

    }
}
