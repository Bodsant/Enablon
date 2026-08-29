namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>integration.messages</c> table. Individual messages processed by an integration run.</summary>
public sealed class IntegrationMessageEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid IntegrationRunId { get; set; }
    public string? ExternalKey { get; set; }
    public string? PayloadHash { get; set; }
    public string ProcessingStatus { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }

    public IntegrationRunEntity? IntegrationRun { get; set; }
}