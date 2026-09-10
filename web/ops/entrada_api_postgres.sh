#!/bin/sh
set -eu
archivo="${POSTGRES_PASSWORD_FILE:-/run/secrets/postgres_app_password}"
[ -r "$archivo" ] || { echo "Falta el secreto PostgreSQL de la API" >&2; exit 78; }
password="$(tr -d '\r\n' < "$archivo")"
password_url="$(printf '%s' "$password" | python -c 'import sys; from urllib.parse import quote; print(quote(sys.stdin.read(), safe=""))')"
export DATABASE_URL="postgresql+psycopg://${POSTGRES_USER}:${password_url}@${POSTGRES_HOST}:${POSTGRES_PORT:-5432}/${POSTGRES_DB}"
unset password password_url

# Los secretos adicionales se montan como root y la aplicación corre como
# appuser. Prepararlos en tmpfs evita exponerlos en variables de entorno y
# permite que el proceso no privilegiado los lea después del cambio de UID.
preparar_secreto() {
    nombre="$1"
    origen="$2"
    destino="/tmp/$3"
    [ -r "$origen" ] || { echo "Falta el secreto $nombre de la API" >&2; exit 78; }
    # El runtime puede impedir cambiar propietarios dentro del tmpfs. El
    # archivo queda de solo lectura y el contenedor no comparte ese tmpfs con
    # otros servicios; la API abandona privilegios antes de iniciar Uvicorn.
    install -m 0444 "$origen" "$destino"
    export "${nombre}_FILE=$destino"
}

preparar_secreto CARNET_QR_CLAVE "${CARNET_QR_CLAVE_FILE:-/run/secrets/carnet_qr_clave}" carnet_qr_clave
preparar_secreto CSRF_SECRET "${CSRF_SECRET_FILE:-/run/secrets/csrf_secret}" csrf_secret
preparar_secreto IMPORTACION_RESULTADOS_KEY "${IMPORTACION_RESULTADOS_KEY_FILE:-/run/secrets/importacion_resultados_key}" importacion_resultados_key

exec setpriv --reuid=10001 --regid=10001 --init-groups \
    uvicorn aplicacion.entrada:crear_aplicacion --factory --host 0.0.0.0 --port 8000 \
    --workers "${UVICORN_WORKERS:-2}" --proxy-headers \
    --forwarded-allow-ips="${FORWARDED_ALLOW_IPS:-127.0.0.1}"
