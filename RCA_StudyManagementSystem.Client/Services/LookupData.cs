using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class LookupData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public LookupData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }


        public async Task<IEnumerable<Lookup>> ListLookupsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Lookup>>(_navigationManager.ToAbsoluteUri($"api/lookups"));
            return response;
        }

        public async Task<IEnumerable<string>> ListLookupsByTypeAsync(string type)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<string>>(_navigationManager.ToAbsoluteUri($"api/lookups/{type}"));
            return response;
        }

        public async Task<Lookup> GetLookupAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Lookup>(_navigationManager.ToAbsoluteUri($"api/lookups/{id}"));
        }

        public async Task<string> GetCountyByFIPSAsync(string fips)
        {
            return await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/lookups/fips/{fips}"));
        }

        public async Task<string> GetTypeByCodeAsync(string type, string typeCode)
        {
            return await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/lookups/typebycode/{type}/{typeCode}"));
        }

        public async Task<string> GetCodeByTypeAsync(string type, string typeValue)
        {
            return await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/lookups/codebytype/{type}/{typeValue}"));
        }
        public async Task<Guid> CreateLookupAsync(Lookup lookup)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/lookups"), lookup);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["lookupId"];

            return id;
        }
        public async Task UpdateLookupAsync(Guid id, Lookup lookup)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/lookups/{id}"), lookup);
        }

        public async Task DeleteLookupAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/lookups/{id}"));
        }

    }
}
