using System;
using System.Collections.Generic;
using System.Text;

namespace RCA_StudyManagementSystem.Shared.ImportViews
{
    public class UNCAffiliatesImportView
    {
        public string? PAT_MRN_ID { get; set; }
        public string? PAT_ID { get; set; }
        public string? PAT_LAST_NAME { get; set; }
        public string? PAT_FIRST_NAME { get; set; }
        public string? PAT_MIDDLE_NAME { get; set; }
        public string? PreferredLang { get; set; }
        public DateTime? DOB { get; set; }
        public string? Gender { get; set; }
        public string? Race { get; set; }
        public string? Marital_Status { get; set; }
        public string? SSN { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EMAIL_ADDRESS { get; set; }
        public string? ADD_LINE_1 { get; set; }
        public string? CITY { get; set; }
        public string? State { get; set; }
        public string? ZIP { get; set; }

        public string? LAB_NAME { get; set; }
        public string? CASE_ID { get; set; }
        public string? RESULT_ID { get; set; }
        public string? SPEC_NUMBER_LN1 { get; set; }
        public string? SPECIMEN_ID { get; set; }
        public string? SpecSource_Name { get; set; }
        public string? SpecSource_Code { get; set; }
        public string? SpecType_Name { get; set; }
        public string? CaseStatus { get; set; }
        public string? CASE_TYPE_ID { get; set; }
        public string? Submitter { get; set; }
        public DateTime? SignedOutDate { get; set; }
        public DateTime? CollectedDate { get; set; }

        public string? FinalDiagnosis { get; set; }
        public string? Addendum_1 { get; set; }
        public string? Addendum_2 { get; set; }
        public string? Addendum_3 { get; set; }
        public string? Addendum_4 { get; set; }
        public string? Addendum_5 { get; set; }
        public string? SynopticTerms { get; set; }
        public string? PosTerm { get; set; }
        public string? NegTerm { get; set; }
        public string? ICD_10 { get; set; }
        public string? Order_Comments { get; set; }
        public string? SNOMED { get; set; }
        public string? SNOMED_Rslt { get; set; }
        public string? DX_NAME { get; set; }
        public string? Diag_Comment { get; set; }
        public string? Clinical_History { get; set; }
        public string? AuthorizingProvider { get; set; }
        public string? Pathologist { get; set; }
        public string? Synoptic_Report { get; set; }
        public string? Gross_Description { get; set; }

        public string? Study { get; set; }
        public string? HospitalName { get; set; } = null;
    }
}
