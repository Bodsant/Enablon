-- ============================================================================
-- 006-dev-seed.sql — Development Seed Data (Synthetic)
-- PostgreSQL 18+ · Neon
-- Idempotent: safe to re-run (INSERT .. ON CONFLICT DO NOTHING / WHERE NOT EXISTS)
-- ============================================================================
-- CONTEXT: Run as table owner (bypasses RLS) OR set app.current_tenant_id.
-- Fixed UUIDs below make every run deterministic and cross-run stable.
-- NOTE: For VALUES+alias patterns, cast explicitly (::uuid, ::text) inside values.

-- ════════════════════════════════════════════════════════════════════════════
-- 1. SUBSCRIPTION PLANS (global — no tenant_id)
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO saas.subscription_plans (id, code, name, description, is_active)
VALUES
    ('10000000-0000-0000-0000-000000000001'::uuid, 'ENTERPRISE', 'Enterprise', 'Full EHSMS platform for enterprises', true)
ON CONFLICT (id) DO NOTHING;

INSERT INTO saas.plan_versions (id, subscription_plan_id, version_number, max_active_users, max_companies, max_business_units, max_sites, max_storage_bytes, max_period_upload_bytes, max_file_size_bytes, effective_from, effective_until, is_current)
VALUES
    ('10000000-0000-0000-0000-000000000011'::uuid, '10000000-0000-0000-0000-000000000001'::uuid, 1, 1000, 50, 200, 50, 1099511627776, 107374182400, 5368709120, '2026-01-01T00:00:00Z', NULL, true)
ON CONFLICT (id) DO NOTHING;

-- ════════════════════════════════════════════════════════════════════════════
-- 2. TENANTS
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO saas.tenants (id, tenant_code, slug, display_name, timezone, billing_anchor_day, status)
VALUES
    ('11111111-1111-1111-1111-111111111111'::uuid, 'MAJU',   'pt-maju-jaya',        'PT Maju Jaya Energi',    'Asia/Jakarta', 1,  'active'),
    ('22222222-2222-2222-2222-222222222222'::uuid, 'SEJAHTERA', 'pt-sejahtera-bersama','PT Sejahtera Bersama', 'Asia/Jakarta', 15, 'active')
ON CONFLICT (id) DO NOTHING;

-- ════════════════════════════════════════════════════════════════════════════
-- 3. TENANT SUBSCRIPTIONS
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO saas.tenant_subscriptions (id, tenant_id, plan_version_id, status, started_at, current_period_start, current_period_end, next_reset_at)
VALUES
    ('11111111-1111-1111-1111-111111111121'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '10000000-0000-0000-0000-000000000011'::uuid, 'active', '2026-01-01T00:00:00Z', '2026-08-01T00:00:00Z', '2026-09-01T00:00:00Z', '2026-09-01T00:00:00Z'),
    ('22222222-2222-2222-2222-222222222221'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, '10000000-0000-0000-0000-000000000011'::uuid, 'active', '2026-02-01T00:00:00Z', '2026-08-01T00:00:00Z', '2026-09-01T00:00:00Z', '2026-09-01T00:00:00Z')
ON CONFLICT (id) DO NOTHING;

-- ════════════════════════════════════════════════════════════════════════════
-- 4. DATA CLASSIFICATIONS (per tenant)
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO platform.data_classifications (id, tenant_id, code, name, rank, is_restricted)
VALUES
    ('13110000-0000-0000-0000-000000000001'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, 'PUBLIC',      'Public',       1, false),
    ('13110000-0000-0000-0000-000000000002'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, 'INTERNAL',    'Internal',     2, false),
    ('13110000-0000-0000-0000-000000000003'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, 'CONFIDENTIAL','Confidential', 3, true),
    ('13110000-0000-0000-0000-000000000004'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, 'RESTRICTED',  'Restricted',   4, true),
    ('13220000-0000-0000-0000-000000000001'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, 'PUBLIC',      'Public',       1, false),
    ('13220000-0000-0000-0000-000000000002'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, 'INTERNAL',    'Internal',     2, false),
    ('13220000-0000-0000-0000-000000000003'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, 'CONFIDENTIAL','Confidential', 3, true),
    ('13220000-0000-0000-0000-000000000004'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, 'RESTRICTED',  'Restricted',   4, true)
ON CONFLICT (id) DO NOTHING;

