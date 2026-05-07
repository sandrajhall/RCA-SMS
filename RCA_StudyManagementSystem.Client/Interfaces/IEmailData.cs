using RCA_StudyManagementSystem.Shared.DTOs;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IEmailData
    {
        Task<bool> SendAsync(EmailRequest request);

        Task<bool> SendFromInvoiceTemplateAsync(InvoiceEmailData request);
    }
}
