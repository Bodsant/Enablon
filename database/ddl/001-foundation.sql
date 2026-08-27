-- ============================================================================
-- 001-foundation.sql — Wave 1: Foundation Schemas (saas, org, iam, platform)
-- PostgreSQL 18+  ·  Neon
-- Generated from database/ehsms-erd.dbml
-- Idempotent — safe to re-run
-- ============================================================================

-- Extensions

-- ============================================================================
-- SCHEMAS
-- ============================================================================
CREATE SCHEMA IF NOT EXISTS saas;
CREATE SCHEMA IF NOT EXISTS org;
CREATE SCHEMA IF NOT EXISTS iam;
CREATE SCHEMA IF NOT EXISTS platform;


-- ############################################################################
-- SAAS SCHEMA  (8 tables)
-- ############################################################################

-- ── saas.subscription_plans ────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS saas.subscription_plans (
    id              uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    code            varchar(30)  NOT NULL UNIQUE,
    name            varchar(100) NOT NULL,
    description     text,
    is_active       boolean      NOT NULL DEFAULT true
);

-- ── saas.plan_versions ─────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS saas.plan_versions (
    id                          uuid      NOT NULL DEFAULT gen_random_uuid(),
    subscription_plan_id        uuid      NOT NULL,
    version_number              int       NOT NULL,
    max_active_users            int       NOT NULL,
    max_companies               int,
    max_business_units          int,
    max_sites                   int,
    max_storage_bytes           bigint    NOT NULL,
    max_period_upload_bytes     bigint    NOT NULL,
    max_file_size_bytes         bigint    NOT NULL,
    effective_from              timestamptz NOT NULL,
    effective_until             timestamptz,
    is_current                  boolean   NOT NULL,
    PRIMARY KEY (id)
);

-- ── saas.tenants ───────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS saas.tenants (
    id                  uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_code         varchar(30)  NOT NULL UNIQUE,
    slug                varchar(100) NOT NULL UNIQUE,
    display_name        varchar(200) NOT NULL,
    timezone            varchar(60)  NOT NULL,
    billing_anchor_day  smallint     NOT NULL,
    status              varchar(30)  NOT NULL,
    created_at          timestamptz  NOT NULL DEFAULT now(),
    updated_at          timestamptz  NOT NULL DEFAULT now(),
    PRIMARY KEY (id)
);

-- ── saas.tenant_subscriptions ──────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS saas.tenant_subscriptions (
    id                           uuid       NOT NULL DEFAULT gen_random_uuid(),
    tenant_id                    uuid       NOT NULL,
    plan_version_id              uuid       NOT NULL,
    status                       varchar(30) NOT NULL,
    started_at                   timestamptz NOT NULL,
    current_period_start         timestamptz NOT NULL,
    current_period_end           timestamptz NOT NULL,
    next_reset_at                timestamptz NOT NULL,
    scheduled_plan_version_id    uuid,
    scheduled_change_at          timestamptz,
    PRIMARY KEY (id)
);

-- ── saas.tenant_storage_usage ──────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS saas.tenant_storage_usage (
    id                  uuid       NOT NULL DEFAULT gen_random_uuid(),
    tenant_id           uuid       NOT NULL,
    active_bytes        bigint     NOT NULL DEFAULT 0,
    recycle_bin_bytes   bigint     NOT NULL DEFAULT 0,
    quarantined_bytes   bigint     NOT NULL DEFAULT 0,
    reserved_bytes      bigint     NOT NULL DEFAULT 0,
    object_count        bigint     NOT NULL DEFAULT 0,
    lock_version        int        NOT NULL DEFAULT 0,
    reconciled_at       timestamptz,
    PRIMARY KEY (id)
);

-- ── saas.tenant_usage_periods ──────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS saas.tenant_usage_periods (
    id                      uuid       NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid       NOT NULL,
    tenant_subscription_id  uuid       NOT NULL,
    period_start            timestamptz NOT NULL,
    period_end              timestamptz NOT NULL,
    uploaded_bytes          bigint     NOT NULL DEFAULT 0,
    reserved_upload_bytes   bigint     NOT NULL DEFAULT 0,
    upload_count            bigint     NOT NULL DEFAULT 0,
    status                  varchar(20) NOT NULL,
    lock_version            int        NOT NULL DEFAULT 0,
    PRIMARY KEY (id)
);

-- ── saas.usage_events ──────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS saas.usage_events (
    id                  uuid       NOT NULL DEFAULT gen_random_uuid(),
    tenant_id           uuid       NOT NULL,
    usage_period_id     uuid,
    event_type          varchar(50) NOT NULL,
    reference_id        uuid,
    storage_bytes_delta  bigint    NOT NULL DEFAULT 0,
    upload_bytes_delta   bigint    NOT NULL DEFAULT 0,
    metadata_json       jsonb,
    occurred_at         timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (id)
);

-- ── saas.upload_sessions ───────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS saas.upload_sessions (
    id                      uuid       NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid       NOT NULL,
    usage_period_id         uuid       NOT NULL,
    requested_by_user_id    uuid       NOT NULL,
    original_file_name      varchar(255) NOT NULL,
    mime_type               varchar(150) NOT NULL,
    requested_size_bytes    bigint     NOT NULL,
    object_key              varchar(600) NOT NULL UNIQUE,
    status                  varchar(30) NOT NULL,
    expires_at              timestamptz NOT NULL,
    completed_at            timestamptz,
    PRIMARY KEY (id)
);


-- ############################################################################
-- ORG SCHEMA  (8 tables)
-- ############################################################################

-- ── org.companies ──────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS org.companies (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    code                    varchar(40)  NOT NULL,
    name                    varchar(200) NOT NULL,
    legal_name              varchar(250),
    registration_number     varchar(100),
    status                  varchar(20)  NOT NULL,
    effective_from          date,
    effective_to            date,
    PRIMARY KEY (id)
);

