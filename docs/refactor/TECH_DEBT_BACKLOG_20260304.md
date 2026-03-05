# Backlog de Deuda Tecnica (2026-03-04)

## Objetivo
Cerrar primero estabilidad de Designer/compilacion y luego terminar la estandarizacion visual hibrida 2026.

## Prioridad P0 (bloquea productividad)
1. Estabilizar Designer en formularios criticos (`LOGIN`, `FrmSeguridadRBAC`, `FrmPrincipal`).
   - Regla: `Inherits` solo en `*.designer.vb`.
   - Regla: constructor sin logica de ventana en modo diseño (`LicenseManager.UsageMode`).
   - Validacion: abrir/cerrar Designer 3 veces seguidas sin error.

2. Eliminar divergencia Designer vs runtime.
   - Estructura UI (layout, Dock, Anchor, RowStyles, ColumnStyles) solo en Designer.
   - Runtime solo tema/estados/datos.

3. Baseline de build en entorno Windows (VS).
   - `Clean + Rebuild` exitoso.
   - Sin errores activos en formularios modernizados.

## Prioridad P1 (riesgo alto runtime)
4. Reducir deuda de tipado legacy.
   - Migrar modulos criticos a `Option Strict On` por fases.
   - Reemplazar conversiones implicitas por conversiones explicitas.

5. Revisar supresion global de warnings en `SCSC_Marcas.vbproj`.
   - Mantener solo supresiones justificadas.
   - Recuperar visibilidad de warnings de alto riesgo (late binding, Object arithmetic, narrowing conversions).

6. Consolidar manejo de recursos (logos/imagenes/iconos).
   - Toda imagen usada en formularios debe existir en `My.Resources` o ruta validada.
   - Evitar referencias rotas en `.resx`.

## Prioridad P2 (calidad visual y mantenibilidad)
7. Estandarizar grids de todo el sistema.
   - Cabecera visible, fila 0 no oculta, altura de fila consistente.
   - Columnas tecnicas (`Id`) ocultas por defecto en vistas operativas.

8. Estandarizar acciones por pestaña en formularios complejos.
   - Botones agrupados por contexto de tab, sin mezclar acciones.
   - Espacio fijo para barra de acciones para evitar superposicion.

9. Normalizar tipografia y escala responsive.
   - Jerarquia de fuentes y espaciado segun `UI_HYBRID_STANDARD_2026.md`.

10. Cerrar migracion de dependencias visuales legacy.
    - Retirar compatibilidad temporal Bunifu en pantallas ya modernizadas.

## Criterio de cierre por fase
- Designer abre sin error.
- Build local en VS sin errores nuevos.
- Smoke manual del flujo impactado con evidencia.
- Captura visual aprobada contra estandar hibrido.

## Avance aplicado (2026-03-04 noche)
- Build automation:
  - `scripts/autobuild.ps1` reforzado con parametros (`Configuration`, `Platform`, `DebounceSeconds`, `MaxLogFiles`).
  - Watcher extendido a `.vb`, `.vbproj`, `.resx`, `.config`.
  - Validacion temprana de rutas y limpieza automatica de logs.
