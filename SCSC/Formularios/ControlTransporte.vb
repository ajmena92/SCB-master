Imports System.Configuration
Imports System.Drawing
Imports System.Linq
Imports System.Media
Imports System.Threading
Imports System.Threading.Tasks

Public Class ControlTransporte
    Private Const DesignerFirstStrict As Boolean = True
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

    Private _totalLecturas As Integer
    Private _totalDuplicadas As Integer
    Private _totalErrores As Integer
    Private _lecturasExitosas As Integer
    Private _muestrasTiempoAtencion As Integer
    Private _acumuladoTiempoAtencionMs As Double
    Private _inicioLectura As DateTime

    Private _lblResultadoOperacion As Label
    Private _lblUltimaLectura As Label
    Private _lblEdadEstado As Label
    Private _lblConexion As Label
    Private _lblKpi As Label
    Private _lblHistorial As Label
    Private _lstHistorial As ListBox
    Private _btnIncidencia As Button
    Private _lblScanHint As Label
    Private _btnSalirOperador As Button
    Private _lblFocusEscaneo As Label
    Private _lblHotkeys As Label
    Private _lblEstadoChip As Label
    Private _modoAltoContraste As Boolean

    Private ReadOnly Cls As New FuncionesDB
    Private ReadOnly TransporteSvc As New TransporteDataService(Cls)
    Private ReadOnly OperacionSvc As New TransporteOperacionService()
    Private ReadOnly Cn As New SqlClient.SqlConnection
    Private DsUsuarios As New DataSet
    Private DsRutas As New DataSet
    Private _inicializadoEventosActividad As Boolean
    Private _eventoPersistidoEnTransaccion As Boolean
    Private _estadoEventoTransaccion As EstadoVisual

    Private Enum EstadoVisual
        Idle = 0
        Processing = 1
        Success = 2
        Warning = 3
        Duplicate = 4
        ErrorGeneral = 5
        NotFound = 6
    End Enum

    Private Enum LayoutMode
        Compact = 0
        Standard = 1
        Wide = 2
    End Enum

    Private Delegate Sub BoolCall(ByVal value As Boolean)
    Private Delegate Sub RowCall(ByVal row As DataRow)

    Private Sub BtnSalir_Click(sender As Object, e As EventArgs) Handles BtnCerrar.Click
        If PermitirCierreOperador Then
            Me.Close()
        End If
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
        Catch
        End Try
        Me.Dispose()
    End Sub

    Private Sub ControlTransporte_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            UIThemeManagerV2.Apply(Me, "operativo")
            CargarPreferenciasOperacion()
            ApplyModernOperationalLayout()
            InicializarControlesOperacion()
            ApplyResponsiveLayout()

            Cls.AbrirConexion(Cn, False)
            OperacionSvc.AsegurarEsquema(Cn)
            DsUsuarios = TransporteSvc.CargarUsuariosActivos(Cn)
            DsRutas = TransporteSvc.CargarRutas(Cn)

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
            Me.Dispose()
        End Try
    End Sub

    Private Sub ControlTransporte_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        ApplyResponsiveLayout()
        EnsureScanFocus(True)
    End Sub

    Private Sub ControlTransporte_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        ApplyResponsiveLayout()
    End Sub

    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtCedula.KeyDown
        RegistrarActividad()

        If e.KeyCode <> Keys.Enter Then
            Exit Sub
        End If

        e.SuppressKeyPress = True
        ProcesarLecturaCarnet(TxtCedula.Text)
    End Sub

    Private Sub ProcesarLecturaCarnet(ByVal carnetRaw As String)
        RegistrarActividad()
        _totalLecturas += 1
        _inicioLectura = DateTime.Now
        ErrUltHuella = False
        EstadoVerificado = False
        _eventoPersistidoEnTransaccion = False

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

        Dim usuario As DataRow = BuscarUsuarioPorCedula(carnet)
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
            If TieneAdvertenciaPermisoSalida(usuario) Then
                UpdateVisualState(EstadoVisual.Warning)
            Else
                UpdateVisualState(EstadoVisual.Success)
            End If
        Else
            _totalErrores += 1
            UpdateVisualState(EstadoVisual.ErrorGeneral)
        End If

        RegistrarTiempoAtencion()
        ActualizarKpisOperacion()
        EnsureScanFocus(True)
    End Sub

    Private Function BuscarUsuarioPorCedula(ByVal cedula As String) As DataRow
        If DsUsuarios Is Nothing OrElse DsUsuarios.Tables.Count = 0 OrElse DsUsuarios.Tables(0).Rows.Count = 0 Then
            Return Nothing
        End If

        For Each row As DataRow In DsUsuarios.Tables(0).Rows
            If String.Equals(CStr(row("Cedula")), cedula, StringComparison.OrdinalIgnoreCase) Then
                Return row
            End If
        Next

        Return Nothing
    End Function

    Private Function TieneAdvertenciaPermisoSalida(ByVal usuario As DataRow) As Boolean
        If usuario Is Nothing OrElse IsDBNull(usuario("PermisoSalida")) Then
            Return True
        End If
        Return Not ConvertirABooleano(usuario("PermisoSalida"))
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

    Protected Sub ProcesarMarca(ByVal usuario As DataRow)
        If Me.InvokeRequired Then
            Invoke(New RowCall(AddressOf ProcesarMarca), usuario)
            Exit Sub
        End If

        Try
            LblCedula.Text = CStr(usuario("Cedula"))
            TxtUsuario.Text = String.Format("{0} {1} {2}", CStr(usuario("Nombre")), CStr(usuario("PrimerApellido")), CStr(usuario("SegundoApellido"))).Trim()
            TxtSeccion.Text = CStr(usuario("Seccion"))
            TxtRuta.Text = String.Empty
            LblRuta.Text = String.Empty

            For Each rut As DataRow In DsRutas.Tables(0).Rows
                If CInt(rut("IdRuta")) = CInt(usuario("IdRuta")) Then
                    TxtRuta.Text = CStr(rut("Codigo"))
                    LblRuta.Text = CStr(rut("Descripcion")) & ": " & TxtUsuario.Text
                    Exit For
                End If
            Next

            If ConvertirABooleano(usuario("PermisoSalida")) Then
                TxtPermisoSalida.Text = "SI Autorizado"
            Else
                TxtPermisoSalida.Text = "NO Autorizado"
            End If

            Ulthuella = CStr(LblCedula.Text)
            If CShort(usuario("CodTipo")) = 1S Then
                TxtTipo.Text = "ESTUDIANTE"
                LblTitulo.Text = "ESTUDIANTE: " & TxtUsuario.Text
            Else
                TxtTipo.Text = "PROF.: " & TxtUsuario.Text
                LblTitulo.Text = "PROF.: " & TxtUsuario.Text
            End If

            Dim estadoEventoTx As EstadoVisual = If(TieneAdvertenciaPermisoSalida(usuario), EstadoVisual.Warning, EstadoVisual.Success)
            Using tx As SqlClient.SqlTransaction = Cn.BeginTransaction()
                TransporteSvc.RegistrarMarcaEnTransaccion(usuario, Cn, FechaServer, tx)
                OperacionSvc.RegistrarEvento(
                    Cn,
                    DateTime.Now,
                    CStr(usuario("Cedula")),
                    ObtenerCodigoEstado(estadoEventoTx),
                    If(estadoEventoTx = EstadoVisual.Warning, "PERMITIDO_CON_ADVERTENCIA", "ACCESO_PERMITIDO"),
                    Nothing,
                    False,
                    estadoEventoTx = EstadoVisual.Warning,
                    False,
                    False,
                    tx)
                tx.Commit()
            End Using

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
        LblCedula.Clear()
        TxtTipo.Clear()
        TxtSeccion.Clear()
        TxtRuta.Clear()
        LblRuta.Text = String.Empty
        TxtUsuario.Clear()
        TxtPermisoSalida.Clear()
        TxtCedula.Clear()
        Picture.Image = My.Resources.Login
        LblRuta.Text = String.Empty
        LblTitulo.Text = "Control de Marcas - Transporte"
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
        If Not DesignerFirstStrict Then
            Me.WindowState = FormWindowState.Maximized
            Me.FormBorderStyle = FormBorderStyle.None
            Me.ControlBox = False
            Me.StartPosition = FormStartPosition.CenterScreen
        End If

        PanelResult.BackColor = Color.FromArgb(242, 246, 252)
        If Not DesignerFirstStrict Then
            PanelResult.BorderStyle = BorderStyle.FixedSingle
            PanelResult.Dock = DockStyle.None
            Panel1.Dock = DockStyle.None
            BunifuGradientPanel1.Dock = DockStyle.None
            Panel1.Visible = False
            BtnCerrar.Visible = False
        End If

        LblTitulo.Font = New Font("Segoe UI Semibold", 28.0!, FontStyle.Bold)
        LblTitulo.ForeColor = Color.FromArgb(17, 33, 59)
        LblTitulo.Text = "Control de Marcas - Transporte"

        lblProcesando.Font = New Font("Segoe UI Semibold", 22.0!, FontStyle.Bold)
        lblProcesando.ForeColor = Color.FromArgb(23, 32, 51)
        lblProcesando.Text = "Esperando lectura de carnet"

        LblFecha.ForeColor = Color.FromArgb(214, 226, 246)
        LblFecha.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)

        TxtCedula.Font = New Font("Segoe UI Semibold", 21.0!, FontStyle.Bold)
        TxtCedula.BorderStyle = BorderStyle.FixedSingle

        TxtUsuario.Font = New Font("Segoe UI", 12.5!, FontStyle.Bold)
        TxtTipo.Font = New Font("Segoe UI", 12.5!, FontStyle.Bold)
        TxtRuta.Font = New Font("Segoe UI", 12.5!, FontStyle.Bold)
        TxtSeccion.Font = New Font("Segoe UI", 12.5!, FontStyle.Bold)
        TxtPermisoSalida.Font = New Font("Segoe UI", 12.5!, FontStyle.Bold)

        BunifuGradientPanel1.BackColor = Color.FromArgb(13, 30, 54)
        GbDatos.BackColor = Color.White
        GbDatos.ForeColor = Color.FromArgb(36, 51, 77)

        Label1.ForeColor = Color.FromArgb(76, 90, 112)
        Label2.ForeColor = Color.FromArgb(76, 90, 112)
        Label3.ForeColor = Color.FromArgb(76, 90, 112)
        Label4.ForeColor = Color.FromArgb(76, 90, 112)
        Label5.ForeColor = Color.FromArgb(76, 90, 112)
        Label6.ForeColor = Color.FromArgb(76, 90, 112)

        TxtCedula.Visible = True
        TxtCedula.BringToFront()
        EnsureSidebarKioskControls()
        _lblScanHint.Text = "Escanear carnet (Enter)" & Environment.NewLine & "Limpiar (F2) | Salir (Esc) | Alto contraste (F7)"
        _lblScanHint.AutoSize = False
        _lblScanHint.TextAlign = ContentAlignment.MiddleLeft
        _lblScanHint.Font = New Font("Segoe UI", 11.0!, FontStyle.Bold)

        LblRuta.BackColor = Color.Transparent
        LblRuta.ForeColor = Color.White
        AplicarModoAltoContraste()
    End Sub

    Private Sub ApplyResponsiveLayout()
        If DesignerFirstStrict Then
            ApplySupplementaryOperationalLayout()
            Return
        End If

        Dim leftWidth As Integer = CInt(Math.Round(Me.ClientSize.Width * 0.32R))
        leftWidth = Math.Max(380, Math.Min(470, leftWidth))

        BunifuGradientPanel1.Width = leftWidth
        BunifuGradientPanel1.Left = 0
        BunifuGradientPanel1.Top = 0
        BunifuGradientPanel1.Height = Me.ClientSize.Height
        Panel1.Left = leftWidth
        Panel1.Top = 0
        Panel1.Width = Me.ClientSize.Width - leftWidth
        PanelResult.Left = leftWidth
        PanelResult.Top = 0
        PanelResult.Width = Me.ClientSize.Width - leftWidth
        PanelResult.Height = Me.ClientSize.Height

        Dim innerX As Integer = 20
        Dim innerW As Integer = leftWidth - 40

        LblFecha.SetBounds(0, 0, leftWidth, 40)
        Picture.SetBounds(innerX + (innerW - 164) \ 2, 48, 164, 108)
        If _lblScanHint IsNot Nothing Then
            _lblScanHint.SetBounds(innerX, 184, innerW, 28)
        End If
        TxtCedula.SetBounds(innerX, 216, innerW, 44)
        GbDatos.SetBounds(12, 286, leftWidth - 24, Math.Max(250, Me.ClientSize.Height - 298))
        If _btnSalirOperador IsNot Nothing Then
            _btnSalirOperador.SetBounds(leftWidth - 204, Me.ClientSize.Height - 52, 190, 36)
        End If

        Dim gX As Integer = 14
        Dim gW As Integer = GbDatos.ClientSize.Width - 28
        Label4.SetBounds(gX, 34, gW, 24)
        LblCedula.SetBounds(gX, 58, gW, 34)
        Label2.SetBounds(gX, 98, gW, 24)
        TxtUsuario.SetBounds(gX, 122, gW, 34)
        Label5.SetBounds(gX, 164, gW, 24)
        TxtTipo.SetBounds(gX, 188, gW, 34)
        Label3.SetBounds(gX, 230, gW, 24)
        TxtSeccion.SetBounds(gX, 254, gW, 34)
        Label6.SetBounds(gX, 296, gW, 24)
        TxtRuta.SetBounds(gX, 320, gW, 34)
        Label1.SetBounds(gX, 362, gW, 24)
        TxtPermisoSalida.SetBounds(gX, 386, gW, 34)

        If _lblHistorial IsNot Nothing Then
            _lblHistorial.SetBounds(gX, 426, gW, 20)
            _lstHistorial.SetBounds(gX, 448, gW, Math.Max(60, GbDatos.ClientSize.Height - 458))
        End If

        Dim statusW As Integer = PanelResult.ClientSize.Width
        Dim statusH As Integer = PanelResult.ClientSize.Height
        LblTitulo.SetBounds(24, 14, statusW - 48, 72)
        LblRuta.SetBounds(0, 90, statusW, 66)

        If _lblResultadoOperacion IsNot Nothing Then
            _lblResultadoOperacion.SetBounds(24, 170, Math.Max(260, statusW - 48), 44)
        End If

        lblProcesando.SetBounds(24, 216, Math.Max(260, statusW - 48), 70)

        Dim iconSize As Integer = Math.Max(120, Math.Min(200, CInt(Math.Round(Math.Min(statusW, statusH) * 0.24R))))
        Dim iconX As Integer = Math.Max(0, (statusW - iconSize) \ 2)
        Dim iconY As Integer = Math.Max(300, (statusH - iconSize) \ 2 + 40)
        Imgprocess.SetBounds(iconX, iconY, iconSize, iconSize)

        If _lblConexion IsNot Nothing Then
            _lblConexion.SetBounds(20, statusH - 96, statusW - 40, 20)
            _lblKpi.SetBounds(20, statusH - 74, statusW - 40, 20)
            _lblUltimaLectura.SetBounds(20, statusH - 52, statusW - 40, 20)
            _lblEdadEstado.SetBounds(20, statusH - 30, statusW - 40, 20)
            _btnIncidencia.SetBounds(statusW - 160, 14, 140, 30)
        End If
    End Sub

    Private Sub ApplySupplementaryOperationalLayout()
        EnsureSidebarKioskControls()

        Dim leftWidth As Integer = Math.Max(300, BunifuGradientPanel1.ClientSize.Width)
        Dim leftHeight As Integer = Math.Max(420, BunifuGradientPanel1.ClientSize.Height)
        Dim statusW As Integer = Math.Max(320, PanelResult.ClientSize.Width)
        Dim statusH As Integer = Math.Max(280, PanelResult.ClientSize.Height)

        If _lblScanHint IsNot Nothing Then
            _lblScanHint.SetBounds(20, 184, leftWidth - 40, 28)
        End If
        If _btnSalirOperador IsNot Nothing Then
            _btnSalirOperador.SetBounds(leftWidth - 204, leftHeight - 52, 190, 36)
        End If

        If _lblHistorial IsNot Nothing Then
            Dim gX As Integer = 14
            Dim gW As Integer = Math.Max(220, GbDatos.ClientSize.Width - 28)
            _lblHistorial.SetBounds(gX, 426, gW, 20)
            _lstHistorial.SetBounds(gX, 448, gW, Math.Max(60, GbDatos.ClientSize.Height - 458))
        End If

        If _lblResultadoOperacion IsNot Nothing Then
            _lblResultadoOperacion.SetBounds(24, 170, Math.Max(260, statusW - 48), 44)
        End If
        If _lblConexion IsNot Nothing Then
            _lblConexion.SetBounds(20, statusH - 96, statusW - 40, 20)
            _lblKpi.SetBounds(20, statusH - 74, statusW - 40, 20)
            _lblUltimaLectura.SetBounds(20, statusH - 52, statusW - 40, 20)
            _lblEdadEstado.SetBounds(20, statusH - 30, statusW - 40, 20)
            _btnIncidencia.SetBounds(Math.Max(20, statusW - 160), 14, 140, 30)
        End If
    End Sub

    Private Sub EnsureSidebarKioskControls()
        If _lblScanHint Is Nothing Then
            _lblScanHint = New Label()
            _lblScanHint.AutoSize = False
            _lblScanHint.ForeColor = Color.FromArgb(220, 232, 252)
            _lblScanHint.Font = New Font("Segoe UI", 12.0!, FontStyle.Bold)
            _lblScanHint.Text = "Escanear carnet (Enter) | Limpiar (F2) | Salir (Esc)"
            BunifuGradientPanel1.Controls.Add(_lblScanHint)
            _lblScanHint.BringToFront()
        End If

        If _btnSalirOperador Is Nothing Then
            _btnSalirOperador = New Button()
            _btnSalirOperador.Text = "Volver al dashboard"
            _btnSalirOperador.FlatStyle = FlatStyle.Flat
            _btnSalirOperador.FlatAppearance.BorderSize = 0
            _btnSalirOperador.BackColor = Color.FromArgb(161, 47, 65)
            _btnSalirOperador.ForeColor = Color.White
            _btnSalirOperador.Font = New Font("Segoe UI", 9.5!, FontStyle.Bold)
            _btnSalirOperador.Visible = PermitirCierreOperador
            AddHandler _btnSalirOperador.Click, AddressOf BtnSalirOperador_Click
            BunifuGradientPanel1.Controls.Add(_btnSalirOperador)
            _btnSalirOperador.BringToFront()
        End If
    End Sub

    Private Sub BtnSalirOperador_Click(ByVal sender As Object, ByVal e As EventArgs)
        If PermitirCierreOperador Then
            Me.Close()
        End If
    End Sub

    Private Sub InicializarControlesOperacion()
        If _lblConexion IsNot Nothing Then
            Exit Sub
        End If

        _lblResultadoOperacion = New Label()
        _lblResultadoOperacion.AutoSize = False
        _lblResultadoOperacion.ForeColor = Color.FromArgb(17, 33, 59)
        _lblResultadoOperacion.BackColor = Color.Transparent
        _lblResultadoOperacion.Font = New Font("Segoe UI Semibold", 16.0!, FontStyle.Bold)
        _lblResultadoOperacion.TextAlign = ContentAlignment.MiddleCenter
        _lblResultadoOperacion.Text = "SIN LECTURA"

        _lblConexion = New Label()
        _lblConexion.AutoSize = False
        _lblConexion.ForeColor = Color.FromArgb(220, 232, 252)
        _lblConexion.Font = New Font("Segoe UI", 8.5!, FontStyle.Bold)

        _lblKpi = New Label()
        _lblKpi.AutoSize = False
        _lblKpi.ForeColor = Color.FromArgb(220, 232, 252)
        _lblKpi.Font = New Font("Segoe UI", 8.5!, FontStyle.Bold)

        _lblUltimaLectura = New Label()
        _lblUltimaLectura.AutoSize = False
        _lblUltimaLectura.ForeColor = Color.FromArgb(220, 232, 252)
        _lblUltimaLectura.Font = New Font("Segoe UI", 8.5!, FontStyle.Bold)
        _lblUltimaLectura.Text = "Ultima lectura: --"

        _lblEdadEstado = New Label()
        _lblEdadEstado.AutoSize = False
        _lblEdadEstado.ForeColor = Color.FromArgb(255, 224, 171)
        _lblEdadEstado.Font = New Font("Segoe UI", 8.5!, FontStyle.Bold)
        _lblEdadEstado.Text = "Estado visible: sin evento"

        _lblHistorial = New Label()
        _lblHistorial.AutoSize = False
        _lblHistorial.Font = New Font("Segoe UI", 8.5!, FontStyle.Bold)
        _lblHistorial.ForeColor = Color.FromArgb(76, 90, 112)
        _lblHistorial.Text = "Ultimos 10 eventos"

        _lstHistorial = New ListBox()
        _lstHistorial.Font = New Font("Segoe UI", 8.5!, FontStyle.Regular)
        _lstHistorial.HorizontalScrollbar = True
        _lstHistorial.IntegralHeight = False

        _btnIncidencia = New Button()
        _btnIncidencia.Text = "Incidencia Rapida"
        _btnIncidencia.FlatStyle = FlatStyle.Flat
        _btnIncidencia.FlatAppearance.BorderSize = 0
        _btnIncidencia.BackColor = Color.FromArgb(209, 143, 36)
        _btnIncidencia.ForeColor = Color.White
        _btnIncidencia.Font = New Font("Segoe UI", 8.5!, FontStyle.Bold)
        AddHandler _btnIncidencia.Click, AddressOf BtnIncidencia_Click

        PanelResult.Controls.Add(_lblResultadoOperacion)
        PanelResult.Controls.Add(_lblConexion)
        PanelResult.Controls.Add(_lblKpi)
        PanelResult.Controls.Add(_lblUltimaLectura)
        PanelResult.Controls.Add(_lblEdadEstado)
        PanelResult.Controls.Add(_btnIncidencia)
        GbDatos.Controls.Add(_lblHistorial)
        GbDatos.Controls.Add(_lstHistorial)

        _lblResultadoOperacion.BringToFront()
        _lblConexion.BringToFront()
        _lblKpi.BringToFront()
        _lblUltimaLectura.BringToFront()
        _lblEdadEstado.BringToFront()
        _btnIncidencia.BringToFront()
    End Sub

    Private Sub BtnIncidencia_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim detalle As String = InputBox("Describa incidencia de transporte:", "Incidencia rapida", "INCIDENCIA_OPERATIVA")
        If String.IsNullOrWhiteSpace(detalle) Then
            Exit Sub
        End If
        RegistrarHistorial(EstadoVisual.ErrorGeneral, "INCIDENCIA: " & detalle.Trim())
        _resultadoOperacionActual = "INCIDENCIA MANUAL"
        _fechaUltimoEstado = DateTime.Now
        RegistrarEventoPersistente(EstadoVisual.ErrorGeneral, True)
        LblRuta.Text = "Incidencia registrada"
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
        _ultimaActividad = DateTime.Now
        _limpiezaAplicadaPorInactividad = False
    End Sub

    Private Sub TimerInactividad_Tick(ByVal sender As Object, ByVal e As EventArgs)
        LblFecha.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
        IntentarReconexionSiCorresponde()
        ActualizarEstadoConexion()
        ActualizarIndicadoresEstadoInfo()

        If _limpiezaAplicadaPorInactividad Then
            Exit Sub
        End If

        Dim segundosSinActividad As Double = DateTime.Now.Subtract(_ultimaActividad).TotalSeconds
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
        If DateTime.Now.Subtract(_ultimoIntentoReconexion).TotalSeconds < 5 Then
            Exit Sub
        End If

        _ultimoIntentoReconexion = DateTime.Now
        Try
            Cls.AbrirConexion(Cn, False)
            OperacionSvc.AsegurarEsquema(Cn)
            DsUsuarios = TransporteSvc.CargarUsuariosActivos(Cn)
            DsRutas = TransporteSvc.CargarRutas(Cn)
            CargarHistorialInicial()
        Catch ex As Exception
            ErrorLogger.LogException("ControlTransporte.IntentarReconexionSiCorresponde", ex)
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
                Imgprocess.Image = My.Resources.Info
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelResult.BackColor = _colorNeutro
                _resultadoOperacionActual = "SIN LECTURA"
                _fechaUltimoEstado = DateTime.MinValue
            Case EstadoVisual.Processing
                lblProcesando.Text = "Procesando lectura..."
                lblProcesando.ForeColor = Color.FromArgb(17, 33, 59)
                LblTitulo.ForeColor = Color.FromArgb(17, 33, 59)
                Imgprocess.Image = My.Resources.Gif_cargando
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelResult.BackColor = _colorProcesando
                _resultadoOperacionActual = "PROCESANDO..."
            Case EstadoVisual.Success
                lblProcesando.Text = "Marca registrada correctamente"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                Imgprocess.Image = My.Resources.Verificado2
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelResult.BackColor = _colorExito
                _resultadoOperacionActual = "ACCESO PERMITIDO"
                _fechaUltimoEstado = DateTime.Now
            Case EstadoVisual.Warning
                lblProcesando.Text = "Acceso permitido con advertencia"
                lblProcesando.ForeColor = Color.FromArgb(31, 41, 55)
                LblTitulo.ForeColor = Color.FromArgb(31, 41, 55)
                Imgprocess.Image = My.Resources.Info
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelResult.BackColor = _colorAdvertencia
                _resultadoOperacionActual = "PERMITIDO CON ADVERTENCIA"
                _fechaUltimoEstado = DateTime.Now
            Case EstadoVisual.Duplicate
                lblProcesando.Text = "Se detecto doble verificacion"
                lblProcesando.ForeColor = Color.FromArgb(31, 41, 55)
                LblTitulo.ForeColor = Color.FromArgb(31, 41, 55)
                Imgprocess.Image = My.Resources.Double_check
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelResult.BackColor = _colorAdvertencia
                _resultadoOperacionActual = "LECTURA DUPLICADA"
                _fechaUltimoEstado = DateTime.Now
            Case EstadoVisual.NotFound
                lblProcesando.Text = "Identificador no valido o no encontrado"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                Imgprocess.Image = My.Resources.Error2
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelResult.BackColor = _colorError
                ResetResultFields()
                _resultadoOperacionActual = "ACCESO DENEGADO"
                _fechaUltimoEstado = DateTime.Now
            Case EstadoVisual.ErrorGeneral
                lblProcesando.Text = "Error al verificar el usuario"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                Imgprocess.Image = My.Resources.Error2
                Imgprocess.SizeMode = PictureBoxSizeMode.Zoom
                PanelResult.BackColor = _colorError
                _resultadoOperacionActual = "ERROR OPERATIVO"
                _fechaUltimoEstado = DateTime.Now
        End Select

        If _lblResultadoOperacion IsNot Nothing Then
            _lblResultadoOperacion.Text = _resultadoOperacionActual
            If state = EstadoVisual.Success OrElse state = EstadoVisual.NotFound OrElse state = EstadoVisual.ErrorGeneral Then
                _lblResultadoOperacion.ForeColor = Color.White
            Else
                _lblResultadoOperacion.ForeColor = Color.FromArgb(31, 41, 55)
            End If
        End If

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

    Private Sub RegistrarHistorial(ByVal state As EstadoVisual, ByVal detalle As String)
        If _lstHistorial Is Nothing Then
            Exit Sub
        End If
        Dim item As String = DateTime.Now.ToString("HH:mm:ss") & " | " & state.ToString() & " | " & detalle
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
                ms = CInt(Math.Max(0, Math.Round(DateTime.Now.Subtract(_inicioLectura).TotalMilliseconds)))
            End If

            OperacionSvc.RegistrarEvento(Cn,
                                         DateTime.Now,
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

        Dim ms As Double = DateTime.Now.Subtract(_inicioLectura).TotalMilliseconds
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
            Catch
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
            Catch
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
        If IO.Path.IsPathRooted(nombreArchivo) AndAlso IO.File.Exists(nombreArchivo) Then
            Return nombreArchivo
        End If

        Dim candidatos As String() = {
            IO.Path.Combine(Application.StartupPath, "Resources", nombreArchivo),
            IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", nombreArchivo),
            IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources", nombreArchivo),
            IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Resources", nombreArchivo)
        }

        For Each candidato As String In candidatos
            Dim rutaCompleta As String = IO.Path.GetFullPath(candidato)
            If IO.File.Exists(rutaCompleta) Then
                Return rutaCompleta
            End If
        Next

        Return candidatos(0)
    End Function

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

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape Then
            Me.Close()
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
        If keyData = Keys.F4 AndAlso _lstHistorial IsNot Nothing Then
            _lstHistorial.Visible = Not _lstHistorial.Visible
            _lblHistorial.Visible = _lstHistorial.Visible
            Return True
        End If

        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Private Sub LblFecha_Click(sender As Object, e As EventArgs) Handles LblFecha.Click
    End Sub

    Private Sub TxtCedula_TextChanged(sender As Object, e As EventArgs) Handles TxtCedula.TextChanged
    End Sub

    Private Sub LblTitulo_Click(sender As Object, e As EventArgs) Handles LblTitulo.Click
        RegistrarActividad()
        EnsureScanFocus(False)
    End Sub

    Private Sub lblProcesando_Click(sender As Object, e As EventArgs) Handles lblProcesando.Click
        RegistrarActividad()
        EnsureScanFocus(False)
    End Sub

    Private Sub PanelResult_Paint(sender As Object, e As PaintEventArgs) Handles PanelResult.Paint
    End Sub
End Class
