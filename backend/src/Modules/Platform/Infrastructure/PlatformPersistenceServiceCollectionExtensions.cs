using Ehsms.BuildingBlocks.Tenancy;
using Ehsms.Modules.Platform.Application;
using Ehsms.Modules.Platform.Contracts;
using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ehsms.Modules.Platform.Infrastructure;

/// <summary>
/// DI registration for the Platform persistence layer.
/// </summary>
public static class PlatformPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformPersistence(
        this IServiceCollection services,
        string connectionString,
        ITenantContext? tenantContext = null)
    {
        services.AddDbContext<PlatformDbContext>(options =>
            options.UseSnakeCaseNamingConvention().UseNpgsql(connectionString));

        // Platform tables live in the dedicated "platform" PostgreSQL schema.
        services.AddSingleton<IPlatformDbSchema, DefaultPlatformDbSchema>();

        // Register a shared tenant context if the caller did not supply its own scoped one.
        if (tenantContext is not null)
        {
            services.AddSingleton(tenantContext);
        }
        else
        {
            services.AddScoped<ITenantContext, ScopedTenantContext>();
        }

        // Platform application services: record creation with number sequence + audit + outbox.
        services.AddScoped<AuditLogWriter>();
        services.AddScoped<IRecordAppService, RecordAppService>();
        services.AddScoped<PlatformDbSeeder>();
        services.AddHostedService<OutboxDispatcherWorker>();

        // Workflow engine: state transitions with permission/condition gates.
        services.AddScoped<IWorkflowEngine, WorkflowEngine>();
        services.AddScoped<IWorkflowPermissionChecker, GrantAllWorkflowPermissionChecker>();
        services.AddScoped<WorkflowDbSeeder>();
        services.AddHostedService<EscalationWorker>();

        // My tasks & notifications.
        services.AddScoped<INotificationService, NotificationService>();

        // Evidence & file lifecycle.
        services.AddScoped<IUploadQuotaValidator, GrantAllUploadQuotaValidator>();
        services.AddScoped<IObjectStorage, LocalFileObjectStorage>();
        services.AddScoped<IFileService, FileService>();
        // Audit trail read API (Trello Sprint 29 R3). Writes stay internal (AuditLogWriter).
        services.AddScoped<IAuditTrailService, AuditTrailService>();
        // Data classification & sensitive data (Trello Sprint 34 R3).
        services.AddScoped<IDataClassificationService, DataClassificationService>();
        services.AddHostedService<PurgeWorker>();

        return services;
    }
}

/// <summary>Default schema binding for the Platform module: PostgreSQL schema <c>platform</c>.</summary>
public sealed class DefaultPlatformDbSchema : IPlatformDbSchema
{
    public string Schema => "platform";
}
