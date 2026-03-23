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
    public class StudiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudiesController> _logger;
        private readonly UserContext _userContext;



        public StudiesController(ApplicationDbContext context, ILogger<StudiesController> logger, UserContext userContext)
        {
            _context = context;
            _logger = logger;
            _userContext = userContext;
        }

        // GET: api/Studies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Study>>> ListStudies()
        {
            _logger.LogInformation("Study list requested.");
            
            // Fetch all active studies
            var query = _context.Studies
            .Where(pl => pl.IsActive) // Filter to only include active studies
            .AsQueryable();

            _logger.LogInformation("Returning {Count} Studies.", query.Count());

            return await query.ToListAsync();
        }


        // GET: api/Studies/archived
        [HttpGet("archived")]
        public async Task<ActionResult<IEnumerable<Study>>> ListArchivedStudies()
        {
            _logger.LogInformation("Study list requested.");

            // Fetch all active studies
            var query = _context.Studies
            .Where(pl => pl.IsArchived) // Filter to only include archived studies
            .AsQueryable();

            _logger.LogInformation("Returning {Count} Studies.", query.Count());

            return await query.ToListAsync();
        }

        // GET: api/Studies/unarchived
        [HttpGet("unarchived")]
        public async Task<ActionResult<IEnumerable<Study>>> ListUnarchivedStudies()
        {
            _logger.LogInformation("Study list requested.");

            // Fetch all active studies
            var query = _context.Studies
            .Where(pl => pl.IsArchived == false) // Filter to only include unarchived studies
            .AsQueryable();

            _logger.LogInformation("Returning {Count} Studies.", query.Count());

            return await query.ToListAsync();
        }


        // GET: api/Studies/5
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Study>> GetStudy(Guid id)
        {
            _logger.LogInformation("Study {id} requested.", id);

            var study = await _context.Studies
                .Include(sc => sc.StudyContacts)
                .Include(sl => sl.StudyLookups!.Where(a => a.IsActive))
                    .ThenInclude(l => l.Lookup)
                .Include(sh => sh.StudyHistologies!.Where(a => a.IsActive))
                    .ThenInclude(h => h.Histology)
                    .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudyId == id);

            if (study == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Returning Study with id: {id}.", study.StudyId);

            return study;
        }

        // GET: api/Studies/info/5
        [HttpGet("info/{id:guid}")]
        public async Task<ActionResult<Study>> GetStudyInfo(Guid id)
        {
            _logger.LogInformation("Study info {id} requested.", id);

            var study = await _context.Studies
                    .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudyId == id);

            if (study == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Returning Study info with id: {id}.", study.StudyId);

            return study;
        }

        // GET: api/Studies/name/5
        [HttpGet("name/{id:guid}")]
        public async Task<ActionResult<string>> GetStudyName(Guid id)
        {
            _logger.LogInformation("Study {id} name requested.", id);

            var studyName = await _context.Studies
                .Where(s => s.StudyId == id)
                .Select(n => n.Name)
                .SingleOrDefaultAsync();

            if (studyName == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Returning Study Name: {studyName}.", studyName);

            return studyName;
        }

        // GET: api/Studies/5
        [HttpGet("color/{id:guid}")]
        public async Task<string> GetStudyColor(Guid id)
        {
            _logger.LogInformation("Study {id} color requested.", id);

            var color = await _context.Studies
                .Where(s => s.StudyId == id)
                .Select(c => c.ColorLight)
                .SingleOrDefaultAsync();

            if (color == null)
            {
                return "Color not found.";
            }

            _logger.LogInformation("Returning Study color: {color}.", color);

            return color;
        }

        // POST: api/Studies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{userId}")]
        public async Task<ActionResult<Study>> CreateStudy(string userId, Study study)
        {
            _logger.LogInformation("Study creation started...");

            _userContext.UserId = userId;
            _context.Studies.Add(study);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Returning Study with id: {id}.", study.StudyId);


            return CreatedAtAction("CreateStudy", new { id = study.StudyId }, study);
        }


        // PUT: api/Studies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}/{userId}")]
        public async Task<IActionResult> UpdateStudy(Guid id, string userId, Study study)
        {
            _logger.LogInformation("Updating study with ID: {id} ", study.StudyId);

            _userContext.UserId = userId;

            var existingEntity = await _context.Studies
                     .Include(c => c.StudyContacts)
                     .AsTracking()
                     .FirstOrDefaultAsync(s => s.StudyId == id);

            if (id != study.StudyId)
            {
                return BadRequest();
            }


            // Update scalar properties
            _context.Entry(existingEntity).CurrentValues.SetValues(study);

            // Update StudyContacts
            var incomingContactIds = study.StudyContacts.Select(c => c.StudyContactId).ToList();
            foreach (var contact in existingEntity.StudyContacts.ToList())
            {
                if (!incomingContactIds.Contains(contact.StudyContactId))
                    _context.StudyContacts.Remove(contact);
            }

            foreach (var incomingContact in study.StudyContacts)
            {
                // Try to find if this Guid already exists in the database's child collection
                var existingContact = existingEntity.StudyContacts
                    .FirstOrDefault(c => c.StudyContactId == incomingContact.StudyContactId);
                if (existingContact == null)
                {
                    // 1. THIS IS A NEW CONTACT
                    // Ensure the Foreign Key is set correctly
                    incomingContact.StudyId = existingEntity.StudyId;

                    // 2. Add it to the collection tracked by the context
                    existingEntity.StudyContacts.Add(incomingContact);

                    // 3. FORCE EF to recognize this as a NEW insert
                    _context.Entry(incomingContact).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
                else
                {
                    // THIS IS AN UPDATE
                    _context.Entry(existingContact).CurrentValues.SetValues(incomingContact);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Study updated.");

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudyExists(id))
                {
                    _logger.LogWarning("Study with id {id} not found for update.", id);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Studies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudy(Guid id)
        {
            _logger.LogInformation("Study marked inactive.");

            var study = await _context.Studies.FindAsync(id);
            if (study == null)
            {
                _logger.LogWarning("Study with id {id} not found for deletion.", id);
                return NotFound();
            }

            study.IsActive = false; // Soft delete by marking as inactive

            await _context.SaveChangesAsync();

            _logger.LogInformation("Study marked inactive.");

            return NoContent();
        }

        // DELETE: api/Studies/archive/5
        [HttpDelete("archive/{id}")]
        public async Task<IActionResult> ArchiveStudy(Guid id)
        {
            _logger.LogInformation("Study marked archived.");

            var study = await _context.Studies.FindAsync(id);
            if (study == null)
            {
                _logger.LogWarning("Study with id {id} not found for deletion.", id);
                return NotFound();
            }

            study.IsArchived = true; // Soft delete by marking as archived

            await _context.SaveChangesAsync();

            _logger.LogInformation("Study marked archived.");

            return NoContent();
        }

        private bool StudyExists(Guid id)
        {
            _logger.LogInformation("Checking if Study with id {id} exists.", id);

            return _context.Studies.Any(e => e.StudyId == id);
        }
    }
}
