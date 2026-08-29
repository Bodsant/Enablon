namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>asset.certificates</c> table. Asset certificates.</summary>
public sealed class CertificateEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid AssetId { get; set; }
    public string CertificateType { get; set; } = string.Empty;
    public string? CertificateNumber { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? Result { get; set; }
    public Guid? FileObjectId { get; set; }

    public AssetEntity? Asset { get; set; }
}