-- ════════════════════════════════════════════════════════════════════════════
-- 5. LOOKUP VALUES (shared categories, per tenant)
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO platform.lookup_values (id, tenant_id, category, code, label, effective_from, effective_to, status, metadata_json)
SELECT lv.id::uuid, t.id, lv.category, lv.code, lv.label, lv.effective_from::date, lv.effective_to::date, lv.status, lv.metadata_json::jsonb
FROM (VALUES
    ('14000000-0000-0000-0000-000000000001', 'incident_type',   'LOST_TIME_INJURY', 'Lost Time Injury',       NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000002', 'incident_type',   'MEDICAL_TREATMENT','Medical Treatment Case', NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000003', 'incident_type',   'NEAR_MISS',        'Near Miss',              NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000004', 'incident_type',   'ENVIRONMENTAL',    'Environmental Incident', NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000005', 'incident_type',   'PROPERTY_DAMAGE',  'Property Damage',        NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000011', 'severity',        'LOW',      'Low',      NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000012', 'severity',        'MEDIUM',   'Medium',   NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000013', 'severity',        'HIGH',     'High',     NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000014', 'severity',        'CRITICAL', 'Critical', NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000021', 'hazard_category', 'MECHANICAL', 'Mechanical Hazard', NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000022', 'hazard_category', 'CHEMICAL',   'Chemical Hazard',   NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000023', 'hazard_category', 'ELECTRICAL', 'Electrical Hazard', NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000024', 'hazard_category', 'ERGONOMIC',  'Ergonomic Hazard',  NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000025', 'hazard_category', 'FALL',       'Fall Hazard',       NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000031', 'observation_type','GOOD_CATCH',   'Good Catch',      NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000032', 'observation_type','UNSAFE_ACT',   'Unsafe Act',      NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000033', 'observation_type','UNSAFE_CONDITION','Unsafe Condition', NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000041', 'record_status',   'OPEN',       'Open',       NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000042', 'record_status',   'IN_PROGRESS','In Progress',NULL, NULL, 'active', NULL),
    ('14000000-0000-0000-0000-000000000043', 'record_status',   'CLOSED',     'Closed',     NULL, NULL, 'active', NULL)
) AS lv(id, category, code, label, effective_from, effective_to, status, metadata_json)
CROSS JOIN saas.tenants t
ON CONFLICT (id) DO NOTHING;

-- ════════════════════════════════════════════════════════════════════════════
-- 6. USERS (global identity — no tenant_id)
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO iam.users (id, email, normalized_email, password_hash, identity_provider, external_subject, status)
VALUES
    ('15000000-0000-0000-0000-000000000001'::uuid, 'admin.maju@ehsms.dev',   'ADMIN.MAJU@EHSMS.DEV',   NULL, 'dev-seed', 'user-1', 'active'),
    ('15000000-0000-0000-0000-000000000002'::uuid, 'operator.maju@ehsms.dev','OPERATOR.MAJU@EHSMS.DEV',NULL, 'dev-seed', 'user-2', 'active'),
    ('15000000-0000-0000-0000-000000000003'::uuid, 'manager.maju@ehsms.dev', 'MANAGER.MAJU@EHSMS.DEV', NULL, 'dev-seed', 'user-3', 'active'),
    ('15000000-0000-0000-0000-000000000004'::uuid, 'admin.sejahtera@ehsms.dev', 'ADMIN.SEJAHTERA@EHSMS.DEV', NULL, 'dev-seed', 'user-4', 'active'),
    ('15000000-0000-0000-0000-000000000005'::uuid, 'operator.sejahtera@ehsms.dev', 'OPERATOR.SEJAHTERA@EHSMS.DEV', NULL, 'dev-seed', 'user-5', 'active'),
    ('15000000-0000-0000-0000-000000000006'::uuid, 'manager.sejahtera@ehsms.dev', 'MANAGER.SEJAHTERA@EHSMS.DEV', NULL, 'dev-seed', 'user-6', 'active')
ON CONFLICT (id) DO NOTHING;

