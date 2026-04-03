using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class DoctorData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public DoctorData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }

        public async Task<IEnumerable<Doctor>> ListDoctorsAsync(CancellationToken token)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Doctor>>(_navigationManager.ToAbsoluteUri($"api/doctors"));
            return response;
        }

        public async Task<IEnumerable<Doctor>> ListPathologistsAsync(CancellationToken token)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Doctor>>(_navigationManager.ToAbsoluteUri($"api/doctors/pathologists"));
            return response;
        }

        public async Task<IEnumerable<Doctor>> ListAllDoctorsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Doctor>>(_navigationManager.ToAbsoluteUri($"api/doctors/all"));
            return response;
        }

        public async Task<Doctor> GetDoctorAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Doctor>(_navigationManager.ToAbsoluteUri($"api/doctors/{id}"));
        }

        public async Task<Doctor> GetDoctorByMigratedIdAsync(string migratedId)
        {
            return await _httpClient.GetFromJsonAsync<Doctor>(_navigationManager.ToAbsoluteUri($"api/doctors/migratedid/{migratedId}"));
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsAsync(string searchTerm)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Doctor>>(_navigationManager.ToAbsoluteUri($"api/doctors/search/{searchTerm}"));
        }

        public async Task<IEnumerable<Doctor>> GetPathologistsAsync(string searchTerm)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Doctor>>(_navigationManager.ToAbsoluteUri($"api/doctors/pathsearch/{searchTerm}"));
        }

        public async Task<List<Doctor>> GetDoctorHistoryAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<List<Doctor>>(_navigationManager.ToAbsoluteUri($"api/doctors/doctorhistory/{id}"));
        }

        public async Task<Guid> CreateDoctorAsync(string userId, Doctor doctor)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/doctors/{userId}"), doctor);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["doctorId"];

            return id;
        }
        public async Task UpdateDoctorAsync(Guid id, string userId, Doctor doctor)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/doctors/{id}/{userId}"), doctor);
        }

        public async Task DeleteDoctorAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/doctors/{id}"));
        }
    }
}
