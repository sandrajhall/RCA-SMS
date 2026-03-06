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
    public class StudyLookupData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

 

        public StudyLookupData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }


        public async Task<IEnumerable<StudyLookupView>> ListStudyLookupsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<StudyLookupView>>(_navigationManager.ToAbsoluteUri($"api/studylookups"));
            return response;
        }

        public async Task<IEnumerable<StudyLookupView>> ListStudyLookupsByStudyIdAsync(Guid id)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<StudyLookupView>>(_navigationManager.ToAbsoluteUri($"api/studylookups/lookups/{id}"));
            return response;
        }

        public async Task<IEnumerable<StudyLookupView>> ListStudyLookupsByTypeAsync(string type)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<StudyLookupView>>(_navigationManager.ToAbsoluteUri($"api/studylookups/{type}"));
            return response;
        }

        public async Task<IEnumerable<StudyLookupView>> ListStudyLookupsAllByTypeAsync(string type)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<StudyLookupView>>(_navigationManager.ToAbsoluteUri($"api/studylookups/all/{type}"));
            return response;
        }


        public async Task<IEnumerable<string>> ListOptionsAsync(string type, Guid studyId)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<string>>(_navigationManager.ToAbsoluteUri($"api/studylookups/options/{type}/{studyId}"));
            return response;
        }

        public async Task<StudyLookup> GetStudyLookupAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<StudyLookup>(_navigationManager.ToAbsoluteUri($"api/studylookups/{id}"));
        }

        public async Task<string> GetValueByOldCodeAsync(Guid studyId, string type, int oldCode)
        {
            var response = await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/studylookups/valuebyoldcode/{studyId}/{type}/{oldCode}"));
            return response;
        }

        public async Task<string> GetCodeByValueAsync(Guid studyId, string type, string value)
        {
            var response = await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/studylookups/codebyvalue/{studyId}/{type}/{value}"));
            return response;
        }

        public async Task<Guid> CreateStudyLookupAsync(StudyLookup studyLookup)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/studylookups"), studyLookup);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["studyLookupId"];

            return id;
        }


        public async Task UpdateStudyLookupAsync(Guid id, StudyLookup studyLookup)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/studylookups/{id}"), studyLookup);
        }


        public async Task DeleteStudyLookupAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/studylookups/{id}"));
        }
    }
}

