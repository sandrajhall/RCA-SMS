using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IPathReportData
    {
        Task<PathReport> GetPathReportAsync(Guid id);

        Task<Guid> CreatePathReportAsync(string userId, PathReport pathReport);

        Task UpdatePathReportAsync(Guid id, string userId, PathReport pathReport);

        Task UpdatePathReportExportStatusAsync(Guid id, PathReport pathReport);

        Task<IEnumerable<PathReportView>> ListPathReportsAsync(string limit);

        Task<string> CheckPathReportNumberAsync(string pathNo);

        Task<IEnumerable<PathReportView>> ListArchivedPathReportsAsync();

        Task<IEnumerable<PathReportView>> ListPathReportsByBatchAsync(string batchNumber);

        Task<IEnumerable<PathReportView>> ListPathReportsByStudyAsync(Guid studyId);

        Task<IEnumerable<PathReportView>> ListPathReportsByStudyForExportAsync(Guid studyId);

        Task<IEnumerable<PathReportView>> ListPathReportsByStudyExportedAsync(Guid studyId);

        Task<IEnumerable<StudyHeader>> GetPathReportHeaderOptionsAsync();

        Task<string> ExportPathReportDataAsync(Guid studyId, string? exportType, Guid? batchId, string pathIds, bool isReport);

        Task<IEnumerable<ExportView>> GetPathReportExportHistoryAsync(Guid id);

        Task DeletePlaceholderPathReportsAsync();
    }
}
