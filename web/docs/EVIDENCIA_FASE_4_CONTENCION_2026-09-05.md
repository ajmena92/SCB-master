# Fase 4: contención, altas y carga sostenida

## Alcance ejecutado

Los cuatro pasos solicitados se ejecutaron en PostgreSQL sintético local.
No se desplegaron cambios ni se modificó la base operativa. Se utilizaron las
skills breves de ejecución del plan y comprobación final ya seleccionadas.

## Fallos reproducidos y corrección

Antes del cambio fallaron las cuatro pruebas iniciales:

- Confirmar simultáneamente: violación de unicidad de reserva sin manejo funcional.
- Cancelar simultáneamente: cuatro liberaciones para una sola reserva.
- Ingresar simultáneamente: violación de unicidad del ingreso.
- Cancelar e ingresar: un ingreso confirmado con saldo restituido indebidamente.

`RepositorioOperacion.bloquear_operacion_persona` usa `SELECT FOR UPDATE` sobre
la persona antes de leer la reserva y modificar el saldo. La fila existe incluso
cuando aún no hay reserva o cuenta. El bloqueo se libera por commit/rollback;
las personas distintas pueden operar en paralelo. La obtención/creación de cuenta
usa el mismo bloqueo para coordinar ventas y movimientos de saldo.

Una reserva consumida o con ingreso previo no se reactiva. Todo ingreso con
reserva la marca consumida, también cuando no inmoviliza tiquetes.

Las pruebas reales cubren confirmaciones, cancelaciones e ingresos repetidos y
los cruces confirmar/cancelar, confirmar/ingresar y cancelar/ingresar. Verifican
saldo, reservados, cantidad de movimientos e ingresos con sesiones independientes.
Los cruces actuales utilizan profesores sintéticos; su ingreso directo permite
ambos órdenes válidos de ejecución. Queda ampliar esa matriz con estudiantes
becados y no becados por HTTP autenticado.

Reproducción, con el contenedor sintético iniciado y su puerto actual:

```bash
cd web/backend
SCB_PUERTO_MEDICION=PUERTO_LOCAL ./.venv/bin/pytest -q pruebas_postgresql/test_contencion_real.py
./.venv/bin/python medir_carga_fase4.py --puerto PUERTO_LOCAL
```

La base debe llamarse `scb_medicion` y existir en localhost con el esquema y
operador sintéticos del medidor anterior. Sin la variable de puerto, las seis
pruebas PostgreSQL se omiten explícitamente, no se sustituyen por SQLite.

## Altas nuevas con Argon2 real

Diez lotes de 25 personas nuevas: mínimo 2.600,93 ms, máximo 3.450,45 ms.
P95/P99 empíricos: 3.450,45 ms; con diez muestras ambos corresponden al máximo.
CPU acumulada del proceso: 53,628 segundos, incluyendo preparación de cada lote.
Se ejecutaron hashes, inserciones y validación de las 25 credenciales emitidas;
cada transacción se revirtió para conservar la base sintética. No se mide aquí
el coste de commit durable ni una importación masiva de mil altas. No se imprimen
credenciales ni hashes. La carga comparte host con verificaciones backend.

## Carga sostenida

HTTPX ASGI, cookies y CSRF de la aplicación, PostgreSQL real, cuatro trabajadores,
60,04 segundos. Total: **2.467 respuestas 200**, cero errores HTTP; unas
41 solicitudes por segundo. No incluye proxy, TLS ni red cliente-servidor.

| Ruta | Muestras | P95 ms | P99 ms |
| --- | ---: | ---: | ---: |
| Personas, página de 25 | 618 | 152,09 | 195,81 |
| Resumen personas | 617 | 124,43 | 143,19 |
| Reporte comedor | 616 | 145,58 | 185,08 |
| Estado comedor | 616 | 142,14 | 177,08 |

CPU del proceso durante la carga: 62,902 segundos. RSS máximo acumulado:
156,79 MiB; incluye cliente ASGI y aplicación. Una muestra Docker de PostgreSQL
dio 27,45 MiB y 0,01 % CPU; no es un pico ni un muestreo continuo de recursos.

## Descomposición de altas y línea base productiva

El medidor local se amplió para aislar los tiempos de previsualización, SQL,
Argon2 y commit. En cinco lotes sintéticos de 25 altas, con commit real y
limpieza posterior de las filas `ALTA-`:

| Fase | P95 ms | Observación |
| --- | ---: | --- |
| Previsualización | 124,35 | La primera muestra incluyó calentamiento; las otras cuatro fueron 4,53–6,74 ms. |
| Confirmación antes de commit | 2.471,49 | Incluye hash, consultas y escritura transaccional. |
| Argon2 dentro de la confirmación | 2.344,47 | Factor dominante, aproximadamente 95 % de la confirmación P95. |
| SQL de confirmación | 101,43 | 82–84 consultas; no es el primer cuello de botella. |
| Commit durable | 6,53 | Los `flush` del servicio ya emiten la mayor parte de los SQL antes del commit. |

La producción se midió únicamente mediante comprobaciones de bajo impacto:
30 solicitudes internas de `GET /health`, todas 200. P50 aproximado 2,7 ms,
P95 4,8 ms y P99 13,6 ms en Nginx local, sin red pública. En reposo la API
usó 168,6 MiB de su límite de 256 MiB; web 5,2 MiB/64 MiB y PostgreSQL 22,1 MiB.
Esto no mide login, importación ni comedor real: hacerlo requeriría actores y
datos de prueba aprobados, y no se ejecutó sobre personas institucionales.

La evidencia ya justifica tratar las importaciones de altas grandes como trabajo
en segundo plano, con progreso, resultado idempotente y concurrencia limitada.
No justifica reducir los parámetros de Argon2. Antes de implementar el cambio,
debe aprobarse el umbral de derivación y una cuenta o entorno de prueba para
medir el efecto concurrente sobre login y operación de comedor.

## Verificación y decisión

- Suite completa con PostgreSQL concurrente habilitado: **93 passed in 54.25s**.
- Mypy: 65 archivos sin errores.
- Ruff: comprobación final aprobada en los dos archivos de operación, el nuevo
  medidor y las pruebas de contención; `git diff --check` sin errores.
- No se ejecutó npm.
- Al finalizar, el contenedor sintético de medición ya estaba detenido.

La corrección funcional de este corte está verificada. El presupuesto propuesto
de dos segundos para confirmación no cubre altas con Argon2 y debe distinguir
altas de actualizaciones. Pendientes para cerrar toda Fase 4: aprobar volumen y
presupuestos; completar medición HTTP desplegada y recursos durante carga;
ampliar contención a estudiantes; medir volumen máximo de filas/descompresión.
