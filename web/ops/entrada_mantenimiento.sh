#!/bin/sh
set -eu

if [ "${MIGRACION_MANUAL_DBA:-}" != "confirmada" ]; then
    echo "El mantenimiento solo puede ejecutarse mediante confirmación manual del DBA." >&2
    exit 78
fi
if [ "${BORRAR_TABLAS_MENU_HISTORICAS:-}" != "CONFIRMADO" ]; then
    echo "Confirme BORRAR_TABLAS_MENU_HISTORICAS=CONFIRMADO para retirar el legado." >&2
    exit 78
fi

exec python -m aplicacion.mantenimiento.retirar_menu_historico
