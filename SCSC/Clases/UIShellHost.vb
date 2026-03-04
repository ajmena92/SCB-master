Imports System.Windows.Forms

Public Class UIShellHost
    Private ReadOnly _owner As Form
    Private ReadOnly _onNavigate As Action(Of String)
    Private _sidebar As Panel
    Private _topBar As Panel
    Private _titleLabel As Label

    Private Class NavItem
        Public Property [Key] As String
        Public Property Text As String
    End Class

    Public Sub New(ByVal owner As Form, ByVal onNavigate As Action(Of String))
        _owner = owner
        _onNavigate = onNavigate
    End Sub

    Public Sub Build()
        If _owner Is Nothing OrElse Not _sidebar Is Nothing Then
            Exit Sub
        End If

        _sidebar = New Panel()
        _sidebar.Name = "ModernSidebar"
        _sidebar.Dock = DockStyle.Left
        _sidebar.Width = 260
        _sidebar.BackColor = UIConstants.Sidebar

        _topBar = New Panel()
        _topBar.Name = "ModernTopBar"
        _topBar.Dock = DockStyle.Top
        _topBar.Height = 56
        _topBar.BackColor = UIConstants.Surface

        _titleLabel = New Label()
        _titleLabel.Text = "Dashboard"
        _titleLabel.AutoSize = True
        _titleLabel.Location = New Point(16, 16)
        _titleLabel.Font = UIConstants.FontSubtitle()
        _titleLabel.ForeColor = UIConstants.TextPrimary
        _topBar.Controls.Add(_titleLabel)

        Dim brand As New Label()
        brand.Text = "SCSC 2026"
        brand.ForeColor = Color.White
        brand.Font = UIConstants.FontTitle()
        brand.AutoSize = False
        brand.TextAlign = ContentAlignment.MiddleLeft
        brand.SetBounds(16, 12, _sidebar.Width - 32, 36)
        _sidebar.Controls.Add(brand)

        Dim hint As New Label()
        hint.Text = "Operación Escolar"
        hint.ForeColor = Color.FromArgb(148, 163, 184)
        hint.Font = UIConstants.FontBody()
        hint.AutoSize = False
        hint.SetBounds(16, 46, _sidebar.Width - 32, 22)
        _sidebar.Controls.Add(hint)

        Dim items As New List(Of NavItem) From {
            New NavItem With {.Key = "estudiantes", .Text = "Estudiantes"},
            New NavItem With {.Key = "comedor", .Text = "Comedor"},
            New NavItem With {.Key = "transporte", .Text = "Transporte"},
            New NavItem With {.Key = "importacion", .Text = "Importar Excel"},
            New NavItem With {.Key = "recargas", .Text = "Recargas"},
            New NavItem With {.Key = "reportes", .Text = "Reportes"},
            New NavItem With {.Key = "rutas", .Text = "Rutas"},
            New NavItem With {.Key = "becas", .Text = "Becas"},
            New NavItem With {.Key = "seguridad", .Text = "Seguridad RBAC"},
            New NavItem With {.Key = "ayuda", .Text = "Ayuda"}
        }

        Dim topPos As Integer = 86
        For Each item As NavItem In items
            Dim btn As New Button()
            btn.Text = item.Text
            btn.Tag = item.Key
            btn.SetBounds(12, topPos, _sidebar.Width - 24, 40)
            btn.FlatStyle = FlatStyle.Flat
            btn.FlatAppearance.BorderSize = 0
            btn.BackColor = UIConstants.Sidebar
            btn.ForeColor = Color.White
            btn.TextAlign = ContentAlignment.MiddleLeft
            btn.Padding = New Padding(12, 0, 0, 0)
            btn.Font = UIConstants.FontBody()
            btn.Cursor = Cursors.Hand
            AddHandler btn.MouseEnter, AddressOf OnButtonEnter
            AddHandler btn.MouseLeave, AddressOf OnButtonLeave
            AddHandler btn.Click,
                Sub(sender, e)
                    SetTitle(item.Text)
                    _onNavigate(item.Key)
                End Sub
            _sidebar.Controls.Add(btn)
            topPos += 44
        Next

        _owner.Controls.Add(_topBar)
        _owner.Controls.Add(_sidebar)
        _sidebar.BringToFront()
        _topBar.BringToFront()
    End Sub

    Public Sub SetTitle(ByVal text As String)
        If _titleLabel IsNot Nothing Then
            _titleLabel.Text = text
        End If
    End Sub

    Private Sub OnButtonEnter(ByVal sender As Object, ByVal e As EventArgs)
        DirectCast(sender, Button).BackColor = UIConstants.SidebarHover
    End Sub

    Private Sub OnButtonLeave(ByVal sender As Object, ByVal e As EventArgs)
        DirectCast(sender, Button).BackColor = UIConstants.Sidebar
    End Sub
End Class
