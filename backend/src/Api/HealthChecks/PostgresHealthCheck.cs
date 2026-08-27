using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Ehsms.Modules.Organisation.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ehsms.Api.HealthChecks;

/// <summary>
/// Readiness probe that verifies the application can actually reach PostgreSQL through
/// both wired module contexts. Readiness now reflects a real database dependency rather
/// than the old process-only claim.
/// </summary>
public sealed class PostgresHealthCheck : IHealthCheck
{
    private readonly OrganisationDbContext _org;
    private readonly IdentityDbContext _identity;

    public PostgresHealthCheck(OrganisationDbContext org, IdentityDbContext identity)
    {
        _org = org;
        _identity = identity;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var orgOk = await _org.Database.CanConnectAsync(cancellationToken);
        var identityOk = await _identity.Database.CanConnectAsync(cancellationToken);

        if (orgOk && identityOk)
        {
            return HealthCheckResult.Healthy("PostgreSQL reachable via organisation and identity contexts.");
        }

        var failures = new List<string>();
        if (!orgOk) failures.Add("org");
        if (!identityOk) failures.Add("iam");
        return HealthCheckResult.Unhealthy($"PostgreSQL unreachable via: {string.Join(", ", failures)}");
    }
}
