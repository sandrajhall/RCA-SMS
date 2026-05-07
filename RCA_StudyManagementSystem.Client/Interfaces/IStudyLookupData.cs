using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IStudyLookupData
    {
        Task<IEnumerable<StudyLookupView>> ListStudyLookupsAsync();

        Task<IEnumerable<StudyLookupView>> ListStudyLookupsByStudyIdAsync(Guid id);

        Task<IEnumerable<StudyLookupView>> ListStudyLookupsByTypeAsync(string type);

        Task<IEnumerable<StudyLookupView>> ListStudyLookupsAllByTypeAsync(string type);

        Task<IEnumerable<string>> ListOptionsAsync(string type, Guid studyId);

        Task<StudyLookup> GetStudyLookupAsync(Guid id);

        Task<string> GetValueByOldCodeAsync(Guid studyId, string type, int oldCode);

        Task<string> GetCodeByValueAsync(Guid studyId, string type, string value);

        Task<Guid> CreateStudyLookupAsync(string userId, StudyLookup studyLookup);

        Task UpdateStudyLookupAsync(Guid id, string userId, StudyLookup studyLookup);

        Task DeleteStudyLookupAsync(Guid id);
    }
}
