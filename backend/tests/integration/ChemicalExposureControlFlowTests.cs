using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class ChemicalExposureControlFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ChemicalExposureControlFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<Guid> LoginAndCreateProductAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        var payload = await login.Content.ReadFromJsonAsync<LoginPayload>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", payload!.AccessToken);

        var create = await _client.PostAsJsonAsync("/api/v1/chemical/products", new
        {
            productName = "Acetone",
            productCode = "CHEM-ACE01",
            supplierName = "Solvent Supply",
        });
        var created = await create.Content.ReadFromJsonAsync<CreateChemicalProductPayload>();
        return created!.Id;
    }

    [Fact]
    public async Task Add_and_list_exposure_controls_round_trips()
    {
        var productId = await LoginAndCreateProductAsync();

        var add = await _client.PostAsJsonAsync(
            $"/api/v1/chemical/products/{productId}/exposure-controls", new
            {
                controlType = "Ventilation",
                description = "Local exhaust ventilation hood mandatory",
                sourceRecordId = (Guid?)null,
            });
        Assert.Equal(HttpStatusCode.Created, add.StatusCode);
        var control = await add.Content.ReadFromJsonAsync<ExposureControlSummary>();
        Assert.Equal("Ventilation", control!.ControlType);
        Assert.Equal(productId, control.ChemicalProductId);

        var list = await _client.GetAsync(
            $"/api/v1/chemical/products/{productId}/exposure-controls");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var items = await list.Content.ReadFromJsonAsync<List<ExposureControlSummary>>();
        Assert.Contains(items!, c => c.Id == control.Id);
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);
    public sealed record CreateChemicalProductPayload(Guid Id, string RecordNumber, string Status);
    public sealed record ExposureControlSummary(Guid Id, Guid ChemicalProductId, string ControlType, string Description, Guid? SourceRecordId);
}