using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class EnvironmentMonitoringFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EnvironmentMonitoringFlowTests(WebApplicationFactory<Program> factory)
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
    public async Task Create_and_list_parameters_round_trips()
    {
        await LoginAsync();
        var code = $"PM25-{Guid.NewGuid():N}"[..14];

        var create = await _client.PostAsJsonAsync("/api/v1/environment/parameters", new
        {
            code,
            name = "Particulate Matter 2.5",
            category = "Air Quality",
            defaultUnit = "ug/m3",
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var param = await create.Content.ReadFromJsonAsync<EnvironmentParameterPayload>();
        Assert.Equal(code, param!.Code);
        Assert.Equal("Active", param.Status);

        var list = await _client.GetAsync("/api/v1/environment/parameters?category=Air%20Quality");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var items = await list.Content.ReadFromJsonAsync<List<EnvironmentParameterPayload>>();
        Assert.Contains(items!, p => p.Id == param.Id);
    }

    [Fact]
    public async Task Create_and_list_sources_round_trips()
    {
        await LoginAsync();

        var create = await _client.PostAsJsonAsync("/api/v1/environment/sources", new
        {
            siteId = Guid.NewGuid(),
            sourceType = "Stack",
            name = "Boiler Stack A",
            permitReference = "PERM-2026-001",
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var source = await create.Content.ReadFromJsonAsync<EnvironmentSourcePayload>();
        Assert.Equal("Stack", source!.SourceType);

        var list = await _client.GetAsync("/api/v1/environment/sources");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var items = await list.Content.ReadFromJsonAsync<List<EnvironmentSourcePayload>>();
        Assert.Contains(items!, s => s.Id == source.Id);
    }

    [Fact]
    public async Task Record_and_list_measurement_computes_compliance()
    {
        await LoginAsync();

        var param = await _client.PostAsJsonAsync("/api/v1/environment/parameters", new
        {
            code = $"NO2-{Guid.NewGuid():N}"[..11],
            name = "Nitrogen Dioxide",
            category = "Air Quality",
            defaultUnit = "ppm",
        });
        var parameter = await param.Content.ReadFromJsonAsync<EnvironmentParameterPayload>();

        // Compliant measurement (result <= limit).
        var compliant = await _client.PostAsJsonAsync("/api/v1/environment/measurements", new
        {
            parameterId = parameter!.Id,
            resultValue = 12.5,
            limitValue = 25.0,
            qualityFlag = "Lab",
        });
        Assert.Equal(HttpStatusCode.Created, compliant.StatusCode);
        var ok = await compliant.Content.ReadFromJsonAsync<EnvironmentMeasurementPayload>();
        Assert.Equal("Compliant", ok!.ComplianceStatus);
        Assert.Equal("ppm", ok.Unit);

        // Exceeded measurement (result > limit).
        var exceeded = await _client.PostAsJsonAsync("/api/v1/environment/measurements", new
        {
            parameterId = parameter.Id,
            resultValue = 42.0,
            limitValue = 25.0,
        });
        var bad = await exceeded.Content.ReadFromJsonAsync<EnvironmentMeasurementPayload>();
        Assert.Equal("Exceeded", bad!.ComplianceStatus);

        var list = await _client.GetAsync($"/api/v1/environment/measurements?parameterId={parameter.Id}");
        var items = await list.Content.ReadFromJsonAsync<List<EnvironmentMeasurementPayload>>();
        Assert.Contains(items!, m => m.Id == ok.Id);
        Assert.Contains(items!, m => m.Id == bad.Id);
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);
    public sealed record EnvironmentParameterPayload(Guid Id, string Code, string Name, string Category, string? DefaultUnit, string Status);
    public sealed record EnvironmentSourcePayload(Guid Id, Guid SiteId, Guid? LocationId, string SourceType, string Name, string? PermitReference);
    public sealed record EnvironmentMeasurementPayload(Guid Id, Guid MonitoringRecordId, Guid ParameterId, DateTimeOffset MeasuredAt, decimal? ResultValue, string? Unit, decimal? LimitValue, decimal? TargetValue, string? QualityFlag, string? ComplianceStatus);
}