namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>cow.jsa_step_hazards</c> table.</summary>
public sealed class JsaStepHazardEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid JsaStepId { get; set; }
    public Guid? HazardId { get; set; }
    public string Consequence { get; set; } = string.Empty;
    public string? ExistingControl { get; set; }
    public string? AdditionalControl { get; set; }
    public string? InitialRiskLevel { get; set; }
    public string? ResidualRiskLevel { get; set; }
    public Guid? ResponsibleMemberId { get; set; }
}
