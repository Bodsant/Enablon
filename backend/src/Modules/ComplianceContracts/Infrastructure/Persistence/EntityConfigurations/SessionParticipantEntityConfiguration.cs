using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence.EntityConfigurations;

/// <summary>Configuration for <see cref="SessionParticipantEntity"/> (<c>training.session_participants</c>).</summary>
public sealed class SessionParticipantEntityConfiguration : IEntityTypeConfiguration<SessionParticipantEntity>
{
    public const string TableName = "session_participants";

    public void Configure(EntityTypeBuilder<SessionParticipantEntity> builder)
    {
        builder.ToTable(TableName, "training");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.TrainingSessionId).IsRequired();
        builder.Property(e => e.PersonId).IsRequired();
        builder.Property(e => e.AttendanceStatus).HasMaxLength(30);
        builder.Property(e => e.AssessmentScore).HasPrecision(10, 2);
        builder.Property(e => e.Result).HasMaxLength(30);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_session_participants_tenant_id");
        builder.HasIndex(e => e.TrainingSessionId).HasDatabaseName("ix_session_participants_training_session_id");
        builder.HasIndex(e => e.PersonId).HasDatabaseName("ix_session_participants_person_id");
    }
}
