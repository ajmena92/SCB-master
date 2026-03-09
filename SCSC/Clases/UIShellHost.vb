Imports System.Windows.Forms
Imports System.Drawing
Imports System.Collections.Generic
Imports System.IO
Imports System.Runtime.InteropServices

Public Partial Class UIShellHost
    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function DestroyIcon(ByVal handle As IntPtr) As Boolean
    End Function

    Private ReadOnly _owner As Form
    Private ReadOnly _onNavigate As Action(Of String)
    Private _sidebar As Panel
    Private _topBar As Panel
    Private _titleLabel As Label
    Private _contextLabel As Label
    Private _lastSuccessBadge As Label
    Private _lastUpdateBadge As Label
    Private _refreshButton As Button
    Private _autoRefreshToggleButton As Button
    Private _refreshStatusBadge As Label
    Private _nextRefreshBadge As Label
    Private _refreshQueueBadge As Label
    Private _refreshIntervalLabel As Label
    Private _refreshIntervalCombo As ComboBox
    Private _suppressRefreshIntervalEvent As Boolean
    Private _activeButton As Button
    Private _contentHost As Panel
    Private _navScrollHost As Panel
    Private _kpiBecadosComedor As Label
    Private _kpiBecadosTransporte As Label
    Private _kpiMarcasComedor As Label
    Private _kpiMarcasTransporte As Label
    Private _sparkBecadosComedor As Panel
    Private _sparkBecadosTransporte As Panel
    Private _sparkMarcasComedor As Panel
    Private _sparkMarcasTransporte As Panel
    Private _comedorChartPanel As Panel
    Private _transporteChartPanel As Panel
    Private _deltaComedorLabel As Label
    Private _deltaTransporteLabel As Label
    Private _alertsBodyLabel As Label
    Private _topRutasBodyLabel As Label
    Private _alertsStateLabel As Label
    Private _topRutasStateLabel As Label
    Private _chartTitleLabel As Label
    Private _sidebarLogo As PictureBox
    Private _lastSnapshot As DashboardSnapshot
    Private ReadOnly _navButtonTexts As New Dictionary(Of Button, String)()
    Private _navTooltip As ToolTip

    Public Class DailyMetric
        Public Property Label As String
        Public Property Comedor As Integer
        Public Property ComedorBecados As Integer
        Public Property Transporte As Integer
    End Class

    Public Class DashboardSnapshot
        Public Property BecadosComedorHoy As Integer
        Public Property BecadosTransporteHoy As Integer
        Public Property MarcasComedorHoy As Integer
        Public Property MarcasTransporteHoy As Integer
        Public Property MarcasComedorAyer As Integer
        Public Property MarcasTransporteAyer As Integer
        Public Property Series As List(Of DailyMetric)
        Public Property Alertas As List(Of String)
        Public Property TopRutas As List(Of String)
    End Class

    Private Class NavItem
        Public Property [Key] As String
        Public Property [Group] As String
        Public Property Text As String
        Public Property Tagline As String
    End Class

    Private Class SparklineData
        Public Property Values As List(Of Integer)
        Public Property LineColor As Color
        Public Property FillColor As Color
    End Class

    Public Sub New(ByVal owner As Form, ByVal onNavigate As Action(Of String))
        _owner = owner
        If onNavigate Is Nothing Then
            _onNavigate = Sub(key As String)
                              ' no-op defensivo para evitar NullReference en eventos UI
                          End Sub
        Else
            _onNavigate = onNavigate
        End If
    End Sub

    Public Sub Build()
        If _owner Is Nothing OrElse Not _sidebar Is Nothing Then
            Exit Sub
        End If

        _owner.BackColor = UIConstants.AppBackground
        ApplyOwnerIcon()
        _navTooltip = New ToolTip()
        _navTooltip.InitialDelay = 180
        _navTooltip.ReshowDelay = 80
        _navTooltip.AutoPopDelay = 7000
        _navTooltip.ShowAlways = True

        _contentHost = New Panel()
        _contentHost.Name = "ModernContentHost"
        _contentHost.Dock = DockStyle.None
        _contentHost.BackColor = Color.FromArgb(243, 246, 251)

        _sidebar = New Panel()
        _sidebar.Name = "ModernSidebar"
        _sidebar.Dock = DockStyle.Left
        _sidebar.Width = 300
        _sidebar.BackColor = Color.FromArgb(16, 26, 46)
        _sidebar.AutoScroll = False

        _topBar = New Panel()
        _topBar.Name = "ModernTopBar"
        _topBar.Dock = DockStyle.Top
        _topBar.Height = 96
        _topBar.BackColor = Color.White

        _titleLabel = New Label()
        _titleLabel.Text = "Panel principal"
        _titleLabel.AutoSize = False
        _titleLabel.Location = New Point(24, 8)
        _titleLabel.Font = New Font("Segoe UI Semibold", 16.0!, FontStyle.Bold, GraphicsUnit.Point)
        _titleLabel.Size = New Size(520, 30)
        _titleLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        _titleLabel.AutoEllipsis = True
        _titleLabel.ForeColor = Color.FromArgb(23, 32, 51)
        _topBar.Controls.Add(_titleLabel)

        _contextLabel = Nothing
        Dim statusBaseY As Integer = 44

        _lastSuccessBadge = New Label()
        _lastSuccessBadge.AutoSize = False
        _lastSuccessBadge.TextAlign = ContentAlignment.MiddleLeft
        _lastSuccessBadge.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
        _lastSuccessBadge.ForeColor = Color.FromArgb(24, 121, 78)
        _lastSuccessBadge.Text = "Ultimo OK: --"
        _lastSuccessBadge.SetBounds(26, statusBaseY, 320, 18)
        _topBar.Controls.Add(_lastSuccessBadge)

        _lastUpdateBadge = New Label()
        _lastUpdateBadge.AutoSize = False
        _lastUpdateBadge.TextAlign = ContentAlignment.MiddleLeft
        _lastUpdateBadge.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
        _lastUpdateBadge.ForeColor = Color.FromArgb(76, 90, 112)
        _lastUpdateBadge.Text = "Actualizacion: --"
        _lastUpdateBadge.SetBounds(26, statusBaseY + 18, 320, 18)
        _topBar.Controls.Add(_lastUpdateBadge)

        _refreshStatusBadge = New Label()
        _refreshStatusBadge.AutoSize = False
        _refreshStatusBadge.TextAlign = ContentAlignment.MiddleLeft
        _refreshStatusBadge.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
        _refreshStatusBadge.ForeColor = Color.FromArgb(98, 111, 129)
        _refreshStatusBadge.Text = "Estado: sin sincronizar"
        _refreshStatusBadge.SetBounds(350, statusBaseY + 18, 420, 18)
        _refreshStatusBadge.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        _topBar.Controls.Add(_refreshStatusBadge)

        _nextRefreshBadge = New Label()
        _nextRefreshBadge.AutoSize = False
        _nextRefreshBadge.TextAlign = ContentAlignment.MiddleLeft
        _nextRefreshBadge.Font = New Font("Segoe UI", 8.75!, FontStyle.Bold, GraphicsUnit.Point)
        _nextRefreshBadge.ForeColor = Color.FromArgb(98, 111, 129)
        _nextRefreshBadge.Text = "Prox. refresh: --"
        _nextRefreshBadge.SetBounds(350, statusBaseY, 250, 18)
        _nextRefreshBadge.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        _topBar.Controls.Add(_nextRefreshBadge)

        _refreshQueueBadge = New Label()
        _refreshQueueBadge.AutoSize = False
        _refreshQueueBadge.TextAlign = ContentAlignment.MiddleLeft
        _refreshQueueBadge.Font = New Font("Segoe UI", 8.75!, FontStyle.Bold, GraphicsUnit.Point)
        _refreshQueueBadge.ForeColor = Color.FromArgb(98, 111, 129)
        _refreshQueueBadge.Text = "Cola refresh: 0"
        _refreshQueueBadge.SetBounds(606, statusBaseY, 140, 18)
        _refreshQueueBadge.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        _topBar.Controls.Add(_refreshQueueBadge)

        Dim topDivider As New Panel()
        topDivider.Dock = DockStyle.Bottom
        topDivider.Height = 1
        topDivider.BackColor = Color.FromArgb(224, 229, 236)
        _topBar.Controls.Add(topDivider)

        Dim userBadge As New Label()
        userBadge.AutoSize = False
        userBadge.TextAlign = ContentAlignment.MiddleRight
        userBadge.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold, GraphicsUnit.Point)
        userBadge.ForeColor = Color.FromArgb(56, 74, 97)
        userBadge.SetBounds(_owner.ClientSize.Width - 320, 10, 300, 24)
        userBadge.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        userBadge.Text = "Usuario: " & GetDisplayUser()
        _topBar.Controls.Add(userBadge)

        Dim dateBadge As New Label()
        dateBadge.AutoSize = False
        dateBadge.TextAlign = ContentAlignment.MiddleRight
        dateBadge.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
        dateBadge.ForeColor = Color.FromArgb(95, 109, 128)
        dateBadge.SetBounds(_owner.ClientSize.Width - 320, 36, 300, 22)
        dateBadge.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        dateBadge.Text = Date.Now.ToString("dddd, dd MMM yyyy")
        _topBar.Controls.Add(dateBadge)

        _refreshButton = New Button()
        _refreshButton.Text = "Actualizar ahora"
        _refreshButton.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
        _refreshButton.ForeColor = Color.FromArgb(56, 74, 97)
        _refreshButton.BackColor = Color.FromArgb(240, 245, 252)
        _refreshButton.FlatStyle = FlatStyle.Flat
        _refreshButton.FlatAppearance.BorderColor = Color.FromArgb(210, 221, 236)
        _refreshButton.FlatAppearance.BorderSize = 1
        _refreshButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 239, 250)
        _refreshButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(220, 232, 247)
        _refreshButton.Cursor = Cursors.Hand
        _refreshButton.Size = New Size(126, 28)
        _refreshButton.Location = New Point(_owner.ClientSize.Width - 460, 24)
        _refreshButton.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        _navTooltip.SetToolTip(_refreshButton, "Refrescar metricas y graficas del dashboard")
        AddHandler _refreshButton.Click, Sub(sender As Object, e As EventArgs) SafeNavigate("dashboard_refresh")
        _topBar.Controls.Add(_refreshButton)

        _autoRefreshToggleButton = New Button()
        _autoRefreshToggleButton.Text = "Auto ON"
        _autoRefreshToggleButton.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
        _autoRefreshToggleButton.ForeColor = Color.FromArgb(24, 121, 78)
        _autoRefreshToggleButton.BackColor = Color.FromArgb(226, 245, 233)
        _autoRefreshToggleButton.FlatStyle = FlatStyle.Flat
        _autoRefreshToggleButton.FlatAppearance.BorderColor = Color.FromArgb(181, 227, 198)
        _autoRefreshToggleButton.FlatAppearance.BorderSize = 1
        _autoRefreshToggleButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(210, 239, 221)
        _autoRefreshToggleButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(196, 231, 209)
        _autoRefreshToggleButton.Cursor = Cursors.Hand
        _autoRefreshToggleButton.Size = New Size(80, 28)
        _autoRefreshToggleButton.Location = New Point(_owner.ClientSize.Width - 548, 24)
        _autoRefreshToggleButton.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        _navTooltip.SetToolTip(_autoRefreshToggleButton, "Pausar o reanudar el auto-refresh")
        AddHandler _autoRefreshToggleButton.Click, Sub(sender As Object, e As EventArgs) SafeNavigate("dashboard_toggle_autorefresh")
        _topBar.Controls.Add(_autoRefreshToggleButton)

        _refreshIntervalLabel = New Label()
        _refreshIntervalLabel.AutoSize = False
        _refreshIntervalLabel.TextAlign = ContentAlignment.MiddleRight
        _refreshIntervalLabel.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
        _refreshIntervalLabel.ForeColor = Color.FromArgb(95, 109, 128)
        _refreshIntervalLabel.Text = "Auto-refresh:"
        _refreshIntervalLabel.SetBounds(_owner.ClientSize.Width - 604, 29, 86, 20)
        _refreshIntervalLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        _topBar.Controls.Add(_refreshIntervalLabel)

        _refreshIntervalCombo = New ComboBox()
        _refreshIntervalCombo.DropDownStyle = ComboBoxStyle.DropDownList
        _refreshIntervalCombo.FlatStyle = FlatStyle.Flat
        _refreshIntervalCombo.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
        _refreshIntervalCombo.ForeColor = Color.FromArgb(56, 74, 97)
        _refreshIntervalCombo.BackColor = Color.White
        _refreshIntervalCombo.Items.AddRange(New Object() {"30s", "60s", "120s"})
        _refreshIntervalCombo.SetBounds(_owner.ClientSize.Width - 514, 26, 54, 26)
        _refreshIntervalCombo.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        AddHandler _refreshIntervalCombo.SelectedIndexChanged, AddressOf RefreshIntervalCombo_SelectedIndexChanged
        _topBar.Controls.Add(_refreshIntervalCombo)
        SetDashboardRefreshIntervalSeconds(60)

        Dim closeButton As New Button()
        closeButton.Text = "X"
        closeButton.Font = New Font("Segoe UI Semibold", 10.0!, FontStyle.Bold, GraphicsUnit.Point)
        closeButton.ForeColor = Color.FromArgb(86, 100, 121)
        closeButton.BackColor = Color.White
        closeButton.FlatStyle = FlatStyle.Flat
        closeButton.FlatAppearance.BorderSize = 0
        closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 247, 251)
        closeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(236, 240, 246)
        closeButton.Cursor = Cursors.Hand
        closeButton.Size = New Size(38, 30)
        closeButton.Location = New Point(_owner.ClientSize.Width - 46, 7)
        closeButton.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        AddHandler closeButton.Click, Sub(sender As Object, e As EventArgs) _owner.Close()
        _topBar.Controls.Add(closeButton)

        Dim sidebarHeader As New Panel()
        sidebarHeader.Dock = DockStyle.Top
        sidebarHeader.Height = 120
        sidebarHeader.BackColor = Color.FromArgb(16, 26, 46)

        Dim footer As New Panel()
        footer.Dock = DockStyle.Bottom
        footer.Height = 80
        footer.BackColor = Color.FromArgb(16, 26, 46)

        Dim footerLine As New Panel()
        footerLine.Dock = DockStyle.Top
        footerLine.Height = 1
        footerLine.BackColor = Color.FromArgb(31, 48, 78)
        footer.Controls.Add(footerLine)

        Dim footerInner As New Panel()
        footerInner.Dock = DockStyle.Fill
        footerInner.Padding = New Padding(14, 18, 14, 18)
        footer.Controls.Add(footerInner)

        _navScrollHost = New Panel()
        _navScrollHost.Dock = DockStyle.Fill
        _navScrollHost.BackColor = Color.FromArgb(16, 26, 46)
        _navScrollHost.AutoScroll = True
        _navScrollHost.Padding = New Padding(0, 4, 0, 4)

        Dim sidebarDivider As New Panel()
        sidebarDivider.Dock = DockStyle.Right
        sidebarDivider.Width = 1
        sidebarDivider.BackColor = Color.FromArgb(31, 48, 78)
        ' Orden de agregado importante para Dock:
        ' primero Fill, luego Bottom/Top y al final Right.
        ' Asi evitamos que el area de navegacion quede por debajo del header.
        _sidebar.Controls.Add(_navScrollHost)
        _sidebar.Controls.Add(footer)
        _sidebar.Controls.Add(sidebarHeader)
        _sidebar.Controls.Add(sidebarDivider)

        Dim brand As New Label()
        brand.Text = "SCSC 2026"
        brand.ForeColor = Color.White
        brand.Font = New Font("Segoe UI Semibold", 18.0!, FontStyle.Bold, GraphicsUnit.Point)
        brand.AutoSize = False
        brand.TextAlign = ContentAlignment.MiddleLeft
        brand.SetBounds(80, 14, _sidebar.Width - 96, 40)
        sidebarHeader.Controls.Add(brand)

        _sidebarLogo = CreateBrandPictureBox("LogoIcon.png", New Rectangle(18, 12, 52, 52), Color.Transparent)
        If _sidebarLogo IsNot Nothing Then
            sidebarHeader.Controls.Add(_sidebarLogo)
            _sidebarLogo.BringToFront()
        End If

        Dim navHeader As New Label()
        navHeader.Text = "NAVEGACION"
        navHeader.ForeColor = Color.FromArgb(116, 141, 178)
        navHeader.Font = New Font("Segoe UI", 9.25!, FontStyle.Bold, GraphicsUnit.Point)
        navHeader.AutoSize = False
        navHeader.SetBounds(20, 92, _sidebar.Width - 40, 20)
        sidebarHeader.Controls.Add(navHeader)

        Dim items As New List(Of NavItem) From {
            New NavItem With {.Group = "OPERACION DIARIA", .Key = "comedor", .Text = "Comedor - Registro diario", .Tagline = "Marcas y uso de comedor"},
            New NavItem With {.Group = "OPERACION DIARIA", .Key = "transporte", .Text = "Transporte - Control de rutas", .Tagline = "Abordaje y rutas activas"},
            New NavItem With {.Group = "OPERACION DIARIA", .Key = "recargas", .Text = "Recargas - Saldos", .Tagline = "Movimientos y recargas"},
            New NavItem With {.Group = "OPERACION DIARIA", .Key = "reportes", .Text = "Reportes - Indicadores", .Tagline = "Paneles y reportes ejecutivos"},
            New NavItem With {.Group = "GESTION ACADEMICA", .Key = "estudiantes", .Text = "Estudiantes - Expediente", .Tagline = "Administracion de estudiantes"},
            New NavItem With {.Group = "GESTION ACADEMICA", .Key = "becas", .Text = "Becas - Beneficios", .Tagline = "Asignacion y control de becas"},
            New NavItem With {.Group = "GESTION ACADEMICA", .Key = "rutas", .Text = "Rutas - Catalogo", .Tagline = "Catalogo de rutas de transporte"},
            New NavItem With {.Group = "CONFIGURACION", .Key = "importacion", .Text = "Importar Excel", .Tagline = "Carga masiva de datos PIAD"},
            New NavItem With {.Group = "CONFIGURACION", .Key = "seguridad", .Text = "Seguridad - Roles y permisos", .Tagline = "Usuarios, perfiles y auditoria"},
            New NavItem With {.Group = "CONFIGURACION", .Key = "parametros", .Text = "Parametros del sistema", .Tagline = "Fila 1 de configuracion operativa"},
            New NavItem With {.Group = "CONFIGURACION", .Key = "ayuda", .Text = "Ayuda - Soporte", .Tagline = "Soporte funcional y tecnico"}
        }

        Dim topPos As Integer = 8
        Dim currentGroup As String = String.Empty
        For Each item As NavItem In items
            If Not String.Equals(currentGroup, item.Group, StringComparison.Ordinal) Then
                currentGroup = item.Group
                Dim sectionLabel As New Label()
                sectionLabel.Text = currentGroup
                sectionLabel.ForeColor = Color.FromArgb(116, 141, 178)
                sectionLabel.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
                sectionLabel.AutoSize = False
                sectionLabel.SetBounds(20, topPos, _sidebar.Width - 40, 20)
                sectionLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
                _navScrollHost.Controls.Add(sectionLabel)
                topPos += 26
            End If

            Dim navKey As String = item.Key
            Dim navText As String = item.Text
            Dim navTagline As String = item.Tagline
            Dim btn As New Button()
            btn.Text = navText
            btn.Tag = navKey
            btn.AccessibleDescription = navTagline
            btn.SetBounds(14, topPos, _sidebar.Width - 28, 48)
            btn.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            btn.FlatStyle = FlatStyle.Flat
            btn.FlatAppearance.BorderSize = 0
            btn.BackColor = Color.FromArgb(16, 26, 46)
            btn.ForeColor = Color.FromArgb(233, 241, 255)
            btn.TextAlign = ContentAlignment.MiddleLeft
            btn.Padding = New Padding(16, 0, 10, 0)
            btn.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
            btn.Cursor = Cursors.Hand
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 46, 74)
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(36, 60, 98)
            _navButtonTexts(btn) = navText
            _navTooltip.SetToolTip(btn, navTagline)
            AddHandler btn.MouseEnter, AddressOf OnButtonEnter
            AddHandler btn.MouseLeave, AddressOf OnButtonLeave
            AddHandler btn.Click,
                Sub(sender, e)
                    ActivateButton(DirectCast(sender, Button), navText, navTagline)
                    SafeNavigate(navKey)
                End Sub
            _navScrollHost.Controls.Add(btn)
            topPos += 50
        Next
        _navScrollHost.AutoScrollPosition = New Point(0, 0)

        Dim btnSalir As New Button()
        btnSalir.Text = "Salir del sistema"
        btnSalir.Dock = DockStyle.Fill
        btnSalir.FlatStyle = FlatStyle.Flat
        btnSalir.FlatAppearance.BorderSize = 0
        btnSalir.BackColor = Color.FromArgb(114, 44, 61)
        btnSalir.ForeColor = Color.White
        btnSalir.TextAlign = ContentAlignment.MiddleLeft
        btnSalir.Padding = New Padding(16, 0, 10, 0)
        btnSalir.Font = New Font("Segoe UI Semibold", 10.0!, FontStyle.Bold, GraphicsUnit.Point)
        btnSalir.Cursor = Cursors.Hand
        btnSalir.FlatAppearance.MouseOverBackColor = Color.FromArgb(133, 54, 72)
        btnSalir.FlatAppearance.MouseDownBackColor = Color.FromArgb(95, 36, 50)
        AddHandler btnSalir.Click,
            Sub(sender As Object, e As EventArgs)
                If MessageBox.Show("Desea cerrar la aplicación?", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                    _owner.Close()
                End If
            End Sub
        footerInner.Controls.Add(btnSalir)

        _owner.Controls.Add(_contentHost)
        _owner.Controls.Add(_sidebar)
        _owner.Controls.Add(_topBar)

        AddHandler _owner.Resize,
            Sub(sender As Object, e As EventArgs)
                LayoutShellHost()
                RefreshDashboardVisuals()
            End Sub
        AddHandler _contentHost.Resize,
            Sub(sender As Object, e As EventArgs)
                RefreshDashboardVisuals()
            End Sub

        LayoutShellHost()
        BuildDashboardSurface()
    End Sub

    Private Sub LayoutShellHost()
        If _owner Is Nothing OrElse _contentHost Is Nothing OrElse _sidebar Is Nothing OrElse _topBar Is Nothing Then
            Exit Sub
        End If

        Dim contentX As Integer = _sidebar.Width
        Dim contentY As Integer = _topBar.Height
        Dim contentW As Integer = Math.Max(520, _owner.ClientSize.Width - contentX)
        Dim contentH As Integer = Math.Max(380, _owner.ClientSize.Height - contentY)
        _contentHost.SetBounds(contentX, contentY, contentW, contentH)
    End Sub

    Public Sub SetTitle(ByVal text As String)
        If _titleLabel IsNot Nothing Then
            _titleLabel.Text = text
        End If
    End Sub

    Private Sub OnButtonEnter(ByVal sender As Object, ByVal e As EventArgs)
        Dim button As Button = DirectCast(sender, Button)
        If button Is _activeButton Then
            Exit Sub
        End If
        button.BackColor = Color.FromArgb(29, 46, 74)
        button.Padding = New Padding(18, 0, 10, 0)
    End Sub

    Private Sub OnButtonLeave(ByVal sender As Object, ByVal e As EventArgs)
        Dim button As Button = DirectCast(sender, Button)
        If button Is _activeButton Then
            Exit Sub
        End If
        button.BackColor = Color.FromArgb(16, 26, 46)
        button.Padding = New Padding(16, 0, 10, 0)
    End Sub

    Private Sub ActivateButton(ByVal button As Button, ByVal title As String, ByVal tagline As String)
        If _activeButton IsNot Nothing Then
            If _navButtonTexts.ContainsKey(_activeButton) Then
                _activeButton.Text = _navButtonTexts(_activeButton)
            End If
            _activeButton.BackColor = Color.FromArgb(16, 26, 46)
            _activeButton.ForeColor = Color.FromArgb(233, 241, 255)
            _activeButton.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
            _activeButton.Padding = New Padding(16, 0, 10, 0)
        End If

        _activeButton = button
        If _navButtonTexts.ContainsKey(_activeButton) Then
            _activeButton.Text = "● " & _navButtonTexts(_activeButton)
        End If
        _activeButton.BackColor = Color.FromArgb(45, 90, 154)
        _activeButton.ForeColor = Color.White
        _activeButton.Font = New Font("Segoe UI Semibold", 10.0!, FontStyle.Bold, GraphicsUnit.Point)
        _activeButton.Padding = New Padding(18, 0, 10, 0)
        If _navScrollHost IsNot Nothing Then
            Try
                _navScrollHost.ScrollControlIntoView(_activeButton)
            Catch
            End Try
        End If
        SetTitle(title)
        If _contextLabel IsNot Nothing Then
            _contextLabel.Text = tagline
        End If
    End Sub

    Private Function GetDisplayUser() As String
        If String.IsNullOrWhiteSpace(NombreUsuario) Then
            Return "Sesion activa"
        End If
        Return NombreUsuario.Trim()
    End Function

    Public Sub BindDashboard(ByVal snapshot As DashboardSnapshot)
        If snapshot Is Nothing Then
            Exit Sub
        End If

        _lastSnapshot = snapshot
        If _kpiBecadosComedor Is Nothing OrElse _kpiBecadosTransporte Is Nothing OrElse _kpiMarcasComedor Is Nothing OrElse _kpiMarcasTransporte Is Nothing Then
            BuildDashboardSurface()
        End If

        _kpiBecadosComedor.Text = snapshot.BecadosComedorHoy.ToString("N0")
        _kpiBecadosTransporte.Text = snapshot.BecadosTransporteHoy.ToString("N0")
        _kpiMarcasComedor.Text = snapshot.MarcasComedorHoy.ToString("N0")
        _kpiMarcasTransporte.Text = snapshot.MarcasTransporteHoy.ToString("N0")
        UpdateKpiSparklines(snapshot.Series)
        UpdateDeltaStatus(snapshot.MarcasComedorHoy, snapshot.MarcasComedorAyer, _deltaComedorLabel, "Comedor")
        UpdateDeltaStatus(snapshot.MarcasTransporteHoy, snapshot.MarcasTransporteAyer, _deltaTransporteLabel, "Transporte")
        UpdateAlertStatus(snapshot.Alertas)
        UpdateTopRutasStatus(snapshot.TopRutas)
        RefreshDashboardVisuals()
    End Sub

    Private Sub RefreshDashboardVisuals()
        If _lastSnapshot Is Nothing Then
            Exit Sub
        End If
        If _comedorChartPanel Is Nothing OrElse _transporteChartPanel Is Nothing Then
            Exit Sub
        End If
        If _comedorChartPanel.Width <= 40 OrElse _comedorChartPanel.Height <= 40 Then
            Exit Sub
        End If

        RenderComedorChart(_lastSnapshot.Series)
        RenderTransporteChart(_lastSnapshot.Series)
    End Sub

    Private Sub BuildDashboardSurface()
        If _contentHost Is Nothing Then
            Exit Sub
        End If

        _contentHost.Controls.Clear()
        Dim canvasPadding As Integer = 24

        Dim title As New Label()
        title.Text = "Dashboard Operativo"
        title.Font = New Font("Segoe UI Semibold", 18.0!, FontStyle.Bold, GraphicsUnit.Point)
        title.ForeColor = Color.FromArgb(28, 40, 65)
        title.AutoSize = True
        title.Location = New Point(canvasPadding, 20)
        _contentHost.Controls.Add(title)

        Dim subtitle As New Label()
        subtitle.Text = "Vista ejecutiva de uso de comedor, becas y transporte"
        subtitle.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
        subtitle.ForeColor = Color.FromArgb(97, 111, 131)
        subtitle.AutoSize = True
        subtitle.Location = New Point(canvasPadding + 2, 52)
        _contentHost.Controls.Add(subtitle)

        Dim availableWidth As Integer = Math.Max(720, _contentHost.ClientSize.Width - (canvasPadding * 2))

        Dim kpiLayout As New TableLayoutPanel()
        kpiLayout.ColumnCount = 4
        kpiLayout.RowCount = 1
        kpiLayout.Location = New Point(canvasPadding, 86)
        kpiLayout.Size = New Size(availableWidth, 138)
        kpiLayout.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        kpiLayout.BackColor = Color.Transparent
        For i As Integer = 0 To 3
            kpiLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        Next
        _contentHost.Controls.Add(kpiLayout)

        Dim card1 As Panel = BuildKpiCard("Becados Comedor (hoy)", Color.FromArgb(16, 94, 170), _kpiBecadosComedor, _sparkBecadosComedor, "comedor", "Abrir Control de Comedor")
        Dim card2 As Panel = BuildKpiCard("Becados Transporte (hoy)", Color.FromArgb(17, 124, 96), _kpiBecadosTransporte, _sparkBecadosTransporte, "transporte", "Abrir Control de Transporte")
        Dim card3 As Panel = BuildKpiCard("Marcas Comedor (hoy)", Color.FromArgb(101, 76, 214), _kpiMarcasComedor, _sparkMarcasComedor, "comedor", "Abrir Control de Comedor")
        Dim card4 As Panel = BuildKpiCard("Marcas Transporte (hoy)", Color.FromArgb(184, 97, 24), _kpiMarcasTransporte, _sparkMarcasTransporte, "transporte", "Abrir Control de Transporte")
        kpiLayout.Controls.Add(card1, 0, 0)
        kpiLayout.Controls.Add(card2, 1, 0)
        kpiLayout.Controls.Add(card3, 2, 0)
        kpiLayout.Controls.Add(card4, 3, 0)

        Dim chartCard As New Panel()
        chartCard.BackColor = Color.White
        chartCard.Location = New Point(canvasPadding, kpiLayout.Bottom + 14)
        chartCard.Size = New Size(availableWidth, Math.Max(360, _contentHost.Height - (kpiLayout.Bottom + 28)))
        chartCard.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom
        chartCard.BorderStyle = BorderStyle.FixedSingle
        _contentHost.Controls.Add(chartCard)

        _chartTitleLabel = New Label()
        _chartTitleLabel.Text = "Tendencia semanal operativa (foco: Comedor)"
        _chartTitleLabel.Font = New Font("Segoe UI Semibold", 12.0!, FontStyle.Bold, GraphicsUnit.Point)
        _chartTitleLabel.ForeColor = Color.FromArgb(38, 52, 79)
        _chartTitleLabel.AutoSize = True
        _chartTitleLabel.Location = New Point(18, 15)
        chartCard.Controls.Add(_chartTitleLabel)

        Dim compactMode As Boolean = chartCard.Width < 1080
        Dim insightWidth As Integer = Math.Max(248, Math.Min(320, CInt(chartCard.Width * 0.28R)))
        If compactMode Then
            insightWidth = chartCard.Width - 36
        End If
        Dim leftChartsHost As New Panel()
        leftChartsHost.Location = New Point(18, 48)
        If compactMode Then
            leftChartsHost.Size = New Size(chartCard.Width - 36, Math.Max(260, CInt((chartCard.Height - 92) * 0.64R)))
        Else
            leftChartsHost.Size = New Size(Math.Max(520, chartCard.Width - (insightWidth + 44)), chartCard.Height - 66)
        End If
        leftChartsHost.Anchor = If(compactMode,
                                   AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right,
                                   AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom)
        leftChartsHost.BackColor = Color.White
        chartCard.Controls.Add(leftChartsHost)

        Dim comedorCard As New Panel()
        comedorCard.Dock = DockStyle.Top
        comedorCard.Height = Math.Max(228, CInt((leftChartsHost.Height - 10) * 0.6R))
        comedorCard.BackColor = Color.FromArgb(250, 252, 255)
        comedorCard.BorderStyle = BorderStyle.FixedSingle
        leftChartsHost.Controls.Add(comedorCard)

        Dim comedorTitle As New Label()
        comedorTitle.Text = "Uso de Comedor (7 dias)"
        comedorTitle.AutoSize = True
        comedorTitle.Font = New Font("Segoe UI Semibold", 10.5!, FontStyle.Bold, GraphicsUnit.Point)
        comedorTitle.ForeColor = Color.FromArgb(35, 52, 80)
        comedorTitle.Location = New Point(12, 10)
        comedorCard.Controls.Add(comedorTitle)

        _comedorChartPanel = New Panel()
        _comedorChartPanel.Location = New Point(12, 36)
        _comedorChartPanel.Size = New Size(comedorCard.Width - 24, comedorCard.Height - 46)
        _comedorChartPanel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom
        _comedorChartPanel.BackColor = Color.FromArgb(250, 252, 255)
        comedorCard.Controls.Add(_comedorChartPanel)

        Dim transporteCard As New Panel()
        transporteCard.Dock = DockStyle.Fill
        transporteCard.BackColor = Color.FromArgb(250, 252, 255)
        transporteCard.BorderStyle = BorderStyle.FixedSingle
        leftChartsHost.Controls.Add(transporteCard)

        Dim transporteTitle As New Label()
        transporteTitle.Text = "Transporte (7 dias)"
        transporteTitle.AutoSize = True
        transporteTitle.Font = New Font("Segoe UI Semibold", 10.0!, FontStyle.Bold, GraphicsUnit.Point)
        transporteTitle.ForeColor = Color.FromArgb(35, 52, 80)
        transporteTitle.Location = New Point(12, 10)
        transporteCard.Controls.Add(transporteTitle)

        _transporteChartPanel = New Panel()
        _transporteChartPanel.Location = New Point(12, 34)
        _transporteChartPanel.Size = New Size(transporteCard.Width - 24, transporteCard.Height - 44)
        _transporteChartPanel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom
        _transporteChartPanel.BackColor = Color.FromArgb(250, 252, 255)
        transporteCard.Controls.Add(_transporteChartPanel)

        Dim insightPanel As New Panel()
        If compactMode Then
            insightPanel.Location = New Point(18, leftChartsHost.Bottom + 8)
            insightPanel.Size = New Size(insightWidth, Math.Max(190, chartCard.Height - (leftChartsHost.Bottom + 20)))
            insightPanel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom
        Else
            insightPanel.Location = New Point(chartCard.Width - (insightWidth + 18), 48)
            insightPanel.Size = New Size(insightWidth, chartCard.Height - 66)
            insightPanel.Anchor = AnchorStyles.Top Or AnchorStyles.Right Or AnchorStyles.Bottom
        End If
        insightPanel.BackColor = Color.White
        chartCard.Controls.Add(insightPanel)

        Dim cardGap As Integer = 8
        Dim deltaHeight As Integer = 96
        Dim remainHeight As Integer = Math.Max(220, insightPanel.Height - deltaHeight - (cardGap * 2))
        Dim alertHeight As Integer = Math.Max(108, remainHeight \ 2)
        Dim rutasHeight As Integer = Math.Max(108, remainHeight - alertHeight)

        Dim deltaCard As Panel = BuildInsightCard("Comparativo Hoy vs Ayer", New Point(0, 0), New Size(insightPanel.Width, deltaHeight))
        _deltaComedorLabel = BuildInsightLine("Comedor: +0%", 40)
        _deltaTransporteLabel = BuildInsightLine("Transporte: +0%", 62)
        deltaCard.Controls.Add(_deltaComedorLabel)
        deltaCard.Controls.Add(_deltaTransporteLabel)
        insightPanel.Controls.Add(deltaCard)

        Dim alertY As Integer = deltaHeight + cardGap
        Dim alertCard As Panel = BuildInsightCard("Alertas Rapidas", New Point(0, alertY), New Size(insightPanel.Width, alertHeight))
        _alertsStateLabel = BuildInsightStateChip("ESTABLE")
        _alertsStateLabel.Location = New Point(Math.Max(12, alertCard.Width - 90), 10)
        _alertsStateLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        alertCard.Controls.Add(_alertsStateLabel)
        _alertsBodyLabel = BuildInsightBody("- Sin alertas operativas para hoy.", New Point(12, 36), New Size(alertCard.Width - 22, alertCard.Height - 42))
        alertCard.Controls.Add(_alertsBodyLabel)
        insightPanel.Controls.Add(alertCard)

        Dim rutasY As Integer = alertY + alertHeight + cardGap
        Dim rutasCard As Panel = BuildInsightCard("Top Rutas (hoy)", New Point(0, rutasY), New Size(insightPanel.Width, rutasHeight))
        _topRutasStateLabel = BuildInsightStateChip("SIN DATOS")
        _topRutasStateLabel.Location = New Point(Math.Max(12, rutasCard.Width - 98), 10)
        _topRutasStateLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        rutasCard.Controls.Add(_topRutasStateLabel)
        _topRutasBodyLabel = BuildInsightBody("- Sin datos de rutas para hoy.", New Point(12, 36), New Size(rutasCard.Width - 22, rutasCard.Height - 42))
        rutasCard.Controls.Add(_topRutasBodyLabel)
        insightPanel.Controls.Add(rutasCard)

        RefreshDashboardVisuals()
    End Sub

    Private Function BuildKpiCard(ByVal title As String, ByVal accentColor As Color, ByRef valueLabel As Label, ByRef sparkPanel As Panel, ByVal navKey As String, ByVal hint As String) As Panel
        Dim card As New Panel()
        card.Margin = New Padding(0, 0, 10, 0)
        card.BackColor = Color.White
        card.Dock = DockStyle.Fill
        card.BorderStyle = BorderStyle.FixedSingle
        card.Cursor = Cursors.Hand

        Dim accent As New Panel()
        accent.Dock = DockStyle.Left
        accent.Width = 6
        accent.BackColor = accentColor
        card.Controls.Add(accent)

        Dim titleLabel As New Label()
        titleLabel.Text = title
        titleLabel.Font = New Font("Segoe UI", 9.75!, FontStyle.Regular, GraphicsUnit.Point)
        titleLabel.ForeColor = Color.FromArgb(100, 112, 131)
        titleLabel.AutoSize = False
        titleLabel.SetBounds(18, 14, 232, 34)
        card.Controls.Add(titleLabel)

        valueLabel = New Label()
        valueLabel.Text = "0"
        valueLabel.Font = New Font("Segoe UI Semibold", 27.0!, FontStyle.Bold, GraphicsUnit.Point)
        valueLabel.ForeColor = Color.FromArgb(25, 38, 62)
        valueLabel.AutoSize = True
        valueLabel.Location = New Point(16, 42)
        card.Controls.Add(valueLabel)

        Dim hintLabel As New Label()
        hintLabel.Text = "Actualizado hoy"
        hintLabel.Font = New Font("Segoe UI", 8.75!, FontStyle.Regular, GraphicsUnit.Point)
        hintLabel.ForeColor = Color.FromArgb(120, 132, 148)
        hintLabel.AutoSize = False
        hintLabel.TextAlign = ContentAlignment.MiddleLeft
        hintLabel.SetBounds(18, 96, 118, 20)
        card.Controls.Add(hintLabel)

        sparkPanel = New Panel()
        sparkPanel.SetBounds(138, 94, 110, 24)
        sparkPanel.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        sparkPanel.BackColor = Color.FromArgb(248, 251, 255)
        sparkPanel.Tag = New SparklineData() With {
            .Values = New List(Of Integer)(),
            .LineColor = accentColor,
            .FillColor = Color.FromArgb(96, accentColor)
        }
        AddHandler sparkPanel.Paint, AddressOf SparklinePanel_Paint
        card.Controls.Add(sparkPanel)

        Dim chip As New Panel()
        chip.Size = New Size(12, 12)
        chip.Location = New Point(card.Width - 22, 12)
        chip.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        chip.BackColor = accentColor
        card.Controls.Add(chip)
        WireKpiCardInteractions(card, navKey, hint)
        Return card
    End Function

    Private Sub WireKpiCardInteractions(ByVal card As Panel, ByVal navKey As String, ByVal hint As String)
        If card Is Nothing OrElse String.IsNullOrWhiteSpace(navKey) Then
            Exit Sub
        End If

        If _navTooltip IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(hint) Then
            _navTooltip.SetToolTip(card, hint)
        End If

        AddHandler card.MouseEnter, Sub(sender As Object, e As EventArgs)
                                        card.BackColor = Color.FromArgb(246, 250, 255)
                                    End Sub
        AddHandler card.MouseLeave, Sub(sender As Object, e As EventArgs)
                                        card.BackColor = Color.White
                                    End Sub

        WireClickRecursive(card, Sub() NavigateFromKpi(navKey))
    End Sub

    Private Sub WireClickRecursive(ByVal ctrl As Control, ByVal action As Action)
        AddHandler ctrl.Click, Sub(sender As Object, e As EventArgs) action()
        For Each child As Control In ctrl.Controls
            WireClickRecursive(child, action)
        Next
    End Sub

    Private Sub NavigateFromKpi(ByVal navKey As String)
        If String.IsNullOrWhiteSpace(navKey) Then
            Exit Sub
        End If

        Select Case navKey
            Case "comedor"
                SetTitle("Comedor - Registro diario")
                If _contextLabel IsNot Nothing Then
                    _contextLabel.Text = "Acceso rapido desde KPI del dashboard."
                End If
            Case "transporte"
                SetTitle("Transporte - Control de rutas")
                If _contextLabel IsNot Nothing Then
                    _contextLabel.Text = "Acceso rapido desde KPI del dashboard."
                End If
        End Select

        _onNavigate(navKey)
    End Sub

    Private Sub UpdateKpiSparklines(ByVal series As List(Of DailyMetric))
        If series Is Nothing OrElse series.Count = 0 Then
            SetSparkValues(_sparkBecadosComedor, New List(Of Integer)())
            SetSparkValues(_sparkBecadosTransporte, New List(Of Integer)())
            SetSparkValues(_sparkMarcasComedor, New List(Of Integer)())
            SetSparkValues(_sparkMarcasTransporte, New List(Of Integer)())
            Exit Sub
        End If

        Dim becadosComedor As New List(Of Integer)()
        Dim becadosTransporte As New List(Of Integer)()
        Dim marcasComedor As New List(Of Integer)()
        Dim marcasTransporte As New List(Of Integer)()

        For Each item As DailyMetric In series
            becadosComedor.Add(item.ComedorBecados)
            becadosTransporte.Add(CInt(Math.Round(item.Transporte * 0.35R)))
            marcasComedor.Add(item.Comedor)
            marcasTransporte.Add(item.Transporte)
        Next

        SetSparkValues(_sparkBecadosComedor, becadosComedor)
        SetSparkValues(_sparkBecadosTransporte, becadosTransporte)
        SetSparkValues(_sparkMarcasComedor, marcasComedor)
        SetSparkValues(_sparkMarcasTransporte, marcasTransporte)
    End Sub

    Private Sub SetSparkValues(ByVal panel As Panel, ByVal values As List(Of Integer))
        If panel Is Nothing Then
            Exit Sub
        End If

        Dim data As SparklineData = TryCast(panel.Tag, SparklineData)
        If data Is Nothing Then
            data = New SparklineData()
            panel.Tag = data
        End If
        data.Values = values
        panel.Invalidate()
    End Sub

    Private Sub SparklinePanel_Paint(ByVal sender As Object, ByVal e As PaintEventArgs)
        Dim panel As Panel = TryCast(sender, Panel)
        If panel Is Nothing Then
            Exit Sub
        End If

        Dim data As SparklineData = TryCast(panel.Tag, SparklineData)
        If data Is Nothing OrElse data.Values Is Nothing OrElse data.Values.Count < 2 Then
            Using p As New Pen(Color.FromArgb(175, 186, 202), 1.0F)
                e.Graphics.DrawLine(p, 4, panel.Height - 6, panel.Width - 4, panel.Height - 6)
            End Using
            Exit Sub
        End If

        Dim values As List(Of Integer) = data.Values
        Dim minVal As Integer = Integer.MaxValue
        Dim maxVal As Integer = Integer.MinValue
        For Each v As Integer In values
            minVal = Math.Min(minVal, v)
            maxVal = Math.Max(maxVal, v)
        Next
        Dim span As Integer = Math.Max(1, maxVal - minVal)

        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias
        Dim leftPad As Integer = 4
        Dim rightPad As Integer = 4
        Dim topPad As Integer = 4
        Dim bottomPad As Integer = 4
        Dim w As Integer = Math.Max(8, panel.Width - leftPad - rightPad)
        Dim h As Integer = Math.Max(8, panel.Height - topPad - bottomPad)
        Dim stepX As Double = w / CDbl(values.Count - 1)

        Dim pts As New List(Of PointF)()
        For i As Integer = 0 To values.Count - 1
            Dim normalized As Double = (values(i) - minVal) / CDbl(span)
            Dim x As Single = CSng(leftPad + (i * stepX))
            Dim y As Single = CSng(topPad + (h - (normalized * h)))
            pts.Add(New PointF(x, y))
        Next

        Using basePen As New Pen(Color.FromArgb(222, 228, 237), 1.0F)
            e.Graphics.DrawLine(basePen, leftPad, panel.Height - bottomPad, panel.Width - rightPad, panel.Height - bottomPad)
        End Using

        Using fillPen As New Pen(data.FillColor, 4.0F)
            e.Graphics.DrawLines(fillPen, pts.ToArray())
        End Using
        Using linePen As New Pen(data.LineColor, 2.0F)
            e.Graphics.DrawLines(linePen, pts.ToArray())
        End Using

        Using nodeBrush As New SolidBrush(data.LineColor)
            Dim last As PointF = pts(pts.Count - 1)
            e.Graphics.FillEllipse(nodeBrush, last.X - 2.5F, last.Y - 2.5F, 5.0F, 5.0F)
        End Using
    End Sub

    Private Sub RenderComedorChart(ByVal series As List(Of DailyMetric))
        If _comedorChartPanel Is Nothing Then
            Exit Sub
        End If

        _comedorChartPanel.Controls.Clear()
        If series Is Nothing OrElse series.Count = 0 Then
            Dim empty As New Label()
            empty.Text = "Sin datos para mostrar."
            empty.AutoSize = True
            empty.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
            empty.ForeColor = Color.FromArgb(120, 132, 148)
            empty.Location = New Point(16, 16)
            _comedorChartPanel.Controls.Add(empty)
            Exit Sub
        End If

        Dim maxValue As Integer = 1
        For Each item As DailyMetric In series
            maxValue = Math.Max(maxValue, Math.Max(item.Comedor, item.ComedorBecados))
        Next

        If maxValue <= 0 Then
            Dim empty As New Label()
            empty.Text = "Sin registros de comedor para este periodo."
            empty.AutoSize = True
            empty.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
            empty.ForeColor = Color.FromArgb(120, 132, 148)
            empty.Location = New Point(16, 28)
            _comedorChartPanel.Controls.Add(empty)
            Exit Sub
        End If

        Dim n As Integer = series.Count
        Dim groupWidth As Integer = Math.Max(56, (_comedorChartPanel.Width - 24) \ Math.Max(1, n))
        Dim baseY As Integer = _comedorChartPanel.Height - 40
        Dim usableHeight As Integer = Math.Max(70, _comedorChartPanel.Height - 72)

        For i As Integer = 0 To n - 1
            Dim item As DailyMetric = series(i)
            Dim left As Integer = 12 + (i * groupWidth)
            Dim barAreaWidth As Integer = groupWidth - 12
            Dim barWidth As Integer = Math.Max(10, (barAreaWidth \ 2) - 4)

            Dim comedorHeight As Integer = CInt((item.Comedor / CDbl(maxValue)) * usableHeight)
            Dim becadosHeight As Integer = CInt((item.ComedorBecados / CDbl(maxValue)) * usableHeight)

            Dim barComedor As New Panel()
            barComedor.BackColor = Color.FromArgb(64, 130, 247)
            barComedor.SetBounds(left + 2, baseY - comedorHeight, barWidth, comedorHeight)
            _comedorChartPanel.Controls.Add(barComedor)

            Dim barBecados As New Panel()
            barBecados.BackColor = Color.FromArgb(237, 138, 37)
            barBecados.SetBounds(left + barWidth + 6, baseY - becadosHeight, barWidth, becadosHeight)
            _comedorChartPanel.Controls.Add(barBecados)

            Dim axisLabel As New Label()
            axisLabel.Text = item.Label
            axisLabel.AutoSize = False
            axisLabel.TextAlign = ContentAlignment.MiddleCenter
            axisLabel.Font = New Font("Segoe UI", 8.5!, FontStyle.Regular, GraphicsUnit.Point)
            axisLabel.ForeColor = Color.FromArgb(96, 109, 127)
            axisLabel.SetBounds(left, baseY + 8, barAreaWidth + 6, 18)
            _comedorChartPanel.Controls.Add(axisLabel)
        Next

        Dim legendComedor As New Label()
        legendComedor.Text = "Comedor Total"
        legendComedor.AutoSize = True
        legendComedor.Font = New Font("Segoe UI", 8.5!, FontStyle.Regular, GraphicsUnit.Point)
        legendComedor.ForeColor = Color.FromArgb(64, 130, 247)
        legendComedor.Location = New Point(12, 8)
        _comedorChartPanel.Controls.Add(legendComedor)

        Dim legendBecados As New Label()
        legendBecados.Text = "Comedor Becados"
        legendBecados.AutoSize = True
        legendBecados.Font = New Font("Segoe UI", 8.5!, FontStyle.Regular, GraphicsUnit.Point)
        legendBecados.ForeColor = Color.FromArgb(237, 138, 37)
        legendBecados.Location = New Point(112, 8)
        _comedorChartPanel.Controls.Add(legendBecados)
    End Sub

    Private Sub RenderTransporteChart(ByVal series As List(Of DailyMetric))
        If _transporteChartPanel Is Nothing Then
            Exit Sub
        End If

        _transporteChartPanel.Controls.Clear()
        If series Is Nothing OrElse series.Count = 0 Then
            Exit Sub
        End If
        If _transporteChartPanel.Width < 160 OrElse _transporteChartPanel.Height < 120 Then
            Exit Sub
        End If

        Dim maxValue As Integer = 1
        For Each item As DailyMetric In series
            maxValue = Math.Max(maxValue, Math.Max(item.Comedor, item.Transporte))
        Next

        If maxValue <= 0 Then
            Dim empty As New Label()
            empty.Text = "Sin registros de transporte para este periodo."
            empty.AutoSize = True
            empty.Font = New Font("Segoe UI", 9.5!, FontStyle.Regular, GraphicsUnit.Point)
            empty.ForeColor = Color.FromArgb(120, 132, 148)
            empty.Location = New Point(12, 26)
            _transporteChartPanel.Controls.Add(empty)
            Exit Sub
        End If

        Dim n As Integer = series.Count
        Dim groupWidth As Integer = Math.Max(50, (_transporteChartPanel.Width - 24) \ Math.Max(1, n))
        Dim baseY As Integer = _transporteChartPanel.Height - 34
        Dim usableHeight As Integer = Math.Max(50, _transporteChartPanel.Height - 62)

        For i As Integer = 0 To n - 1
            Dim item As DailyMetric = series(i)
            Dim left As Integer = 12 + (i * groupWidth)
            Dim barAreaWidth As Integer = groupWidth - 10
            Dim barWidth As Integer = Math.Max(8, (barAreaWidth \ 2) - 3)

            Dim comedorHeight As Integer = CInt((item.Comedor / CDbl(maxValue)) * usableHeight)
            Dim transporteHeight As Integer = CInt((item.Transporte / CDbl(maxValue)) * usableHeight)

            Dim barComedor As New Panel()
            barComedor.BackColor = Color.FromArgb(154, 188, 245)
            barComedor.SetBounds(left + 2, baseY - comedorHeight, barWidth, comedorHeight)
            _transporteChartPanel.Controls.Add(barComedor)

            Dim barTransporte As New Panel()
            barTransporte.BackColor = Color.FromArgb(26, 176, 132)
            barTransporte.SetBounds(left + barWidth + 6, baseY - transporteHeight, barWidth, transporteHeight)
            _transporteChartPanel.Controls.Add(barTransporte)

            Dim axisLabel As New Label()
            axisLabel.Text = item.Label
            axisLabel.AutoSize = False
            axisLabel.TextAlign = ContentAlignment.MiddleCenter
            axisLabel.Font = New Font("Segoe UI", 8.25!, FontStyle.Regular, GraphicsUnit.Point)
            axisLabel.ForeColor = Color.FromArgb(96, 109, 127)
            axisLabel.SetBounds(left, baseY + 6, barAreaWidth + 6, 18)
            _transporteChartPanel.Controls.Add(axisLabel)
        Next

        Dim legendTransporte As New Label()
        legendTransporte.Text = "Transporte"
        legendTransporte.AutoSize = True
        legendTransporte.Font = New Font("Segoe UI", 8.5!, FontStyle.Regular, GraphicsUnit.Point)
        legendTransporte.ForeColor = Color.FromArgb(26, 176, 132)
        legendTransporte.Location = New Point(12, 8)
        _transporteChartPanel.Controls.Add(legendTransporte)

        Dim legendComedorRef As New Label()
        legendComedorRef.Text = "Referencia Comedor"
        legendComedorRef.AutoSize = True
        legendComedorRef.Font = New Font("Segoe UI", 8.5!, FontStyle.Regular, GraphicsUnit.Point)
        legendComedorRef.ForeColor = Color.FromArgb(154, 188, 245)
        legendComedorRef.Location = New Point(96, 8)
        _transporteChartPanel.Controls.Add(legendComedorRef)
    End Sub

    Private Function BuildInsightCard(ByVal title As String, ByVal location As Point, ByVal size As Size) As Panel
        Dim card As New Panel()
        card.Location = location
        card.Size = size
        card.BackColor = Color.FromArgb(248, 251, 255)

        Dim titleLabel As New Label()
        titleLabel.Text = title
        titleLabel.AutoSize = True
        titleLabel.Font = New Font("Segoe UI Semibold", 9.75!, FontStyle.Bold, GraphicsUnit.Point)
        titleLabel.ForeColor = Color.FromArgb(45, 60, 88)
        titleLabel.Location = New Point(10, 10)
        card.Controls.Add(titleLabel)
        Return card
    End Function

    Private Function BuildInsightLine(ByVal text As String, ByVal y As Integer) As Label
        Dim line As New Label()
        line.Text = text
        line.AutoSize = True
        line.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular, GraphicsUnit.Point)
        line.ForeColor = Color.FromArgb(80, 96, 120)
        line.Location = New Point(12, y)
        Return line
    End Function

    Private Function BuildInsightBody(ByVal text As String, ByVal location As Point, ByVal size As Size) As Label
        Dim body As New Label()
        body.Text = text
        body.Location = location
        body.Size = size
        body.AutoEllipsis = True
        body.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular, GraphicsUnit.Point)
        body.ForeColor = Color.FromArgb(80, 96, 120)
        Return body
    End Function

    Private Function BuildInsightStateChip(ByVal text As String) As Label
        Dim chip As New Label()
        chip.Text = text
        chip.AutoSize = False
        chip.Size = New Size(82, 20)
        chip.TextAlign = ContentAlignment.MiddleCenter
        chip.Font = New Font("Segoe UI Semibold", 8.0!, FontStyle.Bold, GraphicsUnit.Point)
        chip.ForeColor = Color.FromArgb(28, 68, 42)
        chip.BackColor = Color.FromArgb(222, 244, 229)
        Return chip
    End Function

    Private Sub UpdateDeltaStatus(ByVal currentValue As Integer, ByVal previousValue As Integer, ByVal target As Label, ByVal name As String)
        If target Is Nothing Then
            Exit Sub
        End If

        target.Text = name & ": " & FormatDelta(currentValue, previousValue)

        If currentValue > previousValue Then
            target.ForeColor = Color.FromArgb(24, 121, 78)
        ElseIf currentValue < previousValue Then
            target.ForeColor = Color.FromArgb(173, 52, 61)
        Else
            target.ForeColor = Color.FromArgb(139, 110, 31)
        End If
    End Sub

    Private Sub UpdateAlertStatus(ByVal alertas As List(Of String))
        If _alertsBodyLabel Is Nothing Then
            Exit Sub
        End If

        If alertas Is Nothing OrElse alertas.Count = 0 Then
            _alertsBodyLabel.Text = "- Sin alertas operativas para hoy."
            _alertsBodyLabel.ForeColor = Color.FromArgb(66, 91, 126)
            SetStateChip(_alertsStateLabel, "ESTABLE", Color.FromArgb(222, 244, 229), Color.FromArgb(28, 68, 42))
            Exit Sub
        End If

        Dim formatted As New List(Of String)()
        Dim criticidadAlta As Boolean = False
        For i As Integer = 0 To alertas.Count - 1
            Dim raw As String = If(alertas(i), String.Empty).Trim()
            If raw.StartsWith("-") Then
                raw = raw.Substring(1).Trim()
            End If
            If raw.IndexOf("caida", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               raw.IndexOf("sin becados", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               raw.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0 Then
                criticidadAlta = True
            End If
            formatted.Add((i + 1).ToString() & ") " & raw)
        Next

        _alertsBodyLabel.Text = String.Join(Environment.NewLine, formatted)
        _alertsBodyLabel.ForeColor = If(criticidadAlta, Color.FromArgb(155, 43, 52), Color.FromArgb(139, 110, 31))
        If criticidadAlta Then
            SetStateChip(_alertsStateLabel, "ATENCION", Color.FromArgb(252, 230, 232), Color.FromArgb(125, 39, 48))
        Else
            SetStateChip(_alertsStateLabel, "MONITOREO", Color.FromArgb(255, 245, 218), Color.FromArgb(116, 80, 17))
        End If
    End Sub

    Private Sub UpdateTopRutasStatus(ByVal topRutas As List(Of String))
        If _topRutasBodyLabel Is Nothing Then
            Exit Sub
        End If

        If topRutas Is Nothing OrElse topRutas.Count = 0 Then
            _topRutasBodyLabel.Text = "- Sin datos de rutas para hoy."
            _topRutasBodyLabel.ForeColor = Color.FromArgb(98, 111, 129)
            SetStateChip(_topRutasStateLabel, "SIN DATOS", Color.FromArgb(233, 238, 244), Color.FromArgb(70, 84, 107))
            Exit Sub
        End If

        Dim formatted As New List(Of String)()
        For i As Integer = 0 To Math.Min(4, topRutas.Count - 1)
            Dim raw As String = If(topRutas(i), String.Empty).Trim()
            If raw.StartsWith("-") Then
                raw = raw.Substring(1).Trim()
            End If
            formatted.Add((i + 1).ToString() & ". " & raw)
        Next

        _topRutasBodyLabel.Text = String.Join(Environment.NewLine, formatted)
        _topRutasBodyLabel.ForeColor = Color.FromArgb(46, 68, 101)
        SetStateChip(_topRutasStateLabel, "ACTIVO", Color.FromArgb(220, 239, 254), Color.FromArgb(21, 86, 142))
    End Sub

    Private Sub SetStateChip(ByVal chip As Label, ByVal text As String, ByVal backColor As Color, ByVal foreColor As Color)
        If chip Is Nothing Then
            Exit Sub
        End If
        chip.Text = text
        chip.BackColor = backColor
        chip.ForeColor = foreColor
    End Sub

    Private Function FormatDelta(ByVal currentValue As Integer, ByVal previousValue As Integer) As String
        If previousValue <= 0 Then
            If currentValue <= 0 Then
                Return "0% (sin cambio)"
            End If
            Return "+100% (base 0)"
        End If

        Dim delta As Double = ((currentValue - previousValue) / CDbl(previousValue)) * 100.0R
        Dim sign As String = If(delta > 0, "+", "")
        Return sign & delta.ToString("0") & "% (" & currentValue.ToString("N0") & "/" & previousValue.ToString("N0") & ")"
    End Function

    Private Sub ApplyOwnerIcon()
        Try
            Dim logoPath As String = ResolveBrandAssetPath("LogoIcon.png")
            Dim logoIcon As Icon = CreateIconFromPng(logoPath)
            If logoIcon IsNot Nothing Then
                _owner.Icon = logoIcon
                Exit Sub
            End If

            Dim iconPath As String = ResolveApplicationIconPath()
            If Global.System.IO.File.Exists(iconPath) Then
                _owner.Icon = New Icon(iconPath)
            End If
        Catch
        End Try
    End Sub

    Private Function CreateIconFromPng(ByVal pngPath As String) As Icon
        If String.IsNullOrWhiteSpace(pngPath) OrElse Not Global.System.IO.File.Exists(pngPath) Then
            Return Nothing
        End If

        Dim hIcon As IntPtr = IntPtr.Zero
        Try
            Using source As New Bitmap(pngPath)
                Using resized As New Bitmap(source, New Size(64, 64))
                    hIcon = resized.GetHicon()
                    Using tmp As Icon = Icon.FromHandle(hIcon)
                        Return DirectCast(tmp.Clone(), Icon)
                    End Using
                End Using
            End Using
        Catch
            Return Nothing
        Finally
            If hIcon <> IntPtr.Zero Then
                DestroyIcon(hIcon)
            End If
        End Try
    End Function

    Private Function CreateBrandPictureBox(ByVal fileName As String, ByVal bounds As Rectangle, ByVal backColor As Color) As PictureBox
        Dim imagePath As String = ResolveBrandAssetPath(fileName)
        If Not Global.System.IO.File.Exists(imagePath) Then
            Return Nothing
        End If

        Try
            Dim pb As New PictureBox()
            pb.BackColor = backColor
            pb.SizeMode = PictureBoxSizeMode.Zoom
            pb.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height)
            Using fs As New FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Dim source As Image = Image.FromStream(fs)
                pb.Image = New Bitmap(source)
                source.Dispose()
            End Using
            Return pb
        Catch
            Return Nothing
        End Try
    End Function

    Private Function ResolveBrandAssetPath(ByVal fileName As String) As String
        Return ResolveResourcePath(fileName)
    End Function

    Private Sub SafeNavigate(ByVal key As String)
        Try
            If _onNavigate Is Nothing Then
                Exit Sub
            End If
            _onNavigate(key)
        Catch ex As Exception
            ErrorLogger.LogException("UIShellHost.SafeNavigate", ex, "key=" & If(key, String.Empty))
        End Try
    End Sub
End Class
