-- ============================================================================
-- 008-rls-dev-login-role.sql — Dev login role for testing RLS
-- PostgreSQL 18+ · Neon
-- Creates a LOGIN role that inherits ehsms_app's privileges (but NOT bypass RLS)
-- Use this role in dev connection strings to exercise RLS.
-- ============================================================================

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'ehsms_dev') THEN
        -- LOGIN + PASSWORD will be managed by Neon console; here just NOLOGIN placeholder
        CREATE ROLE ehsms_dev NOLOGIN;
    END IF;
END $$;

-- Inherit app privileges
GRANT ehsms_app TO ehsms_dev;

-- Alternate approach: directly grant schema privileges to ehsms_dev too
GRANT USAGE ON SCHEMA saas, org, iam, platform, document, safety, risk, incident, capa,
    cow, inspection, audit, compliance, contractor, training, ppe, health, chemical,
    environment, sustainability, asset, emergency, reporting, integration TO ehsms_dev;

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
        EXECUTE format('GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA %I TO ehsms_dev', r.schemaname);
        EXECUTE format('GRANT USAGE ON ALL SEQUENCES IN SCHEMA %I TO ehsms_dev', r.schemaname);
    END LOOP;
END $$;