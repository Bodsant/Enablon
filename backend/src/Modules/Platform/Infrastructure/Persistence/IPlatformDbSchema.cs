namespace Ehsms.Modules.Platform.Infrastructure.Persistence;

/// <summary>Provides the database schema name used by the Platform module.</summary>
public interface IPlatformDbSchema
{
    /// <summary>The schema (PostgreSQL namespace) hosting the Platform tables, e.g. <c>platform</c>.</summary>
    string Schema { get; }
}