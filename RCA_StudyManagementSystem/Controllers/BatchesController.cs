using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RCA_StudyManagementSystem.Controllers;
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
    public class BatchesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BatchesController> _logger;
        private readonly UserContext _userContext;



        public BatchesController(ApplicationDbContext context, ILogger<BatchesController> logger, UserContext userContext)
        {
            _context = context;
            _logger = logger;
            _userContext = userContext;
        }

        // GET: api/Batches
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Batch>>> ListBatches()
        {
            return await _context.Batches.ToListAsync();
        }

        // GET: api/Batches/study/{studyId}
        [HttpGet("study/{studyId}")]
        public async Task<ActionResult<IEnumerable<Batch>>> ListBatchesByStudy(Guid studyId)
        {
            return await _context.Batches
                .Include(b => b.PathReportExports)
                .Where(b => b.StudyId == studyId)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
        }

        // GET: api/Batches/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Batch>> GetBatch(Guid id)
        {
            var batch = await _context.Batches.FindAsync(id);

            if (batch == null)
            {
                return NotFound();
            }

            return batch;
        }

        // GET: api/Batches/lastbatchnumber/{prefix}
        [HttpGet("lastbatchnumber/{prefix}")]
        public async Task<ActionResult<string>> GetLastBatchNumber(string prefix)
        {
            _logger.LogInformation("Batch last batch id requested.");

            var query = _context.Batches?
                .Where(s => s.BatchNumber.StartsWith(prefix))?
                .OrderBy(s => s.BatchNumber)?
                .LastOrDefault()?.BatchNumber;

            _logger.LogInformation("Returning id: {id}.", query);

            return query ?? String.Empty;
        }

        // GET: api/Batches/batchid/{batchNumber}
        [HttpGet("batchid/{batchNumber}")]
        public async Task<ActionResult<Guid>> GetBatchId(string batchNumber)
        {
            _logger.LogInformation("Batch id requested.");

            var query = _context.Batches?
                .Where(s => s.BatchNumber == batchNumber)?
                .OrderBy(s => s.BatchNumber)?
                .FirstOrDefault()?.BatchId;

            _logger.LogInformation("Returning id: {id}.", query);

            return query ?? Guid.Empty;
        }

        // POST: api/Batches
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{userId}")]
        public async Task<ActionResult<Batch>> CreateBatch(string userId, Batch batch)
        {
            _context.Batches.Add(batch);
            _userContext.UserId = userId;
            await _context.SaveChangesAsync();

            return CreatedAtAction("CreateBatch", new { id = batch.BatchId }, batch);
        }

        // PUT: api/Batches/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}/{userId}")]
        public async Task<IActionResult> UpdateBatch(Guid id, string userId, Batch batch)
        {
            if (id != batch.BatchId)
            {
                return BadRequest();
            }

            _context.Entry(batch).State = EntityState.Modified;
            _userContext.UserId = userId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BatchExists(id))
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



        // DELETE: api/Batches/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBatch(Guid id)
        {
            var batch = await _context.Batches.FindAsync(id);
            if (batch == null)
            {
                return NotFound();
            }

            _context.Batches.Remove(batch);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BatchExists(Guid id)
        {
            return _context.Batches.Any(e => e.BatchId == id);
        }
    }
}
