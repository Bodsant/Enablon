-- ============================================================================
-- 005-rls-policies.sql — Row-Level Security for Tenant Isolation
-- PostgreSQL 18+ · Neon
-- Run AFTER all tables created (001-004)
-- ============================================================================

-- This script enables RLS on ALL tables with tenant_id column
-- and creates policies that filter by current_setting('app.current_tenant_id')
--
-- Application MUST set: SET LOCAL app.current_tenant_id = '<tenant-uuid>';
-- before any query. This is typically done in middleware per request.

-- ============================================================================
-- HELPER: Generate ALTER TABLE ... ENABLE ROW LEVEL SECURITY for all tables
-- ============================================================================

DO $$
DECLARE
    r RECORD;
BEGIN
    FOR r IN
        SELECT schemaname, tablename
        FROM pg_tables
        WHERE schemaname NOT IN ('pg_catalog','information_schema')
          AND schemaname NOT LIKE 'pg\_%'
          AND EXISTS (
              SELECT 1 FROM pg_attribute a
              JOIN pg_class c ON a.attrelid = c.oid
              JOIN pg_namespace n ON c.relnamespace = n.oid
              WHERE n.nspname = pg_tables.schemaname
                AND c.relname = pg_tables.tablename
                AND a.attname = 'tenant_id'
                AND NOT a.attisdropped
          )
    LOOP
        EXECUTE format('ALTER TABLE %I.%I ENABLE ROW LEVEL SECURITY', r.schemaname, r.tablename);
        RAISE NOTICE 'RLS enabled on %.%', r.schemaname, r.tablename;
    END LOOP;
END $$;

-- ============================================================================
-- POLICIES: One policy per table (read + write combined)
-- Using FOR ALL to cover SELECT, INSERT, UPDATE, DELETE
-- ============================================================================

DO $$
DECLARE
    r RECORD;
    policy_name TEXT;
    policy_sql TEXT;
BEGIN
    FOR r IN
        SELECT schemaname, tablename
        FROM pg_tables
        WHERE schemaname NOT IN ('pg_catalog','information_schema')
          AND schemaname NOT LIKE 'pg\_%'
          AND EXISTS (
              SELECT 1 FROM pg_attribute a
              JOIN pg_class c ON a.attrelid = c.oid
              JOIN pg_namespace n ON c.relnamespace = n.oid
              WHERE n.nspname = pg_tables.schemaname
                AND c.relname = pg_tables.tablename
                AND a.attname = 'tenant_id'
                AND NOT a.attisdropped
          )
    LOOP
        policy_name := format('pol_tenant_isolation_%s', r.tablename);
        policy_sql := format(
            'CREATE POLICY %I ON %I.%I FOR ALL TO PUBLIC USING (tenant_id = current_setting(''app.current_tenant_id'')::uuid) WITH CHECK (tenant_id = current_setting(''app.current_tenant_id'')::uuid)',
            policy_name, r.schemaname, r.tablename
        );
        EXECUTE policy_sql;
        RAISE NOTICE 'Policy created: % on %.%', policy_name, r.schemaname, r.tablename;
    END LOOP;
END $$;

-- ============================================================================
-- EXCEPTIONS: Tables WITHOUT tenant_id (should be none in our model)
-- ============================================================================

-- Verify: list tables without tenant_id that might need special handling
DO $$
DECLARE
    r RECORD;
BEGIN
    FOR r IN
        SELECT schemaname, tablename
        FROM pg_tables
        WHERE schemaname NOT IN ('pg_catalog','information_schema')
          AND schemaname NOT LIKE 'pg\_%'
          AND NOT EXISTS (
              SELECT 1 FROM pg_attribute a
              JOIN pg_class c ON a.attrelid = c.oid
              JOIN pg_namespace n ON c.relnamespace = n.oid
              WHERE n.nspname = pg_tables.schemaname
                AND c.relname = pg_tables.tablename
                AND a.attname = 'tenant_id'
                AND NOT a.attisdropped
          )
    LOOP
        RAISE NOTICE 'TABLE WITHOUT tenant_id: %.% (may need manual policy)', r.schemaname, r.tablename;
    END LOOP;
END $$;

-- ============================================================================
-- GRANT: Allow app role to SELECT/INSERT/UPDATE/DELETE on all tenant tables
-- ============================================================================

-- NOTE: Adjust role name per your deployment (e.g., 'ehsms_app', 'web_anon', etc.)
-- GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA saas, org, iam, platform, document, safety, risk, incident, capa, cow, inspection, audit, compliance, contractor, training, ppe, health, chemical, environment, sustainability, asset, emergency, reporting, integration TO ehsms_app;

-- ============================================================================
-- VERIFICATION QUERIES (run after this script)
-- ============================================================================

-- 1. Check all tables have RLS enabled
-- SELECT schemaname, tablename, rowsecurity FROM pg_tables WHERE schemaname NOT IN ('pg_catalog','information_schema') ORDER BY schemaname;

-- 2. Check policies
-- SELECT schemaname, tablename, policyname, cmd, permissive, roles, qual, with_check FROM pg_policies WHERE schemaname NOT IN ('pg_catalog','information_schema') ORDER BY schemaname, tablename;

-- 3. Test policy works
-- SET LOCAL app.current_tenant_id = '00000000-0000-0000-0000-000000000000';
-- SELECT * FROM saas.tenants; -- Should return 0 rows
-- SET LOCAL app.current_tenant_id = '<valid-tenant-uuid>';
-- SELECT * FROM saas.tenants; -- Should return only that tenant's rows