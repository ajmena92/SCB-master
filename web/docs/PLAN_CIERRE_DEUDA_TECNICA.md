# Plan de cierre web y deuda técnica

## Alcance

Este plan completa la plataforma web canónica. No incluye migración de datos ni módulos de
WinForms, aliases, adaptadores, doble escritura o compatibilidad temporal. Cada cambio debe vivir
en `web/frontend/src/funcionalidades` o `web/backend/aplicacion` y usar contratos `/api/v1`.

## Puertas globales

- `npm run typecheck`, `npm run lint`, `npm run build` y `npm run test:coverage` aprobados.
- Cobertura mínima: 80 % líneas, 80 % funciones, 80 % sentencias y 75 % ramas.
- `npm run test:e2e` aprobado contra staging.
- Auditoría WCAG AA automatizada y revisión manual de teclado/foco.
- `pytest -q`, Ruff y mypy aprobados en backend.
- `alembic check` y `alembic upgrade head` aprobados en SQL Server staging.
- `docker compose ... config`, build, healthchecks y smoke HTTP aprobados.
- `rg` sin consumidores operativos de rutas históricas, `dbo` o `Seguridad`.

## Fases

### 1. Cobertura y calidad frontend

Fuentes: `frontend/vitest.config.ts`, `frontend/package.json` y
`frontend/src/funcionalidades/administracion/consultas/accesibilidad.test.ts`.

Trabajo:

- Cubrir interacciones y ramas de `RutasTab`, `EditorRuta`, `DashboardTab`, `AuditoriaTab`,
  `StudentCard`, `Plantillas` y consultas compartidas.
- Mantener exclusiones únicamente para `e2e`, `dist`, `node_modules` y artefactos.
- Generar `coverage/coverage-summary.json` y conservarlo como evidencia.
- Ampliar WCAG con axe fijado o una comprobación equivalente documentada, más revisión manual.

Aceptación: cobertura 80/80/80/75, lint y build aprobados, cero `any` innecesarios y auditoría WCAG
sin hallazgos críticos.

### 2. E2E contra staging

Fuentes: `frontend/playwright.config.ts`, `frontend/e2e/rutas-canonicas.spec.ts` y
`docs/DESPLIEGUE_PORTAL.md`.

Trabajo:

- Ejecutar login, navegación, formularios, permisos, CSRF, reportes y estados de error contra la
  URL staging mediante `PLAYWRIGHT_BASE_URL`.
- Usar un runner con Chromium compatible y permisos de proceso; no modificar la aplicación para
  ocultar errores del navegador.
- Publicar HTML de Playwright y capturas de fallos.

Aceptación: todos los recorridos críticos pasan en staging y el informe queda archivado.

### 3. Staging, SQL Server y Alembic

Fuentes: `backend/alembic/env.py`, `backend/alembic/README.md`,
`scripts/validar_alembic_docker.sh`, `ops/compose.production.yml` y `ops/Dockerfile.api`.

Trabajo:

- Confirmar TCP 1433, TLS, ODBC Driver 18 y `SERVERPROPERTY('ProductVersion')` con DBA.
- Ejecutar `current`, `alembic check` y `upgrade head` dentro del contenedor API.
- Validar rollback en una base de staging desechable.
- Ejecutar smoke `/api/health` y `/api/ready` después de la migración.

Aceptación: revisión Alembic en `head`, migraciones repetibles, rollback probado y smoke HTTP verde.

### 4. Cierre de deuda estructural

Fuentes: `docs/ARQUITECTURA.md`, `docs/CONVENCIONES_NOMBRES.md` y
`docs/PLAN_CIERRE_PLATAFORMA_WEB.md`.

Trabajo:

- Integrar el cliente OpenAPI generado en todas las consultas frontend.
- Dividir archivos que superen 300 líneas y eliminar contratos/documentación históricos de la
  aplicación activa.
- Completar observabilidad, auditoría, estados vacíos, errores, i18n y movimiento reducido.
- Actualizar documentación con evidencias y limitaciones reales.

Aceptación: guardas arquitectónicas sin excepciones nuevas, cliente generado actualizado y
documentación de despliegue alineada con Alembic.

### 5. Retiro definitivo de referencias legacy web

Trabajo:

- Ejecutar búsquedas globales de rutas históricas, `/api/admin`, `dbo` y `Seguridad` fuera de fuentes
  históricas explícitamente marcadas.
- Eliminar guardas o fixtures que dependan de módulos retirados, conservando únicamente pruebas
  negativas de ausencia.
- Confirmar que Docker, scripts, CI y frontend solo cargan la entrada modular.

Aceptación: cero consumidores operativos y suite completa aprobada. La migración WinForms queda
como fase posterior independiente.

## Evidencia y orden de ejecución

Cada fase debe registrar comandos, versiones, artefactos y resultado en este documento. No se avanza
si una puerta falla; no se sustituyen pruebas por mocks, exclusiones ni parches de compatibilidad.
