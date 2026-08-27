-- =============================================================================
-- 004-extended.sql — Wave 4 (Extended): 12 Domain Schemas
-- Schemas : compliance, contractor, training, ppe, health, chemical,
--           environment, sustainability, asset, emergency, reporting,
--           integration
-- Source  : database/ehsms-erd.dbml  (PRD v1.1 Final Revised, 26 Aug 2026)
-- Engine  : PostgreSQL 18 (Neon)
-- Idempotent — safe to re-run
-- Run order: 001-infra -> 002-core-ehs -> 003-operational -> 004-extended
-- =============================================================================
-- VOID guard: skip if first schema already exists
DO $$ BEGIN
  PERFORM 1 FROM pg_namespace WHERE nspname = 'compliance';
  IF FOUND THEN
    RAISE NOTICE 'Wave 4 schemas already exist -- skipping creation';
  END IF;
EXCEPTION WHEN OTHERS THEN NULL;
END $$;

-- =============================================================================
-- SCHEMA CREATION
-- =============================================================================
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


--###############################################################################
-- COMPLIANCE SCHEMA  (6 tables)
--###############################################################################

-- ===============================================================================
-- compliance.legal_sources
-- ===============================================================================
CREATE TABLE compliance.legal_sources (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    source_type                    varchar(40)          NOT NULL,
    code                           varchar(100)         ,
    title                          varchar(300)         NOT NULL,
    jurisdiction                   varchar(100)         ,
    publisher                      varchar(200)         ,
    source_url                     text                 ,
    status                         varchar(30)          NOT NULL
);

CREATE INDEX idx_legal_sources_tenant_id ON compliance.legal_sources (tenant_id);

-- ===============================================================================
-- compliance.legal_source_versions
-- ===============================================================================
CREATE TABLE compliance.legal_source_versions (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    legal_source_id                uuid                 NOT NULL,
    version_label                  varchar(100)         NOT NULL,
    published_date                 date                 ,
    effective_date                 date                 ,
    superseded_date                date                 ,
    change_summary                 text                 
);

CREATE INDEX idx_legal_source_versions_tenant_id ON compliance.legal_source_versions (tenant_id);
CREATE INDEX idx_legal_source_versions_source ON compliance.legal_source_versions (legal_source_id);

-- ===============================================================================
-- compliance.obligations
-- ===============================================================================
CREATE TABLE compliance.obligations (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    legal_source_version_id        uuid                 NOT NULL,
    clause_reference               varchar(150)         ,
    requirement_text               text                 NOT NULL,
    owner_member_id                uuid                 NOT NULL,
    frequency                      varchar(80)          ,
    due_date                       date                 ,
    last_review                    date                 ,
    next_review                    date                 
);

CREATE INDEX idx_obligations_tenant_id ON compliance.obligations (tenant_id);
CREATE INDEX idx_obligations_source_version ON compliance.obligations (legal_source_version_id);
CREATE INDEX idx_obligations_owner          ON compliance.obligations (owner_member_id);

-- ===============================================================================
-- compliance.obligation_applicability
-- ===============================================================================
CREATE TABLE compliance.obligation_applicability (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    obligation_id                  uuid                 NOT NULL,
    company_id                     uuid                 ,
    business_unit_id               uuid                 ,
    site_id                        uuid                 ,
    applicability_status           varchar(30)          NOT NULL,
    rationale                      text                 ,
    assessed_by_member_id          uuid                 NOT NULL
);

CREATE INDEX idx_obligation_applicability_tenant_id ON compliance.obligation_applicability (tenant_id);
CREATE INDEX idx_obligation_applicability_obligation  ON compliance.obligation_applicability (obligation_id);
CREATE INDEX idx_obligation_applicability_company     ON compliance.obligation_applicability (company_id);
CREATE INDEX idx_obligation_applicability_bu          ON compliance.obligation_applicability (business_unit_id);
CREATE INDEX idx_obligation_applicability_site        ON compliance.obligation_applicability (site_id);
CREATE INDEX idx_obligation_applicability_assessor    ON compliance.obligation_applicability (assessed_by_member_id);

-- ===============================================================================
-- compliance.evaluations
-- ===============================================================================
CREATE TABLE compliance.evaluations (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    obligation_id                  uuid                 NOT NULL,
    evaluation_period_start        date                 ,
    evaluation_period_end          date                 ,
    compliance_status              varchar(30)          NOT NULL,
    evaluated_by_member_id         uuid                 NOT NULL,
    evaluated_at                   timestamptz          NOT NULL,
    comment                        text                 
);

CREATE INDEX idx_evaluations_tenant_id ON compliance.evaluations (tenant_id);
CREATE INDEX idx_evaluations_obligation  ON compliance.evaluations (obligation_id);
CREATE INDEX idx_evaluations_evaluator   ON compliance.evaluations (evaluated_by_member_id);

-- ===============================================================================
-- compliance.gaps
-- ===============================================================================
CREATE TABLE compliance.gaps (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    evaluation_id                  uuid                 NOT NULL,
    gap_description                text                 NOT NULL,
    severity                       varchar(30)          ,
    owner_member_id                uuid                 ,
    target_date                    date                 
);

CREATE INDEX idx_gaps_tenant_id ON compliance.gaps (tenant_id);
CREATE INDEX idx_compliance_gaps_evaluation ON compliance.gaps (evaluation_id);
CREATE INDEX idx_compliance_gaps_owner      ON compliance.gaps (owner_member_id);

-- Intra-schema FKs
ALTER TABLE compliance.legal_source_versions
    ADD CONSTRAINT fk_lsv_source
    FOREIGN KEY (legal_source_id) REFERENCES compliance.legal_sources (id);

ALTER TABLE compliance.obligations
    ADD CONSTRAINT fk_oblig_lsv
    FOREIGN KEY (legal_source_version_id) REFERENCES compliance.legal_source_versions (id);

ALTER TABLE compliance.obligation_applicability
    ADD CONSTRAINT fk_oa_obligation
    FOREIGN KEY (obligation_id) REFERENCES compliance.obligations (id);

ALTER TABLE compliance.evaluations
    ADD CONSTRAINT fk_eval_obligation
    FOREIGN KEY (obligation_id) REFERENCES compliance.obligations (id);

ALTER TABLE compliance.gaps
    ADD CONSTRAINT fk_gaps_evaluation
    FOREIGN KEY (evaluation_id) REFERENCES compliance.evaluations (id);

-- Cross-schema FKs
ALTER TABLE compliance.obligations
    ADD CONSTRAINT fk_oblig_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE compliance.obligations
    ADD CONSTRAINT fk_oblig_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE compliance.obligation_applicability
    ADD CONSTRAINT fk_oa_company
    FOREIGN KEY (company_id) REFERENCES org.companies (id);

ALTER TABLE compliance.obligation_applicability
    ADD CONSTRAINT fk_oa_bu
    FOREIGN KEY (business_unit_id) REFERENCES org.business_units (id);

ALTER TABLE compliance.obligation_applicability
    ADD CONSTRAINT fk_oa_site
    FOREIGN KEY (site_id) REFERENCES org.sites (id);

