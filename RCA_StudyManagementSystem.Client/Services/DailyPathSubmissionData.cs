using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Client.Interfaces;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class DailyPathSubmissionData : IDailyPathSubmissionData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public DailyPathSubmissionData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }

        public async Task<IEnumerable<DailyPathSubmission>> ListDailyPathSubmissionsAsync(CancellationToken token)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<DailyPathSubmission>>(_navigationManager.ToAbsoluteUri($"api/dailypathsubmissions"));
            return response;
        }


        public async Task<DailyPathSubmission> GetDailyPathSubmissionAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<DailyPathSubmission>(_navigationManager.ToAbsoluteUri($"api/dailypathsubmissions/{id}"));
        }

        public async Task<List<MonthlyPathSubmissionView>> ListMonthlyPathSubmissionAsync(int year, int month, Guid studyId)
        {
            return await _httpClient.GetFromJsonAsync<List<MonthlyPathSubmissionView>>(_navigationManager.ToAbsoluteUri($"api/dailypathsubmissions/monthly/{year}/{month}/{studyId}"));
        }

        public async Task<Guid> CreateDailyPathSubmissionAsync(string userId, DailyPathSubmission dailyPathSubmission)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/dailypathsubmissions/{userId}"), dailyPathSubmission);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["dailyPathSubmissionId"];

            return id;
        }

        public async Task SaveDailyPathSubmissionAsync(string userId, DailyPathSubmission dailyPathSubmission)
        {
            await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/dailypathsubmissions/daily/{userId}"), dailyPathSubmission);

        }
        public async Task UpdateDailyPathSubmissionAsync(Guid id, string userId, DailyPathSubmission dailyPathSubmission)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/dailypathsubmissions/{id}/{userId}"), dailyPathSubmission);
        }

        public async Task DeleteDailyPathSubmissionAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/dailypathsubmissions/{id}"));
        }

    }
}
