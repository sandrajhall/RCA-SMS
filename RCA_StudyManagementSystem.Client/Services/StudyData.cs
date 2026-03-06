using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class StudyData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

 

        public StudyData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }


        public async Task<IEnumerable<Study>> ListStudiesAsync() // Active studies only
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Study>>(_navigationManager.ToAbsoluteUri($"api/studies"));
            return response;
        }

        public async Task<IEnumerable<Study>> ListArchivedStudiesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Study>>(_navigationManager.ToAbsoluteUri($"api/studies/archived"));
            return response;
        }

        public async Task<IEnumerable<Study>> ListUnarchivedStudiesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Study>>(_navigationManager.ToAbsoluteUri($"api/studies/unarchived"));
            return response;
        }

        public async Task<Study> GetStudyAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Study>(_navigationManager.ToAbsoluteUri($"api/studies/{id}"));
        }

        public async Task<Study> GetStudyInfoAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Study>(_navigationManager.ToAbsoluteUri($"api/studies/info/{id}"));
        }

        public async Task<string> GetStudyNameAsync(Guid id)
        {
            return await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/studies/name/{id}"));
        }

        public async Task<string> GetStudyColorAsync(Guid id)
        {
            return await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/studies/color/{id}"));
        }

        public async Task<Guid> CreateStudyAsync(Study study)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/studies"), study);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["studyId"];

            return id;
        }


        public async Task UpdateStudyAsync(Guid id, Study study)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/studies/{id}"), study);
        }


        public async Task DeleteStudyAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/studies/{id}"));
        }

        public async Task ArchiveStudyAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/studies/archive/{id}"));
        }

    }
}

