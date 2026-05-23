namespace WarrantyBee.EventManager.Application.Abstractions.Services;

/// <summary>
/// Defines a service for validating stateful API keys.
/// </summary>
public interface IApiKeyService
{
    /// <summary>
    /// Validates an AppId and AppSecret against the cache or database.
    /// </summary>
    /// <param name="appId">The unique Application ID.</param>
    /// <param name="appSecret">The plain-text Application Secret.</param>
    /// <returns>True if the credentials are valid; otherwise, false.</returns>
    Task<bool> ValidateAsync(string appId, string appSecret);
}
