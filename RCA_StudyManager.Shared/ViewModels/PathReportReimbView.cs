using RCA_StudyManager.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.ViewModels
{
    public class PathReportReimbView : PathReport
    {
        public string ReimbursementHospitalName { get; set; } = string.Empty;
        public Guid ReimbursementHospitalId { get; set; } = Guid.Empty;

        public Guid ReimbursementEntityId { get; set; } = Guid.Empty;

        public string ReimbursementEntityName { get; set; } = string.Empty;

        public Guid StudyId { get; set; } = Guid.Empty;

        public string StudyDesignation { get; set; } = string.Empty;

    }
}
