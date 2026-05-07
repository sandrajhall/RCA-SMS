using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IPatientStatusData
    {
        Task<IEnumerable<PatientStatus>> ListPatientStatusAsync(CancellationToken token);

        Task<PatientStatus> GetPatientStatusAsync(Guid id);

        Task<PatientStatus> GetPatientStatusByCaseNumberAsync(string caseNumber);

        Task<List<PatientStatusView>> ListPatientStatusesByStudyIdAsync(Guid studyId, string startDate, string endDate);

        Task<string> ListPatientStatusesByStudyIdCSVAsync(Guid studyId, string startDate, string endDate);

        Task<Guid> CreatePatientStatusAsync(string userId, PatientStatus patientStatus);

        Task UpdatePatientStatusAsync(Guid id, string userId, PatientStatus patientStatus);

        Task DeletePatientStatusAsync(Guid id);
    }
}