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
    public class StudyLookupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudyLookupsController> _logger;
        private readonly UserContext _userContext;



        public StudyLookupsController(ApplicationDbContext context, ILogger<StudyLookupsController> logger, UserContext userContext)
        {
            _context = context;
            _logger = logger;
            _userContext = userContext;
        }


        // GET: api/StudyLookups
        [HttpGet("lookups")]
        public async Task<ActionResult<IEnumerable<StudyLookupView>>> ListStudyLookups()
        {
            _logger.LogInformation("StudyLookupView list requested.");

            // Fetch all active study lookups
            var query = _context.StudyLookups
                .Include(sl => sl.Lookup)
                .Include(sl => sl.Study)
                .Where(sl => sl.IsActive==true) // Filter to only include active study lookups
                .OrderBy(sl => sl.Order) // Order by Order
                .Select(sl => new StudyLookupView
                {
                    StudyLookupId = sl.StudyLookupId,
                    StudyId = sl.StudyId,
                    LookupId = sl.LookupId,
                    LookupCode = sl.Lookup.LookupCode,
                    LookupName = sl.Lookup.LookupName,
                    LookupType = sl.Lookup.LookupType,
                    Order = sl.Order
                })
            .AsQueryable();

            _logger.LogInformation("Returning {Count} StudyLookupViews.", query.Count());

            return await query.ToListAsync();
        }

        // GET: api/StudyLookups
        [HttpGet("lookups/{id}")]
        public async Task<ActionResult<IEnumerable<StudyLookupView>>> ListStudyLookupsByStudyId(Guid id)
        {
            _logger.LogInformation("StudyLookupView list requested.");

            // Fetch all active study lookups
            var query = _context.StudyLookups
                .Include(sl => sl.Lookup)
                .Include(sl => sl.Study)
                .Where(sl => sl.StudyId == id && sl.IsActive == true)
                .OrderBy(sl => sl.Order)
                .Select(sl => new StudyLookupView
                {
                    StudyLookupId = sl.StudyLookupId,
                    StudyId = sl.StudyId,
                    LookupId = sl.LookupId,
                    LookupCode = sl.Lookup.LookupCode,
                    LookupName = sl.Lookup.LookupName,
                    LookupType = sl.Lookup.LookupType,
                    Order = sl.Order
                })
            .AsQueryable();

            _logger.LogInformation("Returning {Count} StudyLookupViews.", query.Count());

            return await query.ToListAsync();
        }

        // GET: api/StudyLookups/type
        [HttpGet("{type}")]
        public async Task<ActionResult<IEnumerable<StudyLookupView>>> ListStudyLookupsByType(string type)
        {
            _logger.LogInformation("StudyLookupView list requested.");

            // Fetch all active study lookups filtered by type
            var query = _context.StudyLookups
                .Include(sl => sl.Lookup)
                .Include(sl => sl.Study)
                .Where(sl => sl.Lookup.LookupType == type && sl.IsActive == true) // Filter by lookup type and active status
                .OrderBy(sl => sl.Order)
                .Select(sl => new StudyLookupView
                {
                    StudyId = sl.StudyId,
                    LookupId = sl.LookupId,
                    LookupCode = sl.Lookup.LookupCode,
                    LookupName = sl.Lookup.LookupName,
                    LookupType = sl.Lookup.LookupType,
                    StudyLookupId = sl.StudyLookupId,
                    Order = sl.Order
                })
            .AsQueryable();

            _logger.LogInformation("Returning {Count} StudyLookupViews.", query.Count());

            return await query.ToListAsync();
        }

        // GET: api/StudyLookups/valuebyoldcode/{type}/{oldCode}
        [HttpGet("valuebyoldcode/{studyId}/{type}/{oldCode}")]
        public async Task<ActionResult<string>> GetValueByOldCode(Guid studyId, string type, int oldCode)
        {

            _logger.LogInformation("StudyLookupView list requested.");

            var query = _context.StudyLookups
                .Include(sl => sl.Lookup)
                .Include(sl => sl.Study)
                .Where(sl => sl.Lookup.LookupType == type && sl.StudyId == studyId && sl.Order == oldCode)
                .OrderBy(sl => sl.Order)
                .Select(sl => new StudyLookupView
                {
                    StudyId = sl.StudyId,
                    LookupId = sl.LookupId,
                    LookupCode = sl.Lookup.LookupCode,
                    LookupName = sl.Lookup.LookupName,
                    LookupType = sl.Lookup.LookupType,
                    StudyLookupId = sl.StudyLookupId,
                    Order = sl.Order
                })
                .AsQueryable();

            var value = query.ToList().FirstOrDefault().LookupName;

            _logger.LogInformation("Returning {Count} StudyLookupViews.", query.Count());

            return value ?? "Unknown";
        }





        // GET: api/StudyLookups/codebyvalue/{studyId}/{type}/{value}
        [HttpGet("codebyvalue/{studyId}/{type}/{*value}")]
        public async Task<ActionResult<string>> GetCodeByValue(Guid studyId, string type, string value)
        {
            _logger.LogInformation("StudyLookupView list requested.");

            var query = _context.StudyLookups
                .Include(sl => sl.Lookup)
                .Include(sl => sl.Study)
                .Where(sl => sl.Lookup.LookupType == type && sl.StudyId == studyId && sl.Lookup.LookupName == value)
                .OrderBy(sl => sl.Order)
                .Select(sl => new StudyLookupView
                {
                    StudyId = sl.StudyId,
                    LookupId = sl.LookupId,
                    LookupCode = sl.Lookup.LookupCode,
                    LookupName = sl.Lookup.LookupName,
                    LookupType = sl.Lookup.LookupType,
                    StudyLookupId = sl.StudyLookupId,
                    Order = sl.Order
                })
                .AsQueryable();

            var code = query.ToList().FirstOrDefault().LookupCode;
            
            _logger.LogInformation("Returning {Count} StudyLookupViews.", query.Count());
            return code ?? "Unknown";
        }

        // GET: api/StudyLookups/all/type
        [HttpGet("all/{type}")]
        public async Task<ActionResult<IEnumerable<StudyLookupView>>> ListStudyLookupsAllByType(string type)
        {
            _logger.LogInformation("StudyLookupView all list requested.");

            // Fetch all active study lookups filtered by type
            var query = _context.StudyLookups
                .Include(sl => sl.Lookup)
                .Include(sl => sl.Study)
                .Where(sl => sl.Lookup.LookupType == type) // Filter by lookup type
                .OrderBy(sl => sl.Order)
                .Select(sl => new StudyLookupView
                {
                    StudyId = sl.StudyId,
                    LookupId = sl.LookupId,
                    LookupCode = sl.Lookup.LookupCode,
                    LookupName = sl.Lookup.LookupName,
                    LookupType = sl.Lookup.LookupType,
                    StudyLookupId = sl.StudyLookupId,
                    Order = sl.Order
                })
            .AsQueryable();

            _logger.LogInformation("Returning all {Count} StudyLookupViews.", query.Count());

            return await query.ToListAsync();
        }


        // GET: api/StudyLookups/options/type/studyId
        [HttpGet("options/{type}/{studyId}")]
        public async Task<ActionResult<IEnumerable<string>>> ListOptions(string type, Guid studyId)
        {
            _logger.LogInformation("Options list requested.");

            // Fetch all active study lookups filtered by type
            var query = _context.StudyLookups
                .Include(sl => sl.Lookup)
                .Include(sl => sl.Study)
                .Where(sl => sl.Lookup.LookupType == type && sl.StudyId == studyId && sl.IsActive == true) // Filter by lookup type and active status
                .OrderBy(sl => sl.Order)
                .Select(sl => sl.Lookup.LookupName)
            .AsQueryable();

            _logger.LogInformation("Returning {Count} strings.", query.Count());

            return await query.ToListAsync();
        }


        // GET: api/StudyLookups/5
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<StudyLookup>> GetStudyLookup(Guid id)
        {
            _logger.LogInformation("StudyLookup {id} requested.", id);

            var studyLookup = await _context.StudyLookups
                .FirstOrDefaultAsync(s => s.StudyLookupId == id);

            if (studyLookup == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Returning StudyLookup with id: {id}.", studyLookup.StudyLookupId);

            return studyLookup;
        }


        // POST: api/StudyLookups
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{userId}")]
        public async Task<ActionResult<StudyLookup>> CreateStudyLookup(string userId, StudyLookup studyLookup)
        {
            _logger.LogInformation("StudyLookup creation started...");
            _userContext.UserId = userId;

            _context.StudyLookups.Add(studyLookup);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Returning StudyLookup with id: {id}.", studyLookup.StudyId);


            return CreatedAtAction("CreateStudyLookup", new { id = studyLookup.StudyLookupId }, studyLookup);
        }


        // PUT: api/StudyLookups/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}/{userId}")]
        public async Task<IActionResult> UpdateStudyLookup(Guid id, string userId, StudyLookup studyLookup)
        {
            _logger.LogInformation("Study update started...");
            _userContext.UserId = userId;

            if (id != studyLookup.StudyLookupId)
            {
                return BadRequest();
            }


            _context.Attach(studyLookup).State = EntityState.Modified;

           // _context.Update(studyLookup);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("StudyLookup updated.");

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudyLookupExists(id))
                {
                    _logger.LogWarning("StudyLookup with id {id} not found for update.", id);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // DELETE: api/StudyLookups/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudyLookup(Guid id)
        {
            _logger.LogInformation("StudyLookup marked inactive.");

            var studyLookup = await _context.StudyLookups
                            .FirstOrDefaultAsync(s => s.StudyLookupId == id);
            if (studyLookup == null)
            {
                _logger.LogWarning("StudyLookup with id {id} not found for deletion.", id);
                return NotFound();
            }

            studyLookup.IsActive = false; // Soft delete by marking as inactive

            await _context.SaveChangesAsync();

            _logger.LogInformation("StudyLookup marked as inactive.");

            return NoContent();
        }


        private bool StudyLookupExists(Guid id)
        {
            _logger.LogInformation("Checking if StudyLookup with id {id} exists.", id);

            return _context.StudyLookups.Any(e => e.StudyLookupId == id);
        }
    }
}
