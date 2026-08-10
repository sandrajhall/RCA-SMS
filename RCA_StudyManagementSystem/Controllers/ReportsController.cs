using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RCA_StudyManagementSystem.Data;
using RCA_StudyManagementSystem.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RCA_StudyManagementSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudiesController> _logger;



        public ReportsController(ApplicationDbContext context, ILogger<StudiesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Gets the Paths Count by Study and Date Range
        [HttpGet("PathsByStudyByDate/{studyId}/{startDateStr}/{endDateStr}")]
        public async Task<ActionResult<IEnumerable<PathCountByStudyByDate>>> GetPathsByStudyByDate(Guid studyId, string startDateStr, string endDateStr)
        {
            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);

            var submissions = await _context.DailyPathSubmissions
                .Include(d => d.Hospitals)
                .Include(s => s.Studies)
                .Where(s => s.Date >= startDate && s.Date <= endDate && s.StudyId == studyId)
                .GroupBy(s => s.Hospitals.HospitalName)
                .Select(g => new PathCountByStudyByDate
                {
                    HospitalName = g.Key,
                    EligiblePathCount = 0, // Placeholder, as this will be calculated in the main query
                    SubmittedPathCount = 0  // Placeholder, will be calculated in the loop below
                })
                .AsNoTracking()
                .TagWithCallSite()
                .AsSplitQuery()
                .ToListAsync();


            foreach (var item in submissions)
            {
                // Find all DailyPathSubmissions for this hospital in the date range
                var hospitalSubmissions = await _context.DailyPathSubmissions
                    .Where(s => s.Hospitals.HospitalName == item.HospitalName && s.Date >= startDate && s.Date <= endDate)
                    .ToListAsync();

                item.SubmittedPathCount = hospitalSubmissions.Sum(s => int.TryParse(s.Value, out int count) ? count : 0);
            }


            try
            {
                var query = _context.PathReports
                    .Join(_context.Patients,
                        p => p.PatientId,
                        c => c.PatientId,
                        (p, c) => new { p, c })
                    .Join(_context.Hospitals,
                        pc => pc.p.HospitalId,
                        h => h.HospitalId,
                        (pc, h) => new { pc.p, pc.c, h })
                    .Join(_context.Studies,
                        pch => pch.c.StudyId,
                        s => s.StudyId,
                        (pch, s) => new { pch.p, pch.c, pch.h, s })
                    .Where(x => x.p.RcaExportDate >= startDate
                             && x.p.RcaExportDate <= endDate
                             && studyId == x.s.StudyId)
                    .GroupBy(x => x.h.HospitalName)
                    .Select(g => new PathCountByStudyByDate
                    {
                        HospitalName = g.Key,
                        EligiblePathCount = g.Count()
                    });

                var result = await query.ToListAsync();

                var enrolled = await _context.PatientStatuses
                .Include(c => c.Patient)
                .Include(p => p.Patient.PathReports)
                .Include(s => s.Study)
                .Where(s => s.Date >= startDate && s.Date <= endDate && s.Patient.StudyId == studyId && s.Status == "Participating/potential")
                .AsNoTracking()
                .TagWithCallSite()
                .AsSplitQuery()
                .ToListAsync();

                foreach (var item in enrolled)
                {
                    var hospitalName = item.Patient.PathReports.Select(pr => pr.SubmittingHospital).FirstOrDefault();
                    if (!string.IsNullOrEmpty(hospitalName))
                    {
                        var submission = submissions.FirstOrDefault(s => s.HospitalName == hospitalName);
                        if (submission != null)
                        {
                            submission.EnrolledPathCount += 1;
                        }
                        else
                        {
                            submissions.Add(new PathCountByStudyByDate
                            {
                                HospitalName = hospitalName,
                                EligiblePathCount = 0,
                                SubmittedPathCount = 0,
                                EnrolledPathCount = 1
                            });
                        }
                    }
                }

                foreach (var item in submissions)
                {
                    var eligible = result.FirstOrDefault(s => s.HospitalName == item.HospitalName);
                    if (eligible != null)
                    {
                        item.EligiblePathCount = eligible.EligiblePathCount;
                    }
                }
                foreach (var item in result)
                {
                    var submission = submissions.FirstOrDefault(s => s.HospitalName == item.HospitalName);
                    if (submission == null)
                    {
                        submissions.Add(new PathCountByStudyByDate
                        {
                            HospitalName = item.HospitalName,
                            EligiblePathCount = item.EligiblePathCount,
                            SubmittedPathCount = 0
                        });
                    }
                }

                return Ok(submissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Paths by Study by Date");
                return StatusCode(500, "Internal server error");
            }
        }

        // CESC Path Reports by Date
        [HttpGet("CECSPathsByDate/{startDateStr}/{endDateStr}")]
        public async Task<ActionResult<IEnumerable<PathCountByStudyByDate>>> GetCECSPathsByDate(string startDateStr, string endDateStr)
        {
            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);
            try
            {
                var query = _context.PathReports
                    .Join(_context.Patients,
                        p => p.PatientId,
                        c => c.PatientId,
                        (p, c) => new { p, c })
                    .Join(_context.Hospitals,
                        pc => pc.p.HospitalId,
                        h => h.HospitalId,
                        (pc, h) => new { pc.p, pc.c, h })
                    .Join(_context.Studies,
                        pch => pch.c.StudyId,
                        s => s.StudyId,
                        (pch, s) => new { pch.p, pch.c, pch.h, s })
                    .Where(x => x.p.RcaExportDate >= startDate
                                && x.p.RcaExportDate <= endDate
                                && x.s.Prefix == "CECS")
                    .GroupBy(x => x.h.HospitalName)
                    .Select(g => new PathCountByStudyByDate
                    {
                        HospitalName = g.Key,
                        EligiblePathCount = g.Count()
                    });
                var result = await query.ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving CESC Paths by Date");
                return StatusCode(500, "Internal server error");
            }
        }

        // CECS Case Total by Date
        [HttpGet("CECSCasesByDate/{startDateStr}/{endDateStr}")]
        public async Task<ActionResult<int>> GetCECSCasesByDate(string startDateStr, string endDateStr)
        {
            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);
            try
            {
                var query = _context.PathReports
                    .Join(_context.Patients,
                        p => p.PatientId,
                        c => c.PatientId,
                        (p, c) => new { p, c })
                    .Join(_context.Hospitals,
                        pc => pc.p.HospitalId,
                        h => h.HospitalId,
                        (pc, h) => new { pc.p, pc.c, h })
                    .Join(_context.Studies,
                        pch => pch.c.StudyId,
                        s => s.StudyId,
                        (pch, s) => new { pch.p, pch.c, pch.h, s })
                    .Where(x => x.p.RcaExportDate >= startDate
                             && x.p.RcaExportDate <= endDate
                             && x.s.Prefix == "CECS")
                    .GroupBy(x => x.c.PatientId)
                    .Select(g => new PathCountByStudyByDate
                    {
                        HospitalName = g.Key.ToString(),
                        EligiblePathCount = g.Count()
                    });

                int totalCases = query.Sum(g => g.EligiblePathCount);
                return Ok(totalCases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving CECS Cases by Date");
                return StatusCode(500, "Internal server error");
            }
        }

        // CECS Race Count by Date
        [HttpGet("CECSRaceCountByDate/{startDateStr}/{endDateStr}")]
        public async Task<ActionResult<IEnumerable<RaceCountByDate>>> GetCECSRaceCountByDate(string startDateStr, string endDateStr)
        {
            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);
            try
            {
                var query = _context.PathReports
                    .Join(_context.Patients,
                        p => p.PatientId,
                        c => c.PatientId,
                        (p, c) => new { p, c })
                    .Join(_context.Studies,
                        pc => pc.c.StudyId,
                        s => s.StudyId,
                        (pc, s) => new { pc.p, pc.c, s })
                    .Where(x => x.p.RcaExportDate >= startDate
                             && x.p.RcaExportDate <= endDate
                             && x.s.Prefix == "CECS")
                    .GroupBy(x => x.c.Race)
                    .Select(g => new RaceCountByDate
                    {
                        RaceName = g.Key,
                        PathCount = g.Count()
                    });
                var result = await query.ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving CECS Race by Date");
                return StatusCode(500, "Internal server error");
            }

        }

        // CECS Ethnicity Count by Date
        [HttpGet("CECSEthnicityCountByDate/{startDateStr}/{endDateStr}")]
        public async Task<ActionResult<IEnumerable<EthnicityCountByDate>>> GetCECSEthnicityCountByDate(string startDateStr, string endDateStr)
        {
            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);
            try
            {
                var query = _context.PathReports
                    .Join(_context.Patients,
                        p => p.PatientId,
                        c => c.PatientId,
                        (p, c) => new { p, c })
                    .Join(_context.Studies,
                        pc => pc.c.StudyId,
                        s => s.StudyId,
                        (pc, s) => new { pc.p, pc.c, s })
                    .Where(x => x.p.RcaExportDate >= startDate
                             && x.p.RcaExportDate <= endDate
                             && x.s.Prefix == "CECS")
                    .GroupBy(x => x.c.Ethnicity)
                    .Select(g => new EthnicityCountByDate
                    {
                        EthnicityName = g.Key,
                        PathCount = g.Count()
                    });
                var result = await query.ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving CECS Ethnicity by Date");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpGet("PathsByStudyByDateCSV/{studyId}/{startDateStr}/{endDateStr}")]
        public async Task<string> GetPathsByStudyByDateCSV(Guid studyId, string startDateStr, string endDateStr)
        {
            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);

            var submissions = await _context.DailyPathSubmissions
                .Include(d => d.Hospitals)
                .Include(s => s.Studies)
                .Where(s => s.Date >= startDate && s.Date <= endDate && s.StudyId == studyId)
                .GroupBy(s => s.Hospitals.HospitalName)
                .Select(g => new PathCountByStudyByDate
                {
                    HospitalName = g.Key,
                    EligiblePathCount = 0, // Placeholder, as this will be calculated in the main query
                    SubmittedPathCount = 0,  // Placeholder, will be calculated in the loop below
                    EnrolledPathCount = 0 // Placeholder, will be calculated in the loop below
                })
                .AsNoTracking()
                .TagWithCallSite()
                .AsSplitQuery()
                .ToListAsync();


            foreach (var item in submissions)
            {
                // Find all DailyPathSubmissions for this hospital in the date range
                var hospitalSubmissions = await _context.DailyPathSubmissions
                    .Where(s => s.Hospitals.HospitalName == item.HospitalName && s.Date >= startDate && s.Date <= endDate)
                    .ToListAsync();

                item.SubmittedPathCount = hospitalSubmissions.Sum(s => int.TryParse(s.Value, out int count) ? count : 0);
            }

            var query = _context.PathReports
                .Join(_context.Patients,
                    p => p.PatientId,
                    c => c.PatientId,
                    (p, c) => new { p, c })
                .Join(_context.Hospitals,
                    pc => pc.p.HospitalId,
                    h => h.HospitalId,
                    (pc, h) => new { pc.p, pc.c, h })
                .Join(_context.Studies,
                    pch => pch.c.StudyId,
                    s => s.StudyId,
                    (pch, s) => new { pch.p, pch.c, pch.h, s })
                .Where(x => x.p.RcaExportDate >= startDate
                         && x.p.RcaExportDate <= endDate
                         && x.s.StudyId == studyId)
                .GroupBy(x => x.h.HospitalName)
                .Select(g => new PathCountByStudyByDate
                {
                    HospitalName = g.Key,
                    EligiblePathCount = g.Count()
                });
            var result = await query.ToListAsync();

            var enrolled = await _context.PatientStatuses
                .Include(c => c.Patient)
                .Include(p => p.Patient.PathReports)
                .Include(s => s.Study)
                .Where(s => s.Date >= startDate && s.Date <= endDate && s.Patient.StudyId == studyId && s.Status == "Participating/potential")
                .AsNoTracking()
                .TagWithCallSite()
                .AsSplitQuery()
                .ToListAsync();

            foreach (var item in enrolled)
            {
                var hospitalName = item.Patient.PathReports.Select(pr => pr.SubmittingHospital).FirstOrDefault();
                if (!string.IsNullOrEmpty(hospitalName))
                {
                    var submission = submissions.FirstOrDefault(s => s.HospitalName == hospitalName);
                    if (submission != null)
                    {
                        submission.EnrolledPathCount += 1;
                    }
                    else
                    {
                        submissions.Add(new PathCountByStudyByDate
                        {
                            HospitalName = hospitalName,
                            EligiblePathCount = 0,
                            SubmittedPathCount = 0,
                            EnrolledPathCount = 1
                        });
                    }
                }
            }

            foreach (var item in submissions)
            {
                var eligible = result.FirstOrDefault(s => s.HospitalName == item.HospitalName);
                if (eligible != null)
                {
                    item.EligiblePathCount = eligible.EligiblePathCount;
                }
            }
            foreach (var item in result)
            {
                var submission = submissions.FirstOrDefault(s => s.HospitalName == item.HospitalName);
                if (submission == null)
                {
                    submissions.Add(new PathCountByStudyByDate
                    {
                        HospitalName = item.HospitalName,
                        EligiblePathCount = item.EligiblePathCount,
                        SubmittedPathCount = 0
                    });
                }
            }

            using (var writer = new StringWriter())
            {
                using (var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
                {
                    // This writes the data into the StringWriter
                    csv.WriteRecords(submissions);
                }

                // This extracts the actual CSV text from the StringWriter
                return writer.ToString();
            }

        }
    }
}
