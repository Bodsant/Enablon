using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.HealthSafety.Contracts;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.HealthSafety.Infrastructure;

/// <summary>
/// Chemical exposure control records, tenant-scoped and validated against
/// existing chemical products.
/// </summary>
public sealed class ChemicalExposureControlService : IChemicalExposureControlService
{
    private readonly HealthSafetyDbContext _db;
    private readonly ITenantContext _tenant;

    public ChemicalExposureControlService(HealthSafetyDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<ExposureControlSummary> AddAsync(
        CreateExposureControlRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for exposure control.");

        if (string.IsNullOrWhiteSpace(request.ControlType))
            throw new ArgumentException("Control type is required.", nameof(request));

        await EnsureProductInTenantAsync(tenantId, request.ChemicalProductId, cancellationToken);

        var control = new ChemicalExposureControlEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ChemicalProductId = request.ChemicalProductId,
            ControlType = request.ControlType.Trim(),
            Description = request.Description.Trim(),
            SourceRecordId = request.SourceRecordId,
        };

        _db.ChemicalExposureControls.Add(control);
        await _db.SaveChangesAsync(cancellationToken);

        return ToSummary(control);
    }

    public async Task<IReadOnlyList<ExposureControlSummary>> ListAsync(
        Guid chemicalProductId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for exposure control.");

        return await _db.ChemicalExposureControls
            .Where(c => c.TenantId == tenantId && c.ChemicalProductId == chemicalProductId)
            .OrderBy(c => c.ControlType)
            .Select(c => new ExposureControlSummary(
                c.Id,
                c.ChemicalProductId,
                c.ControlType,
                c.Description,
                c.SourceRecordId))
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

    private static ExposureControlSummary ToSummary(ChemicalExposureControlEntity e) =>
        new(e.Id, e.ChemicalProductId, e.ControlType, e.Description, e.SourceRecordId);
}