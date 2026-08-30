using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class IncidentCapaFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IncidentCapaFlowTests(WebApplicationFactory<Program> factory)
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
    public async Task Incident_investigation_capa_full_flow()
    {
        await LoginAsync();

        var typeId = Guid.NewGuid();
        var severityId = Guid.NewGuid();

        // 1. Report an incident.
        var incidentResp = await _client.PostAsJsonAsync("/api/v1/incidents", new
        {
            incidentTypeId = typeId,
            severityId,
            occurredAt = "2026-08-01T08:30:00Z",
            description = "Chemical splash on forearm during dilution",
            immediateAction = "Rinsed with water, first aid applied",
            classificationStatus = "NearMiss",
        });
        incidentResp.EnsureSuccessStatusCode();
        var incident = await incidentResp.Content.ReadFromJsonAsync<JsonElement>();
        var incidentId = GuidFrom(incident.GetProperty("id"));
        Assert.False(string.IsNullOrWhiteSpace(incident.GetProperty("recordNumber").GetString()));

        // 2. Add an involved person.
        var person = await _client.PostAsJsonAsync("/api/v1/incidents/involved-people", new
        {
            incidentId,
            personId = (Guid?)null,
            externalPersonName = "Worker A",
            involvementType = "Affected",
            injuryClassificationId = (Guid?)null,
            lostWorkDays = 2,
        });
        person.EnsureSuccessStatusCode();

        // 3. Start an investigation.
        var inv = await _client.PostAsJsonAsync("/api/v1/incidents/investigations", new
        {
            incidentId,
            method = "5 Whys",
            summary = "Investigating root cause",
        });
        inv.EnsureSuccessStatusCode();
        var investigationId = GuidFrom((await inv.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id"));

        // 4. Add a root cause.
        var cause = await _client.PostAsJsonAsync("/api/v1/incidents/root-causes", new
        {
            investigationId,
            causeType = "Root",
            categoryId = (Guid?)null,
            description = "Missing PPE training",
            evidenceSummary = "Training log review",
        });
        cause.EnsureSuccessStatusCode();

        // 5. Create a CAPA action.
        var actionResp = await _client.PostAsJsonAsync("/api/v1/capa/actions", new
        {
            actionType = "Corrective",
            description = "Conduct PPE refresher training",
            ownerMemberId = (Guid?)null,
            priority = "High",
            dueDate = "2026-09-30",
            verificationRequired = true,
        });
        actionResp.EnsureSuccessStatusCode();
        var action = await actionResp.Content.ReadFromJsonAsync<JsonElement>();
        var actionId = GuidFrom(action.GetProperty("id"));

        // 6. Progress the action.
        var progress = await _client.PostAsJsonAsync($"/api/v1/capa/actions/{actionId}/progress", new
        {
            progressPercentage = 100,
            note = "Training completed",
        });
        progress.EnsureSuccessStatusCode();

        // 7. Verify the action.
        var verify = await _client.PostAsJsonAsync($"/api/v1/capa/actions/{actionId}/verify", new
        {
            result = "Verified",
            comment = "Evidence submitted",
        });
        verify.EnsureSuccessStatusCode();

        // 8. Verify round-trips.
        var incidents = await (await _client.GetAsync("/api/v1/incidents")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(incidents.EnumerateArray(), i => i.GetProperty("id").GetString() == incidentId.ToString());

        var people = await (await _client.GetAsync("/api/v1/incidents/involved-people?incidentId=" + incidentId)).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(people.EnumerateArray());

        var investigations = await (await _client.GetAsync("/api/v1/incidents/investigations?incidentId=" + incidentId)).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(investigations.EnumerateArray());

        var causes = await (await _client.GetAsync("/api/v1/incidents/root-causes?investigationId=" + investigationId)).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(causes.EnumerateArray());

        var actions = await (await _client.GetAsync("/api/v1/capa/actions")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(actions.EnumerateArray(), a => a.GetProperty("id").GetString() == actionId.ToString());

        // Actions progressed should reflect 100%.
        var progressedAction = actions.EnumerateArray().First(a => a.GetProperty("id").GetString() == actionId.ToString());
        Assert.Equal(100, progressedAction.GetProperty("progressPercentage").GetInt32());
    }
}
