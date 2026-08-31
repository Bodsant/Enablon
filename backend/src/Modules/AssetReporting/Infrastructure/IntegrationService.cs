using Ehsms.Modules.AssetReporting.Contracts;
using Ehsms.Modules.AssetReporting.Infrastructure.Persistence;
using Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.AssetReporting.Infrastructure;

/// <summary>
/// Integration &amp; external backend (Trello Sprint 28 R2): integration interfaces,
/// data mappings, runs, messages and reconciliations. Tenant-scoped; plain inserts
/// (no record backing). Owner/approver member ids are optional.
/// </summary>
public sealed class IntegrationService : IIntegrationService
{
    private readonly AssetReportingDbContext _db;

    public IntegrationService(AssetReportingDbContext db)
    {
        _db = db;
    }

    // ---- Interfaces ---------------------------------------------------------

    public async Task<IntegrationInterfaceDto> CreateInterfaceAsync(CreateIntegrationInterfaceRequest request, Guid tenantId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Interface code and name are required.", nameof(request));

        var entity = new IntegrationInterfaceEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            SourceSystem = request.SourceSystem.Trim(),
            TargetSystem = request.TargetSystem.Trim(),
            IntegrationMethod = request.IntegrationMethod.Trim(),
            AuthenticationType = request.AuthenticationType,
            OwnerMemberId = request.OwnerMemberId == Guid.Empty ? null : request.OwnerMemberId,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
        };

