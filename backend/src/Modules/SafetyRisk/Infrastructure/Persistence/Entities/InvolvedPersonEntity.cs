namespace Ehsms.Modules.SafetyRisk.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>incident.involved_people</c> table.</summary>
public sealed class InvolvedPersonEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid IncidentId { get; set; }
    public Guid? PersonId { get; set; }
    public string? ExternalPersonName { get; set; } = string.Empty;
    public string InvolvementType { get; set; } = string.Empty;
    public Guid? InjuryClassificationId { get; set; }
    public int? LostWorkDays { get; set; }
}
