namespace Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;

/// <summary>Represents the <c>iam.access_scopes</c> table. A tenant-bounded access scope over org/contractor entities.</summary>
public sealed class AccessScopeEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ScopeType { get; set; } = string.Empty;
    public Guid? CompanyId { get; set; }
    public Guid? BusinessUnitId { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? ContractorCompanyId { get; set; }
    public Guid? DataClassificationId { get; set; }

    public ICollection<MemberAccessScopeEntity> MemberAccessScopes { get; set; } = new List<MemberAccessScopeEntity>();
    public ICollection<TemporaryAccessGrantEntity> TemporaryAccessGrants { get; set; } = new List<TemporaryAccessGrantEntity>();
}