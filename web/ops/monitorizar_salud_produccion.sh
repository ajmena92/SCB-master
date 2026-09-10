#!/bin/sh
set -eu

umask 077
estado=0

advertir() {
    logger -t scb-salud -p user.warning -- "$1"
    printf '%s\n' "$1" >&2
    estado=1
}

revisar_uso() {
    etiqueta="$1"
    ruta="$2"
    uso="$(df -P "$ruta" | awk 'NR == 2 {gsub(/%/, "", $5); print $5}')"
    case "$uso" in
        ''|*[!0-9]*) advertir "SCB: no se pudo obtener uso de $etiqueta ($ruta)." ;;
        *)
            if [ "$uso" -ge 90 ]; then
                advertir "SCB CRITICO: $etiqueta usa ${uso}% ($ruta)."
            elif [ "$uso" -ge 75 ]; then
                advertir "SCB ADVERTENCIA: $etiqueta usa ${uso}% ($ruta)."
            fi
            ;;
    esac
}

revisar_uso "raiz" /
revisar_uso "PostgreSQL" /srv/scb-postgresql-v2
revisar_uso "WAL" /srv/scb-postgresql-v2/wal-7681048827487653926
revisar_uso "respaldos" /srv/scb-postgresql-v2/respaldos

for contenedor in scb-web-postgres-1 scb-web-api-1 scb-web-web-1; do
    salud="$(docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' "$contenedor" 2>/dev/null || true)"
    case "$salud" in
        healthy|running) ;;
        *) advertir "SCB CRITICO: $contenedor no esta saludable (${salud:-ausente})." ;;
    esac
done

respaldo="$(find /srv/scb-postgresql-v2/respaldos -mindepth 1 -maxdepth 1 -type d -name '20??????T??????Z' -print | sort | tail -n 1)"
if [ -z "$respaldo" ] || [ ! -f "$respaldo/COMPLETADO" ] || [ ! -f "$respaldo/SHA256SUMS" ]; then
    advertir "SCB CRITICO: no existe un respaldo PostgreSQL completo verificable."
elif [ "$(find "$respaldo" -maxdepth 0 -mtime +1 -print)" ]; then
    advertir "SCB CRITICO: el ultimo respaldo PostgreSQL tiene mas de 24 horas."
fi

exit "$estado"
