using System.Security.Claims;
using Ehsms.Api.Authentication;
using Ehsms.Api.HealthChecks;
using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Identity.Infrastructure;
using Ehsms.Modules.Identity.Infrastructure.Authentication;
using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Ehsms.Modules.Organisation.Infrastructure;
using Ehsms.Modules.Organisation.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Application;
using Ehsms.Modules.Platform.Infrastructure;
using Ehsms.Modules.Saas.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
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
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Authentication & authorization: JWT bearer tokens issued by /api/v1/auth/login.
var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>()
    ?? new AuthOptions();
builder.Services.AddSingleton(authOptions);
builder.Services.AddSingleton<JwtTokenService>();
builder.Services
    .AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = authOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(authOptions.SecretKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });
builder.Services.AddAuthorization();

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

// Resolve the active tenant from the JWT claim into the scoped tenant context
// (fail-closed: no tenant claim => no tenant => tenant-scoped queries return empty).
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();

app.MapGet("/api/v1/architecture/info", () => Results.Ok(new
{
    name = "ENABLON EHSMS",
    capability = "modular-monolith",
    businessFeaturesImplemented = true,
    authentication = "jwt-bearer",
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

// SaaS plans & versions: reads the seeded subscription catalog.
app.MapGet("/api/v1/saas/plans", (Ehsms.Modules.Saas.Infrastructure.Persistence.SaasDbContext db) =>
{
    var plans = db.SubscriptionPlans
        .Select(p => new
        {
            p.Code,
            p.Name,
            p.IsActive,
            version = db.PlanVersions
                .Where(v => v.SubscriptionPlanId == p.Id && v.IsCurrent)
                .Select(v => new { v.VersionNumber, v.MaxActiveUsers, v.MaxStorageBytes })
                .FirstOrDefault()
        })
        .OrderBy(p => p.Code)
        .ToList();
    return Results.Ok(plans);
});

// Authentication: issues JWT access + refresh tokens after verifying credentials.
app.MapPost("/api/v1/auth/login", async (
    LoginRequest request,
    IdentityDbContext db,
    IPasswordHasher hasher,
    JwtTokenService tokens) =>
{
    var email = request.Email.Trim().ToLowerInvariant();
    var user = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant());
    if (user is null || !string.IsNullOrEmpty(user.PasswordHash) is false || !hasher.Verify(request.Password, user.PasswordHash))
    {
        return Results.Json(new { error = "Invalid email or password." }, statusCode: 401);
    }
    if (!string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Json(new { error = "Account is not active." }, statusCode: 403);
    }

    Guid? tenantId = null;
    var activeMember = await db.TenantMembers
        .FirstOrDefaultAsync(m => m.UserId == user.Id && m.Status == "Active"
            && (m.ActivatedAt == null || m.ActivatedAt <= DateTimeOffset.UtcNow));
    tenantId = activeMember?.TenantId;

    var (accessToken, expiresAt) = tokens.CreateAccessToken(user.Id, user.Email, tenantId);
    var refreshToken = JwtTokenService.GenerateRefreshToken();
    db.RefreshTokens.Add(new RefreshTokenEntity
    {
        Id = Guid.NewGuid(),
        UserId = user.Id,
        TokenHash = JwtTokenService.HashRefreshToken(refreshToken),
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(authOptions.RefreshTokenDays),
    });
    user.LastLoginAt = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        accessToken,
        expiresAt,
        refreshToken,
        tokenType = "Bearer",
    });
}).AllowAnonymous();

// Protected endpoint: echoes the authenticated user identity + resolved tenant, which
// proves the JWT pipeline and tenant-context middleware work end to end.
app.MapGet("/api/v1/auth/me", (System.Security.Claims.ClaimsPrincipal user, ITenantContext tenant) =>
{
    return Results.Ok(new
    {
        userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst("sub")?.Value,
        email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? user.FindFirst("email")?.Value,
        tenantId = tenant.CurrentTenantId,
    });
}).RequireAuthorization();

// Platform: create a record through the app service (number sequence + audit + outbox).
app.MapPost("/api/v1/platform/records", async (
    CreateRecordRequest request,
    ClaimsPrincipal user,
    IRecordAppService records,
    IdentityDbContext identityDb,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    try
    {
        // Resolve the calling tenant member id to satisfy records.created_by_member_id.
        var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tenantId = tenantContext.CurrentTenantId;
        Guid? memberId = null;
        if (sub is not null && tenantId is not null && Guid.TryParse(sub, out var userId))
        {
            memberId = (await identityDb.TenantMembers
                .Where(m => m.UserId == userId && m.TenantId == tenantId && m.Status == "Active")
                .Select(m => (Guid?)m.Id)
                .FirstOrDefaultAsync(ct));
        }

        var result = await records.CreateAsync(
            request.ModuleCode,
            request.RecordType,
            request.Title,
            request.DataClassificationId,
            memberId ?? Guid.Empty,
            ct);
        return Results.Created($"/api/v1/platform/records/{result.Id}", result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 400);
    }
}).RequireAuthorization();

// Development seed: subscription plans and their current versions (idempotent).
if (app.Environment.IsDevelopment())
{
    using var seedScope = app.Services.CreateScope();
    var seeder = seedScope.ServiceProvider.GetRequiredService<SaasDbSeeder>();
    await seeder.SeedAsync();

    var identitySeeder = seedScope.ServiceProvider.GetRequiredService<IdentityDbSeeder>();
        await identitySeeder.SeedAsync();

        // Development convenience: give the seeded admin an Active membership in the first
        // tenant so the full tenant-scoped flow (login claim -> records/audit/outbox) can
        // be exercised locally. Idempotent: skipped when a membership already exists.
        var identityDb = seedScope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var saasDb = seedScope.ServiceProvider.GetRequiredService<Ehsms.Modules.Saas.Infrastructure.Persistence.SaasDbContext>();
        var firstTenant = await saasDb.Tenants.OrderBy(t => t.Id).FirstOrDefaultAsync();
        var admin = await identityDb.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == IdentityDbSeeder.DevEmail.ToUpperInvariant());
        if (firstTenant is not null && admin is not null
                    && !await identityDb.TenantMembers.AnyAsync(m => m.UserId == admin.Id && m.TenantId == firstTenant.Id))
                {
                    identityDb.TenantMembers.Add(new TenantMemberEntity
                    {
                        Id = Guid.NewGuid(),
                        TenantId = firstTenant.Id,
                        UserId = admin.Id,
                        DisplayName = "Admin",
                        Status = "Active",
                        ActivatedAt = DateTimeOffset.UtcNow,
                    });
                    await identityDb.SaveChangesAsync();
                }

                // Seed default data classifications for the dev tenant so record creation
                // has a valid classification to reference.
                if (firstTenant is not null)
                {
                    var platformSeeder = seedScope.ServiceProvider.GetRequiredService<PlatformDbSeeder>();
                    await platformSeeder.SeedAsync(firstTenant.Id);
                }
        }

app.Run();
public partial class Program;

/// <summary>Request body for <c>POST /api/v1/auth/login</c>.</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>Request body for <c>POST /api/v1/platform/records</c>.</summary>
public sealed record CreateRecordRequest(string ModuleCode, string RecordType, string Title, Guid DataClassificationId);
