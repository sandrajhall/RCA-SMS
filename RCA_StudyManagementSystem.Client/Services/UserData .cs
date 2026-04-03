using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class UserData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public UserData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }


        public async Task<string> GetIdByEmailAsync(string email)
        {
            var response = await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/users/by-email/{email}"));
            return response;
        }

        public async Task<string> GetDisplayNameAsync(string id)
        {
            if(id == string.Empty)
            {
                return string.Empty;
            }
            var response = await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/users/displayname/{id}"));
            return response;
        }

        public async Task<Dictionary<string, string>> GetAllUsersAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<Dictionary<string, string>>(_navigationManager.ToAbsoluteUri($"api/users/all"));
            return response;
        }
    }
}
