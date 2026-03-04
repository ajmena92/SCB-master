# Catálogo Funcional para Refactorización

Este catálogo describe cada `Sub`/`Function` detectada en el código VB (sin archivos designer), con una descripción operativa breve para planificar refactorización.

## SCSC/Busqueda.vb
- Línea 6: CargarGrid (Sub) - Carga de datos iniciales/catálogos o refresco de UI.
- Línea 56: GridConsulta_CellClick (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 68: GridConsulta_CellContentClick (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 79: GridConsulta_CellDoubleClick (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 84: GridConsulta_ColumnHeaderMouseClick (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 99: Guardar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 138: Cancelar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 142: BtnFiltro_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 160: Busqueda_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 173: TxtFiltro_KeyDown (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 181: TxtFiltro_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 185: LblTitulo_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.

## SCSC/Clases/AppData.vb
- Línea 12: Update (Sub) - Operación de acceso a datos (CRUD/consulta SQL).

## SCSC/Clases/CodigoGeneral.vb
- Línea 3: LimpiarSession (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 44: KeyAscii (Function) - Lógica auxiliar/método interno.
- Línea 58: GetAppConfig (Function) - Obtención de configuración, parámetros o datos de soporte.
- Línea 66: ObtenerValorParametroConexion (Function) - Obtención de configuración, parámetros o datos de soporte.
- Línea 87: SeleccionaComboItem (Sub) - Construcción de estructuras auxiliares para consultas o UI.
- Línea 103: SoloNumerosFN (Function) - Lógica auxiliar/método interno.
- Línea 122: SCM (Function) - Lógica auxiliar/método interno.
- Línea 125: ArmaFechaReporte (Function) - Normalización y construcción de fechas para consultas/reportes.
- Línea 130: sen (Function) - Lógica auxiliar/método interno.
- Línea 161: ArmaFechaQueryHora (Function) - Normalización y construcción de fechas para consultas/reportes.
- Línea 164: ArmaFechaQuery (Function) - Normalización y construcción de fechas para consultas/reportes.
- Línea 167: ConvertDate (Function) - Normalización y construcción de fechas para consultas/reportes.
- Línea 171: ObtenerParametroConexion (Function) - Obtención de configuración, parámetros o datos de soporte.

## SCSC/Clases/Encriptacion64.vb
- Línea 12: EncriptaPossic (Function) - Encriptación/desencriptación y utilidades de seguridad local.
- Línea 20: Encrypt (Function) - Encriptación/desencriptación y utilidades de seguridad local.
- Línea 35: encryptQueryString (Function) - Encriptación/desencriptación y utilidades de seguridad local.
- Línea 46: M_Encriptar (Function) - Encriptación/desencriptación y utilidades de seguridad local.
- Línea 87: M_DesEncriptar (Function) - Encriptación/desencriptación y utilidades de seguridad local.
- Línea 134: RecuperaFile01 (Function) - Obtención de configuración, parámetros o datos de soporte.
- Línea 166: RecuperaFile02 (Function) - Obtención de configuración, parámetros o datos de soporte.
- Línea 200: CreaArchivo1 (Sub) - Lógica auxiliar/método interno.
- Línea 238: CreaArchivo2 (Sub) - Lógica auxiliar/método interno.
- Línea 271: CreaArchivos (Function) - Lógica auxiliar/método interno.
- Línea 296: PinGenera (Function) - Encriptación/desencriptación y utilidades de seguridad local.
- Línea 321: PinDesEncripta (Function) - Encriptación/desencriptación y utilidades de seguridad local.

## SCSC/Clases/FunccionesDB.vb
- Línea 21: Redondear (Function) - Lógica auxiliar/método interno.
- Línea 81: AbrirConexion (Sub) - Gestión de ciclo de conexión/transacción SQL Server.
- Línea 105: CerrarConexion (Sub) - Gestión de ciclo de conexión/transacción SQL Server.
- Línea 138: FechaServer (Function) - Normalización y construcción de fechas para consultas/reportes.
- Línea 158: FechaSistema (Function) - Normalización y construcción de fechas para consultas/reportes.
- Línea 178: FechaCierre (Function) - Normalización y construcción de fechas para consultas/reportes.
- Línea 208: Delete (Function) - Operación de acceso a datos (CRUD/consulta SQL).
- Línea 261: Delete (Function) - Operación de acceso a datos (CRUD/consulta SQL).
- Línea 323: Insert (Function) - Operación de acceso a datos (CRUD/consulta SQL).
- Línea 413: InsertScalar (Function) - Operación de acceso a datos (CRUD/consulta SQL).
- Línea 478: Update (Function) - Operación de acceso a datos (CRUD/consulta SQL).
- Línea 557: ConsultarTSQL (Function) - Operación de acceso a datos (CRUD/consulta SQL).
- Línea 650: ConsultarTSQLGroupBy (Function) - Operación de acceso a datos (CRUD/consulta SQL).
- Línea 725: Consultar (Function) - Operación de acceso a datos (CRUD/consulta SQL).
- Línea 906: GuardarActualizar (Function) - Operación de acceso a datos (CRUD/consulta SQL).
- Línea 964: AplicaSQL (Function) - Operación de acceso a datos (CRUD/consulta SQL).
- Línea 1033: ArmaValor (Sub) - Construcción de estructuras auxiliares para consultas o UI.
- Línea 1053: ArmaValor (Sub) - Construcción de estructuras auxiliares para consultas o UI.
- Línea 1077: InicializarArray (Function) - Construcción de estructuras auxiliares para consultas o UI.
- Línea 1083: ObtenerUsuarioBaseDatos (Function) - Obtención de configuración, parámetros o datos de soporte.
- Línea 1133: ObtenerUsuarioBaseDatos (Function) - Obtención de configuración, parámetros o datos de soporte.
- Línea 1183: ObtenerParametroConexion (Function) - Obtención de configuración, parámetros o datos de soporte.
- Línea 1233: CargaCombo (Sub) - Carga de datos iniciales/catálogos o refresco de UI.
- Línea 1320: IniciaSQL (Sub) - Gestión de ciclo de conexión/transacción SQL Server.
- Línea 1334: FinalSQL (Sub) - Gestión de ciclo de conexión/transacción SQL Server.
- Línea 1343: RollSQL (Sub) - Gestión de ciclo de conexión/transacción SQL Server.
- Línea 1355: VereficaCarnet (Function) - Lógica auxiliar/método interno.

## SCSC/Clases/LBItem.vb
- Línea 7: New (Sub) - Lógica auxiliar/método interno.
- Línea 17: ToString (Function) - Lógica auxiliar/método interno.
- Línea 21: ToString (Function) - Lógica auxiliar/método interno.

## SCSC/Clases/VariablesGlobales.vb
- Sin funciones/subrutinas declaradas.

## SCSC/Formularios/ControlComedor.vb
- Línea 22: Verify (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 26: BtnSalir_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 46: Process (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 78: ProcesoBusqueda (Sub) - Búsqueda y filtrado de datos según criterios de usuario.
- Línea 114: UpdateStatus (Sub) - Lógica auxiliar/método interno.
- Línea 119: ConvertSampleToBitmap (Function) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 126: ExtractFeatures (Function) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 138: StartCapture (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 149: StopCapture (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 160: CaptureForm_FormClosed (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 172: OnComplete (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 186: OnFingerGone (Sub) - Lógica auxiliar/método interno.
- Línea 190: OnFingerTouch (Sub) - Lógica auxiliar/método interno.
- Línea 194: OnReaderConnect (Sub) - Lógica auxiliar/método interno.
- Línea 198: OnReaderDisconnect (Sub) - Lógica auxiliar/método interno.
- Línea 202: OnSampleQuality (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 210: SetStatus (Sub) - Lógica auxiliar/método interno.
- Línea 214: _SetStatus (Sub) - Lógica auxiliar/método interno.
- Línea 218: SetPrompt (Sub) - Lógica auxiliar/método interno.
- Línea 222: _SetPrompt (Sub) - Lógica auxiliar/método interno.
- Línea 226: MakeReport (Sub) - Lógica auxiliar/método interno.
- Línea 230: _MakeReport (Sub) - Lógica auxiliar/método interno.
- Línea 234: DrawPicture (Sub) - Lógica auxiliar/método interno.
- Línea 238: _DrawPicture (Sub) - Lógica auxiliar/método interno.
- Línea 242: ControlComedor_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 281: ProcesarMarca (Sub) - Construcción de estructuras auxiliares para consultas o UI.
- Línea 285: _ProcesarMarca (Sub) - Construcción de estructuras auxiliares para consultas o UI.
- Línea 350: LimpiarPantalla (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 354: _LimpiarPantalla (Sub) - Lógica auxiliar/método interno.
- Línea 371: MensajeVisual (Sub) - Lógica auxiliar/método interno.
- Línea 375: TxtCedula_KeyDown (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 433: lblProcesando_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 437: _MensajeVisual (Sub) - Lógica auxiliar/método interno.

## SCSC/Formularios/ControlTransporte.vb
- Línea 20: Verify (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 24: BtnSalir_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 45: Process (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 77: ProcesoBusqueda (Sub) - Búsqueda y filtrado de datos según criterios de usuario.
- Línea 113: UpdateStatus (Sub) - Lógica auxiliar/método interno.
- Línea 118: ConvertSampleToBitmap (Function) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 125: ExtractFeatures (Function) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 137: StartCapture (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 148: StopCapture (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 159: CaptureForm_FormClosed (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 171: OnComplete (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 185: OnFingerGone (Sub) - Lógica auxiliar/método interno.
- Línea 189: OnFingerTouch (Sub) - Lógica auxiliar/método interno.
- Línea 193: OnReaderConnect (Sub) - Lógica auxiliar/método interno.
- Línea 197: OnReaderDisconnect (Sub) - Lógica auxiliar/método interno.
- Línea 201: OnSampleQuality (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 209: SetStatus (Sub) - Lógica auxiliar/método interno.
- Línea 213: _SetStatus (Sub) - Lógica auxiliar/método interno.
- Línea 217: SetPrompt (Sub) - Lógica auxiliar/método interno.
- Línea 221: _SetPrompt (Sub) - Lógica auxiliar/método interno.
- Línea 225: MakeReport (Sub) - Lógica auxiliar/método interno.
- Línea 229: _MakeReport (Sub) - Lógica auxiliar/método interno.
- Línea 233: DrawPicture (Sub) - Lógica auxiliar/método interno.
- Línea 237: _DrawPicture (Sub) - Lógica auxiliar/método interno.
- Línea 241: ControlTransporte_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 266: ProcesarMarca (Sub) - Construcción de estructuras auxiliares para consultas o UI.
- Línea 270: _ProcesarMarca (Sub) - Construcción de estructuras auxiliares para consultas o UI.
- Línea 321: LimpiarPantalla (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 325: _LimpiarPantalla (Sub) - Lógica auxiliar/método interno.
- Línea 345: MensajeVisual (Sub) - Lógica auxiliar/método interno.
- Línea 349: TxtCedula_KeyDown (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 383: TxtTipo_TextChanged (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 386: _MensajeVisual (Sub) - Lógica auxiliar/método interno.
- Línea 491: LblFecha_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 495: TxtCedula_TextChanged (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 499: LblTitulo_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 503: lblProcesando_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 507: PanelResult_Paint (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 511: ControlTransporte_Closed (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.

## SCSC/Formularios/FrmAgregarEstudiante.vb
- Línea 8: FrmEstudiantes_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 24: CargaEspecialidad (Sub) - Carga de datos iniciales/catálogos o refresco de UI.
- Línea 40: CargaHorarios (Sub) - Carga de datos iniciales/catálogos o refresco de UI.
- Línea 64: LimpiarPantalla (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 80: Buscar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 112: TxtCedula_Validated (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 163: TxtCedula_KeyDown (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 173: BtnRegresar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 177: BtnGuardar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 203: BtnCancelar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 209: BtnEliminar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 248: CaptureForm_FormClosed (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.

## SCSC/Formularios/FrmAyuda.vb
- Línea 3: FrmAyuda_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 22: OKButton_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 26: LabelProductName_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 30: TextBoxDescription_TextChanged (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.

## SCSC/Formularios/FrmBecas.vb
- Línea 8: LimpiarPantalla (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 17: LimpiarChek (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 25: FrmRecarga_FormClosed (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 36: FrmEstudiantes_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 54: Buscar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 83: txtCodRuta_Validated (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 131: TxtCedula_KeyDown (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 141: TxtRecarga_KeyDown (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 147: Validacion (Function) - Lógica auxiliar/método interno.
- Línea 163: BtnGuardar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 198: BtnRegresar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 202: txtCodRuta_TextChanged (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 206: CkActivo_CheckedChanged (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 216: BtnEliminar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 241: BtnCancelar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 247: dia_CheckedChanged (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.

## SCSC/Formularios/FrmEstudiantes.vb
- Línea 15: FrmEstudiantes_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 33: CargaGenero (Sub) - Carga de datos iniciales/catálogos o refresco de UI.
- Línea 38: CargaPermiso (Sub) - Carga de datos iniciales/catálogos o refresco de UI.
- Línea 43: CargaRutas (Sub) - Carga de datos iniciales/catálogos o refresco de UI.
- Línea 66: CargaBecas (Sub) - Carga de datos iniciales/catálogos o refresco de UI.
- Línea 91: LimpiarPantalla (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 117: Buscar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 149: TxtCedula_Validated (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 255: TxtCedula_KeyDown (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 265: BtnRegresar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 269: BtnGuardar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 304: BtnCancelar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 312: Init (Sub) - Lógica auxiliar/método interno.
- Línea 329: Process (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 361: UpdateStatus (Sub) - Lógica auxiliar/método interno.
- Línea 365: ConvertSampleToBitmap (Function) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 372: ExtractFeatures (Function) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 384: StartCapture (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 395: StopCapture (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 405: CaptureForm_FormClosed (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 417: OnComplete (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 423: OnFingerGone (Sub) - Lógica auxiliar/método interno.
- Línea 427: OnFingerTouch (Sub) - Lógica auxiliar/método interno.
- Línea 431: OnReaderConnect (Sub) - Lógica auxiliar/método interno.
- Línea 435: OnReaderDisconnect (Sub) - Lógica auxiliar/método interno.
- Línea 439: OnSampleQuality (Sub) - Flujo biométrico: captura, extracción de huella y verificación.
- Línea 448: SetStatus (Sub) - Lógica auxiliar/método interno.
- Línea 452: _SetStatus (Sub) - Lógica auxiliar/método interno.
- Línea 456: SetPrompt (Sub) - Lógica auxiliar/método interno.
- Línea 460: _SetPrompt (Sub) - Lógica auxiliar/método interno.
- Línea 464: MakeReport (Sub) - Lógica auxiliar/método interno.
- Línea 468: _MakeReport (Sub) - Lógica auxiliar/método interno.
- Línea 472: DrawPicture (Sub) - Lógica auxiliar/método interno.
- Línea 476: _DrawPicture (Sub) - Lógica auxiliar/método interno.
- Línea 480: CBRuta_SelectedIndexChanged (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 491: Label16_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.

## SCSC/Formularios/FrmImportarDatos.vb
- Línea 8: LimpiarPantalla (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 13: FrmRecarga_FormClosed (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 23: FrmEstudiantes_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 60: BtnCancelar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 64: Validaciion (Function) - Lógica auxiliar/método interno.
- Línea 76: BtnGuardar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 139: BtnRegresar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 143: Label8_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 147: GroupBox2_Enter (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 152: TxtRecarga_TextChanged (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 156: Label1_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.

## SCSC/Formularios/FrmImportarExcel.vb
- Línea 13: LimpiarPantalla (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 21: BtnCancelar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 25: Validaciion (Function) - Lógica auxiliar/método interno.
- Línea 105: BtnRegresar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 110: FrmImportarExcel_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 119: BtnGuardar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 239: CargaHorarios (Sub) - Carga de datos iniciales/catálogos o refresco de UI.

## SCSC/Formularios/FrmRecargas.vb
- Línea 16: LimpiarPantalla (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 27: FrmRecarga_FormClosed (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 38: FrmEstudiantes_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 54: Buscar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 85: TxtCedula_Validated (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 145: TxtCedula_KeyDown (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 155: TxtRecarga_Validated (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 161: BtnCancelar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 167: TxtRecarga_KeyDown (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 173: Validacion (Function) - Lógica auxiliar/método interno.
- Línea 189: BtnGuardar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 250: BtnRegresar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.

## SCSC/Formularios/FrmRutas.vb
- Línea 7: LimpiarPantalla (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 18: FrmRecarga_FormClosed (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 29: FrmEstudiantes_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 46: Buscar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 75: txtCodRuta_Validated (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 106: TxtCedula_KeyDown (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 117: BtnCancelar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 123: TxtRecarga_KeyDown (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 129: Validacion (Function) - Lógica auxiliar/método interno.
- Línea 149: BtnGuardar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 172: BtnRegresar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 176: txtCodRuta_TextChanged (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 180: CkActivo_CheckedChanged (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 190: BtnEliminar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.

## SCSC/Formularios/IMPRIMIR.vb
- Línea 22: btnCrear_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 42: btnSeleccionarImpresora_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 54: prdoDocumento_PrintPage (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 86: btnVer_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 90: btnCerrar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 94: prdoDocumento_BeginPrint (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.

## SCSC/FrmPrincipal.vb
- Línea 5: FrmPrincipal_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 10: UsuariosToolStripMenuItem_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 14: ControlDeMarcasToolStripMenuItem_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 18: AyudaToolStripMenuItem_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 35: ImportarDatosPIADToolStripMenuItem_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 39: ReporteDiariosToolStripMenuItem_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 43: ImprimirToolStripMenuItem_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 48: UtilitariosToolStripMenuItem_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 52: ImportarDatosListaPIADToolStripMenuItem_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 57: BtnCerrar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 61: ReporteDeServicioTransporteToolStripMenuItem_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 65: RecargasToolStripMenuItem1_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 69: Gesti (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 73: Gesti (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 77: ReporteProyecci (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 81: ReporteEstudiantesBecadosToolStripMenuItem_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 85: AgregarEstudianteManualToolStripMenuItem_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.

## SCSC/My Project/AssemblyInfo.vb
- Sin funciones/subrutinas declaradas.

## SCSC/Reportes/FrmReportViewer.vb
- Línea 2: FrmReportViewer_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.

## SCSC/Reportes/Parametros/FrmBecados.vb
- Línea 6: FrmReporteMarcas_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 30: BtnCancelar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 34: Limpiar (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 41: BtnRegresar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 45: BtnGuardar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 56: ArmaReporte (Function) - Preparación/ejecución de reportes o salida impresa.
- Línea 94: RbGeneral_CheckedChanged (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 98: RbDetallo_CheckedChanged (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.

## SCSC/Reportes/Parametros/FrmProyeccionComedor.vb
- Línea 6: FrmReporteMarcas_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 44: BtnCancelar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 48: Limpiar (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 52: BtnRegresar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 56: BtnGuardar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 67: ArmaReporte (Function) - Preparación/ejecución de reportes o salida impresa.

## SCSC/Reportes/Parametros/FrmReporteComedor.vb
- Línea 6: FrmReporteMarcas_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 56: BtnCancelar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 60: Limpiar (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 66: BtnRegresar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 70: BtnGuardar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 81: ArmaReporte (Function) - Preparación/ejecución de reportes o salida impresa.

## SCSC/Reportes/Parametros/FrmReporteRutas.vb
- Línea 4: FrmReporteMarcas_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 36: BtnCancelar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 40: Limpiar (Sub) - Limpia estado de formulario/controles y reinicia flujo.
- Línea 46: BtnRegresar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 50: BtnGuardar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 61: ArmaReporte (Function) - Preparación/ejecución de reportes o salida impresa.

## SCSC/Reportes/Rpt/RptBecadosComedor.vb
- Línea 24: New (Sub) - Lógica auxiliar/método interno.
- Línea 165: New (Sub) - Lógica auxiliar/método interno.

## SCSC/Reportes/Rpt/RptBecadosTransporte.vb
- Línea 24: New (Sub) - Lógica auxiliar/método interno.
- Línea 165: New (Sub) - Lógica auxiliar/método interno.

## SCSC/Reportes/Rpt/RptBecadosTransporteDetallado.vb
- Línea 24: New (Sub) - Lógica auxiliar/método interno.
- Línea 165: New (Sub) - Lógica auxiliar/método interno.

## SCSC/Reportes/Rpt/RptFechaComedor.vb
- Línea 24: New (Sub) - Lógica auxiliar/método interno.
- Línea 149: New (Sub) - Lógica auxiliar/método interno.

## SCSC/Reportes/Rpt/RptProyecionComedor.vb
- Línea 24: New (Sub) - Lógica auxiliar/método interno.
- Línea 149: New (Sub) - Lógica auxiliar/método interno.

## SCSC/Reportes/Rpt/RptRuta_detallado.vb
- Línea 24: New (Sub) - Lógica auxiliar/método interno.
- Línea 181: New (Sub) - Lógica auxiliar/método interno.

## SCSC/Reportes/Rpt/RptRuta_general.vb
- Línea 24: New (Sub) - Lógica auxiliar/método interno.
- Línea 165: New (Sub) - Lógica auxiliar/método interno.

## SCSC/Seguridad/LOGIN.vb
- Línea 12: ClavePaso_Enter (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 18: ClavePaso_Leave (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 51: CodUsuario_Enter (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 54: CodUsuario_Leave (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 95: Login_Load (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 143: Salir_ClickEvent (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 149: ClavePaso_KeyDown (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 155: CodUsuario_KeyDown (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 161: BtnAceptar_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 187: BtnComedor_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 191: BtnTransporte_Click (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 195: ClavePaso_KeyDown_1 (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 202: ClavePaso_ParentChanged (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.
- Línea 206: CodUsuario_KeyPress (Sub) - Manejador de evento de UI; coordina interacción de usuario y flujo de pantalla.

