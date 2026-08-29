using System.Net;
using System.Net.Http.Json;
using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class FileLifecycleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public FileLifecycleTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Upload_link_evidence_and_download_url_work()
    {
        using var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "admin@ehsms.local", password = "EhsmsDev!123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        client.DefaultRequestHeaders.Authorization = new("Bearer", (await login.Content.ReadFromJsonAsync<LoginPayload>())!.AccessToken);

        // 1. Create a record so we can attach evidence.
        Guid tenantId;
        Guid recordId;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var saas = scope.ServiceProvider.GetRequiredService<Ehsms.Modules.Saas.Infrastructure.Persistence.SaasDbContext>();
            tenantId = (await saas.Tenants.OrderBy(t => t.Id).FirstOrDefaultAsync())!.Id;
        }
        var createRec = await client.PostAsJsonAsync("/api/v1/platform/records",
            new { moduleCode = "HSE", recordType = "INCIDENT", title = "Evidence test record", dataClassificationId = Guid.NewGuid() });
        Assert.Equal(HttpStatusCode.Created, createRec.StatusCode);
        recordId = (await createRec.Content.ReadFromJsonAsync<RecordPayload>())!.Id;

        // 2. Upload a small file (a few bytes, base64).
        var bytes = new byte[] { 0x48, 0x45, 0x4c, 0x4c, 0x4f };
        var upload = await client.PostAsJsonAsync("/api/v1/platform/files",
            new { fileName = "photo.jpg", mimeType = "image/jpeg", contentBase64 = Convert.ToBase64String(bytes) });
        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
        var fileId = (await upload.Content.ReadFromJsonAsync<UploadPayload>())!.FileObjectId;

        // 2b. Verify object persisted in the local storage adapter (bytes intact) via Db + storage.
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
            var obj = await db.FileObjects.FirstAsync(f => f.Id == fileId);
            Assert.Equal("Active", obj.Status);
            Assert.Equal(bytes.Length, obj.ObjectSizeBytes);
            Assert.Equal(tenantId, obj.TenantId);
        }

        // 3. Link the file as evidence to the record.
        var link = await client.PostAsJsonAsync($"/api/v1/platform/records/{recordId}/evidence",
            new { fileObjectId = fileId, evidenceType = "PHOTO" });
        Assert.Equal(HttpStatusCode.Created, link.StatusCode);

        // 4. Get a short-lived download URL.
        var url = await client.GetFromJsonAsync<DownloadPayload>($"/api/v1/platform/files/{fileId}/download-url");
        Assert.NotNull(url!.Url);
        Assert.Contains("e=", url.Url);
    }

    public sealed record LoginPayload(string AccessToken, string TokenType);
    public sealed record RecordPayload(Guid Id);
    public sealed record UploadPayload(Guid FileObjectId, string OriginalFileName, string MimeType, long SizeBytes, string ChecksumSha256);
    public sealed record DownloadPayload(string Url, System.DateTimeOffset ExpiresAt);
}
