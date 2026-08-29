namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>inspection.template_items</c> table.</summary>
public sealed class InspectionTemplateItemEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SectionId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public decimal? Weight { get; set; }
    public string? CriteriaJson { get; set; }
    public int SequenceNumber { get; set; }
}
