Imports System.Collections.Generic
Imports System.Drawing

Public Partial Class FrmPrincipal
    Private _shellHost As UIShellHost
    Private ReadOnly _dashboardService As New DashboardDataService()

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

            UIThemeManagerV2.Apply(Me, "shell")
            Me.MinimumSize = New Size(1280, 760)
            BuildModernShell()
            Me.Show()
            Me.BeginInvoke(New Action(AddressOf LoadDashboardSnapshot))
            ErrorLogger.LogInfo("FrmPrincipal_Load", "Shell principal cargada correctamente.")
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal_Load", ex)
            MsgBox("Error al iniciar la aplicacion. Revise el log: " & ErrorLogger.GetCurrentLogPath(), MsgBoxStyle.Critical)
            Me.Close()
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

    Private Sub MostrarDialogo(ByVal form As Form)
        UIThemeManagerV2.Apply(form, "dialogo")
        form.ShowDialog()
    End Sub

    Private Sub BuildModernShell()
        If Not _shellHost Is Nothing Then
            Exit Sub
        End If

        SuspendLayout()
        MenuStrip1.Visible = False
        MenuStrip1.Dock = DockStyle.None
        Me.MainMenuStrip = Nothing
        Panel1.Visible = False
        Panel1.Dock = DockStyle.None
        Panel2.Visible = False
        Panel2.Dock = DockStyle.None
        BtnCerrar.Visible = False
        _shellHost = New UIShellHost(Me, AddressOf NavigateToModule)
        _shellHost.Build()
        ResumeLayout(True)
    End Sub

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
            Case "ayuda"
                MostrarDialogo(FrmAyuda)
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
        End Try

        _shellHost.BindDashboard(snapshot)
    End Sub
End Class
