using RCA_StudyManagementSystem.Shared.ViewModels;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IReportData
    {
        Task<IEnumerable<PathCountByStudyByDate>> GetPathsByStudyByDateAsync(Guid studyId, string startDate, string endDate);

        Task<string> GetPathsByStudyByDateCSVAsync(Guid studyId, string startDate, string endDate);

        Task<IEnumerable<PathCountByStudyByDate>> GetCECSPathsByDateAsync(string startDate, string endDate);

        Task<int> GetCESCCasesByDateAsync(string startDate, string endDate);

        Task<IEnumerable<RaceCountByDate>> GetCECSRaceCountByDateAsync(string startDate, string endDate);

        Task<IEnumerable<EthnicityCountByDate>> GetCECSEthnicityCountByDateAsync(string startDate, string endDate);
    }
}
