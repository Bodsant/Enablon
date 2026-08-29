# EHSMS — Catatan Sprint (SPRINT-NOTES)

Panduan ringkas untuk me-review kerjaan yang sudah dikerjakan di backend.
Semua perubahan ada di branch `development`, sudah di-push ke `origin/development`.

> Cara buka: buka folder ini di VS Code, lalu cari nama file di bawah via `Ctrl+P`.

---

## Status P0 — Release 0 (Foundation)

| Sprint | Judul | Status | Commit |
|--------|-------|--------|--------|
| 1 | Project Bootstrap | ✅ 6/6 | (awal) |
| 2 | Database Foundation (DDL 175 tabel / 24 schema) | ✅ 5/5 | (awal) |
| 3 | SaaS & Organization | ✅ 6/6 | `0697707` |
| 4 | IAM Core (Authentication) | ✅ 6/6 | `e5eccd3` |
| 5 | Architecture Review | ⏳ 2/5 | — |
| **6** | **Platform Records & Audit** | ✅ 6/6 | `1df0977` |
| 7 | Platform Workflow Engine | ⏳ 2/6 | — |
| 8 | My Tasks & Notifications | ⏳ 1/5 | — |
| 9 | Evidence & File Lifecycle | ⏳ 1/5 | — |
| 10 | Angular Shell & Admin | ⏳ 3/7 | — |

---

## Sprint 3 — SaaS & Organization  (`0697707`)
- **`backend/src/Modules/Saas/Infrastructure/SaasDbSeeder.cs`** — seeder idempotent (upsert by `Code`), 3 plan: ENTERPRISE / PROFESSIONAL / STARTER + plan_versions. Pakai *two-phase SaveChanges* (plans dulu, lalu versions) agar tidak melanggar FK.
- **`backend/src/Modules/Saas/Infrastructure/SaasPersistenceServiceCollectionExtensions.cs`** — registrasi `SaasDbContext` + seeder.
- **`backend/src/Modules/Platform/Infrastructure/PlatformPersistenceServiceCollectionExtensions.cs`** — registrasi `PlatformDbContext` + schema `platform`.
- Endpoint: `GET /api/v1/saas/plans` → 3 plan.

## Sprint 4 — IAM Core: Authentication  (`e5eccd3`)
- **`backend/src/Modules/Identity/Infrastructure/Authentication/Pbkdf2PasswordHasher.cs`** — hashing PBKDF2 (RFC2898) via `Rfc2898DeriveBytes`, format self-describing `PBKDF2$<iter>$<salt>$<hash>`. Tanpa dependency ASP.NET Identity.
- **`backend/src/Api/Authentication/AuthOptions.cs`** — options JWT dari section `Authentication` di appsettings.
- **`backend/src/Api/Authentication/JwtTokenService.cs`** — generate access token (claim `sub`, `email`, `tenant`) + refresh token.
- **`backend/src/Api/Authentication/TenantResolutionMiddleware.cs`** — baca claim `tenant` dari JWT → set `ScopedTenantContext` (fail-closed).
- **`backend/src/Modules/Identity/Infrastructure/IdentityDbSeeder.cs`** — dev user `admin@ehsms.local`.
- **`backend/src/Api/Program.cs`** — wiring JwtBearer + `POST /api/v1/auth/login` + `GET /api/v1/auth/me` (protected).

## Sprint 6 — Platform Records & Audit  (`1df0977`)
- **`backend/src/Modules/Platform/Application/IRecordAppService.cs`** — kontrak create record.
- **`backend/src/Modules/Platform/Infrastructure/RecordAppService.cs`** — inti: alokasi number sequence per-tenant+periode (increment), tulis record + **audit log** + **outbox event** dalam **satu transaksi**. Fail-closed bila tenant tidak ter-resolve.
- **`backend/src/Modules/Platform/Infrastructure/AuditLogWriter.cs`** — helper tulis `platform.audit_logs`.
- **`backend/src/Modules/Platform/Infrastructure/OutboxDispatcherWorker.cs`** — `BackgroundService` (interval 15 dtk) yang mem-*publish* outbox `Pending`, retry dgn backoff. Idempoten.
- **`backend/src/Modules/Platform/Infrastructure/PlatformDbSeeder.cs`** — seed default data classifications (internal / confidential / restricted) per tenant.
- Endpoint: `POST /api/v1/platform/records` (RequireAuthorization) → `201`.

### Perbaikan penting (bug di Sprint 4/6)
1. **Urutan middleware** — `UseAuthentication()` harus SEBELUM tenant middleware, kalau tidak claim `tenant` tidak terbaca → `tenantId` selalu null.
2. **FK `records.created_by_member_id`** — harus di-set dari tenant member aktif (resolve dari JWT `sub`), bukan `Guid.Empty`.
3. **Data classification kosong** — perlu seeder default, kalau tidak create record kena FK violation.

---

## Test  (semua hijau, 29 total)
| Proyek | Jumlah |
|--------|--------|
| UnitTests (termasuk 5 test hasher) | 7 |
| ArchitectureTests | 7 |
| DatabaseTests | 1 |
| IntegrationTests (termasuk AuthFlow + PlatformRecordFlow) | 14 |

File test baru:
- **`backend/tests/unit/Pbkdf2PasswordHasherTests.cs`**
- **`backend/tests/integration/AuthFlowTests.cs`**
- **`backend/tests/integration/PlatformRecordFlowTests.cs`**

Cara jalankan:
```bash
cd "C:/Users/budii/source/repos/Enablon/Enablon/backend"
export PATH="/c/Users/budii/.dotnet:$PATH"
dotnet build Ehsms.sln
dotnet test Ehsms.sln
```

---

## Endpoint yang hidup (verifikasi runtime)
| Endpoint | Keterangan |
|----------|-----------|
| `POST /api/v1/auth/login` | login → access token (+ refresh) |
| `GET  /api/v1/auth/me` | identitas + tenantId (protected) |
| `POST /api/v1/platform/records` | create record → `HSE-<YYYYMM>-<seq>` (protected) |
| `GET  /api/v1/platform/records/count` | jumlah record |
| `GET  /api/v1/saas/plans` | daftar plan |
| `GET  /api/v1/architecture/info` | modul yang ter-wire |
| `/health/ready` | health check |

Coba manual:
```bash
cd "C:/Users/budii/source/repos/Enablon/Enablon/backend/src/Api"
export PATH="/c/Users/budii/.dotnet:$PATH"
ASPNETCORE_ENVIRONMENT=Development dotnet run --urls "http://localhost:5199"
# buka http://localhost:5199/swagger
```

---
*Dibuat otomatis oleh asisten — update saat sprint baru diselesaikan.*
