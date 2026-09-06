# Plan de resolución de pendientes de auditoría

Fecha: 2026-09-03
Estado global: **en ejecución**
Alcance: `web/` y su operación asociada. No autoriza cambios en `escritorio/`, ni doble escritura ni integración de ejecución con WinForms.

## Propósito y reglas de control

Este plan convierte los hallazgos confirmados de la auditoría en cortes pequeños,
reversibles y verificables. El estado de una fase solo puede pasar a **completada**
cuando se adjunte la evidencia indicada en esta tabla y se actualice el registro de
avance. La existencia de código, una prueba aislada o un documento no es evidencia
de cierre por sí sola.

| Estado | Significado |
| --- | --- |
| No iniciada | No se ha preparado el corte ni confirmado su línea base. |
| En curso | Hay cambios en una rama o entorno de trabajo; no hay cierre. |
| Bloqueada | Falta una decisión, acceso o aprobación externa identificada. |
| En revisión | Se entregó evidencia técnica y espera revisión. |
| Completada | Cumple todos los criterios de salida y tiene evidencia enlazada. |
| Completada técnicamente | El comportamiento y los contratos del corte están verificados. Los controles operativos identificados permanecen abiertos y no autorizan declarar el cierre operativo. |

### Registro de avance

| Fase | Estado | Responsable | Inicio | Cierre | Evidencia y enlace | Riesgos o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| 0. Línea base y decisiones | Completada técnicamente | Responsable técnico: ajmena92; revisión: Codex + validación de ajmena92; DBA: administrador de producción | 2026-09-03 | 2026-09-06 | [Evidencia Fase 0](EVIDENCIA_FASE_0_AUDITORIA_2026-09-03.md); [ADR-0003](decisiones/0003-sesion-cookie-y-csrf.md); commit de consolidación | Responsables, fecha y alcance definidos. El árbol concurrente de optimización queda atribuido al commit de consolidación; la aprobación operativa final permanece con ajmena92. |
| 1. Sesión, autenticación y secretos | Completada técnicamente | Responsable técnico: ajmena92; revisión: Codex + validación de ajmena92 | 2026-09-03 | Objetivo: 2026-09-15 | [Evidencia Fase 1](EVIDENCIA_FASE_1_SESION_2026-09-03.md) | Cookie+CSRF y contratos verificados (12/12 ASGI, OpenAPI/TypeScript, Compose y proxy local `204`). Pendientes operativos de ACL y migración de inactividad siguen documentados. |
| 2. Arquitectura y mantenibilidad | Completada técnicamente | Responsable técnico: ajmena92; revisión: Codex + validación de ajmena92 | 2026-09-03 | Objetivo: 2026-09-15 | [Evidencia Fase 2](EVIDENCIA_FASE_2_ARQUITECTURA_2026-09-03.md) | División por dominios verificada; persisten excepciones P2 documentadas. |
| 3. Calidad, pruebas y cobertura | Completada técnicamente | Responsable técnico: ajmena92; revisión: Codex + validación de ajmena92 | 2026-09-04 | 2026-09-06 | Evidencia Fase 3 y commit de consolidación | Pruebas, typecheck, lint, build y E2E documentados; falta solo aprobación operativa. |
| 4. Rendimiento e importaciones | En curso | Responsable técnico: ajmena92; revisión: Codex + validación de ajmena92 | 2026-09-04 | Objetivo: 2026-09-15 | [Evidencia Fase 4](EVIDENCIA_FASE_4_LINEA_BASE_2026-09-04.md) | Falta cerrar P95/P99 de importación, reportes, personas y concurrencia. |
| 5. Datos, migraciones y operación | Completada técnicamente | Responsable técnico: ajmena92; DBA: administrador de producción | 2026-09-05 | 2026-09-06 | Deploy all con confirmación DBA; `alembic current/heads`; respaldo y restauración temporal verificados | Falta completar TSV y el corte definitivo de WinForms; no se autoriza retirar accesos históricos. |
| 6. Diseño, accesibilidad y cierre | En curso | Responsable técnico: ajmena92; revisión: Codex + validación de ajmena92 | 2026-09-03 | Objetivo: 2026-09-15 | [Evidencia accesibilidad](EVIDENCIA_ACCESIBILIDAD_2026-09-03.md) | Falta repetir axe, auditar expediente y validar modales/formularios con datos reales. |

### Tarea añadida — revisión visual de notificaciones

- **Estado:** resuelta técnicamente el 2026-09-04.
- **Descripción:** revisar en Chromium móvil, tableta y escritorio por qué los
  mensajes toast aparecen recortados; validar ancho, salto de línea, botón de
  cierre, contraste y duración.
