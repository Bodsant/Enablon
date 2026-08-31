using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class LegalComplianceFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LegalComplianceFlowTests(WebApplicationFactory<Program> factory)
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
    public async Task Legal_source_version_obligation_applicability_flow()
    {
        await LoginAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // 1. Legal source.
        var source = await _client.PostAsJsonAsync("/api/v1/legal/sources", new
        {
            sourceType = "Regulation",
            code = $"REG-{suffix}",
            title = $"Workplace Safety Regulation {suffix}",
            jurisdiction = "ID",
            publisher = "Government",
            sourceUrl = "https://example.gov/reg",
            status = "Active",
        });
        source.EnsureSuccessStatusCode();
        var sourceDto = await source.Content.ReadFromJsonAsync<JsonElement>();
        var sourceId = GuidFrom(sourceDto.GetProperty("id"));

        // 2. Version of the legal source.
        var version = await _client.PostAsJsonAsync("/api/v1/legal/source-versions", new
        {
            legalSourceId = sourceId,
            versionLabel = "Rev. 2026",
            publishedDate = "2026-01-01",
            effectiveDate = "2026-03-01",
            supersededDate = (string?)null,
            changeSummary = "Annual update",
        });
        version.EnsureSuccessStatusCode();
        var versionDto = await version.Content.ReadFromJsonAsync<JsonElement>();
        var versionId = GuidFrom(versionDto.GetProperty("id"));

        // 3. Obligation (record-backed; OwnerMemberId resolved server-side from active member).
        var obligation = await _client.PostAsJsonAsync("/api/v1/legal/obligations", new
        {
            legalSourceVersionId = versionId,
            clauseReference = "Clause 7.2",
            requirementText = $"Conduct quarterly ergonomic assessment {suffix}",
            ownerMemberId = "00000000-0000-0000-0000-000000000000",
            frequency = "Quarterly",
            dueDate = "2026-12-31",
            lastReview = (string?)null,
            nextReview = "2026-09-30",
        });
        obligation.EnsureSuccessStatusCode();
        var obligationDto = await obligation.Content.ReadFromJsonAsync<JsonElement>();
        var obligationId = GuidFrom(obligationDto.GetProperty("id"));
        Assert.False(string.IsNullOrWhiteSpace(obligationDto.GetProperty("recordNumber").GetString()));
        // Owner member id must not be Guid.Empty (server resolved real member).
        Assert.NotEqual(Guid.Empty, obligationDto.GetProperty("ownerMemberId").GetGuid());

        // 4. Obligation applicability (AssessedByMemberId resolved server-side; org refs null).
        var applicability = await _client.PostAsJsonAsync("/api/v1/legal/obligation-applicability", new
        {
            obligationId,
            companyId = (string?)null,
            businessUnitId = (string?)null,
            siteId = (string?)null,
            applicabilityStatus = "Applicable",
            rationale = "All corporate sites",
            assessedByMemberId = "00000000-0000-0000-0000-000000000000",
        });
        applicability.EnsureSuccessStatusCode();
        var applicabilityDto = await applicability.Content.ReadFromJsonAsync<JsonElement>();
        var applicabilityId = GuidFrom(applicabilityDto.GetProperty("id"));
        Assert.NotEqual(Guid.Empty, applicabilityDto.GetProperty("assessedByMemberId").GetGuid());

        // 5. Verify round-trips.
        var sources = await (await _client.GetAsync("/api/v1/legal/sources")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(sources.EnumerateArray(), s => s.GetProperty("id").GetString() == sourceId.ToString());

        var versions = await (await _client.GetAsync($"/api/v1/legal/source-versions?legalSourceId={sourceId}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(versions.EnumerateArray(), v => v.GetProperty("id").GetString() == versionId.ToString());

        var obligations = await (await _client.GetAsync("/api/v1/legal/obligations")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(obligations.EnumerateArray(), o => o.GetProperty("id").GetString() == obligationId.ToString());

        var applicabilities = await (await _client.GetAsync($"/api/v1/legal/obligation-applicability?obligationId={obligationId}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(applicabilities.EnumerateArray(), a => a.GetProperty("id").GetString() == applicabilityId.ToString());
    }
}