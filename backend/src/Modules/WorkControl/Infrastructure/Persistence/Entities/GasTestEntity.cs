namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.gas_tests</c> table.</summary>
public sealed class GasTestEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PermitId { get; set; }
    public string TestType { get; set; } = string.Empty;
    public DateTimeOffset TestedAt { get; set; }
    public Guid? TestedByPersonId { get; set; }
    public decimal? OxygenPct { get; set; }
    public decimal? LelPct { get; set; }
    public string? ToxicGasJson { get; set; }
    public string Result { get; set; } = string.Empty;
}
