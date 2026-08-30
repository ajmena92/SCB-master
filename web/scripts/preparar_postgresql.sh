#!/usr/bin/env bash
set -euo pipefail

ops_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/../ops" && pwd)"
destino="${1:-$ops_dir}"
secrets_dir="$destino/secrets"
mkdir -p "$secrets_dir" "$destino/importaciones"
chmod 0700 "$secrets_dir" "$destino/importaciones"

crear_secreto() {
    local nombre="$1" longitud="$2"
    local archivo="$secrets_dir/$nombre"
    if [[ -e "$archivo" ]]; then
        echo "Se conserva $archivo"
        return
    fi
    umask 077
    openssl rand -hex "$longitud" > "$archivo"
    chmod 0600 "$archivo"
    echo "Creado $archivo"
}

crear_secreto postgres_admin_password 32
crear_secreto postgres_app_password 32
crear_secreto postgres_migrator_password 32
crear_secreto codigo_migracion_semilla 32
if [[ ! -e "$secrets_dir/sql_server_origen" ]]; then
    umask 077
    : > "$secrets_dir/sql_server_origen"
    echo "Creado $secrets_dir/sql_server_origen; complete allí la cadena ODBC de solo lectura."
fi

if [[ ! -e "$destino/.env" ]]; then
    cp "$ops_dir/.env.example" "$destino/.env"
    chmod 0600 "$destino/.env"
    echo "Creado $destino/.env; ajuste rutas y origen HTTPS antes de producción."
fi
