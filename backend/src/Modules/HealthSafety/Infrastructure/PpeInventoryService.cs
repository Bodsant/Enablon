using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.HealthSafety.Contracts;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.HealthSafety.Infrastructure;

/// <summary>
/// PPE inventory and assignment records, tenant-scoped.
/// </summary>
public sealed class PpeInventoryService : IPpeInventoryService
{
    private readonly HealthSafetyDbContext _db;
    private readonly ITenantContext _tenant;

    public PpeInventoryService(HealthSafetyDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<PpeInventorySummary> RegisterInventoryAsync(
        RegisterPpeInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for PPE inventory.");

        await EnsureCatalogInTenantAsync(tenantId, request.PpeCatalogId, cancellationToken);

        var inventory = new PpeInventoryEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PpeCatalogId = request.PpeCatalogId,
            SiteId = request.SiteId,
            SerialNumber = Normalize(request.SerialNumber),
            QuantityOnHand = request.QuantityOnHand,
            Condition = Normalize(request.Condition),
            Status = "Available",
        };

        _db.PpeInventory.Add(inventory);
        await _db.SaveChangesAsync(cancellationToken);

        return ToSummary(inventory);
    }

    public async Task<IReadOnlyList<PpeInventorySummary>> ListInventoryAsync(
        Guid? ppeCatalogId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for PPE inventory.");

        var query = _db.PpeInventory.Where(i => i.TenantId == tenantId);
        if (ppeCatalogId is not null)
            query = query.Where(i => i.PpeCatalogId == ppeCatalogId.Value);

        var items = await query.OrderBy(i => i.SerialNumber).ToListAsync(cancellationToken);
        return items.Select(ToSummary).ToList();
    }

    public async Task<PpeAssignmentSummary> AssignAsync(
        AssignPpeRequest request,
        Guid issuedByMemberId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for PPE assignment.");

        var inventory = await _db.PpeInventory
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == request.PpeInventoryId, cancellationToken)
            ?? throw new KeyNotFoundException("PPE inventory item not found in this tenant.");

        var assignment = new PpeAssignmentEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PpeInventoryId = request.PpeInventoryId,
            PersonId = request.PersonId,
            IssuedAt = request.IssuedAt ?? DateTimeOffset.UtcNow,
            IssuedByMemberId = issuedByMemberId,
            ReturnedAt = null,
            ConditionOnReturn = null,
        };

        inventory.Status = "Issued";
        _db.PpeAssignments.Add(assignment);
        await _db.SaveChangesAsync(cancellationToken);

        return ToAssignmentSummary(assignment);
    }

    public async Task<PpeAssignmentSummary?> ReturnAsync(
        ReturnPpeRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for PPE return.");

        var assignment = await _db.PpeAssignments
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == request.AssignmentId, cancellationToken)
            ?? throw new KeyNotFoundException("PPE assignment not found in this tenant.");

        if (assignment.ReturnedAt is null)
        {
            assignment.ReturnedAt = request.ReturnedAt ?? DateTimeOffset.UtcNow;
            assignment.ConditionOnReturn = Normalize(request.ConditionOnReturn);

            var inventory = await _db.PpeInventory
                .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == assignment.PpeInventoryId, cancellationToken);
            if (inventory is not null)
                inventory.Status = "Available";

            await _db.SaveChangesAsync(cancellationToken);
        }

        return ToAssignmentSummary(assignment);
    }

    public async Task<IReadOnlyList<PpeAssignmentSummary>> ListAssignmentsAsync(
        Guid? ppeInventoryId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for PPE assignment.");

        var query = _db.PpeAssignments.Where(a => a.TenantId == tenantId);
        if (ppeInventoryId is not null)
            query = query.Where(a => a.PpeInventoryId == ppeInventoryId.Value);

        var items = await query.OrderByDescending(a => a.IssuedAt).ToListAsync(cancellationToken);
        return items.Select(ToAssignmentSummary).ToList();
    }

    private async Task EnsureCatalogInTenantAsync(Guid tenantId, Guid catalogId, CancellationToken ct)
    {
        var exists = await _db.PpeCatalogs
            .AnyAsync(c => c.TenantId == tenantId && c.Id == catalogId, ct);
        if (!exists)
            throw new KeyNotFoundException("PPE catalog item not found in this tenant.");
    }

    private static PpeInventorySummary ToSummary(PpeInventoryEntity e) =>
        new(e.Id, e.PpeCatalogId, e.SiteId, e.SerialNumber, e.QuantityOnHand, e.Condition, e.Status);

    private static PpeAssignmentSummary ToAssignmentSummary(PpeAssignmentEntity e) =>
        new(e.Id, e.PpeInventoryId, e.PersonId, e.IssuedAt, e.IssuedByMemberId, e.ReturnedAt, e.ConditionOnReturn);

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}