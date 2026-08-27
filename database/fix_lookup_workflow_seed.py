import psycopg2
import uuid as _uuid

url = open('.env.local').read().split('NEON_DATABASE_URL=')[1].strip()
TENANT_A = '11111111-1111-1111-1111-111111111111'
TENANT_B = '22222222-2222-2222-2222-222222222222'

conn = psycopg2.connect(url)
conn.autocommit = False
cur = conn.cursor()

def tenant_scoped_id(namespace_hex, tenant_id, seq):
    ns = _uuid.UUID(tenant_id)
    return str(_uuid.uuid5(ns, f'{namespace_hex}:{seq}'))

print('=== FIX lookup_values + workflow (per-tenant CROSS JOIN bug) ===')

# Clear existing synthetic rows for BOTH tenants (old pattern used 14xxxxxx/24xxxxxx)
cur.execute("DELETE FROM platform.lookup_values WHERE id::text LIKE '14%'")
print('Cleared lookup_values:', cur.rowcount)
cur.execute("DELETE FROM platform.workflow_versions WHERE id::text LIKE '24%'")
print('Cleared workflow_versions:', cur.rowcount)
cur.execute("DELETE FROM platform.workflow_definitions WHERE id::text LIKE '24%'")
print('Cleared workflow_definitions:', cur.rowcount)

conn.commit()

# Lookup values — same 20 items, unique id per tenant
lookups = [
    ('incident_type', 'LOST_TIME_INJURY', 'Lost Time Injury'),
    ('incident_type', 'MEDICAL_TREATMENT', 'Medical Treatment Case'),
    ('incident_type', 'NEAR_MISS', 'Near Miss'),
    ('incident_type', 'ENVIRONMENTAL', 'Environmental Incident'),
    ('incident_type', 'PROPERTY_DAMAGE', 'Property Damage'),
    ('severity', 'LOW', 'Low'),
    ('severity', 'MEDIUM', 'Medium'),
    ('severity', 'HIGH', 'High'),
    ('severity', 'CRITICAL', 'Critical'),
    ('hazard_category', 'MECHANICAL', 'Mechanical Hazard'),
    ('hazard_category', 'CHEMICAL', 'Chemical Hazard'),
    ('hazard_category', 'ELECTRICAL', 'Electrical Hazard'),
    ('hazard_category', 'ERGONOMIC', 'Ergonomic Hazard'),
    ('hazard_category', 'FALL', 'Fall Hazard'),
    ('observation_type', 'GOOD_CATCH', 'Good Catch'),
    ('observation_type', 'UNSAFE_ACT', 'Unsafe Act'),
    ('observation_type', 'UNSAFE_CONDITION', 'Unsafe Condition'),
    ('record_status', 'OPEN', 'Open'),
    ('record_status', 'IN_PROGRESS', 'In Progress'),
    ('record_status', 'CLOSED', 'Closed'),
]
for lidx, (cat, code, label) in enumerate(lookups):
    for tid in [TENANT_A, TENANT_B]:
        lid = tenant_scoped_id('lookup', tid, lidx)
        cur.execute(
            'INSERT INTO platform.lookup_values (id, tenant_id, category, code, label, status) VALUES (%s,%s,%s,%s,%s,%s) ON CONFLICT (id) DO NOTHING',
            (lid, tid, cat, code, label, 'active')
        )

# Workflow definitions + versions
wf_defs = [
    ('INCIDENT_CLOSURE', 'Incident Closure Workflow', 'incident'),
    ('OBSERVATION_FOLLOWUP', 'Observation Follow-up Workflow', 'safety'),
    ('CAPA_ACTION', 'CAPA Action Workflow', 'capa'),
]
wf_def_ids = {}
for widx, (code, name, module) in enumerate(wf_defs):
    for tid in [TENANT_A, TENANT_B]:
        wid = tenant_scoped_id('wfdef', tid, widx)
        wf_def_ids[(tid, code)] = wid
        cur.execute(
            'INSERT INTO platform.workflow_definitions (id, tenant_id, code, name, module_code, status) VALUES (%s,%s,%s,%s,%s,%s) ON CONFLICT (id) DO NOTHING',
            (wid, tid, code, name, module, 'active')
        )
        wvid = tenant_scoped_id('wfver', tid, widx)
        cur.execute(
            'INSERT INTO platform.workflow_versions (id, tenant_id, workflow_definition_id, version_number, effective_from, status) VALUES (%s,%s,%s,1,%s,%s) ON CONFLICT (id) DO NOTHING',
            (wvid, tid, wid, '2026-01-01T00:00:00Z', 'active')
        )

conn.commit()

print('\n=== VERIFY ===')
for schema, table in [('platform', 'lookup_values'), ('platform', 'workflow_definitions'), ('platform', 'workflow_versions')]:
    cur.execute(f'SELECT tenant_id, COUNT(*) FROM {schema}.{table} GROUP BY tenant_id ORDER BY tenant_id')
    rows = cur.fetchall()
    counts = {r[0]: r[1] for r in rows}
    a, b = counts.get(TENANT_A, 0), counts.get(TENANT_B, 0)
    flag = '✅' if a == b else '❌'
    print(f'  {flag} {schema}.{table}: tenant A={a}, tenant B={b}')

cur.close()
conn.close()
print('\nDone')