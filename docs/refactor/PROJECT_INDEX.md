# Índice Técnico del Proyecto SCB

> Actualizacion vigente: ver `docs/refactor/PROJECT_ANALYSIS_20260309.md` y `docs/refactor/TECH_DEBT_BACKLOG_20260309.md` para el estado recalculado al 2026-03-09.

## Resumen
- Fecha de indexación: 2026-03-04
- Solución principal: `SCSC_Marcas.sln`
- Archivos VB totales: 55
- Archivos VB (sin designer): 33
- Métodos/Subs/Functions detectados (total): 369
- Métodos/Subs/Functions (sin designer): 330

## Estándares Operativos Activos
- Modo UI WinForms: `Designer-first`.
- Guía oficial: `docs/refactor/DESIGNER_FIRST_GUIDE.md`.
- Estandar visual oficial 2026 (hibrido): `docs/refactor/UI_HYBRID_STANDARD_2026.md`.

## Estructura de Carpetas Relevante
- `SCSC/Clases`: utilidades, acceso a datos, encriptación, variables globales.
- `SCSC/Formularios`: pantallas operativas (comedor, transporte, estudiantes, importación).
- `SCSC/Seguridad`: login/autenticación.
- `SCSC/Reportes`: parámetros y visualización Crystal Reports.
- `SCSC/My Project`: settings/recursos de VB.

## Inventario de Funciones (Detalle por Archivo)

### SCSC/Busqueda.designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Busqueda.vb
- Línea 6:    Sub CargarGrid()
- Línea 56:    Private Sub GridConsulta_CellClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridConsulta.CellClick
- Línea 68:    Private Sub GridConsulta_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridConsulta.CellContentClick
- Línea 79:    Private Sub GridConsulta_CellDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridConsulta.CellDoubleClick
- Línea 84:    Private Sub GridConsulta_ColumnHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles GridConsulta.ColumnHeaderMouseClick
- Línea 99:    Private Sub Guardar_Click(sender As Object, e As EventArgs) Handles Guardar.Click
- Línea 138:    Private Sub Cancelar_Click(sender As Object, e As EventArgs) Handles Cancelar.Click
- Línea 142:    Private Sub BtnFiltro_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnFiltro.Click
- Línea 160:    Private Sub Busqueda_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 173:    Private Sub TxtFiltro_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtFiltro.KeyDown
- Línea 181:    Private Sub TxtFiltro_Click(sender As Object, e As EventArgs) Handles TxtFiltro.Click, TxtFiltro.Enter
- Línea 185:    Private Sub LblTitulo_Click(sender As Object, e As EventArgs) Handles LblTitulo.Click

### SCSC/Clases/AppData.vb
- Línea 12:	Public Sub Update()

### SCSC/Clases/CodigoGeneral.vb
- Línea 3:    Public Sub LimpiarSession()
- Línea 44:    Public Function KeyAscii(ByVal UserKeyArgument As KeyPressEventArgs) As Short
- Línea 58:    Function GetAppConfig(Optional ByVal NombreConfiguracion As String = "Conexion") As String
- Línea 66:    Function ObtenerValorParametroConexion(ByVal pBuscar As String, Optional ByVal pBuscarAux As String = ".") As String
- Línea 87:    Sub SeleccionaComboItem(ByRef Combo As ComboBox, ByVal Valor As String)
- Línea 103:    Public Function SoloNumerosFN(ByVal Keyascii As Short) As Short
- Línea 122:    Function SCM(ByVal valor As String) As String
- Línea 125:    Public Function ArmaFechaReporte(ByRef Campo As String, ByRef FechaInicial As Date, ByRef FechaFinal As Date) As String
- Línea 130:    Function sen(ByVal Valor As String) As String
- Línea 161:    Function ArmaFechaQueryHora(ByVal Campo As String, ByVal FechaInicial As Date, ByVal FechaFinal As Date)
- Línea 164:    Function ArmaFechaQuery(ByVal Campo As String, ByVal FechaInicial As Date, ByVal FechaFinal As Date)
- Línea 167:    Function ConvertDate(ByVal Campo As String, ByVal signo As String, ByVal Fecha As Date)
- Línea 171:    Function ObtenerParametroConexion(ByVal pBuscar As String, Optional ByVal pBuscarAuxiliar As String = ".") As String

