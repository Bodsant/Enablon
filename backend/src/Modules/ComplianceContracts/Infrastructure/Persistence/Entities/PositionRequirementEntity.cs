namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>training.position_requirements</c> table.</summary>
public sealed class PositionRequirementEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PositionId { get; set; }
    public Guid CompetencyId { get; set; }
    public Guid? CourseId { get; set; }
    public bool IsMandatory { get; set; }
    public string? MinimumLevel { get; set; }
}
