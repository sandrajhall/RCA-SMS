using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RCA_StudyManagementSystem.Data;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DailyPathSubmissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DailyPathSubmissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/DailyPathSubmissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DailyPathSubmission>>> GetDailyPathSubmissions()
        {
            return await _context.DailyPathSubmissions.ToListAsync();
        }

        // GET: api/DailyPathSubmissions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DailyPathSubmission>> GetDailyPathSubmission(Guid id)
        {
            var dailyPathSubmission = await _context.DailyPathSubmissions.FindAsync(id);

            if (dailyPathSubmission == null)
            {
                return NotFound();
            }

            return dailyPathSubmission;
        }
        [HttpGet("monthly/{year}/{month}/{studyId}")]
        public async Task<List<MonthlyPathSubmissionView>> GetMonthlySubmissions(int year, int month, Guid studyId)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var hospitals = await _context.Hospitals
                .Include(h => h.DailyPathSubmissions
                    .Where(s => s.Date >= startDate && s.Date <= endDate && s.StudyId == studyId))
                .AsNoTracking()
                .ToListAsync();

            var monthlyList = hospitals.Select(h => new MonthlyPathSubmissionView
            {
                HospitalId = h.HospitalId,
                HospitalName = h.HospitalName,
                HospitalShortName = h.HospitalShortName,
                StudyId = studyId,
                // Map the existing DB records to the dictionary
                DailyValues = h.DailyPathSubmissions.ToDictionary(
                    s => s.Date.Day,
                    s => s.Value
                )
            }).ToList();

            return monthlyList;
        }


        // POST: api/DailyPathSubmissions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DailyPathSubmission>> CreateDailyPathSubmission(DailyPathSubmission dailyPathSubmission)
        {
            _context.DailyPathSubmissions.Add(dailyPathSubmission);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDailyPathSubmission", new { id = dailyPathSubmission.DailyPathSubmissionId }, dailyPathSubmission);
        }

        [HttpPost("daily")]
        public async Task SaveDailyValue(DailyPathSubmission dailyPathSubmission)
        {
            // 1. Try to find the existing entry for this specific day/hospital/study
            var entry = await _context.DailyPathSubmissions
                .FirstOrDefaultAsync(x => x.HospitalId == dailyPathSubmission.HospitalId
                                       && x.StudyId == dailyPathSubmission.StudyId
                                       && x.Date.Date == dailyPathSubmission.Date.Date);

            if (entry != null)
            {
                // Update existing
                entry.Value = dailyPathSubmission.Value;
                entry.ModifiedDate = DateTime.UtcNow; // If you add audit fields to this entity
            }
            else
            {
                // Create new
                _context.DailyPathSubmissions.Add(new DailyPathSubmission
                {
                    DailyPathSubmissionId = Guid.NewGuid(),
                    HospitalId = dailyPathSubmission.HospitalId,
                    StudyId = dailyPathSubmission.StudyId,
                    Date = dailyPathSubmission.Date.Date,
                    Value = dailyPathSubmission.Value
                });
            }

            await _context.SaveChangesAsync();
        }

        // PUT: api/DailyPathSubmissions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDailyPathSubmission(Guid id, DailyPathSubmission dailyPathSubmission)
        {
            if (id != dailyPathSubmission.DailyPathSubmissionId)
            {
                return BadRequest();
            }

            _context.Entry(dailyPathSubmission).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DailyPathSubmissionExists(id))
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


        // DELETE: api/DailyPathSubmissions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDailyPathSubmission(Guid id)
        {
            var dailyPathSubmission = await _context.DailyPathSubmissions.FindAsync(id);
            if (dailyPathSubmission == null)
            {
                return NotFound();
            }

            _context.DailyPathSubmissions.Remove(dailyPathSubmission);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DailyPathSubmissionExists(Guid id)
        {
            return _context.DailyPathSubmissions.Any(e => e.DailyPathSubmissionId == id);
        }
    }
}
