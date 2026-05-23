using System.Security.Cryptography;
using System.Text;
using WarrantyBee.EventManager.Application.Abstractions.Persistence;
using WarrantyBee.EventManager.Application.Abstractions.Services;

namespace WarrantyBee.EventManager.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="IApiKeyService"/> using hashing, database verification, and Redis caching.
/// </summary>
public class ApiKeyService : IApiKeyService
{
    private readonly IApiKeyRepository _keyRepository;
    private readonly ICacheService _cacheService;
    private readonly ITelemetryService _telemetry;

    public ApiKeyService(
        IApiKeyRepository keyRepository,
        ICacheService cacheService,
        ITelemetryService telemetry)
    {
        _keyRepository = keyRepository;
        _cacheService = cacheService;
        _telemetry = telemetry;
    }

    public async Task<bool> ValidateAsync(string appId, string appSecret)
    {
        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appSecret)) return false;

        var secretHash = ComputeHash(appSecret);
        var cacheKey = $"apikey:{appId}:{secretHash}";

        // 1. Check Cache
        var cachedResult = await _cacheService.GetAsync(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult == "1";
        }

        // 2. Check Database
        var isValid = await _keyRepository.ValidateAsync(appId, secretHash);

        // 3. Cache Result (5 minutes)
        await _cacheService.SetAsync(cacheKey, isValid ? "1" : "0", 300);

        if (!isValid)
        {
            _telemetry.Log(Domain.Enums.LogLevel.Warn, $"Invalid API Key attempt for AppId: {appId}");
        }

        return isValid;
    }

    private string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }
}
