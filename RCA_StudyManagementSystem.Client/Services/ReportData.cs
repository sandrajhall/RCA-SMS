using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Client.Interfaces;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class ReportData : IReportData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public ReportData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }

        public async Task<IEnumerable<PathCountByStudyByDate>> GetPathsByStudyByDateAsync(Guid studyId, string startDate, string endDate)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<PathCountByStudyByDate>>(_navigationManager.ToAbsoluteUri($"api/reports/pathsbystudybydate/{studyId}/{startDate}/{endDate}"));
            return response;
        }

        public async Task<string> GetPathsByStudyByDateCSVAsync(Guid studyId, string startDate, string endDate)
        {
            var response = await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/reports/pathsbystudybydatecsv/{studyId}/{startDate}/{endDate}"));
            return response;
        }

        public async Task<IEnumerable<PathCountByStudyByDate>> GetCECSPathsByDateAsync(string startDate, string endDate)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<PathCountByStudyByDate>>(_navigationManager.ToAbsoluteUri($"api/reports/cecspathsbydate/{startDate}/{endDate}"));
            return response;
        }

        public async Task<int> GetCESCCasesByDateAsync(string startDate, string endDate)
        {
            var response = await _httpClient.GetFromJsonAsync<int>(_navigationManager.ToAbsoluteUri($"api/reports/cecscasesbydate/{startDate}/{endDate}"));
            return response;
        }

        public async Task<IEnumerable<RaceCountByDate>> GetCECSRaceCountByDateAsync(string startDate, string endDate)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<RaceCountByDate>>(_navigationManager.ToAbsoluteUri($"api/reports/cecsracecountbydate/{startDate}/{endDate}"));
            return response;
        }

        public async Task<IEnumerable<EthnicityCountByDate>> GetCECSEthnicityCountByDateAsync(string startDate, string endDate)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<EthnicityCountByDate>>(_navigationManager.ToAbsoluteUri($"api/reports/cecsethnicitycountbydate/{startDate}/{endDate}"));
            return response;
        }

    }
}
