using System.Security.Cryptography;
using System.Text;
using WarrantyBee.EventManager.Application.Abstractions.Services;

namespace WarrantyBee.EventManager.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="IApiKeyService"/> using AES encryption to wrap a secret within the API Key.
/// </summary>
public class ApiKeyService : IApiKeyService
{
    private readonly byte[] _encryptionKey;
    private const string SecretPrefix = "WB_INTERNAL_SECRET_";

    public ApiKeyService()
    {
        var key = Environment.GetEnvironmentVariable("WB__API_ENCRYPTION_KEY") ?? "Default_Shared_Secret_Must_Be_32_Chars!!";
        // Ensure key is exactly 32 bytes for AES-256
        _encryptionKey = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
    }

    public string GenerateKey(string clientName)
    {
        var rawPayload = $"{SecretPrefix}{clientName}|{DateTime.UtcNow:O}";
        return Encrypt(rawPayload);
    }

    public string? ValidateKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return null;

        try
        {
            var decrypted = Decrypt(apiKey);
            if (!decrypted.StartsWith(SecretPrefix)) return null;

            var parts = decrypted.Replace(SecretPrefix, "").Split('|');
            return parts[0]; // Return client name
        }
        catch
        {
            return null;
        }
    }

    private string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    private string Decrypt(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;

        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Array.Copy(fullCipher, iv, iv.Length);
        Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipher);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}
