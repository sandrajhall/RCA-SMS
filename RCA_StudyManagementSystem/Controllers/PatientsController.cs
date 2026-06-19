using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Data;
using RCA_StudyManagementSystem.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RCA_StudyManagementSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PatientsController> _logger;
        private readonly IOutputCacheStore _cacheStore;
        private readonly UserContext _userContext;


        public PatientsController(ApplicationDbContext context, ILogger<PatientsController> logger, IOutputCacheStore cacheStore, UserContext userContext)
        {
            _context = context;
            _logger = logger;
            _cacheStore = cacheStore;
            _userContext = userContext;
        }

        // GET: api/Patients
        [OutputCache(PolicyName = "PatientTagPolicy")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patient>>> ListPatients()
        {
            _logger.LogInformation("Patient list requested.");

            // Fetch all active patients with their related PathReports and PatientPhoneNumbers
            var query = _context.Patients
            .Include(pr => pr.PathReports)
            .Include(p => p.PatientPhoneNumbers)
            .Where(s => s.IsActive) // Filter to only include active patients
            .AsQueryable();

            _logger.LogInformation("Returning {Count} Patients.", query.Count());

            return await query.ToListAsync();
        }


        // GET: api/Patients/lastcasenumber/{prefix}
        [OutputCache(PolicyName = "PatientTagPolicy")]
        [HttpGet("lastcasenumber/{prefix}")]
        public async Task<ActionResult<string>> GetLastCaseNumber(string prefix)
        {
            _logger.LogInformation("Patient last case id requested.");

            var query = _context.Patients?
                .Where(s => s.CaseNumber.StartsWith(prefix))?
                .OrderBy(s => s.CaseNumber)?
                .LastOrDefault()?.CaseNumber;

            _logger.LogInformation("Returning id: {id}.", query);

            return query ?? String.Empty;
        }

        // GET: api/Patients/5
        [OutputCache(PolicyName = "PatientTagPolicy")]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Patient>> GetPatient(Guid id)
        {
            _logger.LogInformation("Patient {id} requested.", id);

            var patient = await _context.Patients
                .Include(b => b.PathReports)
                .Include(b => b.PatientPhoneNumbers)
                .FirstOrDefaultAsync(s => s.PatientId == id);

            if (patient == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Returning Patient with id: {id}.", patient.PatientId);

            return patient;
        }

        // GET: api/Patients/5
        [OutputCache(PolicyName = "PatientTagPolicy")]
        [HttpGet("casenumber/{caseNumber}")]
        public async Task<ActionResult<Patient>> GetPatientByCaseNumber(string caseNumber)
        {
            _logger.LogInformation("Patient {caseNumber} requested.", caseNumber);

            var patient = await _context.Patients
                .Include(b => b.PathReports)
                .Include(b => b.PatientPhoneNumbers)
                .FirstOrDefaultAsync(s => s.CaseNumber == caseNumber);

            if (patient == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Returning Patient with id: {id}.", patient.PatientId);

            return patient;
        }

        // GET: api/Patients/patientidbyccrno/{ccrno}
        [OutputCache(PolicyName = "PatientTagPolicy")]
        [HttpGet("patientidbyccrno/{ccrno}")]
        public async Task<ActionResult<Guid>> GetPatientIdByCcrNo(string ccrno)
        {
            _logger.LogInformation("Patient with CCR No {ccrno} requested.", ccrno);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(s => s.MigratedCCRNo == ccrno);
            if (patient == null)
            {
                return Guid.Empty;
            }

            var patientId = patient.PatientId;

            _logger.LogInformation("Returning Patient id: {id}.", patient.PatientId);

            return patientId;
        }

        // GET: api/Patients/clearccrnos/
        [HttpGet("clearccrnos")]
        public async Task<string> ClearCcrNos()
        {
            var patients = await _context.Patients
                .Where(p => p.MigratedCCRNo != null)
                .ToListAsync();
            foreach (var p in patients)
            {
                p.MigratedCCRNo = null;
            }

            await _context.SaveChangesAsync();


            return "cleared";
        }



        // GET: api/Patients/primaryphone/{id}
        [OutputCache(PolicyName = "PatientTagPolicy")]
        [HttpGet("primaryphone/{id:guid}")]
        public async Task<ActionResult<string>> GetPatientPrimaryPhone(Guid id)
        {
            _logger.LogInformation("Patient primary phone for {id} requested.", id);
            var patientPhone = await _context.PatientPhoneNumbers
                .Where(p => p.PatientId == id && p.IsPrimary == true)
                .FirstOrDefaultAsync();
            if (patientPhone == null)
            {
                return NotFound();
            }
            _logger.LogInformation("Returning Patient primary phone: {phone}.", patientPhone.PhoneNumber);
            return patientPhone.PhoneNumber;
        }

        // GET: api/Patients/patienthistory/5
        [OutputCache(PolicyName = "PatientTagPolicy")]
        [HttpGet("patienthistory/{id:guid}")]
        public async Task<ActionResult<List<Patient>>> GetPatientHistory(Guid id)
        {
            _logger.LogInformation("Patient history for {id} requested.", id);

            var history = await _context.Patients
                .TemporalAll()
                .Where(p => p.PatientId == id)
                .OrderByDescending(p => EF.Property<DateTime>(p, "PeriodStart"))
                .ToListAsync();

            if (history == null)
            {
                return NotFound();
            }
            _logger.LogInformation("Returning Patient history with id: {id}.", id);
            return history;
        }

        // GET: api/Patients/patientphonenumberhistory/5
        [OutputCache(PolicyName = "PatientTagPolicy")]
        [HttpGet("patientphonenumberhistory/{id:guid}")]
        public async Task<ActionResult<List<PatientPhoneNumber>>> GetPatientPhoneNumberHistory(Guid id)
        {
            _logger.LogInformation("PatientPhoneNumber history for {id} requested.", id);

            var history = await _context.PatientPhoneNumbers
                .TemporalAll()
                .Where(p => p.PatientId == id)
                .OrderByDescending(p => EF.Property<DateTime>(p, "PeriodStart"))
                .ToListAsync();

            if (history == null)
            {
                return NotFound();
            }
            _logger.LogInformation("Returning PatientPhoneNumber history with id: {id}.", id);
            return history;
        }

        // GET: api/Patients/pathreporthistory/5
        [OutputCache(PolicyName = "PatientTagPolicy")]
        [HttpGet("pathreporthistory/{id:guid}")]
        public async Task<ActionResult<List<PathReport>>> GetPathReportHistory(Guid id)
        {
            _logger.LogInformation("PathReport history for {id} requested.", id);

            var history = await _context.PathReports
                .TemporalAll()
                .Where(p => p.PatientId == id)
                .OrderByDescending(p => EF.Property<DateTime>(p, "PeriodStart"))
                .ToListAsync();

            if (history == null)
            {
                return NotFound();
            }
            _logger.LogInformation("Returning PathReport history with id: {id}.", id);
            return history;
        }

        // GET: api/Patients/exporthistory/5
        [OutputCache(PolicyName = "PatientTagPolicy")]
        [HttpGet("exporthistory/{id:guid}")]
        public async Task<ActionResult<Patient>> GetPatientExportHistory(Guid id)
        {
            _logger.LogInformation("Patient {id} requested.", id);

            var patient = await _context.Patients
                .Include(b => b.PathReports)
                .ThenInclude(pr => pr.PathReportExports)!
                .ThenInclude(pre => pre.Batch)
                .Include(b => b.PatientPhoneNumbers)
                .FirstOrDefaultAsync(s => s.PatientId == id);

            if (patient == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Returning Patient with id: {id}.", patient.PatientId);

            return patient;
        }

        // GET: api/Patients/ssn/{ssn}
        [OutputCache(PolicyName = "PatientTagPolicy")]
        [HttpGet("ssn/{ssn}")]
        public async Task<bool> CheckSSN(string ssn)
        {
            _logger.LogInformation("Patient {ssn} is being checked.", ssn);

            if (string.IsNullOrWhiteSpace(ssn) || ssn == "999-99-9999")
            {
                return false; 
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(s => s.SocialSecurityNumber == ssn);

            if (patient == null)
            {
                return false;
            }

            _logger.LogInformation("Patient exists with ssn: {ssn}.", ssn);

            return true;
        }

        // GET: api/Patients/dob
        [OutputCache(PolicyName = "PatientTagPolicy")]
        [HttpGet("dob")]
        public async Task<bool> CheckDOB([FromQuery] DateTime? dob)
        {
            _logger.LogInformation("Patient {dob} is being checked.", dob);

            if (dob == null)
            {
                _logger.LogWarning("Received a null date of birth.");
                return false;
            }

            bool patientExists = await _context.Patients
                .AnyAsync(s => s.DateOfBirth.HasValue && s.DateOfBirth.Value.Date == dob.Value.Date);

            if (patientExists)
            {
                _logger.LogInformation("Patient exists with dob: {dob}.", dob.Value.ToShortDateString());
            }
            else
            {
                _logger.LogInformation("No patient found with dob: {dob}.", dob.Value.ToShortDateString());
            }

            return patientExists;
        }


        // GET: api/Patients/name/{name}
        [OutputCache(PolicyName = "PatientTagPolicy")]
        [HttpGet("name/{name}")]
        public async Task<bool> CheckName(string name)
        {
            _logger.LogInformation("Patient {name} is being checked.", name);

            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            if(name.Contains(","))
                {
                var parts = name.Split(',', StringSplitOptions.RemoveEmptyEntries);
                name = $"{parts[0].Trim()}";
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(s => s.DisplayName == name);

            if (patient == null)
            {
                return false;
            }

            _logger.LogInformation("Patient exists with name: {name}.", name);

            return true;
        }

        // POST: api/Patients
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{userId}")]
        public async Task<ActionResult<Patient>> CreatePatient(string userId, Patient patient)
        {
            _logger.LogInformation("Patient creation started...");

            //_context.Entry(patient).State = EntityState.Added;
            //_context.Entry(patient.PatientPhoneNumbers).State = EntityState.Added;
            //var statePath = _context.Entry(patient.PathReports).State;
            _context.ChangeTracker.DetectChanges();



            _context.Patients.Add(patient);

            _userContext.UserId = userId;

            try
            { 
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"Direct Cause: {innerMessage}");
            }

            _logger.LogInformation("Returning Patient with id: {id}.", patient.PatientId);
            await _cacheStore.EvictByTagAsync("patient-api", CancellationToken.None);


            return CreatedAtAction("GetPatient", new { id = patient.PatientId }, patient);
        }

        // PUT: api/Patients/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}/{userId}")]
        public async Task<IActionResult> UpdatePatient(Guid id, Patient patient, string userId)
        {
            _logger.LogInformation("Patient update started...");

            _userContext.UserId = userId;

            var existingEntity = await _context.Patients
                                .Include(r => r.PathReports)
                                .Include(s => s.PatientPhoneNumbers)
                                .AsTracking()
                                .FirstOrDefaultAsync(s => s.PatientId == id);

            if (id != patient.PatientId)
            {
                return BadRequest();
            }


            // Update scalar properties
            _context.Entry(existingEntity).CurrentValues.SetValues(patient);
            

            // Update PatientPhoneNumbers (child collection)
            var incomingPhoneIds = patient.PatientPhoneNumbers.Select(c => c.PatientPhoneNumberId).ToList();
            foreach (var phone in existingEntity.PatientPhoneNumbers.ToList())
            {
                if (!incomingPhoneIds.Contains(phone.PatientPhoneNumberId))
                    _context.PatientPhoneNumbers.Remove(phone);
            }

            foreach (var incomingPhone in patient.PatientPhoneNumbers)
            {
                // Try to find if this Guid already exists in the database's child collection
                var existingPhone = existingEntity.PatientPhoneNumbers
                    .FirstOrDefault(c => c.PatientPhoneNumberId == incomingPhone.PatientPhoneNumberId);

                if (existingPhone == null)
                {
                    // 1. THIS IS A NEW PHONE NUMBER
                    // Ensure the Foreign Key is set correctly
                    incomingPhone.PatientId = existingEntity.PatientId;

                    // 2. Add it to the collection tracked by the context
                    existingEntity.PatientPhoneNumbers.Add(incomingPhone);

                    // 3. FORCE EF to recognize this as a NEW insert
                    _context.Entry(incomingPhone).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
                else
                {
                    // THIS IS AN UPDATE
                    _context.Entry(existingPhone).CurrentValues.SetValues(incomingPhone);
                }
            }
            // Update PathReports (child collection)
            var incomingPathIds = patient.PathReports.Select(c => c.PathReportId).ToList();
            foreach (var path in existingEntity.PathReports.ToList())
            {
                if (!incomingPathIds.Contains(path.PathReportId))
                    _context.PathReports.Remove(path);
            }

            foreach (var incomingPath in patient.PathReports)
            {
                var existingPath = existingEntity.PathReports
                    .FirstOrDefault(c => c.PathReportId == incomingPath.PathReportId);

                if (existingPath == null)
                {
                    // New contact
                    incomingPath.PatientId = existingEntity.PatientId;
                    existingEntity.PathReports.Add(incomingPath);
                }
                else
                {
                    // Update fields of existing phone number
                    _context.Entry(existingPath).CurrentValues.SetValues(incomingPath);
                }
            }


            try
            {
                await _context.SaveChangesAsync();
                await _cacheStore.EvictByTagAsync("patient-api", CancellationToken.None);
                _logger.LogInformation("Patient updated.");

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientExists(id))
                {
                    _logger.LogWarning("Patient with id {id} not found for update.", id);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }



        // DELETE: api/Patients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(Guid id)
        {
            _logger.LogInformation("Patient delete requested.");

            var patient = await _context.Patients
                .Include(p => p.PathReports)
                .Include(p => p.PatientPhoneNumbers)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
            {
                _logger.LogWarning("Patient with id {id} not found for deletion.", id);
                return NotFound();
            }

            _context.Patients.Remove(patient);

            //patient.IsActive = false; // Soft delete by marking as inactive

            await _context.SaveChangesAsync();
            await _cacheStore.EvictByTagAsync("patient-api", CancellationToken.None);

            _logger.LogInformation("Patient deleted.");

            return NoContent();
        }

        private bool PatientExists(Guid id)
        {
            _logger.LogInformation("Checking if Patient with id {id} exists.", id);

            return _context.Patients.Any(e => e.PatientId == id);
        }

     
    }
}


       
    
