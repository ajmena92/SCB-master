#!/usr/bin/env bash
# Levanta la plataforma web local sin borrar la base de datos de desarrollo.
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
web_dir="$(cd "$script_dir/.." && pwd)"
ops_dir="$web_dir/ops"
env_file="$ops_dir/.env"

uso() {
    cat <<'EOF'
Uso: web/scripts/levantar-desarrollo-local.sh [--puerto PUERTO] [--sin-construir]

Levanta PostgreSQL, API y frontend para desarrollo local. Conserva siempre el
volumen scb-web_postgres_datos: no ejecuta `down -v`, no borra volúmenes y no
aplica migraciones automáticamente.

Opciones:
  --puerto PUERTO  Publica el frontend en el puerto indicado para esta ejecución.
  --sin-construir  Reutiliza las imágenes existentes en vez de reconstruirlas.
  -h, --help       Muestra esta ayuda.
EOF
}

puerto="8082"
construir=true
while (($#)); do
    case "$1" in
        --puerto)
            [[ $# -ge 2 && "$2" =~ ^[0-9]{2,5}$ ]] || {
                echo "--puerto requiere un número de puerto válido." >&2
                exit 2
            }
            puerto="$2"
            shift 2
            ;;
        --sin-construir)
            construir=false
            shift
            ;;
        -h|--help)
            uso
            exit 0
            ;;
        *)
            uso >&2
            exit 2
            ;;
    esac
done

[[ -f "$env_file" ]] || {
    echo "Falta $env_file. Cree la configuración local antes de levantar el entorno." >&2
    exit 1
}

for secreto in \
    postgres_admin_password \
    postgres_app_password \
    postgres_migrator_password \
    carnet_qr_clave \
    csrf_secret \
    importacion_resultados_key; do
    [[ -s "$ops_dir/secrets/$secreto" ]] || {
        echo "Falta o está vacío el secreto local: ops/secrets/$secreto" >&2
        exit 1
    }
done

compose=(
    docker compose
    --project-name scb-web
    --env-file "$env_file"
    -f "$ops_dir/compose.production.yml"
    -f "$ops_dir/compose.local.yml"
)

entorno=(env)
if [[ -n "$puerto" ]]; then
    entorno+=("WEB_PUERTO=$puerto")
else
    puerto="$(sed -n 's/^WEB_PUERTO=//p' "$env_file" | tail -n 1)"
    puerto="${puerto:-8082}"
fi
[[ "$puerto" =~ ^[0-9]+$ ]] && ((10#$puerto > 0 && 10#$puerto <= 65535)) || exit 2
entorno+=("WEB_PUERTO=$puerto" "WEB_HOST=127.0.0.1"
    "CORS_ORIGIN=http://127.0.0.1:$puerto" "APP_ENV=development" "COOKIE_SECURE=false")
docker volume inspect scb-web_postgres_datos >/dev/null || {
    echo "Falta la base de desarrollo scb-web_postgres_datos. Restaure el respaldo antes de continuar." >&2
    exit 1
}

cd "$web_dir"
echo "Levantando PostgreSQL, API y web en http://127.0.0.1:$puerto"

up_args=(up -d)
if "$construir"; then
    up_args+=(--build)
fi

"${entorno[@]}" "${compose[@]}" "${up_args[@]}" postgres api web

postgres_db="$(sed -n 's/^POSTGRES_DB=//p' "$env_file" | tail -n 1)"
postgres_db="${postgres_db:-scb}"
postgres_user="$(sed -n 's/^POSTGRES_ADMIN_USER=//p' "$env_file" | tail -n 1)"
postgres_user="${postgres_user:-scb_admin}"
if "${compose[@]}" exec -T postgres psql -U "$postgres_user" -d "$postgres_db" -Atc \
    "SELECT to_regclass('public.trabajo_importacion') IS NOT NULL;" | grep -qx 't'; then
    trabajador_args=(up -d)
    if "$construir"; then
        trabajador_args+=(--build)
    fi
    "${entorno[@]}" "${compose[@]}" "${trabajador_args[@]}" trabajador_importacion
else
    echo "El trabajador de importación no se inicia: falta su migración oficial."
fi

for intento in $(seq 1 18); do
    if curl --fail --silent --show-error "http://127.0.0.1:$puerto/health" >/dev/null; then
        python3 "$script_dir/verificar_acceso_local.py" "http://127.0.0.1:$puerto"
        echo "Entorno listo: http://127.0.0.1:$puerto"
        "${entorno[@]}" "${compose[@]}" ps postgres api web
        exit 0
    fi
    sleep 2
done

echo "La web no respondió en http://127.0.0.1:$puerto/health." >&2
"${entorno[@]}" "${compose[@]}" ps postgres api web >&2 || true
"${entorno[@]}" "${compose[@]}" logs --tail=80 api web >&2 || true
exit 1
