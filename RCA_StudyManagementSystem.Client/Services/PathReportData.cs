using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.DTOs;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class PathReportData
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;

        public PathReportData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
        }

        public async Task<PathReport> GetPathReportAsync(Guid id)
        {
            var response = await _httpClient.GetFromJsonAsync<PathReport>(_navigationManager.ToAbsoluteUri($"api/pathreports/{id}"));
            return response;
        }

        public async Task<Guid> CreatePathReportAsync(PathReport pathReport)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/pathreports"), pathReport);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["pathReportId"];

            return id;
        }

        public async Task UpdatePathReportAsync(Guid id, PathReport pathReport)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/pathreports/{id}"), pathReport);
        }

        public async Task UpdatePathReportExportStatusAsync(Guid id, PathReport pathReport)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/pathreports/exportstatus/{id}"), pathReport);
        }

        public async Task<IEnumerable<PathReportView>> ListPathReportsAsync(string limit)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<PathReportView>>(_navigationManager.ToAbsoluteUri($"api/pathreports/limit/{limit}"));
            return response;
        }

        public async Task<string> CheckPathReportNumberAsync(string pathNo)
        {
            var response = await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/pathreports/checkpathreportnumber/{pathNo}"));
            return response;
        }
        public async Task<IEnumerable<PathReportView>> ListArchivedPathReportsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<PathReportView>>(_navigationManager.ToAbsoluteUri($"api/pathreports/archived"));
            return response;
        }

        public async Task<IEnumerable<PathReportView>> ListPathReportsByBatchAsync(string batchNumber)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<PathReportView>>(_navigationManager.ToAbsoluteUri($"api/pathreports/batch/{batchNumber}"));
            return response;
        }

        public async Task<IEnumerable<PathReportView>> ListPathReportsByStudyAsync(Guid studyId)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<PathReportView>>(_navigationManager.ToAbsoluteUri($"api/pathreports/study/{studyId}"));
            return response;
        }

        public async Task<IEnumerable<PathReportView>> ListPathReportsByStudyForExportAsync(Guid studyId)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<PathReportView>>(_navigationManager.ToAbsoluteUri($"api/pathreports/study/export/{studyId}"));
            return response;
        }

        public async Task<IEnumerable<PathReportView>> ListPathReportsByStudyExportedAsync(Guid studyId)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<PathReportView>>(_navigationManager.ToAbsoluteUri($"api/pathreports/study/exported/{studyId}"));
            return response;
        }

        public async Task<IEnumerable<StudyHeader>> GetPathReportHeaderOptionsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<StudyHeader>>(_navigationManager.ToAbsoluteUri($"api/pathreports/headeroptions"));
            return response;
        }

        // Export Path Report Data
        public async Task<string> ExportPathReportDataAsync(Guid studyId, string? exportType, Guid? batchId, string pathIds, bool isReport)
        {
            var response = await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/pathreports/export/{studyId}/{exportType}/{batchId}/{pathIds}/{isReport}"));
            return response;
        }

        public async Task<IEnumerable<ExportView>> GetPathReportExportHistoryAsync(Guid id)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<ExportView>>(_navigationManager.ToAbsoluteUri($"api/pathreports/exporthistory/{id}"));
            return response;
        }

        public async Task DeletePlaceholderPathReportsAsync()
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/pathreports/deleteplaceholders/"));
        }

        //public async Task<FileData> ExportPathReportDataAsync(Guid studyId)
        //{
        //    try
        //    {
        //        // Use HttpCompletionOption.ResponseHeadersRead to start processing the stream immediately.
        //        // This is more memory-efficient for large files as it avoids buffering the entire file in memory.
        //        var response = await _httpClient.GetAsync(_navigationManager.ToAbsoluteUri($"api/pathreports/export/{studyId}"), HttpCompletionOption.ResponseHeadersRead);
        //        response.EnsureSuccessStatusCode(); // Throws an exception for HTTP error status codes.

        //        // Read the Content-Disposition header to get the filename.
        //        var contentDisposition = response.Content.Headers.ContentDisposition;
        //        var fileName = contentDisposition?.FileNameStar ?? contentDisposition?.FileName;

        //        // Read the Content-Type header.
        //        var contentType = response.Content.Headers.ContentType?.MediaType;

        //        // Get the content stream directly from the HTTP response.
        //        var contentStream = await response.Content.ReadAsStreamAsync();

        //        return new FileData
        //        {
        //            FileStream = contentStream,
        //            ContentType = contentType,
        //            FileName = fileName?.Trim('"') // Trim potential quotes from the filename.
        //        };
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        // Log the exception for debugging.
        //        Console.WriteLine($"Error downloading file: {ex.Message}");
        //        return null;
        //    }
        //}
    }

}
