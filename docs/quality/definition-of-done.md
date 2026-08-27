# Definition of Done and quality gates
A future feature is done only with traceability, approved scope/classification, UX states, invariant/workflow/audit behavior, positive and negative tenant/object authorization tests, reviewed migration/index/rollback plan, risk-based tests, contract/docs/telemetry updates, accessibility/performance/privacy/security evidence, and owner acceptance.

PR gate: repository hygiene, pinned restore, build, unit/integration/architecture tests, frontend non-watch tests/build, secret/dependency scan. Production readiness additionally requires E2E/security/performance, backup restore evidence, monitoring/alerts, migration rehearsal and runbooks. This scaffold passing CI is **not** production readiness.
