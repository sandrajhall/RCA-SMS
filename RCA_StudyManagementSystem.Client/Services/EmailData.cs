using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using RCA_StudyManagementSystem.Client.Interfaces;
using RCA_StudyManagementSystem.Shared.DTOs;
using System.Net.Http.Json;

namespace RCA_StudyManagementSystem.Client.Services
{
    public class EmailData : IEmailData
    {
        private readonly HttpClient _httpClient;

        private readonly NavigationManager _navigationManager;

        public EmailData(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;

        }

        public async Task<bool> SendAsync(EmailRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/email/"), request);

                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (HttpRequestException ex)
            {
                // Log or handle the exception appropriately
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendFromInvoiceTemplateAsync(InvoiceEmailData request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_navigationManager.ToAbsoluteUri($"api/email/invoicetemplate"), request);

                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (HttpRequestException ex)
            {
                // Log or handle the exception appropriately
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }

    }
}
