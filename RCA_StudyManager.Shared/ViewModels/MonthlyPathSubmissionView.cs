using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.ViewModels
{
    public class MonthlyPathSubmissionView
    {
        public Guid HospitalId { get; set; }
        public Guid StudyId { get; set; }
        public string HospitalName { get; set; }
        public string HospitalShortName { get; set; }
        // This dictionary will hold the "saved" data for each day
        public Dictionary<int, string> DailyValues { get; set; } = new();
    }
}
