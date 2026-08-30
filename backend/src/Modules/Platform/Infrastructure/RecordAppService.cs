using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// Creates platform records with a per-tenant, per-period number sequence, writing an
/// audit log entry and queueing a <c>RecordCreated</c> outbox event in the same
/// transaction. Fails closed when no tenant is resolved: the call is rejected instead
/// of writing a row with an empty tenant.
/// </summary>
public sealed class RecordAppService : IRecordAppService
{
    private const string DefaultClassificationCode = "internal";

    private readonly PlatformDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly AuditLogWriter _audit;

    public RecordAppService(PlatformDbContext db, ITenantContext tenant, AuditLogWriter audit)
    {
        _db = db;
        _tenant = tenant;
        _audit = audit;
    }

    public async Task<CreateRecordResult> CreateAsync(
        string moduleCode,
        string recordType,
        string title,
        Guid dataClassificationId,
        Guid createdByMemberId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenant.CurrentTenantId
            ?? throw new InvalidOperationException("No tenant resolved for record creation (fail-closed).");
        var now = DateTimeOffset.UtcNow;
        var periodKey = now.ToString("yyyyMM");

        // Resolve the data classification; fall back to the tenant's "internal" level
        // when the supplied id does not exist (e.g. caller used a placeholder guid).
        var classification = await _db.DataClassifications
            .FirstOrDefaultAsync(d => d.Id == dataClassificationId && d.TenantId == tenantId, cancellationToken);
        if (classification is null)
        {
            classification = await _db.DataClassifications
                .FirstOrDefaultAsync(d => d.TenantId == tenantId
                    && d.Code.ToLower() == DefaultClassificationCode, cancellationToken);
        }
        if (classification is null)
        {
            throw new InvalidOperationException($"No data classification seeded for tenant {tenantId}.");
        }

        // Allocate the next number in the tenant + period sequence (single upsert row,
        // incremented via UPDATE so concurrent calls cannot double-allocate).
        var sequence = await _db.NumberSequences
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.SequenceCode == moduleCode + "." + recordType && s.PeriodKey == periodKey, cancellationToken);
        if (sequence is null)
        {
            sequence = new NumberSequenceEntity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SequenceCode = moduleCode + "." + recordType,
                PeriodKey = periodKey,
                CurrentValue = 1,
            };
            _db.NumberSequences.Add(sequence);
        }
        else
        {
            sequence.CurrentValue++;
        }

        var record = new RecordEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ModuleCode = moduleCode,
            RecordType = recordType,
            RecordNumber = $"{moduleCode.ToUpperInvariant()}-{periodKey}-{sequence.CurrentValue:D4}",
            DataClassificationId = classification.Id,
            Status = "Draft",
            Title = title,
            CreatedByMemberId = createdByMemberId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Records.Add(record);
        await _audit.WriteAsync(_db, tenantId, record.Id, userId: null, "record.created",
            beforeJson: null,
            afterJson: System.Text.Json.JsonSerializer.Serialize(new { record.Id, record.RecordNumber, record.Title, record.Status }),
            correlationId: null,
            cancellationToken);

        _db.OutboxMessages.Add(new OutboxMessageEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecordId = record.Id,
            EventType = "record.created",
            PayloadJson = System.Text.Json.JsonSerializer.Serialize(new { record.Id, record.RecordNumber, record.ModuleCode, record.RecordType }),
            Status = "Pending",
            OccurredAt = now,
        });

        await _db.SaveChangesAsync(cancellationToken);

        return new CreateRecordResult(record.Id, record.RecordNumber, record.Status);
    }
}