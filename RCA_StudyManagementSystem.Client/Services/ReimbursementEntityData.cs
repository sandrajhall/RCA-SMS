using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Client.Interfaces;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class ReimbursementEntityData : IReimbursementEntityData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public ReimbursementEntityData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }

        public async Task<IEnumerable<ReimbursementEntity>> ListReimbursementEntitiesAsync(CancellationToken token)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<ReimbursementEntity>>(_navigationManager.ToAbsoluteUri($"api/reimbursemententities"));
            return response;
        }

        public async Task<ReimbursementEntity> GetReimbursementEntityAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<ReimbursementEntity>(_navigationManager.ToAbsoluteUri($"api/reimbursemententities/{id}"));
        }

        public async Task<IEnumerable<ReimbursementEntity>> GetReimbursementEntitiesAsync(string searchTerm)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ReimbursementEntity>>(_navigationManager.ToAbsoluteUri($"api/reimbursemententities/search/{searchTerm}"));
        }


        public async Task<Guid> CreateReimbursementEntityAsync(string userId, ReimbursementEntity reimbEntity)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/reimbursemententities/{userId}"), reimbEntity);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["reimbursementEntityId"];

            return id;
        }
        public async Task UpdateReimbursementEntityAsync(Guid id, string userId, ReimbursementEntity reimbEntity)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/reimbursemententities/{id}/{userId}"), reimbEntity);
        }

        public async Task DeleteReimbursementEntityAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/reimbursemententities/{id}"));
        }
    }
}
