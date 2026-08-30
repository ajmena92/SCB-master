# ADR 0002: PostgreSQL y corte único desde SQL Server

- Estado: aceptada
- Fecha: 2026-08-29

## Contexto

La nueva plataforma solo necesita personas activas, matrícula anual, rutas,
beca vigente y menú. Las marcas, saldos, credenciales y auditoría históricas no
son necesarias y aumentarían el riesgo de una migración que será única.

## Decisión

PostgreSQL 17 es el único almacenamiento productivo de `web/`. SQL Server se
usa únicamente como origen de solo lectura durante un corte con escrituras
congeladas. No se permite doble escritura ni integración posterior. La identidad
de una persona se separa de su matrícula por año lectivo.

La API usa un rol DML sin DDL, Alembic usa un rol migrador y la administración
del motor usa un tercer rol. PostgreSQL permanece en una red Docker interna sin
publicar el puerto 5432. Se archiva WAL y se generan respaldos físicos y lógicos
en almacenamiento montado fuera del volumen de datos.

## Consecuencias

- Las 37 migraciones SQL Server anteriores no forman la historia de la nueva base.
- El corte requiere simulación sin errores, respaldo, congelamiento y reconciliación.
- Las cargas anuales posteriores provienen de XLSX y conservan la persona entre años.
- La recuperación se acepta solo después de restaurar un respaldo en una base temporal.
