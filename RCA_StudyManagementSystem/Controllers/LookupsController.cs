using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RCA_StudyManagementSystem.Data;
using RCA_StudyManagementSystem.Services;
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
    public class LookupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LookupsController> _logger;
        private readonly UserContext _userContext;



        public LookupsController(ApplicationDbContext context, ILogger<LookupsController> logger, UserContext userContext)
        {
            _context = context;
            _logger = logger;
            _userContext = userContext;
        }

        // GET: api/Lookups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lookup>>> ListLookups()
        {
            _logger.LogInformation("Lookup list requested.");
            
            // Fetch all active lookups
            var query = _context.Lookups
            .Where(s => s.IsActive==true) // Filter to only include active lookups
            .OrderBy(s => s.LookupType) // Order by LookupName
            .ThenBy(s => s.LookupCode) // Then by Lookup Name
            .ThenBy(s => s.SortOrder) // Then by Sort Order
            .AsQueryable();

            _logger.LogInformation("Returning {Count} Lookups.", query.Count());

            return await query.ToListAsync();
        }

        // GET: api/Lookups/type
        [HttpGet("{type}")]
        public async Task<ActionResult<IEnumerable<string>>> ListLookupsByType(string type)
        {
            _logger.LogInformation("Lookup list requested for type {type}.", type);

            // Fetch all active lookups
            var query = _context.Lookups
            .Where(s => s.IsActive==true && s.LookupType == type) 
            .OrderBy(s => s.LookupCode) // Order by LookupCode
            .ThenBy(s => s.LookupName)
            .ThenBy(s => s.SortOrder) // Order by Sort Order
            .Select(s => s.LookupName) // Select only the LookupName
            .AsQueryable();

            _logger.LogInformation("Returning {Count} lookups for type {type}.", query.Count(), type);

            return await query.ToListAsync();
        }


        // GET: api/Lookups/5
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Lookup>> GetLookup(Guid id)
        {
            _logger.LogInformation("Lookup {id} requested.", id);

            var lookup = await _context.Lookups
                .FirstOrDefaultAsync(s => s.LookupId == id);

            if (lookup == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Returning Lookup with id: {id}.", lookup.LookupId);

            return lookup;
        }

        // GET: api/Lookups/fips/{fips}
        [HttpGet("fips/{fips}")]
        public async Task<ActionResult<string>> GetCountyByFIPS(string fips)
        {
            _logger.LogInformation("County {fips} requested.", fips);
            var countyCode = fips.Remove(0, 2);
            var lookup = await _context.Lookups
                .FirstOrDefaultAsync(s => s.LookupCode == countyCode && s.LookupType == "County");

            var county = lookup.LookupName;

            if (county == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Returning Lookup with id: {id}.", lookup.LookupId);

            return county;
        }

        // GET: api/Lookups/typebycode/{type}/{typeCode}
        [HttpGet("typebycode/{type}/{typeCode}")]
        public async Task<ActionResult<string>> GetTypeByCode(string type, string typeCode)
        {
           
            _logger.LogInformation("Type {type} {typeCode} requested.", type, typeCode);
            var lookup = await _context.Lookups
                .FirstOrDefaultAsync(s => s.LookupCode == typeCode && s.LookupType == type);

            var typeStr = lookup.LookupName;

            if (typeStr == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Returning Lookup with id: {id}.", lookup.LookupId);

            return typeStr;
        }

        // GET: api/Lookups/codebytype/{type}/{typeValue}
        [HttpGet("codebytype/{type}/{typeValue}")]
        public async Task<ActionResult<string>> GetCodeByType(string type, string typeValue)
        {
            _logger.LogInformation("Type {type} {typeValue} requested.", type, typeValue);
            var lookup = await _context.Lookups
                .FirstOrDefaultAsync(s => s.LookupName == typeValue && s.LookupType == type);

            var codeStr = lookup.LookupCode;

            if (codeStr == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Returning Lookup with id: {id}.", lookup.LookupId);

            return codeStr;
        }


        // POST: api/Lookups
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{userId}")]
        public async Task<ActionResult<Lookup>> CreateLookup(string userId, Lookup lookup)
        {
            _logger.LogInformation("Lookup creation started...");
            _userContext.UserId = userId;

            _context.Lookups.Add(lookup);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Returning Lookup with id: {id}.", lookup.LookupId);


            return CreatedAtAction("GetLookup", new { id = lookup.LookupId }, lookup);
        }

        // PUT: api/Lookups/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}/{userId}")]
        public async Task<IActionResult> UpdateLookup(Guid id, string userId, Lookup lookup)
        {
            _logger.LogInformation("Lookup update started...");
            _userContext.UserId = userId;

            if (id != lookup.LookupId)
            {
                return BadRequest();
            }

            _context.Update(lookup);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Lookup updated.");

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LookupExists(id))
                {
                    _logger.LogWarning("Lookup with id {id} not found for update.", id);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }



        // DELETE: api/Lookups/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLookup(Guid id)
        {
            _logger.LogInformation("Lookup marked inactive.");

            var lookup = await _context.Lookups.FindAsync(id);
            if (lookup == null)
            {
                _logger.LogWarning("Lookup with id {id} not found for deletion.", id);
                return NotFound();
            }

            lookup.IsActive = false; // Soft delete by marking as inactive

            await _context.SaveChangesAsync();

            _logger.LogInformation("Lookup marked inactive.");

            return NoContent();
        }

        private bool LookupExists(Guid id)
        {
            _logger.LogInformation("Checking if Lookup with id {id} exists.", id);

            return _context.Lookups.Any(e => e.LookupId == id);
        }
    }
}
