namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>training.session_participants</c> table.</summary>
public sealed class SessionParticipantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TrainingSessionId { get; set; }
    public Guid PersonId { get; set; }
    public string? AttendanceStatus { get; set; }
    public decimal? AssessmentScore { get; set; }
    public string? Result { get; set; }
}
