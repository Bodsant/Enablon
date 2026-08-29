namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>capa.verifications</c> table.</summary>
public sealed class CapaVerificationEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ActionId { get; set; }
    public Guid VerifierMemberId { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Comment { get; set; } = string.Empty;
    public DateTimeOffset VerifiedAt { get; set; }
}
