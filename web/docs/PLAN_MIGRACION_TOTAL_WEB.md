# Plan de control: migración total a la plataforma web modular

> **Alcance vigente (2026-08-26):** primero se cerrará la plataforma web al 100 % con sus
> propios datos y contratos canónicos. La migración de módulos y datos antiguos de WinForms
> queda pospuesta y no es requisito para el cierre web. El detalle ejecutable del alcance
> vigente está en [PLAN_CIERRE_PLATAFORMA_WEB.md](PLAN_CIERRE_PLATAFORMA_WEB.md).

## Estado del documento

| Campo                | Valor                                                                                                                       |
| -------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| Estado global        | Fase 0 en progreso: toolchain actualizado y puertas locales aprobadas; modularización y TypeScript estricto aún incompletos |
| Objetivo             | Cerrar primero una plataforma web modular completa; migrar WinForms en una fase posterior                                  |
| Arquitectura         | Monolito modular por dominios y cortes verticales                                                                           |
| Convención           | Español por defecto; ASCII en identificadores técnicos                                                                      |
| Última actualización | 2026-08-26                                                                                                                  |

Este es el control de la migración arquitectónica total. El [plan del menú administrativo](PLAN_IMPLEMENTACION_MENU_ADMIN.md) conserva el historial y control de ese subsistema.

## Estado de fases

| Fase | Resultado                                | Estado        | Evidencia de cierre                                                                                            |
| ---- | ---------------------------------------- | ------------- | -------------------------------------------------------------------------------------------------------------- |
| 0    | Fundamentos, arquitectura y convenciones | En progreso   | Documentación completada; guardas iniciales versionadas y pendientes de ampliación junto con la modularización |
| 1    | Modelo de datos web canónico             | ☐ No iniciada | Migraciones versionadas, base vacía reproducible y reconciliación aprobada                                     |
| 2    | Núcleo transversal                       | ☐ No iniciada | Configuración, identidad, RBAC, auditoría, errores y observabilidad modulares                                  |
| 3    | Dominios operativos completos            | ☐ No iniciada | Acta de aceptación por corte vertical                                                                          |
| 4    | Sustitución de funciones locales         | ☐ No iniciada | QR/PIN y reportes web aceptados; cero dependencias de ejecución locales                                        |
| 5    | Migración única y corte definitivo       | Pospuesta     | Se abrirá después de aceptar el cierre 100 % web; no bloquea el desarrollo web actual                       |

Estados permitidos: `No iniciada`, `En progreso`, `Bloqueada`, `Completada`.

## Fase 0: fundamentos

### Completado

- [x] Definir el monolito modular por dominios y sus reglas de dependencia.
- [x] Registrar la decisión y alternativas en ADR-0001.
- [x] Definir nombres españoles ASCII para código y español ortográfico para UI/documentación.
- [x] Documentar excepciones técnicas permitidas.
- [x] Actualizar `AGENTS.md` para declarar `web/` como plataforma activa.
- [x] Declarar migración única, sin doble escritura ni integración en tiempo de ejecución con el sistema local.
- [x] Enlazar los documentos desde el índice oficial.
- [x] Versionar guardas iniciales para referencias a `escritorio`, SQL fuera de repositorios, HTTP directo en componentes, archivos mayores a 300 líneas y vocabulario inglés propio, con pruebas unitarias y excepciones documentadas; la ejecución actual aún reporta archivos sobre el límite.
- [x] Endurecer ESLint con `import`, `jsx-a11y`, `react` y `react-hooks`, conservando excepciones explícitas durante la migración gradual.
- [x] Versionar configuración de Prettier y el script `verificar:formato`.
- [x] Versionar configuración de Ruff y comprobación de tipos backend mediante `web/backend/pyproject.toml`, pendiente de ejecutar en el entorno Python del proyecto.
- [x] Integrar la comprobación conjunta frontend en `npm run verificar`, incluyendo tipos, lint, formato y guardas arquitectónicas.
- [x] Fijar Node `24.19.0` LTS y npm `12.0.2`; sincronizar el lockfile con npm 12 y verificar el árbol instalado.
- [x] Actualizar React 19, Vite 8, FastAPI, Python 3.12 y las dependencias directas a sus líneas actuales compatibles.
- [x] Sustituir los plugins incompatibles con ESLint 10 por `import-x`, `@eslint-react` y `jsx-a11y-x`, sin desactivar reglas.
- [x] Dividir `PlantillasTab.jsx` y `RutasTab.jsx` por debajo del límite de 300 líneas y extraer sus editores a módulos de funcionalidad.
- [x] Completar el primer corte frontend en TypeScript estricto para Plantillas, con pruebas de caracterización, controles compartidos tipados y ubicación definitiva en `src/funcionalidades/menu`.