-- ── org.business_units ─────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS org.business_units (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    company_id              uuid         NOT NULL,
    parent_business_unit_id uuid,
    code                    varchar(40)  NOT NULL,
    name                    varchar(200) NOT NULL,
    status                  varchar(20)  NOT NULL,
    PRIMARY KEY (id)
);

-- ── org.sites ──────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS org.sites (
    id                  uuid          NOT NULL DEFAULT gen_random_uuid(),
    tenant_id           uuid          NOT NULL,
    company_id          uuid          NOT NULL,
    business_unit_id    uuid,
    code                varchar(40)   NOT NULL,
    name                varchar(200)  NOT NULL,
    address             text,
    timezone            varchar(60)   NOT NULL,
    latitude            decimal(10,7),
    longitude           decimal(10,7),
    status              varchar(20)   NOT NULL,
    PRIMARY KEY (id)
);

-- ── org.departments ────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS org.departments (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    business_unit_id        uuid,
    site_id                 uuid,
    parent_department_id    uuid,
    code                    varchar(40)  NOT NULL,
    name                    varchar(200) NOT NULL,
    status                  varchar(20)  NOT NULL,
    PRIMARY KEY (id)
);

-- ── org.locations ──────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS org.locations (
    id                  uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id           uuid         NOT NULL,
    site_id             uuid         NOT NULL,
    parent_location_id  uuid,
    code                varchar(40)  NOT NULL,
    name                varchar(200) NOT NULL,
    location_type       varchar(60),
    status              varchar(20)  NOT NULL,
    PRIMARY KEY (id)
);

-- ── org.positions ──────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS org.positions (
    id          uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id   uuid         NOT NULL,
    code        varchar(50)  NOT NULL,
    name        varchar(150) NOT NULL,
    description text,
    status      varchar(20)  NOT NULL,
    PRIMARY KEY (id)
);

-- ── org.people ─────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS org.people (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    person_type             varchar(30)  NOT NULL,
    full_name               varchar(200) NOT NULL,
    email                   varchar(254),
    phone                   varchar(50),
    status                  varchar(20)  NOT NULL,
    data_classification_id  uuid,
    PRIMARY KEY (id)
);

-- ── org.employees ──────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS org.employees (
    id                  uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id           uuid         NOT NULL,
    person_id           uuid         NOT NULL UNIQUE,
    employee_number     varchar(50)  NOT NULL,
    company_id          uuid         NOT NULL,
    department_id       uuid,
    position_id         uuid,
    manager_person_id   uuid,
    employment_status   varchar(30)  NOT NULL,
    source_system       varchar(60),
    source_id           varchar(100),
    PRIMARY KEY (id)
);


-- ############################################################################
-- IAM SCHEMA  (11 tables)
-- ############################################################################

-- ── iam.users ──────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS iam.users (
    id                  uuid         NOT NULL DEFAULT gen_random_uuid(),
    email               varchar(254) NOT NULL UNIQUE,
    normalized_email    varchar(254) NOT NULL UNIQUE,
    password_hash       text,
    identity_provider   varchar(80),
    external_subject    varchar(200),
    status              varchar(20)  NOT NULL,
    last_login_at       timestamptz,
    PRIMARY KEY (id)
);

-- ── iam.tenant_members ────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS iam.tenant_members (
    id                  uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id           uuid         NOT NULL,
    user_id             uuid         NOT NULL,
    person_id           uuid,
    display_name        varchar(200) NOT NULL,
    status              varchar(20)  NOT NULL,
    activated_at        timestamptz,
    deactivated_at      timestamptz,
    PRIMARY KEY (id)
);

-- ── iam.roles ──────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS iam.roles (
    id          uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id   uuid         NOT NULL,
    code        varchar(60)  NOT NULL,
    name        varchar(120) NOT NULL,
    scope_type  varchar(20)  NOT NULL,
    is_system   boolean      NOT NULL DEFAULT false,
    PRIMARY KEY (id)
);

-- ── iam.permissions ────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS iam.permissions (
    id          uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id   uuid         NOT NULL,
    code        varchar(100) NOT NULL,
    module      varchar(50)  NOT NULL,
    action      varchar(50)  NOT NULL,
    description text,
    PRIMARY KEY (id)
);

-- ── iam.role_permissions ──────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS iam.role_permissions (
    id              uuid NOT NULL DEFAULT gen_random_uuid(),
    tenant_id       uuid NOT NULL,
    role_id         uuid NOT NULL,
    permission_id   uuid NOT NULL,
    PRIMARY KEY (id)
);

-- ── iam.member_roles ──────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS iam.member_roles (
    id                  uuid NOT NULL DEFAULT gen_random_uuid(),
    tenant_id           uuid NOT NULL,
    tenant_member_id    uuid NOT NULL,
    role_id             uuid NOT NULL,
    PRIMARY KEY (id)
);

-- ── iam.access_scopes ─────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS iam.access_scopes (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    scope_type              varchar(30)  NOT NULL,
    company_id              uuid,
    business_unit_id        uuid,
    site_id                 uuid,
    department_id           uuid,
    location_id             uuid,
    contractor_company_id   uuid,
    data_classification_id  uuid,
    PRIMARY KEY (id)
);

-- ── iam.member_access_scopes ─────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS iam.member_access_scopes (
    id                  uuid NOT NULL DEFAULT gen_random_uuid(),
    tenant_id           uuid NOT NULL,
    tenant_member_id    uuid NOT NULL,
    access_scope_id     uuid NOT NULL,
    PRIMARY KEY (id)
);

-- ── iam.temporary_access_grants ──────────────────────────────────────────
CREATE TABLE IF NOT EXISTS iam.temporary_access_grants (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    tenant_member_id        uuid         NOT NULL,
    access_scope_id         uuid         NOT NULL,
    role_id                 uuid,
    approved_by_member_id   uuid         NOT NULL,
    reason                  text         NOT NULL,
    valid_from              timestamptz  NOT NULL,
    valid_until             timestamptz  NOT NULL,
    PRIMARY KEY (id)
);

