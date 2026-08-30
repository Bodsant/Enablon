using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class InspectionAuditFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public InspectionAuditFlowTests(WebApplicationFactory<Program> factory)
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
    public async Task Audit_program_audit_finding_flow()
    {
        await LoginAsync();

        // 1. Create an audit program.
        var program = await _client.PostAsJsonAsync("/api/v1/audit/programs", new
        {
            name = "Annual HSE Compliance",
            periodStart = "2026-01-01",
            periodEnd = "2026-12-31",
            ownerMemberId = (Guid?)null,
            status = "Active",
        });
        program.EnsureSuccessStatusCode();
        var programId = GuidFrom((await program.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id"));

        // 2. Conduct an audit.
        var auditResp = await _client.PostAsJsonAsync("/api/v1/audits", new
        {
            auditProgramId = programId,
            auditType = "Internal",
            scopeText = "Chemical storage area",
            criteriaText = "EHSMS Clause 8.1",
            leadAuditorMemberId = (Guid?)null,
            scheduledStart = "2026-06-01",
            scheduledEnd = "2026-06-05",
        });
        auditResp.EnsureSuccessStatusCode();
        var audit = await auditResp.Content.ReadFromJsonAsync<JsonElement>();
        var auditId = GuidFrom(audit.GetProperty("id"));
        Assert.False(string.IsNullOrWhiteSpace(audit.GetProperty("recordNumber").GetString()));

        // 3. Add a finding.
        var finding = await _client.PostAsJsonAsync("/api/v1/audits/findings", new
        {
            auditId,
            classification = "Nonconformity",
            requirementReference = "8.1.2",
            description = "Emergency signage missing in storage area",
            recommendation = "Install signage",
            ownerMemberId = (Guid?)null,
        });
        finding.EnsureSuccessStatusCode();

        // 4. Verify round-trips.
        var programs = await (await _client.GetAsync("/api/v1/audit/programs")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(programs.EnumerateArray(), p => p.GetProperty("id").GetString() == programId.ToString());

        var audits = await (await _client.GetAsync("/api/v1/audits")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(audits.EnumerateArray(), a => a.GetProperty("id").GetString() == auditId.ToString());

        var findings = await (await _client.GetAsync("/api/v1/audits/findings?auditId=" + auditId)).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(findings.EnumerateArray());
    }

    [Fact]
    public async Task Inspection_and_finding_flow()
    {
        await LoginAsync();

        // 1. Conduct an inspection.
        var inspResp = await _client.PostAsJsonAsync("/api/v1/inspections", new
        {
            scopeText = "PPE compliance walkthrough",
            inspectorMemberId = (Guid?)null,
            plannedAt = "2026-07-01T09:00:00Z",
        });
        inspResp.EnsureSuccessStatusCode();
        var inspection = await inspResp.Content.ReadFromJsonAsync<JsonElement>();
        var inspectionId = GuidFrom(inspection.GetProperty("id"));
        Assert.False(string.IsNullOrWhiteSpace(inspection.GetProperty("recordNumber").GetString()));

        // 2. Add a finding.
        var finding = await _client.PostAsJsonAsync("/api/v1/inspections/findings", new
        {
            inspectionId,
            classification = "Minor",
            severityId = (Guid?)null,
            description = "Respirator not fit-tested",
            ownerMemberId = (Guid?)null,
        });
        finding.EnsureSuccessStatusCode();

        // 3. Verify round-trip.
        var inspections = await (await _client.GetAsync("/api/v1/inspections")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(inspections.EnumerateArray(), i => i.GetProperty("id").GetString() == inspectionId.ToString());

        var findings = await (await _client.GetAsync("/api/v1/inspections/findings?inspectionId=" + inspectionId)).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(findings.EnumerateArray());
    }
}
