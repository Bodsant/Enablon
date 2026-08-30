using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.HealthSafety.Contracts;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.HealthSafety.Infrastructure;

/// <summary>
/// Chemical inventory and SDS (safety data sheet) records, tenant-scoped and
/// validated against existing chemical products.
/// </summary>
public sealed class ChemicalInventoryService : IChemicalInventoryService
{
    private readonly HealthSafetyDbContext _db;
    private readonly ITenantContext _tenant;

    public ChemicalInventoryService(HealthSafetyDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<ChemicalInventorySummary> AddInventoryAsync(
        AddInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for chemical inventory.");

        await EnsureProductInTenantAsync(tenantId, request.ChemicalProductId, cancellationToken);

        var inventory = new ChemicalInventoryEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ChemicalProductId = request.ChemicalProductId,
            LocationId = request.LocationId,
            Quantity = request.Quantity,
            Unit = Normalize(request.Unit),
            StorageCondition = Normalize(request.StorageCondition),
            ExpiryDate = request.ExpiryDate,
        };

        _db.ChemicalInventory.Add(inventory);
        await _db.SaveChangesAsync(cancellationToken);

        return ToSummary(inventory);
    }

    public async Task<IReadOnlyList<ChemicalInventorySummary>> ListInventoryAsync(
        Guid? chemicalProductId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for chemical inventory.");

        var query = _db.ChemicalInventory.Where(i => i.TenantId == tenantId);
        if (chemicalProductId is not null)
            query = query.Where(i => i.ChemicalProductId == chemicalProductId.Value);

        var items = await query.OrderBy(i => i.LocationId).ToListAsync(cancellationToken);
        return items.Select(ToSummary).ToList();
    }

    public async Task<SdsRevisionSummary> RecordSdsRevisionAsync(
        RecordSdsRevisionRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for SDS revision.");

        if (string.IsNullOrWhiteSpace(request.RevisionNumber))
            throw new ArgumentException("Revision number is required.", nameof(request));

        await EnsureProductInTenantAsync(tenantId, request.ChemicalProductId, cancellationToken);

        var revision = new SdsRevisionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ChemicalProductId = request.ChemicalProductId,
            RevisionNumber = request.RevisionNumber.Trim(),
            EffectiveDate = request.EffectiveDate,
            FileObjectId = request.FileObjectId,
            Language = Normalize(request.Language),
            Status = "Active",
        };

        _db.SdsRevisions.Add(revision);
        await _db.SaveChangesAsync(cancellationToken);

        return new SdsRevisionSummary(
            revision.Id,
            revision.ChemicalProductId,
            revision.RevisionNumber,
            revision.EffectiveDate,
            revision.Language,
            revision.Status);
    }

    public async Task<IReadOnlyList<SdsRevisionSummary>> ListSdsRevisionsAsync(
        Guid chemicalProductId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for SDS revision.");

        return await _db.SdsRevisions
            .Where(r => r.TenantId == tenantId && r.ChemicalProductId == chemicalProductId)
            .OrderByDescending(r => r.EffectiveDate)
            .Select(r => new SdsRevisionSummary(
                r.Id,
                r.ChemicalProductId,
                r.RevisionNumber,
                r.EffectiveDate,
                r.Language,
                r.Status))
            .ToListAsync(cancellationToken);
    }

    private async Task EnsureProductInTenantAsync(
        Guid tenantId, Guid productId, CancellationToken ct)
    {
        var exists = await _db.ChemicalProducts
            .AnyAsync(p => p.TenantId == tenantId && p.Id == productId, ct);
        if (!exists)
            throw new KeyNotFoundException("Chemical product not found in this tenant.");
    }

    private static ChemicalInventorySummary ToSummary(ChemicalInventoryEntity e) =>
        new(e.Id, e.ChemicalProductId, e.LocationId, e.Quantity, e.Unit, e.StorageCondition, e.ExpiryDate);

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}