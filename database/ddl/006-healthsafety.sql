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

