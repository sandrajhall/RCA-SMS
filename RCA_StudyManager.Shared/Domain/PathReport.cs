using RCA_StudyManager.Shared.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RCA_StudyManager.Shared.Domain
{
    [Table("PathReport")]
    public class PathReport
    {
        [Key]
        public Guid PathReportId { get; set; }
        public string? MigratedCCRNumber { get; set; } = string.Empty; // For migrated records from CCR
        //[ForeignKey("Patient")]
        public Guid PatientId { get; set; }
        public string? CaseNumber { get; set; } = string.Empty; // For easier reference to the patient case number
        public string? StudyPrefix { get; set; } = string.Empty; // For easier reference to the study prefix
        public string? StudyColor { get; set; } = string.Empty; // For easier reference to the study color
        public int? PathIndex { get; set; } = 0; // Index for the path report, used to order multiple reports in the UI
        [Required(ErrorMessage = "Submitting Hospital is required")]
        public string? SubmittingHospital { get; set; } = string.Empty;
        public Guid? HospitalId { get; set; } = null; // Foreign key to the Hospital entity, if applicable
        public string HospAddress1 { get; set; } = string.Empty;
        public string? HospAddress2 { get; set; } = string.Empty;
        public string? HospCity { get; set; } = string.Empty;
        public string? HospState { get; set; } = string.Empty;
        public string? HospZipCode { get; set; } = string.Empty;
        public string? HospPhoneNumber { get; set; } = string.Empty;
        public string? HospFaxNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Submitting Hospital Path Report Number is required")]
        public string? SubmittingHospitalPathReportNumber { get; set; } = string.Empty;

        public string? OriginatingHospitalName { get; set; } = string.Empty; // Hospital where the path report originated
        public Guid? OrigHospitalId { get; set; } = null; // Foreign key to the Hospital entity, if applicable
        public string OrigHospAddress1 { get; set; } = string.Empty;
        public string? OrigHospAddress2 { get; set; } = string.Empty;
        public string? OrigHospCity { get; set; } = string.Empty;
        public string? OrigHospState { get; set; } = string.Empty;
        public string? OrigHospZipCode { get; set; } = string.Empty;
        public string? OrigHospPhoneNumber { get; set; } = string.Empty;
        public string? OrigHospFaxNumber { get; set; } = string.Empty;
        public string? OriginatingHospitalPathReportNumber { get; set; } = string.Empty;
        public string? OriginatingHospitalComments { get; set; } = string.Empty; // Hospital where the path report originated
        public bool PathReportObtained { get; set; } = false; // Indicates if the path report has been obtained
        public bool StudySubmission { get; set; } = false; // Indicates if the path report is being submitted as part of a study

        public string? SlidesResideAtSubmittingHospital { get; set; } = string.Empty;
        [Required(ErrorMessage = "Date of Procedure is required")]
        public DateTime? DateOfProcedure { get; set; } // set nullable so field can be blank on creation, annotate to do validation
        //[Required(ErrorMessage = "Age at Procedure is required")]
        public string AgeAtProcedure { get; set; } = string.Empty;
        public string? DxAddress1 { get; set; }
        public string? DxAddress2 { get; set; } = string.Empty;
        public string? DxCity { get; set; } = string.Empty;
        public string? DxState { get; set; } = string.Empty;
        public string? DxZipCode { get; set; } = string.Empty;
        public string? DxCountyCode { get; set; } = string.Empty;
        public string? DxCounty { get; set; } = string.Empty;
        public string? DxPhoneNumber { get; set; } = string.Empty;
        public string? ExportStatus { get; set; } = string.Empty;
        public DateTime? RcaExportDate { get; set; } = null;
        public string? BatchNumber { get; set; } = string.Empty;
        public bool IsAddendum { get; set; } = false;
        public bool IsOutsidePathReport { get; set; } = false;
        public bool IsOnHold { get; set; } = false; // Indicates if the path report is currently on hold

        public string? Reimbursement1 { get; set; } = string.Empty;
        public string? Reimbursement2 { get; set; } = string.Empty;
        public bool DoNotInvoice { get; set; } = false; // Indicates if the path report should not be invoiced

        [Required(ErrorMessage = "Site is required")]
        public string Site { get; set; } = string.Empty; // Site of the procedure, e.g., "Left Breast", "Right Lung", etc.
        public string? SiteCode { get; set; } = string.Empty; // Code representing the site, if applicable
        [Required(ErrorMessage = "Path Procedure is required")]
        public string PathProcedure { get; set; } = string.Empty; // Procedure performed during the path report
        public string? PathComments { get; set; } = string.Empty;

        [Required(ErrorMessage = "Histology Diagnosis is required")]
        public string HistologyDiagnosis1 { get; set; } = string.Empty;
        public string HistologyCode1 { get; set; } = string.Empty; // Code representing the histology diagnosis, if applicable
        public string HistologyBehavior1 { get; set; } = string.Empty; // Code representing the histology diagnosis behavior, if applicable
        public string? HistologyDiagnosisComments1 { get; set; } = string.Empty;
        public double? TumorSize { get; set; }
        public string? MarginStatus { get; set; } = string.Empty;
        public string? ALNL_PositiveNodes { get; set; } = string.Empty;
        public string? ALNL_NodesExamined { get; set; } = string.Empty;
        public string? HistologicDiff { get; set; } = string.Empty;
        public string? HistologicGrade { get; set; } = string.Empty;
        public string? HistologicDiff2 { get; set; } = string.Empty;
        public string? HistologicGrade2 { get; set; } = string.Empty;
        public double? TumorSize2 { get; set; }
        public string? MarginStatus2 { get; set; } = string.Empty;
        public string? ALNL_PositiveNodes2 { get; set; } = string.Empty;
        public string? ALNL_NodesExamined2 { get; set; } = string.Empty;
        public string? PSA { get; set; } = string.Empty;
        public double? PSA_ng_ml { get; set; } = 0;
        public string? PerineuralInvasion { get; set; } = string.Empty;
        public int? NumCoresBiopsy { get; set; }
        public int? NumCoresCancer { get; set; }
        public int? PrimGleason1 { get; set; }
        public int? SecGleason1 { get; set; }
        public int? GleasonSum1 { get; set; }
        public int? PrimGleason2 { get; set; }
        public int? SecGleason2 { get; set; }
        public int? GleasonSum2 { get; set; }

        [Required(ErrorMessage = "Authorizing Provider is required")]
        public string AuthorizingProvider { get; set; } = string.Empty;
        public Guid? DoctorId { get; set; } = null; // Foreign key to the Doctor entity, if applicable
        public string? AuthorizingProviderComments { get; set; } = string.Empty;
        public string? MDAddress1 { get; set; } = string.Empty;
        public string? MDAddress2 { get; set; } = string.Empty;
        public string? MDAddress3 { get; set; } = string.Empty;
        public string? MDCity { get; set; } = string.Empty;
        public string? MDState { get; set; } = string.Empty;
        public string? MDZipCode { get; set; } = string.Empty;
        public string? MDCounty { get; set; } = string.Empty;
        public string MDPhoneNumber1 { get; set; } = string.Empty;
        public string? MDPhoneNumber2 { get; set; } = string.Empty;
        public string? MDFaxNumber { get; set; } = string.Empty;
        public string? MDEmail { get; set; } = string.Empty;
        public string? Pathologist { get; set; } = string.Empty;
        public Guid? PathologistId { get; set; } = null; // Foreign key to the Pathologist entity, if applicable
        public string? PathologistComments { get; set; } = string.Empty;
        public string? PathAddress1 { get; set; } = string.Empty;
        public string? PathAddress2 { get; set; } = string.Empty;
        public string? PathAddress3 { get; set; } = string.Empty;
        public string? PathCity { get; set; } = string.Empty;
        public string? PathState { get; set; } = string.Empty;
        public string? PathZipCode { get; set; } = string.Empty;
        public string? PathCounty { get; set; } = string.Empty;
        public string PathPhoneNumber1 { get; set; } = string.Empty;
        public string? PathPhoneNumber2 { get; set; } = string.Empty;
        public string? PathFaxNumber { get; set; } = string.Empty;
        public string? PathEmail { get; set; } = string.Empty;

        public string? Site2 { get; set; } = string.Empty; // Site of the procedure, e.g., "Left Breast", "Right Lung", etc.
        public string? SiteCode2 { get; set; } = string.Empty; // Code representing the site, if applicable
        public string? PathProcedure2 { get; set; } = string.Empty; // Procedure performed during the path report
        public string? PathComments2 { get; set; } = string.Empty;
        public string? HistologyDiagnosis2 { get; set; } = string.Empty;
        public string? HistologyCode2 { get; set; } = string.Empty; // Code representing the histology diagnosis, if applicable
        public string? HistologyBehavior2 { get; set; } = string.Empty; // Code representing the histology diagnosis behavior, if applicable
        public string? HistologyDiagnosisComments2 { get; set; } = string.Empty;
       
        public string? AuthorizingProvider2 { get; set; } = string.Empty;
        public Guid? Doctor2Id { get; set; } = null; // Foreign key to the Doctor entity, if applicable
        public string? AuthorizingProviderComments2 { get; set; } = string.Empty;
        public string? MD2Address1 { get; set; } = string.Empty;
        public string? MD2Address2 { get; set; } = string.Empty;
        public string? MD2Address3 { get; set; } = string.Empty;
        public string? MD2City { get; set; } = string.Empty;
        public string? MD2State { get; set; } = string.Empty;
        public string? MD2ZipCode { get; set; } = string.Empty;
        public string? MD2County { get; set; } = string.Empty;
        public string MD2PhoneNumber1 { get; set; } = string.Empty;
        public string? MD2PhoneNumber2 { get; set; } = string.Empty;
        public string? MD2FaxNumber { get; set; } = string.Empty;
        public string? MD2Email { get; set; } = string.Empty;
        public string? Pathologist2 { get; set; } = string.Empty;
        public Guid? Pathologist2Id { get; set; } = null; // Foreign key to the Pathologist entity, if applicable
        public string? PathologistComments2 { get; set; } = string.Empty;
        public string? Path2Address1 { get; set; } = string.Empty;
        public string? Path2Address2 { get; set; } = string.Empty;
        public string? Path2Address3 { get; set; } = string.Empty;
        public string? Path2City { get; set; } = string.Empty;
        public string? Path2State { get; set; } = string.Empty;
        public string? Path2ZipCode { get; set; } = string.Empty;
        public string? Path2County { get; set; } = string.Empty;
        public string Path2PhoneNumber1 { get; set; } = string.Empty;
        public string? Path2PhoneNumber2 { get; set; } = string.Empty;
        public string? Path2FaxNumber { get; set; } = string.Empty;
        public string? Path2Email { get; set; } = string.Empty;

        public DateTime? EnrollmentDate { get; set; } = null;


        public Guid CreatedUserId { get; set; } = Guid.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? ModifiedUserId { get; set; } = null;
        public DateTime? ModifiedDate { get; set; } = null;

        public Patient? Patient { get; set; } // Navigation property to the Patient entity

        public ICollection<PathReportExport>? PathReportExports { get; set; } // Navigation property to related PathReportExport entities

        [NotMapped]
        public bool IsShownSite2 { get; set; } = false;

        [NotMapped]
        public string? PathId { get; set; } = null; // Used to track the path report ID in the UI, if applicable


        // used for autocomplete
        [NotMapped]
        public bool IsHospitalCleared { get; set; } = false; // Used to track if the hospital is cleared in the UI
        [NotMapped]
        public bool IsOrigHospitalCleared { get; set; } = false; // Used to track if the hospital is cleared in the UI
        [NotMapped]
        public bool IsReimbursement1Cleared { get; set; } = false; // Used to track if the hospital is cleared in the UI
        [NotMapped]
        public bool IsReimbursement2Cleared { get; set; } = false; // Used to track if the hospital is cleared in the UI
        [NotMapped]
        public bool IsDoctor1Cleared { get; set; } = false; // Used to track if the first doctor is cleared in the UI
        [NotMapped]
        public bool IsDoctor2Cleared { get; set; } = false; // Used to track if the second doctor is cleared in the UI
        [NotMapped]
        public bool IsPathologist1Cleared { get; set; } = false; // Used to track if the first pathologist is cleared in the UI
        [NotMapped]
        public bool IsPathologist2Cleared { get; set; } = false; // Used to track if the second pathologist is cleared in the UI

        [NotMapped]
        public Hospital? HospitalEntity { get; set; } = null; // Navigation property to the Hospital entity, if applicable
        [NotMapped]
        public Doctor? DoctorEntity1 { get; set; } = null; // Navigation property to the Doctor entity, if applicable
        [NotMapped]
        public Doctor? DoctorEntity2 { get; set; } = null; // Navigation property to the second Doctor entity, if applicable
        [NotMapped]
        public Doctor? PathologistEntity1 { get; set; } = null; // Navigation property to the Pathologist entity, if applicable
        [NotMapped]
        public Doctor? PathologistEntity2 { get; set; } = null; // Navigation property to the second Pathologist entity, if applicable

    }
}
