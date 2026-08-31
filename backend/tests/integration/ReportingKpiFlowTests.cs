using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class ReportingKpiFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ReportingKpiFlowTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
        });
    }

    private async Task LoginAsync()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@ehsms.local",
            password = "EhsmsDev!123",
        });
        resp.EnsureSuccessStatusCode();
        var payload = await resp.Content.ReadFromJsonAsync<LoginPayload>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", payload!.AccessToken);
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);

    private static Guid GuidFrom(object value) => Guid.Parse(value!.ToString()!);

    [Fact]
    public async Task Report_definition_schedule_execution_and_kpi_flow()
    {
        await LoginAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // 1. Report definition.
        var def = await _client.PostAsJsonAsync("/api/v1/reports/definitions", new
        {
            code = $"RPT-{suffix}",
            name = $"Leadership EHS Dashboard {suffix}",
            reportType = "Dashboard",
            datasetCode = "IncidentsSummary",
            filterSchemaJson = "{\"period\":\"month\"}",
            requiredPermissionId = (string?)null,
        });
        def.EnsureSuccessStatusCode();
        var defDto = await def.Content.ReadFromJsonAsync<JsonElement>();
        var defId = GuidFrom(defDto.GetProperty("id"));
        Assert.Equal("RPT-" + suffix.ToUpperInvariant(), defDto.GetProperty("code").GetString());

        // 2. Report schedule (OwnerMemberId resolved).
        var sched = await _client.PostAsJsonAsync("/api/v1/reports/schedules", new
        {
            reportDefinitionId = defId,
            ownerMemberId = "00000000-0000-0000-0000-000000000000",
            scheduleRule = "0 7 * * 1",
            deliveryConfigurationJson = "{\"email\":true}",
            status = "Active",
        });
        sched.EnsureSuccessStatusCode();
        var schedDto = await sched.Content.ReadFromJsonAsync<JsonElement>();
        var schedId = GuidFrom(schedDto.GetProperty("id"));
        Assert.NotEqual(Guid.Empty, schedDto.GetProperty("ownerMemberId").GetGuid());

        // 3. Report execution (requester resolved; schedule optional).
        var exec = await _client.PostAsJsonAsync("/api/v1/reports/executions", new
        {
            reportDefinitionId = defId,
            reportScheduleId = schedId,
            requestedByMemberId = (string?)null,
            filterValuesJson = "{\"period\":\"2026-08\"}",
            status = "Queued",
        });
        exec.EnsureSuccessStatusCode();
        var execDto = await exec.Content.ReadFromJsonAsync<JsonElement>();
        var execId = GuidFrom(execDto.GetProperty("id"));
        Assert.Equal("Queued", execDto.GetProperty("status").GetString());

        // 4. KPI definition (OwnerMemberId resolved).
        var kpi = await _client.PostAsJsonAsync("/api/v1/kpis", new
        {
            code = $"LTIR-{suffix}",
            name = "Lost Time Injury Rate",
            description = "Rate per 200k hours",
            ownerMemberId = "00000000-0000-0000-0000-000000000000",
            status = "Active",
        });
        kpi.EnsureSuccessStatusCode();
        var kpiDto = await kpi.Content.ReadFromJsonAsync<JsonElement>();
        var kpiId = GuidFrom(kpiDto.GetProperty("id"));
        Assert.NotEqual(Guid.Empty, kpiDto.GetProperty("ownerMemberId").GetGuid());

        // 5. KPI version.
        var ver = await _client.PostAsJsonAsync("/api/v1/kpis/versions", new
        {
            kpiDefinitionId = kpiId,
            versionNumber = 1,
            formulaExpression = "(Fatalities + LostTimeInjuries) / 200000",
            numeratorDefinition = "Fatalities + LostTimeInjuries",
            denominatorDefinition = "200000 (hours)",
            factor = 0,
            periodRule = "monthly",
            scopeRuleJson = (string?)null,
            effectiveFrom = "2026-01-01",
            effectiveTo = (string?)null,
        });
        ver.EnsureSuccessStatusCode();
        var verDto = await ver.Content.ReadFromJsonAsync<JsonElement>();
        var verId = GuidFrom(verDto.GetProperty("id"));
        Assert.Equal(1, verDto.GetProperty("versionNumber").GetInt32());

        // 6. Verify round-trips.
        var defs = await (await _client.GetAsync("/api/v1/reports/definitions")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(defs.EnumerateArray(), r => r.GetProperty("id").GetString() == defId.ToString());

        var scheds = await (await _client.GetAsync("/api/v1/reports/schedules")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(scheds.EnumerateArray(), s => s.GetProperty("id").GetString() == schedId.ToString());

        var execs = await (await _client.GetAsync($"/api/v1/reports/executions?reportDefinitionId={defId}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(execs.EnumerateArray(), e => e.GetProperty("id").GetString() == execId.ToString());

        var kpis = await (await _client.GetAsync("/api/v1/kpis")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(kpis.EnumerateArray(), k => k.GetProperty("id").GetString() == kpiId.ToString());

        var vers = await (await _client.GetAsync($"/api/v1/kpis/versions?kpiDefinitionId={kpiId}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(vers.EnumerateArray(), v => v.GetProperty("id").GetString() == verId.ToString());
    }
}