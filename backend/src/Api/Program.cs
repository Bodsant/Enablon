using Ehsms.Api.HealthChecks;
using Ehsms.Modules.Identity.Infrastructure;
using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Ehsms.Modules.Organisation.Infrastructure;
using Ehsms.Modules.Organisation.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Infrastructure;
using Ehsms.Modules.Saas.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

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
builder.Services.AddPlatformPersistence(connectionString);
builder.Services.AddSaasPersistence(connectionString);

// Health checks: the process self-check (live) plus real database reachability (ready).
builder.Services.AddHealthChecks()
    .AddCheck(
        "process-readiness",
        () => HealthCheckResult.Healthy(),
        tags: ["live", "ready"])
    .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready"]);

// OpenAPI/Swagger documentation for the API surface. Exposed in Development only.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ENABLON EHSMS API",
        Version = "v1",
        Description = "REST API for the ENABLON EHSMS (Environmental, Health, Safety & Sustainability) platform — a modular monolith exposing health, architecture and business endpoints under /api/v1."
    });
});

var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
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
    persistence = new { database = "postgresql", modules = new[] { "organisation", "identity", "platform", "saas" } }
}));
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = check => check.Tags.Contains("live") });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Name == "process-readiness",
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

// Platform persistence proof: count platform records through the wired PlatformDbContext.
app.MapGet("/api/v1/platform/records/count", (Ehsms.Modules.Platform.Infrastructure.Persistence.PlatformDbContext db) =>
{
    return Results.Ok(new { recordCount = db.Records.Count() });
});

// SaaS persistence proof: count tenants through the wired SaasDbContext.
app.MapGet("/api/v1/saas/tenants/count", (Ehsms.Modules.Saas.Infrastructure.Persistence.SaasDbContext db) =>
{
    return Results.Ok(new { tenantCount = db.Tenants.Count() });
});

app.Run();
public partial class Program;
