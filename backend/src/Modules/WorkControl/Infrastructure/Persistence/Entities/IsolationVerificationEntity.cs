namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.isolation_verifications</c> table.</summary>
public sealed class IsolationVerificationEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid IsolationPointId { get; set; }
    public string VerificationType { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public Guid VerifiedByPersonId { get; set; }
    public DateTimeOffset VerifiedAt { get; set; }
    public string? Comment { get; set; }
}
