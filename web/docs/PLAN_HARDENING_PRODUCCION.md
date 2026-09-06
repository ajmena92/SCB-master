# Plan de hardening y publicación segura de producción

**Objetivo:** llevar el proceso de producción de 7.5/10 a 10/10 sin interrumpir
la operación del comedor ni tocar WinForms. La secuencia prioriza riesgos de
credenciales, datos y disponibilidad antes de los ajustes de exposición externa.

## Fase 0 — Puertas y documentación (no modifica producción)

**Qué implementar:** convertir este plan y el informe de auditoría en la lista de
control obligatoria de cada ventana. Confirmar responsable técnico, DBA, ventana,
rollback y revisión aprobada.

**Referencias:** `web/docs/RUNBOOK_DEPLOY_PRODUCCION.md:6-20`,
`web/docs/DESPLIEGUE_PORTAL.md`, `web/docs/decisiones/0002-postgresql-y-corte-unico.md`.

**Verificación:** revisión firmada de respaldo, staging, revisión Alembic,
secretos, HTTPS, smoke test y rollback.

**Guardas:** no desplegar si falta una puerta; no ejecutar DDL desde la API; no
usar credenciales del repositorio.

## Fase 1 — Contención y rotación de secretos (P0/P1)

**Qué implementar:** rotar `carnet_qr_clave` expuesto durante el diagnóstico,
reiniciar únicamente API y validar carnés nuevos y existentes. Revisar historial
de terminales, logs y copias temporales sin imprimir secretos.

**Referencias:** `web/backend/config.py:45-57`,
`web/ops/compose.production.yml:239-253`,
`web/docs/decisiones/0003-sesion-cookie-y-csrf.md:58-62`.

**Verificación:** archivos con `0600 root:root`, valores no versionados, API
saludable, login/CSRF y generación/lectura de carné correctos.

**Guardas:** nunca copiar secretos con rsync, pasarlos por argumentos, dejarlos
en logs o reutilizar el valor anterior.

## Fase 2 — Preflight obligatorio antes de cualquier despliegue (P1)

**Qué implementar:** añadir a `web/scripts/deploy-production.sh` una función
`preflight_prod()` de solo lectura que valide:

- seis archivos de secretos existentes, no vacíos, `0600` y propietario esperado;
- `docker compose config --quiet` con el `.env` real;
- `POSTGRES_DATA_PATH`, WAL y respaldo montados y con espacio suficiente;
- volumen/clúster PostgreSQL inicializado y roles esperados;
- edad y SHA-256 del último respaldo;
- API/PostgreSQL saludables antes de reemplazar imágenes.

**Referencias:** `web/scripts/deploy-production.sh:76-146`,
`web/ops/postgres/verificar_restauracion.sh:7-18`,
`web/docs/POSTGRESQL_OPERACION_Y_MIGRACION.md:31-53`.

**Verificación:** pruebas unitarias del script con secreto ausente, permisos
incorrectos, volumen vacío y checksum inválido; cada caso debe abortar antes de
`compose up`.

**Guardas:** no mostrar contenido de secretos; no corregir permisos o crear
volúmenes automáticamente durante el preflight; no continuar ante advertencias.

## Fase 3 — Despliegue por componente y protección de dependencias (P1)

**Qué implementar:** separar las rutas del script:

- `web`: `compose build web` y `compose up -d --no-deps web`;
- `api`: preflight de PostgreSQL y actualización controlada de API;
- `all`: respaldo previo, confirmación DBA y actualización ordenada.

**Referencias:** `web/scripts/deploy-production.sh:98-146`,
`web/ops/compose.production.yml:94-109`,
`web/docs/RUNBOOK_DEPLOY_PRODUCCION.md:120-137`.

**Verificación:** ensayo `web` con PostgreSQL detenido: el script debe abortar sin
recrear la base; ensayo `web` con API saludable: solo cambia el frontend; smoke
test y `docker compose ps` posteriores.

