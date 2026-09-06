# Fase 4 — Línea base de rendimiento e importaciones

Estado: **En curso**  
Fecha de inicio: 2026-09-04

## Escenarios definidos

1. Previsualización de padrón Excel de hasta 12 MiB.
2. Confirmación de una importación previamente previsualizada.
3. Repetición de confirmación con la misma huella (idempotencia).
4. Consulta paginada de personas y resumen global.
5. Consulta de estado e ingreso concurrente en operación de comedor.
6. Reportes de comedor, transporte y ventas por rango de fechas.

## Instrumentación requerida

Para cada escenario se registrarán tamaño de entrada, filas, número de consultas,
latencia promedio/P95/P99, CPU, memoria máxima, respuestas 2xx/4xx/5xx y
reinicios. Los datos deberán ser anonimizados y generados en el entorno de pruebas.

## Observaciones de línea base estática

- `api_importaciones.py` limita multipart a 12 MiB y lee como máximo un byte por
  encima del límite para detectar exceso (`read(MAXIMO_IMPORTACION_BYTES + 1)`).
- La previsualización mantiene huella e idempotencia; la confirmación vuelve a
  validar la huella antes de escribir.
- Los endpoints async usan repositorios SQLAlchemy síncronos; su impacto debe
  medirse antes de decidir threadpool, endpoint síncrono o migración async.

## Próximo corte

Crear un script de medición reproducible contra ASGI/PostgreSQL de pruebas y
capturar la primera tabla de latencias y memoria. No se declara ningún presupuesto
cumplido hasta disponer de esa ejecución.

## Muestra local no autenticada

Se ejecutaron 30 solicitudes `GET /api/v1/salud` contra `http://127.0.0.1:8081`.
Todas respondieron `200` (0 errores). Latencia: mínimo **3.98 ms**, promedio
**5.31 ms**, P95 **7.61 ms**, máximo **12.57 ms**. Esta muestra valida salud y
proxy local, pero no sustituye las mediciones autenticadas de personas,
importaciones, reportes y comedor requeridas para el cierre.

## Identidad sintética local para medición

Se creó en la base PostgreSQL local una identidad temporal, sin datos reales:

- Usuario: `metrica_lectura_20260904`
- Persona: `Profesor Temporal Metricas` (cédula sintética `METRICA-20260904`)
- Cuenta: `3`; persona: `2192`
- Rol: `operador`
- Permisos: `comedor.operar`, `dashboard.leer`, `reportes.leer`

Las credenciales temporales se entregaron fuera del repositorio y no se
persisten en esta evidencia. La cuenta queda disponible para ejecutar las
mediciones autenticadas de Fase 4 en desarrollo; no fue creada ni replicada en
producción.

## Verificación local adicional — 2026-09-04

- Backend PostgreSQL: `82 passed in 49.51s`.
- Frontend `typecheck`: correcto.
- Frontend `lint`: correcto después de corregir el `role="tablist"` sobre un
  elemento no interactivo en `Plantillas.tsx`.
- Servicio local: `GET /api/v1/salud` respondió `200` en `22.39 ms`; los tres
  contenedores (`web`, `api`, `postgres`) permanecen `healthy`.
- La ejecución completa de Vitest en el volumen WSL2 no produjo proceso activo
  ni resultado antes de ser interrumpida; por tanto no se marca como evidencia
  nueva de aprobación. La evidencia previa de Fase 3 conserva `39 archivos / 105
  pruebas` aprobadas.
- `npm audit` no se completó en esta ejecución por espera de red; la última
  auditoría offline registrada tras actualizar Playwright y happy-dom fue sin
  vulnerabilidades.

## Corrección de API desactualizada — 2026-09-04

El frontend recibía `404` al cancelar una reserva porque el contenedor API era
una imagen anterior que no incluía `DELETE /api/v1/comedor/reservas`. Se
reconstruyó y recreó únicamente `api` con la fuente actual. La ruta ahora llega
al middleware CSRF y responde `403 CSRF inválido` sin sesión, en lugar de `404`;
los contenedores API, web y PostgreSQL permanecen saludables. Las pruebas de
operación de comedor pasaron: `6 passed in 7.19s`.

Posteriormente se ajustó la semántica de cancelación para que una reserva
inexistente sea una operación idempotente (`204`), equivalente al estado “no
asistiré”. La regresión quedó cubierta por `test_cancelar_reserva_inexistente_es_idempotente`;
la suite de comedor pasó `7 tests`.
