using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.ViewModels
{
    public class CCRSummaryView
    {
        public string? CaseNumber { get; set; }

        public string? FName { get; set; }
        public string? LName { get; set; }
        public string? MName { get; set; }
        public string? Suffix { get; set; }
        public string? PreferredName { get; set; }

        public string? DOB { get; set; }
        public string? SSN { get; set; }
        public string? Race { get; set; }
        public string? Sex { get; set; }
        
        public string? PathDate { get; set; }
        public string? PathNo { get; set; }

        public string? MdFName { get; set; }
        public string? MdLName { get; set; }
        public string? MidInitial { get; set; }
        public string? MdSuffix { get; set; }

        public string? Addr1 { get; set; }
        public string? Addr2 { get; set; }
        public string? Addr3 { get; set; }

        public string? MdCity { get; set; }
        public string? MdState { get; set; }
        public string? MdZip { get; set; }

        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? MdFax { get; set; }

        public string? HospName { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Phone { get; set; }

        public string? HospFax { get; set; }
        public string? HospCity { get; set; }
        public string? HospState { get; set; }
        public string? HospZip { get; set; }

        public string? ContactPerson { get; set; }
    }
}
