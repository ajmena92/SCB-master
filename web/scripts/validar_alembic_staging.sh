#!/usr/bin/env bash
# Comprueba la conectividad de Alembic con PostgreSQL sin imprimir secretos.
set -euo pipefail

if [[ -z "${DATABASE_URL:-}" ]]; then
    echo "Falta DATABASE_URL; obténgala del almacén institucional." >&2
    exit 2
fi
if [[ "$DATABASE_URL" != postgresql://* && "$DATABASE_URL" != postgresql+psycopg://* ]]; then
    echo "DATABASE_URL debe usar el esquema postgresql:// o postgresql+psycopg://." >&2
    exit 2
fi

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
backend_dir="$(cd "$script_dir/../backend" && pwd)"
echo "Validando conexión PostgreSQL de staging (secreto omitido)..."
cd "$backend_dir"
# `current` abre la conexión y no aplica DDL.
python_bin="${PYTHON_BIN:-python}"
"$python_bin" -m alembic -c alembic.ini current
echo "Conexión Alembic de staging validada."