### Pendiente de implementación

- [ ] Incorporar TypeScript estricto y reorganizar el frontend por funcionalidades.
- [ ] Modularizar FastAPI por dominios y extraer los archivos centrales.
- [x] Ejecutar ESLint, Prettier, Ruff, mypy y pytest en entornos limpios con las dependencias instaladas.
- [x] Fijar versiones del conjunto de herramientas y unificar la operación frontend en npm 12.
- [x] Crear pruebas de caracterización antes de mover comportamiento en los dominios intervenidos.

### Alcance aún pendiente de la Fase 0

- [x] Integrar las guardas, formato, tipos, dependencias y cobertura en CI con configuración única de npm y Python; queda evidencia remota de ejecución.
- [ ] Ampliar los límites automatizados conforme se creen `funcionalidades/`, módulos backend y contratos OpenAPI; las guardas actuales no declaran que la reorganización ya esté completada.
- [x] Agregar Ruff y mypy al manifiesto Python y comprobar sus versiones fijadas en un entorno efímero limpio.
- [x] Versionar el resolver TypeScript para `@/` y activar la resolución de importaciones con `import-x` sobre la configuración plana de ESLint 10.
- [x] Ejecutar las pruebas de los verificadores y aprobar la guarda arquitectónica en el entorno local.

Estos puntos inician la implementación técnica; no se marcan cumplidos por existir la documentación.

## Secuencia de implementación

### Fase 1 — Datos

Crear esquemas web para `identidad`, `estudiantes`, `comedor`, `asistencia`, `transporte`, `beneficios`, `cuentas`, `importaciones`, `reportes` y `auditoria`. Adoptar SQLAlchemy 2 y Alembic, probar creación desde cero y definir reconciliaciones.

### Fase 2 — Núcleo transversal

Migrar configuración, conexión, transacciones, autenticación, sesiones, CSRF, RBAC explícito, auditoría, errores y comprobaciones de salud. Corregir la propagación de permisos y adoptar Argon2id con restablecimiento obligatorio.

### Fase 3 — Cortes verticales

Orden aprobado: comedor; estudiantes; asistencia; parámetros; transporte; reportes; cuentas; importaciones; identidad administrativa. Cada corte incluye datos, API, frontend, permisos, estados, pruebas y documentación.

### Fase 4 — Sustitución local

Sustituir DigitalPersona por QR/código de barras/PIN y Crystal Reports por salidas web/PDF/CSV/Excel. Eliminar nombres y rutas de compatibilidad; prohibir referencias de ejecución desde `web/` hacia `escritorio/`.

### Fase 5 — Corte posterior de WinForms (pospuesta)

Ejecutar dos ensayos anonimizados, respaldar, congelar escrituras, migrar una vez, reconciliar,
invalidar credenciales anteriores y retirar accesos de WinForms. Esta fase no forma parte del
cierre web-only y no debe iniciarse hasta que [PLAN_CIERRE_PLATAFORMA_WEB.md](PLAN_CIERRE_PLATAFORMA_WEB.md)
registre el hito “100 % web”. El histórico queda de solo lectura fuera de la rama activa tras
la aceptación.

## Puertas globales

- Tipos, análisis estático, formato, pruebas y construcción aprobados.
- Cobertura global mínima de 80 % y 90 % en seguridad, saldos y asistencia.
- Cero dependencias circulares, SQL fuera de repositorios o HTTP directo desde componentes.
- Cero autorización confiada solo al frontend.
- Contratos OpenAPI explícitos y cliente TypeScript generado.
- WCAG AA, movimiento reducido y pruebas en dispositivos reales.
- Migraciones repetibles, reversión ensayada y reconciliación documentada.
- Cero dependencia de ejecución de WinForms, DigitalPersona o Crystal Reports.
- Nombres nuevos en español salvo excepción técnica registrada.

## Registro de avance de la sesión — 2026-08-25

