namespace Ehsms.Modules.AssetReporting.Infrastructure.Persistence;

/// <summary>
/// Provides the database schema name for AssetReporting tables if needed.
/// Currently not used directly because each EntityConfiguration supplies its own schema.
/// </summary>
public interface IAssetReportingDbSchema
{
    /// <summary>The schema (PostgreSQL namespace) hosting the AssetReporting tables.</summary>
    string Schema { get; }
}
