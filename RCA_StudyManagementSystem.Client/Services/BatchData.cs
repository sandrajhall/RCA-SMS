using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Client.Interfaces;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class BatchData : IBatchData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public BatchData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }

        public async Task<IEnumerable<Batch>> ListBatchesAsync(CancellationToken token)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Batch>>(_navigationManager.ToAbsoluteUri($"api/batches"));
            return response;
        }

        public async Task<IEnumerable<Batch>> ListBatchesByStudyAsync(Guid studyId)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Batch>>(_navigationManager.ToAbsoluteUri($"api/batches/study/{studyId}"));
            return response;
        }

        public async Task<Batch> GetBatchAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Batch>(_navigationManager.ToAbsoluteUri($"api/batches/{id}"));
        }

        public async Task<Guid> CreateBatchAsync(string userId,Batch batch)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/batches/{userId}"), batch);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["batchId"];

            return id;
        }
        public async Task UpdateBatchAsync(Guid id, string userId, Batch batch)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/batches/{id}/{userId}"), batch);
        }

        public async Task DeleteBatchAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/batches/{id}"));
        }

        public async Task<string> GetLastBatchNumberAsync(string prefix)
        {
            var response = await _httpClient.GetStringAsync(_navigationManager.ToAbsoluteUri($"api/batches/lastbatchnumber/{prefix}"));
            return response ?? string.Empty;
        }

        public async Task<Guid> GetBatchIdAsync(string batchNumber)
        {
            var response = await _httpClient.GetFromJsonAsync<Guid>(_navigationManager.ToAbsoluteUri($"api/batches/batchid/{batchNumber}"));
            return response;
        }
    }
}
