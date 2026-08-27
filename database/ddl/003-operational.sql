-- ============================================================================
-- 003-operational.sql — Wave 3: Operational Schemas (cow, inspection, audit)
-- PostgreSQL 18+  ·  Neon
-- Generated from database/ehsms-erd.dbml
-- ============================================================================
-- VOID-guard: skip if schemas already exist
DO $$ BEGIN
  PERFORM 1 FROM pg_namespace WHERE nspname = 'cow';
  IF FOUND THEN RAISE NOTICE 'cow schema already exists — skipping creation'; END IF;
EXCEPTION WHEN OTHERS THEN NULL;
END $$;

-- ============================================================================
-- SCHEMA CREATION
-- ============================================================================
CREATE SCHEMA IF NOT EXISTS cow;
CREATE SCHEMA IF NOT EXISTS inspection;
CREATE SCHEMA IF NOT EXISTS audit;


-- ############################################################################
-- COW SCHEMA  — Control of Work  (21 tables)
-- ############################################################################

-- ============================================================================
-- cow.work_requests
-- ============================================================================
CREATE TABLE cow.work_requests (
    id                    uuid        PRIMARY KEY,
    tenant_id             uuid        NOT NULL,
    record_id             uuid        NOT NULL UNIQUE,
    requester_member_id   uuid        NOT NULL,
    work_description      text        NOT NULL,
    contractor_company_id uuid,
    planned_start         timestamptz,
    planned_end           timestamptz,
    work_type             varchar(60) NOT NULL
);

ALTER TABLE cow.work_requests
    ADD CONSTRAINT fk_work_requests_tenant       FOREIGN KEY (tenant_id)             REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_work_requests_record       FOREIGN KEY (record_id)             REFERENCES platform.records (id),
    ADD CONSTRAINT fk_work_requests_requester    FOREIGN KEY (requester_member_id)   REFERENCES iam.tenant_members (id);
-- FK to contractor.companies deferred to after wave 4 (see 005-deferred-fks.sql)

CREATE INDEX idx_work_requests_tenant_id ON cow.work_requests (tenant_id);

-- ============================================================================
-- cow.jsa_templates
-- ============================================================================
CREATE TABLE cow.jsa_templates (
    id                uuid         PRIMARY KEY,
    tenant_id         uuid         NOT NULL,
    code              varchar(50)  NOT NULL,
    name              varchar(200) NOT NULL,
    owner_member_id   uuid         NOT NULL,
    status            varchar(20)  NOT NULL
);

ALTER TABLE cow.jsa_templates
    ADD CONSTRAINT fk_jsa_templates_tenant   FOREIGN KEY (tenant_id)       REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_jsa_templates_owner    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members (id);

CREATE INDEX idx_jsa_templates_tenant_id ON cow.jsa_templates (tenant_id);

-- ============================================================================
-- cow.jsa_template_versions
-- ============================================================================
CREATE TABLE cow.jsa_template_versions (
    id                uuid         PRIMARY KEY,
    tenant_id         uuid         NOT NULL,
    jsa_template_id   uuid         NOT NULL,
    version_number    int          NOT NULL,
    site_id           uuid,
    effective_from    date,
    status            varchar(20)  NOT NULL
);

ALTER TABLE cow.jsa_template_versions
    ADD CONSTRAINT fk_jsa_template_versions_tenant   FOREIGN KEY (tenant_id)       REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_jsa_template_versions_template FOREIGN KEY (jsa_template_id) REFERENCES cow.jsa_templates (id),
    ADD CONSTRAINT fk_jsa_template_versions_site     FOREIGN KEY (site_id)         REFERENCES org.sites (id);

CREATE INDEX idx_jsa_template_versions_tenant_id ON cow.jsa_template_versions (tenant_id);

-- ============================================================================
-- cow.jsa_template_steps
-- ============================================================================
CREATE TABLE cow.jsa_template_steps (
    id                        uuid         PRIMARY KEY,
    tenant_id                 uuid         NOT NULL,
    jsa_template_version_id   uuid         NOT NULL,
    sequence_number           int          NOT NULL,
    work_step                 text         NOT NULL
);

ALTER TABLE cow.jsa_template_steps
    ADD CONSTRAINT fk_jsa_template_steps_tenant   FOREIGN KEY (tenant_id)                 REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_jsa_template_steps_version  FOREIGN KEY (jsa_template_version_id)   REFERENCES cow.jsa_template_versions (id);

CREATE INDEX idx_jsa_template_steps_tenant_id ON cow.jsa_template_steps (tenant_id);

