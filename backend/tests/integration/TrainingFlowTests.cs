using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class TrainingFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TrainingFlowTests(WebApplicationFactory<Program> factory)
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
        var payload = await resp.Content.ReadFromJsonAsync<LoginPayload>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", payload!.AccessToken);
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);

    private static Guid GuidFrom(object value) => Guid.Parse(value!.ToString()!);

    [Fact]
    public async Task Course_session_participant_competency_flow()
    {
        await LoginAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // 1. Create a course.
        var course = await _client.PostAsJsonAsync("/api/v1/courses", new
        {
            code = $"CS-{suffix}",
            name = $"Confined Space Entry {suffix}",
            validityMonths = 24,
            providerType = "Internal",
            status = "Active",
        });
        course.EnsureSuccessStatusCode();
        var courseDto = await course.Content.ReadFromJsonAsync<JsonElement>();
        var courseId = GuidFrom(courseDto.GetProperty("id"));

        // 2. Schedule a training session (record-backed).
        var session = await _client.PostAsJsonAsync("/api/v1/training-sessions", new
        {
            courseId,
            providerName = "Safety Academy",
            startsAt = "2026-09-01T09:00:00Z",
            endsAt = "2026-09-01T13:00:00Z",
            capacity = 12,
            status = "Scheduled",
        });
        session.EnsureSuccessStatusCode();
        var sessionDto = await session.Content.ReadFromJsonAsync<JsonElement>();
        var sessionId = GuidFrom(sessionDto.GetProperty("id"));
        Assert.False(string.IsNullOrWhiteSpace(sessionDto.GetProperty("recordNumber").GetString()));

        // 3. Enrol a participant (PersonId is a cross-schema Guid; orphan FK dropped).
        var personId = Guid.NewGuid();
        var participant = await _client.PostAsJsonAsync("/api/v1/training-sessions/participants", new
        {
            trainingSessionId = sessionId,
            personId,
            attendanceStatus = "Attended",
            assessmentScore = 92.5m,
            result = "Pass",
        });
        participant.EnsureSuccessStatusCode();
        var participantDto = await participant.Content.ReadFromJsonAsync<JsonElement>();
        var participantId = GuidFrom(participantDto.GetProperty("id"));

        // 4. Define a competency and assign it to the person.
        var competency = await _client.PostAsJsonAsync("/api/v1/competencies", new
        {
            code = $"CMP-{suffix}",
            name = "Confined Space Supervision",
            description = "Supervise confined space entry teams",
            status = "Active",
        });
        competency.EnsureSuccessStatusCode();
        var competencyDto = await competency.Content.ReadFromJsonAsync<JsonElement>();
        var competencyId = GuidFrom(competencyDto.GetProperty("id"));

        var workerComp = await _client.PostAsJsonAsync("/api/v1/worker-competencies", new
        {
            personId,
            competencyId,
            level = "Advanced",
            status = "Active",
            validFrom = "2026-09-01",
            validUntil = "2028-09-01",
        });
        workerComp.EnsureSuccessStatusCode();
        var workerCompDto = await workerComp.Content.ReadFromJsonAsync<JsonElement>();
        var workerCompId = GuidFrom(workerCompDto.GetProperty("id"));

        // 5. Verify round-trips.
        var courses = await (await _client.GetAsync("/api/v1/courses")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(courses.EnumerateArray(), c => c.GetProperty("id").GetString() == courseId.ToString());

        var sessions = await (await _client.GetAsync("/api/v1/training-sessions")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(sessions.EnumerateArray(), s => s.GetProperty("id").GetString() == sessionId.ToString());

        var participants = await (await _client.GetAsync($"/api/v1/training-sessions/{sessionId}/participants")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(participants.EnumerateArray(), p => p.GetProperty("id").GetString() == participantId.ToString());

        var competencies = await (await _client.GetAsync("/api/v1/competencies")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(competencies.EnumerateArray(), c => c.GetProperty("id").GetString() == competencyId.ToString());

        var workerComps = await (await _client.GetAsync($"/api/v1/worker-competencies?personId={personId}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(workerComps.EnumerateArray(), w => w.GetProperty("id").GetString() == workerCompId.ToString());
    }
}
