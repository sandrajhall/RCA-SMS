using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RCA_StudyManagementSystem.Api.Data;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Client.Utilities;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static MudBlazor.CategoryTypes;

namespace RCA_StudyManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;


        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InvoicesController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: api/Invoices/notsent
        [HttpGet("notsent")]
        public async Task<ActionResult<IEnumerable<Invoice>>> ListInvoicesNotSent()
        {
            var invoices = new List<Invoice>();

            invoices = await _context.Invoices
                .Where(i => i.DateEmailed == null)
                .ToListAsync();

            if (invoices.Count > 0)
            {
                foreach (var invoice in invoices)
                {
                    invoice.InvoiceItems = await _context.InvoiceItems
                        .Where(ii => ii.InvoiceId == invoice.InvoiceId)
                        .ToListAsync();

                    invoice.ReimbursementEntity = await _context.ReimbursementEntities
                        .FirstOrDefaultAsync(re => re.ReimbursementEntityId == invoice.ReimbursementEntityId);
                }
            }

            return invoices;
        }

        // GET: api/Invoices/sent
        [HttpGet("sent")]
        public async Task<ActionResult<IEnumerable<Invoice>>> ListInvoicesSent()
        {
            var invoices = new List<Invoice>();

            invoices = await _context.Invoices
                .Where(i => i.DateEmailed != null)
                .ToListAsync();

            if (invoices.Count > 0)
            {
                foreach (var invoice in invoices)
                {
                    invoice.InvoiceItems = await _context.InvoiceItems
                        .Where(ii => ii.InvoiceId == invoice.InvoiceId)
                        .ToListAsync();

                    invoice.ReimbursementEntity = await _context.ReimbursementEntities
                        .FirstOrDefaultAsync(re => re.ReimbursementEntityId == invoice.ReimbursementEntityId);
                }
            }

            return invoices;
        }

        // GET: api/Invoices/lastquarter
        [HttpGet("lastquarter")]
        public async Task<ActionResult<int>> GetLastQuarter()
        {
            var quarter = 0;

            var invoice = await _context.Invoices
                .OrderByDescending(i => i.InvoiceQuarter)
                .FirstOrDefaultAsync();

            if (invoice != null)
            {
                quarter = Int32.Parse(invoice.InvoiceQuarter.Split("Quarter ")[1]);
            }

            return quarter;
        }

        // GET: api/Invoices/generate/{startDate}/{endDate}/{quarter}
        [HttpGet("generate/{startDate}/{endDate}/{quarter}")]
        public async Task<ActionResult<IEnumerable<Invoice>>> GenerateInvoices(string startDate, string endDate, int quarter)
        {
            DateTime startDateDate = DateTime.Parse(startDate);
            DateTime endDateDate = DateTime.Parse(endDate);
            decimal reimbAmount = 15.00M;

            var invoices = new List<Invoice>();
            var invoiceItems = new List<InvoiceItem>();
            var reimbPathReports = new List<PathReportReimbView>();

            var pathReports = await _context.PathReports
                .Include(p => p.Patient)
                .ThenInclude(pt => pt.Study)
                .Where(pr => pr.DoNotInvoice == false &&
                             pr.RcaExportDate >= startDateDate &&
                                pr.RcaExportDate <= endDateDate)
                .ToListAsync();

            var pathReportsWithTwoReimb = await _context.PathReports
                .Include(p => p.Patient)
                .ThenInclude(pt => pt.Study)
                .Where(pr => pr.DoNotInvoice == false &&
                             pr.RcaExportDate >= startDateDate &&
                                pr.RcaExportDate <= endDateDate &&
                                pr.Reimbursement2.Length > 1)
                .ToListAsync();


            // Get the current request's details
            var request = _httpContextAccessor.HttpContext.Request;

            // Construct the base URL dynamically
            var baseUrl = $"{request.Scheme}://{request.Host}";

            // Create an HttpClient instance
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(baseUrl);

            foreach (var pr in pathReports)
            {
                pr.Reimbursement1 = pr.Reimbursement1.Trim();
                var pathReportReimb = new PathReportReimbView
                {
                    PathReportId = pr.PathReportId,
                    RcaExportDate = pr.RcaExportDate,
                    ReimbursementHospitalName = pr.Reimbursement1,
                    ReimbursementHospitalId = await httpClient.GetFromJsonAsync<Guid>($"api/hospitals/id/{pr.Reimbursement1}"),
                    StudyId = pr.Patient.StudyId,
                    StudyDesignation = pr.Patient.Study.InvoiceDesignation,
                    ReimbursementEntityId = (Guid)await httpClient.GetFromJsonAsync<Hospital>($"api/hospitals/{pr.HospitalId}").ContinueWith(h => h.Result.ReimbursementEntityId),
                };

                reimbPathReports.Add(pathReportReimb);
            }

            foreach (var pr in pathReportsWithTwoReimb)
            {
                var pathReportReimb = new PathReportReimbView
                {
                    PathReportId = pr.PathReportId,
                    RcaExportDate = pr.RcaExportDate,
                    ReimbursementHospitalName = pr.Reimbursement2,
                    ReimbursementHospitalId = await httpClient.GetFromJsonAsync<Guid>($"api/hospitals/id/{pr.Reimbursement2}"),
                    StudyId = pr.Patient.StudyId,
                    StudyDesignation = pr.Patient.Study.InvoiceDesignation,
                    ReimbursementEntityId = (Guid)await httpClient.GetFromJsonAsync<Hospital>($"api/hospitals/{pr.HospitalId}").ContinueWith(h => h.Result.ReimbursementEntityId),
                };
                reimbPathReports.Add(pathReportReimb);
            }

            foreach (var prr in reimbPathReports)
            {
                prr.ReimbursementEntityName = await httpClient.GetFromJsonAsync<ReimbursementEntity>($"api/reimbursemententities/{prr.ReimbursementEntityId}").ContinueWith(re => re.Result.Name);
            }

            var groupedByReimbEntity = reimbPathReports
                .GroupBy(rpr => rpr.ReimbursementEntityId)
                .ToList();

            foreach (var group in groupedByReimbEntity)
            {
                var reimbEntityPrefix = await httpClient.GetFromJsonAsync<ReimbursementEntity>($"api/reimbursemententities/{group.Key}").ContinueWith(re => re.Result.InvoicePrefix);
                var runningTotal = 0.00M;

                var invoice = new Invoice
                {
                    InvoiceId = Guid.NewGuid(),
                    InvoiceNumber = reimbEntityPrefix + "-" + DateTime.Now.ToString("yyyy-MM-dd"),
                    ReimbursementEntityId = group.Key,
                    InvoiceDate = DateTime.Now,
                    InvoiceQuarter = endDateDate.Year + " Quarter " + quarter,
                    InvoiceItems = new List<InvoiceItem>()
                };
                foreach (var prr in group)
                {
                    var groupedByStudy = group
                        .Where(g => g.StudyId == prr.StudyId)
                        .ToList();

                    if (!invoice.InvoiceItems.Any(ii => ii.StudyId == prr.StudyId))
                    {
                        var invoiceItem = new InvoiceItem
                        {
                            InvoiceItemId = Guid.NewGuid(),
                            InvoiceId = invoice.InvoiceId,
                            StudyId = prr.StudyId,
                            HospitalId = prr.ReimbursementHospitalId,
                            NumPathReports = groupedByStudy.Count,
                        };
                        invoice.InvoiceItems.Add(invoiceItem);
                        runningTotal = (decimal)(runningTotal + (invoiceItem.NumPathReports * reimbAmount));
                    }
                }
                invoice.TotalAmount = runningTotal;
                invoices.Add(invoice);
            }
            // Check invoices to see if they are equal to or over $5000, if so, split into multiple invoices
            var invoicesToAdd = new List<Invoice>();
            foreach (var inv in invoices)
            {
                if (inv.TotalAmount >= 5000.00M)
                {
                    var currentInvoice = new Invoice
                    {
                        InvoiceId = Guid.NewGuid(),
                        InvoiceNumber = inv.InvoiceNumber + "-1",
                        ReimbursementEntityId = inv.ReimbursementEntityId,
                        InvoiceDate = inv.InvoiceDate,
                        InvoiceQuarter = inv.InvoiceQuarter,
                        InvoiceItems = new List<InvoiceItem>()
                    };
                    decimal runningTotal = 0.00M;
                    int invoiceCount = 1;
                    foreach (var item in inv.InvoiceItems)
                    {
                        decimal itemTotal = (decimal)(item.NumPathReports * reimbAmount);
                        if ((runningTotal + itemTotal) >= 5000.00M)
                        {
                            // Save current invoice and start a new one
                            currentInvoice.TotalAmount = runningTotal;
                            invoicesToAdd.Add(currentInvoice);
                            invoiceCount++;
                            currentInvoice = new Invoice
                            {
                                InvoiceId = Guid.NewGuid(),
                                InvoiceNumber = inv.InvoiceNumber + "-" + invoiceCount,
                                ReimbursementEntityId = inv.ReimbursementEntityId,
                                InvoiceDate = inv.InvoiceDate,
                                InvoiceQuarter = inv.InvoiceQuarter,
                                InvoiceItems = new List<InvoiceItem>()
                            };
                            runningTotal = 0.00M;
                        }
                        currentInvoice.InvoiceItems.Add(item);
                        runningTotal += itemTotal;
                    }
                    // Add the last invoice
                    currentInvoice.TotalAmount = runningTotal;
                    invoicesToAdd.Add(currentInvoice);
                }
                else
                {
                    invoicesToAdd.Add(inv);
                }
            }



            _context.Invoices.AddRange(invoicesToAdd);
            _context.InvoiceItems.AddRange(invoicesToAdd.SelectMany(i => i.InvoiceItems));
            await _context.SaveChangesAsync();

            return invoicesToAdd;
        }

        // GET: api/Invoices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(Guid id)
        {
            var invoice = await _context.Invoices
                            .Include(i => i.InvoiceItems)
                            .ThenInclude(h => h.Hospital)
                            .Include(i=> i.InvoiceItems)
                            .ThenInclude(s => s.Study)
                            .Include(i => i.ReimbursementEntity)
                            .ThenInclude(re => re.ReimbursementEntityRCAContacts)
                            .ThenInclude(rerc => rerc.RCAContact)
                            .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
            {
                return NotFound();
            }

            return invoice;
        }

        // POST: api/Invoices
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Invoice>> CreateInvoice(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInvoice", new { id = invoice.InvoiceId }, invoice);
        }

        // PUT: api/Invoices/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(Guid id, Invoice invoice)
        {
            if (id != invoice.InvoiceId)
            {
                return BadRequest();
            }

            _context.Entry(invoice).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(id))
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



        // DELETE: api/Invoices/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(Guid id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
            {
                return NotFound();
            }

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceExists(Guid id)
        {
            return _context.Invoices.Any(e => e.InvoiceId == id);
        }
    }
}
