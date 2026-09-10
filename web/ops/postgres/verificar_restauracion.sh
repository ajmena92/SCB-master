#!/bin/sh
set -eu
export PGPASSFILE=/tmp/.pgpass
printf '%s:%s:*:%s:%s\n' "$PGHOST" "$PGPORT" "$PGUSER" \
    "$(tr -d '\r\n' < /run/secrets/postgres_admin_password)" > "$PGPASSFILE"
chmod 0600 "$PGPASSFILE"
respaldo="${1:-$(find /respaldos -mindepth 1 -maxdepth 1 -type d -name '20??????T??????Z' | sort | tail -1)}"
[ -n "$respaldo" ] && [ -f "$respaldo/scb.dump" ] && [ -f "$respaldo/SHA256SUMS" ] || {
    echo "Uso: verificar_restauracion.sh /respaldos/AAAAmmddTHHMMSSZ" >&2; exit 64;
}
(cd "$respaldo" && sha256sum --check SHA256SUMS)
base="scb_verificacion_$(date -u +%Y%m%d%H%M%S)"
trap 'dropdb --if-exists "$base" >/dev/null 2>&1 || true' EXIT INT TERM
createdb "$base"
pg_restore --exit-on-error --no-owner --no-privileges --dbname="$base" "$respaldo/scb.dump"
psql --set=ON_ERROR_STOP=1 --dbname="$base" --tuples-only --command \
    "SELECT 'tablas=' || count(*) FROM information_schema.tables WHERE table_schema='public';"
dropdb "$base"
trap - EXIT INT TERM
echo "Restauración lógica verificada correctamente."
