Imports System.Drawing
Imports System.Linq
Imports System.Configuration
Imports System.Media
Imports System.Globalization
Imports System.Threading
Imports System.Threading.Tasks

Public Class ControlComedor
    Private Const PermitirMarcaTardia As Boolean = True
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
    Private _lecturasExitosas As Integer
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
    Private _modoAltoContraste As Boolean
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
    Private _estadoVisualActual As EstadoVisual
    Private _fechaUltimoEstado As DateTime
    Private _resultadoOperacionActual As String
    Private _sonidoCancellation As CancellationTokenSource
    Private ReadOnly _sonidoSync As New Object()
    Private _ultimoIntentoReconexion As DateTime

    Private ReadOnly Cls As New FuncionesDB
    Private ReadOnly ComedorSvc As New ComedorDataService(Cls)
    Private ReadOnly OperacionSvc As New ComedorOperacionService()
    Private ReadOnly Cn As New SqlClient.SqlConnection
    Private Ds As New DataSet
    Private DsBeca As New DataSet
    Private DsHorarios As New DataSet

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
        Compact = 0
        Standard = 1
        Wide = 2
    End Enum

    Private Sub ControlComedor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            UIThemeManagerV2.Apply(Me, "operativo")
            CargarPreferenciasOperacion()
            ApplyModernOperationalLayout()
            InicializarControlesOperacion()

            Cls.AbrirConexion(Cn, False)
            OperacionSvc.AsegurarEsquema(Cn)
            SincronizarPermisoSinMarcaTransporte()
            Ds = ComedorSvc.CargarUsuariosConMarcaTransporte(Cn, FechaServer)
            DsBeca = ComedorSvc.CargarBecas(Cn)
            DsHorarios = ComedorSvc.CargarHorarios(Cn)

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
        Catch
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
        _totalLecturas += 1
        _inicioLectura = DateTime.Now
        Dim carnet As String = carnetRaw.Trim()

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

        Dim usuario As DataRow = BuscarUsuarioPorCarnet(carnet)
        If usuario Is Nothing Then
            UpdateVisualState(EstadoVisual.NotFound)
            EnsureScanFocus(True)
            Exit Sub
        End If

        If String.Equals(UltimoCarnetProcesado, carnet, StringComparison.OrdinalIgnoreCase) Then
            ErrorLecturaDuplicada = True
            _totalDuplicadas += 1
            UpdateVisualState(EstadoVisual.Duplicate)
            RegistrarTiempoAtencion()
            ActualizarKpisOperacion()
            EnsureScanFocus(True)
            Exit Sub
        End If

        Dim warningSinMarcaTransporte As Boolean = False
        Dim warningMarcaTardia As Boolean = False

        If EsEstudiante(usuario) Then
            warningSinMarcaTransporte = TieneAdvertenciaSinMarcaTransporte(usuario)
            warningMarcaTardia = TieneAdvertenciaMarcaTardia(usuario)

            If warningSinMarcaTransporte AndAlso Not PermitirSinMarcaTransporte Then
                UpdateVisualState(EstadoVisual.DeniedByRule)
                EnsureScanFocus(True)
                Exit Sub
            End If

            If warningMarcaTardia AndAlso Not PermitirMarcaTardia Then
                UpdateVisualState(EstadoVisual.DeniedByRule)
                EnsureScanFocus(True)
                Exit Sub
            End If
        End If

        RegistrarMarca(usuario)

        If ErrorTiquetes Then
            _totalErrores += 1
            UpdateVisualState(EstadoVisual.NoTickets)
        ElseIf warningSinMarcaTransporte Then
            UpdateVisualState(EstadoVisual.NoTransportMark)
        ElseIf warningMarcaTardia Then
            UpdateVisualState(EstadoVisual.LateTransportMark)
        ElseIf EstadoVerificado Then
            _lecturasExitosas += 1
            UpdateVisualState(EstadoVisual.Success)
        Else
            _totalErrores += 1
            UpdateVisualState(EstadoVisual.NotFound)
        End If

        RegistrarTiempoAtencion()
        ActualizarKpisOperacion()
        EnsureScanFocus(True)
    End Sub

    Private Sub RegistrarMarca(ByVal usuario As DataRow)
        Try
            LblCedula.Text = CStr(usuario("Cedula"))
            TxtUsuario.Text = String.Format("{0} {1} {2}", CStr(usuario("Nombre")), CStr(usuario("PrimerApellido")), CStr(usuario("SegundoApellido"))).Trim()

            If EsEstudiante(usuario) Then
                TxtTipo.Text = "ESTUDIANTE"
            Else
                TxtTipo.Text = "PROFESOR"
            End If

            Dim resultado As ComedorDataService.MarcaComedorResultado = ComedorSvc.RegistrarMarca(usuario, DsBeca, DiaSemana, Cn)
            TxtTiquetes.Text = resultado.TextoTiquetes
            ErrorTiquetes = resultado.ErrorTiquetes

            If resultado.RegistroGuardado Then
                UltimoCarnetProcesado = CStr(usuario("Cedula"))
                EstadoVerificado = True
            End If
        Catch ex As Exception
            EstadoVerificado = False
            ErrorLogger.LogException("ControlComedor.RegistrarMarca", ex)
            UpdateVisualState(EstadoVisual.NotFound)
        End Try
    End Sub

    Private Function BuscarUsuarioPorCarnet(ByVal carnet As String) As DataRow
        If Ds Is Nothing OrElse Ds.Tables.Count = 0 OrElse Ds.Tables(0).Rows.Count = 0 Then
            Return Nothing
        End If

        For Each row As DataRow In Ds.Tables(0).Rows
            If String.Equals(CStr(row("Cedula")), carnet, StringComparison.OrdinalIgnoreCase) Then
                Return row
            End If
        Next

        Return Nothing
    End Function

    Private Function EsEstudiante(ByVal usuario As DataRow) As Boolean
        Return CShort(usuario("CodTipo")) = 1S
    End Function

    Private Function TieneAdvertenciaSinMarcaTransporte(ByVal usuario As DataRow) As Boolean
        If IsDBNull(usuario("MarcaTransporte")) Then
            Return True
        End If

        Return CInt(usuario("MarcaTransporte")) = 0
    End Function

    Private Function TieneAdvertenciaMarcaTardia(ByVal usuario As DataRow) As Boolean
        If DsHorarios Is Nothing OrElse DsHorarios.Tables.Count = 0 Then
            Return False
        End If

        If IsDBNull(usuario("IdHorario")) OrElse IsDBNull(usuario("HoraMarca")) Then
            Return False
        End If

        Dim idHorario As Integer = CInt(usuario("IdHorario"))
        Dim horarios() As DataRow = DsHorarios.Tables(0).Select("IdHorario = " & idHorario.ToString(CultureInfo.InvariantCulture))
        If horarios Is Nothing OrElse horarios.Length = 0 Then
            Return False
        End If

        Dim horaLimite As TimeSpan = ConvertirATimeSpan(horarios(0)("HoraLimite"))
        Dim horaMarca As TimeSpan = CType(usuario("HoraMarca"), Date).TimeOfDay
        Return horaMarca > horaLimite
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

    Private Function ConvertirABooleano(ByVal raw As Object) As Boolean
        If raw Is Nothing OrElse IsDBNull(raw) Then
            Return False
        End If

        If TypeOf raw Is Boolean Then
            Return CBool(raw)
        End If

        If IsNumeric(raw) Then
            Return CInt(raw) <> 0
        End If

        Dim texto As String = CStr(raw).Trim()
        Dim parsed As Boolean
        If Boolean.TryParse(texto, parsed) Then
            Return parsed
        End If

        Return texto = "1"
    End Function

    Private Sub ResetResultFields()
        LblCedula.Clear()
        TxtUsuario.Clear()
        TxtTipo.Clear()
        TxtTiquetes.Clear()
        Picture.Image = ObtenerLogoAplicacion()
        LblRegistroError.Text = String.Empty
    End Sub

    Private Sub ApplyBrandAssets()
        PicBrandHeader.Visible = False
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
            _lblFocusEscaneo.Text = "Lector listo (foco activo)"
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
        PanelMainStatus.BorderStyle = BorderStyle.FixedSingle

        BunifuGradientPanel1.BackColor = Color.FromArgb(13, 30, 54)

        LblTitulo.Font = New Font("Segoe UI Semibold", 28.0!, FontStyle.Bold)
        LblTitulo.ForeColor = Color.FromArgb(17, 33, 59)
        LblTitulo.TextAlign = ContentAlignment.MiddleLeft

        lblProcesando.Font = New Font("Segoe UI Semibold", 24.0!, FontStyle.Bold)
        lblProcesando.ForeColor = Color.FromArgb(23, 32, 51)
        lblProcesando.TextAlign = ContentAlignment.MiddleCenter

        LblFecha.ForeColor = Color.FromArgb(214, 226, 246)
        LblScanHint.ForeColor = Color.FromArgb(220, 232, 252)
        LblScanHint.Text = "Escanear carnet (Enter)" & Environment.NewLine & "Limpiar (F2) | Salir (Esc) | Alto contraste (F7)"
        LblScanHint.AutoSize = False
        LblScanHint.TextAlign = ContentAlignment.MiddleLeft
        LblFecha.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
        LblScanHint.Font = New Font("Segoe UI", 11.0!, FontStyle.Bold)

        TxtCedula.Font = New Font("Segoe UI Semibold", 21.0!, FontStyle.Bold)
        TxtCedula.BorderStyle = BorderStyle.FixedSingle
        TxtCedula.BackColor = Color.White
        TxtCedula.ForeColor = Color.FromArgb(17, 33, 59)

        GbDatos.BackColor = Color.White
        GbDatos.ForeColor = Color.FromArgb(36, 51, 77)
        GbDatos.FlatStyle = FlatStyle.Flat
        GbDatos.Font = New Font("Segoe UI", 11.0!, FontStyle.Bold)

        Label2.ForeColor = Color.FromArgb(76, 90, 112)
        Label3.ForeColor = Color.FromArgb(76, 90, 112)
        Label4.ForeColor = Color.FromArgb(76, 90, 112)
        Label5.ForeColor = Color.FromArgb(76, 90, 112)
        LblRegistroError.ForeColor = Color.FromArgb(161, 47, 65)
        LblRegistroError.Font = New Font("Segoe UI", 9.5!, FontStyle.Bold)

        TxtUsuario.Font = New Font("Segoe UI", 13.0!, FontStyle.Bold)
        TxtUsuario.BackColor = Color.FromArgb(248, 251, 255)
        TxtTipo.Font = New Font("Segoe UI", 13.0!, FontStyle.Bold)
        TxtTipo.BackColor = Color.FromArgb(248, 251, 255)
        TxtTiquetes.Font = New Font("Segoe UI", 24.0!, FontStyle.Bold)
        TxtTiquetes.BackColor = Color.FromArgb(248, 251, 255)
        LblCedula.BackColor = Color.FromArgb(248, 251, 255)

        Picture.BorderStyle = BorderStyle.None
        Picture.BackColor = Color.FromArgb(13, 30, 54)
        Picture.SizeMode = PictureBoxSizeMode.Zoom

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
        Label4.Text = "Carnet"
        Label2.Text = "Nombre"
        Label3.Text = "Tiquetes"

        Label5.Visible = False
        TxtTipo.Visible = False

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

        AplicarModoAltoContraste()
        ResetResultFields()
    End Sub

    Private Sub ApplyResponsiveLayout()
        If _lblMeta Is Nothing OrElse _lblHistorial Is Nothing OrElse _lstHistorial Is Nothing Then
            Exit Sub
        End If

        Dim mode As LayoutMode = ObtenerModoLayout()
        Dim leftWidth As Integer = CInt(Math.Round(Me.ClientSize.Width * 0.3R))
        leftWidth = Math.Max(SidebarMinWidth, Math.Min(SidebarMaxWidth, leftWidth))

        BunifuGradientPanel1.Width = leftWidth

        Dim pad As Integer = 20
        Dim contentX As Integer = leftWidth + pad
        Dim contentWidth As Integer = Math.Max(560, Me.ClientSize.Width - contentX - pad - 2)

        LblTitulo.SetBounds(contentX + 8, 14, Math.Max(240, contentWidth - 220), 72)
        PicBrandHeader.SetBounds(contentX + contentWidth - 206, 22, 190, 54)
        PanelMainStatus.SetBounds(contentX, 92, contentWidth, Math.Max(300, Me.ClientSize.Height - 110))

        Dim statusWidth As Integer = PanelMainStatus.ClientSize.Width
        Dim statusHeight As Integer = PanelMainStatus.ClientSize.Height
        If _lblResultadoOperacion IsNot Nothing Then
            _lblResultadoOperacion.SetBounds(24, 46, Math.Max(260, statusWidth - 48), 52)
        End If
        If _lblEstadoChip IsNot Nothing Then
            _lblEstadoChip.SetBounds((statusWidth \ 2) - 110, 10, 220, 30)
        End If

        lblProcesando.SetBounds(28, 108, Math.Max(260, statusWidth - 56), 82)

        Dim iconSize As Integer = Math.Max(180, Math.Min(280, CInt(Math.Round(Math.Min(statusWidth, statusHeight) * 0.32R))))
        Dim iconX As Integer = Math.Max(0, (statusWidth - iconSize) \ 2)
        Dim iconY As Integer = Math.Max(216, (statusHeight - iconSize) \ 2 + 24)
        Imgprocess.SetBounds(iconX, iconY, iconSize, iconSize)

        ApplySidebarLayout(leftWidth, mode)
        ApplyDynamicOperationalLayout(leftWidth, mode)
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
        _btnIncidencia.SetBounds(20, leftHeight - 232, 188, 36)
        _lblHotkeys.SetBounds(20, leftHeight - 38, leftWidth - 40, 32)

        _lblResultadoOperacion.SetBounds(24, 46, Math.Max(260, statusWidth - 48), 52)
        If _lblEstadoChip IsNot Nothing Then
            _lblEstadoChip.SetBounds((statusWidth \ 2) - 110, 10, 220, 30)
        End If

        Dim gX As Integer = 12
        Dim gW As Integer = Math.Max(220, GbDatos.ClientSize.Width - 28)
        _lblHistorial.SetBounds(gX, 280, gW, 22)
        _lstHistorial.SetBounds(gX, 304, gW, Math.Max(120, GbDatos.ClientSize.Height - 314))
    End Sub

    Private Sub ApplySidebarLayout(ByVal leftWidth As Integer, ByVal mode As LayoutMode)
        If _lblHistorial Is Nothing OrElse _lstHistorial Is Nothing Then
            Exit Sub
        End If

        Dim innerX As Integer = 20
        Dim innerW As Integer = leftWidth - (innerX * 2)
        Dim topY As Integer = 108
        Dim sidebarHeight As Integer = Math.Max(600, BunifuGradientPanel1.ClientSize.Height)
        Dim footerReserve As Integer = If(mode = LayoutMode.Compact, 210, 236)

        Picture.SetBounds(innerX + (innerW - 248) \ 2, topY, 248, 98)
        LblScanHint.SetBounds(innerX, topY + 108, innerW, 52)
        TxtCedula.SetBounds(innerX, topY + 164, innerW, 56)
        If _lblFocusEscaneo IsNot Nothing Then
            _lblFocusEscaneo.SetBounds(innerX, topY + 224, innerW, 24)
        End If

        Dim gbTop As Integer = topY + 252
        Dim gbMaxBottom As Integer = sidebarHeight - footerReserve
        Dim gbHeight As Integer = gbMaxBottom - gbTop
        If mode = LayoutMode.Compact Then
            gbHeight = Math.Max(260, gbHeight)
        Else
            gbHeight = Math.Max(300, gbHeight)
        End If
        GbDatos.SetBounds(12, gbTop, leftWidth - 24, gbHeight)

        Dim gX As Integer = 14
        Dim gW As Integer = GbDatos.ClientSize.Width - 28
        Label4.SetBounds(gX, 30, gW, 22)
        LblCedula.SetBounds(gX, 54, gW, 42)
        Label2.SetBounds(gX, 104, gW, 22)
        TxtUsuario.SetBounds(gX, 128, gW, 42)
        Label3.SetBounds(gX, 178, gW, 22)
        TxtTiquetes.SetBounds(gX, 202, gW, 56)
        LblRegistroError.SetBounds(gX, 262, gW, 34)

        _lblHistorial.SetBounds(gX, 302, gW, 22)
        _lstHistorial.SetBounds(gX, 326, gW, Math.Max(52, GbDatos.ClientSize.Height - 336))

        BtnSalir.SetBounds(0, 0, 0, 0)
    End Sub

    Private Sub ApplyDynamicOperationalLayout(ByVal leftWidth As Integer, ByVal mode As LayoutMode)
        If _lblMeta Is Nothing OrElse _lblHistorial Is Nothing OrElse _lstHistorial Is Nothing Then
            Exit Sub
        End If

        Dim sidebarHeight As Integer = Math.Max(600, BunifuGradientPanel1.ClientSize.Height)
        Dim hotkeysTop As Integer = sidebarHeight - 34
        Dim infoBottom As Integer = hotkeysTop - 4
        Dim recomendacionTop As Integer = infoBottom - 34
        Dim alertaTop As Integer = recomendacionTop - 22
        Dim colaTop As Integer = alertaTop - 22
        Dim kpiTop As Integer = colaTop - 22
        Dim conexionTop As Integer = kpiTop - 22
        Dim incidenciaTop As Integer = Math.Max(GbDatos.Bottom + 8, conexionTop - 44)

        _lblMeta.SetBounds(20, 14, leftWidth - 40, 22)
        _progressMeta.SetBounds(20, 38, leftWidth - 40, 14)
        _lblUltimaLectura.SetBounds(20, 56, leftWidth - 40, 22)
        _lblEdadEstado.SetBounds(20, 78, leftWidth - 40, 22)
        _btnIncidencia.SetBounds(20, incidenciaTop, 188, 36)
        _lblConexion.SetBounds(20, conexionTop, leftWidth - 40, 20)
        _lblKpi.SetBounds(20, kpiTop, leftWidth - 40, 20)
        _lblCola.SetBounds(20, colaTop, leftWidth - 40, 20)
        _lblAlertas.SetBounds(20, alertaTop, leftWidth - 40, 20)
        _lblRecomendacion.SetBounds(20, recomendacionTop, leftWidth - 40, 34)
        _lblHotkeys.SetBounds(20, hotkeysTop, leftWidth - 40, 30)

        Dim gX As Integer = 12
        Dim gW As Integer = GbDatos.ClientSize.Width - 28
        _lblHistorial.SetBounds(gX, 302, gW, 22)
        _lstHistorial.SetBounds(gX, 326, gW, Math.Max(52, GbDatos.ClientSize.Height - 336))

        If mode = LayoutMode.Compact Then
            LblScanHint.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
        Else
            LblScanHint.Font = New Font("Segoe UI", 11.0!, FontStyle.Bold)
        End If
    End Sub

    Private Function ObtenerModoLayout() As LayoutMode
        If Me.ClientSize.Width < 1480 OrElse Me.ClientSize.Height < 820 Then
            Return LayoutMode.Compact
        End If
        If Me.ClientSize.Width >= 1820 Then
            Return LayoutMode.Wide
        End If
        Return LayoutMode.Standard
    End Function

    Private Sub AplicarModoAltoContraste()
        If _modoAltoContraste Then
            PanelResult.BackColor = Color.Black
            PanelMainStatus.BackColor = Color.Black
            PanelMainStatus.BorderStyle = BorderStyle.Fixed3D
            lblProcesando.ForeColor = Color.White
            LblTitulo.ForeColor = Color.White
            LblScanHint.ForeColor = Color.White
            TxtCedula.BackColor = Color.Black
            TxtCedula.ForeColor = Color.White
            TxtCedula.BorderStyle = BorderStyle.Fixed3D
            If _lblResultadoOperacion IsNot Nothing Then
                _lblResultadoOperacion.ForeColor = Color.White
            End If
            If _lblEstadoChip IsNot Nothing Then
                _lblEstadoChip.BackColor = Color.Gold
                _lblEstadoChip.ForeColor = Color.Black
            End If
        Else
            PanelMainStatus.BorderStyle = BorderStyle.FixedSingle
            TxtCedula.BorderStyle = BorderStyle.FixedSingle
            TxtCedula.BackColor = Color.White
            TxtCedula.ForeColor = Color.FromArgb(17, 33, 59)
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
    End Sub

    Private Sub OnActividadMouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        RegistrarActividad()
    End Sub

    Private Sub OnActividadKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        RegistrarActividad()
    End Sub

    Private Sub RegistrarActividad()
        _ultimaActividad = DateTime.Now
        _limpiezaAplicadaPorInactividad = False
    End Sub

    Private Sub TimerInactividad_Tick(ByVal sender As Object, ByVal e As EventArgs)
        IntentarReconexionSiCorresponde()
        LblFecha.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
        ActualizarEstadoConexion()
        ActualizarIndicadoresEstadoInfo()
        ActualizarIndicadorFoco(TxtCedula.Focused)

        If _limpiezaAplicadaPorInactividad Then
            Exit Sub
        End If

        Dim segundosSinActividad As Double = DateTime.Now.Subtract(_ultimaActividad).TotalSeconds
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
        If DateTime.Now.Subtract(_ultimoIntentoReconexion).TotalSeconds < 5 Then
            Exit Sub
        End If

        _ultimoIntentoReconexion = DateTime.Now
        Try
            Cls.AbrirConexion(Cn, False)
            OperacionSvc.AsegurarEsquema(Cn)
            SincronizarPermisoSinMarcaTransporte()
        Catch ex As Exception
            ErrorLogger.LogException("ControlComedor.IntentarReconexionSiCorresponde", ex)
        End Try
    End Sub

    Private Sub SincronizarPermisoSinMarcaTransporte()
        Try
            Dim valores() As FuncionesDB.Campos = Cls.InicializarArray()
            Dim llave() As FuncionesDB.Campos = Cls.InicializarArray()
            Cls.ArmaValor(valores, "PermitirSinMarcaTransporte")
            Cls.ArmaValor(llave, "1", 1)
            Dim dsParam As DataSet = Cls.Consultar("Parametro", valores, llave, Cn)

            Dim permitido As Boolean = False
            If dsParam IsNot Nothing AndAlso dsParam.Tables.Count > 0 AndAlso dsParam.Tables(0).Rows.Count > 0 Then
                Dim raw As Object = dsParam.Tables(0).Rows(0)!PermitirSinMarcaTransporte
                permitido = ConvertirABooleano(raw)
            End If

            PermitirSinMarcaTransporte = permitido
            ErrorLogger.LogInfo("ControlComedor.SincronizarPermisoSinMarcaTransporte", "PermitirSinMarcaTransporte=" & PermitirSinMarcaTransporte.ToString())
        Catch ex As Exception
            PermitirSinMarcaTransporte = False
            ErrorLogger.LogException("ControlComedor.SincronizarPermisoSinMarcaTransporte", ex, "Fallback=false")
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
            Catch
            End Try
        End If

        _logoAplicacion = My.Resources.Login
        Return _logoAplicacion
    End Function

    Private Function ResolverRutaRecursoMarca(ByVal nombreArchivo As String) As String
        If Global.System.IO.Path.IsPathRooted(nombreArchivo) AndAlso Global.System.IO.File.Exists(nombreArchivo) Then
            Return nombreArchivo
        End If

        Dim candidatos As String() = {
            Global.System.IO.Path.Combine(Application.StartupPath, "Resources", nombreArchivo),
            Global.System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", nombreArchivo),
            Global.System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources", nombreArchivo),
            Global.System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Resources", nombreArchivo),
            Global.System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Resources", nombreArchivo)
        }

        For Each candidato As String In candidatos
            Dim rutaCompleta As String = Global.System.IO.Path.GetFullPath(candidato)
            If Global.System.IO.File.Exists(rutaCompleta) Then
                Return rutaCompleta
            End If
        Next

        Return candidatos(0)
    End Function

    Private Sub CargarPreferenciasOperacion()
        _sonidosHabilitados = LeerConfigBool("SonidosHabilitados", True)
        _modoAccesible = LeerConfigBool("ModoAccesible", False)
        _mostrarVistaSupervisor = LeerConfigBool("MostrarVistaSupervisor", False)
        _metaDiaria = LeerConfigInt("MetaDiariaComedor", 650)
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
        If _metaDiaria <= 0 Then
            _metaDiaria = 650
        End If
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
        _lblResultadoOperacion.Font = New Font("Segoe UI Semibold", 22.0!, FontStyle.Bold)
        _lblResultadoOperacion.TextAlign = ContentAlignment.MiddleCenter
        _lblResultadoOperacion.Text = "SIN LECTURA"

        _lblEstadoChip = New Label()
        _lblEstadoChip.AutoSize = False
        _lblEstadoChip.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
        _lblEstadoChip.ForeColor = Color.White
        _lblEstadoChip.BackColor = Color.FromArgb(51, 65, 85)
        _lblEstadoChip.TextAlign = ContentAlignment.MiddleCenter
        _lblEstadoChip.Text = "EN ESPERA"

        _lblHistorial = New Label()
        _lblHistorial.AutoSize = False
        _lblHistorial.Font = New Font("Segoe UI", 9.5!, FontStyle.Bold)
        _lblHistorial.ForeColor = Color.FromArgb(76, 90, 112)
        _lblHistorial.Text = "Ultimos 10 eventos"

        _lstHistorial = New ListBox()
        _lstHistorial.Font = New Font("Segoe UI", 9.5!, FontStyle.Regular)
        _lstHistorial.HorizontalScrollbar = True
        _lstHistorial.IntegralHeight = False
        _lstHistorial.BorderStyle = BorderStyle.FixedSingle

        _lblFocusEscaneo = New Label()
        _lblFocusEscaneo.AutoSize = False
        _lblFocusEscaneo.TextAlign = ContentAlignment.MiddleLeft
        _lblFocusEscaneo.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
        _lblFocusEscaneo.ForeColor = Color.FromArgb(157, 230, 170)
        _lblFocusEscaneo.Text = "Lector listo (foco activo)"

        _lblHotkeys = New Label()
        _lblHotkeys.AutoSize = False
        _lblHotkeys.TextAlign = ContentAlignment.MiddleLeft
        _lblHotkeys.Font = New Font("Segoe UI", 8.5!, FontStyle.Bold)
        _lblHotkeys.ForeColor = Color.FromArgb(194, 214, 243)
        _lblHotkeys.Text = "Esc salir | F2 limpiar | F3 foco | F4 historial | F6 supervisor | F7 contraste"

        _btnIncidencia = New Button()
        _btnIncidencia.Text = "Incidencia Rapida"
        _btnIncidencia.FlatStyle = FlatStyle.Flat
        _btnIncidencia.FlatAppearance.BorderSize = 0
        _btnIncidencia.BackColor = Color.FromArgb(209, 143, 36)
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
        BunifuGradientPanel1.Controls.Add(_btnIncidencia)
        _btnIncidencia.BringToFront()
    End Sub

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
        _lblEdadEstado.Text = "Estado visible: " & ObtenerEdadEstadoTexto(DateTime.Now.Subtract(_fechaUltimoEstado))
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

        Dim avancePct As Double = (_lecturasExitosas / CDbl(Math.Max(1, _metaDiaria))) * 100.0R
        _lblMeta.Text = "Meta diaria: " & _lecturasExitosas.ToString("N0") & "/" & _metaDiaria.ToString("N0") & " (" & avancePct.ToString("0") & "%)"
        _progressMeta.Maximum = Math.Max(1, _metaDiaria)
        _progressMeta.Value = Math.Min(_progressMeta.Maximum, Math.Max(0, _lecturasExitosas))

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

        Dim ms As Double = DateTime.Now.Subtract(_inicioLectura).TotalMilliseconds
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
                Dim diff As Integer = CInt(Math.Max(0, DateTime.Now.Subtract(_inicioLectura).TotalMilliseconds))
                tiempoMs = diff
            End If

            Dim esAdvertencia As Boolean = (state = EstadoVisual.Duplicate OrElse state = EstadoVisual.NoTransportMark OrElse state = EstadoVisual.LateTransportMark)
            Dim esError As Boolean = (state = EstadoVisual.NotFound OrElse state = EstadoVisual.NoTickets OrElse state = EstadoVisual.DeniedByRule)
            Dim estadoEvento As String = ObtenerCodigoEstado(state)
            Dim motivoEvento As String = ObtenerCodigoEvento(state, detalle, esIncidenciaManual, codigoIncidencia)
            Dim cedulaEvento As String = ObtenerCedulaValidaParaEvento()
            OperacionSvc.RegistrarEvento(Cn,
                                         DateTime.Now,
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

        Dim motivo As String = Microsoft.VisualBasic.Interaction.InputBox(
            "Codigo incidencia (SIN_TIQUETES, ERROR_LECTOR, CARNET_DANADO, MARCA_TARDIA_TRANSPORTE, OTRO):",
            "Incidencia Rapida",
            sugerencia)

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

        Dim item As String = DateTime.Now.ToString("HH:mm:ss") & "  |  " & state.ToString() & "  |  " & detalle
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
        Try
            Dim raw As String = ConfigurationManager.AppSettings(key)
            If String.IsNullOrWhiteSpace(raw) Then
                Return valorDefault
            End If
            Dim parsed As Boolean
            If Boolean.TryParse(raw, parsed) Then
                Return parsed
            End If
        Catch
        End Try
        Return valorDefault
    End Function

    Private Function LeerConfigInt(ByVal key As String, ByVal valorDefault As Integer) As Integer
        Try
            Dim raw As String = ConfigurationManager.AppSettings(key)
            If String.IsNullOrWhiteSpace(raw) Then
                Return valorDefault
            End If
            Dim parsed As Integer
            If Integer.TryParse(raw, parsed) Then
                Return parsed
            End If
        Catch
        End Try
        Return valorDefault
    End Function

    Private Function LeerConfigDouble(ByVal key As String, ByVal valorDefault As Double) As Double
        Try
            Dim raw As String = ConfigurationManager.AppSettings(key)
            If String.IsNullOrWhiteSpace(raw) Then
                Return valorDefault
            End If
            Dim parsed As Double
            If Double.TryParse(raw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, parsed) Then
                Return parsed
            End If
            If Double.TryParse(raw, parsed) Then
                Return parsed
            End If
        Catch
        End Try
        Return valorDefault
    End Function

    Private Function LeerConfigTexto(ByVal key As String, ByVal valorDefault As String) As String
        Try
            Dim raw As String = ConfigurationManager.AppSettings(key)
            If String.IsNullOrWhiteSpace(raw) Then
                Return valorDefault
            End If
            Return raw.Trim()
        Catch
        End Try
        Return valorDefault
    End Function

    Private Function LeerConfigColor(ByVal key As String, ByVal valorDefault As Color) As Color
        Try
            Dim raw As String = ConfigurationManager.AppSettings(key)
            If String.IsNullOrWhiteSpace(raw) Then
                Return valorDefault
            End If
            Dim hex As String = raw.Trim()
            If Not hex.StartsWith("#", StringComparison.Ordinal) Then
                hex = "#" & hex
            End If
            Return ColorTranslator.FromHtml(hex)
        Catch
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
                _fechaUltimoEstado = DateTime.Now
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
                _fechaUltimoEstado = DateTime.Now
            Case EstadoVisual.Duplicate
                lblProcesando.Text = "Lectura duplicada detectada"
                lblProcesando.ForeColor = Color.FromArgb(31, 41, 55)
                LblTitulo.ForeColor = Color.FromArgb(31, 41, 55)
                Imgprocess.Image = My.Resources.Double_check
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelMainStatus.BackColor = _colorAdvertencia
                PanelResult.BackColor = _colorAdvertencia
                LblRegistroError.Text = "Accion: espere 1 minuto o escanee otro carnet."
                _resultadoOperacionActual = "PERMITIDO CON ADVERTENCIA"
                _fechaUltimoEstado = DateTime.Now
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
                _fechaUltimoEstado = DateTime.Now
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
                _fechaUltimoEstado = DateTime.Now
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
                _fechaUltimoEstado = DateTime.Now
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
                _fechaUltimoEstado = DateTime.Now
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
        ActualizarIndicadoresEstadoInfo()

        If state <> EstadoVisual.Idle AndAlso state <> EstadoVisual.Processing Then
            ReproducirSonido(state)
            RegistrarHistorial(state, TxtCedula.Text.Trim())
            RegistrarEventoPersistente(state, LblRegistroError.Text)
        End If
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
            Case EstadoVisual.Duplicate, EstadoVisual.NoTransportMark, EstadoVisual.LateTransportMark
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
        If keyData = Keys.F4 AndAlso _lstHistorial IsNot Nothing Then
            _lstHistorial.Visible = Not _lstHistorial.Visible
            _lblHistorial.Visible = _lstHistorial.Visible
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
            Return True
        End If

        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function
End Class
