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

        [HttpGet("PathsByStudyByDate/{studyId}/{startDateStr}/{endDateStr}")]
        public async Task<ActionResult<IEnumerable<PathCountByStudyByDate>>> GetPathsByStudyByDate(
     Guid studyId, string startDateStr, string endDateStr)
        {
            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);

            try
            {
                // Submitted counts by hospital
                var submissionsRaw = await _context.DailyPathSubmissions
                    .Where(s => s.Date >= startDate
                             && s.Date <= endDate
                             && s.StudyId == studyId)
                    .Select(s => new
                    {
                        HospitalName = s.Hospitals.HospitalName,
                        Value = s.Value
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var submissions = submissionsRaw
                    .GroupBy(s => s.HospitalName)
                    .Select(g => new PathCountByStudyByDate
                    {
                        HospitalName = g.Key,
                        SubmittedPathCount = g.Sum(x => int.TryParse(x.Value, out var count) ? count : 0),
                        EligiblePathCount = 0,
                        EnrolledPathCount = 0
                    })
                    .ToList();

                // Eligible counts by hospital
                var eligible = await _context.PathReports
                    .Where(p => p.RcaExportDate >= startDate
                             && p.RcaExportDate <= endDate
                             && p.Patient.StudyId == studyId)
                    .GroupBy(p => p.Hospital.HospitalName)
                    .Select(g => new
                    {
                        HospitalName = g.Key,
                        EligiblePathCount = g.Count()
                    })
                    .AsNoTracking()
                    .ToListAsync();

                // Enrolled counts by hospital
                var enrolled = await _context.PatientStatuses
                    .Where(s => s.Date >= startDate
                             && s.Date <= endDate
                             && s.Patient.StudyId == studyId
                             && s.Status == "Participating/potential")
                    .Select(s => new
                    {
                        HospitalName = s.Patient.PathReports
                            .OrderByDescending(pr => pr.DateOfProcedure)
                            .Select(pr => pr.SubmittingHospital)
                            .FirstOrDefault()
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var enrolledCounts = enrolled
                    .Where(x => !string.IsNullOrEmpty(x.HospitalName))
                    .GroupBy(x => x.HospitalName!)
                    .Select(g => new
                    {
                        HospitalName = g.Key,
                        Count = g.Count()
                    })
                    .ToList();

                // Merge results
                foreach (var item in submissions)
                {
                    var eligibleMatch = eligible.FirstOrDefault(x => x.HospitalName == item.HospitalName);
                    if (eligibleMatch != null)
                    {
                        item.EligiblePathCount = eligibleMatch.EligiblePathCount;
                    }

                    var enrolledMatch = enrolledCounts.FirstOrDefault(x => x.HospitalName == item.HospitalName);
                    if (enrolledMatch != null)
                    {
                        item.EnrolledPathCount = enrolledMatch.Count;
                    }
                }

                // Add hospitals that appear only in eligible
                foreach (var item in eligible)
                {
                    if (!submissions.Any(s => s.HospitalName == item.HospitalName))
                    {
                        var enrolledMatch = enrolledCounts.FirstOrDefault(x => x.HospitalName == item.HospitalName);

                        submissions.Add(new PathCountByStudyByDate
                        {
                            HospitalName = item.HospitalName,
                            EligiblePathCount = item.EligiblePathCount,
                            SubmittedPathCount = 0,
                            EnrolledPathCount = enrolledMatch?.Count ?? 0
                        });
                    }
                }

                // Add hospitals that appear only in enrolled
                foreach (var item in enrolledCounts)
                {
                    if (!submissions.Any(s => s.HospitalName == item.HospitalName))
                    {
                        submissions.Add(new PathCountByStudyByDate
                        {
                            HospitalName = item.HospitalName,
                            EligiblePathCount = 0,
                            SubmittedPathCount = 0,
                            EnrolledPathCount = item.Count
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

        // CECS Path Reports by Date
        [HttpGet("CECSPathsByDate/{startDateStr}/{endDateStr}")]
        public async Task<ActionResult<IEnumerable<PathCountByStudyByDate>>> GetCECSPathsByDate(string startDateStr, string endDateStr)
        {
            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);

            try
            {
                var query = _context.PathReports
                    .Where(p => p.RcaExportDate >= startDate
                             && p.RcaExportDate <= endDate
                             && p.Patient.Study.Prefix == "CECS")
                    .GroupBy(p => p.Hospital.HospitalName)
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
                int totalCases = await _context.PathReports
                    .Where(p => p.RcaExportDate >= startDate
                             && p.RcaExportDate <= endDate
                             && p.Patient.Study.Prefix == "CECS")
                    .CountAsync();

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
                    .Where(p => p.RcaExportDate >= startDate
                             && p.RcaExportDate <= endDate
                             && p.Patient.Study.Prefix == "CECS")
                    .GroupBy(p => p.Patient.Race)
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
                    .Where(p => p.RcaExportDate >= startDate
                             && p.RcaExportDate <= endDate
                             && p.Patient.Study.Prefix == "CECS")
                    .GroupBy(p => p.Patient.Ethnicity)
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
                 .Where(p => p.RcaExportDate >= startDate
                          && p.RcaExportDate <= endDate
                          && p.Patient.Study.StudyId == studyId)
                 .GroupBy(p => p.Hospital.HospitalName)
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
