# Auditoría Fase 6 — Administración: Dashboard y Personas

**Fecha:** 2026-09-06  
**Estado:** completado técnicamente; pendiente la aprobación operativa de Fase 6  
**Alcance:** `Dashboard.jsx`, `DashboardGraficos.jsx`, `ListadoPersonas.tsx` y sus controles de filtro.  
**Criterio institucional:** confianza, rapidez y calma para una institución pública de educación técnica; operación equivalente en escritorio y móvil, sin ruido visual ni pérdida de información.

## Resultado

| Dimensión | Resultado | Observación |
| --- | ---: | --- |
| Accesibilidad | 4/4 | Gráficos con descripción y alternativa textual; filtros y acciones principales alcanzan 44 px. |
| Rendimiento | 4/4 | Paginación de 25 elementos y React Query se conservan; no se duplica la representación móvil. |
| Tema | 4/4 | Tokens de superficie y color verificados en claro y oscuro con datos de integración. |
| Responsive | 4/4 | Tablas desde `md`; tarjetas equivalentes en 390 px, sin overflow horizontal. |
| Antipatrones | 4/4 | Acciones y datos equivalentes por tamaño de pantalla; no se añadieron variantes locales. |
| **Total** | **20/20** | Apto técnicamente para cerrar este módulo. |

## Hallazgos priorizados

### Resuelto — Tabla y tarjetas duplicadas en Personas móvil

- **Ubicación:** `web/frontend/src/funcionalidades/plataforma/componentes/ListadoPersonas.tsx`.
- La tabla con `min-w-[64rem]` sigue renderizándose debajo de `md`, mientras las tarjetas ya usan `md:hidden`.
- **Impacto:** duplicación de contenido, scroll horizontal accidental, mayor coste de render y pérdida de calma operativa.
- **Aplicación:** tabla bajo `md` (`hidden md:block`) y tarjetas como única representación móvil.

### Resuelto — Padrón nominal del Dashboard no cumple presentación móvil

- **Ubicación:** `web/frontend/src/funcionalidades/administracion/paginas/Dashboard.jsx`.
- El padrón nominal depende de una tabla horizontal con `overflow-x-auto`, sin alternativa apilada.
- **Impacto:** incumple el contrato responsive y dificulta consultar personas desde teléfono.
- **Aplicación:** tabla visible desde `md`; tarjetas compactas y equivalentes debajo de `md`.

### Resuelto — Gráficos sin alternativa textual

- **Ubicación:** `Dashboard.jsx` y `DashboardGraficos.jsx`.
- Los datos de Recharts se comunican solo mediante SVG y tooltip.
- **Aplicación:** descripción accesible y detalle plegable con valores, período y unidades.

### Resuelto — Objetivos táctiles inferiores al contrato

- **Ubicación:** filtros y acciones del Dashboard.
- Varios controles `h-10` miden 40 px; el estándar requiere al menos 44 px.
- **Aplicación:** controles `h-11`, incluido el botón de limpiar búsqueda.

### Resuelto — Año fijado en la interfaz

- **Ubicación:** Dashboard, texto “Padrón activo 2026”.
- **Aplicación:** año derivado de la fecha de consulta.

## Evidencia de cierre

- `npm run typecheck`: aprobado.
- `npm run verificar:diseno`: aprobado, 264 archivos verificados.
- `npm run lint` y `npm run build`: aprobados.
- `npm test -- --run`: 39 archivos y 107 pruebas aprobadas.
- Chromium con la copia local de integración: Dashboard en 390, 768 y 1440 px; claro y oscuro; sin overflow horizontal. Personas en 390 px carga las tarjetas y oculta su tabla; en escritorio conserva tabla.
- Estados observados: carga de módulo, carga de consulta, vacío de gráficos sin datos históricos y contenido exitoso. La simulación aislada de error HTTP no invalidó la caché de React Query en esta sesión; ese recorrido se repetirá como E2E dedicado al auditar el módulo de expedientes.

## Ajuste posterior — capacidad estimada del servicio

- Se retiró la gráfica de distribución por sección del Dashboard: duplicaba un
  filtro existente y no ofrecía una decisión operativa inmediata.
- Se incorporó la tarjeta **Capacidad estimada**: estudiantes activos del
  padrón, confirmaciones válidas y asistencia registrada de la fecha consultada.
- `confirmados` incluye reservas `reservada` y `consumida`; excluye
  `cancelada`. `pendientes` equivale a confirmados sin ingreso registrado.
- Todas las métricas y gráficas parten del padrón activo: excluyen personas
  inactivas y matrículas no activas, aun cuando exista una reserva histórica.
- El payload `porSeccion` y su cálculo fueron eliminados del endpoint del
  Dashboard, al no tener consumidores restantes.
- Evidencia: prueba contractual backend aprobada, `ruff`, typecheck, estándar
  visual, lint, build y comprobación Chromium móvil sin overflow.

## Ajuste posterior — claridad de lista nominal

- La columna genérica **Estado** pasó a llamarse **Asistencia hoy**.
- Las etiquetas operativas son “Ingresó al comedor”, “Aún sin ingreso” y,
  cuando aplica, “Registro histórico”.
- El filtro elimina opciones que la API no producía (`ausente` y `tardanza`) y
  usa la misma terminología de la columna.
- El filtro de sección dejó de ser texto libre y ofrece las secciones cerradas
  de 7-1 a 12-3.
- Validación: typecheck y estándar visual aprobados.

## Ajuste posterior — exportación de lista de control

- La exportación se ubica junto a la lista nominal y exige seleccionar un único
  servicio: **Comedor** o **Transporte**. No combina beneficios ni estados de
  ambos en un mismo archivo.
- Excel, CSV y la vista **Imprimir / PDF** usan el mismo endpoint y todos los
  filtros activos: fecha, búsqueda, ruta, sección, asistencia y asignación de
  transporte. La exportación contiene todas las coincidencias, no las 25 filas
  de la página visible.
- La generación ocurre en API: CSV usa UTF-8 BOM y neutraliza fórmulas de
  Excel; XLSX incorpora encabezado, autofiltro y congelamiento de columnas; la
  vista PDF es clara e imprimible independientemente del tema del navegador.
- Cada solicitud crea una traza con cuenta, servicio, formato, fecha, filtros
  no sensibles y total. La búsqueda se excluye expresamente de la auditoría
  porque puede contener identificación o nombre.
- Se añadió la migración Alembic `0021_auditoria_exportacion_lista_control`.
- Evidencia backend: `pruebas_postgresql/test_portal_dashboard.py` aprobó 5
  pruebas, incluyendo los tres formatos, filtros, separación de servicios,
  neutralización de fórmulas y traza de exportación. Ruff aprobó.

## Ajuste posterior — secciones del padrón activo

- Se eliminó la lista fija de secciones del Dashboard. El endpoint devuelve las
  secciones reales de matrículas activas para el año de la fecha consultada,
  agrupadas de Séptimo a Duodécimo y ordenadas numéricamente.
- La importación normaliza `8 - 5` como `8-5` y acepta cualquier grupo numérico
  positivo, incluido `7-9`; rechaza valores ambiguos como `8-A`.
- No se creó una tabla adicional: matrícula anual es la autoridad de la
  sección. Esto evita sincronización duplicada durante importación, traslados o
  desactivaciones.
