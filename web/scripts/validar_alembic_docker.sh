#!/usr/bin/env bash
# Ejecuta Alembic dentro de la imagen API, sin exponer la cadena SQL.
set -euo pipefail

accion="${1:-current}"
case "$accion" in
    current|check|upgrade|downgrade) ;;
    *) echo "Uso: $0 [current|check|upgrade|downgrade]" >&2; exit 2 ;;
esac

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ops_dir="$(cd "$script_dir/../ops" && pwd)"
if [[ ! -f "$ops_dir/.env" ]]; then
    echo "Falta web/ops/.env; créelo desde .env.example mediante el almacén institucional." >&2
    exit 2
fi

cd "$ops_dir"
echo "Ejecutando Alembic en el contenedor API (acción: $accion; secretos omitidos)..."
if [[ "$accion" == "current" ]]; then
    docker compose --env-file .env -f compose.production.yml run --rm --no-deps api \
        python -m alembic -c alembic.ini current
elif [[ "$accion" == "check" ]]; then
    docker compose --env-file .env -f compose.production.yml run --rm --no-deps api \
        python -m alembic -c alembic.ini check
elif [[ "$accion" == "downgrade" ]]; then
    docker compose --env-file .env -f compose.production.yml run --rm --no-deps api \
        python -m alembic -c alembic.ini downgrade -1
else
    docker compose --env-file .env -f compose.production.yml run --rm --no-deps api \
        python -m alembic -c alembic.ini upgrade head
fi
