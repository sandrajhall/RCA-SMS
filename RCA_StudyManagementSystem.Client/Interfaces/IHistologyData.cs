using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IHistologyData
    {
        Task<IEnumerable<Histology>> ListHistologiesAsync();

        Task<IEnumerable<Histology>> ListActiveHistologiesAsync();

        Task<Histology> GetHistologyAsync(Guid id);

        Task<Guid> CreateHistologyAsync(string userId, Histology histology);

        Task UpdateHistologyAsync(Guid id, string userId, Histology histology);

        Task DeleteHistologyAsync(Guid id);
    }
}
