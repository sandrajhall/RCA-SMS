using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IDailyPathSubmissionData
    {
        Task<IEnumerable<DailyPathSubmission>> ListDailyPathSubmissionsAsync(CancellationToken token);

        Task<DailyPathSubmission> GetDailyPathSubmissionAsync(Guid id);

        Task<List<MonthlyPathSubmissionView>> ListMonthlyPathSubmissionAsync(int year, int month, Guid studyId);

        Task<Guid> CreateDailyPathSubmissionAsync(string userId, DailyPathSubmission dailyPathSubmission);

        Task SaveDailyPathSubmissionAsync(string userId, DailyPathSubmission dailyPathSubmission);

        Task UpdateDailyPathSubmissionAsync(Guid id, string userId, DailyPathSubmission dailyPathSubmission);

        Task DeleteDailyPathSubmissionAsync(Guid id);
    }
}
