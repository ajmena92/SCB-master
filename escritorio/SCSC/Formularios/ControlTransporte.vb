Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Media
Imports System.Threading
Imports System.Threading.Tasks

Public Class ControlTransporte
    Private Const SegundosInactividadLimpiarRegistro As Integer = 60
    Private Const PermitirCierreOperador As Boolean = True
    Private Const SidebarMinWidth As Integer = 420
    Private Const SidebarMaxWidth As Integer = 520

    Private Ulthuella As String
    Private ErrUltHuella As Boolean
    Private EstadoVerificado As Boolean

    Private _timerInactividad As System.Windows.Forms.Timer
    Private _timerEstadoVisual As System.Windows.Forms.Timer
    Private _ultimaActividad As DateTime
    Private _limpiezaAplicadaPorInactividad As Boolean
    Private _ultimoIntentoReconexion As DateTime

    Private _estadoVisualActual As EstadoVisual
    Private _fechaUltimoEstado As DateTime
    Private _resultadoOperacionActual As String
    Private _ultimoCarnetLeido As String

    Private _sonidosHabilitados As Boolean
    Private _sonidoOkRuta As String
    Private _sonidoWarnRuta As String
    Private _sonidoErrorRuta As String
    Private _repeticionesSonidoOk As Integer
    Private _repeticionesSonidoWarn As Integer
    Private _repeticionesSonidoError As Integer
    Private _intervaloRepeticionSonidoMs As Integer
    Private _forzarSonidoSistemaFallback As Boolean
    Private _sonidoCancellation As CancellationTokenSource
    Private ReadOnly _sonidoSync As New Object()

    Private _colorExito As Color
    Private _colorAdvertencia As Color
    Private _colorError As Color
    Private _colorNeutro As Color
    Private _colorProcesando As Color
    Private _colorDuplicado As Color

    Private _totalLecturas As Integer
    Private _totalDuplicadas As Integer
    Private _totalErrores As Integer
    Private _lecturasExitosas As Integer
    Private _muestrasTiempoAtencion As Integer
    Private _acumuladoTiempoAtencionMs As Double
    Private _inicioLectura As DateTime
    Private _mostrarEntradaManualTemporal As Boolean

    Private _lblResultadoOperacion As Label
    Private _lblUltimaLectura As Label
    Private _lblEdadEstado As Label
    Private _lblConexion As Label
    Private _lblKpi As Label
    Private _lblHistorial As Label
    Private _lstHistorial As ListBox
    Private _btnIncidencia As Button
    Private _lblScanHint As Label
    Private _lblFocusEscaneo As Label
    Private _lblHotkeys As Label
    Private _lblEstadoChip As Label
    Private _panelSidebarDividerDatos As Panel
    Private _panelSidebarDividerMetricas As Panel
    Private _modoAltoContraste As Boolean

    Private ReadOnly Cls As New FuncionesDB
    Private ReadOnly TransporteSvc As New TransporteDataService(Cls)
    Private ReadOnly OperacionSvc As New TransporteOperacionService()
    Private ReadOnly Cn As New SqlClient.SqlConnection
    Private _usuariosPorCedula As Dictionary(Of String, UsuarioTransporteSnapshot) = New Dictionary(Of String, UsuarioTransporteSnapshot)(StringComparer.OrdinalIgnoreCase)
    Private _rutasPorId As Dictionary(Of Integer, RutaSnapshot) = New Dictionary(Of Integer, RutaSnapshot)()
    Private _inicializadoEventosActividad As Boolean
    Private _eventoPersistidoEnTransaccion As Boolean
    Private _estadoEventoTransaccion As EstadoVisual
    Private _estadoResultadoMarcaActual As EstadoVisual
    Private _mensajeEstadoPersonalizado As String
    Private _resultadoOperacionPersonalizado As String

    Private Enum EstadoVisual
        Idle = 0
        Processing = 1
        Success = 2
        Warning = 3
        Duplicate = 4
        ErrorGeneral = 5
        NotFound = 6
    End Enum

    Private Enum EstadoPermisoSalida
        Autorizado = 0
        NoAutorizado = 1
        NoRegistrado = 2
    End Enum

    Private Enum LayoutMode
        Narrow1366 = 0
        Compact = 1
        Standard = 2
        Wide = 3
    End Enum

    Private Delegate Sub BoolCall(ByVal value As Boolean)
    Private Delegate Sub UsuarioCall(ByVal usuario As UsuarioTransporteSnapshot)

    Private NotInheritable Class UsuarioTransporteSnapshot
        Public Property IdUsuario As Integer
        Public Property Nombre As String
        Public Property PrimerApellido As String
        Public Property SegundoApellido As String
        Public Property CodTipo As Short
        Public Property TieneRuta As Boolean
        Public Property IdRuta As Integer
        Public Property Seccion As String
        Public Property Cedula As String
        Public Property IdHorario As Integer
        Public Property TienePermisoSalida As Boolean
        Public Property PermisoSalida As Boolean

        Public ReadOnly Property NombreCompleto As String
            Get
                Return String.Format("{0} {1} {2}", Nombre, PrimerApellido, SegundoApellido).Trim()
            End Get
        End Property
    End Class

    Private NotInheritable Class RutaSnapshot
        Public Property IdRuta As Integer
        Public Property Codigo As String
        Public Property Descripcion As String
    End Class

    Private Sub BtnSalir_Click(sender As Object, e As EventArgs) Handles BtnCerrar.Click
        EnsureScanFocus(False)
    End Sub

    Private Sub ControlTransporte_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            If _timerInactividad IsNot Nothing Then
                RemoveHandler _timerInactividad.Tick, AddressOf TimerInactividad_Tick
                _timerInactividad.Stop()
                _timerInactividad.Dispose()
                _timerInactividad = Nothing
            End If
            If _timerEstadoVisual IsNot Nothing Then
                RemoveHandler _timerEstadoVisual.Tick, AddressOf TimerEstadoVisual_Tick
                _timerEstadoVisual.Stop()
                _timerEstadoVisual.Dispose()
                _timerEstadoVisual = Nothing
            End If
            DetenerSonidoActivo()

            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
        Catch ex As Exception
            ErrorLogger.LogException("ControlTransporte.FormClosed", ex)
        End Try
    End Sub

    Private Sub ControlTransporte_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If
        Try
            UIThemeManagerV2.Apply(Me, "operativo")
            CargarPreferenciasOperacion()
            ApplyModernOperationalLayout()
            InicializarControlesOperacion()
            ApplyResponsiveLayout()

            Cls.AbrirConexion(Cn, False)
            SincronizarHoraServidorActual()
            RecargarDatosOperacion()

            Ulthuella = String.Empty
            LblFecha.Text = FechaServer.ToString("yyyy/MM/dd HH:mm:ss")
            ResetResultFields()
            UpdateVisualState(EstadoVisual.Idle)

            InicializarControlInactividad()
            InicializarTimerEstadoVisual()
            ActualizarKpisOperacion()
            ActualizarEstadoConexion()
            CargarHistorialInicial()
            EnsureScanFocus(True)
        Catch ex As Exception
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
            ErrorLogger.LogException("ControlTransporte_Load", ex)
            MsgBox("Error al cargar el Formulario: " & ex.Message, MsgBoxStyle.Critical)
            Me.Close()
        End Try
    End Sub

    Private Sub ControlTransporte_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        ApplyResponsiveLayout()
        EnsureScanFocus(True)
    End Sub

    Private Sub ControlTransporte_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        ApplyResponsiveLayout()
    End Sub

    Private Sub ControlTransporte_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If PermitirCierreOperador Then
            Exit Sub
        End If

        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            EnsureScanFocus(False)
        End If
    End Sub

    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtCedula.KeyDown
        RegistrarActividad()

        If e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            e.Handled = True
            EjecutarAccionEscape()
            Exit Sub
        End If

        If e.KeyCode <> Keys.Enter Then
            Exit Sub
        End If

        e.SuppressKeyPress = True
        ProcesarLecturaCarnet(TxtCedula.Text)
    End Sub

    Private Sub ProcesarLecturaCarnet(ByVal carnetRaw As String)
        RegistrarActividad()
        _totalLecturas += 1
        _inicioLectura = ServerClock.Now()
        ErrUltHuella = False
        EstadoVerificado = False
        _eventoPersistidoEnTransaccion = False
        LimpiarResultadoProcesado()
        LimpiarContextoLecturaActual()

        Dim carnet As String = If(carnetRaw, String.Empty).Trim()
        _ultimoCarnetLeido = carnet
        UpdateVisualState(EstadoVisual.Processing)

        If carnet.Length = 0 Then
            _totalErrores += 1
            UpdateVisualState(EstadoVisual.NotFound)
            RegistrarTiempoAtencion()
            ActualizarKpisOperacion()
            EnsureScanFocus(True)
            Exit Sub
        End If

        If Not Cls.VereficaCarnet(carnet) Then
            _totalErrores += 1
            UpdateVisualState(EstadoVisual.NotFound)
            RegistrarTiempoAtencion()
            ActualizarKpisOperacion()
            EnsureScanFocus(True)
            Exit Sub
        End If

        Dim usuario As UsuarioTransporteSnapshot = BuscarUsuarioPorCedula(carnet)
        If usuario Is Nothing Then
            _totalErrores += 1
            UpdateVisualState(EstadoVisual.NotFound)
            RegistrarTiempoAtencion()
            ActualizarKpisOperacion()
            EnsureScanFocus(True)
            Exit Sub
        End If

        If String.Equals(Ulthuella, carnet, StringComparison.OrdinalIgnoreCase) Then
            ErrUltHuella = True
            _totalDuplicadas += 1
            UpdateVisualState(EstadoVisual.Duplicate)
            RegistrarTiempoAtencion()
            ActualizarKpisOperacion()
            EnsureScanFocus(True)
            Exit Sub
        End If

        ProcesarMarca(usuario)

        If EstadoVerificado Then
            _lecturasExitosas += 1
            UpdateVisualState(_estadoResultadoMarcaActual)
        Else
            _totalErrores += 1
            UpdateVisualState(EstadoVisual.ErrorGeneral)
        End If

        RegistrarTiempoAtencion()
        ActualizarKpisOperacion()
        EnsureScanFocus(True)
    End Sub

    Private Function BuscarUsuarioPorCedula(ByVal cedula As String) As UsuarioTransporteSnapshot
        If _usuariosPorCedula Is Nothing OrElse _usuariosPorCedula.Count = 0 Then
            Return Nothing
        End If

        Dim usuario As UsuarioTransporteSnapshot = Nothing
        If _usuariosPorCedula.TryGetValue(cedula, usuario) Then
            Return usuario
        End If

        Return Nothing
    End Function

    Private Function ObtenerEstadoPermisoSalida(ByVal usuario As UsuarioTransporteSnapshot) As EstadoPermisoSalida
        If usuario Is Nothing OrElse Not usuario.TienePermisoSalida Then
            Return EstadoPermisoSalida.NoRegistrado
        End If

        If usuario.PermisoSalida Then
            Return EstadoPermisoSalida.Autorizado
        End If

        Return EstadoPermisoSalida.NoAutorizado
    End Function

    Private Sub RecargarDatosOperacion()
        ' El modulo opera con snapshots indexados para que cada lectura del lector no recorra tablas completas.
        _usuariosPorCedula = ConstruirIndiceUsuarios(TransporteSvc.CargarUsuariosActivos(Cn))
        _rutasPorId = ConstruirIndiceRutas(TransporteSvc.CargarRutas(Cn))
    End Sub

    Private Function ConstruirIndiceUsuarios(ByVal dsUsuarios As DataSet) As Dictionary(Of String, UsuarioTransporteSnapshot)
        Dim indice As New Dictionary(Of String, UsuarioTransporteSnapshot)(StringComparer.OrdinalIgnoreCase)

        If dsUsuarios Is Nothing OrElse dsUsuarios.Tables.Count = 0 Then
            Return indice
        End If

        For Each row As DataRow In dsUsuarios.Tables(0).Rows
            Dim usuario As UsuarioTransporteSnapshot = MapearUsuario(row)
            Dim cedula As String = usuario.Cedula
            If cedula.Length > 0 AndAlso Not indice.ContainsKey(cedula) Then
                indice.Add(cedula, usuario)
            End If
        Next

        Return indice
    End Function

    Private Function ConstruirIndiceRutas(ByVal dsRutas As DataSet) As Dictionary(Of Integer, RutaSnapshot)
        Dim indice As New Dictionary(Of Integer, RutaSnapshot)()

        If dsRutas Is Nothing OrElse dsRutas.Tables.Count = 0 Then
            Return indice
        End If

        For Each row As DataRow In dsRutas.Tables(0).Rows
            If IsDBNull(row("IdRuta")) Then
                Continue For
            End If

            indice(CInt(row("IdRuta"))) = New RutaSnapshot With {
                .IdRuta = CInt(row("IdRuta")),
                .Codigo = CStr(row("Codigo")).Trim(),
                .Descripcion = CStr(row("Descripcion")).Trim()
            }
        Next

        Return indice
    End Function

    Private Function MapearUsuario(ByVal row As DataRow) As UsuarioTransporteSnapshot
        Dim usuario As New UsuarioTransporteSnapshot()
        usuario.IdUsuario = CInt(row("IdUsuario"))
        usuario.Nombre = CStr(row("Nombre")).Trim()
        usuario.PrimerApellido = CStr(row("PrimerApellido")).Trim()
        usuario.SegundoApellido = CStr(row("SegundoApellido")).Trim()
        usuario.CodTipo = CShort(row("CodTipo"))
        usuario.TieneRuta = Not IsDBNull(row("IdRuta"))
        If usuario.TieneRuta Then
            usuario.IdRuta = CInt(row("IdRuta"))
        End If
        usuario.Seccion = CStr(row("Seccion")).Trim()
        usuario.Cedula = CStr(row("Cedula")).Trim()
        usuario.IdHorario = CInt(row("IdHorario"))
        usuario.TienePermisoSalida = Not IsDBNull(row("PermisoSalida"))
        If usuario.TienePermisoSalida Then
            usuario.PermisoSalida = OperativeUiHelper.ConvertToBoolean(row("PermisoSalida"))
        End If
        Return usuario
    End Function

    Private Sub ProcesarMarca(ByVal usuario As UsuarioTransporteSnapshot)
        If Me.InvokeRequired Then
            Invoke(New UsuarioCall(AddressOf ProcesarMarca), usuario)
            Exit Sub
        End If

        Try
            Dim estadoPermiso As EstadoPermisoSalida = ObtenerEstadoPermisoSalida(usuario)

            LblCedula.Text = usuario.Cedula
            TxtUsuario.Text = usuario.NombreCompleto
            TxtSeccion.Text = usuario.Seccion
            TxtRuta.Text = String.Empty
            LblRuta.Text = String.Empty

            CargarDatosRuta(usuario)

            Select Case estadoPermiso
                Case EstadoPermisoSalida.Autorizado
                    TxtPermisoSalida.Text = "SI Autorizado"
                Case EstadoPermisoSalida.NoAutorizado
                    TxtPermisoSalida.Text = "NO Autorizado"
                Case Else
                    TxtPermisoSalida.Text = "NO Registrado"
            End Select
            AplicarEstiloPermisoSalida(estadoPermiso)

            Ulthuella = LblCedula.Text
            If usuario.CodTipo = 1S Then
                TxtTipo.Text = "ESTUDIANTE"
                LblTitulo.Text = "ESTUDIANTE: " & TxtUsuario.Text
            Else
                TxtTipo.Text = "PROF.: " & TxtUsuario.Text
                LblTitulo.Text = "PROF.: " & TxtUsuario.Text
            End If

            Dim resultadoMarca As TransporteDataService.RegistroMarcaResultado
            Dim estadoEventoTx As EstadoVisual = EstadoVisual.Success
            Dim mensajeResultado As String = String.Empty

            Using tx As SqlClient.SqlTransaction = Cn.BeginTransaction()
                resultadoMarca = TransporteSvc.RegistrarMarcaEnTransaccion(usuario.IdUsuario,
                                                                           usuario.IdHorario,
                                                                           usuario.CodTipo,
                                                                           If(usuario.TieneRuta, usuario.IdRuta, 0),
                                                                           Cn,
                                                                           FechaServer,
                                                                           tx)
                estadoEventoTx = DeterminarEstadoResultadoMarca(usuario, resultadoMarca, estadoPermiso, mensajeResultado)
                OperacionSvc.RegistrarEvento(
                    Cn,
                    ServerClock.Now(),
                    usuario.Cedula,
                    ObtenerCodigoEstado(estadoEventoTx),
                    mensajeResultado,
                    Nothing,
                    False,
                    estadoEventoTx = EstadoVisual.Warning,
                    False,
                    False,
                    tx)
                tx.Commit()
            End Using

            _estadoResultadoMarcaActual = estadoEventoTx
            _mensajeEstadoPersonalizado = mensajeResultado
            _resultadoOperacionPersonalizado = String.Empty
            _eventoPersistidoEnTransaccion = True
            _estadoEventoTransaccion = estadoEventoTx
            EstadoVerificado = True
        Catch ex As Exception
            EstadoVerificado = False
            _eventoPersistidoEnTransaccion = False
            ErrorLogger.LogException("ControlTransporte.ProcesarMarca", ex)
            UpdateVisualState(EstadoVisual.ErrorGeneral)
        End Try
    End Sub

    Private Function DeterminarEstadoResultadoMarca(ByVal usuario As UsuarioTransporteSnapshot,
                                                    ByVal resultadoMarca As TransporteDataService.RegistroMarcaResultado,
                                                    ByVal estadoPermiso As EstadoPermisoSalida,
                                                    ByRef mensajeResultado As String) As EstadoVisual
        ' La primera marca del estudiante siempre se trata como entrada exitosa; salida aplica desde la segunda en adelante.
        If EsEstudiante(usuario) AndAlso resultadoMarca.EsPrimeraMarcaEstudianteDelDia Then
            mensajeResultado = "Registro de entrada y ruta de transporte exitoso"
            Return EstadoVisual.Success
        End If

        Select Case estadoPermiso
            Case EstadoPermisoSalida.Autorizado
                mensajeResultado = "Marca Permiso de SALIDA registrada correctamente"
                Return EstadoVisual.Success
            Case EstadoPermisoSalida.NoAutorizado
                mensajeResultado = "NO PERMISO DE SALIDA - ADVERTENCIA NO PUEDE SALIR"
                Return EstadoVisual.Warning
            Case Else
                mensajeResultado = "PERMISO DE SALIDA NO REGISTRADO NO PUEDE SALIR"
                Return EstadoVisual.Warning
        End Select
    End Function

    Private Function EsEstudiante(ByVal usuario As UsuarioTransporteSnapshot) As Boolean
        Return usuario IsNot Nothing AndAlso usuario.CodTipo = 1S
    End Function

    Private Sub CargarDatosRuta(ByVal usuario As UsuarioTransporteSnapshot)
        If usuario Is Nothing OrElse Not usuario.TieneRuta Then
            Exit Sub
        End If

        Dim ruta As RutaSnapshot = Nothing
        If Not _rutasPorId.TryGetValue(usuario.IdRuta, ruta) Then
            Exit Sub
        End If

        TxtRuta.Text = ruta.Codigo
        LblRuta.Text = "Ruta: " & ruta.Descripcion
        LblRuta.Visible = True
    End Sub

    Private Sub LimpiarResultadoProcesado()
        _estadoResultadoMarcaActual = EstadoVisual.Success
        _mensajeEstadoPersonalizado = String.Empty
        _resultadoOperacionPersonalizado = String.Empty
    End Sub

    Protected Sub LimpiarPantalla(ByVal limpiar As Boolean)
        If Me.InvokeRequired Then
            Invoke(New BoolCall(AddressOf LimpiarPantalla), limpiar)
            Exit Sub
        End If

        Try
            ResetResultFields()
            UpdateVisualState(EstadoVisual.Idle)
            EnsureScanFocus(True)
        Catch ex As Exception
            ErrorLogger.LogException("ControlTransporte.LimpiarPantalla", ex)
        End Try
    End Sub

    Private Sub ResetResultFields()
        LimpiarResultadoProcesado()
        LimpiarContextoLecturaActual()
        TxtCedula.Clear()
    End Sub

    Private Sub LimpiarContextoLecturaActual()
        LblCedula.Clear()
        TxtTipo.Clear()
        TxtSeccion.Clear()
        TxtRuta.Clear()
        TxtUsuario.Clear()
        TxtPermisoSalida.Clear()
        LblRuta.Text = String.Empty
        LblRuta.Visible = False
        LblTitulo.Text = "Control de Marcas - Transporte"
        RestablecerEstiloPermisoSalida()
    End Sub

    Private Sub EnsureScanFocus(ByVal selectAll As Boolean)
        If Not TxtCedula.CanFocus Then
            ActualizarIndicadorFoco(False)
            Exit Sub
        End If
        TxtCedula.Focus()
        If selectAll Then
            TxtCedula.SelectAll()
        End If
        ActualizarIndicadorFoco(TxtCedula.Focused)
    End Sub

    Private Sub ActualizarIndicadorFoco(ByVal focusActivo As Boolean)
        If _lblFocusEscaneo Is Nothing Then
            Exit Sub
        End If

        If focusActivo Then
            _lblFocusEscaneo.Text = If(_mostrarEntradaManualTemporal, "Entrada manual visible temporal", "Lector listo (captura oculta)")
            _lblFocusEscaneo.ForeColor = Color.FromArgb(157, 230, 170)
        Else
            _lblFocusEscaneo.Text = "Atencion: foco fuera del lector (F3 para recuperar)"
            _lblFocusEscaneo.ForeColor = Color.FromArgb(255, 214, 170)
        End If
    End Sub

    Private Sub ApplyModernOperationalLayout()
        Me.BackColor = UIConstants.AppBackground
        Me.Font = UIConstants.FontBody()
        Me.KeyPreview = True
        Me.WindowState = FormWindowState.Maximized
        Me.FormBorderStyle = FormBorderStyle.None
        Me.ControlBox = False
        Me.StartPosition = FormStartPosition.CenterScreen

        PanelResult.BackColor = Color.FromArgb(242, 246, 252)
        PanelResult.BorderStyle = BorderStyle.None
        PanelResult.Dock = DockStyle.None
        PanelTopBar.Dock = DockStyle.None
        BunifuGradientPanel1.Dock = DockStyle.None
        PanelTopBar.Visible = False
        BtnCerrar.Visible = False
        BunifuGradientPanel1.BackgroundImage = Nothing

        LblTitulo.Font = New Font("Segoe UI Semibold", 19.5!, FontStyle.Bold)
        LblTitulo.ForeColor = Color.FromArgb(17, 33, 59)
        LblTitulo.Text = "Control de Marcas - Transporte"
        LblTitulo.AutoEllipsis = True
        LblTitulo.Padding = New Padding(18, 0, 18, 0)
        LblTitulo.TextAlign = ContentAlignment.MiddleCenter
        LblTitulo.BackColor = Color.FromArgb(229, 236, 246)

        lblProcesando.Font = New Font("Segoe UI Semibold", 28.0!, FontStyle.Bold)
        lblProcesando.ForeColor = Color.FromArgb(23, 32, 51)
        lblProcesando.Text = "Esperando lectura de carnet"
        lblProcesando.TextAlign = ContentAlignment.TopCenter

        LblFecha.ForeColor = Color.FromArgb(214, 226, 246)
        LblFecha.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
        LblFecha.TextAlign = ContentAlignment.MiddleLeft
        LblFecha.Padding = New Padding(22, 0, 0, 0)

        TxtCedula.Font = New Font("Segoe UI Semibold", 19.0!, FontStyle.Bold)
        TxtCedula.BorderStyle = BorderStyle.FixedSingle

        LblCedula.Font = New Font("Segoe UI", 12.5!, FontStyle.Bold)
        TxtUsuario.Font = New Font("Segoe UI Semibold", 16.0!, FontStyle.Bold)
        TxtTipo.Font = New Font("Segoe UI", 12.5!, FontStyle.Bold)
        TxtRuta.Font = New Font("Segoe UI", 12.5!, FontStyle.Bold)
        TxtSeccion.Font = New Font("Segoe UI", 12.5!, FontStyle.Bold)
        TxtPermisoSalida.Font = New Font("Segoe UI", 12.5!, FontStyle.Bold)
        AplicarEstiloCampoLectura(LblCedula)
        AplicarEstiloCampoLectura(TxtUsuario)
        AplicarEstiloCampoLectura(TxtTipo)
        AplicarEstiloCampoLectura(TxtRuta)
        AplicarEstiloCampoLectura(TxtSeccion)
        AplicarEstiloCampoLectura(TxtPermisoSalida)
        TxtUsuario.BackColor = Color.FromArgb(228, 236, 247)
        TxtUsuario.ForeColor = Color.FromArgb(14, 32, 57)
        RestablecerEstiloPermisoSalida()

        BunifuGradientPanel1.BackColor = Color.FromArgb(13, 30, 54)
        GbDatos.BackColor = Color.FromArgb(252, 253, 255)
        GbDatos.ForeColor = Color.FromArgb(36, 51, 77)
        GbDatos.FlatStyle = FlatStyle.Flat
        GbDatos.Text = "Lectura actual"
        GbDatos.Font = New Font("Segoe UI", 10.5!, FontStyle.Bold)
        GbDatos.Padding = New Padding(12, 14, 12, 12)

        LblPermisoSalidaCaption.ForeColor = Color.FromArgb(76, 90, 112)
        LblUsuarioCaption.ForeColor = Color.FromArgb(76, 90, 112)
        LblSeccionCaption.ForeColor = Color.FromArgb(76, 90, 112)
        LblCedulaCaption.ForeColor = Color.FromArgb(76, 90, 112)
        LblTipoCaption.ForeColor = Color.FromArgb(76, 90, 112)
        LblRutaCaption.ForeColor = Color.FromArgb(76, 90, 112)

        LblUsuarioCaption.Text = "Estudiante"
        AplicarCamposLecturaNoFocusables()

        TxtCedula.Visible = True
        TxtCedula.BringToFront()
        EnsureSidebarKioskControls()
        EnsureSidebarDecorators()
        _lblScanHint.Text = ObtenerTextoScanHint()
        _lblScanHint.AutoSize = False
        _lblScanHint.TextAlign = ContentAlignment.MiddleLeft
        _lblScanHint.Font = New Font("Segoe UI", 11.0!, FontStyle.Bold)
        _lblScanHint.BackColor = Color.FromArgb(18, 42, 74)
        _lblScanHint.Padding = New Padding(14, 0, 14, 0)
        _lblFocusEscaneo.BackColor = Color.FromArgb(15, 36, 63)
        _lblFocusEscaneo.Padding = New Padding(12, 0, 12, 0)
        Picture.Visible = False

        LblRuta.BackColor = Color.FromArgb(238, 242, 247)
        LblRuta.ForeColor = Color.FromArgb(56, 68, 86)
        LblRuta.BorderStyle = BorderStyle.None
        LblRuta.Font = New Font("Segoe UI", 10.5!, FontStyle.Bold)
        LblRuta.AutoEllipsis = True
        LblRuta.Padding = New Padding(12, 0, 12, 0)
        LblRuta.TextAlign = ContentAlignment.MiddleCenter
        LblRuta.Visible = False
        AplicarModoAltoContraste()
        AplicarPrivacidadEscaneo()
    End Sub

    Private Sub AplicarCamposLecturaNoFocusables()
        OperativeUiHelper.DisableFocus(LblCedula, TxtUsuario, TxtTipo, TxtSeccion, TxtRuta, TxtPermisoSalida, BtnCerrar)
    End Sub

    Private Sub ApplyResponsiveLayout()
        If _lblConexion Is Nothing OrElse _lblHistorial Is Nothing OrElse _lstHistorial Is Nothing Then
            Return
        End If

        Dim mode As LayoutMode = ObtenerModoLayout()
        ApplyResponsiveTypography(mode)

        Dim leftWidth As Integer
        Dim pad As Integer
        Dim fechaHeight As Integer
        Dim topY As Integer
        Dim scanHintHeight As Integer
        Dim focusHeight As Integer
        Dim cedulaHeight As Integer
        Dim footerReserve As Integer
        Dim minCardHeight As Integer
        Dim titleTop As Integer
        Dim titleHeight As Integer
        Dim routeTopOffset As Integer
        Dim routeHeight As Integer
        Dim chipWidth As Integer
        Dim chipHeight As Integer
        Dim chipTop As Integer
        Dim messageTop As Integer
        Dim messageHeight As Integer
        Dim statusPadding As Integer
        Dim titleInset As Integer
        Dim iconMin As Integer
        Dim iconMax As Integer
        Dim iconFactor As Double

        Select Case mode
            Case LayoutMode.Narrow1366
                leftWidth = Math.Max(360, Math.Min(392, CInt(Math.Round(Me.ClientSize.Width * 0.285R))))
                pad = 14
                fechaHeight = 46
                topY = 66
                scanHintHeight = 38
                focusHeight = 18
                cedulaHeight = 42
                footerReserve = 160
                minCardHeight = 258
                titleTop = 14
                titleHeight = 34
                routeTopOffset = 12
                routeHeight = 24
                chipWidth = 224
                chipHeight = 28
                chipTop = 94
                messageTop = 142
                messageHeight = 84
                statusPadding = 52
                titleInset = 24
                iconMin = 140
                iconMax = 198
                iconFactor = 0.205R
            Case LayoutMode.Compact
                leftWidth = Math.Max(390, Math.Min(430, CInt(Math.Round(Me.ClientSize.Width * 0.292R))))
                pad = 16
                fechaHeight = 48
                topY = 72
                scanHintHeight = 40
                focusHeight = 18
                cedulaHeight = 44
                footerReserve = 176
                minCardHeight = 292
                titleTop = 16
                titleHeight = 36
                routeTopOffset = 14
                routeHeight = 26
                chipWidth = 232
                chipHeight = 30
                chipTop = 106
                messageTop = 156
                messageHeight = 92
                statusPadding = 60
                titleInset = 34
                iconMin = 156
                iconMax = 214
                iconFactor = 0.225R
            Case LayoutMode.Wide
                leftWidth = Math.Max(460, Math.Min(520, CInt(Math.Round(Me.ClientSize.Width * 0.305R))))
                pad = 24
                fechaHeight = 56
                topY = 84
                scanHintHeight = 46
                focusHeight = 20
                cedulaHeight = 48
                footerReserve = 214
                minCardHeight = 352
                titleTop = 20
                titleHeight = 42
                routeTopOffset = 16
                routeHeight = 30
                chipWidth = 246
                chipHeight = 32
                chipTop = 118
                messageTop = 176
                messageHeight = 112
                statusPadding = 88
                titleInset = 60
                iconMin = 190
                iconMax = 270
                iconFactor = 0.28R
            Case Else
                leftWidth = Math.Max(420, Math.Min(470, CInt(Math.Round(Me.ClientSize.Width * 0.3R))))
                pad = 20
                fechaHeight = 52
                topY = 78
                scanHintHeight = 44
                focusHeight = 20
                cedulaHeight = 46
                footerReserve = 196
                minCardHeight = 328
                titleTop = 18
                titleHeight = 38
                routeTopOffset = 15
                routeHeight = 28
                chipWidth = 238
                chipHeight = 30
                chipTop = 112
                messageTop = 166
                messageHeight = 104
                statusPadding = 72
                titleInset = 48
                iconMin = 170
                iconMax = 240
                iconFactor = 0.248R
        End Select

        BunifuGradientPanel1.Width = leftWidth
        BunifuGradientPanel1.Left = 0
        BunifuGradientPanel1.Top = 0
        BunifuGradientPanel1.Height = Me.ClientSize.Height
        PanelTopBar.Left = leftWidth + pad
        PanelTopBar.Top = 0
        PanelTopBar.Width = Math.Max(320, Me.ClientSize.Width - leftWidth - (pad * 2))
        PanelResult.Left = leftWidth + pad
        PanelResult.Top = 0
        PanelResult.Width = Math.Max(420, Me.ClientSize.Width - leftWidth - (pad * 2))
        PanelResult.Height = Me.ClientSize.Height

        Dim innerX As Integer = 20
        Dim innerW As Integer = leftWidth - 40

        LblFecha.SetBounds(0, 0, leftWidth, fechaHeight)
        Picture.SetBounds(0, 0, 0, 0)
        If _lblScanHint IsNot Nothing Then
            _lblScanHint.SetBounds(innerX, topY, innerW, scanHintHeight)
        End If
        If _lblFocusEscaneo IsNot Nothing Then
            _lblFocusEscaneo.SetBounds(innerX, _lblScanHint.Bottom + 6, innerW, focusHeight)
        End If
        If _mostrarEntradaManualTemporal Then
            TxtCedula.SetBounds(innerX, _lblScanHint.Bottom + 26, innerW, cedulaHeight)
        Else
            TxtCedula.SetBounds(2, 2, 1, 1)
        End If

        Dim gbTop As Integer = If(_mostrarEntradaManualTemporal, TxtCedula.Bottom + 12, _lblFocusEscaneo.Bottom + 16)
        Dim gbHeight As Integer = Math.Max(minCardHeight, BunifuGradientPanel1.ClientSize.Height - gbTop - footerReserve)
        GbDatos.SetBounds(12, gbTop, leftWidth - 24, gbHeight)
        If _panelSidebarDividerDatos IsNot Nothing Then
            _panelSidebarDividerDatos.SetBounds(innerX, gbTop - 14, innerW, 1)
        End If

        Dim gX As Integer = 14
        Dim gW As Integer = GbDatos.ClientSize.Width - 28
        Dim rowTop As Integer
        Dim captionHeight As Integer
        Dim rowGap As Integer
        Dim rowHeight As Integer
        Dim nombreHeight As Integer
        Dim historyMinHeight As Integer

        Select Case mode
            Case LayoutMode.Narrow1366
                rowTop = 22
                captionHeight = 16
                rowGap = 44
                rowHeight = 28
                nombreHeight = 42
                historyMinHeight = 48
            Case LayoutMode.Compact
                rowTop = 24
                captionHeight = 18
                rowGap = 48
                rowHeight = 30
                nombreHeight = 46
                historyMinHeight = 54
            Case LayoutMode.Wide
                rowTop = 28
                captionHeight = 18
                rowGap = 56
                rowHeight = 34
                nombreHeight = 52
                historyMinHeight = 74
            Case Else
                rowTop = 26
                captionHeight = 18
                rowGap = 52
                rowHeight = 32
                nombreHeight = 48
                historyMinHeight = 64
        End Select

        Dim historialTop As Integer
        If mode = LayoutMode.Narrow1366 OrElse mode = LayoutMode.Compact Then
            LblUsuarioCaption.SetBounds(gX, rowTop, gW, captionHeight)
            TxtUsuario.SetBounds(gX, rowTop + captionHeight + 2, gW, nombreHeight)

            Dim row2Top As Integer = TxtUsuario.Bottom + 12
            LblCedulaCaption.SetBounds(gX, row2Top, gW, captionHeight)
            LblCedula.SetBounds(gX, row2Top + captionHeight + 2, gW, rowHeight)
            LblTipoCaption.SetBounds(gX, LblCedula.Bottom + 10, gW, captionHeight)
            TxtTipo.SetBounds(gX, LblTipoCaption.Bottom + 2, gW, rowHeight)
            LblSeccionCaption.SetBounds(gX, TxtTipo.Bottom + 10, gW, captionHeight)
            TxtSeccion.SetBounds(gX, LblSeccionCaption.Bottom + 2, gW, rowHeight)
            LblRutaCaption.SetBounds(gX, TxtSeccion.Bottom + 10, gW, captionHeight)
            TxtRuta.SetBounds(gX, LblRutaCaption.Bottom + 2, gW, rowHeight)
            LblPermisoSalidaCaption.SetBounds(gX, TxtRuta.Bottom + 10, gW, captionHeight)
            TxtPermisoSalida.SetBounds(gX, LblPermisoSalidaCaption.Bottom + 2, gW, rowHeight)
            historialTop = TxtPermisoSalida.Bottom + 10
        Else
            Dim colGap As Integer = 12
            Dim colW As Integer = (gW - colGap) \ 2
            LblUsuarioCaption.SetBounds(gX, rowTop, gW, captionHeight)
            TxtUsuario.SetBounds(gX, rowTop + captionHeight + 2, gW, nombreHeight)

            Dim row2Top As Integer = TxtUsuario.Bottom + 12
            LblCedulaCaption.SetBounds(gX, row2Top, colW, captionHeight)
            LblCedula.SetBounds(gX, row2Top + captionHeight + 2, colW, rowHeight)
            LblTipoCaption.SetBounds(gX + colW + colGap, row2Top, colW, captionHeight)
            TxtTipo.SetBounds(gX + colW + colGap, row2Top + captionHeight + 2, colW, rowHeight)

            Dim row3Top As Integer = row2Top + rowGap
            LblSeccionCaption.SetBounds(gX, row3Top, colW, captionHeight)
            TxtSeccion.SetBounds(gX, row3Top + captionHeight + 2, colW, rowHeight)
            LblRutaCaption.SetBounds(gX + colW + colGap, row3Top, colW, captionHeight)
            TxtRuta.SetBounds(gX + colW + colGap, row3Top + captionHeight + 2, colW, rowHeight)

            Dim row4Top As Integer = row3Top + rowGap
            LblPermisoSalidaCaption.SetBounds(gX, row4Top, gW, captionHeight)
            TxtPermisoSalida.SetBounds(gX, row4Top + captionHeight + 2, gW, rowHeight)
            historialTop = row4Top + rowGap + 10
        End If

        If _lblHistorial IsNot Nothing Then
            _lblHistorial.SetBounds(gX, historialTop, gW, captionHeight)
            _lstHistorial.SetBounds(gX, historialTop + captionHeight + 4, gW, Math.Max(historyMinHeight, GbDatos.ClientSize.Height - (historialTop + captionHeight + 16)))
        End If

        Dim statusW As Integer = PanelResult.ClientSize.Width
        Dim statusH As Integer = PanelResult.ClientSize.Height
        Dim tituloW As Integer = Math.Max(260, Math.Min(statusW - (titleInset * 2), 780))
        Dim tituloX As Integer = Math.Max(36, (statusW - tituloW) \ 2)
        Dim rutaW As Integer = Math.Max(220, Math.Min(statusW - (titleInset * 3), 540))
        Dim rutaX As Integer = Math.Max(54, (statusW - rutaW) \ 2)
        LblTitulo.SetBounds(tituloX, titleTop, tituloW, titleHeight)
        LblRuta.SetBounds(rutaX, LblTitulo.Bottom + routeTopOffset, rutaW, routeHeight)

        If _lblResultadoOperacion IsNot Nothing Then
            _lblResultadoOperacion.SetBounds(36, Math.Max(chipTop - 30, LblRuta.Bottom + 10), Math.Max(280, statusW - 72), 28)
        End If
        If _lblEstadoChip IsNot Nothing Then
            _lblEstadoChip.SetBounds((statusW - chipWidth) \ 2, Math.Max(LblRuta.Bottom + 12, chipTop), chipWidth, chipHeight)
        End If

        lblProcesando.SetBounds(statusPadding, Math.Max(_lblEstadoChip.Bottom + 18, messageTop), Math.Max(280, statusW - (statusPadding * 2)), messageHeight)

        Dim iconSize As Integer = Math.Max(iconMin, Math.Min(iconMax, CInt(Math.Round(Math.Min(statusW, statusH) * iconFactor))))
        Dim iconX As Integer = Math.Max(0, (statusW - iconSize) \ 2)
        Dim iconY As Integer = Math.Max(lblProcesando.Bottom + 12, (statusH - iconSize) \ 2 + If(mode = LayoutMode.Narrow1366, 8, 14))
        Imgprocess.SetBounds(iconX, iconY, iconSize, iconSize)

        ApplySidebarOperationalInfoLayout(leftWidth, mode)
        AplicarDecoracionOperativa()
    End Sub

    Private Sub ApplySidebarOperationalInfoLayout(ByVal leftWidth As Integer, ByVal mode As LayoutMode)
        If _lblConexion Is Nothing OrElse _lblKpi Is Nothing OrElse _lblUltimaLectura Is Nothing OrElse _lblEdadEstado Is Nothing OrElse _btnIncidencia Is Nothing OrElse _lblHotkeys Is Nothing Then
            Exit Sub
        End If

        Dim sidebarHeight As Integer = Math.Max(600, BunifuGradientPanel1.ClientSize.Height)
        Dim buttonWidth As Integer
        Dim buttonHeight As Integer
        Dim hotkeysHeight As Integer
        Dim lineHeight As Integer
        Dim footerBottom As Integer

        Select Case mode
            Case LayoutMode.Narrow1366
                buttonWidth = 152
                buttonHeight = 32
                hotkeysHeight = 24
                lineHeight = 18
                footerBottom = 8
            Case LayoutMode.Compact
                buttonWidth = 160
                buttonHeight = 32
                hotkeysHeight = 26
                lineHeight = 18
                footerBottom = 8
            Case LayoutMode.Wide
                buttonWidth = 176
                buttonHeight = 36
                hotkeysHeight = 30
                lineHeight = 20
                footerBottom = 4
            Case Else
                buttonWidth = 170
                buttonHeight = 34
                hotkeysHeight = 28
                lineHeight = 20
                footerBottom = 6
        End Select

        Dim hotkeysTop As Integer = sidebarHeight - hotkeysHeight - footerBottom
        Dim edadTop As Integer = hotkeysTop - lineHeight - 4
        Dim ultimaTop As Integer = edadTop - lineHeight
        Dim kpiTop As Integer = ultimaTop - lineHeight
        Dim conexionTop As Integer = kpiTop - lineHeight
        Dim incidenciaTop As Integer = Math.Max(GbDatos.Bottom + 10, conexionTop - buttonHeight - 10)

        _btnIncidencia.SetBounds(20, incidenciaTop, buttonWidth, buttonHeight)
        If _panelSidebarDividerMetricas IsNot Nothing Then
            _panelSidebarDividerMetricas.SetBounds(20, incidenciaTop - 12, leftWidth - 40, 1)
        End If
        _lblConexion.SetBounds(20, conexionTop, leftWidth - 40, 20)
        _lblKpi.SetBounds(20, kpiTop, leftWidth - 40, 20)
        _lblUltimaLectura.SetBounds(20, ultimaTop, leftWidth - 40, 20)
        _lblEdadEstado.SetBounds(20, edadTop, leftWidth - 40, 20)
        _lblHotkeys.SetBounds(20, hotkeysTop, leftWidth - 40, hotkeysHeight)
    End Sub

    Private Sub ApplyResponsiveTypography(ByVal mode As LayoutMode)
        Dim titleSize As Single
        Dim routeSize As Single
        Dim messageSize As Single
        Dim dateSize As Single
        Dim scanHintSize As Single
        Dim cardCaptionSize As Single
        Dim cardValueSize As Single
        Dim permisoSize As Single
        Dim telemetrySize As Single
        Dim hotkeySize As Single
        Dim buttonSize As Single

        Select Case mode
            Case LayoutMode.Narrow1366
                titleSize = 18.0!
                routeSize = 9.2!
                messageSize = 22.0!
                dateSize = 9.0!
                scanHintSize = 10.0!
                cardCaptionSize = 9.0!
                cardValueSize = 11.0!
                permisoSize = 10.8!
                telemetrySize = 8.8!
                hotkeySize = 7.8!
                buttonSize = 8.4!
                TxtUsuario.Font = New Font("Segoe UI Semibold", 14.5!, FontStyle.Bold)
            Case LayoutMode.Compact
                titleSize = 19.0!
                routeSize = 9.8!
                messageSize = 24.0!
                dateSize = 9.5!
                scanHintSize = 10.5!
                cardCaptionSize = 9.4!
                cardValueSize = 11.5!
                permisoSize = 11.2!
                telemetrySize = 9.0!
                hotkeySize = 8.0!
                buttonSize = 8.6!
                TxtUsuario.Font = New Font("Segoe UI Semibold", 15.5!, FontStyle.Bold)
            Case LayoutMode.Wide
                titleSize = 22.0!
                routeSize = 10.8!
                messageSize = 30.0!
                dateSize = 10.5!
                scanHintSize = 11.5!
                cardCaptionSize = 10.0!
                cardValueSize = 12.8!
                permisoSize = 12.2!
                telemetrySize = 10.0!
                hotkeySize = 8.8!
                buttonSize = 9.0!
                TxtUsuario.Font = New Font("Segoe UI Semibold", 18.0!, FontStyle.Bold)
            Case Else
                titleSize = 20.0!
                routeSize = 10.3!
                messageSize = 26.0!
                dateSize = 10.0!
                scanHintSize = 11.0!
                cardCaptionSize = 9.8!
                cardValueSize = 12.2!
                permisoSize = 11.8!
                telemetrySize = 9.6!
                hotkeySize = 8.4!
                buttonSize = 8.8!
                TxtUsuario.Font = New Font("Segoe UI Semibold", 16.5!, FontStyle.Bold)
        End Select

        LblFecha.Font = New Font("Segoe UI", dateSize, FontStyle.Bold)
        If _lblScanHint IsNot Nothing Then
            _lblScanHint.Font = New Font("Segoe UI", scanHintSize, FontStyle.Bold)
        End If
        TxtCedula.Font = New Font("Segoe UI Semibold", Math.Max(16.0!, cardValueSize + 4.5!), FontStyle.Bold)
        LblTitulo.Font = New Font("Segoe UI Semibold", titleSize, FontStyle.Bold)
        LblRuta.Font = New Font("Segoe UI", routeSize, FontStyle.Bold)
        lblProcesando.Font = New Font("Segoe UI Semibold", messageSize, FontStyle.Bold)

        Dim captionFont As New Font("Segoe UI", cardCaptionSize, FontStyle.Bold)
        Dim valueFont As New Font("Segoe UI", cardValueSize, FontStyle.Bold)
        LblCedulaCaption.Font = captionFont
        LblUsuarioCaption.Font = captionFont
        LblTipoCaption.Font = captionFont
        LblSeccionCaption.Font = captionFont
        LblRutaCaption.Font = captionFont
        LblPermisoSalidaCaption.Font = captionFont

        LblCedula.Font = valueFont
        TxtTipo.Font = valueFont
        TxtSeccion.Font = valueFont
        TxtRuta.Font = valueFont
        TxtPermisoSalida.Font = New Font("Segoe UI", permisoSize, FontStyle.Bold)
        GbDatos.Font = New Font("Segoe UI", cardCaptionSize + 0.8!, FontStyle.Bold)

        If _lblResultadoOperacion IsNot Nothing Then
            _lblResultadoOperacion.Font = New Font("Segoe UI", Math.Max(12.0!, messageSize - 9.0!), FontStyle.Bold)
        End If
        If _lblEstadoChip IsNot Nothing Then
            _lblEstadoChip.Font = New Font("Segoe UI", Math.Max(8.2!, scanHintSize - 1.0!), FontStyle.Bold)
        End If

        _lblConexion.Font = New Font("Segoe UI", telemetrySize, FontStyle.Bold)
        _lblKpi.Font = New Font("Segoe UI", telemetrySize, FontStyle.Bold)
        _lblUltimaLectura.Font = New Font("Segoe UI", telemetrySize, FontStyle.Bold)
        _lblEdadEstado.Font = New Font("Segoe UI", telemetrySize, FontStyle.Bold)
        _lblHistorial.Font = New Font("Segoe UI", cardCaptionSize, FontStyle.Bold)
        _lstHistorial.Font = New Font("Segoe UI", Math.Max(8.6!, telemetrySize), FontStyle.Regular)
        _lblHotkeys.Font = New Font("Segoe UI", hotkeySize, FontStyle.Bold)
        _btnIncidencia.Font = New Font("Segoe UI", buttonSize, FontStyle.Bold)
    End Sub

    Private Sub AplicarEstiloCampoLectura(ByVal caja As TextBox)
        OperativeUiHelper.ApplyReadOnlyDisplayField(caja,
                                                    Color.FromArgb(246, 249, 252),
                                                    Color.FromArgb(17, 33, 59))
    End Sub

    Private Sub RestablecerEstiloPermisoSalida()
        TxtPermisoSalida.BackColor = Color.FromArgb(246, 249, 252)
        TxtPermisoSalida.ForeColor = Color.FromArgb(17, 33, 59)
    End Sub

    Private Sub AplicarEstiloPermisoSalida(ByVal estadoPermiso As EstadoPermisoSalida)
        Select Case estadoPermiso
            Case EstadoPermisoSalida.Autorizado
                TxtPermisoSalida.BackColor = Color.FromArgb(220, 252, 231)
                TxtPermisoSalida.ForeColor = Color.FromArgb(22, 101, 52)
            Case EstadoPermisoSalida.NoAutorizado
                TxtPermisoSalida.BackColor = Color.FromArgb(254, 243, 199)
                TxtPermisoSalida.ForeColor = Color.FromArgb(146, 64, 14)
            Case Else
                TxtPermisoSalida.BackColor = Color.FromArgb(255, 247, 237)
                TxtPermisoSalida.ForeColor = Color.FromArgb(154, 52, 18)
        End Select
    End Sub

    Private Sub EnsureSidebarDecorators()
        If _panelSidebarDividerDatos Is Nothing Then
            _panelSidebarDividerDatos = New Panel()
            _panelSidebarDividerDatos.BackColor = Color.FromArgb(44, 69, 102)
            BunifuGradientPanel1.Controls.Add(_panelSidebarDividerDatos)
            _panelSidebarDividerDatos.BringToFront()
        End If

        If _panelSidebarDividerMetricas Is Nothing Then
            _panelSidebarDividerMetricas = New Panel()
            _panelSidebarDividerMetricas.BackColor = Color.FromArgb(44, 69, 102)
            BunifuGradientPanel1.Controls.Add(_panelSidebarDividerMetricas)
            _panelSidebarDividerMetricas.BringToFront()
        End If
    End Sub

    Private Sub AplicarDecoracionOperativa()
        AplicarRadioControl(LblTitulo, 18)
        AplicarRadioControl(LblRuta, 14)
        AplicarRadioControl(_lblEstadoChip, 14)
        AplicarRadioControl(_btnIncidencia, 12)
        AplicarRadioControl(_lstHistorial, 12)
        AplicarRadioControl(_lblScanHint, 12)
        AplicarRadioControl(_lblFocusEscaneo, 10)
    End Sub

    Private Sub AplicarRadioControl(ByVal control As Control, ByVal radio As Integer)
        If control Is Nothing OrElse control.Width <= 1 OrElse control.Height <= 1 Then
            Exit Sub
        End If

        Using path As GraphicsPath = CrearRutaRedondeada(New Rectangle(0, 0, control.Width, control.Height), radio)
            control.Region = New Region(path)
        End Using
    End Sub

    Private Function CrearRutaRedondeada(ByVal area As Rectangle, ByVal radio As Integer) As GraphicsPath
        Dim ajuste As Integer = Math.Max(2, radio * 2)
        Dim ruta As New GraphicsPath()
        ruta.StartFigure()
        ruta.AddArc(area.X, area.Y, ajuste, ajuste, 180, 90)
        ruta.AddArc(area.Right - ajuste, area.Y, ajuste, ajuste, 270, 90)
        ruta.AddArc(area.Right - ajuste, area.Bottom - ajuste, ajuste, ajuste, 0, 90)
        ruta.AddArc(area.X, area.Bottom - ajuste, ajuste, ajuste, 90, 90)
        ruta.CloseFigure()
        Return ruta
    End Function

    Private Function ObtenerModoLayout() As LayoutMode
        Return CType(OperativeUiHelper.ResolveLayoutBand(Me.ClientSize), LayoutMode)
    End Function

    Private Sub AplicarModoAltoContraste()
        If _modoAltoContraste Then
            PanelResult.BackColor = Color.Black
            lblProcesando.ForeColor = Color.White
            LblTitulo.ForeColor = Color.White
            LblTitulo.BackColor = Color.FromArgb(32, 32, 32)
            LblRuta.BackColor = Color.FromArgb(24, 24, 24)
            If _lblScanHint IsNot Nothing Then
                _lblScanHint.ForeColor = Color.White
                _lblScanHint.BackColor = Color.FromArgb(26, 26, 26)
            End If
            If _lblFocusEscaneo IsNot Nothing Then
                _lblFocusEscaneo.BackColor = Color.FromArgb(20, 20, 20)
            End If
            TxtCedula.BackColor = Color.Black
            TxtCedula.ForeColor = Color.White
            TxtCedula.BorderStyle = BorderStyle.Fixed3D
            If _lblEstadoChip IsNot Nothing Then
                _lblEstadoChip.BackColor = Color.Gold
                _lblEstadoChip.ForeColor = Color.Black
            End If
            If _lstHistorial IsNot Nothing Then
                _lstHistorial.BackColor = Color.Black
                _lstHistorial.ForeColor = Color.White
            End If
        Else
            TxtCedula.BackColor = Color.White
            TxtCedula.ForeColor = Color.FromArgb(17, 33, 59)
            TxtCedula.BorderStyle = BorderStyle.FixedSingle
            If _lblScanHint IsNot Nothing Then
                _lblScanHint.ForeColor = Color.FromArgb(220, 232, 252)
                _lblScanHint.BackColor = Color.FromArgb(18, 42, 74)
            End If
            If _lblFocusEscaneo IsNot Nothing Then
                _lblFocusEscaneo.BackColor = Color.FromArgb(15, 36, 63)
            End If
            If _lstHistorial IsNot Nothing Then
                _lstHistorial.BackColor = Color.FromArgb(247, 250, 252)
                _lstHistorial.ForeColor = Color.FromArgb(31, 41, 55)
            End If
        End If
    End Sub

    Private Sub EnsureSidebarKioskControls()
        If _lblScanHint Is Nothing Then
            _lblScanHint = New Label()
            _lblScanHint.AutoSize = False
            _lblScanHint.ForeColor = Color.FromArgb(220, 232, 252)
            _lblScanHint.Font = New Font("Segoe UI", 11.0!, FontStyle.Bold)
            _lblScanHint.Text = ObtenerTextoScanHint()
            BunifuGradientPanel1.Controls.Add(_lblScanHint)
            _lblScanHint.BringToFront()
        End If

        If _lblFocusEscaneo Is Nothing Then
            _lblFocusEscaneo = New Label()
            _lblFocusEscaneo.AutoSize = False
            _lblFocusEscaneo.ForeColor = Color.FromArgb(157, 230, 170)
            _lblFocusEscaneo.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
            _lblFocusEscaneo.Text = "Lector listo (captura oculta)"
            _lblFocusEscaneo.TextAlign = ContentAlignment.MiddleLeft
            BunifuGradientPanel1.Controls.Add(_lblFocusEscaneo)
            _lblFocusEscaneo.BringToFront()
        End If

        If _lblHotkeys Is Nothing Then
            _lblHotkeys = New Label()
            _lblHotkeys.AutoSize = False
            _lblHotkeys.ForeColor = Color.FromArgb(194, 214, 243)
            _lblHotkeys.Font = New Font("Segoe UI", 8.5!, FontStyle.Bold)
            _lblHotkeys.TextAlign = ContentAlignment.MiddleLeft
            _lblHotkeys.Text = ObtenerTextoHotkeys()
            BunifuGradientPanel1.Controls.Add(_lblHotkeys)
            _lblHotkeys.BringToFront()
        End If
    End Sub

    Private Sub InicializarControlesOperacion()
        If _lblConexion IsNot Nothing Then
            Exit Sub
        End If

        ' El mensaje principal vive en lblProcesando; este label queda solo como soporte interno para no duplicar la UI.
        _lblResultadoOperacion = New Label()
        _lblResultadoOperacion.AutoSize = False
        _lblResultadoOperacion.ForeColor = Color.FromArgb(17, 33, 59)
        _lblResultadoOperacion.BackColor = Color.Transparent
        _lblResultadoOperacion.Font = New Font("Segoe UI", 13.0!, FontStyle.Bold)
        _lblResultadoOperacion.TextAlign = ContentAlignment.MiddleCenter
        _lblResultadoOperacion.Text = "SIN LECTURA"
        _lblResultadoOperacion.Visible = False

        _lblEstadoChip = New Label()
        _lblEstadoChip.AutoSize = False
        _lblEstadoChip.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
        _lblEstadoChip.ForeColor = Color.White
        _lblEstadoChip.BackColor = Color.FromArgb(51, 65, 85)
        _lblEstadoChip.TextAlign = ContentAlignment.MiddleCenter
        _lblEstadoChip.Text = "ESTADO: EN ESPERA"

        _lblConexion = New Label()
        _lblConexion.AutoSize = False
        _lblConexion.ForeColor = Color.FromArgb(220, 232, 252)
        _lblConexion.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)

        _lblKpi = New Label()
        _lblKpi.AutoSize = False
        _lblKpi.ForeColor = Color.FromArgb(220, 232, 252)
        _lblKpi.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)

        _lblUltimaLectura = New Label()
        _lblUltimaLectura.AutoSize = False
        _lblUltimaLectura.ForeColor = Color.FromArgb(220, 232, 252)
        _lblUltimaLectura.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
        _lblUltimaLectura.Text = "Ultima lectura: --"

        _lblEdadEstado = New Label()
        _lblEdadEstado.AutoSize = False
        _lblEdadEstado.ForeColor = Color.FromArgb(255, 224, 171)
        _lblEdadEstado.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
        _lblEdadEstado.Text = "Estado visible: sin evento"

        _lblHistorial = New Label()
        _lblHistorial.AutoSize = False
        _lblHistorial.Font = New Font("Segoe UI", 8.75!, FontStyle.Bold)
        _lblHistorial.ForeColor = Color.FromArgb(76, 90, 112)
        _lblHistorial.Text = "Ultimos 10 eventos"

        _lstHistorial = New ListBox()
        _lstHistorial.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular)
        _lstHistorial.HorizontalScrollbar = False
        _lstHistorial.IntegralHeight = False
        _lstHistorial.BorderStyle = BorderStyle.FixedSingle
        _lstHistorial.BackColor = Color.FromArgb(247, 250, 252)
        _lstHistorial.ForeColor = Color.FromArgb(31, 41, 55)
        _lstHistorial.DrawMode = DrawMode.OwnerDrawFixed
        _lstHistorial.ItemHeight = 32
        AddHandler _lstHistorial.DrawItem, AddressOf Historial_DrawItem

        _btnIncidencia = New Button()
        _btnIncidencia.Text = "Incidencia rápida"
        _btnIncidencia.FlatStyle = FlatStyle.Flat
        _btnIncidencia.FlatAppearance.BorderSize = 0
        _btnIncidencia.BackColor = Color.FromArgb(194, 120, 39)
        _btnIncidencia.ForeColor = Color.White
        _btnIncidencia.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
        AddHandler _btnIncidencia.Click, AddressOf BtnIncidencia_Click

        PanelResult.Controls.Add(_lblResultadoOperacion)
        PanelResult.Controls.Add(_lblEstadoChip)
        BunifuGradientPanel1.Controls.Add(_lblConexion)
        BunifuGradientPanel1.Controls.Add(_lblKpi)
        BunifuGradientPanel1.Controls.Add(_lblUltimaLectura)
        BunifuGradientPanel1.Controls.Add(_lblEdadEstado)
        BunifuGradientPanel1.Controls.Add(_btnIncidencia)
        GbDatos.Controls.Add(_lblHistorial)
        GbDatos.Controls.Add(_lstHistorial)

        _lblResultadoOperacion.BringToFront()
        _lblEstadoChip.BringToFront()
        _lblConexion.BringToFront()
        _lblKpi.BringToFront()
        _lblUltimaLectura.BringToFront()
        _lblEdadEstado.BringToFront()
        _btnIncidencia.BringToFront()
    End Sub

    Private Sub Historial_DrawItem(ByVal sender As Object, ByVal e As DrawItemEventArgs)
        If e.Index < 0 Then
            Return
        End If

        Dim item As String = CStr(_lstHistorial.Items(e.Index))
        Dim hora As String = String.Empty
        Dim estadoRaw As String = String.Empty
        Dim detalle As String = String.Empty
        DescomponerItemHistorial(item, hora, estadoRaw, detalle)

        Dim colorEstado As Color = ObtenerColorEstadoHistorial(estadoRaw)
        Dim estadoBadge As String = ObtenerBadgeEstadoHistorial(estadoRaw)
        Dim filaFondo As Color = If(e.Index Mod 2 = 0, Color.FromArgb(255, 255, 255), Color.FromArgb(249, 250, 251))
        If (e.State And DrawItemState.Selected) = DrawItemState.Selected Then
            filaFondo = Color.FromArgb(230, 238, 248)
        End If

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Using fondo As New SolidBrush(_lstHistorial.BackColor)
            e.Graphics.FillRectangle(fondo, e.Bounds)
        End Using

        Dim filaRect As New Rectangle(e.Bounds.X + 2, e.Bounds.Y + 3, e.Bounds.Width - 4, e.Bounds.Height - 6)
        Using filaPath As GraphicsPath = CrearRutaRedondeada(filaRect, 10),
              filaBrush As New SolidBrush(filaFondo)
            e.Graphics.FillPath(filaBrush, filaPath)
        End Using

        Using barra As New SolidBrush(colorEstado)
            e.Graphics.FillRectangle(barra, New Rectangle(filaRect.X + 6, filaRect.Y + 5, 4, filaRect.Height - 10))
        End Using

        Dim horaRect As New Rectangle(filaRect.X + 18, filaRect.Y, 68, filaRect.Height)
        Dim badgeRect As New Rectangle(horaRect.Right + 4, filaRect.Y + 5, 52, filaRect.Height - 10)
        Dim detalleRect As New Rectangle(badgeRect.Right + 8, filaRect.Y, filaRect.Width - (badgeRect.Right - filaRect.X) - 16, filaRect.Height)

        Using horaFont As New Font("Segoe UI", 8.0!, FontStyle.Bold),
              badgeFont As New Font("Segoe UI", 7.8!, FontStyle.Bold),
              detalleFont As New Font("Segoe UI", 8.8!, FontStyle.Bold)
            TextRenderer.DrawText(e.Graphics, hora, horaFont, horaRect, Color.FromArgb(100, 116, 139), TextFormatFlags.VerticalCenter Or TextFormatFlags.Left)

            Using badgePath As GraphicsPath = CrearRutaRedondeada(badgeRect, 9),
                  badgeBrush As New SolidBrush(colorEstado)
                e.Graphics.FillPath(badgeBrush, badgePath)
            End Using
            TextRenderer.DrawText(e.Graphics, estadoBadge, badgeFont, badgeRect, Color.White, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)

            If String.IsNullOrWhiteSpace(detalle) Then
                detalle = estadoRaw
            End If
            TextRenderer.DrawText(e.Graphics, detalle, detalleFont, detalleRect, Color.FromArgb(30, 41, 59), TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
        End Using
        e.DrawFocusRectangle()
    End Sub

    Private Sub DescomponerItemHistorial(ByVal item As String, ByRef hora As String, ByRef estado As String, ByRef detalle As String)
        Dim partes() As String = item.Split(New String() {"|"}, StringSplitOptions.None)
        hora = If(partes.Length > 0, partes(0).Trim(), String.Empty)
        estado = If(partes.Length > 1, partes(1).Trim(), String.Empty)
        If partes.Length > 2 Then
            detalle = String.Join(" | ", partes, 2, partes.Length - 2).Trim()
        Else
            detalle = String.Empty
        End If
    End Sub

    Private Function ObtenerBadgeEstadoHistorial(ByVal estado As String) As String
        Dim valor As String = estado.Trim().ToUpperInvariant()
        If valor.Contains("DUPLIC") Then
            Return "DUP"
        End If
        If valor.Contains("WARN") OrElse valor.Contains("ADVERTENCIA") Then
            Return "ADV"
        End If
        If valor.Contains("EXITO") OrElse valor.Contains("SUCCESS") Then
            Return "OK"
        End If
        If valor.Contains("PROCES") Then
            Return "PROC"
        End If
        If valor.Contains("ERROR") OrElse valor.Contains("NO_ENCONTRADO") OrElse valor.Contains("NOTFOUND") Then
            Return "ERR"
        End If
        Return "INFO"
    End Function

    Private Function ObtenerColorEstadoHistorial(ByVal estado As String) As Color
        Dim valor As String = estado.Trim().ToUpperInvariant()
        If valor.Contains("DUPLIC") Then
            Return _colorDuplicado
        End If
        If valor.Contains("WARN") OrElse valor.Contains("ADVERTENCIA") Then
            Return Color.FromArgb(180, 83, 9)
        End If
        If valor.Contains("EXITO") OrElse valor.Contains("SUCCESS") Then
            Return _colorExito
        End If
        If valor.Contains("PROCES") Then
            Return Color.FromArgb(37, 99, 235)
        End If
        If valor.Contains("ERROR") OrElse valor.Contains("NO_ENCONTRADO") OrElse valor.Contains("NOTFOUND") Then
            Return _colorError
        End If
        Return Color.FromArgb(71, 85, 105)
    End Function

    Private Sub BtnIncidencia_Click(ByVal sender As Object, ByVal e As EventArgs)
        Const codigoIncidencia As String = "INCIDENCIA_OPERATIVA"
        RegistrarHistorial(EstadoVisual.ErrorGeneral, "INCIDENCIA: " & codigoIncidencia)
        _resultadoOperacionActual = "INCIDENCIA MANUAL"
        _fechaUltimoEstado = ServerClock.Now()
        RegistrarEventoPersistente(EstadoVisual.ErrorGeneral, True)
        LblRuta.Text = "Incidencia registrada: " & codigoIncidencia
        LblRuta.Visible = True
        EnsureScanFocus(True)
    End Sub

    Private Sub CargarPreferenciasOperacion()
        _sonidosHabilitados = LeerConfigBool("SonidosHabilitados", True)
        _sonidoOkRuta = LeerConfigTexto("SonidoOkRuta", String.Empty)
        _sonidoWarnRuta = LeerConfigTexto("SonidoWarnRuta", String.Empty)
        _sonidoErrorRuta = LeerConfigTexto("SonidoErrorRuta", String.Empty)
        _repeticionesSonidoOk = Math.Max(1, LeerConfigInt("RepeticionesSonidoOk", 2))
        _repeticionesSonidoWarn = Math.Max(1, LeerConfigInt("RepeticionesSonidoWarn", 2))
        _repeticionesSonidoError = Math.Max(1, LeerConfigInt("RepeticionesSonidoError", 3))
        _intervaloRepeticionSonidoMs = Math.Max(50, LeerConfigInt("IntervaloRepeticionSonidoMs", 220))
        _forzarSonidoSistemaFallback = LeerConfigBool("ForzarSonidoSistemaFallback", False)

        _colorExito = LeerConfigColor("ColorExitoHex", Color.FromArgb(22, 163, 74))
        _colorAdvertencia = LeerConfigColor("ColorAdvertenciaHex", Color.FromArgb(245, 158, 11))
        _colorError = LeerConfigColor("ColorErrorHex", Color.FromArgb(220, 38, 38))
        _colorNeutro = LeerConfigColor("ColorNeutroHex", Color.FromArgb(248, 250, 252))
        _colorProcesando = LeerConfigColor("ColorProcesandoHex", Color.FromArgb(238, 242, 255))
        _colorDuplicado = LeerConfigColor("ColorDuplicadoHex", Color.FromArgb(217, 119, 6))
    End Sub

    Private Sub InicializarControlInactividad()
        If _timerInactividad Is Nothing Then
            _timerInactividad = New System.Windows.Forms.Timer()
            _timerInactividad.Interval = 1000
            AddHandler _timerInactividad.Tick, AddressOf TimerInactividad_Tick
        End If

        If Not _inicializadoEventosActividad Then
            VincularEventosActividad(Me)
            _inicializadoEventosActividad = True
        End If

        RegistrarActividad()
        _timerInactividad.Start()
    End Sub

    Private Sub InicializarTimerEstadoVisual()
        If _timerEstadoVisual Is Nothing Then
            _timerEstadoVisual = New System.Windows.Forms.Timer()
            _timerEstadoVisual.Interval = 500
            AddHandler _timerEstadoVisual.Tick, AddressOf TimerEstadoVisual_Tick
            _timerEstadoVisual.Start()
        End If
    End Sub

    Private Sub RegistrarActividad()
        _ultimaActividad = ServerClock.Now()
        _limpiezaAplicadaPorInactividad = False
    End Sub

    Private Sub TimerInactividad_Tick(ByVal sender As Object, ByVal e As EventArgs)
        LblFecha.Text = ServerClock.Now().ToString("yyyy/MM/dd HH:mm:ss")
        IntentarReconexionSiCorresponde()
        ActualizarEstadoConexion()
        ActualizarIndicadoresEstadoInfo()
        ActualizarIndicadorFoco(TxtCedula.Focused)

        If _limpiezaAplicadaPorInactividad Then
            Exit Sub
        End If

        Dim segundosSinActividad As Double = ServerClock.Now().Subtract(_ultimaActividad).TotalSeconds
        If segundosSinActividad < SegundosInactividadLimpiarRegistro Then
            Exit Sub
        End If

        LimpiarPantalla(True)
        _limpiezaAplicadaPorInactividad = True
    End Sub

    Private Sub TimerEstadoVisual_Tick(ByVal sender As Object, ByVal e As EventArgs)
        ActualizarIndicadoresEstadoInfo()
    End Sub

    Private Sub IntentarReconexionSiCorresponde()
        If Cn Is Nothing Then
            Exit Sub
        End If
        If Cn.State = ConnectionState.Open Then
            Exit Sub
        End If
        If ServerClock.Now().Subtract(_ultimoIntentoReconexion).TotalSeconds < 5 Then
            Exit Sub
        End If

        _ultimoIntentoReconexion = ServerClock.Now()
        Try
            Cls.AbrirConexion(Cn, False)
            SincronizarHoraServidorActual()
            RecargarDatosOperacion()
            CargarHistorialInicial()
        Catch ex As Exception
            ErrorLogger.LogException("ControlTransporte.IntentarReconexionSiCorresponde", ex)
        End Try
    End Sub

    Private Sub SincronizarHoraServidorActual()
        If Cn Is Nothing OrElse Cn.State <> ConnectionState.Open Then
            Exit Sub
        End If

        Try
            Dim dsFecha As DataSet = Cls.ConsultarTSQL("Fecha", "SELECT GETDATE() AS Fecha;", Cn:=Cn)
            If dsFecha Is Nothing OrElse dsFecha.Tables.Count = 0 OrElse dsFecha.Tables(0).Rows.Count = 0 Then
                Exit Sub
            End If

            ServerClock.Sync(CDate(dsFecha.Tables(0).Rows(0)("Fecha")))
        Catch ex As Exception
            ErrorLogger.LogException("ControlTransporte.SincronizarHoraServidorActual", ex)
        End Try
    End Sub

    Private Sub UpdateVisualState(ByVal state As EstadoVisual)
        _estadoVisualActual = state

        Select Case state
            Case EstadoVisual.Idle
                lblProcesando.Text = "Esperando lectura de carnet"
                lblProcesando.ForeColor = Color.FromArgb(23, 32, 51)
                LblTitulo.ForeColor = Color.FromArgb(17, 33, 59)
                LblTitulo.Text = "Control de Marcas - Transporte"
                LblRuta.ForeColor = Color.FromArgb(76, 90, 112)
                Imgprocess.Image = My.Resources.Info
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelResult.BackColor = _colorNeutro
                _resultadoOperacionActual = "SIN LECTURA"
                _fechaUltimoEstado = DateTime.MinValue
            Case EstadoVisual.Processing
                lblProcesando.Text = "Procesando lectura..."
                lblProcesando.ForeColor = Color.FromArgb(17, 33, 59)
                LblTitulo.ForeColor = Color.FromArgb(17, 33, 59)
                LblRuta.ForeColor = Color.FromArgb(76, 90, 112)
                Imgprocess.Image = My.Resources.Gif_cargando
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelResult.BackColor = _colorProcesando
                _resultadoOperacionActual = "PROCESANDO..."
            Case EstadoVisual.Success
                lblProcesando.Text = "Marca registrada correctamente"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                LblRuta.ForeColor = Color.FromArgb(239, 246, 255)
                Imgprocess.Image = My.Resources.Verificado2
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelResult.BackColor = _colorExito
                _resultadoOperacionActual = "ACCESO PERMITIDO"
                _fechaUltimoEstado = ServerClock.Now()
            Case EstadoVisual.Warning
                lblProcesando.Text = "Acceso permitido con advertencia"
                lblProcesando.ForeColor = Color.FromArgb(31, 41, 55)
                LblTitulo.ForeColor = Color.FromArgb(31, 41, 55)
                LblRuta.ForeColor = Color.FromArgb(82, 55, 8)
                Imgprocess.Image = My.Resources.Info
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelResult.BackColor = _colorAdvertencia
                _resultadoOperacionActual = "PERMITIDO CON ADVERTENCIA"
                _fechaUltimoEstado = ServerClock.Now()
            Case EstadoVisual.Duplicate
                lblProcesando.Text = "Se detecto doble verificacion"
                lblProcesando.ForeColor = Color.FromArgb(31, 41, 55)
                LblTitulo.ForeColor = Color.FromArgb(31, 41, 55)
                LblRuta.ForeColor = Color.FromArgb(120, 53, 15)
                Imgprocess.Image = My.Resources.Double_check
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelResult.BackColor = _colorDuplicado
                _resultadoOperacionActual = "LECTURA DUPLICADA"
                _fechaUltimoEstado = ServerClock.Now()
            Case EstadoVisual.NotFound
                lblProcesando.Text = "Identificador no valido o no encontrado"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                LblRuta.ForeColor = Color.FromArgb(254, 226, 226)
                Imgprocess.Image = My.Resources.Error2
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelResult.BackColor = _colorError
                ResetResultFields()
                _resultadoOperacionActual = "ACCESO DENEGADO"
                _fechaUltimoEstado = ServerClock.Now()
            Case EstadoVisual.ErrorGeneral
                lblProcesando.Text = "Error al verificar el usuario"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                LblRuta.ForeColor = Color.FromArgb(254, 226, 226)
                Imgprocess.Image = My.Resources.Error2
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelResult.BackColor = _colorError
                _resultadoOperacionActual = "ERROR OPERATIVO"
                _fechaUltimoEstado = ServerClock.Now()
        End Select

        If (state = EstadoVisual.Success OrElse state = EstadoVisual.Warning) AndAlso Not String.IsNullOrWhiteSpace(_mensajeEstadoPersonalizado) Then
            lblProcesando.Text = _mensajeEstadoPersonalizado
        End If
        If (state = EstadoVisual.Success OrElse state = EstadoVisual.Warning) AndAlso Not String.IsNullOrWhiteSpace(_resultadoOperacionPersonalizado) Then
            _resultadoOperacionActual = _resultadoOperacionPersonalizado
        End If

        If _lblResultadoOperacion IsNot Nothing Then
            _lblResultadoOperacion.Text = _resultadoOperacionActual
            If state = EstadoVisual.Success OrElse state = EstadoVisual.NotFound OrElse state = EstadoVisual.ErrorGeneral Then
                _lblResultadoOperacion.ForeColor = Color.White
            Else
                _lblResultadoOperacion.ForeColor = Color.FromArgb(31, 41, 55)
            End If
        End If
        ActualizarChipEstado(state)
        ActualizarTarjetasEstado(state)
        LblRuta.Visible = Not String.IsNullOrWhiteSpace(LblRuta.Text)

        ActualizarIndicadoresEstadoInfo()

        If state <> EstadoVisual.Idle AndAlso state <> EstadoVisual.Processing Then
            ReproducirSonido(state)
            RegistrarHistorial(state, TxtCedula.Text.Trim())
            If Not (_eventoPersistidoEnTransaccion AndAlso (state = EstadoVisual.Success OrElse state = EstadoVisual.Warning) AndAlso state = _estadoEventoTransaccion) Then
                RegistrarEventoPersistente(state)
            End If
            _eventoPersistidoEnTransaccion = False
        End If
    End Sub

    Private Sub ActualizarTarjetasEstado(ByVal state As EstadoVisual)
        If _modoAltoContraste Then
            LblTitulo.BackColor = Color.FromArgb(32, 32, 32)
            LblRuta.BackColor = Color.FromArgb(24, 24, 24)
            Return
        End If

        Select Case state
            Case EstadoVisual.Success
                LblTitulo.BackColor = Color.FromArgb(24, 95, 64)
                LblRuta.BackColor = Color.FromArgb(33, 118, 81)
            Case EstadoVisual.Warning
                LblTitulo.BackColor = Color.FromArgb(250, 204, 21)
                LblRuta.BackColor = Color.FromArgb(254, 240, 138)
            Case EstadoVisual.Duplicate
                LblTitulo.BackColor = Color.FromArgb(245, 158, 11)
                LblRuta.BackColor = Color.FromArgb(253, 230, 138)
            Case EstadoVisual.NotFound, EstadoVisual.ErrorGeneral
                LblTitulo.BackColor = Color.FromArgb(153, 27, 27)
                LblRuta.BackColor = Color.FromArgb(127, 29, 29)
            Case EstadoVisual.Processing
                LblTitulo.BackColor = Color.FromArgb(219, 234, 254)
                LblRuta.BackColor = Color.FromArgb(239, 246, 255)
            Case Else
                LblTitulo.BackColor = Color.FromArgb(229, 236, 246)
                LblRuta.BackColor = Color.FromArgb(238, 242, 247)
        End Select
    End Sub

    Private Sub ActualizarChipEstado(ByVal state As EstadoVisual)
        If _lblEstadoChip Is Nothing Then
            Exit Sub
        End If

        Select Case state
            Case EstadoVisual.Success
                _lblEstadoChip.Text = "ESTADO: OK"
                _lblEstadoChip.BackColor = Color.FromArgb(21, 128, 61)
                _lblEstadoChip.ForeColor = Color.White
            Case EstadoVisual.Processing
                _lblEstadoChip.Text = "ESTADO: PROCESANDO"
                _lblEstadoChip.BackColor = Color.FromArgb(37, 99, 235)
                _lblEstadoChip.ForeColor = Color.White
            Case EstadoVisual.Warning
                _lblEstadoChip.Text = "ESTADO: ADVERTENCIA"
                _lblEstadoChip.BackColor = Color.FromArgb(202, 138, 4)
                _lblEstadoChip.ForeColor = Color.White
            Case EstadoVisual.Duplicate
                _lblEstadoChip.Text = "ESTADO: DOBLE LECTURA"
                _lblEstadoChip.BackColor = Color.FromArgb(180, 83, 9)
                _lblEstadoChip.ForeColor = Color.White
            Case EstadoVisual.NotFound, EstadoVisual.ErrorGeneral
                _lblEstadoChip.Text = "ESTADO: ERROR"
                _lblEstadoChip.BackColor = Color.FromArgb(185, 28, 28)
                _lblEstadoChip.ForeColor = Color.White
            Case Else
                _lblEstadoChip.Text = "ESTADO: EN ESPERA"
                _lblEstadoChip.BackColor = Color.FromArgb(51, 65, 85)
                _lblEstadoChip.ForeColor = Color.White
        End Select
    End Sub

    Private Sub RegistrarHistorial(ByVal state As EstadoVisual, ByVal detalle As String)
        If _lstHistorial Is Nothing Then
            Exit Sub
        End If
        Dim item As String = ServerClock.Now().ToString("HH:mm:ss") & "  |  " & state.ToString() & "  |  " & detalle
        _lstHistorial.Items.Insert(0, item)
        While _lstHistorial.Items.Count > 10
            _lstHistorial.Items.RemoveAt(_lstHistorial.Items.Count - 1)
        End While
    End Sub

    Private Sub CargarHistorialInicial()
        If _lstHistorial Is Nothing Then
            Exit Sub
        End If

        Try
            If Cn.State <> ConnectionState.Open Then
                Exit Sub
            End If

            Dim dt As DataTable = OperacionSvc.ListarUltimosEventos(Cn, 10)
            _lstHistorial.Items.Clear()
            If dt Is Nothing Then
                Exit Sub
            End If

            For Each row As DataRow In dt.Rows
                Dim fechaStr As String = CDate(row("FechaEvento")).ToString("HH:mm:ss")
                Dim estado As String = CStr(row("Estado"))
                Dim cedula As String = CStr(row("Cedula"))
                Dim motivo As String = CStr(row("Motivo"))
                Dim detalle As String = If(String.IsNullOrWhiteSpace(motivo), cedula, motivo)
                _lstHistorial.Items.Add(fechaStr & " | " & estado & " | " & detalle)
            Next
        Catch ex As Exception
            ErrorLogger.LogException("ControlTransporte.CargarHistorialInicial", ex)
        End Try
    End Sub

    Private Sub RegistrarEventoPersistente(ByVal state As EstadoVisual, Optional ByVal esIncidenciaManual As Boolean = False)
        Try
            If Cn Is Nothing OrElse Cn.State <> ConnectionState.Open Then
                Exit Sub
            End If

            Dim esAdvertencia As Boolean = (state = EstadoVisual.Warning OrElse state = EstadoVisual.Duplicate)
            Dim esError As Boolean = (state = EstadoVisual.NotFound OrElse state = EstadoVisual.ErrorGeneral)
            Dim ms As Integer? = Nothing
            If _inicioLectura <> DateTime.MinValue Then
                ms = CInt(Math.Max(0, Math.Round(ServerClock.Now().Subtract(_inicioLectura).TotalMilliseconds)))
            End If

            OperacionSvc.RegistrarEvento(Cn,
                                         ServerClock.Now(),
                                         If(String.IsNullOrWhiteSpace(_ultimoCarnetLeido), LblCedula.Text.Trim(), _ultimoCarnetLeido),
                                         ObtenerCodigoEstado(state),
                                         _resultadoOperacionActual,
                                         ms,
                                         state = EstadoVisual.Duplicate,
                                         esAdvertencia,
                                         esError,
                                         esIncidenciaManual)
        Catch ex As Exception
            ErrorLogger.LogException("ControlTransporte.RegistrarEventoPersistente", ex)
        End Try
    End Sub

    Private Function ObtenerCodigoEstado(ByVal state As EstadoVisual) As String
        Select Case state
            Case EstadoVisual.Success
                Return "EXITO"
            Case EstadoVisual.Processing
                Return "PROCESANDO_LECTURA"
            Case EstadoVisual.Warning
                Return "PERMITIDO_CON_ADVERTENCIA"
            Case EstadoVisual.Duplicate
                Return "LECTURA_DUPLICADA"
            Case EstadoVisual.NotFound
                Return "CARNET_NO_ENCONTRADO"
            Case EstadoVisual.ErrorGeneral
                Return "ERROR_OPERATIVO"
            Case Else
                Return "EN_ESPERA"
        End Select
    End Function

    Private Sub ActualizarEstadoConexion()
        If _lblConexion Is Nothing Then
            Exit Sub
        End If

        If Cn IsNot Nothing AndAlso Cn.State = ConnectionState.Open Then
            _lblConexion.Text = "Conexion DB: online"
            _lblConexion.ForeColor = Color.FromArgb(157, 230, 170)
        Else
            _lblConexion.Text = "Conexion DB: offline"
            _lblConexion.ForeColor = Color.FromArgb(255, 193, 193)
        End If
    End Sub

    Private Sub ActualizarIndicadoresEstadoInfo()
        If _lblUltimaLectura Is Nothing OrElse _lblEdadEstado Is Nothing Then
            Exit Sub
        End If

        If _fechaUltimoEstado = DateTime.MinValue Then
            _lblUltimaLectura.Text = "Ultima lectura: --"
            _lblEdadEstado.Text = "Estado visible: sin evento"
            Exit Sub
        End If

        _lblUltimaLectura.Text = "Ultima lectura: " & _fechaUltimoEstado.ToString("yyyy/MM/dd HH:mm:ss")
        _lblEdadEstado.Text = "Estado visible: " & ObtenerEdadEstadoTexto(ServerClock.Now().Subtract(_fechaUltimoEstado))
    End Sub

    Private Function ObtenerEdadEstadoTexto(ByVal lapso As TimeSpan) As String
        If lapso.TotalSeconds < 0 Then
            Return "ahora"
        End If
        If lapso.TotalSeconds < 60 Then
            Return String.Format("hace {0}s", CInt(Math.Floor(lapso.TotalSeconds)))
        End If
        If lapso.TotalMinutes < 60 Then
            Return String.Format("hace {0}m {1}s", CInt(Math.Floor(lapso.TotalMinutes)), lapso.Seconds)
        End If
        Return String.Format("hace {0}h {1}m", CInt(Math.Floor(lapso.TotalHours)), lapso.Minutes)
    End Function

    Private Sub ActualizarKpisOperacion()
        If _lblKpi Is Nothing Then
            Exit Sub
        End If

        Dim ratioDup As Double = 0
        If _totalLecturas > 0 Then
            ratioDup = (_totalDuplicadas / CDbl(_totalLecturas)) * 100.0R
        End If

        _lblKpi.Text = String.Format("Lecturas: {0} | Exitosas: {1} | Duplicadas: {2} ({3:0}%) | Errores: {4}", _totalLecturas, _lecturasExitosas, _totalDuplicadas, ratioDup, _totalErrores)
    End Sub

    Private Sub RegistrarTiempoAtencion()
        If _inicioLectura = DateTime.MinValue Then
            Exit Sub
        End If

        Dim ms As Double = ServerClock.Now().Subtract(_inicioLectura).TotalMilliseconds
        If ms <= 0 Then
            Exit Sub
        End If

        _acumuladoTiempoAtencionMs += ms
        _muestrasTiempoAtencion += 1
    End Sub

    Private Sub ReproducirSonido(ByVal state As EstadoVisual)
        If Not _sonidosHabilitados Then
            Exit Sub
        End If

        Dim ruta As String = String.Empty
        Dim repeticiones As Integer = 1
        Dim sonidoSistema As SystemSound = Nothing

        Select Case state
            Case EstadoVisual.Success
                ruta = _sonidoOkRuta
                repeticiones = _repeticionesSonidoOk
                sonidoSistema = SystemSounds.Asterisk
            Case EstadoVisual.Warning, EstadoVisual.Duplicate
                ruta = _sonidoWarnRuta
                repeticiones = _repeticionesSonidoWarn
                sonidoSistema = SystemSounds.Exclamation
            Case EstadoVisual.NotFound, EstadoVisual.ErrorGeneral
                ruta = _sonidoErrorRuta
                repeticiones = _repeticionesSonidoError
                sonidoSistema = SystemSounds.Hand
            Case Else
                Exit Sub
        End Select

        DetenerSonidoActivo()
        Dim cts As New CancellationTokenSource()
        SyncLock _sonidoSync
            _sonidoCancellation = cts
        End SyncLock

        Task.Run(Sub()
                     ReproducirSonidoCore(ruta, sonidoSistema, repeticiones, cts.Token)
                 End Sub)
    End Sub

    Private Sub ReproducirSonidoCore(ByVal ruta As String, ByVal sonidoSistema As SystemSound, ByVal repeticiones As Integer, ByVal token As CancellationToken)
        Dim resolved As String = String.Empty
        Dim usarWav As Boolean = False

        If Not _forzarSonidoSistemaFallback AndAlso Not String.IsNullOrWhiteSpace(ruta) Then
            Try
                resolved = ResolverRutaRecurso(ruta)
                usarWav = IO.File.Exists(resolved)
            Catch ex As Exception
                ErrorLogger.LogException("ControlTransporte.ReproducirSonidoCore.ResolverRuta", ex)
                usarWav = False
            End Try
        End If

        For i As Integer = 1 To Math.Max(1, repeticiones)
            If token.IsCancellationRequested Then
                Exit For
            End If

            Try
                If usarWav Then
                    Using player As New SoundPlayer(resolved)
                        player.PlaySync()
                    End Using
                ElseIf sonidoSistema IsNot Nothing Then
                    sonidoSistema.Play()
                End If
            Catch ex As Exception
                ErrorLogger.LogException("ControlTransporte.ReproducirSonidoCore.Play", ex)
            End Try

            If i < repeticiones AndAlso Not token.IsCancellationRequested Then
                Thread.Sleep(_intervaloRepeticionSonidoMs)
            End If
        Next
    End Sub

    Private Sub DetenerSonidoActivo()
        SyncLock _sonidoSync
            If _sonidoCancellation IsNot Nothing Then
                _sonidoCancellation.Cancel()
                _sonidoCancellation.Dispose()
                _sonidoCancellation = Nothing
            End If
        End SyncLock
    End Sub

    Private Function ResolverRutaRecurso(ByVal nombreArchivo As String) As String
        Return ResolveResourcePath(nombreArchivo)
    End Function

    Private Function LeerConfigBool(ByVal key As String, ByVal valorDefault As Boolean) As Boolean
        Return GetAppSettingBoolean(key, valorDefault)
    End Function

    Private Function LeerConfigInt(ByVal key As String, ByVal valorDefault As Integer) As Integer
        Return GetAppSettingInteger(key, valorDefault)
    End Function

    Private Function LeerConfigTexto(ByVal key As String, ByVal valorDefault As String) As String
        Return GetAppSettingValue(key, valorDefault)
    End Function

    Private Function LeerConfigColor(ByVal key As String, ByVal valorDefault As Color) As Color
        Try
            Dim raw As String = GetAppSettingValue(key, String.Empty)
            If String.IsNullOrWhiteSpace(raw) Then
                Return valorDefault
            End If
            Dim hex As String = raw.Trim()
            If Not hex.StartsWith("#", StringComparison.Ordinal) Then
                hex = "#" & hex
            End If
            Return ColorTranslator.FromHtml(hex)
        Catch ex As Exception
            ErrorLogger.LogException("ControlTransporte.LeerConfigColor", ex)
        End Try
        Return valorDefault
    End Function

    Private Sub VincularEventosActividad(ByVal parent As Control)
        AddHandler parent.MouseMove, AddressOf OnActividadMouseMove
        AddHandler parent.Click, AddressOf OnActividadClick
        AddHandler parent.KeyDown, AddressOf OnActividadKeyDown

        For Each child As Control In parent.Controls
            VincularEventosActividad(child)
        Next
    End Sub

    Private Sub OnActividadClick(ByVal sender As Object, ByVal e As EventArgs)
        RegistrarActividad()
        OperativeUiHelper.RecoverScannerFocusAfterClick(Me,
                                                        sender,
                                                        TxtCedula,
                                                        _mostrarEntradaManualTemporal,
                                                        AddressOf EnsureScanFocus)
    End Sub

    Private Sub OnActividadMouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        RegistrarActividad()
    End Sub

    Private Sub OnActividadKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        RegistrarActividad()
    End Sub

    Private Sub ControlTransporte_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            e.Handled = True
            EjecutarAccionEscape()
        End If
    End Sub

    Private Sub EjecutarAccionEscape()
        If PermitirCierreOperador Then
            Me.Close()
        Else
            EnsureScanFocus(False)
        End If
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape Then
            EjecutarAccionEscape()
            Return True
        End If
        If keyData = Keys.F2 Then
            LimpiarPantalla(True)
            Return True
        End If
        If keyData = Keys.F3 Then
            EnsureScanFocus(True)
            Return True
        End If
        If keyData = (Keys.Control Or Keys.F3) Then
            ToggleEntradaManualTemporal()
            Return True
        End If
        If keyData = Keys.F4 AndAlso _lstHistorial IsNot Nothing Then
            _lstHistorial.Visible = Not _lstHistorial.Visible
            _lblHistorial.Visible = _lstHistorial.Visible
            EnsureScanFocus(False)
            Return True
        End If
        If keyData = Keys.F7 Then
            _modoAltoContraste = Not _modoAltoContraste
            ApplyModernOperationalLayout()
            ApplyResponsiveLayout()
            UpdateVisualState(_estadoVisualActual)
            EnsureScanFocus(False)
            Return True
        End If

        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Private Sub ToggleEntradaManualTemporal()
        _mostrarEntradaManualTemporal = Not _mostrarEntradaManualTemporal
        If Not _mostrarEntradaManualTemporal Then
            TxtCedula.Clear()
        End If
        AplicarPrivacidadEscaneo()
        ApplyResponsiveLayout()
        EnsureScanFocus(False)
    End Sub

    Private Sub AplicarPrivacidadEscaneo()
        If _mostrarEntradaManualTemporal Then
            TxtCedula.BackColor = If(_modoAltoContraste, Color.Black, Color.White)
            TxtCedula.ForeColor = If(_modoAltoContraste, Color.White, Color.FromArgb(17, 33, 59))
            TxtCedula.BorderStyle = If(_modoAltoContraste, BorderStyle.Fixed3D, BorderStyle.FixedSingle)
        Else
            TxtCedula.BackColor = BunifuGradientPanel1.BackColor
            TxtCedula.ForeColor = BunifuGradientPanel1.BackColor
            TxtCedula.BorderStyle = BorderStyle.None
        End If

        If _lblScanHint IsNot Nothing Then
            _lblScanHint.Text = ObtenerTextoScanHint()
        End If
        If _lblHotkeys IsNot Nothing Then
            _lblHotkeys.Text = ObtenerTextoHotkeys()
        End If
        ActualizarIndicadorFoco(TxtCedula.Focused)
    End Sub

    Private Function ObtenerTextoScanHint() As String
        Dim lineaPrincipal As String = If(_mostrarEntradaManualTemporal, "Ingreso manual visible (Ctrl+F3 para ocultar)", "Escanear carnet (captura oculta)")
        Return lineaPrincipal & Environment.NewLine & "Limpiar (F2) | Salir (Esc) | Alto contraste (F7)"
    End Function

    Private Function ObtenerTextoHotkeys() As String
        Return "Esc salir | F2 limpiar | F3 foco | Ctrl+F3 mostrar/ocultar | F4 historial | F7 contraste"
    End Function

    Private Sub LblTitulo_Click(sender As Object, e As EventArgs) Handles LblTitulo.Click
        RegistrarActividad()
        EnsureScanFocus(False)
    End Sub

    Private Sub lblProcesando_Click(sender As Object, e As EventArgs) Handles lblProcesando.Click
        RegistrarActividad()
        EnsureScanFocus(False)
    End Sub

End Class
