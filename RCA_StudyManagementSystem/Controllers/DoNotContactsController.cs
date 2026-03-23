using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class DoNotContactsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserContext _userContext;


        public DoNotContactsController(ApplicationDbContext context, UserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        // GET: api/DoNotContacts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoNotContact>>> ListDoNotContacts()
        {
            return await _context.DoNotContacts
                .OrderBy(d => d.LastName)
                .ToListAsync();
        }


        // GET: api/DoNotContacts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DoNotContact>> GetDoNotContact(Guid id)
        {
            var donotcontact = await _context.DoNotContacts.FindAsync(id);

            if (donotcontact == null)
            {
                return NotFound();
            }

            return donotcontact;
        }



        // POST: api/DoNotContacts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{userId}")]
        public async Task<ActionResult<DoNotContact>> CreateDoNotContact(string userId, DoNotContact doNotContact)
        {
            _context.DoNotContacts.Add(doNotContact);
            _userContext.UserId = userId;
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDoNotContact", new { id = doNotContact.DoNotContactId }, doNotContact);
        }

        // PUT: api/DoNotContacts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}/{userId}")]
        public async Task<IActionResult> UpdateDoNotContact(Guid id, string userId, DoNotContact doNotContact)
        {
            if (id != doNotContact.DoNotContactId)
            {
                return BadRequest();
            }
            _userContext.UserId = userId;

            _context.Entry(doNotContact).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DoNotContactExists(id))
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


        // DELETE: api/DoNotContacts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoNotContact(Guid id)
        {
            var doNotContact = await _context.DoNotContacts.FindAsync(id);
            if (doNotContact == null)
            {
                return NotFound();
            }

            _context.DoNotContacts.Remove(doNotContact);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DoNotContactExists(Guid id)
        {
            return _context.DoNotContacts.Any(e => e.DoNotContactId == id);
        }
    }
}
