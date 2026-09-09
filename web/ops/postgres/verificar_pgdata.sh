#!/bin/sh
set -eu
: "${PGDATA:?Falta PGDATA}"
: "${POSTGRES_SYSTEM_IDENTIFIER:?Falta POSTGRES_SYSTEM_IDENTIFIER}"
test -s "$PGDATA/PG_VERSION" && test -s "$PGDATA/global/pg_control" || {
    echo "Arranque bloqueado: no existe un cluster PostgreSQL en PGDATA." >&2
    exit 1
}
actual=$(LC_ALL=C pg_controldata "$PGDATA" | sed -n 's/^Database system identifier: *//p')
test "$actual" = "$POSTGRES_SYSTEM_IDENTIFIER" || {
    echo "Arranque bloqueado: la identidad del cluster no coincide." >&2
    exit 1
}
exec /usr/local/bin/docker-entrypoint.sh "$@"
