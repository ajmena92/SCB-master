#!/usr/bin/env bash
# Crea un cofre GPG para custodios, sin registrar secretos ni claves privadas.
set -euo pipefail

config="${RCLONE_CONFIG:-/etc/scb/rclone.conf}"
destino="${SCB_COFRE_DESTINO:-scb_google_drive:SCB-RESPALDOS/recuperacion/cofre}"
secretos="${SCB_SECRETOS_DIR:-/home/plat/scsc-comedor/ops/secrets}"
clave_arranque="${SCB_CLAVE_LUKS_ARRANQUE:-/etc/cryptsetup-keys.d/scb-postgresql-v2.key}"
cabecera_luks="${SCB_CABECERA_LUKS:-}"
clave_recuperacion="${SCB_CLAVE_LUKS_RECUPERACION:-}"
publicas=()

uso() {
    printf 'Uso: %s --clave-publica CUSTODIO1.asc --clave-publica CUSTODIO2.asc\n' "$0" >&2
    exit 64
}

while (($#)); do
    case "$1" in
        --clave-publica)
            (($# >= 2)) || uso
            publicas+=("$2")
            shift 2
            ;;
        *) uso ;;
    esac
done

(( ${#publicas[@]} == 2 )) || uso
[[ $EUID -eq 0 ]] || { printf 'ERROR: Ejecute como root.\n' >&2; exit 1; }
[[ -r "$config" && -d "$secretos" && -r "$clave_arranque" ]] || {
    printf 'ERROR: Faltan fuentes protegidas requeridas.\n' >&2; exit 1;
}
command -v gpg >/dev/null && command -v rclone >/dev/null || {
    printf 'ERROR: Se requieren gpg y rclone.\n' >&2; exit 1;
}

trabajo="$(mktemp -d /run/scb-cofre.XXXXXX)"
gnupg="$trabajo/gnupg"
publicar="$trabajo/publicar"
salida="$publicar/scb-recuperacion-$(date -u +%Y%m%dT%H%M%SZ).tar.gz.gpg"
trap 'rm -rf "$trabajo"' EXIT
install -d -m 700 "$gnupg" "$trabajo/contenido/secretos" "$publicar"

for publica in "${publicas[@]}"; do
    [[ -r "$publica" ]] || { printf 'ERROR: No se puede leer una clave pública.\n' >&2; exit 1; }
    gpg --homedir "$gnupg" --batch --import "$publica" >/dev/null
done

install -m 600 "$clave_arranque" "$trabajo/contenido/clave_luks_arranque.key"
install -m 600 "$config" "$trabajo/contenido/rclone.conf"
[[ -n "$cabecera_luks" && -r "$cabecera_luks" ]] || {
    printf 'ERROR: Debe indicar una cabecera LUKS protegida.\n' >&2; exit 1;
}
[[ -n "$clave_recuperacion" && -r "$clave_recuperacion" ]] || {
    printf 'ERROR: Debe indicar una clave LUKS de recuperación protegida.\n' >&2; exit 1;
}
install -m 600 "$cabecera_luks" "$trabajo/contenido/cabecera_luks.bin"
install -m 600 "$clave_recuperacion" "$trabajo/contenido/clave_luks_recuperacion.key"
find "$secretos" -maxdepth 1 -type f -exec install -m 600 {} "$trabajo/contenido/secretos/" \;

mapfile -t destinatarios < <(gpg --homedir "$gnupg" --with-colons --list-keys | awk -F: '$1 == "fpr" { print $10 }')
(( ${#destinatarios[@]} == 2 )) || { printf 'ERROR: Las claves públicas deben contener una identidad cada una.\n' >&2; exit 1; }
tar --create --gzip --file "$trabajo/contenido.tar.gz" -C "$trabajo/contenido" .
gpg_args=()
for destinatario in "${destinatarios[@]}"; do gpg_args+=(--recipient "$destinatario"); done
gpg --homedir "$gnupg" --batch --yes --trust-model always --encrypt "${gpg_args[@]}" \
    --output "$salida" "$trabajo/contenido.tar.gz"
(cd "$trabajo" && sha256sum "$(basename "$salida")" >"$(basename "$salida").sha256")
rclone --config "$config" copy "$publicar" "$destino" --checksum
rclone --config "$config" check "$publicar" "$destino" --one-way
printf 'Cofre cifrado y verificado en %s/%s\n' "$destino" "$(basename "$salida")"
