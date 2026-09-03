using System;
using System.Collections.Generic;
using System.Text;

namespace RCA_StudyManagementSystem.Shared.ImportViews
{
    public class UNCRandolphImportView
    {
        public string? FACILITY { get; set; }
        public string? MRN { get; set; }
        public string? LAST_NAME { get; set; }
        public string? FIRST_NAME { get; set; }
        public string? MIDDLE_INITIAL { get; set; }
        public string? ADDRESS { get; set; }
        public string? CITY { get; set; }
        public string? STATE { get; set; }
        public string? ZIP { get; set; }
        public string? SSN { get; set; }
        public string? HOME_PHONE { get; set; }

        public DateTime? BIRTH_DATE { get; set; }

        public string? RACE { get; set; }
        public string? SEX { get; set; }
        public string? MARITAL_STATUS { get; set; }
        public string? DISCHARGE_DISPOSITION { get; set; }

        public string? C_Specimen_Specnum_Formatted { get; set; }
        public DateTime? C_Specimen_Accession_Date { get; set; }
        public string? C_Specimen_Accession_Time { get; set; }

        public string? AUTHRZING_Last_Name { get; set; }
        public string? AUTHRZING_First_Name { get; set; }
        public string? AUTHRZING_Middle_Name { get; set; }

        public string? C_D_Person_Phy_Street { get; set; }
        public string? C_D_Person_Phy_City { get; set; }
        public string? C_D_Person_Phy_State { get; set; }
        public string? C_D_Person_Phy_Zip { get; set; }

        public string? Clinical_History { get; set; }
        public string? Gross_Description { get; set; }
        public string? Final_Microscopic_Diagnosis { get; set; }
        public string? Addendum { get; set; }
        public string? Comments { get; set; }
        public string? Pathologist { get; set; }
        public string? Language { get; set; }

        public string? Study { get; set; }
    }
}