-- ============================================================================
-- cow.jsas
-- ============================================================================
CREATE TABLE cow.jsas (
    id                        uuid         PRIMARY KEY,
    tenant_id                 uuid         NOT NULL,
    record_id                 uuid         NOT NULL UNIQUE,
    work_request_id           uuid         NOT NULL,
    template_version_id       uuid,
    prepared_by_member_id     uuid         NOT NULL,
    status                    varchar(30)  NOT NULL,
    overall_residual_risk     varchar(30)
);

ALTER TABLE cow.jsas
    ADD CONSTRAINT fk_jsas_tenant      FOREIGN KEY (tenant_id)           REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_jsas_record      FOREIGN KEY (record_id)           REFERENCES platform.records (id),
    ADD CONSTRAINT fk_jsas_work_req    FOREIGN KEY (work_request_id)     REFERENCES cow.work_requests (id),
    ADD CONSTRAINT fk_jsas_template    FOREIGN KEY (template_version_id) REFERENCES cow.jsa_template_versions (id),
    ADD CONSTRAINT fk_jsas_prepared_by FOREIGN KEY (prepared_by_member_id) REFERENCES iam.tenant_members (id);

CREATE INDEX idx_jsas_tenant_id ON cow.jsas (tenant_id);

-- ============================================================================
-- cow.jsa_steps
-- ============================================================================
CREATE TABLE cow.jsa_steps (
    id                uuid        PRIMARY KEY,
    tenant_id         uuid        NOT NULL,
    jsa_id            uuid        NOT NULL,
    sequence_number   int         NOT NULL,
    work_step         text        NOT NULL
);

ALTER TABLE cow.jsa_steps
    ADD CONSTRAINT fk_jsa_steps_tenant FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_jsa_steps_jsa    FOREIGN KEY (jsa_id)    REFERENCES cow.jsas (id);

CREATE INDEX idx_jsa_steps_tenant_id ON cow.jsa_steps (tenant_id);

-- ============================================================================
-- cow.jsa_step_hazards
-- ============================================================================
CREATE TABLE cow.jsa_step_hazards (
    id                      uuid         PRIMARY KEY,
    tenant_id               uuid         NOT NULL,
    jsa_step_id             uuid         NOT NULL,
    hazard_id               uuid,
    consequence             text         NOT NULL,
    existing_control        text,
    additional_control      text,
    initial_risk_level      varchar(30),
    residual_risk_level     varchar(30),
    responsible_member_id   uuid
);

ALTER TABLE cow.jsa_step_hazards
    ADD CONSTRAINT fk_jsa_step_hazards_tenant       FOREIGN KEY (tenant_id)             REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_jsa_step_hazards_step         FOREIGN KEY (jsa_step_id)           REFERENCES cow.jsa_steps (id),
    ADD CONSTRAINT fk_jsa_step_hazards_hazard       FOREIGN KEY (hazard_id)             REFERENCES risk.hazards (id),
    ADD CONSTRAINT fk_jsa_step_hazards_responsible  FOREIGN KEY (responsible_member_id) REFERENCES iam.tenant_members (id);

CREATE INDEX idx_jsa_step_hazards_tenant_id ON cow.jsa_step_hazards (tenant_id);

-- ============================================================================
-- cow.permit_types
-- ============================================================================
CREATE TABLE cow.permit_types (
    id                uuid         PRIMARY KEY,
    tenant_id         uuid         NOT NULL,
    code              varchar(50)  NOT NULL,
    name              varchar(150) NOT NULL,
    risk_category     varchar(40),
    status            varchar(20)  NOT NULL
);

ALTER TABLE cow.permit_types
    ADD CONSTRAINT fk_permit_types_tenant FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id);

CREATE INDEX idx_permit_types_tenant_id ON cow.permit_types (tenant_id);

-- ============================================================================
-- cow.permit_type_versions
-- ============================================================================
CREATE TABLE cow.permit_type_versions (
    id                  uuid         PRIMARY KEY,
    tenant_id           uuid         NOT NULL,
    permit_type_id      uuid         NOT NULL,
    version_number      int          NOT NULL,
    effective_from      date         NOT NULL,
    effective_to        date,
    configuration_json  jsonb,
    status              varchar(20)  NOT NULL
);

ALTER TABLE cow.permit_type_versions
    ADD CONSTRAINT fk_permit_type_versions_tenant  FOREIGN KEY (tenant_id)      REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_permit_type_versions_type    FOREIGN KEY (permit_type_id) REFERENCES cow.permit_types (id);

CREATE INDEX idx_permit_type_versions_tenant_id ON cow.permit_type_versions (tenant_id);

