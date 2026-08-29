namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>training.certifications</c> table.</summary>
public sealed class CertificationEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid PersonId { get; set; }
    public Guid? CourseId { get; set; }
    public string? CertificateNumber { get; set; }
    public DateOnly? IssuedAt { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public Guid? FileObjectId { get; set; }
    public string? VerificationStatus { get; set; }
}
