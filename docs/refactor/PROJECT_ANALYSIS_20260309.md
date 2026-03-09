# Analisis Actual del Proyecto SCB

Fecha: 2026-03-09

## Resumen ejecutivo
SCB es una aplicacion WinForms VB.NET sobre .NET Framework 4.6.1 con una base legacy centrada en formularios y un esfuerzo reciente de refactor para extraer servicios, endurecer el tipado y estabilizar el Designer. El proyecto ya no esta en estado puramente monolitico de eventos, pero tampoco tiene una separacion completa entre UI, reglas de negocio y acceso a datos.

El estado general es de sistema operativo-productivo con deuda tecnica acumulada. El mayor riesgo pendiente ya no es solo visual o de Designer; ahora es de configuracion, seguridad operativa y mantenibilidad estructural.

Nota de actualizacion del mismo dia: durante esta revision se saneo `SCSC/app.config` y `My Project/Settings*` para dejar de versionar secretos reales; la operacion ahora depende de variables de entorno. La deuda remanente es asegurar despliegue por entorno y rotar credenciales historicamente expuestas.

## Inventario base verificado
- Solucion principal: `SCSC_Marcas.sln`
- Proyecto principal: `SCSC/SCSC_Marcas.vbproj`
- Archivos `.vb`: 80
- Archivos `.vb` no designer: 64
- Archivos operativos/no infrastructure en `Formularios`, `Seguridad`, `Reportes`: 33
- Framework: `.NET Framework 4.6.1`
- Dependencias externas relevantes:
  - Crystal Reports 13
  - SQL Server
  - SDK DigitalPersona
  - `DocumentFormat.OpenXml`

## Arquitectura observada
### Shell y navegacion
- `SCSC/FrmPrincipal.vb` controla el arranque, login, shell legacy y shell moderno.
- `SCSC/Clases/UIShellHost.vb` contiene una capa visual importante para el shell nuevo y dashboard.

### Formularios de operacion
- `SCSC/Formularios/ControlComedor.vb` y `SCSC/Formularios/ControlTransporte.vb` siguen siendo piezas centrales y pesadas.
- Ambos formularios concentran UI, lectura de huella, validacion operativa, feedback visual/sonoro e historial en una sola clase.

### Mantenimientos e importacion
- CRUDs principales: `FrmEstudiantes`, `FrmAgregarEstudiante`, `FrmBecas`, `FrmRutas`, `FrmRecargas`.
- Importacion: `FrmImportarExcel` y `FrmImportarDatos`.
- Ya existe una base comun de estandarizacion visual (`CrudVisualHelper`, `CrudOperationHelper`, `CrudFormBase`), pero la migracion todavia es incremental.

### Acceso a datos
- `SCSC/Clases/FunccionesDB.vb` sigue siendo la fachada legacy dominante para CRUD, transacciones y SQL genérico.
- `SCSC/Clases/Servicios/` introduce una capa mas segura y explicita para comedor, transporte, dashboard, importacion, parametros y RBAC.
- El proyecto esta en estado mixto: parte del sistema usa `SqlCommand` parametrizado y parte depende aun de `ConsultarTSQL`, `AplicaSQL`, `Insert`, `Update`, `Delete`.

### Estado global y reporting
- `SCSC/Clases/VariablesGlobales.vb` mantiene configuracion y contexto global del sistema.
- `SCSC/Clases/CodigoGeneral.vb` expone `gSession`, usado para flujos de busqueda y parametros de reportes.
- `SCSC/Reportes/FrmReportViewer.vb` sigue dependiendo de `gSession` para seleccionar y parametrizar reportes.

## Hallazgos tecnicos
### Fortalezas actuales
- Se observa trabajo reciente de endurecimiento:
  - `Option Strict On` ya esta activo en varios formularios y servicios criticos.
  - existe una capa de servicios para modulos sensibles.
  - hay estandarizacion visual transversal y guardas de Designer.
  - existe logging centralizado (`ErrorLogger`).

### Riesgos vigentes
1. Configuracion sensible historicamente expuesta.
   - El repositorio ya fue saneado en esta revision, pero la operacion ahora depende de variables de entorno y sigue pendiente rotacion de credenciales historicas.
2. La compilacion oculta warnings.
   - `SCSC/SCSC_Marcas.vbproj` usa `WarningLevel=0` en Debug y Release.
3. El acceso a datos legacy sigue siendo demasiado generico.
   - `SCSC/Clases/FunccionesDB.vb` tiene 1384 lineas y concentra demasiadas responsabilidades.
4. Persisten formularios monoliticos.
   - `SCSC/Formularios/ControlComedor.vb` tiene 1743 lineas.
   - `SCSC/Formularios/ControlTransporte.vb` tiene 1380 lineas.
   - `SCSC/Formularios/FrmImportarExcel.vb` tiene 1135 lineas.
   - `SCSC/Formularios/FrmSeguridadRBAC.vb` tiene 1136 lineas.
   - `SCSC/Clases/UIShellHost.vb` tiene 1349 lineas.
5. Estado compartido global.
   - `VariablesGlobales` y `gSession` siguen siendo dependencias transversales de alto acoplamiento.
6. Cobertura de pruebas inexistente.
   - No hay proyecto de pruebas automatizadas; la validacion sigue siendo manual.

## Evidencia puntual
### Seguridad/configuracion
- `SCSC/app.config` y `SCSC/My Project/Settings*`: saneados para usar placeholders y variables de entorno.
- El login de soporte queda deshabilitado por defecto si no se define configuracion externa.

### Build/calidad
- `SCSC/SCSC_Marcas.vbproj`: `WarningLevel=0` en configuraciones principales.
- En este entorno no fue posible ejecutar `msbuild` ni `nuget`; no estan instalados en WSL actual.

### Tipado
- `Option Strict Off` persiste en wrappers autogenerados de Crystal:
  - `SCSC/Reportes/Rpt/RptRuta_general.vb`
  - `SCSC/Reportes/Rpt/RptRuta_detallado.vb`
  - `SCSC/Reportes/Rpt/RptBecadosTransporte.vb`
  - `SCSC/Reportes/Rpt/RptBecadosTransporteDetallado.vb`
  - `SCSC/Reportes/Rpt/RptBecadosComedor.vb`
  - `SCSC/Reportes/Rpt/RptProyecionComedor.vb`
  - `SCSC/Reportes/Rpt/RptFechaComedor.vb`

### SQL y acoplamiento
- `SCSC/Formularios/FrmRecargas.vb`: mantiene SQL de update armado en formulario.
- `SCSC/Clases/FunccionesDB.vb`: expone CRUD dinamico y consultas genericas de uso transversal.
- `SCSC/Clases/CodigoGeneral.vb`: usa `gSession` como bus de estado para busquedas y reportes.

## Direccion recomendada
1. Resolver configuracion sensible fuera del repositorio.
2. Restaurar warnings del compilador y usar build de Windows como baseline obligatorio.
3. Seguir moviendo consultas y reglas desde formularios hacia `Clases/Servicios`.
4. Reducir `gSession` y `VariablesGlobales` empezando por reportes y busquedas.
5. Partir formularios monoliticos por responsabilidades: captura, validacion, persistencia, feedback visual.
6. Agregar al menos pruebas de humo repetibles o una capa de validacion automatizada para servicios puros.

## Limitaciones de esta revision
- No se ejecuto build local desde este entorno porque `msbuild` y `nuget` no estan disponibles.
- No se realizo smoke manual de UI ni conexion a SQL desde este turno.
- El analisis se baso en estructura, configuracion y lectura de codigo del workspace actual.
