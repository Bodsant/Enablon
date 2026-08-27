# Data, migrations, and tenant isolation
**DECIDED:** PostgreSQL; tenant-owned rows use non-null `tenant_id`, same-tenant composite FKs and tenant-scoped unique keys; UTC `timestamptz`; reviewed expand/contract delivery. Migrations are controlled steps and never API startup behavior.

**BLOCKED/FUTURE:** the 175-table DBML, EF mappings/query filters, RLS policies and runtime roles are absent. Before claiming isolation, integration tests must prove cross-tenant denial at application and database layers, pooled connections must not leak context, and runtime roles must not bypass RLS. Schema migration runs before separate synthetic seed; seed is never a schema substitute.
