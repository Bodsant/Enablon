using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Ehsms.Api.IntegrationTests;

public sealed class OccupationalHealthFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public OccupationalHealthFlowTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
        });
        _output = output;
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

    private async Task<JsonElement> PostPrint(string label, string path, object body)
    {
        var resp = await _client.PostAsJsonAsync(path, body);
        var text = await resp.Content.ReadAsStringAsync();
        _output.WriteLine($"[{label}] {(int)resp.StatusCode} -> {text[..Math.Min(800, text.Length)]}");
        resp.EnsureSuccessStatusCode();
        return JsonDocument.Parse(text).RootElement;
    }

    [Fact]
    public async Task Health_profile_fitness_surveillance_followup_flow()
    {
        await LoginAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var personId = Guid.NewGuid();

        var profileDto = await PostPrint("profile", "/api/v1/health/profiles", new
        {
            personId,
            restrictedIdentifier = $"PH-{suffix}",
        });
        var profileId = GuidFrom(profileDto.GetProperty("id"));

        var fitnessDto = await PostPrint("fitness", "/api/v1/health/fitness-statuses", new
        {
            healthProfileId = profileId,
            fitnessStatus = "Fit",
            validFrom = "2026-09-01",
            validUntil = "2027-09-01",
            restrictionsSummary = "None",
            issuedByMemberId = (string?)null,
        });
        var fitnessId = GuidFrom(fitnessDto.GetProperty("id"));

        var programDto = await PostPrint("program", "/api/v1/health/surveillance-programs", new
        {
            code = $"HS-{suffix}",
            name = $"Hearing Surveillance {suffix}",
            exposureType = "Noise",
            frequencyMonths = 12,
            status = "Active",
        });
        var programId = GuidFrom(programDto.GetProperty("id"));

        var evtDto = await PostPrint("event", "/api/v1/health/surveillance-events", new
        {
            healthProfileId = profileId,
            surveillanceProgramId = programId,
            scheduledDate = "2026-09-15",
            authorizedProvider = "Occupational Clinic",
        });
        var eventId = GuidFrom(evtDto.GetProperty("id"));

        var followupDto = await PostPrint("followup", "/api/v1/health/followups", new
        {
            surveillanceEventId = eventId,
            followupType = "Specialist Referral",
            dueDate = "2026-10-01",
            status = "Open",
            assignedMemberId = (string?)null,
        });
        var followupId = GuidFrom(followupDto.GetProperty("id"));

        var profiles = await (await _client.GetAsync("/api/v1/health/profiles")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(profiles.EnumerateArray(), p => p.GetProperty("id").GetString() == profileId.ToString());

        var fitnesses = await (await _client.GetAsync($"/api/v1/health/fitness-statuses?healthProfileId={profileId}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(fitnesses.EnumerateArray(), f => f.GetProperty("id").GetString() == fitnessId.ToString());

        var programs = await (await _client.GetAsync("/api/v1/health/surveillance-programs")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(programs.EnumerateArray(), p => p.GetProperty("id").GetString() == programId.ToString());

        var events = await (await _client.GetAsync("/api/v1/health/surveillance-events")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(events.EnumerateArray(), e => e.GetProperty("id").GetString() == eventId.ToString());

        var followups = await (await _client.GetAsync($"/api/v1/health/followups?surveillanceEventId={eventId}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(followups.EnumerateArray(), f => f.GetProperty("id").GetString() == followupId.ToString());
    }
}