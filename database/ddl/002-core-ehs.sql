-- ============================================================================
-- Wave 2 — Core EHS DDL
-- Schemas : document, safety, risk, incident, capa
-- Source  : database/ehsms-erd.dbml  (PRD v1.1 Final Revised, 26 Aug 2026)
-- Engine  : PostgreSQL 18 (Neon)
-- Idempotent — safe to re-run
-- ============================================================================

-- Extensions

-- ────────────────────────────────────────────────────────────────────────────
-- SCHEMAS
-- ────────────────────────────────────────────────────────────────────────────
CREATE SCHEMA IF NOT EXISTS document;
CREATE SCHEMA IF NOT EXISTS safety;
CREATE SCHEMA IF NOT EXISTS risk;
CREATE SCHEMA IF NOT EXISTS incident;
CREATE SCHEMA IF NOT EXISTS capa;


-- ╔═══════════════════════════════════════════════════════════════════════════╗
-- ║  DOCUMENT                                                                ║
-- ╚═══════════════════════════════════════════════════════════════════════════╝

-- ── document.controlled_documents ────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS document.controlled_documents (
    id                  uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid        NOT NULL,
    record_id           uuid        NOT NULL UNIQUE,
    document_number     varchar(60) NOT NULL,
    document_type       varchar(50) NOT NULL,
    title               varchar(250) NOT NULL,
    owner_member_id     uuid        NOT NULL,
    review_date         date,
    status              varchar(30) NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_controlled_documents_tenant
    ON document.controlled_documents (tenant_id);
CREATE UNIQUE INDEX IF NOT EXISTS uq_controlled_documents_doc_number
    ON document.controlled_documents (tenant_id, document_number);

-- ── document.revisions ──────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS document.revisions (
    id                      uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id               uuid        NOT NULL,
    controlled_document_id  uuid        NOT NULL,
    revision_number         varchar(30) NOT NULL,
    file_object_id          uuid        NOT NULL,
    effective_date          date,
    status                  varchar(30) NOT NULL,
    approved_by_member_id   uuid
);

CREATE INDEX IF NOT EXISTS idx_revisions_tenant
    ON document.revisions (tenant_id);
CREATE INDEX IF NOT EXISTS idx_revisions_controlled_document
    ON document.revisions (controlled_document_id);

-- ── document.acknowledgements ───────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS document.acknowledgements (
    id                  uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid        NOT NULL,
    document_revision_id uuid       NOT NULL,
    tenant_member_id    uuid        NOT NULL,
    acknowledged_at     timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_acknowledgements_tenant
    ON document.acknowledgements (tenant_id);
CREATE INDEX IF NOT EXISTS idx_acknowledgements_document_revision
    ON document.acknowledgements (document_revision_id);
CREATE UNIQUE INDEX IF NOT EXISTS uq_acknowledgements_member_rev
    ON document.acknowledgements (document_revision_id, tenant_member_id);


-- ╔═══════════════════════════════════════════════════════════════════════════╗
-- ║  SAFETY                                                                  ║
-- ╚═══════════════════════════════════════════════════════════════════════════╝

-- ── safety.observations ─────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS safety.observations (
    id                      uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id               uuid        NOT NULL,
    record_id               uuid        NOT NULL UNIQUE,
    observation_type        varchar(40) NOT NULL,
    reporter_member_id      uuid        NOT NULL,
    reporter_visibility     varchar(30) NOT NULL,
    potential_impact        text,
    description             text        NOT NULL,
    immediate_action        text,
    initial_risk_level      varchar(30),
    assigned_member_id      uuid,
    due_date                date
);

CREATE INDEX IF NOT EXISTS idx_observations_tenant
    ON safety.observations (tenant_id);
CREATE INDEX IF NOT EXISTS idx_observations_reporter
    ON safety.observations (reporter_member_id);
CREATE INDEX IF NOT EXISTS idx_observations_assigned
    ON safety.observations (assigned_member_id);


-- ╔═══════════════════════════════════════════════════════════════════════════╗
-- ║  RISK                                                                    ║
-- ╚═══════════════════════════════════════════════════════════════════════════╝

-- ── risk.hazards ────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS risk.hazards (
    id              uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid        NOT NULL,
    code            varchar(50) NOT NULL,
    name            varchar(200) NOT NULL,
    category_id     uuid,
    description     text,
    status          varchar(20) NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_hazards_tenant
    ON risk.hazards (tenant_id);
CREATE INDEX IF NOT EXISTS idx_hazards_category
    ON risk.hazards (category_id);

-- ── risk.matrix_versions ────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS risk.matrix_versions (
    id                  uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid        NOT NULL,
    name                varchar(150) NOT NULL,
    version_number      int         NOT NULL,
    likelihood_scale    int         NOT NULL,
    severity_scale      int         NOT NULL,
    effective_from      date        NOT NULL,
    effective_to        date,
    status              varchar(20) NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_matrix_versions_tenant
    ON risk.matrix_versions (tenant_id);

-- ── risk.matrix_cells ───────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS risk.matrix_cells (
    id                  uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid        NOT NULL,
    matrix_version_id   uuid        NOT NULL,
    likelihood_value    smallint    NOT NULL,
    severity_value      smallint    NOT NULL,
    risk_score          int         NOT NULL,
    risk_level_code     varchar(30) NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_matrix_cells_tenant
    ON risk.matrix_cells (tenant_id);
CREATE INDEX IF NOT EXISTS idx_matrix_cells_version
    ON risk.matrix_cells (matrix_version_id);
CREATE UNIQUE INDEX IF NOT EXISTS uq_matrix_cells_level
    ON risk.matrix_cells (matrix_version_id, likelihood_value, severity_value);

-- ── risk.registers ──────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS risk.registers (
    id                  uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid        NOT NULL,
    record_id           uuid        NOT NULL UNIQUE,
    hazard_id           uuid        NOT NULL,
    activity_name       varchar(200) NOT NULL,
    risk_event          text        NOT NULL,
    owner_member_id     uuid        NOT NULL,
    review_date         date,
    status              varchar(30) NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_registers_tenant
    ON risk.registers (tenant_id);
CREATE INDEX IF NOT EXISTS idx_registers_hazard
    ON risk.registers (hazard_id);
CREATE INDEX IF NOT EXISTS idx_registers_owner
    ON risk.registers (owner_member_id);

-- ── risk.assessments ────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS risk.assessments (
    id                      uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id               uuid        NOT NULL,
    risk_register_id        uuid        NOT NULL,
    matrix_version_id       uuid        NOT NULL,
    assessment_type         varchar(20) NOT NULL,
    sequence_number         int         NOT NULL,
    likelihood_value        smallint    NOT NULL,
    severity_value          smallint    NOT NULL,
    risk_score              int         NOT NULL,
    risk_level_code         varchar(30) NOT NULL,
    assessed_by_member_id   uuid        NOT NULL,
    assessed_at             timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_assessments_tenant
    ON risk.assessments (tenant_id);
CREATE INDEX IF NOT EXISTS idx_assessments_register
    ON risk.assessments (risk_register_id);
CREATE INDEX IF NOT EXISTS idx_assessments_matrix_version
    ON risk.assessments (matrix_version_id);
CREATE INDEX IF NOT EXISTS idx_assessments_assessor
    ON risk.assessments (assessed_by_member_id);

-- ── risk.controls ───────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS risk.controls (
    id                      uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id               uuid        NOT NULL,
    risk_register_id        uuid        NOT NULL,
    control_type            varchar(40) NOT NULL,
    control_stage           varchar(20) NOT NULL,
    description             text        NOT NULL,
    owner_member_id         uuid,
    due_date                date,
    status                  varchar(30) NOT NULL,
    effectiveness_rating    smallint
);

CREATE INDEX IF NOT EXISTS idx_controls_tenant
    ON risk.controls (tenant_id);
CREATE INDEX IF NOT EXISTS idx_controls_register
    ON risk.controls (risk_register_id);
CREATE INDEX IF NOT EXISTS idx_controls_owner
    ON risk.controls (owner_member_id);

-- ── risk.reviews ────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS risk.reviews (
    id                      uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id               uuid        NOT NULL,
    risk_register_id        uuid        NOT NULL,
    reviewed_by_member_id   uuid        NOT NULL,
    reviewed_at             timestamptz NOT NULL,
    decision                varchar(30) NOT NULL,
    comment                 text
);

CREATE INDEX IF NOT EXISTS idx_reviews_tenant
    ON risk.reviews (tenant_id);
CREATE INDEX IF NOT EXISTS idx_reviews_register
    ON risk.reviews (risk_register_id);
CREATE INDEX IF NOT EXISTS idx_reviews_reviewer
    ON risk.reviews (reviewed_by_member_id);


-- ╔═══════════════════════════════════════════════════════════════════════════╗
-- ║  INCIDENT                                                                ║
-- ╚═══════════════════════════════════════════════════════════════════════════╝

-- ── incident.incidents ──────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS incident.incidents (
    id                      uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id               uuid        NOT NULL,
    record_id               uuid        NOT NULL UNIQUE,
    incident_type_id        uuid        NOT NULL,
    severity_id             uuid        NOT NULL,
    occurred_at             timestamptz NOT NULL,
    reported_at             timestamptz NOT NULL,
    reported_by_member_id   uuid        NOT NULL,
    description             text        NOT NULL,
    immediate_action        text,
    classification_status   varchar(30)
);

CREATE INDEX IF NOT EXISTS idx_incidents_tenant
    ON incident.incidents (tenant_id);
CREATE INDEX IF NOT EXISTS idx_incidents_type
    ON incident.incidents (incident_type_id);
CREATE INDEX IF NOT EXISTS idx_incidents_severity
    ON incident.incidents (severity_id);
CREATE INDEX IF NOT EXISTS idx_incidents_reporter
    ON incident.incidents (reported_by_member_id);
CREATE INDEX IF NOT EXISTS idx_incidents_occurred
    ON incident.incidents (occurred_at);

-- ── incident.involved_people ────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS incident.involved_people (
    id                      uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id               uuid        NOT NULL,
    incident_id             uuid        NOT NULL,
    person_id               uuid,
    external_person_name    varchar(200),
    involvement_type        varchar(40) NOT NULL,
    injury_classification_id uuid,
    lost_work_days          int
);

CREATE INDEX IF NOT EXISTS idx_involved_people_tenant
    ON incident.involved_people (tenant_id);
CREATE INDEX IF NOT EXISTS idx_involved_people_incident
    ON incident.involved_people (incident_id);
CREATE INDEX IF NOT EXISTS idx_involved_people_person
    ON incident.involved_people (person_id);

-- ── incident.investigations ─────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS incident.investigations (
    id                              uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id                       uuid        NOT NULL,
    incident_id                     uuid        NOT NULL,
    lead_investigator_member_id     uuid        NOT NULL,
    method                          varchar(80),
    summary                         text,
    status                          varchar(30) NOT NULL,
    started_at                      timestamptz,
    completed_at                    timestamptz
);

CREATE INDEX IF NOT EXISTS idx_investigations_tenant
    ON incident.investigations (tenant_id);
CREATE INDEX IF NOT EXISTS idx_investigations_incident
    ON incident.investigations (incident_id);
CREATE INDEX IF NOT EXISTS idx_investigations_lead
    ON incident.investigations (lead_investigator_member_id);

-- ── incident.investigation_team ─────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS incident.investigation_team (
    id                  uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid        NOT NULL,
    investigation_id    uuid        NOT NULL,
    tenant_member_id    uuid        NOT NULL,
    team_role           varchar(80)
);

CREATE INDEX IF NOT EXISTS idx_investigation_team_tenant
    ON incident.investigation_team (tenant_id);
CREATE INDEX IF NOT EXISTS idx_investigation_team_investigation
    ON incident.investigation_team (investigation_id);
CREATE INDEX IF NOT EXISTS idx_investigation_team_member
    ON incident.investigation_team (tenant_member_id);

-- ── incident.root_causes ────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS incident.root_causes (
    id                  uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid        NOT NULL,
    investigation_id    uuid        NOT NULL,
    cause_type          varchar(30) NOT NULL,
    category_id         uuid,
    description         text        NOT NULL,
    evidence_summary    text
);

CREATE INDEX IF NOT EXISTS idx_root_causes_tenant
    ON incident.root_causes (tenant_id);
CREATE INDEX IF NOT EXISTS idx_root_causes_investigation
    ON incident.root_causes (investigation_id);
CREATE INDEX IF NOT EXISTS idx_root_causes_category
    ON incident.root_causes (category_id);

-- ── incident.classification_reviews ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS incident.classification_reviews (
    id                  uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid        NOT NULL,
    incident_id         uuid        NOT NULL,
    reviewer_member_id  uuid        NOT NULL,
    classification_json jsonb       NOT NULL,
    decision            varchar(30) NOT NULL,
    reviewed_at         timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_classification_reviews_tenant
    ON incident.classification_reviews (tenant_id);
CREATE INDEX IF NOT EXISTS idx_classification_reviews_incident
    ON incident.classification_reviews (incident_id);
CREATE INDEX IF NOT EXISTS idx_classification_reviews_reviewer
    ON incident.classification_reviews (reviewer_member_id);


-- ╔═══════════════════════════════════════════════════════════════════════════╗
-- ║  CAPA                                                                    ║
-- ╚═══════════════════════════════════════════════════════════════════════════╝

-- ── capa.actions ────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS capa.actions (
    id                      uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id               uuid        NOT NULL,
    record_id               uuid        NOT NULL UNIQUE,
    action_type             varchar(20) NOT NULL,
    description             text        NOT NULL,
    owner_member_id         uuid        NOT NULL,
    priority                varchar(20) NOT NULL,
    due_date                date        NOT NULL,
    progress_percentage     smallint    NOT NULL,
    verification_required   boolean     NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_capa_actions_tenant
    ON capa.actions (tenant_id);
CREATE INDEX IF NOT EXISTS idx_capa_actions_owner
    ON capa.actions (owner_member_id);

-- ── capa.sources ────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS capa.sources (
    id                  uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid        NOT NULL,
    action_id           uuid        NOT NULL,
    source_record_id    uuid        NOT NULL,
    source_role         varchar(40)
);

CREATE INDEX IF NOT EXISTS idx_capa_sources_tenant
    ON capa.sources (tenant_id);
CREATE INDEX IF NOT EXISTS idx_capa_sources_action
    ON capa.sources (action_id);
CREATE INDEX IF NOT EXISTS idx_capa_sources_source_record
    ON capa.sources (source_record_id);

-- ── capa.updates ────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS capa.updates (
    id                      uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id               uuid        NOT NULL,
    action_id               uuid        NOT NULL,
    progress_percentage     smallint    NOT NULL,
    note                    text        NOT NULL,
    updated_by_member_id    uuid        NOT NULL,
    updated_at              timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_capa_updates_tenant
    ON capa.updates (tenant_id);
CREATE INDEX IF NOT EXISTS idx_capa_updates_action
    ON capa.updates (action_id);
CREATE INDEX IF NOT EXISTS idx_capa_updates_updater
    ON capa.updates (updated_by_member_id);

-- ── capa.verifications ──────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS capa.verifications (
    id                      uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id               uuid        NOT NULL,
    action_id               uuid        NOT NULL,
    verifier_member_id      uuid        NOT NULL,
    result                  varchar(30) NOT NULL,
    comment                 text,
    verified_at             timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_capa_verifications_tenant
    ON capa.verifications (tenant_id);
CREATE INDEX IF NOT EXISTS idx_capa_verifications_action
    ON capa.verifications (action_id);
CREATE INDEX IF NOT EXISTS idx_capa_verifications_verifier
    ON capa.verifications (verifier_member_id);


-- ╔═══════════════════════════════════════════════════════════════════════════╗
-- ║  FOREIGN KEYS — Intra-schema                                             ║
-- ╚═══════════════════════════════════════════════════════════════════════════╝

-- ── document ────────────────────────────────────────────────────────────────
ALTER TABLE document.revisions
    ADD CONSTRAINT fk_revisions_controlled_doc
    FOREIGN KEY (controlled_document_id) REFERENCES document.controlled_documents(id);

ALTER TABLE document.acknowledgements
    ADD CONSTRAINT fk_acknowledgements_revision
    FOREIGN KEY (document_revision_id) REFERENCES document.revisions(id);

-- ── risk ────────────────────────────────────────────────────────────────────
ALTER TABLE risk.matrix_cells
    ADD CONSTRAINT fk_matrix_cells_version
    FOREIGN KEY (matrix_version_id) REFERENCES risk.matrix_versions(id);

ALTER TABLE risk.registers
    ADD CONSTRAINT fk_registers_hazard
    FOREIGN KEY (hazard_id) REFERENCES risk.hazards(id);

ALTER TABLE risk.assessments
    ADD CONSTRAINT fk_assessments_register
    FOREIGN KEY (risk_register_id) REFERENCES risk.registers(id);

ALTER TABLE risk.assessments
    ADD CONSTRAINT fk_assessments_matrix_version
    FOREIGN KEY (matrix_version_id) REFERENCES risk.matrix_versions(id);

ALTER TABLE risk.controls
    ADD CONSTRAINT fk_controls_register
    FOREIGN KEY (risk_register_id) REFERENCES risk.registers(id);

ALTER TABLE risk.reviews
    ADD CONSTRAINT fk_reviews_register
    FOREIGN KEY (risk_register_id) REFERENCES risk.registers(id);

-- ── incident ────────────────────────────────────────────────────────────────
ALTER TABLE incident.involved_people
    ADD CONSTRAINT fk_involved_people_incident
    FOREIGN KEY (incident_id) REFERENCES incident.incidents(id);

ALTER TABLE incident.investigations
    ADD CONSTRAINT fk_investigations_incident
    FOREIGN KEY (incident_id) REFERENCES incident.incidents(id);

ALTER TABLE incident.investigation_team
    ADD CONSTRAINT fk_inv_team_investigation
    FOREIGN KEY (investigation_id) REFERENCES incident.investigations(id);

ALTER TABLE incident.root_causes
    ADD CONSTRAINT fk_root_causes_investigation
    FOREIGN KEY (investigation_id) REFERENCES incident.investigations(id);

ALTER TABLE incident.classification_reviews
    ADD CONSTRAINT fk_class_reviews_incident
    FOREIGN KEY (incident_id) REFERENCES incident.incidents(id);

-- ── capa ────────────────────────────────────────────────────────────────────
ALTER TABLE capa.sources
    ADD CONSTRAINT fk_capa_sources_action
    FOREIGN KEY (action_id) REFERENCES capa.actions(id);

ALTER TABLE capa.updates
    ADD CONSTRAINT fk_capa_updates_action
    FOREIGN KEY (action_id) REFERENCES capa.actions(id);

ALTER TABLE capa.verifications
    ADD CONSTRAINT fk_capa_verifications_action
    FOREIGN KEY (action_id) REFERENCES capa.actions(id);


-- ╔═══════════════════════════════════════════════════════════════════════════╗
-- ║  FOREIGN KEYS — Cross-schema (→ platform, org, iam)                     ║
-- ╚═══════════════════════════════════════════════════════════════════════════╝

-- ── document → platform.file_objects, platform.records, iam ─────────────────
ALTER TABLE document.controlled_documents
    ADD CONSTRAINT fk_ctrl_doc_record
    FOREIGN KEY (record_id) REFERENCES platform.records(id);

ALTER TABLE document.controlled_documents
    ADD CONSTRAINT fk_ctrl_doc_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members(id);

ALTER TABLE document.revisions
    ADD CONSTRAINT fk_revisions_file_object
    FOREIGN KEY (file_object_id) REFERENCES platform.file_objects(id);

ALTER TABLE document.revisions
    ADD CONSTRAINT fk_revisions_approved_by
    FOREIGN KEY (approved_by_member_id) REFERENCES iam.tenant_members(id);

ALTER TABLE document.acknowledgements
    ADD CONSTRAINT fk_ack_member
    FOREIGN KEY (tenant_member_id) REFERENCES iam.tenant_members(id);

-- ── safety → platform.records, iam ──────────────────────────────────────────
ALTER TABLE safety.observations
    ADD CONSTRAINT fk_obs_record
    FOREIGN KEY (record_id) REFERENCES platform.records(id);

ALTER TABLE safety.observations
    ADD CONSTRAINT fk_obs_reporter
    FOREIGN KEY (reporter_member_id) REFERENCES iam.tenant_members(id);

ALTER TABLE safety.observations
    ADD CONSTRAINT fk_obs_assigned
    FOREIGN KEY (assigned_member_id) REFERENCES iam.tenant_members(id);

-- ── risk → platform.lookup_values, platform.records, iam ────────────────────
ALTER TABLE risk.hazards
    ADD CONSTRAINT fk_hazards_category
    FOREIGN KEY (category_id) REFERENCES platform.lookup_values(id);

ALTER TABLE risk.registers
    ADD CONSTRAINT fk_registers_record
    FOREIGN KEY (record_id) REFERENCES platform.records(id);

ALTER TABLE risk.registers
    ADD CONSTRAINT fk_registers_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members(id);

ALTER TABLE risk.assessments
    ADD CONSTRAINT fk_assessments_assessor
    FOREIGN KEY (assessed_by_member_id) REFERENCES iam.tenant_members(id);

ALTER TABLE risk.controls
    ADD CONSTRAINT fk_controls_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members(id);

ALTER TABLE risk.reviews
    ADD CONSTRAINT fk_reviews_reviewer
    FOREIGN KEY (reviewed_by_member_id) REFERENCES iam.tenant_members(id);

-- ── incident → platform.lookup_values, platform.records, org.people, iam ───
ALTER TABLE incident.incidents
    ADD CONSTRAINT fk_incidents_record
    FOREIGN KEY (record_id) REFERENCES platform.records(id);

ALTER TABLE incident.incidents
    ADD CONSTRAINT fk_incidents_type
    FOREIGN KEY (incident_type_id) REFERENCES platform.lookup_values(id);

ALTER TABLE incident.incidents
    ADD CONSTRAINT fk_incidents_severity
    FOREIGN KEY (severity_id) REFERENCES platform.lookup_values(id);

ALTER TABLE incident.incidents
    ADD CONSTRAINT fk_incidents_reporter
    FOREIGN KEY (reported_by_member_id) REFERENCES iam.tenant_members(id);

ALTER TABLE incident.involved_people
    ADD CONSTRAINT fk_involved_person
    FOREIGN KEY (person_id) REFERENCES org.people(id);

ALTER TABLE incident.involved_people
    ADD CONSTRAINT fk_involved_injury_class
    FOREIGN KEY (injury_classification_id) REFERENCES platform.lookup_values(id);

ALTER TABLE incident.investigations
    ADD CONSTRAINT fk_invest_lead
    FOREIGN KEY (lead_investigator_member_id) REFERENCES iam.tenant_members(id);

ALTER TABLE incident.investigation_team
    ADD CONSTRAINT fk_inv_team_member
    FOREIGN KEY (tenant_member_id) REFERENCES iam.tenant_members(id);

ALTER TABLE incident.root_causes
    ADD CONSTRAINT fk_root_cause_category
    FOREIGN KEY (category_id) REFERENCES platform.lookup_values(id);

ALTER TABLE incident.classification_reviews
    ADD CONSTRAINT fk_class_review_reviewer
    FOREIGN KEY (reviewer_member_id) REFERENCES iam.tenant_members(id);

-- ── capa → platform.records, iam ────────────────────────────────────────────
ALTER TABLE capa.actions
    ADD CONSTRAINT fk_capa_actions_record
    FOREIGN KEY (record_id) REFERENCES platform.records(id);

ALTER TABLE capa.actions
    ADD CONSTRAINT fk_capa_actions_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members(id);

ALTER TABLE capa.sources
    ADD CONSTRAINT fk_capa_sources_source_record
    FOREIGN KEY (source_record_id) REFERENCES platform.records(id);

ALTER TABLE capa.updates
    ADD CONSTRAINT fk_capa_updates_updater
    FOREIGN KEY (updated_by_member_id) REFERENCES iam.tenant_members(id);

ALTER TABLE capa.verifications
    ADD CONSTRAINT fk_capa_verify_verifier
    FOREIGN KEY (verifier_member_id) REFERENCES iam.tenant_members(id);


-- ============================================================================
-- END OF WAVE 2 — Core EHS
-- ============================================================================
