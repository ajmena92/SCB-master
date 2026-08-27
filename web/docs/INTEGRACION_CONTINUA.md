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

El trabajo usa Python 3.12, instala las versiones exactas de
`requirements-desarrollo.txt` y ejecuta:

1. `ruff check .`;
2. `ruff format --check .`;
3. `mypy` con el alcance de `pyproject.toml`;
4. `pytest -q`.

Las pruebas backend se ejecutan con `pytest-cov`, generan `coverage/coverage.json` y exigen
80 % global. Los módulos críticos de identidad/seguridad, cuentas y asistencia exigen 90 %
mediante `coverage report --include`.

La imagen de API instala únicamente `requirements-produccion.txt`. Las migraciones se
comprueban con `requirements-migracion.txt` dentro de `Dockerfile.migracion`; ninguna de
las imágenes de ejecución instala las herramientas de prueba, Ruff o mypy.

## Puerta operativa de staging

El workflow ofrece una ejecución manual contra un runner `self-hosted` de staging. La
entrada `ejecutar_medicion_memoria` solo debe habilitarse por la persona responsable del
entorno cuando el stack ya esté levantado y `COMPOSE_ENV_FILE` apunte al archivo protegido.
La variable `URL_PRUEBA` debe apuntar al ingreso web de staging y `USUARIOS_PRUEBA` debe
representar la concurrencia aprobada para la prueba.

El entorno protegido `staging` debe definir `COMPOSE_ENV_FILE`, `URL_PRUEBA`,
`LATENCIA_P95_UMBRAL_MS` o `LATENCIA_P95_REFERENCIA_MS`, y los valores aprobados de
`UMBRAL_MEMORIA_PORCENTAJE` y `AUMENTO_LATENCIA_MAXIMO_PORCENTAJE`. La ejecución fija los
nombres de sus TSV con el identificador de la ejecución para no mezclar evidencias en runners
persistentes.

El script `web/scripts/medir_memoria_operativa.sh` genera carga concurrente durante la
duración indicada y conserva dos TSV por ejecución: un resumen fechado y sus muestras de
memoria. El resumen registra fecha de inicio y fin, usuarios, workers de Uvicorn, duración,
pico de memoria en MiB y porcentaje, latencia promedio y P95, total de solicitudes,
respuestas 413, errores HTTP 4xx/5xx, errores de red, reinicios y estado `OOMKilled`.
La puerta falla si algún servicio supera el 70 % de su límite, se reinicia, queda
`OOMKilled` o produce respuestas 5xx/errores de red. El límite porcentual se puede ajustar
con `UMBRAL_MEMORIA_PORCENTAJE` solo mediante una aprobación del entorno.

La acción `workflow_dispatch` conserva ambos TSV como artefacto obligatorio. La prueba debe
ejecutarse con tráfico representativo en staging, incluyendo cargas que deban ser rechazadas
por el límite de cuerpo; un cero en `respuestas_413` significa que no se ejercitó ese caso,
no que el límite quedó validado. La puerta requiere además `LATENCIA_P95_UMBRAL_MS` o
`LATENCIA_P95_REFERENCIA_MS`; con referencia, falla si el aumento excede
`AUMENTO_LATENCIA_MAXIMO_PORCENTAJE`.

La ejecución normal de Compose no activa `migracion`. Ese servicio solo tiene el perfil
`migracion`, no se reinicia automáticamente y la imagen rechaza cualquier inicio que no
reciba la confirmación explícita de la herramienta manual del DBA. El acceso al socket Docker,
al archivo `.env` y al grupo `GRUPO_DBA_MIGRACION` debe estar restringido por infraestructura.

## Alcance pendiente

La puerta de cobertura backend ya está versionada: el proveedor `pytest-cov`, sus exclusiones,
el umbral global de 80 % y el umbral de 90 % para dominios críticos se ejecutan en CI y se
conservan como artefacto JSON. La cobertura frontend mantiene sus propios umbrales V8
versionados.

