using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class AssetEmergencyFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AssetEmergencyFlowTests(WebApplicationFactory<Program> factory)
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
    public async Task Asset_emergency_plan_team_equipment_drill_finding_flow()
    {
        await LoginAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var siteId = Guid.NewGuid();
        var personId = Guid.NewGuid();

        // 1. Safety-critical asset (record-backed 'AST'; site/location FKs dropped).
        var asset = await _client.PostAsJsonAsync("/api/v1/assets", new
        {
            sourceSystem = "CMMS",
            sourceId = $"SRC-{suffix}",
            assetCode = $"AST-{suffix}",
            assetName = $"Fire Pump {suffix}",
            assetType = "Fire Protection",
            siteId,
            locationId = (string?)null,
            isSafetyCritical = true,
            status = "Active",
        });
        asset.EnsureSuccessStatusCode();
        var assetDto = await asset.Content.ReadFromJsonAsync<JsonElement>();
        var assetId = GuidFrom(assetDto.GetProperty("id"));
        Assert.False(string.IsNullOrWhiteSpace(assetDto.GetProperty("recordNumber").GetString()));
        Assert.True(assetDto.GetProperty("isSafetyCritical").GetBoolean());

        // 2. Emergency plan (record-backed 'EMG'; OwnerMemberId resolved; site FK dropped).
        var plan = await _client.PostAsJsonAsync("/api/v1/emergency/plans", new
        {
            code = $"PLN-{suffix}",
            name = $"Fire Response Plan {suffix}",
            siteId,
            ownerMemberId = "00000000-0000-0000-0000-000000000000",
            status = "Active",
        });
        plan.EnsureSuccessStatusCode();
        var planDto = await plan.Content.ReadFromJsonAsync<JsonElement>();
        var planId = GuidFrom(planDto.GetProperty("id"));
        Assert.False(string.IsNullOrWhiteSpace(planDto.GetProperty("recordNumber").GetString()));
        Assert.NotEqual(Guid.Empty, planDto.GetProperty("ownerMemberId").GetGuid());

        // 3. Team member (PersonId cross-schema; FK dropped).
        var member = await _client.PostAsJsonAsync("/api/v1/emergency/team-members", new
        {
            emergencyPlanId = planId,
            personId,
            emergencyRole = "Incident Commander",
            validFrom = "2026-09-01",
            validTo = "2027-09-01",
        });
        member.EnsureSuccessStatusCode();
        var memberDto = await member.Content.ReadFromJsonAsync<JsonElement>();
        var memberId = GuidFrom(memberDto.GetProperty("id"));

        // 4. Emergency equipment (site FK dropped; location/asset null).
        var equipment = await _client.PostAsJsonAsync("/api/v1/emergency/equipment", new
        {
            siteId,
            locationId = (string?)null,
            equipmentType = "Fire Extinguisher",
            assetId = (string?)null,
            inspectionDueDate = "2026-10-01",
            maintenanceDueDate = "2026-11-01",
            status = "Operational",
        });
        equipment.EnsureSuccessStatusCode();
        var equipmentDto = await equipment.Content.ReadFromJsonAsync<JsonElement>();
        var equipmentId = GuidFrom(equipmentDto.GetProperty("id"));

        // 5. Drill (record-backed 'EMG'; coordinator null).
        var drill = await _client.PostAsJsonAsync("/api/v1/emergency/drills", new
        {
            emergencyPlanId = planId,
            scenario = $"Full-scale fire drill {suffix}",
            scheduledAt = "2026-10-15T09:00:00Z",
            coordinatorMemberId = (string?)null,
        });
        drill.EnsureSuccessStatusCode();
        var drillDto = await drill.Content.ReadFromJsonAsync<JsonElement>();
        var drillId = GuidFrom(drillDto.GetProperty("id"));
        Assert.False(string.IsNullOrWhiteSpace(drillDto.GetProperty("recordNumber").GetString()));

        // 6. Drill finding (record-backed 'EMG'; OwnerMemberId resolved).
        var finding = await _client.PostAsJsonAsync("/api/v1/emergency/drill-findings", new
        {
            emergencyDrillId = drillId,
            description = $"Evacuation route blockage {suffix}",
            severity = "Medium",
            ownerMemberId = "00000000-0000-0000-0000-000000000000",
        });
        finding.EnsureSuccessStatusCode();
        var findingDto = await finding.Content.ReadFromJsonAsync<JsonElement>();
        var findingId = GuidFrom(findingDto.GetProperty("id"));
        Assert.NotEqual(Guid.Empty, findingDto.GetProperty("ownerMemberId").GetGuid());

        // 7. Verify round-trips.
        var assets = await (await _client.GetAsync("/api/v1/assets")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(assets.EnumerateArray(), a => a.GetProperty("id").GetString() == assetId.ToString());

        var plans = await (await _client.GetAsync("/api/v1/emergency/plans")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(plans.EnumerateArray(), p => p.GetProperty("id").GetString() == planId.ToString());

        var members = await (await _client.GetAsync($"/api/v1/emergency/team-members?planId={planId}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(members.EnumerateArray(), m => m.GetProperty("id").GetString() == memberId.ToString());

        var equipments = await (await _client.GetAsync("/api/v1/emergency/equipment")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(equipments.EnumerateArray(), e => e.GetProperty("id").GetString() == equipmentId.ToString());

        var drills = await (await _client.GetAsync("/api/v1/emergency/drills")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(drills.EnumerateArray(), d => d.GetProperty("id").GetString() == drillId.ToString());

        var findings = await (await _client.GetAsync($"/api/v1/emergency/drill-findings?drillId={drillId}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(findings.EnumerateArray(), f => f.GetProperty("id").GetString() == findingId.ToString());
    }
}