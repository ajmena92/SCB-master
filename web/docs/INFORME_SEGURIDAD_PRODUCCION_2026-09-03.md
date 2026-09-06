# Auditoría de seguridad del proceso de producción

**Fecha:** 2026-09-03  
**Alcance:** servidor productivo, Compose, Nginx, configuración FastAPI y script de publicación.  
**Método:** inspección de código/configuración, validación no destructiva por SSH, comprobación de salud y revisión de encabezados HTTPS. No se leyeron valores de secretos ni se ejecutaron pruebas destructivas.

## Veredicto ejecutivo

Producción está **operativa, pero no 10/10 todavía**. La API y el frontend están saludables, la configuración efectiva de Compose es válida, la base restaurada contiene 34 tablas públicas y el origen administrativo exige HTTPS. La puntuación de preparación de seguridad del proceso es **7.5/10**.

La principal brecha no está en el código de ejecución, sino en el procedimiento de publicación: un despliegue puede recrear dependencias y una base PostgreSQL vacía si no se valida el volumen y los secretos antes de levantar servicios. La restauración realizada corrigió el incidente actual, pero debe quedar automatizada como preflight y con prueba de recuperación.

## Evidencia positiva

- `api`, `web` y `postgres` aparecen saludables en producción; PostgreSQL no está publicado en una interfaz externa y la web solo está enlazada a `127.0.0.1:8081` en Compose (`web/ops/compose.production.yml:209-227`).
- La red `privada` está marcada como `internal` y API/PostgreSQL usan `read_only`, `no-new-privileges` y reducción de capacidades (`web/ops/compose.production.yml:98-109`, `232-238`).
- Producción usa `CORS_ORIGIN=https://scsc.ctpplatanares.ed.cr`, `COOKIE_SECURE=true`, `TRUSTED_PROXY_CIDRS` y `FORWARDED_ALLOW_IPS` limitados a la red del proxy.
- La aplicación rechaza orígenes HTTP cuando `APP_ENV=production` y solo permite `COOKIE_SECURE=false` para localhost (`web/backend/config.py:108-121`).
- Las respuestas observadas incluyen `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy` y CSP (`web/ops/nginx/default.conf:15-20`). La API no expone documentación pública (`web/ops/nginx/default.conf:34-36`).
- Los secretos montados en producción tienen permisos `0600` y propietario `root:root`; no forman parte del árbol sincronizado por rsync (`web/scripts/deploy-production.sh:90-95`).
- La restauración del respaldo fue verificada por checksum y dejó la API saludable; `/health` respondió estado correcto desde el servidor.

## Hallazgos priorizados

### SEC-P1-01 — El despliegue completo puede recrear dependencias críticas

**Ubicación:** `web/scripts/deploy-production.sh:124-135`; `web/ops/compose.production.yml:94-96`, `213-216`.  
El script ejecuta `compose up -d --build` para los servicios solicitados. Compose evalúa dependencias y, ante un volumen nuevo o secretos ausentes, puede iniciar/recrear PostgreSQL y API durante un cambio de frontend. Eso fue observable durante esta publicación: el volumen productivo quedó inicializado sin el esquema y fue necesario restaurar el respaldo.

**Riesgo:** indisponibilidad, arranque con base vacía o pérdida de continuidad si se publica sin respaldo/restauración validada.

**Mejora requerida:** separar `web` como `build web` + `up -d --no-deps web`; para `api` exigir preflight de PostgreSQL saludable, volumen esperado y secretos; para `all` crear respaldo comprobable y confirmación explícita antes de tocar PostgreSQL.

### SEC-P1-02 — Falta un preflight obligatorio de almacenamiento, secretos y respaldo

**Ubicación:** `web/scripts/deploy-production.sh:76-146`; `web/ops/compose.production.yml:229-253`.  
El script valida sintaxis y salud después de levantar, pero no comprueba antes que existan los seis archivos de secretos, que el volumen contenga un clúster inicializado, que las rutas WAL/respaldo estén montadas ni que el último respaldo tenga checksum válido.

**Riesgo:** detectar el fallo después de recrear contenedores, cuando la recuperación ya requiere intervención DBA.

**Mejora requerida:** añadir `preflight_prod()` de solo lectura que compruebe permisos `0600`, propietarios, tamaño no vacío sin imprimir contenido, volumen PostgreSQL no anónimo, espacio libre, WAL/backups y una verificación de restauración en base temporal.

### SEC-P1-03 — Rotación obligatoria de un secreto expuesto durante diagnóstico

Durante la recuperación se imprimió accidentalmente un valor secreto en una salida de diagnóstico de terminal. El valor no se incluye en este informe ni debe reutilizarse.

**Riesgo:** acceso a funciones que dependan de ese secreto, especialmente generación/verificación de carnés.

**Acción inmediata:** rotar `carnet_qr_clave` en producción, reiniciar API de forma controlada y verificar que carnés nuevos y existentes sigan funcionando. Auditar historial de terminal/logs y eliminar copias no necesarias.

### SEC-P2-01 — Verificación SSH no está fijada explícitamente en el script

**Ubicación:** `web/scripts/deploy-production.sh:76-87`.  
El acceso usa el puerto correcto y la huella fue confirmada en el equipo actual, pero el script no pasa `StrictHostKeyChecking=yes` ni un `UserKnownHostsFile` controlado.

**Riesgo:** depender de la configuración SSH del operador y aceptar una política menos estricta en otro equipo.

**Mejora:** definir opciones SSH comunes (`BatchMode=yes`, `StrictHostKeyChecking=yes`, `UserKnownHostsFile=...`, `IdentitiesOnly=yes`) y abortar si la huella no coincide.

