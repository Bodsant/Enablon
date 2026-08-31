using Ehsms.Modules.AssetReporting.Contracts;
using Ehsms.Modules.AssetReporting.Infrastructure.Persistence;
using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.AssetReporting.Infrastructure;

/// <summary>
/// Reporting &amp; KPI backend (Trello Sprint 27 R2): report definitions, schedules
/// (owner member), executions, KPI definitions (owner member) and KPI versions.
/// Tenant-scoped; owner/requested member ids come from the resolved active member.
/// </summary>
public sealed class ReportingKpiService : IReportingKpiService
{
    private readonly AssetReportingDbContext _db;

    public ReportingKpiService(AssetReportingDbContext db)
    {
        _db = db;
    }

    // ---- Report definitions -------------------------------------------------

    public async Task<ReportDefinitionDto> CreateReportDefinitionAsync(CreateReportDefinitionRequest request, Guid tenantId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Report code and name are required.", nameof(request));

        var entity = new ReportDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            ReportType = request.ReportType.Trim(),
            DatasetCode = request.DatasetCode.Trim(),
            FilterSchemaJson = request.FilterSchemaJson,
            RequiredPermissionId = request.RequiredPermissionId,
        };

        _db.ReportDefinitions.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new ReportDefinitionDto(entity.Id, entity.Code, entity.Name, entity.ReportType,
            entity.DatasetCode, entity.FilterSchemaJson, entity.RequiredPermissionId);
    }

    public async Task<IReadOnlyList<ReportDefinitionDto>> ListReportDefinitionsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.ReportDefinitions.Where(r => r.TenantId == tenantId).OrderBy(r => r.Code).ToListAsync(ct);
        return items.Select(r => new ReportDefinitionDto(
            r.Id, r.Code, r.Name, r.ReportType, r.DatasetCode, r.FilterSchemaJson, r.RequiredPermissionId)).ToList();
    }

    // ---- Report schedules ---------------------------------------------------

    public async Task<ReportScheduleDto> CreateReportScheduleAsync(
        CreateReportScheduleRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        await EnsureReportDefinitionInTenantAsync(tenantId, request.ReportDefinitionId, ct);

        var entity = new ReportScheduleEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReportDefinitionId = request.ReportDefinitionId,
            OwnerMemberId = request.OwnerMemberId == Guid.Empty ? createdByMemberId : request.OwnerMemberId,
            ScheduleRule = request.ScheduleRule.Trim(),
            DeliveryConfigurationJson = request.DeliveryConfigurationJson,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
        };

        _db.ReportSchedules.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new ReportScheduleDto(entity.Id, entity.ReportDefinitionId, entity.OwnerMemberId,
            entity.ScheduleRule, entity.DeliveryConfigurationJson, entity.Status);
    }

    public async Task<IReadOnlyList<ReportScheduleDto>> ListReportSchedulesAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.ReportSchedules.Where(s => s.TenantId == tenantId).OrderBy(s => s.ScheduleRule).ToListAsync(ct);
        return items.Select(s => new ReportScheduleDto(
            s.Id, s.ReportDefinitionId, s.OwnerMemberId, s.ScheduleRule, s.DeliveryConfigurationJson, s.Status)).ToList();
    }

    // ---- Report executions --------------------------------------------------

    public async Task<ReportExecutionDto> CreateReportExecutionAsync(
        CreateReportExecutionRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        await EnsureReportDefinitionInTenantAsync(tenantId, request.ReportDefinitionId, ct);

        var entity = new ReportExecutionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReportDefinitionId = request.ReportDefinitionId,
            ReportScheduleId = request.ReportScheduleId,
            RequestedByMemberId = request.RequestedByMemberId ?? createdByMemberId,
            FilterValuesJson = request.FilterValuesJson,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Queued" : request.Status.Trim(),
            StartedAt = DateTimeOffset.UtcNow,
        };

        _db.ReportExecutions.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new ReportExecutionDto(entity.Id, entity.ReportDefinitionId, entity.ReportScheduleId,
            entity.RequestedByMemberId, entity.FilterValuesJson, entity.Status, entity.StartedAt, entity.CompletedAt);
    }

    public async Task<IReadOnlyList<ReportExecutionDto>> ListReportExecutionsAsync(Guid reportDefinitionId, Guid tenantId, CancellationToken ct)
    {
        var items = await _db.ReportExecutions
            .Where(e => e.TenantId == tenantId && e.ReportDefinitionId == reportDefinitionId)
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync(ct);
        return items.Select(e => new ReportExecutionDto(
            e.Id, e.ReportDefinitionId, e.ReportScheduleId, e.RequestedByMemberId, e.FilterValuesJson,
            e.Status, e.StartedAt, e.CompletedAt)).ToList();
    }

    // ---- KPI definitions ----------------------------------------------------

    public async Task<KpiDefinitionDto> CreateKpiDefinitionAsync(
        CreateKpiDefinitionRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("KPI code and name are required.", nameof(request));

        var entity = new KpiDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Description = request.Description,
            OwnerMemberId = request.OwnerMemberId == Guid.Empty ? createdByMemberId : request.OwnerMemberId,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
        };

        _db.KpiDefinitions.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new KpiDefinitionDto(entity.Id, entity.Code, entity.Name, entity.Description, entity.OwnerMemberId, entity.Status);
    }

    public async Task<IReadOnlyList<KpiDefinitionDto>> ListKpiDefinitionsAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.KpiDefinitions.Where(k => k.TenantId == tenantId).OrderBy(k => k.Code).ToListAsync(ct);
        return items.Select(k => new KpiDefinitionDto(
            k.Id, k.Code, k.Name, k.Description, k.OwnerMemberId, k.Status)).ToList();
    }

    // ---- KPI versions -------------------------------------------------------

    public async Task<KpiVersionDto> CreateKpiVersionAsync(CreateKpiVersionRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureKpiDefinitionInTenantAsync(tenantId, request.KpiDefinitionId, ct);

        var entity = new KpiVersionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            KpiDefinitionId = request.KpiDefinitionId,
            VersionNumber = request.VersionNumber,
            FormulaExpression = request.FormulaExpression.Trim(),
            NumeratorDefinition = request.NumeratorDefinition,
            DenominatorDefinition = request.DenominatorDefinition,
            Factor = request.Factor,
            PeriodRule = request.PeriodRule,
            ScopeRuleJson = request.ScopeRuleJson,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
        };

        _db.KpiVersions.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new KpiVersionDto(entity.Id, entity.KpiDefinitionId, entity.VersionNumber, entity.FormulaExpression,
            entity.NumeratorDefinition, entity.DenominatorDefinition, entity.Factor, entity.PeriodRule,
            entity.ScopeRuleJson, entity.EffectiveFrom, entity.EffectiveTo);
    }

    public async Task<IReadOnlyList<KpiVersionDto>> ListKpiVersionsAsync(Guid kpiDefinitionId, Guid tenantId, CancellationToken ct)
    {
        var items = await _db.KpiVersions
            .Where(v => v.TenantId == tenantId && v.KpiDefinitionId == kpiDefinitionId)
            .OrderBy(v => v.VersionNumber)
            .ToListAsync(ct);
        return items.Select(v => new KpiVersionDto(
            v.Id, v.KpiDefinitionId, v.VersionNumber, v.FormulaExpression, v.NumeratorDefinition,
            v.DenominatorDefinition, v.Factor, v.PeriodRule, v.ScopeRuleJson, v.EffectiveFrom, v.EffectiveTo)).ToList();
    }

    // ---- Helpers -----------------------------------------------------------

    private async Task EnsureReportDefinitionInTenantAsync(Guid tenantId, Guid reportDefinitionId, CancellationToken ct)
    {
        var exists = await _db.ReportDefinitions.AnyAsync(r => r.TenantId == tenantId && r.Id == reportDefinitionId, ct);
        if (!exists)
            throw new KeyNotFoundException("Report definition not found in this tenant.");
    }

    private async Task EnsureKpiDefinitionInTenantAsync(Guid tenantId, Guid kpiDefinitionId, CancellationToken ct)
    {
        var exists = await _db.KpiDefinitions.AnyAsync(k => k.TenantId == tenantId && k.Id == kpiDefinitionId, ct);
        if (!exists)
            throw new KeyNotFoundException("KPI definition not found in this tenant.");
    }
}