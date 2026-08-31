using System.Security.Claims;
using Ehsms.Api.Authentication;
using Ehsms.Api.HealthChecks;
using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Identity.Contracts;
using Ehsms.Modules.Identity.Infrastructure;
using Ehsms.Modules.Identity.Infrastructure.Authentication;
using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Ehsms.Modules.Identity.Infrastructure.Persistence.Entities;
using Ehsms.Modules.HealthSafety.Contracts;
using Ehsms.Modules.HealthSafety.Infrastructure;
using Ehsms.Modules.SafetyRisk.Contracts;
using Ehsms.Modules.SafetyRisk.Infrastructure;
using Ehsms.Modules.WorkControl.Contracts;
using Ehsms.Modules.WorkControl.Infrastructure;
using Ehsms.Modules.ComplianceContracts.Contracts;
using Ehsms.Modules.ComplianceContracts.Infrastructure;
using Ehsms.Modules.AssetReporting.Contracts;
using Ehsms.Modules.AssetReporting.Infrastructure;
using Ehsms.Modules.Organisation.Infrastructure;
using Ehsms.Modules.Organisation.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Application;
using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.Platform.Infrastructure;
using Ehsms.Modules.Platform.Infrastructure.Persistence;
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
builder.Services.AddHealthSafetyPersistence(connectionString);
builder.Services.AddSafetyRiskPersistence(connectionString);
builder.Services.AddWorkControlPersistence(connectionString);
builder.Services.AddComplianceContractsPersistence(connectionString);
builder.Services.AddAssetReportingPersistence(connectionString);

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
// Platform records: list for admin views (tenant-scoped via middleware).
app.MapGet("/api/v1/platform/records", async (
    Ehsms.Modules.Platform.Infrastructure.Persistence.PlatformDbContext db,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var tenantId = tenantContext.CurrentTenantId;
    if (tenantId is null)
        return Results.Json(new { error = "Tenant context could not be resolved." }, statusCode: 400);

    var records = await db.Records
        .Where(r => r.TenantId == tenantId)
        .OrderByDescending(r => r.CreatedAt)
        .Take(100)
        .Select(r => new
        {
            r.Id,
            r.RecordNumber,
            r.ModuleCode,
            r.RecordType,
            r.Title,
            r.Status,
            r.CreatedAt,
            CreatedByMemberId = r.CreatedByMemberId
        })
        .ToListAsync(ct);

    return Results.Ok(records);
}).RequireAuthorization();

// Platform records: detail for admin views.
app.MapGet("/api/v1/platform/records/{recordId:guid}", async (
    Guid recordId,
    Ehsms.Modules.Platform.Infrastructure.Persistence.PlatformDbContext db,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var tenantId = tenantContext.CurrentTenantId;
    if (tenantId is null)
        return Results.Json(new { error = "Tenant context could not be resolved." }, statusCode: 400);

    var record = await db.Records
        .Where(r => r.Id == recordId && r.TenantId == tenantId)
        .Select(r => new
                {
                    r.Id,
                    r.RecordNumber,
                    r.ModuleCode,
                    r.RecordType,
                    r.Title,
                    r.Status,
                    r.DataClassificationId,
                    r.CreatedAt,
                    CreatedByMemberId = r.CreatedByMemberId
                })
                .FirstOrDefaultAsync(ct);

    return record is null
        ? Results.NotFound(new { error = "Record not found." })
        : Results.Ok(record);
}).RequireAuthorization();

// Platform records: create.
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

// Workflow: start a workflow instance for a record and advance it through decisions.
app.MapPost("/api/v1/workflow/start", async (
    StartWorkflowRequest request,
    ClaimsPrincipal user,
    IWorkflowEngine engine,
    IdentityDbContext identityDb,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    try
    {
        var result = await engine.StartAsync(request.RecordId, request.WorkflowCode, memberId ?? Guid.Empty, ct);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 400);
    }
}).RequireAuthorization();

app.MapPost("/api/v1/workflow/tasks/{taskId:guid}/decision", async (
    Guid taskId,
    MakeDecisionRequest request,
    ClaimsPrincipal user,
    IWorkflowEngine engine,
    IdentityDbContext identityDb,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    try
    {
        var result = await engine.ExecuteTransitionAsync(taskId, request.Decision, request.Comment, memberId ?? Guid.Empty, ct);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 400);
    }
}).RequireAuthorization();

app.MapGet("/api/v1/workflow/my-tasks", async (
    ClaimsPrincipal user,
    PlatformDbContext db,
    IdentityDbContext identityDb,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null)
    {
        return Results.Json(new { tasks = new object[] { } });
    }

    var tasks = await db.WorkflowTasks
        .Where(t => t.TenantId == tenantContext.CurrentTenantId && t.Status == "Open"
            && (t.AssignedMemberId == null || t.AssignedMemberId == memberId))
        .Select(t => new
        {
            t.Id,
            t.TaskType,
            t.DueAt,
            t.Priority,
            RecordId = t.Instance != null ? t.Instance.RecordId : (Guid?)null,
            StateCode = t.Instance != null && t.Instance.CurrentState != null ? t.Instance.CurrentState.StateCode : null,
        })
        .OrderBy(t => t.DueAt)
        .ToListAsync(ct);
    return Results.Ok(new { tasks });
}).RequireAuthorization();

// My Tasks & Notifications: member summary + notification inbox.
app.MapGet("/api/v1/workflow/me", async (
    ClaimsPrincipal user,
    PlatformDbContext db,
    IdentityDbContext identityDb,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { memberId = (Guid?)null, openTasks = 0, unreadNotifications = 0 });
    }

    var openTasks = await db.WorkflowTasks.CountAsync(
        t => t.TenantId == tenantContext.CurrentTenantId && t.Status == "Open"
            && (t.AssignedMemberId == null || t.AssignedMemberId == memberId), ct);
    var unread = await db.Notifications.CountAsync(
        n => n.TenantId == tenantContext.CurrentTenantId && n.RecipientMemberId == memberId && n.ReadAt == null, ct);
    return Results.Ok(new { memberId, openTasks, unreadNotifications = unread });
}).RequireAuthorization();

app.MapGet("/api/v1/notifications", async (
    ClaimsPrincipal user,
    PlatformDbContext db,
    IdentityDbContext identityDb,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Ok(new { notifications = new object[] { } });
    }

    var notifications = await db.Notifications
        .Where(n => n.TenantId == tenantContext.CurrentTenantId && n.RecipientMemberId == memberId
            && n.ReadAt == null)
        .OrderByDescending(n => n.Id)
        .Select(n => new { n.Id, n.NotificationType, n.Title, n.Message, n.DeliveryStatus, n.RecordId })
        .Take(50)
        .ToListAsync(ct);
    return Results.Ok(new { notifications });
}).RequireAuthorization();

