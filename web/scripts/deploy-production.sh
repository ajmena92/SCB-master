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
# El proxy público expone la salud de la API en /health. El valor se puede
# reemplazar al verificar un balanceador o dominio externo.
health_url="${SCSC_HEALTH_URL:-http://127.0.0.1:8081/health}"
ops_dir="$web_dir/ops"
env_file="$ops_dir/.env"
compose=(docker compose --env-file "$env_file" -f "$ops_dir/compose.production.yml")

valor_entorno() {
    local clave="$1"
    local valor
    valor="$(sed -n -E "s/^[[:space:]]*${clave}[[:space:]]*=[[:space:]]*(.*)[[:space:]]*$/\1/p" "$env_file" | tail -n 1)"
    valor="${valor%$'\r'}"
    valor="${valor#\"}"
    valor="${valor%\"}"
    printf '%s' "$valor"
}

ruta_ops() {
    local ruta="$1"
    if [[ "$ruta" == ./* ]]; then
        printf '%s/%s' "$ops_dir" "${ruta#./}"
    else
        printf '%s' "$ruta"
    fi
}

fallar_preflight() {
    echo "Preflight bloqueado: $1" >&2
    exit 1
}

verificar_secreto() {
    local nombre="$1"
    local archivo="$2"
    local exigir_root="$3"
    [[ -s "$archivo" ]] || fallar_preflight "falta o está vacío el secreto $nombre."

    local modo propietario
    modo="$(stat -c '%a' "$archivo")"
    propietario="$(stat -c '%U:%G' "$archivo")"
    [[ "$modo" == "600" ]] || fallar_preflight "el secreto $nombre debe tener permisos 0600."
    if [[ "$exigir_root" == "si" && "$propietario" != "root:root" ]]; then
        fallar_preflight "el secreto $nombre debe pertenecer a root:root en producción."
    fi
}

servicio_saludable() {
    local servicio="$1"
    local identificador estado
    identificador="$("${compose[@]}" ps -q "$servicio")"
    [[ -n "$identificador" ]] || fallar_preflight "el servicio $servicio no existe o no está iniciado."
    estado="$(docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' "$identificador")"
    [[ "$estado" == "healthy" || "$estado" == "running" ]] || \
        fallar_preflight "el servicio $servicio no está saludable (estado: $estado)."
}

verificar_espacio() {
    local etiqueta="$1"
    local ruta="$2"
    local minimo_kb="$3"
    [[ -d "$ruta" ]] || fallar_preflight "no existe el montaje de $etiqueta: $ruta."
    local disponible_kb
    disponible_kb="$(df -Pk "$ruta" | awk 'NR == 2 {print $4}')"
    [[ "$disponible_kb" =~ ^[0-9]+$ && "$disponible_kb" -ge "$minimo_kb" ]] || \
        fallar_preflight "espacio insuficiente en $etiqueta."
}

preflight_despliegue() {
    local entorno="$1"
    local exigir_root="no"
    local ruta_wal ruta_respaldos respaldo_reciente
    local postgres_db postgres_admin postgres_app postgres_migrador cantidad_roles version
    local postgres_data_path postgres_mount
    local minimo_kb="${PREFLIGHT_ESPACIO_MINIMO_KB:-1048576}"

    [[ -f "$env_file" ]] || fallar_preflight "falta $env_file."
    [[ "$entorno" == "produccion" ]] && exigir_root="si"
    [[ "$minimo_kb" =~ ^[0-9]+$ ]] || fallar_preflight "PREFLIGHT_ESPACIO_MINIMO_KB no es válido."

    "${compose[@]}" config --quiet || fallar_preflight "la configuración de Compose no es válida."
    verificar_secreto postgres_admin_password "$(ruta_ops "$(valor_entorno POSTGRES_ADMIN_PASSWORD_FILE || true)")" "$exigir_root"
    verificar_secreto postgres_app_password "$(ruta_ops "$(valor_entorno POSTGRES_APP_PASSWORD_FILE || true)")" "$exigir_root"
    verificar_secreto postgres_migrator_password "$(ruta_ops "$(valor_entorno POSTGRES_MIGRATOR_PASSWORD_FILE || true)")" "$exigir_root"
    verificar_secreto carnet_qr_clave "$(ruta_ops "$(valor_entorno CARNET_QR_CLAVE_FILE || true)")" "$exigir_root"
    verificar_secreto csrf_secret "$(ruta_ops "$(valor_entorno CSRF_SECRET_FILE || true)")" "$exigir_root"
    verificar_secreto importacion_resultados_key "$(ruta_ops "$(valor_entorno IMPORTACION_RESULTADOS_KEY_FILE || true)")" "$exigir_root"

    if [[ "$entorno" == "local" ]]; then
        # El Compose local conserva explícitamente este volumen externo.
        docker volume inspect scb-web_postgres_datos >/dev/null 2>&1 || \
            fallar_preflight "no existe el volumen PostgreSQL local esperado scb-web_postgres_datos."
    else
        # Producción usa el montaje bind administrado por infraestructura, no un
        # volumen Docker que podría inicializarse vacío durante un despliegue.
        postgres_data_path="$(valor_entorno POSTGRES_DATA_PATH)"
        [[ -n "$postgres_data_path" ]] || \
            fallar_preflight "falta POSTGRES_DATA_PATH para el montaje PostgreSQL de producción."
        [[ -d "$postgres_data_path" ]] || \
            fallar_preflight "no existe el montaje PostgreSQL de producción: $postgres_data_path."
        postgres_mount="$(docker inspect --format '{{range .Mounts}}{{if eq .Destination "/var/lib/postgresql/data"}}{{.Type}}:{{.Source}}{{end}}{{end}}' "$("${compose[@]}" ps -q postgres)")"
        [[ "$postgres_mount" == "bind:$postgres_data_path" ]] || \
            fallar_preflight "PostgreSQL no está montado desde POSTGRES_DATA_PATH."
    fi
    servicio_saludable postgres

    postgres_db="$(valor_entorno POSTGRES_DB)"
    postgres_db="${postgres_db:-scb}"
    postgres_admin="$(valor_entorno POSTGRES_ADMIN_USER)"
    postgres_admin="${postgres_admin:-scb_admin}"
    postgres_app="$(valor_entorno POSTGRES_APP_USER)"
    postgres_app="${postgres_app:-scb_api}"
    postgres_migrador="$(valor_entorno POSTGRES_MIGRATOR_USER)"
    postgres_migrador="${postgres_migrador:-scb_migrador}"
    version="$("${compose[@]}" exec -T postgres psql -v ON_ERROR_STOP=1 -U "$postgres_admin" -d "$postgres_db" -Atc \
        "SELECT version_num FROM public.alembic_version LIMIT 1;")" || \
        fallar_preflight "el clúster PostgreSQL no contiene una versión Alembic consultable."
    [[ -n "$version" ]] || fallar_preflight "el clúster PostgreSQL no está inicializado."
    cantidad_roles="$("${compose[@]}" exec -T postgres psql -v ON_ERROR_STOP=1 -U "$postgres_admin" -d "$postgres_db" -Atc \
        "SELECT count(*) FROM pg_roles WHERE rolname IN ('${postgres_admin}', '${postgres_app}', '${postgres_migrador}');")" || \
        fallar_preflight "no se pudieron comprobar los roles PostgreSQL."
    [[ "$cantidad_roles" == "3" ]] || fallar_preflight "faltan roles PostgreSQL esperados."

    if [[ "$entorno" == "produccion" ]]; then
        ruta_wal="$(valor_entorno POSTGRES_WAL_ARCHIVE_PATH)"
        ruta_respaldos="$(valor_entorno POSTGRES_BACKUP_PATH)"
        verificar_espacio "archivo WAL" "$ruta_wal" "$minimo_kb"
        verificar_espacio "respaldos" "$ruta_respaldos" "$minimo_kb"
        respaldo_reciente="$(find "$ruta_respaldos" -mindepth 1 -maxdepth 1 -type d -name '20??????T??????Z' -print | sort | tail -n 1)"
        [[ -n "$respaldo_reciente" && -f "$respaldo_reciente/COMPLETADO" && -f "$respaldo_reciente/SHA256SUMS" ]] || \
            fallar_preflight "no hay un respaldo PostgreSQL completo verificable."
        sha256sum --check --status "$respaldo_reciente/SHA256SUMS" || \
            fallar_preflight "el checksum del último respaldo PostgreSQL no es válido."
        echo "Preflight: último respaldo comprobado $(basename "$respaldo_reciente")."
    fi

    echo "Preflight $entorno aprobado: secretos, Compose y PostgreSQL están listos."
}

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
    compose+=(-f "$ops_dir/compose.local.yml")
    cd "$web_dir"
    preflight_despliegue "local"
    if "$dry_run"; then
        echo "Dry run local aprobado: se construirían los servicios $services sin dependencias."
        exit 0
    fi
    if [[ "$component" == "all" ]]; then
        echo "Ejecutando migraciones PostgreSQL locales..."
        "${compose[@]}" --profile migracion run --rm --no-deps --build \
            -e MIGRACION_MANUAL_DBA=confirmada migracion upgrade head
        current=$("${compose[@]}" --profile migracion run --rm --no-deps \
            -e MIGRACION_MANUAL_DBA=confirmada migracion current 2>/dev/null | tail -n 1 | sed 's/ (head)$//')
        head=$("${compose[@]}" --profile migracion run --rm --no-deps \
            -e MIGRACION_MANUAL_DBA=confirmada migracion heads 2>/dev/null | tail -n 1 | sed 's/ (head)$//')
        if [[ -z "$current" || "$current" != "$head" ]]; then
            echo "La versión Alembic instalada ($current) no coincide con la cabeza ($head)." >&2
            exit 1
        fi
    fi
    "${compose[@]}" build $services
    "${compose[@]}" up -d --no-deps $services
    puerto_web="${WEB_PUERTO:-8081}"
    curl --fail --silent --show-error "http://127.0.0.1:${puerto_web}/health" >/dev/null
    if [[ "$component" == "web" || "$component" == "all" ]]; then
        curl --fail --silent --show-error "http://127.0.0.1:${puerto_web}/admin" >/dev/null
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
remote_confirmation=$(printf '%q' "${CONFIRMAR_MIGRACION_DBA:-}")
preflight_functions="$(declare -f valor_entorno ruta_ops fallar_preflight verificar_secreto servicio_saludable verificar_espacio preflight_despliegue)"
remote_preflight="set -euo pipefail
cd $(printf '%q' "$remote_dir")
ops_dir=\"\$PWD/ops\"
env_file=\"\$ops_dir/.env\"
compose=(docker compose --env-file \"\$env_file\" -f \"\$ops_dir/compose.production.yml\")
if [[ -f ops/compose.production.server.yml ]]; then
    compose+=(-f \"\$ops_dir/compose.production.server.yml\")
fi
$preflight_functions
preflight_despliegue produccion"
run_remote "$remote_preflight"

remote_deploy="set -euo pipefail
cd $(printf '%q' "$remote_dir")
compose=(docker compose --env-file ops/.env -f ops/compose.production.yml)
if [[ -f ops/compose.production.server.yml ]]; then
    compose+=(-f ops/compose.production.server.yml)
fi
if [[ $(printf '%q' "$component") == all ]]; then
    if [[ $remote_confirmation != SI ]]; then
        echo \"deploy all requiere CONFIRMAR_MIGRACION_DBA=SI para ejecutar migraciones.\" >&2
        exit 2
    fi
    if ! \"\${compose[@]}\" --profile migracion run --rm --no-deps --build \\
        -e MIGRACION_MANUAL_DBA=confirmada migracion upgrade head >> $(printf '%q' "$deploy_log") 2>&1; then
        tail -n 120 $(printf '%q' "$deploy_log")
        exit 1
    fi
    current=\$(\"\${compose[@]}\" --profile migracion run --rm --no-deps \\
        -e MIGRACION_MANUAL_DBA=confirmada migracion current 2>/dev/null || true)
    current=\$(printf '%s\\n' "\$current" | tail -n 1 | sed 's/ (head)$//')
    head=\$(\"\${compose[@]}\" --profile migracion run --rm --no-deps \\
        -e MIGRACION_MANUAL_DBA=confirmada migracion heads 2>/dev/null || true)
    head=\$(printf '%s\\n' "\$head" | tail -n 1 | sed 's/ (head)$//')
    if [[ -z "\$current" || "\$current" != "\$head" ]]; then
        echo \"La versión Alembic instalada (\$current) no coincide con la cabeza (\$head).\" >&2
        exit 1
    fi
fi
if ! \"\${compose[@]}\" build $services > $(printf '%q' "$deploy_log") 2>&1; then
    tail -n 120 $(printf '%q' "$deploy_log")
    exit 1
fi
if ! \"\${compose[@]}\" up -d --no-deps $services >> $(printf '%q' "$deploy_log") 2>&1; then
    tail -n 120 $(printf '%q' "$deploy_log")
    exit 1
fi
\"\${compose[@]}\" ps $services
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
