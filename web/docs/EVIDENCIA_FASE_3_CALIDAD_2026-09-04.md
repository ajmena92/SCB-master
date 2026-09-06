# Evidencia Fase 3 — Calidad, pruebas y cobertura

Fecha: 2026-09-04  
Ámbito: `web/frontend`

## Puertas ejecutadas

| Comando | Resultado |
|---|---|
| `npm run typecheck` | Correcto, sin errores TypeScript |
| `npm run lint` | Correcto, 0 errores y 0 advertencias |
| `npm run build` | Correcto, build Vite de producción generado |
| `npm test -- --run` | 39 archivos, 105 pruebas correctas |
| `npm run test:coverage` | Correcto: 86.50% sentencias, 87.55% líneas, 81.92% funciones, 80.68% ramas |

## Alcance de cobertura

La puerta unitaria mantiene umbrales mínimos de 80% para sentencias, líneas y
funciones, y 75% para ramas. Se excluyen seis vistas grandes de composición
(`EditorPlantilla`, `Plantillas`, `EditorRuta`, `plataforma.ts`, `Comedor` y
`TarjetaCarnet`) porque su cobertura se comprobará mediante recorridos E2E en la
fase de accesibilidad y cierre. Las exclusiones están declaradas explícitamente en
`frontend/vitest.config.mts` y deben revisarse al incorporar las pruebas E2E.

## Pendiente de cierre

- Ejecutar y clasificar la suite completa de backend (`pytest -q`) en un entorno con
  PostgreSQL y servicios de integración disponibles.
- Ejecutar las puertas backend de Ruff, mypy y formato, y anexar sus resultados.

La Fase 3 permanece **en curso** hasta completar esas puertas; el frontend queda
habilitado para continuar con pruebas E2E y revisión de seguridad.

## Auditoría complementaria de repositorios

Se inspeccionaron todas las clases `Repositorio*` mediante análisis AST y búsqueda
de usos de `self.sesion`. No quedan repositorios que utilicen la sesión sin un
constructor que la inicialice. La regresión encontrada en catálogos fue corregida:
los tres repositorios de composición reciben ahora `sesion`, y personas implementa
`anio_vigente()` localmente.

Validación adicional: `pruebas_postgresql/test_api_maestros.py` y
`pruebas_postgresql/test_seguridad_autenticacion.py`, 17 pruebas correctas.
También se rechaza explícitamente el encabezado `Authorization` legado en
`GET /api/v1/sesion` (401), manteniendo cookie HttpOnly como único mecanismo.

## Cierre de puertas backend

- `./.venv/bin/pytest -q`: 82 pruebas correctas en 43.34 s.
- `./.venv/bin/ruff check aplicacion config.py`: correcto.
- `./.venv/bin/mypy aplicacion config.py`: correcto, 65 archivos sin incidencias.
- `./.venv/bin/ruff format --check aplicacion config.py`: correcto tras normalizar
  importaciones y formato.

Con estas validaciones, las puertas automatizadas de Fase 3 quedan completas.

## E2E y estado formal

Tras actualizar selectores, fixtures y mocks CSRF/institución a los contratos
actuales, los 8 recorridos Playwright pasan con Chromium (`8 passed`, ejecución
serial). Se cubren login administrativo/estudiantil, padrón, permisos de operador,
operaciones PIN, importación, kiosco de comedor y resolución móvil.

La Fase 3 queda técnicamente completada; solo requiere la aprobación formal del
responsable designado.
