#!/usr/bin/env bash
# Ejecuta una prueba operativa de staging y conserva su evidencia TSV.
# La URL debe apuntar al ingreso web de staging. La carga y la medición
# comienzan juntas para relacionar memoria, latencia y respuestas HTTP.
set -euo pipefail

duracion="${DURACION_MEDICION_SEGUNDOS:-60}"
intervalo="${INTERVALO_MEDICION_SEGUNDOS:-1}"
usuarios="${USUARIOS_PRUEBA:-1}"
tiempo_maximo_solicitud="${TIEMPO_MAXIMO_SOLICITUD_SEGUNDOS:-30}"
url_prueba="${URL_PRUEBA:-http://127.0.0.1:8081/api/health}"
umbral_memoria_porcentaje="${UMBRAL_MEMORIA_PORCENTAJE:-70}"
latencia_p95_umbral_ms="${LATENCIA_P95_UMBRAL_MS:-}"
latencia_p95_referencia_ms="${LATENCIA_P95_REFERENCIA_MS:-}"
aumento_latencia_maximo_porcentaje="${AUMENTO_LATENCIA_MAXIMO_PORCENTAJE:-20}"
archivo_compose="${COMPOSE_FILE:-compose.production.yml}"
archivo_entorno="${COMPOSE_ENV_FILE:-.env}"
exigir_umbral="${EXIGIR_UMBRAL_MEMORIA:-true}"
fecha_nombre="$(date -u +%Y%m%dT%H%M%SZ)"
salida="${SALIDA_MEDICION_MEMORIA:-/tmp/scsc-memoria-staging-${fecha_nombre}.tsv}"
salida_muestras="${SALIDA_MUESTRAS_MEMORIA:-${salida%.tsv}-muestras.tsv}"

if [[ ! "$duracion" =~ ^[1-9][0-9]*$ || ! "$intervalo" =~ ^[1-9][0-9]*$ ]]; then
    echo "DURACION_MEDICION_SEGUNDOS e INTERVALO_MEDICION_SEGUNDOS deben ser enteros positivos." >&2
    exit 2
fi
if [[ ! "$usuarios" =~ ^[1-9][0-9]*$ || "$usuarios" -gt 1000 ]]; then
    echo "USUARIOS_PRUEBA debe ser un entero entre 1 y 1000." >&2
    exit 2
fi
if [[ ! "$tiempo_maximo_solicitud" =~ ^[1-9][0-9]*$ ]]; then
    echo "TIEMPO_MAXIMO_SOLICITUD_SEGUNDOS debe ser un entero positivo." >&2
    exit 2
fi
if [[ ! "$umbral_memoria_porcentaje" =~ ^[0-9]+([.][0-9]+)?$ ]]; then
    echo "UMBRAL_MEMORIA_PORCENTAJE debe ser un número no negativo." >&2
    exit 2
fi
if [[ -n "$latencia_p95_umbral_ms" && ! "$latencia_p95_umbral_ms" =~ ^[0-9]+([.][0-9]+)?$ ]]; then
    echo "LATENCIA_P95_UMBRAL_MS debe ser un número no negativo." >&2
    exit 2
fi
if [[ -n "$latencia_p95_referencia_ms" && ! "$latencia_p95_referencia_ms" =~ ^[0-9]+([.][0-9]+)?$ ]]; then
    echo "LATENCIA_P95_REFERENCIA_MS debe ser un número no negativo." >&2
    exit 2
fi
if [[ ! "$aumento_latencia_maximo_porcentaje" =~ ^[0-9]+([.][0-9]+)?$ ]]; then
    echo "AUMENTO_LATENCIA_MAXIMO_PORCENTAJE debe ser un número no negativo." >&2
    exit 2
fi
if [[ "$exigir_umbral" != "true" && "$exigir_umbral" != "false" ]]; then
    echo "EXIGIR_UMBRAL_MEMORIA debe ser true o false." >&2
    exit 2
