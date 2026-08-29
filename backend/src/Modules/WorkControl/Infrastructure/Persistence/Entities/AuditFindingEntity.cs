namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>audit.findings</c> table.</summary>
public sealed class AuditFindingEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid AuditId { get; set; }
    public Guid? AuditResponseId { get; set; }
    public string Classification { get; set; } = string.Empty;
    public string? RequirementReference { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Recommendation { get; set; }
    public Guid? OwnerMemberId { get; set; }
}
