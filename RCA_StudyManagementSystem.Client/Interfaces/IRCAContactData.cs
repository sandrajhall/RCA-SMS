using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IRCAContactData
    {
        Task<IEnumerable<RCAContact>> ListRCAContactsAsync(CancellationToken token);

        Task<RCAContact> GetRCAContactAsync(Guid id);

        Task<IEnumerable<RCAContact>> GetRCAContactsAsync(string searchTerm);

        Task<IEnumerable<RCAContact>> ListRCAContactsByReimbursementEntityIdAsync(Guid reimbursementEntityId);

        Task<RCAContact> ListReimbursementEntitiesAsync(Guid rcaContactId);

        Task<Guid> CreateRCAContactAsync(string userId, RCAContact rcaContact);

        Task UpdateRCAContactAsync(Guid id, string userId, RCAContact rcaContact);

        Task DeleteRCAContactAsync(Guid id);
    }
}