-- ============================================================================
-- cow.permit_checklist_items
-- ============================================================================
CREATE TABLE cow.permit_checklist_items (
    id                        uuid         PRIMARY KEY,
    tenant_id                 uuid         NOT NULL,
    permit_type_version_id    uuid         NOT NULL,
    sequence_number           int          NOT NULL,
    prompt                    text         NOT NULL,
    is_mandatory              boolean      NOT NULL,
    validation_type           varchar(30)
);

ALTER TABLE cow.permit_checklist_items
    ADD CONSTRAINT fk_permit_checklist_items_tenant  FOREIGN KEY (tenant_id)              REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_permit_checklist_items_version FOREIGN KEY (permit_type_version_id) REFERENCES cow.permit_type_versions (id);

CREATE INDEX idx_permit_checklist_items_tenant_id ON cow.permit_checklist_items (tenant_id);

-- ============================================================================
-- cow.permits
-- ============================================================================
CREATE TABLE cow.permits (
    id                        uuid         PRIMARY KEY,
    tenant_id                 uuid         NOT NULL,
    record_id                 uuid         NOT NULL UNIQUE,
    work_request_id           uuid         NOT NULL,
    jsa_id                    uuid,
    permit_type_version_id    uuid         NOT NULL,
    requester_member_id       uuid         NOT NULL,
    executor_person_id        uuid,
    contractor_company_id     uuid,
    valid_from                timestamptz,
    valid_until               timestamptz,
    suspension_reason         text,
    extension_count           int          NOT NULL
);

ALTER TABLE cow.permits
    ADD CONSTRAINT fk_permits_tenant       FOREIGN KEY (tenant_id)             REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_permits_record       FOREIGN KEY (record_id)             REFERENCES platform.records (id),
    ADD CONSTRAINT fk_permits_work_req     FOREIGN KEY (work_request_id)       REFERENCES cow.work_requests (id),
    ADD CONSTRAINT fk_permits_jsa          FOREIGN KEY (jsa_id)                REFERENCES cow.jsas (id),
    ADD CONSTRAINT fk_permits_type_version FOREIGN KEY (permit_type_version_id) REFERENCES cow.permit_type_versions (id),
    ADD CONSTRAINT fk_permits_requester    FOREIGN KEY (requester_member_id)   REFERENCES iam.tenant_members (id),
    ADD CONSTRAINT fk_permits_executor     FOREIGN KEY (executor_person_id)    REFERENCES org.people (id);
-- FK to contractor.companies deferred to after wave 4 (see 005-deferred-fks.sql)

CREATE INDEX idx_permits_tenant_id ON cow.permits (tenant_id);

-- ============================================================================
-- cow.permit_workers
-- ============================================================================
CREATE TABLE cow.permit_workers (
    id                    uuid         PRIMARY KEY,
    tenant_id             uuid         NOT NULL,
    permit_id             uuid         NOT NULL,
    person_id             uuid         NOT NULL,
    work_role             varchar(60),
    eligibility_status    varchar(30)  NOT NULL
);

ALTER TABLE cow.permit_workers
    ADD CONSTRAINT fk_permit_workers_tenant FOREIGN KEY (tenant_id)   REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_permit_workers_permit FOREIGN KEY (permit_id)   REFERENCES cow.permits (id),
    ADD CONSTRAINT fk_permit_workers_person FOREIGN KEY (person_id)   REFERENCES org.people (id);

CREATE INDEX idx_permit_workers_tenant_id ON cow.permit_workers (tenant_id);

-- ============================================================================
-- cow.permit_checklist_responses
-- ============================================================================
CREATE TABLE cow.permit_checklist_responses (
    id                    uuid         PRIMARY KEY,
    tenant_id             uuid         NOT NULL,
    permit_id             uuid         NOT NULL,
    checklist_item_id     uuid         NOT NULL,
    response_json         jsonb,
    is_satisfied          boolean,
    checked_by_member_id  uuid,
    checked_at            timestamptz
);

ALTER TABLE cow.permit_checklist_responses
    ADD CONSTRAINT fk_pcr_tenant  FOREIGN KEY (tenant_id)           REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_pcr_permit  FOREIGN KEY (permit_id)           REFERENCES cow.permits (id),
    ADD CONSTRAINT fk_pcr_item    FOREIGN KEY (checklist_item_id)   REFERENCES cow.permit_checklist_items (id),
    ADD CONSTRAINT fk_pcr_checked FOREIGN KEY (checked_by_member_id) REFERENCES iam.tenant_members (id);

CREATE INDEX idx_permit_checklist_responses_tenant_id ON cow.permit_checklist_responses (tenant_id);