-- ── iam.access_reviews ────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS iam.access_reviews (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    review_period_start     date         NOT NULL,
    review_period_end       date         NOT NULL,
    reviewer_member_id      uuid         NOT NULL,
    status                  varchar(20)  NOT NULL,
    completed_at            timestamptz,
    PRIMARY KEY (id)
);

-- ── iam.refresh_tokens ────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS iam.refresh_tokens (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    user_id                 uuid         NOT NULL,
    token_hash              varchar(255) NOT NULL UNIQUE,
    expires_at              timestamptz  NOT NULL,
    revoked_at              timestamptz,
    replaced_by_token_id    uuid,
    PRIMARY KEY (id)
);


-- ############################################################################
-- PLATFORM SCHEMA  (19 tables)
-- ############################################################################

-- ── platform.data_classifications ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.data_classifications (
    id              uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id       uuid         NOT NULL,
    code            varchar(30)  NOT NULL,
    name            varchar(100) NOT NULL,
    rank            smallint     NOT NULL,
    is_restricted   boolean      NOT NULL DEFAULT false,
    PRIMARY KEY (id)
);

-- ── platform.retention_policies ───────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.retention_policies (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    record_type             varchar(60)  NOT NULL,
    classification_id       uuid,
    retention_days          int,
    archive_after_days      int,
    recycle_bin_days        int,
    legal_hold_supported    boolean      NOT NULL DEFAULT false,
    PRIMARY KEY (id)
);

-- ── platform.records ──────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.records (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    module_code             varchar(40)  NOT NULL,
    record_type             varchar(60)  NOT NULL,
    record_number           varchar(60)  NOT NULL,
    company_id              uuid,
    business_unit_id        uuid,
    site_id                 uuid,
    department_id           uuid,
    location_id             uuid,
    data_classification_id  uuid         NOT NULL,
    status                  varchar(40)  NOT NULL,
    title                   varchar(250),
    created_by_member_id    uuid         NOT NULL,
    created_at              timestamptz  NOT NULL DEFAULT now(),
    updated_at              timestamptz  NOT NULL DEFAULT now(),
    archived_at             timestamptz,
    voided_at               timestamptz,
    PRIMARY KEY (id)
);

-- ── platform.record_links ─────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.record_links (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    source_record_id        uuid         NOT NULL,
    target_record_id        uuid         NOT NULL,
    link_type               varchar(60)  NOT NULL,
    created_by_member_id    uuid         NOT NULL,
    created_at              timestamptz  NOT NULL DEFAULT now(),
    PRIMARY KEY (id)
);

-- ── platform.workflow_definitions ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.workflow_definitions (
    id          uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id   uuid         NOT NULL,
    code        varchar(60)  NOT NULL,
    name        varchar(150) NOT NULL,
    module_code varchar(40)  NOT NULL,
    status      varchar(20)  NOT NULL,
    PRIMARY KEY (id)
);

-- ── platform.workflow_versions ────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.workflow_versions (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    workflow_definition_id  uuid         NOT NULL,
    version_number          int          NOT NULL,
    effective_from          timestamptz  NOT NULL,
    effective_to            timestamptz,
    status                  varchar(20)  NOT NULL,
    PRIMARY KEY (id)
);

-- ── platform.workflow_states ──────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.workflow_states (
    id                  uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id           uuid         NOT NULL,
    workflow_version_id uuid         NOT NULL,
    state_code          varchar(50)  NOT NULL,
    state_name          varchar(100) NOT NULL,
    is_initial          boolean      NOT NULL DEFAULT false,
    is_terminal         boolean      NOT NULL DEFAULT false,
    PRIMARY KEY (id)
);

-- ── platform.workflow_transitions ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.workflow_transitions (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    workflow_version_id     uuid         NOT NULL,
    from_state_id           uuid         NOT NULL,
    to_state_id             uuid         NOT NULL,
    action_code             varchar(50)  NOT NULL,
    required_permission_id  uuid,
    validation_rule_json    jsonb,
    requires_comment        boolean      NOT NULL DEFAULT false,
    PRIMARY KEY (id)
);

-- ── platform.workflow_instances ───────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.workflow_instances (
    id                  uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id           uuid         NOT NULL,
    record_id           uuid         NOT NULL UNIQUE,
    workflow_version_id uuid         NOT NULL,
    current_state_id    uuid         NOT NULL,
    started_at          timestamptz  NOT NULL DEFAULT now(),
    completed_at        timestamptz,
    PRIMARY KEY (id)
);

-- ── platform.workflow_tasks ───────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.workflow_tasks (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    workflow_instance_id    uuid         NOT NULL,
    task_type               varchar(40)  NOT NULL,
    assigned_member_id      uuid,
    assigned_role_id        uuid,
    due_at                  timestamptz,
    priority                varchar(20),
    status                  varchar(30)  NOT NULL,
    completed_at            timestamptz,
    PRIMARY KEY (id)
);

-- ── platform.workflow_decisions ───────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.workflow_decisions (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    workflow_task_id        uuid         NOT NULL,
    transition_id           uuid,
    decision                varchar(30)  NOT NULL,
    comment                 text,
    decided_by_member_id    uuid         NOT NULL,
    decided_at              timestamptz  NOT NULL DEFAULT now(),
    PRIMARY KEY (id)
);

-- ── platform.escalation_rules ─────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.escalation_rules (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    workflow_version_id     uuid,
    event_code              varchar(60)  NOT NULL,
    condition_json          jsonb        NOT NULL,
    action_json             jsonb        NOT NULL,
    is_active               boolean      NOT NULL DEFAULT true,
    PRIMARY KEY (id)
);

