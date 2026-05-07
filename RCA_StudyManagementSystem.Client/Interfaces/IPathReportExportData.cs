using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IPathReportExportData
    {
        Task<IEnumerable<PathReportExport>> ListPathReportExportsAsync(CancellationToken token);

        Task<PathReportExport> GetPathReportExportAsync(Guid id);

        Task<Guid> CreatePathReportExportAsync(string userId, PathReportExport batch);

        Task UpdatePathReportExportAsync(Guid id, string userId, PathReportExport pathReportExport);

        Task DeletePathReportExportAsync(Guid id);
    }
}
