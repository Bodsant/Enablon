using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class WorkflowFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public WorkflowFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Full_incident_approval_flow_start_submit_approve()
    {
        using var client = _factory.CreateClient();

        // Login to get a tenant-scoped token.
        var login = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        var loginPayload = await login.Content.ReadFromJsonAsync<LoginPayload>();
        client.DefaultRequestHeaders.Authorization = new("Bearer", loginPayload!.AccessToken);

        // Create a record to run the workflow against.
        var create = await client.PostAsJsonAsync("/api/v1/platform/records", new
        {
            moduleCode = "HSE",
            recordType = "Incident",
            title = "Workflow integration record",
            dataClassificationId = "00000000-0000-0000-0000-000000000001",
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var record = await create.Content.ReadFromJsonAsync<RecordPayload>();

        // Start the incident-approval workflow.
        var start = await client.PostAsJsonAsync("/api/v1/workflow/start",
            new { recordId = record!.Id, workflowCode = "incident-approval" });
        Assert.Equal(HttpStatusCode.OK, start.StatusCode);
        var started = await start.Content.ReadFromJsonAsync<StartPayload>();
        Assert.Equal("draft", started!.StateCode);
        Assert.NotNull(started.FirstTaskId);

        // First decision: submit (initial draft -> submitted).
        var taskId = started.FirstTaskId!.Value;
        var submit = await client.PostAsJsonAsync($"/api/v1/workflow/tasks/{taskId}/decision",
            new { decision = "submit", comment = "Ready for review" });
        Assert.Equal(HttpStatusCode.OK, submit.StatusCode);
        var submitted = await submit.Content.ReadFromJsonAsync<TransitionPayload>();
        Assert.Equal("submitted", submitted!.ToStateCode);
        Assert.False(submitted.IsCompleted);
        Assert.NotNull(submitted.NextTaskId);

        // Second decision: approve (submitted -> approved, terminal).
        var approve = await client.PostAsJsonAsync($"/api/v1/workflow/tasks/{submitted.NextTaskId}/decision",
            new { decision = "approve", comment = "Approved" });
        Assert.Equal(HttpStatusCode.OK, approve.StatusCode);
        var approved = await approve.Content.ReadFromJsonAsync<TransitionPayload>();
        Assert.Equal("approved", approved!.ToStateCode);
        Assert.True(approved.IsCompleted);
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);
    public sealed record RecordPayload(Guid Id, string RecordNumber, string Status);
    public sealed record StartPayload(Guid InstanceId, string StateCode, Guid? FirstTaskId);
    public sealed record TransitionPayload(Guid InstanceId, string FromStateCode, string ToStateCode, Guid? NextTaskId, bool IsCompleted);
}