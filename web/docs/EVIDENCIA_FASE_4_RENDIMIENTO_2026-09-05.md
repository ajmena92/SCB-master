# Fase 4 — Medición y optimizaciones locales

Estado: **en curso; corte de optimización medido**. Fecha: 2026-09-05.

## Entorno y método

Python 3.13, PostgreSQL `17.6-bookworm` en contenedores exclusivos de medición,
SQLAlchemy y HTTPX ASGI. Mil personas sintéticas y, en la muestra final, mil
registros por reporte de comedor, transporte y ventas. Concurrencia HTTP: cuatro;
30 solicitudes por endpoint. No se utilizaron datos ni credenciales de producción.
El medidor genera secretos efímeros y solo imprime métricas agregadas.

Reproducción: crear un PostgreSQL local vacío con base `scb_medicion` y ejecutar
desde `web/backend`:

```bash
./.venv/bin/python medir_fase4.py --puerto PUERTO_LOCAL --filas 1000 --muestras 30
```

El programa rechaza bases no locales, con otro nombre o con tablas existentes.
La preparación crea tablas desde metadatos: no verifica migraciones Alembic.
`datos_medicion_fase4.py` prepara las relaciones y un XLSX válido de 12 MiB:
incluye una columna adicional y una entrada ZIP auxiliar para llegar al límite.
Ese archivo prueba tamaño y parsing, no el máximo de filas ni expansión ZIP.

## Resultados

| Escenario | N | P95 ms | P99 ms | SQL por solicitud |
| --- | ---: | ---: | ---: | ---: |
| Previsualización, mil filas | 30 | 112,30 | 143,45 | 3 |
| Repetición idempotente | 30 | 39,64 | 114,86 | 4 |
| Personas, página de 25 | 30 | 180,03 | 183,46 | 9 |
| Resumen de personas | 30 | 156,75 | 161,44 | 6 |
| Reporte comedor, mil registros | 30 | 167,09 | 167,78 | 4 |
| Reporte transporte, mil registros | 30 | 137,41 | 142,11 | 4 |
| Reporte ventas, mil registros | 30 | 252,33 | 274,63 | 4 |
| Estado comedor | 30 | 180,07 | 184,60 | 8 |
| Ingresos concurrentes, personas distintas | 30 | 170,47 | 172,44 | 16 |
| XLSX de exactamente 12 MiB por ASGI | 3 | 309,30 | 309,30 | 6 |

Confirmación de mil personas existentes con matrículas nuevas: **234,52 ms y
13 sentencias SQL**, una muestra; no representa un percentil estadístico.
Lectura del XLSX normal de 24.311 bytes: máximo 80,52 ms en tres muestras.
El máximo RSS acumulado del proceso, que incluye generador, cliente y aplicación,
fue **199,91 MiB**. CPU acumulada de las 30 solicitudes de personas: 0,865 s;
reportes comedor/transporte/ventas: 1,063/0,833/1,347 s; ingresos: 1,034 s.
Estos valores son tiempo de CPU del proceso, no porcentaje de CPU del servidor.

Resultados HTTP: 180 lecturas `200`, 30 ingresos `201`, tres archivos de 12 MiB
`200`, archivo de 12 MiB + 1 byte `413`. Login y CSRF reales de la aplicación,
sin reemplazar dependencias de autorización. Las respuestas no se registran.

## Comparación y cambios

La copia temporal de comparación restauró el patrón de consultas original de
importación, manteniendo el resto de infraestructura del medidor. No es un
checkout histórico completo. Las ejecuciones comparten host y pueden tener ruido.

| Operación | Antes | Después |
| --- | ---: | ---: |
| SQL por previsualización de mil filas | 1.001 | 3 |
| P95 previsualización | 1.928,90 ms | 112,30 ms |
| SQL confirmación (una muestra) | 4.006 | 13 |
| Tiempo confirmación (una muestra) | 9.618,12 ms | 234,52 ms |
| SQL repetición idempotente | 1.002 | 4 |

La primera muestra de personas ejecutaba 31 consultas sin año vigente; la final
ejecuta 9 con año vigente y matrículas. La carga es más completa, por lo que no
se presenta como comparación estricta de latencia.

- Personas y matrículas de importación se consultan en bloques de 500.
- La previsualización cuenta ausentes sin materializar sus entidades.
- La confirmación agrupa el guardado de matrículas.
- El lector XLSX itera filas y cierra el libro, eliminando la lista intermedia.
- Las relaciones de personas se cargan por página; resumen individual y listado
  reutilizan el mismo constructor.
- Maestros, operación, reportes y confirmación usan endpoints síncronos para el
  threadpool de FastAPI. La lectura multipart sigue async y delega el trabajo
  bloqueante. Autenticación por sesión y cierre transaccional también son síncronos.
  No se comparte una Session entre solicitudes ni se convierte el driver a async.
- El límite del archivo se verifica independientemente del overhead multipart.
- Se corrigió una prueba de portal que fallaba los fines de semana al intentar
  crear una plantilla fuera de lunes a viernes.

## Alcance del cierre pendiente

La suite completa pasó **87 pruebas en 47,38 s** antes de la consolidación final
del resumen individual. Las guardas nuevas cubren consultas por bloques,
equivalencia de huella XLSX y consulta única de saldos por página.

Verificación posterior: **12 pruebas focales en 15,04 s**, mypy aprobado en
65 archivos y Ruff aprobado. No se ejecutaron comprobaciones npm.
Se usaron `executing-plans` y `verification-before-completion` para seguimiento
del plan y comprobación de los criterios de salida.

Faltan para el cierre completo original:

1. Aprobar presupuestos y volumen representativo. Propuesta para este escenario
   local: P95 HTTP ≤500 ms, previsualización ≤500 ms, confirmación ≤2 s y RSS
   combinado ≤256 MiB. Son umbrales propuestos después de medir, no SLA aprobado.
2. Medir altas nuevas con coste real de Argon2 y confirmaciones repetidas en bases
   vacías independientes para obtener percentiles válidos.
3. Medir contención sobre una misma persona/reserva y carga sostenida por HTTP
   desplegado, con CPU/memoria muestreadas durante la ejecución y reinicios.
4. Validar el máximo de filas/descompresión XLSX, reportes de mayor volumen y
   comparación antes/después bajo la misma carga sin procesos competidores.

No se ejecutó deploy de este corte. La medición ASGI no incluye Nginx, red ni TLS.
No se declara completada la fase ni se extrapolan los resultados a producción.

## Corte de verificación — 2026-09-06

- La prueba focal `backend/pruebas_postgresql/test_importacion_rendimiento.py`
  pasó **3/3** en 8,55 s.
- El despliegue productivo completo validó API/web saludables y no cambió los
  datos de medición sintética.
- Se acepta como presupuesto técnico provisional para el entorno local:
  P95 HTTP ≤500 ms, previsualización ≤500 ms, confirmación ≤2 s y RSS combinado
  ≤256 MiB. La aprobación operativa definitiva corresponde a ajmena92.
- La fase permanece en curso porque todavía no existe evidencia reproducible de
  coste Argon2 con altas nuevas, contención sobre una misma persona/reserva ni
  carga sostenida a través de Nginx/TLS.