fi
if [[ "$exigir_umbral" == "true" && -z "$latencia_p95_umbral_ms" && -z "$latencia_p95_referencia_ms" ]]; then
    echo "La puerta exige LATENCIA_P95_UMBRAL_MS o LATENCIA_P95_REFERENCIA_MS." >&2
    exit 2
fi

directorio_script="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
directorio_ops="$(cd "$directorio_script/../ops" && pwd)"
cd "$directorio_ops"
mkdir -p "$(dirname "$salida")" "$(dirname "$salida_muestras")"
source "$directorio_script/medir_memoria_docker.sh"

api="$(obtener_contenedor api)"
web="$(obtener_contenedor web)"
verificar_contenedor_activo api "$api"
verificar_contenedor_activo web "$web"
trabajadores="$(obtener_trabajadores "$api")"
MUESTRA_MEMORIA_FALLIDA=0

estado_inicial_api="$(obtener_estado_operativo "$api")"
estado_inicial_web="$(obtener_estado_operativo "$web")"
reinicios_iniciales_api="${estado_inicial_api%%$'\t'*}"
reinicios_iniciales_web="${estado_inicial_web%%$'\t'*}"

directorio_temporal="$(mktemp -d)"
limpiar_temporal() {
    rm -rf "$directorio_temporal"
}
trap limpiar_temporal EXIT

generar_carga() {
    local usuario="$1"
    local fin_epoch="$2"
    local archivo_resultados="$directorio_temporal/usuario-${usuario}.tsv"
    local resultado
    : > "$archivo_resultados"
    while (( $(date +%s) < fin_epoch )); do
        if ! resultado="$(curl --silent --show-error --output /dev/null \
            --connect-timeout 5 --max-time "$tiempo_maximo_solicitud" \
            --write-out '%{http_code}\t%{time_total}' "$url_prueba" 2>/dev/null)"; then
            resultado=$'000\t0'
        fi
        printf '%s\n' "$resultado" >> "$archivo_resultados"
    done
}

fecha_inicio_utc="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
inicio_epoch="$(date +%s)"
fin_epoch=$((inicio_epoch + duracion))
procesos_carga=()
for ((usuario = 1; usuario <= usuarios; usuario++)); do
    generar_carga "$usuario" "$fin_epoch" &
    procesos_carga+=("$!")
done

muestras=$(( (duracion + intervalo - 1) / intervalo + 1 ))
maximo_api_mib="0.00"
maximo_web_mib="0.00"
maximo_api_porcentaje="0.00"
maximo_web_porcentaje="0.00"
printf 'fecha_utc\tapi_mib\tapi_porcentaje\tweb_mib\tweb_porcentaje\treinicios_api\treinicios_web\toom_killed_api\toom_killed_web\n' > "$salida_muestras"

for ((indice = 0; indice < muestras; indice++)); do
    if ! memoria_api="$(obtener_memoria "$api")"; then
        MUESTRA_MEMORIA_FALLIDA=1
        memoria_api=$'0.00\t0.00'
    fi
    if ! memoria_web="$(obtener_memoria "$web")"; then
        MUESTRA_MEMORIA_FALLIDA=1
        memoria_web=$'0.00\t0.00'
    fi
    estado_api="$(obtener_estado_operativo "$api")"
    estado_web="$(obtener_estado_operativo "$web")"
    api_mib="${memoria_api%%$'\t'*}"
    api_porcentaje="${memoria_api#*$'\t'}"
    web_mib="${memoria_web%%$'\t'*}"
    web_porcentaje="${memoria_web#*$'\t'}"
    reinicios_api="${estado_api%%$'\t'*}"
    reinicios_web="${estado_web%%$'\t'*}"
    oom_api="${estado_api#*$'\t'}"
    oom_web="${estado_web#*$'\t'}"
    printf '%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n' \
        "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$api_mib" "$api_porcentaje" "$web_mib" \
        "$web_porcentaje" "$reinicios_api" "$reinicios_web" "$oom_api" "$oom_web" >> "$salida_muestras"
    maximo_api_mib="$(awk -v actual="$api_mib" -v maximo="$maximo_api_mib" 'BEGIN { print (actual > maximo) ? actual : maximo }')"
    maximo_web_mib="$(awk -v actual="$web_mib" -v maximo="$maximo_web_mib" 'BEGIN { print (actual > maximo) ? actual : maximo }')"
    maximo_api_porcentaje="$(awk -v actual="$api_porcentaje" -v maximo="$maximo_api_porcentaje" 'BEGIN { print (actual > maximo) ? actual : maximo }')"
    maximo_web_porcentaje="$(awk -v actual="$web_porcentaje" -v maximo="$maximo_web_porcentaje" 'BEGIN { print (actual > maximo) ? actual : maximo }')"
    if (( indice + 1 < muestras )); then
        sleep "$intervalo"
    fi
