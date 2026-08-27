# Local development
PostgreSQL is an optional compose fixture for future integration work; the current API has no database wiring and its readiness endpoint does not inspect PostgreSQL.

1. If the fixture is needed, run `cp .env.example .env` and set a unique local password.
2. Validate with `docker compose -f infra/local/compose.yml --env-file .env config`, then start with `up -d`.
3. Fixture health is visible with `docker compose -f infra/local/compose.yml --env-file .env ps`; it is separate from API `/health/ready`.
4. Build/test commands are in the root README and `scripts/verify.sh`.
5. Stop with `docker compose -f infra/local/compose.yml --env-file .env down`.