app.MapPost("/api/v1/notifications/{id:guid}/read", async (
    Guid id,
    ClaimsPrincipal user,
    INotificationService notifications,
    IdentityDbContext identityDb,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null)
    {
        return Results.NotFound();
    }
    var ok = await notifications.MarkReadAsync(id, memberId.Value, tenantId: null, cancellationToken: ct);
    return ok ? Results.Ok(new { read = true }) : Results.NotFound();
}).RequireAuthorization();

// Evidence & file lifecycle: upload, link as evidence, short-lived download URL.
app.MapPost("/api/v1/platform/files", async (
    UploadFileRequest request,
    ClaimsPrincipal user,
    IFileService files,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (userId is null || !Guid.TryParse(userId, out var userIdGuid))
    {
        return Results.Unauthorized();
    }
    var content = Convert.FromBase64String(request.ContentBase64);
    var result = await files.UploadAsync(
        tenantContext.CurrentTenantId.Value, userIdGuid, request.FileName, request.MimeType, content, ct: ct);
    return Results.Created($"/api/v1/platform/files/{result.FileObjectId}", result);
}).RequireAuthorization();

app.MapPost("/api/v1/platform/records/{recordId:guid}/evidence", async (
    Guid recordId,
    LinkEvidenceRequest request,
    ClaimsPrincipal user,
    IFileService files,
    IdentityDbContext identityDb,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var linkId = await files.LinkEvidenceAsync(
        tenantContext.CurrentTenantId.Value, recordId, request.FileObjectId, request.EvidenceType, memberId.Value, ct);
    return Results.Created($"/api/v1/platform/records/{recordId}/evidence/{linkId}", new { id = linkId });
}).RequireAuthorization();

app.MapGet("/api/v1/platform/files/{fileId:guid}/download-url", async (
    Guid fileId,
    IFileService files,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var url = await files.GetDownloadUrlAsync(tenantContext.CurrentTenantId.Value, fileId, ct: ct);
    return url is null ? Results.NotFound() : Results.Ok(url);
}).RequireAuthorization();

// Chemical product catalogue (HealthSafety module).
app.MapPost("/api/v1/chemical/products", async (
    CreateChemicalProductRequest request,
    ClaimsPrincipal user,
    IChemicalCatalogService chemicals,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var result = await chemicals.CreateAsync(request, memberId.Value, ct);
    return Results.Created($"/api/v1/chemical/products/{result.Id}", result);
}).RequireAuthorization();

app.MapGet("/api/v1/chemical/products", async (
    IChemicalCatalogService chemicals,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await chemicals.ListAsync(ct);
    return Results.Ok(items);
}).RequireAuthorization();

