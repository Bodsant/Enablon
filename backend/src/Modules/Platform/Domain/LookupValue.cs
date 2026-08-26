using Ehsms.BuildingBlocks;

namespace Ehsms.Modules.Platform.Domain;

public class LookupValue : TenantEntity
{
    public string Category { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ParentCode { get; set; }
}

public class DataClassification
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty; // PUBLIC/INTERNAL/CONFIDENTIAL/RESTRICTED
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsGlobal { get; set; }
}

public class RetentionPolicy
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string RecordType { get; set; } = string.Empty;
    public int RetentionDays { get; set; }
    public bool AllowLegalHold { get; set; }
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
}
