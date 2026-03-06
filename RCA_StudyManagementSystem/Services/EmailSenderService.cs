using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using RCA_StudyManagementSystem.Client.Pages.Invoices;
using RCA_StudyManagementSystem.Shared.DTOs;

namespace RCA_StudyManagementSystem.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
        Task SendEmailFromInvoiceTemplateAsync(string toEmail, string subject, InvoiceEmailData data);

    }

    public class EmailSender : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        private readonly IHtmlRenderingService _htmlRenderingService;

        public EmailSender(IOptions<MailSettings> mailSettings, IHtmlRenderingService htmlRenderingService)
        {
            _mailSettings = mailSettings.Value;
            _htmlRenderingService = htmlRenderingService;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailSettings.FromName, _mailSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = message };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port);
                //await smtp.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }

        public async Task SendEmailFromInvoiceTemplateAsync(string toEmail, string subject, InvoiceEmailData data)
        {
            var parameters = new Dictionary<string, object>
        {
            { "Data", data }
        };

            // 2. Render the Razor component to an HTML string
            var emailBodyHtml = await _htmlRenderingService.RenderComponentToHtml<EmailTemplate>(parameters);

            // 3. Create the MimeMessage with the rendered HTML
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailSettings.FromName, _mailSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = emailBodyHtml };
            email.Body = bodyBuilder.ToMessageBody();

            // 4. Send the email using MailKit
            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port);
                //await smtp.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);
                await smtp.SendAsync(email);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
