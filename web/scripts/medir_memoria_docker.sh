#!/usr/bin/env bash

obtener_contenedor() {
    local servicio="$1"
    local contenedor
    contenedor="$(docker compose --env-file "$archivo_entorno" -f "$archivo_compose" ps -q "$servicio" | head -n 1)"
    if [[ -z "$contenedor" ]]; then
        echo "El servicio $servicio no tiene un contenedor activo." >&2
        exit 2
    fi
    echo "$contenedor"
}

verificar_contenedor_activo() {
    local servicio="$1"
    local contenedor="$2"
    local estado
    estado="$(docker inspect --format '{{.State.Status}}' "$contenedor")"
    if [[ "$estado" != "running" ]]; then
        echo "El servicio $servicio no está activo: $estado." >&2
        exit 2
    fi
}

convertir_a_mib() {
    local valor="$1"
    case "$valor" in
        *GiB) awk -v numero="${valor%GiB}" 'BEGIN { printf "%.2f", numero * 1024 }' ;;
        *MiB) awk -v numero="${valor%MiB}" 'BEGIN { printf "%.2f", numero }' ;;
        *KiB) awk -v numero="${valor%KiB}" 'BEGIN { printf "%.2f", numero / 1024 }' ;;
        *B) awk -v numero="${valor%B}" 'BEGIN { printf "%.2f", numero / 1024 / 1024 }' ;;
        *) echo "Unidad de memoria no reconocida: $valor" >&2; exit 2 ;;
    esac
}

obtener_memoria() {
    local contenedor="$1"
    local lectura uso porcentaje
    if ! lectura="$(docker stats --no-stream --format '{{.MemUsage}}|{{.MemPerc}}' "$contenedor" 2>/dev/null)"; then
        return 1
    fi
    uso="${lectura%%|*}"
    porcentaje="${lectura#*|}"
    porcentaje="${porcentaje%%%}"
    printf '%s\t%s\n' "$(convertir_a_mib "${uso%% *}")" "$porcentaje"
}

obtener_estado_operativo() {
    local contenedor="$1"
    local lectura
    if ! lectura="$(docker inspect --format '{{.RestartCount}}|{{.State.OOMKilled}}' "$contenedor" 2>/dev/null)"; then
        printf '0\ttrue\n'
        return
    fi
    printf '%s\t%s\n' "${lectura%%|*}" "${lectura#*|}"
}

obtener_trabajadores() {
    local contenedor="$1"
    local trabajadores
    trabajadores="$(docker inspect --format '{{range .Config.Env}}{{println .}}{{end}}' "$contenedor" 2>/dev/null | awk -F= '$1 == "UVICORN_WORKERS" { print $2; exit }')"
    if [[ -z "$trabajadores" ]]; then
        trabajadores=2
    fi
    echo "$trabajadores"
}
