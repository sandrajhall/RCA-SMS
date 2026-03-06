using RCA_StudyManagementSystem.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.ViewModels
{
    public class ExportView : Batch
    {
        public string PathId { get; set; } = string.Empty;
    }
}
