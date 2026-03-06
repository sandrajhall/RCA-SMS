using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MudBlazor;
using MudBlazor.Utilities;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using RCA_StudyManagementSystem.Shared.Interfaces;
using RCA_StudyManagementSystem.Shared.ViewModels;

namespace RCA_StudyManagementSystem.Shared.Domain
{
    [Table("Study")]
    public class Study : ITrackable
    {
        [Key]
        public Guid StudyId { get; set; }

        [Required(ErrorMessage = "Study name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Study prefix is required")]
        public string Prefix { get; set; } = string.Empty;

        public string? PrincipalInvestigator { get; set; } = null;
        public string? ProjectManager { get; set; } = null;

        public DateTime? CCR_F14_ApprovalDate { get; set; } = null;

        public string? StudySite { get; set; } = string.Empty;

        public string? ColorLight { get; set; } = null;
        public string? ColorDark { get; set; } = null;

        [NotMapped]
        public MudColor? MudColorLight
        {
            get => ColorLight != null ? new MudColor(ColorLight) : null;
            set => ColorLight = value?.ToString();
        }

        public DateTime? StartDate { get; set; } = null; // Start date of the study
        public DateTime? EndDate { get; set; } = null; // End date of the study

        public string? MinAge { get; set; } = null; // Minimum age for patients in the study
        public string? MaxAge { get; set; } = null; // Maximum age for patients in the study

        public string? PathMinAge { get; set; } = null; // Minimum age for pathology samples
        public string? PathMaxAge { get; set; } = null; // Maximum age for pathology samples
        public string? PathMaxNum { get; set; } = null; // Maximum number of path reports per patient

        public string? Design { get; set; } = string.Empty; // Study design description, number of patients, etc.
        public string? Comments { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true; // Indicates if the study is currently active
        public bool IsArchived { get; set; } = false; // Indicates if the study is archived
        public DateTime? LastEnrolledDate { get; set; } = null; // Date of the last patient enrolled in the study

        public string? InvoiceDesignation { get; set; } = null; // Designation for invoicing purposes

        public ICollection<PatientStatus>? PatientStatuses { get; set; } // Navigation property for related patient status records

        [NotMapped]
        public int DaysSinceLastEnrollment
        {
            get
            {
                if (LastEnrolledDate.HasValue)
                {
                    return (DateTime.UtcNow - LastEnrolledDate.Value).Days;
                }
                return 0; // If never enrolled, return a zero
            }
        }



        public ICollection<StudyLookup>? StudyLookups { get; set; } = new List<StudyLookup>();

        public ICollection<StudyHistology>? StudyHistologies { get; set; } = new List<StudyHistology>();

        public ICollection<StudyHeader>? StudyHeaders { get; set; } = new List<StudyHeader>();

        public ICollection<StudyContact>? StudyContacts { get; set; } = new List<StudyContact>();

        public Guid CreatedUserId { get; set; } = Guid.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? ModifiedUserId { get; set; } = null;
        public DateTime? ModifiedDate { get; set; } = null;

        public bool IsMinAgeValid(Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));
            if (!patient.DateOfBirth.HasValue) return false; // If DOB is not set, cannot validate
            if (string.IsNullOrWhiteSpace(MinAge)) return true; // No minimum age set, so valid by default
            if (!int.TryParse(MinAge, out int minAgeValue)) return true; // If MinAge is not a valid integer, consider it valid
            var age = CalculateAge(patient.DateOfBirth.Value, DateTime.UtcNow);
            return age >= minAgeValue;
        }

