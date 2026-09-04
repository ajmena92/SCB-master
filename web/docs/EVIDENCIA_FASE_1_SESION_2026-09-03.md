# Evidencia Fase 1 — sesión, autenticación y secretos

Fecha: 2026-09-03  
Estado: **Completada técnicamente — cookie+CSRF; cierre operativo pendiente**

## Dictamen independiente

La Fase 1 se considera **completada técnicamente únicamente respecto del
contrato cookie+CSRF**. La decisión se sustenta en las 12 pruebas ASGI, la
regeneración y verificación del contrato TypeScript desde OpenAPI, y la
validación estática de Compose descritas abajo.

No se declara cierre operativo. Permanecen abiertos el provisionamiento físico
de `CARNET_QR_CLAVE_FILE` y `CSRF_SECRET_FILE` con permisos mínimos, el diseño y
DDL aprobados de vencimiento por inactividad en Fase 5, y la decisión sobre
`pyodbc` para las pruebas de migración SQL Server. `pyodbc` queda clasificado
como dependencia de esa suite histórica/de migración, fuera del contrato web de
sesión, no como una dependencia omitida para hacer verde el corte.

La ejecución final de `npm run typecheck` es **inconclusa**: se inició una
repetición para el dictamen, pero excedió la ventana disponible. Cualquier
resultado anterior no se usa para ratificar el typecheck de cierre.

## Corte aplicado

- La API usa la cookie de host exclusivo `scb_sesion`, con `HttpOnly`, `Secure`
  por configuración productiva, `SameSite=Lax` y `Path=/api/v1`; no define
  `Domain`.
- El CSRF legible `csrf_token` usa `Path=/` para que la SPA pueda leerlo; la
  sesión `scb_sesion` permanece restringida a `Path=/api/v1` y es `HttpOnly`.
- Login exige `Origin` exacto y un CSRF anónimo firmado, con `iat`, `exp` y TTL
  configurable (`CSRF_ANONYMOUS_TTL_SECONDS`). Un login correcto emite
  la cookie de sesión y un CSRF legible, criptográficamente vinculado a esa
  sesión, sin devolver el portador en JSON.
- `POST`, `PUT`, `PATCH` y `DELETE` bajo `/api/v1` validan `Origin` y
  `X-CSRF-Token`. La ausencia o invalidez recibe `403`; una sesión ausente o
  inválida recibe `401` desde la dependencia de autenticación.
- Login, renovación y las cookies emitidas incluyen `Max-Age` y `Expires` desde
  el vencimiento absoluto. `STUDENT_SESSION_DAYS` y
  `ADMIN_SESSION_MINUTES` definen, respectivamente, la vigencia de portal y
  administración. La renovación rota el identificador sin extenderla.
- Logout, cambio de PIN y cambio de contraseña limpian ambas cookies. Los cambios
  de credencial mantienen además la revocación de sesiones en servidor.
- CORS limita credenciales al origen configurado e incluye `PATCH` y
  `X-CSRF-Token`.
- El frontend dejó de enviar `Authorization: Bearer` y se retiró
  `token_sesion.ts`; antes de una mutación obtiene el CSRF desde
  `GET /api/v1/autenticacion/csrf`.
- Compose consume `CARNET_QR_CLAVE_FILE` y `CSRF_SECRET_FILE` mediante Docker
  secrets. No se rotó ni se copió ningún valor de secreto.
- `COOKIE_SECURE=false` se rechaza fuera de `http://localhost:5173`; los orígenes
  HTTPS requieren cookies seguras. El cliente rechaza también
  `VITE_API_BASE_URL` con el prefijo protocol-relative `//`.

## Verificación ejecutada

- `python -m compileall -q aplicacion config.py` en `web/backend`: aprobado.
- `pytest -q tests/test_config_sesion.py`: **2 aprobadas** (lectura de vigencias
  absolutas y rechazo de `COOKIE_SECURE=false` fuera de localhost).
- `npm test -- --run src/compartido/consultas/csrf.test.ts
  src/compartido/consultas/manejo_sesion.test.ts`: **5 aprobadas**. Incluyen
  envío de CSRF desde cookie, bootstrap anónimo previo a login y respuesta 401
  manejada por el cliente. El typecheck no se considera aprobado para este
  dictamen: su repetición final quedó inconclusa.
