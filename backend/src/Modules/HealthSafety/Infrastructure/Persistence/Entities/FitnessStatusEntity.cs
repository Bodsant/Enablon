namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>health.fitness_statuses</c> table.</summary>
public sealed class FitnessStatusEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid HealthProfileId { get; set; } = Guid.Empty;
    public string FitnessStatus { get; set; } = string.Empty;
    public DateOnly ValidFrom { get; set; } = default;
    public DateOnly? ValidUntil { get; set; } = null;
    public string? RestrictionsSummary { get; set; } = null;
    public Guid? IssuedByMemberId { get; set; } = null;
}
