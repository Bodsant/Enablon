namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>incident.investigations</c> table.</summary>
public sealed class InvestigationEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid IncidentId { get; set; }
    public Guid LeadInvestigatorMemberId { get; set; }
    public string? Method { get; set; } = string.Empty;
    public string? Summary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