- Se creó el entorno aislado `/tmp/scb-fase1-venv` e instaló
  `requirements-desarrollo.txt` (incluye `httpx==0.28.1` y
  `argon2-cffi==25.1.0` por sus requisitos transitivos fijados). No se modificó
  el Python global.
- `/tmp/scb-fase1-venv/bin/pytest -q tests/test_config_sesion.py
  tests/test_seguridad_csrf.py pruebas_postgresql/test_seguridad_autenticacion.py`:
  **12 aprobadas**: `test_configuracion_lee_vigencias_absolutas`,
  `test_cookie_no_segura_se_rechaza_fuera_de_localhost`,
  `test_csrf_anonimo_tiene_emision_vencimiento_y_no_es_reutilizable_tras_ttl`,
  `test_csrf_de_sesion_exige_el_token_opaco_correcto`,
  `test_sesiones_usan_vencimiento_absoluto_por_tipo`,
  `test_login_administrativo_se_bloquea_de_forma_persistente`,
  `test_login_exitoso_elimina_los_fallos_previos`,
  `test_login_emite_cookie_segura_y_no_expone_portador`,
  `test_csrf_es_legible_desde_la_spa_y_sesion_se_restringe_a_la_api`,
  `test_logout_y_renovacion_exigen_csrf_y_limpian_o_rotan_cookies`,
  `test_mutacion_rechaza_origin_o_csrf_y_sesion_bearer` y
  `test_preflight_cors_permite_solo_csrf_y_patch_del_origen_configurado`.
  Cubren login cookie+CSRF, ausencia de token de login en JSON, renovación,
  logout, `401`/`403`, Origin y preflight CORS.
- Las pruebas activas y sus fixtures PostgreSQL se migraron a clientes que
  conservan cookies y emiten `Origin`/`X-CSRF-Token`. Una búsqueda de Bearer en
  los `testpaths` activos solo conserva el caso negativo que confirma que un
  encabezado Bearer no autentica. No se modificaron pruebas históricas excluidas.
- `npm run generar:cliente && npm run verificar:cliente` en `web/frontend`:
  **aprobado**. El contrato TypeScript se produjo únicamente con el generador
  documentado desde OpenAPI, sin edición manual.
- `docker compose --env-file /tmp/scb-fase1-compose.env -f
  compose.production.yml config` usando una copia temporal de `.env.example`:
  **aprobado**. La configuración resultante contiene
  `CSRF_ANONYMOUS_TTL_SECONDS: "600"`. No se leyó, copió ni montó un secreto y
  no se levantó el stack.
- La ejecución de `pytest -q` completa se detiene durante la colección de dos
  pruebas de importación SQL Server (`test_importador_corte.py` y
  `test_sincronizar_estudiantes_2026.py`) porque `pyodbc` no pertenece a
  `requirements-desarrollo.txt`. Es una brecha preexistente de dependencias de
  migración, ajena al flujo cookie+CSRF; no se excluyeron ni alteraron esas
  pruebas para obtener un resultado artificialmente verde.
- La repetición de `npm run typecheck` inició, pero excedió la ventana de
  ejecución disponible antes de entregar el cierre; su resultado no se usa como
  evidencia nueva en este corte.
- `git diff --check`: aprobado. Se preservaron las modificaciones no
  relacionadas existentes en el árbol de trabajo.

## Bloqueos y riesgos abiertos

1. El archivo físico para `CARNET_QR_CLAVE_FILE` no se creó: mover el valor
   existente desde `.env` exige una acción operativa autorizada que no lea ni
   copie su contenido al repositorio. También debe provisionarse
   `CSRF_SECRET_FILE` con permisos mínimos.
2. El modelo `sesion_acceso` ahora sí aplica vencimiento absoluto configurable,
   pero no persiste última actividad. **No se afirma que exista vencimiento por
   inactividad**: requiere una migración DDL aprobada, fuera de este corte.
3. La puerta completa de backend requiere declarar e instalar explícitamente la
   dependencia de migración SQL Server (`pyodbc`) o separar esas pruebas de la
   suite web mediante una decisión documentada. No corresponde resolverlo
   modificando este corte de seguridad.

## Siguiente control

Provisionar secretos en staging, aprobar el diseño/migración de inactividad,
y resolver la dependencia de migración SQL Server para recuperar la puerta
backend completa. El contrato HTTP de cookie, CSRF, Origin, CORS, renovación,
logout y rechazo ya quedó verificado de manera ASGI.
