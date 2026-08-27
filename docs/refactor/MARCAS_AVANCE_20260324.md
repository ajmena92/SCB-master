# Avance Modulo de Marcas

Fecha: 2026-03-24

## Alcance
- `escritorio/SCSC/Formularios/ControlComedor.vb`
- `escritorio/SCSC/Formularios/ControlTransporte.vb`
- `escritorio/SCSC/Clases/Servicios/ComedorDataService.vb`
- `escritorio/SCSC/Clases/Servicios/TransporteDataService.vb`
- `escritorio/SCSC/Clases/Servicios/ComedorOperacionService.vb`
- `escritorio/SCSC/Clases/Servicios/TransporteOperacionService.vb`
- `escritorio/SCSC/Clases/Servicios/ParametroSistemaService.vb`
- `escritorio/SCSC/Clases/OperativeUiHelper.vb`
- `escritorio/SCSC/Clases/OperativeDialogHelper.vb`
- `escritorio/SCSC/Clases/ServerClock.vb`
- `escritorio/SCSC/Seguridad/LOGIN.vb`
- `escritorio/SCSC/FrmPrincipal.vb`
- `escritorio/SCSC/FrmPrincipal.DashboardRefresh.vb`
- `escritorio/SCSC/Clases/FunccionesDB.vb`

## Avance registrado
### 1. Comedor: reglas operativas y comportamiento
- Se integraron las reglas:
  - `PermitirSinMarcaTransporte`
  - `PermitirMarcaTardia`
  - `ApagaAdvertenciaTransporte`
- `ApagaAdvertenciaTransporte` deja en verde los casos de:
  - estudiante sin marca de transporte;
  - estudiante con marca tardia.
- Las denegaciones por regla muestran el usuario correcto y cuentan en metricas/KPI.
- Se implemento la restriccion de asistencia unica diaria por estudiante.
- Cuando un estudiante ya marco asistencia en comedor el mismo dia:
  - no se vuelve a registrar asistencia;
  - no se rebajan tiquetes;
  - se muestra `Duplicate` con mensaje especifico.
- Las compras/recargas siguen permitiendo multiples registros por dia.

### 2. Comedor: optimizacion y meta diaria dinamica
- Snapshot de usuarios indexado por carnet en memoria.
- Catalogos de becas y horarios indexados en memoria.
- Carga de usuarios con marca de transporte consolidada en una sola consulta.
- Rebajo de tiquetes atomico dentro de transaccion.
- Meta diaria dinamica calculada desde snapshot:
  - estudiantes becados vigentes hoy;
  - estudiantes no becados con al menos un tiquete;
  - sin duplicar estudiantes.
- La barra de progreso usa asistencias validas registradas del dia, no una meta fija por configuracion.
- Se saco DDL del camino operativo del formulario.

### 3. Transporte: reglas de marcas actualizadas
- Primera marca del dia para estudiante:
  - verde;
  - mensaje de entrada y ruta exitosa.
- Segunda marca y siguientes:
  - `PermisoSalida = True`: verde;
  - `PermisoSalida = False`: amarillo;
  - `PermisoSalida = NULL`: amarillo.
- Se mantuvo la proteccion por doble lectura consecutiva.

### 4. Transporte: optimizacion de acceso a datos
- Snapshot de usuarios indexado por carnet en memoria.
- Rutas indexadas en memoria.
- Consultas diarias reescritas con rango por fecha en lugar de `CAST(Fecha AS date)`.
- Parametros SQL tipados en el hot path.
- Se saco DDL del camino operativo del formulario.

### 5. Indices creados en SQL Server
- `dbo.RegistroTransporte(IdUsuario, Fecha)`
- `dbo.RegistroDocentes(IdUsuario, Fecha)`
- `dbo.Usuario(Activo, Cedula)` con `INCLUDE` de columnas usadas por marcas.

### 6. Transporte: rediseño visual operativo
- Jerarquia visual centrada en:
  - contexto del usuario;
  - chip de estado;
  - mensaje principal;
  - icono.
- Sidebar compactado y menos administrativo.
- Historial con render por estado.
- `Duplicate` diferenciado de `Warning`.
- Se mejoro el responsive entre `1366x768` y `1920x1080`.
- Se removieron las imagenes laterales para liberar espacio operativo.
- Se reforzo visualmente el nombre del estudiante en el bloque izquierdo.

### 7. Comedor: adaptacion visual compatible
- Se trasladaron solo las mejoras visuales que aplican a comedor.
- Se limpio el contexto anterior antes de una nueva lectura para no arrastrar datos viejos.
- Se reforzo la jerarquia visual del panel principal.
- `Duplicate` quedo visualmente diferenciado de las advertencias de transporte.
- Historial con render por estado.
- Alto contraste actualizado para los nuevos bloques visuales.
- `Incidencia rapida` se movio a la parte inferior derecha.
- Se mejoro el responsive entre `1366x768` y `1920x1080`.
- Se removieron las imagenes laterales para ampliar el area operativa.
- Se reforzo visualmente el nombre del estudiante en el bloque izquierdo.

### 8. Operacion kiosk: foco, dialogos y apertura limpia
- El foco del lector (`TxtCedula`) quedo reforzado en comedor y transporte.
- Los campos auxiliares del panel izquierdo ya no capturan foco.
- `F4`, `F7`, clicks laterales, limpieza por inactividad y reapertura vuelven a reclamar foco para el lector.
- La incidencia rapida ya no usa `InputBox`; ahora usa un dialogo operativo dedicado.
- Comedor y transporte se abren con instancia nueva para evitar arrastre de estado visual al cerrar y reabrir.

### 9. Infraestructura operativa saneada
- Se introdujo `ServerClock` como reloj operativo basado en hora del servidor.
- Login, principal, dashboard, comedor y transporte se sincronizan contra hora servidor.
- Timestamps operativos, historial, contadores y temporizadores usan `ServerClock`.
- Se removio `AsegurarEsquema` del runtime operativo y de formularios auxiliares.
- Se limpiaron metodos `AsegurarEsquema` ya sin uso en servicios.
- Se tiparon parametros SQL criticos en servicios operativos.
- Se sustituyeron snapshots `DataRow` del hot path por snapshots tipados en memoria.

### 10. Correccion de arranque por fecha servidor
- Se corrigio el fallo de inicio provocado por convertir `GETDATE()` como texto dependiente de cultura regional.
- `FuncionesDB.FechaServer()` ahora obtiene la fecha de SQL Server como `datetime` tipado.

## Validacion realizada
- Revision estatica de codigo y diff.
- `git diff --check` sin errores en los cambios aplicados.
- Verificaciones puntuales para asegurar que en los modulos operativos ya no queden:
  - `AddWithValue` en servicios criticos;
  - `InputBox` en incidencia rapida;
  - `AsegurarEsquema` en el camino operativo;
  - meta fija legacy de comedor;
  - restos visuales de imagenes removidas en los formularios operativos.

## Pendiente de validacion manual
- Build real en Windows/Visual Studio.
- Smoke operativo completo de:
  - login;
  - comedor;
  - transporte;
  - recargas;
  - importacion;
  - dashboard principal.
- Validacion visual completa en distintas resoluciones y escalados.
- Validacion operativa de:
  - lectura normal;
  - duplicado;
  - sin tiquetes;
  - sin marca de transporte;
  - marca tardia;
  - asistencia unica diaria en comedor;
  - primera y segunda marca en transporte;
  - reconexion;
  - reapertura de formularios.

## Referencia
- La deuda tecnica pendiente se actualizo en `docs/refactor/TECH_DEBT_BACKLOG_20260324.md`.
