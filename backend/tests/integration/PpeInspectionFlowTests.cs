using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class PpeInspectionFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PpeInspectionFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid InventoryId, Guid AssignmentId)> SetupAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        var payload = await login.Content.ReadFromJsonAsync<LoginPayload>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", payload!.AccessToken);

        var cat = await _client.PostAsJsonAsync("/api/v1/ppe/catalog", new
        {
            code = "RS-01",
            name = "Respirator",
            ppeCategory = "Respiratory",
        });
        var catalog = await cat.Content.ReadFromJsonAsync<PpeCatalogPayload>();

        var inv = await _client.PostAsJsonAsync("/api/v1/ppe/inventory", new
        {
            ppeCatalogId = catalog!.Id,
            siteId = Guid.NewGuid(),
            serialNumber = "RESP-001",
            quantityOnHand = 2,
        });
        var inventory = await inv.Content.ReadFromJsonAsync<PpeInventoryPayload>();

        var assign = await _client.PostAsJsonAsync("/api/v1/ppe/assignments", new
        {
            ppeInventoryId = inventory!.Id,
            personId = Guid.NewGuid(),
        });
        var assignment = await assign.Content.ReadFromJsonAsync<PpeAssignmentPayload>();

        return (inventory.Id, assignment!.Id);
    }

    [Fact]
    public async Task Record_and_list_inspection_round_trips()
    {
        var (inventoryId, _) = await SetupAsync();

        var create = await _client.PostAsJsonAsync("/api/v1/ppe/inspections", new
        {
            ppeInventoryId = inventoryId,
            condition = "Good",
            result = "Passed",
            nextDueDate = "2026-09-30",
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var inspection = await create.Content.ReadFromJsonAsync<PpeInspectionPayload>();
        Assert.Equal("Passed", inspection!.Result);
        Assert.NotEqual(default, inspection.InspectedAt);

        var list = await _client.GetAsync($"/api/v1/ppe/inspections?ppeInventoryId={inventoryId}");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var items = await list.Content.ReadFromJsonAsync<List<PpeInspectionPayload>>();
        Assert.Contains(items!, i => i.Id == inspection.Id);
    }

    [Fact]
    public async Task Failed_inspection_flips_inventory_status()
    {
        var (inventoryId, _) = await SetupAsync();

        var create = await _client.PostAsJsonAsync("/api/v1/ppe/inspections", new
        {
            ppeInventoryId = inventoryId,
            condition = "Torn strap",
            result = "Defective",
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var listInv = await _client.GetAsync("/api/v1/ppe/inventory");
        // fetch all inventory and find ours
        var invItems = await listInv.Content.ReadFromJsonAsync<List<PpeInventoryPayload>>();
        Assert.Contains(invItems!, i => i.Id == inventoryId && i.Status == "NeedsReplacement");
    }

    [Fact]
    public async Task Request_and_complete_replacement_round_trips()
    {
        var (_, assignmentId) = await SetupAsync();

        var create = await _client.PostAsJsonAsync("/api/v1/ppe/replacements", new
        {
            ppeAssignmentId = assignmentId,
            replacementReason = "Damaged beyond repair",
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var replacement = await create.Content.ReadFromJsonAsync<PpeReplacementPayload>();
        Assert.Equal("Damaged beyond repair", replacement!.ReplacementReason);
        Assert.Null(replacement.CompletedAt);

        var complete = await _client.PostAsJsonAsync(
            $"/api/v1/ppe/replacements/{replacement.Id}/complete", new { });
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);
        var done = await complete.Content.ReadFromJsonAsync<PpeReplacementPayload>();
        Assert.NotNull(done!.CompletedAt);

        var list = await _client.GetAsync($"/api/v1/ppe/replacements?ppeAssignmentId={assignmentId}");
        var items = await list.Content.ReadFromJsonAsync<List<PpeReplacementPayload>>();
        Assert.Contains(items!, r => r.Id == replacement.Id && r.CompletedAt is not null);
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);
    public sealed record PpeCatalogPayload(Guid Id);
    public sealed record PpeInventoryPayload(Guid Id, string Status);
    public sealed record PpeAssignmentPayload(Guid Id);
    public sealed record PpeInspectionPayload(Guid Id, Guid PpeInventoryId, DateTimeOffset InspectedAt, string Condition, string Result, DateOnly? NextDueDate);
    public sealed record PpeReplacementPayload(Guid Id, Guid PpeAssignmentId, string ReplacementReason, DateTimeOffset RequestedAt, DateTimeOffset? CompletedAt);
}