-- ============================================================================
-- cow.permit_approvals
-- ============================================================================
CREATE TABLE cow.permit_approvals (
    id                    uuid         PRIMARY KEY,
    tenant_id             uuid         NOT NULL,
    permit_id             uuid         NOT NULL,
    workflow_task_id      uuid         NOT NULL,
    approval_level        int          NOT NULL,
    decision              varchar(30),
    approver_member_id    uuid,
    decided_at            timestamptz
);

ALTER TABLE cow.permit_approvals
    ADD CONSTRAINT fk_permit_approvals_tenant   FOREIGN KEY (tenant_id)          REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_permit_approvals_permit   FOREIGN KEY (permit_id)          REFERENCES cow.permits (id),
    ADD CONSTRAINT fk_permit_approvals_task     FOREIGN KEY (workflow_task_id)   REFERENCES platform.workflow_tasks (id),
    ADD CONSTRAINT fk_permit_approvals_approver FOREIGN KEY (approver_member_id) REFERENCES iam.tenant_members (id);

CREATE INDEX idx_permit_approvals_tenant_id ON cow.permit_approvals (tenant_id);

-- ============================================================================
-- cow.gas_tests
-- ============================================================================
CREATE TABLE cow.gas_tests (
    id                uuid          PRIMARY KEY,
    tenant_id         uuid          NOT NULL,
    permit_id         uuid          NOT NULL,
    test_type         varchar(50)   NOT NULL,
    tested_at         timestamptz   NOT NULL,
    tested_by_person_id uuid,
    oxygen_pct        decimal(6,3),
    lel_pct           decimal(6,3),
    toxic_gas_json    jsonb,
    result            varchar(30)   NOT NULL
);

ALTER TABLE cow.gas_tests
    ADD CONSTRAINT fk_gas_tests_tenant  FOREIGN KEY (tenant_id)          REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_gas_tests_permit  FOREIGN KEY (permit_id)          REFERENCES cow.permits (id),
    ADD CONSTRAINT fk_gas_tests_tester  FOREIGN KEY (tested_by_person_id) REFERENCES org.people (id);

CREATE INDEX idx_gas_tests_tenant_id ON cow.gas_tests (tenant_id);

-- ============================================================================
-- cow.work_executions
-- ============================================================================
CREATE TABLE cow.work_executions (
    id                    uuid         PRIMARY KEY,
    tenant_id             uuid         NOT NULL,
    permit_id             uuid         NOT NULL,
    started_at            timestamptz,
    completed_at          timestamptz,
    execution_status      varchar(30)  NOT NULL,
    completion_notes      text
);

ALTER TABLE cow.work_executions
    ADD CONSTRAINT fk_work_executions_tenant FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_work_executions_permit FOREIGN KEY (permit_id) REFERENCES cow.permits (id);

CREATE INDEX idx_work_executions_tenant_id ON cow.work_executions (tenant_id);

-- ============================================================================
-- cow.work_monitoring
-- ============================================================================
CREATE TABLE cow.work_monitoring (
    id                    uuid         PRIMARY KEY,
    tenant_id             uuid         NOT NULL,
    work_execution_id     uuid         NOT NULL,
    monitored_by_member_id uuid        NOT NULL,
    monitored_at          timestamptz  NOT NULL,
    condition_status      varchar(30)  NOT NULL,
    notes                 text
);

ALTER TABLE cow.work_monitoring
    ADD CONSTRAINT fk_work_monitoring_tenant    FOREIGN KEY (tenant_id)             REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_work_monitoring_execution FOREIGN KEY (work_execution_id)     REFERENCES cow.work_executions (id),
    ADD CONSTRAINT fk_work_monitoring_member    FOREIGN KEY (monitored_by_member_id) REFERENCES iam.tenant_members (id);

CREATE INDEX idx_work_monitoring_tenant_id ON cow.work_monitoring (tenant_id);

-- ============================================================================
-- cow.isolation_plans
-- ============================================================================
CREATE TABLE cow.isolation_plans (
    id                    uuid         PRIMARY KEY,
    tenant_id             uuid         NOT NULL,
    record_id             uuid         NOT NULL UNIQUE,
    permit_id             uuid         NOT NULL,
    prepared_by_member_id uuid         NOT NULL,
    status                varchar(30)  NOT NULL
);

ALTER TABLE cow.isolation_plans
    ADD CONSTRAINT fk_isolation_plans_tenant   FOREIGN KEY (tenant_id)             REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_isolation_plans_record   FOREIGN KEY (record_id)             REFERENCES platform.records (id),
    ADD CONSTRAINT fk_isolation_plans_permit   FOREIGN KEY (permit_id)             REFERENCES cow.permits (id),
    ADD CONSTRAINT fk_isolation_plans_prepared FOREIGN KEY (prepared_by_member_id) REFERENCES iam.tenant_members (id);

