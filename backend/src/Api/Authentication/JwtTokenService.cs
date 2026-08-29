using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Ehsms.Api.Authentication;

/// <summary>
/// Issues JWT access tokens and opaque refresh-token hashes for the API. Access tokens
/// carry the <c>sub</c> (user id), email and an optional <c>tenant</c> claim so tenant
/// isolation can be resolved without a database round-trip on every request.
/// </summary>
public sealed class JwtTokenService
{
    private readonly AuthOptions _options;

    public JwtTokenService(AuthOptions options)
    {
        _options = options;
    }

    public (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(Guid userId, string email, Guid? tenantId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };
        if (tenantId.HasValue)
        {
            claims.Add(new Claim("tenant", tenantId.Value.ToString()));
        }

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public static string HashRefreshToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    public static string GenerateRefreshToken() => RandomNumberGenerator.GetHexString(48, lowercase: true);
}