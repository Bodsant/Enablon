namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>compliance.legal_source_versions</c> table.</summary>
public sealed class LegalSourceVersionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid LegalSourceId { get; set; }
    public string VersionLabel { get; set; } = string.Empty;
    public DateOnly? PublishedDate { get; set; }
    public DateOnly? EffectiveDate { get; set; }
    public DateOnly? SupersededDate { get; set; }
    public string? ChangeSummary { get; set; }
}
