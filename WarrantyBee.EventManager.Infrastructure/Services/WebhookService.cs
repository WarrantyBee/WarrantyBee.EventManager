using System.Text;
using Polly;
using Polly.Retry;
using WarrantyBee.EventManager.Application.Abstractions.Services;

namespace WarrantyBee.EventManager.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public WebhookService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        
        // Resilience: Retry 3 times with exponential backoff for 5xx errors or timeouts
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    public async Task<bool> SendWebhookAsync(string url, string payload, string secret)
    {
        var client = _httpClientFactory.CreateClient("WebhookClient");
        
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        // Add signature for security (simplified)
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("X-WarrantyBee-Signature", secret);

        try
        {
            var response = await _retryPolicy.ExecuteAsync(() => client.PostAsync(url, content));
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