        _db.IntegrationInterfaces.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new IntegrationInterfaceDto(entity.Id, entity.Code, entity.Name, entity.SourceSystem,
            entity.TargetSystem, entity.IntegrationMethod, entity.AuthenticationType, entity.OwnerMemberId, entity.Status);
    }

    public async Task<IReadOnlyList<IntegrationInterfaceDto>> ListInterfacesAsync(Guid tenantId, CancellationToken ct)
    {
        var items = await _db.IntegrationInterfaces.Where(i => i.TenantId == tenantId).OrderBy(i => i.Code).ToListAsync(ct);
        return items.Select(i => new IntegrationInterfaceDto(
            i.Id, i.Code, i.Name, i.SourceSystem, i.TargetSystem, i.IntegrationMethod,
            i.AuthenticationType, i.OwnerMemberId, i.Status)).ToList();
    }

    // ---- Data mappings ------------------------------------------------------

    public async Task<IntegrationDataMappingDto> CreateDataMappingAsync(CreateIntegrationDataMappingRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureInterfaceInTenantAsync(tenantId, request.InterfaceId, ct);

        var entity = new IntegrationDataMappingEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InterfaceId = request.InterfaceId,
            VersionNumber = request.VersionNumber,
            SourceSchemaJson = request.SourceSchemaJson,
            TargetSchemaJson = request.TargetSchemaJson,
            MappingRulesJson = request.MappingRulesJson,
            EffectiveFrom = request.EffectiveFrom,
        };

        _db.IntegrationDataMappings.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new IntegrationDataMappingDto(entity.Id, entity.InterfaceId, entity.VersionNumber,
            entity.SourceSchemaJson, entity.TargetSchemaJson, entity.MappingRulesJson, entity.EffectiveFrom);
    }

    public async Task<IReadOnlyList<IntegrationDataMappingDto>> ListDataMappingsAsync(Guid interfaceId, Guid tenantId, CancellationToken ct)
    {
        var items = await _db.IntegrationDataMappings
            .Where(m => m.TenantId == tenantId && m.InterfaceId == interfaceId)
            .OrderBy(m => m.VersionNumber)
            .ToListAsync(ct);
        return items.Select(m => new IntegrationDataMappingDto(
            m.Id, m.InterfaceId, m.VersionNumber, m.SourceSchemaJson, m.TargetSchemaJson,
            m.MappingRulesJson, m.EffectiveFrom)).ToList();
    }

    // ---- Runs ---------------------------------------------------------------

    public async Task<IntegrationRunDto> CreateRunAsync(CreateIntegrationRunRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureInterfaceInTenantAsync(tenantId, request.InterfaceId, ct);

        var entity = new IntegrationRunEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InterfaceId = request.InterfaceId,
            MappingId = request.MappingId,
            CorrelationId = request.CorrelationId,
            StartedAt = DateTimeOffset.UtcNow,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Running" : request.Status.Trim(),
            ReceivedCount = request.ReceivedCount,
            SuccessCount = request.SuccessCount,
            ErrorCount = request.ErrorCount,
        };

        _db.IntegrationRuns.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new IntegrationRunDto(entity.Id, entity.InterfaceId, entity.MappingId, entity.CorrelationId,
            entity.StartedAt, entity.CompletedAt, entity.Status, entity.ReceivedCount, entity.SuccessCount, entity.ErrorCount);
    }

    public async Task<IReadOnlyList<IntegrationRunDto>> ListRunsAsync(Guid interfaceId, Guid tenantId, CancellationToken ct)
    {
        var items = await _db.IntegrationRuns
            .Where(r => r.TenantId == tenantId && r.InterfaceId == interfaceId)
            .OrderByDescending(r => r.StartedAt)
            .ToListAsync(ct);
        return items.Select(r => new IntegrationRunDto(
            r.Id, r.InterfaceId, r.MappingId, r.CorrelationId, r.StartedAt, r.CompletedAt, r.Status,
            r.ReceivedCount, r.SuccessCount, r.ErrorCount)).ToList();
    }

    // ---- Messages -----------------------------------------------------------

    public async Task<IntegrationMessageDto> CreateMessageAsync(CreateIntegrationMessageRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureRunInTenantAsync(tenantId, request.IntegrationRunId, ct);

        var entity = new IntegrationMessageEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            IntegrationRunId = request.IntegrationRunId,
            ExternalKey = request.ExternalKey,
            PayloadHash = request.PayloadHash,
            ProcessingStatus = string.IsNullOrWhiteSpace(request.ProcessingStatus) ? "Pending" : request.ProcessingStatus.Trim(),
            ErrorCode = request.ErrorCode,
            ErrorMessage = request.ErrorMessage,
            RetryCount = request.RetryCount,
        };

        _db.IntegrationMessages.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new IntegrationMessageDto(entity.Id, entity.IntegrationRunId, entity.ExternalKey, entity.PayloadHash,
            entity.ProcessingStatus, entity.ErrorCode, entity.ErrorMessage, entity.RetryCount);
    }

    public async Task<IReadOnlyList<IntegrationMessageDto>> ListMessagesAsync(Guid integrationRunId, Guid tenantId, CancellationToken ct)
    {
        var items = await _db.IntegrationMessages
            .Where(m => m.TenantId == tenantId && m.IntegrationRunId == integrationRunId)
            .OrderBy(m => m.ExternalKey)
            .ToListAsync(ct);
        return items.Select(m => new IntegrationMessageDto(
            m.Id, m.IntegrationRunId, m.ExternalKey, m.PayloadHash, m.ProcessingStatus,
            m.ErrorCode, m.ErrorMessage, m.RetryCount)).ToList();
    }

    // ---- Reconciliations ----------------------------------------------------

    public async Task<IntegrationReconciliationDto> CreateReconciliationAsync(CreateIntegrationReconciliationRequest request, Guid tenantId, CancellationToken ct)
    {
        await EnsureRunInTenantAsync(tenantId, request.IntegrationRunId, ct);

        var entity = new IntegrationReconciliationEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            IntegrationRunId = request.IntegrationRunId,
            SourceCount = request.SourceCount,
            TargetCount = request.TargetCount,
            MatchedCount = request.MatchedCount,
            UnmatchedCount = request.UnmatchedCount,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Open" : request.Status.Trim(),
            ApprovedByMemberId = request.ApprovedByMemberId == Guid.Empty ? null : request.ApprovedByMemberId,
        };

        _db.IntegrationReconciliations.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new IntegrationReconciliationDto(entity.Id, entity.IntegrationRunId, entity.SourceCount,
            entity.TargetCount, entity.MatchedCount, entity.UnmatchedCount, entity.Status, entity.ApprovedByMemberId);
    }

    public async Task<IReadOnlyList<IntegrationReconciliationDto>> ListReconciliationsAsync(Guid integrationRunId, Guid tenantId, CancellationToken ct)
    {
        var items = await _db.IntegrationReconciliations
            .Where(r => r.TenantId == tenantId && r.IntegrationRunId == integrationRunId)
            .OrderBy(r => r.Status)
            .ToListAsync(ct);
        return items.Select(r => new IntegrationReconciliationDto(
            r.Id, r.IntegrationRunId, r.SourceCount, r.TargetCount, r.MatchedCount,
            r.UnmatchedCount, r.Status, r.ApprovedByMemberId)).ToList();
    }

    // ---- Helpers -----------------------------------------------------------

    private async Task EnsureInterfaceInTenantAsync(Guid tenantId, Guid interfaceId, CancellationToken ct)
    {
        var exists = await _db.IntegrationInterfaces.AnyAsync(i => i.TenantId == tenantId && i.Id == interfaceId, ct);
        if (!exists)
            throw new KeyNotFoundException("Integration interface not found in this tenant.");
    }

    private async Task EnsureRunInTenantAsync(Guid tenantId, Guid integrationRunId, CancellationToken ct)
    {
        var exists = await _db.IntegrationRuns.AnyAsync(r => r.TenantId == tenantId && r.Id == integrationRunId, ct);
        if (!exists)
            throw new KeyNotFoundException("Integration run not found in this tenant.");
    }
}