using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IInvoiceData
    {
        Task<IEnumerable<Invoice>> ListInvoicesNotSentAsync(CancellationToken token);

        Task<IEnumerable<Invoice>> ListInvoicesSentAsync(CancellationToken token);

        Task<IEnumerable<Invoice>> GenerateInvoicesAsync(string startDate, string endDate, int quarter);

        Task<int> GetLastQuarterAsync(CancellationToken token);

        Task<Invoice> GetInvoiceAsync(Guid id);

        Task<Guid> CreateInvoiceAsync(string userId, Invoice invoice);

        Task UpdateInvoiceAsync(Guid id, string userId, Invoice invoice);

        Task DeleteInvoiceAsync(Guid id);
    }
}
