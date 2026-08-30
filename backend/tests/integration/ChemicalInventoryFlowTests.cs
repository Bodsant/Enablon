using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class ChemicalInventoryFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ChemicalInventoryFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string Token, Guid ProductId)> LoginAndCreateProductAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        var payload = await login.Content.ReadFromJsonAsync<LoginPayload>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", payload!.AccessToken);

        var create = await _client.PostAsJsonAsync("/api/v1/chemical/products", new
        {
            productName = "Isopropyl Alcohol 99%",
            productCode = "CHEM-IPA01",
            supplierName = "ChemSupply Co",
        });
        var created = await create.Content.ReadFromJsonAsync<CreateChemicalProductPayload>();
        return (payload.AccessToken, created!.Id);
    }

    [Fact]
    public async Task Add_and_list_inventory_round_trips()
    {
        var (_, productId) = await LoginAndCreateProductAsync();

        var locationId = Guid.NewGuid();
        var add = await _client.PostAsJsonAsync("/api/v1/chemical/inventory", new
        {
            chemicalProductId = productId,
            locationId = locationId,
            quantity = 25.5m,
            unit = "L",
            storageCondition = "Flamable cabinet",
            expiryDate = "2027-01-01",
        });
        Assert.Equal(HttpStatusCode.Created, add.StatusCode);
        var line = await add.Content.ReadFromJsonAsync<ChemicalInventorySummary>();
        Assert.Equal(locationId, line!.LocationId);
        Assert.Equal(productId, line.ChemicalProductId);

        var list = await _client.GetAsync($"/api/v1/chemical/inventory?productId={productId}");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var items = await list.Content.ReadFromJsonAsync<List<ChemicalInventorySummary>>();
        Assert.Contains(items!, i => i.Id == line.Id);
    }

    [Fact]
    public async Task Record_and_list_sds_revisions_round_trips()
    {
        var (_, productId) = await LoginAndCreateProductAsync();

        var record = await _client.PostAsJsonAsync($"/api/v1/chemical/products/{productId}/sds", new
        {
            revisionNumber = "Rev.3",
            effectiveDate = "2026-08-01",
            fileObjectId = Guid.NewGuid(),
            language = "en",
        });
        Assert.Equal(HttpStatusCode.Created, record.StatusCode);
        var sds = await record.Content.ReadFromJsonAsync<SdsRevisionSummary>();
        Assert.Equal("Rev.3", sds!.RevisionNumber);

        var list = await _client.GetAsync($"/api/v1/chemical/products/{productId}/sds");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var items = await list.Content.ReadFromJsonAsync<List<SdsRevisionSummary>>();
        Assert.Contains(items!, s => s.Id == sds.Id && s.Status == "Active");
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);
    public sealed record CreateChemicalProductPayload(Guid Id, string RecordNumber, string Status);
    public sealed record ChemicalInventorySummary(Guid Id, Guid ChemicalProductId, Guid LocationId, decimal? Quantity, string? Unit, string? StorageCondition, DateOnly? ExpiryDate);
    public sealed record SdsRevisionSummary(Guid Id, Guid ChemicalProductId, string RevisionNumber, DateOnly? EffectiveDate, string? Language, string Status);
}