- **Alcance:** `frontend/src/components/ui/sonner.jsx` y tokens compartidos;
  actualizar `ESTANDAR_MODULOS_FRONTEND.md` si se modifica el patrón visual.
- **Criterio de salida:** evidencia visual en los tres tamaños y
  `npm run verificar:diseno` sin errores.
- **Resultado:** se aplicaron tokens semánticos, salto de línea y espacio para
  cierre; Chromium confirmó ausencia de overflow en 390, 768 y 1440 px. Se
  añadió deduplicación por identificador estable, sin imponer un límite nuevo de
  avisos visibles. “No asistiré” quedó como acción `warning` reversible y el contador
  usa `primary`/`warning` para conservar contraste en tema oscuro. El build
  local Docker se compiló y el servicio web respondió HTTP 200.

### Corte de progreso — 2026-09-04

| Fase | Progreso técnico estimado | Estado verificable en este corte |
| --- | ---: | --- |
| 0. Línea base y decisiones | 70% | En curso; pendientes responsables, fechas y atribución del inventario. |
| 1. Sesión, autenticación y secretos | 95% | Completada técnicamente; pendiente validación operativa de ACL y migración de inactividad. |
| 2. Arquitectura y mantenibilidad | 90% | Completada técnicamente; quedan excepciones P2 documentadas. |
| 3. Calidad, pruebas y cobertura | 100% | Completada técnicamente con evidencia de typecheck, lint, build, cobertura, backend y E2E. |
| 4. Rendimiento e importaciones | 30% | En curso; línea base parcial, cuenta sintética local y corrección de reservas disponibles. |
| 5. Datos, migraciones y operación | 0% | No iniciada. |
| 6. Diseño, accesibilidad y cierre | 70% | En curso; toast normalizado y puertas locales aprobadas; falta validación axe/bundle desplegado y modales reales. |

### Actualización de progreso — 2026-09-04 (portal y notificaciones)

| Fase | Avance actualizado | Evidencia del corte |
| --- | ---: | --- |
| 4. Rendimiento e importaciones | 30% | Sin cambios: continúa pendiente la medición P95/P99 de importaciones, reportes, personas y concurrencia. |
| 6. Diseño, accesibilidad y cierre | 78% | Toast deduplicado y opaco; navegación Carnet/Menú y contador con contraste semántico en tema oscuro; build Docker local exitoso y `HTTP 200` en `127.0.0.1:8081`. |

El porcentaje es una señal de seguimiento técnico, no un criterio de cierre. Solo
los estados y evidencias enlazados determinan si una fase está completada.

### Actualización de progreso — 2026-09-04 (credencial e impresión en producción)

- La credencial PDF ahora muestra debajo de la fotografía la fecha de impresión y
  el año lectivo; los iconos auxiliares del bloque QR se ocultan al imprimir.
- La hoja se configura en tamaño carta vertical, con la credencial horizontal
  centrada y el QR visible.
- Build frontend validado correctamente y frontend desplegado en producción con
  `deploy-production.sh web --remote`; el sitio respondió HTTP 200.
- No se ejecutaron migraciones en este corte.

### Actualización de progreso — 2026-09-05 (verificación PostgreSQL productiva)

- PostgreSQL productivo se verificó con el rol administrador: `alembic_version`
  registra `0018_control_intentos_autenticacion`, que coincide con la cabeza
  disponible; no hay migraciones pendientes.
- La tabla `intento_autenticacion` existe, confirmando que la revisión instalada
  está aplicada.
- El DBA transfirió la propiedad DDL de tablas y secuencias públicas a
  `scb_migrador`, conservando los permisos DML de `scb_api`.
- `alembic current` y `upgrade head` ejecutados con el contenedor migrador,
  ambos correctos; la base ya estaba en `0018_control_intentos_autenticacion`.
- El bloqueo de permisos queda resuelto técnicamente; permanece la validación
  operativa de respaldo/restauración de la Fase 5.

### Actualización de progreso — 2026-09-06 (puerta de despliegue y recuperación)

- `deploy all` ahora exige `CONFIRMAR_MIGRACION_DBA=SI` antes de construir o
  reiniciar servicios; sin la confirmación termina con código 2.
- El despliegue productivo completo se ejecutó con confirmación explícita y API
  saludable; `alembic current` coincidió con `alembic heads`.
- El respaldo productivo generó el conjunto lógico, globals, base física y WAL.
- La restauración temporal verificó hashes y restauró 34 tablas correctamente.
- La puerta técnica de migraciones, respaldo y recuperación queda completada;
  permanecen fuera de este corte los TSV de importación y el retiro definitivo
  de WinForms.

