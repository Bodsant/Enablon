using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class HazardRiskFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HazardRiskFlowTests(WebApplicationFactory<Program> factory)
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
        var payload = await resp.Content.ReadFromJsonAsync<AuthPayload>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", payload!.AccessToken);
    }

    private sealed record AuthPayload(string AccessToken);

    private static Guid GuidFrom(object value) => Guid.Parse(value!.ToString()!);

    [Fact]
    public async Task Full_hazard_risk_flow_round_trips()
    {
        await LoginAsync();

        // 1. Create a risk matrix version + a cell.
        var matrix = await _client.PostAsJsonAsync("/api/v1/risk/matrix-versions", new
        {
            name = "5x5 Standard",
            versionNumber = 1,
            likelihoodScale = 5,
            severityScale = 5,
            effectiveFrom = "2026-01-01",
            effectiveTo = (string?)null,
            status = "Active",
        });
        matrix.EnsureSuccessStatusCode();
        var matrixId = GuidFrom((await matrix.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id"));

        var cell = await _client.PostAsJsonAsync("/api/v1/risk/matrix-cells", new
        {
            matrixVersionId = matrixId,
            likelihoodValue = 3,
            severityValue = 4,
            riskScore = 12,
            riskLevelCode = "High",
        });
        cell.EnsureSuccessStatusCode();

        // 2. Create a hazard.
        var hazard = await _client.PostAsJsonAsync("/api/v1/risk/hazards", new
        {
            code = "HZ-" + Guid.NewGuid().ToString("N")[..6],
            name = "Chemical handling",
            categoryId = (Guid?)null,
            description = "Exposure to cleaning solvents",
            status = "Active",
        });
        hazard.EnsureSuccessStatusCode();
        var hazardId = GuidFrom((await hazard.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id"));

        // 3. Register the hazard in a risk register.
        var register = await _client.PostAsJsonAsync("/api/v1/risk/registers", new
        {
            hazardId,
            activityName = "Batch mixing",
            riskEvent = "Skin irritation from solvent splash",
            ownerMemberId = (Guid?)null,
            reviewDate = "2026-06-30",
            status = "Active",
        });
        register.EnsureSuccessStatusCode();
        var registerId = GuidFrom((await register.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id"));

        // 4. Assess the risk (likelihood 3 x severity 4 => score 12 => High).
        var assessment = await _client.PostAsJsonAsync("/api/v1/risk/assessments", new
        {
            riskRegisterId = registerId,
            matrixVersionId = matrixId,
            assessmentType = "Initial",
            likelihoodValue = 3,
            severityValue = 4,
        });
        assessment.EnsureSuccessStatusCode();
        var assessmentObj = await assessment.Content.ReadFromJsonAsync<JsonElement>();

        // 5. Add a control.
        var control = await _client.PostAsJsonAsync("/api/v1/risk/controls", new
        {
            riskRegisterId = registerId,
            controlType = "PPE",
            controlStage = "Prevention",
            description = "Provide nitrile gloves",
            ownerMemberId = (Guid?)null,
            dueDate = "2026-05-01",
            status = "Planned",
        });
        control.EnsureSuccessStatusCode();

        // 6. List everything and verify round-trip.
        var hazards = (await (await _client.GetAsync("/api/v1/risk/hazards")).Content.ReadFromJsonAsync<JsonElement>())
            .EnumerateArray().Where(h => h.GetProperty("id").GetString() == hazardId.ToString()).ToList();
        Assert.Single(hazards);

        var registers = (await (await _client.GetAsync("/api/v1/risk/registers")).Content.ReadFromJsonAsync<JsonElement>())
            .EnumerateArray().Where(r => r.GetProperty("id").GetString() == registerId.ToString()).ToList();
        Assert.Single(registers);

        var assessments = (await (await _client.GetAsync("/api/v1/risk/assessments")).Content.ReadFromJsonAsync<JsonElement>())
            .EnumerateArray().Where(a => a.GetProperty("riskRegisterId").GetString() == registerId.ToString()).ToList();
        Assert.Single(assessments);

        var controls = (await (await _client.GetAsync("/api/v1/risk/controls")).Content.ReadFromJsonAsync<JsonElement>())
            .EnumerateArray().Where(c => c.GetProperty("riskRegisterId").GetString() == registerId.ToString()).ToList();
        Assert.Single(controls);
    }

    [Fact]
    public async Task Assessment_scores_and_classifies_risk()
    {
        await LoginAsync();

        var matrix = await _client.PostAsJsonAsync("/api/v1/risk/matrix-versions", new
        {
            name = "3x3 Standard",
            versionNumber = 1,
            likelihoodScale = 3,
            severityScale = 3,
            effectiveFrom = "2026-01-01",
            effectiveTo = (string?)null,
            status = "Active",
        });
        matrix.EnsureSuccessStatusCode();
        var matrixId = GuidFrom((await matrix.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id"));

        var hazard = await _client.PostAsJsonAsync("/api/v1/risk/hazards", new
        {
            code = "HZ-" + Guid.NewGuid().ToString("N")[..6],
            name = "Working at height",
            categoryId = (Guid?)null,
            description = (string?)null,
            status = "Active",
        });
        hazard.EnsureSuccessStatusCode();
        var hazardId = GuidFrom((await hazard.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id"));

        var register = await _client.PostAsJsonAsync("/api/v1/risk/registers", new
        {
            hazardId,
            activityName = "Roof maintenance",
            riskEvent = "Fall from height",
            ownerMemberId = (Guid?)null,
            reviewDate = (string?)null,
            status = "Active",
        });
        register.EnsureSuccessStatusCode();
        var registerId = GuidFrom((await register.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id"));

        // 5 x 5 = 25 => Extreme.
        var assessment = await _client.PostAsJsonAsync("/api/v1/risk/assessments", new
        {
            riskRegisterId = registerId,
            matrixVersionId = matrixId,
            assessmentType = "Initial",
            likelihoodValue = 5,
            severityValue = 5,
        });
        assessment.EnsureSuccessStatusCode();

        var assessments = await (await _client.GetAsync("/api/v1/risk/assessments")).Content.ReadFromJsonAsync<JsonElement>();
        var created = assessments.EnumerateArray()
            .First(a => a.GetProperty("riskRegisterId").GetString() == registerId.ToString());
        Assert.Equal(25, created.GetProperty("riskScore").GetInt32());
        Assert.Equal("Extreme", created.GetProperty("riskLevelCode").GetString());
    }
}
