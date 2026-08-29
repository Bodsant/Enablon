namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.permit_workers</c> table.</summary>
public sealed class PermitWorkerEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PermitId { get; set; }
    public Guid PersonId { get; set; }
    public string? WorkRole { get; set; }
    public string EligibilityStatus { get; set; } = string.Empty;
}
