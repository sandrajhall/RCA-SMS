using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IStudyReportHeaderData
    {
        Task<IEnumerable<StudyReportHeader>> ListStudyReportHeadersAsync();

        Task<IEnumerable<StudyReportHeader>> ListStudyReportHeadersByStudyIdAsync(Guid studyId);

        Task<StudyReportHeader> GetStudyReportHeaderAsync(Guid id);

        Task<StudyReportHeader> CreateStudyReportHeaderAsync(string userId, StudyReportHeader studyReportHeader);

        Task UpdateStudyReportHeaderAsync(Guid id, string userId, StudyReportHeader studyReportHeader);

        Task DeleteStudyReportHeaderAsync(Guid id);
    }
}
