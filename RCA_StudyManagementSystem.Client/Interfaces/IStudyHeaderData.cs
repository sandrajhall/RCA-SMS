using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IStudyHeaderData
    {
        Task<IEnumerable<StudyHeader>> ListStudyHeadersAsync();

        Task<IEnumerable<StudyHeader>> ListStudyHeadersByStudyIdAsync(Guid studyId);

        Task<StudyHeader> GetStudyHeaderAsync(Guid id);

        Task<StudyHeader> CreateStudyHeaderAsync(string userId, StudyHeader studyHeader);

        Task UpdateStudyHeaderAsync(Guid id, string userId, StudyHeader studyHeader);

        Task DeleteStudyHeaderAsync(Guid id);
    }
}
