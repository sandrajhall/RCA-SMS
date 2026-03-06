using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class PathReportExportData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public PathReportExportData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }

        public async Task<IEnumerable<PathReportExport>> ListPathReportExportsAsync(CancellationToken token)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<PathReportExport>>(_navigationManager.ToAbsoluteUri($"api/pathreportexports"));
            return response;
        }

        public async Task<PathReportExport> GetPathReportExportAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<PathReportExport>(_navigationManager.ToAbsoluteUri($"api/pathreportexports/{id}"));
        }

        public async Task<Guid> CreatePathReportExportAsync(PathReportExport batch)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/pathreportexports"), batch);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["pathReportExportId"];

            return id;
        }
        public async Task UpdatePathReportExportAsync(Guid id, PathReportExport pathReportExport)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/pathreportexports/{id}"), pathReportExport);
        }

        public async Task DeletePathReportExportAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/pathreportexports/{id}"));
        }
    }
}
