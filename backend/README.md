# EHSMS Backend — Enterprise Environment, Health & Safety Management System

Backend ASP.NET Core untuk Project Enablon. Modular monolith dengan PostgreSQL, multi-tenant, dan bounded context.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| API | ASP.NET Core 8 Minimal API + Controllers |
| Worker | .NET 8 Background Service |
| Database | PostgreSQL 16 |
| ORM | EF Core 8 + Npgsql |
| Auth | JWT Bearer + OIDC/SSO (planned) |
| Messaging | Outbox pattern (in-process) |
| CI/CD | GitHub Actions |

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (untuk PostgreSQL)
- [EF Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

## Project Structure

```
backend/
├── src/
│   ├── Api/                              # HTTP endpoints, middleware, auth, OpenAPI
│   ├── Worker/                           # Outbox dispatcher, notification, escalation
│   ├── BuildingBlocks/                   # Result, TenantContext, Clock, Entity bases, IRepository
│   └── Modules/
│       ├── Platform/                     # Core platform: record registry, workflow, evidence, audit, outbox
│       │   ├── Domain/
│       │   ├── Application/
│       │   ├── Infrastructure/           # DbContext, EF mappings, repositories
│       │   └── Contracts/
│       ├── Identity/                     # IAM: users, roles, permissions, scopes, tenant members
│       ├── Organisation/                 # Org hierarchy: companies, sites, departments
│       └── Saas/                         # Tenants, subscriptions, quotas
├── tests/
├── database/
│   ├── migrations/                       # EF Core migrations
│   ├── seed/                             # Seed SQL (auto-run via docker-entrypoint-initdb.d)
│   ├── views/                            # Materialized views for reporting
│   └── runbooks/                         # DB operational runbooks
├── infra/                                # Docker, CI/CD, environment configs
├── docs/                                 # ADR, API docs, data docs
├── docker-compose.yml                    # Local PostgreSQL
└── Ehsms.sln
```

## Quick Start — Database Setup

### 1. Start PostgreSQL

```bash
cd backend
docker-compose up -d
```

Ini akan:
- Start PostgreSQL 16 di port `5432`
- Database: `ehsms`, User: `ehsms`, Password: `ehsms_dev_password`
- Auto-create 24 schema (saas, org, iam, platform, dll) dari seed file

### 2. Install EF Core CLI

```bash
dotnet tool install --global dotnet-ef
```

### 3. Create Initial Migration

```bash
cd backend
dotnet ef migrations add InitialCreate \
  --project src/Modules/Platform/Infrastructure \
  --startup-project src/Api \
  --output-dir Migrations
```

### 4. Apply Migration to Database

```bash
dotnet ef database update \
  --project src/Modules/Platform/Infrastructure \
  --startup-project src/Api
```

Atau gunakan auto-migrate saat development (sudah dikonfigurasi di Program.cs — otomatis jalan saat API start di environment Development).

### 5. Verify

```bash
# Cek koneksi
psql -h localhost -U ehsms -d ehsms -c "\dn"    # lihat semua schema
psql -h localhost -U ehsms -d ehsms -c "\dt saas.*"  # lihat tabel saas
```

## Run API

```bash
cd backend
dotnet run --project src/Api
```

API tersedia di:
- **Swagger UI**: http://localhost:5000/swagger
- **Health Live**: http://localhost:5000/health/live
- **Health Ready**: http://localhost:5000/health/ready

## Run Worker

```bash
dotnet run --project src/Worker
```

Worker memproses: outbox dispatch, notification, escalation, quota reconciliation, recycle purge.

## Database Schemas (24 schemas, 175 tables)

| Schema | Tables | Purpose |
|--------|--------|---------|
| saas | 8 | Tenant, subscription, quota |
| org | 8 | Company, BU, site, department |
| iam | 11 | Users, roles, permissions, scopes |
| platform | 19 | Records, workflow, evidence, audit, outbox |
| document | 3 | Controlled documents |
| safety | 1 | Hazard observations |
| risk | 7 | Risk register, assessment, matrix |
| incident | 6 | Incidents, investigations |
| capa | 4 | Corrective/preventive actions |
| cow | 21 | PTW, JSA, LOTO |
| inspection | 8 | Inspection templates & execution |
| audit | 7 | Audit programs & findings |
| compliance | 6 | Legal register, obligations |
| contractor | 6 | Contractor management |
| training | 9 | Courses, competency, certifications |
| ppe | 6 | PPE inventory & assignments |
| health | 6 | Occupational health (RESTRICTED) |
| chemical | 5 | Chemical inventory & SDS |
| environment | 7 | Environmental monitoring |
| sustainability | 4 | ESG indicators |
| asset | 6 | Safety-critical assets |
| emergency | 7 | Emergency plans & drills |
| reporting | 5 | KPI definitions & reports |
| integration | 5 | Interface runs & reconciliation |

## Key Architecture Decisions

- **ADR-001**: Modular monolith — satu deployable, bounded context dengan ownership schema tegas
- **ADR-003**: PostgreSQL shared database — tenant isolation via tenant_id + composite FK + RLS
- **ADR-005**: Generic record/workflow/evidence platform — traceability lintas modul
- **ADR-006**: Outbox + background worker — durable async untuk notification/integration

## Multi-Tenant Pattern

- Setiap tenant-owned table memiliki `tenant_id NOT NULL`
- Parent: `UNIQUE (tenant_id, id)`
- Child: composite FK `(tenant_id, parent_id)`
- EF Core global query filter sebagai developer guardrail
- PostgreSQL RLS sebagai database enforcement
- TenantContext di-set per connection dari authenticated claim

## Conventions

- **ID**: UUID (server-generated v4)
- **Timestamp**: UTC di storage, display pakai tenant timezone
- **Status**: varchar + check/lookup
- **Soft delete**: untuk master data
- **State transition**: untuk record lifecycle
- **Optimistic concurrency**: `version` column pada mutable aggregates
- **Audit**: setiap privileged change mencatat who/when/tenant/scope/reason

## Useful Commands

```bash
# Add migration
dotnet ef migrations add <MigrationName> \
  --project src/Modules/Platform/Infrastructure \
  --startup-project src/Api

# Update database
dotnet ef database update \
  --project src/Modules/Platform/Infrastructure \
  --startup-project src/Api

# Remove last migration
dotnet ef migrations remove \
  --project src/Modules/Platform/Infrastructure \
  --startup-project src/Api

# Generate SQL script
dotnet ef migrations script \
  --project src/Modules/Platform/Infrastructure \
  --startup-project src/Api \
  --output database/migrations/script.sql
```

## Seed Data

Seed data otomatis dijalankan saat PostgreSQL pertama kali start via `docker-entrypoint-initdb.d`:

- 3 subscription plans (Regular/Advance/Premium)
- 1 demo tenant
- 19 permissions (incident, capa, risk, inspection, permit, admin)
- 6 default roles (System Admin, HSE Manager, HSE Officer, Supervisor, Worker, Auditor)
- 4 data classifications (Public, Internal, Confidential, Restricted)
- 8 lookup values (severity + priority levels)
