using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Client.Interfaces;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class HistologyData : IHistologyData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public HistologyData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }


        public async Task<IEnumerable<Histology>> ListHistologiesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Histology>>(_navigationManager.ToAbsoluteUri($"api/histologies"));
            return response;
        }

        public async Task<IEnumerable<Histology>> ListActiveHistologiesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Histology>>(_navigationManager.ToAbsoluteUri($"api/histologies/active"));
            return response;
        }

        public async Task<Histology> GetHistologyAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Histology>(_navigationManager.ToAbsoluteUri($"api/histologies/{id}"));
        }
        public async Task<Guid> CreateHistologyAsync(string userId, Histology histology)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/histologies/{userId}"), histology);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["histologyId"];

            return id;
        }
        public async Task UpdateHistologyAsync(Guid id, string userId, Histology histology)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/histologies/{id}/{userId}"), histology);
        }

        public async Task DeleteHistologyAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/histologies/{id}"));
        }

    }
}
