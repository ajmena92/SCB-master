#!/bin/sh
set -eu
archivo="${POSTGRES_PASSWORD_FILE:-/run/secrets/postgres_app_password}"
[ -r "$archivo" ] || { echo "Falta el secreto PostgreSQL de la API" >&2; exit 78; }
password="$(tr -d '\r\n' < "$archivo")"
export DATABASE_URL="postgresql+psycopg://${POSTGRES_USER}:${password}@${POSTGRES_HOST}:${POSTGRES_PORT:-5432}/${POSTGRES_DB}"
unset password
exec uvicorn aplicacion.entrada:crear_aplicacion --factory --host 0.0.0.0 --port 8000 \
    --workers "${UVICORN_WORKERS:-2}" --proxy-headers \
    --forwarded-allow-ips="${FORWARDED_ALLOW_IPS:-127.0.0.1}"
