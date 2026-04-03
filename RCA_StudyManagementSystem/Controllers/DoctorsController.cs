using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using RCA_StudyManagementSystem.Data;
using RCA_StudyManagementSystem.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IOutputCacheStore _cacheStore;
        private readonly UserContext _userContext;



        public DoctorsController(ApplicationDbContext context, IOutputCacheStore cacheStore, UserContext userContext)
        {
            _context = context;
            _cacheStore = cacheStore;
            _userContext = userContext;
        }

        // GET: api/Doctors
        [OutputCache(PolicyName = "DoctorTagPolicy")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Doctor>>> ListDoctors()
        {
            return await _context.Doctors
                .Where(d => d.IsActive == true && d.IsPathologist == false)
                .OrderBy(d => d.LastName)
                .ToListAsync();
        }

        // GET: api/Doctors/pathologists
        [OutputCache(PolicyName = "DoctorTagPolicy")]
        [HttpGet("pathologists")]
        public async Task<ActionResult<IEnumerable<Doctor>>> ListPathologists()
        {
            return await _context.Doctors
                .Where(d => d.IsActive == true && d.IsPathologist == true)
                .OrderBy(d => d.LastName)
                .ToListAsync();
        }


        // GET: api/Doctors/all
        [OutputCache(PolicyName = "DoctorTagPolicy")]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Doctor>>> ListAllDoctors()
        {
            return await _context.Doctors
                .Where(d => d.IsActive == true)
                .OrderBy(d => d.LastName)
                .ToListAsync();
        }

        // GET: api/Doctors/5
        [OutputCache(PolicyName = "DoctorTagPolicy")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Doctor>> GetDoctor(Guid id)
        {
            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
            {
                return NotFound();
            }

            return doctor;
        }

        // GET: api/Doctors/migratedid/{migratedId}
        [OutputCache(PolicyName = "DoctorTagPolicy")]
        [HttpGet("migratedid/{migratedId}")]
        public async Task<ActionResult<Doctor>> GetDoctorByMigratedId(string migratedId)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.MigratedDoctorId == migratedId);
            if (doctor == null)
            {
                return NotFound();
            }
            return doctor;
        }

        // GET: api/Doctors/search/{searchTerm}
        [OutputCache(PolicyName = "DoctorTagPolicy")]
        [HttpGet("search/{searchTerm}")]
        public async Task<IEnumerable<Doctor>> GetDoctorsAsync(string searchTerm)
        {
            var query = _context.Doctors
                .Where(d => !d.IsPathologist && (d.DisplayName.Contains(searchTerm) || d.City.Contains(searchTerm) || d.PrimarySpecialty.Contains(searchTerm)))
                .OrderBy(d => d.LastName)
                .Take(50); // Important: Limit the number of records returned

            return await query.ToListAsync();
        }

        // GET: api/Doctors/pathsearch/{searchTerm}
        [OutputCache(PolicyName = "DoctorTagPolicy")]
        [HttpGet("pathsearch/{searchTerm}")]
        public async Task<IEnumerable<Doctor>> GetPathologistsAsync(string searchTerm)
        {
            var query = _context.Doctors
                .Where(d => d.IsPathologist && (d.DisplayName.Contains(searchTerm) || d.City.Contains(searchTerm) || d.PrimarySpecialty.Contains(searchTerm)))
                .OrderBy(d => d.LastName)
                .Take(50); // Important: Limit the number of records returned

            return await query.ToListAsync();
        }

        // GET: api/Doctors/doctorhistory/5
        [OutputCache(PolicyName = "DoctorTagPolicy")]
        [HttpGet("doctorhistory/{id:guid}")]
        public async Task<ActionResult<List<Doctor>>> GetDoctorHistory(Guid id)
        {
            var history = await _context.Doctors    
                .TemporalAll()
                .Where(p => p.DoctorId == id)
                .OrderByDescending(p => EF.Property<DateTime>(p, "PeriodStart"))
                .ToListAsync();

            if (history == null)
            {
                return NotFound();
            }
            return history;
        }


        // POST: api/Doctors
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{userId}")]
        public async Task<ActionResult<Doctor>> CreateDoctor(string userId, Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            _userContext.UserId = userId;
            await _context.SaveChangesAsync();
            await _cacheStore.EvictByTagAsync("doctor-api", CancellationToken.None);

            return CreatedAtAction("GetDoctor", new { id = doctor.DoctorId }, doctor);
        }

        // PUT: api/Doctors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}/{userId}")]
        public async Task<IActionResult> UpdateDoctor(Guid id, string userId, Doctor doctor)
        {
            if (id != doctor.DoctorId)
            {
                return BadRequest();
            }
            _userContext.UserId = userId;

            _context.Entry(doctor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await _cacheStore.EvictByTagAsync("doctor-api", CancellationToken.None);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DoctorExists(id))
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


        // DELETE: api/Doctors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(Guid id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            doctor.IsActive = false;

            await _context.SaveChangesAsync();
           
            await _cacheStore.EvictByTagAsync("doctor-api", CancellationToken.None);

            return NoContent();
        }

        private bool DoctorExists(Guid id)
        {
            return _context.Doctors.Any(e => e.DoctorId == id);
        }
    }
}
