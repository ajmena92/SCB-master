#!/usr/bin/env bash
# Deploy the portal through the preconfigured SSH alias, without copying secrets.
set -euo pipefail

usage() {
    echo "Uso: $0 [api|web|all] [--dry-run]"
    echo "  api  Despliega solo el backend (predeterminado)."
    echo "  web  Despliega solo el frontend."
    echo "  all  Despliega backend y frontend."
    echo "Las migraciones y el retiro de tablas históricas se ejecutan con rutinas DBA separadas."
}

component="api"
dry_run=false
for argument in "$@"; do
    case "$argument" in
        api|web|all) component="$argument" ;;
        --dry-run) dry_run=true ;;
        -h|--help) usage; exit 0 ;;
        *) usage >&2; exit 2 ;;
    esac
done

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
web_dir="$(cd "$script_dir/.." && pwd)"
remote_host="${SCSC_PRODUCTION_HOST:-scsc-production}"
remote_dir="${SCSC_PRODUCTION_DIR:-/home/plat/scsc-comedor}"
health_url="${SCSC_HEALTH_URL:-http://127.0.0.1:8081/health}"
compose="docker compose --env-file ops/.env -f ops/compose.production.yml"

run_remote() {
    local command="$1"
    ssh "$remote_host" "sudo bash -lc $(printf '%q' "$command")"
}

sync_directory() {
    local directory="$1"
    local options=(-az --exclude '__pycache__/' --exclude '*.pyc' --exclude 'node_modules/' --exclude 'build/')
    if "$dry_run"; then
        options+=(--dry-run)
    fi
    rsync "${options[@]}" --rsync-path='sudo rsync' "$web_dir/$directory/" "$remote_host:$remote_dir/$directory/"
}

sync_ops_directory() {
    local options=(-az --exclude '.env' --exclude '.env.local')
    if "$dry_run"; then
        options+=(--dry-run)
    fi
    rsync "${options[@]}" --rsync-path='sudo rsync' "$web_dir/ops/" "$remote_host:$remote_dir/ops/"
}

case "$component" in
    api)
        sync_directory backend
        sync_ops_directory
        services="api"
        ;;
    web)
        sync_directory frontend
        sync_ops_directory
        services="web"
        ;;
    all)
        sync_directory backend
        sync_directory frontend
        sync_ops_directory
        services="api web"
        ;;
esac

if "$dry_run"; then
    echo "Dry run completado: no se reconstruyó ningún contenedor."
    exit 0
fi

deploy_log="/tmp/scsc-deploy-${component}.log"
remote_deploy="set -euo pipefail
cd $(printf '%q' "$remote_dir")
if ! $compose up -d --build $services > $(printf '%q' "$deploy_log") 2>&1; then
    tail -n 120 $(printf '%q' "$deploy_log")
    exit 1
fi
$compose ps $services
rm -f $(printf '%q' "$deploy_log")"
run_remote "$remote_deploy"

for attempt in $(seq 1 18); do
    if run_remote "curl --fail --silent --show-error $(printf '%q' "$health_url")"; then
        echo "Despliegue completado: API saludable (intento $attempt)."
        exit 0
    fi
    sleep 5
done

echo "La API no superó la verificación de salud. Revise: $compose logs --tail=120 api" >&2
exit 1
