using System;
using System.Collections.Generic;
using System.Text;

namespace RCA_StudyManagementSystem.Shared.ImportViews
{
    public class DukeImportView
    {
        public string? MedicalRecordNumber { get; set; }

        public string? LastName { get; set; }

        public string? FirstName { get; set; }

        public string? MiddleName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Sex { get; set; }

        public string? SocialSecurityNumber { get; set; }

        public string? CurrentAddress { get; set; }

        public string? CurrentCity { get; set; }

        public string? DiagnosisState { get; set; }

        public string? CurrentPostalCode { get; set; }

        public string? Race1 { get; set; }

        public string? MaritalStatusAtDiagnosis { get; set; }

        public string? Telephone { get; set; }

        public string? PathReportNumberId { get; set; }

        public DateTime? PathReportCollectedDate { get; set; }

        public string? PathReportAuthorizingProvider { get; set; }

        public string? PathReportOrderingLocation { get; set; }

        public string? PathReportPathologist { get; set; }

        public string? PathReportFinalDiagnosis { get; set; }

        public string? PathReportComment { get; set; }

        public string? PathReportClinicalHistory { get; set; }

        public string? PathReportMicroscopicDescription { get; set; }

        public string? Addendum1 { get; set; }

        public string? Addendum2 { get; set; }

        public string? Addendum3 { get; set; }

        public string? Addendum4 { get; set; }

        public string? Addendum5 { get; set; }

        public string? Study { get; set; }
    }
}
