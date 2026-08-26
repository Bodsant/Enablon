-- EHSMS Initial Seed Data
-- Run after migrations: dotnet ef database update

-- ========== SCHEMAS ==========
CREATE SCHEMA IF NOT EXISTS saas;
CREATE SCHEMA IF NOT EXISTS org;
CREATE SCHEMA IF NOT EXISTS iam;
CREATE SCHEMA IF NOT EXISTS platform;
CREATE SCHEMA IF NOT EXISTS document;
CREATE SCHEMA IF NOT EXISTS safety;
CREATE SCHEMA IF NOT EXISTS risk;
CREATE SCHEMA IF NOT EXISTS incident;
CREATE SCHEMA IF NOT EXISTS capa;
CREATE SCHEMA IF NOT EXISTS cow;
CREATE SCHEMA IF NOT EXISTS inspection;
CREATE SCHEMA IF NOT EXISTS audit;
CREATE SCHEMA IF NOT EXISTS compliance;
CREATE SCHEMA IF NOT EXISTS contractor;
CREATE SCHEMA IF NOT EXISTS training;
CREATE SCHEMA IF NOT EXISTS ppe;
CREATE SCHEMA IF NOT EXISTS health;
CREATE SCHEMA IF NOT EXISTS chemical;
CREATE SCHEMA IF NOT EXISTS environment;
CREATE SCHEMA IF NOT EXISTS sustainability;
CREATE SCHEMA IF NOT EXISTS asset;
CREATE SCHEMA IF NOT EXISTS emergency;
CREATE SCHEMA IF NOT EXISTS reporting;
CREATE SCHEMA IF NOT EXISTS integration;

-- ========== SUBSCRIPTION PLANS ==========
INSERT INTO saas.subscription_plans (id, name, "MaxActiveUsers", "TotalStorageBytes", "UploadPerPeriodBytes", "IsActive", "CreatedAt", "CreatedBy")
VALUES
  ('a0000000-0000-0000-0000-000000000001', 'Regular', 10, 1073741824, 524288000, true, NOW(), 'system'),
  ('a0000000-0000-0000-0000-000000000002', 'Advance', 30, 5368709120, 2147483648, true, NOW(), 'system'),
  ('a0000000-0000-0000-0000-000000000003', 'Premium', 100, 21474836480, 10737418240, true, NOW(), 'system');

-- ========== DEFAULT TENANT (Demo) ==========
INSERT INTO saas.tenants (id, name, "IsActive", "CreatedAt", "CreatedBy")
VALUES ('b0000000-0000-0000-0000-000000000001', 'Demo Tenant', true, NOW(), 'system');

-- ========== DEFAULT PERMISSIONS ==========
INSERT INTO iam.permissions (id, "Code", "Module", "Description")
VALUES
  -- Platform
  ('c0000000-0000-0000-0000-000000000001', 'platform.read', 'Platform', 'Read platform records'),
  ('c0000000-0000-0000-0000-000000000002', 'platform.manage', 'Platform', 'Manage platform settings'),
  -- Incident
  ('c0000000-0000-0000-0000-000000000010', 'incident.create', 'Incident', 'Create incidents'),
  ('c0000000-0000-0000-0000-000000000011', 'incident.read', 'Incident', 'Read incidents'),
  ('c0000000-0000-0000-0000-000000000012', 'incident.investigate', 'Incident', 'Investigate incidents'),
  -- CAPA
  ('c0000000-0000-0000-0000-000000000020', 'capa.create', 'CAPA', 'Create CAPAs'),
  ('c0000000-0000-0000-0000-000000000021', 'capa.read', 'CAPA', 'Read CAPAs'),
  ('c0000000-0000-0000-0000-000000000022', 'capa.verify', 'CAPA', 'Verify CAPAs'),
  -- Risk
  ('c0000000-0000-0000-0000-000000000030', 'risk.create', 'Risk', 'Create risk assessments'),
  ('c0000000-0000-0000-0000-000000000031', 'risk.read', 'Risk', 'Read risk assessments'),
  ('c0000000-0000-0000-0000-000000000032', 'risk.accept', 'Risk', 'Accept risk assessments'),
  -- Inspection
  ('c0000000-0000-0000-0000-000000000040', 'inspection.create', 'Inspection', 'Create inspections'),
  ('c0000000-0000-0000-0000-000000000041', 'inspection.read', 'Inspection', 'Read inspections'),
  -- Permit (PTW)
  ('c0000000-0000-0000-0000-000000000050', 'permit.create', 'Control of Work', 'Create permits'),
  ('c0000000-0000-0000-0000-000000000051', 'permit.activate', 'Control of Work', 'Activate permits'),
  -- Admin
  ('c0000000-0000-0000-0000-000000000090', 'admin.users', 'Admin', 'Manage users'),
  ('c0000000-0000-0000-0000-000000000091', 'admin.roles', 'Admin', 'Manage roles'),
  ('c0000000-0000-0000-0000-000000000092', 'admin.workflows', 'Admin', 'Manage workflows'),
  ('c0000000-0000-0000-0000-000000000093', 'admin.lookups', 'Admin', 'Manage lookup values');

