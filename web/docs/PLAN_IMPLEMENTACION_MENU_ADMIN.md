# Plan de implementación: navegación administrativa tipo app

## Estado del documento

| Campo | Valor |
| --- | --- |
| Estado global | Fases 0–5 implementadas; modernización Vite completada; Fase 6 en cierre |
| Alcance inicial | Shell administrativo, navegación responsive y rutas profundas |
| Alcance posterior | Integración en la migración total de WinForms a web |
| Responsable | Por asignar |
| Última actualización | 2026-08-25 |
| Documento relacionado | [PRD del portal](../memory/PRD.md) |
| Plan arquitectónico superior | [Migración total a la plataforma web](PLAN_MIGRACION_TOTAL_WEB.md) |

> Este documento es el control oficial de fases. No se debe marcar una fase como completada sin adjuntar evidencia en la columna correspondiente.

## Índice

- [1. Objetivo y resultado esperado](#1-objetivo-y-resultado-esperado)
- [2. Arquitectura aprobada](#2-arquitectura-aprobada)
- [3. Estado de fases](#3-estado-de-fases)
- [4. Fase 0: alcance y matriz funcional](#4-fase-0-alcance-y-matriz-funcional)
- [5. Fase 1: fuente única de navegación](#5-fase-1-fuente-única-de-navegación)
- [6. Fase 2: rutas reales](#6-fase-2-rutas-reales)
- [7. Fase 3: shell responsive](#7-fase-3-shell-responsive)
- [8. Fase 4: integración de módulos actuales](#8-fase-4-integración-de-módulos-actuales)
- [9. Fase 5: permisos RBAC](#9-fase-5-permisos-rbac)
- [10. Fase 6: pruebas y despliegue gradual](#10-fase-6-pruebas-y-despliegue-gradual)
- [11. Fase 7: incorporación a la migración total](#11-fase-7-incorporación-a-la-migración-total)
- [12. Modernización del toolchain frontend](#12-modernización-del-toolchain-frontend)
- [13. Criterios globales de aceptación](#13-criterios-globales-de-aceptación)
- [14. Registro de decisiones, riesgos y evidencias](#14-registro-de-decisiones-riesgos-y-evidencias)

## 1. Objetivo y resultado esperado

Transformar el panel administrativo actual —que mantiene diez módulos en pestañas horizontales dentro de un único estado React— en una experiencia tipo aplicación:

- Barra inferior fija en móvil.
- Menú lateral persistente en escritorio.
- Mismos grupos y destinos en ambos dispositivos.
- URLs profundas por módulo, compatibles con recarga, favoritos y Atrás/Adelante.
- Menú filtrado por permisos, sin usar la interfaz como mecanismo de seguridad.
- Base preparada para migrar todo el sistema local por oleadas.

El panel actual se encuentra en [AdminPanel.jsx](../frontend/src/pages/AdminPanel.jsx). La navegación móvil reutilizable está en [AdminBottomNav.jsx](../frontend/src/compartido/componentes/AdminBottomNav.jsx).

## 2. Arquitectura aprobada

### 2.1 Grupos de navegación

| Grupo | Función predeterminada | Módulos iniciales |
| --- | --- | --- |
| Inicio | Resumen operativo | Dashboard |
| Operación | Trabajo diario | Menú, Calendario, Sustituciones, Rutas, Correcciones |
| Personas | Expedientes y credenciales | Estudiantes/PIN |
| Reportes | Consultas y exportaciones | Registro de transporte |
| Más | Configuración y trazabilidad | Parámetros del portal, Auditoría |

La barra inferior tendrá como máximo cinco destinos. El botón **Más** abrirá un `Drawer` inferior en móvil; sus opciones también tendrán rutas directas.

### 2.2 Rutas objetivo

```text
/admin                         Login administrativo
/admin/panel                   Redirección compatible a /admin/panel/inicio
/admin/panel/inicio            Dashboard
/admin/panel/operacion         Hub de Operación
/admin/panel/operacion/menu
/admin/panel/operacion/calendario
/admin/panel/operacion/sustituciones
/admin/panel/operacion/rutas
/admin/panel/operacion/correcciones
/admin/panel/personas          Hub de Personas
/admin/panel/personas/estudiantes
/admin/panel/reportes          Hub de Reportes
/admin/panel/reportes/transporte
/admin/panel/mas/parametros
/admin/panel/mas/auditoria
```

### 2.3 Límites conocidos

El portal web todavía no equivale completamente a WinForms. Control biométrico, recargas, importación, becas, seguridad RBAC, reportes Crystal e impresión masiva requieren fases propias. DigitalPersona no debe darse por migrado sin decidir si se conservará una estación local, se usará un agente de hardware o se sustituirá por QR/PIN.

## 3. Estado de fases

| Fase | Nombre | Estado | Evidencia requerida |
| --- | --- | --- | --- |
| 0 | Alcance y matriz funcional | ✅ Completada | PRD actualizado, matriz escritorio/web y guardas verificadas |
| 1 | Fuente única de navegación | ✅ Completada | Catálogo y pruebas ejecutados con Vitest |
| 2 | Rutas reales | ✅ Completada | Build Vite y rutas anidadas verificados |
| 3 | Shell responsive | ✅ Completada | Build Vite y shell responsive verificados |
| 4 | Integración de módulos actuales | ✅ Completada | 11/11 archivos de pruebas pasan; 44 casos frontend verificados |
| 5 | Permisos RBAC | ✅ Completada | `/api/v1/sesion`, navegación y todos los endpoints administrativos actuales aplican roles/permisos; pruebas unitarias aprobadas |
| 6 | Pruebas y despliegue gradual | ◐ En progreso | Build y pruebas de módulos pasan; falta staging y validación multidispositivo |
| 7 | Incorporación a la migración total | ☐ No iniciada | Acta de aceptación por dominio y plan de corte único |

Estados permitidos: `No iniciada`, `En progreso`, `Bloqueada`, `Completada`.

## 4. Fase 0: alcance y matriz funcional

**Estado: Completada, 2026-08-24.** La [Matriz de migración del menú administrativo](MATRIZ_MIGRACION_MENU_ADMIN.md) está publicada, el PRD fue sincronizado y las guardas de alcance fueron verificadas con evidencia documental y de código. Las rutas, el shell responsive y el control RBAC quedaron implementados en las fases siguientes.

### Tareas

- [x] Actualizar el PRD: el objetivo pasa de portal complementario a migración progresiva del sistema local.
- [x] Crear una matriz con módulo, grupo, ruta, permiso, estado web y dependencia técnica.
- [x] Comparar los 15 destinos del shell legacy con las 15 entradas del shell moderno (`UIShellHost`); se documentan sus diferencias.
- [x] Documentar que el inventario consolidado contiene 17 filas: la unión de ambos listados (16 capacidades, porque `Imprimir` es legacy y `Seguridad` es moderno) más Dashboard.
- [x] Confirmar los cinco destinos inferiores y el comportamiento de **Más**.
- [x] Definir qué ocurre cuando una URL no está autorizada: `403` y pantalla de acceso denegado; `401` redirige al login.
- [x] Documentar la matriz de permisos de Dashboard y módulos actuales; las claves web pendientes quedan explícitas para la Fase 5.

### Referencias

- [PRD.md](../memory/PRD.md)
- [REQUISITOS_COMEDOR.md](REQUISITOS_COMEDOR.md)
- [FrmPrincipal.vb](../../escritorio/SCSC/FrmPrincipal.vb)
- [SeguridadPermisosSistema.vb](../../escritorio/SCSC/Clases/SeguridadPermisosSistema.vb)

### Verificación y guardas

- [x] Ningún módulo se marca como migrado si solo tiene funcionalidad parcial; la matriz distingue `Implementado`, `Parcial` y `No iniciado`.
- [x] “Reporte WinForms” se renombra conceptualmente a “Registro de transporte” hasta tener paridad real.
- [x] No se trasladan a Internet permisos propios de estaciones biométricas sin una decisión de seguridad.

> Advertencia de control: `Implementado` en la matriz significa que existe una vista web utilizable para la capacidad indicada. No equivale a paridad funcional, operativa ni visual con WinForms; esa afirmación requiere la ficha de paridad y las pruebas de la Fase 7.

### Evidencia de comprobación

- Fuentes comparadas: `web/memory/PRD.md`, `web/docs/REQUISITOS_COMEDOR.md`, `escritorio/SCSC/FrmPrincipal.vb`, `escritorio/SCSC/Clases/UIShellHost.vb` y `escritorio/SCSC/Clases/SeguridadPermisosSistema.vb`.
- Comando: inspección de los cinco grupos de `DropDownItems`/`Text` en `FrmPrincipal.Designer.vb` (resultado comprobado: 15 destinos legacy: 4 Mantenimiento, 5 Utilitarios, 4 Reportes, Ayuda e Imprimir).
- Comando: `rg -n 'New NavItem With' escritorio/SCSC/Clases/UIShellHost.vb` (resultado comprobado: 15 entradas del shell moderno; incluye Seguridad y no incluye Imprimir).
- Comando: `rg -n 'ADMIN_NAVIGATION|getVisibleAdminModules' web/frontend/src` para verificar el catálogo único de vistas administrativas web.
- Resultado: matriz publicada y enlazada desde este plan; no se modificaron frontend ni backend.

## 5. Fase 1: fuente única de navegación

**Estado: Completada, 2026-08-25.** Se creó el catálogo único, se conectó al shell responsive y se eliminaron las fuentes de navegación duplicadas.

**Corrección responsive adicional (2026-08-25):** la pantalla de cambio de PIN estudiantil ahora usa una cuadrícula fluida de seis columnas (`min-w-0`, `w-full`) y padding adaptativo (`p-4`/`sm:p-6`), evitando desbordamiento en teléfonos de 320–375 px sin reducir el objetivo táctil por debajo de 44 px.

### Tareas

- [x] Crear `web/frontend/src/config/adminNavigation.js`.
- [x] Registrar `id`, `label`, `shortLabel`, `path`, `group`, `icon`, `requiredPermissions`, `adminOnly` y componente lazy.
- [x] Crear `getVisibleModules(session)`, `getActiveGroup(pathname)` y `getDefaultRoute(session)`.
- [x] Sustituir el arreglo `TABS` por el catálogo `ADMIN_NAVIGATION` como fuente única, sin exports de compatibilidad.
- [x] Añadir pruebas para Administrador, Operador, catálogo de permisos y rutas fuera del catálogo.

### Referencias y APIs permitidas

- `lazy` y `Suspense` desde [AdminPanel.jsx](../frontend/src/pages/AdminPanel.jsx).
- `NavLink`, `Routes`, `Route`, `Navigate`, `Outlet`, `useLocation` y `useNavigate` desde la versión instalada de React Router.
- Pruebas DOM con `createRoot`, `act` y eventos nativos, siguiendo las pruebas del shell administrativo.

### Evidencia de comprobación

- `rg` confirma 10 entradas de navegación, cinco grupos y diez destinos lazy existentes.
- `AdminPanel.test.js` verifica 10 módulos para Administrador, 9 para Operador, los metadatos `requiredPermissions` y el rechazo de rutas fuera del catálogo.
- `git diff --check` no reporta errores en los archivos de la fase.
- `npm test -- --reporter=dot`: 44 pruebas pasan en 11 archivos.

### Verificación y guardas

- [x] Móvil y escritorio leerán el mismo catálogo; la integración actual ya consume `ADMIN_NAVIGATION` mediante `getVisibleAdminModules`.
- [x] No se codifican permisos directamente en cada componente visual; `requiredPermissions` queda como metadato para la Fase 5.
- [x] La sección activa se resuelve desde la URL y no únicamente desde `useState`.

## 6. Fase 2: rutas reales

**Estado: Completada, 2026-08-25.** El layout y las rutas anidadas están implementados; las rutas profundas y el fallback SPA responden correctamente en Vite y Nginx.

### Tareas

- [x] Convertir `AdminPanel` en layout administrativo.
- [x] Renderizar los módulos mediante `Outlet`.
- [x] Añadir rutas anidadas bajo `/admin/panel`.
- [x] Redirigir `/admin/panel` a `/admin/panel/inicio`.
- [x] Mantener `AdminLogin` y redirecciones literales a `/admin/panel` como entrada compatible; el índice redirige a Inicio.
- [x] Mantener la protección de sesión y CSRF sin cambios.
- [x] Definir salida controlada para rutas desconocidas y no autorizadas (fallback a Inicio y vista 403 para módulo restringido).

### Referencias

- [App.jsx](../frontend/src/App.jsx:17)
- [ProtectedRoute.jsx](../frontend/src/components/ProtectedRoute.jsx:5)
- [Contratos de la API](CONTRATOS_API.md)
- [default.conf](../ops/nginx/default.conf:46)

### Verificación y guardas

- [x] Recargar una ruta profunda conserva el módulo.
- [x] Las rutas SPA funcionan detrás de Nginx.
- [ ] Atrás y Adelante cambian de módulo correctamente (pendiente de prueba interactiva con navegador).
- [x] No se inventan APIs de React Router ni se cambian las rutas `/api`.

### Evidencia de implementación

- `App.jsx` contiene las rutas de módulo bajo el layout protegido `/admin/panel` y una redirección por defecto a `inicio`.
- `AdminModule.jsx` resuelve el componente lazy desde el catálogo y lo renderiza dentro de `Suspense`.
- El parser local analiza `App.jsx`, `AdminPanel.jsx` y los dos componentes de navegación sin errores.
- La validación en tiempo de ejecución ya no depende de Craco/Jest; se ejecuta mediante Vitest.


## 7. Fase 3: shell responsive

**Estado: Completada en código, 2026-08-25.** Se implementaron `AdminSidebar` expandido para escritorio y `AdminBottomNav` con `Drawer` para móvil usando el catálogo común y los tokens visuales del proyecto. El ajuste visual posterior mejoró jerarquía, selección activa, contención horizontal y legibilidad sin cambiar el tema institucional.

### Móvil y tablet

- [x] Crear `AdminBottomNav` con cinco destinos.
- [x] Mantener `aria-label`, `aria-current`, foco visible y área táctil mínima de 44 px.
- [x] Añadir `safe-area-inset-bottom`.
- [x] Añadir `pb` suficiente al contenido para que la barra no tape formularios.
- [x] Implementar `Drawer` controlado para **Más**.

### Escritorio

- [x] Mantener sidebar lateral expandido desde `lg` con 256 px, según decisión visual aprobada.
- [x] Mostrar grupos y enlaces autorizados.
- [x] Mantener encabezado con nombre, rol y cierre de sesión.
- [x] Eliminar el límite global de 1024 px y usar ancho fluido para tablas y dashboards.

### Referencias

- [AdminBottomNav.jsx](../frontend/src/compartido/componentes/AdminBottomNav.jsx:1)
- [drawer.jsx](../frontend/src/components/ui/drawer.jsx:6)
- [design_guidelines.json](../design_guidelines.json:89)

### Verificación y guardas

- [x] Probar 320, 375, 390, 768, 1024, 1111, 1280, 1440 y 1920 px sobre el contenedor web desplegado.
- [x] Probar orientación vertical y horizontal en 812 × 375 y 844 × 390.
- [x] Eliminar la fila `TabsList` con `overflow-x-auto` como navegación principal.
- [x] No crear un componente Sidebar externo que no existe en el repositorio.

### Evidencia de implementación

- `AdminSidebar.jsx` usa los cinco grupos del catálogo, se muestra expandido desde `lg`, separa mejor cada grupo y marca el módulo activo con superficie, icono e indicador lateral.
- `AdminBottomNav.jsx` ofrece cuatro destinos principales más **Más**, `Drawer` controlado, safe-area y áreas táctiles mínimas de 48 px.
- `AdminPanel.jsx` elimina `TabsList` como navegación principal, añade `Outlet`, reserva espacio inferior en móvil, incorpora salto al contenido y ocupa el ancho disponible sin `max-width` global.
- `PlantillasTab.jsx` usa una grilla fluida de una, dos o tres columnas; las tarjetas tienen `min-w-0`, títulos con ajuste seguro, acciones de 44 px y estados visibles de carga, vacío y error con reintento.
- Verificación Chromium automatizada: en los once tamaños evaluados `scrollWidth` coincide con el ancho del viewport; el sidebar permanece expandido desde 1024 px y la navegación inferior se usa por debajo de `lg`.

## 8. Fase 4: integración de módulos actuales

**Estado: Completada, 2026-08-25.** Los diez módulos lazy están conectados a sus rutas, los hubs de Operación/Personas/Reportes son navegables, el layout muestra grupo y módulo activos y el shell fluido conserva el menú mobile.

### Tareas

- [x] Conectar Dashboard a Inicio.
- [x] Conectar Menú, Calendario, Sustituciones, Rutas y Correcciones a Operación.
- [x] Conectar Estudiantes/PIN a Personas.
- [x] Conectar Registro de transporte a Reportes.
- [x] Conectar Parámetros del portal y Auditoría a Más.
- [x] Añadir breadcrumbs y título de sección.
- [ ] Revisar tablas, formularios, estados vacíos, carga y error en cada viewport.
- [x] Mantener carga diferida para no aumentar innecesariamente el bundle inicial.

### Verificación

- [x] Pasan las pruebas existentes de Dashboard, Rutas, Parámetros, Auditoría y AdminPanel.
- [x] Plantillas incluye cobertura para títulos extensos, contención responsive, error visible y reintento.
- [ ] Smoke test de los diez módulos con cuentas autorizadas en staging.
- [x] No se modifican contratos de API sin regenerar y verificar [Contratos de la API](CONTRATOS_API.md).

### Evidencia de implementación

- `AdminGroupHub.jsx` implementa accesos agrupados para Operación, Personas y Reportes.
- `AdminPanel.jsx` muestra el grupo y título del módulo activo a partir de la URL.
- `AdminModule.jsx` devuelve una vista 403 para el acceso directo de un operador a Correcciones.
- Verificación estática: 10/10 módulos del catálogo tienen ruta explícita y el parser Babel no reporta errores.

## 9. Fase 5: permisos RBAC

**Estado: Completada, 2026-08-25.** Se conectó la sesión administrativa con `Seguridad.UsuarioRol`, `Seguridad.RolPermiso` y `Seguridad.Permiso`. El frontend consume `permisos[]` y todos los endpoints administrativos actuales pasan por `require_permission(...)`. La validación contra SQL Server institucional queda como actividad operativa de despliegue, no como pendiente de código.

Esta fase debe completarse antes de migrar módulos administrativos sensibles.

### Backend

- [x] Leer roles y permisos desde `Seguridad.UsuarioRol`, `Seguridad.RolPermiso` y `Seguridad.Permiso`.
- [x] Exponer sesión y permisos mediante `/api/v1/sesion`.
- [x] Crear una dependencia `require_permission(...)`.
- [x] Aplicar permisos por familia de endpoint en parámetros, rutas, estudiantes, menú, calendario, dashboard, reportes, correcciones y auditoría.
- [ ] Definir permisos nuevos para correcciones, auditoría, menú, PIN, fotografía y beneficios cuando no exista una clave equivalente.

### Frontend

- [x] Conservar `roles[]` y `permisos[]` en `AuthContext`.
- [x] Añadir filtrado de permisos en `getVisibleAdminModules(...)`.
- [x] Filtrar navegación desde la configuración central.
- [x] Proteger también el acceso directo por URL mediante `AdminModule` y `require_permission`.

### Referencias

La implementación canónica se documenta en `web/backend/aplicacion/modulos` y
`web/frontend/src/funcionalidades`. Los artefactos históricos del sistema local no
forman parte de la entrada web ni se enlazan desde el código activo.

### Verificación y guardas

- [x] Probar `403`, Administrador, Operador autorizado y Operador no autorizado mediante pruebas backend/frontend; `401` queda para smoke con sesión expirada en staging.
- [x] No confiar en ocultar botones como protección de seguridad.
- [ ] Mantener Administrador como superusuario solo si la matriz institucional lo aprueba.

### Evidencia actual

- La persistencia de asignaciones institucionales se realiza en el repositorio del módulo de identidad.
- La sesión administrativa devuelve `roles` y `permisos` mediante el contrato canónico de identidad.
- `AdminPanel.test.js`: 6 pruebas de catálogo pasan, incluida la visibilidad de un Operador con `rutas.administrar`.
- `StudentPortal.jsx` mantiene el temporizador activo después de confirmar para permitir cancelar hasta el cierre; sus 12 pruebas de transición pasan con Vitest.
- `test_rbac_dependencies.py`: cuatro casos RBAC pasan; la dependencia conserva acceso de Administrador y rechaza Operadores sin permiso.
- La matriz de permisos se verifica sobre los endpoints modulares mediante pruebas de autorización.

## 10. Fase 6: pruebas y despliegue gradual

### Automatización

- [x] Ejecutar `npm test`.
- [x] Ejecutar `npm run build`.
- [x] Ejecutar pruebas backend con `pytest`.
- [x] Añadir pruebas con `MemoryRouter` para URL inicial, navegación, Atrás/Adelante y permisos.

### Validación manual

- [ ] Chrome y Edge en escritorio.
- [ ] Android y iPhone reales.
- [ ] Login, logout, sesión expirada y redirección.
- [ ] Formularios, tablas, drawers, descargas y estados de error.
- [ ] Validación en staging antes de producción.

### Puerta de despliegue

No promover a producción sin evidencia de build, pruebas, smoke test, respaldo y aprobación de staging según [DESPLIEGUE_PORTAL.md](DESPLIEGUE_PORTAL.md).

### Evidencia actual

- `npm run build`: ✅ Vite 8 transforma 2.389 módulos y genera `dist/`.
- `npm test`: la suite se ejecuta con Vitest; la validación de staging y multidispositivo permanece pendiente.
- `/tmp/scb-web-venv/bin/pytest -q`: 50/50 casos backend pasan.
- Verificación HTTP local: Vite responde con `Comedor SCSC` y carga `/src/index.jsx`.
- Verificación HTTP de ruta profunda: `/admin/panel/inicio` responde `200` con el shell SPA.
- La automatización Playwright no pudo iniciar dentro del entorno porque `npx playwright` queda esperando la resolución del CLI; se conserva como validación manual requerida para dispositivos reales.
- `npm run start -- --host 127.0.0.1 --port 4174`: ✅ Vite listo; el binding requiere permisos de red del entorno administrado.
- Validación smoke HTTP (2026-08-25): ✅ `/` y `/admin/panel/inicio` responden `200`; el documento incluye el título `Comedor SCSC`, la entrada `/src/index.jsx` y el fallback SPA.
- Validación Docker local (2026-08-25): ✅ `docker compose config`; contenedores `api` y `web` saludables; `/api/health` y `/api/ready` responden `200`. La ruta pública correcta incluye el prefijo `/api`.
- Imagen Docker Vite (2026-08-25): ✅ `docker compose build` completó API y web; la imagen web servida temporalmente respondió `200` en `/` y `/admin/panel/inicio` con assets presentes.
- Puerto 8081 (2026-08-25): ✅ se confirmó que pertenecía al stack `scsc-comedor-local` y servía el bundle CRA anterior (`/static/js/main...`). Se detuvo ese stack sin borrar volúmenes y se levantó el stack Vite actualizado; `/`, `/admin/panel/inicio`, `/api/health` y `/api/ready` responden `200`.
- Incidente Operador/profesor (2026-08-25): ✅ corregido manteniendo RBAC estricto. Si `permisos[]` está vacío, el frontend no muestra módulos y `AdminModule` rechaza también el acceso directo por URL; la API devuelve 403 mediante `require_permission`. Cobertura backend: 50/50.
- Validación de navegador: ⚠️ el wrapper Playwright no pudo resolver `@playwright/cli` dentro del entorno (timeout de `npx`); no se inventan resultados visuales. Requiere ejecutar la misma secuencia en un entorno con el CLI/browser instalado.

## 11. Fase 7: incorporación a la migración total

**Estado: sustituida como plan arquitectónico por el [Plan de migración total a la plataforma web](PLAN_MIGRACION_TOTAL_WEB.md).** Este apartado conserva el inventario funcional del menú administrativo. La migración ya no se plantea como convivencia progresiva con WinForms: cada capacidad se implementará como corte vertical completo y el sistema local se retirará mediante un único corte auditable.

### Oleada 1: Personas

- [ ] Expediente completo.
- [ ] Alta manual de estudiante.
- [ ] Catálogo y mantenimiento de becas.

### Oleada 2: Reportes

- [ ] Servicio de comedor.
- [ ] Servicio de transporte.
- [ ] Proyección de comedor.
- [ ] Estudiantes becados.
- [ ] Exportación CSV/Excel.

### Oleada 3: Administración

- [ ] Usuarios.
- [ ] Roles y permisos.
- [ ] Ayuda y soporte.

### Oleada 4: Operaciones

- [ ] Recargas y saldos.
- [ ] Importación PIAD/Excel.
- [ ] Auditoría de importación.

### Oleada 5: hardware

- [ ] Control de comedor.
- [ ] Control de transporte.
- [ ] Sustitución de DigitalPersona por QR, código de barras y PIN web, sin integración en tiempo de ejecución.
- [ ] Impresión masiva.

Cada módulo debe tener una ficha de paridad con: flujo local, flujo web, datos afectados, permisos, pruebas, dependencia de hardware, plan de reversión y aprobación del usuario operativo.

## 12. Modernización del toolchain frontend

**Estado: Completada, 2026-08-25.** Se sustituyeron Create React App, Craco, `react-scripts` y Jest por Vite 8, React 19, Vitest 4 y el plugin oficial de React para Vite.

### Cambios realizados

- `npm start` usa Vite con host local explícito y proxy `/api` configurable mediante `API_PROXY_TARGET`.
- `npm run build` usa `vite build` y genera `dist/`.
- `npm test` usa `vitest run` con entorno `jsdom`.
- Se creó `vite.config.mjs`, `index.html` raíz y `src/testSetup.js`.
- Se renombraron entradas JSX a `.jsx` para evitar parsers ambiguos.
- `src/compartido/consultas/cliente_http.ts` configura el cliente HTTP relativo y sus interceptores.
- Se eliminó `craco.config.js`, `public/index.html`, `react-scripts` y `@craco/craco`.
- Node requerido: `24.19.0` LTS, con npm `12.0.2`, según `.nvmrc` y `package.json`.

### Evidencia

- `npm ls react-scripts @craco/craco --depth=0`: árbol vacío.
- `npm run build`: completado correctamente; 2.389 módulos transformados.
- `npm run start -- --host 127.0.0.1 --port 4174`: Vite listo en 2.98 s con permisos de red aprobados.
- `npm test`: 44 pruebas pasan en 11 archivos.

## 13. Criterios globales de aceptación

- [x] La navegación móvil no depende de desplazamiento horizontal.
- [x] La navegación de escritorio permite acceder a todos los módulos autorizados.
- [x] Las dos interfaces usan la misma arquitectura de grupos y rutas.
- [x] Una ruta profunda se puede recargar y compartir.
- [ ] El botón Atrás funciona (pendiente de prueba interactiva en navegador).
- [x] La API continúa siendo la autoridad de permisos.
- [x] No se almacenan sesiones, JWT ni credenciales en `localStorage`.
- [x] Se conservan cookies HttpOnly, CSRF y rutas relativas `/api`.
- [ ] Cada botón, entrada y elemento dinámico clave tiene `data-testid`.
- [ ] Se prueban estados de carga, vacío, error y falta de permisos.

## 14. Registro de decisiones, riesgos y evidencias

### Decisiones tomadas

| ID | Decisión | Responsable | Estado |
| --- | --- | --- | --- |
| D-01 | URL sin sesión redirige a login; sesión sin permiso muestra pantalla 403 | Equipo del proyecto | Tomada, 2026-08-24 |
| D-06 | Adoptar monolito modular por dominios, nombres españoles y migración única sin ejecución heredada | Equipo del proyecto | Tomada, 2026-08-25; [ADR-0001](decisiones/0001-monolito-modular-por-dominios.md) |

### Decisiones pendientes

| ID | Decisión | Responsable | Estado |
| --- | --- | --- | --- |
| D-02 | Matriz web de permisos finos | Por asignar | Pendiente |
| D-03 | Estrategia para DigitalPersona | Equipo del proyecto | Resuelta: sustitución por QR, código de barras y PIN web; sin integración en tiempo de ejecución |
| D-04 | Paridad de reportes Crystal | Por asignar | Pendiente |
| D-05 | Política de formularios con cambios sin guardar | Por asignar | Pendiente |

### Riesgos

| Riesgo | Mitigación |
| --- | --- |
| El menú oculta funciones pero la API aún permite acceso directo | Completar Fase 5 antes de migrar módulos sensibles |
| El cambio de tabs a rutas desmonta formularios | Auditar formularios y definir guardado/advertencia antes de Fase 2 |
| Reportes web no tienen paridad con Crystal | Fichas de paridad y validación con usuarios de reportes |
| DigitalPersona no funciona directamente en navegador móvil | Decisión específica de hardware antes de Oleada 5 |
| PRD y código quedan desalineados | Actualizar el PRD en Fase 0 |

### Registro de evidencias

| Fecha | Fase | Evidencia/enlace | Resultado | Responsable |
| --- | --- | --- | --- | --- |
| 2026-08-24 | 0 | [PRD.md](../memory/PRD.md), [MATRIZ_MIGRACION_MENU_ADMIN.md](MATRIZ_MIGRACION_MENU_ADMIN.md), fuentes WinForms y registro de verificación de la [Fase 0](#4-fase-0-alcance-y-matriz-funcional) | Cierre documental completado: alcance, matriz, cinco destinos inferiores y guardas verificadas; Fase 1 no iniciada | Codex |
| 2026-08-25 | Arquitectura | [Plan de migración total](PLAN_MIGRACION_TOTAL_WEB.md), [arquitectura](ARQUITECTURA.md), [convenciones](CONVENCIONES_NOMBRES.md) y [ADR-0001](decisiones/0001-monolito-modular-por-dominios.md) | Fase 0 documental de la estandarización completada; implementación técnica aún no iniciada | Codex |
| 2026-08-24 | 1–4 | `node` + parser local, `git diff --check`, inspección estática de `App.jsx` y catálogo | 7 archivos JSX/JS analizados, 10 módulos con rutas, 3 hubs, fallback global y sin referencias `TabsList`/`TabsContent` en el layout | Codex |
