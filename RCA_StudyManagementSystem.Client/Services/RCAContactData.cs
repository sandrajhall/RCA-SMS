using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class RCAContactData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public RCAContactData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }

        public async Task<IEnumerable<RCAContact>> ListRCAContactsAsync(CancellationToken token)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<RCAContact>>(_navigationManager.ToAbsoluteUri($"api/rcacontacts"));
            return response;
        }

        public async Task<RCAContact> GetRCAContactAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<RCAContact>(_navigationManager.ToAbsoluteUri($"api/rcacontacts/{id}"));
        }

        public async Task<IEnumerable<RCAContact>> GetRCAContactsAsync(string searchTerm)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<RCAContact>>(_navigationManager.ToAbsoluteUri($"api/rcacontacts/search/{searchTerm}"));
        }

        public async Task<IEnumerable<RCAContact>> ListRCAContactsByReimbursementEntityIdAsync(Guid reimbursementEntityId)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<RCAContact>>(_navigationManager.ToAbsoluteUri($"api/rcacontacts/reimbursemententity/{reimbursementEntityId}"));
            return response;
        }

        public async Task<RCAContact> ListReimbursementEntitiesAsync(Guid rcaContactId)
        {
            var response = await _httpClient.GetFromJsonAsync<RCAContact>(_navigationManager.ToAbsoluteUri($"api/rcacontacts/reimbursemententitylist/{rcaContactId}"));
            return response;
        }

        public async Task<Guid> CreateRCAContactAsync(string userId, RCAContact rcaContact)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/rcacontacts/{userId}"), rcaContact);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["rcaContactId"];

            return id;
        }
        public async Task UpdateRCAContactAsync(Guid id, string userId, RCAContact rcaContact)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/rcacontacts/{id}/{userId}"), rcaContact);
        }

        public async Task DeleteRCAContactAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/rcacontacts/{id}"));
        }
    }
}