-- ── platform.file_objects ─────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.file_objects (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    upload_session_id       uuid,
    bucket_name             varchar(100) NOT NULL,
    object_key              varchar(600) NOT NULL UNIQUE,
    original_file_name      varchar(255) NOT NULL,
    mime_type               varchar(150) NOT NULL,
    object_size_bytes       bigint       NOT NULL,
    checksum_sha256         varchar(64)  NOT NULL,
    status                  varchar(30)  NOT NULL,
    uploaded_by_user_id     uuid         NOT NULL,
    deleted_at              timestamptz,
    purge_after             timestamptz,
    purged_at               timestamptz,
    PRIMARY KEY (id)
);

-- ── platform.evidence_links ───────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.evidence_links (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    record_id               uuid         NOT NULL,
    file_object_id          uuid         NOT NULL,
    evidence_type           varchar(50)  NOT NULL,
    document_revision_id    uuid,
    link_status             varchar(20)  NOT NULL,
    linked_by_member_id     uuid         NOT NULL,
    linked_at               timestamptz  NOT NULL DEFAULT now(),
    invalidation_reason     text,
    PRIMARY KEY (id)
);

-- ── platform.notifications ────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.notifications (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    recipient_member_id     uuid         NOT NULL,
    record_id               uuid,
    notification_type       varchar(60)  NOT NULL,
    title                   varchar(200) NOT NULL,
    message                 text         NOT NULL,
    delivery_channel        varchar(30),
    delivery_status         varchar(30),
    read_at                 timestamptz,
    PRIMARY KEY (id)
);

-- ── platform.audit_logs ───────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.audit_logs (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    record_id               uuid,
    user_id                 uuid,
    tenant_member_id        uuid,
    action_code             varchar(100) NOT NULL,
    before_json             jsonb,
    after_json              jsonb,
    ip_address              varchar(64),
    correlation_id          varchar(100),
    occurred_at             timestamptz  NOT NULL DEFAULT now(),
    PRIMARY KEY (id)
);

-- ── platform.outbox_messages ──────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.outbox_messages (
    id                      uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id               uuid         NOT NULL,
    record_id               uuid,
    event_type              varchar(100) NOT NULL,
    payload_json            jsonb        NOT NULL,
    status                  varchar(20)  NOT NULL,
    attempt_count           int          NOT NULL DEFAULT 0,
    next_retry_at           timestamptz,
    occurred_at             timestamptz  NOT NULL DEFAULT now(),
    PRIMARY KEY (id)
);

-- ── platform.number_sequences ─────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.number_sequences (
    id              uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id       uuid         NOT NULL,
    sequence_code   varchar(40)  NOT NULL,
    period_key      varchar(20)  NOT NULL,
    current_value   bigint       NOT NULL DEFAULT 0,
    lock_version    int          NOT NULL DEFAULT 0,
    PRIMARY KEY (id)
);

-- ── platform.lookup_values ────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS platform.lookup_values (
    id              uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id       uuid         NOT NULL,
    category        varchar(80)  NOT NULL,
    code            varchar(60)  NOT NULL,
    label           varchar(150) NOT NULL,
    effective_from  date,
    effective_to    date,
    status          varchar(20)  NOT NULL,
    metadata_json   jsonb,
    PRIMARY KEY (id)
);


-- ============================================================================
-- FOREIGN KEYS — Topological order (parent → child)
-- ============================================================================

-- ── saas.plan_versions → saas.subscription_plans ──────────────────────────
ALTER TABLE saas.plan_versions
    ADD CONSTRAINT fk_plan_versions_plan
    FOREIGN KEY (subscription_plan_id) REFERENCES saas.subscription_plans (id);

-- ── saas.tenant_subscriptions → saas.tenants, saas.plan_versions ──────────
ALTER TABLE saas.tenant_subscriptions
    ADD CONSTRAINT fk_tenant_subscriptions_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_tenant_subscriptions_plan_version
    FOREIGN KEY (plan_version_id) REFERENCES saas.plan_versions (id),
    ADD CONSTRAINT fk_tenant_subscriptions_scheduled_plan_version
    FOREIGN KEY (scheduled_plan_version_id) REFERENCES saas.plan_versions (id);

-- ── saas.tenant_storage_usage → saas.tenants ──────────────────────────────
ALTER TABLE saas.tenant_storage_usage
    ADD CONSTRAINT fk_tenant_storage_usage_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id);

-- ── saas.tenant_usage_periods → saas.tenants, saas.tenant_subscriptions ───
ALTER TABLE saas.tenant_usage_periods
    ADD CONSTRAINT fk_tenant_usage_periods_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_tenant_usage_periods_subscription
    FOREIGN KEY (tenant_subscription_id) REFERENCES saas.tenant_subscriptions (id);

-- ── saas.usage_events → saas.tenants, saas.tenant_usage_periods ───────────
ALTER TABLE saas.usage_events
    ADD CONSTRAINT fk_usage_events_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_usage_events_period
    FOREIGN KEY (usage_period_id) REFERENCES saas.tenant_usage_periods (id);

-- ── saas.upload_sessions → saas.tenants, saas.tenant_usage_periods, iam.users
ALTER TABLE saas.upload_sessions
    ADD CONSTRAINT fk_upload_sessions_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_upload_sessions_period
    FOREIGN KEY (usage_period_id) REFERENCES saas.tenant_usage_periods (id),
    ADD CONSTRAINT fk_upload_sessions_user
    FOREIGN KEY (requested_by_user_id) REFERENCES iam.users (id);

-- ── org.companies → saas.tenants ──────────────────────────────────────────
ALTER TABLE org.companies
    ADD CONSTRAINT fk_companies_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id);

-- ── org.business_units → saas.tenants, org.companies ──────────────────────
ALTER TABLE org.business_units
    ADD CONSTRAINT fk_business_units_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_business_units_company
    FOREIGN KEY (company_id) REFERENCES org.companies (id),
    ADD CONSTRAINT fk_business_units_parent
    FOREIGN KEY (parent_business_unit_id) REFERENCES org.business_units (id);

