using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Data;
using RCA_StudyManagementSystem.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace RCA_StudyManagementSystem.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class PathReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private List<dynamic> dtoData = new List<dynamic>();

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserContext _userContext;


        public PathReportsController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, UserContext userContext)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _userContext = userContext;
        }

        // GET: api/PathReports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PathReport>> GetPathReport(Guid id)
        {
            var pathReport = await _context.PathReports.FindAsync(id);
            if (pathReport == null)
            {
                return NotFound();
            }
            return pathReport;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PathReportView>>> GetPathReports()
        {
            var result = await _context.PathReports
                .AsNoTracking()
                .TagWithCallSite()
                .Where(pr => !pr.Patient!.Study!.IsArchived)
                .GroupJoin(
                    _context.Hospitals.AsNoTracking(),
                    pr => new { HospitalName = pr.SubmittingHospital, City = pr.HospCity },
                    h => new { HospitalName = h.HospitalName, City = h.City },
                    (pr, hospitals) => new { pr, hospitals }
                )
                .SelectMany(
                    x => x.hospitals.DefaultIfEmpty(),
                    (x, h) => new PathReportView
                    {
                        PathReportId = x.pr.PathReportId,
                        MigratedCCRNumber = x.pr.MigratedCCRNumber,
                        PatientId = x.pr.PatientId,
                        PathIndex = x.pr.PathIndex,
                        CaseNumber = x.pr.CaseNumber,
                        StudyPrefix = x.pr.StudyPrefix,
                        StudyColor = x.pr.StudyColor,
                        SubmittingHospital = x.pr.SubmittingHospital,
                        SubmittingHospitalPathReportNumber = x.pr.SubmittingHospitalPathReportNumber,
                        OriginatingHospitalPathReportNumber = x.pr.OriginatingHospitalPathReportNumber,
                        OriginatingHospitalComments = x.pr.OriginatingHospitalComments,
                        SlidesResideAtSubmittingHospital = x.pr.SlidesResideAtSubmittingHospital,
                        DateOfProcedure = x.pr.DateOfProcedure,
                        AgeAtProcedure = x.pr.AgeAtProcedure,
                        ExportStatus = x.pr.ExportStatus,
                        RcaExportDate = x.pr.RcaExportDate,
                        Reimbursement1 = x.pr.Reimbursement1,
                        Reimbursement2 = x.pr.Reimbursement2,
                        Site = x.pr.Site,
                        SiteCode = x.pr.SiteCode,
                        PathProcedure = x.pr.PathProcedure,
                        PathComments = x.pr.PathComments,
                        IsOnHold = x.pr.IsOnHold,
                        HistologyDiagnosis1 = x.pr.HistologyDiagnosis1,
                        HistologyCode1 = x.pr.HistologyCode1,
                        HistologyBehavior1 = x.pr.HistologyBehavior1,
                        HistologyDiagnosisComments1 = x.pr.HistologyDiagnosisComments1,
                        AuthorizingProvider = x.pr.AuthorizingProvider,
                        AuthorizingProviderComments = x.pr.AuthorizingProviderComments,
                        Site2 = x.pr.Site2,
                        SiteCode2 = x.pr.SiteCode2,
                        PathProcedure2 = x.pr.PathProcedure2,
                        PathComments2 = x.pr.PathComments2,
                        HistologyDiagnosis2 = x.pr.HistologyDiagnosis2,
                        HistologyCode2 = x.pr.HistologyCode2,
                        HistologyBehavior2 = x.pr.HistologyBehavior2,
                        HistologyDiagnosisComments2 = x.pr.HistologyDiagnosisComments2,
                        HospCity = x.pr.HospCity,
                        BatchNumber = x.pr.BatchNumber,

                        LastName = x.pr.Patient!.LastName ?? string.Empty,
                        FirstName = x.pr.Patient.FirstName ?? string.Empty,
                        MiddleName = x.pr.Patient.MiddleName ?? string.Empty,
                        DisplayName = x.pr.Patient.DisplayName ?? string.Empty,
                        DateOfBirth = x.pr.Patient.DateOfBirth.HasValue
                            ? x.pr.Patient.DateOfBirth.Value.ToShortDateString()
                            : string.Empty,
                        SocialSecurityNumber = x.pr.Patient.SocialSecurityNumber,
                        StudyId = x.pr.Patient.StudyId,

                        HospShortName = h != null ? h.HospitalShortName : string.Empty
                    }
                )
                .ToListAsync();

            return result;
        }

        // GET: api/PathReports/batch/{batchNumber}
        [HttpGet("batch/{batchNumber}")]
        public async Task<ActionResult<IEnumerable<PathReportView>>> ListPathReportsByBatch(string batchNumber)
        {
            // List of PathReportViews by batch number
            var pathReports = (from pr in _context.PathReports
                               join pre in _context.PathReportExports on pr.PathReportId equals pre.PathReportId
                               join b in _context.Batches on pre.BatchId equals b.BatchId
                               join p in _context.Patients on pr.PatientId equals p.PatientId
                               where b.BatchNumber == batchNumber
                               select new PathReportView
                               {
                                   PathReportId = pr.PathReportId,
                                   MigratedCCRNumber = pr.MigratedCCRNumber,
                                   PatientId = pr.PatientId,
                                   PathIndex = pr.PathIndex,
                                   CaseNumber = pr.CaseNumber,
                                   StudyPrefix = pr.StudyPrefix,
                                   StudyColor = pr.StudyColor,
                                   SubmittingHospital = pr.SubmittingHospital,
                                   SubmittingHospitalPathReportNumber = pr.SubmittingHospitalPathReportNumber,
                                   OriginatingHospitalPathReportNumber = pr.OriginatingHospitalPathReportNumber,
                                   OriginatingHospitalComments = pr.OriginatingHospitalComments,
                                   SlidesResideAtSubmittingHospital = pr.SlidesResideAtSubmittingHospital,
                                   DateOfProcedure = pr.DateOfProcedure,
                                   AgeAtProcedure = pr.AgeAtProcedure,
                                   ExportStatus = pr.ExportStatus,
                                   RcaExportDate = pr.RcaExportDate,
                                   Reimbursement1 = pr.Reimbursement1,
                                   Reimbursement2 = pr.Reimbursement2,
                                   Site = pr.Site,
                                   SiteCode = pr.SiteCode,
                                   PathProcedure = pr.PathProcedure,
                                   PathComments = pr.PathComments,
                                   IsOnHold = pr.IsOnHold,
                                   HistologyDiagnosis1 = pr.HistologyDiagnosis1,
                                   HistologyCode1 = pr.HistologyCode1,
                                   HistologyBehavior1 = pr.HistologyBehavior1,
                                   HistologyDiagnosisComments1 = pr.HistologyDiagnosisComments1,
                                   AuthorizingProvider = pr.AuthorizingProvider,
                                   AuthorizingProviderComments = pr.AuthorizingProviderComments,
                                   Site2 = pr.Site2,
                                   SiteCode2 = pr.SiteCode2,
                                   PathProcedure2 = pr.PathProcedure2,
                                   PathComments2 = pr.PathComments2,
                                   HistologyDiagnosis2 = pr.HistologyDiagnosis2,
                                   HistologyCode2 = pr.HistologyCode2,
                                   HistologyBehavior2 = pr.HistologyBehavior2,
                                   HistologyDiagnosisComments2 = pr.HistologyDiagnosisComments2,
                                   HospCity = pr.HospCity,
                                   BatchNumber = b.BatchNumber,
                                   // Include patient details
                                   LastName = p.LastName ?? string.Empty,
                                   FirstName = p.FirstName ?? string.Empty,
                                   MiddleName = p.MiddleName ?? string.Empty,
                                   DisplayName = p.DisplayName ?? string.Empty,
                                   DateOfBirth = p.DateOfBirth.HasValue == true ? p.DateOfBirth.Value.ToShortDateString() : string.Empty,
                                   SocialSecurityNumber = p.SocialSecurityNumber,
                                   StudyId = p.StudyId,
                               });

            return pathReports.ToList();
        }

        // GET: api/PathReports/checkpathreportnumber/{pathNo}
        [HttpGet("checkpathreportnumber/{pathNo}")]
        public async Task<ActionResult<string>> CheckPathReportNumber(string pathNo)
        {
            var query = _context.PathReports
                .Where(p => p.SubmittingHospitalPathReportNumber == pathNo);

            if (query.Count() < 1)
                return "false";
            else
                return "true";
        }

        // POST: api/PathReports
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{userId}")]
        public async Task<ActionResult<PathReport>> CreatePathReport(string userId, PathReport pathReport)
        {
            _userContext.UserId = userId;
            _context.PathReports.Add(pathReport);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPathReport", new { id = pathReport.PathReportId }, pathReport);
        }

        [HttpPut("{id}/{userId}")]
        public async Task<IActionResult> UpdatePathReport(Guid id, string userId, PathReport pathReport)
        {
            _userContext.UserId = userId;

            if (id != pathReport.PathReportId)
            {
                return BadRequest();
            }
            _context.Entry(pathReport).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PathReportExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        [HttpPut("exportstatus/{id}")]
        public async Task<IActionResult> UpdatePathReportExportStatus(Guid id, PathReport pathReport)
        {
            var entry = _context.PathReports.Attach(pathReport);

            entry.Property(x => x.ExportStatus).IsModified = true;
            entry.Property(x => x.RcaExportDate).IsModified = true;
            entry.Property(x => x.BatchNumber).IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PathReportExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        private bool PathReportExists(Guid id)
        {
            return _context.PathReports.Any(e => e.PathReportId == id);
        }

        [HttpGet("limit/{limit}")]
        public async Task<ActionResult<IEnumerable<PathReportView>>> GetPathReports(string limit)
        {
            var cutoff = limit switch
            {
                "Week" => DateTime.Now.AddDays(-7),
                "ThreeMonths" => DateTime.Now.AddMonths(-3),
                "Year" => DateTime.Now.AddYears(-1),
                _ => (DateTime?)null
            };

            var query = _context.PathReports
                .AsNoTracking()
                .TagWithCallSite()
                .Where(pr => !pr.Patient!.Study!.IsArchived);

            if (cutoff.HasValue)
            {
                query = query.Where(pr => pr.CreatedDate >= cutoff.Value);
            }

            var result = await query
                .GroupJoin(
                    _context.Hospitals.AsNoTracking(),
                    pr => new { HospitalName = pr.SubmittingHospital, City = pr.HospCity },
                    h => new { HospitalName = h.HospitalName, City = h.City },
                    (pr, hospitals) => new { pr, hospitals }
                )
                .SelectMany(
                    x => x.hospitals.DefaultIfEmpty(),
                    (x, h) => new PathReportView
                    {
                        PathReportId = x.pr.PathReportId,
                        MigratedCCRNumber = x.pr.MigratedCCRNumber,
                        PatientId = x.pr.PatientId,
                        PathIndex = x.pr.PathIndex,
                        CaseNumber = x.pr.CaseNumber,
                        StudyPrefix = x.pr.StudyPrefix,
                        StudyColor = x.pr.StudyColor,
                        SubmittingHospital = x.pr.SubmittingHospital,
                        SubmittingHospitalPathReportNumber = x.pr.SubmittingHospitalPathReportNumber,
                        OriginatingHospitalPathReportNumber = x.pr.OriginatingHospitalPathReportNumber,
                        OriginatingHospitalComments = x.pr.OriginatingHospitalComments,
                        SlidesResideAtSubmittingHospital = x.pr.SlidesResideAtSubmittingHospital,
                        DateOfProcedure = x.pr.DateOfProcedure,
                        AgeAtProcedure = x.pr.AgeAtProcedure,
                        ExportStatus = x.pr.ExportStatus,
                        RcaExportDate = x.pr.RcaExportDate,
                        Reimbursement1 = x.pr.Reimbursement1,
                        Reimbursement2 = x.pr.Reimbursement2,
                        Site = x.pr.Site,
                        SiteCode = x.pr.SiteCode,
                        PathProcedure = x.pr.PathProcedure,
                        PathComments = x.pr.PathComments,
                        IsOnHold = x.pr.IsOnHold,
                        HistologyDiagnosis1 = x.pr.HistologyDiagnosis1,
                        HistologyCode1 = x.pr.HistologyCode1,
                        HistologyBehavior1 = x.pr.HistologyBehavior1,
                        HistologyDiagnosisComments1 = x.pr.HistologyDiagnosisComments1,
                        AuthorizingProvider = x.pr.AuthorizingProvider,
                        AuthorizingProviderComments = x.pr.AuthorizingProviderComments,
                        Site2 = x.pr.Site2,
                        SiteCode2 = x.pr.SiteCode2,
                        PathProcedure2 = x.pr.PathProcedure2,
                        PathComments2 = x.pr.PathComments2,
                        HistologyDiagnosis2 = x.pr.HistologyDiagnosis2,
                        HistologyCode2 = x.pr.HistologyCode2,
                        HistologyBehavior2 = x.pr.HistologyBehavior2,
                        HistologyDiagnosisComments2 = x.pr.HistologyDiagnosisComments2,
                        HospCity = x.pr.HospCity,
                        BatchNumber = x.pr.BatchNumber,

                        LastName = x.pr.Patient!.LastName ?? string.Empty,
                        FirstName = x.pr.Patient.FirstName ?? string.Empty,
                        MiddleName = x.pr.Patient.MiddleName ?? string.Empty,
                        DisplayName = x.pr.Patient.DisplayName ?? string.Empty,
                        DateOfBirth = x.pr.Patient.DateOfBirth.HasValue
                            ? x.pr.Patient.DateOfBirth.Value.ToShortDateString()
                            : string.Empty,
                        SocialSecurityNumber = x.pr.Patient.SocialSecurityNumber,
                        StudyId = x.pr.Patient.StudyId,
                        HospShortName = h != null ? h.HospitalShortName : string.Empty
                    }
                )
                .ToListAsync();

            return result;
        }

        [HttpGet("archived")]
        public async Task<ActionResult<IEnumerable<PathReportView>>> GetArchivedPathReports()
        {
            var pathReports = await _context.PathReports
               .Include(x => x.Patient)
               .Include(s => s.Patient!.Study)
                .Where(s => s.Patient!.Study!.IsArchived == true)
                .AsNoTracking()
                .TagWithCallSite()
                .AsSplitQuery()
               .ToListAsync();

            var result = new List<PathReportView>();
            foreach (var x in pathReports)
            {
                result.Add(new PathReportView
                {
                    PathReportId = x.PathReportId,
                    MigratedCCRNumber = x.MigratedCCRNumber,
                    PatientId = x.PatientId,
                    PathIndex = x.PathIndex,
                    CaseNumber = x.CaseNumber,
                    StudyPrefix = x.StudyPrefix,
                    StudyColor = x.StudyColor,
                    SubmittingHospital = x.SubmittingHospital,
                    SubmittingHospitalPathReportNumber = x.SubmittingHospitalPathReportNumber,
                    OriginatingHospitalPathReportNumber = x.OriginatingHospitalPathReportNumber,
                    OriginatingHospitalComments = x.OriginatingHospitalComments,
                    SlidesResideAtSubmittingHospital = x.SlidesResideAtSubmittingHospital,
                    DateOfProcedure = x.DateOfProcedure,
                    AgeAtProcedure = x.AgeAtProcedure,
                    ExportStatus = x.ExportStatus,
                    RcaExportDate = x.RcaExportDate,
                    Reimbursement1 = x.Reimbursement1,
                    Reimbursement2 = x.Reimbursement2,
                    Site = x.Site,
                    SiteCode = x.SiteCode,
                    PathProcedure = x.PathProcedure,
                    PathComments = x.PathComments,
                    IsOnHold = x.IsOnHold,
                    HistologyDiagnosis1 = x.HistologyDiagnosis1,
                    HistologyCode1 = x.HistologyCode1,
                    HistologyBehavior1 = x.HistologyBehavior1,
                    HistologyDiagnosisComments1 = x.HistologyDiagnosisComments1,
                    AuthorizingProvider = x.AuthorizingProvider,
                    AuthorizingProviderComments = x.AuthorizingProviderComments,
                    Site2 = x.Site2,
                    SiteCode2 = x.SiteCode2,
                    PathProcedure2 = x.PathProcedure2,
                    PathComments2 = x.PathComments2,
                    HistologyDiagnosis2 = x.HistologyDiagnosis2,
                    HistologyCode2 = x.HistologyCode2,
                    HistologyBehavior2 = x.HistologyBehavior2,
                    HistologyDiagnosisComments2 = x.HistologyDiagnosisComments2,
                    HospCity = x.HospCity,
                    BatchNumber = x.BatchNumber,
                    // Include patient details
                    LastName = x.Patient?.LastName ?? string.Empty,
                    FirstName = x.Patient?.FirstName ?? string.Empty,
                    MiddleName = x.Patient?.MiddleName ?? string.Empty,
                    DisplayName = x.Patient?.DisplayName ?? string.Empty,
                    DateOfBirth = x.Patient?.DateOfBirth.HasValue == true ? x.Patient.DateOfBirth.Value.ToShortDateString() : string.Empty,
                    SocialSecurityNumber = x.Patient?.SocialSecurityNumber,
                    StudyId = x.Patient?.StudyId ?? Guid.Empty,
                });
            }

            // Get the current request's details
            var request = _httpContextAccessor.HttpContext.Request;

            // Construct the base URL dynamically
            var baseUrl = $"{request.Scheme}://{request.Host}";

            // Create an HttpClient instance
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(baseUrl);

            // Call api directly to get hospital short names
            foreach (var item in result)
            {

                item.HospShortName = await httpClient.GetStringAsync($"api/hospitals/shortname/{item.SubmittingHospital}/{item.HospCity}");
            }

            return result;
        }

        [HttpGet("study/{studyId}")]
        public async Task<ActionResult<IEnumerable<PathReportView>>> GetPathReportsByStudy(Guid studyId)
        {
            // Get the current request's details
            var request = _httpContextAccessor.HttpContext.Request;

            // Construct the base URL dynamically
            var baseUrl = $"{request.Scheme}://{request.Host}";

            // Create an HttpClient instance
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(baseUrl);

            // Call api directly to get pathreports

            var pathReports = await httpClient.GetFromJsonAsync<IEnumerable<PathReportView>>($"api/pathreports");

            var pathReportByStudy = pathReports.Where(pr => pr.StudyId == studyId).ToList();

            return pathReportByStudy;
        }

        [HttpGet("study/export/{studyId}")]
        public async Task<ActionResult<IEnumerable<PathReportView>>> GetPathReportsByStudyForExport(Guid studyId)
        {
            // Get the current request's details
            var request = _httpContextAccessor.HttpContext.Request;

            // Construct the base URL dynamically
            var baseUrl = $"{request.Scheme}://{request.Host}";

            // Create an HttpClient instance
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(baseUrl);

            // Call api directly to get pathreports

            var pathReports = await httpClient.GetFromJsonAsync<IEnumerable<PathReportView>>($"api/pathreports");

            var pathReportByStudyForExport = pathReports.Where(pr => pr.StudyId == studyId && pr.ExportStatus == "Ready" && !pr.IsOnHold).ToList();

            return pathReportByStudyForExport;
        }

        [HttpGet("study/exported/{studyId}")]
        public async Task<ActionResult<IEnumerable<PathReportView>>> GetPathReportsByStudyExported(Guid studyId)
        {
            // Get the current request's details
            var request = _httpContextAccessor.HttpContext.Request;

            // Construct the base URL dynamically
            var baseUrl = $"{request.Scheme}://{request.Host}";

            // Create an HttpClient instance
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(baseUrl);

            // Call api directly to get pathreports

            var pathReports = await httpClient.GetFromJsonAsync<IEnumerable<PathReportView>>($"api/pathreports");

            var pathReportByStudyExported = pathReports.Where(pr => pr.StudyId == studyId && pr.ExportStatus == "Exported").ToList();

            return pathReportByStudyExported;
        }

        // GET: api/PathReports/exporthistory/{pathReportId}
        [HttpGet("exporthistory/{pathReportId}")]
        public async Task<ActionResult<IEnumerable<ExportView>>> GetPathReportExportHistory(Guid pathReportId)
        {
            var pathReportExports = await (from pre in _context.PathReportExports
                                           join pr in _context.PathReports on pre.PathReportId equals pr.PathReportId
                                           join b in _context.Batches on pre.BatchId equals b.BatchId
                                           join p in _context.Patients on pr.PatientId equals p.PatientId
                                           where pre.PathReportId == pathReportId
                                           select new ExportView
                                           {
                                               BatchId = pre.BatchId,
                                               BatchNumber = b.BatchNumber,
                                               CreatedDate = b.CreatedDate,
                                               PathId = p.CaseNumber + "-" + pr.PathIndex,
                                           }).ToListAsync();
            return pathReportExports;
        }

        // GET: api/PathReports/HeaderOptions
        [HttpGet("headeroptions")]
        public IEnumerable<StudyHeader> GetPathReportHeaderOptions()
        {
            var patientProperties = typeof(Patient).GetProperties();
            var patientPhoneProperties = typeof(PatientPhoneNumber).GetProperties();
            var pathReportProperties = typeof(PathReport).GetProperties();
            var allProperties = patientProperties
                .Concat(patientPhoneProperties)
                .Concat(pathReportProperties)
                .Select(p => new StudyHeader
                {
                    HeaderName = p.Name,
                    Order = 0, // Default order, can be adjusted later
                    IsActive = true,
                })
                .Distinct()
                .AsEnumerable()
                .ToList();

            var tableName = "Patient"; // Default table name
            var index = 0;
            foreach (var prop in allProperties)
            {
                prop.Order = index++;

                // Mark collection properties as inactive by default
                var propertyInfo = patientProperties.Concat(patientPhoneProperties).Concat(pathReportProperties).FirstOrDefault(p => p.Name == prop.HeaderName);
                if (propertyInfo != null && (propertyInfo.PropertyType.IsGenericType &&
                    (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
                     propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))))
                {
                    prop.IsActive = false;
                }

                if (prop.HeaderName == "CreatedUserId" || prop.HeaderName == "CreatedDate" ||
                    prop.HeaderName == "ModifiedUserId" || prop.HeaderName == "ModifiedDate")
                {
                    prop.IsActive = false; // Exclude tracking properties from being active
                }

                if (prop.HeaderName == "Patient" || prop.HeaderName == "IsShownSite2" || prop.HeaderName == "PathId" ||
                    prop.HeaderName == "PatientId" || prop.HeaderName == "PatientPhoneNumberId" || prop.HeaderName == "PathReportId" ||
                    prop.HeaderName == "StudyId" || prop.HeaderName == "SocialSecurityNumber" || prop.HeaderName == "DisplayName" ||
                    prop.HeaderName == "HospitalId" || prop.HeaderName == "OrigHospitalId" || prop.HeaderName == "DoctorId" || prop.HeaderName == "Doctor2Id" ||
                    prop.HeaderName == "PathologistId" || prop.HeaderName == "Pathologist2Id" ||
                    prop.HeaderName.Contains("Cleared") || prop.HeaderName.Contains("Entity"))
                {
                    prop.IsActive = false; // Exclude other properties from being active
                }

                // Determine the table name based on the property source
                if (patientProperties.Any(p => p.Name == prop.HeaderName))
                {
                    tableName = "Patient";
                }
                else if (patientPhoneProperties.Any(p => p.Name == prop.HeaderName))
                {
                    tableName = "PatientPhoneNumber";
                }
                else if (pathReportProperties.Any(p => p.Name == prop.HeaderName))
                {
                    tableName = "PathReport";
                }

                prop.TableName = tableName;
            }

            return allProperties;
        }


        // GET: api/PathReports/export/{studyId}/{exportType}/{batchId}/{pathIds}
        [HttpGet("export/{studyId}/{exportType}/{batchId}/{pathIds}/{isReport}")]
        public async Task<string> ExportPathReportsToCsv(Guid studyId, string? exportType, Guid? batchId, string? pathIds, bool isReport)
        {
            var guids = new List<Guid>();
            if (pathIds != "Not applicable")
            {
                guids = pathIds.Split(',')
                            .Select(s => Guid.TryParse(s, out var guid) ? guid : (Guid?)null)
                            .Where(g => g.HasValue)
                            .Select(g => g.Value)
                            .ToList();
            }

            var flattenedData = new List<List<dynamic>>();
            int maxPhoneNumbers = 0;
            if (exportType == "Current")
            {
                maxPhoneNumbers = _context.PathReports
                    .Include(ph => ph.Patient.PatientPhoneNumbers)
                    .Where(p => p.Patient.StudyId == studyId &&
                                 p.ExportStatus == "Ready" &&
                                    p.IsOnHold != true)
                     .Max(ph => ph.Patient.PatientPhoneNumbers.Count);

            }
            if (exportType == "Past")
            {
                maxPhoneNumbers = _context.PathReports
                    .Include(ph => ph.Patient.PatientPhoneNumbers)
                    .Where(p => p.Patient.StudyId == studyId &&
                                 p.ExportStatus == "Exported" &&
                                    p.IsOnHold != true)
                     .Max(ph => ph.Patient.PatientPhoneNumbers.Count);
            }
            if (exportType == "Selected")
            {
                maxPhoneNumbers = _context.PathReports
                    .Include(ph => ph.Patient.PatientPhoneNumbers)
                    .Where(p => p.Patient.StudyId == studyId &&
                                 guids.Contains(p.PathReportId) &&
                                    p.IsOnHold != true)
                     .Max(ph => ph.Patient.PatientPhoneNumbers.Count);
            }

            var StudyName = _context.Studies
                .Where(s => s.StudyId == studyId)
                .Select(s => s.Name)
                .FirstOrDefault();

            var headers = new List<StudyHeader>();
            var reportHeaders = new List<StudyReportHeader>();

            if (isReport)
            {
                reportHeaders = _context.StudyReportHeaders
                            .Where(sh => sh.StudyId == studyId && sh.IsActive)
                            .OrderBy(sh => sh.Order)
                            .ToList();

                var headerIndex = reportHeaders.FindIndex(h => h.HeaderName == "PhoneNumber");

                reportHeaders.RemoveRange(headerIndex, 4);

                // Insert placeholders for flattened phone number columns

                for (int i = 0; i < maxPhoneNumbers; i++)
                {
                    int phoneIndex = i + 1;
                    var phoneNumberHeader = new StudyReportHeader
                    {
                        HeaderName = $"PhoneNumber{phoneIndex}",
                        ExportTitle = $"PhoneNumber{phoneIndex}",
                        TableName = "PatientPhoneNumber",
                        Order = headerIndex + 0,
                        IsActive = true,
                        StudyId = studyId
                    };
                    reportHeaders.Insert(headerIndex + 0, phoneNumberHeader);

                    var phoneTypeHeader = new StudyReportHeader
                    {
                        HeaderName = $"PhoneNumberType{phoneIndex}",
                        ExportTitle = $"PhoneNumberType{phoneIndex}",
                        TableName = "PatientPhoneNumber",
                        Order = headerIndex + 1,
                        IsActive = true,
                        StudyId = studyId
                    };
                    reportHeaders.Insert(headerIndex + 1, phoneTypeHeader);

                    var phoneCommentsHeader = new StudyReportHeader
                    {
                        HeaderName = $"PhoneNumberComments{phoneIndex}",
                        ExportTitle = $"PhoneNumberComments{phoneIndex}",
                        TableName = "PatientPhoneNumber",
                        Order = headerIndex + 2,
                        IsActive = true,
                        StudyId = studyId
                    };
                    reportHeaders.Insert(headerIndex + 2, phoneCommentsHeader);

                    var isPrimaryHeader = new StudyReportHeader
                    {
                        HeaderName = $"IsPrimaryPhone{phoneIndex}",
                        ExportTitle = $"IsPrimaryPhone{phoneIndex}",
                        TableName = "PatientPhoneNumber",
                        Order = headerIndex + 3,
                        IsActive = true,
                        StudyId = studyId
                    };
                    reportHeaders.Insert(headerIndex + 3, isPrimaryHeader);
                    headerIndex += 4;
                }
            }
            else
            {
                headers = _context.StudyHeaders
                .Where(sh => sh.StudyId == studyId && sh.IsActive)
                .OrderBy(sh => sh.Order)
                .ToList();

                var headerIndex = headers.FindIndex(h => h.HeaderName == "PhoneNumber");

                headers.RemoveRange(headerIndex, 4);

                // Insert placeholders for flattened phone number columns

                for (int i = 0; i < maxPhoneNumbers; i++)
                {
                    int phoneIndex = i + 1;
                    var phoneNumberHeader = new StudyHeader
                    {
                        HeaderName = $"PhoneNumber{phoneIndex}",
                        ExportTitle = $"PhoneNumber{phoneIndex}",
                        TableName = "PatientPhoneNumber",
                        Order = headerIndex + 0,
                        IsActive = true,
                        StudyId = studyId
                    };
                    headers.Insert(headerIndex + 0, phoneNumberHeader);

                    var phoneTypeHeader = new StudyHeader
                    {
                        HeaderName = $"PhoneNumberType{phoneIndex}",
                        ExportTitle = $"PhoneNumberType{phoneIndex}",
                        TableName = "PatientPhoneNumber",
                        Order = headerIndex + 1,
                        IsActive = true,
                        StudyId = studyId
                    };
                    headers.Insert(headerIndex + 1, phoneTypeHeader);

                    var phoneCommentsHeader = new StudyHeader
                    {
                        HeaderName = $"PhoneNumberComments{phoneIndex}",
                        ExportTitle = $"PhoneNumberComments{phoneIndex}",
                        TableName = "PatientPhoneNumber",
                        Order = headerIndex + 2,
                        IsActive = true,
                        StudyId = studyId
                    };
                    headers.Insert(headerIndex + 2, phoneCommentsHeader);

                    var isPrimaryHeader = new StudyHeader
                    {
                        HeaderName = $"IsPrimaryPhone{phoneIndex}",
                        ExportTitle = $"IsPrimaryPhone{phoneIndex}",
                        TableName = "PatientPhoneNumber",
                        Order = headerIndex + 3,
                        IsActive = true,
                        StudyId = studyId
                    };
                    headers.Insert(headerIndex + 3, isPrimaryHeader);
                    headerIndex += 4;
                }
            }



            await AddPropertiesToDtoAsync(dtoData, headers, reportHeaders, studyId, maxPhoneNumbers, exportType, batchId, guids);
            flattenedData.Add(dtoData);

            var records = GenerateCsv(flattenedData, headers, reportHeaders);

            return records;
        }

        private string GenerateCsv(List<List<dynamic>> data, List<StudyHeader> headers, List<StudyReportHeader> reportHeaders)
        {
            if (data == null || !data.Any())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var uniqueHeaders = new HashSet<string>();
            //sb.Append($"PatientId, ");


            if (headers.Count > 0)
            {
                foreach (var item in headers)
                {
                    uniqueHeaders.Add(item.ExportTitle.Length > 0 ? item.ExportTitle : item.HeaderName);
                }
            }
            else if (reportHeaders.Count > 0)
            {
                foreach (var item in reportHeaders)
                {
                    uniqueHeaders.Add(item.ExportTitle.Length > 0 ? item.ExportTitle : item.HeaderName);
                }
            }


            // Write headers
            sb.AppendLine(string.Join(",", uniqueHeaders.Select(h => $"\"{h.Replace("\"", "\"\"")}\"")));



            // Write data rows
            foreach (var itemRow in data)
            {
                var rb = new StringBuilder();

                foreach (var item in itemRow)
                {

                    foreach (var result in item)
                    {
                        if (result is Guid)
                        {
                            continue;
                        }
                        if (result is DateTime)
                        {
                            var dateValue = (DateTime)result;
                            rb.Append(dateValue.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append(",");
                            continue;
                        }
                        if (result.ToString().Contains(",") || result.ToString().Contains("\"") || result.ToString().Contains("\n") || result.ToString().Contains("\r"))
                        {
                            var stringValue = result.ToString() ?? "";
                            var escapedValue = $"\"{stringValue.Replace("\"", "\"\"")}\"";
                            rb.Append(escapedValue).Append(",");
                            continue;
                        }
                        else
                        {
                            rb.Append(result?.ToString() ?? "").Append(",");
                        }
                    }
                    sb.AppendLine(rb.ToString());
                    rb.Clear();
                }
            }
            return sb.ToString();
        }

        private async Task AddPropertiesToDtoAsync(
            List<dynamic> dtoList,
            IEnumerable<StudyHeader>? headers,
            IEnumerable<StudyReportHeader>? reportHeaders,
            Guid studyId,
            int maxPhoneNumbers,
            string? exportType,
            Guid? batchId,
            List<Guid>? pathIds)
        {
            // Normalize header source once
            var headerItems = (headers?.Any() == true)
                ? headers.Select(h => new HeaderMeta(h.TableName, h.HeaderName, h.ExportTitle)).ToList()
                : (reportHeaders?.Any() == true)
                    ? reportHeaders.Select(h => new HeaderMeta(h.TableName, h.HeaderName, h.ExportTitle)).ToList()
                    : new List<HeaderMeta>();

            // Build SELECT segments (before PhoneNumber1 and after IsPrimaryPhone{maxPhoneNumbers})
            var prePhoneHeaders = headerItems
                .TakeWhile(h => !string.Equals(h.HeaderName, "PhoneNumber1", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var phoneNumberEnd = $"IsPrimaryPhone{maxPhoneNumbers}";
            var postPhoneHeaders = headerItems
                .SkipWhile(h => !string.Equals(h.HeaderName, phoneNumberEnd, StringComparison.OrdinalIgnoreCase))
                .Skip(1) // start AFTER IsPrimaryPhone{N}
                .ToList();

            // Query #1: pre-phone columns + PatientId
            var preSql = BuildSelectSql(prePhoneHeaders, exportType, includePhoneJson: false, pathIds: pathIds);
            var preRows = await ExecuteDynamicRowsAsync(preSql.Sql, preSql.Parameters, studyId, batchId, pathIds);

            // Initialize dto rows from pre-query result
            foreach (var result in preRows)
            {
                var row = new List<dynamic>();
                foreach (var kvp in result)
                    row.Add(kvp.Value);
                dtoList.Add(row);
            }

            // Add flattened phone columns (EF query scoped by same export type)
            var phoneLookup = await GetPhoneLookupAsync(studyId, exportType, batchId, pathIds);

            foreach (var dto in dtoList)
            {
                var patientId = dto[0] is Guid g ? g : Guid.Parse(dto[0].ToString()!);

                phoneLookup.TryGetValue(patientId, out List<PatientPhoneNumber>? phones);
                phones ??= new List<PatientPhoneNumber>();

                for (int i = 0; i < maxPhoneNumbers; i++)
                {
                    var phone = (i < phones.Count) ? phones[i] : null;
                    dto.Add(phone?.PhoneNumber ?? string.Empty);
                    dto.Add(phone?.PhoneType ?? string.Empty);
                    dto.Add(phone?.PhoneNumberComments ?? string.Empty);
                    dto.Add(phone != null && phone.IsPrimary ? "Yes" : "No");
                }
            }

            // Query #2: post-phone columns + PatientId
            var postSql = BuildSelectSql(postPhoneHeaders, exportType, includePhoneJson: false, pathIds: pathIds);
            var postRows = await ExecuteDynamicRowsAsync(postSql.Sql, postSql.Parameters, studyId, batchId, pathIds);

            // Append post-query fields to existing dto rows (skip PatientId because it's already first column in dto)
            for (int i = 0; i < postRows.Count && i < dtoList.Count; i++)
            {
                bool isFirst = true;
                foreach (var kvp in postRows[i])
                {
                    if (isFirst) { isFirst = false; continue; } // skip PatientId
                    dtoList[i].Add(kvp.Value);
                }
            }
        }

        private sealed record HeaderMeta(string TableName, string HeaderName, string? ExportTitle);

        private sealed class SqlBuildResult
        {
            public string Sql { get; init; } = string.Empty;
            public List<SqlParameter> Parameters { get; init; } = new();
        }

        private SqlBuildResult BuildSelectSql(
            List<HeaderMeta> selectedHeaders,
            string? exportType,
            bool includePhoneJson,
            List<Guid>? pathIds)
        {
            var sb = new StringBuilder("SELECT Patient.PatientId");

            foreach (var h in selectedHeaders)
            {
                string table = h.TableName;
                if (string.Equals(table, "PatientPhoneNumber", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (!string.IsNullOrWhiteSpace(h.ExportTitle))
                    sb.Append($", {h.TableName}.{h.HeaderName} AS [{h.ExportTitle}]");
                else
                    sb.Append($", {h.TableName}.{h.HeaderName}");
            }

            if (includePhoneJson)
            {
                sb.Append(@",
(
    SELECT
        pn.PhoneNumber,
        pn.PhoneType,
        pn.PhoneNumberComments,
        CASE WHEN pn.IsPrimary = 1 THEN 'Yes' ELSE 'No' END AS IsPrimary
    FROM PatientPhoneNumber pn
    WHERE pn.PatientId = Patient.PatientId
    FOR JSON PATH
) AS PhoneNumbersJson");
            }

            sb.Append(" FROM Patient ");
            sb.Append("INNER JOIN PathReport ON Patient.PatientId = PathReport.PatientId ");

            var parameters = new List<SqlParameter>();

            switch (exportType)
            {
                case "Current":
                    sb.Append("WHERE PathReport.ExportStatus = @exportStatus ");
                    sb.Append("AND ISNULL(PathReport.IsOnHold, 0) <> 1 ");
                    sb.Append("AND Patient.StudyId = @studyId ");
                    parameters.Add(new SqlParameter("@exportStatus", SqlDbType.NVarChar, 50) { Value = "Ready" });
                    parameters.Add(new SqlParameter("@studyId", SqlDbType.UniqueIdentifier));
                    break;

                case "Past":
                    sb.Append("INNER JOIN PathReportExport ON PathReportExport.PathReportId = PathReport.PathReportId ");
                    sb.Append("WHERE PathReportExport.BatchId = @batchId ");
                    parameters.Add(new SqlParameter("@batchId", SqlDbType.UniqueIdentifier));
                    break;

                case "Selected":
                    if (pathIds == null || pathIds.Count == 0)
                    {
                        // Force empty result safely
                        sb.Append("WHERE 1 = 0 ");
                    }
                    else
                    {
                        var inParams = new List<string>();
                        for (int i = 0; i < pathIds.Count; i++)
                        {
                            var pName = $"@pathId{i}";
                            inParams.Add(pName);
                            parameters.Add(new SqlParameter(pName, SqlDbType.UniqueIdentifier));
                        }

                        sb.Append($"WHERE PathReport.PathReportId IN ({string.Join(", ", inParams)}) ");
                    }
                    break;

                default:
                    // unknown export type => empty result
                    sb.Append("WHERE 1 = 0 ");
                    break;
            }

            return new SqlBuildResult { Sql = sb.ToString(), Parameters = parameters };
        }

        private async Task<List<IDictionary<string, object>>> ExecuteDynamicRowsAsync(
            string sql,
            List<SqlParameter> parametersTemplate,
            Guid studyId,
            Guid? batchId,
            List<Guid>? pathIds)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection is not SqlConnection)
                throw new NotSupportedException("This method is only implemented for SqlConnection.");

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            // Clone/add parameter values
            foreach (var p in parametersTemplate)
            {
                var cp = new SqlParameter(p.ParameterName, p.SqlDbType, p.Size)
                {
                    Value = p.ParameterName switch
                    {
                        "@studyId" => studyId,
                        "@batchId" => (object?)batchId ?? DBNull.Value,
                        _ when p.ParameterName.StartsWith("@pathId", StringComparison.Ordinal) =>
                            pathIds![int.Parse(p.ParameterName.Replace("@pathId", ""))],
                        _ => p.Value ?? DBNull.Value
                    }
                };
                command.Parameters.Add(cp);
            }

            var results = new List<IDictionary<string, object>>();
            await connection.OpenAsync();

            try
            {
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    IDictionary<string, object> row = new ExpandoObject();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? string.Empty : reader.GetValue(i);
                    }
                    results.Add(row);
                }
            }
            finally
            {
                await connection.CloseAsync();
            }

            return results;
        }

        private async Task<Dictionary<Guid, List<PatientPhoneNumber>>> GetPhoneLookupAsync(
            Guid studyId,
            string? exportType,
            Guid? batchId,
            List<Guid>? pathIds)
        {
            IQueryable<PathReport> reportQuery = _context.PathReports.AsNoTracking();

            reportQuery = exportType switch
            {
                "Current" => reportQuery.Where(r => r.ExportStatus == "Ready" && !r.IsOnHold),
                "Past" => reportQuery.Where(r => r.PathReportExports.Any(e => e.BatchId == batchId)),
                "Selected" => (pathIds != null && pathIds.Count > 0)
                    ? reportQuery.Where(r => pathIds.Contains(r.PathReportId))
                    : reportQuery.Where(_ => false),
                _ => reportQuery.Where(_ => false)
            };

            var patientIds = await reportQuery
                .Where(r => r.Patient.StudyId == studyId)
                .Select(r => r.PatientId)
                .Distinct()
                .ToListAsync();

            var phones = await _context.PatientPhoneNumbers
                .AsNoTracking()
                .Where(p => patientIds.Contains(p.PatientId))
                .OrderByDescending(p => p.IsPrimary)
                .ThenBy(p => p.PatientPhoneNumberId) // replace with your stable ordering column
                .ToListAsync();

            return phones
                .GroupBy(p => p.PatientId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        // DELETE: api/PathReports/deleteplaceholders
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlaceholderPathReports()
        {

            var pathreports = await _context.PathReports
                .Where(p => p.Site == "Placeholder" && p.PathProcedure == "Placeholder" && p.AuthorizingProvider == "Placeholder")
                .ToListAsync();

            if (pathreports == null)
            {
                return NotFound();
            }

            foreach (var pathreport in pathreports)
            {
                _context.PathReports.Remove(pathreport);
            }

            await _context.SaveChangesAsync();


            return NoContent();
        }

    }
}


