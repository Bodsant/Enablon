namespace Ehsms.Modules.HealthSafety.Contracts;

/// <summary>Payload to create an environment parameter.</summary>
public sealed record CreateEnvironmentParameterRequest(
    string Code,
    string Name,
    string Category,
    string? DefaultUnit = null);

/// <summary>An environment parameter summary.</summary>
public sealed record EnvironmentParameterSummary(
    Guid Id,
    string Code,
    string Name,
    string Category,
    string? DefaultUnit,
    string Status);

/// <summary>Payload to register an environment emission source.</summary>
public sealed record CreateEnvironmentSourceRequest(
    Guid SiteId,
    Guid? LocationId,
    string SourceType,
    string Name,
    string? PermitReference = null);

/// <summary>An environment source summary.</summary>
public sealed record EnvironmentSourceSummary(
    Guid Id,
    Guid SiteId,
    Guid? LocationId,
    string SourceType,
    string Name,
    string? PermitReference);

/// <summary>Payload to record an environment measurement.</summary>
public sealed record RecordEnvironmentMeasurementRequest(
    Guid ParameterId,
    DateTimeOffset? MeasuredAt = null,
    decimal? ResultValue = null,
    string? Unit = null,
    decimal? LimitValue = null,
    decimal? TargetValue = null,
    string? QualityFlag = null);

/// <summary>An environment measurement summary.</summary>
public sealed record EnvironmentMeasurementSummary(
    Guid Id,
    Guid MonitoringRecordId,
    Guid ParameterId,
    DateTimeOffset MeasuredAt,
    decimal? ResultValue,
    string? Unit,
    decimal? LimitValue,
    decimal? TargetValue,
    string? QualityFlag,
    string? ComplianceStatus);

/// <summary>
/// Cross-module contract for environment monitoring in the HealthSafety module.
/// Tenant-scoped and validated against existing parameters / sources.
/// </summary>
public interface IEnvironmentMonitoringService
{
    Task<EnvironmentParameterSummary> CreateParameterAsync(
        CreateEnvironmentParameterRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EnvironmentParameterSummary>> ListParametersAsync(
        string? category = null,
        CancellationToken cancellationToken = default);

    Task<EnvironmentSourceSummary> CreateSourceAsync(
        CreateEnvironmentSourceRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EnvironmentSourceSummary>> ListSourcesAsync(
        Guid? siteId = null,
        CancellationToken cancellationToken = default);

    Task<EnvironmentMeasurementSummary> RecordMeasurementAsync(
        RecordEnvironmentMeasurementRequest request,
        Guid monitoringRecordId,
        Guid createdByMemberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EnvironmentMeasurementSummary>> ListMeasurementsAsync(
        Guid? parameterId = null,
        CancellationToken cancellationToken = default);
}