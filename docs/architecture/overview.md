# Architecture overview
**DECIDED (TDD baseline):** modular monolith; Angular responsive client; PostgreSQL shared database/schema set; private object storage; generic platform; outbox/worker; versioned configuration; free tier only for dev/demo. This repository implements structural project boundaries and marker types only; the API composition root does **not** wire module entry points or business services.

The API exposes `/api/v1/architecture/info`, `/health/live`, and `/health/ready`. Both probes currently select the named `process-readiness` self-check. Readiness therefore means only that this scaffold process can respond; there are no mandatory external dependencies and it makes no database-readiness claim. Authentication is explicitly **not configured**, so no protected business endpoint exists. The worker only hosts a cancellation-aware idle service and does not claim outbox processing.

PostgreSQL in `infra/local/compose.yml` is an optional local development fixture. It is not wired to the API, and starting or stopping it does not affect the current process-readiness result.

**BLOCKED/FUTURE:** module composition, tenant resolution, EF query filters, PostgreSQL RLS, DBML migrations, authentication/authorization and dependency readiness checks require approved contracts and integration evidence. No tenancy/RLS/dependency health claim is made. Architecture tests enforce the current project-reference direction and reject the former `|| true` bypass pattern.