CREATE INDEX idx_isolation_plans_tenant_id ON cow.isolation_plans (tenant_id);

-- ============================================================================
-- cow.isolation_points
-- ============================================================================
CREATE TABLE cow.isolation_points (
    id                    uuid          PRIMARY KEY,
    tenant_id             uuid          NOT NULL,
    isolation_plan_id     uuid          NOT NULL,
    asset_id              uuid,
    energy_source         varchar(80)   NOT NULL,
    isolation_method      varchar(100)  NOT NULL,
    point_identifier      varchar(100)  NOT NULL
);

ALTER TABLE cow.isolation_points
    ADD CONSTRAINT fk_isolation_points_tenant FOREIGN KEY (tenant_id)         REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_isolation_points_plan   FOREIGN KEY (isolation_plan_id) REFERENCES cow.isolation_plans (id),
    ADD CONSTRAINT fk_isolation_points_asset  FOREIGN KEY (asset_id)          REFERENCES asset.assets (id);

CREATE INDEX idx_isolation_points_tenant_id ON cow.isolation_points (tenant_id);

-- ============================================================================
-- cow.isolation_locks
-- ============================================================================
CREATE TABLE cow.isolation_locks (
    id                    uuid          PRIMARY KEY,
    tenant_id             uuid          NOT NULL,
    isolation_point_id    uuid          NOT NULL,
    lock_identifier       varchar(100)  NOT NULL,
    tag_identifier        varchar(100),
    applied_by_person_id  uuid          NOT NULL,
    applied_at            timestamptz   NOT NULL,
    removed_by_person_id  uuid,
    removed_at            timestamptz
);

ALTER TABLE cow.isolation_locks
    ADD CONSTRAINT fk_isolation_locks_tenant   FOREIGN KEY (tenant_id)            REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_isolation_locks_point    FOREIGN KEY (isolation_point_id)   REFERENCES cow.isolation_points (id),
    ADD CONSTRAINT fk_isolation_locks_applied  FOREIGN KEY (applied_by_person_id) REFERENCES org.people (id),
    ADD CONSTRAINT fk_isolation_locks_removed  FOREIGN KEY (removed_by_person_id) REFERENCES org.people (id);

CREATE INDEX idx_isolation_locks_tenant_id ON cow.isolation_locks (tenant_id);

-- ============================================================================
-- cow.isolation_verifications
-- ============================================================================
CREATE TABLE cow.isolation_verifications (
    id                    uuid          PRIMARY KEY,
    tenant_id             uuid          NOT NULL,
    isolation_point_id    uuid          NOT NULL,
    verification_type     varchar(50)   NOT NULL,
    result                varchar(30)   NOT NULL,
    verified_by_person_id uuid          NOT NULL,
    verified_at           timestamptz   NOT NULL,
    comment               text
);

ALTER TABLE cow.isolation_verifications
    ADD CONSTRAINT fk_isolation_verifs_tenant   FOREIGN KEY (tenant_id)            REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_isolation_verifs_point    FOREIGN KEY (isolation_point_id)   REFERENCES cow.isolation_points (id),
    ADD CONSTRAINT fk_isolation_verifs_verifier FOREIGN KEY (verified_by_person_id) REFERENCES org.people (id);

CREATE INDEX idx_isolation_verifications_tenant_id ON cow.isolation_verifications (tenant_id);


-- ############################################################################
-- INSPECTION SCHEMA  (8 tables)
-- ############################################################################

-- ============================================================================
-- inspection.templates
-- ============================================================================
CREATE TABLE inspection.templates (
    id                uuid         PRIMARY KEY,
    tenant_id         uuid         NOT NULL,
    code              varchar(50)  NOT NULL,
    name              varchar(200) NOT NULL,
    inspection_type   varchar(60),
    owner_member_id   uuid         NOT NULL,
    status            varchar(20)  NOT NULL
);

ALTER TABLE inspection.templates
    ADD CONSTRAINT fk_inspection_templates_tenant FOREIGN KEY (tenant_id)       REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_inspection_templates_owner  FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members (id);

CREATE INDEX idx_inspection_templates_tenant_id ON inspection.templates (tenant_id);

-- ============================================================================
-- inspection.template_versions
-- ============================================================================
CREATE TABLE inspection.template_versions (
    id                uuid         PRIMARY KEY,
    tenant_id         uuid         NOT NULL,
    template_id       uuid         NOT NULL,
    version_number    int          NOT NULL,
    effective_from    date,
    status            varchar(20)  NOT NULL
);

ALTER TABLE inspection.template_versions
    ADD CONSTRAINT fk_inspection_tv_tenant   FOREIGN KEY (tenant_id)   REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_inspection_tv_template FOREIGN KEY (template_id) REFERENCES inspection.templates (id);

