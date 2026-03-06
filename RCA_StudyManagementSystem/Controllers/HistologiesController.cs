using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RCA_StudyManagementSystem.Api.Data;
using RCA_StudyManagementSystem.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RCA_StudyManagementSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistologiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HistologiesController> _logger;
        private readonly IOutputCacheStore _cacheStore;


        public HistologiesController(ApplicationDbContext context, ILogger<HistologiesController> logger, IOutputCacheStore cacheStore)
        {
            _context = context;
            _logger = logger;
            _cacheStore = cacheStore;
        }

        // GET: api/Histologies
        [OutputCache(PolicyName = "HistologyTagPolicy")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Histology>>> ListHistologies()
        {
            _logger.LogInformation("Histology list requested.");
            
            // Fetch all active lookups
            var query = _context.Histologies
            .OrderBy(s => s.HistologyCode)
            .ThenBy(s => s.HistologyBehavior)
            .ThenBy(s => s.HistologyName) // 
            .AsQueryable();

            _logger.LogInformation("Returning {Count} Histologies.", query.Count());

            return await query.ToListAsync();
        }

        // GET: api/Histologies/active
        [OutputCache(PolicyName = "HistologyTagPolicy")]
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Histology>>> ListActiveHistologies()
        {
            _logger.LogInformation("Histology list requested.");

            // Fetch all active lookups
            var query = _context.Histologies
            .Where(s => s.IsActive == true) // Filter to only include active histologies
            .OrderBy(s => s.HistologyCode)
            .ThenBy(s => s.HistologyBehavior)
            .ThenBy(s => s.HistologyName) // 
            .AsQueryable();

            _logger.LogInformation("Returning {Count} Histologies.", query.Count());

            return await query.ToListAsync();
        }


        // GET: api/Histologies/5
        [OutputCache(PolicyName = "HistologyTagPolicy")]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Histology>> GetHistology(Guid id)
        {
            _logger.LogInformation("Histology {id} requested.", id);

            var histology = await _context.Histologies
                .FirstOrDefaultAsync(s => s.HistologyId == id);

            if (histology == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Returning Histology with id: {id}.", histology.HistologyId);

            return histology;
        }

        // POST: api/Histologies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Histology>> CreateHistology(Histology histology)
        {
            _logger.LogInformation("Histology creation started...");

            _context.Histologies.Add(histology);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Returning Histology with id: {id}.", histology.HistologyId);
            await _cacheStore.EvictByTagAsync("histology-api", CancellationToken.None);


            return CreatedAtAction("GetHistology", new { id = histology.HistologyId }, histology);
        }

        // PUT: api/Histologies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHistology(Guid id, Histology histology)
        {
            _logger.LogInformation("Histology update started...");

            if (id != histology.HistologyId)
            {
                return BadRequest();
            }

            _context.Update(histology);

            try
            {
                await _context.SaveChangesAsync();
                await _cacheStore.EvictByTagAsync("histology-api", CancellationToken.None);
                _logger.LogInformation("Histology updated.");

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HistologyExists(id))
                {
                    _logger.LogWarning("Histology with id {id} not found for update.", id);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }



        // DELETE: api/Histologies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHistology(Guid id)
        {
            _logger.LogInformation("Histology marked inactive.");

            var histology = await _context.Histologies.FindAsync(id);
            if (histology == null)
            {
                _logger.LogWarning("Histology with id {id} not found for deletion.", id);
                return NotFound();
            }

            histology.IsActive = false; // Soft delete by marking as inactive

            await _context.SaveChangesAsync();
            await _cacheStore.EvictByTagAsync("histology-api", CancellationToken.None);

            _logger.LogInformation("Histology marked inactive.");

            return NoContent();
        }

        private bool HistologyExists(Guid id)
        {
            _logger.LogInformation("Checking if Histology with id {id} exists.", id);

            return _context.Histologies.Any(e => e.HistologyId == id);
        }
    }
}
