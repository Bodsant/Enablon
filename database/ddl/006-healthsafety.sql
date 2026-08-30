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
