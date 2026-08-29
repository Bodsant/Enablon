namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>health.profiles</c> table.</summary>
public sealed class HealthProfileEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid PersonId { get; set; } = Guid.Empty;
    public string? RestrictedIdentifier { get; set; } = null;
    public Guid DataClassificationId { get; set; } = Guid.Empty;

    public ICollection<FitnessStatusEntity> FitnessStatusRecords { get; set; } = new List<FitnessStatusEntity>();
}