-- ════════════════════════════════════════════════════════════════════════════
-- 7. PEOPLE (per tenant)
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO org.people (id, tenant_id, person_type, full_name, email, phone, status, data_classification_id)
VALUES
    ('16110000-0000-0000-0000-000000000001'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, 'employee', 'Budi Santoso',   'budi.santoso@ehsms.dev',   '0812-1111-0001', 'active', '13110000-0000-0000-0000-000000000002'::uuid),
    ('16110000-0000-0000-0000-000000000002'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, 'employee', 'Siti Rahayu',    'siti.rahayu@ehsms.dev',    '0812-1111-0002', 'active', '13110000-0000-0000-0000-000000000003'::uuid),
    ('16110000-0000-0000-0000-000000000003'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, 'employee', 'Agus Wijaya',    'agus.wijaya@ehsms.dev',    '0812-1111-0003', 'active', '13110000-0000-0000-0000-000000000002'::uuid),
    ('16220000-0000-0000-0000-000000000001'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, 'employee', 'Dewi Lestari',   'dewi.lestari@ehsms.dev',   '0812-2222-0001', 'active', '13220000-0000-0000-0000-000000000002'::uuid),
    ('16220000-0000-0000-0000-000000000002'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, 'employee', 'Rudi Hartono',   'rudi.hartono@ehsms.dev',   '0812-2222-0002', 'active', '13220000-0000-0000-0000-000000000003'::uuid),
    ('16220000-0000-0000-0000-000000000003'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, 'employee', 'Maya Anggraini', 'maya.anggraini@ehsms.dev', '0812-2222-0003', 'active', '13220000-0000-0000-0000-000000000002'::uuid)
ON CONFLICT (id) DO NOTHING;

-- ════════════════════════════════════════════════════════════════════════════
-- 8. TENANT MEMBERS (link users → tenants → people)
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO iam.tenant_members (id, tenant_id, user_id, person_id, display_name, status, activated_at, deactivated_at)
VALUES
    ('17000000-0000-0000-0000-000000000001'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '15000000-0000-0000-0000-000000000001'::uuid, '16110000-0000-0000-0000-000000000001'::uuid, 'Budi Santoso',   'active', now(), NULL),
    ('17000000-0000-0000-0000-000000000002'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '15000000-0000-0000-0000-000000000002'::uuid, '16110000-0000-0000-0000-000000000002'::uuid, 'Siti Rahayu',    'active', now(), NULL),
    ('17000000-0000-0000-0000-000000000003'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '15000000-0000-0000-0000-000000000003'::uuid, '16110000-0000-0000-0000-000000000003'::uuid, 'Agus Wijaya',    'active', now(), NULL),
    ('17000000-0000-0000-0000-000000000004'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, '15000000-0000-0000-0000-000000000004'::uuid, '16220000-0000-0000-0000-000000000001'::uuid, 'Dewi Lestari',   'active', now(), NULL),
    ('17000000-0000-0000-0000-000000000005'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, '15000000-0000-0000-0000-000000000005'::uuid, '16220000-0000-0000-0000-000000000002'::uuid, 'Rudi Hartono',   'active', now(), NULL),
    ('17000000-0000-0000-0000-000000000006'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, '15000000-0000-0000-0000-000000000006'::uuid, '16220000-0000-0000-0000-000000000003'::uuid, 'Maya Anggraini', 'active', now(), NULL)
ON CONFLICT (id) DO NOTHING;

-- ════════════════════════════════════════════════════════════════════════════
-- 9. ROLES (per tenant)
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO iam.roles (id, tenant_id, code, name, scope_type, is_system)
SELECT r.id::uuid, t.id, r.code, r.name, r.scope_type, r.is_system::boolean
FROM (VALUES
    ('18000000-0000-0000-0000-000000000001', 'TENANT_ADMIN', 'Tenant Administrator', 'tenant',  true),
    ('18000000-0000-0000-0000-000000000002', 'SITE_MANAGER', 'Site Manager',         'site',    true),
    ('18000000-0000-0000-0000-000000000003', 'HSE_STAFF',    'HSE Staff',            'site',    true),
    ('18000000-0000-0000-0000-000000000004', 'WORKER',       'Worker',               'company', true)
) AS r(id, code, name, scope_type, is_system)
CROSS JOIN saas.tenants t
ON CONFLICT (id) DO NOTHING;

