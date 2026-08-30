using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class ChemicalCatalogFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ChemicalCatalogFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_and_list_chemical_product_round_trips()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var payload = await login.Content.ReadFromJsonAsync<LoginPayload>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", payload!.AccessToken);

        var create = await _client.PostAsJsonAsync("/api/v1/chemical/products", new
        {
            productName = "Sodium Hypochlorite 12%",
            productCode = "CHEM-001",
            supplierName = "Acme Chemical Supply",
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<CreateChemicalProductPayload>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.NotEmpty(created.RecordNumber);

        var list = await _client.GetAsync("/api/v1/chemical/products");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var items = await list.Content.ReadFromJsonAsync<List<ChemicalProductSummaryPayload>>();
        Assert.NotNull(items);
        Assert.Contains(items!, i => i.Id == created.Id && i.ProductName == "Sodium Hypochlorite 12%");
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);
    public sealed record CreateChemicalProductPayload(Guid Id, string RecordNumber, string Status);
    public sealed record ChemicalProductSummaryPayload(Guid Id, string RecordNumber, string ProductName, string? ProductCode, string? SupplierName, string Status);
}