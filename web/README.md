# Portal web de comedor SCSC

Plataforma de comedor y transporte: frontend React, API FastAPI y PostgreSQL 17. El navegador solo accede al mismo origen HTTPS; la API es la única ruta hacia la base de datos.

## Documentación

- [Requisitos funcionales](docs/REQUISITOS_COMEDOR.md)
- [Análisis y puerta de integración a producción](docs/ANALISIS_INTEGRACION_PRODUCCION.md)
- [Despliegue seguro en staging y producción](docs/DESPLIEGUE_PORTAL.md)
- [Operación del carnet digital](docs/CARNET_DIGITAL_OPERACION.md)

## Estado

La plataforma se reconstruye sobre PostgreSQL con una migración base Alembic. SQL Server solo participa como fuente de lectura en el corte único de personas activas, matrícula 2026, rutas, becas vigentes y menú. No existe doble escritura.

### Avances funcionales registrados

- Parámetros exclusivos del portal para la hora límite por horario y el aviso previo, aplicados dinámicamente y auditados.
- Reloj y tiempo restante del estudiante sincronizados con SQL Server; apertura, cierre y extensiones de horario se reconcilian con el servidor.
- Confirmación con marca de hora del servidor: **Confirmar almuerzo** queda deshabilitado y **No asistiré** permite cancelar la marca web antes del cierre.
- Estado final de solo lectura después del cierre, sin acciones disponibles; se conserva el menú y se informa si el estudiante marcó asistencia.
- El contrato de asistencia usa el campo canónico `estado` en minúscula para evitar ambigüedad entre API y frontend.

## Operación futura

Los artefactos canónicos están en `ops/`. La API y PostgreSQL permanecen privados; 5432 no se publica y HTTPS termina en el proxy institucional. Prepare el entorno con `./scripts/preparar_postgresql.sh` y consulte el [runbook PostgreSQL](docs/POSTGRESQL_OPERACION_Y_MIGRACION.md).
