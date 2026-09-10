#!/usr/bin/env bash
set -euo pipefail

CONFIG=/etc/scb/rclone.conf
ORIGEN=/srv/scb-postgresql-v2/respaldos
REMOTO=scb_google_crypt:postgresql

registrar() {
    logger -t scb-respaldo-google -- "$*"
    printf '%s\n' "$*"
}

fallar() {
    registrar "ERROR: $*"
    exit 1
}

[[ -r "$CONFIG" ]] || fallar "No existe la configuración cifrada de Google Drive."
[[ -d "$ORIGEN" ]] || fallar "No existe el directorio local de respaldos."
rclone --config "$CONFIG" listremotes | grep -qx 'scb_google_crypt:' || \
    fallar "No está configurado el remoto cifrado de Google Drive."

ultimo=$(find "$ORIGEN" -mindepth 1 -maxdepth 1 -type d -printf '%T@ %p\n' |
    sort -nr | head -1 | cut -d' ' -f2-)
[[ -n "$ultimo" ]] || fallar "No hay respaldo local para subir."

nombre=$(basename "$ultimo")
for archivo in scb.dump globals.sql base/base.tar.gz base/pg_wal.tar.gz; do
    [[ -f "$ultimo/$archivo" ]] || fallar "Falta $archivo en $nombre."
done

rclone --config "$CONFIG" copy "$ultimo" "$REMOTO/diarios/$nombre" \
    --checksum --transfers 2 --checkers 4
rclone --config "$CONFIG" check "$ultimo" "$REMOTO/diarios/$nombre" --one-way

if [[ $(date +%d) == 01 ]]; then
    rclone --config "$CONFIG" copy "$ultimo" "$REMOTO/mensuales/$nombre" \
        --checksum --transfers 2 --checkers 4
    rclone --config "$CONFIG" check "$ultimo" "$REMOTO/mensuales/$nombre" --one-way
fi

rclone --config "$CONFIG" delete "$REMOTO/diarios" --min-age 31d
rclone --config "$CONFIG" rmdirs "$REMOTO/diarios" --leave-root
if rclone --config "$CONFIG" lsd "$REMOTO/mensuales" >/dev/null 2>&1; then
    rclone --config "$CONFIG" delete "$REMOTO/mensuales" --min-age 397d
    rclone --config "$CONFIG" rmdirs "$REMOTO/mensuales" --leave-root
fi
registrar "Copia cifrada verificada en Google Drive: $nombre"