**Guardas:** no usar `up -d --build` sin `--no-deps` para frontend; no ejecutar
`down` como parte normal de un despliegue; no hacer downgrade automático.

## Fase 4 — SSH, almacenamiento y recuperación (P1/P2)

**Qué implementar:** fijar en el script `BatchMode=yes`,
`StrictHostKeyChecking=yes`, `IdentitiesOnly=yes` y un `known_hosts` controlado.
Confirmar que PostgreSQL usa `POSTGRES_DATA_PATH` externo en producción y que WAL
y respaldos están fuera del volumen Docker. Programar respaldo, checksum y
restauración temporal mensual y antes de cada actualización.

**Referencias:** `web/scripts/deploy-production.sh:76-95`,
`web/ops/compose.production.server.yml`,
`web/docs/POSTGRESQL_OPERACION_Y_MIGRACION.md:12-18,31-53`.

**Verificación:** huella SSH coincidente, `compose config --quiet`, respaldo
`COMPLETADO`, restauración temporal sin errores, prueba de espacio y simulación de
rollback en staging.

**Guardas:** nunca probar PITR sobre el volumen productivo; no borrar un volumen
para resolver un fallo; no guardar claves en el repositorio.

## Fase 5 — Endurecimiento HTTPS y exposición mínima (P2)

**Qué implementar:** elegir una sola capa como autoridad de cabeceras; elevar
HSTS a un año con `includeSubDomains` después de confirmar todos los subdominios
HTTPS; reducir `/health` externo a `{"estado":"ok"}` y mantener el detalle solo
para monitoreo interno.

**Referencias:** `web/ops/nginx/default.conf:15-36`,
`web/backend/config.py:108-121`,
`web/docs/decisiones/0003-sesion-cookie-y-csrf.md:49-62`.

**Verificación:** desde red externa validar redirección HTTP→HTTPS, certificado,
cadena, TLS 1.2/1.3, HSTS, CSP, ausencia de cabeceras duplicadas y contenido
mixto; comprobar CORS y cookies seguras.

**Guardas:** no activar HSTS de subdominios antes de verificar que todos sirven
HTTPS; no abrir `/docs`, `/redoc`, `openapi.json` ni PostgreSQL.

## Fase 6 — Evidencia operativa, monitoreo y cierre 10/10

**Qué implementar:** ejecutar smoke test autenticado administrativo y estudiantil,
pruebas negativas `401/403`, CSRF, permisos, reportes, comedor, PIN y logout;
añadir alertas para salud, disco, WAL, edad de respaldo, expiración TLS y errores
5xx. Documentar rollback y responsables.

**Referencias:** `web/docs/RUNBOOK_DEPLOY_PRODUCCION.md:139-162,202-214`,
`web/docs/DESPLIEGUE_PORTAL.md`, `web/docs/INTEGRACION_CONTINUA.md`.

**Verificación:** expediente de la ventana con commit, hashes de imágenes,
resultados de pruebas, capturas, logs sin datos sensibles, respaldo y decisión de
apertura del proxy.

**Guardas:** no considerar `health` único criterio de éxito; no registrar PIN,
cookies, contraseñas ni datos personales innecesarios.

## Orden de ejecución recomendado

`Fase 0 → Fase 1 → Fase 2 → Fase 3 → Fase 4 → Fase 5 → Fase 6`.

Las fases 1 y 2 son bloqueantes para cualquier nueva publicación. Las fases 3 y
4 protegen los datos y la recuperación. Las fases 5 y 6 cierran la exposición y
aportan evidencia suficiente para declarar 10/10.

## Criterio final 10/10

No queda ningún hallazgo P0/P1 abierto; el script aborta antes de tocar servicios
si falla una puerta; una publicación frontend no recrea dependencias; existe un
respaldo restaurable probado; secretos rotados y fuera del repositorio; HTTPS,
DNS y TLS comprobados desde red externa; smoke test autenticado y rollback
ensayados; y toda la evidencia queda archivada en `web/docs/`.
