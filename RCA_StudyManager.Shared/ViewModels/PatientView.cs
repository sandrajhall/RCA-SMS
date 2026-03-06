using RCA_StudyManager.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.ViewModels
{
    public class PatientView : Patient
    {
        public string StudyColor { get; set; } = string.Empty;
    
    }
}
