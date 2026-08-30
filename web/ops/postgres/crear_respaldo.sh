#!/bin/sh
set -eu
umask 077

marca="$(date -u +%Y%m%dT%H%M%SZ)"
destino="/respaldos/$marca"
temporal="$destino.incompleto"
export PGPASSFILE=/tmp/.pgpass
printf '%s:%s:*:%s:%s\n' "$PGHOST" "$PGPORT" "$PGUSER" \
    "$(tr -d '\r\n' < /run/secrets/postgres_admin_password)" > "$PGPASSFILE"
chmod 0600 "$PGPASSFILE"
mkdir -p "$temporal"
pg_dump --format=custom --compress=9 --file="$temporal/scb.dump" "$PGDATABASE"
pg_dumpall --globals-only --file="$temporal/globals.sql"
pg_basebackup --format=tar --gzip --wal-method=stream --checkpoint=fast --pgdata="$temporal/base"
(cd "$temporal" && sha256sum scb.dump globals.sql base/base.tar.gz base/pg_wal.tar.gz > SHA256SUMS)
printf '%s\n' "$marca" > "$temporal/COMPLETADO"
mv "$temporal" "$destino"
find /respaldos -mindepth 1 -maxdepth 1 -type d -mtime "+${RETENCION_RESPALDOS_DIAS}" \
    -name '20??????T??????Z' -exec rm -rf -- {} +
printf 'Respaldo lógico y físico completado: %s\n' "$destino"