        public bool IsMaxAgeValid(Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));
            if (!patient.DateOfBirth.HasValue) return false; // If DOB is not set, cannot validate
            if (string.IsNullOrWhiteSpace(MaxAge)) return true; // No maximum age set, so valid by default
            if (!int.TryParse(MaxAge, out int maxAgeValue)) return true; // If MaxAge is not a valid integer, consider it valid
            var age = CalculateAge(patient.DateOfBirth.Value, DateTime.UtcNow);
            return age <= maxAgeValue;
        }

        private int CalculateAge(DateTime birthDate, DateTime currentDate)
        {
            int age = currentDate.Year - birthDate.Year;
            if (currentDate < birthDate.AddYears(age)) age--;
            return age;
        }

        public bool IsPathMinAgeValid(PathReport path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!path.DateOfProcedure.HasValue) return false; // If DOB is not set, cannot validate
            if (string.IsNullOrWhiteSpace(PathMinAge)) return true; // No minimum age set, so valid by default
            if (!int.TryParse(PathMinAge, out int minAgeValue)) return true; // If PathMinAge is not a valid integer, consider it valid
            var age = CalculateAgeDays(path.DateOfProcedure.Value, DateTime.UtcNow);
            return age >= minAgeValue;
        }

        public bool IsPathMaxAgeValid(PathReport path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!path.DateOfProcedure.HasValue) return false; // If DOB is not set, cannot validate
            if (string.IsNullOrWhiteSpace(PathMaxAge)) return true; // No maximum age set, so valid by default
            if (!int.TryParse(PathMaxAge, out int maxAgeValue)) return true; // If PathMaxAge is not a valid integer, consider it valid
            var age = CalculateAgeDays(path.DateOfProcedure.Value, DateTime.UtcNow);
            return age <= maxAgeValue;
        }

        public bool IsPathMaxAgeWeekOut(PathReport path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!path.DateOfProcedure.HasValue) return false; // If DOB is not set, cannot validate
            if (string.IsNullOrWhiteSpace(PathMaxAge)) return true; // No maximum age set, so valid by default
            if (!int.TryParse(PathMaxAge, out int maxAgeValue)) return true; // If PathMaxAge is not a valid integer, consider it valid
            var age = CalculateAgeDays(path.DateOfProcedure.Value, DateTime.UtcNow);
            return (age <= maxAgeValue) && ((maxAgeValue - age) <= 7);
        }

        public int CalculateAgeDays(DateTime procedureDate, DateTime currentDate)
        {
            TimeSpan difference = currentDate - procedureDate;
            int age = (int)difference.TotalDays;
            return age;
        }

        public bool IsPathMaxNumValid(int currentPathCount)
        {
            if (string.IsNullOrWhiteSpace(PathMaxNum)) return true; // No maximum number set, so valid by default
            if (!int.TryParse(PathMaxNum, out int maxNumValue)) return true; // If PathMaxNum is not a valid integer, consider it valid
            return currentPathCount < maxNumValue;
        }

        public bool IsCountyValid(string county)
        {
            var studyCountyLookups = StudyLookups?
                .Where(sl => sl.Lookup != null && sl.Lookup.LookupType == "County" && sl.StudyId == this.StudyId && sl.IsActive)
                .Select(sl => sl.Lookup.LookupName).ToList();
            if (studyCountyLookups == null || !studyCountyLookups.Any()) return true; // No county restrictions, so valid by default
            if (string.IsNullOrWhiteSpace(county)) return false; // Patient county is not set, cannot validate
            if (studyCountyLookups.Any(c => c == county)) return true;
            return false;
        }

        public void CheckEligibility(Patient patient, PathReport pathReport, ref PathReportView pathReportView)
        {

            if (IsMinAgeValid(patient))
            {
                pathReportView.IsMinAgeValid = true;
            }
            if (IsMaxAgeValid(patient))
            {
                pathReportView.IsMaxAgeValid = true;
            }
            if (IsPathMinAgeValid(pathReport))
            {
                pathReportView.IsPathMinAgeValid = true;
            }
            else
            {
                pathReportView.IsPathMinAgeValid = false;
                TimeSpan difference = DateTime.Now - pathReportView.DateOfProcedure.Value;
                pathReportView.MinAgeDiff = Int32.Parse(this.PathMinAge) - (int)difference.TotalDays;
            }
            if (IsPathMaxAgeValid(pathReport))
            {
                pathReportView.IsPathMaxAgeValid = true;
                if(this.PathMaxAge != null)
                {
                    TimeSpan difference = DateTime.Now - pathReportView.DateOfProcedure.Value;
                    pathReportView.MaxAgeDiff = (int)difference.TotalDays - Int32.Parse(this.PathMaxAge);
                    if (pathReportView.MaxAgeDiff <= 7)
                    {
                        pathReportView.IsPathMaxAgeWeekOut = true;
                    }
                }

            }

            int currentPathCount = patient.PathReports?.Count ?? 0;
            if (IsPathMaxNumValid(currentPathCount))
            {
                pathReportView.IsPathMaxNumValid = true;
            }
            if (IsCountyValid(patient.County))
            {
                pathReportView.IsCountyValid = true;
            }
        }
    }
}
