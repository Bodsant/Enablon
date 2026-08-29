namespace Ehsms.Modules.WorkControl.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>inspection.responses</c> table.</summary>
public sealed class InspectionResponseEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid InspectionId { get; set; }
    public Guid TemplateItemId { get; set; }
    public string? ResponseJson { get; set; }
    public string? ComplianceStatus { get; set; }
    public decimal? Score { get; set; }
    public string? Comment { get; set; }
    public Guid AnsweredByMemberId { get; set; }
}
