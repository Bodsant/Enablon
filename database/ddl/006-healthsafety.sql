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

