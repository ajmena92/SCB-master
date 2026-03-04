Imports System.Windows.Forms
Imports System.Drawing
Imports System.Collections.Generic
Imports System.IO
Imports System.Runtime.InteropServices

Public Class UIShellHost
    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function DestroyIcon(ByVal handle As IntPtr) As Boolean
    End Function

    Private ReadOnly _owner As Form
    Private ReadOnly _onNavigate As Action(Of String)
    Private _sidebar As Panel
    Private _topBar As Panel
    Private _titleLabel As Label
    Private _contextLabel As Label
    Private _activeButton As Button
    Private _contentHost As Panel
    Private _kpiBecadosComedor As Label
    Private _kpiBecadosTransporte As Label
    Private _kpiMarcasComedor As Label
    Private _kpiMarcasTransporte As Label
    Private _comedorChartPanel As Panel
    Private _transporteChartPanel As Panel
    Private _deltaComedorLabel As Label
    Private _deltaTransporteLabel As Label
    Private _alertsBodyLabel As Label
    Private _topRutasBodyLabel As Label
    Private _chartTitleLabel As Label
    Private _sidebarLogo As PictureBox

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

    Public Sub New(ByVal owner As Form, ByVal onNavigate As Action(Of String))
        _owner = owner
        _onNavigate = onNavigate
    End Sub

    Public Sub Build()
        If _owner Is Nothing OrElse Not _sidebar Is Nothing Then
            Exit Sub
        End If

        _owner.BackColor = UIConstants.AppBackground
        ApplyOwnerIcon()

        _contentHost = New Panel()
        _contentHost.Name = "ModernContentHost"
        _contentHost.Dock = DockStyle.None
        _contentHost.BackColor = Color.FromArgb(243, 246, 251)

        _sidebar = New Panel()
        _sidebar.Name = "ModernSidebar"
        _sidebar.Dock = DockStyle.Left
        _sidebar.Width = 260
        _sidebar.BackColor = Color.FromArgb(16, 26, 46)
        _sidebar.AutoScroll = True

        _topBar = New Panel()
        _topBar.Name = "ModernTopBar"
        _topBar.Dock = DockStyle.Top
        _topBar.Height = 72
        _topBar.BackColor = Color.White

        _titleLabel = New Label()
        _titleLabel.Text = "Panel principal"
        _titleLabel.AutoSize = True
        _titleLabel.Location = New Point(24, 14)
        _titleLabel.Font = New Font("Segoe UI Semibold", 18.0!, FontStyle.Bold, GraphicsUnit.Point)
        _titleLabel.ForeColor = Color.FromArgb(23, 32, 51)
        _topBar.Controls.Add(_titleLabel)

        _contextLabel = New Label()
        _contextLabel.AutoSize = True
        _contextLabel.Location = New Point(25, 44)
        _contextLabel.Font = New Font("Segoe UI", 10.5!, FontStyle.Regular, GraphicsUnit.Point)
        _contextLabel.ForeColor = Color.FromArgb(95, 109, 128)
        _contextLabel.Text = "Navegue por operaciones, catalogos y configuracion."
        _topBar.Controls.Add(_contextLabel)

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

        Dim brand As New Label()
        brand.Text = "SCSC 2026"
        brand.ForeColor = Color.White
        brand.Font = New Font("Segoe UI Semibold", 18.0!, FontStyle.Bold, GraphicsUnit.Point)
        brand.AutoSize = False
        brand.TextAlign = ContentAlignment.MiddleLeft
        brand.SetBounds(80, 12, _sidebar.Width - 96, 40)
        _sidebar.Controls.Add(brand)

        _sidebarLogo = CreateBrandPictureBox("LogoIcon.png", New Rectangle(16, 10, 54, 54), Color.Transparent)
        If _sidebarLogo IsNot Nothing Then
            _sidebar.Controls.Add(_sidebarLogo)
            _sidebarLogo.BringToFront()
        End If

        Dim navHeader As New Label()
        navHeader.Text = "NAVEGACION"
        navHeader.ForeColor = Color.FromArgb(116, 141, 178)
        navHeader.Font = New Font("Segoe UI", 9.25!, FontStyle.Bold, GraphicsUnit.Point)
        navHeader.AutoSize = False
        navHeader.SetBounds(20, 94, _sidebar.Width - 40, 20)
        _sidebar.Controls.Add(navHeader)

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
            New NavItem With {.Group = "CONFIGURACION", .Key = "ayuda", .Text = "Ayuda - Soporte", .Tagline = "Soporte funcional y tecnico"}
        }

        Dim topPos As Integer = 124
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
                _sidebar.Controls.Add(sectionLabel)
                topPos += 24
            End If

            Dim navKey As String = item.Key
            Dim navText As String = item.Text
            Dim navTagline As String = item.Tagline
            Dim btn As New Button()
            btn.Text = "   " & navText
            btn.Tag = navKey
            btn.AccessibleDescription = navTagline
            btn.SetBounds(12, topPos, _sidebar.Width - 24, 42)
            btn.FlatStyle = FlatStyle.Flat
            btn.FlatAppearance.BorderSize = 0
            btn.BackColor = Color.FromArgb(16, 26, 46)
            btn.ForeColor = Color.FromArgb(233, 241, 255)
            btn.TextAlign = ContentAlignment.MiddleLeft
            btn.Padding = New Padding(14, 0, 8, 0)
            btn.Font = New Font("Segoe UI", 10.5!, FontStyle.Regular, GraphicsUnit.Point)
            btn.Cursor = Cursors.Hand
            AddHandler btn.MouseEnter, AddressOf OnButtonEnter
            AddHandler btn.MouseLeave, AddressOf OnButtonLeave
            AddHandler btn.Click,
                Sub(sender, e)
                    ActivateButton(DirectCast(sender, Button), navText, navTagline)
                    _onNavigate(navKey)
                End Sub
            _sidebar.Controls.Add(btn)
            topPos += 44
        Next

        Dim sidebarDivider As New Panel()
        sidebarDivider.Dock = DockStyle.Right
        sidebarDivider.Width = 1
        sidebarDivider.BackColor = Color.FromArgb(31, 48, 78)
        _sidebar.Controls.Add(sidebarDivider)

        Dim footer As New Panel()
        footer.Dock = DockStyle.Bottom
        footer.Height = 72
        footer.BackColor = Color.FromArgb(16, 26, 46)
        _sidebar.Controls.Add(footer)

        Dim footerLine As New Panel()
        footerLine.Dock = DockStyle.Top
        footerLine.Height = 1
        footerLine.BackColor = Color.FromArgb(31, 48, 78)
        footer.Controls.Add(footerLine)

        Dim btnSalir As New Button()
        btnSalir.Text = "   Salir"
        btnSalir.SetBounds(12, 16, _sidebar.Width - 24, 40)
        btnSalir.FlatStyle = FlatStyle.Flat
        btnSalir.FlatAppearance.BorderSize = 0
        btnSalir.BackColor = Color.FromArgb(114, 44, 61)
        btnSalir.ForeColor = Color.White
        btnSalir.TextAlign = ContentAlignment.MiddleLeft
        btnSalir.Padding = New Padding(14, 0, 8, 0)
        btnSalir.Font = New Font("Segoe UI Semibold", 10.0!, FontStyle.Bold, GraphicsUnit.Point)
        btnSalir.Cursor = Cursors.Hand
        btnSalir.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom
        btnSalir.FlatAppearance.MouseOverBackColor = Color.FromArgb(133, 54, 72)
        btnSalir.FlatAppearance.MouseDownBackColor = Color.FromArgb(95, 36, 50)
        AddHandler btnSalir.Click,
            Sub(sender As Object, e As EventArgs)
                If MessageBox.Show("Desea cerrar la aplicación?", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                    _owner.Close()
                End If
            End Sub
        footer.Controls.Add(btnSalir)

        _owner.Controls.Add(_contentHost)
        _owner.Controls.Add(_sidebar)
        _owner.Controls.Add(_topBar)

        AddHandler _owner.Resize,
            Sub(sender As Object, e As EventArgs)
                LayoutShellHost()
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
    End Sub

    Private Sub OnButtonLeave(ByVal sender As Object, ByVal e As EventArgs)
        Dim button As Button = DirectCast(sender, Button)
        If button Is _activeButton Then
            Exit Sub
        End If
        button.BackColor = Color.FromArgb(16, 26, 46)
    End Sub

    Private Sub ActivateButton(ByVal button As Button, ByVal title As String, ByVal tagline As String)
        If _activeButton IsNot Nothing Then
            _activeButton.BackColor = Color.FromArgb(16, 26, 46)
            _activeButton.ForeColor = Color.FromArgb(233, 241, 255)
            _activeButton.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
        End If

        _activeButton = button
        _activeButton.BackColor = Color.FromArgb(45, 90, 154)
        _activeButton.ForeColor = Color.White
        _activeButton.Font = New Font("Segoe UI Semibold", 10.0!, FontStyle.Bold, GraphicsUnit.Point)
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

        _kpiBecadosComedor.Text = snapshot.BecadosComedorHoy.ToString("N0")
        _kpiBecadosTransporte.Text = snapshot.BecadosTransporteHoy.ToString("N0")
        _kpiMarcasComedor.Text = snapshot.MarcasComedorHoy.ToString("N0")
        _kpiMarcasTransporte.Text = snapshot.MarcasTransporteHoy.ToString("N0")
        If Not _deltaComedorLabel Is Nothing Then
            _deltaComedorLabel.Text = "Comedor: " & FormatDelta(snapshot.MarcasComedorHoy, snapshot.MarcasComedorAyer)
        End If
        If Not _deltaTransporteLabel Is Nothing Then
            _deltaTransporteLabel.Text = "Transporte: " & FormatDelta(snapshot.MarcasTransporteHoy, snapshot.MarcasTransporteAyer)
        End If
        If Not _alertsBodyLabel Is Nothing Then
            If snapshot.Alertas Is Nothing OrElse snapshot.Alertas.Count = 0 Then
                _alertsBodyLabel.Text = "- Sin alertas operativas para hoy."
            Else
                _alertsBodyLabel.Text = String.Join(Environment.NewLine, snapshot.Alertas)
            End If
        End If
        If Not _topRutasBodyLabel Is Nothing Then
            If snapshot.TopRutas Is Nothing OrElse snapshot.TopRutas.Count = 0 Then
                _topRutasBodyLabel.Text = "- Sin datos de rutas para hoy."
            Else
                _topRutasBodyLabel.Text = String.Join(Environment.NewLine, snapshot.TopRutas)
            End If
        End If
        RenderComedorChart(snapshot.Series)
        RenderTransporteChart(snapshot.Series)
    End Sub

    Private Sub BuildDashboardSurface()
        If _contentHost Is Nothing Then
            Exit Sub
        End If

        _contentHost.Controls.Clear()

        Dim title As New Label()
        title.Text = "Dashboard Operativo"
        title.Font = New Font("Segoe UI Semibold", 18.0!, FontStyle.Bold, GraphicsUnit.Point)
        title.ForeColor = Color.FromArgb(28, 40, 65)
        title.AutoSize = True
        title.Location = New Point(26, 22)
        _contentHost.Controls.Add(title)

        Dim subtitle As New Label()
        subtitle.Text = "Vista ejecutiva de uso de comedor, becas y transporte"
        subtitle.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
        subtitle.ForeColor = Color.FromArgb(97, 111, 131)
        subtitle.AutoSize = True
        subtitle.Location = New Point(29, 54)
        _contentHost.Controls.Add(subtitle)

        Dim kpiLayout As New TableLayoutPanel()
        kpiLayout.ColumnCount = 4
        kpiLayout.RowCount = 1
        kpiLayout.Location = New Point(26, 90)
        kpiLayout.Size = New Size(Math.Max(860, _contentHost.Width - 52), 124)
        kpiLayout.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        For i As Integer = 0 To 3
            kpiLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        Next
        _contentHost.Controls.Add(kpiLayout)

        Dim card1 As Panel = BuildKpiCard("Becados Comedor (hoy)", Color.FromArgb(16, 94, 170), _kpiBecadosComedor)
        Dim card2 As Panel = BuildKpiCard("Becados Transporte (hoy)", Color.FromArgb(17, 124, 96), _kpiBecadosTransporte)
        Dim card3 As Panel = BuildKpiCard("Marcas Comedor (hoy)", Color.FromArgb(101, 76, 214), _kpiMarcasComedor)
        Dim card4 As Panel = BuildKpiCard("Marcas Transporte (hoy)", Color.FromArgb(184, 97, 24), _kpiMarcasTransporte)
        kpiLayout.Controls.Add(card1, 0, 0)
        kpiLayout.Controls.Add(card2, 1, 0)
        kpiLayout.Controls.Add(card3, 2, 0)
        kpiLayout.Controls.Add(card4, 3, 0)

        Dim chartCard As New Panel()
        chartCard.BackColor = Color.White
        chartCard.Location = New Point(26, 220)
        chartCard.Size = New Size(Math.Max(860, _contentHost.Width - 52), Math.Max(320, _contentHost.Height - 236))
        chartCard.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom
        _contentHost.Controls.Add(chartCard)

        _chartTitleLabel = New Label()
        _chartTitleLabel.Text = "Tendencia semanal (Comedor principal + Transporte de referencia)"
        _chartTitleLabel.Font = New Font("Segoe UI Semibold", 12.0!, FontStyle.Bold, GraphicsUnit.Point)
        _chartTitleLabel.ForeColor = Color.FromArgb(38, 52, 79)
        _chartTitleLabel.AutoSize = True
        _chartTitleLabel.Location = New Point(18, 15)
        chartCard.Controls.Add(_chartTitleLabel)

        Dim insightWidth As Integer = 264
        Dim leftChartsHost As New Panel()
        leftChartsHost.Location = New Point(18, 48)
        leftChartsHost.Size = New Size(Math.Max(430, chartCard.Width - (insightWidth + 44)), chartCard.Height - 66)
        leftChartsHost.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom
        leftChartsHost.BackColor = Color.White
        chartCard.Controls.Add(leftChartsHost)

        Dim comedorCard As New Panel()
        comedorCard.Dock = DockStyle.Top
        comedorCard.Height = 220
        comedorCard.BackColor = Color.FromArgb(250, 252, 255)
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
        insightPanel.Location = New Point(chartCard.Width - (insightWidth + 18), 48)
        insightPanel.Size = New Size(insightWidth, chartCard.Height - 66)
        insightPanel.Anchor = AnchorStyles.Top Or AnchorStyles.Right Or AnchorStyles.Bottom
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
        _alertsBodyLabel = BuildInsightBody("- Sin alertas operativas para hoy.", New Point(12, 36), New Size(alertCard.Width - 22, alertCard.Height - 42))
        alertCard.Controls.Add(_alertsBodyLabel)
        insightPanel.Controls.Add(alertCard)

        Dim rutasY As Integer = alertY + alertHeight + cardGap
        Dim rutasCard As Panel = BuildInsightCard("Top Rutas (hoy)", New Point(0, rutasY), New Size(insightPanel.Width, rutasHeight))
        _topRutasBodyLabel = BuildInsightBody("- Sin datos de rutas para hoy.", New Point(12, 36), New Size(rutasCard.Width - 22, rutasCard.Height - 42))
        rutasCard.Controls.Add(_topRutasBodyLabel)
        insightPanel.Controls.Add(rutasCard)
    End Sub

    Private Function BuildKpiCard(ByVal title As String, ByVal accentColor As Color, ByRef valueLabel As Label) As Panel
        Dim card As New Panel()
        card.Margin = New Padding(0, 0, 12, 0)
        card.BackColor = Color.White
        card.Dock = DockStyle.Fill

        Dim accent As New Panel()
        accent.Dock = DockStyle.Left
        accent.Width = 6
        accent.BackColor = accentColor
        card.Controls.Add(accent)

        Dim titleLabel As New Label()
        titleLabel.Text = title
        titleLabel.Font = New Font("Segoe UI", 9.25!, FontStyle.Regular, GraphicsUnit.Point)
        titleLabel.ForeColor = Color.FromArgb(100, 112, 131)
        titleLabel.AutoSize = True
        titleLabel.Location = New Point(16, 18)
        card.Controls.Add(titleLabel)

        valueLabel = New Label()
        valueLabel.Text = "0"
        valueLabel.Font = New Font("Segoe UI Semibold", 24.0!, FontStyle.Bold, GraphicsUnit.Point)
        valueLabel.ForeColor = Color.FromArgb(25, 38, 62)
        valueLabel.AutoSize = True
        valueLabel.Location = New Point(14, 44)
        card.Controls.Add(valueLabel)
        Return card
    End Function

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
        body.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular, GraphicsUnit.Point)
        body.ForeColor = Color.FromArgb(80, 96, 120)
        Return body
    End Function

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

            Dim iconPath As String = Global.System.IO.Path.Combine(Application.StartupPath, "favicon.ico")
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
        Dim candidates As String() = {
            Global.System.IO.Path.Combine(Application.StartupPath, "Resources", fileName),
            Global.System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName),
            Global.System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources", fileName),
            Global.System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Resources", fileName),
            Global.System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Resources", fileName)
        }

        For Each candidate As String In candidates
            Dim full As String = Global.System.IO.Path.GetFullPath(candidate)
            If Global.System.IO.File.Exists(full) Then
                Return full
            End If
        Next

        Return candidates(0)
    End Function
End Class
