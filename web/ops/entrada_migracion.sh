#!/bin/sh
set -eu

if [ "${MIGRACION_MANUAL_DBA:-}" != "confirmada" ]; then
    echo "La imagen de migración solo puede ejecutarse mediante la confirmación manual del DBA." >&2
    exit 78
fi

exec python -m alembic -c alembic.ini "$@"
