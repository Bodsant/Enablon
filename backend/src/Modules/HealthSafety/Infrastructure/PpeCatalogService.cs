using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.HealthSafety.Contracts;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.HealthSafety.Infrastructure;

/// <summary>
/// PPE catalogue and per-item requirement records, tenant-scoped.
/// </summary>
public sealed class PpeCatalogService : IPpeCatalogService
{
    private readonly HealthSafetyDbContext _db;
    private readonly ITenantContext _tenant;

    public PpeCatalogService(HealthSafetyDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<PpeCatalogSummary> CreateCatalogAsync(
        CreatePpeCatalogRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for PPE catalog.");

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("PPE code and name are required.", nameof(request));

        var catalog = new PpeCatalogEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            PpeCategory = Normalize(request.PpeCategory),
            InspectionIntervalDays = request.InspectionIntervalDays,
            ReplacementIntervalDays = request.ReplacementIntervalDays,
            Status = "Active",
        };

        _db.PpeCatalogs.Add(catalog);
        await _db.SaveChangesAsync(cancellationToken);

        return ToSummary(catalog);
    }

    public async Task<IReadOnlyList<PpeCatalogSummary>> ListCatalogsAsync(
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for PPE catalog.");

        return await _db.PpeCatalogs
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.Code)
            .Select(c => new PpeCatalogSummary(
                c.Id, c.Code, c.Name, c.PpeCategory,
                c.InspectionIntervalDays, c.ReplacementIntervalDays, c.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<PpeRequirementSummary> CreateRequirementAsync(
        CreatePpeRequirementRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for PPE requirement.");

        var catalogExists = await _db.PpeCatalogs
            .AnyAsync(c => c.TenantId == tenantId && c.Id == request.PpeCatalogId, cancellationToken);
        if (!catalogExists)
            throw new KeyNotFoundException("PPE catalog item not found in this tenant.");

        var requirement = new PpeRequirementEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PpeCatalogId = request.PpeCatalogId,
            SourceRecordId = request.SourceRecordId,
            PermitTypeId = request.PermitTypeId,
            IsMandatory = request.IsMandatory,
            Notes = Normalize(request.Notes),
        };

        _db.PpeRequirements.Add(requirement);
        await _db.SaveChangesAsync(cancellationToken);

        return new PpeRequirementSummary(
            requirement.Id,
            requirement.PpeCatalogId,
            requirement.IsMandatory,
            requirement.SourceRecordId,
            requirement.PermitTypeId,
            requirement.Notes);
    }

    public async Task<IReadOnlyList<PpeRequirementSummary>> ListRequirementsAsync(
        Guid? ppeCatalogId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for PPE requirement.");

        var query = _db.PpeRequirements.Where(r => r.TenantId == tenantId);
        if (ppeCatalogId is not null)
            query = query.Where(r => r.PpeCatalogId == ppeCatalogId.Value);

        return await query
            .OrderBy(r => r.PpeCatalogId)
            .Select(r => new PpeRequirementSummary(
                r.Id, r.PpeCatalogId, r.IsMandatory, r.SourceRecordId, r.PermitTypeId, r.Notes))
            .ToListAsync(cancellationToken);
    }

    private static PpeCatalogSummary ToSummary(PpeCatalogEntity e) =>
        new(e.Id, e.Code, e.Name, e.PpeCategory,
            e.InspectionIntervalDays, e.ReplacementIntervalDays, e.Status);

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}