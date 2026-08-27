using Ehsms.Modules.Organisation.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ehsms.Modules.Organisation.Infrastructure.Persistence.Configuration;

/// <summary>
/// Relational mapping for the org schema tables, aligned to database/ddl/001-foundation.sql.
/// </summary>
public sealed class OrganisationEntityConfigurations : IEntityTypeConfiguration<CompanyEntity>,
    IEntityTypeConfiguration<BusinessUnitEntity>,
    IEntityTypeConfiguration<SiteEntity>,
    IEntityTypeConfiguration<DepartmentEntity>,
    IEntityTypeConfiguration<LocationEntity>,
    IEntityTypeConfiguration<PositionEntity>,
    IEntityTypeConfiguration<PersonEntity>,
    IEntityTypeConfiguration<EmployeeEntity>
{
    private const string OrgSchema = OrganisationDbContext.Schema;

    public void Configure(EntityTypeBuilder<CompanyEntity> builder)
    {
        builder.ToTable("companies", OrgSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Code).HasMaxLength(40).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.LegalName).HasMaxLength(250);
        builder.Property(e => e.RegistrationNumber).HasMaxLength(100);
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();
    }

    public void Configure(EntityTypeBuilder<BusinessUnitEntity> builder)
    {
        builder.ToTable("business_units", OrgSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Code).HasMaxLength(40).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

        builder.HasOne<CompanyEntity>().WithMany().HasForeignKey(e => e.CompanyId);
        builder.HasOne<BusinessUnitEntity>().WithMany().HasForeignKey(e => e.ParentBusinessUnitId);
    }

    public void Configure(EntityTypeBuilder<SiteEntity> builder)
    {
        builder.ToTable("sites", OrgSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Code).HasMaxLength(40).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Timezone).HasMaxLength(60).IsRequired();
        builder.Property(e => e.Latitude).HasPrecision(10, 7);
        builder.Property(e => e.Longitude).HasPrecision(10, 7);
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

        builder.HasOne<CompanyEntity>().WithMany().HasForeignKey(e => e.CompanyId);
        builder.HasOne<BusinessUnitEntity>().WithMany().HasForeignKey(e => e.BusinessUnitId);
    }

    public void Configure(EntityTypeBuilder<DepartmentEntity> builder)
    {
        builder.ToTable("departments", OrgSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Code).HasMaxLength(40).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

        builder.HasOne<BusinessUnitEntity>().WithMany().HasForeignKey(e => e.BusinessUnitId);
        builder.HasOne<SiteEntity>().WithMany().HasForeignKey(e => e.SiteId);
        builder.HasOne<DepartmentEntity>().WithMany().HasForeignKey(e => e.ParentDepartmentId);
    }

    public void Configure(EntityTypeBuilder<LocationEntity> builder)
    {
        builder.ToTable("locations", OrgSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Code).HasMaxLength(40).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.LocationType).HasMaxLength(60);
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();

        builder.HasOne<SiteEntity>().WithMany().HasForeignKey(e => e.SiteId);
        builder.HasOne<LocationEntity>().WithMany().HasForeignKey(e => e.ParentLocationId);
    }

    public void Configure(EntityTypeBuilder<PositionEntity> builder)
    {
        builder.ToTable("positions", OrgSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();
    }

    public void Configure(EntityTypeBuilder<PersonEntity> builder)
    {
        builder.ToTable("people", OrgSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.PersonType).HasMaxLength(30).IsRequired();
        builder.Property(e => e.FullName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(254);
        builder.Property(e => e.Phone).HasMaxLength(50);
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();
        // data_classification_id -> platform.data_classifications (cross-schema, kept as scalar)
    }

    public void Configure(EntityTypeBuilder<EmployeeEntity> builder)
    {
        builder.ToTable("employees", OrgSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.EmployeeNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.EmploymentStatus).HasMaxLength(30).IsRequired();
        builder.Property(e => e.SourceSystem).HasMaxLength(60);
        builder.Property(e => e.SourceId).HasMaxLength(100);

        builder.HasIndex(e => e.PersonId).IsUnique();
        builder.HasOne<PersonEntity>().WithMany().HasForeignKey(e => e.PersonId);
        builder.HasOne<CompanyEntity>().WithMany().HasForeignKey(e => e.CompanyId);
        builder.HasOne<DepartmentEntity>().WithMany().HasForeignKey(e => e.DepartmentId);
        builder.HasOne<PositionEntity>().WithMany().HasForeignKey(e => e.PositionId);
        builder.HasOne<PersonEntity>().WithMany().HasForeignKey(e => e.ManagerPersonId);
    }
}
