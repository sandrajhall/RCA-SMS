using Microsoft.AspNetCore.Components;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class StudyHeaderData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;
        
        public StudyHeaderData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
        }
        public async Task<IEnumerable<StudyHeader>> ListStudyHeadersAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<StudyHeader>>(_navigationManager.ToAbsoluteUri($"api/studyheaders"));
        }

        public async Task<IEnumerable<StudyHeader>> ListStudyHeadersByStudyIdAsync(Guid studyId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<StudyHeader>>(_navigationManager.ToAbsoluteUri($"api/studyheaders/study/{studyId}"));
        }
        public async Task<StudyHeader> GetStudyHeaderAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<StudyHeader>(_navigationManager.ToAbsoluteUri($"api/studyheaders/{id}"));
        }
        public async Task<StudyHeader> CreateStudyHeaderAsync(StudyHeader studyHeader)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/studyheaders"), studyHeader);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StudyHeader>();
        }
        public async Task UpdateStudyHeaderAsync(Guid id, StudyHeader studyHeader)
        {
            var response = await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/studyheaders/{id}"), studyHeader);
            response.EnsureSuccessStatusCode();
        }
        public async Task DeleteStudyHeaderAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/studyheaders/{id}"));
            response.EnsureSuccessStatusCode();
        }
    }
}
