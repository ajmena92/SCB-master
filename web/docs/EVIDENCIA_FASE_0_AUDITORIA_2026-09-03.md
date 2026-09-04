# Evidencia de Fase 0 — línea base y decisiones

Fecha de corte: 2026-09-03
Estado: **en curso; no apta para cierre**
Alcance: observación, clasificación de `web/` y registro documental de la decisión
de sesión. No se aplicó DDL, no se cambió la aplicación, infraestructura, secretos
ni la configuración efectiva del modelo de sesión.

## Trazabilidad de la observación

- Directorio de trabajo observado: `/mnt/c/Dev/SCB-master`.
- Fecha del corte documental: 2026-09-03 (zona horaria del entorno:
  `America/Costa_Rica`).
- Comandos de observación registrados en esta evidencia: `git status --short`,
  `git diff --check`, `python3 -m pip show httpx cryptography ruff mypy` y
  `cd web/frontend && npm run verificar:arquitectura`.
- Esta evidencia conserva los resultados y conteos observados, no adjunta la
  salida íntegra de `git status --short`. Por ello no permite atribuir el detalle
  de cada archivo a una fase, persona o cambio concreto.

## Identidad del corte y árbol de trabajo

- Commit evaluado: `e4703c3026996160d5a90b571d926d7aae2b0e99`
  (`Depuracion de estilo visuales y mejroa backend y documentacion`,
  2026-09-02T19:21:29-06:00).
- `git status --short`, antes de crear esta evidencia, registró los conteos
  observados de **93 archivos rastreados modificados** y **11 no rastreados**
  (104 entradas en total). El listado detallado no fue adjuntado a este corte.
- El árbol no estaba limpio; por tanto esta evidencia no atribuye esos cambios
  a la Fase 0 ni autoriza su reversión. Las modificaciones de Alembic, backend,
  frontend, operación y documentación existentes requieren revisión de su
  propietario antes de cualquier corte posterior.

## Herramientas y dependencias locales observadas

| Componente | Resultado |
| --- | --- |
| Node | `v24.19.0` |
| npm | `12.0.2` |
| Python | `3.12.3` |
| pip | `24.0` (`/usr/lib/python3/dist-packages/pip`) |
| `cryptography` | instalado: `41.0.7` |
| `httpx` | no instalado |
| `ruff` | no instalado |
| `mypy` | no instalado |

La ausencia local de esos tres paquetes no contradice que estén declarados en
`backend/requirements-desarrollo.txt` ni sustituye una instalación limpia de
la Fase 3. Impide, en este entorno, repetir ahora las puertas backend completas.

## Guardas reproducibles ejecutadas

| Comando | Resultado | Evidencia / interpretación |
| --- | --- | --- |
| `cd web/frontend && npm run verificar:arquitectura` | **Falló** | El verificador informó 9 archivos mayores de 300 líneas sin excepción: `CalendarioMenu.tsx` (533), `plataforma.ts` (310), `EditarEstudiante.tsx` (419), `OperacionComedor.tsx` (340), `PersonasMatriculas.tsx` (407), `TarifasVentas.tsx` (504), `Rutas.tsx` (313), `repositorios_catalogos.py` (518) y `servicios.py` (380). Es bloqueo de la Fase 2, no autorización para refactorizar en Fase 0. |
| `python3 -m pip show httpx cryptography ruff mypy` | **Parcial** | Solo `cryptography` respondió; el comando informó que faltan `httpx`, `ruff` y `mypy`. |
| `git diff --check` | **Aprobó** | No reportó errores de espacios. Git avisó que el archivo ajeno `web/docs/INTEGRACION_CONTINUA.md` cambiaría CRLF a LF si Git lo reescribe; no fue modificado en esta fase. |

No se ejecutaron `npm test`, `npm run build`, Ruff, mypy ni Pytest: no son
necesarios para clasificar la línea base de esta fase y la puerta backend está
incompleta por las dependencias anteriores. La Fase 3 debe hacer la instalación
versionada y ejecutar todas las puertas.

## Inventario de contradicciones documentales inicial

| Tema | Evidencia | Clasificación y acción requerida |
| --- | --- | --- |
| Autoridad de DDL | `ARQUITECTURA.md` designa PostgreSQL y Alembic como objetivo; `sql/migrations/README.md` presenta 001–037 como migraciones manuales, y sus scripts usan sintaxis SQL Server (`OBJECT_ID`, `NVARCHAR`, `IDENTITY`). | Clasificación provisional por ruta y sintaxis. La decisión de datos y DBA deben validar, archivo por archivo y por destino, cuál es ejecutable, histórico o retirado; no se infiere autoridad operativa de esta lectura. |
| Revisión 0034 | `sql/migrations/034_migracion_datos_legados.sql` declara que su ruta oficial es “Alembic 0034”; el árbol Alembic observado contiene revisiones hasta `0018_control_intentos_autenticacion`. | Referencia documental pendiente de procedencia. No se ejecutó `alembic heads`, por lo que no se afirma una cabeza única ni cadena aplicable; DBA debe validar archivo, destino y ruta antes de cambios. |
| Migración/cierre de WinForms | `ARQUITECTURA.md` describe un corte único con transferencia; `PLAN_CIERRE_PLATAFORMA_WEB.md` ubica la migración WinForms en una fase posterior. | Alcance secuenciado: el cierre definitivo queda posterior al hito web, no es un conflicto por sí mismo. Producto, arquitectura y DBA deben aprobar sus precondiciones antes de promover transferencia. |
| Diseño visual | `ESTANDAR_PANTALLAS_EDICION.md` y `GUIA_VISUAL_COMPONENTES.md` exigen Segoe UI y azul histórico; `design_guidelines.json` prescribe Chivo/Karla y periwinkle. | Jerarquía canónica no resuelta. Fase 6 debe decidir la fuente vigente y el tratamiento de las restantes; esta evidencia no declara ninguna como histórica ni secundaria. |
| Estado de calidad | `CONTROL_RESOLUCION_AUDITORIA.md` registra ejecuciones locales verdes del 2026-09-03; el corte actual falla la guarda arquitectónica y no tiene herramientas backend instaladas. | Evidencia histórica no verificable para este commit y árbol porque no se adjuntaron salidas ni artefactos reproducibles a este corte. No sustituye una ejecución actual. |
| Estructura modular | `ARQUITECTURA.md` e `INTEGRACION_CONTINUA.md` describen `aplicacion/modulos/<dominio>` como arquitectura objetivo; la guarda observó composición actual transitoria y archivos fuera del límite. | Separación entre diseño objetivo y composición observada. Fase 2 debe caracterizar y migrar por dominio; no se afirma que la arquitectura objetivo ya esté activa. |

