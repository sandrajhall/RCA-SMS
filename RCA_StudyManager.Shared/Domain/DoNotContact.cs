using Microsoft.AspNetCore.Components;
using RCA_StudyManager.Shared.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace RCA_StudyManager.Shared.Domain
{

    [Table("DoNotContact")]
    public class DoNotContact : ITrackable
    {
        [Key]
        public Guid DoNotContactId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }
        public string? MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        public string? DisplayName { get; set; } = string.Empty;    

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime? DateOfBirth { get; set; }  // set nullable so field can be blank on creation, annotate to do validation
        [Required(ErrorMessage = "SSN is required")]
        public string SocialSecurityNumber { get; set; } = string.Empty;

        public string? StudyName { get; set; } = string.Empty; 

        public DateTime? DateLastContact { get; set; }

        public string? PhoneNumber { get; set; } = string.Empty;


        public Guid CreatedUserId { get; set; } = Guid.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? ModifiedUserId { get; set; } = null;
        public DateTime? ModifiedDate { get; set; } = null;

    }
}