ALTER TABLE compliance.obligation_applicability
    ADD CONSTRAINT fk_oa_assessor
    FOREIGN KEY (assessed_by_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE compliance.evaluations
    ADD CONSTRAINT fk_eval_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE compliance.evaluations
    ADD CONSTRAINT fk_eval_evaluator
    FOREIGN KEY (evaluated_by_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE compliance.gaps
    ADD CONSTRAINT fk_gaps_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE compliance.gaps
    ADD CONSTRAINT fk_gaps_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members (id);


--###############################################################################
-- CONTRACTOR SCHEMA  (6 tables)
--###############################################################################

-- ===============================================================================
-- contractor.companies
-- ===============================================================================
CREATE TABLE contractor.companies (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    vendor_code                    varchar(60)          ,
    name                           varchar(250)         NOT NULL,
    tax_identifier                 varchar(100)         ,
    qualification_status           varchar(30)          ,
    eligibility_status             varchar(30)          ,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_companies_tenant_id ON contractor.companies (tenant_id);
CREATE INDEX idx_contractor_companies_vendor ON contractor.companies (vendor_code);

-- ===============================================================================
-- contractor.contracts
-- ===============================================================================
CREATE TABLE contractor.contracts (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    contractor_company_id          uuid                 NOT NULL,
    contract_number                varchar(80)          ,
    start_date                     date                 ,
    end_date                       date                 ,
    contract_status                varchar(30)          ,
    procurement_source_id          varchar(100)         
);

CREATE INDEX idx_contracts_tenant_id ON contractor.contracts (tenant_id);
CREATE INDEX idx_contracts_company ON contractor.contracts (contractor_company_id);

-- ===============================================================================
-- contractor.workers
-- ===============================================================================
CREATE TABLE contractor.workers (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    person_id                      uuid                 NOT NULL UNIQUE,
    contractor_company_id          uuid                 NOT NULL,
    worker_number                  varchar(60)          ,
    position_name                  varchar(150)         ,
    eligibility_status             varchar(30)          ,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_workers_tenant_id ON contractor.workers (tenant_id);
CREATE INDEX idx_workers_company ON contractor.workers (contractor_company_id);

-- ===============================================================================
-- contractor.documents
-- ===============================================================================
CREATE TABLE contractor.documents (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    contractor_company_id          uuid                 ,
    contractor_worker_id           uuid                 ,
    document_type                  varchar(60)          NOT NULL,
    document_number                varchar(100)         ,
    file_object_id                 uuid                 NOT NULL,
    issue_date                     date                 ,
    expiry_date                    date                 ,
    verification_status            varchar(30)          
);

CREATE INDEX idx_documents_tenant_id ON contractor.documents (tenant_id);
CREATE INDEX idx_contractor_docs_company ON contractor.documents (contractor_company_id);
CREATE INDEX idx_contractor_docs_worker  ON contractor.documents (contractor_worker_id);

-- ===============================================================================
-- contractor.qualification_evaluations
-- ===============================================================================
CREATE TABLE contractor.qualification_evaluations (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    contractor_company_id          uuid                 NOT NULL,
    evaluation_type                varchar(50)          NOT NULL,
    result                         varchar(30)          NOT NULL,
    score                          decimal(10,2)        ,
    evaluated_by_member_id         uuid                 NOT NULL,
    valid_until                    date                 
);

CREATE INDEX idx_qualification_evaluations_tenant_id ON contractor.qualification_evaluations (tenant_id);
CREATE INDEX idx_qual_evals_company ON contractor.qualification_evaluations (contractor_company_id);
CREATE INDEX idx_qual_evals_evaluator ON contractor.qualification_evaluations (evaluated_by_member_id);

-- ===============================================================================
-- contractor.performance_periods
-- ===============================================================================
CREATE TABLE contractor.performance_periods (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    contractor_company_id          uuid                 NOT NULL,
    period_start                   date                 NOT NULL,
    period_end                     date                 NOT NULL,
    indicator_values_json          jsonb                ,
    overall_rating                 decimal(10,2)        
);

CREATE INDEX idx_performance_periods_tenant_id ON contractor.performance_periods (tenant_id);
CREATE INDEX idx_perf_periods_company ON contractor.performance_periods (contractor_company_id);

-- Intra-schema FKs
ALTER TABLE contractor.contracts
    ADD CONSTRAINT fk_contracts_company
    FOREIGN KEY (contractor_company_id) REFERENCES contractor.companies (id);

ALTER TABLE contractor.workers
    ADD CONSTRAINT fk_workers_company
    FOREIGN KEY (contractor_company_id) REFERENCES contractor.companies (id);

ALTER TABLE contractor.documents
    ADD CONSTRAINT fk_cdocs_company
    FOREIGN KEY (contractor_company_id) REFERENCES contractor.companies (id);

ALTER TABLE contractor.documents
    ADD CONSTRAINT fk_cdocs_worker
    FOREIGN KEY (contractor_worker_id) REFERENCES contractor.workers (id);

ALTER TABLE contractor.qualification_evaluations
    ADD CONSTRAINT fk_qe_company
    FOREIGN KEY (contractor_company_id) REFERENCES contractor.companies (id);

ALTER TABLE contractor.performance_periods
    ADD CONSTRAINT fk_pp_company
    FOREIGN KEY (contractor_company_id) REFERENCES contractor.companies (id);

-- Cross-schema FKs
ALTER TABLE contractor.companies
    ADD CONSTRAINT fk_coco_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE contractor.workers
    ADD CONSTRAINT fk_cowo_person
    FOREIGN KEY (person_id) REFERENCES org.people (id);

ALTER TABLE contractor.documents
    ADD CONSTRAINT fk_cdocs_file
    FOREIGN KEY (file_object_id) REFERENCES platform.file_objects (id);

ALTER TABLE contractor.qualification_evaluations
    ADD CONSTRAINT fk_qe_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE contractor.qualification_evaluations
    ADD CONSTRAINT fk_qe_evaluator
    FOREIGN KEY (evaluated_by_member_id) REFERENCES iam.tenant_members (id);


--###############################################################################
-- TRAINING SCHEMA  (9 tables)
--###############################################################################

-- ===============================================================================
-- training.courses
-- ===============================================================================
CREATE TABLE training.courses (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    code                           varchar(50)          NOT NULL,
    name                           varchar(200)         NOT NULL,
    validity_months                int                  ,
    provider_type                  varchar(40)          ,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_courses_tenant_id ON training.courses (tenant_id);

-- ===============================================================================
-- training.competencies
-- ===============================================================================
CREATE TABLE training.competencies (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    code                           varchar(50)          NOT NULL,
    name                           varchar(200)         NOT NULL,
    description                    text                 ,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_competencies_tenant_id ON training.competencies (tenant_id);

-- ===============================================================================
-- training.position_requirements
-- ===============================================================================
CREATE TABLE training.position_requirements (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    position_id                    uuid                 NOT NULL,
    competency_id                  uuid                 NOT NULL,
    course_id                      uuid                 ,
    is_mandatory                   boolean              NOT NULL,
    minimum_level                  varchar(30)          
);

CREATE INDEX idx_position_requirements_tenant_id ON training.position_requirements (tenant_id);
CREATE INDEX idx_pos_req_position   ON training.position_requirements (position_id);
CREATE INDEX idx_pos_req_competency ON training.position_requirements (competency_id);
CREATE INDEX idx_pos_req_course     ON training.position_requirements (course_id);

-- ===============================================================================
-- training.sessions
-- ===============================================================================
CREATE TABLE training.sessions (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    course_id                      uuid                 NOT NULL,
    provider_name                  varchar(200)         ,
    starts_at                      timestamptz          ,
    ends_at                        timestamptz          ,
    capacity                       int                  ,
    status                         varchar(30)          NOT NULL
);

CREATE INDEX idx_sessions_tenant_id ON training.sessions (tenant_id);
CREATE INDEX idx_sessions_course ON training.sessions (course_id);

-- ===============================================================================
-- training.session_participants
-- ===============================================================================
CREATE TABLE training.session_participants (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    training_session_id            uuid                 NOT NULL,
    person_id                      uuid                 NOT NULL,
    attendance_status              varchar(30)          ,
    assessment_score               decimal(10,2)        ,
    result                         varchar(30)          
);

CREATE INDEX idx_session_participants_tenant_id ON training.session_participants (tenant_id);
CREATE INDEX idx_session_participants_session ON training.session_participants (training_session_id);
CREATE INDEX idx_session_participants_person ON training.session_participants (person_id);

-- ===============================================================================
-- training.worker_competencies
-- ===============================================================================
CREATE TABLE training.worker_competencies (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    person_id                      uuid                 NOT NULL,
    competency_id                  uuid                 NOT NULL,
    level                          varchar(30)          ,
    status                         varchar(30)          NOT NULL,
    valid_from                     date                 ,
    valid_until                    date                 
);

CREATE INDEX idx_worker_competencies_tenant_id ON training.worker_competencies (tenant_id);
CREATE INDEX idx_wc_person     ON training.worker_competencies (person_id);
CREATE INDEX idx_wc_competency ON training.worker_competencies (competency_id);

-- ===============================================================================
-- training.certifications
-- ===============================================================================
CREATE TABLE training.certifications (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    person_id                      uuid                 NOT NULL,
    course_id                      uuid                 ,
    certificate_number             varchar(100)         ,
    issued_at                      date                 ,
    expires_at                     date                 ,
    file_object_id                 uuid                 ,
    verification_status            varchar(30)          
);

CREATE INDEX idx_certifications_tenant_id ON training.certifications (tenant_id);
CREATE INDEX idx_certs_person ON training.certifications (person_id);
CREATE INDEX idx_certs_course ON training.certifications (course_id);

-- ===============================================================================
-- training.eligibility_checks
-- ===============================================================================
CREATE TABLE training.eligibility_checks (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    person_id                      uuid                 NOT NULL,
    target_record_id               uuid                 NOT NULL,
    result                         varchar(30)          NOT NULL,
    checked_at                     timestamptz          NOT NULL,
    details_json                   jsonb                
);

CREATE INDEX idx_eligibility_checks_tenant_id ON training.eligibility_checks (tenant_id);
CREATE INDEX idx_elig_checks_person ON training.eligibility_checks (person_id);
CREATE INDEX idx_elig_checks_target ON training.eligibility_checks (target_record_id);

-- ===============================================================================
-- training.eligibility_overrides
-- ===============================================================================
CREATE TABLE training.eligibility_overrides (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    eligibility_check_id           uuid                 NOT NULL,
    approved_by_member_id          uuid                 NOT NULL,
    reason                         text                 NOT NULL,
    valid_until                    timestamptz          
);

CREATE INDEX idx_eligibility_overrides_tenant_id ON training.eligibility_overrides (tenant_id);
CREATE INDEX idx_elig_overrides_check ON training.eligibility_overrides (eligibility_check_id);
CREATE INDEX idx_elig_overrides_approver ON training.eligibility_overrides (approved_by_member_id);

-- Intra-schema FKs
ALTER TABLE training.position_requirements
    ADD CONSTRAINT fk_pr_position
    FOREIGN KEY (position_id) REFERENCES org.positions (id);

ALTER TABLE training.position_requirements
    ADD CONSTRAINT fk_pr_competency
    FOREIGN KEY (competency_id) REFERENCES training.competencies (id);

ALTER TABLE training.position_requirements
    ADD CONSTRAINT fk_pr_course
    FOREIGN KEY (course_id) REFERENCES training.courses (id);

ALTER TABLE training.sessions
    ADD CONSTRAINT fk_sess_course
    FOREIGN KEY (course_id) REFERENCES training.courses (id);

ALTER TABLE training.session_participants
    ADD CONSTRAINT fk_sp_session
    FOREIGN KEY (training_session_id) REFERENCES training.sessions (id);

ALTER TABLE training.worker_competencies
    ADD CONSTRAINT fk_wc_competency
    FOREIGN KEY (competency_id) REFERENCES training.competencies (id);

ALTER TABLE training.certifications
    ADD CONSTRAINT fk_cert_course
    FOREIGN KEY (course_id) REFERENCES training.courses (id);

ALTER TABLE training.eligibility_overrides
    ADD CONSTRAINT fk_eo_check
    FOREIGN KEY (eligibility_check_id) REFERENCES training.eligibility_checks (id);

-- Cross-schema FKs
ALTER TABLE training.sessions
    ADD CONSTRAINT fk_sess_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE training.session_participants
    ADD CONSTRAINT fk_sp_person
    FOREIGN KEY (person_id) REFERENCES org.people (id);

ALTER TABLE training.worker_competencies
    ADD CONSTRAINT fk_wc_person
    FOREIGN KEY (person_id) REFERENCES org.people (id);

ALTER TABLE training.certifications
    ADD CONSTRAINT fk_cert_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE training.certifications
    ADD CONSTRAINT fk_cert_person
    FOREIGN KEY (person_id) REFERENCES org.people (id);

ALTER TABLE training.certifications
    ADD CONSTRAINT fk_cert_file
    FOREIGN KEY (file_object_id) REFERENCES platform.file_objects (id);

ALTER TABLE training.eligibility_checks
    ADD CONSTRAINT fk_ec_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE training.eligibility_checks
    ADD CONSTRAINT fk_ec_person
    FOREIGN KEY (person_id) REFERENCES org.people (id);

ALTER TABLE training.eligibility_checks
    ADD CONSTRAINT fk_ec_target
    FOREIGN KEY (target_record_id) REFERENCES platform.records (id);

ALTER TABLE training.eligibility_overrides
    ADD CONSTRAINT fk_eo_approver
    FOREIGN KEY (approved_by_member_id) REFERENCES iam.tenant_members (id);


--###############################################################################
-- PPE SCHEMA  (6 tables)
--###############################################################################

-- ===============================================================================
-- ppe.catalog
-- ===============================================================================
CREATE TABLE ppe.catalog (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    code                           varchar(50)          NOT NULL,
    name                           varchar(200)         NOT NULL,
    ppe_category                   varchar(60)          ,
    inspection_interval_days       int                  ,
    replacement_interval_days      int                  ,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_catalog_tenant_id ON ppe.catalog (tenant_id);

-- ===============================================================================
-- ppe.inventory
-- ===============================================================================
CREATE TABLE ppe.inventory (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    ppe_catalog_id                 uuid                 NOT NULL,
    site_id                        uuid                 NOT NULL,
    serial_number                  varchar(100)         ,
    quantity_on_hand               int                  ,
    condition                      varchar(30)          ,
    status                         varchar(30)          NOT NULL
);

CREATE INDEX idx_inventory_tenant_id ON ppe.inventory (tenant_id);
CREATE INDEX idx_ppe_inventory_catalog ON ppe.inventory (ppe_catalog_id);
CREATE INDEX idx_ppe_inventory_site    ON ppe.inventory (site_id);

-- ===============================================================================
-- ppe.requirements
-- ===============================================================================
CREATE TABLE ppe.requirements (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    ppe_catalog_id                 uuid                 NOT NULL,
    source_record_id               uuid                 ,
    permit_type_id                 uuid                 ,
    is_mandatory                   boolean              NOT NULL,
    notes                          text                 
);

CREATE INDEX idx_requirements_tenant_id ON ppe.requirements (tenant_id);
CREATE INDEX idx_ppe_requirements_catalog ON ppe.requirements (ppe_catalog_id);

-- ===============================================================================
-- ppe.assignments
-- ===============================================================================
CREATE TABLE ppe.assignments (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    ppe_inventory_id               uuid                 NOT NULL,
    person_id                      uuid                 NOT NULL,
    issued_at                      timestamptz          NOT NULL,
    issued_by_member_id            uuid                 NOT NULL,
    returned_at                    timestamptz          ,
    condition_on_return            varchar(30)          
);

CREATE INDEX idx_assignments_tenant_id ON ppe.assignments (tenant_id);
CREATE INDEX idx_ppe_assignments_inventory ON ppe.assignments (ppe_inventory_id);
CREATE INDEX idx_ppe_assignments_person    ON ppe.assignments (person_id);
CREATE INDEX idx_ppe_assignments_issuer    ON ppe.assignments (issued_by_member_id);

-- ===============================================================================
-- ppe.inspections
-- ===============================================================================
CREATE TABLE ppe.inspections (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    ppe_inventory_id               uuid                 NOT NULL,
    inspected_by_member_id         uuid                 NOT NULL,
    inspected_at                   timestamptz          NOT NULL,
    condition                      varchar(30)          NOT NULL,
    result                         varchar(30)          NOT NULL,
    next_due_date                  date                 
);

CREATE INDEX idx_inspections_tenant_id ON ppe.inspections (tenant_id);
CREATE INDEX idx_ppe_inspections_inventory ON ppe.inspections (ppe_inventory_id);
CREATE INDEX idx_ppe_inspections_inspector ON ppe.inspections (inspected_by_member_id);

-- ===============================================================================
-- ppe.replacements
-- ===============================================================================
CREATE TABLE ppe.replacements (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    ppe_assignment_id              uuid                 NOT NULL,
    replacement_reason             varchar(80)          NOT NULL,
    requested_at                   timestamptz          NOT NULL,
    completed_at                   timestamptz          
);

CREATE INDEX idx_replacements_tenant_id ON ppe.replacements (tenant_id);
CREATE INDEX idx_ppe_replacements_assignment ON ppe.replacements (ppe_assignment_id);

-- Intra-schema FKs
ALTER TABLE ppe.inventory
    ADD CONSTRAINT fk_ppe_inv_catalog
    FOREIGN KEY (ppe_catalog_id) REFERENCES ppe.catalog (id);

ALTER TABLE ppe.requirements
    ADD CONSTRAINT fk_ppe_req_catalog
    FOREIGN KEY (ppe_catalog_id) REFERENCES ppe.catalog (id);

ALTER TABLE ppe.assignments
    ADD CONSTRAINT fk_ppe_asgn_inventory
    FOREIGN KEY (ppe_inventory_id) REFERENCES ppe.inventory (id);

ALTER TABLE ppe.inspections
    ADD CONSTRAINT fk_ppe_insp_inventory
    FOREIGN KEY (ppe_inventory_id) REFERENCES ppe.inventory (id);

ALTER TABLE ppe.replacements
    ADD CONSTRAINT fk_ppe_repl_assignment
    FOREIGN KEY (ppe_assignment_id) REFERENCES ppe.assignments (id);

-- Cross-schema FKs
ALTER TABLE ppe.inventory
    ADD CONSTRAINT fk_ppe_inv_site
    FOREIGN KEY (site_id) REFERENCES org.sites (id);

ALTER TABLE ppe.requirements
    ADD CONSTRAINT fk_ppe_req_record
    FOREIGN KEY (source_record_id) REFERENCES platform.records (id);

ALTER TABLE ppe.requirements
    ADD CONSTRAINT fk_ppe_req_permit
    FOREIGN KEY (permit_type_id) REFERENCES cow.permit_types (id);

ALTER TABLE ppe.assignments
    ADD CONSTRAINT fk_ppe_asgn_person
    FOREIGN KEY (person_id) REFERENCES org.people (id);

ALTER TABLE ppe.assignments
    ADD CONSTRAINT fk_ppe_asgn_issuer
    FOREIGN KEY (issued_by_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE ppe.inspections
    ADD CONSTRAINT fk_ppe_insp_inspector
    FOREIGN KEY (inspected_by_member_id) REFERENCES iam.tenant_members (id);


--###############################################################################
-- HEALTH SCHEMA  (6 tables)
--###############################################################################

-- ===============================================================================
-- health.profiles
-- ===============================================================================
CREATE TABLE health.profiles (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    person_id                      uuid                 NOT NULL UNIQUE,
    restricted_identifier          varchar(100)         ,
    data_classification_id         uuid                 NOT NULL
);

CREATE INDEX idx_profiles_tenant_id ON health.profiles (tenant_id);
CREATE INDEX idx_health_profiles_person ON health.profiles (person_id);

-- ===============================================================================
-- health.surveillance_programs
-- ===============================================================================
CREATE TABLE health.surveillance_programs (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    code                           varchar(50)          NOT NULL,
    name                           varchar(200)         NOT NULL,
    exposure_type                  varchar(100)         ,
    frequency_months               int                  ,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_surveillance_programs_tenant_id ON health.surveillance_programs (tenant_id);

-- ===============================================================================
-- health.surveillance_events
-- ===============================================================================
CREATE TABLE health.surveillance_events (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    health_profile_id              uuid                 NOT NULL,
    surveillance_program_id        uuid                 NOT NULL,
    scheduled_date                 date                 ,
    completed_date                 date                 ,
    authorized_provider            varchar(200)         ,
    result_summary_code            varchar(50)          
);

CREATE INDEX idx_surveillance_events_tenant_id ON health.surveillance_events (tenant_id);
CREATE INDEX idx_surv_events_profile   ON health.surveillance_events (health_profile_id);
CREATE INDEX idx_surv_events_program   ON health.surveillance_events (surveillance_program_id);

-- ===============================================================================
-- health.fitness_statuses
-- ===============================================================================
CREATE TABLE health.fitness_statuses (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    health_profile_id              uuid                 NOT NULL,
    fitness_status                 varchar(40)          NOT NULL,
    valid_from                     date                 NOT NULL,
    valid_until                    date                 ,
    restrictions_summary           text                 ,
    issued_by_member_id            uuid                 
);

CREATE INDEX idx_fitness_statuses_tenant_id ON health.fitness_statuses (tenant_id);
CREATE INDEX idx_fitness_profile ON health.fitness_statuses (health_profile_id);
CREATE INDEX idx_fitness_issuer  ON health.fitness_statuses (issued_by_member_id);

-- ===============================================================================
-- health.exposure_links
-- ===============================================================================
CREATE TABLE health.exposure_links (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    health_profile_id              uuid                 NOT NULL,
    source_record_id               uuid                 NOT NULL,
    exposure_type                  varchar(100)         NOT NULL,
    exposure_period_start          date                 ,
    exposure_period_end            date                 
);

CREATE INDEX idx_exposure_links_tenant_id ON health.exposure_links (tenant_id);
CREATE INDEX idx_exposure_links_profile ON health.exposure_links (health_profile_id);

-- ===============================================================================
-- health.followups
-- ===============================================================================
CREATE TABLE health.followups (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    surveillance_event_id          uuid                 NOT NULL,
    followup_type                  varchar(60)          NOT NULL,
    due_date                       date                 ,
    status                         varchar(30)          NOT NULL,
    assigned_member_id             uuid                 
);

CREATE INDEX idx_followups_tenant_id ON health.followups (tenant_id);
CREATE INDEX idx_followups_event  ON health.followups (surveillance_event_id);
CREATE INDEX idx_followups_assign ON health.followups (assigned_member_id);

-- Intra-schema FKs
ALTER TABLE health.surveillance_events
    ADD CONSTRAINT fk_se_profile
    FOREIGN KEY (health_profile_id) REFERENCES health.profiles (id);

ALTER TABLE health.surveillance_events
    ADD CONSTRAINT fk_se_program
    FOREIGN KEY (surveillance_program_id) REFERENCES health.surveillance_programs (id);

ALTER TABLE health.fitness_statuses
    ADD CONSTRAINT fk_fs_profile
    FOREIGN KEY (health_profile_id) REFERENCES health.profiles (id);

ALTER TABLE health.exposure_links
    ADD CONSTRAINT fk_el_profile
    FOREIGN KEY (health_profile_id) REFERENCES health.profiles (id);

ALTER TABLE health.followups
    ADD CONSTRAINT fk_fu_event
    FOREIGN KEY (surveillance_event_id) REFERENCES health.surveillance_events (id);

-- Cross-schema FKs
ALTER TABLE health.profiles
    ADD CONSTRAINT fk_hp_person
    FOREIGN KEY (person_id) REFERENCES org.people (id);

ALTER TABLE health.profiles
    ADD CONSTRAINT fk_hp_class
    FOREIGN KEY (data_classification_id) REFERENCES platform.data_classifications (id);

ALTER TABLE health.surveillance_events
    ADD CONSTRAINT fk_se_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE health.fitness_statuses
    ADD CONSTRAINT fk_fs_issuer
    FOREIGN KEY (issued_by_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE health.exposure_links
    ADD CONSTRAINT fk_el_source_record
    FOREIGN KEY (source_record_id) REFERENCES platform.records (id);

ALTER TABLE health.followups
    ADD CONSTRAINT fk_fu_assign
    FOREIGN KEY (assigned_member_id) REFERENCES iam.tenant_members (id);


--###############################################################################
-- CHEMICAL SCHEMA  (5 tables)
--###############################################################################

-- ===============================================================================
-- chemical.products
-- ===============================================================================
CREATE TABLE chemical.products (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    product_code                   varchar(60)          ,
    product_name                   varchar(200)         NOT NULL,
    supplier_name                  varchar(200)         ,
    hazard_classification_json     jsonb                ,
    owner_member_id                uuid                 ,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_products_tenant_id ON chemical.products (tenant_id);
CREATE INDEX idx_chem_products_owner ON chemical.products (owner_member_id);

-- ===============================================================================
-- chemical.inventory
-- ===============================================================================
CREATE TABLE chemical.inventory (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    chemical_product_id            uuid                 NOT NULL,
    location_id                    uuid                 NOT NULL,
    quantity                       decimal(18,4)        ,
    unit                           varchar(30)          ,
    storage_condition              varchar(100)         ,
    expiry_date                    date                 
);

CREATE INDEX idx_inventory_tenant_id ON chemical.inventory (tenant_id);
CREATE INDEX idx_chem_inventory_product ON chemical.inventory (chemical_product_id);
CREATE INDEX idx_chem_inventory_location ON chemical.inventory (location_id);

-- ===============================================================================
-- chemical.sds_revisions
-- ===============================================================================
CREATE TABLE chemical.sds_revisions (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    chemical_product_id            uuid                 NOT NULL,
    revision_number                varchar(50)          NOT NULL,
    effective_date                 date                 ,
    file_object_id                 uuid                 NOT NULL,
    language                       varchar(20)          ,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_sds_revisions_tenant_id ON chemical.sds_revisions (tenant_id);
CREATE INDEX idx_sds_product ON chemical.sds_revisions (chemical_product_id);

-- ===============================================================================
-- chemical.storage_inspections
-- ===============================================================================
CREATE TABLE chemical.storage_inspections (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    chemical_inventory_id          uuid                 NOT NULL,
    inspected_by_member_id         uuid                 NOT NULL,
    inspected_at                   timestamptz          NOT NULL,
    result                         varchar(30)          NOT NULL,
    next_review_date               date                 
);

CREATE INDEX idx_storage_inspections_tenant_id ON chemical.storage_inspections (tenant_id);
CREATE INDEX idx_chem_si_inventory ON chemical.storage_inspections (chemical_inventory_id);
CREATE INDEX idx_chem_si_inspector ON chemical.storage_inspections (inspected_by_member_id);

-- ===============================================================================
-- chemical.exposure_controls
-- ===============================================================================
CREATE TABLE chemical.exposure_controls (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    chemical_product_id            uuid                 NOT NULL,
    control_type                   varchar(60)          NOT NULL,
    description                    text                 NOT NULL,
    source_record_id               uuid                 
);

CREATE INDEX idx_exposure_controls_tenant_id ON chemical.exposure_controls (tenant_id);
CREATE INDEX idx_chem_ec_product ON chemical.exposure_controls (chemical_product_id);

-- Intra-schema FKs
ALTER TABLE chemical.inventory
    ADD CONSTRAINT fk_ci_product
    FOREIGN KEY (chemical_product_id) REFERENCES chemical.products (id);

ALTER TABLE chemical.sds_revisions
    ADD CONSTRAINT fk_sds_product
    FOREIGN KEY (chemical_product_id) REFERENCES chemical.products (id);

ALTER TABLE chemical.storage_inspections
    ADD CONSTRAINT fk_si_inventory
    FOREIGN KEY (chemical_inventory_id) REFERENCES chemical.inventory (id);

ALTER TABLE chemical.exposure_controls
    ADD CONSTRAINT fk_ec_product
    FOREIGN KEY (chemical_product_id) REFERENCES chemical.products (id);

-- Cross-schema FKs
ALTER TABLE chemical.products
    ADD CONSTRAINT fk_cp_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE chemical.products
    ADD CONSTRAINT fk_cp_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE chemical.inventory
    ADD CONSTRAINT fk_ci_location
    FOREIGN KEY (location_id) REFERENCES org.locations (id);

ALTER TABLE chemical.sds_revisions
    ADD CONSTRAINT fk_sds_file
    FOREIGN KEY (file_object_id) REFERENCES platform.file_objects (id);

ALTER TABLE chemical.storage_inspections
    ADD CONSTRAINT fk_si_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE chemical.storage_inspections
    ADD CONSTRAINT fk_si_inspector
    FOREIGN KEY (inspected_by_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE chemical.exposure_controls
    ADD CONSTRAINT fk_ec_source
    FOREIGN KEY (source_record_id) REFERENCES platform.records (id);


--###############################################################################
-- ENVIRONMENT SCHEMA  (7 tables)
--###############################################################################

-- ===============================================================================
-- environment.parameters
-- ===============================================================================
CREATE TABLE environment.parameters (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    code                           varchar(60)          NOT NULL,
    name                           varchar(200)         NOT NULL,
    category                       varchar(60)          NOT NULL,
    default_unit                   varchar(30)          ,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_parameters_tenant_id ON environment.parameters (tenant_id);

-- ===============================================================================
-- environment.sources
-- ===============================================================================
CREATE TABLE environment.sources (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    site_id                        uuid                 NOT NULL,
    location_id                    uuid                 ,
    source_type                    varchar(60)          NOT NULL,
    name                           varchar(200)         NOT NULL,
    permit_reference               varchar(100)         
);

CREATE INDEX idx_sources_tenant_id ON environment.sources (tenant_id);
CREATE INDEX idx_env_sources_site ON environment.sources (site_id);

-- ===============================================================================
-- environment.monitoring_records
-- ===============================================================================
CREATE TABLE environment.monitoring_records (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    environment_source_id          uuid                 NOT NULL,
    period_start                   timestamptz          ,
    period_end                     timestamptz          ,
    performed_by_member_id         uuid                 ,
    status                         varchar(30)          NOT NULL
);

CREATE INDEX idx_monitoring_records_tenant_id ON environment.monitoring_records (tenant_id);
CREATE INDEX idx_monitoring_source ON environment.monitoring_records (environment_source_id);
CREATE INDEX idx_monitoring_performer ON environment.monitoring_records (performed_by_member_id);

-- ===============================================================================
-- environment.measurements
-- ===============================================================================
CREATE TABLE environment.measurements (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    monitoring_record_id           uuid                 NOT NULL,
    parameter_id                   uuid                 NOT NULL,
    measured_at                    timestamptz          NOT NULL,
    result_value                   decimal(24,8)        ,
    unit                           varchar(30)          ,
    limit_value                    decimal(24,8)        ,
    target_value                   decimal(24,8)        ,
    quality_flag                   varchar(30)          ,
    compliance_status              varchar(30)          
);

CREATE INDEX idx_measurements_tenant_id ON environment.measurements (tenant_id);
CREATE INDEX idx_measurements_monitoring ON environment.measurements (monitoring_record_id);
CREATE INDEX idx_measurements_parameter  ON environment.measurements (parameter_id);

-- ===============================================================================
-- environment.waste_records
-- ===============================================================================
CREATE TABLE environment.waste_records (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    waste_type                     varchar(80)          NOT NULL,
    is_hazardous                   boolean              NOT NULL,
    quantity                       decimal(24,8)        ,
    unit                           varchar(30)          ,
    source_location_id             uuid                 ,
    handler_name                   varchar(200)         ,
    manifest_number                varchar(100)         ,
    record_date                    date                 NOT NULL
);

CREATE INDEX idx_waste_records_tenant_id ON environment.waste_records (tenant_id);
CREATE INDEX idx_waste_location ON environment.waste_records (source_location_id);

-- ===============================================================================
-- environment.resource_usage
-- ===============================================================================
CREATE TABLE environment.resource_usage (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    resource_type                  varchar(50)          NOT NULL,
    site_id                        uuid                 NOT NULL,
    period_start                   date                 NOT NULL,
    period_end                     date                 NOT NULL,
    quantity                       decimal(24,8)        NOT NULL,
    unit                           varchar(30)          NOT NULL,
    source_reference               varchar(100)         
);

CREATE INDEX idx_resource_usage_tenant_id ON environment.resource_usage (tenant_id);
CREATE INDEX idx_resource_usage_site ON environment.resource_usage (site_id);

-- ===============================================================================
-- environment.targets
-- ===============================================================================
CREATE TABLE environment.targets (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    parameter_id                   uuid                 ,
    site_id                        uuid                 ,
    period_start                   date                 ,
    period_end                     date                 ,
    target_value                   decimal(24,8)        ,
    unit                           varchar(30)          ,
    owner_member_id                uuid                 
);

CREATE INDEX idx_targets_tenant_id ON environment.targets (tenant_id);
CREATE INDEX idx_env_targets_parameter ON environment.targets (parameter_id);
CREATE INDEX idx_env_targets_site      ON environment.targets (site_id);
CREATE INDEX idx_env_targets_owner     ON environment.targets (owner_member_id);

-- Intra-schema FKs
ALTER TABLE environment.monitoring_records
    ADD CONSTRAINT fk_mr_source
    FOREIGN KEY (environment_source_id) REFERENCES environment.sources (id);

ALTER TABLE environment.measurements
    ADD CONSTRAINT fk_meas_monitor
    FOREIGN KEY (monitoring_record_id) REFERENCES environment.monitoring_records (id);

ALTER TABLE environment.measurements
    ADD CONSTRAINT fk_meas_param
    FOREIGN KEY (parameter_id) REFERENCES environment.parameters (id);

ALTER TABLE environment.targets
    ADD CONSTRAINT fk_env_tgt_param
    FOREIGN KEY (parameter_id) REFERENCES environment.parameters (id);

-- Cross-schema FKs
ALTER TABLE environment.sources
    ADD CONSTRAINT fk_src_site
    FOREIGN KEY (site_id) REFERENCES org.sites (id);

ALTER TABLE environment.sources
    ADD CONSTRAINT fk_src_location
    FOREIGN KEY (location_id) REFERENCES org.locations (id);

ALTER TABLE environment.monitoring_records
    ADD CONSTRAINT fk_mr_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE environment.monitoring_records
    ADD CONSTRAINT fk_mr_performer
    FOREIGN KEY (performed_by_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE environment.waste_records
    ADD CONSTRAINT fk_waste_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE environment.waste_records
    ADD CONSTRAINT fk_waste_location
    FOREIGN KEY (source_location_id) REFERENCES org.locations (id);

ALTER TABLE environment.resource_usage
    ADD CONSTRAINT fk_ru_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE environment.resource_usage
    ADD CONSTRAINT fk_ru_site
    FOREIGN KEY (site_id) REFERENCES org.sites (id);

ALTER TABLE environment.targets
    ADD CONSTRAINT fk_env_tgt_site
    FOREIGN KEY (site_id) REFERENCES org.sites (id);

ALTER TABLE environment.targets
    ADD CONSTRAINT fk_env_tgt_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members (id);


--###############################################################################
-- SUSTAINABILITY SCHEMA  (4 tables)
--###############################################################################

-- ===============================================================================
-- sustainability.indicator_definitions
-- ===============================================================================
CREATE TABLE sustainability.indicator_definitions (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    code                           varchar(60)          NOT NULL,
    name                           varchar(200)         NOT NULL,
    boundary_definition            text                 ,
    default_unit                   varchar(30)          ,
    framework_reference            varchar(150)         ,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_indicator_definitions_tenant_id ON sustainability.indicator_definitions (tenant_id);

-- ===============================================================================
-- sustainability.factor_versions
-- ===============================================================================
CREATE TABLE sustainability.factor_versions (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    factor_code                    varchar(80)          NOT NULL,
    version_number                 int                  NOT NULL,
    factor_value                   decimal(24,10)       NOT NULL,
    unit                           varchar(60)          NOT NULL,
    source_reference               text                 ,
    effective_from                 date                 ,
    effective_to                   date                 
);

CREATE INDEX idx_factor_versions_tenant_id ON sustainability.factor_versions (tenant_id);

-- ===============================================================================
-- sustainability.measurements
-- ===============================================================================
CREATE TABLE sustainability.measurements (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    indicator_definition_id        uuid                 NOT NULL,
    factor_version_id              uuid                 ,
    scope_code                     varchar(20)          ,
    period_start                   date                 NOT NULL,
    period_end                     date                 NOT NULL,
    actual_value                   decimal(24,8)        ,
    unit                           varchar(30)          ,
    calculation_json               jsonb                ,
    owner_member_id                uuid                 
);

CREATE INDEX idx_measurements_tenant_id ON sustainability.measurements (tenant_id);
CREATE INDEX idx_sust_meas_indicator ON sustainability.measurements (indicator_definition_id);
CREATE INDEX idx_sust_meas_factor    ON sustainability.measurements (factor_version_id);
CREATE INDEX idx_sust_meas_owner     ON sustainability.measurements (owner_member_id);

-- ===============================================================================
-- sustainability.targets
-- ===============================================================================
CREATE TABLE sustainability.targets (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    indicator_definition_id        uuid                 NOT NULL,
    site_id                        uuid                 ,
    period_start                   date                 ,
    period_end                     date                 ,
    target_value                   decimal(24,8)        ,
    unit                           varchar(30)          
);

CREATE INDEX idx_targets_tenant_id ON sustainability.targets (tenant_id);
CREATE INDEX idx_sust_targets_indicator ON sustainability.targets (indicator_definition_id);
CREATE INDEX idx_sust_targets_site      ON sustainability.targets (site_id);

-- Intra-schema FKs
ALTER TABLE sustainability.measurements
    ADD CONSTRAINT fk_sust_meas_indicator
    FOREIGN KEY (indicator_definition_id) REFERENCES sustainability.indicator_definitions (id);

ALTER TABLE sustainability.measurements
    ADD CONSTRAINT fk_sust_meas_factor
    FOREIGN KEY (factor_version_id) REFERENCES sustainability.factor_versions (id);

ALTER TABLE sustainability.targets
    ADD CONSTRAINT fk_sust_tgt_indicator
    FOREIGN KEY (indicator_definition_id) REFERENCES sustainability.indicator_definitions (id);

-- Cross-schema FKs
ALTER TABLE sustainability.measurements
    ADD CONSTRAINT fk_sust_meas_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE sustainability.measurements
    ADD CONSTRAINT fk_sust_meas_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE sustainability.targets
    ADD CONSTRAINT fk_sust_tgt_site
    FOREIGN KEY (site_id) REFERENCES org.sites (id);


--###############################################################################
-- ASSET SCHEMA  (6 tables)
--###############################################################################

-- ===============================================================================
-- asset.assets
-- ===============================================================================
CREATE TABLE asset.assets (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    source_system                  varchar(60)          ,
    source_id                      varchar(100)         ,
    asset_code                     varchar(80)          NOT NULL,
    asset_name                     varchar(200)         NOT NULL,
    asset_type                     varchar(80)          ,
    site_id                        uuid                 NOT NULL,
    location_id                    uuid                 ,
    is_safety_critical             boolean              NOT NULL,
    status                         varchar(30)          NOT NULL
);

CREATE INDEX idx_assets_tenant_id ON asset.assets (tenant_id);
CREATE INDEX idx_assets_code   ON asset.assets (asset_code);
CREATE INDEX idx_assets_site   ON asset.assets (site_id);
CREATE INDEX idx_assets_location ON asset.assets (location_id);

-- ===============================================================================
-- asset.safety_requirements
-- ===============================================================================
CREATE TABLE asset.safety_requirements (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    asset_id                       uuid                 NOT NULL,
    requirement_type               varchar(60)          NOT NULL,
    frequency_days                 int                  ,
    competency_id                  uuid                 ,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_safety_requirements_tenant_id ON asset.safety_requirements (tenant_id);
CREATE INDEX idx_asset_safety_req_asset     ON asset.safety_requirements (asset_id);
CREATE INDEX idx_asset_safety_req_competency ON asset.safety_requirements (competency_id);

-- ===============================================================================
-- asset.inspections
-- ===============================================================================
CREATE TABLE asset.inspections (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    asset_id                       uuid                 NOT NULL,
    inspection_type                varchar(60)          NOT NULL,
    inspected_at                   timestamptz          ,
    inspected_by_person_id         uuid                 ,
    result                         varchar(30)          NOT NULL,
    next_due_date                  date                 
);

CREATE INDEX idx_inspections_tenant_id ON asset.inspections (tenant_id);
CREATE INDEX idx_asset_insp_asset   ON asset.inspections (asset_id);
CREATE INDEX idx_asset_insp_person  ON asset.inspections (inspected_by_person_id);

-- ===============================================================================
-- asset.certificates
-- ===============================================================================
CREATE TABLE asset.certificates (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    asset_id                       uuid                 NOT NULL,
    certificate_type               varchar(60)          NOT NULL,
    certificate_number             varchar(100)         ,
    issue_date                     date                 ,
    expiry_date                    date                 ,
    result                         varchar(30)          ,
    file_object_id                 uuid                 
);

CREATE INDEX idx_certificates_tenant_id ON asset.certificates (tenant_id);
CREATE INDEX idx_asset_certs_asset ON asset.certificates (asset_id);

-- ===============================================================================
-- asset.defects
-- ===============================================================================
CREATE TABLE asset.defects (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    asset_id                       uuid                 NOT NULL,
    description                    text                 NOT NULL,
    severity                       varchar(30)          ,
    restriction_status             varchar(30)          ,
    maintenance_reference          varchar(100)         ,
    owner_member_id                uuid                 
);

CREATE INDEX idx_defects_tenant_id ON asset.defects (tenant_id);
CREATE INDEX idx_asset_defects_asset  ON asset.defects (asset_id);
CREATE INDEX idx_asset_defects_owner  ON asset.defects (owner_member_id);

-- ===============================================================================
-- asset.operator_assignments
-- ===============================================================================
CREATE TABLE asset.operator_assignments (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    asset_id                       uuid                 NOT NULL,
    person_id                      uuid                 NOT NULL,
    assigned_from                  date                 ,
    assigned_to                    date                 ,
    eligibility_status             varchar(30)          
);

CREATE INDEX idx_operator_assignments_tenant_id ON asset.operator_assignments (tenant_id);
CREATE INDEX idx_asset_op_assign_asset  ON asset.operator_assignments (asset_id);
CREATE INDEX idx_asset_op_assign_person ON asset.operator_assignments (person_id);

-- Intra-schema FKs
ALTER TABLE asset.safety_requirements
    ADD CONSTRAINT fk_asr_asset
    FOREIGN KEY (asset_id) REFERENCES asset.assets (id);

ALTER TABLE asset.inspections
    ADD CONSTRAINT fk_ainsp_asset
    FOREIGN KEY (asset_id) REFERENCES asset.assets (id);

ALTER TABLE asset.certificates
    ADD CONSTRAINT fk_acert_asset
    FOREIGN KEY (asset_id) REFERENCES asset.assets (id);

ALTER TABLE asset.defects
    ADD CONSTRAINT fk_adef_asset
    FOREIGN KEY (asset_id) REFERENCES asset.assets (id);

ALTER TABLE asset.operator_assignments
    ADD CONSTRAINT fk_oa_asset
    FOREIGN KEY (asset_id) REFERENCES asset.assets (id);

-- Cross-schema FKs
ALTER TABLE asset.assets
    ADD CONSTRAINT fk_assets_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE asset.assets
    ADD CONSTRAINT fk_assets_site
    FOREIGN KEY (site_id) REFERENCES org.sites (id);

ALTER TABLE asset.assets
    ADD CONSTRAINT fk_assets_location
    FOREIGN KEY (location_id) REFERENCES org.locations (id);

ALTER TABLE asset.safety_requirements
    ADD CONSTRAINT fk_asr_competency
    FOREIGN KEY (competency_id) REFERENCES training.competencies (id);

ALTER TABLE asset.inspections
    ADD CONSTRAINT fk_ainsp_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE asset.inspections
    ADD CONSTRAINT fk_ainsp_person
    FOREIGN KEY (inspected_by_person_id) REFERENCES org.people (id);

ALTER TABLE asset.certificates
    ADD CONSTRAINT fk_acert_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE asset.certificates
    ADD CONSTRAINT fk_acert_file
    FOREIGN KEY (file_object_id) REFERENCES platform.file_objects (id);

ALTER TABLE asset.defects
    ADD CONSTRAINT fk_adef_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE asset.defects
    ADD CONSTRAINT fk_adef_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE asset.operator_assignments
    ADD CONSTRAINT fk_oa_person
    FOREIGN KEY (person_id) REFERENCES org.people (id);


--###############################################################################
-- EMERGENCY SCHEMA  (7 tables)
--###############################################################################

-- ===============================================================================
-- emergency.plans
-- ===============================================================================
CREATE TABLE emergency.plans (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    code                           varchar(50)          NOT NULL,
    name                           varchar(200)         NOT NULL,
    site_id                        uuid                 NOT NULL,
    owner_member_id                uuid                 NOT NULL,
    status                         varchar(30)          NOT NULL
);

CREATE INDEX idx_plans_tenant_id ON emergency.plans (tenant_id);
CREATE INDEX idx_emerg_plans_site   ON emergency.plans (site_id);
CREATE INDEX idx_emerg_plans_owner  ON emergency.plans (owner_member_id);

-- ===============================================================================
-- emergency.plan_revisions
-- ===============================================================================
CREATE TABLE emergency.plan_revisions (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    emergency_plan_id              uuid                 NOT NULL,
    revision_number                varchar(30)          NOT NULL,
    effective_date                 date                 ,
    file_object_id                 uuid                 ,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_plan_revisions_tenant_id ON emergency.plan_revisions (tenant_id);
CREATE INDEX idx_emerg_pr_plan ON emergency.plan_revisions (emergency_plan_id);

-- ===============================================================================
-- emergency.team_members
-- ===============================================================================
CREATE TABLE emergency.team_members (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    emergency_plan_id              uuid                 NOT NULL,
    person_id                      uuid                 NOT NULL,
    emergency_role                 varchar(80)          NOT NULL,
    valid_from                     date                 ,
    valid_to                       date                 
);

CREATE INDEX idx_team_members_tenant_id ON emergency.team_members (tenant_id);
CREATE INDEX idx_emerg_tm_plan   ON emergency.team_members (emergency_plan_id);
CREATE INDEX idx_emerg_tm_person ON emergency.team_members (person_id);

-- ===============================================================================
-- emergency.equipment
-- ===============================================================================
CREATE TABLE emergency.equipment (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    site_id                        uuid                 NOT NULL,
    location_id                    uuid                 ,
    equipment_type                 varchar(80)          NOT NULL,
    asset_id                       uuid                 ,
    inspection_due_date            date                 ,
    maintenance_due_date           date                 ,
    status                         varchar(30)          NOT NULL
);

CREATE INDEX idx_equipment_tenant_id ON emergency.equipment (tenant_id);
CREATE INDEX idx_emerg_equip_site     ON emergency.equipment (site_id);
CREATE INDEX idx_emerg_equip_location ON emergency.equipment (location_id);
CREATE INDEX idx_emerg_equip_asset    ON emergency.equipment (asset_id);

-- ===============================================================================
-- emergency.drills
-- ===============================================================================
CREATE TABLE emergency.drills (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    emergency_plan_id              uuid                 NOT NULL,
    scenario                       text                 NOT NULL,
    scheduled_at                   timestamptz          ,
    conducted_at                   timestamptz          ,
    result_summary                 text                 ,
    coordinator_member_id          uuid                 
);

CREATE INDEX idx_drills_tenant_id ON emergency.drills (tenant_id);
CREATE INDEX idx_emerg_drill_plan       ON emergency.drills (emergency_plan_id);
CREATE INDEX idx_emerg_drill_coordinator ON emergency.drills (coordinator_member_id);

-- ===============================================================================
-- emergency.drill_participants
-- ===============================================================================
CREATE TABLE emergency.drill_participants (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    emergency_drill_id             uuid                 NOT NULL,
    person_id                      uuid                 NOT NULL,
    participant_role               varchar(80)          ,
    attendance_status              varchar(30)          
);

CREATE INDEX idx_drill_participants_tenant_id ON emergency.drill_participants (tenant_id);
CREATE INDEX idx_emerg_dp_drill  ON emergency.drill_participants (emergency_drill_id);
CREATE INDEX idx_emerg_dp_person ON emergency.drill_participants (person_id);

-- ===============================================================================
-- emergency.drill_findings
-- ===============================================================================
CREATE TABLE emergency.drill_findings (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    record_id                      uuid                 NOT NULL UNIQUE,
    emergency_drill_id             uuid                 NOT NULL,
    description                    text                 NOT NULL,
    severity                       varchar(30)          ,
    owner_member_id                uuid                 
);

CREATE INDEX idx_drill_findings_tenant_id ON emergency.drill_findings (tenant_id);
CREATE INDEX idx_emerg_df_drill ON emergency.drill_findings (emergency_drill_id);
CREATE INDEX idx_emerg_df_owner ON emergency.drill_findings (owner_member_id);

-- Intra-schema FKs
ALTER TABLE emergency.plan_revisions
    ADD CONSTRAINT fk_epr_plan
    FOREIGN KEY (emergency_plan_id) REFERENCES emergency.plans (id);

ALTER TABLE emergency.team_members
    ADD CONSTRAINT fk_etm_plan
    FOREIGN KEY (emergency_plan_id) REFERENCES emergency.plans (id);

ALTER TABLE emergency.drills
    ADD CONSTRAINT fk_edrill_plan
    FOREIGN KEY (emergency_plan_id) REFERENCES emergency.plans (id);

ALTER TABLE emergency.drill_participants
    ADD CONSTRAINT fk_edp_drill
    FOREIGN KEY (emergency_drill_id) REFERENCES emergency.drills (id);

ALTER TABLE emergency.drill_findings
    ADD CONSTRAINT fk_edf_drill
    FOREIGN KEY (emergency_drill_id) REFERENCES emergency.drills (id);

-- Cross-schema FKs
ALTER TABLE emergency.plans
    ADD CONSTRAINT fk_ep_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE emergency.plans
    ADD CONSTRAINT fk_ep_site
    FOREIGN KEY (site_id) REFERENCES org.sites (id);

ALTER TABLE emergency.plans
    ADD CONSTRAINT fk_ep_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE emergency.plan_revisions
    ADD CONSTRAINT fk_epr_file
    FOREIGN KEY (file_object_id) REFERENCES platform.file_objects (id);

ALTER TABLE emergency.team_members
    ADD CONSTRAINT fk_etm_person
    FOREIGN KEY (person_id) REFERENCES org.people (id);

ALTER TABLE emergency.equipment
    ADD CONSTRAINT fk_ee_site
    FOREIGN KEY (site_id) REFERENCES org.sites (id);

ALTER TABLE emergency.equipment
    ADD CONSTRAINT fk_ee_location
    FOREIGN KEY (location_id) REFERENCES org.locations (id);

ALTER TABLE emergency.equipment
    ADD CONSTRAINT fk_ee_asset
    FOREIGN KEY (asset_id) REFERENCES asset.assets (id);

ALTER TABLE emergency.drills
    ADD CONSTRAINT fk_edrill_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE emergency.drills
    ADD CONSTRAINT fk_edrill_coord
    FOREIGN KEY (coordinator_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE emergency.drill_participants
    ADD CONSTRAINT fk_edp_person
    FOREIGN KEY (person_id) REFERENCES org.people (id);

ALTER TABLE emergency.drill_findings
    ADD CONSTRAINT fk_edf_record
    FOREIGN KEY (record_id) REFERENCES platform.records (id);

ALTER TABLE emergency.drill_findings
    ADD CONSTRAINT fk_edf_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members (id);


--###############################################################################
-- REPORTING SCHEMA  (5 tables)
--###############################################################################

-- ===============================================================================
-- reporting.kpi_definitions
-- ===============================================================================
CREATE TABLE reporting.kpi_definitions (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    code                           varchar(60)          NOT NULL,
    name                           varchar(200)         NOT NULL,
    description                    text                 ,
    owner_member_id                uuid                 NOT NULL,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_kpi_definitions_tenant_id ON reporting.kpi_definitions (tenant_id);
CREATE INDEX idx_kpi_defs_owner ON reporting.kpi_definitions (owner_member_id);

-- ===============================================================================
-- reporting.kpi_versions
-- ===============================================================================
CREATE TABLE reporting.kpi_versions (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    kpi_definition_id              uuid                 NOT NULL,
    version_number                 int                  NOT NULL,
    formula_expression             text                 NOT NULL,
    numerator_definition           text                 ,
    denominator_definition         text                 ,
    factor                         decimal(24,8)        ,
    period_rule                    varchar(60)          ,
    scope_rule_json                jsonb                ,
    effective_from                 date                 ,
    effective_to                   date                 
);

CREATE INDEX idx_kpi_versions_tenant_id ON reporting.kpi_versions (tenant_id);
CREATE INDEX idx_kpi_versions_kpi_def ON reporting.kpi_versions (kpi_definition_id);

-- ===============================================================================
-- reporting.report_definitions
-- ===============================================================================
CREATE TABLE reporting.report_definitions (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    code                           varchar(60)          NOT NULL,
    name                           varchar(200)         NOT NULL,
    report_type                    varchar(40)          NOT NULL,
    dataset_code                   varchar(80)          NOT NULL,
    filter_schema_json             jsonb                ,
    required_permission_id         uuid                 
);

CREATE INDEX idx_report_definitions_tenant_id ON reporting.report_definitions (tenant_id);

-- ===============================================================================
-- reporting.report_schedules
-- ===============================================================================
CREATE TABLE reporting.report_schedules (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    report_definition_id           uuid                 NOT NULL,
    owner_member_id                uuid                 NOT NULL,
    schedule_rule                  varchar(200)         NOT NULL,
    delivery_configuration_json    jsonb                ,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_report_schedules_tenant_id ON reporting.report_schedules (tenant_id);
CREATE INDEX idx_rpt_schedules_rpt_def ON reporting.report_schedules (report_definition_id);
CREATE INDEX idx_rpt_schedules_owner   ON reporting.report_schedules (owner_member_id);

-- ===============================================================================
-- reporting.report_executions
-- ===============================================================================
CREATE TABLE reporting.report_executions (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    report_definition_id           uuid                 NOT NULL,
    report_schedule_id             uuid                 ,
    requested_by_member_id         uuid                 ,
    filter_values_json             jsonb                ,
    status                         varchar(30)          NOT NULL,
    output_file_object_id          uuid                 ,
    started_at                     timestamptz          ,
    completed_at                   timestamptz          
);

CREATE INDEX idx_report_executions_tenant_id ON reporting.report_executions (tenant_id);
CREATE INDEX idx_rpt_exec_rpt_def   ON reporting.report_executions (report_definition_id);
CREATE INDEX idx_rpt_exec_schedule  ON reporting.report_executions (report_schedule_id);
CREATE INDEX idx_rpt_exec_requester ON reporting.report_executions (requested_by_member_id);

-- Intra-schema FKs
ALTER TABLE reporting.kpi_versions
    ADD CONSTRAINT fk_kv_kpi_def
    FOREIGN KEY (kpi_definition_id) REFERENCES reporting.kpi_definitions (id);

ALTER TABLE reporting.report_schedules
    ADD CONSTRAINT fk_rs_rpt_def
    FOREIGN KEY (report_definition_id) REFERENCES reporting.report_definitions (id);

ALTER TABLE reporting.report_executions
    ADD CONSTRAINT fk_re_rpt_def
    FOREIGN KEY (report_definition_id) REFERENCES reporting.report_definitions (id);

ALTER TABLE reporting.report_executions
    ADD CONSTRAINT fk_re_schedule
    FOREIGN KEY (report_schedule_id) REFERENCES reporting.report_schedules (id);

-- Cross-schema FKs
ALTER TABLE reporting.kpi_definitions
    ADD CONSTRAINT fk_kd_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE reporting.report_schedules
    ADD CONSTRAINT fk_rs_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE reporting.report_definitions
    ADD CONSTRAINT fk_rd_permission
    FOREIGN KEY (required_permission_id) REFERENCES iam.permissions (id);

ALTER TABLE reporting.report_executions
    ADD CONSTRAINT fk_re_requester
    FOREIGN KEY (requested_by_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE reporting.report_executions
    ADD CONSTRAINT fk_re_output_file
    FOREIGN KEY (output_file_object_id) REFERENCES platform.file_objects (id);


--###############################################################################
-- INTEGRATION SCHEMA  (5 tables)
--###############################################################################

-- ===============================================================================
-- integration.interfaces
-- ===============================================================================
CREATE TABLE integration.interfaces (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    code                           varchar(60)          NOT NULL,
    name                           varchar(200)         NOT NULL,
    source_system                  varchar(100)         NOT NULL,
    target_system                  varchar(100)         NOT NULL,
    integration_method             varchar(30)          NOT NULL,
    authentication_type            varchar(50)          ,
    owner_member_id                uuid                 ,
    status                         varchar(20)          NOT NULL
);

CREATE INDEX idx_interfaces_tenant_id ON integration.interfaces (tenant_id);
CREATE INDEX idx_intf_owner ON integration.interfaces (owner_member_id);

-- ===============================================================================
-- integration.data_mappings
-- ===============================================================================
CREATE TABLE integration.data_mappings (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    interface_id                   uuid                 NOT NULL,
    version_number                 int                  NOT NULL,
    source_schema_json             jsonb                ,
    target_schema_json             jsonb                ,
    mapping_rules_json             jsonb                ,
    effective_from                 timestamptz          
);

CREATE INDEX idx_data_mappings_tenant_id ON integration.data_mappings (tenant_id);
CREATE INDEX idx_dm_interface ON integration.data_mappings (interface_id);

-- ===============================================================================
-- integration.runs
-- ===============================================================================
CREATE TABLE integration.runs (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    interface_id                   uuid                 NOT NULL,
    mapping_id                     uuid                 ,
    correlation_id                 varchar(100)         ,
    started_at                     timestamptz          NOT NULL,
    completed_at                   timestamptz          ,
    status                         varchar(30)          NOT NULL,
    received_count                 bigint               ,
    success_count                  bigint               ,
    error_count                    bigint               
);

CREATE INDEX idx_runs_tenant_id ON integration.runs (tenant_id);
CREATE INDEX idx_intg_runs_interface ON integration.runs (interface_id);
CREATE INDEX idx_intg_runs_mapping   ON integration.runs (mapping_id);

-- ===============================================================================
-- integration.messages
-- ===============================================================================
CREATE TABLE integration.messages (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    integration_run_id             uuid                 NOT NULL,
    external_key                   varchar(200)         ,
    payload_hash                   varchar(64)          ,
    processing_status              varchar(30)          NOT NULL,
    error_code                     varchar(80)          ,
    error_message                  text                 ,
    retry_count                    int                  NOT NULL
);

CREATE INDEX idx_messages_tenant_id ON integration.messages (tenant_id);
CREATE INDEX idx_intg_msgs_run ON integration.messages (integration_run_id);

-- ===============================================================================
-- integration.reconciliations
-- ===============================================================================
CREATE TABLE integration.reconciliations (
    id                             uuid                 PRIMARY KEY,
    tenant_id                      uuid                 NOT NULL,
    integration_run_id             uuid                 NOT NULL,
    source_count                   bigint               ,
    target_count                   bigint               ,
    matched_count                  bigint               ,
    unmatched_count                bigint               ,
    status                         varchar(30)          NOT NULL,
    approved_by_member_id          uuid                 
);

CREATE INDEX idx_reconciliations_tenant_id ON integration.reconciliations (tenant_id);
CREATE INDEX idx_intg_recon_run      ON integration.reconciliations (integration_run_id);
CREATE INDEX idx_intg_recon_approver ON integration.reconciliations (approved_by_member_id);

-- Intra-schema FKs
ALTER TABLE integration.data_mappings
    ADD CONSTRAINT fk_dm_interface
    FOREIGN KEY (interface_id) REFERENCES integration.interfaces (id);

ALTER TABLE integration.runs
    ADD CONSTRAINT fk_ir_interface
    FOREIGN KEY (interface_id) REFERENCES integration.interfaces (id);

ALTER TABLE integration.runs
    ADD CONSTRAINT fk_ir_mapping
    FOREIGN KEY (mapping_id) REFERENCES integration.data_mappings (id);

ALTER TABLE integration.messages
    ADD CONSTRAINT fk_im_run
    FOREIGN KEY (integration_run_id) REFERENCES integration.runs (id);

ALTER TABLE integration.reconciliations
    ADD CONSTRAINT fk_irn_run
    FOREIGN KEY (integration_run_id) REFERENCES integration.runs (id);

-- Cross-schema FKs
ALTER TABLE integration.interfaces
    ADD CONSTRAINT fk_int_owner
    FOREIGN KEY (owner_member_id) REFERENCES iam.tenant_members (id);

ALTER TABLE integration.reconciliations
    ADD CONSTRAINT fk_irn_approver
    FOREIGN KEY (approved_by_member_id) REFERENCES iam.tenant_members (id);

-- =============================================================================
-- END OF WAVE 4 — Extended (12 domain schemas)
-- =============================================================================
