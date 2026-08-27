import psycopg2

# Build app connection string (same host/db, different role)
OWNER_URL = open('.env.local').read().split('NEON_DATABASE_URL=')[1].strip()
APP_URL = OWNER_URL.replace('neondb_owner', 'ehsms_app').replace('npg_yUMLOwmCG05Y', 'ehsms-dev-password-2026')

TENANT_A = '11111111-1111-1111-1111-111111111111'
TENANT_B = '22222222-2222-2222-2222-222222222222'

print('=== RLS ISOLATION TEST (as ehsms_app LOGIN role) ===\n')

conn = psycopg2.connect(APP_URL)
conn.autocommit = True
cur = conn.cursor()

def view_as(tenant_id, label):
    cur.execute("SELECT set_config('app.current_tenant_id', %s, false)", (tenant_id,))
    tables = [
        ('saas', 'tenant_subscriptions'), ('org', 'companies'), ('org', 'sites'),
        ('org', 'people'), ('iam', 'tenant_members'), ('iam', 'roles'),
        ('iam', 'member_roles'), ('platform', 'records'), ('platform', 'lookup_values'),
        ('platform', 'workflow_definitions'),
    ]
    print(f'--- {label} ---')
    for schema, table in tables:
        cur.execute(f'SELECT COUNT(*) FROM {schema}.{table}')
        print(f'  {schema}.{table}: {cur.fetchone()[0]}')

view_as(TENANT_A, 'PT Maju Jaya Energi (Tenant A)')
print()
view_as(TENANT_B, 'PT Sejahtera Bersama (Tenant B)')

print()
print('--- Cross-tenant attempts ---')
cur.execute("SELECT set_config('app.current_tenant_id', %s, false)", (TENANT_A,))
cur.execute("SELECT COUNT(*) FROM platform.records WHERE tenant_id = %s", (TENANT_B,))
leak = cur.fetchone()[0]
print(f'  Tenant A queries Tenant B records: {leak} rows (must be 0)')

cur.execute("SELECT set_config('app.current_tenant_id', %s, false)", (TENANT_B,))
cur.execute("SELECT COUNT(*) FROM platform.records WHERE tenant_id = %s", (TENANT_A,))
leak2 = cur.fetchone()[0]
print(f'  Tenant B queries Tenant A records: {leak2} rows (must be 0)')

print()
print('--- No tenant context (should FAIL or return 0) ---')
cur.execute("SELECT set_config('app.current_tenant_id', '', false)")
try:
    cur.execute("SELECT COUNT(*) FROM platform.records")
    print(f'  platform.records with no tenant: {cur.fetchone()[0]} rows')
    no_ctx_ok = cur.fetchone()[0] == 0
except Exception as e:
    print(f'  Query rejected without tenant context: {type(e).__name__} — ❌ intended fail-closed')
    print(f'    ({str(e)[:80]})')
    no_ctx_ok = True  # rejection is the expected fail-closed behavior

cur.close()
conn.close()
print()
status = '✅ PASS' if (leak == 0 and leak2 == 0 and no_ctx_ok) else '❌ FAIL'
print(f'=== Cross-tenant isolation: {status} ===')