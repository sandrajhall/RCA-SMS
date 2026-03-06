using RCA_StudyManager.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.Domain
{
    [Table("Hospital")]
    public class Hospital : ITrackable
    {
        public Guid HospitalId { get; set; }
        public string? MigratedHospitalId { get; set; } = string.Empty;
        [Required(ErrorMessage = "Hospital Name is required")]
        public string HospitalName { get; set; } = string.Empty;
        public string? HospitalShortName { get; set; } = string.Empty;
        public string? HospitalCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "Adddress1 is required")]
        public string Address1 { get; set; } = string.Empty;
        public string? Address2 { get; set; } = string.Empty;
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;
        [Required(ErrorMessage = "State is required")]
        public string State { get; set; } = string.Empty;
        [Required(ErrorMessage = "ZipCode is required")]
        public string ZipCode { get; set; } = string.Empty;
        public string? County {  get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public string? FaxNumber { get; set; } = string.Empty;
        public string? HospitalComments { get; set; } = string.Empty;
        public Guid? ReimbursementEntityId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDuplicate { get; set; } = false; // Flag to indicate if this hospital is a potential duplicate of another record
        public string? DuplicateOfHospitalId { get; set; } = null; // If IsDuplicate is true, this field can store the HospitalId of the record it is a duplicate of

        public Guid CreatedUserId { get; set; } = Guid.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? ModifiedUserId { get; set; } = null;
        public DateTime? ModifiedDate { get; set; } = null;

        public ICollection<DailyPathSubmission> DailyPathSubmissions { get; set; }
    }
}
