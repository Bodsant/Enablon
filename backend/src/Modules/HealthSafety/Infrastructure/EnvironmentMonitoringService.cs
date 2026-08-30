using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.HealthSafety.Contracts;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.HealthSafety.Infrastructure;

/// <summary>
/// Environment parameters, emission sources and measurements, tenant-scoped.
/// </summary>
public sealed class EnvironmentMonitoringService : IEnvironmentMonitoringService
{
    private readonly HealthSafetyDbContext _db;
    private readonly ITenantContext _tenant;

    public EnvironmentMonitoringService(HealthSafetyDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<EnvironmentParameterSummary> CreateParameterAsync(
        CreateEnvironmentParameterRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for environment parameters.");

        var code = request.Code.Trim();
        var duplicate = await _db.EnvironmentParameters
            .AnyAsync(p => p.TenantId == tenantId && p.Code == code, cancellationToken);
        if (duplicate)
            throw new InvalidOperationException($"Environment parameter code '{code}' already exists in this tenant.");

        var parameter = new EnvironmentParameterEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = code,
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            DefaultUnit = Normalize(request.DefaultUnit),
            Status = "Active",
        };

        _db.EnvironmentParameters.Add(parameter);
        await _db.SaveChangesAsync(cancellationToken);
        return ToParameterSummary(parameter);
    }

    public async Task<IReadOnlyList<EnvironmentParameterSummary>> ListParametersAsync(
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for environment parameters.");

        var query = _db.EnvironmentParameters.Where(p => p.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category.Trim());

        var items = await query.OrderBy(p => p.Code).ToListAsync(cancellationToken);
        return items.Select(ToParameterSummary).ToList();
    }

    public async Task<EnvironmentSourceSummary> CreateSourceAsync(
        CreateEnvironmentSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for environment sources.");

        var source = new EnvironmentSourceEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SiteId = request.SiteId,
            LocationId = request.LocationId,
            SourceType = request.SourceType.Trim(),
            Name = request.Name.Trim(),
            PermitReference = Normalize(request.PermitReference),
        };

        _db.EnvironmentSources.Add(source);
        await _db.SaveChangesAsync(cancellationToken);
        return ToSourceSummary(source);
    }

    public async Task<IReadOnlyList<EnvironmentSourceSummary>> ListSourcesAsync(
        Guid? siteId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for environment sources.");

        var query = _db.EnvironmentSources.Where(s => s.TenantId == tenantId);
        if (siteId is not null)
            query = query.Where(s => s.SiteId == siteId.Value);

        var items = await query.OrderBy(s => s.Name).ToListAsync(cancellationToken);
        return items.Select(ToSourceSummary).ToList();
    }

    public async Task<EnvironmentMeasurementSummary> RecordMeasurementAsync(
        RecordEnvironmentMeasurementRequest request,
        Guid monitoringRecordId,
        Guid createdByMemberId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for environment measurements.");

        var parameter = await _db.EnvironmentParameters
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ParameterId, cancellationToken)
            ?? throw new KeyNotFoundException("Environment parameter not found in this tenant.");

        var compliance = ComputeCompliance(request.ResultValue, request.LimitValue);

        var measurement = new EnvironmentMeasurementEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MonitoringRecordId = monitoringRecordId,
            ParameterId = request.ParameterId,
            MeasuredAt = request.MeasuredAt ?? DateTimeOffset.UtcNow,
            ResultValue = request.ResultValue,
            Unit = request.Unit ?? parameter.DefaultUnit,
            LimitValue = request.LimitValue,
            TargetValue = request.TargetValue,
            QualityFlag = Normalize(request.QualityFlag),
            ComplianceStatus = compliance,
        };

        _db.EnvironmentMeasurements.Add(measurement);
        await _db.SaveChangesAsync(cancellationToken);
        return ToMeasurementSummary(measurement);
    }

    public async Task<IReadOnlyList<EnvironmentMeasurementSummary>> ListMeasurementsAsync(
        Guid? parameterId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for environment measurements.");

        var query = _db.EnvironmentMeasurements.Where(m => m.TenantId == tenantId);
        if (parameterId is not null)
            query = query.Where(m => m.ParameterId == parameterId.Value);

        var items = await query.OrderByDescending(m => m.MeasuredAt).ToListAsync(cancellationToken);
        return items.Select(ToMeasurementSummary).ToList();
    }

    private static string? ComputeCompliance(decimal? result, decimal? limit)
    {
        if (result is null || limit is null)
            return "NotAssessed";
        return result.Value <= limit.Value ? "Compliant" : "Exceeded";
    }

    private static EnvironmentParameterSummary ToParameterSummary(EnvironmentParameterEntity e) =>
        new(e.Id, e.Code, e.Name, e.Category, e.DefaultUnit, e.Status);

    private static EnvironmentSourceSummary ToSourceSummary(EnvironmentSourceEntity e) =>
        new(e.Id, e.SiteId, e.LocationId, e.SourceType, e.Name, e.PermitReference);

    private static EnvironmentMeasurementSummary ToMeasurementSummary(EnvironmentMeasurementEntity e) =>
        new(e.Id, e.MonitoringRecordId, e.ParameterId, e.MeasuredAt, e.ResultValue, e.Unit,
            e.LimitValue, e.TargetValue, e.QualityFlag, e.ComplianceStatus);

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}