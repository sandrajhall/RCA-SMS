using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.DTOs
{
    public class InvoiceEmailData
    {
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ReimbursementEntity { get; set; }
        public string Quarter { get; set; }
        public int Year { get; set; }   
        public string TotalAmount { get; set; }
    }
}
