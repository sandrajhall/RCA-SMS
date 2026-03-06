using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.DTOs
{
    public class FileData
    {
        public Stream FileStream { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }
}
