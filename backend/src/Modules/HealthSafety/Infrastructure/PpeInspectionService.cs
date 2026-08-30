using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.HealthSafety.Contracts;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.HealthSafety.Infrastructure;

/// <summary>
/// PPE inspections and replacement requests, tenant-scoped.
/// </summary>
public sealed class PpeInspectionService : IPpeInspectionService
{
    private readonly HealthSafetyDbContext _db;
    private readonly ITenantContext _tenant;

    public PpeInspectionService(HealthSafetyDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<PpeInspectionSummary> RecordInspectionAsync(
        RecordPpeInspectionRequest request,
        Guid inspectedByMemberId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for PPE inspection.");

        var inventory = await _db.PpeInventory
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == request.PpeInventoryId, cancellationToken)
            ?? throw new KeyNotFoundException("PPE inventory item not found in this tenant.");

        var inspection = new PpeInspectionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PpeInventoryId = request.PpeInventoryId,
            InspectedByMemberId = inspectedByMemberId,
            InspectedAt = request.InspectedAt ?? DateTimeOffset.UtcNow,
            Condition = request.Condition,
            Result = request.Result,
            NextDueDate = request.NextDueDate,
        };

        _db.PpeInspections.Add(inspection);

        // A failed/defective inspection flips inventory status so it is not issued.
        if (request.Result.Equals("Failed", StringComparison.OrdinalIgnoreCase)
            || request.Result.Equals("Defective", StringComparison.OrdinalIgnoreCase))
        {
            inventory.Status = "NeedsReplacement";
            inventory.Condition = request.Condition;
        }
        else
        {
            inventory.Condition = request.Condition;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return ToInspectionSummary(inspection);
    }

    public async Task<IReadOnlyList<PpeInspectionSummary>> ListInspectionsAsync(
        Guid? ppeInventoryId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for PPE inspection.");

        var query = _db.PpeInspections.Where(i => i.TenantId == tenantId);
        if (ppeInventoryId is not null)
            query = query.Where(i => i.PpeInventoryId == ppeInventoryId.Value);

        var items = await query.OrderByDescending(i => i.InspectedAt).ToListAsync(cancellationToken);
        return items.Select(ToInspectionSummary).ToList();
    }

    public async Task<PpeReplacementSummary> RequestReplacementAsync(
        RequestPpeReplacementRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for PPE replacement.");

        var assignment = await _db.PpeAssignments
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == request.PpeAssignmentId, cancellationToken)
            ?? throw new KeyNotFoundException("PPE assignment not found in this tenant.");

        var replacement = new PpeReplacementEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PpeAssignmentId = request.PpeAssignmentId,
            ReplacementReason = request.ReplacementReason,
            RequestedAt = request.RequestedAt ?? DateTimeOffset.UtcNow,
            CompletedAt = null,
        };

        _db.PpeReplacements.Add(replacement);
        await _db.SaveChangesAsync(cancellationToken);
        return ToReplacementSummary(replacement);
    }

    public async Task<PpeReplacementSummary?> CompleteReplacementAsync(
        Guid replacementId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for PPE replacement.");

        var replacement = await _db.PpeReplacements
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == replacementId, cancellationToken)
            ?? throw new KeyNotFoundException("PPE replacement request not found in this tenant.");

        if (replacement.CompletedAt is null)
        {
            replacement.CompletedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return ToReplacementSummary(replacement);
    }

    public async Task<IReadOnlyList<PpeReplacementSummary>> ListReplacementsAsync(
        Guid? ppeAssignmentId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for PPE replacement.");

        var query = _db.PpeReplacements.Where(r => r.TenantId == tenantId);
        if (ppeAssignmentId is not null)
            query = query.Where(r => r.PpeAssignmentId == ppeAssignmentId.Value);

        var items = await query.OrderByDescending(r => r.RequestedAt).ToListAsync(cancellationToken);
        return items.Select(ToReplacementSummary).ToList();
    }

    private static PpeInspectionSummary ToInspectionSummary(PpeInspectionEntity e) =>
        new(e.Id, e.PpeInventoryId, e.InspectedByMemberId, e.InspectedAt, e.Condition, e.Result, e.NextDueDate);

    private static PpeReplacementSummary ToReplacementSummary(PpeReplacementEntity e) =>
        new(e.Id, e.PpeAssignmentId, e.ReplacementReason, e.RequestedAt, e.CompletedAt);
}