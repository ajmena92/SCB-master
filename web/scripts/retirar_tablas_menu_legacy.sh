#!/usr/bin/env bash
# Retira exclusivamente las tablas históricas del almacenamiento de menú.
set -euo pipefail

if [[ "${CONFIRMAR_MIGRACION_DBA:-}" != "SI" ]]; then
    echo "Confirme la operación DBA con CONFIRMAR_MIGRACION_DBA=SI." >&2
    exit 2
fi
if [[ "${CONFIRMAR_BORRADO_TABLAS_DEPRECIADAS:-}" != "SI" ]]; then
    echo "Confirme el borrado con CONFIRMAR_BORRADO_TABLAS_DEPRECIADAS=SI." >&2
    exit 2
fi

grupo_dba="${GRUPO_DBA_MIGRACION:-dba}"
if ! id -nG | tr ' ' '\n' | grep -Fxq "$grupo_dba"; then
    echo "La operación solo puede ejecutarla una cuenta del grupo institucional $grupo_dba." >&2
    exit 2
fi

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ops_dir="$(cd "$script_dir/../ops" && pwd)"
if [[ ! -f "$ops_dir/.env" ]]; then
    echo "Falta web/ops/.env; créelo desde .env.example mediante el almacén institucional." >&2
    exit 2
fi

cd "$ops_dir"
echo "Se validarán conteos y se retirarán únicamente ComedorPortal.MenuComponente y ComedorPortal.MenuPlantilla."
echo "Esta operación no se ejecuta automáticamente durante el despliegue."
docker compose --env-file .env --profile migracion -f compose.production.yml run --rm --no-deps \
    --env MIGRACION_MANUAL_DBA=confirmada \
    --env BORRAR_TABLAS_MENU_HISTORICAS=CONFIRMADO \
    --entrypoint /usr/local/bin/entrada_mantenimiento.sh migracion
