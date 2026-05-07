using Microsoft.AspNetCore.Components;
using RCA_StudyManagementSystem.Client.Interfaces;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class StudyReportHeaderData : IStudyReportHeaderData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;
        
        public StudyReportHeaderData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
        }
        public async Task<IEnumerable<StudyReportHeader>> ListStudyReportHeadersAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<StudyReportHeader>>(_navigationManager.ToAbsoluteUri($"api/studyreportheaders"));
        }

        public async Task<IEnumerable<StudyReportHeader>> ListStudyReportHeadersByStudyIdAsync(Guid studyId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<StudyReportHeader>>(_navigationManager.ToAbsoluteUri($"api/studyreportheaders/study/{studyId}"));
        }
        public async Task<StudyReportHeader> GetStudyReportHeaderAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<StudyReportHeader>(_navigationManager.ToAbsoluteUri($"api/studyreportheaders/{id}"));
        }
        public async Task<StudyReportHeader> CreateStudyReportHeaderAsync(string userId, StudyReportHeader studyReportHeader)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/studyreportheaders/{userId}"), studyReportHeader);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StudyReportHeader>();
        }
        public async Task UpdateStudyReportHeaderAsync(Guid id, string userId, StudyReportHeader studyReportHeader)
        {
            var response = await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/studyreportheaders/{id}/{userId}"), studyReportHeader);
            response.EnsureSuccessStatusCode();
        }
        public async Task DeleteStudyReportHeaderAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/studyreportheaders/{id}"));
            response.EnsureSuccessStatusCode();
        }
    }
}
