#!/usr/bin/env bash
# Ejecuta Alembic dentro de su imagen aislada, sin exponer la cadena SQL.
set -euo pipefail

accion="${1:-current}"
case "$accion" in
    current|check|upgrade|downgrade) ;;
    *) echo "Uso: $0 [current|check|upgrade|downgrade]" >&2; exit 2 ;;
esac

grupo_dba="${GRUPO_DBA_MIGRACION:-dba}"
if ! id -nG | tr ' ' '\n' | grep -Fxq "$grupo_dba"; then
    echo "La migración solo puede ejecutarla una cuenta del grupo institucional $grupo_dba." >&2
    exit 2
fi
if [[ "${CONFIRMAR_MIGRACION_DBA:-}" != "SI" ]]; then
    echo "Confirme la ejecución manual del DBA con CONFIRMAR_MIGRACION_DBA=SI." >&2
    exit 2
fi

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ops_dir="$(cd "$script_dir/../ops" && pwd)"
if [[ ! -f "$ops_dir/.env" ]]; then
    echo "Falta web/ops/.env; créelo desde .env.example mediante el almacén institucional." >&2
    exit 2
fi

cd "$ops_dir"
echo "Ejecutando Alembic en la imagen de migración (acción: $accion; secretos omitidos)..."
docker compose --env-file .env --profile migracion -f compose.production.yml run --rm --no-deps \
    --env MIGRACION_MANUAL_DBA=confirmada migracion "$accion"