-- ── org.sites → saas.tenants, org.companies, org.business_units ──────────
ALTER TABLE org.sites
    ADD CONSTRAINT fk_sites_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_sites_company
    FOREIGN KEY (company_id) REFERENCES org.companies (id),
    ADD CONSTRAINT fk_sites_business_unit
    FOREIGN KEY (business_unit_id) REFERENCES org.business_units (id);

-- ── org.departments → saas.tenants, org.business_units, org.sites ────────
ALTER TABLE org.departments
    ADD CONSTRAINT fk_departments_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_departments_business_unit
    FOREIGN KEY (business_unit_id) REFERENCES org.business_units (id),
    ADD CONSTRAINT fk_departments_site
    FOREIGN KEY (site_id) REFERENCES org.sites (id),
    ADD CONSTRAINT fk_departments_parent
    FOREIGN KEY (parent_department_id) REFERENCES org.departments (id);

-- ── org.locations → saas.tenants, org.sites ──────────────────────────────
ALTER TABLE org.locations
    ADD CONSTRAINT fk_locations_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_locations_site
    FOREIGN KEY (site_id) REFERENCES org.sites (id),
    ADD CONSTRAINT fk_locations_parent
    FOREIGN KEY (parent_location_id) REFERENCES org.locations (id);

-- ── org.positions → saas.tenants ──────────────────────────────────────────
ALTER TABLE org.positions
    ADD CONSTRAINT fk_positions_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id);

-- ── org.people → saas.tenants, platform.data_classifications ─────────────
ALTER TABLE org.people
    ADD CONSTRAINT fk_people_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_people_data_classification
    FOREIGN KEY (data_classification_id) REFERENCES platform.data_classifications (id);

-- ── org.employees → saas.tenants, org.people, org.companies, org.departments, org.positions
ALTER TABLE org.employees
    ADD CONSTRAINT fk_employees_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_employees_person
    FOREIGN KEY (person_id) REFERENCES org.people (id),
    ADD CONSTRAINT fk_employees_company
    FOREIGN KEY (company_id) REFERENCES org.companies (id),
    ADD CONSTRAINT fk_employees_department
    FOREIGN KEY (department_id) REFERENCES org.departments (id),
    ADD CONSTRAINT fk_employees_position
    FOREIGN KEY (position_id) REFERENCES org.positions (id),
    ADD CONSTRAINT fk_employees_manager
    FOREIGN KEY (manager_person_id) REFERENCES org.people (id);

-- ── iam.tenant_members → saas.tenants, iam.users, org.people ─────────────
ALTER TABLE iam.tenant_members
    ADD CONSTRAINT fk_tenant_members_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_tenant_members_user
    FOREIGN KEY (user_id) REFERENCES iam.users (id),
    ADD CONSTRAINT fk_tenant_members_person
    FOREIGN KEY (person_id) REFERENCES org.people (id);

-- ── iam.roles → saas.tenants ─────────────────────────────────────────────
ALTER TABLE iam.roles
    ADD CONSTRAINT fk_roles_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id);

-- ── iam.permissions → saas.tenants ───────────────────────────────────────
ALTER TABLE iam.permissions
    ADD CONSTRAINT fk_permissions_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id);

-- ── iam.role_permissions → saas.tenants, iam.roles, iam.permissions ──────
ALTER TABLE iam.role_permissions
    ADD CONSTRAINT fk_role_permissions_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_role_permissions_role
    FOREIGN KEY (role_id) REFERENCES iam.roles (id),
    ADD CONSTRAINT fk_role_permissions_permission
    FOREIGN KEY (permission_id) REFERENCES iam.permissions (id);

-- ── iam.member_roles → saas.tenants, iam.tenant_members, iam.roles ──────
ALTER TABLE iam.member_roles
    ADD CONSTRAINT fk_member_roles_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_member_roles_member
    FOREIGN KEY (tenant_member_id) REFERENCES iam.tenant_members (id),
    ADD CONSTRAINT fk_member_roles_role
    FOREIGN KEY (role_id) REFERENCES iam.roles (id);

-- ── iam.access_scopes → saas.tenants, org.*, contractor.companies, platform.data_classifications
ALTER TABLE iam.access_scopes
    ADD CONSTRAINT fk_access_scopes_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_access_scopes_company
    FOREIGN KEY (company_id) REFERENCES org.companies (id),
    ADD CONSTRAINT fk_access_scopes_business_unit
    FOREIGN KEY (business_unit_id) REFERENCES org.business_units (id),
    ADD CONSTRAINT fk_access_scopes_site
    FOREIGN KEY (site_id) REFERENCES org.sites (id),
    ADD CONSTRAINT fk_access_scopes_department
    FOREIGN KEY (department_id) REFERENCES org.departments (id),
    ADD CONSTRAINT fk_access_scopes_location
    FOREIGN KEY (location_id) REFERENCES org.locations (id),
    ADD CONSTRAINT fk_access_scopes_data_classification
    FOREIGN KEY (data_classification_id) REFERENCES platform.data_classifications (id);
-- NOTE: contractor_company_id FK deferred to after contractor.companies is created

-- ── iam.member_access_scopes → saas.tenants, iam.tenant_members, iam.access_scopes
ALTER TABLE iam.member_access_scopes
    ADD CONSTRAINT fk_member_access_scopes_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_member_access_scopes_member
    FOREIGN KEY (tenant_member_id) REFERENCES iam.tenant_members (id),
    ADD CONSTRAINT fk_member_access_scopes_scope
    FOREIGN KEY (access_scope_id) REFERENCES iam.access_scopes (id);

