Option Explicit On
Option Strict On

Imports System.Data
Imports System.ComponentModel
Imports System.Linq
Imports System.Drawing
Imports System.Windows.Forms

Partial Friend Class FrmSeguridadRBAC
    Inherits System.Windows.Forms.Form

    Private ReadOnly _service As New SeguridadRbacService()
    Private _layoutReady As Boolean = False
    Private _usuariosData As DataTable
    Private _rolesData As DataTable
    Private _permisosData As DataTable


    Private _lblFeedbackUsuarios As Label
    Private _lblFeedbackRoles As Label
    Private _lblFeedbackPermisos As Label


    Private _tooltips As ToolTip

    Public Sub New()
        InitializeComponent()
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MinimumSize = New Size(1220, 780)
        Me.Size = New Size(1320, 860)
        Me.WindowState = FormWindowState.Maximized
    End Sub

    Private Sub FrmSeguridadRBAC_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If IsInDesignMode() Then
            Return
        End If

        Try
            Me.KeyPreview = True
            UIThemeManagerV2.Apply(Me, "dialogo")
            ApplyVisualStandard2026()
            PanelFooter.BringToFront()
            _btnSalir.BringToFront()

            ConfigurarGrid(_gridUsuarios)
            ConfigurarGrid(_gridRoles)
            ConfigurarGrid(_gridPermisos)

            _txtContrasenaUsuario.PasswordChar = "*"c
            _cmbRolUsuario.DropDownStyle = ComboBoxStyle.DropDownList
            _cmbPermisoRol.DropDownStyle = ComboBoxStyle.DropDownList
            _tooltips = New ToolTip()
            ConfigureFieldTooltips()
            RecargarTodo()
            UpdateActionStates()
        Catch ex As Exception
            MsgBox("Error cargando formulario de seguridad RBAC: " & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub FrmSeguridadRBAC_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        PanelFooter.Visible = True
        PanelFooter.BringToFront()
        _btnSalir.Visible = True
        _btnSalir.BringToFront()
    End Sub

    Private Sub FrmSeguridadRBAC_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        PanelFooter.BringToFront()
        _btnSalir.BringToFront()
    End Sub

    Private Sub FormatearBoton(ByVal btn As Button, Optional ByVal isDanger As Boolean = False)
        btn.AutoSize = False
        btn.AutoEllipsis = True
        btn.Dock = DockStyle.None
        btn.Anchor = AnchorStyles.Left Or AnchorStyles.Top
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.BackColor = If(isDanger, UIConstants.Danger, UIConstants.Accent)
        btn.ForeColor = Color.White
        btn.Font = UIConstants.FontBodyStrong()
        btn.Height = 34
        btn.Width = 112
        btn.Margin = New Padding(0, 0, 6, 0)
        btn.FlatAppearance.MouseOverBackColor = If(isDanger, Color.FromArgb(178, 67, 82), Color.FromArgb(36, 112, 191))
        btn.FlatAppearance.MouseDownBackColor = If(isDanger, Color.FromArgb(132, 47, 61), Color.FromArgb(26, 89, 152))
    End Sub

    Private Sub FormatearCampos(ByVal ctrl As Control)
        If TypeOf ctrl Is TextBox Then
            Dim tb As TextBox = DirectCast(ctrl, TextBox)
            tb.BorderStyle = BorderStyle.FixedSingle
            tb.BackColor = Color.White
            tb.ForeColor = Color.FromArgb(29, 42, 61)
            tb.Margin = New Padding(0, 0, 10, 8)
        ElseIf TypeOf ctrl Is ComboBox Then
            Dim cb As ComboBox = DirectCast(ctrl, ComboBox)
            cb.FlatStyle = FlatStyle.Flat
            cb.BackColor = Color.White
            cb.ForeColor = Color.FromArgb(29, 42, 61)
            cb.Margin = New Padding(0, 0, 10, 8)
        ElseIf TypeOf ctrl Is ListBox Then
            Dim lb As ListBox = DirectCast(ctrl, ListBox)
            lb.BorderStyle = BorderStyle.FixedSingle
            lb.BackColor = Color.White
            lb.ForeColor = Color.FromArgb(29, 42, 61)
            lb.IntegralHeight = False
            lb.Margin = New Padding(0, 0, 10, 8)
        End If
    End Sub

    Private Sub ConfigurarGrid(ByVal grid As DataGridView)
        grid.ReadOnly = True
        grid.MultiSelect = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.ColumnHeadersVisible = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AllowUserToResizeRows = False
        grid.RowHeadersVisible = False
        grid.BackgroundColor = Color.White
        grid.BorderStyle = BorderStyle.None
        grid.GridColor = Color.FromArgb(224, 231, 242)
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 238, 248)
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(29, 42, 61)
        grid.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI Semibold", 10.0!, FontStyle.Bold)
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
        grid.ColumnHeadersHeight = 36
        grid.RowTemplate.Height = 34
        grid.DefaultCellStyle.BackColor = Color.White
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(36, 51, 77)
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 230, 252)
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(22, 37, 63)
        grid.DefaultCellStyle.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular)
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 254)
        grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False
        AddHandler grid.DataBindingComplete, AddressOf Grid_DataBindingComplete
    End Sub

    Private Sub EnsureCrudAreaVisible()
        ' Modo designer-first estricto:
        ' el layout lo define exclusivamente FrmSeguridadRBAC.Designer.vb.
        _layoutReady = True
    End Sub

    Private Sub SetupTabUx(ByVal tab As TabPage, ByVal splitName As String, ByVal title As String, ByVal subtitle As String)
        ' Método deshabilitado en modo designer-first estricto.
        ' La estructura del layout se controla solo desde Designer.
    End Sub

    Private Sub ConfigureFeedbackLabel(ByVal tab As TabPage, ByVal splitName As String)
        Dim targetLayout As TableLayoutPanel = Nothing
        Dim targetPanel As Panel = Nothing

        Select Case splitName
            Case "SplitUsuarios"
                targetLayout = LayoutUsuarios
                targetPanel = PanelUsuariosBottom
            Case "SplitRoles"
                targetLayout = LayoutRoles
                targetPanel = PanelRolesBottom
            Case "SplitPermisos"
                targetLayout = LayoutPermisos
                targetPanel = PanelPermisosBottom
        End Select

        If targetPanel Is Nothing Then
            Exit Sub
        End If

        Dim key As String = splitName & "_Feedback"

        ' Limpia cualquier etiqueta de feedback previa que haya quedado dentro del TableLayout.
        If Not targetLayout Is Nothing Then
            Dim legacy As Label = TryCast(targetLayout.Controls.Find(key, False).FirstOrDefault(), Label)
            If Not legacy Is Nothing Then
                targetLayout.Controls.Remove(legacy)
                legacy.Dispose()
            End If
        End If

        Dim lbl As Label = TryCast(targetPanel.Controls.Find(key, False).FirstOrDefault(), Label)
        If lbl Is Nothing Then
            lbl = New Label()
            lbl.Name = key
            lbl.Dock = DockStyle.Bottom
            lbl.Height = 22
            lbl.TextAlign = ContentAlignment.MiddleLeft
            lbl.Padding = New Padding(2, 0, 0, 0)
            lbl.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
            lbl.ForeColor = Color.FromArgb(76, 90, 112)
            lbl.Text = "Listo."
            targetPanel.Controls.Add(lbl)
            lbl.BringToFront()
        End If

        Select Case splitName
            Case "SplitUsuarios"
                _lblFeedbackUsuarios = lbl
            Case "SplitRoles"
                _lblFeedbackRoles = lbl
            Case "SplitPermisos"
                _lblFeedbackPermisos = lbl
        End Select
    End Sub

    Private Sub ConfigureCrudLayoutTables()
        If LayoutUsuarios.RowStyles.Count >= 4 Then
            LayoutUsuarios.RowStyles(0).SizeType = SizeType.Absolute
            LayoutUsuarios.RowStyles(0).Height = 40
            LayoutUsuarios.RowStyles(1).SizeType = SizeType.Absolute
            LayoutUsuarios.RowStyles(1).Height = 40
            LayoutUsuarios.RowStyles(2).SizeType = SizeType.Percent
            LayoutUsuarios.RowStyles(2).Height = 100
            LayoutUsuarios.RowStyles(3).SizeType = SizeType.Absolute
            LayoutUsuarios.RowStyles(3).Height = 48
        End If

        If LayoutRoles.RowStyles.Count >= 4 Then
            LayoutRoles.RowStyles(0).SizeType = SizeType.Absolute
            LayoutRoles.RowStyles(0).Height = 40
            LayoutRoles.RowStyles(1).SizeType = SizeType.Absolute
            LayoutRoles.RowStyles(1).Height = 40
            LayoutRoles.RowStyles(2).SizeType = SizeType.Percent
            LayoutRoles.RowStyles(2).Height = 100
            LayoutRoles.RowStyles(3).SizeType = SizeType.Absolute
            LayoutRoles.RowStyles(3).Height = 48
        End If

        If LayoutPermisos.RowStyles.Count >= 3 Then
            LayoutPermisos.RowStyles(0).SizeType = SizeType.Absolute
            LayoutPermisos.RowStyles(0).Height = 40
            LayoutPermisos.RowStyles(1).SizeType = SizeType.Absolute
            LayoutPermisos.RowStyles(1).Height = 40
            LayoutPermisos.RowStyles(2).SizeType = SizeType.Absolute
            LayoutPermisos.RowStyles(2).Height = 48
        End If
    End Sub

    Private Sub ConfigureCrudFlow(ByVal flow As FlowLayoutPanel)
        flow.AutoSize = False
        flow.WrapContents = False
        flow.Dock = DockStyle.Top
        flow.FlowDirection = FlowDirection.LeftToRight
        flow.AutoScroll = True
        flow.Height = 38
        flow.Padding = New Padding(0, 2, 0, 2)
        flow.Margin = New Padding(0, 2, 0, 0)
    End Sub

    Private Sub ConfigureActionRowButtons(ByVal flow As FlowLayoutPanel, ByVal ParamArray buttons() As Button)
        flow.SuspendLayout()
        Try
            For Each btn As Button In buttons
                If btn Is Nothing Then
                    Continue For
                End If
                btn.AutoSize = False
                btn.Dock = DockStyle.None
                btn.Anchor = AnchorStyles.Left Or AnchorStyles.Top
                btn.Height = 34
                btn.Width = 112
                btn.Margin = New Padding(0, 0, 6, 0)
                flow.SetFlowBreak(btn, False)
            Next
        Finally
            flow.ResumeLayout()
        End Try
    End Sub

    Private Sub ConfigureTabSplit(ByVal host As TabPage, ByVal grid As DataGridView, ByVal editorPanel As Panel, ByVal splitName As String, ByVal topRatio As Double)
        Dim split As SplitContainer = TryCast(host.Controls.Find(splitName, False).FirstOrDefault(), SplitContainer)
        If split Is Nothing Then
            split = New SplitContainer()
            split.Name = splitName
            split.Dock = DockStyle.Fill
            split.Orientation = Orientation.Horizontal
            split.IsSplitterFixed = False
            split.FixedPanel = FixedPanel.None
            split.Panel1MinSize = 120
            split.Panel2MinSize = 120
            split.SplitterWidth = 6
            split.BackColor = Color.FromArgb(223, 229, 238)

            host.Controls.Remove(grid)
            host.Controls.Remove(editorPanel)
            split.Panel1.Controls.Add(grid)
            split.Panel2.Controls.Add(editorPanel)
            host.Controls.Add(split)
            split.BringToFront()
        End If

        grid.Dock = DockStyle.Fill
        editorPanel.Dock = DockStyle.Fill
        editorPanel.Padding = New Padding(10, 10, 10, 18)
        EnsureSplitDistance(host, splitName, topRatio)
    End Sub

    Private Sub EnsureSplitDistance(ByVal host As TabPage, ByVal splitName As String, ByVal topRatio As Double)
        Dim split As SplitContainer = TryCast(host.Controls.Find(splitName, False).FirstOrDefault(), SplitContainer)
        If split Is Nothing Then
            Exit Sub
        End If

        If Not split.IsHandleCreated Then
            Exit Sub
        End If

        Dim available As Integer = split.ClientSize.Height - split.SplitterWidth
        If available <= 0 Then
            Exit Sub
        End If

        Dim minDistance As Integer = Math.Max(0, split.Panel1MinSize)
        Dim maxDistance As Integer = available - Math.Max(0, split.Panel2MinSize)

        If maxDistance < minDistance Then
            ' En tamaños transitorios (carga inicial / resize extremo) no hay rango válido aún.
            Exit Sub
        End If

        Dim target As Integer = CInt(Math.Round(available * topRatio))
        target = Math.Max(minDistance, target)
        target = Math.Min(maxDistance, target)

        If target >= minDistance AndAlso target <= maxDistance Then
            split.SplitterDistance = target
        End If
    End Sub

    Private Sub ApplyVisualStandard2026()
        Me.BackColor = UIConstants.AppBackground
        Me.FormBorderStyle = FormBorderStyle.Sizable
        Me.ControlBox = True
        Me.MaximizeBox = True
        Me.MinimizeBox = True
        Me.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular)
        TabsPrincipal.Font = New Font("Segoe UI Semibold", 10.5!, FontStyle.Bold)
        TabsPrincipal.Padding = New Point(18, 8)
        TabsPrincipal.ItemSize = New Size(120, 34)
        TabsPrincipal.SizeMode = TabSizeMode.Normal
        TabsPrincipal.Appearance = TabAppearance.Normal
        TabsPrincipal.HotTrack = True
        TabsPrincipal.BackColor = Color.FromArgb(240, 244, 250)
        TabUsuarios.BackColor = Color.FromArgb(245, 248, 253)
        TabRoles.BackColor = Color.FromArgb(245, 248, 253)
        TabPermisos.BackColor = Color.FromArgb(245, 248, 253)

        LabelUsuarioNombre.Font = UIConstants.FontBodyStrong()
        LabelUsuarioCompleto.Font = UIConstants.FontBodyStrong()
        LabelUsuarioContrasena.Font = UIConstants.FontBodyStrong()
        LabelRolAsignar.Font = UIConstants.FontBodyStrong()
        LabelRolesUsuario.Font = UIConstants.FontBodyStrong()

        LabelNombreRol.Font = UIConstants.FontBodyStrong()
        LabelDescripcionRol.Font = UIConstants.FontBodyStrong()
        LabelPermisoAsignar.Font = UIConstants.FontBodyStrong()
        LabelPermisosRol.Font = UIConstants.FontBodyStrong()

        LabelClavePermiso.Font = UIConstants.FontBodyStrong()
        LabelDescripcionPermiso.Font = UIConstants.FontBodyStrong()

        _txtNombreUsuario.Font = UIConstants.FontBody()
        _txtNombreCompletoUsuario.Font = UIConstants.FontBody()
        _txtContrasenaUsuario.Font = UIConstants.FontBody()
        _txtNombreRol.Font = UIConstants.FontBody()
        _txtDescripcionRol.Font = UIConstants.FontBody()
        _txtClavePermiso.Font = UIConstants.FontBody()
        _txtDescripcionPermiso.Font = UIConstants.FontBody()
        _lstRolesUsuario.Font = UIConstants.FontBody()
        _lstPermisosRol.Font = UIConstants.FontBody()
        _cmbRolUsuario.Font = UIConstants.FontBody()
        _cmbPermisoRol.Font = UIConstants.FontBody()

        LabelUsuarioNombre.ForeColor = Color.FromArgb(76, 90, 112)
        LabelUsuarioCompleto.ForeColor = Color.FromArgb(76, 90, 112)
        LabelUsuarioContrasena.ForeColor = Color.FromArgb(76, 90, 112)
        LabelRolAsignar.ForeColor = Color.FromArgb(76, 90, 112)
        LabelRolesUsuario.ForeColor = Color.FromArgb(76, 90, 112)
        LabelNombreRol.ForeColor = Color.FromArgb(76, 90, 112)
        LabelDescripcionRol.ForeColor = Color.FromArgb(76, 90, 112)
        LabelPermisoAsignar.ForeColor = Color.FromArgb(76, 90, 112)
        LabelPermisosRol.ForeColor = Color.FromArgb(76, 90, 112)
        LabelClavePermiso.ForeColor = Color.FromArgb(76, 90, 112)
        LabelDescripcionPermiso.ForeColor = Color.FromArgb(76, 90, 112)

        FormatearCampos(_txtNombreUsuario)
        FormatearCampos(_txtNombreCompletoUsuario)
        FormatearCampos(_txtContrasenaUsuario)
        FormatearCampos(_txtNombreRol)
        FormatearCampos(_txtDescripcionRol)
        FormatearCampos(_txtClavePermiso)
        FormatearCampos(_txtDescripcionPermiso)
        FormatearCampos(_cmbRolUsuario)
        FormatearCampos(_cmbPermisoRol)
        FormatearCampos(_lstRolesUsuario)
        FormatearCampos(_lstPermisosRol)

        PanelUsuariosBottom.BackColor = Color.White
        PanelRolesBottom.BackColor = Color.White
        PanelPermisosBottom.BackColor = Color.White
        LayoutUsuarios.BackColor = Color.White
        LayoutRoles.BackColor = Color.White
        LayoutPermisos.BackColor = Color.White
        FlowUsuariosBotones.BackColor = Color.White
        FlowRolesBotones.BackColor = Color.White
        FlowPermisosBotones.BackColor = Color.White

        FormatearBoton(_btnCrearUsuario)
        FormatearBoton(_btnActualizarUsuario)
        FormatearBoton(_btnEliminarUsuario, True)
        FormatearBoton(_btnCambiarClave)
        FormatearBoton(_btnAsignarRolUsuario)
        FormatearBoton(_btnRevocarRolUsuario, True)

        FormatearBoton(_btnCrearRol)
        FormatearBoton(_btnActualizarRol)
        FormatearBoton(_btnEliminarRol, True)
        FormatearBoton(_btnAsignarPermisoRol)
        FormatearBoton(_btnRevocarPermisoRol, True)

        FormatearBoton(_btnCrearPermiso)
        FormatearBoton(_btnActualizarPermiso)
        FormatearBoton(_btnEliminarPermiso, True)
        FormatearBoton(_btnSalir)

        ApplyActionTextByTab()
        ConfigureActionRowButtons(FlowUsuariosBotones, _btnCrearUsuario, _btnActualizarUsuario, _btnEliminarUsuario, _btnCambiarClave, _btnAsignarRolUsuario, _btnRevocarRolUsuario)
        ConfigureActionRowButtons(FlowRolesBotones, _btnCrearRol, _btnActualizarRol, _btnEliminarRol, _btnAsignarPermisoRol, _btnRevocarPermisoRol)
        ConfigureActionRowButtons(FlowPermisosBotones, _btnCrearPermiso, _btnActualizarPermiso, _btnEliminarPermiso)
        _btnSalir.Width = 118
        _btnSalir.Height = 34
        PanelFooter.Height = 46
        PanelFooter.Padding = New Padding(8, 6, 8, 6)
        PanelFooter.BackColor = Color.FromArgb(240, 244, 250)
        _btnSalir.BackColor = Color.FromArgb(52, 74, 104)
        _btnSalir.FlatAppearance.MouseOverBackColor = Color.FromArgb(67, 92, 126)
        _btnSalir.FlatAppearance.MouseDownBackColor = Color.FromArgb(44, 62, 88)
    End Sub

    Private Sub ApplyVisualGuards()
        ' Modo designer-first estricto:
        ' evitar ajustes de geometría en runtime para no desalinear el diseño.
    End Sub

    Private Sub EnsureHeaderReserve(ByVal splitName As String)
        Dim tab As TabPage = Nothing
        Select Case splitName
            Case "SplitUsuarios" : tab = TabUsuarios
            Case "SplitRoles" : tab = TabRoles
            Case "SplitPermisos" : tab = TabPermisos
        End Select
        If tab Is Nothing Then
            Exit Sub
        End If

        Dim split As SplitContainer = TryCast(tab.Controls.Find(splitName, False).FirstOrDefault(), SplitContainer)
        If split Is Nothing Then
            Exit Sub
        End If

        split.Panel1.Padding = New Padding(0, 66, 0, 0)
    End Sub

    Private Sub EnsureActionRowVisible(ByVal flow As FlowLayoutPanel)
        If flow Is Nothing Then
            Exit Sub
        End If
        flow.Dock = DockStyle.Top
        flow.Height = 38
        flow.WrapContents = False
        flow.AutoScroll = True
        flow.Padding = New Padding(0, 2, 0, 2)
        flow.Margin = New Padding(0, 2, 0, 0)
    End Sub

    Private Sub EnsureGridTopVisible(ByVal grid As DataGridView)
        If grid Is Nothing Then
            Exit Sub
        End If

        grid.ColumnHeadersVisible = True

        If grid.Rows.Count = 0 Then
            Exit Sub
        End If

        Try
            grid.FirstDisplayedScrollingRowIndex = 0
        Catch
        End Try

        Try
            If grid.CurrentCell Is Nothing Then
                For Each col As DataGridViewColumn In grid.Columns
                    If col.Visible Then
                        grid.CurrentCell = grid.Rows(0).Cells(col.Index)
                        Exit For
                    End If
                Next
            End If
        Catch
        End Try
    End Sub

    Private Sub ApplyActionTextByTab()
        _btnCrearUsuario.Text = "Crear usuario"
        _btnActualizarUsuario.Text = "Actualizar usuario"
        _btnEliminarUsuario.Text = "Eliminar usuario"
        _btnCambiarClave.Text = "Cambiar clave"
        _btnAsignarRolUsuario.Text = "Asignar rol"
        _btnRevocarRolUsuario.Text = "Revocar rol"

        _btnCrearRol.Text = "Crear rol"
        _btnActualizarRol.Text = "Actualizar rol"
        _btnEliminarRol.Text = "Eliminar rol"
        _btnAsignarPermisoRol.Text = "Asignar permiso"
        _btnRevocarPermisoRol.Text = "Revocar permiso"

        _btnCrearPermiso.Text = "Crear permiso"
        _btnActualizarPermiso.Text = "Actualizar permiso"
        _btnEliminarPermiso.Text = "Eliminar permiso"

        _btnCrearUsuario.Width = 118
        _btnActualizarUsuario.Width = 136
        _btnEliminarUsuario.Width = 128
        _btnCambiarClave.Width = 122
        _btnAsignarRolUsuario.Width = 118
        _btnRevocarRolUsuario.Width = 118

        _btnCrearRol.Width = 106
        _btnActualizarRol.Width = 124
        _btnEliminarRol.Width = 116
        _btnAsignarPermisoRol.Width = 132
        _btnRevocarPermisoRol.Width = 136

        _btnCrearPermiso.Width = 124
        _btnActualizarPermiso.Width = 142
        _btnEliminarPermiso.Width = 134
    End Sub

    Private Function IsInDesignMode() As Boolean
        If LicenseManager.UsageMode = LicenseUsageMode.Designtime Then
            Return True
        End If
        If Me.Site IsNot Nothing AndAlso Me.Site.DesignMode Then
            Return True
        End If
        Return False
    End Function

    Private Sub RecargarTodo()
        CargarUsuarios()
        CargarRoles()
        CargarPermisos()
        CargarCombos()
    End Sub

    Private Sub CargarUsuarios()
        _usuariosData = _service.ListarUsuarios()
        _gridUsuarios.DataSource = _usuariosData
        ApplyUserFilter()
    End Sub

    Private Sub CargarRoles()
        _rolesData = _service.ListarRoles()
        _gridRoles.DataSource = _rolesData
        ApplyRoleFilter()
    End Sub

    Private Sub CargarPermisos()
        _permisosData = _service.ListarPermisos()
        _gridPermisos.DataSource = _permisosData
        ApplyPermisoFilter()
    End Sub

    Private Sub ApplyUserFilter()
        If _usuariosData Is Nothing Then
            Exit Sub
        End If
        Dim f As String = ""
        If f.Length = 0 Then
            _gridUsuarios.DataSource = _usuariosData
            Exit Sub
        End If
        Dim dv As New DataView(_usuariosData)
        dv.RowFilter = String.Format("Convert(NombreUsuario, 'System.String') LIKE '%{0}%' OR Convert(NombreCompleto, 'System.String') LIKE '%{0}%'", f)
        _gridUsuarios.DataSource = dv
    End Sub

    Private Sub ApplyRoleFilter()
        If _rolesData Is Nothing Then
            Exit Sub
        End If
        Dim f As String = ""
        If f.Length = 0 Then
            _gridRoles.DataSource = _rolesData
            Exit Sub
        End If
        Dim dv As New DataView(_rolesData)
        dv.RowFilter = String.Format("Convert(NombreRol, 'System.String') LIKE '%{0}%' OR Convert(Descripcion, 'System.String') LIKE '%{0}%'", f)
        _gridRoles.DataSource = dv
    End Sub

    Private Sub ApplyPermisoFilter()
        If _permisosData Is Nothing Then
            Exit Sub
        End If
        Dim f As String = ""
        If f.Length = 0 Then
            _gridPermisos.DataSource = _permisosData
            Exit Sub
        End If
        Dim dv As New DataView(_permisosData)
        dv.RowFilter = String.Format("Convert(ClavePermiso, 'System.String') LIKE '%{0}%' OR Convert(Descripcion, 'System.String') LIKE '%{0}%'", f)
        _gridPermisos.DataSource = dv
    End Sub

    Private Function SafeLikeValue(ByVal value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return String.Empty
        End If
        Return value.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]")
    End Function

    Private Sub CargarCombos()
        Dim roles As DataTable = _service.ListarRoles()
        _cmbRolUsuario.DataSource = roles
        _cmbRolUsuario.DisplayMember = "NombreRol"
        _cmbRolUsuario.ValueMember = "IdRol"

        Dim permisos As DataTable = _service.ListarPermisos()
        _cmbPermisoRol.DataSource = permisos
        _cmbPermisoRol.DisplayMember = "ClavePermiso"
        _cmbPermisoRol.ValueMember = "IdPermiso"
    End Sub

    Private Sub GridUsuarios_SelectionChanged(sender As Object, e As EventArgs) Handles _gridUsuarios.SelectionChanged
        If _gridUsuarios.CurrentRow Is Nothing Then
            _lstRolesUsuario.DataSource = Nothing
            UpdateActionStates()
            Return
        End If

        _txtNombreUsuario.Text = ValorCelda(_gridUsuarios.CurrentRow, "NombreUsuario")
        _txtNombreCompletoUsuario.Text = ValorCelda(_gridUsuarios.CurrentRow, "NombreCompleto")
        _chkUsuarioActivo.Checked = ValorCelda(_gridUsuarios.CurrentRow, "EsActivo") = "True"

        Dim idUsuario As Integer
        If Integer.TryParse(ValorCelda(_gridUsuarios.CurrentRow, "IdUsuario"), idUsuario) Then
            Dim roles As DataTable = _service.ListarRolesDeUsuario(idUsuario)
            _lstRolesUsuario.DataSource = roles
            _lstRolesUsuario.DisplayMember = "NombreRol"
            _lstRolesUsuario.ValueMember = "IdRol"
        End If
        UpdateActionStates()
    End Sub

    Private Sub GridRoles_SelectionChanged(sender As Object, e As EventArgs) Handles _gridRoles.SelectionChanged
        If _gridRoles.CurrentRow Is Nothing Then
            _lstPermisosRol.DataSource = Nothing
            UpdateActionStates()
            Return
        End If

        _txtNombreRol.Text = ValorCelda(_gridRoles.CurrentRow, "NombreRol")
        _txtDescripcionRol.Text = ValorCelda(_gridRoles.CurrentRow, "Descripcion")
        _chkRolActivo.Checked = ValorCelda(_gridRoles.CurrentRow, "EsActivo") = "True"

        Dim idRol As Integer
        If Integer.TryParse(ValorCelda(_gridRoles.CurrentRow, "IdRol"), idRol) Then
            Dim permisos As DataTable = _service.ListarPermisosDeRol(idRol)
            _lstPermisosRol.DataSource = permisos
            _lstPermisosRol.DisplayMember = "ClavePermiso"
            _lstPermisosRol.ValueMember = "IdPermiso"
        End If
        UpdateActionStates()
    End Sub

    Private Sub GridPermisos_SelectionChanged(sender As Object, e As EventArgs) Handles _gridPermisos.SelectionChanged
        If _gridPermisos.CurrentRow Is Nothing Then
            UpdateActionStates()
            Return
        End If

        _txtClavePermiso.Text = ValorCelda(_gridPermisos.CurrentRow, "ClavePermiso")
        _txtDescripcionPermiso.Text = ValorCelda(_gridPermisos.CurrentRow, "Descripcion")
        UpdateActionStates()
    End Sub

    Private Sub TabsPrincipal_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabsPrincipal.SelectedIndexChanged
        UpdateActionStates()
    End Sub

    Private Sub CamposUsuarios_TextChanged(sender As Object, e As EventArgs) Handles _txtNombreUsuario.TextChanged, _txtNombreCompletoUsuario.TextChanged, _txtContrasenaUsuario.TextChanged
        UpdateActionStates()
    End Sub

    Private Sub CamposRoles_TextChanged(sender As Object, e As EventArgs) Handles _txtNombreRol.TextChanged, _txtDescripcionRol.TextChanged
        UpdateActionStates()
    End Sub

    Private Sub CamposPermisos_TextChanged(sender As Object, e As EventArgs) Handles _txtClavePermiso.TextChanged, _txtDescripcionPermiso.TextChanged
        UpdateActionStates()
    End Sub

    Private Sub ListasSeleccionChanged(sender As Object, e As EventArgs) Handles _cmbRolUsuario.SelectedIndexChanged, _cmbPermisoRol.SelectedIndexChanged, _lstRolesUsuario.SelectedIndexChanged, _lstPermisosRol.SelectedIndexChanged
        UpdateActionStates()
    End Sub

    Private Function ObtenerIdSeleccionado(ByVal grid As DataGridView, ByVal nombreColumna As String) As Integer
        If grid.CurrentRow Is Nothing Then
            Throw New Exception("Debe seleccionar un registro.")
        End If

        Dim id As Integer
        If Not Integer.TryParse(ValorCelda(grid.CurrentRow, nombreColumna), id) Then
            Throw New Exception("No se pudo resolver el identificador seleccionado.")
        End If
        Return id
    End Function

    Private Function ValorCelda(ByVal row As DataGridViewRow, ByVal columna As String) As String
        Dim valor As Object = row.Cells(columna).Value
        If valor Is Nothing OrElse valor Is DBNull.Value Then
            Return ""
        End If
        Return valor.ToString()
    End Function

    Private Sub BtnCrearUsuario_Click(sender As Object, e As EventArgs) Handles _btnCrearUsuario.Click
        Try
            If String.IsNullOrWhiteSpace(_txtNombreUsuario.Text) OrElse String.IsNullOrWhiteSpace(_txtNombreCompletoUsuario.Text) OrElse String.IsNullOrWhiteSpace(_txtContrasenaUsuario.Text) Then
                Throw New Exception("NombreUsuario, NombreCompleto y Contraseña son obligatorios.")
            End If

            _service.CrearUsuario(_txtNombreUsuario.Text, _txtNombreCompletoUsuario.Text, _txtContrasenaUsuario.Text, _chkUsuarioActivo.Checked)
            _txtContrasenaUsuario.Text = ""
            CargarUsuarios()
            CargarCombos()
            UpdateActionStates()
            SetFeedback("usuarios", "Usuario creado correctamente.", False)
        Catch ex As Exception
            SetFeedback("usuarios", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnActualizarUsuario_Click(sender As Object, e As EventArgs) Handles _btnActualizarUsuario.Click
        Try
            Dim idUsuario As Integer = ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")
            _service.ActualizarUsuario(idUsuario, _txtNombreCompletoUsuario.Text, _chkUsuarioActivo.Checked)
            CargarUsuarios()
            UpdateActionStates()
            SetFeedback("usuarios", "Usuario actualizado correctamente.", False)
        Catch ex As Exception
            SetFeedback("usuarios", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnCambiarClave_Click(sender As Object, e As EventArgs) Handles _btnCambiarClave.Click
        Try
            Dim idUsuario As Integer = ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")
            If String.IsNullOrWhiteSpace(_txtContrasenaUsuario.Text) Then
                Throw New Exception("Debe indicar la nueva contraseña.")
            End If
            _service.CambiarContrasenaUsuario(idUsuario, _txtContrasenaUsuario.Text)
            _txtContrasenaUsuario.Text = ""
            MsgBox("Contraseña actualizada.", MsgBoxStyle.Information)
            UpdateActionStates()
            SetFeedback("usuarios", "Contraseña actualizada.", False)
        Catch ex As Exception
            SetFeedback("usuarios", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnEliminarUsuario_Click(sender As Object, e As EventArgs) Handles _btnEliminarUsuario.Click
        Try
            Dim idUsuario As Integer = ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")
            _service.EliminarUsuario(idUsuario)
            CargarUsuarios()
            CargarCombos()
            UpdateActionStates()
            SetFeedback("usuarios", "Usuario eliminado correctamente.", False)
        Catch ex As Exception
            SetFeedback("usuarios", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnAsignarRolUsuario_Click(sender As Object, e As EventArgs) Handles _btnAsignarRolUsuario.Click
        Try
            Dim idUsuario As Integer = ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")
            If _cmbRolUsuario.SelectedValue Is Nothing Then
                Throw New Exception("No hay rol seleccionado.")
            End If
            Dim idRol As Integer = Convert.ToInt32(_cmbRolUsuario.SelectedValue)
            _service.AsignarRolAUsuario(idUsuario, idRol)
            GridUsuarios_SelectionChanged(Nothing, EventArgs.Empty)
            CargarUsuarios()
            UpdateActionStates()
            SetFeedback("usuarios", "Rol asignado al usuario.", False)
        Catch ex As Exception
            SetFeedback("usuarios", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnRevocarRolUsuario_Click(sender As Object, e As EventArgs) Handles _btnRevocarRolUsuario.Click
        Try
            Dim idUsuario As Integer = ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")
            If _lstRolesUsuario.SelectedValue Is Nothing Then
                Throw New Exception("Seleccione el rol a revocar del listado de roles del usuario.")
            End If
            Dim idRol As Integer = Convert.ToInt32(_lstRolesUsuario.SelectedValue)
            _service.RevocarRolAUsuario(idUsuario, idRol)
            GridUsuarios_SelectionChanged(Nothing, EventArgs.Empty)
            CargarUsuarios()
            UpdateActionStates()
            SetFeedback("usuarios", "Rol revocado del usuario.", False)
        Catch ex As Exception
            SetFeedback("usuarios", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnCrearRol_Click(sender As Object, e As EventArgs) Handles _btnCrearRol.Click
        Try
            If String.IsNullOrWhiteSpace(_txtNombreRol.Text) Then
                Throw New Exception("NombreRol es obligatorio.")
            End If
            _service.CrearRol(_txtNombreRol.Text, _txtDescripcionRol.Text, _chkRolActivo.Checked)
            CargarRoles()
            CargarCombos()
            UpdateActionStates()
            SetFeedback("roles", "Rol creado correctamente.", False)
        Catch ex As Exception
            SetFeedback("roles", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnActualizarRol_Click(sender As Object, e As EventArgs) Handles _btnActualizarRol.Click
        Try
            Dim idRol As Integer = ObtenerIdSeleccionado(_gridRoles, "IdRol")
            _service.ActualizarRol(idRol, _txtDescripcionRol.Text, _chkRolActivo.Checked)
            CargarRoles()
            CargarCombos()
            UpdateActionStates()
            SetFeedback("roles", "Rol actualizado correctamente.", False)
        Catch ex As Exception
            SetFeedback("roles", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnEliminarRol_Click(sender As Object, e As EventArgs) Handles _btnEliminarRol.Click
        Try
            Dim idRol As Integer = ObtenerIdSeleccionado(_gridRoles, "IdRol")
            _service.EliminarRol(idRol)
            CargarRoles()
            CargarCombos()
            UpdateActionStates()
            SetFeedback("roles", "Rol eliminado correctamente.", False)
        Catch ex As Exception
            SetFeedback("roles", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnAsignarPermisoRol_Click(sender As Object, e As EventArgs) Handles _btnAsignarPermisoRol.Click
        Try
            Dim idRol As Integer = ObtenerIdSeleccionado(_gridRoles, "IdRol")
            If _cmbPermisoRol.SelectedValue Is Nothing Then
                Throw New Exception("No hay permiso seleccionado.")
            End If
            Dim idPermiso As Integer = Convert.ToInt32(_cmbPermisoRol.SelectedValue)
            _service.AsignarPermisoARol(idRol, idPermiso)
            GridRoles_SelectionChanged(Nothing, EventArgs.Empty)
            UpdateActionStates()
            SetFeedback("roles", "Permiso asignado al rol.", False)
        Catch ex As Exception
            SetFeedback("roles", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnRevocarPermisoRol_Click(sender As Object, e As EventArgs) Handles _btnRevocarPermisoRol.Click
        Try
            Dim idRol As Integer = ObtenerIdSeleccionado(_gridRoles, "IdRol")
            If _lstPermisosRol.SelectedValue Is Nothing Then
                Throw New Exception("Seleccione el permiso a revocar del listado de permisos del rol.")
            End If
            Dim idPermiso As Integer = Convert.ToInt32(_lstPermisosRol.SelectedValue)
            _service.RevocarPermisoARol(idRol, idPermiso)
            GridRoles_SelectionChanged(Nothing, EventArgs.Empty)
            UpdateActionStates()
            SetFeedback("roles", "Permiso revocado del rol.", False)
        Catch ex As Exception
            SetFeedback("roles", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnCrearPermiso_Click(sender As Object, e As EventArgs) Handles _btnCrearPermiso.Click
        Try
            If String.IsNullOrWhiteSpace(_txtClavePermiso.Text) Then
                Throw New Exception("ClavePermiso es obligatoria.")
            End If
            _service.CrearPermiso(_txtClavePermiso.Text, _txtDescripcionPermiso.Text)
            CargarPermisos()
            CargarCombos()
            UpdateActionStates()
            SetFeedback("permisos", "Permiso creado correctamente.", False)
        Catch ex As Exception
            SetFeedback("permisos", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnActualizarPermiso_Click(sender As Object, e As EventArgs) Handles _btnActualizarPermiso.Click
        Try
            Dim idPermiso As Integer = ObtenerIdSeleccionado(_gridPermisos, "IdPermiso")
            _service.ActualizarPermiso(idPermiso, _txtDescripcionPermiso.Text)
            CargarPermisos()
            CargarCombos()
            UpdateActionStates()
            SetFeedback("permisos", "Permiso actualizado correctamente.", False)
        Catch ex As Exception
            SetFeedback("permisos", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnEliminarPermiso_Click(sender As Object, e As EventArgs) Handles _btnEliminarPermiso.Click
        Try
            Dim idPermiso As Integer = ObtenerIdSeleccionado(_gridPermisos, "IdPermiso")
            _service.EliminarPermiso(idPermiso)
            CargarPermisos()
            CargarCombos()
            UpdateActionStates()
            SetFeedback("permisos", "Permiso eliminado correctamente.", False)
        Catch ex As Exception
            SetFeedback("permisos", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnSalir_Click(sender As Object, e As EventArgs) Handles _btnSalir.Click
        Me.Close()
    End Sub

    Private Sub FrmSeguridadRBAC_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Escape Then
            Me.Close()
            e.SuppressKeyPress = True
            Return
        End If

        If e.KeyCode = Keys.Enter Then
            ExecutePrimaryAction()
            e.SuppressKeyPress = True
        End If
    End Sub

    Private Sub ExecutePrimaryAction()
        Select Case TabsPrincipal.SelectedTab.Name
            Case TabUsuarios.Name
                If _btnCrearUsuario.Enabled Then
                    BtnCrearUsuario_Click(_btnCrearUsuario, EventArgs.Empty)
                End If
            Case TabRoles.Name
                If _btnCrearRol.Enabled Then
                    BtnCrearRol_Click(_btnCrearRol, EventArgs.Empty)
                End If
            Case TabPermisos.Name
                If _btnCrearPermiso.Enabled Then
                    BtnCrearPermiso_Click(_btnCrearPermiso, EventArgs.Empty)
                End If
        End Select
    End Sub

    Private Sub UpdateActionStates()
        If IsInDesignMode() Then
            Return
        End If

        Dim usuarioSeleccionado As Boolean = Not _gridUsuarios.CurrentRow Is Nothing
        Dim rolSeleccionado As Boolean = Not _gridRoles.CurrentRow Is Nothing
        Dim permisoSeleccionado As Boolean = Not _gridPermisos.CurrentRow Is Nothing

        Dim usuarioValidoParaCrear As Boolean = Not String.IsNullOrWhiteSpace(_txtNombreUsuario.Text) AndAlso
                                              Not String.IsNullOrWhiteSpace(_txtNombreCompletoUsuario.Text) AndAlso
                                              Not String.IsNullOrWhiteSpace(_txtContrasenaUsuario.Text)
        _btnCrearUsuario.Enabled = usuarioValidoParaCrear
        _btnActualizarUsuario.Enabled = usuarioSeleccionado
        _btnEliminarUsuario.Enabled = usuarioSeleccionado
        _btnCambiarClave.Enabled = usuarioSeleccionado AndAlso Not String.IsNullOrWhiteSpace(_txtContrasenaUsuario.Text)
        _btnAsignarRolUsuario.Enabled = usuarioSeleccionado AndAlso Not _cmbRolUsuario.SelectedValue Is Nothing
        _btnRevocarRolUsuario.Enabled = usuarioSeleccionado AndAlso Not _lstRolesUsuario.SelectedValue Is Nothing

        Dim rolValidoParaCrear As Boolean = Not String.IsNullOrWhiteSpace(_txtNombreRol.Text)
        _btnCrearRol.Enabled = rolValidoParaCrear
        _btnActualizarRol.Enabled = rolSeleccionado
        _btnEliminarRol.Enabled = rolSeleccionado
        _btnAsignarPermisoRol.Enabled = rolSeleccionado AndAlso Not _cmbPermisoRol.SelectedValue Is Nothing
        _btnRevocarPermisoRol.Enabled = rolSeleccionado AndAlso Not _lstPermisosRol.SelectedValue Is Nothing

        Dim permisoValidoParaCrear As Boolean = Not String.IsNullOrWhiteSpace(_txtClavePermiso.Text)
        _btnCrearPermiso.Enabled = permisoValidoParaCrear
        _btnActualizarPermiso.Enabled = permisoSeleccionado
        _btnEliminarPermiso.Enabled = permisoSeleccionado
    End Sub

    Private Sub SetFeedback(ByVal scope As String, ByVal text As String, ByVal isError As Boolean)
        Dim lbl As Label = Nothing
        Select Case scope
            Case "usuarios"
                lbl = _lblFeedbackUsuarios
            Case "roles"
                lbl = _lblFeedbackRoles
            Case "permisos"
                lbl = _lblFeedbackPermisos
        End Select

        If lbl Is Nothing Then
            Exit Sub
        End If

        lbl.ForeColor = If(isError, Color.FromArgb(166, 47, 63), Color.FromArgb(47, 104, 54))
        lbl.Text = text
    End Sub

    Private Sub Grid_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs)
        Dim grid As DataGridView = DirectCast(sender, DataGridView)
        Dim headers As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase) From {
            {"NombreUsuario", "Usuario"},
            {"NombreCompleto", "Nombre completo"},
            {"EsActivo", "Activo"},
            {"IntentosFallidos", "Intentos fallidos"},
            {"BloqueadoHasta", "Bloqueado hasta"},
            {"FechaCreacion", "Fecha creación"},
            {"FechaUltimoIngreso", "Último ingreso"},
            {"FechaUltimoIn", "Último ingreso"},
            {"Roles", "Roles"},
            {"NombreRol", "Rol"},
            {"Descripcion", "Descripción"},
            {"Permisos", "Permisos"},
            {"ClavePermiso", "Clave permiso"}
        }

        For Each col As DataGridViewColumn In grid.Columns
            If col Is Nothing OrElse String.IsNullOrEmpty(col.Name) Then
                Continue For
            End If
            If col.Name.StartsWith("Id", StringComparison.OrdinalIgnoreCase) Then
                col.Visible = False
                Continue For
            End If

            If headers.ContainsKey(col.Name) Then
                col.HeaderText = headers(col.Name)
            End If

            If String.Equals(col.Name, "Descripcion", StringComparison.OrdinalIgnoreCase) Then
                col.FillWeight = 170
            ElseIf String.Equals(col.Name, "NombreCompleto", StringComparison.OrdinalIgnoreCase) Then
                col.FillWeight = 155
            ElseIf String.Equals(col.Name, "FechaCreacion", StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(col.Name, "FechaUltimoIngreso", StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(col.Name, "FechaUltimoIn", StringComparison.OrdinalIgnoreCase) Then
                col.FillWeight = 115
            End If
        Next

        If grid.Rows.Count > 0 Then
            Try
                grid.FirstDisplayedScrollingRowIndex = 0
                If grid.CurrentCell Is Nothing AndAlso grid.Columns.Count > 0 Then
                    grid.CurrentCell = grid.Rows(0).Cells(grid.Columns.Cast(Of DataGridViewColumn)().First(Function(c) c.Visible).Index)
                End If
            Catch
            End Try
        End If
    End Sub

    Private Sub ConfigureFieldTooltips()
        If _tooltips Is Nothing Then
            Exit Sub
        End If
        _tooltips.SetToolTip(_txtNombreUsuario, "Nombre de inicio de sesión del usuario.")
        _tooltips.SetToolTip(_txtNombreCompletoUsuario, "Nombre completo visible para reportes y auditoría.")
        _tooltips.SetToolTip(_txtContrasenaUsuario, "Contraseña temporal o nueva contraseña.")
        _tooltips.SetToolTip(_txtNombreRol, "Nombre único del rol.")
        _tooltips.SetToolTip(_txtDescripcionRol, "Descripción funcional del rol.")
        _tooltips.SetToolTip(_txtClavePermiso, "Clave técnica del permiso (única).")
        _tooltips.SetToolTip(_txtDescripcionPermiso, "Descripción funcional del permiso.")
    End Sub
End Class