-- ========== DEFAULT ROLES ==========
INSERT INTO iam.roles (id, name, "IsSystem", "CreatedAt")
VALUES
  ('d0000000-0000-0000-0000-000000000001', 'System Admin', true, NOW()),
  ('d0000000-0000-0000-0000-000000000002', 'HSE Manager', true, NOW()),
  ('d0000000-0000-0000-0000-000000000003', 'HSE Officer', true, NOW()),
  ('d0000000-0000-0000-0000-000000000004', 'Supervisor', true, NOW()),
  ('d0000000-0000-0000-0000-000000000005', 'Worker', true, NOW()),
  ('d0000000-0000-0000-0000-000000000006', 'Auditor', true, NOW());

-- ========== DATA CLASSIFICATIONS ==========
INSERT INTO platform.data_classifications (id, "Code", "Label", "Description", "IsGlobal")
VALUES
  ('e0000000-0000-0000-0000-000000000001', 'PUBLIC', 'Public', 'Publicly accessible', true),
  ('e0000000-0000-0000-0000-000000000002', 'INTERNAL', 'Internal', 'Internal use only', true),
  ('e0000000-0000-0000-0000-000000000003', 'CONFIDENTIAL', 'Confidential', 'Sensitive business data', true),
  ('e0000000-0000-0000-0000-000000000004', 'RESTRICTED', 'Restricted', 'Highly sensitive (health, personal)', true);

-- ========== DEFAULT LOOKUP VALUES ==========
INSERT INTO platform.lookup_values (id, "TenantId", "Category", "Code", "Label", "SortOrder", "IsActive", "CreatedAt", "CreatedBy")
VALUES
  ('f0000000-0000-0000-0000-000000000001', 'b0000000-0000-0000-0000-000000000001', 'SEVERITY', 'LOW', 'Low', 1, true, NOW(), 'system'),
  ('f0000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000001', 'SEVERITY', 'MEDIUM', 'Medium', 2, true, NOW(), 'system'),
  ('f0000000-0000-0000-0000-000000000003', 'b0000000-0000-0000-0000-000000000001', 'SEVERITY', 'HIGH', 'High', 3, true, NOW(), 'system'),
  ('f0000000-0000-0000-0000-000000000004', 'b0000000-0000-0000-0000-000000000001', 'SEVERITY', 'CRITICAL', 'Critical', 4, true, NOW(), 'system'),
  ('f0000000-0000-0000-0000-000000000010', 'b0000000-0000-0000-0000-000000000001', 'PRIORITY', 'LOW', 'Low', 1, true, NOW(), 'system'),
  ('f0000000-0000-0000-0000-000000000011', 'b0000000-0000-0000-0000-000000000001', 'PRIORITY', 'MEDIUM', 'Medium', 2, true, NOW(), 'system'),
  ('f0000000-0000-0000-0000-000000000012', 'b0000000-0000-0000-0000-000000000001', 'PRIORITY', 'HIGH', 'High', 3, true, NOW(), 'system'),
  ('f0000000-0000-0000-0000-000000000013', 'b0000000-0000-0000-0000-000000000001', 'PRIORITY', 'URGENT', 'Urgent', 4, true, NOW(), 'system');