-- ── iam.temporary_access_grants → saas.tenants, iam.tenant_members, iam.access_scopes, iam.roles
ALTER TABLE iam.temporary_access_grants
    ADD CONSTRAINT fk_temp_access_grants_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_temp_access_grants_member
    FOREIGN KEY (tenant_member_id) REFERENCES iam.tenant_members (id),
    ADD CONSTRAINT fk_temp_access_grants_scope
    FOREIGN KEY (access_scope_id) REFERENCES iam.access_scopes (id),
    ADD CONSTRAINT fk_temp_access_grants_role
    FOREIGN KEY (role_id) REFERENCES iam.roles (id),
    ADD CONSTRAINT fk_temp_access_grants_approver
    FOREIGN KEY (approved_by_member_id) REFERENCES iam.tenant_members (id);

-- ── iam.access_reviews → saas.tenants, iam.tenant_members ────────────────
ALTER TABLE iam.access_reviews
    ADD CONSTRAINT fk_access_reviews_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_access_reviews_reviewer
    FOREIGN KEY (reviewer_member_id) REFERENCES iam.tenant_members (id);

-- ── iam.refresh_tokens → iam.users ───────────────────────────────────────
ALTER TABLE iam.refresh_tokens
    ADD CONSTRAINT fk_refresh_tokens_user
    FOREIGN KEY (user_id) REFERENCES iam.users (id),
    ADD CONSTRAINT fk_refresh_tokens_replaced_by
    FOREIGN KEY (replaced_by_token_id) REFERENCES iam.refresh_tokens (id);

-- ── platform.retention_policies → saas.tenants, platform.data_classifications
ALTER TABLE platform.retention_policies
    ADD CONSTRAINT fk_retention_policies_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_retention_policies_classification
    FOREIGN KEY (classification_id) REFERENCES platform.data_classifications (id);

-- ── platform.records → saas.tenants, org.*, platform.data_classifications, iam.tenant_members
ALTER TABLE platform.records
    ADD CONSTRAINT fk_records_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_records_company
    FOREIGN KEY (company_id) REFERENCES org.companies (id),
    ADD CONSTRAINT fk_records_business_unit
    FOREIGN KEY (business_unit_id) REFERENCES org.business_units (id),
    ADD CONSTRAINT fk_records_site
    FOREIGN KEY (site_id) REFERENCES org.sites (id),
    ADD CONSTRAINT fk_records_department
    FOREIGN KEY (department_id) REFERENCES org.departments (id),
    ADD CONSTRAINT fk_records_location
    FOREIGN KEY (location_id) REFERENCES org.locations (id),
    ADD CONSTRAINT fk_records_data_classification
    FOREIGN KEY (data_classification_id) REFERENCES platform.data_classifications (id),
    ADD CONSTRAINT fk_records_created_by
    FOREIGN KEY (created_by_member_id) REFERENCES iam.tenant_members (id);

-- ── platform.record_links → saas.tenants, platform.records ────────────────
ALTER TABLE platform.record_links
    ADD CONSTRAINT fk_record_links_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_record_links_source
    FOREIGN KEY (source_record_id) REFERENCES platform.records (id),
    ADD CONSTRAINT fk_record_links_target
    FOREIGN KEY (target_record_id) REFERENCES platform.records (id),
    ADD CONSTRAINT fk_record_links_created_by
    FOREIGN KEY (created_by_member_id) REFERENCES iam.tenant_members (id);

-- ── platform.workflow_definitions → saas.tenants ──────────────────────────
ALTER TABLE platform.workflow_definitions
    ADD CONSTRAINT fk_workflow_definitions_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id);

-- ── platform.workflow_versions → saas.tenants, platform.workflow_definitions
ALTER TABLE platform.workflow_versions
    ADD CONSTRAINT fk_workflow_versions_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_workflow_versions_definition
    FOREIGN KEY (workflow_definition_id) REFERENCES platform.workflow_definitions (id);

-- ── platform.workflow_states → saas.tenants, platform.workflow_versions ──
ALTER TABLE platform.workflow_states
    ADD CONSTRAINT fk_workflow_states_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_workflow_states_version
    FOREIGN KEY (workflow_version_id) REFERENCES platform.workflow_versions (id);

-- ── platform.workflow_transitions → saas.tenants, platform.workflow_versions, platform.workflow_states
ALTER TABLE platform.workflow_transitions
    ADD CONSTRAINT fk_workflow_transitions_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_workflow_transitions_version
    FOREIGN KEY (workflow_version_id) REFERENCES platform.workflow_versions (id),
    ADD CONSTRAINT fk_workflow_transitions_from_state
    FOREIGN KEY (from_state_id) REFERENCES platform.workflow_states (id),
    ADD CONSTRAINT fk_workflow_transitions_to_state
    FOREIGN KEY (to_state_id) REFERENCES platform.workflow_states (id),
    ADD CONSTRAINT fk_workflow_transitions_permission
    FOREIGN KEY (required_permission_id) REFERENCES iam.permissions (id);

-- ── platform.workflow_instances → saas.tenants, platform.records, platform.workflow_versions, platform.workflow_states
ALTER TABLE platform.workflow_instances
    ADD CONSTRAINT fk_workflow_instances_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_workflow_instances_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id),
    ADD CONSTRAINT fk_workflow_instances_version
    FOREIGN KEY (workflow_version_id) REFERENCES platform.workflow_versions (id),
    ADD CONSTRAINT fk_workflow_instances_current_state
    FOREIGN KEY (current_state_id) REFERENCES platform.workflow_states (id);

-- ── platform.workflow_tasks → saas.tenants, platform.workflow_instances, iam.tenant_members, iam.roles
ALTER TABLE platform.workflow_tasks
    ADD CONSTRAINT fk_workflow_tasks_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_workflow_tasks_instance
    FOREIGN KEY (workflow_instance_id) REFERENCES platform.workflow_instances (id),
    ADD CONSTRAINT fk_workflow_tasks_assigned_member
    FOREIGN KEY (assigned_member_id) REFERENCES iam.tenant_members (id),
    ADD CONSTRAINT fk_workflow_tasks_assigned_role
    FOREIGN KEY (assigned_role_id) REFERENCES iam.roles (id);

