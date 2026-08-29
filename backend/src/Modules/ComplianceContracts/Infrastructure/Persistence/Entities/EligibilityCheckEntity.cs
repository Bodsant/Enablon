namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>training.eligibility_checks</c> table.</summary>
public sealed class EligibilityCheckEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public Guid PersonId { get; set; }
    public Guid TargetRecordId { get; set; }
    public string Result { get; set; } = string.Empty;
    public DateTimeOffset CheckedAt { get; set; }
    public string? DetailsJson { get; set; }
}