## Clasificación provisional de fuentes de migración

La clasificación se basa exclusivamente en rutas, README y sintaxis; no prueba
que una migración haya sido aplicada, que sea ejecutable en algún destino ni que
una fuente sea la autoridad final. DBA y responsable técnico deben validarla
archivo por archivo y por destino (PostgreSQL web, SQL Server heredado, local o
staging). La verificación de `alembic heads`, cadena Alembic, base vacía, SQL
offline, reversión y staging queda expresamente para la Fase 5.

| Fuente | Elementos | Clasificación inicial | Razón y límite |
| --- | --- | --- | --- |
| `backend/alembic/postgresql_versions/` | `0001_postgresql_inicial` a `0018_control_intentos_autenticacion` (18 archivos; la revisión interna `0016_confirmacion_sin_tiquete` está en el archivo `0015_confirmacion_sin_tiquete.py`) | **Candidata PostgreSQL web; pendiente DBA** | El README declara modelo web nuevo y PostgreSQL. No se ejecutó `alembic heads`; nombres y lectura estática no prueban una sola cabeza, cadena continua ni destino autorizado. |
| `sql/migrations/` | `001_menu_storage.sql` a `037_valida_horarios_operativos.sql`, incluido `008_identidad_canonica_revertir.sql` | **Candidata SQL Server histórica/transición; pendiente DBA** | La sintaxis y objetos observados sugieren SQL Server. Se requiere validación por archivo y destino; no se declara sin más como histórica ni se autoriza ejecución. |
| `sql/local/001_seguridad_compat_desarrollo.sql` | 001 | **Candidata local/heredada; pendiente DBA** | El README la limita a una copia local heredada y prohíbe producción; falta confirmar destino real, trazabilidad y retiro. |
| `sql/local/002_fotografias_personas_postgresql.sql`, `003_cedula_como_identificador_operativo.sql` | 002–003 | **Candidata PostgreSQL local; pendiente DBA** | Son SQL PostgreSQL locales, fuera de Alembic. Falta determinar archivo por archivo si se retiran, documentan o incorporan en una ruta aprobada. |
| `sql/dev/import_menu_escolar_2026.sql` y `sql/limpieza_nocturno_postgresql.sql` | semilla y limpieza operativa | **Candidata semilla/operación; pendiente DBA** | Su nombre y ubicación sugieren uso no migratorio, pero la clasificación no sustituye validar destino, permisos y procedimiento operativo. |

## Decisiones, responsables y próximos controles

| Control requerido | Responsable / autoridad | Fecha objetivo | Estado |
| --- | --- | --- | --- |
| ADR de un único modelo de sesión | Responsable técnico y revisor de seguridad **por designar** | Por asignar | **Aceptada**: [ADR-0003](decisiones/0003-sesion-cookie-y-csrf.md) adopta cookie `HttpOnly`/`Secure`/`SameSite=Lax`, sin `Domain` amplio, y CSRF firmado o vinculado criptográficamente a sesión. Fase 1 debe validar `Origin`, CORS con `PATCH`/`X-CSRF-Token`, respuestas `401`/`403` sin mutación, login/logout/renovación y retirar tokens de las respuestas de login. También debe migrar `CARNET_QR_CLAVE` a secreto montado, sin rotarla. La asignación formal de responsables y fecha de implementación continúa pendiente. |
| Nombrar responsable, revisor y fecha de cada fase | Patrocinador del plan | Por asignar | **Pendiente externo**: esta evidencia no inventa personas ni compromisos. |
| Validar autoridad y cadena de DDL en staging | DBA autorizado y responsable técnico | Por asignar | Pendiente de Fase 5; no se ejecutó DDL. |
| Corregir los 9 archivos fuera del límite o documentar excepciones fechadas | Responsable técnico de arquitectura | Posterior a ADR y asignación | Pendiente de Fase 2. |
| Instalar entorno limpio y ejecutar puertas completas | Responsable de calidad | Posterior a asignación | Pendiente de Fase 3. |

## Vacíos conocidos

- No hay identificador de PR ni responsables/fechas aprobados disponibles en el
  repositorio para este corte.
- No se consultó una base de datos, no se ejecutó `alembic current` ni se aplicó
  SQL; por ello no hay evidencia de `head`, historial aplicado, reversión o
  staging.
- La decisión de sesión está aceptada en ADR-0003, pero aún no se han asignado
  responsables ni fechas para implementarla, ni se han inspeccionado o expuesto
  valores de secretos.
