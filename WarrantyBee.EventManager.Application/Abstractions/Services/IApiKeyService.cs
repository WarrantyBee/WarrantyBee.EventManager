namespace WarrantyBee.EventManager.Application.Abstractions.Services;

/// <summary>
/// Defines a service for generating and validating encrypted API keys.
/// </summary>
public interface IApiKeyService
{
    /// <summary>
    /// Generates an encrypted API key for a specific client.
    /// </summary>
    /// <param name="clientName">Name of the client (e.g., 'MainApi').</param>
    /// <returns>An encrypted Base64 string representing the API key.</returns>
    string GenerateKey(string clientName);

    /// <summary>
    /// Validates an encrypted API key and returns the client name if valid.
    /// </summary>
    /// <param name="apiKey">The encrypted API key string.</param>
    /// <returns>The client name if valid; otherwise, null.</returns>
    string? ValidateKey(string apiKey);
}
