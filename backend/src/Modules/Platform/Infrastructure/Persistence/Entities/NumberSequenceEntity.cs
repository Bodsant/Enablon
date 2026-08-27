namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.number_sequences</c> table. Per-tenant counters for generated numbers.</summary>
public sealed class NumberSequenceEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string SequenceCode { get; set; } = string.Empty;
    public string PeriodKey { get; set; } = string.Empty;
    public long CurrentValue { get; set; }
    public int LockVersion { get; set; }
}