namespace Ehsms.Modules.ComplianceContracts.Contracts;

/// <summary>Payload to create a legal source (regulation / standard).</summary>
public sealed record CreateLegalSourceRequest(
    string SourceType,
    string? Code,
    string Title,
    string? Jurisdiction,
    string? Publisher,
    string? SourceUrl,
    string Status);

/// <summary>Payload to create a version of a legal source.</summary>
public sealed record CreateLegalSourceVersionRequest(
    Guid LegalSourceId,
    string VersionLabel,
    DateOnly? PublishedDate,
    DateOnly? EffectiveDate,
    DateOnly? SupersededDate,
    string? ChangeSummary);

/// <summary>Payload to register a compliance obligation from a legal source version.</summary>
public sealed record CreateObligationRequest(
    Guid LegalSourceVersionId,
    string? ClauseReference,
    string RequirementText,
    Guid OwnerMemberId,
    string? Frequency,
    DateOnly? DueDate,
    DateOnly? LastReview,
    DateOnly? NextReview);

/// <summary>Payload to record where an obligation applies.</summary>
public sealed record CreateObligationApplicabilityRequest(
    Guid ObligationId,
    Guid? CompanyId,
    Guid? BusinessUnitId,
    Guid? SiteId,
    string ApplicabilityStatus,
    string? Rationale,
    Guid AssessedByMemberId);

public sealed record LegalSourceDto(
    Guid Id,
    string SourceType,
    string? Code,
    string Title,
    string? Jurisdiction,
    string? Publisher,
    string? SourceUrl,
    string Status);

public sealed record LegalSourceVersionDto(
    Guid Id,
    Guid LegalSourceId,
    string VersionLabel,
    DateOnly? PublishedDate,
    DateOnly? EffectiveDate,
    DateOnly? SupersededDate,
    string? ChangeSummary);

public sealed record ObligationDto(
    Guid Id,
    string RecordNumber,
    Guid LegalSourceVersionId,
    string? ClauseReference,
    string RequirementText,
    Guid OwnerMemberId,
    string? Frequency,
    DateOnly? DueDate,
    DateOnly? LastReview,
    DateOnly? NextReview);

public sealed record ObligationApplicabilityDto(
    Guid Id,
    Guid ObligationId,
    Guid? CompanyId,
    Guid? BusinessUnitId,
    Guid? SiteId,
    string ApplicabilityStatus,
    string? Rationale,
    Guid AssessedByMemberId);

/// <summary>Legal &amp; compliance backend service (Trello Sprint 25 R2).</summary>
public interface ILegalComplianceService
{
    Task<LegalSourceDto> CreateLegalSourceAsync(CreateLegalSourceRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<LegalSourceDto>> ListLegalSourcesAsync(Guid tenantId, CancellationToken ct);

    Task<LegalSourceVersionDto> CreateLegalSourceVersionAsync(CreateLegalSourceVersionRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<LegalSourceVersionDto>> ListLegalSourceVersionsAsync(Guid legalSourceId, Guid tenantId, CancellationToken ct);

    Task<ObligationDto> CreateObligationAsync(
        CreateObligationRequest request, Guid tenantId, Guid createdByMemberId, CancellationToken ct);
    Task<IReadOnlyList<ObligationDto>> ListObligationsAsync(Guid tenantId, CancellationToken ct);

    Task<ObligationApplicabilityDto> CreateObligationApplicabilityAsync(CreateObligationApplicabilityRequest request, Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<ObligationApplicabilityDto>> ListObligationApplicabilitiesAsync(Guid obligationId, Guid tenantId, CancellationToken ct);
}