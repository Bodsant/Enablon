# Backend

ASP.NET Core 8 modular monolith aligned to the `database/ddl` schema set. API plus the
**Organisation** (`org`) and **Identity** (`iam`) modules are wired end-to-end against
PostgreSQL: each module owns an EF Core `DbContext`, entity mapping is kept aligned with
`001-foundation.sql` (snake_case columns via `EFCore.NamingConventions`), and tenant
isolation is enforced at query time through a fail-closed filter (`TenantIsolation`) with
PostgreSQL RLS as the complementary database layer.

## What is implemented
- `OrgDbContext` / `IdentityDbContext`: 19 entity mappings across the `org` and `iam` schemas,
  aligned to `001-foundation.sql` (cross-schema FKs kept as scalar `Guid` properties so module
  boundaries stay intact).
- Tenant isolation: `ITenantContext` (BuildingBlocks) + `TenantIsolation.ForTenant()` (fail-closed).
  A global model filter is intentionally not used (see `TenantIsolation` for the EF Core why).
- DI wiring: `AddOrganisationPersistence` / `AddIdentityPersistence` (`UseSnakeCaseNamingConvention().UseNpgsql(...)`).
- Health checks: `/health/live` (process) and `/health/ready` (which now includes a real
  `postgres` check via `PostgresHealthCheck`).
- API metadata defaulting to `capability = modular-monolith` once persistence is wired.
- Integration tests (`PersistenceMappingTests`) that run against a live PostgreSQL, verifying
  the mappings and cross-tenant fail-closed behaviour. They skip when no database is reachable.

## Not yet implemented (future)
- SaaS, Platform and operational EHS modules and their schema mappings.
- Real authentication/authorization (identity is persisted but not secured).
- PostgreSQL RLS policies; `BEGIN ATOMIC`-style definitions pending.
- Outbox processor, worker, and object storage.

## Run
```bash
dotnet run --project backend/src/Api
```
Connection string comes from `ConnectionStrings:EhSms` in `backend/src/Api/appsettings.json`.
Set `EHSMS_TEST_DB` to point integration tests at a real database.
