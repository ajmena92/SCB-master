Imports System.Windows.Forms
Imports System.Drawing
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices

Public Partial Class UIShellHost
    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function DestroyIcon(ByVal handle As IntPtr) As Boolean
    End Function

    Private ReadOnly _owner As Form
    Private ReadOnly _onNavigate As Action(Of String)
    Private ReadOnly _canAccessModule As Func(Of String, Boolean)
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
    Private _kpiConRutaTransporte As Label
    Private _kpiMarcasComedor As Label
    Private _kpiMarcasTransporte As Label
    Private _sparkBecadosComedor As Panel
    Private _sparkConRutaTransporte As Panel
    Private _sparkMarcasComedor As Panel
    Private _sparkMarcasTransporte As Panel
    Private _comedorChartPanel As Panel
    Private _transporteChartPanel As Panel
    Private _deltaComedorLabel As Label
    Private _deltaTransporteLabel As Label
    Private _deltaConRutaLabel As Label
    Private _comparisonTitleLabel As Label
    Private _alertsBodyLabel As Label
    Private _topRutasBodyLabel As Label
    Private _alertsStateLabel As Label
    Private _topRutasStateLabel As Label
    Private _chartTitleLabel As Label
    Private _sidebarLogo As PictureBox
    Private _lastSnapshot As DashboardContracts.Snapshot
    Private _dashboardCompactMode As Boolean
    Private _dashboardSurfaceBuilt As Boolean
    Private ReadOnly _navButtonTexts As New Dictionary(Of Button, String)()
    Private _navTooltip As ToolTip

    Private Class NavItem
        Public Property [Key] As String
        Public Property [Group] As String
        Public Property Text As String
        Public Property Tagline As String
    End Class

    Private Class SparklineData
        Public Property Values As List(Of Integer)
        Public Property Labels As List(Of String)
        Public Property LineColor As Color
        Public Property FillColor As Color
        Public Property ShowPointValues As Boolean
    End Class

    Private Class DashboardKpiDefinition
        Public Property [Key] As String
        Public Property Title As String
        Public Property AccentColor As Color
        Public Property Hint As String
        Public Property DetailText As String
        Public Property CurrentValueSelector As Func(Of DashboardContracts.Snapshot, Integer)
        Public Property SeriesSelector As Func(Of DashboardContracts.DailyMetric, Integer)
    End Class

    Private Class DashboardChartDefinition
        Public Property [Key] As String
        Public Property Title As String
        Public Property Subtitle As String
        Public Property Hint As String
    End Class

    Private Const KpiModalFadeIntervalMs As Integer = 18
    Private Const KpiModalFadeStep As Double = 0.12R

    Public Sub New(ByVal owner As Form, ByVal onNavigate As Action(Of String), Optional ByVal canAccessModule As Func(Of String, Boolean) = Nothing)
        _owner = owner
        If onNavigate Is Nothing Then
            _onNavigate = Sub(key As String)
                              ' no-op defensivo para evitar NullReference en eventos UI
                          End Sub
        Else
            _onNavigate = onNavigate
        End If

        If canAccessModule Is Nothing Then
            _canAccessModule = Function(key As String) True
        Else
            _canAccessModule = canAccessModule
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
        _contentHost.AutoScroll = True

        _sidebar = New Panel()
        _sidebar.Name = "ModernSidebar"
        _sidebar.Dock = DockStyle.Left
        _sidebar.Width = 300
        _sidebar.BackColor = Color.FromArgb(16, 26, 46)
        _sidebar.AutoScroll = False

        _topBar = New Panel()
        _topBar.Name = "ModernTopBar"
        _topBar.Dock = DockStyle.Top
        _topBar.Height = 100
        _topBar.BackColor = Color.White

        Dim topLayout As New TableLayoutPanel()
        topLayout.Dock = DockStyle.Fill
        topLayout.Margin = New Padding(0)
        topLayout.Padding = New Padding(18, 8, 14, 8)
        topLayout.BackColor = Color.White
        topLayout.ColumnCount = 3
        topLayout.RowCount = 2
        topLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 44.0F))
        topLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 28.0F))
        topLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 360.0F))
        topLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0F))
        topLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        _topBar.Controls.Add(topLayout)

        _titleLabel = New Label()
        _titleLabel.Text = "Panel principal"
        _titleLabel.Dock = DockStyle.Fill
        _titleLabel.Font = New Font("Segoe UI Semibold", 16.0!, FontStyle.Bold, GraphicsUnit.Point)
        _titleLabel.AutoEllipsis = True
        _titleLabel.ForeColor = Color.FromArgb(23, 32, 51)
        _titleLabel.TextAlign = ContentAlignment.MiddleLeft

        _contextLabel = New Label()
        _contextLabel.Text = "Centro operativo, estado de sincronizacion y accesos directos."
        _contextLabel.Dock = DockStyle.Fill
        _contextLabel.AutoEllipsis = True
        _contextLabel.Font = New Font("Segoe UI", 9.25!, FontStyle.Regular, GraphicsUnit.Point)
        _contextLabel.ForeColor = Color.FromArgb(97, 111, 131)
        _contextLabel.TextAlign = ContentAlignment.MiddleLeft

        Dim headerPanel As New TableLayoutPanel()
        headerPanel.Dock = DockStyle.Fill
        headerPanel.Margin = New Padding(0)
        headerPanel.Padding = New Padding(0)
        headerPanel.BackColor = Color.White
        headerPanel.ColumnCount = 1
        headerPanel.RowCount = 2
        headerPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        headerPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 32.0F))
        headerPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 24.0F))
        headerPanel.Controls.Add(_titleLabel, 0, 0)
        headerPanel.Controls.Add(_contextLabel, 0, 1)
        topLayout.Controls.Add(headerPanel, 0, 0)
        topLayout.SetRowSpan(headerPanel, 2)

        _refreshStatusBadge = New Label()
        _refreshStatusBadge.Dock = DockStyle.Fill
        _refreshStatusBadge.TextAlign = ContentAlignment.MiddleLeft
        _refreshStatusBadge.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
        _refreshStatusBadge.ForeColor = Color.FromArgb(98, 111, 129)
        _refreshStatusBadge.Text = "Estado: listo"

        _lastSuccessBadge = New Label()
        _lastSuccessBadge.AutoSize = True
        _lastSuccessBadge.Font = New Font("Segoe UI", 8.75!, FontStyle.Bold, GraphicsUnit.Point)
        _lastSuccessBadge.ForeColor = Color.FromArgb(24, 121, 78)
        _lastSuccessBadge.Text = "OK --"
        _lastSuccessBadge.Margin = New Padding(0, 0, 8, 0)

        _lastUpdateBadge = New Label()
        _lastUpdateBadge.AutoSize = True
        _lastUpdateBadge.Font = New Font("Segoe UI", 8.75!, FontStyle.Bold, GraphicsUnit.Point)
        _lastUpdateBadge.ForeColor = Color.FromArgb(76, 90, 112)
        _lastUpdateBadge.Text = "Sync --"
        _lastUpdateBadge.Margin = New Padding(0, 0, 8, 0)

        _nextRefreshBadge = New Label()
        _nextRefreshBadge.AutoSize = True
        _nextRefreshBadge.Font = New Font("Segoe UI", 8.75!, FontStyle.Bold, GraphicsUnit.Point)
        _nextRefreshBadge.ForeColor = Color.FromArgb(98, 111, 129)
        _nextRefreshBadge.Text = "Sig. --"
        _nextRefreshBadge.Margin = New Padding(0, 0, 8, 0)

        _refreshQueueBadge = New Label()
        _refreshQueueBadge.AutoSize = True
        _refreshQueueBadge.Font = New Font("Segoe UI", 8.75!, FontStyle.Bold, GraphicsUnit.Point)
        _refreshQueueBadge.ForeColor = Color.FromArgb(98, 111, 129)
        _refreshQueueBadge.Text = "Pend. 0"
        _refreshQueueBadge.Margin = New Padding(0)

        Dim statusFlow As New FlowLayoutPanel()
        statusFlow.Dock = DockStyle.Fill
        statusFlow.Margin = New Padding(0)
        statusFlow.Padding = New Padding(0)
        statusFlow.WrapContents = False
        statusFlow.AutoScroll = False
        statusFlow.FlowDirection = FlowDirection.LeftToRight
        statusFlow.BackColor = Color.White
        statusFlow.Controls.Add(_lastSuccessBadge)
        statusFlow.Controls.Add(_lastUpdateBadge)
        statusFlow.Controls.Add(_nextRefreshBadge)
        statusFlow.Controls.Add(_refreshQueueBadge)

        Dim statusPanel As New TableLayoutPanel()
        statusPanel.Dock = DockStyle.Fill
        statusPanel.Margin = New Padding(0, 1, 0, 0)
        statusPanel.Padding = New Padding(0)
        statusPanel.BackColor = Color.White
        statusPanel.ColumnCount = 1
        statusPanel.RowCount = 2
        statusPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        statusPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 22.0F))
        statusPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 24.0F))
        statusPanel.Controls.Add(_refreshStatusBadge, 0, 0)
        statusPanel.Controls.Add(statusFlow, 0, 1)
        topLayout.Controls.Add(statusPanel, 1, 0)
        topLayout.SetRowSpan(statusPanel, 2)

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
        _refreshButton.Size = New Size(108, 28)
        _navTooltip.SetToolTip(_refreshButton, "Refrescar metricas y graficas del dashboard")
        AddHandler _refreshButton.Click, Sub(sender As Object, e As EventArgs) SafeNavigate("dashboard_refresh")

        _autoRefreshToggleButton = New Button()
        _autoRefreshToggleButton.Text = "Auto: ON"
        _autoRefreshToggleButton.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
        _autoRefreshToggleButton.ForeColor = Color.FromArgb(24, 121, 78)
        _autoRefreshToggleButton.BackColor = Color.FromArgb(226, 245, 233)
        _autoRefreshToggleButton.FlatStyle = FlatStyle.Flat
        _autoRefreshToggleButton.FlatAppearance.BorderColor = Color.FromArgb(181, 227, 198)
        _autoRefreshToggleButton.FlatAppearance.BorderSize = 1
        _autoRefreshToggleButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(210, 239, 221)
        _autoRefreshToggleButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(196, 231, 209)
        _autoRefreshToggleButton.Cursor = Cursors.Hand
        _autoRefreshToggleButton.Size = New Size(84, 28)
        _navTooltip.SetToolTip(_autoRefreshToggleButton, "Pausar o reanudar el auto-refresh")
        AddHandler _autoRefreshToggleButton.Click, Sub(sender As Object, e As EventArgs) SafeNavigate("dashboard_toggle_autorefresh")

        _refreshIntervalLabel = New Label()
        _refreshIntervalLabel.AutoSize = True
        _refreshIntervalLabel.TextAlign = ContentAlignment.MiddleRight
        _refreshIntervalLabel.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
        _refreshIntervalLabel.ForeColor = Color.FromArgb(95, 109, 128)
        _refreshIntervalLabel.Text = "Cada:"
        _refreshIntervalLabel.Margin = New Padding(0, 6, 0, 0)

        _refreshIntervalCombo = New ComboBox()
        _refreshIntervalCombo.DropDownStyle = ComboBoxStyle.DropDownList
        _refreshIntervalCombo.FlatStyle = FlatStyle.Flat
        _refreshIntervalCombo.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
        _refreshIntervalCombo.ForeColor = Color.FromArgb(56, 74, 97)
        _refreshIntervalCombo.BackColor = Color.White
        _refreshIntervalCombo.Items.AddRange(New Object() {"30s", "60s", "120s"})
        _refreshIntervalCombo.Size = New Size(64, 26)
        _refreshIntervalCombo.Margin = New Padding(6, 0, 0, 0)
        AddHandler _refreshIntervalCombo.SelectedIndexChanged, AddressOf RefreshIntervalCombo_SelectedIndexChanged
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
        closeButton.Size = New Size(34, 28)
        closeButton.Margin = New Padding(6, 0, 0, 0)
        AddHandler closeButton.Click, Sub(sender As Object, e As EventArgs) _owner.Close()

        Dim actionsFlow As New FlowLayoutPanel()
        actionsFlow.Dock = DockStyle.Fill
        actionsFlow.Margin = New Padding(0)
        actionsFlow.Padding = New Padding(0)
        actionsFlow.WrapContents = False
        actionsFlow.AutoScroll = False
        actionsFlow.FlowDirection = FlowDirection.RightToLeft
        actionsFlow.BackColor = Color.White
        actionsFlow.Controls.Add(closeButton)
        actionsFlow.Controls.Add(_refreshButton)
        actionsFlow.Controls.Add(_autoRefreshToggleButton)
        actionsFlow.Controls.Add(_refreshIntervalCombo)
        actionsFlow.Controls.Add(_refreshIntervalLabel)

        Dim userBadge As New Label()
        userBadge.Dock = DockStyle.Fill
        userBadge.TextAlign = ContentAlignment.MiddleRight
        userBadge.Font = New Font("Segoe UI", 9.75!, FontStyle.Bold, GraphicsUnit.Point)
        userBadge.ForeColor = Color.FromArgb(56, 74, 97)
        userBadge.Text = "Usuario: " & GetDisplayUser()

        Dim dateBadge As New Label()
        dateBadge.Dock = DockStyle.Fill
        dateBadge.TextAlign = ContentAlignment.MiddleRight
        dateBadge.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular, GraphicsUnit.Point)
        dateBadge.ForeColor = Color.FromArgb(95, 109, 128)
        dateBadge.Text = Date.Now.ToString("dddd, dd MMM yyyy")

        Dim userPanel As New TableLayoutPanel()
        userPanel.Dock = DockStyle.Fill
        userPanel.Margin = New Padding(0)
        userPanel.Padding = New Padding(0)
        userPanel.BackColor = Color.White
        userPanel.ColumnCount = 1
        userPanel.RowCount = 2
        userPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        userPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 18.0F))
        userPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 18.0F))
        userPanel.Controls.Add(userBadge, 0, 0)
        userPanel.Controls.Add(dateBadge, 0, 1)

        Dim rightPanel As New TableLayoutPanel()
        rightPanel.Dock = DockStyle.Fill
        rightPanel.Margin = New Padding(0)
        rightPanel.Padding = New Padding(0)
        rightPanel.BackColor = Color.White
        rightPanel.ColumnCount = 1
        rightPanel.RowCount = 2
        rightPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        rightPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0F))
        rightPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 24.0F))
        rightPanel.Controls.Add(actionsFlow, 0, 0)
        rightPanel.Controls.Add(userPanel, 0, 1)
        topLayout.Controls.Add(rightPanel, 2, 0)
        topLayout.SetRowSpan(rightPanel, 2)

        Dim topDivider As New Panel()
        topDivider.Dock = DockStyle.Bottom
        topDivider.Height = 1
        topDivider.BackColor = Color.FromArgb(224, 229, 236)
        _topBar.Controls.Add(topDivider)

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
            New NavItem With {.Group = "OPERACION DIARIA", .Key = "reporte_comedor", .Text = "Reporte Servicio Comedor", .Tagline = "Reporte diario y filtros de comedor"},
            New NavItem With {.Group = "OPERACION DIARIA", .Key = "reporte_transporte", .Text = "Reporte Servicio Transporte", .Tagline = "Reporte general y detallado de rutas"},
            New NavItem With {.Group = "OPERACION DIARIA", .Key = "reporte_proyeccion", .Text = "Reporte Proyeccion Comedor", .Tagline = "Proyeccion operativa de consumo"},
            New NavItem With {.Group = "OPERACION DIARIA", .Key = "reporte_becados", .Text = "Reporte Estudiantes Becados", .Tagline = "Reportes de becados comedor y transporte"},
            New NavItem With {.Group = "GESTION ACADEMICA", .Key = "estudiantes", .Text = "Estudiantes - Expediente", .Tagline = "Administracion de estudiantes"},
            New NavItem With {.Group = "GESTION ACADEMICA", .Key = "becas", .Text = "Becas - Beneficios", .Tagline = "Asignacion y control de becas"},
            New NavItem With {.Group = "GESTION ACADEMICA", .Key = "rutas", .Text = "Rutas - Catalogo", .Tagline = "Catalogo de rutas de transporte"},
            New NavItem With {.Group = "CONFIGURACION", .Key = "importacion", .Text = "Importar Excel", .Tagline = "Carga masiva de datos PIAD"},
            New NavItem With {.Group = "CONFIGURACION", .Key = "agregar_estudiante", .Text = "Agregar estudiante manual", .Tagline = "Alta y edicion manual por cedula"},
            New NavItem With {.Group = "CONFIGURACION", .Key = "seguridad", .Text = "Seguridad - Roles y permisos", .Tagline = "Usuarios, perfiles y auditoria"},
            New NavItem With {.Group = "CONFIGURACION", .Key = "parametros", .Text = "Parametros del sistema", .Tagline = "Fila 1 de configuracion operativa"},
            New NavItem With {.Group = "CONFIGURACION", .Key = "ayuda", .Text = "Ayuda - Soporte", .Tagline = "Soporte funcional y tecnico"}
        }

        Dim visibleItems As List(Of NavItem) = items.Where(Function(item) CanShowModule(item.Key)).ToList()
        Dim topPos As Integer = 8
        Dim currentGroup As String = String.Empty
        If visibleItems.Count = 0 Then
            Dim emptyState As New Label()
            emptyState.Text = "Sin accesos asignados"
            emptyState.ForeColor = Color.FromArgb(180, 197, 222)
            emptyState.Font = New Font("Segoe UI Semibold", 10.0!, FontStyle.Bold, GraphicsUnit.Point)
            emptyState.TextAlign = ContentAlignment.MiddleCenter
            emptyState.SetBounds(20, topPos + 12, _sidebar.Width - 40, 40)
            emptyState.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            _navScrollHost.Controls.Add(emptyState)
        End If

        For Each item As NavItem In visibleItems
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
        btnSalir.Text = "Cerrar"
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
                If MessageBox.Show("Desea cerrar la aplicación?", "Cerrar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
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
            End Sub
        AddHandler _contentHost.Resize,
            Sub(sender As Object, e As EventArgs)
                HandleDashboardHostResize()
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

    Private Sub HandleDashboardHostResize()
        If _contentHost Is Nothing OrElse _contentHost.ClientSize.Width <= 0 OrElse _contentHost.ClientSize.Height <= 0 Then
            Exit Sub
        End If

        Dim compactMode As Boolean = _contentHost.ClientSize.Width < 1140
        If Not _dashboardSurfaceBuilt OrElse compactMode <> _dashboardCompactMode Then
            BuildDashboardSurface()
        Else
            RefreshDashboardVisuals()
        End If
    End Sub

    Private Sub SetDashboardHeaderContext()
        SetTitle("Dashboard Operativo")
        If _contextLabel IsNot Nothing Then
            _contextLabel.Text = "Corte diario de comedor, becas, transporte y alertas operativas."
        End If
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

    Private Function CanShowModule(ByVal moduleKey As String) As Boolean
        Try
            Return _canAccessModule(moduleKey)
        Catch ex As Exception
            ErrorLogger.LogException("UIShellHost.CanShowModule", ex, "ModuleKey=" & moduleKey)
            Return False
        End Try
    End Function

    Public Sub BindDashboard(ByVal snapshot As DashboardContracts.Snapshot)
        If snapshot Is Nothing Then
            Exit Sub
        End If

        _lastSnapshot = snapshot
        SetDashboardHeaderContext()
        If _kpiBecadosComedor Is Nothing OrElse _kpiConRutaTransporte Is Nothing OrElse _kpiMarcasComedor Is Nothing OrElse _kpiMarcasTransporte Is Nothing OrElse _comedorChartPanel Is Nothing OrElse _transporteChartPanel Is Nothing Then
            BuildDashboardSurface()
        End If

        ApplyDashboardSnapshotToSurface(snapshot)
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

    Private Sub ApplyDashboardSnapshotToSurface(ByVal snapshot As DashboardContracts.Snapshot)
        If snapshot Is Nothing Then
            Exit Sub
        End If
        If _kpiBecadosComedor Is Nothing OrElse _kpiConRutaTransporte Is Nothing OrElse _kpiMarcasComedor Is Nothing OrElse _kpiMarcasTransporte Is Nothing Then
            Exit Sub
        End If

        SetDashboardHeaderContext()
        _kpiBecadosComedor.Text = snapshot.BecadosComedorHoy.ToString("N0")
        _kpiConRutaTransporte.Text = snapshot.EstudiantesConRutaHoy.ToString("N0")
        _kpiMarcasComedor.Text = snapshot.MarcasComedorHoy.ToString("N0")
        _kpiMarcasTransporte.Text = snapshot.MarcasTransporteHoy.ToString("N0")
        If _chartTitleLabel IsNot Nothing Then
            _chartTitleLabel.Text = ResolveDashboardPeriodTitle(snapshot)
        End If
        If _comparisonTitleLabel IsNot Nothing Then
            _comparisonTitleLabel.Text = ResolveComparisonTitle(snapshot)
        End If
        UpdateKpiSparklines(snapshot.Series)
        UpdateComparisonStatus(snapshot)
        UpdateAlertStatus(snapshot.Alertas)
        UpdateTopRutasStatus(snapshot.TopRutas)
        RefreshDashboardVisuals()
    End Sub

    Private Sub BuildDashboardSurface()
        If _contentHost Is Nothing OrElse _contentHost.ClientSize.Width <= 0 OrElse _contentHost.ClientSize.Height <= 0 Then
            Exit Sub
        End If

        Dim compactMode As Boolean = _contentHost.ClientSize.Width < 1140
        _dashboardCompactMode = compactMode
        _dashboardSurfaceBuilt = True
        SetDashboardHeaderContext()

        _contentHost.SuspendLayout()
        Try
            _contentHost.Controls.Clear()

            Dim root As New TableLayoutPanel()
            root.Dock = DockStyle.Fill
            root.Margin = New Padding(0)
            root.Padding = New Padding(28, 22, 28, 24)
            root.BackColor = _contentHost.BackColor
            root.ColumnCount = 1
            root.RowCount = 2
            root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, If(compactMode, 312.0F, 158.0F)))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            _contentHost.Controls.Add(root)

            Dim kpiLayout As New TableLayoutPanel()
            kpiLayout.Dock = DockStyle.Fill
            kpiLayout.Margin = New Padding(0, 0, 0, 16)
            kpiLayout.Padding = New Padding(0)
            kpiLayout.BackColor = Color.Transparent
            If compactMode Then
                kpiLayout.ColumnCount = 2
                kpiLayout.RowCount = 2
                kpiLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
                kpiLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
                kpiLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0F))
                kpiLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0F))
            Else
                kpiLayout.ColumnCount = 4
                kpiLayout.RowCount = 1
                For i As Integer = 0 To 3
                    kpiLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
                Next
                kpiLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            End If

            Dim card1 As Panel = BuildKpiCard(GetDashboardKpiDefinition("transporte_ingresos"), _kpiMarcasTransporte, _sparkMarcasTransporte)
            Dim card2 As Panel = BuildKpiCard(GetDashboardKpiDefinition("transporte_con_ruta"), _kpiConRutaTransporte, _sparkConRutaTransporte)
            Dim card3 As Panel = BuildKpiCard(GetDashboardKpiDefinition("comedor_entradas"), _kpiMarcasComedor, _sparkMarcasComedor)
            Dim card4 As Panel = BuildKpiCard(GetDashboardKpiDefinition("comedor_becados"), _kpiBecadosComedor, _sparkBecadosComedor)

            card1.Margin = New Padding(0, 0, If(compactMode, 10, 10), 10)
            card2.Margin = New Padding(If(compactMode, 10, 0), 0, 0, 10)
            card3.Margin = New Padding(0, If(compactMode, 10, 0), If(compactMode, 10, 10), 0)
            card4.Margin = New Padding(If(compactMode, 10, 0), If(compactMode, 10, 0), 0, 0)

            If compactMode Then
                kpiLayout.Controls.Add(card1, 0, 0)
                kpiLayout.Controls.Add(card2, 1, 0)
                kpiLayout.Controls.Add(card3, 0, 1)
                kpiLayout.Controls.Add(card4, 1, 1)
            Else
                kpiLayout.Controls.Add(card1, 0, 0)
                kpiLayout.Controls.Add(card2, 1, 0)
                kpiLayout.Controls.Add(card3, 2, 0)
                kpiLayout.Controls.Add(card4, 3, 0)
            End If
            root.Controls.Add(kpiLayout, 0, 0)

            Dim contentLayout As New TableLayoutPanel()
            contentLayout.Dock = DockStyle.Fill
            contentLayout.Margin = New Padding(0)
            contentLayout.Padding = New Padding(0)
            contentLayout.BackColor = Color.Transparent
            If compactMode Then
                contentLayout.ColumnCount = 1
                contentLayout.RowCount = 2
                contentLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
                contentLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 62.0F))
                contentLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 38.0F))
            Else
                contentLayout.ColumnCount = 2
                contentLayout.RowCount = 1
                contentLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 68.0F))
                contentLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 32.0F))
                contentLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            End If
            root.Controls.Add(contentLayout, 0, 1)

            Dim chartsSection As Panel = CreateDashboardSurfaceCard(Color.FromArgb(252, 253, 255))
            chartsSection.Margin = If(compactMode, New Padding(0, 0, 0, 12), New Padding(0, 0, 14, 0))
            chartsSection.Padding = New Padding(18, 16, 18, 18)

            Dim chartsSectionLayout As New TableLayoutPanel()
            chartsSectionLayout.Dock = DockStyle.Fill
            chartsSectionLayout.Margin = New Padding(0)
            chartsSectionLayout.Padding = New Padding(0)
            chartsSectionLayout.BackColor = chartsSection.BackColor
            chartsSectionLayout.ColumnCount = 1
            chartsSectionLayout.RowCount = 3
            chartsSectionLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            chartsSectionLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 26.0F))
            chartsSectionLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 22.0F))
            chartsSectionLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            chartsSection.Controls.Add(chartsSectionLayout)

            _chartTitleLabel = New Label()
            _chartTitleLabel.Text = "Tendencia operativa de los ultimos 7 dias lectivos"
            _chartTitleLabel.Dock = DockStyle.Fill
            _chartTitleLabel.Font = New Font("Segoe UI Semibold", 12.0!, FontStyle.Bold, GraphicsUnit.Point)
            _chartTitleLabel.ForeColor = Color.FromArgb(38, 52, 79)
            _chartTitleLabel.TextAlign = ContentAlignment.MiddleLeft
            chartsSectionLayout.Controls.Add(_chartTitleLabel, 0, 0)

            Dim chartSubtitle As New Label()
            chartSubtitle.Text = "Historico de los ultimos 7 dias lectivos con entradas, becas y cobertura de ruta"
            chartSubtitle.Dock = DockStyle.Fill
            chartSubtitle.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular, GraphicsUnit.Point)
            chartSubtitle.ForeColor = Color.FromArgb(97, 111, 131)
            chartSubtitle.TextAlign = ContentAlignment.MiddleLeft
            chartsSectionLayout.Controls.Add(chartSubtitle, 0, 1)

            Dim chartsGrid As New TableLayoutPanel()
            chartsGrid.Dock = DockStyle.Fill
            chartsGrid.Margin = New Padding(0, 8, 0, 0)
            chartsGrid.Padding = New Padding(0)
            chartsGrid.BackColor = chartsSection.BackColor
            chartsGrid.ColumnCount = 1
            chartsGrid.RowCount = 2
            chartsGrid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            chartsGrid.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0F))
            chartsGrid.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0F))
            chartsSectionLayout.Controls.Add(chartsGrid, 0, 2)

            Dim comedorCard As Panel = CreateDashboardSurfaceCard(Color.FromArgb(248, 251, 255))
            comedorCard.Margin = New Padding(0, 0, 0, 10)
            comedorCard.Padding = New Padding(14, 12, 14, 14)

            Dim comedorLayout As New TableLayoutPanel()
            comedorLayout.Dock = DockStyle.Fill
            comedorLayout.Margin = New Padding(0)
            comedorLayout.Padding = New Padding(0)
            comedorLayout.BackColor = comedorCard.BackColor
            comedorLayout.ColumnCount = 1
            comedorLayout.RowCount = 2
            comedorLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            comedorLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 32.0F))
            comedorLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            comedorCard.Controls.Add(comedorLayout)

            Dim comedorTitle As New Label()
            comedorTitle.Text = "Uso de Comedor"
            comedorTitle.Dock = DockStyle.Fill
            comedorTitle.Font = New Font("Segoe UI Semibold", 10.5!, FontStyle.Bold, GraphicsUnit.Point)
            comedorTitle.ForeColor = Color.FromArgb(35, 52, 80)
            comedorTitle.TextAlign = ContentAlignment.MiddleLeft
            comedorLayout.Controls.Add(comedorTitle, 0, 0)

            _comedorChartPanel = New Panel()
            _comedorChartPanel.Dock = DockStyle.Fill
            _comedorChartPanel.Margin = New Padding(0)
            _comedorChartPanel.BackColor = comedorCard.BackColor
            comedorLayout.Controls.Add(_comedorChartPanel, 0, 1)
            WireChartCardInteractions(comedorCard, "comedor_chart", "Ampliar grafico")
            chartsGrid.Controls.Add(comedorCard, 0, 0)

            Dim transporteCard As Panel = CreateDashboardSurfaceCard(Color.FromArgb(248, 251, 255))
            transporteCard.Margin = New Padding(0)
            transporteCard.Padding = New Padding(14, 12, 14, 14)

            Dim transporteLayout As New TableLayoutPanel()
            transporteLayout.Dock = DockStyle.Fill
            transporteLayout.Margin = New Padding(0)
            transporteLayout.Padding = New Padding(0)
            transporteLayout.BackColor = transporteCard.BackColor
            transporteLayout.ColumnCount = 1
            transporteLayout.RowCount = 2
            transporteLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            transporteLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0F))
            transporteLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            transporteCard.Controls.Add(transporteLayout)

            Dim transporteTitle As New Label()
            transporteTitle.Text = "Uso de Transporte"
            transporteTitle.Dock = DockStyle.Fill
            transporteTitle.Font = New Font("Segoe UI Semibold", 10.0!, FontStyle.Bold, GraphicsUnit.Point)
            transporteTitle.ForeColor = Color.FromArgb(35, 52, 80)
            transporteTitle.TextAlign = ContentAlignment.MiddleLeft
            transporteLayout.Controls.Add(transporteTitle, 0, 0)

            _transporteChartPanel = New Panel()
            _transporteChartPanel.Dock = DockStyle.Fill
            _transporteChartPanel.Margin = New Padding(0)
            _transporteChartPanel.BackColor = transporteCard.BackColor
            transporteLayout.Controls.Add(_transporteChartPanel, 0, 1)
            WireChartCardInteractions(transporteCard, "transporte_chart", "Ampliar grafico")
            chartsGrid.Controls.Add(transporteCard, 0, 1)

            Dim insightsLayout As New TableLayoutPanel()
            insightsLayout.Dock = DockStyle.Fill
            insightsLayout.Margin = If(compactMode, New Padding(0), New Padding(0))
            insightsLayout.Padding = New Padding(0)
            insightsLayout.BackColor = Color.Transparent
            insightsLayout.ColumnCount = 1
            insightsLayout.RowCount = 3
            insightsLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            insightsLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 118.0F))
            insightsLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0F))
            insightsLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0F))

            Dim deltaCard As Panel = CreateDashboardSurfaceCard(Color.FromArgb(247, 250, 253))
            deltaCard.Margin = New Padding(0, 0, 0, 10)
            deltaCard.Padding = New Padding(14, 12, 14, 12)
            Dim deltaLayout As New TableLayoutPanel()
            deltaLayout.Dock = DockStyle.Fill
            deltaLayout.Margin = New Padding(0)
            deltaLayout.Padding = New Padding(0)
            deltaLayout.BackColor = deltaCard.BackColor
            deltaLayout.ColumnCount = 1
            deltaLayout.RowCount = 4
            deltaLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            deltaLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 24.0F))
            deltaLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 38.0F))
            deltaLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 38.0F))
            deltaLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 38.0F))
            deltaCard.Controls.Add(deltaLayout)

            _comparisonTitleLabel = New Label()
            _comparisonTitleLabel.Text = "Comparativo hoy vs dia lectivo previo"
            _comparisonTitleLabel.Dock = DockStyle.Fill
            _comparisonTitleLabel.Font = New Font("Segoe UI Semibold", 9.75!, FontStyle.Bold, GraphicsUnit.Point)
            _comparisonTitleLabel.ForeColor = Color.FromArgb(45, 60, 88)
            _comparisonTitleLabel.TextAlign = ContentAlignment.MiddleLeft
            deltaLayout.Controls.Add(_comparisonTitleLabel, 0, 0)

            _deltaComedorLabel = New Label()
            _deltaComedorLabel.Dock = DockStyle.Fill
            _deltaComedorLabel.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular, GraphicsUnit.Point)
            _deltaComedorLabel.ForeColor = Color.FromArgb(80, 96, 120)
            _deltaComedorLabel.TextAlign = ContentAlignment.MiddleLeft
            _deltaComedorLabel.Text = "Comedor" & Environment.NewLine & "SIN CAMBIO  |  sin cambio (0/0)"
            deltaLayout.Controls.Add(_deltaComedorLabel, 0, 1)

            _deltaTransporteLabel = New Label()
            _deltaTransporteLabel.Dock = DockStyle.Fill
            _deltaTransporteLabel.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular, GraphicsUnit.Point)
            _deltaTransporteLabel.ForeColor = Color.FromArgb(80, 96, 120)
            _deltaTransporteLabel.TextAlign = ContentAlignment.MiddleLeft
            _deltaTransporteLabel.Text = "Transporte" & Environment.NewLine & "SIN CAMBIO  |  sin cambio (0/0)"
            deltaLayout.Controls.Add(_deltaTransporteLabel, 0, 2)

            _deltaConRutaLabel = New Label()
            _deltaConRutaLabel.Dock = DockStyle.Fill
            _deltaConRutaLabel.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular, GraphicsUnit.Point)
            _deltaConRutaLabel.ForeColor = Color.FromArgb(80, 96, 120)
            _deltaConRutaLabel.TextAlign = ContentAlignment.MiddleLeft
            _deltaConRutaLabel.Text = "Con ruta" & Environment.NewLine & "SIN CAMBIO  |  sin cambio (0/0)"
            deltaLayout.Controls.Add(_deltaConRutaLabel, 0, 3)
            insightsLayout.Controls.Add(deltaCard, 0, 0)

            Dim alertCard As Panel = CreateDashboardSurfaceCard(Color.FromArgb(247, 250, 253))
            alertCard.Margin = New Padding(0, 0, 0, 10)
            alertCard.Padding = New Padding(14, 12, 14, 12)
            Dim alertLayout As New TableLayoutPanel()
            alertLayout.Dock = DockStyle.Fill
            alertLayout.Margin = New Padding(0)
            alertLayout.Padding = New Padding(0)
            alertLayout.BackColor = alertCard.BackColor
            alertLayout.ColumnCount = 1
            alertLayout.RowCount = 2
            alertLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            alertLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 24.0F))
            alertLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            alertCard.Controls.Add(alertLayout)

            Dim alertHeader As New TableLayoutPanel()
            alertHeader.Dock = DockStyle.Fill
            alertHeader.Margin = New Padding(0)
            alertHeader.Padding = New Padding(0)
            alertHeader.BackColor = alertCard.BackColor
            alertHeader.ColumnCount = 2
            alertHeader.RowCount = 1
            alertHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            alertHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 94.0F))
            Dim alertTitle As New Label()
            alertTitle.Text = "Alertas rápidas"
            alertTitle.Dock = DockStyle.Fill
            alertTitle.Font = New Font("Segoe UI Semibold", 9.75!, FontStyle.Bold, GraphicsUnit.Point)
            alertTitle.ForeColor = Color.FromArgb(45, 60, 88)
            alertTitle.TextAlign = ContentAlignment.MiddleLeft
            _alertsStateLabel = BuildInsightStateChip("ESTABLE")
            _alertsStateLabel.Dock = DockStyle.Right
            alertHeader.Controls.Add(alertTitle, 0, 0)
            alertHeader.Controls.Add(_alertsStateLabel, 1, 0)
            alertLayout.Controls.Add(alertHeader, 0, 0)

            _alertsBodyLabel = New Label()
            _alertsBodyLabel.Text = "- Sin alertas operativas para hoy."
            _alertsBodyLabel.Dock = DockStyle.Fill
            _alertsBodyLabel.AutoEllipsis = False
            _alertsBodyLabel.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular, GraphicsUnit.Point)
            _alertsBodyLabel.ForeColor = Color.FromArgb(80, 96, 120)
            _alertsBodyLabel.Padding = New Padding(0, 8, 0, 0)
            alertLayout.Controls.Add(_alertsBodyLabel, 0, 1)
            insightsLayout.Controls.Add(alertCard, 0, 1)

            Dim rutasCard As Panel = CreateDashboardSurfaceCard(Color.FromArgb(247, 250, 253))
            rutasCard.Margin = New Padding(0)
            rutasCard.Padding = New Padding(14, 12, 14, 12)
            Dim rutasLayout As New TableLayoutPanel()
            rutasLayout.Dock = DockStyle.Fill
            rutasLayout.Margin = New Padding(0)
            rutasLayout.Padding = New Padding(0)
            rutasLayout.BackColor = rutasCard.BackColor
            rutasLayout.ColumnCount = 1
            rutasLayout.RowCount = 2
            rutasLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            rutasLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 24.0F))
            rutasLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            rutasCard.Controls.Add(rutasLayout)

            Dim rutasHeader As New TableLayoutPanel()
            rutasHeader.Dock = DockStyle.Fill
            rutasHeader.Margin = New Padding(0)
            rutasHeader.Padding = New Padding(0)
            rutasHeader.BackColor = rutasCard.BackColor
            rutasHeader.ColumnCount = 2
            rutasHeader.RowCount = 1
            rutasHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            rutasHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 94.0F))
            Dim rutasTitle As New Label()
            rutasTitle.Text = "Top rutas del día"
            rutasTitle.Dock = DockStyle.Fill
            rutasTitle.Font = New Font("Segoe UI Semibold", 9.75!, FontStyle.Bold, GraphicsUnit.Point)
            rutasTitle.ForeColor = Color.FromArgb(45, 60, 88)
            rutasTitle.TextAlign = ContentAlignment.MiddleLeft
            _topRutasStateLabel = BuildInsightStateChip("SIN DATOS")
            _topRutasStateLabel.Dock = DockStyle.Right
            rutasHeader.Controls.Add(rutasTitle, 0, 0)
            rutasHeader.Controls.Add(_topRutasStateLabel, 1, 0)
            rutasLayout.Controls.Add(rutasHeader, 0, 0)

            _topRutasBodyLabel = New Label()
            _topRutasBodyLabel.Text = "- Sin datos de rutas para hoy."
            _topRutasBodyLabel.Dock = DockStyle.Fill
            _topRutasBodyLabel.AutoEllipsis = False
            _topRutasBodyLabel.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular, GraphicsUnit.Point)
            _topRutasBodyLabel.ForeColor = Color.FromArgb(80, 96, 120)
            _topRutasBodyLabel.Padding = New Padding(0, 8, 0, 0)
            rutasLayout.Controls.Add(_topRutasBodyLabel, 0, 1)
            insightsLayout.Controls.Add(rutasCard, 0, 2)

            If compactMode Then
                contentLayout.Controls.Add(chartsSection, 0, 0)
                contentLayout.Controls.Add(insightsLayout, 0, 1)
            Else
                contentLayout.Controls.Add(chartsSection, 0, 0)
                contentLayout.Controls.Add(insightsLayout, 1, 0)
            End If
        Finally
            _contentHost.ResumeLayout(True)
        End Try

        If _lastSnapshot IsNot Nothing Then
            ApplyDashboardSnapshotToSurface(_lastSnapshot)
        End If
    End Sub

    Private Function CreateDashboardSurfaceCard(Optional ByVal cardBackColor As Color = Nothing) As Panel
        Dim card As New Panel()
        card.BackColor = If(cardBackColor.IsEmpty, Color.White, cardBackColor)
        card.Dock = DockStyle.Fill
        card.Margin = New Padding(0)
        card.Padding = New Padding(0)
        card.BorderStyle = BorderStyle.FixedSingle
        Return card
    End Function

    Private Function BuildKpiCard(ByVal definition As DashboardKpiDefinition, ByRef valueLabel As Label, ByRef sparkPanel As Panel) As Panel
        If definition Is Nothing Then
            Throw New InvalidOperationException("No se encontro configuracion de KPI para construir la tarjeta.")
        End If

        Dim card As New Panel()
        card.BackColor = Color.White
        card.Dock = DockStyle.Fill
        card.BorderStyle = BorderStyle.FixedSingle
        card.Cursor = Cursors.Hand
        card.Padding = New Padding(0)

        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.Margin = New Padding(0)
        layout.Padding = New Padding(16, 14, 16, 14)
        layout.BackColor = card.BackColor
        layout.ColumnCount = 2
        layout.RowCount = 3
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 64.0F))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 36.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 24.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 24.0F))
        card.Controls.Add(layout)

        Dim accent As New Panel()
        accent.Dock = DockStyle.Top
        accent.Height = 4
        accent.BackColor = definition.AccentColor
        card.Controls.Add(accent)

        Dim titleLabel As New Label()
        titleLabel.Text = definition.Title
        titleLabel.Font = New Font("Segoe UI", 9.75!, FontStyle.Regular, GraphicsUnit.Point)
        titleLabel.ForeColor = Color.FromArgb(100, 112, 131)
        titleLabel.Dock = DockStyle.Fill
        titleLabel.AutoEllipsis = True
        titleLabel.TextAlign = ContentAlignment.MiddleLeft
        layout.Controls.Add(titleLabel, 0, 0)

        Dim actionLabel As New Label()
        actionLabel.Text = "Ampliar"
        actionLabel.Dock = DockStyle.Right
        actionLabel.AutoSize = False
        actionLabel.Size = New Size(92, 22)
        actionLabel.Margin = New Padding(0, 0, 0, 0)
        actionLabel.Padding = New Padding(8, 0, 8, 0)
        actionLabel.TextAlign = ContentAlignment.MiddleCenter
        actionLabel.Font = New Font("Segoe UI Semibold", 8.5!, FontStyle.Bold, GraphicsUnit.Point)
        actionLabel.BackColor = Color.FromArgb(241, 246, 252)
        actionLabel.ForeColor = definition.AccentColor
        layout.Controls.Add(actionLabel, 1, 0)

        valueLabel = New Label()
        valueLabel.Text = "0"
        valueLabel.Dock = DockStyle.Fill
        valueLabel.Font = New Font("Segoe UI Semibold", 24.0!, FontStyle.Bold, GraphicsUnit.Point)
        valueLabel.ForeColor = Color.FromArgb(25, 38, 62)
        valueLabel.AutoSize = False
        valueLabel.TextAlign = ContentAlignment.MiddleLeft
        layout.Controls.Add(valueLabel, 0, 1)

        Dim hintLabel As New Label()
        hintLabel.Text = "Corte del dia"
        hintLabel.Font = New Font("Segoe UI", 8.75!, FontStyle.Regular, GraphicsUnit.Point)
        hintLabel.ForeColor = Color.FromArgb(120, 132, 148)
        hintLabel.Dock = DockStyle.Fill
        hintLabel.AutoEllipsis = True
        hintLabel.TextAlign = ContentAlignment.MiddleLeft
        layout.Controls.Add(hintLabel, 0, 2)

        sparkPanel = New Panel()
        sparkPanel.Dock = DockStyle.Fill
        sparkPanel.Margin = New Padding(10, 4, 0, 2)
        sparkPanel.BackColor = Color.FromArgb(247, 250, 255)
        sparkPanel.Tag = New SparklineData() With {
            .Values = New List(Of Integer)(),
            .Labels = New List(Of String)(),
            .LineColor = definition.AccentColor,
            .FillColor = Color.FromArgb(96, definition.AccentColor),
            .ShowPointValues = False
        }
        AddHandler sparkPanel.Paint, AddressOf SparklinePanel_Paint
        layout.Controls.Add(sparkPanel, 1, 1)
        layout.SetRowSpan(sparkPanel, 2)

        WireKpiCardInteractions(card, definition.Key, definition.Hint)
        Return card
    End Function

    Private Sub WireKpiCardInteractions(ByVal card As Panel, ByVal navKey As String, ByVal hint As String)
        If card Is Nothing Then
            Exit Sub
        End If

        If _navTooltip IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(hint) Then
            _navTooltip.SetToolTip(card, hint)
        End If

        AddHandler card.MouseEnter, Sub(sender As Object, e As EventArgs)
                                        SetKpiCardBackColor(card, Color.FromArgb(248, 251, 255))
                                    End Sub
        AddHandler card.MouseLeave, Sub(sender As Object, e As EventArgs)
                                        SetKpiCardBackColor(card, Color.White)
                                    End Sub

        WireClickRecursive(card, Sub() NavigateFromKpi(navKey))
    End Sub

    Private Sub SetKpiCardBackColor(ByVal card As Panel, ByVal backColor As Color)
        If card Is Nothing Then
            Exit Sub
        End If

        card.BackColor = backColor
        For Each child As Control In card.Controls
            If TypeOf child Is TableLayoutPanel Then
                child.BackColor = backColor
            End If
        Next
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

        ShowDashboardKpiModal(navKey)
    End Sub

    Private Sub WireChartCardInteractions(ByVal card As Control, ByVal chartKey As String, ByVal hint As String)
        If card Is Nothing OrElse String.IsNullOrWhiteSpace(chartKey) Then
            Exit Sub
        End If

        SetCursorRecursive(card, Cursors.Hand)
        If _navTooltip IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(hint) Then
            _navTooltip.SetToolTip(card, hint)
        End If

        WireClickRecursive(card, Sub() ShowDashboardChartModal(chartKey))
    End Sub

    Private Sub SetCursorRecursive(ByVal ctrl As Control, ByVal cursor As Cursor)
        If ctrl Is Nothing Then
            Exit Sub
        End If

        ctrl.Cursor = cursor
        For Each child As Control In ctrl.Controls
            SetCursorRecursive(child, cursor)
        Next
    End Sub

    Private Sub ShowDashboardKpiModal(ByVal kpiKey As String)
        If _owner Is Nothing Then
            Exit Sub
        End If
        If _lastSnapshot Is Nothing Then
            MessageBox.Show(_owner, "No hay datos del dashboard para ampliar en este momento.", "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        Try
            Using dialog As Form = BuildDashboardKpiModal(_lastSnapshot, kpiKey)
                dialog.ShowDialog(_owner)
            End Using
        Catch ex As Exception
            ErrorLogger.LogException("UIShellHost.ShowDashboardKpiModal", ex)
            MessageBox.Show(_owner, "No se pudo abrir la vista ampliada del dashboard.", "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ShowDashboardChartModal(ByVal chartKey As String)
        If _owner Is Nothing Then
            Exit Sub
        End If
        If _lastSnapshot Is Nothing Then
            MessageBox.Show(_owner, "No hay datos del dashboard para ampliar en este momento.", "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        Try
            Using dialog As Form = BuildDashboardChartModal(_lastSnapshot, chartKey)
                dialog.ShowDialog(_owner)
            End Using
        Catch ex As Exception
            ErrorLogger.LogException("UIShellHost.ShowDashboardChartModal", ex)
            MessageBox.Show(_owner, "No se pudo abrir la vista ampliada del grafico.", "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function BuildDashboardKpiModal(ByVal snapshot As DashboardContracts.Snapshot, ByVal kpiKey As String) As Form
        Dim definition As DashboardKpiDefinition = GetDashboardKpiDefinition(kpiKey)
        If definition Is Nothing Then
            Throw New InvalidOperationException("No se encontro configuracion para la KPI seleccionada.")
        End If

        Dim dialog As New Form()
        dialog.Text = definition.Title
        dialog.StartPosition = FormStartPosition.CenterParent
        dialog.FormBorderStyle = FormBorderStyle.Sizable
        dialog.ShowInTaskbar = False
        dialog.MinimizeBox = False
        dialog.MaximizeBox = True
        dialog.ClientSize = New Size(1160, 820)
        dialog.MinimumSize = New Size(1020, 720)
        dialog.BackColor = Color.FromArgb(244, 247, 252)
        dialog.Font = New Font("Segoe UI", 9.75!, FontStyle.Regular, GraphicsUnit.Point)
        dialog.Opacity = 0.0R

        Dim root As New TableLayoutPanel()
        root.Dock = DockStyle.Fill
        root.Margin = New Padding(0)
        root.Padding = New Padding(24, 22, 24, 18)
        root.BackColor = dialog.BackColor
        root.ColumnCount = 1
        root.RowCount = 3
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 68.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 56.0F))
        dialog.Controls.Add(root)

        Dim headerLayout As New TableLayoutPanel()
        headerLayout.Dock = DockStyle.Fill
        headerLayout.Margin = New Padding(0)
        headerLayout.Padding = New Padding(0)
        headerLayout.BackColor = Color.Transparent
        headerLayout.ColumnCount = 1
        headerLayout.RowCount = 2
        headerLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        headerLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0F))
        headerLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        root.Controls.Add(headerLayout, 0, 0)

        Dim titleLabel As New Label()
        titleLabel.Text = definition.Title
        titleLabel.Dock = DockStyle.Fill
        titleLabel.Font = New Font("Segoe UI Semibold", 18.0!, FontStyle.Bold, GraphicsUnit.Point)
        titleLabel.ForeColor = Color.FromArgb(29, 41, 63)
        titleLabel.TextAlign = ContentAlignment.MiddleLeft
        headerLayout.Controls.Add(titleLabel, 0, 0)

        Dim subtitleLabel As New Label()
        subtitleLabel.Text = definition.DetailText & " con tendencia de los ultimos 7 dias lectivos."
        subtitleLabel.Dock = DockStyle.Fill
        subtitleLabel.Font = New Font("Segoe UI", 9.75!, FontStyle.Regular, GraphicsUnit.Point)
        subtitleLabel.ForeColor = Color.FromArgb(101, 114, 133)
        subtitleLabel.TextAlign = ContentAlignment.MiddleLeft
        headerLayout.Controls.Add(subtitleLabel, 0, 1)

        Dim cardHost As New Panel()
        cardHost.Dock = DockStyle.Fill
        cardHost.Margin = New Padding(0)
        cardHost.Padding = New Padding(0, 8, 0, 12)
        cardHost.BackColor = Color.Transparent
        root.Controls.Add(cardHost, 0, 1)

        Dim selectedCard As Panel = BuildDashboardModalKpiCard(definition, snapshot)
        selectedCard.Margin = New Padding(0)
        selectedCard.Dock = DockStyle.Fill
        cardHost.Controls.Add(selectedCard)

        Dim footer As New TableLayoutPanel()
        footer.Dock = DockStyle.Fill
        footer.Margin = New Padding(0)
        footer.Padding = New Padding(0)
        footer.BackColor = Color.Transparent
        footer.ColumnCount = 2
        footer.RowCount = 1
        footer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        footer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 132.0F))
        root.Controls.Add(footer, 0, 2)

        Dim footerText As New Label()
        footerText.Text = "Corte del dia y tendencia de los ultimos 7 dias lectivos."
        footerText.Dock = DockStyle.Fill
        footerText.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular, GraphicsUnit.Point)
        footerText.ForeColor = Color.FromArgb(109, 120, 138)
        footerText.TextAlign = ContentAlignment.MiddleLeft
        footer.Controls.Add(footerText, 0, 0)

        Dim closeButton As New Button()
        closeButton.Text = "Cerrar"
        closeButton.Dock = DockStyle.Right
        closeButton.Width = 116
        closeButton.Height = 36
        closeButton.BackColor = Color.FromArgb(54, 71, 95)
        closeButton.ForeColor = Color.White
        closeButton.FlatStyle = FlatStyle.Flat
        closeButton.FlatAppearance.BorderSize = 0
        closeButton.Cursor = Cursors.Hand
        closeButton.DialogResult = DialogResult.OK
        footer.Controls.Add(closeButton, 1, 0)

        dialog.AcceptButton = closeButton
        dialog.CancelButton = closeButton
        AddHandler dialog.Shown, Sub(sender As Object, e As EventArgs) StartFadeIn(dialog)

        Return dialog
    End Function

    Private Function BuildDashboardChartModal(ByVal snapshot As DashboardContracts.Snapshot, ByVal chartKey As String) As Form
        Dim definition As DashboardChartDefinition = GetDashboardChartDefinition(chartKey)
        If definition Is Nothing Then
            Throw New InvalidOperationException("No se encontro configuracion para el grafico seleccionado.")
        End If

        Dim dialog As New Form()
        dialog.Text = definition.Title
        dialog.StartPosition = FormStartPosition.CenterParent
        dialog.FormBorderStyle = FormBorderStyle.Sizable
        dialog.ShowInTaskbar = False
        dialog.MinimizeBox = False
        dialog.MaximizeBox = True
        dialog.ClientSize = New Size(1160, 760)
        dialog.MinimumSize = New Size(980, 660)
        dialog.BackColor = Color.FromArgb(244, 247, 252)
        dialog.Font = New Font("Segoe UI", 9.75!, FontStyle.Regular, GraphicsUnit.Point)
        dialog.Opacity = 0.0R

        Dim root As New TableLayoutPanel()
        root.Dock = DockStyle.Fill
        root.Margin = New Padding(0)
        root.Padding = New Padding(24, 22, 24, 18)
        root.BackColor = dialog.BackColor
        root.ColumnCount = 1
        root.RowCount = 3
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 68.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 56.0F))
        dialog.Controls.Add(root)

        Dim headerLayout As New TableLayoutPanel()
        headerLayout.Dock = DockStyle.Fill
        headerLayout.Margin = New Padding(0)
        headerLayout.Padding = New Padding(0)
        headerLayout.BackColor = Color.Transparent
        headerLayout.ColumnCount = 1
        headerLayout.RowCount = 2
        headerLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        headerLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0F))
        headerLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        root.Controls.Add(headerLayout, 0, 0)

        Dim titleLabel As New Label()
        titleLabel.Text = definition.Title
        titleLabel.Dock = DockStyle.Fill
        titleLabel.Font = New Font("Segoe UI Semibold", 18.0!, FontStyle.Bold, GraphicsUnit.Point)
        titleLabel.ForeColor = Color.FromArgb(29, 41, 63)
        titleLabel.TextAlign = ContentAlignment.MiddleLeft
        headerLayout.Controls.Add(titleLabel, 0, 0)

        Dim subtitleLabel As New Label()
        subtitleLabel.Text = definition.Subtitle & " con cantidades visibles al ampliar."
        subtitleLabel.Dock = DockStyle.Fill
        subtitleLabel.Font = New Font("Segoe UI", 9.75!, FontStyle.Regular, GraphicsUnit.Point)
        subtitleLabel.ForeColor = Color.FromArgb(101, 114, 133)
        subtitleLabel.TextAlign = ContentAlignment.MiddleLeft
        headerLayout.Controls.Add(subtitleLabel, 0, 1)

        Dim chartPanel As Panel = Nothing
        Dim chartCard As Panel = BuildDashboardModalChartCard(definition.Title, definition.Subtitle, chartPanel)
        chartCard.Dock = DockStyle.Fill
        chartCard.Margin = New Padding(0)
        root.Controls.Add(chartCard, 0, 1)

        Dim footer As New TableLayoutPanel()
        footer.Dock = DockStyle.Fill
        footer.Margin = New Padding(0)
        footer.Padding = New Padding(0)
        footer.BackColor = Color.Transparent
        footer.ColumnCount = 2
        footer.RowCount = 1
        footer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        footer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 132.0F))
        root.Controls.Add(footer, 0, 2)

        Dim footerText As New Label()
        footerText.Text = "Vista ampliada del grafico seleccionado."
        footerText.Dock = DockStyle.Fill
        footerText.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular, GraphicsUnit.Point)
        footerText.ForeColor = Color.FromArgb(109, 120, 138)
        footerText.TextAlign = ContentAlignment.MiddleLeft
        footer.Controls.Add(footerText, 0, 0)

        Dim closeButton As New Button()
        closeButton.Text = "Cerrar"
        closeButton.Dock = DockStyle.Right
        closeButton.Width = 116
        closeButton.Height = 36
        closeButton.BackColor = Color.FromArgb(54, 71, 95)
        closeButton.ForeColor = Color.White
        closeButton.FlatStyle = FlatStyle.Flat
        closeButton.FlatAppearance.BorderSize = 0
        closeButton.Cursor = Cursors.Hand
        closeButton.DialogResult = DialogResult.OK
        footer.Controls.Add(closeButton, 1, 0)

        dialog.AcceptButton = closeButton
        dialog.CancelButton = closeButton
        AddHandler dialog.Shown,
            Sub(sender As Object, e As EventArgs)
                StartFadeIn(dialog)
                RenderDashboardChartModal(chartKey, chartPanel, snapshot.Series)
            End Sub
        AddHandler chartPanel.Resize,
            Sub(sender As Object, e As EventArgs)
                RenderDashboardChartModal(chartKey, chartPanel, snapshot.Series)
            End Sub

        Return dialog
    End Function

    Private Function GetDashboardKpiDefinition(ByVal kpiKey As String) As DashboardKpiDefinition
        Select Case (If(kpiKey, String.Empty)).Trim().ToLowerInvariant()
            Case "transporte_ingresos"
                Return New DashboardKpiDefinition() With {
                    .Key = "transporte_ingresos",
                    .Title = "Ingresos / Institucion",
                    .AccentColor = Color.FromArgb(53, 154, 129),
                    .Hint = "Ampliar grafico",
                    .DetailText = "Registros de ingreso del dia",
                    .CurrentValueSelector = Function(item) item.MarcasTransporteHoy,
                    .SeriesSelector = Function(item) item.Transporte
                }
            Case "transporte_con_ruta"
                Return New DashboardKpiDefinition() With {
                    .Key = "transporte_con_ruta",
                    .Title = "Becados / Transporte",
                    .AccentColor = Color.FromArgb(28, 132, 110),
                    .Hint = "Ampliar grafico",
                    .DetailText = "Estudiantes con ruta registrados hoy",
                    .CurrentValueSelector = Function(item) item.EstudiantesConRutaHoy,
                    .SeriesSelector = Function(item) item.TransporteConRuta
                }
            Case "comedor_entradas"
                Return New DashboardKpiDefinition() With {
                    .Key = "comedor_entradas",
                    .Title = "Entradas / Comedor",
                    .AccentColor = Color.FromArgb(74, 126, 196),
                    .Hint = "Ampliar grafico",
                    .DetailText = "Entradas de comedor del dia",
                    .CurrentValueSelector = Function(item) item.MarcasComedorHoy,
                    .SeriesSelector = Function(item) item.Comedor
                }
            Case "comedor_becados"
                Return New DashboardKpiDefinition() With {
                    .Key = "comedor_becados",
                    .Title = "Becados / Comedor",
                    .AccentColor = Color.FromArgb(37, 92, 165),
                    .Hint = "Ampliar grafico",
                    .DetailText = "Becados de comedor registrados hoy",
                    .CurrentValueSelector = Function(item) item.BecadosComedorHoy,
                    .SeriesSelector = Function(item) item.ComedorBecados
                }
            Case Else
                Return Nothing
        End Select
    End Function

    Private Function GetDashboardChartDefinition(ByVal chartKey As String) As DashboardChartDefinition
        Select Case (If(chartKey, String.Empty)).Trim().ToLowerInvariant()
            Case "comedor_chart"
                Return New DashboardChartDefinition() With {
                    .Key = "comedor_chart",
                    .Title = "Uso de Comedor",
                    .Subtitle = "Entradas y becados por dia lectivo",
                    .Hint = "Ampliar grafico"
                }
            Case "transporte_chart"
                Return New DashboardChartDefinition() With {
                    .Key = "transporte_chart",
                    .Title = "Uso de Transporte",
                    .Subtitle = "Abordajes y estudiantes con ruta por dia lectivo",
                    .Hint = "Ampliar grafico"
                }
            Case Else
                Return Nothing
        End Select
    End Function

    Private Function BuildDashboardModalKpiCard(ByVal definition As DashboardKpiDefinition, ByVal snapshot As DashboardContracts.Snapshot) As Panel
        If definition Is Nothing Then
            Throw New InvalidOperationException("No se encontro configuracion de KPI para el modal.")
        End If

        Dim card As Panel = CreateDashboardSurfaceCard(Color.White)
        card.Padding = New Padding(0)

        Dim accent As New Panel()
        accent.Dock = DockStyle.Top
        accent.Height = 5
        accent.BackColor = definition.AccentColor
        card.Controls.Add(accent)

        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.Margin = New Padding(0)
        layout.Padding = New Padding(22, 20, 22, 20)
        layout.BackColor = Color.White
        layout.ColumnCount = 1
        layout.RowCount = 4
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 62.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        card.Controls.Add(layout)

        Dim titleLabel As New Label()
        titleLabel.Text = definition.Title
        titleLabel.Dock = DockStyle.Fill
        titleLabel.Font = New Font("Segoe UI Semibold", 11.25!, FontStyle.Bold, GraphicsUnit.Point)
        titleLabel.ForeColor = Color.FromArgb(57, 71, 96)
        titleLabel.TextAlign = ContentAlignment.MiddleLeft
        layout.Controls.Add(titleLabel, 0, 0)

        Dim valueLabel As New Label()
        valueLabel.Text = ResolveCurrentKpiValue(snapshot, definition).ToString("N0")
        valueLabel.Dock = DockStyle.Fill
        valueLabel.Font = New Font("Segoe UI Semibold", 30.0!, FontStyle.Bold, GraphicsUnit.Point)
        valueLabel.ForeColor = Color.FromArgb(25, 38, 62)
        valueLabel.TextAlign = ContentAlignment.MiddleLeft
        layout.Controls.Add(valueLabel, 0, 1)

        Dim sparkPanel As New Panel()
        sparkPanel.Dock = DockStyle.Fill
        sparkPanel.Margin = New Padding(0, 8, 0, 8)
        sparkPanel.BackColor = Color.FromArgb(247, 250, 255)
        sparkPanel.Tag = New SparklineData() With {
            .Values = ExtractMetricSeries(If(snapshot Is Nothing, Nothing, snapshot.Series), definition.SeriesSelector),
            .Labels = ExtractMetricLabels(If(snapshot Is Nothing, Nothing, snapshot.Series)),
            .LineColor = definition.AccentColor,
            .FillColor = Color.FromArgb(96, definition.AccentColor),
            .ShowPointValues = True
        }
        AddHandler sparkPanel.Paint, AddressOf SparklinePanel_Paint
        layout.Controls.Add(sparkPanel, 0, 2)

        Dim detailLabel As New Label()
        detailLabel.Text = definition.DetailText & "  |  Ultimos 7 dias lectivos"
        detailLabel.Dock = DockStyle.Fill
        detailLabel.Font = New Font("Segoe UI", 9.25!, FontStyle.Regular, GraphicsUnit.Point)
        detailLabel.ForeColor = Color.FromArgb(120, 132, 148)
        detailLabel.TextAlign = ContentAlignment.MiddleLeft
        layout.Controls.Add(detailLabel, 0, 3)

        Return card
    End Function

    Private Function BuildDashboardModalChartCard(ByVal title As String, ByVal subtitle As String, ByRef chartPanel As Panel) As Panel
        Dim card As Panel = CreateDashboardSurfaceCard(Color.FromArgb(252, 253, 255))
        card.Margin = New Padding(0, 0, 0, 12)
        card.Padding = New Padding(18, 16, 18, 18)

        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.Margin = New Padding(0)
        layout.Padding = New Padding(0)
        layout.BackColor = card.BackColor
        layout.ColumnCount = 1
        layout.RowCount = 3
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 26.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 22.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        card.Controls.Add(layout)

        Dim titleLabel As New Label()
        titleLabel.Text = title
        titleLabel.Dock = DockStyle.Fill
        titleLabel.Font = New Font("Segoe UI Semibold", 12.0!, FontStyle.Bold, GraphicsUnit.Point)
        titleLabel.ForeColor = Color.FromArgb(38, 52, 79)
        titleLabel.TextAlign = ContentAlignment.MiddleLeft
        layout.Controls.Add(titleLabel, 0, 0)

        Dim subtitleLabel As New Label()
        subtitleLabel.Text = subtitle
        subtitleLabel.Dock = DockStyle.Fill
        subtitleLabel.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular, GraphicsUnit.Point)
        subtitleLabel.ForeColor = Color.FromArgb(97, 111, 131)
        subtitleLabel.TextAlign = ContentAlignment.MiddleLeft
        layout.Controls.Add(subtitleLabel, 0, 1)

        chartPanel = New Panel()
        chartPanel.Dock = DockStyle.Fill
        chartPanel.Margin = New Padding(0, 8, 0, 0)
        chartPanel.BackColor = Color.FromArgb(248, 251, 255)
        layout.Controls.Add(chartPanel, 0, 2)

        Return card
    End Function

    Private Function ResolveCurrentKpiValue(ByVal snapshot As DashboardContracts.Snapshot, ByVal definition As DashboardKpiDefinition) As Integer
        If snapshot Is Nothing OrElse definition Is Nothing OrElse definition.CurrentValueSelector Is Nothing Then
            Return 0
        End If

        Return definition.CurrentValueSelector(snapshot)
    End Function

    Private Sub RenderDashboardChartModal(ByVal chartKey As String, ByVal targetPanel As Panel, ByVal series As List(Of DashboardContracts.DailyMetric))
        Select Case (If(chartKey, String.Empty)).Trim().ToLowerInvariant()
            Case "comedor_chart"
                RenderComedorChartOnPanel(targetPanel, series, True)
            Case "transporte_chart"
                RenderTransporteChartOnPanel(targetPanel, series, True)
        End Select
    End Sub

    Private Function ExtractMetricSeries(ByVal series As List(Of DashboardContracts.DailyMetric), ByVal selector As Func(Of DashboardContracts.DailyMetric, Integer)) As List(Of Integer)
        Dim values As New List(Of Integer)()
        If series Is Nothing OrElse selector Is Nothing Then
            Return values
        End If

        For Each item As DashboardContracts.DailyMetric In series
            If item Is Nothing Then
                Continue For
            End If
            values.Add(selector(item))
        Next

        Return values
    End Function

    Private Function ExtractMetricLabels(ByVal series As List(Of DashboardContracts.DailyMetric)) As List(Of String)
        Dim labels As New List(Of String)()
        If series Is Nothing Then
            Return labels
        End If

        For Each item As DashboardContracts.DailyMetric In series
            If item Is Nothing Then
                Continue For
            End If

            If Not String.IsNullOrWhiteSpace(item.DayNameShort) Then
                labels.Add(item.DayNameShort)
            Else
                labels.Add(String.Empty)
            End If
        Next

        Return labels
    End Function

    Private Sub StartFadeIn(ByVal dialog As Form)
        If dialog Is Nothing OrElse dialog.IsDisposed Then
            Exit Sub
        End If

        dialog.Opacity = 0.0R
        Dim fadeTimer As New Timer()
        fadeTimer.Interval = KpiModalFadeIntervalMs

        AddHandler fadeTimer.Tick,
            Sub(sender As Object, e As EventArgs)
                If dialog.IsDisposed Then
                    fadeTimer.Stop()
                    fadeTimer.Dispose()
                    Exit Sub
                End If

                dialog.Opacity = Math.Min(1.0R, dialog.Opacity + KpiModalFadeStep)
                If dialog.Opacity >= 1.0R Then
                    fadeTimer.Stop()
                    fadeTimer.Dispose()
                End If
            End Sub

        AddHandler dialog.FormClosed,
            Sub(sender As Object, e As FormClosedEventArgs)
                If fadeTimer IsNot Nothing Then
                    fadeTimer.Stop()
                    fadeTimer.Dispose()
                End If
            End Sub

        fadeTimer.Start()
    End Sub

    Private Sub UpdateKpiSparklines(ByVal series As List(Of DashboardContracts.DailyMetric))
        If series Is Nothing OrElse series.Count = 0 Then
            SetSparkValues(_sparkBecadosComedor, New List(Of Integer)())
            SetSparkValues(_sparkConRutaTransporte, New List(Of Integer)())
            SetSparkValues(_sparkMarcasComedor, New List(Of Integer)())
            SetSparkValues(_sparkMarcasTransporte, New List(Of Integer)())
            Exit Sub
        End If

        Dim becadosComedor As New List(Of Integer)()
        Dim conRutaTransporte As New List(Of Integer)()
        Dim marcasComedor As New List(Of Integer)()
        Dim marcasTransporte As New List(Of Integer)()

        For Each item As DashboardContracts.DailyMetric In series
            becadosComedor.Add(item.ComedorBecados)
            conRutaTransporte.Add(item.TransporteConRuta)
            marcasComedor.Add(item.Comedor)
            marcasTransporte.Add(item.Transporte)
        Next

        SetSparkValues(_sparkBecadosComedor, becadosComedor)
        SetSparkValues(_sparkConRutaTransporte, conRutaTransporte)
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
        Dim bottomPad As Integer = If(data.ShowPointValues AndAlso data.Labels IsNot Nothing AndAlso data.Labels.Count = values.Count, 22, 4)
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
            If data.ShowPointValues Then
                For Each point As PointF In pts
                    e.Graphics.FillEllipse(nodeBrush, point.X - 3.0F, point.Y - 3.0F, 6.0F, 6.0F)
                Next
            Else
                Dim last As PointF = pts(pts.Count - 1)
                e.Graphics.FillEllipse(nodeBrush, last.X - 2.5F, last.Y - 2.5F, 5.0F, 5.0F)
            End If
        End Using

        If data.ShowPointValues Then
            Using labelFont As New Font("Segoe UI Semibold", 8.0!, FontStyle.Bold, GraphicsUnit.Point)
                For i As Integer = 0 To pts.Count - 1
                    DrawSparkPointValue(e.Graphics, pts(i), values(i), data.LineColor, panel.ClientSize, labelFont)
                Next
            End Using

            If data.Labels IsNot Nothing AndAlso data.Labels.Count = pts.Count Then
                Using axisFont As New Font("Segoe UI", 7.25!, FontStyle.Regular, GraphicsUnit.Point)
                    Using axisBrush As New SolidBrush(Color.FromArgb(96, 109, 127))
                        For i As Integer = 0 To pts.Count - 1
                            Dim labelText As String = data.Labels(i)
                            If String.IsNullOrWhiteSpace(labelText) Then
                                Continue For
                            End If

                            Dim labelSize As SizeF = e.Graphics.MeasureString(labelText, axisFont)
                            Dim labelX As Single = pts(i).X - (labelSize.Width / 2.0F)
                            labelX = Math.Max(0.0F, Math.Min(labelX, panel.ClientSize.Width - labelSize.Width))
                            Dim labelY As Single = panel.ClientSize.Height - labelSize.Height - 2.0F
                            e.Graphics.DrawString(labelText, axisFont, axisBrush, labelX, labelY)
                        Next
                    End Using
                End Using
            End If
        End If
    End Sub

    Private Sub RenderComedorChart(ByVal series As List(Of DashboardContracts.DailyMetric))
        RenderComedorChartOnPanel(_comedorChartPanel, series, False)
    End Sub

    Private Sub RenderComedorChartOnPanel(ByVal targetPanel As Panel, ByVal series As List(Of DashboardContracts.DailyMetric), Optional ByVal showValues As Boolean = False)
        If targetPanel Is Nothing Then
            Exit Sub
        End If

        targetPanel.Controls.Clear()
        If series Is Nothing OrElse series.Count = 0 Then
            Dim empty As New Label()
            empty.Text = "Sin datos para mostrar."
            empty.AutoSize = True
            empty.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
            empty.ForeColor = Color.FromArgb(120, 132, 148)
            empty.Location = New Point(16, 16)
            targetPanel.Controls.Add(empty)
            Exit Sub
        End If
        If targetPanel.Width <= 48 OrElse targetPanel.Height <= 48 Then
            Exit Sub
        End If

        Dim maxValue As Integer = 1
        For Each item As DashboardContracts.DailyMetric In series
            maxValue = Math.Max(maxValue, Math.Max(item.Comedor, item.ComedorBecados))
        Next

        If maxValue <= 0 Then
            Dim empty As New Label()
            empty.Text = "Sin registros de comedor para este periodo."
            empty.AutoSize = True
            empty.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
            empty.ForeColor = Color.FromArgb(120, 132, 148)
            empty.Location = New Point(16, 28)
            targetPanel.Controls.Add(empty)
            Exit Sub
        End If

        Dim n As Integer = series.Count
        Dim groupWidth As Integer = Math.Max(56, (targetPanel.Width - 24) \ Math.Max(1, n))
        Dim chartTopPadding As Integer = If(showValues, 50, 24)
        Dim baseY As Integer = targetPanel.Height - 40
        Dim usableHeight As Integer = Math.Max(70, baseY - chartTopPadding)

        For i As Integer = 0 To n - 1
            Dim item As DashboardContracts.DailyMetric = series(i)
            Dim left As Integer = 12 + (i * groupWidth)
            Dim barAreaWidth As Integer = groupWidth - 12
            Dim barWidth As Integer = Math.Max(10, (barAreaWidth \ 2) - 4)

            Dim comedorHeight As Integer = CInt((item.Comedor / CDbl(maxValue)) * usableHeight)
            Dim becadosHeight As Integer = CInt((item.ComedorBecados / CDbl(maxValue)) * usableHeight)

            Dim barComedor As New Panel()
            barComedor.BackColor = Color.FromArgb(58, 110, 192)
            barComedor.SetBounds(left + 2, baseY - comedorHeight, barWidth, comedorHeight)
            targetPanel.Controls.Add(barComedor)
            If showValues AndAlso item.Comedor > 0 Then
                targetPanel.Controls.Add(CreateBarValueLabel(item.Comedor, left + 2, baseY - comedorHeight, barWidth, Color.FromArgb(58, 110, 192), targetPanel.Width, chartTopPadding - 20))
            End If

            Dim barBecados As New Panel()
            barBecados.BackColor = Color.FromArgb(160, 198, 245)
            barBecados.SetBounds(left + barWidth + 6, baseY - becadosHeight, barWidth, becadosHeight)
            targetPanel.Controls.Add(barBecados)
            If showValues AndAlso item.ComedorBecados > 0 Then
                targetPanel.Controls.Add(CreateBarValueLabel(item.ComedorBecados, left + barWidth + 6, baseY - becadosHeight, barWidth, Color.FromArgb(82, 126, 185), targetPanel.Width, chartTopPadding - 20))
            End If

            Dim axisLabel As New Label()
            axisLabel.Text = GetDashboardAxisLabel(item)
            axisLabel.AutoSize = False
            axisLabel.TextAlign = ContentAlignment.MiddleCenter
            axisLabel.Font = New Font("Segoe UI", 8.5!, FontStyle.Regular, GraphicsUnit.Point)
            axisLabel.ForeColor = Color.FromArgb(96, 109, 127)
            axisLabel.SetBounds(left, baseY + 6, barAreaWidth + 6, 32)
            targetPanel.Controls.Add(axisLabel)
        Next

        Dim legendComedor As New Label()
        legendComedor.Text = "Entradas"
        legendComedor.AutoSize = True
        legendComedor.Font = New Font("Segoe UI", 8.5!, FontStyle.Regular, GraphicsUnit.Point)
        legendComedor.ForeColor = Color.FromArgb(58, 110, 192)
        legendComedor.Location = New Point(12, 8)
        targetPanel.Controls.Add(legendComedor)

        Dim legendBecados As New Label()
        legendBecados.Text = "Becados"
        legendBecados.AutoSize = True
        legendBecados.Font = New Font("Segoe UI", 8.5!, FontStyle.Regular, GraphicsUnit.Point)
        legendBecados.ForeColor = Color.FromArgb(160, 198, 245)
        legendBecados.Location = New Point(112, 8)
        targetPanel.Controls.Add(legendBecados)
    End Sub

    Private Sub RenderTransporteChart(ByVal series As List(Of DashboardContracts.DailyMetric))
        RenderTransporteChartOnPanel(_transporteChartPanel, series, False)
    End Sub

    Private Sub RenderTransporteChartOnPanel(ByVal targetPanel As Panel, ByVal series As List(Of DashboardContracts.DailyMetric), Optional ByVal showValues As Boolean = False)
        If targetPanel Is Nothing Then
            Exit Sub
        End If

        targetPanel.Controls.Clear()
        If series Is Nothing OrElse series.Count = 0 Then
            Exit Sub
        End If
        If targetPanel.Width <= 48 OrElse targetPanel.Height <= 48 Then
            Exit Sub
        End If

        Dim maxValue As Integer = 1
        For Each item As DashboardContracts.DailyMetric In series
            maxValue = Math.Max(maxValue, Math.Max(item.Transporte, item.TransporteConRuta))
        Next

        If maxValue <= 0 Then
            Dim empty As New Label()
            empty.Text = "Sin registros de transporte para este periodo."
            empty.AutoSize = True
            empty.Font = New Font("Segoe UI", 9.5!, FontStyle.Regular, GraphicsUnit.Point)
            empty.ForeColor = Color.FromArgb(120, 132, 148)
            empty.Location = New Point(12, 26)
            targetPanel.Controls.Add(empty)
            Exit Sub
        End If

        Dim n As Integer = series.Count
        Dim groupWidth As Integer = Math.Max(50, (targetPanel.Width - 24) \ Math.Max(1, n))
        Dim chartTopPadding As Integer = If(showValues, 48, 22)
        Dim baseY As Integer = Math.Max(42, targetPanel.Height - 30)
        Dim usableHeight As Integer = Math.Max(28, baseY - chartTopPadding)

        For i As Integer = 0 To n - 1
            Dim item As DashboardContracts.DailyMetric = series(i)
            Dim left As Integer = 12 + (i * groupWidth)
            Dim barAreaWidth As Integer = groupWidth - 10
            Dim barWidth As Integer = Math.Max(8, (barAreaWidth \ 2) - 3)

            Dim conRutaHeight As Integer = CInt((item.TransporteConRuta / CDbl(maxValue)) * usableHeight)
            Dim transporteHeight As Integer = CInt((item.Transporte / CDbl(maxValue)) * usableHeight)

            Dim barConRuta As New Panel()
            barConRuta.BackColor = Color.FromArgb(169, 194, 233)
            barConRuta.SetBounds(left + 2, baseY - conRutaHeight, barWidth, conRutaHeight)
            targetPanel.Controls.Add(barConRuta)
            If showValues AndAlso item.TransporteConRuta > 0 Then
                targetPanel.Controls.Add(CreateBarValueLabel(item.TransporteConRuta, left + 2, baseY - conRutaHeight, barWidth, Color.FromArgb(88, 121, 171), targetPanel.Width, chartTopPadding - 18))
            End If

            Dim barTransporte As New Panel()
            barTransporte.BackColor = Color.FromArgb(53, 154, 129)
            barTransporte.SetBounds(left + barWidth + 6, baseY - transporteHeight, barWidth, transporteHeight)
            targetPanel.Controls.Add(barTransporte)
            If showValues AndAlso item.Transporte > 0 Then
                targetPanel.Controls.Add(CreateBarValueLabel(item.Transporte, left + barWidth + 6, baseY - transporteHeight, barWidth, Color.FromArgb(35, 121, 101), targetPanel.Width, chartTopPadding - 18))
            End If

            Dim axisLabel As New Label()
            axisLabel.Text = GetDashboardAxisLabel(item)
            axisLabel.AutoSize = False
            axisLabel.TextAlign = ContentAlignment.MiddleCenter
            axisLabel.Font = New Font("Segoe UI", 8.25!, FontStyle.Regular, GraphicsUnit.Point)
            axisLabel.ForeColor = Color.FromArgb(96, 109, 127)
            axisLabel.SetBounds(left, baseY + 4, barAreaWidth + 6, 32)
            targetPanel.Controls.Add(axisLabel)
        Next

        Dim legendTransporte As New Label()
        legendTransporte.Text = "Abordajes"
        legendTransporte.AutoSize = True
        legendTransporte.Font = New Font("Segoe UI", 8.5!, FontStyle.Regular, GraphicsUnit.Point)
        legendTransporte.ForeColor = Color.FromArgb(53, 154, 129)
        legendTransporte.Location = New Point(12, 8)
        targetPanel.Controls.Add(legendTransporte)

        Dim legendConRuta As New Label()
        legendConRuta.Text = "Estudiantes con ruta"
        legendConRuta.AutoSize = True
        legendConRuta.Font = New Font("Segoe UI", 8.5!, FontStyle.Regular, GraphicsUnit.Point)
        legendConRuta.ForeColor = Color.FromArgb(169, 194, 233)
        legendConRuta.Location = New Point(96, 8)
        targetPanel.Controls.Add(legendConRuta)
    End Sub

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

    Private Sub UpdateComparisonStatus(ByVal snapshot As DashboardContracts.Snapshot)
        If snapshot Is Nothing Then
            Exit Sub
        End If

        Dim items As List(Of DashboardContracts.DashboardComparisonItem) = If(snapshot.Comparativos, New List(Of DashboardContracts.DashboardComparisonItem)())
        If items.Count = 0 Then
            items = New List(Of DashboardContracts.DashboardComparisonItem) From {
                New DashboardContracts.DashboardComparisonItem With {.Label = "Comedor", .CurrentValue = snapshot.MarcasComedorHoy, .PreviousValue = snapshot.MarcasComedorAyer, .DeltaText = FormatDelta(snapshot.MarcasComedorHoy, snapshot.MarcasComedorAyer), .StatusText = "SIN CAMBIO", .Trend = DashboardContracts.ComparisonTrend.Flat},
                New DashboardContracts.DashboardComparisonItem With {.Label = "Transporte", .CurrentValue = snapshot.MarcasTransporteHoy, .PreviousValue = snapshot.MarcasTransporteAyer, .DeltaText = FormatDelta(snapshot.MarcasTransporteHoy, snapshot.MarcasTransporteAyer), .StatusText = "SIN CAMBIO", .Trend = DashboardContracts.ComparisonTrend.Flat},
                New DashboardContracts.DashboardComparisonItem With {.Label = "Con ruta", .CurrentValue = snapshot.EstudiantesConRutaHoy, .PreviousValue = snapshot.EstudiantesConRutaAyer, .DeltaText = FormatDelta(snapshot.EstudiantesConRutaHoy, snapshot.EstudiantesConRutaAyer), .StatusText = "SIN CAMBIO", .Trend = DashboardContracts.ComparisonTrend.Flat}
            }
        End If

        ApplyComparisonItem(FindComparisonItem(items, "Comedor"), _deltaComedorLabel)
        ApplyComparisonItem(FindComparisonItem(items, "Transporte"), _deltaTransporteLabel)
        ApplyComparisonItem(FindComparisonItem(items, "Con ruta"), _deltaConRutaLabel)
    End Sub

    Private Function FindComparisonItem(ByVal items As List(Of DashboardContracts.DashboardComparisonItem), ByVal label As String) As DashboardContracts.DashboardComparisonItem
        If items Is Nothing Then
            Return Nothing
        End If

        For Each item As DashboardContracts.DashboardComparisonItem In items
            If item Is Nothing Then
                Continue For
            End If
            If String.Equals(If(item.Label, String.Empty).Trim(), label, StringComparison.OrdinalIgnoreCase) Then
                Return item
            End If
        Next

        Return Nothing
    End Function

    Private Sub ApplyComparisonItem(ByVal item As DashboardContracts.DashboardComparisonItem, ByVal target As Label)
        If target Is Nothing Then
            Exit Sub
        End If

        If item Is Nothing Then
            target.Text = "Sin datos" & Environment.NewLine & "SIN CAMBIO  |  sin cambio (0/0)"
            target.ForeColor = Color.FromArgb(139, 110, 31)
            Exit Sub
        End If

        target.Text = item.Label & "  ·  " & item.StatusText & Environment.NewLine & item.DeltaText

        Select Case item.Trend
            Case DashboardContracts.ComparisonTrend.Up
                target.ForeColor = Color.FromArgb(24, 121, 78)
            Case DashboardContracts.ComparisonTrend.Down
                target.ForeColor = Color.FromArgb(173, 52, 61)
            Case DashboardContracts.ComparisonTrend.NoBase
                target.ForeColor = Color.FromArgb(37, 92, 165)
            Case Else
                target.ForeColor = Color.FromArgb(139, 110, 31)
        End Select
    End Sub

    Private Sub UpdateAlertStatus(ByVal alertas As List(Of DashboardContracts.DashboardAlert))
        If _alertsBodyLabel Is Nothing Then
            Exit Sub
        End If

        If alertas Is Nothing OrElse alertas.Count = 0 Then
            _alertsBodyLabel.Text = "- Sin alertas operativas para hoy."
            _alertsBodyLabel.ForeColor = Color.FromArgb(66, 91, 126)
            SetStateChip(_alertsStateLabel, "ESTABLE", Color.FromArgb(222, 244, 229), Color.FromArgb(28, 68, 42))
            Exit Sub
        End If

        Dim ordered As List(Of DashboardContracts.DashboardAlert) = SortAlerts(alertas)
        Dim formatted As New List(Of String)()
        Dim severity As DashboardContracts.AlertSeverity = DashboardContracts.AlertSeverity.Info
        For Each alert As DashboardContracts.DashboardAlert In ordered
            If alert Is Nothing Then
                Continue For
            End If

            Dim raw As String = BuildAlertBody(alert)
            If raw.Length = 0 Then
                Continue For
            End If

            severity = CType(Math.Max(CInt(severity), CInt(alert.Severity)), DashboardContracts.AlertSeverity)
            If formatted.Count < 3 Then
                formatted.Add(raw)
            End If
        Next

        If formatted.Count = 0 Then
            _alertsBodyLabel.Text = "- Sin alertas operativas para hoy."
            _alertsBodyLabel.ForeColor = Color.FromArgb(66, 91, 126)
            SetStateChip(_alertsStateLabel, "ESTABLE", Color.FromArgb(222, 244, 229), Color.FromArgb(28, 68, 42))
            Exit Sub
        End If

        If ordered.Count > formatted.Count Then
            formatted.Add("+ " & (ordered.Count - formatted.Count).ToString() & " alerta(s) adicional(es).")
        End If

        _alertsBodyLabel.Text = String.Join(Environment.NewLine, formatted)
        Select Case severity
            Case DashboardContracts.AlertSeverity.Critical
                _alertsBodyLabel.ForeColor = Color.FromArgb(155, 43, 52)
                SetStateChip(_alertsStateLabel, "ATENCION", Color.FromArgb(252, 230, 232), Color.FromArgb(125, 39, 48))
            Case DashboardContracts.AlertSeverity.Warning
                _alertsBodyLabel.ForeColor = Color.FromArgb(139, 110, 31)
                SetStateChip(_alertsStateLabel, "MONITOREO", Color.FromArgb(255, 245, 218), Color.FromArgb(116, 80, 17))
            Case Else
                _alertsBodyLabel.ForeColor = Color.FromArgb(66, 91, 126)
                SetStateChip(_alertsStateLabel, "ESTABLE", Color.FromArgb(222, 244, 229), Color.FromArgb(28, 68, 42))
        End Select
    End Sub

    Private Function SortAlerts(ByVal alertas As List(Of DashboardContracts.DashboardAlert)) As List(Of DashboardContracts.DashboardAlert)
        Dim sorted As New List(Of DashboardContracts.DashboardAlert)()
        If alertas Is Nothing Then
            Return sorted
        End If

        sorted.AddRange(alertas)
        sorted.Sort(
            Function(left As DashboardContracts.DashboardAlert, right As DashboardContracts.DashboardAlert)
                Dim leftSeverity As Integer = If(left Is Nothing, -1, CInt(left.Severity))
                Dim rightSeverity As Integer = If(right Is Nothing, -1, CInt(right.Severity))
                Dim compareSeverity As Integer = rightSeverity.CompareTo(leftSeverity)
                If compareSeverity <> 0 Then
                    Return compareSeverity
                End If

                Dim leftOrder As Integer = If(left Is Nothing, Integer.MaxValue, left.SortOrder)
                Dim rightOrder As Integer = If(right Is Nothing, Integer.MaxValue, right.SortOrder)
                Dim compareOrder As Integer = leftOrder.CompareTo(rightOrder)
                If compareOrder <> 0 Then
                    Return compareOrder
                End If

                Dim leftTitle As String = If(If(left Is Nothing, Nothing, left.Title), String.Empty)
                Dim rightTitle As String = If(If(right Is Nothing, Nothing, right.Title), String.Empty)
                Return String.Compare(leftTitle, rightTitle, StringComparison.OrdinalIgnoreCase)
            End Function)
        Return sorted
    End Function

    Private Function BuildAlertBody(ByVal alert As DashboardContracts.DashboardAlert) As String
        If alert Is Nothing Then
            Return String.Empty
        End If

        Dim prefix As String
        Select Case alert.Severity
            Case DashboardContracts.AlertSeverity.Critical
                prefix = "[CRIT]"
            Case DashboardContracts.AlertSeverity.Warning
                prefix = "[WARN]"
            Case Else
                prefix = "[INFO]"
        End Select

        Dim title As String = If(alert.Title, String.Empty).Trim()
        Dim message As String = If(alert.Message, String.Empty).Trim()
        If title.Length > 0 AndAlso message.Length > 0 Then
            Return prefix & " " & title & ": " & message
        End If
        If message.Length > 0 Then
            Return prefix & " " & message
        End If
        Return String.Empty
    End Function

    Private Function ResolveComparisonTitle(ByVal snapshot As DashboardContracts.Snapshot) As String
        If snapshot IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(snapshot.ComparisonLabel) Then
            Return snapshot.ComparisonLabel
        End If

        Return "Comparativo hoy vs dia lectivo previo"
    End Function

    Private Sub DrawSparkPointValue(ByVal graphics As Graphics, ByVal point As PointF, ByVal value As Integer, ByVal foreColor As Color, ByVal bounds As Size, ByVal font As Font)
        Dim text As String = value.ToString("N0")
        Dim textSize As Size = TextRenderer.MeasureText(text, font, New Size(Integer.MaxValue, Integer.MaxValue), TextFormatFlags.NoPadding)
        Dim textX As Integer = CInt(Math.Round(point.X - (textSize.Width / 2.0F)))
        textX = Math.Max(4, Math.Min(bounds.Width - textSize.Width - 4, textX))

        Dim preferredY As Integer = CInt(Math.Round(point.Y - textSize.Height - 9.0F))
        If preferredY < 4 Then
            preferredY = CInt(Math.Round(point.Y + 8.0F))
        End If
        preferredY = Math.Max(4, Math.Min(bounds.Height - textSize.Height - 4, preferredY))

        Dim rect As New Rectangle(textX - 2, preferredY - 1, textSize.Width + 4, textSize.Height + 2)
        Using backgroundBrush As New SolidBrush(Color.FromArgb(232, 255, 255, 255))
            graphics.FillRectangle(backgroundBrush, rect)
        End Using

        TextRenderer.DrawText(graphics, text, font, New Point(textX, preferredY), foreColor, TextFormatFlags.NoPadding)
    End Sub

    Private Function CreateBarValueLabel(ByVal value As Integer, ByVal left As Integer, ByVal barTop As Integer, ByVal barWidth As Integer, ByVal foreColor As Color, ByVal panelWidth As Integer, ByVal minimumTop As Integer) As Label
        Dim labelFont As New Font("Segoe UI Semibold", 7.5!, FontStyle.Bold, GraphicsUnit.Point)
        Dim text As String = value.ToString("N0")
        Dim measured As Size = TextRenderer.MeasureText(text, labelFont, New Size(Integer.MaxValue, Integer.MaxValue), TextFormatFlags.NoPadding)
        Dim labelWidth As Integer = Math.Max(Math.Max(30, barWidth + 12), measured.Width + 8)
        Dim labelHeight As Integer = 16
        Dim x As Integer = left + CInt(Math.Truncate((barWidth - labelWidth) / 2.0R))
        x = Math.Max(0, Math.Min(Math.Max(0, panelWidth - labelWidth), x))
        Dim y As Integer = Math.Max(minimumTop, barTop - labelHeight - 4)

        Dim label As New Label()
        label.Text = text
        label.AutoSize = False
        label.Size = New Size(labelWidth, labelHeight)
        label.TextAlign = ContentAlignment.MiddleCenter
        label.Font = labelFont
        label.ForeColor = foreColor
        label.BackColor = Color.White
        label.BorderStyle = BorderStyle.FixedSingle
        label.Location = New Point(x, y)
        Return label
    End Function

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

    Private Function ResolveDashboardPeriodTitle(ByVal snapshot As DashboardContracts.Snapshot) As String
        If snapshot IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(snapshot.PeriodLabel) Then
            Return "Tendencia operativa de " & snapshot.PeriodLabel.ToLowerInvariant()
        End If

        Return "Tendencia operativa de los ultimos 7 dias lectivos"
    End Function

    Private Function GetDashboardAxisLabel(ByVal metric As DashboardContracts.DailyMetric) As String
        If metric Is Nothing Then
            Return String.Empty
        End If

        If Not String.IsNullOrWhiteSpace(metric.DayNameShort) AndAlso metric.MetricDate <> Date.MinValue Then
            Return metric.DayNameShort & Environment.NewLine & metric.MetricDate.ToString("dd/MM")
        End If

        Return If(metric.Label, String.Empty)
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
