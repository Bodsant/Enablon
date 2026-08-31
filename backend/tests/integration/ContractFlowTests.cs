using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ehsms.Api.IntegrationTests;

public sealed class ContractFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ContractFlowTests(WebApplicationFactory<Program> factory)
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
    public async Task Contractor_company_contract_worker_document_full_flow()
    {
        await LoginAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // 1. Register a contractor company (record-backed).
        var company = await _client.PostAsJsonAsync("/api/v1/contractor/companies", new
        {
            name = $"PT Maju Bersama {suffix}",
            vendorCode = $"VC-{suffix}",
            taxIdentifier = "1234567890",
            qualificationStatus = "Qualified",
            eligibilityStatus = "Eligible",
            status = "Active",
        });
        company.EnsureSuccessStatusCode();
        var companyDto = await company.Content.ReadFromJsonAsync<JsonElement>();
        var companyId = GuidFrom(companyDto.GetProperty("id"));
        Assert.False(string.IsNullOrWhiteSpace(companyDto.GetProperty("recordNumber").GetString()));

        // 2. Create a contract against the company.
        var contract = await _client.PostAsJsonAsync("/api/v1/contractor/contracts", new
        {
            contractorCompanyId = companyId,
            contractNumber = $"CNTR-{suffix}",
            startDate = "2026-01-01",
            endDate = "2026-12-31",
            contractStatus = "Active",
            procurementSourceId = "SRC-VENDOR",
        });
        contract.EnsureSuccessStatusCode();
        var contractDto = await contract.Content.ReadFromJsonAsync<JsonElement>();
        var contractId = GuidFrom(contractDto.GetProperty("id"));

        // 3. Register a contractor worker (PersonId is a cross-schema Guid; orphan FK dropped).
        var worker = await _client.PostAsJsonAsync("/api/v1/contractor/workers", new
        {
            contractorCompanyId = companyId,
            personId = Guid.NewGuid(),
            workerNumber = $"WKR-{suffix}",
            positionName = "Welder",
            eligibilityStatus = "Eligible",
            status = "Active",
        });
        worker.EnsureSuccessStatusCode();
        var workerDto = await worker.Content.ReadFromJsonAsync<JsonElement>();
        var workerId = GuidFrom(workerDto.GetProperty("id"));

        // 4. Attach a document to the company (FileObjectId is a cross-schema Guid; orphan FK dropped).
        var doc = await _client.PostAsJsonAsync("/api/v1/contractor/documents", new
        {
            contractorCompanyId = companyId,
            contractorWorkerId = (Guid?)null,
            documentType = "Insurance",
            documentNumber = $"POL-{suffix}",
            fileObjectId = Guid.NewGuid(),
            issueDate = "2026-01-15",
            expiryDate = "2026-12-31",
            verificationStatus = "Verified",
        });
        doc.EnsureSuccessStatusCode();
        var docDto = await doc.Content.ReadFromJsonAsync<JsonElement>();
        var docId = GuidFrom(docDto.GetProperty("id"));

        // 5. Verify round-trips.
        var companies = await (await _client.GetAsync("/api/v1/contractor/companies")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(companies.EnumerateArray(), c => c.GetProperty("id").GetString() == companyId.ToString());

        var contracts = await (await _client.GetAsync("/api/v1/contractor/contracts")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(contracts.EnumerateArray(), c => c.GetProperty("id").GetString() == contractId.ToString());

        var workers = await (await _client.GetAsync("/api/v1/contractor/workers")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(workers.EnumerateArray(), w => w.GetProperty("id").GetString() == workerId.ToString());

        var documents = await (await _client.GetAsync("/api/v1/contractor/documents")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(documents.EnumerateArray(), d => d.GetProperty("id").GetString() == docId.ToString());
    }
}
