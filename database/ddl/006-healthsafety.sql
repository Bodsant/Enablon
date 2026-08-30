-- HealthSafety module schemas and tables used by the running application.
-- Idempotent: safe to run on every provisioning pass.
CREATE SCHEMA IF NOT EXISTS chemical;
CREATE SCHEMA IF NOT EXISTS ppe;
CREATE SCHEMA IF NOT EXISTS health;
CREATE SCHEMA IF NOT EXISTS environment;
CREATE SCHEMA IF NOT EXISTS sustainability;

-- chemical.products: core catalogue row for a chemical product.
CREATE TABLE IF NOT EXISTS chemical.products (
    id                         uuid PRIMARY KEY,
    tenant_id                  uuid NOT NULL,
    record_id                  uuid NOT NULL,
    product_code               text,
    product_name               text NOT NULL,
    supplier_name              text,
    hazard_classification_json text,
    owner_member_id            uuid,
    status                     text NOT NULL DEFAULT 'Active'
);

CREATE INDEX IF NOT EXISTS idx_chemical_products_tenant
    ON chemical.products (tenant_id);
CREATE INDEX IF NOT EXISTS idx_chemical_products_record
    ON chemical.products (record_id);

-- chemical.inventory: stock of a chemical product at a location.
CREATE TABLE IF NOT EXISTS chemical.inventory (
    id               uuid PRIMARY KEY,
    tenant_id        uuid NOT NULL,
    chemical_product_id uuid NOT NULL,
    location_id      uuid NOT NULL,
    quantity         numeric,
    unit             text,
    storage_condition text,
    expiry_date      date
);

CREATE INDEX IF NOT EXISTS idx_chemical_inventory_tenant
    ON chemical.inventory (tenant_id);
CREATE INDEX IF NOT EXISTS idx_chemical_inventory_product
    ON chemical.inventory (chemical_product_id);

-- chemical.sds_revisions: safety data sheet document revisions per product.
CREATE TABLE IF NOT EXISTS chemical.sds_revisions (
    id               uuid PRIMARY KEY,
    tenant_id        uuid NOT NULL,
    chemical_product_id uuid NOT NULL,
    revision_number  text NOT NULL,
    effective_date   date,
    file_object_id   uuid NOT NULL,
    language         text,
    status           text NOT NULL DEFAULT 'Active'
);

CREATE INDEX IF NOT EXISTS idx_chemical_sds_product
    ON chemical.sds_revisions (chemical_product_id);

-- chemical.exposure_controls: exposure control measures per product.
CREATE TABLE IF NOT EXISTS chemical.exposure_controls (
    id                 uuid PRIMARY KEY,
    tenant_id          uuid NOT NULL,
    chemical_product_id uuid NOT NULL,
    control_type       text NOT NULL,
    description        text NOT NULL,
    source_record_id   uuid
);

CREATE INDEX IF NOT EXISTS idx_chemical_exposure_tenant
    ON chemical.exposure_controls (tenant_id);
CREATE INDEX IF NOT EXISTS idx_chemical_exposure_product
    ON chemical.exposure_controls (chemical_product_id);

-- chemical.storage_inspections: inspection records per inventory line.
CREATE TABLE IF NOT EXISTS chemical.storage_inspections (
    id                    uuid PRIMARY KEY,
    tenant_id             uuid NOT NULL,
    record_id             uuid NOT NULL,
    chemical_inventory_id uuid NOT NULL,
    inspected_by_member_id uuid NOT NULL,
    inspected_at          timestamptz NOT NULL,
    result                text NOT NULL,
    next_review_date      date
);

CREATE INDEX IF NOT EXISTS idx_chemical_storage_tenant
    ON chemical.storage_inspections (tenant_id);
CREATE INDEX IF NOT EXISTS idx_chemical_storage_inventory
    ON chemical.storage_inspections (chemical_inventory_id);

