import psycopg2, os, time, re

url = open('.env.local').read().split('NEON_DATABASE_URL=')[1].strip()
conn = psycopg2.connect(url)
conn.autocommit = False
cur = conn.cursor()

def strip_alter_tables(sql):
    """Remove ALTER TABLE ADD CONSTRAINT blocks, keeping only CREATE TABLE and CREATE INDEX."""
    lines = sql.split('\n')
    result = []
    skip = False
    for line in lines:
        stripped = line.strip().upper()
        if stripped.startswith('ALTER TABLE'):
            skip = True
            continue
        if skip:
            # Skip continuation lines of ALTER TABLE
            if stripped.startswith('ADD CONSTRAINT') or stripped.startswith('--'):
                continue
            if stripped == '' or stripped.startswith('CREATE') or stripped.startswith('--'):
                skip = False
            else:
                skip = False
        # Also skip standalone commented ALTER TABLE references
        if stripped.startswith('--ADD CONSTRAINT'):
            continue
        if stripped.startswith('-- FK TO'):
            continue
        result.append(line)
    return '\n'.join(result)

# Apply wave 3 tables only
print('Applying Wave 3 tables (no FKs) ...')
with open('database/ddl/003-operational.sql') as f:
    sql3 = f.read()
sql3_tables = strip_alter_tables(sql3)
try:
    cur.execute(sql3_tables)
    conn.commit()
    print('Wave 3 tables OK')
except Exception as e:
    conn.rollback()
    print('Wave 3 tables FAILED:', e)

# Apply wave 4 tables only
print('Applying Wave 4 tables (no FKs) ...')
with open('database/ddl/004-extended.sql') as f:
    sql4 = f.read()
sql4_tables = strip_alter_tables(sql4)
try:
    cur.execute(sql4_tables)
    conn.commit()
    print('Wave 4 tables OK')
except Exception as e:
    conn.rollback()
    print('Wave 4 tables FAILED:', e)

# Now apply ALL ALTER TABLE constraints from wave 3 and wave 4
# These are safe because all tables now exist
print('Applying Wave 3 FKs ...')
alter3_blocks = re.findall(r'ALTER TABLE.*?;', sql3, re.DOTALL)
for block in alter3_blocks:
    try:
        cur.execute(block)
    except Exception as e:
        conn.rollback()
        print(f'  FK failed: {str(e)[:100]}')
        conn.autocommit = False
conn.commit()
print('Wave 3 FKs done')

print('Applying Wave 4 FKs ...')
alter4_blocks = re.findall(r'ALTER TABLE.*?;', sql4, re.DOTALL)
for block in alter4_blocks:
    try:
        cur.execute(block)
    except Exception as e:
        conn.rollback()
        print(f'  FK failed: {str(e)[:100]}')
        conn.autocommit = False
conn.commit()
print('Wave 4 FKs done')

# Apply deferred FKs
print('Applying deferred FKs ...')
deferred = """
ALTER TABLE cow.work_requests
    ADD CONSTRAINT fk_work_requests_contractor
    FOREIGN KEY (contractor_company_id) REFERENCES contractor.companies (id);
ALTER TABLE cow.permits
    ADD CONSTRAINT fk_permits_contractor
    FOREIGN KEY (contractor_company_id) REFERENCES contractor.companies (id);
"""
try:
    cur.execute(deferred)
    conn.commit()
    print('Deferred FKs OK')
except Exception as e:
    conn.rollback()
    print('Deferred FKs FAILED:', e)

# Final summary
cur.execute("""SELECT schemaname, COUNT(*) FROM pg_tables
    WHERE schemaname NOT IN ('pg_catalog','information_schema')
    GROUP BY schemaname ORDER BY schemaname""")
print('\nSchema summary:')
total = 0
for row in cur.fetchall():
    print(f'  {row[0]}: {row[1]} tables')
    total += row[1]
print(f'  TOTAL: {total} tables')

cur.execute("""SELECT n.nspname, COUNT(*) FROM pg_constraint c
    JOIN pg_class cl ON c.conrelid=cl.oid
    JOIN pg_namespace n ON cl.relnamespace=n.oid
    WHERE c.contype='f' AND n.nspname NOT IN ('pg_catalog','information_schema')
    GROUP BY n.nspname ORDER BY n.nspname""")
fk_total = 0
print('\nFK summary:')
for row in cur.fetchall():
    print(f'  {row[0]}: {row[1]} foreign keys')
    fk_total += row[1]
print(f'  TOTAL: {fk_total} foreign keys')

cur.close()
conn.close()
print('\nAll done!')
