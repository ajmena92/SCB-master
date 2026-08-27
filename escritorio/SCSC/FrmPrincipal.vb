Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms

Partial Public Class FrmPrincipal
    Private ReadOnly UseModernShell As Boolean
    Private Const DashboardRefreshIntervalMs As Integer = 60000
    Private _shellHost As UIShellHost
    Private ReadOnly _dashboardService As DashboardDataService
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
    Private ReadOnly _securityService As New SeguridadRbacService()
    Private _currentAccessContext As SeguridadRbacService.UserAccessContext
    Private Shared ReadOnly EsCulture As CultureInfo = CultureInfo.GetCultureInfo("es-CR")

    Public Sub New()
        MyBase.New()
        InitializeComponent()

        _dashboardService = New DashboardDataService()
        If System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime Then
            UseModernShell = False
        Else
            UseModernShell = ObtenerFlag("UseModernShell", False)
        End If
    End Sub

    Private Sub FrmPrincipal_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime Then
            Exit Sub
        End If

        SetControlVisibleSafe(PanelDesignSurface, False)

        Try
            Dim appIcon As Icon = CreateApplicationIcon()
            If appIcon IsNot Nothing Then
                Me.Icon = appIcon
            End If

            If DeploymentBootstrapper.ShouldRunSetupOnly() Then
                Me.Hide()
                Using setupForm As New FrmDeploymentSetup(True)
                    setupForm.ShowDialog(Me)
                End Using
                Me.Close()
                Exit Sub
            End If

            SincronizarFechaServidor()
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

            InicializarAccesoSeguridad()
            Me.MinimumSize = New Size(1280, 760)
            ErrorLogger.LogInfo("FrmPrincipal_Load", "Configurando shell base. UseModernShell=" & UseModernShell.ToString())
            PrepararShellInicial()
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
        If Me.IsDisposed OrElse Not Me.IsHandleCreated OrElse _shellHost Is Nothing Then
            Exit Sub
        End If

        Try
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
        NavigateToModule("estudiantes")
    End Sub

    Private Sub ControlDeMarcasToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ControlDeMarcasToolStripMenuItem.Click
        NavigateToModule("comedor")
    End Sub

    Private Sub AyudaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AyudaToolStripMenuItem.Click
        NavigateToModule("ayuda")
    End Sub

    Private Sub ImportarDatosPIADToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImportarDatosPIADToolStripMenuItem.Click
        NavigateToModule("importacion")
    End Sub

    Private Sub ReporteDiariosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteDiariosToolStripMenuItem.Click
        NavigateToModule("reporte_comedor")
    End Sub

    Private Sub ImprimirToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImprimirToolStripMenuItem.Click
        NavigateToModule("imprimir")
    End Sub

    Private Sub UtilitariosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UtilitariosToolStripMenuItem.Click
    End Sub

    Private Sub ImportarDatosListaPIADToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImportarDatosListaPIADToolStripMenuItem.Click
        NavigateToModule("transporte")
    End Sub

    Private Sub BtnCerrar_Click(sender As Object, e As EventArgs) Handles BtnCerrar.Click
        Me.Dispose()
    End Sub

    Private Sub ReporteDeServicioTransporteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteDeServicioTransporteToolStripMenuItem.Click
        NavigateToModule("reporte_transporte")
    End Sub

    Private Sub RecargasToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles RecargasToolStripMenuItem1.Click
        NavigateToModule("recargas")
    End Sub

    Private Sub GestiónRutasToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GestiónRutasToolStripMenuItem.Click
        NavigateToModule("rutas")
    End Sub

    Private Sub GestiónBecasToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GestiónBecasToolStripMenuItem.Click
        NavigateToModule("becas")
    End Sub

    Private Sub ReporteProyecciónComedorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteProyecciónComedorToolStripMenuItem.Click
        NavigateToModule("reporte_proyeccion")
    End Sub

    Private Sub ReporteEstudiantesBecadosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReporteEstudiantesBecadosToolStripMenuItem.Click
        NavigateToModule("reporte_becados")
    End Sub

    Private Sub AgregarEstudianteManualToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AgregarEstudianteManualToolStripMenuItem.Click
        NavigateToModule("agregar_estudiante")
    End Sub

    Private Sub ParametrosSistemaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ParametrosSistemaToolStripMenuItem.Click
        NavigateToModule("parametros")
    End Sub

    Private Sub MostrarDialogo(ByVal form As Form)
        UIThemeManagerV2.Apply(form, "dialogo")
        form.ShowDialog(Me)
        RefreshDashboardDeferred()
    End Sub

    Private Sub MostrarDialogoNuevo(Of T As {Form, New})()
        Using form As New T()
            UIThemeManagerV2.Apply(form, "dialogo")
            form.ShowDialog(Me)
        End Using
        RefreshDashboardDeferred()
    End Sub

    Private Sub SincronizarFechaServidor()
        Dim cls As New FuncionesDB()
        ServerClock.Sync(cls.FechaServer())
    End Sub

    Private Sub ShowLegacyShell()
        SetControlVisibleSafe(MenuStrip1, True)
        SetDockSafe(MenuStrip1, DockStyle.Top)
        Try
            Me.MainMenuStrip = MenuStrip1
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal.ShowLegacyShell.MainMenuStrip", ex)
        End Try
        SetControlVisibleSafe(PanelMenuLateral, True)
        SetDockSafe(PanelMenuLateral, DockStyle.Left)
        SetControlVisibleSafe(PanelCabeceraModulo, True)
        SetDockSafe(PanelCabeceraModulo, DockStyle.Top)
        SetControlVisibleSafe(PanelDesignSurface, False)
        SetControlVisibleSafe(BtnCerrar, True)
        ApplyLegacyMenuPermissions()
    End Sub

    Private Function BuildModernShell() As Boolean
        If _shellHost IsNot Nothing Then
            Return True
        End If

        Try
            If Me.IsDisposed Then
                Return False
            End If
            If Me.Controls Is Nothing Then
                Return False
            End If
            _shellHost = New UIShellHost(Me, AddressOf NavigateToModule, AddressOf CanAccessModuleSilently)
            _shellHost.Build()

            SetControlVisibleSafe(MenuStrip1, False)
            Try
                Me.MainMenuStrip = Nothing
            Catch ex As Exception
                ErrorLogger.LogException("FrmPrincipal.BuildModernShell.MainMenuStrip", ex)
            End Try
            SetControlVisibleSafe(PanelMenuLateral, False)
            SetControlVisibleSafe(PanelCabeceraModulo, False)
            SetControlVisibleSafe(PanelDesignSurface, False)
            SetControlVisibleSafe(BtnCerrar, True)
            BtnCerrar.BringToFront()
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

    Private Sub PrepararShellInicial()
        If UseModernShell Then
            ErrorLogger.LogInfo("FrmPrincipal.PrepararShellInicial", "Intentando shell moderno como shell principal.")
            If BuildModernShell() Then
                Exit Sub
            End If

            ErrorLogger.LogInfo("FrmPrincipal.PrepararShellInicial", "Fallo shell moderno. Se aplica fallback al shell clasico.")
        End If

        ShowLegacyShell()
    End Sub

    Private Sub FrmPrincipal_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If BtnCerrar IsNot Nothing AndAlso Not BtnCerrar.IsDisposed Then
            BtnCerrar.BringToFront()
        End If
    End Sub

    Private Sub ActivarFallbackShellClasico(ByVal motivo As String)
        Try
            ErrorLogger.LogInfo("FrmPrincipal.ActivarFallbackShellClasico", motivo)
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal.ActivarFallbackShellClasico", ex)
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
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal.DesmontarShellModerno.ContentHost", ex)
        End Try

        Try
            Dim sidebarModerno As Control = Me.Controls("ModernSidebar")
            If sidebarModerno IsNot Nothing Then
                Me.Controls.Remove(sidebarModerno)
                sidebarModerno.Dispose()
            End If
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal.DesmontarShellModerno.Sidebar", ex)
        End Try

        Try
            Dim topbarModerna As Control = Me.Controls("ModernTopBar")
            If topbarModerna IsNot Nothing Then
                Me.Controls.Remove(topbarModerna)
                topbarModerna.Dispose()
            End If
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal.DesmontarShellModerno.TopBar", ex)
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
        Return GetAppSettingBoolean(key, defaultValue)
    End Function

    Private Sub NavigateToModule(ByVal moduleKey As String)
        If Not CanAccessModule(moduleKey, True) Then
            Exit Sub
        End If

        Select Case moduleKey
            Case "estudiantes"
                MostrarDialogoNuevo(Of FrmEstudiantes)()
            Case "comedor"
                MostrarDialogoNuevo(Of ControlComedor)()
            Case "transporte"
                MostrarDialogoNuevo(Of ControlTransporte)()
            Case "importacion"
                MostrarDialogoNuevo(Of FrmImportarExcel)()
            Case "recargas"
                MostrarDialogoNuevo(Of FrmRecarga)()
            Case "reporte_comedor"
                MostrarDialogoNuevo(Of FrmReporteComedor)()
            Case "reporte_transporte"
                MostrarDialogoNuevo(Of FrmReporteRutas)()
            Case "reporte_proyeccion"
                MostrarDialogoNuevo(Of FrmProyeccionComedor)()
            Case "reporte_becados"
                MostrarDialogoNuevo(Of FrmBecados)()
            Case "rutas"
                MostrarDialogoNuevo(Of FrmRutas)()
            Case "becas"
                MostrarDialogoNuevo(Of FrmBecas)()
            Case "seguridad"
                MostrarDialogo(New FrmSeguridadRBAC())
            Case "parametros"
                MostrarDialogo(New FrmParametrosSistema())
            Case "agregar_estudiante"
                MostrarDialogoNuevo(Of FrmAgregarEstudiante)()
            Case "ayuda"
                MostrarDialogoNuevo(Of FrmAyuda)()
            Case "imprimir"
                MostrarDialogoNuevo(Of IMPRIMIR)()
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

        Dim snapshot As DashboardContracts.Snapshot
        Try
            snapshot = _dashboardService.CargarSnapshot(ServerClock.Today())
            _lastDashboardRefreshSuccess = True
            _lastDashboardRefreshMessage = "comedor=" & snapshot.MarcasComedorHoy.ToString("N0") &
                ", transporte=" & snapshot.MarcasTransporteHoy.ToString("N0") &
                ", con_ruta=" & snapshot.EstudiantesConRutaHoy.ToString("N0")
            _lastDashboardRefreshSuccessAt = ServerClock.Now()
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal.LoadDashboardSnapshot", ex)
            snapshot = New DashboardContracts.Snapshot()
            Dim hoyLectivo As Date = ResolveCurrentSchoolDay(FechaServer.Date)
            snapshot.BecadosComedorHoy = 0
            snapshot.EstudiantesConRutaHoy = 0
            snapshot.EstudiantesConRutaAyer = 0
            snapshot.MarcasComedorHoy = 0
            snapshot.MarcasTransporteHoy = 0
            snapshot.MarcasComedorAyer = 0
            snapshot.MarcasTransporteAyer = 0
            snapshot.Series = New List(Of DashboardContracts.DailyMetric)()
            snapshot.Comparativos = New List(Of DashboardContracts.DashboardComparisonItem)()
            snapshot.PeriodKind = DashboardContracts.DashboardPeriodKind.SchoolDays
            snapshot.PeriodLabel = "Ultimos 7 dias lectivos"
            snapshot.ComparisonLabel = "Comparativo hoy vs dia lectivo previo"
            For Each day As Date In BuildSchoolDayRange(hoyLectivo, 7)
                snapshot.Series.Add(New DashboardContracts.DailyMetric With {
                    .MetricDate = day,
                    .DayNameShort = FormatShortDayName(day),
                    .Label = BuildMetricLabel(day),
                    .Comedor = 0,
                    .ComedorBecados = 0,
                    .Transporte = 0,
                    .TransporteConRuta = 0
                })
            Next
            snapshot.Comparativos.Add(New DashboardContracts.DashboardComparisonItem With {
                .Label = "Comedor",
                .CurrentValue = 0,
                .PreviousValue = 0,
                .DeltaText = "sin cambio (0/0)",
                .StatusText = "SIN CAMBIO",
                .Trend = DashboardContracts.ComparisonTrend.Flat
            })
            snapshot.Comparativos.Add(New DashboardContracts.DashboardComparisonItem With {
                .Label = "Transporte",
                .CurrentValue = 0,
                .PreviousValue = 0,
                .DeltaText = "sin cambio (0/0)",
                .StatusText = "SIN CAMBIO",
                .Trend = DashboardContracts.ComparisonTrend.Flat
            })
            snapshot.Comparativos.Add(New DashboardContracts.DashboardComparisonItem With {
                .Label = "Con ruta",
                .CurrentValue = 0,
                .PreviousValue = 0,
                .DeltaText = "sin cambio (0/0)",
                .StatusText = "SIN CAMBIO",
                .Trend = DashboardContracts.ComparisonTrend.Flat
            })
            snapshot.Alertas = New List(Of DashboardContracts.DashboardAlert)()
            snapshot.Alertas.Add(New DashboardContracts.DashboardAlert With {
                .Code = "dashboard.safe_mode",
                .Title = "Dashboard",
                .Message = "Dashboard en modo seguro por error de datos.",
                .Severity = DashboardContracts.AlertSeverity.Critical,
                .SortOrder = 0
            })
            snapshot.TopRutas = New List(Of String)()
            _lastDashboardRefreshSuccess = False
            _lastDashboardRefreshMessage = ex.Message
        End Try

        _shellHost.BindDashboard(snapshot)
        _shellHost.SetDashboardLastUpdate(ServerClock.Now())
        If _lastDashboardRefreshSuccess AndAlso _lastDashboardRefreshSuccessAt <> Date.MinValue Then
            _shellHost.SetDashboardLastSuccessUpdate(_lastDashboardRefreshSuccessAt)
        End If
    End Sub

    Private Function BuildSchoolDayRange(ByVal referenceDay As Date, ByVal dayCount As Integer) As List(Of Date)
        Dim result As New List(Of Date)()
        Dim current As Date = ResolveCurrentSchoolDay(referenceDay)

        While result.Count < Math.Max(1, dayCount)
            If IsSchoolDay(current) Then
                result.Add(current)
            End If
            current = current.AddDays(-1)
        End While

        result.Reverse()
        Return result
    End Function

    Private Function ResolveCurrentSchoolDay(ByVal referenceDay As Date) As Date
        Dim current As Date = referenceDay.Date
        While Not IsSchoolDay(current)
            current = current.AddDays(-1)
        End While
        Return current
    End Function

    Private Function IsSchoolDay(ByVal value As Date) As Boolean
        Return value.DayOfWeek <> DayOfWeek.Saturday AndAlso value.DayOfWeek <> DayOfWeek.Sunday
    End Function

    Private Function BuildMetricLabel(ByVal value As Date) As String
        Return FormatShortDayName(value) & " " & value.ToString("dd/MM")
    End Function

    Private Function FormatShortDayName(ByVal value As Date) As String
        Dim dayName As String = EsCulture.DateTimeFormat.GetAbbreviatedDayName(value.DayOfWeek)
        If String.IsNullOrWhiteSpace(dayName) Then
            Return value.ToString("ddd", EsCulture)
        End If

        dayName = dayName.Trim().TrimEnd("."c)
        If dayName.Length = 0 Then
            Return value.ToString("ddd", EsCulture)
        End If

        Return Char.ToUpperInvariant(dayName(0)) & dayName.Substring(1).ToLowerInvariant()
    End Function

    Private Sub InicializarAccesoSeguridad()
        _securityService.EnsurePermissionCatalog()
        _currentAccessContext = _securityService.GetUserAccessContext(CodigoUsuario)

        If _currentAccessContext Is Nothing Then
            _currentAccessContext = New SeguridadRbacService.UserAccessContext()
            _currentAccessContext.NombreUsuario = CodigoUsuario
            _currentAccessContext.NombreCompleto = CodigoUsuario
        End If

        If String.IsNullOrWhiteSpace(NombreUsuario) Then
            If Not String.IsNullOrWhiteSpace(_currentAccessContext.NombreCompleto) Then
                NombreUsuario = _currentAccessContext.NombreCompleto
            Else
                NombreUsuario = CodigoUsuario
            End If
        End If
    End Sub

    Private Function CanAccessModuleSilently(ByVal moduleKey As String) As Boolean
        Return CanAccessModule(moduleKey, False)
    End Function

    Private Function CanAccessModule(ByVal moduleKey As String, Optional ByVal showDeniedMessage As Boolean = False) As Boolean
        Dim normalizedKey As String = If(moduleKey, String.Empty).Trim().ToLowerInvariant()
        If normalizedKey.Length = 0 Then
            Return False
        End If

        Dim isAllowed As Boolean
        Select Case normalizedKey
            Case "dashboard_refresh", "dashboard_interval_30", "dashboard_interval_60", "dashboard_interval_120", "dashboard_toggle_autorefresh", "ayuda"
                isAllowed = True
            Case Else
                If SeguridadPermisosSistema.IsModuleAccessibleWithoutLogin(normalizedKey) Then
                    isAllowed = True
                ElseIf normalizedKey = "parametros" Then
                    isAllowed = SeguridadPermisosSistema.IsUsuarioAutorizadoParaParametros(CodigoUsuario)
                Else
                    Dim requiredPermissions As String() = SeguridadPermisosSistema.GetModulePermissionKeys(normalizedKey)
                    isAllowed = _currentAccessContext IsNot Nothing AndAlso _currentAccessContext.TienePermiso(requiredPermissions)
                End If
        End Select

        If Not isAllowed AndAlso showDeniedMessage Then
            If normalizedKey = "parametros" Then
                MsgBox("Solo admin y amenaa pueden ingresar a Parametros del Sistema.", MsgBoxStyle.Exclamation)
            Else
                MsgBox("No tiene permisos para acceder a esta opcion.", MsgBoxStyle.Exclamation)
            End If
        End If

        Return isAllowed
    End Function

    Private Sub ApplyLegacyMenuPermissions()
        If MenuStrip1 Is Nothing Then
            Exit Sub
        End If

        UsuariosToolStripMenuItem.Visible = CanAccessModule("estudiantes")
        GestiónRutasToolStripMenuItem.Visible = CanAccessModule("rutas")
        GestiónBecasToolStripMenuItem.Visible = CanAccessModule("becas")
        ParametrosSistemaToolStripMenuItem.Visible = CanAccessModule("parametros")

        ControlDeMarcasToolStripMenuItem.Visible = CanAccessModule("comedor")
        ImportarDatosListaPIADToolStripMenuItem.Visible = CanAccessModule("transporte")
        ImportarDatosPIADToolStripMenuItem.Visible = CanAccessModule("importacion")
        AgregarEstudianteManualToolStripMenuItem.Visible = CanAccessModule("agregar_estudiante")
        RecargasToolStripMenuItem1.Visible = CanAccessModule("recargas")

        ReporteEstudiantesBecadosToolStripMenuItem.Visible = CanAccessModule("reporte_becados")
        ReporteDiariosToolStripMenuItem.Visible = CanAccessModule("reporte_comedor")
        ReporteDeServicioTransporteToolStripMenuItem.Visible = CanAccessModule("reporte_transporte")
        ReporteProyecciónComedorToolStripMenuItem.Visible = CanAccessModule("reporte_proyeccion")

        AyudaToolStripMenuItem.Visible = CanAccessModule("ayuda")
        ImprimirToolStripMenuItem.Visible = CanAccessModule("imprimir")

        MantenimientoToolStripMenuItem.Visible = HasVisibleDropDownItems(MantenimientoToolStripMenuItem)
        UtilitariosToolStripMenuItem.Visible = HasVisibleDropDownItems(UtilitariosToolStripMenuItem)
        ReportesToolStripMenuItem.Visible = HasVisibleDropDownItems(ReportesToolStripMenuItem)
    End Sub

    Private Function HasVisibleDropDownItems(ByVal menuItem As ToolStripMenuItem) As Boolean
        If menuItem Is Nothing Then
            Return False
        End If

        For Each item As ToolStripItem In menuItem.DropDownItems
            If item.Visible Then
                Return True
            End If
        Next

        Return False
    End Function

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
