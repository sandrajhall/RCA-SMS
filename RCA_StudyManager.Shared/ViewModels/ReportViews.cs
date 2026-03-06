using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static MudBlazor.Colors;

namespace RCA_StudyManager.Shared.ViewModels
{

    public class  PathCountByStudyByDate
    {
        public string HospitalName { get; set; } = string.Empty;
        public int EligiblePathCount { get; set; }
        public int SubmittedPathCount { get; set; }
        public int EnrolledPathCount { get; set; }

    }

    public class RaceCountByDate
    {
        public string RaceName { get; set; } = string.Empty;
        public int PathCount { get; set; }

    }

    public class EthnicityCountByDate
    {
        public string EthnicityName { get; set; } = string.Empty;
        public int PathCount { get; set; }

    }

    public class RaceEthnicityView
    {
        public int White { get; set; }
        public int BlackOrAfricanAmerican { get; set; }
        public int AmericanIndianOrAlaskaNative { get; set; }
        public int Chinese { get; set; }
        public int Japanese { get; set; }
        public int Filipino { get; set; }
        public int NativeHawaiian { get; set; }
        public int Korean { get; set; }
        public int Vietnamese { get; set; }
        public int Laotian { get; set; }
        public int Hmong { get; set; }
        public int Cambodian { get; set; }
        public int Thai { get; set; }
        public int AsianIndianorPakistani { get; set; }
        public int AsianIndian { get; set; }
        public int Pakistani { get; set; }
        public int Micronesian { get; set; }
        public int ChamorroChamoru { get; set; }
        public int Guamanian { get; set; }
        public int Polynesian { get; set; }
        public int Tahitian { get; set; }
        public int Samoan { get; set; }
        public int Tongan { get; set; }
        public int Melanesian { get; set; }
        public int FijiIslander { get; set; }
        public int PapuaNewGuinean { get; set; }
        public int OtherAsian { get; set; }
        public int PacificIslander { get; set; }
        public int SomeOtherRace { get; set; }
        public int RaceUnknown { get; set; }
        public int NonSpanishNonHispanic { get; set; }
        public int Mexican { get; set; }
        public int PuertoRican { get; set; }
        public int Cuban { get; set; }
        public int SouthorCentralAmerican { get; set; }
        public int OtherSpanishHispanic { get; set; }
        public int SpanishHispanicLatino { get; set; }
        public int Spanishsurnameonly { get; set; }
        public int DominicanRepublic { get; set; }
        public int UnknownSpanishHispanic { get; set; }

    }
}
