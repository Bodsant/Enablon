using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.HealthSafety.Contracts;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence.Entities;
using Ehsms.Modules.Platform.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.HealthSafety.Infrastructure;

/// <summary>
/// Chemical product catalogue backed by a platform record (so the ledger and the
/// HealthSafety <c>chemical.products</c> row stay consistent) and tenant-scoped.
/// </summary>
public sealed class ChemicalCatalogService : IChemicalCatalogService
{
    private const string DefaultClassificationCode = "internal";

    private readonly HealthSafetyDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IRecordAppService _records;

    public ChemicalCatalogService(
        HealthSafetyDbContext db,
        ITenantContext tenant,
        IRecordAppService records)
    {
        _db = db;
        _tenant = tenant;
        _records = records;
    }

    public async Task<CreateChemicalProductResult> CreateAsync(
        CreateChemicalProductRequest request,
        Guid createdByMemberId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for chemical product creation.");

        if (string.IsNullOrWhiteSpace(request.ProductName))
            throw new ArgumentException("Product name is required.", nameof(request));

        // Create the backing platform record (adapted to the records contract).
        var record = await _records.CreateAsync(
            moduleCode: "CHEM",
            recordType: "ChemicalProduct",
            title: request.ProductName,
            dataClassificationId: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            createdByMemberId: createdByMemberId,
            cancellationToken: cancellationToken);

        var product = new ChemicalProductEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            ProductCode = Normalize(request.ProductCode),
            ProductName = request.ProductName.Trim(),
            SupplierName = Normalize(request.SupplierName),
            HazardClassificationJson = Normalize(request.HazardClassificationJson),
            OwnerMemberId = createdByMemberId == Guid.Empty ? null : createdByMemberId,
            Status = "Active",
        };

        _db.ChemicalProducts.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateChemicalProductResult(product.Id, record.RecordNumber, product.Status);
    }

    public async Task<IReadOnlyList<ChemicalProductSummary>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for chemical product list.");

        return await _db.ChemicalProducts
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.Id)
            .Select(p => new ChemicalProductSummary(
                p.Id,
                p.ProductCode ?? string.Empty,
                p.ProductName,
                p.ProductCode,
                p.SupplierName,
                p.Status))
            .ToListAsync(cancellationToken);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}