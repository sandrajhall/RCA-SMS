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
    public class PathReportExportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PathReportExportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PathReportExports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PathReportExport>>> ListPathReportExports()
        {
            return await _context.PathReportExports.ToListAsync();
        }

        // GET: api/PathReportExports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PathReportExport>> GetPathReportExport(Guid id)
        {
            var pathReportExport = await _context.PathReportExports.FindAsync(id);

            if (pathReportExport == null)
            {
                return NotFound();
            }

            return pathReportExport;
        }


        // POST: api/PathReportExports
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PathReportExport>> CreatePathReportExport(PathReportExport pathReportExport)
        {
            _context.PathReportExports.Add(pathReportExport);
            await _context.SaveChangesAsync();

            return CreatedAtAction("CreatePathReportExport", new { id = pathReportExport.PathReportExportId }, pathReportExport);
        }

        // PUT: api/PathReportExports/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePathReportExport(Guid id, PathReportExport pathReportExport)
        {
            if (id != pathReportExport.PathReportExportId)
            {
                return BadRequest();
            }

            _context.Entry(pathReportExport).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PathReportExportExists(id))
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


        // DELETE: api/PathReportExports/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePathReportExport(Guid id)
        {
            var pathReportExport = await _context.PathReportExports.FindAsync(id);
            if (pathReportExport == null)
            {
                return NotFound();
            }

            _context.PathReportExports.Remove(pathReportExport);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PathReportExportExists(Guid id)
        {
            return _context.PathReportExports.Any(e => e.PathReportExportId == id);
        }
    }
}
