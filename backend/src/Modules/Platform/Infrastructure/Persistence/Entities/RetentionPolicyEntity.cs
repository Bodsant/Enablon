namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.retention_policies</c> table. Retention/archival rules per record type.</summary>
public sealed class RetentionPolicyEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string RecordType { get; set; } = string.Empty;
    public Guid? ClassificationId { get; set; }
    public int? RetentionDays { get; set; }
    public int? ArchiveAfterDays { get; set; }
    public int? RecycleBinDays { get; set; }
    public bool LegalHoldSupported { get; set; }

    public DataClassificationEntity? Classification { get; set; }
}