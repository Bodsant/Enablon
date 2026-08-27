namespace Ehsms.Modules.Identity.Infrastructure.Persistence;

/// <summary>Provides the database schema name used by the Identity module.</summary>
public interface IDbContextSchema
{
    /// <summary>The schema (PostgreSQL namespace) hosting the Identity tables, e.g. <c>iam</c>.</summary>
    string Schema { get; }
}