## Fase 0 — Línea base, alcance y decisiones

**Objetivo.** Eliminar contradicciones de evidencia antes de cambiar comportamiento.

1. Congelar el inventario inicial: commit/PR, salida de `git status --short`,
   resultado de `npm run verificar:arquitectura`, versiones de Node/Python y estado
   de las dependencias locales.
2. Clasificar los documentos históricos por fecha y vigencia. En particular,
   contrastar `CONTROL_RESOLUCION_AUDITORIA.md`, `INTEGRACION_CONTINUA.md`, el plan
   de cierre y los manuales de PostgreSQL; ninguna afirmación fechada sustituye una
   ejecución actual.
3. Emitir ADR breve que elija un único modelo de sesión:
   - sesión opaca Bearer, sin CSRF y sin token en almacenamiento JavaScript; o
   - cookie `HttpOnly`/`Secure`/`SameSite` con CSRF validado de extremo a extremo.
4. Inventariar provisionalmente cada migración SQL y Alembic por archivo y
   destino como candidata a `PostgreSQL web`, `SQL Server histórica/transición`,
   `local` o `retirada`. La autoridad efectiva de DDL queda pendiente de
   validación DBA y decisión documentada; no se deduce solo de rutas o nombres.
5. Designar responsable técnico, revisor y fecha objetivo por fase.

**Referencias.** `docs/ARQUITECTURA.md`, ADR-0001, ADR-0002,
`backend/aplicacion/README.md`, `backend/alembic/README.md`,
`sql/migrations/README.md` y `docs/PLAN_CIERRE_PLATAFORMA_WEB.md`.

**Criterio de salida.** ADR aprobado; inventarios enlazados; un único estado de
línea base; y ningún trabajo de sesión o migración iniciado contra contratos ambiguos.

**Guardas.** No reescribir la aplicación de forma masiva, no eliminar pruebas
históricas para ocultar fallos y no ejecutar DDL ni rotar secretos durante esta fase.

## Fase 1 — Sesión, autenticación, autorización y secretos

**Objetivo.** Implementar y documentar un solo contrato productivo de seguridad.

1. Escribir pruebas de caracterización para login administrativo y portal, bloqueo,
   desbloqueo por tiempo, vencimiento, logout, revocación por cambio de credencial,
   RBAC y token/cookie inválidos.
2. Según el ADR de Fase 0, implementar un único flujo en frontend, API, OpenAPI,
   CORS y documentación. Retirar el flujo alterno, incluidas pruebas y código CSRF
   que no participe en el contrato elegido.
3. Conforme a ADR-0003, comprobar `Set-Cookie`: `HttpOnly`, `Secure`,
   `SameSite=Lax`, `Path` explícito y ausencia de `Domain` amplio; rotación,
   expiración absoluta y por inactividad, revocación y limpieza. Verificar CSRF
   firmado o vinculado criptográficamente a sesión, `Origin` en mutaciones y
   login, logout y renovación. Cada rechazo `401` o `403` debe demostrar que no
   cambió estado; login no devuelve token de sesión.
4. Validar CORS con credenciales para los orígenes aprobados y comprobar que el
   preflight permite explícitamente `PATCH` y `X-CSRF-Token`, pero no métodos,
   cabeceras u orígenes no aprobados.
5. Crear inventario de secretos sin valores: propietario, origen autorizado,
   permisos del archivo, consumidores, procedimiento de reemplazo y evidencia de
   que no aparecen en logs, imágenes o repositorio.
6. Migrar `CARNET_QR_CLAVE` desde `.env` a un secreto montado y con permisos
   mínimos, sin exponer su valor en repositorio, imágenes o registros. Esta fase
   no rota la clave: una rotación exige posteriormente ventana aprobada, inventario
   y reemisión/inutilización de carnets.

**Referencias.** `backend/aplicacion/entrada.py`, `dependencias_v1.py`,
`casos_identidad.py`, `repositorios_identidad.py`,
`frontend/src/compartido/consultas/{cliente_http,csrf,token_sesion}.ts`,
`docs/CONTRATOS_API.md` y `docs/DESPLIEGUE_PORTAL.md`.

**Criterio de salida.** Documentación, contratos, pruebas y ejecución describen el
mismo flujo; matriz de seguridad aprobada; CORS incluye y limita `PATCH` y
`X-CSRF-Token`; login no entrega token de sesión; y `CARNET_QR_CLAVE` se consume
como secreto montado sin rotación ni exposición de su valor.

