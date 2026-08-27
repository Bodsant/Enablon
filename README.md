# ENABLON EHSMS — architecture scaffold

Architecture-ready, **non-business** foundation aligned to TDD v1.0: Angular, ASP.NET Core 8 modular monolith project boundaries, worker host, and an optional local PostgreSQL compose fixture. Implemented now: build/test scaffolds, dependency guards, process-only API metadata/probes, safe configuration, CI and engineering documentation. PostgreSQL is not wired to the API. Not implemented: module composition, domain workflows/endpoints, the 175-table model, real authentication/authorization, RLS, object storage/outbox processing, production deployment.

## Quickstart (Windows Git Bash compatible)
```bash
cp .env.example .env        # set a local POSTGRES_PASSWORD; never commit it
docker compose -f infra/local/compose.yml --env-file .env up -d
dotnet restore backend/Ehsms.sln
dotnet build backend/Ehsms.sln --no-restore
dotnet test backend/Ehsms.sln --no-build
npm --prefix frontend ci
npm --prefix frontend test
npm --prefix frontend run build
```
Run API: `dotnet run --project backend/src/Api`; frontend: `npm --prefix frontend start`.

**Security action:** a plaintext secret existed in prior Git history. Its owner must rotate/revoke it and follow repository history-remediation policy; this scaffold neither reads nor republishes it.
