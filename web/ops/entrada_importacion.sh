#!/bin/sh
set -eu
leer() { tr -d '\r\n' < "$1"; }
password="$(leer "${POSTGRES_PASSWORD_FILE:-/run/secrets/postgres_migrator_password}")"
export URL_USUARIO="$POSTGRES_USER" URL_PASSWORD="$password" URL_HOST="$POSTGRES_HOST" \
    URL_PORT="${POSTGRES_PORT:-5432}" URL_DB="$POSTGRES_DB"
DATABASE_URL="$(python -c 'import os,urllib.parse as u; print("postgresql+psycopg://%s:%s@%s:%s/%s"%(u.quote(os.environ["URL_USUARIO"],safe=""),u.quote(os.environ["URL_PASSWORD"],safe=""),os.environ["URL_HOST"],os.environ["URL_PORT"],u.quote(os.environ["URL_DB"],safe="")))')"
export DATABASE_URL
unset password URL_PASSWORD
if [ -r "${SQL_SERVER_ORIGEN_FILE:-/run/secrets/sql_server_origen}" ]; then
    SQL_SERVER_ORIGEN="$(leer "${SQL_SERVER_ORIGEN_FILE:-/run/secrets/sql_server_origen}")"
    export SQL_SERVER_ORIGEN
fi
CODIGO_MIGRACION_SEMILLA="$(leer "${CODIGO_MIGRACION_SEMILLA_FILE:-/run/secrets/codigo_migracion_semilla}")"
export CODIGO_MIGRACION_SEMILLA
exec python "$@"
