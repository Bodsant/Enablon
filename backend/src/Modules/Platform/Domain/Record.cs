using Ehsms.BuildingBlocks;

namespace Ehsms.Modules.Platform.Domain;

/// <summary>
/// Central record registry — all major aggregates reference platform.records.
/// Provides cross-module traceability via real FKs, not polymorphic free text.
/// </summary>
public class Record : AuditableEntity
{
    public string RecordType { get; set; } = string.Empty;
    public string RecordNumber { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string Status { get; set; } = "DRAFT";
    public Guid? SourceRecordId { get; set; }
    public string? SourceRecordType { get; set; }
    public string? Metadata { get; set; } // JSONB for extensible metadata
}

public class RecordLink
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SourceRecordId { get; set; }
    public Guid TargetRecordId { get; set; }
    public string LinkType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
