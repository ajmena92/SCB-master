#!/bin/sh
set -eu
password="$(tr -d '\r\n' < /run/secrets/postgres_app_password)"
password_url="$(printf '%s' "$password" | python -c 'import sys; from urllib.parse import quote; print(quote(sys.stdin.read(), safe=""))')"
export DATABASE_URL="postgresql+psycopg://${POSTGRES_USER}:${password_url}@${POSTGRES_HOST}:${POSTGRES_PORT:-5432}/${POSTGRES_DB}"
unset password password_url
for secreto in carnet_qr_clave csrf_secret importacion_resultados_key; do
    install -m 0444 "/run/secrets/$secreto" "/tmp/$secreto"
done
export CARNET_QR_CLAVE_FILE=/tmp/carnet_qr_clave
export CSRF_SECRET_FILE=/tmp/csrf_secret
export IMPORTACION_RESULTADOS_KEY_FILE=/tmp/importacion_resultados_key
exec setpriv --reuid=10001 --regid=10001 --init-groups python -m aplicacion.worker_importacion
