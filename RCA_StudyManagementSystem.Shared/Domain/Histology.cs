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
    [Table("Histology")]
    public class Histology : ITrackable
    {
        [Key]
        public Guid HistologyId { get; set; } = Guid.NewGuid();
        public string HistologyCode { get; set; } = string.Empty;
        public string HistologyBehavior { get; set; } = string.Empty;
        public string? HistologyName { get; set; } = null; // The name displayed in the UI for this histology
        public bool IsPreferred { get; set; } = false; // Indicates if this histology is preferred for the study
        public string? Comments { get; set; } = null;
        public bool IsActive { get; set; } = true; // Indicates if the histology is currently active or not
        public int SortOrder { get; set; } = 0; // Used to order histology in the UI

        public ICollection<StudyHistology>? StudyHistologies { get; set; } = new List<StudyHistology>();

        [NotMapped]
        public bool IsModified { get; set; } = false; // Flag to indicate if the histology has been modified since last save
        [NotMapped]
        public bool IsNew { get; set; } = false; // Flag to indicate if the histology is new and has not been saved yet

        public DateTime CreatedDate { get; set; }
        public Guid CreatedUserId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? ModifiedUserId { get; set; }

    }
}
