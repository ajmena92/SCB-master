#!/usr/bin/env bash
# Deploy the portal through the preconfigured SSH alias, without copying secrets.
set -euo pipefail

usage() {
    echo "Uso: $0 [api|web|all] [--local|--remote] [--dry-run]"
    echo "  api  Despliega solo el backend (predeterminado)."
    echo "  web  Despliega solo el frontend."
    echo "  all  Despliega backend y frontend."
    echo "  --local  Construye y publica en el Compose local."
    echo "  --remote Sincroniza y despliega en el servidor productivo."
    echo "deploy all ejecuta migraciones solo con CONFIRMAR_MIGRACION_DBA=SI; el retiro de tablas históricas sigue separado."
}

component="api"
dry_run=false
deployment_mode=""
for argument in "$@"; do
    case "$argument" in
        api|web|all) component="$argument" ;;
        --dry-run) dry_run=true ;;
        --local) deployment_mode="local" ;;
        --remote) deployment_mode="remote" ;;
        -h|--help) usage; exit 0 ;;
        *) usage >&2; exit 2 ;;
    esac
done

if [[ -z "$deployment_mode" ]]; then
    if [[ -t 0 ]]; then
        echo "Destino del despliegue:"
        echo "  1) Local (127.0.0.1:8081)"
        echo "  2) Remoto (servidor productivo)"
        read -r -p "Seleccione [1/2]: " destino
        case "$destino" in
            1) deployment_mode="local" ;;
            2) deployment_mode="remote" ;;
            *) echo "Destino inválido." >&2; exit 2 ;;
        esac
    else
        echo "Debe indicar --local o --remote cuando no hay terminal interactiva." >&2
        exit 2
    fi
fi

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
web_dir="$(cd "$script_dir/.." && pwd)"
remote_host="${SCSC_PRODUCTION_HOST:-181.193.106.90}"
remote_user="${SCSC_PRODUCTION_USER:-plat}"
remote_port="${SCSC_PRODUCTION_PORT:-40222}"
remote_dir="${SCSC_PRODUCTION_DIR:-/home/plat/scsc-comedor}"
health_url="${SCSC_HEALTH_URL:-http://127.0.0.1:8081/health}"
compose="docker compose --env-file ops/.env -f ops/compose.production.yml"

case "$component" in
    api) services="api" ;;
    web) services="web" ;;
    all) services="api web" ;;
esac

if [[ "$component" == "all" && "$dry_run" == false && "${CONFIRMAR_MIGRACION_DBA:-}" != "SI" ]]; then
    echo "deploy all requiere CONFIRMAR_MIGRACION_DBA=SI para ejecutar migraciones." >&2
    exit 2
fi

if [[ "$deployment_mode" == "local" ]]; then
    if "$dry_run"; then
        echo "Dry run local: se construirían los servicios $services."
        exit 0
    fi
    cd "$web_dir"
    $compose build $services
    $compose up -d $services
    if [[ "$component" == "all" ]]; then
        if [[ "${CONFIRMAR_MIGRACION_DBA:-}" != "SI" ]]; then
            echo "deploy all requiere CONFIRMAR_MIGRACION_DBA=SI para ejecutar migraciones." >&2
            exit 2
        fi
        echo "Ejecutando migraciones PostgreSQL locales..."
        $compose --profile migracion run --rm --no-deps --build \
            -e MIGRACION_MANUAL_DBA=confirmada migracion upgrade head
        current=$($compose --profile migracion run --rm --no-deps \
            -e MIGRACION_MANUAL_DBA=confirmada migracion current 2>/dev/null | tail -n 1 | sed 's/ (head)$//')
        head=$($compose --profile migracion run --rm --no-deps \
            -e MIGRACION_MANUAL_DBA=confirmada migracion heads 2>/dev/null | tail -n 1 | sed 's/ (head)$//')
        if [[ -z "$current" || "$current" != "$head" ]]; then
            echo "La versión Alembic instalada ($current) no coincide con la cabeza ($head)." >&2
            exit 1
        fi
    fi
    if [[ "$component" == "web" || "$component" == "all" ]]; then
        curl --fail --silent --show-error "http://127.0.0.1:${WEB_PUERTO:-8081}/admin" >/dev/null
    fi
    echo "Despliegue local completado: $services."
    exit 0
fi

run_remote() {
    local command="$1"
    ssh -p "$remote_port" "$remote_user@$remote_host" "sudo bash -lc $(printf '%q' "$command")"
}

sync_directory() {
    local directory="$1"
    # Los entornos virtuales y cachés son locales; sincronizarlos aumenta el
    # despliegue y puede introducir binarios de otra plataforma.
    local options=(-az --delete --exclude '__pycache__/' --exclude '*.pyc' --exclude '.venv/' --exclude '.venv-*/' --exclude 'node_modules/' --exclude 'build/')
    if "$dry_run"; then
        options+=(--dry-run)
    fi
    rsync "${options[@]}" -e "ssh -p $remote_port" --rsync-path='sudo rsync' "$web_dir/$directory/" "$remote_user@$remote_host:$remote_dir/$directory/"
}

sync_ops_directory() {
    local options=(-az --delete --exclude '.env' --exclude '.env.local' --exclude 'secrets/' --exclude 'importaciones/')
    if "$dry_run"; then
        options+=(--dry-run)
    fi
    rsync "${options[@]}" -e "ssh -p $remote_port" --rsync-path='sudo rsync' "$web_dir/ops/" "$remote_user@$remote_host:$remote_dir/ops/"
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
remote_compose=$(printf '%q' "$compose")
remote_confirmation=$(printf '%q' "${CONFIRMAR_MIGRACION_DBA:-}")
remote_deploy="set -euo pipefail
cd $(printf '%q' "$remote_dir")
compose=$remote_compose
if [[ -f ops/compose.production.server.yml ]]; then
    compose=\"\$compose -f ops/compose.production.server.yml\"
fi
if ! \$compose up -d --build $services > $(printf '%q' "$deploy_log") 2>&1; then
    tail -n 120 $(printf '%q' "$deploy_log")
    exit 1
fi
if [[ $(printf '%q' "$component") == all ]]; then
    if [[ $remote_confirmation != SI ]]; then
        echo \"deploy all requiere CONFIRMAR_MIGRACION_DBA=SI para ejecutar migraciones.\" >&2
        exit 2
    fi
    if ! \$compose --profile migracion run --rm --no-deps --build \\
        -e MIGRACION_MANUAL_DBA=confirmada migracion upgrade head >> $(printf '%q' "$deploy_log") 2>&1; then
        tail -n 120 $(printf '%q' "$deploy_log")
        exit 1
    fi
    current=\$(\$compose --profile migracion run --rm --no-deps \\
        -e MIGRACION_MANUAL_DBA=confirmada migracion current 2>/dev/null || true)
    current=\$(printf '%s\\n' "\$current" | tail -n 1 | sed 's/ (head)$//')
    head=\$(\$compose --profile migracion run --rm --no-deps \\
        -e MIGRACION_MANUAL_DBA=confirmada migracion heads 2>/dev/null || true)
    head=\$(printf '%s\\n' "\$head" | tail -n 1 | sed 's/ (head)$//')
    if [[ -z "\$current" || "\$current" != "\$head" ]]; then
        echo \"La versión Alembic instalada (\$current) no coincide con la cabeza (\$head).\" >&2
        exit 1
    fi
fi
\$compose ps $services
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
