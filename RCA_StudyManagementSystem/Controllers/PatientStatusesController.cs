using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RCA_StudyManagementSystem.Data;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientStatusesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientStatusesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PatientStatuses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientStatus>>> GetPatientStatuses()
        {
            return await _context.PatientStatuses.ToListAsync();
        }

        [HttpGet("patientstatusview/{StudyId}/{startDateStr}/{endDateStr}")]
        public async Task<ActionResult<IEnumerable<PatientStatusView>>> ListPatientStatusViewsByStudyId(Guid StudyId, string startDateStr, string endDateStr) 
        {

            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);

            var result = new List<PatientStatusView>();

            var patientStatusViews = await _context.PatientStatuses
                .Include(x => x.Patient)
                    .Where(s => s.StudyId == StudyId && s.Date >= startDate && s.Date <= endDate)
                    .OrderByDescending(s => s.Date)
                    .ToListAsync();

            foreach (var view in patientStatusViews)
            {
                result.Add(new PatientStatusView
                {
                    Date = view.Date,
                    CaseNumber = view.CaseNumber,
                    FirstName = view.Patient.FirstName,
                    LastName = view.Patient.LastName,
                    DateOfBirth = view.Patient.DateOfBirth,
                    NoContact = view.NoContact,
                    Status = view.Status,
                    DateOfDeath = view.DateOfDeath, 
                    DateLastContact = view.DateLastContact,
                    InformedOfCancerDiagnosis = view.InformedOfCancerDiagnosis,
                    StatedNoCancerDiagnosis = view.StatedNoCancerDiagnosis,
                    Comments = view.Comments,   

                });
            }
            return result;
        }

        [HttpGet("patientstatusviewcsv/{StudyId}/{startDateStr}/{endDateStr}")]
        public async Task<string> ListPatientStatusViewsByStudyIdCSV(Guid StudyId, string startDateStr, string endDateStr)
        {

            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);

            var result = new List<PatientStatusCSVView>();

            var patientStatusViews = await _context.PatientStatuses
                .Include(x => x.Patient)
                    .Include(s => s.Patient!.Study)
                    .Where(s => s.Patient!.Study!.StudyId == StudyId && s.Date >= startDate && s.Date <= endDate)
                    .OrderByDescending(s => s.Date)
                    .ToListAsync();

            foreach (var view in patientStatusViews)
            {
                result.Add(new PatientStatusCSVView
                {
                    Date = view.Date.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture),
                    CaseNumber = view.CaseNumber,
                    FirstName = view.Patient.FirstName,
                    LastName = view.Patient.LastName,
                    DateOfBirth = view.Patient.DateOfBirth?.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture) ?? string.Empty,
                    NoContact = view.NoContact == true ? "Y" : "N",
                    Status = view.Status,
                    DateOfDeath = view.DateOfDeath?.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture) ?? string.Empty,
                    DateLastContact = view.DateLastContact?.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture) ?? string.Empty,
                    InformedOfCancerDiagnosis = view.InformedOfCancerDiagnosis == true ? "Y" : "N",
                    StatedNoCancerDiagnosis = view.StatedNoCancerDiagnosis == true ? "Y" : "N",
                    Comments = view.Comments,

                });
            }

            using (var writer = new StringWriter())
            {
                using (var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
                {
                    // This writes the data into the StringWriter
                    csv.WriteRecords(result);
                }

                // This extracts the actual CSV text from the StringWriter
                return writer.ToString();
            }
        }

        // GET: api/PatientStatuses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientStatus>> GetPatientStatus(Guid id)
        {
            var patientStatus = await _context.PatientStatuses.FindAsync(id);

            if (patientStatus == null)
            {
                return NotFound();
            }

            return patientStatus;
        }

        // GET: api/PatientStatuses/casenumber/Prefix-12345
        [HttpGet("casenumber/{caseNumber}")]
        public async Task<ActionResult<PatientStatus>> GetPatientStatusByCaseNumber(string caseNumber)
        {
            var patientStatus = await _context.PatientStatuses
                                .Where(p => p.CaseNumber == caseNumber)
                                .FirstOrDefaultAsync();

            if (patientStatus == null)
            {
                return Ok(new PatientStatus()); // Return an empty PatientStatus object if not found
            }

                return patientStatus;
        }

        // POST: api/PatientStatuses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PatientStatus>> CreatePatientStatus(PatientStatus patientStatus)
        {
            _context.PatientStatuses.Add(patientStatus);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPatientStatus", new { id = patientStatus.PatientStatusId }, patientStatus);
        }

        // PUT: api/PatientStatuses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatientStatus(Guid id, PatientStatus patientStatus)
        {
            if (id != patientStatus.PatientStatusId)
            {
                return BadRequest();
            }

            var existingEntry = await _context.PatientStatuses.FindAsync(id);

            _context.Entry(existingEntry).CurrentValues.SetValues(patientStatus);
            //_context.Entry(existingEntry).State = EntityState.Modified;
            //_context.PatientStatuses.Update(existingEntry);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientStatusExists(id))
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



        // DELETE: api/PatientStatuses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatientStatus(Guid id)
        {
            var patientStatus = await _context.PatientStatuses.FindAsync(id);
            if (patientStatus == null)
            {
                return NotFound();
            }

            _context.PatientStatuses.Remove(patientStatus);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PatientStatusExists(Guid id)
        {
            return _context.PatientStatuses.Any(e => e.PatientStatusId == id);
        }
    }
}
