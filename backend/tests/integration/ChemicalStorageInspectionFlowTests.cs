using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class ChemicalStorageInspectionFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ChemicalStorageInspectionFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid ProductId, Guid InventoryId)> LoginCreateProductAndInventoryAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        var payload = await login.Content.ReadFromJsonAsync<LoginPayload>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", payload!.AccessToken);

        var create = await _client.PostAsJsonAsync("/api/v1/chemical/products", new
        {
            productName = "Sulfuric Acid",
            productCode = "CHEM-H2SO4",
            supplierName = "Acid Plant Supply",
        });
        var created = await create.Content.ReadFromJsonAsync<CreateChemicalProductPayload>();

        var addInv = await _client.PostAsJsonAsync("/api/v1/chemical/inventory", new
        {
            chemicalProductId = created!.Id,
            locationId = Guid.NewGuid(),
            quantity = 10m,
            unit = "L",
            storageCondition = "Corrosive cabinet",
        });
        var inv = await addInv.Content.ReadFromJsonAsync<CreateInventoryPayload>();
        return (created.Id, inv!.Id);
    }

    [Fact]
    public async Task Create_and_list_storage_inspection_round_trips()
    {
        var (_, inventoryId) = await LoginCreateProductAndInventoryAsync();

        var create = await _client.PostAsJsonAsync("/api/v1/chemical/storage-inspections", new
        {
            chemicalInventoryId = inventoryId,
            result = "Pass - containers labeled, secondary containment intact",
            inspectedAt = "2026-08-30T08:00:00Z",
            nextReviewDate = "2026-09-30",
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var inspection = await create.Content.ReadFromJsonAsync<StorageInspectionSummary>();
        Assert.Equal(inventoryId, inspection!.ChemicalInventoryId);
        Assert.NotNull(inspection.RecordNumber);
        Assert.True(inspection.InspectedAt > DateTimeOffset.MinValue);

        var list = await _client.GetAsync($"/api/v1/chemical/storage-inspections?chemicalInventoryId={inventoryId}");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var items = await list.Content.ReadFromJsonAsync<List<StorageInspectionSummary>>();
        Assert.Contains(items!, i => i.Id == inspection.Id);
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);
    public sealed record CreateChemicalProductPayload(Guid Id, string RecordNumber, string Status);
    public sealed record CreateInventoryPayload(Guid Id);
    public sealed record StorageInspectionSummary(Guid Id, string RecordNumber, Guid ChemicalInventoryId, Guid InspectedByMemberId, DateTimeOffset InspectedAt, string Result, DateOnly? NextReviewDate);
}