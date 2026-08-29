namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>emergency.drill_participants</c> table. Participants of an emergency drill.</summary>
public sealed class EmergencyDrillParticipantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmergencyDrillId { get; set; }
    public Guid PersonId { get; set; }
    public string? ParticipantRole { get; set; }
    public string? AttendanceStatus { get; set; }

    public EmergencyDrillEntity? EmergencyDrill { get; set; }
}