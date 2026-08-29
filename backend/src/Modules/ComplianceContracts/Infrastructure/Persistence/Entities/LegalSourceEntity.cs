namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>compliance.legal_sources</c> table.</summary>
public sealed class LegalSourceEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Jurisdiction { get; set; }
    public string? Publisher { get; set; }
    public string? SourceUrl { get; set; }
    public string Status { get; set; } = string.Empty;
}
