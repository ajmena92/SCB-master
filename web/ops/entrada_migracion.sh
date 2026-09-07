#!/bin/sh
set -eu

if [ "${MIGRACION_MANUAL_DBA:-}" != "confirmada" ]; then
    echo "La imagen de migración solo puede ejecutarse mediante la confirmación manual del DBA." >&2
    exit 78
fi

archivo="${POSTGRES_PASSWORD_FILE:-/run/secrets/postgres_migrator_password}"
[ -r "$archivo" ] || { echo "Falta el secreto PostgreSQL del migrador" >&2; exit 78; }
password="$(tr -d '\r\n' < "$archivo")"
password_url="$(printf '%s' "$password" | python -c 'import sys; from urllib.parse import quote; print(quote(sys.stdin.read(), safe=""))')"
export DATABASE_URL="postgresql+psycopg://${POSTGRES_USER}:${password_url}@${POSTGRES_HOST}:${POSTGRES_PORT:-5432}/${POSTGRES_DB}"
unset password password_url

# El contenedor es efímero y conserva cap_drop/no-new-privileges; la conexión
# usa el rol PostgreSQL de migración con mínimo privilegio. No se eleva ningún
# permiso del host ni se reutiliza este proceso para ejecutar la API.
exec python -m alembic -c alembic.ini "$@"
