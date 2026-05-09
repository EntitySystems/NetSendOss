using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetSend.Server.Lib.Services;
using XisaraCdn.Server.Lib.Config;
using XisaraCdn.Server.Lib.Models.Dbo;

namespace XisaraCdn.Server.Lib.Services;

public class AuthorizationValidator
{
    private readonly MasterDb _masterDb;
    private readonly ServerSession _serverSession;
    private readonly XisaraCertificateStore _certificateStore;
    private readonly ICtLogger _logger;

    public AuthorizationValidator(
        MasterDb masterDb,
        ServerSession serverSession,
        XisaraCertificateStore certificateStore,
        ICtLogger logger)
    {
        _masterDb = masterDb;
        _serverSession = serverSession;
        _certificateStore = certificateStore;
        _logger = logger;
    }

    private const string _validAlgorithm = "ES384";

    private static string _validAudience
    {
        get
        {
            if (field is not null)
                return field;

            var json = JsonDocument.Parse(SecretsConfig.XisaraJsonConfig);
            var clientId = json
                .RootElement
                .GetProperty("application_id")
                .GetString();

            return field = clientId ?? throw new XisaraException("JSON Config was invalid");
        }
    }

    public async Task<Result<UserAuthorizationResult, string>> GetUserAsync(string jwt)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            var keyId = Guid.Parse(token.Header.Kid);
            if (!token.Header.TryGetValue(JwtRegisteredClaimNames.Jti, out var jti))
                return new(
                    "Could not get JTI from JWT header",
                    DebugCode.Unauthorized);

            var tokenId = jti.ToString();

            var provider = await _certificateStore.GetCertificateSigningKeyAsync(keyId);
            if (provider.IsError)
                return new(provider.Error);

            var parameters = new TokenValidationParameters
            {
                IssuerSigningKey = provider.Value,
                ValidAudience = _validAudience,
                ValidIssuer = _buildIssuerUrl(keyId),
                ValidTypes = ["JWT"],
                ValidAlgorithms = [_validAlgorithm],
            };

            var jwtDbRecord = await _masterDb.JwtIssueRecords.FirstAsync(x => x.JwtId == tokenId);
            if (!jwtDbRecord.Valid)
                return new(
                    "The JWT validity has been revoked",
                    DebugCode.JwtRevoked);
            if (jwtDbRecord.Expiry < DateTime.UtcNow)
                return new(
                    "Token has expired",
                    DebugCode.JwtRevoked);

            var validation = await handler.ValidateTokenAsync(jwt, parameters);
            if (!validation.IsValid)
                return new(
                    "Could not validate token with given KID",
                    DebugCode.JwtInvalidSignature);

            return await _parseValidationResultAsync(validation, token);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogError(ex, "could not validate jwt");

            return new(
                "Could not verify JWT",
                DebugCode.Unauthorized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);

            return new(
                "Could not verify JWT, unknown error",
                DebugCode.GenericError);
        }
    }

    private async Task<Result<UserAuthorizationResult, string>> _parseValidationResultAsync(
        TokenValidationResult validationResult,
        JwtSecurityToken jwtSecurityToken)
    {
        try
        {
            var xsUserId = validationResult.Claims[JwtKeys.XisaraUserId].ToString();
            if (xsUserId is null)
                return new(
                    "Could not get JWT subject",
                    DebugCode.Unauthorized);
            var email = validationResult.Claims[JwtKeys.XisaraEmail].ToString();
            if (email is null)
                return new(
                    "Could not get email",
                    DebugCode.Unauthorized);
            var scopesJson = validationResult.Claims[JwtKeys.XisaraScopes].ToString();
            if (scopesJson is null)
                return new(
                    "Could not get JWT scopes",
                    DebugCode.Unauthorized);

            var userId = Guid.Parse(xsUserId);
            var scopes = scopesJson.FromJson<IReadOnlyList<IdentityScope>>();

            if (!scopes.Any(x => x.ScopeDirectory is XisaraPackageManager.PackageOperations))
                return new(
                    $"Scope '{XisaraPackageManager.PackageOperations}' is missing",
                    DebugCode.Unauthorized);

            var user = await GetUserAsync(userId, email);
            if (user.IsError)
                return new(user.Error);

            var expClaim = Convert.ToInt32(validationResult.Claims[JwtRegisteredClaimNames.Exp]);
            var expiry = DateTime.UnixEpoch.AddSeconds(expClaim);

            _serverSession.User = user.Value;
            _serverSession.Jwt = jwtSecurityToken;
            _serverSession.JwtExpires = expiry;

            return new(
                new UserAuthorizationResult(
                    user.Value,
                    scopes,
                    validationResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not verify JWT");
            return new("Could not verify JWT", DebugCode.Unauthorized);
        }
    }

    public async Task<Result<User, string>> GetUserAsync(
        Guid xsUserId,
        string email)
    {
        var user = await _masterDb.Users.Where(x => x.XisaraUserId == xsUserId).FirstOrDefaultAsync();
        if (user is null)
        {
            return await _registerUserAsync(xsUserId, email);
        }

        return new(user);
    }

    private async Task<Result<User, string>> _registerUserAsync(
        Guid xsUserId,
        string email)
    {
        var user = new User
        {
            XisaraUserId = xsUserId,
            Email = email,
            XisaraRefreshToken = null,
        };

        _masterDb.Users.Add(user);
        await _masterDb.SaveChangesAsync();

        return new(user);
    }

    private static string _buildIssuerUrl(Guid keyId)
        => $"{EnvConfig.AuthorizationServerUrl}" +
           $"/api/Certificates/GetJwks/{keyId}";
}

public record UserAuthorizationResult(
    User User,
    IReadOnlyList<IdentityScope> Scopes,
    TokenValidationResult ValidationResult);