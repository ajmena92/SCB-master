Imports System.Collections.Generic
Imports System.Drawing
Imports System.Configuration

Partial Class FrmPrincipal
    Private ReadOnly UseModernShell As Boolean = ObtenerFlag("UseModernShell", False)
    Private Const DashboardRefreshIntervalMs As Integer = 60000
    Private _shellHost As UIShellHost
    Private ReadOnly _dashboardService As New DashboardDataService()
    Private _dashboardRefreshTimer As System.Windows.Forms.Timer
    Private _dashboardCountdownTimer As System.Windows.Forms.Timer
    Private _dashboardRefreshInProgress As Boolean
    Private _dashboardRefreshPending As Boolean
    Private _lastDashboardRefreshSuccess As Boolean = False
    Private _lastDashboardRefreshMessage As String = "pendiente"
    Private _lastDashboardRefreshSuccessAt As Date = Date.MinValue
    Private _dashboardRefreshIntervalMs As Integer = DashboardRefreshIntervalMs
    Private _dashboardAutoRefreshEnabled As Boolean = True
    Private _nextDashboardRefreshAt As DateTime = Date.MinValue

    Private Sub FrmPrincipal_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime Then
            Exit Sub
        End If

        Try
            FechaServer = Date.Now()
            Me.Hide()
            ErrorLogger.LogInfo("FrmPrincipal_Load", "Iniciando flujo de autenticacion.")

            Dim loginResult As DialogResult
            Using frmLogin As New Login()
                UIThemeManagerV2.Apply(frmLogin, "login")
                loginResult = frmLogin.ShowDialog(Me)
            End Using

            ErrorLogger.LogInfo("FrmPrincipal_Load", "Resultado Login=" & loginResult.ToString())

            If loginResult <> DialogResult.OK Then
                ErrorLogger.LogInfo("FrmPrincipal_Load", "Cierre de aplicacion por login cancelado/no valido.")
                Me.Close()
                Exit Sub
            End If

            Try
                ErrorLogger.LogInfo("FrmPrincipal_Load", "Aplicando tema shell.")
                UIThemeManagerV2.Apply(Me, "shell")
            Catch ex As Exception
                ErrorLogger.LogException("FrmPrincipal_Load.ApplyShellTheme", ex)
            End Try
            Me.MinimumSize = New Size(1280, 760)
            ErrorLogger.LogInfo("FrmPrincipal_Load", "Configurando shell base. UseModernShell=" & UseModernShell.ToString())
            ShowLegacyShell()
            ErrorLogger.LogInfo("FrmPrincipal_Load", "Mostrando formulario principal.")
            Me.Show()
            If UseModernShell Then
                Me.BeginInvoke(New Action(AddressOf InicializarShellModernoDiferido))
            End If
            ErrorLogger.LogInfo("FrmPrincipal_Load", "Shell principal cargada correctamente.")
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal_Load", ex)
            MsgBox("Error al iniciar la aplicacion. Revise el log: " & ErrorLogger.GetCurrentLogPath(), MsgBoxStyle.Critical)
            Me.Close()
        End Try
    End Sub

    Private Sub InicializarShellModernoDiferido()
        If Me.IsDisposed OrElse Not Me.IsHandleCreated Then
            Exit Sub
        End If

        Try
            ErrorLogger.LogInfo("FrmPrincipal.InicializarShellModernoDiferido", "Iniciando construccion shell moderno.")
            If Not BuildModernShell() Then
                ErrorLogger.LogInfo("FrmPrincipal.InicializarShellModernoDiferido", "BuildModernShell retorno False. Se mantiene shell clasico.")
                Exit Sub
            End If

            If _shellHost Is Nothing Then
                ErrorLogger.LogInfo("FrmPrincipal.InicializarShellModernoDiferido", "_shellHost es Nothing tras Build. Se mantiene shell clasico.")
                Exit Sub
            End If

            _shellHost.SetDashboardAutoRefreshEnabled(_dashboardAutoRefreshEnabled)
            _shellHost.SetDashboardRefreshIntervalSeconds(_dashboardRefreshIntervalMs \ 1000)
            _shellHost.SetDashboardRefreshQueueCount(0)
            EnsureDashboardCountdownTimer()
            Me.BeginInvoke(New Action(AddressOf RefreshDashboardDeferred))
            EnsureDashboardRefreshTimer()
            ErrorLogger.LogInfo("FrmPrincipal.InicializarShellModernoDiferido", "Shell moderno inicializado correctamente.")
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal.InicializarShellModernoDiferido", ex)
            ActivarFallbackShellClasico("No se pudo inicializar el shell moderno en modo diferido.")
        End Try
    End Sub

    Private Sub UsuariosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UsuariosToolStripMenuItem.Click
        MostrarDialogo(FrmEstudiantes)
    End Sub

    Private Sub ControlDeMarcasToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ControlDeMarcasToolStripMenuItem.Click
        MostrarDialogo(ControlComedor)
    End Sub

    Private Sub AyudaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AyudaToolStripMenuItem.Click
        MostrarDialogo(FrmAyuda)
    End Sub

    'Private Sub ReporteDeEstudiantesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteDeEstudiantesToolStripMenuItem.Click
    '    ReporteEstudiantes.ShowDialog()
    'End Sub

    'Private Sub ReporteDiariosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteDiariosToolStripMenuItem.Click
    '    RpTransacciones.ShowDialog()
    'End Sub


    'Private Sub ContadorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ContadorToolStripMenuItem.Click
    '    FrmReport.ShowDialog()
    'End Sub

    Private Sub ImportarDatosPIADToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImportarDatosPIADToolStripMenuItem.Click
        MostrarDialogo(FrmImportarExcel)
    End Sub

    Private Sub ReporteDiariosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteDiariosToolStripMenuItem.Click
        MostrarDialogo(FrmReporteComedor)
    End Sub

    Private Sub ImprimirToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImprimirToolStripMenuItem.Click
        MostrarDialogo(IMPRIMIR)

    End Sub

    Private Sub UtilitariosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UtilitariosToolStripMenuItem.Click

    End Sub

    Private Sub ImportarDatosListaPIADToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImportarDatosListaPIADToolStripMenuItem.Click
        MostrarDialogo(ControlTransporte)

    End Sub

    Private Sub BtnCerrar_Click(sender As Object, e As EventArgs) Handles BtnCerrar.Click
        Me.Dispose()
    End Sub

    Private Sub ReporteDeServicioTransporteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteDeServicioTransporteToolStripMenuItem.Click
        MostrarDialogo(FrmReporteRutas)
    End Sub

    Private Sub RecargasToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles RecargasToolStripMenuItem1.Click
        MostrarDialogo(FrmRecarga)
    End Sub

    Private Sub GestiónRutasToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GestiónRutasToolStripMenuItem.Click
        MostrarDialogo(FrmRutas)
    End Sub

    Private Sub GestiónBecasToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GestiónBecasToolStripMenuItem.Click
        MostrarDialogo(FrmBecas)
    End Sub

    Private Sub ReporteProyecciónComedorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteProyecciónComedorToolStripMenuItem.Click
        MostrarDialogo(FrmProyeccionComedor)
    End Sub

    Private Sub ReporteEstudiantesBecadosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteEstudiantesBecadosToolStripMenuItem.Click
        MostrarDialogo(FrmBecados)
    End Sub

    Private Sub AgregarEstudianteManualToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AgregarEstudianteManualToolStripMenuItem.Click
        MostrarDialogo(FrmAgregarEstudiante)
    End Sub

    Private Sub ParametrosSistemaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ParametrosSistemaToolStripMenuItem.Click
        MostrarDialogo(New FrmParametrosSistema())
    End Sub

    Private Sub MostrarDialogo(ByVal form As Form)
        UIThemeManagerV2.Apply(form, "dialogo")
        form.ShowDialog()
        RefreshDashboardDeferred()
    End Sub

    Private Sub ShowLegacyShell()
        SetControlVisibleSafe(MenuStrip1, True)
        SetDockSafe(MenuStrip1, DockStyle.Top)
        Try
            Me.MainMenuStrip = MenuStrip1
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal.ShowLegacyShell.MainMenuStrip", ex)
        End Try
        SetControlVisibleSafe(Panel1, True)
        SetDockSafe(Panel1, DockStyle.Left)
        SetControlVisibleSafe(Panel2, True)
        SetDockSafe(Panel2, DockStyle.Top)
        SetControlVisibleSafe(BtnCerrar, True)
    End Sub

    Private Function BuildModernShell() As Boolean
        If Not _shellHost Is Nothing Then
            Return True
        End If

        Try
            If Me.IsDisposed Then
                Return False
            End If
            _shellHost = New UIShellHost(Me, AddressOf NavigateToModule)
            _shellHost.Build()

            SetControlVisibleSafe(MenuStrip1, False)
            Try
                Me.MainMenuStrip = Nothing
            Catch ex As Exception
                ErrorLogger.LogException("FrmPrincipal.BuildModernShell.MainMenuStrip", ex)
            End Try
            SetControlVisibleSafe(Panel1, False)
            SetControlVisibleSafe(Panel2, False)
            SetControlVisibleSafe(BtnCerrar, False)
            Return True
        Catch ex As Exception
            Try
                ErrorLogger.LogException("FrmPrincipal.BuildModernShell", ex)
            Catch
            End Try
            Try
                DesmontarShellModerno()
            Catch
            End Try
            _shellHost = Nothing
            Return False
        End Try
    End Function

    Private Sub ActivarFallbackShellClasico(ByVal motivo As String)
        Try
            ErrorLogger.LogInfo("FrmPrincipal.ActivarFallbackShellClasico", motivo)
        Catch
        End Try
        DesmontarShellModerno()
        ShowLegacyShell()
    End Sub

    Private Sub DesmontarShellModerno()
        Try
            Dim panelModerno As Control = Me.Controls("ModernContentHost")
            If panelModerno IsNot Nothing Then
                Me.Controls.Remove(panelModerno)
                panelModerno.Dispose()
            End If
        Catch
        End Try

        Try
            Dim sidebarModerno As Control = Me.Controls("ModernSidebar")
            If sidebarModerno IsNot Nothing Then
                Me.Controls.Remove(sidebarModerno)
                sidebarModerno.Dispose()
            End If
        Catch
        End Try

        Try
            Dim topbarModerna As Control = Me.Controls("ModernTopBar")
            If topbarModerna IsNot Nothing Then
                Me.Controls.Remove(topbarModerna)
                topbarModerna.Dispose()
            End If
        Catch
        End Try

        _shellHost = Nothing
    End Sub

    Private Sub SetControlVisibleSafe(ByVal ctrl As Control, ByVal visible As Boolean)
        If ctrl Is Nothing Then
            Exit Sub
        End If

        Try
            If ctrl.IsDisposed Then
                Exit Sub
            End If
            ctrl.Visible = visible
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal.SetControlVisibleSafe", ex, "Control=" & ctrl.Name & ", Visible=" & visible.ToString())
        End Try
    End Sub

    Private Sub SetDockSafe(ByVal ctrl As Control, ByVal dock As DockStyle)
        If ctrl Is Nothing Then
            Exit Sub
        End If

        Try
            If ctrl.IsDisposed Then
                Exit Sub
            End If
            ctrl.Dock = dock
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal.SetDockSafe", ex, "Control=" & ctrl.Name & ", Dock=" & dock.ToString())
        End Try
    End Sub

    Private Shared Function ObtenerFlag(ByVal key As String, ByVal defaultValue As Boolean) As Boolean
        Try
            Dim raw As String = Convert.ToString(ConfigurationManager.AppSettings(key))
            If String.IsNullOrWhiteSpace(raw) Then
                Return defaultValue
            End If
            Dim value As Boolean
            If Boolean.TryParse(raw.Trim(), value) Then
                Return value
            End If
        Catch
        End Try
        Return defaultValue
    End Function

    Private Sub NavigateToModule(ByVal moduleKey As String)
        Select Case moduleKey
            Case "estudiantes"
                MostrarDialogo(FrmEstudiantes)
            Case "comedor"
                MostrarDialogo(ControlComedor)
            Case "transporte"
                MostrarDialogo(ControlTransporte)
            Case "importacion"
                MostrarDialogo(FrmImportarExcel)
            Case "recargas"
                MostrarDialogo(FrmRecarga)
            Case "reportes"
                MostrarDialogo(FrmReporteComedor)
            Case "rutas"
                MostrarDialogo(FrmRutas)
            Case "becas"
                MostrarDialogo(FrmBecas)
            Case "seguridad"
                MostrarDialogo(New FrmSeguridadRBAC())
            Case "parametros"
                MostrarDialogo(New FrmParametrosSistema())
            Case "ayuda"
                MostrarDialogo(FrmAyuda)
            Case "dashboard_refresh"
                RefreshDashboardDeferred()
            Case "dashboard_interval_30"
                UpdateDashboardRefreshInterval(30)
            Case "dashboard_interval_60"
                UpdateDashboardRefreshInterval(60)
            Case "dashboard_interval_120"
                UpdateDashboardRefreshInterval(120)
            Case "dashboard_toggle_autorefresh"
                ToggleDashboardAutoRefresh()
        End Select
    End Sub

    Private Sub LoadDashboardSnapshot()
        If _shellHost Is Nothing Then
            Exit Sub
        End If

        Dim snapshot As New UIShellHost.DashboardSnapshot()
        Try
            Dim raw As DashboardDataService.Snapshot = _dashboardService.CargarSnapshot(FechaServer.Date)
            snapshot.BecadosComedorHoy = raw.BecadosComedorHoy
            snapshot.BecadosTransporteHoy = raw.BecadosTransporteHoy
            snapshot.MarcasComedorHoy = raw.MarcasComedorHoy
            snapshot.MarcasTransporteHoy = raw.MarcasTransporteHoy
            snapshot.MarcasComedorAyer = raw.MarcasComedorAyer
            snapshot.MarcasTransporteAyer = raw.MarcasTransporteAyer
            snapshot.Alertas = If(raw.Alertas, New List(Of String)())
            snapshot.TopRutas = If(raw.TopRutas, New List(Of String)())
            snapshot.Series = New List(Of UIShellHost.DailyMetric)()
            If Not raw.Series Is Nothing Then
                For Each item As DashboardDataService.DailyMetric In raw.Series
                    snapshot.Series.Add(New UIShellHost.DailyMetric With {
                        .Label = item.Label,
                        .Comedor = item.Comedor,
                        .ComedorBecados = item.ComedorBecados,
                        .Transporte = item.Transporte
                    })
                Next
            End If
            _lastDashboardRefreshSuccess = True
            _lastDashboardRefreshMessage = "comedor=" & snapshot.MarcasComedorHoy.ToString("N0") & ", transporte=" & snapshot.MarcasTransporteHoy.ToString("N0")
            _lastDashboardRefreshSuccessAt = Date.Now()
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal.LoadDashboardSnapshot", ex)
            snapshot.BecadosComedorHoy = 0
            snapshot.BecadosTransporteHoy = 0
            snapshot.MarcasComedorHoy = 0
            snapshot.MarcasTransporteHoy = 0
            snapshot.MarcasComedorAyer = 0
            snapshot.MarcasTransporteAyer = 0
            snapshot.Series = New List(Of UIShellHost.DailyMetric)()
            For i As Integer = 6 To 0 Step -1
                Dim day As Date = FechaServer.Date.AddDays(-i)
                snapshot.Series.Add(New UIShellHost.DailyMetric With {
                    .Label = day.ToString("dd/MM"),
                    .Comedor = 0,
                    .ComedorBecados = 0,
                    .Transporte = 0
                })
            Next
            snapshot.Alertas = New List(Of String)()
            snapshot.Alertas.Add("- Dashboard en modo seguro por error de datos.")
            snapshot.TopRutas = New List(Of String)()
            _lastDashboardRefreshSuccess = False
            _lastDashboardRefreshMessage = ex.Message
        End Try

        _shellHost.BindDashboard(snapshot)
        _shellHost.SetDashboardLastUpdate(Date.Now())
        If _lastDashboardRefreshSuccess AndAlso _lastDashboardRefreshSuccessAt <> Date.MinValue Then
            _shellHost.SetDashboardLastSuccessUpdate(_lastDashboardRefreshSuccessAt)
        End If
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.F5 Then
            RefreshDashboardDeferred()
            Return True
        End If
        If keyData = (Keys.Control Or Keys.R) Then
            ToggleDashboardAutoRefresh()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function
End Class
