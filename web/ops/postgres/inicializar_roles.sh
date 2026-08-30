#!/bin/sh
set -eu

leer_secreto() {
    archivo="$1"
    [ -r "$archivo" ] || { echo "No se puede leer el secreto $archivo" >&2; exit 78; }
    tr -d '\r\n' < "$archivo"
}

app_password="$(leer_secreto /run/secrets/postgres_app_password)"
migrator_password="$(leer_secreto /run/secrets/postgres_migrator_password)"

psql --set=ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" \
    --set=app_user="$POSTGRES_APP_USER" --set=app_password="$app_password" \
    --set=migrator_user="$POSTGRES_MIGRATOR_USER" --set=migrator_password="$migrator_password" <<'SQL'
SELECT format('CREATE ROLE %I LOGIN PASSWORD %L NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT', :'migrator_user', :'migrator_password')
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = :'migrator_user') \gexec
SELECT format('CREATE ROLE %I LOGIN PASSWORD %L NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT', :'app_user', :'app_password')
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = :'app_user') \gexec
SELECT format('GRANT CONNECT ON DATABASE %I TO %I', current_database(), :'migrator_user') \gexec
SELECT format('GRANT CONNECT ON DATABASE %I TO %I', current_database(), :'app_user') \gexec
GRANT CREATE, USAGE ON SCHEMA public TO :"migrator_user";
GRANT USAGE ON SCHEMA public TO :"app_user";
ALTER DEFAULT PRIVILEGES FOR ROLE :"migrator_user" IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO :"app_user";
ALTER DEFAULT PRIVILEGES FOR ROLE :"migrator_user" IN SCHEMA public
    GRANT USAGE, SELECT ON SEQUENCES TO :"app_user";
SQL

regla="host replication $POSTGRES_USER all scram-sha-256"
if ! grep -Fqx "$regla" "$PGDATA/pg_hba.conf"; then
    printf '%s\n' "$regla" >> "$PGDATA/pg_hba.conf"
fi
psql --set=ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" \
    --command "SELECT pg_reload_conf();" >/dev/null
unset app_password migrator_password
