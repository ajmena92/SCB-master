Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Media
Imports System.Threading
Imports System.Threading.Tasks

Public Class ControlComedor
    Private Const PermitirCierreOperador As Boolean = True
    Private Const SegundosInactividadLimpiarRegistro As Integer = 60
    Private Const SidebarMinWidth As Integer = 420
    Private Const SidebarMaxWidth As Integer = 520

    Private UltimoCarnetProcesado As String
    Private ErrorLecturaDuplicada As Boolean
    Private ErrorTiquetes As Boolean
    Private EstadoVerificado As Boolean
    Private _timerInactividad As System.Windows.Forms.Timer
    Private _ultimaActividad As DateTime
    Private _limpiezaAplicadaPorInactividad As Boolean
    Private _logoAplicacion As Image
    Private _timerEstadoVisual As System.Windows.Forms.Timer
    Private _sonidosHabilitados As Boolean
    Private _modoAccesible As Boolean
    Private _mostrarVistaSupervisor As Boolean
    Private _metaDiaria As Integer
    Private _totalLecturas As Integer
    Private _totalDuplicadas As Integer
    Private _totalErrores As Integer
    Private _asistenciasValidasDia As Integer
    Private _inicializadoEventosActividad As Boolean
    Private _lblMeta As Label
    Private _progressMeta As ProgressBar
    Private _lblConexion As Label
    Private _lblKpi As Label
    Private _lblCola As Label
    Private _lblAlertas As Label
    Private _lblRecomendacion As Label
    Private _lblResultadoOperacion As Label
    Private _lblUltimaLectura As Label
    Private _lblEdadEstado As Label
    Private _lblHistorial As Label
    Private _lstHistorial As ListBox
    Private _btnIncidencia As Button
    Private _lblHotkeys As Label
    Private _lblFocusEscaneo As Label
    Private _lblEstadoChip As Label
    Private _panelSidebarDividerRegistro As Panel
    Private _panelSidebarDividerMetricas As Panel
    Private _modoAltoContraste As Boolean
    Private _mostrarEntradaManualTemporal As Boolean
    Private _inicioLectura As DateTime
    Private _acumuladoTiempoAtencionMs As Double
    Private _muestrasTiempoAtencion As Integer
    Private _umbralDuplicadosPct As Double
    Private _umbralErrores As Integer
    Private _sonidoOkRuta As String
    Private _sonidoWarnRuta As String
    Private _sonidoErrorRuta As String
    Private _repeticionesSonidoOk As Integer
    Private _repeticionesSonidoWarn As Integer
    Private _repeticionesSonidoError As Integer
    Private _intervaloRepeticionSonidoMs As Integer
    Private _forzarSonidoSistemaFallback As Boolean
    Private _colorExito As Color
    Private _colorAdvertencia As Color
    Private _colorError As Color
    Private _colorNeutro As Color
    Private _colorProcesando As Color
    Private _colorDuplicado As Color
    Private _estadoVisualActual As EstadoVisual
    Private _fechaUltimoEstado As DateTime
    Private _resultadoOperacionActual As String
    Private _sonidoCancellation As CancellationTokenSource
    Private ReadOnly _sonidoSync As New Object()
    Private _ultimoIntentoReconexion As DateTime
    Private _permitirMarcaTardia As Boolean
    Private _apagaAdvertenciaTransporte As Boolean

    Private ReadOnly Cls As New FuncionesDB
    Private ReadOnly ComedorSvc As New ComedorDataService(Cls)
    Private ReadOnly OperacionSvc As New ComedorOperacionService()
    Private ReadOnly ParametroSvc As New ParametroSistemaService()
    Private ReadOnly Cn As New SqlClient.SqlConnection
    Private _usuariosPorCarnet As Dictionary(Of String, UsuarioComedorSnapshot) = New Dictionary(Of String, UsuarioComedorSnapshot)(StringComparer.OrdinalIgnoreCase)
    Private _diasBecaPorId As Dictionary(Of Integer, String) = New Dictionary(Of Integer, String)()
    Private _horaLimitePorHorario As Dictionary(Of Integer, TimeSpan) = New Dictionary(Of Integer, TimeSpan)()
    Private _asistenciasComedorHoy As HashSet(Of Integer) = New HashSet(Of Integer)()
    Private _fechaAsistenciaComedorCache As Date = Date.MinValue
    Private _mensajeDuplicadoPersonalizado As String = String.Empty
    Private _detalleDuplicadoPersonalizado As String = String.Empty
    Private _ultimoIntentoSincronizacionDia As DateTime = DateTime.MinValue

    Private Enum EstadoVisual
        Idle = 0
        Success = 1
        Processing = 2
        Duplicate = 3
        NoTickets = 4
        NoTransportMark = 5
        LateTransportMark = 6
        NotFound = 7
        DeniedByRule = 8
    End Enum

    Private Enum LayoutMode
        Narrow1366 = 0
        Compact = 1
        Standard = 2
        Wide = 3
    End Enum

    Private NotInheritable Class UsuarioComedorSnapshot
        Public Property IdUsuario As Integer
        Public Property TieneTipoBeca As Boolean
        Public Property TipoBecaId As Integer
        Public Property CantidadTiquetes As Integer
        Public Property Nombre As String
        Public Property PrimerApellido As String
        Public Property SegundoApellido As String
        Public Property CodTipo As Short
        Public Property Cedula As String
        Public Property TieneHorario As Boolean
        Public Property IdHorario As Integer
        Public Property MarcaTransporte As Boolean
        Public Property TieneHoraMarca As Boolean
        Public Property HoraMarca As Date

        Public ReadOnly Property NombreCompleto As String
            Get
                Return String.Format("{0} {1} {2}", Nombre, PrimerApellido, SegundoApellido).Trim()
            End Get
        End Property
    End Class

    Private Sub ControlComedor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If
        Try
            UIThemeManagerV2.Apply(Me, "operativo")
            CargarPreferenciasOperacion()
            ApplyModernOperationalLayout()
            InicializarControlesOperacion()

            Cls.AbrirConexion(Cn, False)
            SincronizarHoraServidorActual()
            SincronizarReglasOperacion()
            RecargarDatosOperacion()

            LblFecha.Text = FechaServer.ToString("yyyy/MM/dd HH:mm:ss")
            UltimoCarnetProcesado = String.Empty
            ResetResultFields()
            UpdateVisualState(EstadoVisual.Idle)
            ApplyResponsiveLayout()
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
            ErrorLogger.LogException("ControlComedor_Load", ex)
            MsgBox("Error al cargar ControlComedor: " & ex.Message, MsgBoxStyle.Critical)
            Me.Dispose()
        End Try
    End Sub

    Private Sub ControlComedor_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        ApplyResponsiveLayout()
        EnsureScanFocus(True)
    End Sub

    Private Sub ControlComedor_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        ApplyResponsiveLayout()
    End Sub

    Private Sub ControlComedor_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
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
            ErrorLogger.LogException("ControlComedor.FormClosed", ex)
        End Try
    End Sub

    Private Sub ControlComedor_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If PermitirCierreOperador Then
            Exit Sub
        End If

        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            EnsureScanFocus(False)
        End If
    End Sub

    Private Sub ControlComedor_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        EnsureScanFocus(False)
    End Sub

    Private Sub BtnSalir_Click(sender As Object, e As EventArgs) Handles BtnSalir.Click
        'Boton deshabilitado por politica de permisos.
    End Sub

    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtCedula.KeyDown
        RegistrarActividad()

        If e.KeyCode <> Keys.Enter Then
            Exit Sub
        End If

        e.SuppressKeyPress = True
        ProcesarLecturaCarnet(TxtCedula.Text)
    End Sub

    Private Sub lblProcesando_Click(sender As Object, e As EventArgs) Handles lblProcesando.Click
        RegistrarActividad()
        EnsureScanFocus(False)
    End Sub

    Private Sub ProcesarLecturaCarnet(ByVal carnetRaw As String)
        RegistrarActividad()
        SincronizarOperacionSiCambioDia()
        _totalLecturas += 1
        _inicioLectura = ServerClock.Now()
        Dim carnet As String = carnetRaw.Trim()

        LimpiarMensajesDuplicado()
        LimpiarContextoUltimoRegistro()
        UpdateVisualState(EstadoVisual.Processing)
        ErrorLecturaDuplicada = False
        ErrorTiquetes = False
        EstadoVerificado = False

        If carnet.Length = 0 Then
            UpdateVisualState(EstadoVisual.NotFound)
            EnsureScanFocus(True)
            Exit Sub
        End If

        If Not Cls.VereficaCarnet(carnet) Then
            UpdateVisualState(EstadoVisual.NotFound)
            EnsureScanFocus(True)
            Exit Sub
        End If

        Dim usuario As UsuarioComedorSnapshot = BuscarUsuarioPorCarnet(carnet)
        If usuario Is Nothing Then
            UpdateVisualState(EstadoVisual.NotFound)
            EnsureScanFocus(True)
            Exit Sub
        End If

        If String.Equals(UltimoCarnetProcesado, carnet, StringComparison.OrdinalIgnoreCase) Then
            RegistrarLecturaDuplicada(usuario,
                                      "Lectura duplicada detectada",
                                      "Accion: espere 1 minuto o escanee otro carnet.")
            Exit Sub
        End If

        Dim warningSinMarcaTransporte As Boolean = False
        Dim warningMarcaTardia As Boolean = False
        Dim aplicaWarningSinMarcaTransporte As Boolean = False
        Dim aplicaWarningMarcaTardia As Boolean = False

        If EsEstudiante(usuario) Then
            CargarDatosUsuario(usuario, True)

            If TieneAsistenciaComedorHoy(usuario) Then
                RegistrarLecturaDuplicada(usuario,
                                          "Asistencia ya registrada hoy",
                                          "Accion: no registrar otra marca de comedor hoy.")
                Exit Sub
            End If

            warningSinMarcaTransporte = TieneAdvertenciaSinMarcaTransporte(usuario)
            warningMarcaTardia = TieneAdvertenciaMarcaTardia(usuario)
            aplicaWarningSinMarcaTransporte = warningSinMarcaTransporte AndAlso Not _apagaAdvertenciaTransporte
            aplicaWarningMarcaTardia = warningMarcaTardia AndAlso Not _apagaAdvertenciaTransporte

            If aplicaWarningSinMarcaTransporte AndAlso Not PermitirSinMarcaTransporte Then
                RegistrarDenegacionPorRegla(usuario)
                Exit Sub
            End If

            If aplicaWarningMarcaTardia AndAlso Not _permitirMarcaTardia Then
                RegistrarDenegacionPorRegla(usuario)
                Exit Sub
            End If
        End If

        RegistrarMarca(usuario)

        If ErrorTiquetes Then
            _totalErrores += 1
            UpdateVisualState(EstadoVisual.NoTickets)
        ElseIf aplicaWarningSinMarcaTransporte Then
            UpdateVisualState(EstadoVisual.NoTransportMark)
        ElseIf aplicaWarningMarcaTardia Then
            UpdateVisualState(EstadoVisual.LateTransportMark)
        ElseIf EstadoVerificado Then
            UpdateVisualState(EstadoVisual.Success)
        Else
            _totalErrores += 1
            UpdateVisualState(EstadoVisual.NotFound)
        End If

        RegistrarTiempoAtencion()
        ActualizarKpisOperacion()
        EnsureScanFocus(True)
    End Sub

    Private Sub RegistrarMarca(ByVal usuario As UsuarioComedorSnapshot)
        Try
            CargarDatosUsuario(usuario, False)

            Dim resultado As ComedorDataService.MarcaComedorResultado = ComedorSvc.RegistrarMarca(usuario.IdUsuario, usuario.CodTipo, EsBecadoHoy(usuario), Cn)
            TxtTiquetes.Text = resultado.TextoTiquetes
            ErrorTiquetes = resultado.ErrorTiquetes

            If resultado.RegistroGuardado Then
                RegistrarAsistenciaComedorHoy(usuario)
                UltimoCarnetProcesado = usuario.Cedula
                EstadoVerificado = True
            End If
        Catch ex As Exception
            EstadoVerificado = False
            ErrorLogger.LogException("ControlComedor.RegistrarMarca", ex)
            UpdateVisualState(EstadoVisual.NotFound)
        End Try
    End Sub

    Private Sub CargarDatosUsuario(ByVal usuario As UsuarioComedorSnapshot, ByVal limpiarTiquetes As Boolean)
        LblCedula.Text = usuario.Cedula
        TxtUsuario.Text = usuario.NombreCompleto

        If EsEstudiante(usuario) Then
            TxtTipo.Text = "ESTUDIANTE"
        Else
            TxtTipo.Text = "PROFESOR"
        End If

        If limpiarTiquetes Then
            TxtTiquetes.Clear()
        End If
    End Sub

    Private Sub RegistrarDenegacionPorRegla(ByVal usuario As UsuarioComedorSnapshot)
        CargarDatosUsuario(usuario, True)
        _totalErrores += 1
        UpdateVisualState(EstadoVisual.DeniedByRule)
        RegistrarTiempoAtencion()
        ActualizarKpisOperacion()
        EnsureScanFocus(True)
    End Sub

    Private Sub RegistrarLecturaDuplicada(ByVal usuario As UsuarioComedorSnapshot,
                                          ByVal mensaje As String,
                                          ByVal detalle As String)
        ErrorLecturaDuplicada = True
        _totalDuplicadas += 1
        CargarDatosUsuario(usuario, True)
        _mensajeDuplicadoPersonalizado = mensaje
        _detalleDuplicadoPersonalizado = detalle
        UpdateVisualState(EstadoVisual.Duplicate)
        RegistrarTiempoAtencion()
        ActualizarKpisOperacion()
        EnsureScanFocus(True)
    End Sub

    Private Sub RecargarDatosOperacion()
        ' El formulario trabaja con snapshots indexados para que cada lectura evite recorridos completos en memoria.
        Dim dsUsuarios As DataSet = ComedorSvc.CargarUsuariosConMarcaTransporte(Cn, FechaServer)
        _usuariosPorCarnet = ConstruirIndiceUsuarios(dsUsuarios)
        _diasBecaPorId = ConstruirIndiceDiasBeca(ComedorSvc.CargarBecas(Cn))
        _horaLimitePorHorario = ConstruirIndiceHorarios(ComedorSvc.CargarHorarios(Cn))
        _asistenciasComedorHoy = ComedorSvc.CargarAsistenciasComedorDia(Cn, FechaServer)
        _fechaAsistenciaComedorCache = FechaServer.Date
        _asistenciasValidasDia = ObtenerTotalAsistenciasValidasDia()

        ' La meta diaria se calcula desde el snapshot cargado para no castigar la BD en cada lectura.
        Dim metaCalculada As Integer = CalcularMetaDiariaDesdeSnapshot(_usuariosPorCarnet.Values)
        _metaDiaria = Math.Max(0, metaCalculada)

        If _lblMeta IsNot Nothing Then
            ActualizarKpisOperacion()
        End If
    End Sub

    Private Function ConstruirIndiceUsuarios(ByVal dsUsuarios As DataSet) As Dictionary(Of String, UsuarioComedorSnapshot)
        Dim indice As New Dictionary(Of String, UsuarioComedorSnapshot)(StringComparer.OrdinalIgnoreCase)

        If dsUsuarios Is Nothing OrElse dsUsuarios.Tables.Count = 0 Then
            Return indice
        End If

        For Each row As DataRow In dsUsuarios.Tables(0).Rows
            Dim usuario As UsuarioComedorSnapshot = MapearUsuario(row)
            Dim cedula As String = usuario.Cedula
            If cedula.Length > 0 AndAlso Not indice.ContainsKey(cedula) Then
                indice.Add(cedula, usuario)
            End If
        Next

        Return indice
    End Function

    Private Function MapearUsuario(ByVal row As DataRow) As UsuarioComedorSnapshot
        Dim usuario As New UsuarioComedorSnapshot()
        usuario.IdUsuario = CInt(row("IdUsuario"))
        usuario.TieneTipoBeca = Not IsDBNull(row("TipoBeca"))
        If usuario.TieneTipoBeca Then
            usuario.TipoBecaId = CInt(row("TipoBeca"))
        End If
        If Not IsDBNull(row("CantidadTiquetes")) Then
            usuario.CantidadTiquetes = CInt(row("CantidadTiquetes"))
        End If
        usuario.Nombre = CStr(row("Nombre")).Trim()
        usuario.PrimerApellido = CStr(row("PrimerApellido")).Trim()
        usuario.SegundoApellido = CStr(row("SegundoApellido")).Trim()
        usuario.CodTipo = CShort(row("CodTipo"))
        usuario.Cedula = CStr(row("Cedula")).Trim()
        usuario.TieneHorario = Not IsDBNull(row("IdHorario"))
        If usuario.TieneHorario Then
            usuario.IdHorario = CInt(row("IdHorario"))
        End If
        usuario.MarcaTransporte = Not IsDBNull(row("MarcaTransporte")) AndAlso CInt(row("MarcaTransporte")) <> 0
        usuario.TieneHoraMarca = Not IsDBNull(row("HoraMarca"))
        If usuario.TieneHoraMarca Then
            usuario.HoraMarca = CDate(row("HoraMarca"))
        End If
        Return usuario
    End Function

    Private Function ConstruirIndiceDiasBeca(ByVal dsBecas As DataSet) As Dictionary(Of Integer, String)
        Dim indice As New Dictionary(Of Integer, String)()

        If dsBecas Is Nothing OrElse dsBecas.Tables.Count = 0 Then
            Return indice
        End If

        For Each row As DataRow In dsBecas.Tables(0).Rows
            If IsDBNull(row("IdBeca")) Then
                Continue For
            End If

            indice(CInt(row("IdBeca"))) = CStr(row("DiasBeca"))
        Next

        Return indice
    End Function

    Private Function ConstruirIndiceHorarios(ByVal dsHorarios As DataSet) As Dictionary(Of Integer, TimeSpan)
        Dim indice As New Dictionary(Of Integer, TimeSpan)()

        If dsHorarios Is Nothing OrElse dsHorarios.Tables.Count = 0 Then
            Return indice
        End If

        For Each row As DataRow In dsHorarios.Tables(0).Rows
            If IsDBNull(row("IdHorario")) Then
                Continue For
            End If

            indice(CInt(row("IdHorario"))) = ConvertirATimeSpan(row("HoraLimite"))
        Next

        Return indice
    End Function

    Private Function CalcularMetaDiariaDesdeSnapshot(ByVal usuarios As IEnumerable(Of UsuarioComedorSnapshot)) As Integer
        If usuarios Is Nothing Then
            Return -1
        End If

        Dim idsContados As New HashSet(Of Integer)()

        For Each usuario As UsuarioComedorSnapshot In usuarios
            If usuario Is Nothing OrElse Not EsEstudiante(usuario) Then
                Continue For
            End If

            If idsContados.Contains(usuario.IdUsuario) Then
                Continue For
            End If

            Dim incluirEnMeta As Boolean = EsBecadoHoy(usuario)
            If Not incluirEnMeta Then
                incluirEnMeta = TieneTiquetesDisponibles(usuario)
            End If

            If incluirEnMeta Then
                idsContados.Add(usuario.IdUsuario)
            End If
        Next

        Return idsContados.Count
    End Function

    Private Function BuscarUsuarioPorCarnet(ByVal carnet As String) As UsuarioComedorSnapshot
        If _usuariosPorCarnet Is Nothing OrElse _usuariosPorCarnet.Count = 0 Then
            Return Nothing
        End If

        Dim usuario As UsuarioComedorSnapshot = Nothing
        If _usuariosPorCarnet.TryGetValue(carnet, usuario) Then
            Return usuario
        End If

        Return Nothing
    End Function

    Private Function EsEstudiante(ByVal usuario As UsuarioComedorSnapshot) As Boolean
        Return usuario IsNot Nothing AndAlso usuario.CodTipo = 1S
    End Function

    Private Function TieneTiquetesDisponibles(ByVal usuario As UsuarioComedorSnapshot) As Boolean
        Return usuario IsNot Nothing AndAlso usuario.CantidadTiquetes >= 1
    End Function

    Private Function TieneAdvertenciaSinMarcaTransporte(ByVal usuario As UsuarioComedorSnapshot) As Boolean
        Return usuario IsNot Nothing AndAlso Not usuario.MarcaTransporte
    End Function

    Private Function TieneAdvertenciaMarcaTardia(ByVal usuario As UsuarioComedorSnapshot) As Boolean
        If _horaLimitePorHorario Is Nothing OrElse _horaLimitePorHorario.Count = 0 Then
            Return False
        End If

        If usuario Is Nothing OrElse Not usuario.TieneHorario OrElse Not usuario.TieneHoraMarca Then
            Return False
        End If

        Dim horaLimite As TimeSpan = TimeSpan.Zero
        If Not _horaLimitePorHorario.TryGetValue(usuario.IdHorario, horaLimite) Then
            Return False
        End If

        Dim horaMarca As TimeSpan = usuario.HoraMarca.TimeOfDay
        Return horaMarca > horaLimite
    End Function

    Private Function TieneAsistenciaComedorHoy(ByVal usuario As UsuarioComedorSnapshot) As Boolean
        If usuario Is Nothing Then
            Return False
        End If

        If _asistenciasComedorHoy Is Nothing OrElse _asistenciasComedorHoy.Count = 0 Then
            Return False
        End If

        Return _asistenciasComedorHoy.Contains(usuario.IdUsuario)
    End Function

    Private Sub RegistrarAsistenciaComedorHoy(ByVal usuario As UsuarioComedorSnapshot)
        If usuario Is Nothing Then
            Exit Sub
        End If

        If Not EsEstudiante(usuario) Then
            Exit Sub
        End If

        If _asistenciasComedorHoy Is Nothing Then
            _asistenciasComedorHoy = New HashSet(Of Integer)()
        End If

        _asistenciasComedorHoy.Add(usuario.IdUsuario)
        _asistenciasValidasDia = ObtenerTotalAsistenciasValidasDia()
    End Sub

    Private Function ObtenerTotalAsistenciasValidasDia() As Integer
        If _asistenciasComedorHoy Is Nothing Then
            Return 0
        End If

        Return _asistenciasComedorHoy.Count
    End Function

    Private Function EsBecadoHoy(ByVal usuario As UsuarioComedorSnapshot) As Boolean
        If _diasBecaPorId Is Nothing OrElse _diasBecaPorId.Count = 0 Then
            Return False
        End If

        If usuario Is Nothing OrElse Not usuario.TieneTipoBeca Then
            Return False
        End If

        Dim diasBeca As String = String.Empty
        If Not _diasBecaPorId.TryGetValue(usuario.TipoBecaId, diasBeca) Then
            Return False
        End If

        Return InStr(diasBeca, DiaSemana) > 0
    End Function

    Private Function ConvertirATimeSpan(ByVal raw As Object) As TimeSpan
        If raw Is Nothing OrElse IsDBNull(raw) Then
            Return TimeSpan.Zero
        End If

        If TypeOf raw Is TimeSpan Then
            Return CType(raw, TimeSpan)
        End If

        If TypeOf raw Is DateTime Then
            Return CType(raw, DateTime).TimeOfDay
        End If

        Dim parsed As TimeSpan
        If TimeSpan.TryParse(CStr(raw), parsed) Then
            Return parsed
        End If

        Return TimeSpan.Zero
    End Function

    Private Sub ResetResultFields()
        LimpiarMensajesDuplicado()
        LimpiarContextoUltimoRegistro()
        TxtCedula.Clear()
    End Sub

    Private Sub LimpiarContextoUltimoRegistro()
        LblCedula.Clear()
        TxtUsuario.Clear()
        TxtTipo.Clear()
        TxtTiquetes.Clear()
        LblRegistroError.Text = String.Empty
    End Sub

    Private Sub LimpiarMensajesDuplicado()
        _mensajeDuplicadoPersonalizado = String.Empty
        _detalleDuplicadoPersonalizado = String.Empty
    End Sub

    Private Sub ApplyBrandAssets()
        PicBrandHeader.Visible = False
        Picture.Visible = False
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
        ApplyBrandAssets()

        PanelResult.BackColor = Color.FromArgb(242, 246, 252)
        PanelMainStatus.BackColor = Color.White
        PanelMainStatus.BorderStyle = BorderStyle.None

        BunifuGradientPanel1.BackColor = Color.FromArgb(13, 30, 54)

        LblTitulo.Font = New Font("Segoe UI Semibold", 20.0!, FontStyle.Bold)
        LblTitulo.ForeColor = Color.FromArgb(17, 33, 59)
        LblTitulo.TextAlign = ContentAlignment.MiddleCenter
        LblTitulo.AutoEllipsis = True
        LblTitulo.Padding = New Padding(18, 0, 18, 0)
        LblTitulo.BackColor = Color.FromArgb(229, 236, 246)

        lblProcesando.Font = New Font("Segoe UI Semibold", 28.0!, FontStyle.Bold)
        lblProcesando.ForeColor = Color.FromArgb(23, 32, 51)
        lblProcesando.TextAlign = ContentAlignment.MiddleCenter

        LblFecha.ForeColor = Color.FromArgb(214, 226, 246)
        LblScanHint.ForeColor = Color.FromArgb(220, 232, 252)
        LblScanHint.Text = ObtenerTextoScanHint()
        LblScanHint.AutoSize = False
        LblScanHint.TextAlign = ContentAlignment.MiddleLeft
        LblFecha.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
        LblScanHint.Font = New Font("Segoe UI", 11.0!, FontStyle.Bold)
        LblFecha.TextAlign = ContentAlignment.MiddleLeft
        LblFecha.Padding = New Padding(22, 0, 0, 0)
        LblScanHint.BackColor = Color.FromArgb(18, 42, 74)
        LblScanHint.Padding = New Padding(14, 0, 14, 0)

        TxtCedula.Font = New Font("Segoe UI Semibold", 19.0!, FontStyle.Bold)
        TxtCedula.BorderStyle = BorderStyle.FixedSingle
        TxtCedula.BackColor = Color.White
        TxtCedula.ForeColor = Color.FromArgb(17, 33, 59)

        GbDatos.BackColor = Color.FromArgb(252, 253, 255)
        GbDatos.ForeColor = Color.FromArgb(36, 51, 77)
        GbDatos.FlatStyle = FlatStyle.Flat
        GbDatos.Font = New Font("Segoe UI", 11.0!, FontStyle.Bold)
        GbDatos.Padding = New Padding(12, 14, 12, 12)

        LblUsuarioCaption.ForeColor = Color.FromArgb(76, 90, 112)
        LblTiquetesCaption.ForeColor = Color.FromArgb(76, 90, 112)
        LblCarnetCaption.ForeColor = Color.FromArgb(76, 90, 112)
        LblTipoCaption.ForeColor = Color.FromArgb(76, 90, 112)
        LblRegistroError.ForeColor = Color.FromArgb(161, 47, 65)
        LblRegistroError.Font = New Font("Segoe UI", 9.5!, FontStyle.Bold)

        LblCedula.Font = New Font("Segoe UI", 12.5!, FontStyle.Bold)
        TxtUsuario.Font = New Font("Segoe UI Semibold", 16.0!, FontStyle.Bold)
        TxtTipo.Font = New Font("Segoe UI", 13.0!, FontStyle.Bold)
        TxtTiquetes.Font = New Font("Segoe UI", 24.0!, FontStyle.Bold)
        AplicarEstiloCampoLectura(LblCedula)
        AplicarEstiloCampoLectura(TxtUsuario)
        AplicarEstiloCampoLectura(TxtTipo)
        AplicarEstiloCampoLectura(TxtTiquetes)
        TxtUsuario.BackColor = Color.FromArgb(228, 236, 247)
        TxtUsuario.ForeColor = Color.FromArgb(14, 32, 57)

        Picture.BorderStyle = BorderStyle.None
        Picture.BackColor = Color.FromArgb(13, 30, 54)
        Picture.SizeMode = PictureBoxSizeMode.Zoom
        Picture.Visible = False

        BtnSalir.Visible = False
        BtnSalir.Enabled = False
        BtnSalir.TabStop = False
        BtnSalir.FlatStyle = FlatStyle.Flat
        BtnSalir.FlatAppearance.BorderSize = 0
        BtnSalir.BackColor = Color.FromArgb(161, 47, 65)
        BtnSalir.ForeColor = Color.White
        BtnSalir.Font = New Font("Segoe UI", 9.5!, FontStyle.Bold)
        BtnSalir.Text = String.Empty

        GbDatos.Text = "Último registro"
        LblCarnetCaption.Text = "Carnet"
        LblUsuarioCaption.Text = "Estudiante"
        LblTiquetesCaption.Text = "Tiquetes"

        LblTipoCaption.Visible = False
        TxtTipo.Visible = False
        AplicarCamposLecturaNoFocusables()

        If _modoAccesible Then
            TxtCedula.Font = New Font("Segoe UI Semibold", 24.0!, FontStyle.Bold)
            lblProcesando.Font = New Font("Segoe UI Semibold", 28.0!, FontStyle.Bold)
            LblScanHint.Font = New Font("Segoe UI", 14.0!, FontStyle.Bold)
            PanelMainStatus.BorderStyle = BorderStyle.Fixed3D
            If _lblResultadoOperacion IsNot Nothing Then
                _lblResultadoOperacion.Font = New Font("Segoe UI Semibold", 20.0!, FontStyle.Bold)
            End If
            If _lblUltimaLectura IsNot Nothing Then
                _lblUltimaLectura.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
            End If
            If _lblEdadEstado IsNot Nothing Then
                _lblEdadEstado.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
            End If
        End If

        If _lblFocusEscaneo IsNot Nothing Then
            _lblFocusEscaneo.BackColor = Color.FromArgb(15, 36, 63)
            _lblFocusEscaneo.Padding = New Padding(12, 0, 12, 0)
        End If
        EnsureSidebarDecorators()
        AplicarModoAltoContraste()
        AplicarPrivacidadEscaneo()
        ResetResultFields()
    End Sub

    Private Sub ApplyResponsiveLayout()
        If _lblMeta Is Nothing OrElse _lblHistorial Is Nothing OrElse _lstHistorial Is Nothing Then
            Exit Sub
        End If

        Dim mode As LayoutMode = ObtenerModoLayout()
        ApplyResponsiveTypography(mode)

        Dim leftWidth As Integer
        Dim pad As Integer
        Dim tituloTop As Integer
        Dim tituloHeight As Integer
        Dim panelTop As Integer
        Dim panelBottomMargin As Integer
        Dim panelMinHeight As Integer
        Dim chipWidth As Integer
        Dim chipHeight As Integer
        Dim chipTop As Integer
        Dim statusPadding As Integer
        Dim messageTop As Integer
        Dim messageHeight As Integer
        Dim titleInset As Integer

        Select Case mode
            Case LayoutMode.Narrow1366
                leftWidth = Math.Max(360, Math.Min(392, CInt(Math.Round(Me.ClientSize.Width * 0.285R))))
                pad = 14
                tituloTop = 12
                tituloHeight = 34
                panelTop = 56
                panelBottomMargin = 14
                panelMinHeight = 250
                chipWidth = 224
                chipHeight = 28
                chipTop = 18
                statusPadding = 50
                messageTop = 64
                messageHeight = 84
                titleInset = 28
            Case LayoutMode.Compact
                leftWidth = Math.Max(390, Math.Min(430, CInt(Math.Round(Me.ClientSize.Width * 0.292R))))
                pad = 16
                tituloTop = 14
                tituloHeight = 36
                panelTop = 62
                panelBottomMargin = 16
                panelMinHeight = 280
                chipWidth = 232
                chipHeight = 30
                chipTop = 20
                statusPadding = 64
                messageTop = 78
                messageHeight = 92
                titleInset = 42
            Case LayoutMode.Wide
                leftWidth = Math.Max(460, Math.Min(520, CInt(Math.Round(Me.ClientSize.Width * 0.305R))))
                pad = 24
                tituloTop = 18
                tituloHeight = 42
                panelTop = 74
                panelBottomMargin = 20
                panelMinHeight = 340
                chipWidth = 244
                chipHeight = 32
                chipTop = 24
                statusPadding = 86
                messageTop = 98
                messageHeight = 110
                titleInset = 64
            Case Else
                leftWidth = Math.Max(420, Math.Min(470, CInt(Math.Round(Me.ClientSize.Width * 0.3R))))
                pad = 20
                tituloTop = 16
                tituloHeight = 38
                panelTop = 68
                panelBottomMargin = 18
                panelMinHeight = 312
                chipWidth = 236
                chipHeight = 30
                chipTop = 22
                statusPadding = 76
                messageTop = 90
                messageHeight = 104
                titleInset = 56
        End Select

        BunifuGradientPanel1.Width = leftWidth

        Dim contentX As Integer = leftWidth + pad
        Dim contentWidth As Integer = Math.Max(420, Me.ClientSize.Width - contentX - pad - 2)

        LblTitulo.SetBounds(contentX + titleInset, tituloTop, Math.Max(250, contentWidth - (titleInset * 2)), tituloHeight)
        PicBrandHeader.SetBounds(0, 0, 0, 0)
        PanelMainStatus.SetBounds(contentX, panelTop, contentWidth, Math.Max(panelMinHeight, Me.ClientSize.Height - panelTop - panelBottomMargin))

        Dim statusWidth As Integer = PanelMainStatus.ClientSize.Width
        Dim statusHeight As Integer = PanelMainStatus.ClientSize.Height
        If _lblResultadoOperacion IsNot Nothing Then
            _lblResultadoOperacion.SetBounds(30, messageTop - 26, Math.Max(240, statusWidth - 60), 28)
        End If
        If _lblEstadoChip IsNot Nothing Then
            _lblEstadoChip.SetBounds((statusWidth - chipWidth) \ 2, chipTop, chipWidth, chipHeight)
        End If

        lblProcesando.SetBounds(statusPadding, messageTop, Math.Max(260, statusWidth - (statusPadding * 2)), messageHeight)

        Dim minIcon As Integer
        Dim maxIcon As Integer
        Dim iconFactor As Double
        Select Case mode
            Case LayoutMode.Narrow1366
                minIcon = 140
                maxIcon = 205
                iconFactor = 0.2R
            Case LayoutMode.Compact
                minIcon = 155
                maxIcon = 220
                iconFactor = 0.225R
            Case LayoutMode.Wide
                minIcon = 185
                maxIcon = 260
                iconFactor = 0.28R
            Case Else
                minIcon = 170
                maxIcon = 240
                iconFactor = 0.245R
        End Select

        Dim iconSize As Integer = Math.Max(minIcon, Math.Min(maxIcon, CInt(Math.Round(Math.Min(statusWidth, statusHeight) * iconFactor))))
        Dim iconX As Integer = Math.Max(0, (statusWidth - iconSize) \ 2)
        Dim iconY As Integer = Math.Max(lblProcesando.Bottom + 12, (statusHeight - iconSize) \ 2 + If(mode = LayoutMode.Narrow1366, 8, 14))
        Imgprocess.SetBounds(iconX, iconY, iconSize, iconSize)

        ApplySidebarLayout(leftWidth, mode)
        ApplyDynamicOperationalLayout(leftWidth, mode)
        ApplyIncidenciaLayout(mode)
        AplicarDecoracionOperativa()
    End Sub

    Private Sub ApplySupplementaryMetricsLayout()
        If _lblMeta Is Nothing Then
            Exit Sub
        End If

        Dim leftWidth As Integer = Math.Max(SidebarMinWidth, BunifuGradientPanel1.ClientSize.Width)
        Dim leftHeight As Integer = Math.Max(420, BunifuGradientPanel1.ClientSize.Height)
        Dim statusWidth As Integer = Math.Max(380, PanelMainStatus.ClientSize.Width)

        _lblMeta.SetBounds(20, 14, leftWidth - 40, 22)
        _progressMeta.SetBounds(20, 38, leftWidth - 40, 14)
        _lblUltimaLectura.SetBounds(20, 56, leftWidth - 40, 22)
        _lblEdadEstado.SetBounds(20, 78, leftWidth - 40, 22)
        _lblConexion.SetBounds(20, leftHeight - 164, leftWidth - 40, 20)
        _lblKpi.SetBounds(20, leftHeight - 142, leftWidth - 40, 20)
        _lblCola.SetBounds(20, leftHeight - 120, leftWidth - 40, 20)
        _lblAlertas.SetBounds(20, leftHeight - 98, leftWidth - 40, 20)
        _lblRecomendacion.SetBounds(20, leftHeight - 76, leftWidth - 40, 36)
        If _panelSidebarDividerMetricas IsNot Nothing Then
            _panelSidebarDividerMetricas.SetBounds(20, _lblConexion.Top - 12, leftWidth - 40, 1)
        End If
        _lblHotkeys.SetBounds(20, leftHeight - 38, leftWidth - 40, 32)

        _lblResultadoOperacion.SetBounds(36, 58, Math.Max(260, statusWidth - 72), 30)
        If _lblEstadoChip IsNot Nothing Then
            _lblEstadoChip.SetBounds((statusWidth \ 2) - 118, 24, 236, 30)
        End If

        Dim gX As Integer = 12
        Dim gW As Integer = Math.Max(220, GbDatos.ClientSize.Width - 28)
        _lblHistorial.SetBounds(gX, 264, gW, 18)
        _lstHistorial.SetBounds(gX, 286, gW, Math.Max(120, GbDatos.ClientSize.Height - 296))
    End Sub

    Private Sub ApplySidebarLayout(ByVal leftWidth As Integer, ByVal mode As LayoutMode)
        If _lblHistorial Is Nothing OrElse _lstHistorial Is Nothing Then
            Exit Sub
        End If

        Dim innerX As Integer = 20
        Dim innerW As Integer = leftWidth - (innerX * 2)
        Dim sidebarHeight As Integer = Math.Max(600, BunifuGradientPanel1.ClientSize.Height)
        Dim topY As Integer
        Dim scanHintHeight As Integer
        Dim focusHeight As Integer
        Dim cedulaHeight As Integer
        Dim footerReserve As Integer
        Dim minCardHeight As Integer
        Dim rowTop As Integer
        Dim captionHeight As Integer
        Dim valueHeight As Integer
        Dim nombreHeight As Integer
        Dim tiquetesHeight As Integer
        Dim errorHeight As Integer
        Dim historyMinHeight As Integer
        Dim sectionGap As Integer

        Select Case mode
            Case LayoutMode.Narrow1366
                topY = 74
                scanHintHeight = 38
                focusHeight = 18
                cedulaHeight = 42
                footerReserve = 164
                minCardHeight = 270
                rowTop = 22
                captionHeight = 16
                valueHeight = 30
                nombreHeight = 42
                tiquetesHeight = 44
                errorHeight = 38
                historyMinHeight = 44
                sectionGap = 10
            Case LayoutMode.Compact
                topY = 82
                scanHintHeight = 40
                focusHeight = 18
                cedulaHeight = 44
                footerReserve = 186
                minCardHeight = 300
                rowTop = 24
                captionHeight = 18
                valueHeight = 32
                nombreHeight = 46
                tiquetesHeight = 46
                errorHeight = 40
                historyMinHeight = 48
                sectionGap = 10
            Case LayoutMode.Wide
                topY = 96
                scanHintHeight = 46
                focusHeight = 20
                cedulaHeight = 48
                footerReserve = 226
                minCardHeight = 356
                rowTop = 28
                captionHeight = 18
                valueHeight = 34
                nombreHeight = 52
                tiquetesHeight = 52
                errorHeight = 42
                historyMinHeight = 60
                sectionGap = 12
            Case Else
                topY = 90
                scanHintHeight = 44
                focusHeight = 20
                cedulaHeight = 46
                footerReserve = 208
                minCardHeight = 332
                rowTop = 26
                captionHeight = 18
                valueHeight = 34
                nombreHeight = 48
                tiquetesHeight = 50
                errorHeight = 40
                historyMinHeight = 52
                sectionGap = 12
        End Select

        Picture.SetBounds(0, 0, 0, 0)
        LblScanHint.SetBounds(innerX, topY, innerW, scanHintHeight)
        If _mostrarEntradaManualTemporal Then
            TxtCedula.SetBounds(innerX, LblScanHint.Bottom + 26, innerW, cedulaHeight)
        Else
            TxtCedula.SetBounds(2, 2, 1, 1)
        End If
        If _lblFocusEscaneo IsNot Nothing Then
            _lblFocusEscaneo.SetBounds(innerX, LblScanHint.Bottom + 6, innerW, focusHeight)
        End If

        Dim gbTop As Integer = If(_mostrarEntradaManualTemporal, TxtCedula.Bottom + 12, _lblFocusEscaneo.Bottom + 16)
        Dim gbMaxBottom As Integer = sidebarHeight - footerReserve
        Dim gbHeight As Integer = gbMaxBottom - gbTop
        gbHeight = Math.Max(minCardHeight, gbHeight)
        GbDatos.SetBounds(12, gbTop, leftWidth - 24, gbHeight)
        If _panelSidebarDividerRegistro IsNot Nothing Then
            _panelSidebarDividerRegistro.SetBounds(innerX, gbTop - 14, innerW, 1)
        End If

        Dim gX As Integer = 14
        Dim gW As Integer = GbDatos.ClientSize.Width - 28
        LblUsuarioCaption.SetBounds(gX, rowTop, gW, captionHeight)
        TxtUsuario.SetBounds(gX, LblUsuarioCaption.Bottom + 2, gW, nombreHeight)
        LblCarnetCaption.SetBounds(gX, TxtUsuario.Bottom + sectionGap, gW, captionHeight)
        LblCedula.SetBounds(gX, LblCarnetCaption.Bottom + 2, gW, valueHeight)
        LblTiquetesCaption.SetBounds(gX, LblCedula.Bottom + sectionGap, gW, captionHeight)
        TxtTiquetes.SetBounds(gX, LblTiquetesCaption.Bottom + 2, gW, tiquetesHeight)
        LblRegistroError.SetBounds(gX, TxtTiquetes.Bottom + 10, gW, errorHeight)

        Dim historialTop As Integer = LblRegistroError.Bottom + 10
        _lblHistorial.SetBounds(gX, historialTop, gW, captionHeight)
        Dim historialListTop As Integer = _lblHistorial.Bottom + 6
        _lstHistorial.SetBounds(gX, historialListTop, gW, Math.Max(historyMinHeight, GbDatos.ClientSize.Height - historialListTop - 12))

        BtnSalir.SetBounds(0, 0, 0, 0)
    End Sub

    Private Sub ApplyDynamicOperationalLayout(ByVal leftWidth As Integer, ByVal mode As LayoutMode)
        If _lblMeta Is Nothing OrElse _lblHistorial Is Nothing OrElse _lstHistorial Is Nothing Then
            Exit Sub
        End If

        Dim sidebarHeight As Integer = Math.Max(600, BunifuGradientPanel1.ClientSize.Height)
        Dim hotkeysHeight As Integer
        Dim lineHeight As Integer
        Dim recommendationHeight As Integer
        Dim footerBottom As Integer

        Select Case mode
            Case LayoutMode.Narrow1366
                hotkeysHeight = 24
                lineHeight = 18
                recommendationHeight = 26
                footerBottom = 8
            Case LayoutMode.Compact
                hotkeysHeight = 26
                lineHeight = 18
                recommendationHeight = 30
                footerBottom = 8
            Case Else
                hotkeysHeight = 30
                lineHeight = 20
                recommendationHeight = 34
                footerBottom = 4
        End Select

        Dim hotkeysTop As Integer = sidebarHeight - hotkeysHeight - footerBottom
        Dim infoBottom As Integer = hotkeysTop - 4
        Dim recomendacionTop As Integer = infoBottom - recommendationHeight
        Dim alertaTop As Integer = recomendacionTop - lineHeight
        Dim colaTop As Integer = alertaTop - lineHeight
        Dim kpiTop As Integer = colaTop - lineHeight
        Dim conexionTop As Integer = kpiTop - lineHeight

        _lblMeta.SetBounds(20, 14, leftWidth - 40, 22)
        _progressMeta.SetBounds(20, 38, leftWidth - 40, 14)
        _lblUltimaLectura.SetBounds(20, 56, leftWidth - 40, 22)
        _lblEdadEstado.SetBounds(20, 78, leftWidth - 40, 22)
        If _panelSidebarDividerMetricas IsNot Nothing Then
            _panelSidebarDividerMetricas.SetBounds(20, conexionTop - 12, leftWidth - 40, 1)
        End If
        _lblConexion.SetBounds(20, conexionTop, leftWidth - 40, 20)
        _lblKpi.SetBounds(20, kpiTop, leftWidth - 40, 20)
        _lblCola.SetBounds(20, colaTop, leftWidth - 40, 20)
        _lblAlertas.SetBounds(20, alertaTop, leftWidth - 40, 20)
        _lblRecomendacion.SetBounds(20, recomendacionTop, leftWidth - 40, recommendationHeight)
        _lblHotkeys.SetBounds(20, hotkeysTop, leftWidth - 40, hotkeysHeight)

        Dim gX As Integer = 12
        Dim gW As Integer = GbDatos.ClientSize.Width - 28

        If mode = LayoutMode.Narrow1366 Then
            LblScanHint.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
            _lblHotkeys.Font = New Font("Segoe UI", 7.8!, FontStyle.Bold)
        ElseIf mode = LayoutMode.Compact Then
            LblScanHint.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
            _lblHotkeys.Font = New Font("Segoe UI", 8.0!, FontStyle.Bold)
        Else
            LblScanHint.Font = New Font("Segoe UI", 11.0!, FontStyle.Bold)
            _lblHotkeys.Font = New Font("Segoe UI", 8.5!, FontStyle.Bold)
        End If
    End Sub

    Private Sub ApplyIncidenciaLayout(ByVal mode As LayoutMode)
        If _btnIncidencia Is Nothing Then
            Exit Sub
        End If

        Dim buttonWidth As Integer
        Dim buttonHeight As Integer
        Dim marginRight As Integer
        Dim marginBottom As Integer

        Select Case mode
            Case LayoutMode.Narrow1366
                buttonWidth = 152
                buttonHeight = 32
                marginRight = 16
                marginBottom = 14
            Case LayoutMode.Compact
                buttonWidth = 160
                buttonHeight = 32
                marginRight = 18
                marginBottom = 16
            Case Else
                buttonWidth = 170
                buttonHeight = 34
                marginRight = 24
                marginBottom = 20
        End Select

        Dim x As Integer = Math.Max(BunifuGradientPanel1.Right + 20, PanelMainStatus.Right - buttonWidth - marginRight)
        Dim y As Integer = Math.Max(PanelMainStatus.Top + 20, PanelMainStatus.Bottom - buttonHeight - marginBottom)

        _btnIncidencia.SetBounds(x, y, buttonWidth, buttonHeight)
        _btnIncidencia.BringToFront()
    End Sub

    Private Sub ApplyResponsiveTypography(ByVal mode As LayoutMode)
        Select Case mode
            Case LayoutMode.Narrow1366
                LblTitulo.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 20.0!, 18.0!), FontStyle.Bold)
                lblProcesando.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 24.0!, 22.0!), FontStyle.Bold)
                LblFecha.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
                LblScanHint.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
                TxtCedula.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 20.0!, 16.0!), FontStyle.Bold)
                LblCedula.Font = New Font("Segoe UI", 11.5!, FontStyle.Bold)
                TxtUsuario.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 17.0!, 14.5!), FontStyle.Bold)
                TxtTiquetes.Font = New Font("Segoe UI", If(_modoAccesible, 24.0!, 21.0!), FontStyle.Bold)
                LblRegistroError.Font = New Font("Segoe UI", 8.8!, FontStyle.Bold)
                GbDatos.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
                _lblMeta.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
                _lblUltimaLectura.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
                _lblEdadEstado.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
                _btnIncidencia.Font = New Font("Segoe UI", 8.5!, FontStyle.Bold)
            Case LayoutMode.Compact
                LblTitulo.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 21.0!, 19.0!), FontStyle.Bold)
                lblProcesando.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 26.0!, 24.0!), FontStyle.Bold)
                LblFecha.Font = New Font("Segoe UI", 9.5!, FontStyle.Bold)
                LblScanHint.Font = New Font("Segoe UI", 10.5!, FontStyle.Bold)
                TxtCedula.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 22.0!, 17.5!), FontStyle.Bold)
                LblCedula.Font = New Font("Segoe UI", 12.0!, FontStyle.Bold)
                TxtUsuario.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 18.0!, 15.5!), FontStyle.Bold)
                TxtTiquetes.Font = New Font("Segoe UI", If(_modoAccesible, 26.0!, 22.0!), FontStyle.Bold)
                LblRegistroError.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
                GbDatos.Font = New Font("Segoe UI", 10.5!, FontStyle.Bold)
                _lblMeta.Font = New Font("Segoe UI", 9.5!, FontStyle.Bold)
                _lblUltimaLectura.Font = New Font("Segoe UI", 9.5!, FontStyle.Bold)
                _lblEdadEstado.Font = New Font("Segoe UI", 9.5!, FontStyle.Bold)
                _btnIncidencia.Font = New Font("Segoe UI", 8.8!, FontStyle.Bold)
            Case LayoutMode.Wide
                LblTitulo.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 24.0!, 22.0!), FontStyle.Bold)
                lblProcesando.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 32.0!, 30.0!), FontStyle.Bold)
                LblFecha.Font = New Font("Segoe UI", 10.5!, FontStyle.Bold)
                LblScanHint.Font = New Font("Segoe UI", 11.5!, FontStyle.Bold)
                TxtCedula.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 25.0!, 20.0!), FontStyle.Bold)
                LblCedula.Font = New Font("Segoe UI", 13.0!, FontStyle.Bold)
                TxtUsuario.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 21.0!, 18.0!), FontStyle.Bold)
                TxtTiquetes.Font = New Font("Segoe UI", If(_modoAccesible, 30.0!, 26.0!), FontStyle.Bold)
                LblRegistroError.Font = New Font("Segoe UI", 9.8!, FontStyle.Bold)
                GbDatos.Font = New Font("Segoe UI", 11.5!, FontStyle.Bold)
                _lblMeta.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
                _lblUltimaLectura.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
                _lblEdadEstado.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
                _btnIncidencia.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
            Case Else
                LblTitulo.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 22.0!, 20.0!), FontStyle.Bold)
                lblProcesando.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 28.0!, 26.0!), FontStyle.Bold)
                LblFecha.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
                LblScanHint.Font = New Font("Segoe UI", 11.0!, FontStyle.Bold)
                TxtCedula.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 24.0!, 19.0!), FontStyle.Bold)
                LblCedula.Font = New Font("Segoe UI", 12.5!, FontStyle.Bold)
                TxtUsuario.Font = New Font("Segoe UI Semibold", If(_modoAccesible, 19.5!, 16.5!), FontStyle.Bold)
                TxtTiquetes.Font = New Font("Segoe UI", If(_modoAccesible, 28.0!, 24.0!), FontStyle.Bold)
                LblRegistroError.Font = New Font("Segoe UI", 9.5!, FontStyle.Bold)
                GbDatos.Font = New Font("Segoe UI", 11.0!, FontStyle.Bold)
                _lblMeta.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
                _lblUltimaLectura.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
                _lblEdadEstado.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
                _btnIncidencia.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
        End Select
    End Sub

    Private Sub AplicarEstiloCampoLectura(ByVal caja As TextBox)
        OperativeUiHelper.ApplyReadOnlyDisplayField(caja,
                                                    Color.FromArgb(246, 249, 252),
                                                    Color.FromArgb(17, 33, 59))
    End Sub

    Private Sub EnsureSidebarDecorators()
        If _panelSidebarDividerRegistro Is Nothing Then
            _panelSidebarDividerRegistro = New Panel()
            _panelSidebarDividerRegistro.BackColor = Color.FromArgb(44, 69, 102)
            BunifuGradientPanel1.Controls.Add(_panelSidebarDividerRegistro)
            _panelSidebarDividerRegistro.BringToFront()
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
        AplicarRadioControl(PanelMainStatus, 22)
        AplicarRadioControl(_lblEstadoChip, 14)
        AplicarRadioControl(_btnIncidencia, 12)
        AplicarRadioControl(_lstHistorial, 12)
        AplicarRadioControl(LblScanHint, 12)
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
            PanelMainStatus.BackColor = Color.Black
            PanelMainStatus.BorderStyle = BorderStyle.Fixed3D
            lblProcesando.ForeColor = Color.White
            LblTitulo.ForeColor = Color.White
            LblTitulo.BackColor = Color.FromArgb(32, 32, 32)
            LblScanHint.ForeColor = Color.White
            LblScanHint.BackColor = Color.FromArgb(26, 26, 26)
            TxtCedula.BackColor = Color.Black
            TxtCedula.ForeColor = Color.White
            TxtCedula.BorderStyle = BorderStyle.Fixed3D
            If _lblFocusEscaneo IsNot Nothing Then
                _lblFocusEscaneo.BackColor = Color.FromArgb(20, 20, 20)
            End If
            If _lblResultadoOperacion IsNot Nothing Then
                _lblResultadoOperacion.ForeColor = Color.White
            End If
            If _lblEstadoChip IsNot Nothing Then
                _lblEstadoChip.BackColor = Color.Gold
                _lblEstadoChip.ForeColor = Color.Black
            End If
            If _lstHistorial IsNot Nothing Then
                _lstHistorial.BackColor = Color.Black
                _lstHistorial.ForeColor = Color.White
            End If
            If _btnIncidencia IsNot Nothing Then
                _btnIncidencia.BackColor = Color.Gold
                _btnIncidencia.ForeColor = Color.Black
            End If
        Else
            PanelResult.BackColor = Color.FromArgb(242, 246, 252)
            PanelMainStatus.BorderStyle = BorderStyle.None
            TxtCedula.BorderStyle = BorderStyle.FixedSingle
            TxtCedula.BackColor = Color.White
            TxtCedula.ForeColor = Color.FromArgb(17, 33, 59)
            LblTitulo.BackColor = Color.FromArgb(229, 236, 246)
            LblScanHint.BackColor = Color.FromArgb(18, 42, 74)
            If _lblFocusEscaneo IsNot Nothing Then
                _lblFocusEscaneo.BackColor = Color.FromArgb(15, 36, 63)
            End If
            If _lstHistorial IsNot Nothing Then
                _lstHistorial.BackColor = Color.FromArgb(247, 250, 252)
                _lstHistorial.ForeColor = Color.FromArgb(31, 41, 55)
            End If
            If _btnIncidencia IsNot Nothing Then
                _btnIncidencia.BackColor = Color.FromArgb(194, 120, 39)
                _btnIncidencia.ForeColor = Color.White
            End If
        End If
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

    Private Sub AplicarCamposLecturaNoFocusables()
        OperativeUiHelper.DisableFocus(LblCedula, TxtUsuario, TxtTipo, TxtTiquetes, BtnSalir)
    End Sub

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

    Private Sub RegistrarActividad()
        _ultimaActividad = ServerClock.Now()
        _limpiezaAplicadaPorInactividad = False
    End Sub

    Private Sub TimerInactividad_Tick(ByVal sender As Object, ByVal e As EventArgs)
        IntentarReconexionSiCorresponde()
        SincronizarOperacionSiCambioDia()
        LblFecha.Text = ServerClock.Now().ToString("yyyy/MM/dd HH:mm:ss")
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

        LimpiarUltimoRegistroPorInactividad()
        _limpiezaAplicadaPorInactividad = True
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
            SincronizarReglasOperacion()
            RecargarDatosOperacion()
        Catch ex As Exception
            ErrorLogger.LogException("ControlComedor.IntentarReconexionSiCorresponde", ex)
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
            ErrorLogger.LogException("ControlComedor.SincronizarHoraServidorActual", ex)
        End Try
    End Sub

    Private Sub SincronizarOperacionSiCambioDia()
        If _fechaAsistenciaComedorCache = Date.MinValue Then
            Exit Sub
        End If

        If ServerClock.Today() <= _fechaAsistenciaComedorCache.Date Then
            _ultimoIntentoSincronizacionDia = DateTime.MinValue
            Exit Sub
        End If

        If _ultimoIntentoSincronizacionDia <> DateTime.MinValue AndAlso ServerClock.Now().Subtract(_ultimoIntentoSincronizacionDia).TotalMinutes < 5 Then
            Exit Sub
        End If
        _ultimoIntentoSincronizacionDia = ServerClock.Now()

        If Cn Is Nothing Then
            Exit Sub
        End If

        If Cn.State <> ConnectionState.Open Then
            IntentarReconexionSiCorresponde()
            If Cn.State <> ConnectionState.Open Then
                Exit Sub
            End If
        End If

        Try
            Dim dsFecha As DataSet = Cls.ConsultarTSQL("Fecha", "SELECT GETDATE() AS Fecha;", Cn:=Cn)
            If dsFecha Is Nothing OrElse dsFecha.Tables.Count = 0 OrElse dsFecha.Tables(0).Rows.Count = 0 Then
                Exit Sub
            End If

            Dim nuevaFechaServidor As Date = CDate(dsFecha.Tables(0).Rows(0)("Fecha"))
            Dim nuevaFecha As Date = nuevaFechaServidor.Date
            If nuevaFecha = _fechaAsistenciaComedorCache.Date Then
                Return
            End If

            ServerClock.Sync(nuevaFechaServidor)
            DiaSemana = Weekday(nuevaFecha).ToString()
            SincronizarReglasOperacion()
            RecargarDatosOperacion()
            UltimoCarnetProcesado = String.Empty
            _ultimoIntentoSincronizacionDia = DateTime.MinValue
            ErrorLogger.LogInfo("ControlComedor.SincronizarOperacionSiCambioDia", "Cache diario de comedor reiniciado. Fecha=" & nuevaFecha.ToString("yyyy-MM-dd"))
        Catch ex As Exception
            ErrorLogger.LogException("ControlComedor.SincronizarOperacionSiCambioDia", ex)
        End Try
    End Sub

    Private Sub SincronizarReglasOperacion()
        Try
            Dim cfg As ParametroSistemaService.ParametroSistemaConfig = ParametroSvc.ObtenerFila1(Cn)

            PermitirSinMarcaTransporte = False
            _permitirMarcaTardia = False
            _apagaAdvertenciaTransporte = False

            If cfg IsNot Nothing Then
                PermitirSinMarcaTransporte = cfg.PermitirSinMarcaTransporte
                _permitirMarcaTardia = cfg.PermitirMarcaTardia
                _apagaAdvertenciaTransporte = cfg.ApagaAdvertenciaTransporte
            End If

            ErrorLogger.LogInfo("ControlComedor.SincronizarReglasOperacion", "PermitirSinMarcaTransporte=" & PermitirSinMarcaTransporte.ToString() & ", PermitirMarcaTardia=" & _permitirMarcaTardia.ToString() & ", ApagaAdvertenciaTransporte=" & _apagaAdvertenciaTransporte.ToString())
        Catch ex As Exception
            PermitirSinMarcaTransporte = False
            _permitirMarcaTardia = False
            _apagaAdvertenciaTransporte = False
            ErrorLogger.LogException("ControlComedor.SincronizarReglasOperacion", ex, "Fallback=false")
        End Try
    End Sub

    Private Sub TimerEstadoVisual_Tick(ByVal sender As Object, ByVal e As EventArgs)
        ActualizarIndicadoresEstadoInfo()
    End Sub

    Private Sub LimpiarUltimoRegistroPorInactividad()
        UltimoCarnetProcesado = String.Empty
        ErrorLecturaDuplicada = False
        ErrorTiquetes = False
        EstadoVerificado = False
        TxtCedula.Clear()
        ResetResultFields()
        UpdateVisualState(EstadoVisual.Idle)
        EnsureScanFocus(True)
    End Sub

    Private Function ObtenerLogoAplicacion() As Image
        If _logoAplicacion IsNot Nothing Then
            Return _logoAplicacion
        End If

        Dim ruta As String = ResolverRutaRecursoMarca("LogoHorizontal.png")
        If Global.System.IO.File.Exists(ruta) Then
            Try
                Using fs As New Global.System.IO.FileStream(ruta, Global.System.IO.FileMode.Open, Global.System.IO.FileAccess.Read, Global.System.IO.FileShare.ReadWrite)
                    Using source As Image = Image.FromStream(fs)
                        _logoAplicacion = New Bitmap(source)
                        Return _logoAplicacion
                    End Using
                End Using
            Catch ex As Exception
                ErrorLogger.LogException("ControlComedor.CargarLogoOperacion", ex)
            End Try
        End If

        _logoAplicacion = My.Resources.Login
        Return _logoAplicacion
    End Function

    Private Function ResolverRutaRecursoMarca(ByVal nombreArchivo As String) As String
        Return ResolveResourcePath(nombreArchivo)
    End Function

    Private Sub CargarPreferenciasOperacion()
        _sonidosHabilitados = LeerConfigBool("SonidosHabilitados", True)
        _modoAccesible = LeerConfigBool("ModoAccesible", False)
        _mostrarVistaSupervisor = LeerConfigBool("MostrarVistaSupervisor", False)
        _umbralDuplicadosPct = LeerConfigDouble("UmbralDuplicadosPct", 15.0R)
        _umbralErrores = Math.Max(1, LeerConfigInt("UmbralErrores", 5))
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
        _metaDiaria = 0
    End Sub

    Private Sub InicializarControlesOperacion()
        If _lblMeta IsNot Nothing Then
            Exit Sub
        End If

        _lblMeta = New Label()
        _lblMeta.AutoSize = False
        _lblMeta.ForeColor = Color.FromArgb(220, 232, 252)
        _lblMeta.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)

        _progressMeta = New ProgressBar()
        _progressMeta.Minimum = 0
        _progressMeta.Maximum = Math.Max(1, _metaDiaria)
        _progressMeta.Style = ProgressBarStyle.Continuous

        _lblConexion = New Label()
        _lblConexion.AutoSize = False
        _lblConexion.ForeColor = Color.FromArgb(220, 232, 252)
        _lblConexion.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
        _lblConexion.Text = "Conexion DB: verificando..."

        _lblKpi = New Label()
        _lblKpi.AutoSize = False
        _lblKpi.ForeColor = Color.FromArgb(220, 232, 252)
        _lblKpi.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)

        _lblCola = New Label()
        _lblCola.AutoSize = False
        _lblCola.ForeColor = Color.FromArgb(220, 232, 252)
        _lblCola.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
        _lblCola.Text = "Antifila: calculando..."

        _lblAlertas = New Label()
        _lblAlertas.AutoSize = False
        _lblAlertas.ForeColor = Color.FromArgb(255, 215, 155)
        _lblAlertas.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
        _lblAlertas.Text = "Alertas: sin incidencias"

        _lblRecomendacion = New Label()
        _lblRecomendacion.AutoSize = False
        _lblRecomendacion.ForeColor = Color.FromArgb(220, 232, 252)
        _lblRecomendacion.Font = New Font("Segoe UI", 8.5!, FontStyle.Italic)
        _lblRecomendacion.Text = "Recomendacion: mantener flujo de escaneo continuo."

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
        _lblEstadoChip.Text = "EN ESPERA"

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

        _lblFocusEscaneo = New Label()
        _lblFocusEscaneo.AutoSize = False
        _lblFocusEscaneo.TextAlign = ContentAlignment.MiddleLeft
        _lblFocusEscaneo.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
        _lblFocusEscaneo.ForeColor = Color.FromArgb(157, 230, 170)
        _lblFocusEscaneo.BackColor = Color.FromArgb(15, 36, 63)
        _lblFocusEscaneo.Padding = New Padding(12, 0, 12, 0)
        _lblFocusEscaneo.Text = "Lector listo (captura oculta)"

        _lblHotkeys = New Label()
        _lblHotkeys.AutoSize = False
        _lblHotkeys.TextAlign = ContentAlignment.MiddleLeft
        _lblHotkeys.Font = New Font("Segoe UI", 8.5!, FontStyle.Bold)
        _lblHotkeys.ForeColor = Color.FromArgb(194, 214, 243)
        _lblHotkeys.Text = ObtenerTextoHotkeys()

        _btnIncidencia = New Button()
        _btnIncidencia.Text = "Incidencia rápida"
        _btnIncidencia.FlatStyle = FlatStyle.Flat
        _btnIncidencia.FlatAppearance.BorderSize = 0
        _btnIncidencia.BackColor = Color.FromArgb(194, 120, 39)
        _btnIncidencia.ForeColor = Color.White
        _btnIncidencia.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
        AddHandler _btnIncidencia.Click, AddressOf BtnIncidencia_Click

        BunifuGradientPanel1.Controls.Add(_lblMeta)
        BunifuGradientPanel1.Controls.Add(_progressMeta)
        BunifuGradientPanel1.Controls.Add(_lblConexion)
        BunifuGradientPanel1.Controls.Add(_lblKpi)
        BunifuGradientPanel1.Controls.Add(_lblCola)
        BunifuGradientPanel1.Controls.Add(_lblAlertas)
        BunifuGradientPanel1.Controls.Add(_lblRecomendacion)
        BunifuGradientPanel1.Controls.Add(_lblUltimaLectura)
        BunifuGradientPanel1.Controls.Add(_lblEdadEstado)
        BunifuGradientPanel1.Controls.Add(_lblFocusEscaneo)
        BunifuGradientPanel1.Controls.Add(_lblHotkeys)
        PanelMainStatus.Controls.Add(_lblResultadoOperacion)
        PanelMainStatus.Controls.Add(_lblEstadoChip)
        GbDatos.Controls.Add(_lblHistorial)
        GbDatos.Controls.Add(_lstHistorial)
        _lblMeta.BringToFront()
        _progressMeta.BringToFront()
        _lblConexion.BringToFront()
        _lblKpi.BringToFront()
        _lblCola.BringToFront()
        _lblAlertas.BringToFront()
        _lblRecomendacion.BringToFront()
        _lblUltimaLectura.BringToFront()
        _lblEdadEstado.BringToFront()
        _lblResultadoOperacion.BringToFront()
        _lblEstadoChip.BringToFront()
        _lblHistorial.BringToFront()
        _lstHistorial.BringToFront()
        PanelResult.Controls.Add(_btnIncidencia)
        _btnIncidencia.BringToFront()
    End Sub

    Private Sub Historial_DrawItem(ByVal sender As Object, ByVal e As DrawItemEventArgs)
        If e.Index < 0 Then
            Exit Sub
        End If

        Dim item As String = CStr(_lstHistorial.Items(e.Index))
        Dim hora As String = String.Empty
        Dim estadoRaw As String = String.Empty
        Dim detalle As String = String.Empty
        DescomponerItemHistorial(item, hora, estadoRaw, detalle)

        Dim colorEstado As Color = ObtenerColorEstadoHistorial(estadoRaw)
        Dim estadoBadge As String = ObtenerBadgeEstadoHistorial(estadoRaw)
        Dim fondoHistorial As Color = If(_modoAltoContraste, Color.Black, _lstHistorial.BackColor)
        Dim filaFondo As Color
        Dim colorHora As Color
        Dim colorDetalle As Color

        If _modoAltoContraste Then
            filaFondo = If(e.Index Mod 2 = 0, Color.FromArgb(18, 18, 18), Color.FromArgb(28, 28, 28))
            colorHora = Color.FromArgb(255, 230, 154)
            colorDetalle = Color.White
        Else
            filaFondo = If(e.Index Mod 2 = 0, Color.FromArgb(255, 255, 255), Color.FromArgb(249, 250, 251))
            colorHora = Color.FromArgb(100, 116, 139)
            colorDetalle = Color.FromArgb(30, 41, 59)
        End If

        If (e.State And DrawItemState.Selected) = DrawItemState.Selected Then
            filaFondo = If(_modoAltoContraste, Color.FromArgb(42, 42, 42), Color.FromArgb(230, 238, 248))
        End If

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Using fondo As New SolidBrush(fondoHistorial)
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
            TextRenderer.DrawText(e.Graphics, hora, horaFont, horaRect, colorHora, TextFormatFlags.VerticalCenter Or TextFormatFlags.Left)

            Using badgePath As GraphicsPath = CrearRutaRedondeada(badgeRect, 9),
                  badgeBrush As New SolidBrush(colorEstado)
                e.Graphics.FillPath(badgeBrush, badgePath)
            End Using
            Dim colorBadgeTexto As Color = If(colorEstado.GetBrightness() > 0.58F, Color.Black, Color.White)
            TextRenderer.DrawText(e.Graphics, estadoBadge, badgeFont, badgeRect, colorBadgeTexto, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)

            If String.IsNullOrWhiteSpace(detalle) Then
                detalle = estadoRaw
            End If
            TextRenderer.DrawText(e.Graphics, detalle, detalleFont, detalleRect, colorDetalle, TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
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
        If valor.Contains("SIN_MARCA") OrElse valor.Contains("MARCA_TARDIA") OrElse valor.Contains("NOTRANSPORT") OrElse valor.Contains("LATETRANSPORT") Then
            Return "ADV"
        End If
        If valor.Contains("SIN_TIQUETES") OrElse valor.Contains("NOTICKETS") OrElse valor.Contains("DENEGADO") OrElse valor.Contains("DENIED") OrElse valor.Contains("NOTFOUND") OrElse valor.Contains("NO_ENCONTRADO") Then
            Return "ERR"
        End If
        If valor.Contains("EXITO") OrElse valor.Contains("SUCCESS") Then
            Return "OK"
        End If
        If valor.Contains("PROCES") Then
            Return "PROC"
        End If
        Return "INFO"
    End Function

    Private Function ObtenerColorEstadoHistorial(ByVal estado As String) As Color
        Dim valor As String = estado.Trim().ToUpperInvariant()
        If valor.Contains("DUPLIC") Then
            Return _colorDuplicado
        End If
        If valor.Contains("SIN_MARCA") OrElse valor.Contains("MARCA_TARDIA") OrElse valor.Contains("NOTRANSPORT") OrElse valor.Contains("LATETRANSPORT") Then
            Return Color.FromArgb(180, 83, 9)
        End If
        If valor.Contains("SIN_TIQUETES") OrElse valor.Contains("NOTICKETS") OrElse valor.Contains("DENEGADO") OrElse valor.Contains("DENIED") OrElse valor.Contains("NOTFOUND") OrElse valor.Contains("NO_ENCONTRADO") Then
            Return _colorError
        End If
        If valor.Contains("EXITO") OrElse valor.Contains("SUCCESS") Then
            Return _colorExito
        End If
        If valor.Contains("PROCES") Then
            Return Color.FromArgb(37, 99, 235)
        End If
        Return Color.FromArgb(71, 85, 105)
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
        If _lblMeta Is Nothing Then
            Exit Sub
        End If

        Dim avancePct As Double = (_asistenciasValidasDia / CDbl(Math.Max(1, _metaDiaria))) * 100.0R
        _lblMeta.Text = "Meta diaria: " & _asistenciasValidasDia.ToString("N0") & "/" & _metaDiaria.ToString("N0") & " (" & avancePct.ToString("0") & "%)"
        _progressMeta.Maximum = Math.Max(1, _metaDiaria)
        _progressMeta.Value = Math.Min(_progressMeta.Maximum, Math.Max(0, _asistenciasValidasDia))

        Dim ratioDup As Double = 0
        If _totalLecturas > 0 Then
            ratioDup = (_totalDuplicadas / CDbl(_totalLecturas)) * 100.0R
        End If
        _lblKpi.Text = String.Format("Lecturas: {0} | Duplicadas: {1} ({2:0}%) | Errores: {3}", _totalLecturas, _totalDuplicadas, ratioDup, _totalErrores)
        _lblKpi.Visible = _mostrarVistaSupervisor
        _lblCola.Visible = _mostrarVistaSupervisor
        _lblAlertas.Visible = _mostrarVistaSupervisor
        _lblRecomendacion.Visible = _mostrarVistaSupervisor

        Dim promedioSegundos As Double = ObtenerPromedioAtencionSegundos()
        If promedioSegundos > 0 Then
            Dim capacidadHora As Integer = CInt(Math.Round(3600.0R / promedioSegundos))
            _lblCola.Text = String.Format("Antifila: promedio {0:0.0}s | capacidad {1}/hora", promedioSegundos, capacidadHora)
        Else
            _lblCola.Text = "Antifila: calculando..."
        End If

        _lblAlertas.Text = GenerarTextoAlertas(ratioDup)
        _lblRecomendacion.Text = GenerarRecomendacionOperativa(ratioDup, promedioSegundos)
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

    Private Function ObtenerPromedioAtencionSegundos() As Double
        If _muestrasTiempoAtencion <= 0 Then
            Return 0
        End If
        Return (_acumuladoTiempoAtencionMs / CDbl(_muestrasTiempoAtencion)) / 1000.0R
    End Function

    Private Function GenerarTextoAlertas(ByVal ratioDup As Double) As String
        If _totalErrores >= _umbralErrores Then
            Return "Alertas: alto nivel de errores, revisar lector/carnets."
        End If
        If ratioDup >= _umbralDuplicadosPct Then
            Return "Alertas: duplicados elevados en el turno."
        End If
        Return "Alertas: operacion estable."
    End Function

    Private Function GenerarRecomendacionOperativa(ByVal ratioDup As Double, ByVal promedioSegundos As Double) As String
        If promedioSegundos > 6 Then
            Return "Recomendacion: habilitar apoyo en fila y validar enfoque del lector."
        End If
        If ratioDup >= _umbralDuplicadosPct Then
            Return "Recomendacion: recordar al usuario retirar carnet tras lectura."
        End If
        If _totalErrores >= 3 Then
            Return "Recomendacion: revisar tiquetes pendientes antes de hora pico."
        End If
        Return "Recomendacion: mantener flujo continuo de escaneo."
    End Function

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
            For Each row As DataRow In dt.Rows
                Dim fechaStr As String = CDate(row("FechaEvento")).ToString("HH:mm:ss")
                Dim estado As String = CStr(row("Estado"))
                Dim cedula As String = CStr(row("Cedula"))
                Dim motivo As String = CStr(row("Motivo"))
                Dim detalle As String = If(String.IsNullOrWhiteSpace(motivo), cedula, motivo)
                _lstHistorial.Items.Add(fechaStr & " | " & estado & " | " & detalle)
            Next
        Catch ex As Exception
            ErrorLogger.LogException("ControlComedor.CargarHistorialInicial", ex)
        End Try
    End Sub

    Private Sub RegistrarEventoPersistente(ByVal state As EstadoVisual,
                                           ByVal detalle As String,
                                           Optional ByVal esIncidenciaManual As Boolean = False,
                                           Optional ByVal codigoIncidencia As String = "")
        Try
            If Cn.State <> ConnectionState.Open Then
                Exit Sub
            End If

            Dim tiempoMs As Integer? = Nothing
            If _inicioLectura <> DateTime.MinValue Then
                Dim diff As Integer = CInt(Math.Max(0, ServerClock.Now().Subtract(_inicioLectura).TotalMilliseconds))
                tiempoMs = diff
            End If

            Dim esAdvertencia As Boolean = (state = EstadoVisual.Duplicate OrElse state = EstadoVisual.NoTransportMark OrElse state = EstadoVisual.LateTransportMark)
            Dim esError As Boolean = (state = EstadoVisual.NotFound OrElse state = EstadoVisual.NoTickets OrElse state = EstadoVisual.DeniedByRule)
            Dim estadoEvento As String = ObtenerCodigoEstado(state)
            Dim motivoEvento As String = ObtenerCodigoEvento(state, detalle, esIncidenciaManual, codigoIncidencia)
            Dim cedulaEvento As String = ObtenerCedulaValidaParaEvento()
            OperacionSvc.RegistrarEvento(Cn,
                                         ServerClock.Now(),
                                         cedulaEvento,
                                         estadoEvento,
                                         motivoEvento,
                                         tiempoMs,
                                         state = EstadoVisual.Duplicate,
                                         esAdvertencia,
                                         esError,
                                         esIncidenciaManual)
        Catch ex As Exception
            ErrorLogger.LogException("ControlComedor.RegistrarEventoPersistente", ex)
        End Try
    End Sub

    Private Function ObtenerCodigoEstado(ByVal state As EstadoVisual) As String
        Select Case state
            Case EstadoVisual.Success
                Return "EXITO"
            Case EstadoVisual.Processing
                Return "PROCESANDO"
            Case EstadoVisual.Duplicate
                Return "DUPLICADO"
            Case EstadoVisual.NoTickets
                Return "SIN_TIQUETES"
            Case EstadoVisual.NoTransportMark
                Return "SIN_MARCA_TRANSPORTE"
            Case EstadoVisual.LateTransportMark
                Return "MARCA_TARDIA_TRANSPORTE"
            Case EstadoVisual.NotFound
                Return "CARNET_NO_ENCONTRADO"
            Case EstadoVisual.DeniedByRule
                Return "DENEGADO_POR_REGLA"
            Case Else
                Return "EN_ESPERA"
        End Select
    End Function

    Private Sub BtnIncidencia_Click(ByVal sender As Object, ByVal e As EventArgs)
        RegistrarActividad()

        Dim sugerencia As String = "SIN_TIQUETES"
        If _totalErrores >= _umbralErrores Then
            sugerencia = "ERROR_LECTOR"
        End If

        Dim motivo As String = OperativeDialogHelper.SolicitarCodigoIncidenciaRapida(Me, sugerencia)

        If String.IsNullOrWhiteSpace(motivo) Then
            EnsureScanFocus(True)
            Exit Sub
        End If

        Dim codigo As String = NormalizarCodigoIncidencia(motivo)
        RegistrarHistorial(EstadoVisual.DeniedByRule, "INCIDENCIA: " & codigo)
        RegistrarEventoPersistente(EstadoVisual.DeniedByRule, "INCIDENCIA: " & codigo, True, codigo)
        LblRegistroError.Text = "Incidencia registrada: " & codigo
        EnsureScanFocus(True)
    End Sub

    Private Function ObtenerCedulaValidaParaEvento() As String
        Dim raw As String = TxtCedula.Text.Trim()
        If String.IsNullOrWhiteSpace(raw) Then
            Return String.Empty
        End If
        If Not Cls.VereficaCarnet(raw) Then
            Return String.Empty
        End If
        Return raw
    End Function

    Private Function ObtenerCodigoEvento(ByVal state As EstadoVisual,
                                         ByVal detalle As String,
                                         ByVal esIncidenciaManual As Boolean,
                                         ByVal codigoIncidencia As String) As String
        If esIncidenciaManual Then
            If Not String.IsNullOrWhiteSpace(codigoIncidencia) Then
                Return NormalizarCodigoIncidencia(codigoIncidencia)
            End If
            Return "INCIDENCIA_MANUAL"
        End If

        Select Case state
            Case EstadoVisual.Success
                Return "COMIDA_REGISTRADA"
            Case EstadoVisual.Processing
                Return "PROCESANDO_LECTURA"
            Case EstadoVisual.Duplicate
                Return "LECTURA_DUPLICADA"
            Case EstadoVisual.NoTickets
                Return "SIN_TIQUETES"
            Case EstadoVisual.NoTransportMark
                Return "SIN_MARCA_TRANSPORTE"
            Case EstadoVisual.LateTransportMark
                Return "MARCA_TARDIA_TRANSPORTE"
            Case EstadoVisual.NotFound
                Return "CARNET_NO_ENCONTRADO"
            Case EstadoVisual.DeniedByRule
                Return "DENEGADO_POR_REGLA"
            Case Else
                Return "EN_ESPERA"
        End Select
    End Function

    Private Function NormalizarCodigoIncidencia(ByVal input As String) As String
        Dim v As String = If(input, String.Empty).Trim().ToUpperInvariant()
        If String.IsNullOrWhiteSpace(v) Then
            Return "OTRO"
        End If

        Select Case v
            Case "SINTIQUETES", "SIN_TIQUETES", "NO_TICKETS"
                Return "SIN_TIQUETES"
            Case "LECTOR", "SCANNER", "SCANNER_ISSUE", "ERROR_LECTOR"
                Return "ERROR_LECTOR"
            Case "CARNETDANADO", "CARNET_DANADO", "DAMAGED_CARD"
                Return "CARNET_DANADO"
            Case "MARCATARDIA", "MARCA_TARDIA", "TRANSPORT_LATE", "TRANSPORT_LATE_MARK", "MARCA_TARDIA_TRANSPORTE"
                Return "MARCA_TARDIA_TRANSPORTE"
            Case "OTHER", "OTRO"
                Return "OTRO"
            Case Else
                Return v.Replace(" ", "_")
        End Select
    End Function

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

    Private Sub ReproducirSonido(ByVal state As EstadoVisual)
        If Not _sonidosHabilitados Then
            Exit Sub
        End If

        Dim ruta As String = String.Empty
        Dim repeticiones As Integer = 1
        Dim sonidoSistema As SystemSound = Nothing

        If Not ObtenerConfiguracionSonido(state, ruta, repeticiones, sonidoSistema) Then
            Exit Sub
        End If

        DetenerSonidoActivo()
        Dim cts As New CancellationTokenSource()
        SyncLock _sonidoSync
            _sonidoCancellation = cts
        End SyncLock

        Task.Run(Sub()
                     ReproducirSonidoCore(ruta, sonidoSistema, repeticiones, cts.Token)
                 End Sub)
    End Sub

    Private Function ObtenerConfiguracionSonido(
        ByVal state As EstadoVisual,
        ByRef ruta As String,
        ByRef repeticiones As Integer,
        ByRef sonidoSistema As SystemSound
    ) As Boolean
        ruta = String.Empty
        repeticiones = 1
        sonidoSistema = Nothing

        Select Case state
            Case EstadoVisual.Success
                ruta = _sonidoOkRuta
                repeticiones = _repeticionesSonidoOk
                sonidoSistema = SystemSounds.Asterisk
                Return True
            Case EstadoVisual.Duplicate, EstadoVisual.NoTransportMark, EstadoVisual.LateTransportMark
                ruta = _sonidoWarnRuta
                repeticiones = _repeticionesSonidoWarn
                sonidoSistema = SystemSounds.Exclamation
                Return True
            Case EstadoVisual.NoTickets, EstadoVisual.NotFound, EstadoVisual.DeniedByRule
                ruta = _sonidoErrorRuta
                repeticiones = _repeticionesSonidoError
                sonidoSistema = SystemSounds.Hand
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Sub ReproducirSonidoCore(
        ByVal ruta As String,
        ByVal sonidoSistema As SystemSound,
        ByVal repeticiones As Integer,
        ByVal token As CancellationToken
    )
        Dim resolved As String = String.Empty
        Dim usarWav As Boolean = False

        If Not _forzarSonidoSistemaFallback AndAlso Not String.IsNullOrWhiteSpace(ruta) Then
            Try
                resolved = ResolverRutaRecursoMarca(ruta)
                usarWav = Global.System.IO.File.Exists(resolved)
                If Not usarWav Then
                    ErrorLogger.LogInfo("ControlComedor.ReproducirSonido", "WAV no encontrado, fallback sistema: " & resolved)
                End If
            Catch ex As Exception
                ErrorLogger.LogException("ControlComedor.ReproducirSonido", ex, "Fallo resolviendo WAV")
                usarWav = False
            End Try
        End If

        Dim i As Integer
        For i = 1 To Math.Max(1, repeticiones)
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
                ErrorLogger.LogException("ControlComedor.ReproducirSonido", ex, "Fallo reproduciendo sonido")
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

    Private Function LeerConfigBool(ByVal key As String, ByVal valorDefault As Boolean) As Boolean
        Return GetAppSettingBoolean(key, valorDefault)
    End Function

    Private Function LeerConfigInt(ByVal key As String, ByVal valorDefault As Integer) As Integer
        Return GetAppSettingInteger(key, valorDefault)
    End Function

    Private Function LeerConfigDouble(ByVal key As String, ByVal valorDefault As Double) As Double
        Return GetAppSettingDouble(key, valorDefault)
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
            ErrorLogger.LogException("ControlComedor.LeerConfigColor", ex)
        End Try
        Return valorDefault
    End Function

    Private Sub UpdateVisualState(ByVal state As EstadoVisual)
        _estadoVisualActual = state

        Select Case state
            Case EstadoVisual.Idle
                lblProcesando.Text = "Esperando lectura de carnet"
                lblProcesando.ForeColor = Color.FromArgb(23, 32, 51)
                LblTitulo.ForeColor = Color.FromArgb(17, 33, 59)
                Imgprocess.Image = My.Resources.Info
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelMainStatus.BackColor = Color.White
                PanelResult.BackColor = _colorNeutro
                LblRegistroError.Text = String.Empty
                _resultadoOperacionActual = "SIN LECTURA"
                _fechaUltimoEstado = DateTime.MinValue
            Case EstadoVisual.Processing
                lblProcesando.Text = "Procesando lectura..."
                lblProcesando.ForeColor = Color.FromArgb(17, 33, 59)
                LblTitulo.ForeColor = Color.FromArgb(17, 33, 59)
                Imgprocess.Image = My.Resources.Gif_cargando
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelMainStatus.BackColor = _colorProcesando
                PanelResult.BackColor = _colorProcesando
                LblRegistroError.Text = String.Empty
                _resultadoOperacionActual = "PROCESANDO..."
            Case EstadoVisual.Success
                lblProcesando.Text = "Entrada registrada correctamente (confirmado)"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                Imgprocess.Image = My.Resources.Verificado2
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelMainStatus.BackColor = _colorExito
                PanelResult.BackColor = _colorExito
                LblRegistroError.Text = String.Empty
                _resultadoOperacionActual = "ACCESO PERMITIDO"
                _fechaUltimoEstado = ServerClock.Now()
            Case EstadoVisual.NotFound
                lblProcesando.Text = "Carnet no valido o no encontrado"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                Imgprocess.Image = My.Resources.Error2
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelMainStatus.BackColor = _colorError
                PanelResult.BackColor = _colorError
                ResetResultFields()
                LblRegistroError.Text = "Accion: valide el carnet y vuelva a escanear."
                _resultadoOperacionActual = "CARNET NO VALIDO"
                _fechaUltimoEstado = ServerClock.Now()
            Case EstadoVisual.Duplicate
                lblProcesando.Text = If(String.IsNullOrWhiteSpace(_mensajeDuplicadoPersonalizado), "Lectura duplicada detectada", _mensajeDuplicadoPersonalizado)
                lblProcesando.ForeColor = Color.FromArgb(31, 41, 55)
                LblTitulo.ForeColor = Color.FromArgb(31, 41, 55)
                Imgprocess.Image = My.Resources.Double_check
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelMainStatus.BackColor = _colorDuplicado
                PanelResult.BackColor = _colorDuplicado
                LblRegistroError.Text = If(String.IsNullOrWhiteSpace(_detalleDuplicadoPersonalizado), "Accion: espere 1 minuto o escanee otro carnet.", _detalleDuplicadoPersonalizado)
                _resultadoOperacionActual = "PERMITIDO CON ADVERTENCIA"
                _fechaUltimoEstado = ServerClock.Now()
            Case EstadoVisual.NoTickets
                lblProcesando.Text = "Sin tiquetes disponibles"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                Imgprocess.Image = My.Resources.Error2
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelMainStatus.BackColor = _colorError
                PanelResult.BackColor = _colorError
                LblRegistroError.Text = "Accion: recargue tiquetes antes de permitir acceso."
                _resultadoOperacionActual = "ACCESO DENEGADO"
                _fechaUltimoEstado = ServerClock.Now()
            Case EstadoVisual.NoTransportMark
                lblProcesando.Text = "Sin marca de transporte: acceso permitido con advertencia"
                lblProcesando.ForeColor = Color.FromArgb(31, 41, 55)
                LblTitulo.ForeColor = Color.FromArgb(31, 41, 55)
                Imgprocess.Image = My.Resources.Info
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelMainStatus.BackColor = _colorAdvertencia
                PanelResult.BackColor = _colorAdvertencia
                LblRegistroError.Text = "Aviso: registrar incidencia si aplica."
                _resultadoOperacionActual = "PERMITIDO CON ADVERTENCIA"
                _fechaUltimoEstado = ServerClock.Now()
            Case EstadoVisual.LateTransportMark
                lblProcesando.Text = "Marca tardía en transporte: acceso permitido con advertencia"
                lblProcesando.ForeColor = Color.FromArgb(31, 41, 55)
                LblTitulo.ForeColor = Color.FromArgb(31, 41, 55)
                Imgprocess.Image = My.Resources.Info
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelMainStatus.BackColor = _colorAdvertencia
                PanelResult.BackColor = _colorAdvertencia
                LblRegistroError.Text = "Aviso: marca fuera de horario, continuidad permitida."
                _resultadoOperacionActual = "PERMITIDO CON ADVERTENCIA"
                _fechaUltimoEstado = ServerClock.Now()
            Case EstadoVisual.DeniedByRule
                lblProcesando.Text = "Lectura denegada por política operativa"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                Imgprocess.Image = My.Resources.Error2
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelMainStatus.BackColor = _colorError
                PanelResult.BackColor = _colorError
                LblRegistroError.Text = "Accion: revise politica operativa vigente."
                _resultadoOperacionActual = "ACCESO DENEGADO"
                _fechaUltimoEstado = ServerClock.Now()
        End Select

        If _lblResultadoOperacion IsNot Nothing Then
            _lblResultadoOperacion.Text = _resultadoOperacionActual
            If state = EstadoVisual.Success OrElse state = EstadoVisual.NotFound OrElse state = EstadoVisual.NoTickets OrElse state = EstadoVisual.DeniedByRule Then
                _lblResultadoOperacion.ForeColor = Color.White
            Else
                _lblResultadoOperacion.ForeColor = Color.FromArgb(31, 41, 55)
            End If
        End If
        ActualizarChipEstado(state)
        ActualizarTarjetasEstado(state)
        ActualizarColorDetalleRegistro(state)
        ActualizarIndicadoresEstadoInfo()

        If state <> EstadoVisual.Idle AndAlso state <> EstadoVisual.Processing Then
            ReproducirSonido(state)
            RegistrarHistorial(state, TxtCedula.Text.Trim())
            RegistrarEventoPersistente(state, LblRegistroError.Text)
        End If
    End Sub

    Private Sub ActualizarTarjetasEstado(ByVal state As EstadoVisual)
        If _modoAltoContraste Then
            LblTitulo.BackColor = Color.FromArgb(32, 32, 32)
            Return
        End If

        Select Case state
            Case EstadoVisual.Success
                LblTitulo.BackColor = Color.FromArgb(24, 95, 64)
            Case EstadoVisual.Processing
                LblTitulo.BackColor = Color.FromArgb(219, 234, 254)
            Case EstadoVisual.Duplicate
                LblTitulo.BackColor = Color.FromArgb(245, 158, 11)
            Case EstadoVisual.NoTransportMark, EstadoVisual.LateTransportMark
                LblTitulo.BackColor = Color.FromArgb(250, 204, 21)
            Case EstadoVisual.NoTickets, EstadoVisual.NotFound, EstadoVisual.DeniedByRule
                LblTitulo.BackColor = Color.FromArgb(153, 27, 27)
            Case Else
                LblTitulo.BackColor = Color.FromArgb(229, 236, 246)
        End Select
    End Sub

    Private Sub ActualizarColorDetalleRegistro(ByVal state As EstadoVisual)
        Select Case state
            Case EstadoVisual.Duplicate, EstadoVisual.NoTransportMark, EstadoVisual.LateTransportMark
                LblRegistroError.ForeColor = Color.FromArgb(146, 64, 14)
            Case EstadoVisual.NoTickets, EstadoVisual.NotFound, EstadoVisual.DeniedByRule
                LblRegistroError.ForeColor = Color.FromArgb(153, 27, 27)
            Case Else
                LblRegistroError.ForeColor = Color.FromArgb(71, 85, 105)
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
            Case EstadoVisual.Duplicate
                _lblEstadoChip.Text = "ESTADO: DOBLE LECTURA"
                _lblEstadoChip.BackColor = Color.FromArgb(180, 83, 9)
                _lblEstadoChip.ForeColor = Color.White
            Case EstadoVisual.NoTransportMark, EstadoVisual.LateTransportMark
                _lblEstadoChip.Text = "ESTADO: ADVERTENCIA"
                _lblEstadoChip.BackColor = Color.FromArgb(202, 138, 4)
                _lblEstadoChip.ForeColor = Color.White
            Case EstadoVisual.NoTickets, EstadoVisual.NotFound, EstadoVisual.DeniedByRule
                _lblEstadoChip.Text = "ESTADO: ERROR"
                _lblEstadoChip.BackColor = Color.FromArgb(185, 28, 28)
                _lblEstadoChip.ForeColor = Color.White
            Case Else
                _lblEstadoChip.Text = "ESTADO: EN ESPERA"
                _lblEstadoChip.BackColor = Color.FromArgb(51, 65, 85)
                _lblEstadoChip.ForeColor = Color.White
        End Select
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape Then
            Me.Close()
            Return True
        End If
        If keyData = Keys.F2 Then
            LimpiarUltimoRegistroPorInactividad()
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
        If keyData = Keys.F6 Then
            _mostrarVistaSupervisor = Not _mostrarVistaSupervisor
            ActualizarKpisOperacion()
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

        LblScanHint.Text = ObtenerTextoScanHint()
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
        Return "Esc salir | F2 limpiar | F3 foco | Ctrl+F3 mostrar/ocultar | F4 historial | F6 supervisor | F7 contraste"
    End Function
End Class
