#!/usr/bin/env bash
# Comprueba configuración y conectividad Alembic en staging sin imprimir secretos.
set -euo pipefail

if [[ -z "${SQL_CONNECTION_STRING:-}" ]]; then
    echo "Falta SQL_CONNECTION_STRING; obténgala del almacén institucional." >&2
    exit 2
fi
if [[ "$SQL_CONNECTION_STRING" != *"ODBC Driver 18 for SQL Server"* ||
      "$SQL_CONNECTION_STRING" != *"Encrypt=yes"* ]]; then
    echo "SQL_CONNECTION_STRING debe usar ODBC Driver 18 y Encrypt=yes." >&2
    exit 2
fi

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
backend_dir="$(cd "$script_dir/../backend" && pwd)"
echo "Validando conexión Alembic de staging (secreto omitido)..."
cd "$backend_dir"
# `current` abre la conexión y no aplica DDL.
python_bin="${PYTHON_BIN:-python}"
"$python_bin" -m alembic -c alembic.ini current
echo "Conexión Alembic de staging validada."
