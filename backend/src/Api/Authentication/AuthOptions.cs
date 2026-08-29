namespace Ehsms.Api.Authentication;

/// <summary>JWT options bound from the <c>Authentication</c> configuration section.</summary>
public sealed class AuthOptions
{
    public const string SectionName = "Authentication";

    public string Issuer { get; set; } = "ehsms";
    public string Audience { get; set; } = "ehsms-api";
    public string SecretKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 14;
}