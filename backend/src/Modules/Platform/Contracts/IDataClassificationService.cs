namespace Ehsms.Modules.Platform.Contracts;

/// <summary>Payload to create a data classification level.</summary>
public sealed record CreateDataClassificationRequest(
    string Code,
    string Name,
    short Rank,
    bool IsRestricted);

public sealed record DataClassificationDto(Guid Id, string Code, string Name, short Rank, bool IsRestricted);

/// <summary>Result of classifying a piece of data (used before exposing sensitive data).</summary>
public sealed record ClassificationCheckDto(bool Restricted, string Code, string Name);

/// <summary>Body for the restricted/clearance check.</summary>
public sealed record CheckClassificationRequest(Guid ClassificationId);

/// <summary>
/// Data classification backend (Trello Sprint 34 R3): manage sensitivity levels and
/// provide a restricted/clearance check before exposing potentially sensitive data.
/// </summary>
public interface IDataClassificationService
{
    Task<IReadOnlyList<DataClassificationDto>> ListClassificationsAsync(Guid tenantId, CancellationToken ct);
    Task<DataClassificationDto> CreateClassificationAsync(CreateDataClassificationRequest request, Guid tenantId, CancellationToken ct);
    Task<ClassificationCheckDto> CheckAsync(Guid classificationId, Guid tenantId, CancellationToken ct);
}