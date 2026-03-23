using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Shared.Domain;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class InvoiceData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public InvoiceData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }

        public async Task<IEnumerable<Invoice>> ListInvoicesNotSentAsync(CancellationToken token)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Invoice>>(_navigationManager.ToAbsoluteUri($"api/invoices/notsent"));
            return response;
        }

        public async Task<IEnumerable<Invoice>> ListInvoicesSentAsync(CancellationToken token)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Invoice>>(_navigationManager.ToAbsoluteUri($"api/invoices/sent"));
            return response;
        }

        public async Task<IEnumerable<Invoice>> GenerateInvoicesAsync(string startDate, string endDate, int quarter)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<Invoice>>(_navigationManager.ToAbsoluteUri($"api/invoices/generate/{startDate}/{endDate}/{quarter}"));
            return response;
        }

        public async Task<int> GetLastQuarterAsync(CancellationToken token)
        {
            var response = await _httpClient.GetFromJsonAsync<int>(_navigationManager.ToAbsoluteUri($"api/invoices/lastquarter"));
            return response;
        }

        public async Task<Invoice> GetInvoiceAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Invoice>(_navigationManager.ToAbsoluteUri($"api/invoices/{id}"));
        }


        public async Task<Guid> CreateInvoiceAsync(string userId, Invoice invoice)
        {
            var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/invoices/{userId}"), invoice);
            var jsonString = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            Guid id = (Guid)jsonObject["invoiceId"];

            return id;
        }
        public async Task UpdateInvoiceAsync(Guid id, string userId, Invoice invoice)
        {
            await _httpClient.PutAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/invoices/{id}/{userId}"), invoice);
        }

        public async Task DeleteInvoiceAsync(Guid id)
        {
            await _httpClient.DeleteAsync(_navigationManager.ToAbsoluteUri($"api/invoices/{id}"));
        }
    }
}