En la verificación local del 2026-08-27, los dominios críticos alcanzaron 98,59 %, pero el
conjunto backend alcanzó 55,58 % y por eso la puerta global permanece correctamente fallida
hasta ampliar la cobertura funcional. La primera ejecución remota del workflow debe conservarse
como evidencia externa antes de considerar aprobada la puerta CI.

## Evidencia local del corte — 2026-08-25

- Backend: Ruff y formato aprobados, mypy sin errores y 52/52 pruebas aprobadas.
- Frontend: TypeScript, ESLint, Prettier, guardas arquitectónicas y build Vite aprobados.
- Vitest: 46 pruebas iniciadas en el recorrido global aprobaron; un worker no inició dentro del
  tiempo disponible en WSL. El archivo omitido se repitió de forma aislada y aprobó sus 2 pruebas.
- Contrato del workflow: 2/2 pruebas focalizadas aprobadas y sintaxis YAML aceptada por el parser
  local.

La cobertura backend y sus umbrales ya forman parte del flujo versionado. La primera ejecución
remota de GitHub Actions debe conservarse como evidencia externa de la ejecución efectiva.

## Modelos y límites de dominio

La base ORM transversal está en `aplicacion/nucleo/modelos_base.py`. Cada dominio mantiene sus
modelos en `aplicacion/modulos/<dominio>/modelos.py`; Alembic importa explícitamente todos esos
módulos antes de construir `target_metadata`. No existe `nucleo/modelos.py` ni código de identidad
dentro de `nucleo/identidad`.

## Registro de avance operativo — 2026-08-27

Completado en este corte:

- La imagen API mantiene únicamente dependencias de producción; pruebas, Ruff, mypy y Alembic
  quedan fuera de la imagen final.
- La imagen de migración es independiente, ejecuta exclusivamente Alembic y rechaza el inicio
  sin confirmación manual. El servicio `migracion` está excluido del Compose normal, pertenece
  al perfil manual `migracion` y no tiene reinicio automático.
- La puerta de staging carga la URL configurada con el número de usuarios indicado, descubre
  los workers de Uvicorn y registra memoria MiB/porcentaje, duración, latencia promedio/P95,
  413, 4xx, 5xx, errores de red, reinicios y `OOMKilled`.
- Cada ejecución conserva un TSV resumen fechado y otro TSV de muestras. CI los sube como un
  artefacto separado por `run_id`, sin reutilizar archivos de un runner persistente.
- La puerta rechaza uso superior al 70 %, reinicios, `OOMKilled`, 5xx, errores de red y P95
  superior al objetivo o al aumento aprobado.

Validaciones realizadas:

- 50 pruebas backend aprobadas; Ruff, formato y mypy aprobados.
- `bash -n` y `sh -n` aprobados; workflow YAML y `docker compose config` aprobados.
- Compose normal expone solo `api` y `web`; con `--profile migracion` aparece `migracion`.
- La imagen de migración construida rechazó el inicio sin confirmación (`código 78`) y quedó
  con entrada `/usr/local/bin/entrada_migracion.sh`, usuario no privilegiado y comando Alembic.
- Prueba controlada del TSV aprobada: 25 columnas, carga concurrente, latencia y estados
  normales. Prueba negativa controlada rechazada por memoria, latencia, OOMKilled y 5xx.

Pendiente para cerrar la puerta operativa:

- Levantar el stack en staging con la carga representativa y definir las variables protegidas
  del entorno (`COMPOSE_ENV_FILE`, `URL_PRUEBA`, usuarios, workers, memoria y latencia).
- Ejecutar manualmente `workflow_dispatch` con `ejecutar_medicion_memoria=true`, conservar el
  artefacto TSV y obtener la aprobación del responsable de staging.
- Ejercitar una carga que deba ser rechazada por el límite de cuerpo y confirmar `413`; un cero
  en esa columna no constituye evidencia de esa prueba.
- El DBA debe ejecutar migraciones únicamente desde una cuenta del grupo configurado en
  `GRUPO_DBA_MIGRACION`, con `CONFIRMAR_MIGRACION_DBA=SI`; no se ejecutó ninguna migración en
  este corte.
