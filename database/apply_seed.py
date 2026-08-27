import psycopg2, os

url = open('.env.local').read().split('NEON_DATABASE_URL=')[1].strip()
conn = psycopg2.connect(url)
conn.autocommit = False
cur = conn.cursor()

print('Applying seed data...')
try:
    with open('database/seed/006-dev-seed.sql') as f:
        sql = f.read()
    # Table owner bypasses RLS; but explicit SET LOCAL makes intent clear
    cur.execute("SET LOCAL app.current_tenant_id = '00000000-0000-0000-0000-000000000000'")
    cur.execute(sql)
    conn.commit()
    print('Seed applied successfully!')
except Exception as e:
    conn.rollback()
    print('SEED FAILED:', str(e)[:500])
    import traceback; traceback.print_exc()

print()
print('=== SEED VERIFICATION ===')
checks = [
    ('saas.tenants', 'SELECT COUNT(*) FROM saas.tenants'),
    ('saas.subscription_plans', 'SELECT COUNT(*) FROM saas.subscription_plans'),
    ('saas.plan_versions', 'SELECT COUNT(*) FROM saas.plan_versions'),
    ('saas.tenant_subscriptions', 'SELECT COUNT(*) FROM saas.tenant_subscriptions'),
    ('iam.users', 'SELECT COUNT(*) FROM iam.users'),
    ('iam.tenant_members', 'SELECT COUNT(*) FROM iam.tenant_members'),
    ('iam.roles', 'SELECT COUNT(*) FROM iam.roles'),
    ('iam.permissions', 'SELECT COUNT(*) FROM iam.permissions'),
    ('iam.role_permissions', 'SELECT COUNT(*) FROM iam.role_permissions'),
    ('iam.member_roles', 'SELECT COUNT(*) FROM iam.member_roles'),
    ('org.companies', 'SELECT COUNT(*) FROM org.companies'),
    ('org.business_units', 'SELECT COUNT(*) FROM org.business_units'),
    ('org.sites', 'SELECT COUNT(*) FROM org.sites'),
    ('org.departments', 'SELECT COUNT(*) FROM org.departments'),
    ('org.locations', 'SELECT COUNT(*) FROM org.locations'),
    ('org.positions', 'SELECT COUNT(*) FROM org.positions'),
    ('org.people', 'SELECT COUNT(*) FROM org.people'),
    ('org.employees', 'SELECT COUNT(*) FROM org.employees'),
    ('platform.data_classifications', 'SELECT COUNT(*) FROM platform.data_classifications'),
    ('platform.lookup_values', 'SELECT COUNT(*) FROM platform.lookup_values'),
    ('platform.records', 'SELECT COUNT(*) FROM platform.records'),
    ('platform.workflow_definitions', 'SELECT COUNT(*) FROM platform.workflow_definitions'),
    ('platform.workflow_versions', 'SELECT COUNT(*) FROM platform.workflow_versions'),
]
for name, q in checks:
    cur.execute(q)
    print(f'  {name}: {cur.fetchone()[0]} rows')

cur.close()
conn.close()
print('Done!')