using System.Net;
using System.Net.Http.Json;
using Ehsms.Modules.Platform.Application;
using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class NotificationFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public NotificationFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task NotificationService_deduplicates_and_marks_read()
    {
        // Find the dev member id (admin in the first seeded tenant) so we can target them.
        Guid? memberId = null;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
            memberId = await db.WorkflowTasks.Where(t => t.Status == "Open").Select(t => (Guid?)t.AssignedMemberId).FirstOrDefaultAsync();
            if (memberId is null)
            {
                // Fall back: any member id via identity — pull the first active tenant member.
                memberId = await FirstActiveMemberIdAsync();
            }
        }
        if (memberId is null)
        {
            return; // No members seeded; skip assertion-less.
        }

        // Resolve a tenant id (first seeded tenant) through the same scope used by the seeder.
        Guid? tenantId = null;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var saas = scope.ServiceProvider.GetRequiredService<Ehsms.Modules.Saas.Infrastructure.Persistence.SaasDbContext>();
            tenantId = (await saas.Tenants.OrderBy(t => t.Id).FirstOrDefaultAsync())?.Id;
        }

        using var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        client.DefaultRequestHeaders.Authorization = new("Bearer", (await login.Content.ReadFromJsonAsync<LoginPayload>())!.AccessToken);

        Guid notificationId;
        var type = "test.reminder." + Guid.NewGuid().ToString("N"); // unique per run so re-runs stay idempotent
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<INotificationService>();
            // First create.
            var first = await svc.CreateAsync(memberId!.Value, type, "Reminder", "hello",
                recordId: null, deliveryChannel: "email", tenantId: tenantId);
            Assert.False(first.Deduplicated);
            notificationId = first.Id;

            // Duplicate create of the same unread type on same record should dedup.
            var dup = await svc.CreateAsync(memberId.Value, type, "Reminder", "hello again",
                recordId: null, deliveryChannel: "email", tenantId: tenantId);
            Assert.True(dup.Deduplicated);
        }

        // A non in-app channel queued an outbox message for email/SMS delivery.
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
            // The dispatcher may already have acked it, so accept Pending or Dispatched.
            var queued = await db.OutboxMessages.AnyAsync(
                m => m.EventType == "notification.created" && m.RecordId == null && (m.Status == "Pending" || m.Status == "Dispatched"));
            Assert.True(queued, "email-channel notification should queue an outbox message");

            // Mark read using the id returned by CreateAsync (no re-query ambiguity).
            var svc = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var ok = await svc.MarkReadAsync(notificationId, memberId!.Value, tenantId: tenantId);
            if (!ok)
            {
                var diag = await db.Notifications.Where(n => n.Id == notificationId)
                    .Select(n => new { n.RecipientMemberId, n.TenantId, n.ReadAt }).ToListAsync();
                throw new Xunit.Sdk.XunitException($"MarkRead FAILED id={notificationId} tenant={tenantId} member={memberId} notif={System.Text.Json.JsonSerializer.Serialize(diag)}");
            }
            Assert.True(ok);
            var again = await svc.MarkReadAsync(notificationId, memberId!.Value, tenantId: tenantId);
            Assert.False(again); // already read
        }
    }

    [Fact]
    public async Task MyTasks_and_me_endpoints_work_for_authenticated_member()
    {
        using var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        client.DefaultRequestHeaders.Authorization = new("Bearer", (await login.Content.ReadFromJsonAsync<LoginPayload>())!.AccessToken);

        var me = await client.GetAsync("/api/v1/workflow/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        var mePayload = await me.Content.ReadFromJsonAsync<MePayload>();
        Assert.NotNull(mePayload);

        var tasks = await client.GetAsync("/api/v1/workflow/my-tasks");
        Assert.Equal(HttpStatusCode.OK, tasks.StatusCode);
    }

    private async Task<Guid?> FirstActiveMemberIdAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<Ehsms.Modules.Identity.Infrastructure.Persistence.IdentityDbContext>();
        return await db.TenantMembers.Where(m => m.Status == "Active")
            .Select(m => (Guid?)m.Id).FirstOrDefaultAsync();
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);
    public sealed record MePayload(Guid? MemberId, int OpenTasks, int UnreadNotifications);
}
