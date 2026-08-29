namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>training.worker_competencies</c> table.</summary>
public sealed class WorkerCompetencyEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public Guid CompetencyId { get; set; }
    public string? Level { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
}
