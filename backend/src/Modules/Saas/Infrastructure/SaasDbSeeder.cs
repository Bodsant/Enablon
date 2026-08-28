using Ehsms.Modules.Saas.Infrastructure.Persistence;
using Ehsms.Modules.Saas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Saas.Infrastructure;

/// <summary>
/// Idempotent development seed for the SaaS module: subscription plans and their
/// current plan versions. Upserts per plan <c>Code</c>, so running against a
/// database that already contains partial data (e.g. an existing ENTERPRISE plan)
/// fills in the missing plans without touching or duplicating existing rows.
/// </summary>
public sealed class SaasDbSeeder
{
    private readonly SaasDbContext _db;

    public SaasDbSeeder(SaasDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var existingPlans = await _db.SubscriptionPlans.ToDictionaryAsync(p => p.Code, cancellationToken);

        var definitions = new[]
        {
            NewDefinition("STARTER", "Starter",
                "For small teams getting started with EHS fundamentals.",
                maxUsers: 25, maxCompanies: 1, maxSites: 3,
                storageBytes: 10L * 1024 * 1024 * 1024, periodUploadBytes: 2L * 1024 * 1024 * 1024,
                fileSizeBytes: 50L * 1024 * 1024),
            NewDefinition("PROFESSIONAL", "Professional",
                "For growing organisations needing full HSE workflows.",
                maxUsers: 150, maxCompanies: 5, maxSites: 20,
                storageBytes: 100L * 1024 * 1024 * 1024, periodUploadBytes: 20L * 1024 * 1024 * 1024,
                fileSizeBytes: 200L * 1024 * 1024),
            NewDefinition("ENTERPRISE", "Enterprise",
                "For large multi-site organisations with advanced compliance needs.",
                maxUsers: 1000, maxCompanies: null, maxSites: null,
                storageBytes: 500L * 1024 * 1024 * 1024, periodUploadBytes: 100L * 1024 * 1024 * 1024,
                fileSizeBytes: 500L * 1024 * 1024),
        };

        foreach (var def in definitions)
        {
            // Upsert plan by code.
            if (!existingPlans.TryGetValue(def.Code, out var plan))
            {
                plan = new SubscriptionPlan
                {
                    Id = Guid.NewGuid(),
                    Code = def.Code,
                    Name = def.Name,
                    Description = def.Description,
                    IsActive = true,
                };
                _db.SubscriptionPlans.Add(plan);
                existingPlans[def.Code] = plan;
            }
            else if (plan.Name != def.Name || plan.Description != def.Description)
            {
                plan.Name = def.Name;
                plan.Description = def.Description;
            }
        }

        // Persist plans first so their IDs exist before any plan_version rows reference them.
        await _db.SaveChangesAsync(cancellationToken);

        var versionByPlan = await _db.PlanVersions
            .Where(v => v.IsCurrent)
            .ToDictionaryAsync(v => v.SubscriptionPlanId, cancellationToken);

        foreach (var def in definitions)
        {
            var plan = existingPlans[def.Code];

            // Upsert current version for the plan.
            if (!versionByPlan.TryGetValue(plan.Id, out var version))
            {
                version = new PlanVersion
                {
                    Id = Guid.NewGuid(),
                    SubscriptionPlanId = plan.Id,
                    VersionNumber = 1,
                    MaxActiveUsers = def.MaxUsers,
                    MaxCompanies = def.MaxCompanies,
                    MaxBusinessUnits = null,
                    MaxSites = def.MaxSites,
                    MaxStorageBytes = def.StorageBytes,
                    MaxPeriodUploadBytes = def.PeriodUploadBytes,
                    MaxFileSizeBytes = def.FileSizeBytes,
                    EffectiveFrom = now,
                    EffectiveUntil = null,
                    IsCurrent = true,
                };
                _db.PlanVersions.Add(version);
                versionByPlan[plan.Id] = version;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static (string Code, string Name, string Description, int MaxUsers, int? MaxCompanies, int? MaxSites, long StorageBytes, long PeriodUploadBytes, long FileSizeBytes) NewDefinition(
        string code, string name, string description,
        int maxUsers, int? maxCompanies, int? maxSites,
        long storageBytes, long periodUploadBytes, long fileSizeBytes)
    {
        return (code, name, description, maxUsers, maxCompanies, maxSites, storageBytes, periodUploadBytes, fileSizeBytes);
    }
}