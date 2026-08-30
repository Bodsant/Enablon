using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.HealthSafety.Contracts;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Ehsms.Modules.Platform.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.HealthSafety.Infrastructure;

/// <summary>
/// Chemical storage inspections, tenant-scoped. Each inspection is backed by a
/// platform record (via contract) and validated against an existing chemical
/// inventory line in the tenant.
/// </summary>
public sealed class ChemicalStorageInspectionService : IChemicalStorageInspectionService
{
    private readonly HealthSafetyDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IRecordAppService _records;

    public ChemicalStorageInspectionService(
        HealthSafetyDbContext db,
        ITenantContext tenant,
        IRecordAppService records)
    {
        _db = db;
        _tenant = tenant;
        _records = records;
    }

    public async Task<StorageInspectionSummary> CreateAsync(
        CreateStorageInspectionRequest request,
        Guid createdByMemberId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for storage inspection.");

        if (string.IsNullOrWhiteSpace(request.Result))
            throw new ArgumentException("Inspection result is required.", nameof(request));

        await EnsureInventoryInTenantAsync(tenantId, request.ChemicalInventoryId, cancellationToken);

        var inspectedAt = request.InspectedAt ?? DateTimeOffset.UtcNow;

        // Backing platform record so the inspection is tracked in the record ledger.
        var record = await _records.CreateAsync(
            moduleCode: "CHEM",
            recordType: "StorageInspection",
            title: "Chemical storage inspection",
            dataClassificationId: new Guid("00000000-0000-0000-0000-000000000001"),
            createdByMemberId: createdByMemberId,
            cancellationToken);

        var inspection = new ChemicalStorageInspectionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            ChemicalInventoryId = request.ChemicalInventoryId,
            InspectedByMemberId = createdByMemberId,
            InspectedAt = inspectedAt,
            Result = request.Result.Trim(),
            NextReviewDate = request.NextReviewDate,
        };

        _db.ChemicalStorageInspections.Add(inspection);
        await _db.SaveChangesAsync(cancellationToken);

        return new StorageInspectionSummary(
            inspection.Id,
            record.RecordNumber,
            inspection.ChemicalInventoryId,
            inspection.InspectedByMemberId,
            inspection.InspectedAt,
            inspection.Result,
            inspection.NextReviewDate);
    }

    public async Task<IReadOnlyList<StorageInspectionSummary>> ListAsync(
        Guid? chemicalInventoryId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for storage inspection.");

        var query = _db.ChemicalStorageInspections.Where(i => i.TenantId == tenantId);
        if (chemicalInventoryId is not null)
            query = query.Where(i => i.ChemicalInventoryId == chemicalInventoryId.Value);

        var items = await query.OrderByDescending(i => i.InspectedAt).ToListAsync(cancellationToken);

        return items.Select(i => new StorageInspectionSummary(
            i.Id,
            i.RecordId.ToString("N")[..8].ToUpperInvariant(),
            i.ChemicalInventoryId,
            i.InspectedByMemberId,
            i.InspectedAt,
            i.Result,
            i.NextReviewDate)).ToList();
    }

    private async Task EnsureInventoryInTenantAsync(
        Guid tenantId, Guid inventoryId, CancellationToken ct)
    {
        var exists = await _db.ChemicalInventory
            .AnyAsync(i => i.TenantId == tenantId && i.Id == inventoryId, ct);
        if (!exists)
            throw new KeyNotFoundException("Chemical inventory line not found in this tenant.");
    }
}