-- ── platform.workflow_decisions → saas.tenants, platform.workflow_tasks, platform.workflow_transitions, iam.tenant_members
ALTER TABLE platform.workflow_decisions
    ADD CONSTRAINT fk_workflow_decisions_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_workflow_decisions_task
    FOREIGN KEY (workflow_task_id) REFERENCES platform.workflow_tasks (id),
    ADD CONSTRAINT fk_workflow_decisions_transition
    FOREIGN KEY (transition_id) REFERENCES platform.workflow_transitions (id),
    ADD CONSTRAINT fk_workflow_decisions_decided_by
    FOREIGN KEY (decided_by_member_id) REFERENCES iam.tenant_members (id);

-- ── platform.escalation_rules → saas.tenants, platform.workflow_versions ─
ALTER TABLE platform.escalation_rules
    ADD CONSTRAINT fk_escalation_rules_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_escalation_rules_version
    FOREIGN KEY (workflow_version_id) REFERENCES platform.workflow_versions (id);

-- ── platform.file_objects → saas.tenants, saas.upload_sessions, iam.users ─
ALTER TABLE platform.file_objects
    ADD CONSTRAINT fk_file_objects_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_file_objects_upload_session
    FOREIGN KEY (upload_session_id) REFERENCES saas.upload_sessions (id),
    ADD CONSTRAINT fk_file_objects_uploaded_by
    FOREIGN KEY (uploaded_by_user_id) REFERENCES iam.users (id);

-- ── platform.evidence_links → saas.tenants, platform.records, platform.file_objects, iam.tenant_members
ALTER TABLE platform.evidence_links
    ADD CONSTRAINT fk_evidence_links_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_evidence_links_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id),
    ADD CONSTRAINT fk_evidence_links_file_object
    FOREIGN KEY (file_object_id) REFERENCES platform.file_objects (id),
    ADD CONSTRAINT fk_evidence_links_linked_by
    FOREIGN KEY (linked_by_member_id) REFERENCES iam.tenant_members (id);
-- NOTE: document_revision_id FK deferred to after document.revisions is created

-- ── platform.notifications → saas.tenants, iam.tenant_members, platform.records
ALTER TABLE platform.notifications
    ADD CONSTRAINT fk_notifications_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_notifications_recipient
    FOREIGN KEY (recipient_member_id) REFERENCES iam.tenant_members (id),
    ADD CONSTRAINT fk_notifications_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

-- ── platform.audit_logs → saas.tenants, platform.records, iam.users, iam.tenant_members
ALTER TABLE platform.audit_logs
    ADD CONSTRAINT fk_audit_logs_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_audit_logs_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id),
    ADD CONSTRAINT fk_audit_logs_user
    FOREIGN KEY (user_id) REFERENCES iam.users (id),
    ADD CONSTRAINT fk_audit_logs_tenant_member
    FOREIGN KEY (tenant_member_id) REFERENCES iam.tenant_members (id);

-- ── platform.outbox_messages → saas.tenants, platform.records ──────────────
ALTER TABLE platform.outbox_messages
    ADD CONSTRAINT fk_outbox_messages_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id),
    ADD CONSTRAINT fk_outbox_messages_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

-- ── platform.number_sequences → saas.tenants ──────────────────────────────
ALTER TABLE platform.number_sequences
    ADD CONSTRAINT fk_number_sequences_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id);

-- ── platform.lookup_values → saas.tenants ──────────────────────────────────
ALTER TABLE platform.lookup_values
    ADD CONSTRAINT fk_lookup_values_tenant
    FOREIGN KEY (tenant_id) REFERENCES saas.tenants (id);


-- ============================================================================
-- INDEXES — tenant_id on all tables + common query patterns
-- ============================================================================

-- saas
CREATE INDEX IF NOT EXISTS idx_subscription_plans_active ON saas.subscription_plans (is_active) WHERE is_active = true;
CREATE INDEX IF NOT EXISTS idx_plan_versions_plan ON saas.plan_versions (subscription_plan_id);
CREATE INDEX IF NOT EXISTS idx_plan_versions_effective ON saas.plan_versions (effective_from, effective_until);
CREATE INDEX IF NOT EXISTS idx_tenants_slug ON saas.tenants (slug);
CREATE INDEX IF NOT EXISTS idx_tenant_subscriptions_tenant ON saas.tenant_subscriptions (tenant_id);
CREATE INDEX IF NOT EXISTS idx_tenant_storage_usage_tenant ON saas.tenant_storage_usage (tenant_id);
CREATE INDEX IF NOT EXISTS idx_tenant_usage_periods_tenant ON saas.tenant_usage_periods (tenant_id);
CREATE INDEX IF NOT EXISTS idx_tenant_usage_periods_subscription ON saas.tenant_usage_periods (tenant_subscription_id);
CREATE INDEX IF NOT EXISTS idx_usage_events_tenant ON saas.usage_events (tenant_id);
CREATE INDEX IF NOT EXISTS idx_usage_events_period ON saas.usage_events (usage_period_id);
CREATE INDEX IF NOT EXISTS idx_upload_sessions_tenant ON saas.upload_sessions (tenant_id);
CREATE INDEX IF NOT EXISTS idx_upload_sessions_status ON saas.upload_sessions (status);