CREATE INDEX idx_inspection_template_versions_tenant_id ON inspection.template_versions (tenant_id);

-- ============================================================================
-- inspection.template_sections
-- ============================================================================
CREATE TABLE inspection.template_sections (
    id                    uuid         PRIMARY KEY,
    tenant_id             uuid         NOT NULL,
    template_version_id   uuid         NOT NULL,
    title                 varchar(200) NOT NULL,
    sequence_number       int          NOT NULL
);

ALTER TABLE inspection.template_sections
    ADD CONSTRAINT fk_inspection_ts_tenant FOREIGN KEY (tenant_id)             REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_inspection_ts_version FOREIGN KEY (template_version_id)  REFERENCES inspection.template_versions (id);

CREATE INDEX idx_inspection_template_sections_tenant_id ON inspection.template_sections (tenant_id);

-- ============================================================================
-- inspection.template_items
-- ============================================================================
CREATE TABLE inspection.template_items (
    id                uuid         PRIMARY KEY,
    tenant_id         uuid         NOT NULL,
    section_id        uuid         NOT NULL,
    item_code         varchar(50)  NOT NULL,
    prompt            text         NOT NULL,
    response_type     varchar(30)  NOT NULL,
    is_required       boolean      NOT NULL,
    weight            decimal(10,2),
    criteria_json     jsonb,
    sequence_number   int          NOT NULL
);

ALTER TABLE inspection.template_items
    ADD CONSTRAINT fk_inspection_ti_tenant  FOREIGN KEY (tenant_id)  REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_inspection_ti_section FOREIGN KEY (section_id) REFERENCES inspection.template_sections (id);

CREATE INDEX idx_inspection_template_items_tenant_id ON inspection.template_items (tenant_id);

-- ============================================================================
-- inspection.schedules
-- ============================================================================
CREATE TABLE inspection.schedules (
    id                    uuid          PRIMARY KEY,
    tenant_id             uuid          NOT NULL,
    template_version_id   uuid          NOT NULL,
    site_id               uuid          NOT NULL,
    location_id           uuid,
    assigned_member_id    uuid,
    recurrence_rule       varchar(300),
    next_execution_at     timestamptz,
    status                varchar(20)   NOT NULL
);

ALTER TABLE inspection.schedules
    ADD CONSTRAINT fk_inspection_schedules_tenant   FOREIGN KEY (tenant_id)           REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_inspection_schedules_version  FOREIGN KEY (template_version_id) REFERENCES inspection.template_versions (id),
    ADD CONSTRAINT fk_inspection_schedules_site     FOREIGN KEY (site_id)             REFERENCES org.sites (id),
    ADD CONSTRAINT fk_inspection_schedules_location FOREIGN KEY (location_id)         REFERENCES org.locations (id),
    ADD CONSTRAINT fk_inspection_schedules_assigned FOREIGN KEY (assigned_member_id)  REFERENCES iam.tenant_members (id);

CREATE INDEX idx_inspection_schedules_tenant_id ON inspection.schedules (tenant_id);

-- ============================================================================
-- inspection.inspections
-- ============================================================================
CREATE TABLE inspection.inspections (
    id                      uuid          PRIMARY KEY,
    tenant_id               uuid          NOT NULL,
    record_id               uuid          NOT NULL UNIQUE,
    schedule_id             uuid,
    template_version_id     uuid          NOT NULL,
    inspector_member_id     uuid          NOT NULL,
    planned_at              timestamptz,
    started_at              timestamptz,
    completed_at            timestamptz,
    compliance_percentage   decimal(5,2)
);

ALTER TABLE inspection.inspections
    ADD CONSTRAINT fk_inspections_tenant   FOREIGN KEY (tenant_id)           REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_inspections_record   FOREIGN KEY (record_id)           REFERENCES platform.records (id),
    ADD CONSTRAINT fk_inspections_schedule FOREIGN KEY (schedule_id)         REFERENCES inspection.schedules (id),
    ADD CONSTRAINT fk_inspections_version  FOREIGN KEY (template_version_id) REFERENCES inspection.template_versions (id),
    ADD CONSTRAINT fk_inspections_inspector FOREIGN KEY (inspector_member_id) REFERENCES iam.tenant_members (id);

CREATE INDEX idx_inspections_tenant_id ON inspection.inspections (tenant_id);

-- ============================================================================
-- inspection.responses
-- ============================================================================
CREATE TABLE inspection.responses (
    id                      uuid         PRIMARY KEY,
    tenant_id               uuid         NOT NULL,
    inspection_id           uuid         NOT NULL,
    template_item_id        uuid         NOT NULL,
    response_json           jsonb,
    compliance_status       varchar(30),
    score                   decimal(10,2),
    comment                 text,
    answered_by_member_id   uuid         NOT NULL
);

