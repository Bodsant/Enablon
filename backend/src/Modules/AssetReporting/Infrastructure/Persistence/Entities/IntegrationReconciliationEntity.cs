namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>integration.reconciliations</c> table. Reconciliation results of an integration run.</summary>
public sealed class IntegrationReconciliationEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid IntegrationRunId { get; set; }
    public long? SourceCount { get; set; }
    public long? TargetCount { get; set; }
    public long? MatchedCount { get; set; }
    public long? UnmatchedCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? ApprovedByMemberId { get; set; }

    public IntegrationRunEntity? IntegrationRun { get; set; }
}