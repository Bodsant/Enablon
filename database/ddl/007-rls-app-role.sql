-- ============================================================================
-- 007-rls-app-role.sql — Application Role + Fail-Closed RLS Policies
-- PostgreSQL 18+ · Neon
-- Run AFTER 005-rls-policies.sql
--
-- IMPORTANT FIX: Previous policies used current_setting('app.current_tenant_id')
-- which THROWS an error when the param is unset. They also only protect against
-- non-owner roles. This script:
--   1. Creates a dedicated app role (no BYPASSRLS)
--   2. Grants schema/table privileges to that role
--   3. Recreates all tenant policies as FAIL-CLOSED:
--      current_setting('app.current_tenant_id', true)  -- missing_ok=true → NULL → no rows
-- ============================================================================

-- 1. Application role (NOLOGIN; app connects via owner + SET ROLE, or LOGIN in real env)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'ehsms_app') THEN
        CREATE ROLE ehsms_app NOLOGIN;
    END IF;
END $$;

-- 2. Schema usage
GRANT USAGE ON SCHEMA saas, org, iam, platform, document, safety, risk, incident, capa,
    cow, inspection, audit, compliance, contractor, training, ppe, health, chemical,
    environment, sustainability, asset, emergency, reporting, integration TO ehsms_app;

-- 3. Table privileges (all tables in tenant schemas + public)
DO $$
DECLARE
    r RECORD;
BEGIN
    FOR r IN
        SELECT schemaname
        FROM pg_tables
        WHERE schemaname NOT IN ('pg_catalog','information_schema')
        GROUP BY schemaname
    LOOP
        EXECUTE format('GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA %I TO ehsms_app', r.schemaname);
    END LOOP;
END $$;

-- Default privileges: future tables also granted
ALTER DEFAULT PRIVILEGES IN SCHEMA saas, org, iam, platform, document, safety, risk, incident, capa,
    cow, inspection, audit, compliance, contractor, training, ppe, health, chemical,
    environment, sustainability, asset, emergency, reporting, integration
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO ehsms_app;

-- 4. Recreate ALL tenant policies as FAIL-CLOSED (missing_ok=true → NULL → no match → 0 rows)
DO $$
DECLARE
    r RECORD;
    policy_name TEXT;
    drop_sql TEXT;
    create_sql TEXT;
BEGIN
    FOR r IN
        SELECT schemaname, tablename
        FROM pg_tables
        WHERE schemaname NOT IN ('pg_catalog','information_schema')
          AND EXISTS (
              SELECT 1 FROM pg_attribute a
              JOIN pg_class c ON a.attrelid = c.oid
              JOIN pg_namespace n ON c.relnamespace = n.oid
              WHERE n.nspname = pg_tables.schemaname
                AND c.relname = pg_tables.tablename
                AND a.attname = 'tenant_id' AND NOT a.attisdropped
          )
    LOOP
        policy_name := format('pol_tenant_isolation_%s', r.tablename);
        -- Drop existing policy if present
        drop_sql := format('DROP POLICY IF EXISTS %I ON %I.%I', policy_name, r.schemaname, r.tablename);
        BEGIN
            EXECUTE drop_sql;
        EXCEPTION WHEN OTHERS THEN NULL;  -- policy may not exist yet
        END;
        -- Create fail-closed policy
        create_sql := format(
            'CREATE POLICY %I ON %I.%I FOR ALL TO ehsms_app USING (tenant_id = current_setting(''app.current_tenant_id'', true)::uuid) WITH CHECK (tenant_id = current_setting(''app.current_tenant_id'', true)::uuid)',
            policy_name, r.schemaname, r.tablename
        );
        EXECUTE create_sql;
    END LOOP;
END $$;

-- 5. Summary
SELECT 'RLS app role configured' AS status;