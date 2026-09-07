# Precomprobación de despliegue completo

Destino autorizado: https://scsc.ctpplatanares.ed.cr.
Fecha del respaldo en UTC: 2026-09-06 03:03:59 (2026-09-05 en Costa Rica).

## Evidencia

- API, web y PostgreSQL encontrados saludables antes de modificar servicios.
- Compose remoto válido (`config --quiet`).
- Base PostgreSQL: 37 MB; revisión instalada consultada directamente:
  `0017_configuracion_institucional`.
- Código local: incluye `0018_control_intentos_autenticacion`, que amplía el
  identificador Alembic y crea la tabla de intentos de autenticación y su índice.
- Respaldo lógico y físico: `/respaldos/20260906T030359Z` dentro del montaje
  de respaldos. No se purgaron respaldos anteriores.
- Cuatro comprobaciones SHA256 aprobadas; restauración lógica temporal correcta,
  34 tablas públicas. La rutina eliminó su base temporal al terminar.
- HTTPS con validación de certificado: `/health` 200 (0,519 s), `/admin` 200
  (0,356 s), `/api/v1/sesion` 204 sin sesión. Son muestras individuales, no
  percentiles ni prueba de carga.

## Estado

## Despliegue efectuado

Con confirmación DBA explícita, se sincronizaron `backend/`, `frontend/`,
`ops/` y `scripts/`, preservando secretos. Se excluyeron entornos virtuales y
cachés locales; el sincronizador versionado ahora los excluye también en futuras
ejecuciones.

La primera comprobación del migrador descubrió que `alembic_version` era de
`scb_admin` y no concedía permisos al rol `scb_migrador`. Se transfirió solo la
propiedad de esa tabla al migrador, que ya tenía `USAGE` y `CREATE` en `public`.
No se modificaron permisos de la API. La ejecución posterior aplicó
`0018_control_intentos_autenticacion` y `current` confirmó la misma revisión
como `head`; existe `public.intento_autenticacion`.

Luego se reconstruyeron API y frontend. API, PostgreSQL y web quedaron
saludables. Las comprobaciones HTTPS posteriores devolvieron: `/health` 200,
`/admin` 200, `/api/v1/sesion` 204 sin sesión y emisión CSRF 204 con cookie
`Secure` y `SameSite=Lax`. No se iniciaron sesiones, no se hicieron
importaciones, pruebas de carga ni cambios de reservas o personas.

La restauración previa verificó el respaldo, pero esta revisión aún no se
ensayó sobre una copia restaurada separada antes de aplicarse. Debe quedar como
mejora obligatoria del runbook para toda migración futura con datos.
