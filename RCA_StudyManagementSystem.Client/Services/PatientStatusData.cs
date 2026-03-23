using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class PatientStatusData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public PatientStatusData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }

        public async Task<IEnumerable<PatientStatus>> ListPatientStatusAsync(CancellationToken token)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<PatientStatus>>(_navigationManager.ToAbsoluteUri($"api/patientstatuses"));
            return response;
        }


        public async Task<PatientStatus> GetPatientStatusAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<PatientStatus>(_navigationManager.ToAbsoluteUri($"api/patientstatuses/{id}"));
        }

        public async Task<PatientStatus> GetPatientStatusByCaseNumberAsync(string caseNumber)
        {
            return await _httpClient.GetFromJsonAsync<PatientStatus>(_navigationManager.ToAbsoluteUri($"api/patientstatuses/casenumber/{caseNumber}"));
        }

        public async Task<List<PatientStatusView>> ListPatientStatusesByStudyIdAsync(Guid studyId, string startDate, string endDate)
        {
            return await _httpClient.GetFromJsonAsync<List<PatientStatusView>>(_navigationManager.ToAbsoluteUri($"api/patientstatuses/patientstatusview/{studyId}/{startDate}/{endDate}"));
        }

        public async Task<string> ListPatientStatusesByStudyIdCSVAsync(Guid studyId, string startDate, string endDate)
        {
            return await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/patientstatuses/patientstatusviewcsv/{studyId}/{startDate}/{endDate}"));
        }

        public async Task<Guid> CreatePatientStatusAsync(string userId, PatientStatus patientStatus)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/patientstatuses/{userId}"), patientStatus);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["patientStatusId"];

            return id;
        }

 
        public async Task UpdatePatientStatusAsync(Guid id, string userId, PatientStatus patientStatus)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/patientstatuses/{id}/{userId}"), patientStatus);
        }

        public async Task DeletePatientStatusAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/patientstatuses/{id}"));
        }

    }
}
