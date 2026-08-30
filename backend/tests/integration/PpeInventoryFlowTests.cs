using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class PpeInventoryFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PpeInventoryFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<Guid> LoginAndCreateCatalogAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        var payload = await login.Content.ReadFromJsonAsync<LoginPayload>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", payload!.AccessToken);

        var create = await _client.PostAsJsonAsync("/api/v1/ppe/catalog", new
        {
            code = "SR-01",
            name = "Safety Harness",
            ppeCategory = "Fall Protection",
        });
        var cat = await create.Content.ReadFromJsonAsync<PpeCatalogPayload>();
        return cat!.Id;
    }

    [Fact]
    public async Task Register_and_list_inventory_round_trips()
    {
        var catalogId = await LoginAndCreateCatalogAsync();

        var create = await _client.PostAsJsonAsync("/api/v1/ppe/inventory", new
        {
            ppeCatalogId = catalogId,
            siteId = Guid.NewGuid(),
            serialNumber = "HARNESS-001",
            quantityOnHand = 3,
            condition = "New",
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var item = await create.Content.ReadFromJsonAsync<PpeInventoryPayload>();
        Assert.Equal("HARNESS-001", item!.SerialNumber);
        Assert.Equal("Available", item.Status);

        var list = await _client.GetAsync($"/api/v1/ppe/inventory?ppeCatalogId={catalogId}");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var items = await list.Content.ReadFromJsonAsync<List<PpeInventoryPayload>>();
        Assert.Contains(items!, i => i.Id == item.Id);
    }

    [Fact]
    public async Task Assign_and_return_ppe_round_trips()
    {
        var catalogId = await LoginAndCreateCatalogAsync();

        var inv = await _client.PostAsJsonAsync("/api/v1/ppe/inventory", new
        {
            ppeCatalogId = catalogId,
            siteId = Guid.NewGuid(),
            serialNumber = "HARNESS-002",
            quantityOnHand = 1,
        });
        var inventory = await inv.Content.ReadFromJsonAsync<PpeInventoryPayload>();

        var assign = await _client.PostAsJsonAsync("/api/v1/ppe/assignments", new
        {
            ppeInventoryId = inventory!.Id,
            personId = Guid.NewGuid(),
        });
        Assert.Equal(HttpStatusCode.Created, assign.StatusCode);
        var assignment = await assign.Content.ReadFromJsonAsync<PpeAssignmentPayload>();
        Assert.NotEqual(default, assignment!.IssuedAt);

        // Inventory now Issued.
        var listInv = await _client.GetAsync($"/api/v1/ppe/inventory?ppeCatalogId={catalogId}");
        var invItems = await listInv.Content.ReadFromJsonAsync<List<PpeInventoryPayload>>();
        Assert.Contains(invItems!, i => i.Id == inventory.Id && i.Status == "Issued");

        // Return.
        var ret = await _client.PostAsJsonAsync(
            $"/api/v1/ppe/assignments/{assignment.Id}/return", new
            {
                conditionOnReturn = "Good",
            });
        Assert.Equal(HttpStatusCode.OK, ret.StatusCode);
        var returned = await ret.Content.ReadFromJsonAsync<PpeAssignmentPayload>();
        Assert.NotNull(returned!.ReturnedAt);

        var list = await _client.GetAsync($"/api/v1/ppe/assignments?ppeInventoryId={inventory.Id}");
        var items = await list.Content.ReadFromJsonAsync<List<PpeAssignmentPayload>>();
        Assert.Contains(items!, a => a.Id == assignment.Id && a.ReturnedAt is not null);
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);
    public sealed record PpeCatalogPayload(Guid Id);
    public sealed record PpeInventoryPayload(Guid Id, Guid PpeCatalogId, Guid SiteId, string? SerialNumber, int? QuantityOnHand, string? Condition, string Status);
    public sealed record PpeAssignmentPayload(Guid Id, Guid PpeInventoryId, Guid PersonId, DateTimeOffset IssuedAt, Guid IssuedByMemberId, DateTimeOffset? ReturnedAt, string? ConditionOnReturn);
}