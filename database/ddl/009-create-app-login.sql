-- ============================================================================
-- 009-create-app-login.sql — Create LOGIN role for EHSMS application
-- PostgreSQL 18+ · Neon
-- Creates a LOGIN role that the API will use. This role is NOT table owner,
-- so RLS policies apply to every query it makes.
--
-- ⚠️ SECURITY NOTE: This dev password is for LOCAL/NON-PRODUCTION only.
--    Rotate/change before any production use.
-- ============================================================================

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'ehsms_app') THEN
        CREATE ROLE ehsms_app LOGIN PASSWORD 'ehsms-dev-password-2026';
    ELSE
        ALTER ROLE ehsms_app LOGIN PASSWORD 'ehsms-dev-password-2026';
    END IF;
END $$;

-- Grant schema usage & table privileges (idempotent)
GRANT USAGE ON SCHEMA saas, org, iam, platform, document, safety, risk, incident, capa,
    cow, inspection, audit, compliance, contractor, training, ppe, health, chemical,
    environment, sustainability, asset, emergency, reporting, integration TO ehsms_app;

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
        EXECUTE format('GRANT USAGE ON ALL SEQUENCES IN SCHEMA %I TO ehsms_app', r.schemaname);
    END LOOP;
END $$;

-- Revoke default PUBLIC schema access on tenant schemas (defense in depth)
REVOKE ALL ON SCHEMA saas FROM PUBLIC;
REVOKE ALL ON SCHEMA org FROM PUBLIC;
REVOKE ALL ON SCHEMA iam FROM PUBLIC;
REVOKE ALL ON SCHEMA platform FROM PUBLIC;

SELECT 'ehsms_app login role created' AS status;