Estado de relevo recibido al reanudar la sesión, conservado como trazabilidad y superado por la actualización siguiente:

- Prettier `3.9.6` fue agregado al manifiesto frontend y el lockfile quedó sincronizado.
- Se comprobó que `npm ci` puede ejecutarse de forma temporal y reproducible para validar el toolchain; esta comprobación no implica que el resto de las puertas de calidad esté aprobado.
- Prettier `3.9.6` pasa `prettier --check` sobre el alcance amplio validado mediante una instalación temporal reproducible. `npm run verificar:formato` no se registra como aprobado localmente porque falta `node_modules` en el entorno de trabajo.
- ESLint, Ruff y mypy aún no son ejecutables en el entorno disponible; no se registran como aprobados.
- Las guardas arquitectónicas no pasan en la comprobación disponible: `PlantillasTab.jsx` tiene 380 líneas y `RutasTab.jsx` tiene 415 líneas.

### Actualización del toolchain y modularización — 2026-08-25

- Stack confirmado para esta etapa: React 19 + TypeScript + Vite; FastAPI + Python 3.12; SQL Server se conserva temporalmente como base de datos activa.
- Node `24.19.0` LTS y npm `12.0.2` están instalados y declarados en `.nvmrc`, `engines` y `packageManager`.
- Un `npm ci` limpio instaló 469 paquetes desde el lockfile con npm 12 y la política versionada `allowScripts`.
- React `19.2.8`, Vite `8.2.2`, ESLint `10.9.1`, Prettier `3.9.6`, FastAPI `0.141.1`, Ruff `0.16.4`, mypy `2.3.1` y el resto de dependencias directas quedaron fijados exactamente.
- ESLint 10 usa plugins actuales compatibles. Se retiraron `.eslintrc.cjs`, los plugins sin soporte para ESLint 10 y los patrones antiguos `forwardRef`, `Context.Provider` y `useContext` mediante codemods oficiales de React 19.
- TypeScript permanece en `6.0.3`: `7.0.2` no es instalable limpiamente con el analizador actual del ecosistema React/ESLint, que declara TypeScript menor a `6.1`. No se usaron `--force`, overrides ni parches.
- Las consultas React se migraron a TanStack Query `5.102.4`; los estados externos del carrusel usan `useSyncExternalStore` y las pruebas montan el proveedor real de consultas.
- Tailwind 4 quedó configurado con su plugin PostCSS actual y el build genera correctamente la hoja completa.
- `PlantillasTab.jsx` quedó en 218 líneas y `RutasTab.jsx` en 289. Sus editores viven en `src/funcionalidades/menu` y `src/funcionalidades/rutas`, ambos por debajo de 300 líneas.
- FastAPI usa `lifespan`, se retiró el alias de entrada `minutosAviso` y se sustituyó `datetime.utcnow()` por una convención UTC explícita compatible con `datetime2` de SQL Server.
- Se eliminaron únicamente dos copias incompletas y generadas de `node_modules`; no contenían código fuente y no son recuperables.

Validaciones aprobadas en esta actualización:

- frontend: TypeScript sin errores, ESLint 10 sin advertencias, Prettier aprobado, 48/48 pruebas y build Vite aprobado;
- backend: Ruff check y formato aprobados, mypy sin errores, 60/60 pruebas;
- guardas arquitectónicas aprobadas y `git diff --check` sin errores.

Hallazgos críticos que permanecen abiertos después de la actualización:

- hashes y verificación de credenciales legacy;
- referencias a `dbo` y `Seguridad`;
- archivos monolíticos centrales pendientes de extracción, especialmente `StudentPortal.jsx` y `EstudiantesTab.jsx`;
- rutas y permisos heredados que deben sustituirse por contratos canónicos en español.

### Primer corte frontend en TypeScript estricto — 2026-08-25

