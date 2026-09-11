#!/usr/bin/env bash
# Migra copias SCB existentes a Mi unidad/SCB-RESPALDOS sin borrar las fuentes.
set -euo pipefail

config="${RCLONE_CONFIG:-/etc/scb/rclone.conf}"
raiz_destino="${SCB_DRIVE_ROOT:-SCB-RESPALDOS}"
origen_recuperacion="${SCB_RECUPERACION_LEGADA:-SCB-RECUPERACION-2026}"
marca="$(date -u +%Y%m%dT%H%M%SZ)"
temporal="$(mktemp /run/scb-rclone-migracion.XXXXXX)"
respaldo_config="${config}.antes-scb-respaldos-${marca}"

limpiar() {
    rm -f "$temporal"
}
trap limpiar EXIT

fallar() {
    printf 'ERROR: %s\n' "$*" >&2
    exit 1
}

[[ $EUID -eq 0 ]] || fallar "Ejecute como root."
[[ -r "$config" ]] || fallar "No existe la configuración rclone protegida."
command -v rclone >/dev/null || fallar "rclone no está instalado."

install -o root -g root -m 600 "$config" "$temporal"
awk '
    BEGIN { activo=0 }
    /^\[scb_google_crypt\]$/ {
        activo=1
        print "[scb_google_crypt_destino]"
        next
    }
    activo && /^\[/ { exit }
    activo {
        if ($0 ~ /^remote[[:space:]]*=/) {
            print "remote = scb_google_drive:" destino
        } else {
            print
        }
    }
' destino="$raiz_destino" "$config" >>"$temporal"

rclone --config "$temporal" listremotes | grep -qx 'scb_google_crypt_destino:' || \
    fallar "No fue posible construir el remoto cifrado temporal."

# La copia y la comprobación son idempotentes. Las fuentes nunca se borran aquí.
rclone --config "$temporal" copy scb_google_crypt:postgresql \
    scb_google_crypt_destino:postgresql --checksum --transfers 2 --checkers 4
rclone --config "$temporal" check scb_google_crypt:postgresql \
    scb_google_crypt_destino:postgresql --one-way
rclone --config "$temporal" copy "scb_google_drive:${origen_recuperacion}" \
    "scb_google_drive:${raiz_destino}/recuperacion/legado" --checksum --transfers 2 --checkers 4
rclone --config "$temporal" check "scb_google_drive:${origen_recuperacion}" \
    "scb_google_drive:${raiz_destino}/recuperacion/legado" --one-way

# Solo después de las comprobaciones se cambia la ruta usada por el timer diario.
install -o root -g root -m 600 "$config" "$respaldo_config"
rclone --config "$config" config update scb_google_crypt \
    "remote=scb_google_drive:${raiz_destino}"
rclone --config "$config" lsf scb_google_crypt:postgresql --dirs-only --max-depth 1 | \
    grep -qx 'diarios/' || fallar "El remoto activo no muestra los respaldos migrados."

printf 'Migración verificada. Respaldo protegido de configuración: %s\n' "$respaldo_config"
