using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
        public interface IDoNotContactData
        {
            Task<IEnumerable<DoNotContact>> ListDoNotContactsAsync(CancellationToken token);

            Task<DoNotContact> GetDoNotContactAsync(Guid id);

            Task<Guid> CreateDoNotContactAsync(string userId, DoNotContact doNotContact);

            Task UpdateDoNotContactAsync(Guid id, string userId, DoNotContact doNotContact);

            Task DeleteDoNotContactAsync(Guid id);
        }
}