done

for proceso in "${procesos_carga[@]}"; do
    wait "$proceso" || true
done

resultados="$directorio_temporal/resultados.tsv"
: > "$resultados"
for archivo_usuario in "$directorio_temporal"/usuario-*.tsv; do
    [[ -e "$archivo_usuario" ]] || continue
    while IFS= read -r linea; do
        printf '%s\n' "$linea" >> "$resultados"
    done < "$archivo_usuario"
done

total_solicitudes="$(awk -F'\t' 'NF >= 2 { total++ } END { print total + 0 }' "$resultados")"
respuestas_413="$(awk -F'\t' '$1 == 413 { total++ } END { print total + 0 }' "$resultados")"
errores_5xx="$(awk -F'\t' '$1 >= 500 && $1 <= 599 { total++ } END { print total + 0 }' "$resultados")"
errores_red="$(awk -F'\t' '$1 == 0 { total++ } END { print total + 0 }' "$resultados")"
errores_http="$(awk -F'\t' '$1 >= 400 && $1 <= 499 { total++ } END { print total + 0 }' "$resultados")"
errores_total=$((errores_http + errores_5xx + errores_red))
archivo_latencias="$directorio_temporal/latencias.tsv"
awk -F'\t' '$1 >= 100 && $1 <= 599 && $2 ~ /^[0-9]+([.][0-9]+)?$/ { print $2 * 1000 }' "$resultados" | sort -n > "$archivo_latencias"
latencia_promedio_ms="$(awk '{ suma += $1; total++ } END { printf "%.2f", (total ? suma / total : 0) }' "$archivo_latencias")"
cantidad_latencias="$(wc -l < "$archivo_latencias")"
if (( cantidad_latencias == 0 )); then
    latencia_p95_ms="0.00"
else
    posicion_p95="$(awk -v total="$cantidad_latencias" 'BEGIN { posicion = int((total * 95 + 99) / 100); print (posicion > total) ? total : posicion }')"
    latencia_p95_ms="$(sed -n "${posicion_p95}p" "$archivo_latencias")"
fi

estado_final_api="$(obtener_estado_operativo "$api")"
estado_final_web="$(obtener_estado_operativo "$web")"
reinicios_finales_api="${estado_final_api%%$'\t'*}"
reinicios_finales_web="${estado_final_web%%$'\t'*}"
oom_killed_api="${estado_final_api#*$'\t'}"
oom_killed_web="${estado_final_web#*$'\t'}"
reinicios_api=$((reinicios_finales_api - reinicios_iniciales_api))
reinicios_web=$((reinicios_finales_web - reinicios_iniciales_web))
fecha_fin_utc="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
aumento_latencia_porcentaje="0.00"
if [[ -n "$latencia_p95_referencia_ms" ]]; then
    aumento_latencia_porcentaje="$(awk -v actual="$latencia_p95_ms" -v referencia="$latencia_p95_referencia_ms" 'BEGIN { if (referencia == 0) print "0.00"; else printf "%.2f", ((actual - referencia) / referencia) * 100 }')"
fi

