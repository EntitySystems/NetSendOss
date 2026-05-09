using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NetSend.Shared.Types;

namespace NetSend.Server.Lib.Services;

public class XisaraCertificateStore
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public XisaraCertificateStore(
        HttpClient httpClient,
        ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    private static readonly MemoryCache _certificatesCache = new(new MemoryCacheOptions
    {
        ExpirationScanFrequency = TimeSpan.FromMinutes(5)
    });

    public async Task<Result<SecurityKey, string>> GetCertificateSigningKeyAsync(Guid keyId)
    {
        var cacheSecurityKey = _certificatesCache.Get<SecurityKey>(keyId);
        if (cacheSecurityKey is not null)
            return new(cacheSecurityKey);

        var url = _generateIssuerUrl(keyId);
        var jwksJson = await _httpClient.GetStringAsync(url);
        var jwks = JsonWebKeySet.Create(jwksJson);

        var signingKey = jwks.GetSigningKeys().First();
        _certificatesCache.Set(keyId, signingKey);

        return new(signingKey);
    }

    private string _generateIssuerUrl(Guid keyId)
    {
        return $"https://authorization.xisara.com/api/Certificates/GetJwksJson/{keyId}.json";
    }
}