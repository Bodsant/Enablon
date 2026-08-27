import psycopg2, os, time, re

url = open('.env.local').read().split('NEON_DATABASE_URL=')[1].strip()
conn = psycopg2.connect(url)
conn.autocommit = False
cur = conn.cursor()

def split_statements(sql):
    """Split SQL into individual statements at semicolons, respecting $$ blocks."""
    stmts = []
    current = []
    in_dollar = False
    for line in sql.split('\n'):
        stripped = line.strip()
        if stripped.startswith('$$'):
            in_dollar = not in_dollar
        current.append(line)
        if not in_dollar and stripped.endswith(';'):
            stmts.append('\n'.join(current))
            current = []
    if current:
        remaining = '\n'.join(current).strip()
        if remaining:
            stmts.append(remaining)
    return stmts

def is_alter_table(stmt):
    return stmt.strip().upper().startswith('ALTER TABLE')

def is_create_table(stmt):
    return stmt.strip().upper().startswith('CREATE TABLE')

def is_create_index(stmt):
    s = stmt.strip().upper()
    return s.startswith('CREATE INDEX') or s.startswith('CREATE UNIQUE INDEX')

# --- Apply Wave 4 ---
print('Applying Wave 4 tables only ...')
with open('database/ddl/004-extended.sql') as f:
    sql4 = f.read()

stmts4 = split_statements(sql4)
create_stmts = [s for s in stmts4 if is_create_table(s) or is_create_index(s)]
alter_stmts = [s for s in stmts4 if is_alter_table(s)]

print(f'  {len(create_stmts)} CREATE TABLE/INDEX statements, {len(alter_stmts)} ALTER TABLE statements')

# Apply creates
for stmt in create_stmts:
    try:
        cur.execute(stmt)
    except Exception as e:
        conn.rollback()
        print(f'  CREATE failed: {str(e)[:120]}')
        conn.autocommit = False
conn.commit()
print('Wave 4 tables OK')

# Apply alter tables
print('Applying Wave 4 FKs ...')
fk_ok = 0
fk_fail = 0
for stmt in alter_stmts:
    try:
        cur.execute(stmt)
        fk_ok += 1
    except Exception as e:
        conn.rollback()
        fk_fail += 1
        if fk_fail <= 5:
            print(f'  FK failed: {str(e)[:120]}')
        conn.autocommit = False
conn.commit()
print(f'  FKs: {fk_ok} ok, {fk_fail} failed')

# Deferred FKs
print('Applying deferred FKs ...')
deferred = [
    "ALTER TABLE cow.work_requests ADD CONSTRAINT fk_work_requests_contractor FOREIGN KEY (contractor_company_id) REFERENCES contractor.companies (id);",
    "ALTER TABLE cow.permits ADD CONSTRAINT fk_permits_contractor FOREIGN KEY (contractor_company_id) REFERENCES contractor.companies (id);",
]
for stmt in deferred:
    try:
        cur.execute(stmt)
    except Exception as e:
        conn.rollback()
        print(f'  Deferred FK failed: {str(e)[:120]}')
        conn.autocommit = False
conn.commit()

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
print('\nDone!')