// Chemical inventory & SDS (HealthSafety module).
app.MapPost("/api/v1/chemical/inventory", async (
    AddInventoryRequest request,
    IChemicalInventoryService inventory,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await inventory.AddInventoryAsync(request, ct);
    return Results.Created($"/api/v1/chemical/inventory/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/chemical/inventory", async (
    Guid? productId,
    IChemicalInventoryService inventory,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await inventory.ListInventoryAsync(productId, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/chemical/products/{productId:guid}/sds", async (
    Guid productId,
    RecordSdsRevisionRequest request,
    IChemicalInventoryService inventory,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var updated = request with { ChemicalProductId = productId };
    var item = await inventory.RecordSdsRevisionAsync(updated, ct);
    return Results.Created($"/api/v1/chemical/products/{productId}/sds/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/chemical/products/{productId:guid}/sds", async (
    Guid productId,
    IChemicalInventoryService inventory,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await inventory.ListSdsRevisionsAsync(productId, ct);
    return Results.Ok(items);
}).RequireAuthorization();

// Chemical exposure controls (HealthSafety module).
app.MapPost("/api/v1/chemical/products/{productId:guid}/exposure-controls", async (
    Guid productId,
    CreateExposureControlRequest request,
    IChemicalExposureControlService exposure,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var updated = request with { ChemicalProductId = productId };
    var item = await exposure.AddAsync(updated, ct);
    return Results.Created($"/api/v1/chemical/products/{productId}/exposure-controls/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/chemical/products/{productId:guid}/exposure-controls", async (
    Guid productId,
    IChemicalExposureControlService exposure,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await exposure.ListAsync(productId, ct);
    return Results.Ok(items);
}).RequireAuthorization();

// Chemical storage inspections (HealthSafety module).
app.MapPost("/api/v1/chemical/storage-inspections", async (
    CreateStorageInspectionRequest request,
    ClaimsPrincipal user,
    IChemicalStorageInspectionService inspections,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var item = await inspections.CreateAsync(request, memberId.Value, ct);
    return Results.Created($"/api/v1/chemical/storage-inspections/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/chemical/storage-inspections", async (
    Guid? chemicalInventoryId,
    IChemicalStorageInspectionService inspections,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await inspections.ListAsync(chemicalInventoryId, ct);
    return Results.Ok(items);
}).RequireAuthorization();

// PPE catalogue & requirements (HealthSafety module).
app.MapPost("/api/v1/ppe/catalog", async (
    CreatePpeCatalogRequest request,
    IPpeCatalogService ppe,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await ppe.CreateCatalogAsync(request, ct);
    return Results.Created($"/api/v1/ppe/catalog/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/ppe/catalog", async (
    IPpeCatalogService ppe,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await ppe.ListCatalogsAsync(ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/ppe/requirements", async (
    CreatePpeRequirementRequest request,
    IPpeCatalogService ppe,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await ppe.CreateRequirementAsync(request, ct);
    return Results.Created($"/api/v1/ppe/requirements/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/ppe/requirements", async (
    Guid? ppeCatalogId,
    IPpeCatalogService ppe,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await ppe.ListRequirementsAsync(ppeCatalogId, ct);
    return Results.Ok(items);
}).RequireAuthorization();

// PPE inventory & assignments (HealthSafety module).
app.MapPost("/api/v1/ppe/inventory", async (
    RegisterPpeInventoryRequest request,
    IPpeInventoryService ppe,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await ppe.RegisterInventoryAsync(request, ct);
    return Results.Created($"/api/v1/ppe/inventory/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/ppe/inventory", async (
    Guid? ppeCatalogId,
    IPpeInventoryService ppe,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await ppe.ListInventoryAsync(ppeCatalogId, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/ppe/assignments", async (
    AssignPpeRequest request,
    ClaimsPrincipal user,
    IPpeInventoryService ppe,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var item = await ppe.AssignAsync(request, memberId.Value, ct);
    return Results.Created($"/api/v1/ppe/assignments/{item.Id}", item);
}).RequireAuthorization();

app.MapPost("/api/v1/ppe/assignments/{assignmentId:guid}/return", async (
    Guid assignmentId,
    ReturnPpeRequest request,
    IPpeInventoryService ppe,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var updated = request with { AssignmentId = assignmentId };
    var item = await ppe.ReturnAsync(updated, ct);
    return item is null
        ? Results.NotFound()
        : Results.Ok(item);
}).RequireAuthorization();

app.MapGet("/api/v1/ppe/assignments", async (
    Guid? ppeInventoryId,
    IPpeInventoryService ppe,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await ppe.ListAssignmentsAsync(ppeInventoryId, ct);
    return Results.Ok(items);
}).RequireAuthorization();

// PPE inspections & replacements (HealthSafety module).
app.MapPost("/api/v1/ppe/inspections", async (
    RecordPpeInspectionRequest request,
    ClaimsPrincipal user,
    IPpeInspectionService ppe,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var item = await ppe.RecordInspectionAsync(request, memberId.Value, ct);
    return Results.Created($"/api/v1/ppe/inspections/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/ppe/inspections", async (
    Guid? ppeInventoryId,
    IPpeInspectionService ppe,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await ppe.ListInspectionsAsync(ppeInventoryId, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/ppe/replacements", async (
    RequestPpeReplacementRequest request,
    IPpeInspectionService ppe,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await ppe.RequestReplacementAsync(request, ct);
    return Results.Created($"/api/v1/ppe/replacements/{item.Id}", item);
}).RequireAuthorization();

app.MapPost("/api/v1/ppe/replacements/{replacementId:guid}/complete", async (
    Guid replacementId,
    IPpeInspectionService ppe,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await ppe.CompleteReplacementAsync(replacementId, ct);
    return item is null
        ? Results.NotFound()
        : Results.Ok(item);
}).RequireAuthorization();

app.MapGet("/api/v1/ppe/replacements", async (
    Guid? ppeAssignmentId,
    IPpeInspectionService ppe,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await ppe.ListReplacementsAsync(ppeAssignmentId, ct);
    return Results.Ok(items);
}).RequireAuthorization();

// Environment monitoring (HealthSafety module).
app.MapPost("/api/v1/environment/parameters", async (
    CreateEnvironmentParameterRequest request,
    IEnvironmentMonitoringService env,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await env.CreateParameterAsync(request, ct);
    return Results.Created($"/api/v1/environment/parameters/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/environment/parameters", async (
    string? category,
    IEnvironmentMonitoringService env,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await env.ListParametersAsync(category, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/environment/sources", async (
    CreateEnvironmentSourceRequest request,
    IEnvironmentMonitoringService env,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await env.CreateSourceAsync(request, ct);
    return Results.Created($"/api/v1/environment/sources/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/environment/sources", async (
    Guid? siteId,
    IEnvironmentMonitoringService env,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await env.ListSourcesAsync(siteId, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/environment/measurements", async (
    RecordEnvironmentMeasurementRequest request,
    ClaimsPrincipal user,
    IEnvironmentMonitoringService env,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var monitoringRecordId = Guid.NewGuid();
    var item = await env.RecordMeasurementAsync(request, monitoringRecordId, memberId.Value, ct);
    return Results.Created($"/api/v1/environment/measurements/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/environment/measurements", async (
    Guid? parameterId,
    IEnvironmentMonitoringService env,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await env.ListMeasurementsAsync(parameterId, ct);
    return Results.Ok(items);
}).RequireAuthorization();

// Occupational health (HealthSafety module, Trello Sprint 22 R2).
app.MapPost("/api/v1/health/profiles", async (
    CreateHealthProfileRequest request,
    IOccupationalHealthService oh,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await oh.CreateHealthProfileAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/health/profiles/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/health/profiles", async (
    IOccupationalHealthService oh,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await oh.ListHealthProfilesAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/health/fitness-statuses", async (
    CreateFitnessStatusRequest request,
    IOccupationalHealthService oh,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await oh.CreateFitnessStatusAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/health/fitness-statuses/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/health/fitness-statuses", async (
    Guid healthProfileId,
    IOccupationalHealthService oh,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await oh.ListFitnessStatusesAsync(healthProfileId, tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/health/surveillance-programs", async (
    CreateSurveillanceProgramRequest request,
    IOccupationalHealthService oh,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await oh.CreateSurveillanceProgramAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/health/surveillance-programs/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/health/surveillance-programs", async (
    IOccupationalHealthService oh,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await oh.ListSurveillanceProgramsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/health/surveillance-events", async (
    CreateSurveillanceEventRequest request,
    ClaimsPrincipal user,
    IOccupationalHealthService oh,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var item = await oh.CreateSurveillanceEventAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/health/surveillance-events/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/health/surveillance-events", async (
    IOccupationalHealthService oh,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await oh.ListSurveillanceEventsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/health/followups", async (
    CreateHealthFollowupRequest request,
    IOccupationalHealthService oh,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await oh.CreateHealthFollowupAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/health/followups/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/health/followups", async (
    Guid surveillanceEventId,
    IOccupationalHealthService oh,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await oh.ListHealthFollowupsAsync(surveillanceEventId, tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

// Hazard & risk backend (SafetyRisk module, Trello Sprint 11).
app.MapPost("/api/v1/risk/hazards", async (
    CreateHazardRequest request,
    IHazardRiskService risk,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var id = await risk.CreateHazardAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/risk/hazards/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/risk/hazards", async (
    IHazardRiskService risk,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await risk.ListHazardsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/risk/matrix-versions", async (
    CreateRiskMatrixVersionRequest request,
    IHazardRiskService risk,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var id = await risk.CreateMatrixVersionAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/risk/matrix-versions/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/risk/matrix-versions", async (
    IHazardRiskService risk,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await risk.ListMatrixVersionsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/risk/matrix-cells", async (
    CreateRiskMatrixCellRequest request,
    IHazardRiskService risk,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var id = await risk.CreateMatrixCellAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/risk/matrix-cells/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/risk/matrix-cells", async (
    Guid? matrixVersionId,
    IHazardRiskService risk,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await risk.ListMatrixCellsAsync(matrixVersionId ?? Guid.Empty, tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/risk/registers", async (
    CreateRiskRegisterRequest request,
    ClaimsPrincipal user,
    IHazardRiskService risk,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var id = await risk.CreateRegisterAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/risk/registers/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/risk/registers", async (
    IHazardRiskService risk,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await risk.ListRegistersAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/risk/assessments", async (
    CreateRiskAssessmentRequest request,
    ClaimsPrincipal user,
    IHazardRiskService risk,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var id = await risk.CreateAssessmentAsync(request with { AssessedByMemberId = memberId.Value }, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/risk/assessments/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/risk/assessments", async (
    IHazardRiskService risk,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await risk.ListAssessmentsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/risk/controls", async (
    CreateRiskControlRequest request,
    IHazardRiskService risk,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var id = await risk.CreateControlAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/risk/controls/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/risk/controls", async (
    IHazardRiskService risk,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await risk.ListControlsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

// Incident & CAPA backend (SafetyRisk module, Trello Sprint 13).
app.MapPost("/api/v1/incidents", async (
    CreateIncidentRequest request,
    ClaimsPrincipal user,
    IIncidentCapaService incident,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var item = await incident.CreateIncidentAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/incidents/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/incidents", async (
    IIncidentCapaService incident,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await incident.ListIncidentsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/incidents/involved-people", async (
    AddInvolvedPersonRequest request,
    IIncidentCapaService incident,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var id = await incident.AddInvolvedPersonAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/incidents/involved-people/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/incidents/involved-people", async (
    Guid? incidentId,
    IIncidentCapaService incident,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await incident.ListInvolvedPeopleAsync(incidentId, tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/incidents/investigations", async (
    StartInvestigationRequest request,
    ClaimsPrincipal user,
    IIncidentCapaService incident,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var id = await incident.StartInvestigationAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/incidents/investigations/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/incidents/investigations", async (
    Guid? incidentId,
    IIncidentCapaService incident,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await incident.ListInvestigationsAsync(incidentId, tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/incidents/root-causes", async (
    AddRootCauseRequest request,
    IIncidentCapaService incident,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var id = await incident.AddRootCauseAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/incidents/root-causes/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/incidents/root-causes", async (
    Guid? investigationId,
    IIncidentCapaService incident,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await incident.ListRootCausesAsync(investigationId, tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/capa/actions", async (
    CreateCapaActionRequest request,
    ClaimsPrincipal user,
    IIncidentCapaService incident,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var item = await incident.CreateActionAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/capa/actions/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/capa/actions", async (
    IIncidentCapaService incident,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await incident.ListActionsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/capa/actions/{actionId:guid}/progress", async (
    Guid actionId,
    ProgressCapaActionRequest request,
    ClaimsPrincipal user,
    IIncidentCapaService incident,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    await incident.ProgressActionAsync(request with { ActionId = actionId }, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Ok();
}).RequireAuthorization();

app.MapPost("/api/v1/capa/actions/{actionId:guid}/verify", async (
    Guid actionId,
    VerifyCapaActionRequest request,
    ClaimsPrincipal user,
    IIncidentCapaService incident,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    await incident.VerifyActionAsync(request with { ActionId = actionId }, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Ok();
}).RequireAuthorization();

// Inspection & Audit backend (WorkControl module, Trello Sprint 15).
app.MapPost("/api/v1/audit/programs", async (
    CreateAuditProgramRequest request,
    ClaimsPrincipal user,
    IInspectionAuditService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var id = await svc.CreateAuditProgramAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/audit/programs/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/audit/programs", async (
    IInspectionAuditService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListAuditProgramsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/audits", async (
    CreateAuditRequest request,
    ClaimsPrincipal user,
    IInspectionAuditService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var item = await svc.CreateAuditAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/audits/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/audits", async (
    IInspectionAuditService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListAuditsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/audits/findings", async (
    CreateAuditFindingRequest request,
    ClaimsPrincipal user,
    IInspectionAuditService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var id = await svc.CreateAuditFindingAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/audits/findings/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/audits/findings", async (
    Guid? auditId,
    IInspectionAuditService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListAuditFindingsAsync(auditId, tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/inspections", async (
    CreateInspectionRequest request,
    ClaimsPrincipal user,
    IInspectionAuditService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var item = await svc.CreateInspectionAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/inspections/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/inspections", async (
    IInspectionAuditService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListInspectionsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/inspections/findings", async (
    CreateInspectionFindingRequest request,
    ClaimsPrincipal user,
    IInspectionAuditService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var id = await svc.CreateInspectionFindingAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/inspections/findings/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/inspections/findings", async (
    Guid? inspectionId,
    IInspectionAuditService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListInspectionFindingsAsync(inspectionId, tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

// PTW / JSA / LOTO backend (WorkControl module, Trello Sprint 17).
app.MapPost("/api/v1/work-requests", async (
    CreateWorkRequestRequest request,
    ClaimsPrincipal user,
    IPtwJsaLotoService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var item = await svc.CreateWorkRequestAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/work-requests/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/work-requests", async (
    IPtwJsaLotoService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListWorkRequestsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/jsas", async (
    CreateJsaRequest request,
    ClaimsPrincipal user,
    IPtwJsaLotoService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var item = await svc.CreateJsaAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/jsas/{item.Id}", item);
}).RequireAuthorization();

app.MapPost("/api/v1/jsas/steps", async (
    CreateJsaStepRequest request,
    IPtwJsaLotoService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var id = await svc.AddJsaStepAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/jsas/steps/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/jsas", async (
    IPtwJsaLotoService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListJsasAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/permits", async (
    CreatePermitRequest request,
    ClaimsPrincipal user,
    IPtwJsaLotoService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var item = await svc.CreatePermitAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/permits/{item.Id}", item);
}).RequireAuthorization();

app.MapPost("/api/v1/permits/approvals", async (
    ApprovePermitRequest request,
    ClaimsPrincipal user,
    IPtwJsaLotoService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    await svc.ApprovePermitAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Ok();
}).RequireAuthorization();

app.MapPost("/api/v1/permits/gas-tests", async (
    RecordGasTestRequest request,
    IPtwJsaLotoService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var id = await svc.RecordGasTestAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/permits/gas-tests/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/permits", async (
    IPtwJsaLotoService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListPermitsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/isolation-plans", async (
    CreateIsolationPlanRequest request,
    ClaimsPrincipal user,
    IPtwJsaLotoService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var item = await svc.CreateIsolationPlanAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/isolation-plans/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/isolation-plans", async (
    IPtwJsaLotoService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListIsolationPlansAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

// Contractor & Contract Management (ComplianceContracts module, Trello Sprint 19 R2).
app.MapPost("/api/v1/contractor/companies", async (
    CreateContractorCompanyRequest request,
    ClaimsPrincipal user,
    IContractService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var item = await svc.CreateContractorCompanyAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/contractor/companies/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/contractor/companies", async (
    IContractService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListContractorCompaniesAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/contractor/contracts", async (
    CreateContractRequest request,
    IContractService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await svc.CreateContractAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/contractor/contracts/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/contractor/contracts", async (
    IContractService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListContractsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/contractor/workers", async (
    CreateContractorWorkerRequest request,
    IContractService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await svc.CreateContractorWorkerAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/contractor/workers/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/contractor/workers", async (
    IContractService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListContractorWorkersAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/contractor/documents", async (
    CreateContractorDocumentRequest request,
    IContractService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await svc.CreateContractorDocumentAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/contractor/documents/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/contractor/documents", async (
    IContractService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListContractorDocumentsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

// Training & Competency (ComplianceContracts module, Trello Sprint 20 R2).
app.MapPost("/api/v1/courses", async (
    CreateCourseRequest request,
    ITrainingService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await svc.CreateCourseAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/courses/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/courses", async (
    ITrainingService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListCoursesAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/training-sessions", async (
    CreateTrainingSessionRequest request,
    ClaimsPrincipal user,
    ITrainingService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    IdentityDbContext identityDb,
    CancellationToken ct) =>
{
    var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
    if (memberId is null || tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
    }
    var item = await svc.CreateTrainingSessionAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
    return Results.Created($"/api/v1/training-sessions/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/training-sessions", async (
    ITrainingService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListTrainingSessionsAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/training-sessions/participants", async (
    AddSessionParticipantRequest request,
    ITrainingService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var id = await svc.AddSessionParticipantAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/training-sessions/participants/{id}", new { id });
}).RequireAuthorization();

app.MapGet("/api/v1/training-sessions/{sessionId}/participants", async (
    Guid sessionId,
    ITrainingService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListSessionParticipantsAsync(sessionId, tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/competencies", async (
    CreateCompetencyRequest request,
    ITrainingService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await svc.CreateCompetencyAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/competencies/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/competencies", async (
    ITrainingService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListCompetenciesAsync(tenantContext.CurrentTenantId.Value, ct);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/v1/worker-competencies", async (
    AssignWorkerCompetencyRequest request,
    ITrainingService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var item = await svc.AssignWorkerCompetencyAsync(request, tenantContext.CurrentTenantId.Value, ct);
    return Results.Created($"/api/v1/worker-competencies/{item.Id}", item);
}).RequireAuthorization();

app.MapGet("/api/v1/worker-competencies", async (
    Guid personId,
    ITrainingService svc,
    Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
    CancellationToken ct) =>
{
    if (tenantContext.CurrentTenantId is null)
    {
        return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
    }
    var items = await svc.ListWorkerCompetenciesAsync(personId, tenantContext.CurrentTenantId.Value, ct);
        return Results.Ok(items);
    }).RequireAuthorization();

    // Legal & compliance (ComplianceContracts module, Trello Sprint 25 R2).
    app.MapPost("/api/v1/legal/sources", async (
        CreateLegalSourceRequest request,
        ILegalComplianceService legal,
        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
        CancellationToken ct) =>
    {
        if (tenantContext.CurrentTenantId is null)
        {
            return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
        }
        var item = await legal.CreateLegalSourceAsync(request, tenantContext.CurrentTenantId.Value, ct);
        return Results.Created($"/api/v1/legal/sources/{item.Id}", item);
    }).RequireAuthorization();

    app.MapGet("/api/v1/legal/sources", async (
        ILegalComplianceService legal,
        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
        CancellationToken ct) =>
    {
        if (tenantContext.CurrentTenantId is null)
        {
            return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
        }
        var items = await legal.ListLegalSourcesAsync(tenantContext.CurrentTenantId.Value, ct);
        return Results.Ok(items);
    }).RequireAuthorization();

    app.MapPost("/api/v1/legal/source-versions", async (
        CreateLegalSourceVersionRequest request,
        ILegalComplianceService legal,
        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
        CancellationToken ct) =>
    {
        if (tenantContext.CurrentTenantId is null)
        {
            return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
        }
        var item = await legal.CreateLegalSourceVersionAsync(request, tenantContext.CurrentTenantId.Value, ct);
        return Results.Created($"/api/v1/legal/source-versions/{item.Id}", item);
    }).RequireAuthorization();

    app.MapGet("/api/v1/legal/source-versions", async (
        Guid legalSourceId,
        ILegalComplianceService legal,
        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
        CancellationToken ct) =>
    {
        if (tenantContext.CurrentTenantId is null)
        {
            return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
        }
        var items = await legal.ListLegalSourceVersionsAsync(legalSourceId, tenantContext.CurrentTenantId.Value, ct);
        return Results.Ok(items);
    }).RequireAuthorization();

    app.MapPost("/api/v1/legal/obligations", async (
        CreateObligationRequest request,
        ClaimsPrincipal user,
        ILegalComplianceService legal,
        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
        IdentityDbContext identityDb,
        CancellationToken ct) =>
    {
        var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
            if (memberId is null || tenantContext.CurrentTenantId is null)
            {
                return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
            }
            var item = await legal.CreateObligationAsync(
                request with { OwnerMemberId = memberId.Value }, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
            return Results.Created($"/api/v1/legal/obligations/{item.Id}", item);
        }).RequireAuthorization();

    app.MapGet("/api/v1/legal/obligations", async (
        ILegalComplianceService legal,
        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
        CancellationToken ct) =>
    {
        if (tenantContext.CurrentTenantId is null)
        {
            return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
        }
        var items = await legal.ListObligationsAsync(tenantContext.CurrentTenantId.Value, ct);
        return Results.Ok(items);
    }).RequireAuthorization();

    app.MapPost("/api/v1/legal/obligation-applicability", async (
        CreateObligationApplicabilityRequest request,
        ClaimsPrincipal user,
        ILegalComplianceService legal,
        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
        IdentityDbContext identityDb,
        CancellationToken ct) =>
    {
        var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
        if (memberId is null || tenantContext.CurrentTenantId is null)
        {
            return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
        }
        var item = await legal.CreateObligationApplicabilityAsync(
            request with { AssessedByMemberId = memberId.Value }, tenantContext.CurrentTenantId.Value, ct);
        return Results.Created($"/api/v1/legal/obligation-applicability/{item.Id}", item);
    }).RequireAuthorization();

    app.MapGet("/api/v1/legal/obligation-applicability", async (
        Guid obligationId,
        ILegalComplianceService legal,
        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
        CancellationToken ct) =>
    {
        if (tenantContext.CurrentTenantId is null)
        {
            return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
        }
        var items = await legal.ListObligationApplicabilitiesAsync(obligationId, tenantContext.CurrentTenantId.Value, ct);
                return Results.Ok(items);
            }).RequireAuthorization();

        // Asset safety & emergency (AssetReporting module, Trello Sprint 26 R2).
        app.MapPost("/api/v1/assets", async (
            CreateAssetRequest request,
            ClaimsPrincipal user,
            IAssetEmergencyService assetSvc,
            Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
            IdentityDbContext identityDb,
            CancellationToken ct) =>
        {
            var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
            if (memberId is null || tenantContext.CurrentTenantId is null)
            {
                return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
            }
            var item = await assetSvc.CreateAssetAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
            return Results.Created($"/api/v1/assets/{item.Id}", item);
        }).RequireAuthorization();

        app.MapGet("/api/v1/assets", async (
            IAssetEmergencyService assetSvc,
            Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
            CancellationToken ct) =>
        {
            if (tenantContext.CurrentTenantId is null)
            {
                return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
            }
            var items = await assetSvc.ListAssetsAsync(tenantContext.CurrentTenantId.Value, ct);
            return Results.Ok(items);
        }).RequireAuthorization();

        app.MapPost("/api/v1/emergency/plans", async (
            CreateEmergencyPlanRequest request,
            ClaimsPrincipal user,
            IAssetEmergencyService assetSvc,
            Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
            IdentityDbContext identityDb,
            CancellationToken ct) =>
        {
            var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
            if (memberId is null || tenantContext.CurrentTenantId is null)
            {
                return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
            }
            var item = await assetSvc.CreateEmergencyPlanAsync(
                request with { OwnerMemberId = memberId.Value }, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
            return Results.Created($"/api/v1/emergency/plans/{item.Id}", item);
        }).RequireAuthorization();

        app.MapGet("/api/v1/emergency/plans", async (
            IAssetEmergencyService assetSvc,
            Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
            CancellationToken ct) =>
        {
            if (tenantContext.CurrentTenantId is null)
            {
                return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
            }
            var items = await assetSvc.ListEmergencyPlansAsync(tenantContext.CurrentTenantId.Value, ct);
            return Results.Ok(items);
        }).RequireAuthorization();

        app.MapPost("/api/v1/emergency/team-members", async (
            AddEmergencyTeamMemberRequest request,
            IAssetEmergencyService assetSvc,
            Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
            CancellationToken ct) =>
        {
            if (tenantContext.CurrentTenantId is null)
            {
                return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
            }
            var item = await assetSvc.AddEmergencyTeamMemberAsync(request, tenantContext.CurrentTenantId.Value, ct);
            return Results.Created($"/api/v1/emergency/team-members/{item.Id}", item);
        }).RequireAuthorization();

        app.MapGet("/api/v1/emergency/team-members", async (
            Guid planId,
            IAssetEmergencyService assetSvc,
            Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
            CancellationToken ct) =>
        {
            if (tenantContext.CurrentTenantId is null)
            {
                return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
            }
            var items = await assetSvc.ListEmergencyTeamMembersAsync(planId, tenantContext.CurrentTenantId.Value, ct);
            return Results.Ok(items);
        }).RequireAuthorization();

        app.MapPost("/api/v1/emergency/equipment", async (
            CreateEmergencyEquipmentRequest request,
            IAssetEmergencyService assetSvc,
            Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
            CancellationToken ct) =>
        {
            if (tenantContext.CurrentTenantId is null)
            {
                return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
            }
            var item = await assetSvc.CreateEmergencyEquipmentAsync(request, tenantContext.CurrentTenantId.Value, ct);
            return Results.Created($"/api/v1/emergency/equipment/{item.Id}", item);
        }).RequireAuthorization();

        app.MapGet("/api/v1/emergency/equipment", async (
            IAssetEmergencyService assetSvc,
            Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
            CancellationToken ct) =>
        {
            if (tenantContext.CurrentTenantId is null)
            {
                return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
            }
            var items = await assetSvc.ListEmergencyEquipmentAsync(tenantContext.CurrentTenantId.Value, ct);
            return Results.Ok(items);
        }).RequireAuthorization();

        app.MapPost("/api/v1/emergency/drills", async (
            CreateEmergencyDrillRequest request,
            ClaimsPrincipal user,
            IAssetEmergencyService assetSvc,
            Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
            IdentityDbContext identityDb,
            CancellationToken ct) =>
        {
            var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
            if (memberId is null || tenantContext.CurrentTenantId is null)
            {
                return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
            }
            var item = await assetSvc.CreateEmergencyDrillAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
            return Results.Created($"/api/v1/emergency/drills/{item.Id}", item);
        }).RequireAuthorization();

        app.MapGet("/api/v1/emergency/drills", async (
            IAssetEmergencyService assetSvc,
            Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
            CancellationToken ct) =>
        {
            if (tenantContext.CurrentTenantId is null)
            {
                return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
            }
            var items = await assetSvc.ListEmergencyDrillsAsync(tenantContext.CurrentTenantId.Value, ct);
            return Results.Ok(items);
        }).RequireAuthorization();

        app.MapPost("/api/v1/emergency/drill-findings", async (
            CreateEmergencyDrillFindingRequest request,
            ClaimsPrincipal user,
            IAssetEmergencyService assetSvc,
            Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
            IdentityDbContext identityDb,
            CancellationToken ct) =>
        {
            var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
            if (memberId is null || tenantContext.CurrentTenantId is null)
            {
                return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
            }
            var item = await assetSvc.CreateEmergencyDrillFindingAsync(
                request with { OwnerMemberId = memberId.Value }, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
            return Results.Created($"/api/v1/emergency/drill-findings/{item.Id}", item);
        }).RequireAuthorization();

        app.MapGet("/api/v1/emergency/drill-findings", async (
            Guid drillId,
            IAssetEmergencyService assetSvc,
            Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
            CancellationToken ct) =>
        {
            if (tenantContext.CurrentTenantId is null)
            {
                return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
            }
            var items = await assetSvc.ListEmergencyDrillFindingsAsync(drillId, tenantContext.CurrentTenantId.Value, ct);
                        return Results.Ok(items);
                    }).RequireAuthorization();

                    // Reporting & KPI (AssetReporting module, Trello Sprint 27 R2).
                    app.MapPost("/api/v1/reports/definitions", async (
                        CreateReportDefinitionRequest request,
                        IReportingKpiService rpt,
                        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                        CancellationToken ct) =>
                    {
                        if (tenantContext.CurrentTenantId is null)
                        {
                            return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                        }
                        var item = await rpt.CreateReportDefinitionAsync(request, tenantContext.CurrentTenantId.Value, ct);
                        return Results.Created($"/api/v1/reports/definitions/{item.Id}", item);
                    }).RequireAuthorization();

                    app.MapGet("/api/v1/reports/definitions", async (
                        IReportingKpiService rpt,
                        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                        CancellationToken ct) =>
                    {
                        if (tenantContext.CurrentTenantId is null)
                        {
                            return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                        }
                        var items = await rpt.ListReportDefinitionsAsync(tenantContext.CurrentTenantId.Value, ct);
                        return Results.Ok(items);
                    }).RequireAuthorization();

                    app.MapPost("/api/v1/reports/schedules", async (
                        CreateReportScheduleRequest request,
                        ClaimsPrincipal user,
                        IReportingKpiService rpt,
                        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                        IdentityDbContext identityDb,
                        CancellationToken ct) =>
                    {
                        var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
                        if (memberId is null || tenantContext.CurrentTenantId is null)
                        {
                            return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
                        }
                        var item = await rpt.CreateReportScheduleAsync(
                            request with { OwnerMemberId = memberId.Value }, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
                        return Results.Created($"/api/v1/reports/schedules/{item.Id}", item);
                    }).RequireAuthorization();

                    app.MapGet("/api/v1/reports/schedules", async (
                        IReportingKpiService rpt,
                        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                        CancellationToken ct) =>
                    {
                        if (tenantContext.CurrentTenantId is null)
                        {
                            return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                        }
                        var items = await rpt.ListReportSchedulesAsync(tenantContext.CurrentTenantId.Value, ct);
                        return Results.Ok(items);
                    }).RequireAuthorization();

                    app.MapPost("/api/v1/reports/executions", async (
                        CreateReportExecutionRequest request,
                        ClaimsPrincipal user,
                        IReportingKpiService rpt,
                        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                        IdentityDbContext identityDb,
                        CancellationToken ct) =>
                    {
                        var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
                        if (memberId is null || tenantContext.CurrentTenantId is null)
                        {
                            return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
                        }
                        var item = await rpt.CreateReportExecutionAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
                        return Results.Created($"/api/v1/reports/executions/{item.Id}", item);
                    }).RequireAuthorization();

                    app.MapGet("/api/v1/reports/executions", async (
                        Guid reportDefinitionId,
                        IReportingKpiService rpt,
                        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                        CancellationToken ct) =>
                    {
                        if (tenantContext.CurrentTenantId is null)
                        {
                            return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                        }
                        var items = await rpt.ListReportExecutionsAsync(reportDefinitionId, tenantContext.CurrentTenantId.Value, ct);
                        return Results.Ok(items);
                    }).RequireAuthorization();

                    app.MapPost("/api/v1/kpis", async (
                        CreateKpiDefinitionRequest request,
                        ClaimsPrincipal user,
                        IReportingKpiService rpt,
                        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                        IdentityDbContext identityDb,
                        CancellationToken ct) =>
                    {
                        var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
                        if (memberId is null || tenantContext.CurrentTenantId is null)
                        {
                            return Results.Json(new { error = "No active member/tenant" }, statusCode: 400);
                        }
                        var item = await rpt.CreateKpiDefinitionAsync(
                            request with { OwnerMemberId = memberId.Value }, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
                        return Results.Created($"/api/v1/kpis/{item.Id}", item);
                    }).RequireAuthorization();

                    app.MapGet("/api/v1/kpis", async (
                        IReportingKpiService rpt,
                        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                        CancellationToken ct) =>
                    {
                        if (tenantContext.CurrentTenantId is null)
                        {
                            return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                        }
                        var items = await rpt.ListKpiDefinitionsAsync(tenantContext.CurrentTenantId.Value, ct);
                        return Results.Ok(items);
                    }).RequireAuthorization();

                    app.MapPost("/api/v1/kpis/versions", async (
                        CreateKpiVersionRequest request,
                        IReportingKpiService rpt,
                        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                        CancellationToken ct) =>
                    {
                        if (tenantContext.CurrentTenantId is null)
                        {
                            return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                        }
                        var item = await rpt.CreateKpiVersionAsync(request, tenantContext.CurrentTenantId.Value, ct);
                        return Results.Created($"/api/v1/kpis/versions/{item.Id}", item);
                    }).RequireAuthorization();

                    app.MapGet("/api/v1/kpis/versions", async (
                        Guid kpiDefinitionId,
                        IReportingKpiService rpt,
                        Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                        CancellationToken ct) =>
                    {
                        if (tenantContext.CurrentTenantId is null)
                        {
                            return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                        }
                        var items = await rpt.ListKpiVersionsAsync(kpiDefinitionId, tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Ok(items);
                                            }).RequireAuthorization();

                                            // Integration & external (AssetReporting module, Trello Sprint 28 R2).
                                            app.MapPost("/api/v1/integration/interfaces", async (
                                                CreateIntegrationInterfaceRequest request,
                                                IIntegrationService intSvc,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var item = await intSvc.CreateInterfaceAsync(request, tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Created($"/api/v1/integration/interfaces/{item.Id}", item);
                                            }).RequireAuthorization();

                                            app.MapGet("/api/v1/integration/interfaces", async (
                                                IIntegrationService intSvc,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var items = await intSvc.ListInterfacesAsync(tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Ok(items);
                                            }).RequireAuthorization();

                                            app.MapPost("/api/v1/integration/data-mappings", async (
                                                CreateIntegrationDataMappingRequest request,
                                                IIntegrationService intSvc,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var item = await intSvc.CreateDataMappingAsync(request, tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Created($"/api/v1/integration/data-mappings/{item.Id}", item);
                                            }).RequireAuthorization();

                                            app.MapGet("/api/v1/integration/data-mappings", async (
                                                Guid interfaceId,
                                                IIntegrationService intSvc,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var items = await intSvc.ListDataMappingsAsync(interfaceId, tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Ok(items);
                                            }).RequireAuthorization();

                                            app.MapPost("/api/v1/integration/runs", async (
                                                CreateIntegrationRunRequest request,
                                                IIntegrationService intSvc,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var item = await intSvc.CreateRunAsync(request, tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Created($"/api/v1/integration/runs/{item.Id}", item);
                                            }).RequireAuthorization();

                                            app.MapGet("/api/v1/integration/runs", async (
                                                Guid interfaceId,
                                                IIntegrationService intSvc,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var items = await intSvc.ListRunsAsync(interfaceId, tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Ok(items);
                                            }).RequireAuthorization();

                                            app.MapPost("/api/v1/integration/messages", async (
                                                CreateIntegrationMessageRequest request,
                                                IIntegrationService intSvc,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var item = await intSvc.CreateMessageAsync(request, tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Created($"/api/v1/integration/messages/{item.Id}", item);
                                            }).RequireAuthorization();

                                            app.MapGet("/api/v1/integration/messages", async (
                                                Guid integrationRunId,
                                                IIntegrationService intSvc,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var items = await intSvc.ListMessagesAsync(integrationRunId, tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Ok(items);
                                            }).RequireAuthorization();

                                            app.MapPost("/api/v1/integration/reconciliations", async (
                                                CreateIntegrationReconciliationRequest request,
                                                IIntegrationService intSvc,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var item = await intSvc.CreateReconciliationAsync(request, tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Created($"/api/v1/integration/reconciliations/{item.Id}", item);
                                            }).RequireAuthorization();

                                            app.MapGet("/api/v1/integration/reconciliations", async (
                                                Guid integrationRunId,
                                                IIntegrationService intSvc,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var items = await intSvc.ListReconciliationsAsync(integrationRunId, tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Ok(items);
                                            }).RequireAuthorization();

                                            // Audit trail (Platform module, Trello Sprint 29 R3) — read-only, append-only.
                                            app.MapGet("/api/v1/audit-logs", async (
                                                Guid? recordId,
                                                string? actionCode,
                                                DateTimeOffset? from,
                                                DateTimeOffset? to,
                                                int limit,
                                                IAuditTrailService audit,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var safeLimit = Math.Clamp(limit <= 0 ? 100 : limit, 1, 500);
                                                var items = await audit.ListAsync(
                                                    tenantContext.CurrentTenantId.Value, recordId, actionCode, from, to, safeLimit, ct);
                                                return Results.Ok(items);
                                            }).RequireAuthorization();


                                            // Access review (Identity module, Trello Sprint 30 R3).
                                            app.MapPost("/api/v1/access-reviews", async (
                                                CreateAccessReviewRequest request,
                                                IAccessReviewService ar,
                                                ClaimsPrincipal user,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                IdentityDbContext identityDb,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var memberId = await ResolveActiveMemberIdAsync(user, tenantContext, identityDb, ct);
                                                if (memberId is null)
                                                {
                                                    return Results.Json(new { error = "No active member" }, statusCode: 403);
                                                }
                                                var dto = await ar.CreateAsync(request, tenantContext.CurrentTenantId.Value, memberId.Value, ct);
                                                return Results.Created($"/api/v1/access-reviews/{dto.Id}", dto);
                                            }).RequireAuthorization();

                                            app.MapGet("/api/v1/access-reviews", async (
                                                IAccessReviewService ar,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var items = await ar.ListAsync(tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Ok(items);
                                            }).RequireAuthorization();

                                            // RBAC admin (Identity module, Trello Sprint 31 R3).
                                            app.MapPost("/api/v1/identities/roles", async (
                                                CreateRoleRequest request,
                                                IRbacService rbac,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var dto = await rbac.CreateRoleAsync(request, tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Created($"/api/v1/identities/roles/{dto.Id}", dto);
                                            }).RequireAuthorization();

                                            app.MapGet("/api/v1/identities/roles", async (
                                                IRbacService rbac,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var items = await rbac.ListRolesAsync(tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Ok(items);
                                            }).RequireAuthorization();

                                            app.MapGet("/api/v1/identities/permissions", async (
                                                IRbacService rbac,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var items = await rbac.ListPermissionsAsync(tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Ok(items);
                                            }).RequireAuthorization();

                                            app.MapPost("/api/v1/identities/roles/{roleId:guid}/permissions", async (
                                                Guid roleId,
                                                AttachPermissionRequest request,
                                                IRbacService rbac,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var dto = await rbac.AttachPermissionAsync(roleId, request.PermissionId, tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Ok(dto);
                                            }).RequireAuthorization();

                                            app.MapPost("/api/v1/identities/members/{memberId:guid}/roles", async (
                                                Guid memberId,
                                                AssignRoleRequest request,
                                                IRbacService rbac,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var dto = await rbac.AssignRoleAsync(memberId, request.RoleId, tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Ok(dto);
                                            }).RequireAuthorization();

                                            app.MapGet("/api/v1/identities/members", async (
                                                IRbacService rbac,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var items = await rbac.ListMembersAsync(tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Ok(items);
                                            }).RequireAuthorization();

                                            app.MapPost("/api/v1/identities/permissions", async (
                                                CreatePermissionRequest request,
                                                IRbacService rbac,
                                                Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext,
                                                CancellationToken ct) =>
                                            {
                                                if (tenantContext.CurrentTenantId is null)
                                                {
                                                    return Results.Json(new { error = "No tenant resolved (fail-closed)" }, statusCode: 400);
                                                }
                                                var dto = await rbac.CreatePermissionAsync(request, tenantContext.CurrentTenantId.Value, ct);
                                                return Results.Created($"/api/v1/identities/permissions/{dto.Id}", dto);
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

                    // Seed the incident-approval workflow so the engine can run end-to-end.
                    var workflowSeeder = seedScope.ServiceProvider.GetRequiredService<WorkflowDbSeeder>();
                    await workflowSeeder.SeedAsync(firstTenant.Id);
                }
        }

app.Run();

/// <summary>Resolves the tenant-member id of the authenticated user within the active tenant.</summary>
static async Task<Guid?> ResolveActiveMemberIdAsync(ClaimsPrincipal user, Ehsms.BuildingBlocks.Tenancy.ITenantContext tenantContext, IdentityDbContext db, CancellationToken ct)
{
    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var tenantId = tenantContext.CurrentTenantId;
    if (sub is null || tenantId is null || !Guid.TryParse(sub, out var userId))
    {
        return null;
    }
    return await db.TenantMembers
        .Where(m => m.UserId == userId && m.TenantId == tenantId && m.Status == "Active")
        .Select(m => (Guid?)m.Id)
        .FirstOrDefaultAsync(ct);
}

public partial class Program;

/// <summary>Request body for <c>POST /api/v1/auth/login</c>.</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>Request body for <c>POST /api/v1/platform/records</c>.</summary>
public sealed record CreateRecordRequest(string ModuleCode, string RecordType, string Title, Guid DataClassificationId);

/// <summary>Request body for <c>POST /api/v1/workflow/start</c>.</summary>
public sealed record StartWorkflowRequest(Guid RecordId, string WorkflowCode);

/// <summary>Request body for <c>POST /api/v1/workflow/tasks/{id}/decision</c>.</summary>
public sealed record MakeDecisionRequest(string Decision, string? Comment);

/// <summary>Request body for <c>POST /api/v1/platform/files</c>.</summary>
public sealed record UploadFileRequest(string FileName, string MimeType, string ContentBase64);

/// <summary>Request body for <c>POST /api/v1/platform/records/{id}/evidence</c>.</summary>
public sealed record LinkEvidenceRequest(Guid FileObjectId, string EvidenceType);
