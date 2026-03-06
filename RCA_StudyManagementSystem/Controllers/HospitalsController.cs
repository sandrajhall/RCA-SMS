using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RCA_StudyManagementSystem.Data;
using RCA_StudyManagementSystem.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HospitalsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HospitalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Hospitals
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Hospital>>> ListHospitals()
        {
            return await _context.Hospitals
                .Where(h => h.IsActive == true) // Only active hospitals
                .OrderBy(h => h.HospitalName)
                .ToListAsync();
        }


        // GET: api/Hospitals/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Hospital>>> ListAllHospitals()
        {
            return await _context.Hospitals.OrderBy(h => h.HospitalName).ToListAsync();
        }

        // GET: api/Hospitals/reimbursemententity/{reimbursementEntityId}
        [HttpGet("reimbursemententity/{reimbursemententityid}")]
        public async Task<ActionResult<IEnumerable<Hospital>>> ListHospitalsForReimbursementEntity(Guid reimbursemententityid)
        {
            return await _context.Hospitals.Where(h => h.ReimbursementEntityId == reimbursemententityid).OrderBy(h => h.HospitalName).ToListAsync();
        }

        // GET: api/Hospitals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Hospital>> GetHospital(Guid id)
        {
            var hospital = await _context.Hospitals.FindAsync(id);

            if (hospital == null)
            {
                return NotFound();
            }

            return hospital;
        }

        // GET: api/Hospitals/migratedid/{migratedId}
        [HttpGet("migratedid/{migratedId}")]
        public async Task<ActionResult<Hospital>> GetHospitalByMigratedId(string migratedId)
        {
            var hospital = await _context.Hospitals
                .Where(h => h.MigratedHospitalId == migratedId)
                .FirstOrDefaultAsync();
            if (hospital == null)
            {
                return NotFound();
            }
            return hospital;
        }

        // GET: api/Hospitals/search/{searchTerm}
        [HttpGet("search/{searchTerm}")]
        public async Task<IEnumerable<Hospital>> GetHospitalsAsync(string searchTerm)
        {
            var query = _context.Hospitals
                .Where(h => h.HospitalName.Contains(searchTerm) || h.HospitalShortName.Contains(searchTerm) || h.City.Contains(searchTerm))
                .OrderBy(h => h.HospitalName)
                .Take(50); // Important: Limit the number of records returned

            return await query.ToListAsync();
        }

        // GET: api/Hospitals/id/{hospName}
        [HttpGet("id/{hospName}")]
        public async Task<ActionResult<Guid>> GetHospitalId(string hospName)
        {
            var id = _context.Hospitals
                .Where(x => x.HospitalName == hospName)
                .FirstOrDefault()?.HospitalId;

            if (id == null)
            {
                return NotFound();
            }

            return id;
        }


        // GET: api/Hospitals/shortname/{hospName}/{city}
        [HttpGet("shortname/{hospName}/{city}")]
        public async Task<ActionResult<string>> GetHospShortName(string hospName, string city)
        {
            var shortName = _context.Hospitals
                .Where(x => x.HospitalName == hospName && x.City == city)
                .FirstOrDefault()?.HospitalShortName;

            if (shortName == null)
            {
                return NotFound();
            }

            return shortName;
        }


        // POST: api/Hospitals
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Hospital>> CreateHospital(Hospital hospital)
        {
            _context.Hospitals.Add(hospital);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHospital", new { id = hospital.HospitalId }, hospital);
        }

        // PUT: api/Hospitals/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHospital(Guid id, Hospital hospital)
        {
            if (id != hospital.HospitalId)
            {
                return BadRequest();
            }

            _context.Entry(hospital).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HospitalExists(id))
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


        // DELETE: api/Hospitals/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHospital(Guid id)
        {
            var hospital = await _context.Hospitals.FindAsync(id);
            if (hospital == null)
            {
                return NotFound();
            }

            hospital.IsActive = false; // Soft delete by marking as inactive

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HospitalExists(Guid id)
        {
            return _context.Hospitals.Any(e => e.HospitalId == id);
        }
    }
}
