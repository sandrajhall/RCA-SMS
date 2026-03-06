using Microsoft.AspNetCore.Mvc;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Services;
using RCA_StudyManagementSystem.Shared.DTOs;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailSender _emailSender;

    public EmailController(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    [HttpPost]
    public async Task<IActionResult> Send(EmailRequest request)
    {
        await _emailSender.SendEmailAsync(request.To, request.Subject, request.Body);
        return Ok();
    }

    [HttpPost("invoicetemplate")]
    public async Task<IActionResult> SendFromInvoiceTemplate(InvoiceEmailData request)
    {
        var toEmail = request.ContactEmail;
        await _emailSender.SendEmailFromInvoiceTemplateAsync(toEmail, "RCA Invoice - " + request.ReimbursementEntity, request);
        return Ok();
    }
}


