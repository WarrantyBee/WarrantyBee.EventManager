using System.Net;
using System.Text.Json;
using RestSharp;
using WarrantyBee.EventManager.Application.Abstractions.Services;

namespace WarrantyBee.EventManager.Infrastructure.Services;

/// <summary>
/// Provides caching services using Upstash (Redis over HTTP).
/// </summary>
public class UpstashCacheService : ICacheService
{
    private readonly string _baseUrl;
    private readonly string _token;
    private readonly RestClient _client;

    public UpstashCacheService()
    {
        var host = Environment.GetEnvironmentVariable("WB__UPSTASH_HOST") ?? "localhost";
        _token = Environment.GetEnvironmentVariable("WB__UPSTASH_TOKEN") ?? "";
        _baseUrl = host.StartsWith("http") ? host : $"https://{host}";
        _client = new RestClient(_baseUrl);
    }

    public async Task SetAsync(string key, string value, int? expirySeconds = null)
    {
        var command = new List<object> { "SET", key, value };
        if (expirySeconds.HasValue && expirySeconds.Value > 0)
        {
            command.Add("EX");
            command.Add(expirySeconds.Value);
        }
        await SendAsync(command);
    }

    public async Task<string?> GetAsync(string key)
    {
        var command = new List<object> { "GET", key };
        var response = await SendAsync(command);
        
        using var doc = JsonDocument.Parse(response);
        if (doc.RootElement.TryGetProperty("result", out var result) && result.ValueKind != JsonValueKind.Null)
        {
            return result.GetString();
        }
        return null;
    }

    public async Task DeleteAsync(string key)
    {
        var command = new List<object> { "DEL", key };
        await SendAsync(command);
    }

    private async Task<string> SendAsync(List<object> command)
    {
        var request = new RestRequest("/", Method.Post);
        request.AddHeader("Authorization", $"Bearer {_token}");
        request.AddJsonBody(command);

        var response = await _client.ExecuteAsync(request);

        if (!response.IsSuccessful)
            throw new Exception($"Upstash request failed: {response.StatusCode}");

        return response.Content ?? string.Empty;
    }
}
