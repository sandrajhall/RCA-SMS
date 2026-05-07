using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IStudyData
    {
        Task<IEnumerable<Study>> ListStudiesAsync();

        Task<IEnumerable<Study>> ListArchivedStudiesAsync();

        Task<IEnumerable<Study>> ListUnarchivedStudiesAsync();

        Task<Study> GetStudyAsync(Guid id);

        Task<Study> GetStudyInfoAsync(Guid id);

        Task<string> GetStudyNameAsync(Guid id);

        Task<string> GetStudyColorAsync(Guid id);

        Task<Guid> CreateStudyAsync(string userId, Study study);

        Task UpdateStudyAsync(Guid id, string userId, Study study);

        Task DeleteStudyAsync(Guid id);

        Task ArchiveStudyAsync(Guid id);
    }
}