**Guardas.** No cambiar cookies/CORS sin pruebas negativas, no asumir que
`COOKIE_SECURE` implica que se emiten cookies, no aceptar doble envío CSRF sin
vínculo criptográfico a sesión y no rotar QR sin el procedimiento operativo
aprobado.

### Dictamen independiente de cierre técnico

La Fase 1 queda **completada técnicamente solo para el contrato cookie+CSRF**,
con evidencia enlazada. El dictamen no equivale a cierre operativo ni a la
conclusión de todos los criterios operativos originales de la fase.

- La ejecución ASGI focal fue de **12/12 pruebas aprobadas**: 2 de configuración
  de sesión, 3 de seguridad CSRF y 7 de autenticación PostgreSQL. Cubre emisión
  de cookie, ausencia de portador en JSON, renovación, logout, `401`/`403`,
  `Origin`, Bearer rechazado y preflight CORS.
- El contrato se regeneró y verificó con `npm run generar:cliente && npm run
  verificar:cliente`; la evidencia confirma que proviene de OpenAPI y no de
  edición manual.
- `docker compose --env-file /tmp/scb-fase1-compose.env -f
  compose.production.yml config` confirmó
  `CSRF_ANONYMOUS_TTL_SECONDS: "600"`, sin levantar servicios ni leer secretos.
- La repetición destinada a ratificar `npm run typecheck` no terminó dentro de
  la ventana disponible. Por tanto, el **typecheck final es inconcluso** y no se
  usa como evidencia de cierre.

Quedan expresamente fuera del cierre técnico: el provisionamiento físico y con
permisos mínimos de `CARNET_QR_CLAVE_FILE` y `CSRF_SECRET_FILE`; el diseño y DDL
aprobados para vencimiento por inactividad, que corresponde a la Fase 5; y la
decisión de declarar/instalar `pyodbc` para la suite de migración SQL Server. Esa
dependencia no forma parte del contrato web cookie+CSRF y no debe resolverse
excluyendo pruebas ni alterando el flujo de sesión.

## Fase 2 — Arquitectura y mantenibilidad

**Objetivo.** Reducir deuda de transición sin perder comportamiento ni permisos.

1. Crear pruebas de caracterización para cada corte que se moverá desde la
   composición actual a `modulos/<dominio>`.
2. Migrar por dominio y por contrato público: API → servicio → repositorio. Mantener
   `entrada.py` como única composición hasta que el nuevo corte esté completo.
3. Resolver los nueve archivos actualmente fuera del límite de 300 líneas mediante
   extracción cohesionada o excepción temporal con responsable, razón y fecha de
   retiro. No añadir excepciones genéricas.
4. Aplicar las guardas de arquitectura: sin SQL fuera de repositorios, HTTP directo
   desde componentes, acceso a repositorios privados de otro dominio, aliases
   heredados ni dependencias de `escritorio/`.

**Referencias.** `docs/ARQUITECTURA.md`, `scripts/verificar_arquitectura.py`,
`scripts/configuracion_verificadores.json` y `docs/EXCEPCIONES_VERIFICADORES.md`.

**Criterio de salida.** `npm run verificar:arquitectura` aprobado o cada excepción
restante está aprobada, fechada y justificada; pruebas de los cortes modificados
aprobadas; y no hay regresión de autorización.

## Fase 3 — Calidad, pruebas y cobertura reproducibles

**Objetivo.** Convertir los manifiestos versionados y CI en evidencia repetible.

1. Preparar entornos limpios con Node 24/npm 12 y Python 3.12 mediante
   `npm ci` y `pip install -r requirements-desarrollo.txt`; registrar plataforma,
   versiones y hash/commit evaluado.
2. Ejecutar las puertas frontend: `npm run verificar`, `npm test` y `npm run build`.
3. Ejecutar las puertas backend: Ruff, formato, mypy, `pytest -q`, cobertura global
   mínima de 80 % y 90 % para dominios críticos.
4. Clasificar cada prueba histórica excluida como portable al diseño activo,
   históricamente preservada con motivo verificable, o retirada con autorización.
   No ampliar `testpaths` ni borrar pruebas para obtener verde artificial.
5. Obtener una ejecución remota verde del workflow y conservar enlaces a artefactos
   de cobertura; corregir primero fallos reales de la línea base.

**Referencias.** `.github/workflows/verificacion.yml`,
`backend/requirements-desarrollo.txt`, `backend/pytest.ini`,
`frontend/package.json` y `docs/INTEGRACION_CONTINUA.md`.

**Criterio de salida.** Puertas locales limpias y ejecución CI enlazada para el
commit evaluado; cobertura conforme; inventario de pruebas excluidas revisado.

## Fase 4 — Rendimiento, importaciones y concurrencia

