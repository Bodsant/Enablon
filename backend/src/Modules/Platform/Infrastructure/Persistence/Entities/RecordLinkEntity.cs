namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.record_links</c> table. Directed links between records.</summary>
public sealed class RecordLinkEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SourceRecordId { get; set; }
    public Guid TargetRecordId { get; set; }
    public string LinkType { get; set; } = string.Empty;
    public Guid CreatedByMemberId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public RecordEntity? SourceRecord { get; set; }
    public RecordEntity? TargetRecord { get; set; }
}