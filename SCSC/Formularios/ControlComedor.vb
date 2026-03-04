Imports System.Drawing
Imports System.Linq

Public Class ControlComedor
    Private Const PermitirSinMarcaTransporte As Boolean = True
    Private Const PermitirMarcaTardia As Boolean = True
    Private Const PermitirCierreOperador As Boolean = False

    Private UltimoCarnetProcesado As String
    Private ErrorLecturaDuplicada As Boolean
    Private ErrorTiquetes As Boolean
    Private EstadoVerificado As Boolean

    Private ReadOnly Cls As New FuncionesDB
    Private ReadOnly ComedorSvc As New ComedorDataService(Cls)
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

    Private Sub ControlComedor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            UIThemeManagerV2.Apply(Me, "operativo")
            ApplyModernOperationalLayout()

            Cls.AbrirConexion(Cn, False)
            Ds = ComedorSvc.CargarUsuariosConMarcaTransporte(Cn, FechaServer)
            DsBeca = ComedorSvc.CargarBecas(Cn)
            DsHorarios = ComedorSvc.CargarHorarios(Cn)

            LblFecha.Text = FechaServer.ToString("yyyy/MM/dd HH:mm:ss")
            UltimoCarnetProcesado = String.Empty
            ResetResultFields()
            UpdateVisualState(EstadoVisual.Idle)
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

    Private Sub ControlComedor_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
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
        If PermitirCierreOperador Then
            Me.Close()
        End If
    End Sub

    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtCedula.KeyDown
        If e.KeyCode <> Keys.Enter Then
            Exit Sub
        End If

        e.SuppressKeyPress = True
        ProcesarLecturaCarnet(TxtCedula.Text)
    End Sub

    Private Sub lblProcesando_Click(sender As Object, e As EventArgs) Handles lblProcesando.Click
        EnsureScanFocus(False)
    End Sub

    Private Sub ProcesarLecturaCarnet(ByVal carnetRaw As String)
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
            UpdateVisualState(EstadoVisual.Duplicate)
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
            UpdateVisualState(EstadoVisual.NoTickets)
        ElseIf warningSinMarcaTransporte Then
            UpdateVisualState(EstadoVisual.NoTransportMark)
        ElseIf warningMarcaTardia Then
            UpdateVisualState(EstadoVisual.LateTransportMark)
        ElseIf EstadoVerificado Then
            UpdateVisualState(EstadoVisual.Success)
        Else
            UpdateVisualState(EstadoVisual.NotFound)
        End If

        EnsureScanFocus(True)
    End Sub

    Private Sub RegistrarMarca(ByVal usuario As DataRow)
        Try
            LblCedula.Text = CStr(usuario!Cedula)
            TxtUsuario.Text = String.Format("{0} {1} {2}", CStr(usuario!Nombre), CStr(usuario!PrimerApellido), CStr(usuario!SegundoApellido)).Trim()

            If EsEstudiante(usuario) Then
                TxtTipo.Text = "ESTUDIANTE"
            Else
                TxtTipo.Text = "PROFESOR"
            End If

            Dim resultado As ComedorDataService.MarcaComedorResultado = ComedorSvc.RegistrarMarca(usuario, DsBeca, DiaSemana, Cn)
            TxtTiquetes.Text = resultado.TextoTiquetes
            ErrorTiquetes = resultado.ErrorTiquetes

            If resultado.RegistroGuardado Then
                UltimoCarnetProcesado = CStr(usuario!Cedula)
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
            If String.Equals(CStr(row!Cedula), carnet, StringComparison.OrdinalIgnoreCase) Then
                Return row
            End If
        Next

        Return Nothing
    End Function

    Private Function EsEstudiante(ByVal usuario As DataRow) As Boolean
        Return CShort(usuario!CodTipo) = 1S
    End Function

    Private Function TieneAdvertenciaSinMarcaTransporte(ByVal usuario As DataRow) As Boolean
        If IsDBNull(usuario!MarcaTransporte) Then
            Return True
        End If

        Return CInt(usuario!MarcaTransporte) = 0
    End Function

    Private Function TieneAdvertenciaMarcaTardia(ByVal usuario As DataRow) As Boolean
        If DsHorarios Is Nothing OrElse DsHorarios.Tables.Count = 0 Then
            Return False
        End If

        If IsDBNull(usuario!IdHorario) OrElse IsDBNull(usuario!HoraMarca) Then
            Return False
        End If

        Dim horarios() As DataRow = DsHorarios.Tables(0).Select(String.Format("IdHorario = '{0}'", usuario!IdHorario))
        If horarios Is Nothing OrElse horarios.Length = 0 Then
            Return False
        End If

        Dim horaLimite As TimeSpan = CType(horarios(0).ItemArray(1), TimeSpan)
        Dim horaMarca As TimeSpan = CType(usuario!HoraMarca, Date).TimeOfDay
        Return horaMarca > horaLimite
    End Function

    Private Sub ResetResultFields()
        LblCedula.Clear()
        TxtUsuario.Clear()
        TxtTipo.Clear()
        TxtTiquetes.Clear()
        Picture.Image = My.Resources.LogoIcon
    End Sub

    Private Sub EnsureScanFocus(ByVal selectAll As Boolean)
        If Not TxtCedula.CanFocus Then
            Exit Sub
        End If

        TxtCedula.Focus()
        If selectAll Then
            TxtCedula.SelectAll()
        End If
    End Sub

    Private Sub ApplyModernOperationalLayout()
        Me.BackColor = UIConstants.AppBackground
        Me.Font = UIConstants.FontBody()
        Me.KeyPreview = True

        PanelResult.BackColor = Color.FromArgb(243, 246, 251)
        PanelMainStatus.BackColor = Color.White

        LblTitulo.Font = New Font("Segoe UI Semibold", 34.0!, FontStyle.Bold)
        LblTitulo.ForeColor = Color.FromArgb(17, 33, 59)

        lblProcesando.Font = New Font("Segoe UI Semibold", 30.0!, FontStyle.Bold)
        lblProcesando.ForeColor = Color.FromArgb(23, 32, 51)

        LblFecha.ForeColor = Color.White
        LblScanHint.ForeColor = Color.White

        TxtCedula.Font = New Font("Segoe UI Semibold", 22.0!, FontStyle.Bold)
        TxtCedula.BorderStyle = BorderStyle.FixedSingle

        TxtUsuario.Font = New Font("Segoe UI", 15.0!, FontStyle.Bold)
        TxtTipo.Font = New Font("Segoe UI", 16.0!, FontStyle.Bold)
        TxtTiquetes.Font = New Font("Segoe UI", 28.0!, FontStyle.Bold)

        BtnSalir.Visible = PermitirCierreOperador
        BtnSalir.FlatStyle = FlatStyle.Flat
        BtnSalir.FlatAppearance.BorderSize = 0
        BtnSalir.BackColor = UIConstants.Danger
        BtnSalir.ForeColor = Color.White

        ResetResultFields()
    End Sub

    Private Sub UpdateVisualState(ByVal state As EstadoVisual)
        Select Case state
            Case EstadoVisual.Idle
                lblProcesando.Text = "Esperando lectura de carnet"
                lblProcesando.ForeColor = Color.FromArgb(23, 32, 51)
                LblTitulo.ForeColor = Color.FromArgb(17, 33, 59)
                Imgprocess.Image = My.Resources.Ingreso
                PanelMainStatus.BackColor = Color.White
                PanelResult.BackColor = Color.FromArgb(243, 246, 251)
            Case EstadoVisual.Processing
                lblProcesando.Text = "Procesando lectura..."
                lblProcesando.ForeColor = Color.FromArgb(17, 33, 59)
                LblTitulo.ForeColor = Color.FromArgb(17, 33, 59)
                Imgprocess.Image = My.Resources.Gif_cargando
                PanelMainStatus.BackColor = Color.FromArgb(239, 245, 255)
                PanelResult.BackColor = Color.FromArgb(234, 241, 252)
            Case EstadoVisual.Success
                lblProcesando.Text = "Entrada registrada correctamente"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                Imgprocess.Image = My.Resources.Verificado2
                PanelMainStatus.BackColor = UIConstants.Success
                PanelResult.BackColor = UIConstants.Success
            Case EstadoVisual.NotFound
                lblProcesando.Text = "Carnet no válido o no encontrado"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                Imgprocess.Image = My.Resources.Error2
                PanelMainStatus.BackColor = UIConstants.Danger
                PanelResult.BackColor = UIConstants.Danger
                ResetResultFields()
            Case EstadoVisual.Duplicate
                lblProcesando.Text = "Lectura duplicada detectada"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                Imgprocess.Image = My.Resources.Double_check
                PanelMainStatus.BackColor = UIConstants.Accent
                PanelResult.BackColor = UIConstants.Accent
            Case EstadoVisual.NoTickets
                lblProcesando.Text = "Sin tiquetes disponibles"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                Imgprocess.Image = My.Resources.Error2
                PanelMainStatus.BackColor = UIConstants.Warning
                PanelResult.BackColor = UIConstants.Warning
            Case EstadoVisual.NoTransportMark
                lblProcesando.Text = "Sin marca de transporte: acceso permitido con advertencia"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                Imgprocess.Image = My.Resources.Info
                PanelMainStatus.BackColor = UIConstants.Warning
                PanelResult.BackColor = UIConstants.Warning
            Case EstadoVisual.LateTransportMark
                lblProcesando.Text = "Marca tardía en transporte: acceso permitido con advertencia"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                Imgprocess.Image = My.Resources.Info
                PanelMainStatus.BackColor = UIConstants.Warning
                PanelResult.BackColor = UIConstants.Warning
            Case EstadoVisual.DeniedByRule
                lblProcesando.Text = "Lectura denegada por política operativa"
                lblProcesando.ForeColor = Color.White
                LblTitulo.ForeColor = Color.White
                Imgprocess.Image = My.Resources.Error2
                PanelMainStatus.BackColor = UIConstants.Danger
                PanelResult.BackColor = UIConstants.Danger
        End Select
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If Not PermitirCierreOperador Then
            If keyData = Keys.Escape OrElse keyData = (Keys.Alt Or Keys.F4) Then
                EnsureScanFocus(False)
                Return True
            End If
        End If

        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function
End Class