**Objetivo.** Corregir únicamente los cuellos de botella demostrados con medición.

1. Definir escenarios, datos anonimizados, volumen máximo de Excel, concurrencia,
   presupuesto de memoria, CPU, consultas y latencia P95 para importación, reportes
   y operación de comedor.
2. Medir la línea base de la lectura Excel de 12 MiB, previsualización, confirmación
   y endpoints que usan sesiones SQLAlchemy síncronas desde rutas `async`.
3. Eliminar patrones N+1 y cargas completas no necesarias mediante consultas por
   lote, paginación o bloques. Preservar el contrato de previsualización, huella e
   idempotencia.
4. Elegir y registrar una sola estrategia para operaciones bloqueantes: endpoints
   síncronos/aislamiento en threadpool, trabajo en segundo plano, o migración async
   integral. No convertir solo la dependencia de sesión.
5. Repetir las mediciones y adjuntar comparación antes/después.

**Referencias.** `backend/aplicacion/api_importaciones.py`,
`casos_importacion.py`, `repositorios_importacion.py`, `nucleo/postgresql.py` y
`docs/INTEGRACION_CONTINUA.md`.

**Criterio de salida.** Presupuestos aprobados y cumplidos en el escenario acordado;
sin regresión funcional, de autorización o de idempotencia.

## Fase 5 — Datos, migraciones y operación

**Objetivo.** Asegurar un despliegue reproducible y recuperable del modelo web.

1. Validar en entorno aislado `alembic heads`, migración desde base PostgreSQL vacía,
   SQL offline y ruta de reversión aprobada. Registrar la cabeza real, sin inferirla
   desde nombres de archivos o documentos históricos.
2. Corregir las discrepancias entre documentación y scripts de importación anual
   antes de promover datos: campos admitidos, simulación, aplicación, reconciliación
   y tratamiento de PIN.
3. Ejecutar en staging el respaldo/restauración, la validación de migración y la
   reconciliación de datos con DBA autorizado; la API nunca ejecuta DDL.
4. Configurar y ejecutar la puerta manual de memoria en staging con tráfico
   representativo, caso rechazado `413`, P95, límites de memoria, TSV y aprobación
   de la persona responsable.
5. Mantener el corte definitivo de WinForms como fase separada posterior al hito
   “100 % web”: ensayos anonimizados, respaldo, congelamiento, migración única,
   reconciliación, invalidación de sesiones y retiro de accesos.

**Referencias.** `backend/alembic/README.md`, `sql/migrations/README.md`,
`ops/compose.production.yml`, `scripts/validar_alembic_docker.sh`,
`docs/POSTGRESQL_OPERACION_Y_MIGRACION.md` y `docs/PLAN_CIERRE_PLATAFORMA_WEB.md`.

**Criterio de salida.** Evidencia de staging, restauración y migración repetible;
artefactos TSV de carga aprobados; y ninguna migración aplicada con rol de API.

## Fase 6 — Diseño, accesibilidad y cierre

**Objetivo.** Dejar una única referencia de interfaz y comprobar calidad final.

1. Resolver mediante decisión documentada la jerarquía canónica aún no resuelta entre
   `ESTANDAR_PANTALLAS_EDICION.md`/`GUIA_VISUAL_COMPONENTES.md` y
   `design_guidelines.json`; designar una fuente vigente y el tratamiento explícito
   de las restantes.
2. Auditar los recorridos críticos en móvil y escritorio: navegación por teclado,
   foco visible, etiquetas, contraste 4.5:1, objetivos táctiles de 44 px, errores,
   carga, vacío, movimiento reducido y ausencia de desbordamiento horizontal.
3. Añadir pruebas de componentes y Playwright para los recorridos críticos que se
   hayan modificado; adjuntar capturas y resultados de revisión manual.
4. Ejecutar una revisión final contra este plan, actualizar el registro de avance y
   enlazar evidencia de todas las fases.

**Criterio de salida.** Una guía visual vigente, WCAG AA comprobado en recorridos
críticos, pruebas finales aprobadas y cierre firmado por responsables técnicos y
operativos.

## Cadencia de seguimiento

- Actualizar el registro al inicio y al cierre de cada corte, nunca al finalizar
  solo una edición local.
- En cada revisión: estado, cambios, evidencia, métricas, riesgos nuevos, decisión
  requerida y siguiente fase habilitada.
- Una fase bloqueada debe registrar la decisión o autoridad faltante; no se sustituye
  con suposiciones técnicas.
- Reejecutar las puertas proporcionales después de cada corte y todas las puertas
  de Fase 3 antes de cerrar el plan.