-- ════════════════════════════════════════════════════════════════════════════
-- 10. PERMISSIONS (per tenant, minimal set)
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO iam.permissions (id, tenant_id, code, module, action, description)
SELECT p.id::uuid, t.id, p.code, p.module, p.action, p.description
FROM (VALUES
    ('19000000-0000-0000-0000-000000000001', 'incident.view',   'incident', 'view',   'View incidents'),
    ('19000000-0000-0000-0000-000000000002', 'incident.create', 'incident', 'create', 'Create incidents'),
    ('19000000-0000-0000-0000-000000000003', 'incident.resolve','incident', 'resolve','Resolve incidents'),
    ('19000000-0000-0000-0000-000000000004', 'safety.observe',  'safety',   'create', 'Report observations'),
    ('19000000-0000-0000-0000-000000000005', 'risk.assess',     'risk',     'assess', 'Perform risk assessments'),
    ('19000000-0000-0000-0000-000000000006', 'tenant.admin',    'saas',     'admin',  'Tenant administration'),
    ('19000000-0000-0000-0000-000000000007', 'org.manage',      'org',      'manage', 'Manage organization structure')
) AS p(id, code, module, action, description)
CROSS JOIN saas.tenants t
ON CONFLICT (id) DO NOTHING;

-- ════════════════════════════════════════════════════════════════════════════
-- 11. ROLE_PERMISSIONS (per tenant)
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO iam.role_permissions (id, tenant_id, role_id, permission_id)
SELECT rp.id::uuid, t.id, rp.role_id::uuid, rp.permission_id::uuid
FROM (VALUES
    -- TENANT_ADMIN gets everything
    ('20000000-0000-0000-0000-000000000101', '18000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000001'),
    ('20000000-0000-0000-0000-000000000102', '18000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000002'),
    ('20000000-0000-0000-0000-000000000103', '18000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000003'),
    ('20000000-0000-0000-0000-000000000104', '18000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000004'),
    ('20000000-0000-0000-0000-000000000105', '18000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000005'),
    ('20000000-0000-0000-0000-000000000106', '18000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000006'),
    ('20000000-0000-0000-0000-000000000107', '18000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000007'),
    -- SITE_MANAGER
    ('20000000-0000-0000-0000-000000000201', '18000000-0000-0000-0000-000000000002', '19000000-0000-0000-0000-000000000001'),
    ('20000000-0000-0000-0000-000000000202', '18000000-0000-0000-0000-000000000002', '19000000-0000-0000-0000-000000000002'),
    ('20000000-0000-0000-0000-000000000203', '18000000-0000-0000-0000-000000000002', '19000000-0000-0000-0000-000000000003'),
    ('20000000-0000-0000-0000-000000000204', '18000000-0000-0000-0000-000000000002', '19000000-0000-0000-0000-000000000004'),
    ('20000000-0000-0000-0000-000000000205', '18000000-0000-0000-0000-000000000002', '19000000-0000-0000-0000-000000000005'),
    -- HSE_STAFF
    ('20000000-0000-0000-0000-000000000301', '18000000-0000-0000-0000-000000000003', '19000000-0000-0000-0000-000000000001'),
    ('20000000-0000-0000-0000-000000000302', '18000000-0000-0000-0000-000000000003', '19000000-0000-0000-0000-000000000002'),
    ('20000000-0000-0000-0000-000000000303', '18000000-0000-0000-0000-000000000003', '19000000-0000-0000-0000-000000000004'),
    ('20000000-0000-0000-0000-000000000304', '18000000-0000-0000-0000-000000000003', '19000000-0000-0000-0000-000000000005'),
    -- WORKER
    ('20000000-0000-0000-0000-000000000401', '18000000-0000-0000-0000-000000000004', '19000000-0000-0000-0000-000000000004')
) AS rp(id, role_id, permission_id)
CROSS JOIN saas.tenants t
ON CONFLICT (id) DO NOTHING;