-- org
CREATE INDEX IF NOT EXISTS idx_companies_tenant ON org.companies (tenant_id);
CREATE INDEX IF NOT EXISTS idx_companies_status ON org.companies (tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_business_units_tenant ON org.business_units (tenant_id);
CREATE INDEX IF NOT EXISTS idx_business_units_company ON org.business_units (company_id);
CREATE INDEX IF NOT EXISTS idx_sites_tenant ON org.sites (tenant_id);
CREATE INDEX IF NOT EXISTS idx_sites_company ON org.sites (company_id);
CREATE INDEX IF NOT EXISTS idx_departments_tenant ON org.departments (tenant_id);
CREATE INDEX IF NOT EXISTS idx_departments_site ON org.departments (site_id);
CREATE INDEX IF NOT EXISTS idx_locations_tenant ON org.locations (tenant_id);
CREATE INDEX IF NOT EXISTS idx_locations_site ON org.locations (site_id);
CREATE INDEX IF NOT EXISTS idx_positions_tenant ON org.positions (tenant_id);
CREATE INDEX IF NOT EXISTS idx_people_tenant ON org.people (tenant_id);
CREATE INDEX IF NOT EXISTS idx_people_email ON org.people (email) WHERE email IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_employees_tenant ON org.employees (tenant_id);
CREATE INDEX IF NOT EXISTS idx_employees_company ON org.employees (company_id);

-- iam
CREATE INDEX IF NOT EXISTS idx_tenant_members_tenant ON iam.tenant_members (tenant_id);
CREATE INDEX IF NOT EXISTS idx_tenant_members_user ON iam.tenant_members (user_id);
CREATE INDEX IF NOT EXISTS idx_tenant_members_status ON iam.tenant_members (tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_roles_tenant ON iam.roles (tenant_id);
CREATE INDEX IF NOT EXISTS idx_permissions_tenant ON iam.permissions (tenant_id);
CREATE INDEX IF NOT EXISTS idx_role_permissions_tenant ON iam.role_permissions (tenant_id);
CREATE INDEX IF NOT EXISTS idx_role_permissions_role ON iam.role_permissions (role_id);
CREATE INDEX IF NOT EXISTS idx_member_roles_tenant ON iam.member_roles (tenant_id);
CREATE INDEX IF NOT EXISTS idx_member_roles_member ON iam.member_roles (tenant_member_id);
CREATE INDEX IF NOT EXISTS idx_access_scopes_tenant ON iam.access_scopes (tenant_id);
CREATE INDEX IF NOT EXISTS idx_member_access_scopes_tenant ON iam.member_access_scopes (tenant_id);
CREATE INDEX IF NOT EXISTS idx_temp_access_grants_tenant ON iam.temporary_access_grants (tenant_id);
CREATE INDEX IF NOT EXISTS idx_access_reviews_tenant ON iam.access_reviews (tenant_id);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user ON iam.refresh_tokens (user_id);

-- platform
CREATE INDEX IF NOT EXISTS idx_data_classifications_tenant ON platform.data_classifications (tenant_id);
CREATE INDEX IF NOT EXISTS idx_retention_policies_tenant ON platform.retention_policies (tenant_id);
CREATE INDEX IF NOT EXISTS idx_records_tenant ON platform.records (tenant_id);
CREATE INDEX IF NOT EXISTS idx_records_module ON platform.records (tenant_id, module_code);
CREATE INDEX IF NOT EXISTS idx_records_status ON platform.records (tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_records_record_number ON platform.records (tenant_id, record_number);
CREATE INDEX IF NOT EXISTS idx_record_links_tenant ON platform.record_links (tenant_id);
CREATE INDEX IF NOT EXISTS idx_record_links_source ON platform.record_links (source_record_id);
CREATE INDEX IF NOT EXISTS idx_workflow_definitions_tenant ON platform.workflow_definitions (tenant_id);
CREATE INDEX IF NOT EXISTS idx_workflow_versions_tenant ON platform.workflow_versions (tenant_id);
CREATE INDEX IF NOT EXISTS idx_workflow_states_tenant ON platform.workflow_states (tenant_id);
CREATE INDEX IF NOT EXISTS idx_workflow_transitions_tenant ON platform.workflow_transitions (tenant_id);
CREATE INDEX IF NOT EXISTS idx_workflow_instances_tenant ON platform.workflow_instances (tenant_id);
CREATE INDEX IF NOT EXISTS idx_workflow_instances_record ON platform.workflow_instances (record_id);
CREATE INDEX IF NOT EXISTS idx_workflow_tasks_tenant ON platform.workflow_tasks (tenant_id);
CREATE INDEX IF NOT EXISTS idx_workflow_tasks_instance ON platform.workflow_tasks (workflow_instance_id);
CREATE INDEX IF NOT EXISTS idx_workflow_decisions_tenant ON platform.workflow_decisions (tenant_id);
CREATE INDEX IF NOT EXISTS idx_escalation_rules_tenant ON platform.escalation_rules (tenant_id);
CREATE INDEX IF NOT EXISTS idx_file_objects_tenant ON platform.file_objects (tenant_id);
CREATE INDEX IF NOT EXISTS idx_file_objects_status ON platform.file_objects (status);
CREATE INDEX IF NOT EXISTS idx_evidence_links_tenant ON platform.evidence_links (tenant_id);
CREATE INDEX IF NOT EXISTS idx_evidence_links_record ON platform.evidence_links (record_id);
CREATE INDEX IF NOT EXISTS idx_notifications_tenant ON platform.notifications (tenant_id);
CREATE INDEX IF NOT EXISTS idx_notifications_recipient ON platform.notifications (recipient_member_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_tenant ON platform.audit_logs (tenant_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_record ON platform.audit_logs (record_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_occurred ON platform.audit_logs (occurred_at);
CREATE INDEX IF NOT EXISTS idx_outbox_messages_tenant ON platform.outbox_messages (tenant_id);
CREATE INDEX IF NOT EXISTS idx_outbox_messages_status ON platform.outbox_messages (status, next_retry_at);
CREATE INDEX IF NOT EXISTS idx_number_sequences_tenant ON platform.number_sequences (tenant_id);
CREATE INDEX IF NOT EXISTS idx_lookup_values_tenant ON platform.lookup_values (tenant_id);
CREATE INDEX IF NOT EXISTS idx_lookup_values_category ON platform.lookup_values (tenant_id, category);
