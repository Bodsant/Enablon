# EF Core Scaffold – ENABLON Architecture

## Ringkasan Modul
| Modul | DbContext | Lokasi | Entity (contoh) |
|------|----------|--------|-----------------|
| **Platform** | `PlatformDbContext` | `backend/src/Modules/Platform/Infrastructure/Persistence/PlatformDbContext.cs` | `RecordEntity`, `WorkflowDefinitionEntity`, … |
| **Saas** | `SaasDbContext` | `backend/src/Modules/Saas/Infrastructure/Persistence/SaasDbContext.cs` | `Tenant`, `SubscriptionPlan`, `TenantSubscription`, … |
| **Organisation** | `OrganisationDbContext` | `backend/src/Modules/Organisation/Infrastructure/Persistence/OrganisationDbContext.cs` | `Company`, `BusinessUnit`, `Site`, `Department`, … |
| **Identity** | `EhsmsIdentityDbContext` (plain `DbContext`) | `backend/src/Modules/Identity/Infrastructure/Persistence/EhsmsIdentityDbContext.cs` | `UserEntity`, `RoleEntity`, `TenantMemberEntity`, … |

## 1. Build & Test
```bash
# Build seluruh solution
cd backend
dotnet build Ehsms.sln --nologo -v q

# Jalankan unit‑test termasuk schema verification
dotnet test Ehsms.sln --no-build
```
Semua modul harus **lulus** (0 error, 0 warning).  Jika ada error, pastikan `Directory.Packages.props` berisi:
```xml
<PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.11" />
```
dan tidak ada versi hard‑coded di *.csproj*.

## 2. Membuat Migration EF Core
Setiap modul memiliki `DbContext`‑nya masing‑mahasiswa.  Contoh untuk **Platform**:
```bash
# Dari folder root backend
# --project = lokasi csproj DbContext
# --startup-project = API (atau project yang memuat konfigurasi connection string)

dotnet ef migrations add InitPlatform \
    --project src/Modules/Platform/Infrastructure/Ehsms.Modules.Platform.Infrastructure.csproj \
    --startup-project src/Api/Ehsms.Api.csproj

# Terapkan ke Neon (atau DB lokal)

dotnet ef database update \
    --project src/Modules/Platform/Infrastructure/Ehsms.Modules.Platform.Infrastructure.csproj \
    --startup-project src/Api/Ehsms.Api.csproj
```
Ulangi langkah di atas untuk **Saas**, **Organisation**, dan **Identity** (ganti nama migrasi, mis. `InitSaas`, `InitOrganisation`, `InitIdentity`).

> **Catatan**: Karena `Identity` tidak memakai `IdentityDbContext`, tidak ada tabel ASP.NET Identity otomatis.  Semua tabel IAM dipetakan secara manual lewat *EntityConfigurations*.

## 3. Seed Data Development
Tambahkan kelas `DatabaseSeeder.cs` (mis. di `backend/src/Infrastructure/Seed/DatabaseSeeder.cs`) dan registrasikan sebagai **Hosted Service** di `Program.cs`:
```csharp
builder.Services.AddHostedService<DatabaseSeeder>();
```
Contoh isi `DatabaseSeeder` (sederhana):
```csharp
public sealed class DatabaseSeeder : IHostedService
{
    private readonly IServiceProvider _sp;
    public DatabaseSeeder(IServiceProvider sp) => _sp = sp;
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var platform = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var saas = scope.ServiceProvider.GetRequiredService<SaasDbContext>();
        var org = scope.ServiceProvider.GetRequiredService<OrganisationDbContext>();
        var id = scope.ServiceProvider.GetRequiredService<EhsmsIdentityDbContext>();
        // contoh tenant A & B
        if (!await saas.Tenants.AnyAsync(ct))
        {
            var tA = new Tenant { Id = Guid.NewGuid(), TenantCode = "A001", Slug = "pt-maju", DisplayName = "PT Maju Jaya", Timezone = "Asia/Jakarta", BillingAnchorDay = 1, Status = "Active", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
            var tB = new Tenant { Id = Guid.NewGuid(), TenantCode = "B001", Slug = "pt-sejahtera", DisplayName = "PT Sejahtera", Timezone = "Asia/Jakarta", BillingAnchorDay = 1, Status = "Active", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
            await saas.Tenants.AddRangeAsync(new[] { tA, tB }, ct);
            await saas.SaveChangesAsync(ct);
        }
        // admin user & role (Identity)
        if (!await id.Users.AnyAsync(ct))
        {
            var admin = new UserEntity { Id = Guid.NewGuid(), Email = "admin@example.com", NormalizedEmail = "ADMIN@EXAMPLE.COM", Status = "Active", LastLoginAt = null };
            await id.Users.AddAsync(admin, ct);
            await id.SaveChangesAsync(ct);
        }
    }
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
```
Sesuaikan seed sesuai kebutuhan tim.

## 4. Apa yang Harus Dikerjakan Selanjutnya (Team‑Engineer Checklist)
- **Generate & Apply Migrations** untuk tiap modul (lihat bagian 2).  Commit migration files ke `src/Modules/*/Infrastructure/Migrations/`.
- **Implementasikan API CRUD** di `src/Api/Controllers/…` menggunakan *Application* layer yang sudah ada (semua Services / Use‑Cases dapat dipanggil).
- **Integrasi Authentication / OIDC** jika aplikasi akan memakai login eksternal – extend `UserEntity`/`RoleEntity` atau gunakan ASP.NET Core Identity dengan custom store.
- **Tambahkan Unit‑/Integration Tests** untuk repository & service layer (pastikan coverage >80%).
- **Perbarui CI pipeline** (`.github/workflows/ci.yml`) – langkah build, test, dan `dotnet ef migrations script` untuk menghasilkan artefak SQL migrasi yang dapat diterapkan pada environment Neon.
- **Documentation** – perbarui README root proyek dengan link ke dokumen ini serta contoh connection‑string ke Neon (`NEON_DATABASE_URL`).

## 5. Referensi Tambahan
- **EF Core Fluent API** – lihat file `EntityConfigurations/*.cs` untuk contoh pola penamaan, indeks `tenant_id`, dan constraint `IsRequired`.
- **RLS Policy** – tidak berubah; sudah diterapkan di tahap database seeding.  Pastikan role aplikasi (`ehsms_app`) tidak memiliki hak owner sehingga RLS aktif.
- **Central Package Version** – semua modul memakai versi paket yang didefinisikan di `Directory.Packages.props`.  Jika menambahkan paket baru, cukup tambahkan entry `<PackageVersion>` di sana.

---
*Dokumen ini dibuat otomatis oleh NiraLifehouse Coordinator sebagai hand‑off untuk tim engineer.  Jika ada tambahan atau perubahan, cukup edit file ini dan commit ke repository.*