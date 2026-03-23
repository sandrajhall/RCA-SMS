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
    public class StudyHistologiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudyHistologiesController> _logger;
        private readonly UserContext _userContext;



        public StudyHistologiesController(ApplicationDbContext context, ILogger<StudyHistologiesController> logger, UserContext userContext)
        {
            _context = context;
            _logger = logger;
            _userContext = userContext;
        }


        // GET: api/StudyHistologies/histologies
        [HttpGet("histologies")]
        public async Task<ActionResult<IEnumerable<StudyHistologyView>>> ListStudyHistologies()
        {
            _logger.LogInformation("StudyHistologyView list requested.");

            // Fetch all active study histologies
            var query = _context.StudyHistologies
                .Include(sl => sl.Histology)
                .Include(sl => sl.Study)
                .Where(sl => sl.IsActive == true) // Filter to only include active study histologies
                .Select(sl => new StudyHistologyView
                {
                    StudyHistologyId = sl.StudyHistologyId,
                    StudyId = sl.StudyId,
                    HistologyId = sl.HistologyId,
                    HistologyCode = sl.Histology.HistologyCode,
                    HistologyBehavior = sl.Histology.HistologyBehavior,
                    HistologyName = sl.Histology.HistologyName,
                    IsActive = sl.IsActive,
                    IsPreferred = sl.Histology.IsPreferred,
                    Order = sl.Order
                })
            .AsQueryable();

            _logger.LogInformation("Returning {Count} StudyHistologyView.", query.Count());

            return await query.ToListAsync();
        }

        // GET: api/StudyHistologies/histologies/all
        [HttpGet("histologies/all")]
        public async Task<ActionResult<IEnumerable<StudyHistologyView>>> ListStudyHistologiesAll()
        {
            _logger.LogInformation("StudyHistologyView all list requested.");

            // Fetch all active study histologies
            var query = _context.StudyHistologies
                .Include(sl => sl.Histology)
                .Include(sl => sl.Study)
                .Select(sl => new StudyHistologyView
                {
                    StudyHistologyId = sl.StudyHistologyId,
                    StudyId = sl.StudyId,
                    HistologyId = sl.HistologyId,
                    HistologyCode = sl.Histology.HistologyCode,
                    HistologyBehavior = sl.Histology.HistologyBehavior,
                    HistologyName = sl.Histology.HistologyName,
                    IsActive = sl.IsActive,
                    IsPreferred = sl.Histology.IsPreferred,
                    Order = sl.Order
                })
            .AsQueryable();

            _logger.LogInformation("Returning all {Count} StudyHistologyView.", query.Count());

            return await query.ToListAsync();
        }

        // GET: api/StudyHistologies/histologies/{id}
        [HttpGet("histologies/{id}")]
        public async Task<ActionResult<IEnumerable<StudyHistologyView>>> ListStudyHistologiesByStudyId(Guid id)
        {
            _logger.LogInformation("StudyHistologyView list requested.");

            // Fetch all active study histologies
            var query = _context.StudyHistologies
                .Include(sl => sl.Histology)
                .Include(sl => sl.Study)
                .Where(sl => sl.StudyId == id && sl.IsActive == true) // Filter to only include active study histologies
                .OrderBy(sl => sl.Histology.HistologyCode)
                .ThenBy(sl => sl.Histology.HistologyName)
                .Select(sl => new StudyHistologyView
                {
                    StudyHistologyId = sl.StudyHistologyId,
                    StudyId = sl.StudyId,
                    HistologyId = sl.HistologyId,
                    HistologyCode = sl.Histology.HistologyCode,
                    HistologyBehavior = sl.Histology.HistologyBehavior,
                    HistologyName = sl.Histology.HistologyName,
                    IsActive = sl.IsActive,
                    IsPreferred = sl.Histology.IsPreferred,
                    Order = sl.Order
                })
            .AsQueryable();

            _logger.LogInformation("Returning {Count} StudyHistologyView.", query.Count());

            return await query.ToListAsync();
        }

        // GET: api/StudyHistologies/options/type
        [HttpGet("options/{type}/{studyId}")]
        public async Task<ActionResult<IEnumerable<string>>> ListOptions(string type, Guid studyId)
        {
            _logger.LogInformation("Options list requested.");

            // Fetch all active study histologies filtered by type
            var query = _context.StudyHistologies
                .Include(sl => sl.Histology)
                .Include(sl => sl.Study)
                .Where(sl => sl.StudyId == studyId) // Filter by histology type and active status
                .OrderBy(sl => sl.Order)
                .Select(sl => sl.Histology.HistologyName)
            .AsQueryable();

            _logger.LogInformation("Returning {Count} strings.", query.Count());

            return await query.ToListAsync();
        }


        // GET: api/StudyHistologies/5
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<StudyHistology>> GetStudyHistology(Guid id)
        {
            _logger.LogInformation("StudyHistology {id} requested.", id);

            var studyHistology = await _context.StudyHistologies
                .FirstOrDefaultAsync(s => s.StudyHistologyId == id);

            if (studyHistology == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Returning StudyHistology with id: {id}.", studyHistology.StudyHistologyId);

            return studyHistology;
        }

        // GET: api/StudyHistologies/valuebyoldcode/{studyId}/{oldCode}
        [HttpGet("valuebyoldcode/{studyId}/{oldCode}")]
        public async Task<ActionResult<Histology>> GetValueByOldCode(Guid studyId, int oldCode)
        {
            var query = _context.StudyHistologies
                .Include(s => s.Study)
                .Include(h => h.Histology)
                .Where(sl => sl.StudyId == studyId && sl.Order == oldCode)
                .AsQueryable();

            var histology = await query.Select(sl => sl.Histology).FirstOrDefaultAsync();

            if (histology == null)
            {
                return NotFound();
            }
            return histology;
        }


        // POST: api/StudyHistologies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{userId}")]
        public async Task<ActionResult<StudyHistology>> CreateStudyHistology(string userId, StudyHistology studyHistology)
        {
            _userContext.UserId = userId;
            _logger.LogInformation("StudyHistology creation started...");

            _context.StudyHistologies.Add(studyHistology);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Returning StudyHistology with id: {id}.", studyHistology.StudyId);


            return CreatedAtAction("CreateStudyHistology", new { id = studyHistology.StudyHistologyId }, studyHistology);
        }


        // PUT: api/StudyHistologies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}/{userId}")]
        public async Task<IActionResult> UpdateStudyHistology(Guid id, string userId, StudyHistology studyHistology)
        {
            _userContext.UserId = userId;
            _logger.LogInformation("StudyHistology update started...");

            if (id != studyHistology.StudyHistologyId)
            {
                return BadRequest();
            }


            _context.Attach(studyHistology).State = EntityState.Modified;


            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("StudyHistology updated.");

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudyHistologyExists(id))
                {
                    _logger.LogWarning("StudyHistology with id {id} not found for update.", id);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // DELETE: api/StudyHistologies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudyHistology(Guid id)
        {
            _logger.LogInformation("StudyHistology marked inactive.");

            var studyHistology = await _context.StudyHistologies
                            .FirstOrDefaultAsync(s => s.StudyHistologyId == id);
            if (studyHistology == null)
            {
                _logger.LogWarning("StudyHistology with id {id} not found for deletion.", id);
                return NotFound();
            }

            studyHistology.IsActive = false; // Soft delete by marking as inactive

            await _context.SaveChangesAsync();

            _logger.LogInformation("StudyHistology marked as inactive.");

            return NoContent();
        }


        private bool StudyHistologyExists(Guid id)
        {
            _logger.LogInformation("Checking if StudyHistology with id {id} exists.", id);

            return _context.StudyHistologies.Any(e => e.StudyHistologyId == id);
        }
    }
}