printf 'fecha_inicio_utc\tfecha_fin_utc\tusuarios_prueba\ttrabajadores_uvicorn\tduracion_segundos\turl_prueba\tpico_api_mib\tpico_api_porcentaje\tpico_web_mib\tpico_web_porcentaje\tlatencia_promedio_ms\tlatencia_p95_ms\tlatencia_p95_umbral_ms\tlatencia_p95_referencia_ms\taumento_latencia_porcentaje\ttotal_solicitudes\trespuestas_413\terrores_5xx\terrores_http_4xx\terrores_red\terrores_total\treinicios_api\treinicios_web\toom_killed_api\toom_killed_web\n' > "$salida"
printf '%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n' \
    "$fecha_inicio_utc" "$fecha_fin_utc" "$usuarios" "$trabajadores" "$duracion" "$url_prueba" \
    "$maximo_api_mib" "$maximo_api_porcentaje" "$maximo_web_mib" "$maximo_web_porcentaje" \
    "$latencia_promedio_ms" "$latencia_p95_ms" "$latencia_p95_umbral_ms" "$latencia_p95_referencia_ms" \
    "$aumento_latencia_porcentaje" "$total_solicitudes" "$respuestas_413" "$errores_5xx" \
    "$errores_http" "$errores_red" "$errores_total" "$reinicios_api" "$reinicios_web" \
    "$oom_killed_api" "$oom_killed_web" >> "$salida"

echo "Pico API: ${maximo_api_mib} MiB (${maximo_api_porcentaje}%)"
echo "Pico web: ${maximo_web_mib} MiB (${maximo_web_porcentaje}%)"
echo "Latencia promedio/P95: ${latencia_promedio_ms}/${latencia_p95_ms} ms"
echo "Aumento P95 frente a referencia: ${aumento_latencia_porcentaje}%"
echo "Solicitudes: ${total_solicitudes}; 413: ${respuestas_413}; 5xx: ${errores_5xx}; errores totales: ${errores_total}"
echo "Reinicios API/web: ${reinicios_api}/${reinicios_web}; OOMKilled API/web: ${oom_killed_api}/${oom_killed_web}"
echo "TSV resumen: $salida"
echo "TSV muestras: $salida_muestras"

fallos=0
if [[ "$exigir_umbral" == "true" ]]; then
    if awk -v api="$maximo_api_porcentaje" -v web="$maximo_web_porcentaje" -v limite="$umbral_memoria_porcentaje" 'BEGIN { exit !(api > limite || web > limite) }'; then
        echo "FALLO: el uso de memoria superó ${umbral_memoria_porcentaje}% en API o web." >&2
        fallos=1
    fi
    if [[ -n "$latencia_p95_umbral_ms" ]] && awk -v actual="$latencia_p95_ms" -v limite="$latencia_p95_umbral_ms" 'BEGIN { exit !(actual > limite) }'; then
        echo "FALLO: la latencia P95 superó ${latencia_p95_umbral_ms} ms." >&2
        fallos=1
    fi
    if [[ -n "$latencia_p95_referencia_ms" ]] && awk -v actual="$aumento_latencia_porcentaje" -v limite="$aumento_latencia_maximo_porcentaje" 'BEGIN { exit !(actual > limite) }'; then
        echo "FALLO: la latencia P95 aumentó más de ${aumento_latencia_maximo_porcentaje}%." >&2
        fallos=1
    fi
fi
if (( reinicios_api > 0 || reinicios_web > 0 )); then
    echo "FALLO: hubo reinicios de contenedor durante la prueba." >&2
    fallos=1
fi
if [[ "$oom_killed_api" == "true" || "$oom_killed_web" == "true" ]]; then
    echo "FALLO: un contenedor terminó con estado OOMKilled." >&2
    fallos=1
fi
if (( errores_5xx > 0 || errores_red > 0 )); then
    echo "FALLO: se detectaron errores 5xx o de red." >&2
    fallos=1
fi
if (( MUESTRA_MEMORIA_FALLIDA > 0 )); then
    echo "FALLO: no fue posible medir memoria en al menos una muestra." >&2
    fallos=1
fi
exit "$fallos"
