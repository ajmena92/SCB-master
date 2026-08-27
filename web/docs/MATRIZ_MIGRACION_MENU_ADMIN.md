# Matriz de migración del menú administrativo

Esta matriz es el inventario de control de la Fase 0. El estado describe la cobertura disponible en la web al 2026-08-24; una ruta objetivo no implica que el módulo ya exista ni que haya paridad con WinForms.

## Criterios de estado

| Estado | Significado |
| --- | --- |
| Implementado | Existe una vista web utilizable para la capacidad indicada, aunque debe validarse en staging; no significa paridad con WinForms. |
| Parcial | La web cubre una parte de la capacidad o la cubre con otro flujo; requiere cerrar brechas antes de declarar paridad. |
| No iniciado | No hay vista web equivalente identificada. |
| Pendiente de decisión | Requiere una decisión de hardware, seguridad o alcance antes de definir una ruta final. |

## Destinos de navegación propuestos

Las rutas se agrupan en cinco destinos para el shell responsive. En móvil se muestran como Inicio, Operación, Personas, Reportes y Más; en escritorio se presentan como grupos del menú lateral.

| Grupo | Destino | Ruta base |
| --- | --- | --- |
| Inicio | Dashboard | `/admin/panel/inicio` |
| Operación | Menú, calendario, sustituciones, rutas y correcciones | `/admin/panel/operacion` |
| Personas | Estudiantes y PIN | `/admin/panel/personas` |
| Reportes | Reportes y registro de transporte | `/admin/panel/reportes` |
| Más | Parámetros, auditoría, seguridad, importación, recargas, ayuda e impresión | `/admin/panel/mas` |

## Las 10 vistas web administrativas actuales

Este inventario refleja exactamente las diez entradas del arreglo `TABS` de `web/frontend/src/pages/AdminPanel.jsx`. El estado conserva el significado de la matriz: `Implementado` indica que existe la vista web, mientras que `Parcial` indica que aún no existe paridad funcional completa con WinForms.

| # | Vista web actual | Grupo | Ruta objetivo | Estado web |
| --- | --- | --- | --- | --- |
| 1 | Dashboard | Inicio | `/admin/panel/inicio` | Implementado |
| 2 | Menú | Operación | `/admin/panel/operacion/menu` | Implementado |
| 3 | Calendario | Operación | `/admin/panel/operacion/calendario` | Implementado |
| 4 | Sustituciones | Operación | `/admin/panel/operacion/sustituciones` | Implementado |
| 5 | Estudiantes/PIN | Personas | `/admin/panel/personas/estudiantes` | Implementado |
| 6 | Rutas | Operación | `/admin/panel/operacion/rutas` | Implementado |
| 7 | Correcciones | Operación | `/admin/panel/operacion/correcciones` | Implementado |
| 8 | Parámetros | Más | `/admin/panel/mas/parametros` | Implementado |
| 9 | Auditoría | Más | `/admin/panel/mas/auditoria` | Implementado |
| 10 | Registro de transporte | Reportes | `/admin/panel/reportes/transporte` | Parcial |

## Inventario escritorio → web

El conteo se interpreta así: el menú legacy de `FrmPrincipal.Designer.vb` contiene 15 destinos y el shell moderno de `UIShellHost.vb` define los mismos 15 `NavItem`. El inventario consolidado de esta sección tiene 17 filas porque añade `Dashboard` (panel transversal) e `Imprimir` (capacidad identificada fuera de esos 15 destinos). Las 10 vistas web actuales son otro conteo: corresponden exclusivamente a las entradas del arreglo `TABS` de `AdminPanel.jsx`.

Las claves de permiso son las definidas por `SeguridadPermisosSistema`. `No definido en web` es intencional: el backend web todavía no consume la matriz RBAC de escritorio y debe resolverse en la Fase 5.

