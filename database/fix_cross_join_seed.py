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

print('=== HARD CLEAN of identity seed rows (both tenants) ===')
# Order: children first
delete_plan = [
    ('iam', 'member_roles', "id::text LIKE '21%' OR id::text LIKE 'cded6138%'"),
    ('iam', 'role_permissions', "id::text LIKE '20%' OR id::text LIKE '8c4b9b1f%' OR id::text LIKE '33f7c0a2%'"),
    ('iam', 'roles', "id::text LIKE '18%' OR id::text LIKE '2c1b6b1e%'"),
    ('iam', 'permissions', "id::text LIKE '19%' OR id::text LIKE 'e63074f2%'"),
]
for schema, table, where in delete_plan:
    cur.execute(f'DELETE FROM {schema}.{table} WHERE {where}')
    print(f'  Cleared {schema}.{table}: {cur.rowcount}')

conn.commit()

print('\n=== RE-SEED clean ===')
# All ids via uuid5(ns=tenant, ...) — fully unique and deterministic
roles = [
    ('TENANT_ADMIN', 'Tenant Administrator', 'tenant', True),
    ('SITE_MANAGER', 'Site Manager', 'site', True),
    ('HSE_STAFF', 'HSE Staff', 'site', True),
    ('WORKER', 'Worker', 'company', True),
]
role_ids = {}
for ridx, (rcode, rname, scope, sysflag) in enumerate(roles):
    for tid in [TENANT_A, TENANT_B]:
        rid = tenant_scoped_id('role', tid, ridx)
        role_ids[(tid, rcode)] = rid
        cur.execute('INSERT INTO iam.roles (id, tenant_id, code, name, scope_type, is_system) VALUES (%s,%s,%s,%s,%s,%s) ON CONFLICT (id) DO NOTHING',
                    (rid, tid, rcode, rname, scope, sysflag))

perms = [
    ('incident.view', 'incident', 'view', 'View incidents'),
    ('incident.create', 'incident', 'create', 'Create incidents'),
    ('incident.resolve', 'incident', 'resolve', 'Resolve incidents'),
    ('safety.observe', 'safety', 'create', 'Report observations'),
    ('risk.assess', 'risk', 'assess', 'Perform risk assessments'),
    ('tenant.admin', 'saas', 'admin', 'Tenant administration'),
    ('org.manage', 'org', 'manage', 'Manage organization structure'),
]
perm_ids = {}
for pidx, (pcode, pmod, pact, pdesc) in enumerate(perms):
    for tid in [TENANT_A, TENANT_B]:
        pid = tenant_scoped_id('perm', tid, pidx)
        perm_ids[(tid, pcode)] = pid
        cur.execute('INSERT INTO iam.permissions (id, tenant_id, code, module, action, description) VALUES (%s,%s,%s,%s,%s,%s) ON CONFLICT (id) DO NOTHING',
                    (pid, tid, pcode, pmod, pact, pdesc))

rp_matrix = {
    'TENANT_ADMIN': [p for p, *_ in perms],
    'SITE_MANAGER': ['incident.view', 'incident.create', 'incident.resolve', 'safety.observe', 'risk.assess'],
    'HSE_STAFF': ['incident.view', 'incident.create', 'safety.observe', 'risk.assess'],
    'WORKER': ['safety.observe'],
}
rp_count = 0
for tid in [TENANT_A, TENANT_B]:
    for rcode, pcode_list in rp_matrix.items():
        for pcode in pcode_list:
            rpid = tenant_scoped_id('rp', tid, rp_count)
            cur.execute('INSERT INTO iam.role_permissions (id, tenant_id, role_id, permission_id) VALUES (%s,%s,%s,%s) ON CONFLICT (id) DO NOTHING',
                        (rpid, tid, role_ids[(tid, rcode)], perm_ids[(tid, pcode)]))
            rp_count += 1

member_roles = [
    (TENANT_A, '17000000-0000-0000-0000-000000000001', 'TENANT_ADMIN'),
    (TENANT_A, '17000000-0000-0000-0000-000000000002', 'HSE_STAFF'),
    (TENANT_A, '17000000-0000-0000-0000-000000000003', 'SITE_MANAGER'),
    (TENANT_B, '17000000-0000-0000-0000-000000000004', 'TENANT_ADMIN'),
    (TENANT_B, '17000000-0000-0000-0000-000000000005', 'HSE_STAFF'),
    (TENANT_B, '17000000-0000-0000-0000-000000000006', 'SITE_MANAGER'),
]
for midx, (tid, mid, rcode) in enumerate(member_roles):
    mrid = tenant_scoped_id('mr', tid, midx)
    cur.execute('INSERT INTO iam.member_roles (id, tenant_id, tenant_member_id, role_id) VALUES (%s,%s,%s,%s) ON CONFLICT (id) DO NOTHING',
                (mrid, tid, mid, role_ids[(tid, rcode)]))

conn.commit()

print('=== VERIFY (must be equal per tenant) ===')
ok = True
for schema, table in [('iam', 'roles'), ('iam', 'permissions'), ('iam', 'role_permissions'), ('iam', 'member_roles')]:
    cur.execute(f'SELECT tenant_id, COUNT(*) FROM {schema}.{table} GROUP BY tenant_id ORDER BY tenant_id')
    rows = cur.fetchall()
    counts = {r[0]: r[1] for r in rows}
    a, b = counts.get(TENANT_A, 0), counts.get(TENANT_B, 0)
    flag = '✅' if a == b else '❌'
    if a != b: ok = False
    print(f'  {flag} {schema}.{table}: tenant A={a}, tenant B={b}')

cur.close()
conn.close()
print('\n' + ('✅ All balanced' if ok else '❌ Imbalance — investigate'))