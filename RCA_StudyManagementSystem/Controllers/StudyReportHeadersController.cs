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
    public class StudyReportHeadersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudyReportHeadersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/StudyReportHeaders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudyReportHeader>>> ListStudyReportHeaders()
        {
            return await _context.StudyReportHeaders.ToListAsync();
        }

        // GET: api/StudyReportHeaders/study/{studyId}
        [HttpGet("study/{studyid}")]
        public async Task<ActionResult<IEnumerable<StudyReportHeader>>> ListStudyReportHeadersByStudyId(Guid studyid)
        {
            return await _context.StudyReportHeaders
                .Where(sh => sh.StudyId == studyid)
                .ToListAsync();
        }

        // GET: api/StudyReportHeaders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StudyReportHeader>> GetStudyReportHeader(Guid id)
        {
            var studyReportHeader = await _context.StudyReportHeaders.FindAsync(id);

            if (studyReportHeader == null)
            {
                return NotFound();
            }

            return studyReportHeader;
        }

        // POST: api/StudyReportHeaders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<StudyReportHeader>> CreateStudyReportHeader(StudyReportHeader studyReportHeader)
        {
            _context.StudyReportHeaders.Add(studyReportHeader);
            await _context.SaveChangesAsync();

            return CreatedAtAction("CreateStudyReportHeader", new { id = studyReportHeader.StudyReportHeaderId }, studyReportHeader);
        }

        // PUT: api/StudyReportHeaders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudyReportHeader(Guid id, StudyReportHeader studyReportHeader)
        {
            if (id != studyReportHeader.StudyReportHeaderId)
            {
                return BadRequest();
            }

            _context.Entry(studyReportHeader).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudyReportHeaderExists(id))
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

        // DELETE: api/StudyReportHeaders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudyReportHeader(Guid id)
        {
            var studyReportHeader = await _context.StudyReportHeaders.FindAsync(id);
            if (studyReportHeader == null)
            {
                return NotFound();
            }

            _context.StudyReportHeaders.Remove(studyReportHeader);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StudyReportHeaderExists(Guid id)
        {
            return _context.StudyReportHeaders.Any(e => e.StudyReportHeaderId == id);
        }
    }
}