| Módulo/título en escritorio | Clave | Grupo web | Ruta objetivo | Estado web | Permiso escritorio | Permiso web actual | Dependencia o brecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Dashboard / panel principal | `dashboard` | Inicio | `/admin/panel/inicio` | Implementado | Sin clave; controles de dashboard permitidos | Rol administrativo; RBAC detallado pendiente | Validar métricas contra SQL Server y permisos del dashboard. |
| Estudiantes | `estudiantes` | Personas | `/admin/panel/personas/estudiantes` | Implementado | `Modulos.Estudiantes.Acceso` | Rol administrativo; RBAC detallado pendiente | Alinear expediente, PIN, fotografía, ruta y beneficio. |
| Gestión Rutas | `rutas` | Operación | `/admin/panel/operacion/rutas` | Implementado | `Modulos.Rutas.Acceso` | Rol administrativo; RBAC detallado pendiente | Validar paridad de altas, edición y asignación. |
| Gestión Becas | `becas` | Personas | `/admin/panel/personas/becas` | No iniciado | `Modulos.Becas.Acceso` | No definido en web | Crear módulo y definir catálogo/flujo de beneficios. |
| Parámetros Sistema | `parametros` | Más | `/admin/panel/mas/parametros` | Implementado | `Configuracion.Modificar`; además excepción de usuarios `admin`/`amenaa` | Operador y Administrador según el flujo web; RBAC detallado pendiente | Sustituir excepción nominal por autorización institucional documentada. |
| Control de Marcas Comedor | `comedor` | Operación | `/admin/panel/operacion/comedor` | Parcial | `Modulos.Comedor.Acceso` (el shell legacy lo permite sin login) | No definido en web | El portal registra confirmaciones estudiantiles, pero no equivale aún al control operativo de marcas. |
| Control de Marcas Transporte | `transporte` | Operación | `/admin/panel/operacion/transporte` | No iniciado | `Modulos.Transporte.Acceso` (el shell legacy lo permite sin login) | No definido en web | Depende de hardware/estación y de la decisión de migración del control de marcas. |
| Importar Datos PIAD | `importacion` | Más | `/admin/panel/mas/importacion` | No iniciado | `Modulos.Importacion.Acceso` | No definido en web | Definir validación, carga segura, reversión y formatos soportados. |
| Agregar Estudiante Manual | `agregar_estudiante` | Personas | `/admin/panel/personas/estudiantes/nuevo` | Parcial | `Modulos.AgregarEstudiante.Acceso` + `Modulos.Estudiantes.Acceso` | No definido en web | La consulta/gestión web no demuestra todavía alta manual completa. |
| Recargas | `recargas` | Más | `/admin/panel/mas/recargas` | No iniciado | `Modulos.Recargas.Acceso` | No definido en web | Requiere especificar operación, conciliación y auditoría. |
| Reporte Estudiantes Becados | `reporte_becados` | Reportes | `/admin/panel/reportes/becados` | No iniciado | `Reportes.Becados.Ver` + `Reportes.Ver` | No definido en web | Migrar salida y filtros; validar contra reporte Crystal/WinForms. |
| Reporte Servicio Comedor | `reporte_comedor` | Reportes | `/admin/panel/reportes/comedor` | Parcial | `Reportes.Comedor.Ver` + `Reportes.Ver` | No definido en web | Dashboard y nominal no sustituyen aún el reporte completo/exportable. |
| Reporte de Servicio Transporte | `reporte_transporte` | Reportes | `/admin/panel/reportes/transporte` | Parcial | `Reportes.Transporte.Ver` + `Reportes.Ver` | No definido en web | La vista actual se llama “Reporte WinForms”; renombrar conceptualmente hasta comprobar paridad. |
| Reporte Proyección Comedor | `reporte_proyeccion` | Reportes | `/admin/panel/reportes/proyeccion` | No iniciado | `Reportes.Proyeccion.Ver` + `Reportes.Ver` | No definido en web | Migrar cálculo, filtros y exportación con validación de datos. |
| Ayuda | `ayuda` | Más | `/admin/panel/mas/ayuda` | No iniciado | Sin permiso; permitido por el shell | No definido en web | Publicar ayuda operativa y enlaces de soporte. |
| Imprimir | `imprimir` | Más | `/admin/panel/mas/imprimir` | No iniciado | `Carnets.Imprimir` | No definido en web | Requiere definir impresión web/PDF y compatibilidad con impresoras locales. |
| Seguridad - Roles y permisos | `seguridad` | Más | `/admin/panel/mas/seguridad` | No iniciado | `Seguridad.Ver` y permisos de usuarios/roles/permisos | No definido en web | Es requisito de la Fase 5 antes de exponer administración sensible. |

## Funciones web actuales que no tienen hoja equivalente en WinForms

La web ya contiene capacidades propias del portal que se deben conservar al migrar: confirmación/cancelación de asistencia del estudiante, cambio de PIN web, menú por fecha, sustituciones, correcciones auditadas y parámetros de cierre. Se consideran parte del dominio comedor y no deben perderse al reorganizar el menú.

## Decisión de acceso para las rutas (D-01)

- Sin sesión: `401` en API y redirección a `/admin` en la interfaz.
- Con sesión pero sin permiso: respuesta `403` y pantalla de acceso denegado; no redirigir silenciosamente a Inicio, para que el usuario distinga una ruta inexistente de una restricción de permisos.
- La navegación oculta destinos no autorizados, pero la API debe volver a comprobar el permiso.
- No se trasladan a Internet las excepciones `comedor`/`transporte` del shell legacy sin una revisión de seguridad.

## Vacíos de la Fase 0

1. Confirmar con las áreas usuarias la agrupación final de Becas, Seguridad e Importación.
2. Definir nombres y permisos web oficiales; esta matriz usa temporalmente las claves de escritorio.
3. Decidir el modelo para lectores biométricos, impresión local y control de marcas.
4. Obtener evidencia de staging para cambiar `Parcial` a `Implementado`.
