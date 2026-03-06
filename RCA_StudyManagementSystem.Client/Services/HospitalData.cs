using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class HospitalData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public HospitalData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }

        public async Task<IEnumerable<Hospital>> ListHospitalsAsync(CancellationToken token)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Hospital>>(_navigationManager.ToAbsoluteUri($"api/hospitals"));
            return response;
        }
        
        public async Task<IEnumerable<Hospital>> ListAllHospitalsAsync(CancellationToken token)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Hospital>>(_navigationManager.ToAbsoluteUri($"api/hospitals/all"));
            return response;
        }

        public async Task<IEnumerable<Hospital>> ListHospitalsForReimbursementEntityAsync(Guid reimbursementEntityId)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Hospital>>(_navigationManager.ToAbsoluteUri($"api/hospitals/reimbursemententity/{reimbursementEntityId}"));
            return response;
        }

        public async Task<Hospital> GetHospitalAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Hospital>(_navigationManager.ToAbsoluteUri($"api/hospitals/{id}"));
        }

        public async Task<Hospital> GetHospitalByMigratedIdAsync(string migratedId)
        {
            return await _httpClient.GetFromJsonAsync<Hospital>(_navigationManager.ToAbsoluteUri($"api/hospitals/migratedid/{migratedId}"));
        }

        public async Task<IEnumerable<Hospital>> GetHospitalsAsync(string searchTerm)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Hospital>>(_navigationManager.ToAbsoluteUri($"api/hospitals/search/{searchTerm}"));
        }

        public async Task<string> GetHospShortNameAsync(string hospName)
        {
            return await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/hospitals/shortname/{hospName}"));
        }
        public async Task<Guid> CreateHospitalAsync(Hospital hospital)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/hospitals"), hospital);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["hospitalId"];

            return id;
        }
        public async Task UpdateHospitalAsync(Guid id, Hospital hospital)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/hospitals/{id}"), hospital);
        }

        public async Task DeleteHospitalAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/hospitals/{id}"));
        }

        public async Task<Guid> GetHospitalIdAsync(string? hospitalName)
        {
            return await _httpClient.GetFromJsonAsync<Guid>(_navigationManager.ToAbsoluteUri($"api/hospitals/id/{hospitalName}"));
        }
    }
}
