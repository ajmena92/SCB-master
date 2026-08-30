#!/bin/sh
set -eu

if [ "${MIGRACION_MANUAL_DBA:-}" != "confirmada" ]; then
    echo "La imagen de migración solo puede ejecutarse mediante la confirmación manual del DBA." >&2
    exit 78
fi

archivo="${POSTGRES_PASSWORD_FILE:-/run/secrets/postgres_migrator_password}"
[ -r "$archivo" ] || { echo "Falta el secreto PostgreSQL del migrador" >&2; exit 78; }
password="$(tr -d '\r\n' < "$archivo")"
export DATABASE_URL="postgresql+psycopg://${POSTGRES_USER}:${password}@${POSTGRES_HOST}:${POSTGRES_PORT:-5432}/${POSTGRES_DB}"
unset password

exec python -m alembic -c alembic.ini "$@"