-- ppe.catalog: master list of PPE items.
CREATE TABLE IF NOT EXISTS ppe.catalog (
    id                        uuid PRIMARY KEY,
    tenant_id                 uuid NOT NULL,
    code                      varchar(50) NOT NULL,
    name                      varchar(200) NOT NULL,
    ppe_category              varchar(60),
    inspection_interval_days  integer,
    replacement_interval_days integer,
    status                    varchar(20) NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_ppe_catalog_tenant
    ON ppe.catalog (tenant_id);

-- ppe.requirements: mandatory/suggested PPE per job or permit context.
CREATE TABLE IF NOT EXISTS ppe.requirements (
    id               uuid PRIMARY KEY,
    tenant_id        uuid NOT NULL,
    ppe_catalog_id   uuid NOT NULL,
    source_record_id uuid,
    permit_type_id   uuid,
    is_mandatory     boolean NOT NULL DEFAULT false,
    notes            text
);

CREATE INDEX IF NOT EXISTS idx_ppe_requirements_tenant
    ON ppe.requirements (tenant_id);
CREATE INDEX IF NOT EXISTS idx_ppe_requirements_catalog
    ON ppe.requirements (ppe_catalog_id);

-- ppe.inventory: PPE stock per site.
CREATE TABLE IF NOT EXISTS ppe.inventory (
    id              uuid PRIMARY KEY,
    tenant_id       uuid NOT NULL,
    ppe_catalog_id  uuid NOT NULL,
    site_id         uuid NOT NULL,
    serial_number   varchar(100),
    quantity_on_hand integer,
    condition       varchar(30),
    status          varchar(30) NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_ppe_inventory_tenant
    ON ppe.inventory (tenant_id);
CREATE INDEX IF NOT EXISTS idx_ppe_inventory_catalog
    ON ppe.inventory (ppe_catalog_id);

-- ppe.assignments: PPE issued to a person.
CREATE TABLE IF NOT EXISTS ppe.assignments (
    id                  uuid PRIMARY KEY,
    tenant_id           uuid NOT NULL,
    ppe_inventory_id    uuid NOT NULL,
    person_id           uuid NOT NULL,
    issued_at           timestamptz NOT NULL,
    issued_by_member_id uuid NOT NULL,
    returned_at         timestamptz,
    condition_on_return varchar(30)
);

CREATE INDEX IF NOT EXISTS idx_ppe_assignments_tenant
    ON ppe.assignments (tenant_id);
CREATE INDEX IF NOT EXISTS idx_ppe_assignments_inventory
    ON ppe.assignments (ppe_inventory_id);

-- ppe.inspections: periodic inspection of PPE items.
CREATE TABLE IF NOT EXISTS ppe.inspections (
    id                    uuid PRIMARY KEY,
    tenant_id             uuid NOT NULL,
    ppe_inventory_id      uuid NOT NULL,
    inspected_by_member_id uuid NOT NULL,
    inspected_at          timestamptz NOT NULL,
    condition             varchar(30) NOT NULL,
    result                varchar(30) NOT NULL,
    next_due_date         date
);

CREATE INDEX IF NOT EXISTS idx_ppe_inspections_tenant
    ON ppe.inspections (tenant_id);
CREATE INDEX IF NOT EXISTS idx_ppe_inspections_inventory
    ON ppe.inspections (ppe_inventory_id);

-- ppe.replacements: replacement requests for PPE assignments.
CREATE TABLE IF NOT EXISTS ppe.replacements (
    id                 uuid PRIMARY KEY,
    tenant_id          uuid NOT NULL,
    ppe_assignment_id  uuid NOT NULL,
    replacement_reason text NOT NULL,
    requested_at       timestamptz NOT NULL,
    completed_at       timestamptz
);

CREATE INDEX IF NOT EXISTS idx_ppe_replacements_tenant
    ON ppe.replacements (tenant_id);
CREATE INDEX IF NOT EXISTS idx_ppe_replacements_assignment
    ON ppe.replacements (ppe_assignment_id);

-- environment.parameters: measurable environmental parameters.
CREATE TABLE IF NOT EXISTS environment.parameters (
    id           uuid PRIMARY KEY,
    tenant_id    uuid NOT NULL,
    code         varchar(50) NOT NULL,
    name         varchar(200) NOT NULL,
    category     varchar(60) NOT NULL,
    default_unit varchar(30),
    status       varchar(20) NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_env_parameters_tenant
    ON environment.parameters (tenant_id);
CREATE INDEX IF NOT EXISTS idx_env_parameters_category
    ON environment.parameters (category);

-- environment.sources: emission/exposure sources.
CREATE TABLE IF NOT EXISTS environment.sources (
    id               uuid PRIMARY KEY,
    tenant_id        uuid NOT NULL,
    site_id          uuid NOT NULL,
    location_id      uuid,
    source_type      varchar(60) NOT NULL,
    name             varchar(200) NOT NULL,
    permit_reference varchar(100)
);

CREATE INDEX IF NOT EXISTS idx_env_sources_tenant
    ON environment.sources (tenant_id);
CREATE INDEX IF NOT EXISTS idx_env_sources_site
    ON environment.sources (site_id);

-- environment.measurements: monitoring results.
CREATE TABLE IF NOT EXISTS environment.measurements (
    id                  uuid PRIMARY KEY,
    tenant_id           uuid NOT NULL,
    monitoring_record_id uuid NOT NULL,
    parameter_id        uuid NOT NULL,
    measured_at         timestamptz NOT NULL,
    result_value        numeric(18,6),
    unit                varchar(30),
    limit_value         numeric(18,6),
    target_value        numeric(18,6),
    quality_flag        varchar(30),
    compliance_status   varchar(30)
);

CREATE INDEX IF NOT EXISTS idx_env_measurements_tenant
    ON environment.measurements (tenant_id);
CREATE INDEX IF NOT EXISTS idx_env_measurements_parameter
    ON environment.measurements (parameter_id);

-- =====================================================================
-- SafetyRisk module: risk schema (Trello Sprint 11 - Hazard & Risk Backend)
-- =====================================================================
CREATE SCHEMA IF NOT EXISTS risk;

CREATE TABLE IF NOT EXISTS risk.matrix_versions (
    id               uuid PRIMARY KEY,
    tenant_id        uuid NOT NULL,
    name             text NOT NULL,
    version_number   integer NOT NULL,
    likelihood_scale integer NOT NULL,
    severity_scale   integer NOT NULL,
    effective_from   date NOT NULL,
    effective_to     date,
    status           text NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_risk_matrix_versions_tenant
    ON risk.matrix_versions (tenant_id);

CREATE TABLE IF NOT EXISTS risk.matrix_cells (
    id                uuid PRIMARY KEY,
    tenant_id         uuid NOT NULL,
    matrix_version_id uuid NOT NULL,
    likelihood_value  smallint NOT NULL,
    severity_value    smallint NOT NULL,
    risk_score        integer NOT NULL,
    risk_level_code   text NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_risk_matrix_cells_tenant
    ON risk.matrix_cells (tenant_id);
CREATE INDEX IF NOT EXISTS idx_risk_matrix_cells_version
    ON risk.matrix_cells (matrix_version_id);

CREATE TABLE IF NOT EXISTS risk.hazards (
    id          uuid PRIMARY KEY,
    tenant_id   uuid NOT NULL,
    code        text NOT NULL,
    name        text NOT NULL,
    category_id uuid,
    description text,
    status      text NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_risk_hazards_tenant
    ON risk.hazards (tenant_id);

CREATE TABLE IF NOT EXISTS risk.registers (
    id             uuid PRIMARY KEY,
    tenant_id      uuid NOT NULL,
    record_id      uuid NOT NULL,
    hazard_id      uuid NOT NULL,
    activity_name  text NOT NULL,
    risk_event     text NOT NULL,
    owner_member_id uuid NOT NULL,
    review_date    date,
    status         text NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_risk_registers_tenant
    ON risk.registers (tenant_id);
CREATE INDEX IF NOT EXISTS idx_risk_registers_hazard
    ON risk.registers (hazard_id);

CREATE TABLE IF NOT EXISTS risk.assessments (
    id                 uuid PRIMARY KEY,
    tenant_id          uuid NOT NULL,
    risk_register_id   uuid NOT NULL,
    matrix_version_id  uuid NOT NULL,
    assessment_type    text NOT NULL,
    sequence_number    integer NOT NULL,
    likelihood_value   smallint NOT NULL,
    severity_value     smallint NOT NULL,
    risk_score         integer NOT NULL,
    risk_level_code    text NOT NULL,
    assessed_by_member_id uuid NOT NULL,
    assessed_at        timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_risk_assessments_tenant
    ON risk.assessments (tenant_id);
CREATE INDEX IF NOT EXISTS idx_risk_assessments_register
    ON risk.assessments (risk_register_id);

CREATE TABLE IF NOT EXISTS risk.controls (
    id                  uuid PRIMARY KEY,
    tenant_id           uuid NOT NULL,
    risk_register_id    uuid NOT NULL,
    control_type        text NOT NULL,
    control_stage       text NOT NULL,
    description         text NOT NULL,
    owner_member_id     uuid,
    due_date            date,
    status              text NOT NULL,
    effectiveness_rating smallint
);

CREATE INDEX IF NOT EXISTS idx_risk_controls_tenant
    ON risk.controls (tenant_id);
CREATE INDEX IF NOT EXISTS idx_risk_controls_register
    ON risk.controls (risk_register_id);

CREATE TABLE IF NOT EXISTS risk.reviews (
    id                    uuid PRIMARY KEY,
    tenant_id             uuid NOT NULL,
    risk_register_id      uuid NOT NULL,
    reviewed_by_member_id uuid NOT NULL,
    reviewed_at           timestamptz NOT NULL,
    decision              text NOT NULL,
    comment               text
);

CREATE INDEX IF NOT EXISTS idx_risk_reviews_tenant
    ON risk.reviews (tenant_id);
CREATE INDEX IF NOT EXISTS idx_risk_reviews_register
    ON risk.reviews (risk_register_id);

-- =====================================================================
-- SafetyRisk module: incident schema (Trello Sprint 13 - Incident & CAPA)
-- =====================================================================
CREATE SCHEMA IF NOT EXISTS incident;

CREATE TABLE IF NOT EXISTS incident.incidents (
    id                      uuid PRIMARY KEY,
    tenant_id               uuid NOT NULL,
    record_id               uuid NOT NULL,
    incident_type_id        uuid NOT NULL,
    severity_id             uuid NOT NULL,
    occurred_at             timestamptz NOT NULL,
    reported_at             timestamptz NOT NULL,
    reported_by_member_id   uuid NOT NULL,
    description             text NOT NULL,
    immediate_action        text,
    classification_status   text
);

CREATE INDEX IF NOT EXISTS idx_incidents_tenant
    ON incident.incidents (tenant_id);

CREATE TABLE IF NOT EXISTS incident.involved_people (
    id                         uuid PRIMARY KEY,
    tenant_id                  uuid NOT NULL,
    incident_id                uuid NOT NULL,
    person_id                  uuid,
    external_person_name       text,
    involvement_type           text NOT NULL,
    injury_classification_id   uuid,
    lost_work_days             integer
);

CREATE INDEX IF NOT EXISTS idx_involved_people_tenant
    ON incident.involved_people (tenant_id);
CREATE INDEX IF NOT EXISTS idx_involved_people_incident
    ON incident.involved_people (incident_id);

CREATE TABLE IF NOT EXISTS incident.investigations (
    id                          uuid PRIMARY KEY,
    tenant_id                   uuid NOT NULL,
    incident_id                 uuid NOT NULL,
    lead_investigator_member_id uuid NOT NULL,
    method                      text,
    summary                     text,
    status                      text NOT NULL,
    started_at                  timestamptz,
    completed_at                timestamptz
);

CREATE INDEX IF NOT EXISTS idx_investigations_tenant
    ON incident.investigations (tenant_id);
CREATE INDEX IF NOT EXISTS idx_investigations_incident
    ON incident.investigations (incident_id);

CREATE TABLE IF NOT EXISTS incident.investigation_team (
    id                uuid PRIMARY KEY,
    tenant_id         uuid NOT NULL,
    investigation_id  uuid NOT NULL,
    tenant_member_id  uuid NOT NULL,
    team_role         text
);

CREATE INDEX IF NOT EXISTS idx_investigation_team_tenant
    ON incident.investigation_team (tenant_id);

CREATE TABLE IF NOT EXISTS incident.root_causes (
    id               uuid PRIMARY KEY,
    tenant_id        uuid NOT NULL,
    investigation_id uuid NOT NULL,
    cause_type       text NOT NULL,
    category_id      uuid,
    description      text NOT NULL,
    evidence_summary text
);

CREATE INDEX IF NOT EXISTS idx_root_causes_tenant
    ON incident.root_causes (tenant_id);
CREATE INDEX IF NOT EXISTS idx_root_causes_investigation
    ON incident.root_causes (investigation_id);

CREATE TABLE IF NOT EXISTS incident.classification_reviews (
    id                    uuid PRIMARY KEY,
    tenant_id             uuid NOT NULL,
    incident_id           uuid NOT NULL,
    reviewer_member_id    uuid NOT NULL,
    classification_json   text NOT NULL,
    decision              text NOT NULL,
    reviewed_at           timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_classification_reviews_tenant
    ON incident.classification_reviews (tenant_id);

-- =====================================================================
-- SafetyRisk module: capa schema
-- =====================================================================
CREATE SCHEMA IF NOT EXISTS capa;

CREATE TABLE IF NOT EXISTS capa.actions (
    id                   uuid PRIMARY KEY,
    tenant_id            uuid NOT NULL,
    record_id            uuid NOT NULL,
    action_type          text NOT NULL,
    description          text NOT NULL,
    owner_member_id      uuid NOT NULL,
    priority             text NOT NULL,
    due_date             date NOT NULL,
    progress_percentage  smallint NOT NULL,
    verification_required boolean NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_capa_actions_tenant
    ON capa.actions (tenant_id);

CREATE TABLE IF NOT EXISTS capa.sources (
    id               uuid PRIMARY KEY,
    tenant_id        uuid NOT NULL,
    action_id        uuid NOT NULL,
    source_record_id uuid NOT NULL,
    source_role      text
);

CREATE INDEX IF NOT EXISTS idx_capa_sources_tenant
    ON capa.sources (tenant_id);

CREATE TABLE IF NOT EXISTS capa.updates (
    id                    uuid PRIMARY KEY,
    tenant_id             uuid NOT NULL,
    action_id             uuid NOT NULL,
    progress_percentage   smallint NOT NULL,
    note                  text NOT NULL,
    updated_by_member_id  uuid NOT NULL,
    updated_at            timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_capa_updates_tenant
    ON capa.updates (tenant_id);

CREATE TABLE IF NOT EXISTS capa.verifications (
    id                uuid PRIMARY KEY,
    tenant_id         uuid NOT NULL,
    action_id         uuid NOT NULL,
    verifier_member_id uuid NOT NULL,
    result            text NOT NULL,
    comment           text,
    verified_at       timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_capa_verifications_tenant
    ON capa.verifications (tenant_id);

-- =====================================================================
-- WorkControl module: inspection schema (Trello Sprint 15)
-- =====================================================================
CREATE SCHEMA IF NOT EXISTS inspection;

CREATE TABLE IF NOT EXISTS inspection.inspections (
    id                  uuid PRIMARY KEY,
    tenant_id           uuid NOT NULL,
    record_id           uuid NOT NULL,
    schedule_id         uuid,
    template_version_id uuid,
    inspector_member_id uuid NOT NULL,
    planned_at          timestamptz,
    started_at          timestamptz,
    completed_at        timestamptz,
    compliance_percentage numeric(5,2)
);

CREATE INDEX IF NOT EXISTS idx_inspections_tenant
    ON inspection.inspections (tenant_id);

CREATE TABLE IF NOT EXISTS inspection.findings (
    id                 uuid PRIMARY KEY,
    tenant_id          uuid NOT NULL,
    record_id          uuid NOT NULL,
    inspection_id      uuid NOT NULL,
    response_id        uuid,
    classification     text,
    severity_id        uuid,
    description        text NOT NULL,
    owner_member_id    uuid
);

CREATE INDEX IF NOT EXISTS idx_inspection_findings_tenant
    ON inspection.findings (tenant_id);
CREATE INDEX IF NOT EXISTS idx_inspection_findings_inspection
    ON inspection.findings (inspection_id);

CREATE TABLE IF NOT EXISTS inspection.schedules (
    id              uuid PRIMARY KEY,
    tenant_id       uuid NOT NULL,
    name            text NOT NULL,
    frequency       text,
    next_due_date   date,
    status          text NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_inspection_schedules_tenant
    ON inspection.schedules (tenant_id);

-- =====================================================================
-- WorkControl module: audit schema
-- =====================================================================
CREATE SCHEMA IF NOT EXISTS audit;

CREATE TABLE IF NOT EXISTS audit.programs (
    id             uuid PRIMARY KEY,
    tenant_id      uuid NOT NULL,
    record_id      uuid NOT NULL,
    name           text NOT NULL,
    period_start   date,
    period_end     date,
    owner_member_id uuid NOT NULL,
    status         text NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_audit_programs_tenant
    ON audit.programs (tenant_id);

CREATE TABLE IF NOT EXISTS audit.audits (
    id                  uuid PRIMARY KEY,
    tenant_id           uuid NOT NULL,
    record_id           uuid NOT NULL,
    audit_program_id    uuid,
    checklist_template_id uuid,
    audit_type          text NOT NULL,
    scope_text          text NOT NULL,
    criteria_text       text,
    lead_auditor_member_id uuid NOT NULL,
    scheduled_start     date,
    scheduled_end       date
);

CREATE INDEX IF NOT EXISTS idx_audits_tenant
    ON audit.audits (tenant_id);

CREATE TABLE IF NOT EXISTS audit.findings (
    id                   uuid PRIMARY KEY,
    tenant_id            uuid NOT NULL,
    record_id            uuid NOT NULL,
    audit_id             uuid NOT NULL,
    audit_response_id    uuid,
    classification       text NOT NULL,
    requirement_reference text,
    description          text NOT NULL,
    recommendation       text,
    owner_member_id      uuid
);

CREATE INDEX IF NOT EXISTS idx_audit_findings_tenant
    ON audit.findings (tenant_id);
CREATE INDEX IF NOT EXISTS idx_audit_findings_audit
    ON audit.findings (audit_id);

-- =====================================================================
-- WorkControl module: cow schema (PTW / JSA / LOTO, Trello Sprint 17)
-- =====================================================================
CREATE SCHEMA IF NOT EXISTS cow;

CREATE TABLE IF NOT EXISTS cow.work_requests (
    id                   uuid PRIMARY KEY,
    tenant_id            uuid NOT NULL,
    record_id            uuid NOT NULL,
    requester_member_id  uuid NOT NULL,
    work_description     text NOT NULL,
    contractor_company_id uuid,
    planned_start        timestamptz,
    planned_end          timestamptz,
    work_type            text NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_work_requests_tenant
    ON cow.work_requests (tenant_id);

CREATE TABLE IF NOT EXISTS cow.jsas (
    id                    uuid PRIMARY KEY,
    tenant_id             uuid NOT NULL,
    record_id             uuid NOT NULL,
    work_request_id       uuid NOT NULL,
    template_version_id   uuid,
    prepared_by_member_id uuid NOT NULL,
    status                text NOT NULL,
    overall_residual_risk text
);

CREATE INDEX IF NOT EXISTS idx_jsas_tenant
    ON cow.jsas (tenant_id);

CREATE TABLE IF NOT EXISTS cow.jsa_steps (
    id              uuid PRIMARY KEY,
    tenant_id       uuid NOT NULL,
    jsa_id          uuid NOT NULL,
    sequence_number int NOT NULL,
    work_step       text NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_jsa_steps_tenant
    ON cow.jsa_steps (tenant_id);
CREATE INDEX IF NOT EXISTS idx_jsa_steps_jsa
    ON cow.jsa_steps (jsa_id);

CREATE TABLE IF NOT EXISTS cow.permits (
    id                     uuid PRIMARY KEY,
    tenant_id              uuid NOT NULL,
    record_id              uuid NOT NULL,
    work_request_id        uuid NOT NULL,
    jsa_id                 uuid,
    permit_type_version_id uuid NOT NULL,
    requester_member_id    uuid NOT NULL,
    executor_person_id     uuid,
    contractor_company_id  uuid,
    valid_from             timestamptz,
    valid_until            timestamptz,
    suspension_reason      text,
    extension_count        int NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS idx_permits_tenant
    ON cow.permits (tenant_id);

CREATE TABLE IF NOT EXISTS cow.permit_approvals (
    id                 uuid PRIMARY KEY,
    tenant_id          uuid NOT NULL,
    permit_id          uuid NOT NULL,
    workflow_task_id   uuid NOT NULL,
    approval_level     int NOT NULL,
    decision           text,
    approver_member_id uuid,
    decided_at         timestamptz
);

CREATE INDEX IF NOT EXISTS idx_permit_approvals_tenant
    ON cow.permit_approvals (tenant_id);
CREATE INDEX IF NOT EXISTS idx_permit_approvals_permit
    ON cow.permit_approvals (permit_id);

CREATE TABLE IF NOT EXISTS cow.gas_tests (
    id                 uuid PRIMARY KEY,
    tenant_id          uuid NOT NULL,
    permit_id          uuid NOT NULL,
    test_type          text NOT NULL,
    tested_at          timestamptz NOT NULL,
    tested_by_person_id uuid,
    oxygen_pct         numeric(5,2),
    lel_pct            numeric(5,2),
    toxic_gas_json     text,
    result             text NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_gas_tests_tenant
    ON cow.gas_tests (tenant_id);
CREATE INDEX IF NOT EXISTS idx_gas_tests_permit
    ON cow.gas_tests (permit_id);

CREATE TABLE IF NOT EXISTS cow.isolation_plans (
    id                    uuid PRIMARY KEY,
    tenant_id             uuid NOT NULL,
    record_id             uuid NOT NULL,
    permit_id             uuid NOT NULL,
    prepared_by_member_id uuid NOT NULL,
    status                text NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_isolation_plans_tenant
    ON cow.isolation_plans (tenant_id);