-- ════════════════════════════════════════════════════════════════════════════
-- 12. MEMBER_ROLES (assign roles to members, per tenant)
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO iam.member_roles (id, tenant_id, tenant_member_id, role_id)
SELECT mr.id::uuid, t.id, mr.tenant_member_id::uuid, mr.role_id::uuid
FROM (VALUES
    ('21000000-0000-0000-0000-000000000001', '17000000-0000-0000-0000-000000000001', '18000000-0000-0000-0000-000000000001'), -- Budi = Tenant Admin
    ('21000000-0000-0000-0000-000000000002', '17000000-0000-0000-0000-000000000002', '18000000-0000-0000-0000-000000000003'), -- Siti = HSE Staff
    ('21000000-0000-0000-0000-000000000003', '17000000-0000-0000-0000-000000000003', '18000000-0000-0000-0000-000000000002'), -- Agus = Site Manager
    ('21000000-0000-0000-0000-000000000004', '17000000-0000-0000-0000-000000000004', '18000000-0000-0000-0000-000000000001'), -- Dewi = Tenant Admin
    ('21000000-0000-0000-0000-000000000005', '17000000-0000-0000-0000-000000000005', '18000000-0000-0000-0000-000000000003'), -- Rudi = HSE Staff
    ('21000000-0000-0000-0000-000000000006', '17000000-0000-0000-0000-000000000006', '18000000-0000-0000-0000-000000000002')  -- Maya = Site Manager
) AS mr(id, tenant_member_id, role_id)
CROSS JOIN saas.tenants t
ON CONFLICT (id) DO NOTHING;