- Deuda de tipado (fase incremental):
  - `SCSC/Clases/CodigoGeneral.vb`: funciones de fecha SQL ahora con tipo de retorno explicito (`As String`).
  - Objetivo: reducir warnings de tipo `Function without an 'As' clause`.
  - `SCSC/Clases/FunccionesDB.vb`: primer bloque de conversiones implicitas corregido (redondeo, fechas, scalar identity, llaves `Object`, `PadLeft` con `Char`, combo y carnet).
  - `SCSC/Clases/Servicios/ComedorDataService.vb`: conversiones explicitas en validacion de beca y tiquetes.
  - `SCSC/Clases/Servicios/TransporteDataService.vb`: conversiones explicitas para `CodTipo` e `IdUsuario` en SQL.
  - `SCSC/Busqueda.vb`: comparacion de columnas por igualdad explicita (`String.Equals`) y lectura segura de celdas nulas al construir `gSession.Resultado`.
  - `SCSC/Reportes/FrmReportViewer.vb`: eliminado late binding (reporte tipado como `ReportDocument`) y parametros `IIf` migrados a `If` con `String.IsNullOrEmpty`.
  - `SCSC/Reportes/Parametros/*.vb`: casteo explicito de `LBItem`, uso de `DateTimePicker.Value.Date`, inicializacion de `Criterio` y retorno booleano real en `ArmaReporte()`.
  - `SCSC/Formularios/ControlComedor.vb`: parseo robusto de `PermitirSinMarcaTransporte` (bool/int/string) y conversion segura de `HoraLimite`/`HoraMarca` a `TimeSpan`.
  - `SCSC/Formularios/FrmRecargas.vb`: conversiones explicitas en carga de usuario/tiquetes, recarga tipada, transaccion inicializada y `LblTotal` consistente como texto.
  - `SCSC/Formularios/FrmRutas.vb`: tipado de `Tag`/`IdRuta`, `MsgBox` tipado (`MsgBoxResult`) y limpieza de variables no usadas.
  - `SCSC/Formularios/FrmEstudiantes.vb`: normalizacion de `LBItem` en combos, acceso tipado a `DataRow`, asignaciones de `SelectedValue`/`Tag` con `CInt`, y manejo seguro de ruta seleccionada nula.
  - `SCSC/Formularios/FrmBecas.vb`: tipado de `Tag`, `DataRow` tipado, `DiasBecados` inicializado, cast explicito de `CheckBox` y `MsgBoxResult` en eliminacion.
  - `SCSC/Formularios/FrmImportarExcel.vb`: transaccion inicializada a `Nothing`, rollback condicionado, concatenaciones seguras con `&`, cierre seguro de `conn`, y carga de horarios con `LBItem` tipado.
  - `SCSC/Formularios/FrmAgregarEstudiante.vb`: `DataRow` tipado, conversiones explicitas (`CInt/CStr/CDate`) y confirmaciones tipadas con `MsgBoxResult`.
  - `SCSC/Formularios/ControlTransporte.vb`: conversión booleana robusta (`ConvertirABooleano`) para `PermisoSalida` y eliminación de casteos directos frágiles desde `Object`.
  - `SCSC/Clases/FunccionesDB.vb`: helper `EsValorSqlNull` para evitar `ToString` sobre `Object` potencialmente nulo, comparación segura de `IS NOT NULL` y limpieza de variables locales sin uso en métodos de cadena de conexión.
  - `SCSC/Clases/Encriptacion64.vb` y `SCSC/Reportes/FrmReportViewer.vb`: eliminación de declaraciones múltiples ambiguas (`Dim a, b As ...`) para evitar inferencia accidental a `Object`.
  - `SCSC/Formularios/FrmImportarDatos.vb`: tipado de `Validaciion` como `Private Function`, cast seguro de `CbCursoLectivo.SelectedItem` a `LBItem`, conversiones explicitas de `DataRow` y manejo seguro de rollback/cierre en error de importación.
  - `SCSC/Clases/Encriptacion64.vb`: eliminación de código inalcanzable y estandarización de acceso a archivos con `Using` en lectura/escritura para evitar fugas de recursos y bloqueos de archivo.
  - `SCSC/Formularios/FrmBecas.vb` y `SCSC/Formularios/FrmRutas.vb`: normalización de lecturas `DataRow`, validaciones con retorno explícito (`Private Function ... As Boolean`) y concatenaciones/control de `Tag` con conversiones seguras para reducir warnings de conversión implícita.
  - `SCSC/Clases/FunccionesDB.vb`: limpieza de `Throw ex` (rethrow correcto), retorno explícito en utilidades de fecha/redondeo y refuerzo de estabilidad en manejo de excepciones.
  - `SCSC/Busqueda.vb`: eliminación de mutación de `gSession.Valor3` durante selección de columnas (usa copia local), tipado explícito de variables de parsing y menor riesgo de efectos secundarios en búsquedas consecutivas.
  - `SCSC/Formularios/FrmRecargas.vb`: parseo de recarga centralizado, validación con retornos explícitos y lecturas de `DataRow` con acceso tipado (`row("Campo")`) para reducir conversiones implícitas.
  - `SCSC/Formularios/FrmImportarExcel.vb`: validación tipada (`Private Function`) y limpieza de concatenación/variable no usada al abrir archivo Excel.
  - `SCSC/Formularios/FrmAgregarEstudiante.vb`: lecturas `DataRow` tipadas, eliminación de campos duplicados en consulta, validaciones de longitud con `Trim().Length` y guardado seguro de valores de combo (`Especialidad`/`IdHorario`).
  - `SCSC/Formularios/FrmEstudiantes.vb`: normalización de acceso a `DataRow`, reducción de ramas booleanas redundantes, validaciones por `Trim().Length` y guardado seguro de `SelectedValue` para ruta/beca.
  - `SCSC/Formularios/FrmBecas.vb`, `SCSC/Formularios/FrmRutas.vb`, `SCSC/Formularios/FrmRecargas.vb`: reemplazo incremental de `Len(...)` por validaciones explícitas de texto para disminuir conversiones implícitas.
  - `SCSC/Formularios/ControlComedor.vb` y `SCSC/Formularios/ControlTransporte.vb`: barrido amplio de `DataRow` para eliminar accesos `!Campo`, conversiones explícitas y mayor consistencia en historial/estado operativo.
  - `SCSC/Clases/Servicios/ComedorDataService.vb` y `SCSC/Clases/Servicios/TransporteDataService.vb`: tipado de claves (`IdUsuario`, `IdHorario`, `CodTipo`, `TipoBeca`) y normalización de asignaciones/consultas para reducir conversiones implícitas y errores por `Object`.
  - `SCSC/Seguridad/LOGIN.vb`: limpieza de validaciones `Len(...)`, acceso tipado a parámetros de configuración (`DataRow`) y conversiones explícitas para precio/fecha/permisos.
  - `SCSC/Reportes/Parametros/*`: normalización de `ArmaReporte()` a `Private Function` con retorno explícito y eliminación de filtros duplicados de beca en reportes de comedor/proyección.
  - `SCSC/Reportes/Parametros/FrmReporteComedor.vb`, `FrmProyeccionComedor.vb`, `FrmReporteRutas.vb`, `FrmBecados.vb`: uso consistente de `LBItem` tipado en combos y construcción de criterios sin duplicados; retorno booleano correcto (`True` solo en éxito).
  - `SCSC/Formularios/FrmRecargas.vb`: `Validacion()` corregida para retornar `False` en recarga inválida y parseo seguro (`Integer.TryParse`) en cantidad.
  - `SCSC/Formularios/FrmRutas.vb`: centralización de parseo de `IdRuta` desde `Tag` (`ObtenerIdRutaDesdeTag`) para eliminar conversiones repetitivas con `Val`.
  - `SCSC/Clases/CodigoGeneral.vb` y `SCSC/Clases/Encriptacion64.vb`: reducción incremental de `Len/Val` legacy en puntos seguros, con uso de `.Length` y `Integer.TryParse`.
