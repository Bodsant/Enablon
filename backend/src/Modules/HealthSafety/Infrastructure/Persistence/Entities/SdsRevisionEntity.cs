namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>chemical.sds_revisions</c> table.</summary>
public sealed class SdsRevisionEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TenantId { get; set; } = Guid.Empty;
    public Guid ChemicalProductId { get; set; } = Guid.Empty;
    public string RevisionNumber { get; set; } = string.Empty;
    public DateOnly? EffectiveDate { get; set; } = null;
    public Guid FileObjectId { get; set; } = Guid.Empty;
    public string? Language { get; set; } = null;
    public string Status { get; set; } = string.Empty;
}
