using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RCA_StudyManagementSystem.Data;
using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RCAContactsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RCAContactsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/RCAContacts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RCAContact>>> ListRCAContacts()
        {
            return await _context.RCAContacts
                .Where(h => h.IsActive == true) // Only active RCAContacts
                .OrderBy(h => h.LastName)
                .ToListAsync();
        }


        // GET: api/RCAContacts/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<RCAContact>>> ListAllRCAContacts()
        {
            return await _context.RCAContacts.OrderBy(h => h.LastName).ToListAsync();
        }

        // GET: api/RCAContacts/reimbursemententity/{reimbursementEntityId}
        [HttpGet("reimbursemententity/{reimbursemententityid}")]
        public async Task<ActionResult<IEnumerable<RCAContact>>> ListRCAContactsByReimbursementEntityId(Guid reimbursemententityid)
        {
            return await _context.RCAContacts
                .Include(c => c.ReimbursementEntityRCAContacts)
                .Where(c => c.ReimbursementEntityRCAContacts.Any(re => re.ReimbursementEntityId == reimbursemententityid))
                .OrderBy(c => c.ReimbursementEntityRCAContacts.Any(re => re.IsPrimaryContact) ? 0 : 1) // Primary contacts first
                .ToListAsync();
        }

        // GET: api/RCAContacts/reimbursemententitylist/{rcacontactid}
        [HttpGet("reimbursemententitylist/{rcacontactid}")]
        public async Task<ActionResult<RCAContact>> ListReimbursementEntities(Guid rcacontactid)
        {
            var contacts = await _context.RCAContacts
                        .Include(c => c.ReimbursementEntityRCAContacts)
                        .ThenInclude(re => re.ReimbursementEntity)
                        .ToListAsync();

            var contact = contacts.FirstOrDefault(c => c.RCAContactId == rcacontactid);

            contact.ReimbursementEntities = contact.ReimbursementEntityRCAContacts.Select(re => re.ReimbursementEntity).ToList();

            return contact;
        }

        // GET: api/RCAContacts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RCAContact>> GetRCAContact(Guid id)
        {
            var rcaContact = await _context.RCAContacts.FindAsync(id);

            if (rcaContact == null)
            {
                return NotFound();
            }

            return rcaContact;
        }
        // GET: api/RCAContacts/search/{searchTerm}
        [HttpGet("search/{searchTerm}")]
        public async Task<IEnumerable<RCAContact>> GetRCAContactsAsync(string searchTerm)
        {
            var query = _context.RCAContacts
                .Where(h => h.LastName.Contains(searchTerm) || h.FirstName.Contains(searchTerm) || h.Title.Contains(searchTerm))
                .OrderBy(h => h.LastName)
                .Take(50); // Important: Limit the number of records returned

            return await query.ToListAsync();
        }


        // POST: api/RCAContacts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<RCAContact>> CreateRCAContact(RCAContact rcaContact)
        {
            _context.RCAContacts.Add(rcaContact);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRCAContact", new { id = rcaContact.RCAContactId }, rcaContact);
        }

        // PUT: api/RCAContacts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRCAContact(Guid id, RCAContact rcaContact)
        {
            if (id != rcaContact.RCAContactId)
            {
                return BadRequest();
            }

            _context.Entry(rcaContact).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RCAContactExists(id))
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


        // DELETE: api/RCAContacts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRCAContact(Guid id)
        {
            var rcaContact = await _context.RCAContacts.FindAsync(id);
            if (rcaContact == null)
            {
                return NotFound();
            }

            rcaContact.IsActive = false; // Soft delete by marking as inactive

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RCAContactExists(Guid id)
        {
            return _context.RCAContacts.Any(e => e.RCAContactId == id);
        }
    }
}