### SEC-P2-02 — HSTS demasiado corto y cabeceras duplicadas

La respuesta HTTPS observada incluye `Strict-Transport-Security: max-age=86400` y cabeceras repetidas por el proxy externo y Nginx interno. Un día de HSTS ofrece protección limitada frente a downgrade.

**Mejora:** establecer HSTS desde el proxy TLS con `max-age=31536000; includeSubDomains` (solo después de confirmar que todos los subdominios son HTTPS), y definir una única capa como autoridad de cada cabecera.

### SEC-P2-03 — Endpoint de salud revela el motor de base de datos

**Ubicación:** `web/ops/nginx/default.conf:22-28`.  
`/health` devuelve `{"estado":"ok","baseDatos":"postgresql"}` públicamente.

**Riesgo:** divulgación menor de tecnología y superficie de reconocimiento.

**Mejora:** devolver únicamente `{"estado":"ok"}` al exterior y conservar el detalle de base de datos en una ruta interna/monitor autenticado.

### SEC-P2-04 — DNS público no quedó verificable desde este entorno

La resolución externa de `scsc.ctpplatanares.ed.cr` falló desde el entorno de auditoría, aunque Nginx respondió correctamente por HTTPS en el servidor usando el encabezado Host. No se debe considerar el DNS validado hasta comprobar registros A/AAAA, certificado y renovación desde una red externa.

## Plan para llegar a 10/10

1. Rotar inmediatamente el secreto expuesto y reiniciar solo API.
2. Implementar preflight de producción y despliegue `web` sin dependencias.
3. Fijar opciones de host-key SSH y añadir prueba de `docker compose config --quiet` antes de sincronizar.
4. Automatizar respaldo previo, checksum y restauración periódica en una base temporal aislada.
5. Endurecer HSTS y normalizar cabeceras en una sola capa.
6. Reducir la respuesta pública de `/health` y verificar DNS/certificado externamente.
7. Ejecutar una simulación documentada de rollback y revisar alertas de API, PostgreSQL, disco y vencimiento TLS.

## Validaciones realizadas

- `docker compose ... config --quiet`: correcto en producción.
- Contenedores `api`, `web` y `postgres`: saludables.
- Puerto PostgreSQL: no publicado externamente; web enlazada a loopback.
- Secretos: permisos `0600`, propietario `root:root`, sin lectura de contenido.
- HTTPS: `COOKIE_SECURE=true`, CORS HTTPS, CSP y cabeceras de seguridad presentes.
- Salud: `/health` respondió correctamente desde el servidor.

## Incidente posterior: CSRF 404 en producción

El 2026-09-04 se reprodujo `GET /api/v1/autenticacion/csrf` con `404` mientras
`/api/v1/sesion` respondía sin sesión. La fuente del backend ya contenía la ruta,
pero el contenedor API ejecutaba una imagen anterior. Se reconstruyó **solo API**
sin dependencias (`build --no-cache api` y `up -d --no-deps api`), sin modificar
PostgreSQL ni frontend.

Resultado posterior:

- API `healthy`.
- CSRF responde `204` y emite cookie `Secure`, `SameSite=Lax`, `Path=/`.
- `/api/v1/sesion` sin cookie responde `204`, comportamiento esperado.
- El HTML de origen no contiene Cloudflare Beacon.

El mensaje de CSP sobre `static.cloudflareinsights.com` proviene de la inyección
del proxy Cloudflare, no de la aplicación. La política lo bloquea de forma
intencional; para eliminar el mensaje sin debilitar CSP debe desactivarse Web
Analytics/Insights en Cloudflare. No se autorizó agregar un tercero a
`script-src`.

## Incidente posterior: login estudiantil HTTP 500

El 2026-09-04 el login estudiantil falló porque la base restaurada estaba en
Alembic `0017_configuracion_institucional`, mientras la API ya utilizaba la
tabla `intento_autenticacion` de `0018_control_intentos_autenticacion`. Además,
el contenedor migrador no podía leer el secreto `0600 root:root` y, tras
corregirlo, el rol migrador no podía modificar `alembic_version` propiedad del
administrador.

Se creó un respaldo nuevo antes de intervenir y se aplicó la estructura exacta
de la revisión 0018 en una transacción con el administrador PostgreSQL. La
verificación dejó `version_num=0018_control_intentos_autenticacion` y la tabla
`public.intento_autenticacion` presente. Con CSRF válido y credenciales ficticias,
el endpoint ahora responde `401 Cedula o PIN incorrecto` en lugar de `500`.

Queda como mejora del proceso corregir la separación de privilegios del migrador:
las migraciones autorizadas por DBA deben ejecutarse con un rol de migración que
pueda modificar objetos existentes, sin convertir el rol de API en propietario ni
conceder superusuario a la aplicación.

## Incidente posterior: carné del portal HTTP 503

El 2026-09-04 `/api/v1/portal/carnet` respondió `503` porque el secreto QR
montado tenía 65 bytes (incluido salto de línea), no una clave Fernet válida de
44 caracteres base64. Se generó una nueva clave Fernet sin imprimirla, se
conservaron permisos `0600 root:root` y se recreó únicamente el contenedor API
para montar el secreto actualizado.

Validación autenticada con la cuenta de prueba proporcionada:

- login portal: `200`;
- carné: `200`;
- fotografía del carné: `200`.

La rotación invalida códigos QR emitidos con la clave anterior; deben regenerarse
si existieran carnés impresos o digitales previos.

## Limitaciones

No se ejecutaron escaneos intrusivos, pruebas de credenciales, explotación, reinicio de PIN, venta, ni pruebas de carga. La auditoría no sustituye una revisión externa de penetración ni la rotación de credenciales compartidas previamente.