ALTER TABLE inspection.responses
    ADD CONSTRAINT fk_inspection_responses_tenant   FOREIGN KEY (tenant_id)            REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_inspection_responses_insp     FOREIGN KEY (inspection_id)        REFERENCES inspection.inspections (id),
    ADD CONSTRAINT fk_inspection_responses_item     FOREIGN KEY (template_item_id)     REFERENCES inspection.template_items (id),
    ADD CONSTRAINT fk_inspection_responses_answered FOREIGN KEY (answered_by_member_id) REFERENCES iam.tenant_members (id);

CREATE INDEX idx_inspection_responses_tenant_id ON inspection.responses (tenant_id);

-- ============================================================================
-- inspection.findings
-- ============================================================================
CREATE TABLE inspection.findings (
    id                    uuid         PRIMARY KEY,
    tenant_id             uuid         NOT NULL,
    record_id             uuid         NOT NULL UNIQUE,
    inspection_id         uuid         NOT NULL,
    response_id           uuid,
    classification        varchar(40),
    severity_id           uuid,
    description           text         NOT NULL,
    owner_member_id       uuid
);

ALTER TABLE inspection.findings
    ADD CONSTRAINT fk_inspection_findings_tenant  FOREIGN KEY (tenant_id)        REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_inspection_findings_record  FOREIGN KEY (record_id)        REFERENCES platform.records (id),
    ADD CONSTRAINT fk_inspection_findings_insp    FOREIGN KEY (inspection_id)    REFERENCES inspection.inspections (id),
    ADD CONSTRAINT fk_inspection_findings_resp    FOREIGN KEY (response_id)      REFERENCES inspection.responses (id),
    ADD CONSTRAINT fk_inspection_findings_sev     FOREIGN KEY (severity_id)      REFERENCES platform.lookup_values (id),
    ADD CONSTRAINT fk_inspection_findings_owner   FOREIGN KEY (owner_member_id)  REFERENCES iam.tenant_members (id);

CREATE INDEX idx_inspection_findings_tenant_id ON inspection.findings (tenant_id);


-- ############################################################################
-- AUDIT SCHEMA  (7 tables)
-- ############################################################################

-- ============================================================================
-- audit.programs
-- ============================================================================
CREATE TABLE audit.programs (
    id                uuid         PRIMARY KEY,
    tenant_id         uuid         NOT NULL,
    record_id         uuid         NOT NULL UNIQUE,
    name              varchar(200) NOT NULL,
    period_start      date,
    period_end        date,
    owner_member_id   uuid         NOT NULL,
    status            varchar(30)  NOT NULL
);

ALTER TABLE audit.programs
    ADD CONSTRAINT fk_audit_programs_tenant FOREIGN KEY (tenant_id)       REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_audit_programs_record FOREIGN KEY (record_id)       REFERENCES platform.records (id),
    ADD CONSTRAINT fk_audit_programs_owner  FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members (id);

CREATE INDEX idx_audit_programs_tenant_id ON audit.programs (tenant_id);

-- ============================================================================
-- audit.checklist_templates
-- ============================================================================
CREATE TABLE audit.checklist_templates (
    id                    uuid         PRIMARY KEY,
    tenant_id             uuid         NOT NULL,
    code                  varchar(50)  NOT NULL,
    name                  varchar(200) NOT NULL,
    standard_reference    varchar(200),
    version_number        int          NOT NULL,
    status                varchar(20)  NOT NULL
);

ALTER TABLE audit.checklist_templates
    ADD CONSTRAINT fk_audit_checklist_templates_tenant FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id);

CREATE INDEX idx_audit_checklist_templates_tenant_id ON audit.checklist_templates (tenant_id);

-- ============================================================================
-- audit.checklist_items
-- ============================================================================
CREATE TABLE audit.checklist_items (
    id                          uuid         PRIMARY KEY,
    tenant_id                   uuid         NOT NULL,
    checklist_template_id       uuid         NOT NULL,
    sequence_number             int          NOT NULL,
    requirement_reference       varchar(200),
    prompt                      text         NOT NULL,
    classification_rule_json    jsonb
);

ALTER TABLE audit.checklist_items
    ADD CONSTRAINT fk_audit_checklist_items_tenant    FOREIGN KEY (tenant_id)             REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_audit_checklist_items_template  FOREIGN KEY (checklist_template_id) REFERENCES audit.checklist_templates (id);

CREATE INDEX idx_audit_checklist_items_tenant_id ON audit.checklist_items (tenant_id);

