namespace Ehsms.Modules.AssetReporting.Contracts;

/// <summary>Payload to register an integration interface.</summary>
public sealed record CreateIntegrationInterfaceRequest(
    string Code,
    string Name,
    string SourceSystem,
    string TargetSystem,
    string IntegrationMethod,
    string? AuthenticationType,
    Guid? OwnerMemberId,
    string Status);

/// <summary>Payload to version the data mapping of an interface.</summary>
public sealed record CreateIntegrationDataMappingRequest(
    Guid InterfaceId,
    int VersionNumber,
    string? SourceSchemaJson,
    string? TargetSchemaJson,
    string? MappingRulesJson,
    DateTimeOffset? EffectiveFrom);

/// <summary>Payload to start an integration run.</summary>
public sealed record CreateIntegrationRunRequest(
    Guid InterfaceId,
    Guid? MappingId,
    string? CorrelationId,
    string Status,
    long? ReceivedCount,
    long? SuccessCount,
    long? ErrorCount);

/// <summary>Payload to record an integration message.</summary>
public sealed record CreateIntegrationMessageRequest(
    Guid IntegrationRunId,
    string? ExternalKey,
    string? PayloadHash,
    string ProcessingStatus,
    string? ErrorCode,
    string? ErrorMessage,
    int RetryCount);

/// <summary>Payload to record a reconciliation.</summary>
public sealed record CreateIntegrationReconciliationRequest(
    Guid IntegrationRunId,
    long? SourceCount,
    long? TargetCount,
    long? MatchedCount,
    long? UnmatchedCount,
    string Status,
    Guid? ApprovedByMemberId);

public sealed record IntegrationInterfaceDto(
    Guid Id,
    string Code,
    string Name,
    string SourceSystem,
    string TargetSystem,
    string IntegrationMethod,
    string? AuthenticationType,
    Guid? OwnerMemberId,
    string Status);

public sealed record IntegrationDataMappingDto(
    Guid Id,
    Guid InterfaceId,
    int VersionNumber,
    string? SourceSchemaJson,
    string? TargetSchemaJson,
    string? MappingRulesJson,
    DateTimeOffset? EffectiveFrom);

public sealed record IntegrationRunDto(
    Guid Id,
    Guid InterfaceId,
    Guid? MappingId,
    string? CorrelationId,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    string Status,
    long? ReceivedCount,
    long? SuccessCount,
    long? ErrorCount);

public sealed record IntegrationMessageDto(
    Guid Id,
    Guid IntegrationRunId,
    string? ExternalKey,
    string? PayloadHash,
    string ProcessingStatus,
    string? ErrorCode,
    string? ErrorMessage,
    int RetryCount);

public sealed record IntegrationReconciliationDto(
    Guid Id,
    Guid IntegrationRunId,
    long? SourceCount,
    long? TargetCount,
    long? MatchedCount,
    long? UnmatchedCount,
    string Status,
    Guid? ApprovedByMemberId);

/// <summary>Integration &amp; external backend service (Trello Sprint 28 R2).</summary>
public interface IIntegrationService
{
    Task<IntegrationInterfaceDto> CreateInterfaceAsync(CreateIntegrationInterfaceRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<IntegrationInterfaceDto>> ListInterfacesAsync(Guid tenantId, CancellationToken ct);

    Task<IntegrationDataMappingDto> CreateDataMappingAsync(CreateIntegrationDataMappingRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<IntegrationDataMappingDto>> ListDataMappingsAsync(Guid interfaceId, Guid tenantId, CancellationToken ct);

    Task<IntegrationRunDto> CreateRunAsync(CreateIntegrationRunRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<IntegrationRunDto>> ListRunsAsync(Guid interfaceId, Guid tenantId, CancellationToken ct);

    Task<IntegrationMessageDto> CreateMessageAsync(CreateIntegrationMessageRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<IntegrationMessageDto>> ListMessagesAsync(Guid integrationRunId, Guid tenantId, CancellationToken ct);

    Task<IntegrationReconciliationDto> CreateReconciliationAsync(CreateIntegrationReconciliationRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<IntegrationReconciliationDto>> ListReconciliationsAsync(Guid integrationRunId, Guid tenantId, CancellationToken ct);
}