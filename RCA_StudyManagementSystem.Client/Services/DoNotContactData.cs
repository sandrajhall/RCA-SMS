using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Client.Interfaces;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class DoNotContactData : IDoNotContactData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public DoNotContactData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }

        public async Task<IEnumerable<DoNotContact>> ListDoNotContactsAsync(CancellationToken token)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<DoNotContact>>(_navigationManager.ToAbsoluteUri($"api/donotcontacts"));
            return response;
        }


        public async Task<DoNotContact> GetDoNotContactAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<DoNotContact>(_navigationManager.ToAbsoluteUri($"api/donotcontacts/{id}"));
        }

        public async Task<Guid> CreateDoNotContactAsync(string userId, DoNotContact doNotContact)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/donotcontacts/{userId}"), doNotContact);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["doNotContactId"];

            return id;
        }
        public async Task UpdateDoNotContactAsync(Guid id, string userId, DoNotContact doNotContact)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/donotcontacts/{id}/{userId}"), doNotContact);
        }

        public async Task DeleteDoNotContactAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/donotcontacts/{id}"));
        }

    }
}
