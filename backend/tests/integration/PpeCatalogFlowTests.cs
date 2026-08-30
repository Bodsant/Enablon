using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class PpeCatalogFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PpeCatalogFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task LoginAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        var payload = await login.Content.ReadFromJsonAsync<LoginPayload>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", payload!.AccessToken);
    }

    [Fact]
    public async Task Create_and_list_ppe_catalog_round_trips()
    {
        await LoginAsync();

        var create = await _client.PostAsJsonAsync("/api/v1/ppe/catalog", new
        {
            code = "HF-01",
            name = "Safety Helmet",
            ppeCategory = "Head",
            inspectionIntervalDays = 365,
            replacementIntervalDays = 730,
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var item = await create.Content.ReadFromJsonAsync<PpeCatalogSummary>();
        Assert.Equal("HF-01", item!.Code);
        Assert.Equal("Active", item.Status);

        var list = await _client.GetAsync("/api/v1/ppe/catalog");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var items = await list.Content.ReadFromJsonAsync<List<PpeCatalogSummary>>();
        Assert.Contains(items!, c => c.Id == item.Id);
    }

    [Fact]
    public async Task Create_and_list_ppe_requirement_round_trips()
    {
        await LoginAsync();

        var createCat = await _client.PostAsJsonAsync("/api/v1/ppe/catalog", new
        {
            code = "GL-01",
            name = "Chemical Gloves",
            ppeCategory = "Hands",
        });
        var cat = await createCat.Content.ReadFromJsonAsync<PpeCatalogSummary>();

        var createReq = await _client.PostAsJsonAsync("/api/v1/ppe/requirements", new
        {
            ppeCatalogId = cat!.Id,
            isMandatory = true,
            notes = "Required for chemical handling",
        });
        Assert.Equal(HttpStatusCode.Created, createReq.StatusCode);
        var req = await createReq.Content.ReadFromJsonAsync<PpeRequirementSummary>();
        Assert.True(req!.IsMandatory);
        Assert.Equal(cat.Id, req.PpeCatalogId);

        var list = await _client.GetAsync($"/api/v1/ppe/requirements?ppeCatalogId={cat.Id}");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var items = await list.Content.ReadFromJsonAsync<List<PpeRequirementSummary>>();
        Assert.Contains(items!, r => r.Id == req.Id);
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);
    public sealed record PpeCatalogSummary(Guid Id, string Code, string Name, string? PpeCategory, int? InspectionIntervalDays, int? ReplacementIntervalDays, string Status);
    public sealed record PpeRequirementSummary(Guid Id, Guid PpeCatalogId, bool IsMandatory, Guid? SourceRecordId, Guid? PermitTypeId, string? Notes);
}