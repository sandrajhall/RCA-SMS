using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Client.Interfaces;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using static System.Net.WebRequestMethods;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class PatientData : IPatientData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public PatientData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }

        //var response = await Http.GetFromJsonAsync<Patient>(NavigationManager.ToAbsoluteUri($"api/Patients/{PatientId}"));


        public async Task<IEnumerable<Patient>> ListPatientsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Patient>>(_navigationManager.ToAbsoluteUri($"api/patients"));
            return response;
        }

        public async Task<Patient> GetPatientAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Patient>(_navigationManager.ToAbsoluteUri($"api/patients/{id}"));
        }
        public async Task<Patient> GetPatientByCaseNumberAsync(string caseNumber)
        {
            return await _httpClient.GetFromJsonAsync<Patient>(_navigationManager.ToAbsoluteUri($"api/patients/casenumber/{caseNumber}"));
        }

        public async Task<List<Patient>> GetPatientHistoryAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<List<Patient>>(_navigationManager.ToAbsoluteUri($"api/patients/patienthistory/{id}"));
        }

        public async Task<List<PatientPhoneNumber>> GetPatientPhoneNumberHistoryAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<List<PatientPhoneNumber>>(_navigationManager.ToAbsoluteUri($"api/patients/patientphonenumberhistory/{id}"));
        }

        public async Task<List<PathReport>> GetPathReportHistoryAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<List<PathReport>>(_navigationManager.ToAbsoluteUri($"api/patients/pathreporthistory/{id}"));
        }

        public async Task<Patient> GetPatientExportHistoryAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Patient>(_navigationManager.ToAbsoluteUri($"api/patients/exporthistory/{id}"));
        }

        public async Task<Guid> GetPatientIdByCCRNoAsync(string ccrno)
        {
            return await _httpClient.GetFromJsonAsync<Guid>(_navigationManager.ToAbsoluteUri($"api/patients/patientidbyccrno/{ccrno}"));
        }

        public async Task<string> ClearCCRNosAsync()
        {
            return await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/patients/clearccrnos"));
        }

        public async Task<string> GetPatientPrimaryPhoneAsync(Guid id)
        {
            var response = await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/patients/primaryphone/{id}"));
            return response ?? string.Empty;
        }   

        public async Task<bool> CheckSSNAsync(string ssn)
        {
            return await _httpClient.GetFromJsonAsync<bool>(_navigationManager.ToAbsoluteUri($"api/patients/ssn/{ssn}"));
        }

        public async Task<bool> CheckDOBAsync(DateTime? dob)
        {
            //await Http.GetStringAsync($"api/Time/GetByDate?targetDate={dateToSend:yyyy-MM-dd}");
            return await _httpClient.GetFromJsonAsync<bool>(_navigationManager.ToAbsoluteUri($"api/patients/dob?dob={dob}"));
        }

        public async Task<bool> CheckNameAsync(string name)
        {
            return await _httpClient.GetFromJsonAsync<bool>(_navigationManager.ToAbsoluteUri($"api/patients/name/{name}"));
        }

        public async Task<Guid> CreatePatientAsync(string userId, Patient patient)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/patients/{userId}"), patient);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["patientId"];

            return id;
        }
        public async Task UpdatePatientAsync(Guid id, string userId, Patient patient)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/patients/{id}/{userId}"), patient);
        }

        public async Task DeletePatientAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/patients/{id}"));
        }

        public async Task<string> GetLastCaseNumberAsync(string prefix)
        {
            var response = await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/patients/lastcasenumber/{prefix}"));
            return response ?? string.Empty;
        }
    }
}
