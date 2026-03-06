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
    public class StudyHistologyData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

 

        public StudyHistologyData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }


        public async Task<IEnumerable<StudyHistologyView>> ListStudyHistologiesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<StudyHistologyView>>(_navigationManager.ToAbsoluteUri($"api/studyhistologies/histologies"));
            return response;
        }

        public async Task<IEnumerable<StudyHistologyView>> ListStudyHistologiesAllAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<StudyHistologyView>>(_navigationManager.ToAbsoluteUri($"api/studyhistologies/histologies/all"));
            return response;
        }

        public async Task<IEnumerable<StudyHistologyView>> ListStudyHistologiesByStudyIdAsync(Guid id)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<StudyHistologyView>>(_navigationManager.ToAbsoluteUri($"api/studyhistologies/histologies/{id}"));
            return response;
        }

        public async Task<IEnumerable<string>> ListOptionsAsync(string type, Guid studyId)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<string>>(_navigationManager.ToAbsoluteUri($"api/studyhistologies/options/{type}/{studyId}"));
            return response;
        }

        public async Task<StudyHistology> GetStudyHistologyAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<StudyHistology>(_navigationManager.ToAbsoluteUri($"api/studyhistologies/{id}"));
        }

        public async Task<Histology> GetValueByOldCodeAsync(Guid studyId, int oldCode)
        {
            return await _httpClient.GetFromJsonAsync<Histology>(_navigationManager.ToAbsoluteUri($"api/studyhistologies/valuebyoldcode/{studyId}/{oldCode}"));
        }

        public async Task<Guid> CreateStudyHistologyAsync(StudyHistology studyHistology)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/studyhistologies"), studyHistology);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["studyHistologyId"];

            return id;
        }


        public async Task UpdateStudyHistologyAsync(Guid id, StudyHistology studyHistology)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/studyhistologies/{id}"), studyHistology);
        }


        public async Task DeleteStudyHistologyAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/studyhistologies/{id}"));
        }
    }
}

