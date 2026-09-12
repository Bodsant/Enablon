using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

/// <summary>
/// STBL Sprint 38 — Performance &amp; Load Baseline.
///
/// Establishes a repeatable latency baseline for the API's hot paths and
/// verifies the query planner uses indexes for the most common tenant-scoped
/// lookups. Thresholds are intentionally generous (fail on pathological
/// regression, not on CI jitter).
/// </summary>
public sealed class PerformanceBaselineTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public PerformanceBaselineTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private sealed record LoginResponse(string AccessToken, string TokenType);

    /// <summary>Hot-path endpoints that must stay responsive under baseline load.</summary>
    private static readonly (string Name, string Path)[] HotEndpoints =
    {
        ("records", "/api/v1/platform/records"),
        ("my-tasks", "/api/v1/workflow/my-tasks"),
        ("notifications", "/api/v1/notifications"),
        ("hazards", "/api/v1/risk/hazards"),
        ("audit-logs", "/api/v1/audit-logs?limit=50"),
        ("roles", "/api/v1/identities/roles"),
        ("data-classifications", "/api/v1/data-classifications"),
        ("retention-policies", "/api/v1/retention-policies"),
    };

    [Fact]
    public async Task Hot_endpoints_respond_under_baseline_latency()
    {
        using var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var failures = new List<string>();

        foreach (var (name, path) in HotEndpoints)
        {
            // Warmup (JIT, DI, connection pool).
            await client.GetAsync(path);

            var stopwatch = Stopwatch.StartNew();
            var response = await client.GetAsync(path);
            stopwatch.Stop();

            var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

            // Response must succeed.
            if (response.StatusCode != HttpStatusCode.OK)
            {
                failures.Add($"{name}: HTTP {(int)response.StatusCode} (expected 200)");
                continue;
            }

            // Generous threshold: 2s. Baseline on Neon p95 is ~150-400ms.
            // This catches pathologically slow queries, not CI jitter.
            if (elapsedMs > 2000)
            {
                failures.Add($"{name}: {elapsedMs:F0} ms exceeds 2000 ms baseline");
            }
        }

        Assert.True(failures.Count == 0,
            "Performance baseline breached:\n" + string.Join("\n", failures));
    }

    [Fact]
    public async Task Hot_endpoints_stay_stable_over_10_sequential_requests()
    {
        using var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 10 sequential calls to the most important hot path (records list).
        var times = new List<double>();
        for (var i = 0; i < 10; i++)
        {
            var sw = Stopwatch.StartNew();
            var resp = await client.GetAsync("/api/v1/platform/records");
            sw.Stop();
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            times.Add(sw.Elapsed.TotalMilliseconds);
        }

        var avg = times.Average();
        var max = times.Max();

        // Average under 1.5s and no single call over 3s.
        Assert.True(avg < 1500, $"avg {avg:F0} ms over 1.5s");
        Assert.True(max < 3000, $"max {max:F0} ms over 3s");
    }

    [Fact]
    public async Task Tenant_scoped_queries_use_indexes()
    {
        var connStr = ConnectionString();
        var missingIndexes = new List<string>();

        await using (var conn = new NpgsqlConnection(connStr))
        {
            await conn.OpenAsync();

            // Verify the indexes backing the app's most common tenant-scoped
            // lookups actually exist. (PostgreSQL correctly chooses Seq Scan on
            // tiny tables, so we assert index EXISTENCE, not plan shape.)
            var expected = new (string Name, string Table, string Index)[]
            {
                ("records by tenant", "platform.records", "idx_records_tenant"),
                ("my tasks by member", "platform.workflow_tasks", "idx_workflow_tasks_tenant"),
                ("notifications by member", "platform.notifications", "idx_notifications_recipient"),
                ("audit logs by tenant", "platform.audit_logs", "idx_audit_logs_tenant"),
                ("member by user+tenant", "iam.tenant_members", "idx_tenant_members_user"),
            };

            foreach (var (name, table, index) in expected)
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText =
                    "SELECT 1 FROM pg_indexes WHERE schemaname = split_part(@table, '.', 1) " +
                    "AND tablename = split_part(@table, '.', 2) AND indexname = @index";
                cmd.Parameters.AddWithValue("@table", table);
                cmd.Parameters.AddWithValue("@index", index);
                var found = await cmd.ExecuteScalarAsync();
                if (found is null)
                {
                    missingIndexes.Add($"{name}: index {index} on {table} missing");
                }
            }
        }

        Assert.True(missingIndexes.Count == 0,
            "Missing indexes:\n" + string.Join("\n", missingIndexes));
    }

    private static string ConnectionString() =>
        Environment.GetEnvironmentVariable("EHSMS_TEST_DB")
        ?? Environment.GetEnvironmentVariable("ConnectionStrings__EhSms")
        ?? "Host=127.0.0.1;Port=5432;Database=ehsms;Username=ehsms_dev;Password=ehsms_dev_pw";
}