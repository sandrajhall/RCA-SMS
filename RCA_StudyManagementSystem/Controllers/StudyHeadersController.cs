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
    public class StudyHeadersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserContext _userContext;


        public StudyHeadersController(ApplicationDbContext context, UserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        // GET: api/StudyHeaders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudyHeader>>> ListStudyHeaders()
        {
            return await _context.StudyHeaders.ToListAsync();
        }

        // GET: api/StudyHeaders/study/{studyId}
        [HttpGet("study/{studyid}")]
        public async Task<ActionResult<IEnumerable<StudyHeader>>> ListStudyHeadersByStudyId(Guid studyid)
        {
            return await _context.StudyHeaders
                .Where(sh => sh.StudyId == studyid)
                .ToListAsync();
        }

        // GET: api/StudyHeaders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StudyHeader>> GetStudyHeader(Guid id)
        {
            var studyHeader = await _context.StudyHeaders.FindAsync(id);

            if (studyHeader == null)
            {
                return NotFound();
            }

            return studyHeader;
        }

        // POST: api/StudyHeaders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{userId}")]
        public async Task<ActionResult<StudyHeader>> CreateStudyHeader(string userId, StudyHeader studyHeader)
        {
            _userContext.UserId = userId;
            _context.StudyHeaders.Add(studyHeader);
            await _context.SaveChangesAsync();

            return CreatedAtAction("CreateStudyHeader", new { id = studyHeader.StudyHeaderId }, studyHeader);
        }

        // PUT: api/StudyHeaders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}/{userId}")]
        public async Task<IActionResult> UpdateStudyHeader(Guid id, string userId, StudyHeader studyHeader)
        {
            _userContext.UserId = userId;

            if (id != studyHeader.StudyHeaderId)
            {
                return BadRequest();
            }

            _context.Entry(studyHeader).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudyHeaderExists(id))
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

        // DELETE: api/StudyHeaders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudyHeader(Guid id)
        {
            var studyHeader = await _context.StudyHeaders.FindAsync(id);
            if (studyHeader == null)
            {
                return NotFound();
            }

            _context.StudyHeaders.Remove(studyHeader);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StudyHeaderExists(Guid id)
        {
            return _context.StudyHeaders.Any(e => e.StudyHeaderId == id);
        }
    }
}
