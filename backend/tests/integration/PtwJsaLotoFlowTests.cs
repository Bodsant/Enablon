using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class PtwJsaLotoFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PtwJsaLotoFlowTests(WebApplicationFactory<Program> factory)
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
    public async Task Work_request_jsa_permit_loto_full_flow()
    {
        await LoginAsync();

        // 1. Create a work request.
        var wr = await _client.PostAsJsonAsync("/api/v1/work-requests", new
        {
            workDescription = "Replace flange gasket on line 7",
            workType = "Maintenance",
            contractorCompanyId = (Guid?)null,
            plannedStart = "2026-08-01T08:00:00Z",
            plannedEnd = "2026-08-01T16:00:00Z",
        });
        wr.EnsureSuccessStatusCode();
        var wrDto = await wr.Content.ReadFromJsonAsync<JsonElement>();
        var workRequestId = GuidFrom(wrDto.GetProperty("id"));
        Assert.False(string.IsNullOrWhiteSpace(wrDto.GetProperty("recordNumber").GetString()));

        // 2. Create a JSA for the work request.
        var jsa = await _client.PostAsJsonAsync("/api/v1/jsas", new
        {
            workRequestId,
            templateVersionId = (Guid?)null,
            overallResidualRisk = "Low",
            status = "Approved",
        });
        jsa.EnsureSuccessStatusCode();
        var jsaDto = await jsa.Content.ReadFromJsonAsync<JsonElement>();
        var jsaId = GuidFrom(jsaDto.GetProperty("id"));

        // 3. Add a JSA step.
        var step = await _client.PostAsJsonAsync("/api/v1/jsas/steps", new
        {
            jsaId,
            sequenceNumber = 1,
            workStep = "Isolate pipeline",
        });
        step.EnsureSuccessStatusCode();

        // 4. Create a permit to work.
        var permit = await _client.PostAsJsonAsync("/api/v1/permits", new
        {
            workRequestId,
            jsaId,
            permitTypeVersionId = Guid.NewGuid(),
            validFrom = "2026-08-01T08:00:00Z",
            validUntil = "2026-08-01T16:00:00Z",
        });
        permit.EnsureSuccessStatusCode();
        var permitDto = await permit.Content.ReadFromJsonAsync<JsonElement>();
        var permitId = GuidFrom(permitDto.GetProperty("id"));
        Assert.False(string.IsNullOrWhiteSpace(permitDto.GetProperty("recordNumber").GetString()));

        // 5. Approve the permit.
        var approval = await _client.PostAsJsonAsync("/api/v1/permits/approvals", new
        {
            permitId,
            approvalLevel = 1,
            decision = "Approved",
        });
        approval.EnsureSuccessStatusCode();

        // 6. Record a gas test.
        var gas = await _client.PostAsJsonAsync("/api/v1/permits/gas-tests", new
        {
            permitId,
            testType = "PreJob",
            testedAt = "2026-08-01T07:55:00Z",
            oxygenPct = 20.9m,
            lelPct = 0.0m,
            toxicGasJson = (string?)null,
            result = "Pass",
        });
        gas.EnsureSuccessStatusCode();

        // 7. Create a LOTO isolation plan for the permit.
        var loto = await _client.PostAsJsonAsync("/api/v1/isolation-plans", new
        {
            permitId,
            status = "Active",
        });
        loto.EnsureSuccessStatusCode();
        var lotoDto = await loto.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(lotoDto.GetProperty("recordNumber").GetString()));

        // 8. Verify round-trips.
        var workRequests = await (await _client.GetAsync("/api/v1/work-requests")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(workRequests.EnumerateArray(), w => w.GetProperty("id").GetString() == workRequestId.ToString());

        var jsas = await (await _client.GetAsync("/api/v1/jsas")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(jsas.EnumerateArray(), j => j.GetProperty("id").GetString() == jsaId.ToString());

        var permits = await (await _client.GetAsync("/api/v1/permits")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(permits.EnumerateArray(), p => p.GetProperty("id").GetString() == permitId.ToString());

        var plans = await (await _client.GetAsync("/api/v1/isolation-plans")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(plans.EnumerateArray(), p => p.GetProperty("id").GetString() == lotoDto.GetProperty("id").GetString());
    }
}
