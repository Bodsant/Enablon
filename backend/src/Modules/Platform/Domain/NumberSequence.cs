using Ehsms.BuildingBlocks;

namespace Ehsms.Modules.Platform.Domain;

public class NumberSequence : TenantEntity
{
    public string RecordType { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public int NextValue { get; set; } = 1;
    public int Increment { get; set; } = 1;
    public string? Format { get; set; } // e.g., "{Prefix}-{Year}-{Value:D4}"
}
