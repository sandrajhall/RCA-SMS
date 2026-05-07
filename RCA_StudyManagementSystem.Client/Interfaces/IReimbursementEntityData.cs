using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IReimbursementEntityData
    {
        Task<IEnumerable<ReimbursementEntity>> ListReimbursementEntitiesAsync(CancellationToken token);

        Task<ReimbursementEntity> GetReimbursementEntityAsync(Guid id);

        Task<IEnumerable<ReimbursementEntity>> GetReimbursementEntitiesAsync(string searchTerm);

        Task<Guid> CreateReimbursementEntityAsync(string userId, ReimbursementEntity reimbEntity);

        Task UpdateReimbursementEntityAsync(Guid id, string userId, ReimbursementEntity reimbEntity);

        Task DeleteReimbursementEntityAsync(Guid id);
    }
}
