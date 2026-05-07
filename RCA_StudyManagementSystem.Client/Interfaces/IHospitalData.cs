using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IHospitalData
    {
        Task<IEnumerable<Hospital>> ListHospitalsAsync(CancellationToken token);

        Task<IEnumerable<Hospital>> ListAllHospitalsAsync(CancellationToken token);

        Task<IEnumerable<Hospital>> ListHospitalsForReimbursementEntityAsync(Guid reimbursementEntityId);

        Task<Hospital> GetHospitalAsync(Guid id);

        Task<Hospital> GetHospitalByMigratedIdAsync(string migratedId);

        Task<IEnumerable<Hospital>> GetHospitalsAsync(string searchTerm);

        Task<string> GetHospShortNameAsync(string hospName);

        Task<List<Hospital>> GetHospitalHistoryAsync(Guid id);

        Task<Guid> CreateHospitalAsync(string userId, Hospital hospital);

        Task UpdateHospitalAsync(Guid id, string userId, Hospital hospital);

        Task DeleteHospitalAsync(Guid id);

        Task<Guid> GetHospitalIdAsync(string? hospitalName);
    }
}