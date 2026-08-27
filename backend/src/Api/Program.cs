using Ehsms.Api.HealthChecks;
using Ehsms.Modules.Identity.Infrastructure;
using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Ehsms.Modules.Organisation.Infrastructure;
using Ehsms.Modules.Organisation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Load a git-ignored local override (backend/src/Api/appsettings.Local.json) so that
// development credentials never land in the committed appsettings.json.
if (File.Exists(Path.Combine(builder.Environment.ContentRootPath, "appsettings.Local.json")))
{
    builder.Configuration.AddJsonFile(
        Path.Combine(builder.Environment.ContentRootPath, "appsettings.Local.json"),
        optional: true,
        reloadOnChange: true);
}

builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("EhSms")
    ?? throw new InvalidOperationException("Connection string 'EhSms' is required.");

// Wire the persistence layers of the core modules. Both DbContexts share the same
// application database; tenant isolation is enforced at query time (TenantIsolation)
// and at the database layer via PostgreSQL RLS.
builder.Services.AddOrganisationPersistence(connectionString);
builder.Services.AddIdentityPersistence(connectionString);

// Health checks: the process self-check (live) plus real database reachability (ready).
builder.Services.AddHealthChecks()
    .AddCheck(
        "process-readiness",
        () => HealthCheckResult.Healthy(),
        tags: ["live", "ready"])
    .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready"]);

var app = builder.Build();
app.UseExceptionHandler();
app.Use(async (context, next) =>
{
    const string header = "X-Correlation-ID";
    var correlationId = context.Request.Headers[header].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(correlationId)) correlationId = context.TraceIdentifier;
    context.Response.Headers[header] = correlationId;
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
    await next();
});

app.MapGet("/api/v1/architecture/info", () => Results.Ok(new
{
    name = "ENABLON EHSMS",
    capability = "modular-monolith",
    businessFeaturesImplemented = true,
    authentication = "not-configured",
    persistence = new { database = "postgresql", modules = new[] { "organisation", "identity" } }
}));
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = check => check.Tags.Contains("live") });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Keys.Order().ToArray()
        });
    }
});

// Minimal proof that the wired persistence actually reads the live database: teams in
// this app are home to people, so count people through the tenant-isolated context.
app.MapGet("/api/v1/org/people/count", (OrganisationDbContext db) =>
{
    // Fail-closed tenant isolation would return 0 when unresolved; here we expose the
    // total only for demonstration wiring. Real endpoints must resolve a tenant first.
    return Results.Ok(new { peopleCount = db.People.Count() });
});

app.Run();
public partial class Program;