-- ============================================================================
-- audit.audits
-- ============================================================================
CREATE TABLE audit.audits (
    id                        uuid         PRIMARY KEY,
    tenant_id                 uuid         NOT NULL,
    record_id                 uuid         NOT NULL UNIQUE,
    audit_program_id          uuid,
    checklist_template_id     uuid,
    audit_type                varchar(50)  NOT NULL,
    scope_text                text         NOT NULL,
    criteria_text             text,
    lead_auditor_member_id    uuid         NOT NULL,
    scheduled_start           date,
    scheduled_end             date
);

ALTER TABLE audit.audits
    ADD CONSTRAINT fk_audits_tenant     FOREIGN KEY (tenant_id)              REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_audits_record     FOREIGN KEY (record_id)              REFERENCES platform.records (id),
    ADD CONSTRAINT fk_audits_program    FOREIGN KEY (audit_program_id)       REFERENCES audit.programs (id),
    ADD CONSTRAINT fk_audits_checklist  FOREIGN KEY (checklist_template_id)  REFERENCES audit.checklist_templates (id),
    ADD CONSTRAINT fk_audits_lead       FOREIGN KEY (lead_auditor_member_id) REFERENCES iam.tenant_members (id);

CREATE INDEX idx_audits_tenant_id ON audit.audits (tenant_id);

-- ============================================================================
-- audit.team_members
-- ============================================================================
CREATE TABLE audit.team_members (
    id                    uuid         PRIMARY KEY,
    tenant_id             uuid         NOT NULL,
    audit_id              uuid         NOT NULL,
    tenant_member_id      uuid         NOT NULL,
    audit_role            varchar(60)
);

ALTER TABLE audit.team_members
    ADD CONSTRAINT fk_audit_team_tenant  FOREIGN KEY (tenant_id)        REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_audit_team_audit   FOREIGN KEY (audit_id)         REFERENCES audit.audits (id),
    ADD CONSTRAINT fk_audit_team_member  FOREIGN KEY (tenant_member_id) REFERENCES iam.tenant_members (id);

CREATE INDEX idx_audit_team_members_tenant_id ON audit.team_members (tenant_id);

-- ============================================================================
-- audit.responses
-- ============================================================================
CREATE TABLE audit.responses (
    id                    uuid         PRIMARY KEY,
    tenant_id             uuid         NOT NULL,
    audit_id              uuid         NOT NULL,
    checklist_item_id     uuid         NOT NULL,
    response              varchar(30),
    comment               text,
    auditor_member_id     uuid         NOT NULL
);

ALTER TABLE audit.responses
    ADD CONSTRAINT fk_audit_responses_tenant   FOREIGN KEY (tenant_id)          REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_audit_responses_audit    FOREIGN KEY (audit_id)           REFERENCES audit.audits (id),
    ADD CONSTRAINT fk_audit_responses_item     FOREIGN KEY (checklist_item_id)  REFERENCES audit.checklist_items (id),
    ADD CONSTRAINT fk_audit_responses_auditor  FOREIGN KEY (auditor_member_id)  REFERENCES iam.tenant_members (id);

CREATE INDEX idx_audit_responses_tenant_id ON audit.responses (tenant_id);

-- ============================================================================
-- audit.findings
-- ============================================================================
CREATE TABLE audit.findings (
    id                      uuid         PRIMARY KEY,
    tenant_id               uuid         NOT NULL,
    record_id               uuid         NOT NULL UNIQUE,
    audit_id                uuid         NOT NULL,
    audit_response_id       uuid,
    classification          varchar(40)  NOT NULL,
    requirement_reference   varchar(200),
    description             text         NOT NULL,
    recommendation          text,
    owner_member_id         uuid
);

ALTER TABLE audit.findings
    ADD CONSTRAINT fk_audit_findings_tenant   FOREIGN KEY (tenant_id)         REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_audit_findings_record   FOREIGN KEY (record_id)         REFERENCES platform.records (id),
    ADD CONSTRAINT fk_audit_findings_audit    FOREIGN KEY (audit_id)          REFERENCES audit.audits (id),
    ADD CONSTRAINT fk_audit_findings_resp     FOREIGN KEY (audit_response_id) REFERENCES audit.responses (id),
    ADD CONSTRAINT fk_audit_findings_owner    FOREIGN KEY (owner_member_id)   REFERENCES iam.tenant_members (id);

CREATE INDEX idx_audit_findings_tenant_id ON audit.findings (tenant_id);


-- ============================================================================
-- END OF 003-operational.sql
-- COW: 21 tables | INSPECTION: 8 tables | AUDIT: 7 tables  →  36 total
-- ============================================================================
