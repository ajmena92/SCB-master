#!/bin/sh
set -eu

archivo="${POSTGRES_PASSWORD_FILE:-/run/secrets/postgres_migrator_password}"
[ -r "$archivo" ] || { echo "Falta el secreto PostgreSQL de analítica" >&2; exit 78; }
contrasena="$(tr -d '\r\n' < "$archivo")"
export DATABASE_URL="postgresql+psycopg://${POSTGRES_USER}:${contrasena}@${POSTGRES_HOST}:${POSTGRES_PORT:-5432}/${POSTGRES_DB}"
unset contrasena
exec python -m analitica.ejecutor "$@"
