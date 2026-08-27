# Integración continua de la plataforma web

El flujo versionado en [`.github/workflows/verificacion.yml`](../../.github/workflows/verificacion.yml)
ejecuta las puertas automatizadas de Fase 0 para cambios en `web/backend`, `web/frontend` y
`web/scripts`. También puede iniciarse manualmente.

## Puertas frontend

El trabajo usa la versión de Node declarada en `.nvmrc`, instala exclusivamente desde
`package-lock.json` mediante `npm ci` y ejecuta:

1. `npm run verificar`: TypeScript, ESLint, Prettier y guardas arquitectónicas;
2. `npm test`: pruebas Vitest;
3. `npm run build`: construcción de Vite.

## Puertas backend

El trabajo usa Python 3.12, instala las versiones exactas de `requirements.txt` y ejecuta:

1. `ruff check .`;
2. `ruff format --check .`;
3. `mypy` con el alcance de `pyproject.toml`;
4. `pytest -q`.

## Alcance pendiente

Esta línea base no declara completa la puerta global de cobertura. El repositorio todavía no
versiona el proveedor de cobertura frontend ni `pytest-cov`, y tampoco define exclusiones o
umbrales ejecutables para el 80 % global y el 90 % de seguridad, saldos y asistencia. Esos
controles deben incorporarse en un corte posterior, con su configuración, pruebas y evidencia,
sin presentar una medición parcial como cumplimiento.

Las ejecuciones locales validan los comandos que consume el flujo. La primera ejecución remota
del workflow debe conservarse como evidencia externa antes de considerar aprobada la puerta CI.

## Evidencia local del corte — 2026-08-25

- Backend: Ruff y formato aprobados, mypy sin errores y 62/62 pruebas aprobadas.
- Frontend: TypeScript, ESLint, Prettier, guardas arquitectónicas y build Vite aprobados.
- Vitest: 46 pruebas iniciadas en el recorrido global aprobaron; un worker no inició dentro del
  tiempo disponible en WSL. El archivo omitido se repitió de forma aislada y aprobó sus 2 pruebas.
- Contrato del workflow: 2/2 pruebas focalizadas aprobadas y sintaxis YAML aceptada por el parser
  local.

Esta evidencia comprueba los comandos y la configuración local, pero no sustituye la primera
ejecución de GitHub Actions ni habilita todavía los umbrales de cobertura pendientes.
