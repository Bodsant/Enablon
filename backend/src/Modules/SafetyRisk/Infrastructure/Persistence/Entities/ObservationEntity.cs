namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>safety.observations</c> table.</summary>
public sealed class ObservationEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public string ObservationType { get; set; } = string.Empty;
    public Guid ReporterMemberId { get; set; }
    public string ReporterVisibility { get; set; } = string.Empty;
    public string? PotentialImpact { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImmediateAction { get; set; } = string.Empty;
    public string? InitialRiskLevel { get; set; } = string.Empty;
    public Guid? AssignedMemberId { get; set; }
    public DateOnly? DueDate { get; set; }
}
