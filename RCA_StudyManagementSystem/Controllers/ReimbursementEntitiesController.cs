using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RCA_StudyManagementSystem.Data;
using RCA_StudyManagementSystem.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace RCA_StudyManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReimbursementEntitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReimbursementEntitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ReimbursementEntities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReimbursementEntity>>> ListReimbursementEntities()
        {
            return await _context.ReimbursementEntities
                .Where(r => r.IsActive == true) // Only active ReimbursementEntities
                .OrderBy(r => r.Name)
                .ToListAsync();
        }


        // GET: api/ReimbursementEntities/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<ReimbursementEntity>>> ListAllReimbursementEntities()
        {
            return await _context.ReimbursementEntities.OrderBy(h => h.Name).ToListAsync();
        }

        // GET: api/ReimbursementEntities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReimbursementEntity>> GetReimbursementEntity(Guid id)
        {
            var reimbEntity = await _context.ReimbursementEntities
                .Include(c => c.ReimbursementEntityRCAContacts)
                .Where(r => r.ReimbursementEntityId == id)
                .FirstOrDefaultAsync();

            if (reimbEntity == null)
            {
                return NotFound();
            }

            return reimbEntity;
        }
        // GET: api/ReimbursementEntities/search/{searchTerm}
        [HttpGet("search/{searchTerm}")]
        public async Task<IEnumerable<ReimbursementEntity>> GetReimbursementEntitiesAsync(string searchTerm)
        {
            var query = _context.ReimbursementEntities
                .Where(r => r.Name.Contains(searchTerm) || r.PayableTo.Contains(searchTerm) || r.AttentionTo.Contains(searchTerm))
                .OrderBy(r => r.Name)
                .Take(50); // Important: Limit the number of records returned

            return await query.ToListAsync();
        }



        // POST: api/ReimbursementEntities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ReimbursementEntity>> CreateReimbursementEntity(ReimbursementEntity reimbEntity)
        {
            _context.ReimbursementEntities.Add(reimbEntity);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReimbursementEntity", new { id = reimbEntity.ReimbursementEntityId }, reimbEntity);
        }

        // PUT: api/ReimbursementEntities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReimbursementEntity(Guid id, ReimbursementEntity reimbEntity)
        {
            // Check if the incoming model data meets all validation rules (e.g., [Required], [StringLength])
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingEntity = await _context.ReimbursementEntities
                                 .Include(c => c.ReimbursementEntityRCAContacts)
                                 .AsTracking()
                                 .FirstOrDefaultAsync(s => s.ReimbursementEntityId == id);

            if (id != reimbEntity.ReimbursementEntityId)
            {
                return BadRequest();
            }


            // Update scalar properties
            _context.Entry(existingEntity).CurrentValues.SetValues(reimbEntity);

            // Update StudyContacts
            var incomingContactIds = reimbEntity.ReimbursementEntityRCAContacts.Select(c => c.ReimbursementEntityRCAContactId).ToList();
            foreach (var contact in existingEntity.ReimbursementEntityRCAContacts.ToList())
            {
                if (!incomingContactIds.Contains(contact.ReimbursementEntityRCAContactId))
                    _context.ReimbursementEntityRCAContacts.Remove(contact);
            }

            foreach (var incomingContact in reimbEntity.ReimbursementEntityRCAContacts)
            {
                // Try to find if this Guid already exists in the database's child collection
                var existingContact = existingEntity.ReimbursementEntityRCAContacts
                    .FirstOrDefault(c => c.ReimbursementEntityRCAContactId == incomingContact.ReimbursementEntityRCAContactId);
                if (existingContact == null)
                {
                    // 1. THIS IS A NEW CONTACT
                    // Ensure the Foreign Key is set correctly
                    incomingContact.ReimbursementEntityId = existingEntity.ReimbursementEntityId;

                    // 2. Add it to the collection tracked by the context
                    existingEntity.ReimbursementEntityRCAContacts.Add(incomingContact);

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
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReimbursementEntityExists(id))
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


        // DELETE: api/ReimbursementEntities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReimbursementEntity(Guid id)
        {
            var reimbEntity = await _context.ReimbursementEntities.FindAsync(id);
            if (reimbEntity == null)
            {
                return NotFound();
            }

            reimbEntity.IsActive = false; // Soft delete by marking as inactive

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReimbursementEntityExists(Guid id)
        {
            return _context.ReimbursementEntities.Any(e => e.ReimbursementEntityId == id);
        }
    }
}