- Plantillas se movió desde `src/components/admin/PlantillasTab.jsx` a `src/funcionalidades/menu/paginas/Plantillas.tsx`; la navegación carga únicamente la ruta nueva, sin alias ni adaptador.
- El editor, los modelos de formulario y el saneamiento del payload quedaron tipados en `EditorPlantilla.tsx` y `componentesMenu.ts`. Las claves locales de edición se eliminan mediante un contrato explícito antes de publicar.
- `button`, `badge`, `dialog`, `input`, `label`, `select`, `skeleton`, `textarea` y `cn` se convirtieron a TypeScript con contratos de React 19 y Radix, eliminando la frontera JSX sin tipos que impedía el modo estricto.
- ESLint 10 incorpora `typescript-eslint` `8.68.0` y resuelve `@/` desde `tsconfig.json`. Esta versión declara compatibilidad con ESLint 10 y TypeScript `>=4.8.4 <6.1.0`; no se agregaron supresiones, overrides ni reglas desactivadas.
- Plantillas quedó en 229 líneas y su editor en 228; ninguna pieza nueva supera 300 líneas.
- SQL Server sigue siendo la única base activa de esta etapa. Este corte no modifica persistencia, esquemas ni conexiones.

Evidencia de verificación del corte:

- `npm run typecheck`, `npm run lint`, `npm run verificar:formato`, `npm run verificar:arquitectura`, `npm run build` y `git diff --check`: aprobados;
- Vitest: 48/48 pruebas aprobadas. El intento global concurrente agotó el tiempo de inicio de tres workers en WSL; 35 pruebas ejecutadas pasaron y los tres archivos no iniciados se repitieron por separado con 13 pruebas aprobadas. No hubo fallos de aserción.

### Reanudación prevista — 2026-08-26

1. Convertir el siguiente corte frontend, Rutas, a TypeScript estricto y mover los controles compartidos restantes solo cuando el tipado del dominio los requiera.
2. Dividir `StudentPortal.jsx` y `EstudiantesTab.jsx`, que aún superan el límite de 300 líneas.
3. Continuar la extracción por dominios y el reemplazo de rutas, permisos, `dbo`/`Seguridad` y hashes legacy, con pruebas de caracterización antes de mover comportamiento.
4. Integrar las puertas ya aprobadas en CI y añadir cobertura medible.
5. Mantener SQL Server durante esta etapa, sin introducir una segunda base ni sincronización; cualquier cambio futuro de motor requerirá una decisión y plan independientes.

## Evidencias

| Fecha      | Fase | Evidencia                                                                                                                                                                                                                                                                                                                                                                                      | Resultado                                                                                                                                                                      |
| ---------- | ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 2026-08-25 | 0    | [ARQUITECTURA.md](ARQUITECTURA.md), [CONVENCIONES_NOMBRES.md](CONVENCIONES_NOMBRES.md), [ADR-0001](decisiones/0001-monolito-modular-por-dominios.md), [AGENTS.md](../../AGENTS.md)                                                                                                                                                                                                             | Dirección, límites, lenguaje y política de corte documentados; implementación técnica permanece pendiente                                                                      |
| 2026-08-25 | 0    | [`verificar_arquitectura.py`](../scripts/verificar_arquitectura.py), [configuración y excepciones](EXCEPCIONES_VERIFICADORES.md), pruebas `backend/tests/test_verificar_arquitectura.py`                                                                                                                                                                                                       | Guardas iniciales implementadas; su validación automatizada, la comprobación del script y la integración CI se registrarán únicamente tras ejecutarse en el entorno versionado |
| 2026-08-25 | 0    | [`frontend/eslint.config.mjs`](../frontend/eslint.config.mjs), [`frontend/.prettierrc.json`](../frontend/.prettierrc.json), [`frontend/package.json`](../frontend/package.json), [`frontend/package-lock.json`](../frontend/package-lock.json), [`backend/pyproject.toml`](../backend/pyproject.toml)                                                                                          | Node 24 LTS/npm 12; ESLint 10, Prettier, Ruff y mypy aprobados; frontend 46/46, backend 60/60, build y guardas aprobados                                                       |
| 2026-08-25 | 0    | [`Plantillas.tsx`](../frontend/src/funcionalidades/menu/paginas/Plantillas.tsx), [`EditorPlantilla.tsx`](../frontend/src/funcionalidades/menu/EditorPlantilla.tsx), [`componentesMenu.ts`](../frontend/src/funcionalidades/menu/componentesMenu.ts), [`RutasTab.jsx`](../frontend/src/components/admin/RutasTab.jsx), [`EditorRuta.jsx`](../frontend/src/funcionalidades/rutas/EditorRuta.jsx) | Plantillas completó su primer corte en TypeScript estricto; Rutas permanece modularizado en JSX como siguiente corte. Sin aliases ni adaptadores de compatibilidad             |
