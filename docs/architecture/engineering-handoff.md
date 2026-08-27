# Engineering handoff
## Implemented scaffold
Exact .NET 8 SDK and Node patch pins, centrally pinned packages with NuGet lock files, Angular lazy route shell, module boundary projects/markers, unit/integration/architecture tests, process-only probe/metadata API, idle worker, optional local PostgreSQL compose fixture, and CI dependency scans.

The module projects are boundary placeholders; the API does not yet wire module entry points or business registrations. `/health/ready` selects only the named process self-check and proves no external dependency. PostgreSQL is not connected to the API.

## Engineer work (future)
1. Approve DBML and create reviewed additive migrations before seed data.
2. Design authenticated tenant context; prove EF filtering plus PostgreSQL RLS with Testcontainers negative cross-tenant tests. Fail closed.
3. Select and implement authentication/authorization through workshop/ADR; do not treat UI guards as security.
4. Add business vertical slices via RED→GREEN tests; do not put domain logic in endpoints.
5. Implement outbox only with durable schema, idempotency/retry/dead-letter evidence.
6. Wire approved module entry points at the composition root and add required readiness checks only when mandatory dependencies exist.

## Accepted risks and follow-up
- GitHub Actions remain pinned to reviewed major tags (`@v4`) rather than immutable SHAs. No SHA was guessed; verify trusted upstream commit SHAs and pin them as a supply-chain hardening follow-up.
- The full frontend development-tree audit can contain transitive tooling findings. The enforced runtime audit is `npm audit --omit=dev --audit-level=high` and must remain free of high runtime vulnerabilities.
- Rotate/revoke the credential formerly committed and decide history remediation. Never paste it into issues, logs, docs, examples, or CI.
