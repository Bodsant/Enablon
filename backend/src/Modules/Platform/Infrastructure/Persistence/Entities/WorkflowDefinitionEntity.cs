namespace Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>platform.workflow_definitions</c> table. Named workflow templates.</summary>
public sealed class WorkflowDefinitionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public ICollection<WorkflowVersionEntity> Versions { get; set; } = new List<WorkflowVersionEntity>();
}