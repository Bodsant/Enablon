namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.permit_type_versions</c> table.</summary>
public sealed class PermitTypeVersionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PermitTypeId { get; set; }
    public int VersionNumber { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string? ConfigurationJson { get; set; }
    public string Status { get; set; } = string.Empty;
}