-- ════════════════════════════════════════════════════════════════════════════
-- 13. ORG STRUCTURE (per tenant)
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO org.companies (id, tenant_id, code, name, legal_name, registration_number, status, effective_from, effective_to)
VALUES
    ('22110000-0000-0000-0000-000000000001'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, 'MJE', 'PT Maju Jaya Energi', 'PT Maju Jaya Energi Tbk', 'REG-001', 'active', '2026-01-01', NULL),
    ('22220000-0000-0000-0000-000000000001'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, 'SBB', 'PT Sejahtera Bersama','PT Sejahtera Bersama',    'REG-002', 'active', '2026-02-01', NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO org.business_units (id, tenant_id, company_id, parent_business_unit_id, code, name, status)
VALUES
    ('22110000-0000-0000-0000-000000000011'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '22110000-0000-0000-0000-000000000001'::uuid, NULL, 'OPR', 'Operations',   'active'),
    ('22110000-0000-0000-0000-000000000012'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '22110000-0000-0000-0000-000000000001'::uuid, NULL, 'MNT', 'Maintenance', 'active'),
    ('22220000-0000-0000-0000-000000000011'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, '22220000-0000-0000-0000-000000000001'::uuid, NULL, 'OPR', 'Operations',   'active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO org.sites (id, tenant_id, company_id, business_unit_id, code, name, address, timezone, latitude, longitude, status)
VALUES
    ('22110000-0000-0000-0000-000000000021'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '22110000-0000-0000-0000-000000000001'::uuid, '22110000-0000-0000-0000-000000000011'::uuid, 'PLN-1', 'Plant 1', 'Jl. Raya Industri No.1, Cilegon', 'Asia/Jakarta', -6.0029, 106.0323, 'active'),
    ('22110000-0000-0000-0000-000000000022'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '22110000-0000-0000-0000-000000000001'::uuid, '22110000-0000-0000-0000-000000000012'::uuid, 'PLN-2', 'Plant 2', 'Jl. Raya Industri No.2, Cilegon', 'Asia/Jakarta', -6.0100, 106.0400, 'active'),
    ('22220000-0000-0000-0000-000000000021'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, '22220000-0000-0000-0000-000000000001'::uuid, '22220000-0000-0000-0000-000000000011'::uuid, 'GRK-1', 'Gresik Plant', 'Jl. Manyar No.5, Gresik', 'Asia/Jakarta', -7.1538, 112.6561, 'active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO org.departments (id, tenant_id, business_unit_id, site_id, parent_department_id, code, name, status)
VALUES
    ('22110000-0000-0000-0000-000000000031'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '22110000-0000-0000-0000-000000000011'::uuid, '22110000-0000-0000-0000-000000000021'::uuid, NULL, 'HSE',  'HSE Department', 'active'),
    ('22110000-0000-0000-0000-000000000032'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '22110000-0000-0000-0000-000000000012'::uuid, '22110000-0000-0000-0000-000000000022'::uuid, NULL, 'TECH', 'Technical Dept', 'active'),
    ('22220000-0000-0000-0000-000000000031'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, '22220000-0000-0000-0000-000000000011'::uuid, '22220000-0000-0000-0000-000000000021'::uuid, NULL, 'HSE',  'HSE Department', 'active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO org.locations (id, tenant_id, site_id, parent_location_id, code, name, location_type, status)
VALUES
    ('22110000-0000-0000-0000-000000000041'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '22110000-0000-0000-0000-000000000021'::uuid, NULL, 'A-01', 'Area A - Production', 'area', 'active'),
    ('22110000-0000-0000-0000-000000000042'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '22110000-0000-0000-0000-000000000021'::uuid, NULL, 'B-01', 'Area B - Warehouse',  'area', 'active'),
    ('22220000-0000-0000-0000-000000000041'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, '22220000-0000-0000-0000-000000000021'::uuid, NULL, 'P-01', 'Processing Area',     'area', 'active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO org.positions (id, tenant_id, code, name, description, status)
VALUES
    ('22110000-0000-0000-0000-000000000051'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, 'OPS-MGR', 'Operations Manager', 'Manages operations', 'active'),
    ('22110000-0000-0000-0000-000000000052'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, 'HSE-OFF', 'HSE Officer',        'Safety officer',     'active'),
    ('22220000-0000-0000-0000-000000000051'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, 'OPS-MGR', 'Operations Manager', 'Manages operations', 'active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO org.employees (id, tenant_id, person_id, employee_number, company_id, department_id, position_id, manager_person_id, employment_status, source_system, source_id)
VALUES
    ('22110000-0000-0000-0000-000000000061'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '16110000-0000-0000-0000-000000000001'::uuid, 'EMP-0001', '22110000-0000-0000-0000-000000000001'::uuid, '22110000-0000-0000-0000-000000000031'::uuid, '22110000-0000-0000-0000-000000000052'::uuid, NULL, 'active', 'dev-seed', NULL),
    ('22110000-0000-0000-0000-000000000062'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '16110000-0000-0000-0000-000000000002'::uuid, 'EMP-0002', '22110000-0000-0000-0000-000000000001'::uuid, '22110000-0000-0000-0000-000000000031'::uuid, '22110000-0000-0000-0000-000000000052'::uuid, '16110000-0000-0000-0000-000000000001'::uuid, 'active', 'dev-seed', NULL),
    ('22110000-0000-0000-0000-000000000063'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, '16110000-0000-0000-0000-000000000003'::uuid, 'EMP-0003', '22110000-0000-0000-0000-000000000001'::uuid, '22110000-0000-0000-0000-000000000031'::uuid, '22110000-0000-0000-0000-000000000051'::uuid, '16110000-0000-0000-0000-000000000001'::uuid, 'active', 'dev-seed', NULL),
    ('22220000-0000-0000-0000-000000000061'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, '16220000-0000-0000-0000-000000000001'::uuid, 'EMP-1001', '22220000-0000-0000-0000-000000000001'::uuid, '22220000-0000-0000-0000-000000000031'::uuid, '22220000-0000-0000-0000-000000000051'::uuid, NULL, 'active', 'dev-seed', NULL),
    ('22220000-0000-0000-0000-000000000062'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, '16220000-0000-0000-0000-000000000002'::uuid, 'EMP-1002', '22220000-0000-0000-0000-000000000001'::uuid, '22220000-0000-0000-0000-000000000031'::uuid, '22220000-0000-0000-0000-000000000051'::uuid, '16220000-0000-0000-0000-000000000001'::uuid, 'active', 'dev-seed', NULL),
    ('22220000-0000-0000-0000-000000000063'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, '16220000-0000-0000-0000-000000000003'::uuid, 'EMP-1003', '22220000-0000-0000-0000-000000000001'::uuid, '22220000-0000-0000-0000-000000000031'::uuid, '22220000-0000-0000-0000-000000000051'::uuid, '16220000-0000-0000-0000-000000000001'::uuid, 'active', 'dev-seed', NULL)
ON CONFLICT (id) DO NOTHING;

-- ════════════════════════════════════════════════════════════════════════════
-- 14. SAMPLE PLATFORM RECORDS (per tenant)
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO platform.records (id, tenant_id, module_code, record_type, record_number, company_id, business_unit_id, site_id, department_id, location_id, data_classification_id, status, title, created_by_member_id, created_at, updated_at)
VALUES
    ('23000000-0000-0000-0000-000000000001'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, 'safety',   'observation', 'OBS-2026-0001', '22110000-0000-0000-0000-000000000001'::uuid, '22110000-0000-0000-0000-000000000011'::uuid, '22110000-0000-0000-0000-000000000021'::uuid, '22110000-0000-0000-0000-000000000031'::uuid, '22110000-0000-0000-0000-000000000041'::uuid, '13110000-0000-0000-0000-000000000002'::uuid, 'OPEN', 'Housekeeping debris near walkway', '17000000-0000-0000-0000-000000000001'::uuid, now(), now()),
    ('23000000-0000-0000-0000-000000000002'::uuid, '11111111-1111-1111-1111-111111111111'::uuid, 'incident', 'incident',    'INC-2026-0001', '22110000-0000-0000-0000-000000000001'::uuid, '22110000-0000-0000-0000-000000000011'::uuid, '22110000-0000-0000-0000-000000000021'::uuid, '22110000-0000-0000-0000-000000000031'::uuid, '22110000-0000-0000-0000-000000000041'::uuid, '13110000-0000-0000-0000-000000000003'::uuid, 'IN_PROGRESS', 'Slip on wet floor near platform', '17000000-0000-0000-0000-000000000002'::uuid, now(), now()),
    ('23000000-0000-0000-0000-000000000003'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, 'safety',   'observation', 'OBS-2026-0001', '22220000-0000-0000-0000-000000000001'::uuid, '22220000-0000-0000-0000-000000000011'::uuid, '22220000-0000-0000-0000-000000000021'::uuid, '22220000-0000-0000-0000-000000000031'::uuid, '22220000-0000-0000-0000-000000000041'::uuid, '13220000-0000-0000-0000-000000000002'::uuid, 'OPEN', 'Loose guard rail on stairway', '17000000-0000-0000-0000-000000000004'::uuid, now(), now()),
    ('23000000-0000-0000-0000-000000000004'::uuid, '22222222-2222-2222-2222-222222222222'::uuid, 'incident', 'incident',    'INC-2026-0001', '22220000-0000-0000-0000-000000000001'::uuid, '22220000-0000-0000-0000-000000000011'::uuid, '22220000-0000-0000-0000-000000000021'::uuid, '22220000-0000-0000-0000-000000000031'::uuid, '22220000-0000-0000-0000-000000000041'::uuid, '13220000-0000-0000-0000-000000000003'::uuid, 'CLOSED', 'Spill of solvent during transfer', '17000000-0000-0000-0000-000000000005'::uuid, now(), now())
ON CONFLICT (id) DO NOTHING;

-- ════════════════════════════════════════════════════════════════════════════
-- 15. WORKFLOW DEFINITIONS (per tenant, version 1)
-- ════════════════════════════════════════════════════════════════════════════
INSERT INTO platform.workflow_definitions (id, tenant_id, code, name, module_code, status)
SELECT wd.id::uuid, t.id, wd.code, wd.name, wd.module_code, wd.status
FROM (VALUES
    ('24000000-0000-0000-0000-000000000001', 'INCIDENT_CLOSURE',    'Incident Closure Workflow',    'incident', 'active'),
    ('24000000-0000-0000-0000-000000000002', 'OBSERVATION_FOLLOWUP','Observation Follow-up Workflow','safety',   'active'),
    ('24000000-0000-0000-0000-000000000003', 'CAPA_ACTION',         'CAPA Action Workflow',          'capa',     'active')
) AS wd(id, code, name, module_code, status)
CROSS JOIN saas.tenants t
ON CONFLICT (id) DO NOTHING;

INSERT INTO platform.workflow_versions (id, tenant_id, workflow_definition_id, version_number, effective_from, effective_to, status)
SELECT wv.id::uuid, t.id, wv.workflow_definition_id::uuid, wv.version_number, wv.effective_from::timestamptz, wv.effective_to::timestamptz, wv.status
FROM (VALUES
    ('24000000-0000-0000-0000-000000000011', '24000000-0000-0000-0000-000000000001', 1, '2026-01-01T00:00:00Z', NULL, 'active'),
    ('24000000-0000-0000-0000-000000000012', '24000000-0000-0000-0000-000000000002', 1, '2026-01-01T00:00:00Z', NULL, 'active'),
    ('24000000-0000-0000-0000-000000000013', '24000000-0000-0000-0000-000000000003', 1, '2026-01-01T00:00:00Z', NULL, 'active')
) AS wv(id, workflow_definition_id, version_number, effective_from, effective_to, status)
CROSS JOIN saas.tenants t
ON CONFLICT (id) DO NOTHING;