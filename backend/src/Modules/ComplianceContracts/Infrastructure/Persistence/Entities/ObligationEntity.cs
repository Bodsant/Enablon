namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>compliance.obligations</c> table.</summary>
public sealed class ObligationEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid LegalSourceVersionId { get; set; }
    public string? ClauseReference { get; set; }
    public string RequirementText { get; set; } = string.Empty;
    public Guid OwnerMemberId { get; set; }
    public string? Frequency { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateOnly? LastReview { get; set; }
    public DateOnly? NextReview { get; set; }
}
