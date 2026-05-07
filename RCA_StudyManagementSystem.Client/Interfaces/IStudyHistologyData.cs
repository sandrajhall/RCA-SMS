using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IStudyHistologyData
    {
        Task<IEnumerable<StudyHistologyView>> ListStudyHistologiesAsync();

        Task<IEnumerable<StudyHistologyView>> ListStudyHistologiesAllAsync();

        Task<IEnumerable<StudyHistologyView>> ListStudyHistologiesByStudyIdAsync(Guid id);

        Task<IEnumerable<string>> ListOptionsAsync(string type, Guid studyId);

        Task<StudyHistology> GetStudyHistologyAsync(Guid id);

        Task<Histology> GetValueByOldCodeAsync(Guid studyId, int oldCode);

        Task<Guid> CreateStudyHistologyAsync(string userId, StudyHistology studyHistology);

        Task UpdateStudyHistologyAsync(Guid id, string userId, StudyHistology studyHistology);

        Task DeleteStudyHistologyAsync(Guid id);
    }
}
