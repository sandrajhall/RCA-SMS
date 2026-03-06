using RCA_StudyManagementSystem.Shared.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RCA_StudyManagementSystem.Shared.Domain
{
    [Table("Lookup")]
    public class Lookup : ITrackable
    {
        [Key]
        public Guid LookupId { get; set; } = Guid.NewGuid();
        public string LookupName { get; set; } = string.Empty; // The name displayed in the UI for this lookup
        public string LookupType { get; set; } = string.Empty; // Type of lookup
        public string? LookupCode { get; set; } = string.Empty; // Unique code for the lookup
        public string? ParentCategory { get; set; } = null; // Optional parent category for hierarchical lookups
        public string? Comments { get; set; } = null;
        public bool IsActive { get; set; } = true; // Indicates if the lookup is currently active or not
        public int SortOrder { get; set; } = 0; // Used to order lookups in the UI

        public ICollection<StudyLookup>? StudyLookups { get; set; } = new List<StudyLookup>();

        [NotMapped]
        public bool IsModified { get; set; } = false; // Flag to indicate if the lookup has been modified since last save
        [NotMapped]
        public bool IsNew { get; set; } = false; // Flag to indicate if the lookup is new and has not been saved yet

        public Guid CreatedUserId { get; set; } = Guid.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? ModifiedUserId { get; set; } = null;
        public DateTime? ModifiedDate { get; set; } = null;


    }
}