### SCSC/Clases/Encriptacion64.vb
- Línea 12:        Function EncriptaPossic(ByVal pClave As String) As String
- Línea 20:        Private Function Encrypt(ByVal stringToEncrypt As String, ByVal SEncryptionKey As String) As String
- Línea 35:        Public Function encryptQueryString(ByVal strQueryString As String, ByVal pkey As String) As String
- Línea 46:        Public Function M_Encriptar(ByVal Texto As String, ByVal Factor As Byte) As String
- Línea 87:        Public Function M_DesEncriptar(ByVal Texto As String) As String
- Línea 134:        Function RecuperaFile01() As SysFile
- Línea 166:        Function RecuperaFile02() As SysFile
- Línea 200:        Sub CreaArchivo1(ByVal Info As SysFile)
- Línea 238:        Sub CreaArchivo2(ByVal Info As SysFile)
- Línea 271:        Function CreaArchivos(ByVal Info As SysFile) As Boolean
- Línea 296:        Function PinGenera(ByVal Info As SysFile) As String
- Línea 321:        Function PinDesEncripta(ByVal lPIN As String) As String

### SCSC/Clases/FunccionesDB.vb
- Línea 21:    Function Redondear(ValorNumero)
- Línea 81:    Public Sub AbrirConexion(ByRef pCn As SqlConnection, ByVal pUsarTransaccion As Boolean, Optional ByRef pTran As SqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion")
- Línea 105:    Public Sub CerrarConexion(ByRef pCn As SqlConnection, Optional ByRef pTran As SqlTransaction = Nothing)
- Línea 138:    Function FechaServer(Optional ByVal Conexion As String = "Conexion") As Date
- Línea 158:    Function FechaSistema(Optional ByVal Conexion As String = "Conexion") As Date
- Línea 178:    Function FechaCierre(Optional ByVal Conexion As String = "Conexion") As Date
- Línea 208:    Public Function Delete(ByVal Tabla As String, ByVal Llave As String, _
- Línea 261:    Public Function Delete(ByVal Tabla As String, ByVal Llave As Campos(), Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef PTransac As SqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As Boolean
- Línea 323:    Public Function Insert(ByVal Tabla As String, ByVal Campo As Campos(), Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef PTransac As SqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As Boolean
- Línea 413:    Public Function InsertScalar(ByVal Tabla As String, ByVal Campo As Campos(), Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef PTransac As SqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As Integer
- Línea 478:    Public Function Update(ByVal Tabla As String, ByVal Campo As Campos(), ByRef LlavePrimaria As Campos(), Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef PTransac As SqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As Boolean
- Línea 557:    Public Function ConsultarTSQL(ByVal Tabla As String, ByVal pSQL As String, Optional ByVal LlavePrimaria As Campos() = Nothing, _
- Línea 650:    Public Function ConsultarTSQLGroupBy(ByVal Tabla As String, ByVal pSQL As String, _
- Línea 725:    Public Function Consultar(ByVal Tabla As String, ByVal Campo As Campos(), _
- Línea 906:    Public Function GuardarActualizar(ByVal Tabla As String, ByVal Campo As Campos(), Optional ByRef LlavePrimaria As Campos() = Nothing, Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef PTransac As SqlTransaction = Nothing, Optional ByVal Conexion As String = "Conexion") As Boolean
- Línea 964:    Public Function AplicaSQL(ByVal Sql As String, Optional ByRef Cn As SqlConnection = Nothing, Optional ByRef PTransac As SqlTransaction = Nothing, Optional ByVal Llave As Campos() = Nothing, Optional ByVal pOperadorLogico As String = " and ", Optional ByVal Conexion As String = "Conexion", Optional TimeOut As Integer = 45) As Boolean
- Línea 1033:    Sub ArmaValor(ByRef Array() As FuncionesDB.Campos, ByVal NombreCampo As String, Optional ByVal Valor As String = "", Optional ByVal Formato As String = "")
- Línea 1053:    Sub ArmaValor(ByRef Array() As FuncionesDB.Campos, ByVal NombreCampo As String, ByVal Valor As Object, Optional ByVal Formato As String = "")
- Línea 1077:    Function InicializarArray() As FuncionesDB.Campos()
- Línea 1083:    Function ObtenerUsuarioBaseDatos(ByVal pBuscar As String, Optional ByVal pBuscarAuxiliar As String = ".", Optional ByVal Conexion As String = "Conexion") As String
- Línea 1133:    Function ObtenerUsuarioBaseDatos(ByVal pBuscar As String, Optional ByVal pBuscarAuxiliar As String = ".") As String
- Línea 1183:    Function ObtenerParametroConexion(ByVal pBuscar As String, Optional ByVal pBuscarAuxiliar As String = ".", Optional ByVal CadenaConexion As String = "") As String
- Línea 1233:    Sub CargaCombo(ByRef Combo As ComboBox, ByVal pTabla As String, ByVal pCodigo As String, ByVal pNombre As String, Optional Cn As SqlClient.SqlConnection = Nothing, Optional Transac As SqlClient.SqlTransaction = Nothing)
- Línea 1320:    Public Sub IniciaSQL(ByRef pCn As SqlConnection, ByRef pTran As SqlTransaction)
- Línea 1334:    Public Sub FinalSQL(ByRef pTran As SqlTransaction)
- Línea 1343:    Public Sub RollSQL(ByRef pTran As SqlTransaction)
- Línea 1355:    Public Function VereficaCarnet(ByRef pCedula As String) As Boolean

### SCSC/Clases/LBItem.vb
- Línea 7:    Public Sub New(ByVal pValor As String, ByVal pDescripcion As String, Optional ByVal pValor2 As Object = Nothing)
- Línea 17:    Public Overrides Function ToString() As String
- Línea 21:    Public Overloads Function ToString(ByVal pFormato As Short) As String

### SCSC/Clases/VariablesGlobales.vb

### SCSC/Formularios/ControlComedor.Designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Formularios/ControlComedor.vb
- Línea 22:    Public Sub Verify(ByVal template As DPFP.Template)
- Línea 26:    Private Sub BtnSalir_Click(sender As Object, e As EventArgs) Handles BtnSalir.Click
- Línea 46:    Protected Sub Process(ByVal Sample As DPFP.Sample)
- Línea 78:    Private Sub ProcesoBusqueda(ByVal pBuscar As BusquedaHuella)
- Línea 114:    Protected Sub UpdateStatus(ByVal FAR As Integer)
- Línea 119:    Protected Function ConvertSampleToBitmap(ByVal Sample As DPFP.Sample) As Bitmap
- Línea 126:    Protected Function ExtractFeatures(ByVal Sample As DPFP.Sample, ByVal Purpose As DPFP.Processing.DataPurpose) As DPFP.FeatureSet
- Línea 138:    Protected Sub StartCapture()
- Línea 149:    Protected Sub StopCapture()
- Línea 160:    Private Sub CaptureForm_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
- Línea 172:    Sub OnComplete(ByVal Capture As Object, ByVal ReaderSerialNumber As String, ByVal Sample As DPFP.Sample) Implements DPFP.Capture.EventHandler.OnComplete
- Línea 186:    Sub OnFingerGone(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnFingerGone
- Línea 190:    Sub OnFingerTouch(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnFingerTouch
- Línea 194:    Sub OnReaderConnect(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnReaderConnect
- Línea 198:    Sub OnReaderDisconnect(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnReaderDisconnect
- Línea 202:    Sub OnSampleQuality(ByVal Capture As Object, ByVal ReaderSerialNumber As String, ByVal CaptureFeedback As DPFP.Capture.CaptureFeedback) Implements DPFP.Capture.EventHandler.OnSampleQuality
- Línea 210:    Protected Sub SetStatus(ByVal status)
- Línea 214:    Private Sub _SetStatus(ByVal status)
- Línea 218:    Protected Sub SetPrompt(ByVal text)
- Línea 222:    Private Sub _SetPrompt(ByVal text)
- Línea 226:    Protected Sub MakeReport(ByVal status)
- Línea 230:    Private Sub _MakeReport(ByVal status)
- Línea 234:    Protected Sub DrawPicture(ByVal bmp)
- Línea 238:    Private Sub _DrawPicture(ByVal bmp)
- Línea 242:    Private Sub ControlComedor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 281:    Protected Sub ProcesarMarca(ByVal Usuario As DataRow)
- Línea 285:    Sub _ProcesarMarca(ByVal Usuario As DataRow)
- Línea 350:    Protected Sub LimpiarPantalla(ByVal Limpiar)
- Línea 354:    Private Sub _LimpiarPantalla(ByVal Limpiar)
- Línea 371:    Protected Sub MensajeVisual(ByVal TipoImagen As Int16)
- Línea 375:    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtCedula.KeyDown
- Línea 433:    Private Sub lblProcesando_Click(sender As Object, e As EventArgs) Handles lblProcesando.Click
- Línea 437:    Private Sub _MensajeVisual(ByVal TipoImagen As Int16)

### SCSC/Formularios/ControlTransporte.Designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Formularios/ControlTransporte.vb
- Línea 20:    Public Sub Verify(ByVal template As DPFP.Template)
- Línea 24:    Private Sub BtnSalir_Click(sender As Object, e As EventArgs) Handles BtnCerrar.Click
- Línea 45:    Protected Sub Process(ByVal Sample As DPFP.Sample)
- Línea 77:    Private Sub ProcesoBusqueda(ByVal pBuscar As BusquedaHuella2)
- Línea 113:    Protected Sub UpdateStatus(ByVal FAR As Integer)
- Línea 118:    Protected Function ConvertSampleToBitmap(ByVal Sample As DPFP.Sample) As Bitmap
- Línea 125:    Protected Function ExtractFeatures(ByVal Sample As DPFP.Sample, ByVal Purpose As DPFP.Processing.DataPurpose) As DPFP.FeatureSet
- Línea 137:    Protected Sub StartCapture()
- Línea 148:    Protected Sub StopCapture()
- Línea 159:    Private Sub CaptureForm_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
- Línea 171:    Sub OnComplete(ByVal Capture As Object, ByVal ReaderSerialNumber As String, ByVal Sample As DPFP.Sample) Implements DPFP.Capture.EventHandler.OnComplete
- Línea 185:    Sub OnFingerGone(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnFingerGone
- Línea 189:    Sub OnFingerTouch(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnFingerTouch
- Línea 193:    Sub OnReaderConnect(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnReaderConnect
- Línea 197:    Sub OnReaderDisconnect(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnReaderDisconnect
- Línea 201:    Sub OnSampleQuality(ByVal Capture As Object, ByVal ReaderSerialNumber As String, ByVal CaptureFeedback As DPFP.Capture.CaptureFeedback) Implements DPFP.Capture.EventHandler.OnSampleQuality
- Línea 209:    Protected Sub SetStatus(ByVal status)
- Línea 213:    Private Sub _SetStatus(ByVal status)
- Línea 217:    Protected Sub SetPrompt(ByVal text)
- Línea 221:    Private Sub _SetPrompt(ByVal text)
- Línea 225:    Protected Sub MakeReport(ByVal status)
- Línea 229:    Private Sub _MakeReport(ByVal status)
- Línea 233:    Protected Sub DrawPicture(ByVal bmp)
- Línea 237:    Private Sub _DrawPicture(ByVal bmp)
- Línea 241:    Private Sub ControlTransporte_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 266:    Protected Sub ProcesarMarca(ByVal Usuario As DataRow)
- Línea 270:    Sub _ProcesarMarca(ByVal Usuario As DataRow)
- Línea 321:    Protected Sub LimpiarPantalla(ByVal Limpiar)
- Línea 325:    Private Sub _LimpiarPantalla(ByVal Limpiar)
- Línea 345:    Protected Sub MensajeVisual(ByVal TipoImagen As Int16)
- Línea 349:    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtCedula.KeyDown
- Línea 383:    Private Sub TxtTipo_TextChanged(sender As Object, e As EventArgs) Handles TxtTipo.TextChanged
- Línea 386:    Private Sub _MensajeVisual(ByVal TipoImagen As Int16)
- Línea 491:    Private Sub LblFecha_Click(sender As Object, e As EventArgs) Handles LblFecha.Click
- Línea 495:    Private Sub TxtCedula_TextChanged(sender As Object, e As EventArgs) Handles TxtCedula.TextChanged
- Línea 499:    Private Sub LblTitulo_Click(sender As Object, e As EventArgs) Handles LblTitulo.Click
- Línea 503:    Private Sub lblProcesando_Click(sender As Object, e As EventArgs) Handles lblProcesando.Click
- Línea 507:    Private Sub PanelResult_Paint(sender As Object, e As PaintEventArgs) Handles PanelResult.Paint
- Línea 511:    Private Sub ControlTransporte_Closed(sender As Object, e As EventArgs) Handles Me.Closed

### SCSC/Formularios/FrmAgregarEstudiante.Designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Formularios/FrmAgregarEstudiante.vb
- Línea 8:    Private Sub FrmEstudiantes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 24:    Sub CargaEspecialidad(ByRef Combo As ComboBox)
- Línea 40:    Sub CargaHorarios(ByRef Combo As ComboBox)
- Línea 64:    Sub LimpiarPantalla(Optional pFocus As Boolean = True)
- Línea 80:    Private Sub Buscar_Click(sender As Object, e As EventArgs) Handles Buscar.Click
- Línea 112:    Private Sub TxtCedula_Validated(sender As Object, e As EventArgs) Handles TxtCedula.Validated
- Línea 163:    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtCedula.KeyDown
- Línea 173:    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
- Línea 177:    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
- Línea 203:    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
- Línea 209:    Private Sub BtnEliminar_Click(sender As Object, e As EventArgs) Handles BtnEliminar.Click
- Línea 248:    Private Sub CaptureForm_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed

### SCSC/Formularios/FrmAyuda.Designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 32:    Private Sub InitializeComponent()

### SCSC/Formularios/FrmAyuda.vb
- Línea 3:    Private Sub FrmAyuda_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
- Línea 22:    Private Sub OKButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OKButton.Click
- Línea 26:    Private Sub LabelProductName_Click(sender As Object, e As EventArgs) Handles LabelProductName.Click
- Línea 30:    Private Sub TextBoxDescription_TextChanged(sender As Object, e As EventArgs) Handles TextBoxDescription.TextChanged

### SCSC/Formularios/FrmBecas.designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Formularios/FrmBecas.vb
- Línea 8:    Sub LimpiarPantalla()
- Línea 17:    Sub LimpiarChek()
- Línea 25:    Private Sub FrmRecarga_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
- Línea 36:    Private Sub FrmEstudiantes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 54:    Private Sub Buscar_Click(sender As Object, e As EventArgs) Handles Buscar.Click
- Línea 83:    Private Sub txtCodRuta_Validated(sender As Object, e As EventArgs) Handles txtCodBeca.Validated
- Línea 131:    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles txtCodBeca.KeyDown
- Línea 141:    Private Sub TxtRecarga_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtDescripcion.KeyDown
- Línea 147:    Function Validacion() As Boolean
- Línea 163:    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
- Línea 198:    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
- Línea 202:    Private Sub txtCodRuta_TextChanged(sender As Object, e As EventArgs) Handles txtCodBeca.TextChanged
- Línea 206:    Private Sub CkActivo_CheckedChanged(sender As Object, e As EventArgs) Handles CkActivo.CheckedChanged
- Línea 216:    Private Sub BtnEliminar_Click(sender As Object, e As EventArgs) Handles BtnEliminar.Click
- Línea 241:    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
- Línea 247:    Private Sub dia_CheckedChanged(sender As Object, e As EventArgs) Handles Ck2.CheckedChanged, Ck3.CheckedChanged, Ck4.CheckedChanged, Ck5.CheckedChanged, Ck6.CheckedChanged

### SCSC/Formularios/FrmEstudiantes.Designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Formularios/FrmEstudiantes.vb
- Línea 15:    Private Sub FrmEstudiantes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 33:    Sub CargaGenero(ByRef Combo As ComboBox)
- Línea 38:    Sub CargaPermiso(ByRef Combo As ComboBox)
- Línea 43:    Sub CargaRutas(ByRef Combo As ComboBox)
- Línea 66:    Sub CargaBecas(ByRef Combo As ComboBox)
- Línea 91:    Sub LimpiarPantalla(Optional pFocus As Boolean = True)
- Línea 117:    Private Sub Buscar_Click(sender As Object, e As EventArgs) Handles Buscar.Click
- Línea 149:    Private Sub TxtCedula_Validated(sender As Object, e As EventArgs) Handles TxtCedula.Validated
- Línea 255:    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtCedula.KeyDown
- Línea 265:    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
- Línea 269:    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
- Línea 304:    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
- Línea 312:    Protected Sub Init()
- Línea 329:    Protected Sub Process(ByVal Sample As DPFP.Sample)
- Línea 361:    Protected Sub UpdateStatus()
- Línea 365:    Protected Function ConvertSampleToBitmap(ByVal Sample As DPFP.Sample) As Bitmap
- Línea 372:    Protected Function ExtractFeatures(ByVal Sample As DPFP.Sample, ByVal Purpose As DPFP.Processing.DataPurpose) As DPFP.FeatureSet
- Línea 384:    Protected Sub StartCapture()
- Línea 395:    Protected Sub StopCapture()
- Línea 405:    Private Sub CaptureForm_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
- Línea 417:    Sub OnComplete(ByVal Capture As Object, ByVal ReaderSerialNumber As String, ByVal Sample As DPFP.Sample) Implements DPFP.Capture.EventHandler.OnComplete
- Línea 423:    Sub OnFingerGone(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnFingerGone
- Línea 427:    Sub OnFingerTouch(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnFingerTouch
- Línea 431:    Sub OnReaderConnect(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnReaderConnect
- Línea 435:    Sub OnReaderDisconnect(ByVal Capture As Object, ByVal ReaderSerialNumber As String) Implements DPFP.Capture.EventHandler.OnReaderDisconnect
- Línea 439:    Sub OnSampleQuality(ByVal Capture As Object, ByVal ReaderSerialNumber As String, ByVal CaptureFeedback As DPFP.Capture.CaptureFeedback) Implements DPFP.Capture.EventHandler.OnSampleQuality
- Línea 448:    Protected Sub SetStatus(ByVal status)
- Línea 452:    Private Sub _SetStatus(ByVal status)
- Línea 456:    Protected Sub SetPrompt(ByVal text)
- Línea 460:    Private Sub _SetPrompt(ByVal text)
- Línea 464:    Protected Sub MakeReport(ByVal status)
- Línea 468:    Private Sub _MakeReport(ByVal status)
- Línea 472:    Protected Sub DrawPicture(ByVal bmp)
- Línea 476:    Private Sub _DrawPicture(ByVal bmp)
- Línea 480:    Private Sub CBRuta_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CBRuta.SelectedIndexChanged
- Línea 491:    Private Sub Label16_Click(sender As Object, e As EventArgs) Handles Label16.Click

### SCSC/Formularios/FrmImportarDatos.designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Formularios/FrmImportarDatos.vb
- Línea 8:    Sub LimpiarPantalla()
- Línea 13:    Private Sub FrmRecarga_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
- Línea 23:    Private Sub FrmEstudiantes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 60:    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
- Línea 64:    Function Validaciion() As Boolean
- Línea 76:    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
- Línea 139:    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
- Línea 143:    Private Sub Label8_Click(sender As Object, e As EventArgs)
- Línea 147:    Private Sub GroupBox2_Enter(sender As Object, e As EventArgs)
- Línea 152:    Private Sub TxtRecarga_TextChanged(sender As Object, e As EventArgs)
- Línea 156:    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

### SCSC/Formularios/FrmImportarExcel.designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Formularios/FrmImportarExcel.vb
- Línea 13:    Sub LimpiarPantalla()
- Línea 21:    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
- Línea 25:    Function Validaciion() As Boolean
- Línea 105:    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
- Línea 110:    Private Sub FrmImportarExcel_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 119:    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
- Línea 239:    Sub CargaHorarios(ByRef Combo As ComboBox)

### SCSC/Formularios/FrmRecargas.designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Formularios/FrmRecargas.vb
- Línea 16:    Sub LimpiarPantalla()
- Línea 27:    Private Sub FrmRecarga_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
- Línea 38:    Private Sub FrmEstudiantes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 54:    Private Sub Buscar_Click(sender As Object, e As EventArgs) Handles Buscar.Click
- Línea 85:    Private Sub TxtCedula_Validated(sender As Object, e As EventArgs) Handles txtCedula.Validated
- Línea 145:    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles txtCedula.KeyDown
- Línea 155:    Private Sub TxtRecarga_Validated(sender As Object, e As EventArgs) Handles TxtRecarga.Validated
- Línea 161:    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
- Línea 167:    Private Sub TxtRecarga_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtRecarga.KeyDown
- Línea 173:    Function Validacion() As Boolean
- Línea 189:    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
- Línea 250:    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click

### SCSC/Formularios/FrmRutas.designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Formularios/FrmRutas.vb
- Línea 7:    Sub LimpiarPantalla(Optional PCodigo = True)
- Línea 18:    Private Sub FrmRecarga_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
- Línea 29:    Private Sub FrmEstudiantes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 46:    Private Sub Buscar_Click(sender As Object, e As EventArgs) Handles Buscar.Click
- Línea 75:    Private Sub txtCodRuta_Validated(sender As Object, e As EventArgs) Handles txtCodRuta.Validated
- Línea 106:    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles txtCodRuta.KeyDown
- Línea 117:    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
- Línea 123:    Private Sub TxtRecarga_KeyDown(sender As Object, e As KeyEventArgs)
- Línea 129:    Function Validacion() As Boolean
- Línea 149:    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
- Línea 172:    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
- Línea 176:    Private Sub txtCodRuta_TextChanged(sender As Object, e As EventArgs) Handles txtCodRuta.TextChanged
- Línea 180:    Private Sub CkActivo_CheckedChanged(sender As Object, e As EventArgs) Handles CkActivo.CheckedChanged
- Línea 190:    Private Sub BtnEliminar_Click(sender As Object, e As EventArgs) Handles BtnEliminar.Click

### SCSC/Formularios/IMPRIMIR.Designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Formularios/IMPRIMIR.vb
- Línea 22:    Private Sub btnCrear_Click(ByVal sender As System.Object, _
- Línea 42:    Private Sub btnSeleccionarImpresora_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSeleccionarImpresora.Click
- Línea 54:    Private Sub prdoDocumento_PrintPage(ByVal sender As System.Object, ByVal e As System.Drawing.Printing.PrintPageEventArgs) Handles prdoDocumento.PrintPage
- Línea 86:    Private Sub btnVer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVer.Click
- Línea 90:    Private Sub btnCerrar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btncerrar.Click
- Línea 94:    Private Sub prdoDocumento_BeginPrint(ByVal sender As Object, ByVal e As System.Drawing.Printing.PrintEventArgs) Handles prdoDocumento.BeginPrint

### SCSC/FrmPrincipal.Designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/FrmPrincipal.vb
- Línea 5:    Private Sub FrmPrincipal_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 10:    Private Sub UsuariosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UsuariosToolStripMenuItem.Click
- Línea 14:    Private Sub ControlDeMarcasToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ControlDeMarcasToolStripMenuItem.Click
- Línea 18:    Private Sub AyudaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AyudaToolStripMenuItem.Click
- Línea 35:    Private Sub ImportarDatosPIADToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImportarDatosPIADToolStripMenuItem.Click
- Línea 39:    Private Sub ReporteDiariosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteDiariosToolStripMenuItem.Click
- Línea 43:    Private Sub ImprimirToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImprimirToolStripMenuItem.Click
- Línea 48:    Private Sub UtilitariosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UtilitariosToolStripMenuItem.Click
- Línea 52:    Private Sub ImportarDatosListaPIADToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImportarDatosListaPIADToolStripMenuItem.Click
- Línea 57:    Private Sub BtnCerrar_Click(sender As Object, e As EventArgs) Handles BtnCerrar.Click
- Línea 61:    Private Sub ReporteDeServicioTransporteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteDeServicioTransporteToolStripMenuItem.Click
- Línea 65:    Private Sub RecargasToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles RecargasToolStripMenuItem1.Click
- Línea 69:    Private Sub GestiónRutasToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GestiónRutasToolStripMenuItem.Click
- Línea 73:    Private Sub GestiónBecasToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GestiónBecasToolStripMenuItem.Click
- Línea 77:    Private Sub ReporteProyecciónComedorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteProyecciónComedorToolStripMenuItem.Click
- Línea 81:    Private Sub ReporteEstudiantesBecadosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteEstudiantesBecadosToolStripMenuItem.Click
- Línea 85:    Private Sub AgregarEstudianteManualToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AgregarEstudianteManualToolStripMenuItem.Click

### SCSC/My Project/Application.Designer.vb
- Línea 25:        Public Sub New()
- Línea 34:        Protected Overrides Sub OnCreateMainForm()

### SCSC/My Project/AssemblyInfo.vb

### SCSC/My Project/Resources.Designer.vb

### SCSC/My Project/Settings.Designer.vb
- Línea 32:    Private Shared Sub AutoSaveSettings(sender As Global.System.Object, e As Global.System.EventArgs)

### SCSC/Reportes/FrmReportViewer.designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Reportes/FrmReportViewer.vb
- Línea 2:    Private Sub FrmReportViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load

### SCSC/Reportes/Parametros/FrmBecados.Designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Reportes/Parametros/FrmBecados.vb
- Línea 6:    Private Sub FrmReporteMarcas_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 30:    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
- Línea 34:    Sub Limpiar()
- Línea 41:    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
- Línea 45:    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
- Línea 56:    Function ArmaReporte() As Boolean
- Línea 94:    Private Sub RbGeneral_CheckedChanged(sender As Object, e As EventArgs) Handles RbGeneral.CheckedChanged
- Línea 98:    Private Sub RbDetallo_CheckedChanged(sender As Object, e As EventArgs) Handles RbDetallo.CheckedChanged

### SCSC/Reportes/Parametros/FrmProyeccionComedor.Designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Reportes/Parametros/FrmProyeccionComedor.vb
- Línea 6:    Private Sub FrmReporteMarcas_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 44:    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
- Línea 48:    Sub Limpiar()
- Línea 52:    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
- Línea 56:    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
- Línea 67:    Function ArmaReporte() As Boolean

### SCSC/Reportes/Parametros/FrmReporteComedor.Designer.vb
- Línea 8:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 25:    Private Sub InitializeComponent()

### SCSC/Reportes/Parametros/FrmReporteComedor.vb
- Línea 6:    Private Sub FrmReporteMarcas_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 56:    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
- Línea 60:    Sub Limpiar()
- Línea 66:    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
- Línea 70:    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
- Línea 81:    Function ArmaReporte() As Boolean

### SCSC/Reportes/Parametros/FrmReporteRutas.Designer.vb
- Línea 7:    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
- Línea 24:    Private Sub InitializeComponent()

### SCSC/Reportes/Parametros/FrmReporteRutas.vb
- Línea 4:    Private Sub FrmReporteMarcas_Load(sender As Object, e As EventArgs) Handles MyBase.Load
- Línea 36:    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
- Línea 40:    Sub Limpiar()
- Línea 46:    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
- Línea 50:    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
- Línea 61:    Function ArmaReporte() As Boolean

### SCSC/Reportes/Rpt/RptBecadosComedor.vb
- Línea 24:    Public Sub New()
- Línea 165:    Public Sub New()

### SCSC/Reportes/Rpt/RptBecadosTransporte.vb
- Línea 24:    Public Sub New()
- Línea 165:    Public Sub New()

### SCSC/Reportes/Rpt/RptBecadosTransporteDetallado.vb
- Línea 24:    Public Sub New()
- Línea 165:    Public Sub New()

### SCSC/Reportes/Rpt/RptFechaComedor.vb
- Línea 24:    Public Sub New()
- Línea 149:    Public Sub New()

### SCSC/Reportes/Rpt/RptProyecionComedor.vb
- Línea 24:    Public Sub New()
- Línea 149:    Public Sub New()

### SCSC/Reportes/Rpt/RptRuta_detallado.vb
- Línea 24:    Public Sub New()
- Línea 181:    Public Sub New()

### SCSC/Reportes/Rpt/RptRuta_general.vb
- Línea 24:    Public Sub New()
- Línea 165:    Public Sub New()

### SCSC/Seguridad/LOGIN.designer.vb

### SCSC/Seguridad/LOGIN.vb
- Línea 12:    Private Sub ClavePaso_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles ClavePaso.Enter
- Línea 18:    Private Sub ClavePaso_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles ClavePaso.Validated
- Línea 51:    Private Sub CodUsuario_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs)
- Línea 54:    Private Sub CodUsuario_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles CodUsuario.Validated
- Línea 95:    Private Sub Login_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
- Línea 143:    Private Sub Salir_ClickEvent(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles BtnCerrar.Click
- Línea 149:    Private Sub ClavePaso_KeyDown(sender As Object, e As KeyEventArgs)
- Línea 155:    Private Sub CodUsuario_KeyDown(sender As Object, e As KeyEventArgs)
- Línea 161:    Private Sub BtnAceptar_Click(sender As Object, e As EventArgs) Handles BtnLogin.Click
- Línea 187:    Private Sub BtnComedor_Click(sender As Object, e As EventArgs) Handles BtnComedor.Click
- Línea 191:    Private Sub BtnTransporte_Click(sender As Object, e As EventArgs) Handles BtnTransporte.Click
- Línea 195:    Private Sub ClavePaso_KeyDown_1(sender As Object, e As KeyEventArgs) Handles ClavePaso.KeyDown
- Línea 202:    Private Sub ClavePaso_ParentChanged(sender As Object, e As EventArgs) Handles ClavePaso.ParentChanged
- Línea 206:    Private Sub CodUsuario_KeyPress(sender As Object, e As KeyEventArgs) Handles CodUsuario.KeyDown
