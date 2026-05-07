using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
        public interface IDoctorData
        {
            Task<IEnumerable<Doctor>> ListDoctorsAsync(CancellationToken token);

            Task<IEnumerable<Doctor>> ListPathologistsAsync(CancellationToken token);

            Task<IEnumerable<Doctor>> ListAllDoctorsAsync();

            Task<Doctor> GetDoctorAsync(Guid id);

            Task<Doctor> GetDoctorByMigratedIdAsync(string migratedId);

            Task<IEnumerable<Doctor>> GetDoctorsAsync(string searchTerm);

            Task<IEnumerable<Doctor>> GetPathologistsAsync(string searchTerm);

            Task<List<Doctor>> GetDoctorHistoryAsync(Guid id);

            Task<Guid> CreateDoctorAsync(string userId, Doctor doctor);

            Task UpdateDoctorAsync(Guid id, string userId, Doctor doctor);

            Task DeleteDoctorAsync(Guid id);
        }
}
