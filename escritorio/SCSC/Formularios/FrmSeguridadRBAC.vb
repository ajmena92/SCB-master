Option Explicit On
Option Strict On

Imports System.Data
Imports System.ComponentModel
Imports System.Linq
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

Partial Friend Class FrmSeguridadRBAC

    Private ReadOnly _service As New SeguridadRbacService()
    Private _accessContext As SeguridadRbacService.UserAccessContext
    Private _layoutReady As Boolean = False
    Private _usuariosData As DataTable
    Private _rolesData As DataTable
    Private _usuariosFilteredData As DataTable
    Private _rolesFilteredData As DataTable
    Private _usuariosPageIndex As Integer
    Private _rolesPageIndex As Integer
    Private _lblFeedbackUsuarios As Label
    Private _lblFeedbackRoles As Label


    Private _tooltips As ToolTip
    Private _usuariosHost As TableLayoutPanel
    Private _rolesHost As TableLayoutPanel
    Private _rolesPageHost As TableLayoutPanel
    Private _usuariosActionsPanel As Panel
    Private _rolesActionsPanel As Panel
    Private _rolesCrudActions As FlowLayoutPanel
    Private _rolesPermissionActions As FlowLayoutPanel
    Private _footerHost As TableLayoutPanel
    Private _usuariosHeader As Panel
    Private _rolesHeader As Panel
    Private _floatingCloseHost As Panel
    Private _txtBuscarUsuarios As TextBox
    Private _cmbEstadoUsuarios As ComboBox
    Private _lblResumenUsuarios As Label
    Private _txtBuscarRoles As TextBox
    Private _cmbEstadoRoles As ComboBox
    Private _lblResumenRoles As Label
    Private _lblFooterHint As Label
    Private _lblFooterPageInfo As Label
    Private _lnkPaginaAnterior As LinkLabel
    Private _lnkPaginaSiguiente As LinkLabel
    Private _btnNuevoRegistro As Button
    Private _footerStatusText As String = String.Empty
    Private _footerStatusIsError As Boolean

    Private Const DefaultFooterHint As String = ""
    Private Const GridPageSize As Integer = 10
    Private Const GridActionEditColumnName As String = "__Editar"
    Private Const GridActionDeleteColumnName As String = "__Eliminar"

    Private Sub EnsurePermissionsTabHidden()
        If TabsPrincipal.TabPages.Contains(TabPermisos) Then
            TabsPrincipal.TabPages.Remove(TabPermisos)
        End If
    End Sub

    Private Function PuedeAbrirModuloSeguridad() As Boolean
        Return TienePermisoSeguridad(
            "Seguridad.Ver",
            "Usuarios.Ver",
            "Usuarios.Crear",
            "Usuarios.Editar",
            "Usuarios.Eliminar",
            "Usuarios.CambiarClave",
            "Roles.Ver",
            "Roles.Crear",
            "Roles.Editar",
            "Roles.Eliminar",
            "Roles.Permisos.Gestionar")
    End Function

    Private Function PuedeVerUsuarios() As Boolean
        Return TienePermisoSeguridad("Seguridad.Ver", "Usuarios.Ver", "Usuarios.Crear", "Usuarios.Editar", "Usuarios.Eliminar", "Usuarios.CambiarClave")
    End Function

    Private Function PuedeVerRoles() As Boolean
        Return TienePermisoSeguridad("Seguridad.Ver", "Roles.Ver", "Roles.Crear", "Roles.Editar", "Roles.Eliminar", "Roles.Permisos.Gestionar")
    End Function

    Private Function TienePermisoSeguridad(ParamArray permissionKeys() As String) As Boolean
        If _accessContext Is Nothing Then
            Return False
        End If

        Return _accessContext.TienePermiso(permissionKeys)
    End Function

    Private Sub ApplyAccessValidation()
        Dim canViewUsers As Boolean = PuedeVerUsuarios()
        Dim canViewRoles As Boolean = PuedeVerRoles()

        RebuildVisibleTabs(canViewUsers, canViewRoles)

        SetButtonAccess(_btnCrearUsuario, TienePermisoSeguridad("Usuarios.Crear"))
        SetButtonAccess(_btnActualizarUsuario, TienePermisoSeguridad("Usuarios.Editar"))
        SetButtonAccess(_btnCambiarClave, TienePermisoSeguridad("Usuarios.CambiarClave", "Usuarios.Editar"))
        SetButtonAccess(_btnAsignarRolUsuario, TienePermisoSeguridad("Usuarios.Editar"))
        SetButtonAccess(_btnRevocarRolUsuario, TienePermisoSeguridad("Usuarios.Editar"))
        SetButtonAccess(_btnEliminarUsuario, TienePermisoSeguridad("Usuarios.Eliminar"))

        SetButtonAccess(_btnCrearRol, TienePermisoSeguridad("Roles.Crear"))
        SetButtonAccess(_btnActualizarRol, TienePermisoSeguridad("Roles.Editar"))
        SetButtonAccess(_btnEliminarRol, TienePermisoSeguridad("Roles.Eliminar"))
        SetButtonAccess(_btnAsignarPermisoRol, TienePermisoSeguridad("Roles.Permisos.Gestionar"))
        SetButtonAccess(_btnRevocarPermisoRol, TienePermisoSeguridad("Roles.Permisos.Gestionar"))

        If _cmbPermisoRol IsNot Nothing Then
            _cmbPermisoRol.Enabled = TienePermisoSeguridad("Roles.Permisos.Gestionar")
        End If

        EnsurePermissionsTabHidden()
    End Sub

    Private Sub RebuildVisibleTabs(ByVal canViewUsers As Boolean, ByVal canViewRoles As Boolean)
        TabsPrincipal.SuspendLayout()
        Try
            EnsurePermissionsTabHidden()
            TabsPrincipal.TabPages.Clear()

            If canViewUsers Then
                TabsPrincipal.TabPages.Add(TabUsuarios)
            End If

            If canViewRoles Then
                TabsPrincipal.TabPages.Add(TabRoles)
            End If
        Finally
            TabsPrincipal.ResumeLayout()
        End Try

        If TabsPrincipal.TabPages.Count = 0 Then
            MsgBox("No tiene permisos asignados para administrar usuarios o roles.", MsgBoxStyle.Exclamation)
            Me.Close()
            Return
        End If

        TabsPrincipal.SelectedIndex = 0
    End Sub

    Private Sub SetButtonAccess(ByVal button As Button, ByVal isAllowed As Boolean)
        If button Is Nothing Then
            Exit Sub
        End If

        button.Visible = isAllowed
        button.Enabled = isAllowed
    End Sub

    Public Sub New()
        InitializeComponent()
        If LicenseManager.UsageMode = LicenseUsageMode.Designtime Then
            Return
        End If
        EnsurePermissionsTabHidden()
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MinimumSize = New Size(1080, 520)
        Me.Size = New Size(1220, 560)
        Me.WindowState = FormWindowState.Maximized
        Me.Text = "Gestion de Seguridad RBAC"
        AddHandler Me.Shown, AddressOf SetInitialFocus
    End Sub

    Private Sub FrmSeguridadRBAC_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If IsInDesignMode() Then
            Return
        End If

        Try
            _service.EnsurePermissionCatalog()
            _accessContext = _service.GetUserAccessContext(CodigoUsuario)
            ErrorLogger.LogInfo("FrmSeguridadRBAC.Load", "Usuario=" & CodigoUsuario & ", SeguridadVer=" & TienePermisoSeguridad("Seguridad.Ver").ToString())
            If Not PuedeAbrirModuloSeguridad() Then
                MsgBox("No tiene permisos para acceder al modulo de seguridad.", MsgBoxStyle.Exclamation)
                Me.Close()
                Return
            End If

            Me.KeyPreview = True
            UIThemeManagerV2.Apply(Me, "dialogo")
            ApplyVisualStandard2026()
            EnsureCrudRuntimeLayout()
            ApplyAccessValidation()
            ErrorLogger.LogInfo("FrmSeguridadRBAC.Load", "TabsVisibles=" & TabsPrincipal.TabPages.Count.ToString())
            If Me.IsDisposed Then
                Return
            End If
            RefreshStableFooterLayout()

            ConfigurarGrid(_gridUsuarios)
            ConfigurarGrid(_gridRoles)
            _txtNombreUsuario.ReadOnly = True
            _txtNombreCompletoUsuario.ReadOnly = True
            _txtContrasenaUsuario.PasswordChar = "*"c
            _txtContrasenaUsuario.ReadOnly = True
            _chkUsuarioActivo.AutoCheck = False
            _txtNombreRol.ReadOnly = True
            _txtDescripcionRol.ReadOnly = True
            _chkRolActivo.AutoCheck = False
            _cmbRolUsuario.DropDownStyle = ComboBoxStyle.DropDownList
            _cmbPermisoRol.DropDownStyle = ComboBoxStyle.DropDownList
            _tooltips = New ToolTip()
            ConfigureFieldTooltips()
            RecargarTodo()
            UpdateActionStates()
            ApplySearchPlaceholder(_txtBuscarUsuarios)
            ApplySearchPlaceholder(_txtBuscarRoles)
        Catch ex As Exception
            ErrorLogger.LogException("FrmSeguridadRBAC.Load", ex)
            MsgBox("Error cargando formulario de seguridad RBAC: " & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub FrmSeguridadRBAC_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        RefreshStableFooterLayout()
        AdjustCrudGridHeights()
    End Sub

    Private Sub FrmSeguridadRBAC_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If Not IsInDesignMode() Then
            RefreshStableFooterLayout()
            AdjustCrudGridHeights()
        End If
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
        grid.AllowUserToOrderColumns = False
        grid.AllowUserToResizeRows = False
        grid.RowHeadersVisible = False
        grid.BackgroundColor = Color.White
        grid.BorderStyle = BorderStyle.None
        grid.GridColor = Color.FromArgb(223, 228, 236)
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(244, 246, 249)
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(29, 42, 61)
        grid.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI Semibold", 10.0!, FontStyle.Bold)
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
        grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(244, 246, 249)
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(29, 42, 61)
        grid.ColumnHeadersHeight = 36
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        grid.RowTemplate.Height = 38
        grid.DefaultCellStyle.BackColor = Color.White
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(36, 51, 77)
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(228, 236, 246)
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 33, 59)
        grid.DefaultCellStyle.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular)
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 254)
        grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False
        grid.ScrollBars = ScrollBars.Vertical
        AddHandler grid.DataBindingComplete, AddressOf Grid_DataBindingComplete
        AddHandler grid.CellFormatting, AddressOf Grid_CellFormatting
        AddHandler grid.CellPainting, AddressOf Grid_CellPainting
        AddHandler grid.RowPrePaint, AddressOf Grid_RowPrePaint
        AddHandler grid.DataError, AddressOf Grid_DataError
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
        End Select
    End Sub

    Private Sub ConfigureCrudLayoutTables()
        If LayoutUsuarios.RowStyles.Count >= 2 Then
            LayoutUsuarios.RowStyles(0).SizeType = SizeType.Percent
            LayoutUsuarios.RowStyles(0).Height = 100
            LayoutUsuarios.RowStyles(1).SizeType = SizeType.Absolute
            LayoutUsuarios.RowStyles(1).Height = 56
        End If

        If LayoutRoles.RowStyles.Count >= 4 Then
            LayoutRoles.RowStyles(0).SizeType = SizeType.Absolute
            LayoutRoles.RowStyles(0).Height = 40
            LayoutRoles.RowStyles(1).SizeType = SizeType.Absolute
            LayoutRoles.RowStyles(1).Height = 40
            LayoutRoles.RowStyles(2).SizeType = SizeType.Percent
            LayoutRoles.RowStyles(2).Height = 100
            LayoutRoles.RowStyles(3).SizeType = SizeType.Absolute
            LayoutRoles.RowStyles(3).Height = 56
        End If

    End Sub

    Private Sub ConfigureCrudFlow(ByVal flow As FlowLayoutPanel)
        flow.AutoSize = False
        flow.WrapContents = False
        flow.Dock = DockStyle.Fill
        flow.FlowDirection = FlowDirection.LeftToRight
        flow.AutoScroll = True
        flow.Height = 46
        flow.Padding = New Padding(0, 4, 0, 2)
        flow.Margin = New Padding(0, 4, 0, 0)
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

    Private Sub ApplyVisualStandard2026()
        Me.BackColor = Color.FromArgb(246, 248, 252)
        Me.FormBorderStyle = FormBorderStyle.Sizable
        Me.ControlBox = True
        Me.MaximizeBox = True
        Me.MinimizeBox = True
        Me.WindowState = FormWindowState.Maximized
        Me.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular)
        TabsPrincipal.Font = New Font("Segoe UI Semibold", 10.5!, FontStyle.Bold)
        TabsPrincipal.Padding = New Point(20, 10)
        TabsPrincipal.ItemSize = New Size(132, 36)
        TabsPrincipal.SizeMode = TabSizeMode.Normal
        TabsPrincipal.Appearance = TabAppearance.Normal
        TabsPrincipal.HotTrack = True
        TabsPrincipal.BackColor = Color.FromArgb(246, 248, 252)
        TabUsuarios.BackColor = Color.FromArgb(246, 248, 252)
        TabRoles.BackColor = Color.FromArgb(246, 248, 252)
        LabelUsuarioNombre.Font = UIConstants.FontBodyStrong()
        LabelUsuarioCompleto.Font = UIConstants.FontBodyStrong()
        LabelUsuarioContrasena.Font = UIConstants.FontBodyStrong()
        LabelRolAsignar.Font = UIConstants.FontBodyStrong()
        LabelRolesUsuario.Font = UIConstants.FontBodyStrong()

        LabelNombreRol.Font = UIConstants.FontBodyStrong()
        LabelDescripcionRol.Font = UIConstants.FontBodyStrong()
        LabelPermisoAsignar.Font = UIConstants.FontBodyStrong()
        LabelPermisosRol.Font = UIConstants.FontBodyStrong()

        LabelUsuarioNombre.Text = "Usuario"
        LabelUsuarioCompleto.Text = "Nombre completo"
        LabelUsuarioContrasena.Text = "Contraseña"
        LabelRolAsignar.Text = "Rol para asignar"
        LabelRolesUsuario.Text = "Roles asignados"
        LabelNombreRol.Text = "Rol"
        LabelDescripcionRol.Text = "Descripción"
        LabelPermisoAsignar.Text = "Permiso para asignar"
        LabelPermisosRol.Text = "Permisos asignados"

        _txtNombreUsuario.Font = UIConstants.FontBody()
        _txtNombreCompletoUsuario.Font = UIConstants.FontBody()
        _txtContrasenaUsuario.Font = UIConstants.FontBody()
        _txtNombreRol.Font = UIConstants.FontBody()
        _txtDescripcionRol.Font = UIConstants.FontBody()
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
        FormatearCampos(_txtNombreUsuario)
        FormatearCampos(_txtNombreCompletoUsuario)
        FormatearCampos(_txtContrasenaUsuario)
        FormatearCampos(_txtNombreRol)
        FormatearCampos(_txtDescripcionRol)
        FormatearCampos(_cmbRolUsuario)
        FormatearCampos(_cmbPermisoRol)
        FormatearCampos(_lstRolesUsuario)
        FormatearCampos(_lstPermisosRol)

        PanelUsuariosBottom.BackColor = Color.FromArgb(246, 248, 252)
        PanelRolesBottom.BackColor = Color.White
        LayoutUsuarios.BackColor = Color.FromArgb(246, 248, 252)
        LayoutUsuariosWorkspace.BackColor = Color.FromArgb(246, 248, 252)
        LayoutUsuarioDetalle.BackColor = Color.White
        LayoutUsuarioRoles.BackColor = Color.White
        LayoutRoles.BackColor = Color.White
        GroupUsuarioDetalle.BackColor = Color.White
        GroupUsuarioRoles.BackColor = Color.White
        GroupUsuarioDetalle.ForeColor = Color.FromArgb(36, 51, 77)
        GroupUsuarioRoles.ForeColor = Color.FromArgb(36, 51, 77)
        GroupUsuarioDetalle.Font = UIConstants.FontBodyStrong()
        GroupUsuarioRoles.Font = UIConstants.FontBodyStrong()
        FlowUsuariosBotones.BackColor = Color.FromArgb(246, 248, 252)
        FlowRolesBotones.BackColor = PanelRolesBottom.BackColor
        _txtNombreUsuario.BackColor = Color.FromArgb(248, 250, 254)
        _txtNombreCompletoUsuario.BackColor = Color.FromArgb(248, 250, 254)
        _txtContrasenaUsuario.BackColor = Color.FromArgb(248, 250, 254)
        _txtNombreRol.BackColor = Color.FromArgb(248, 250, 254)
        _txtDescripcionRol.BackColor = Color.FromArgb(248, 250, 254)
        _chkUsuarioActivo.Enabled = False
        _chkRolActivo.Enabled = False
        _chkUsuarioActivo.ForeColor = Color.FromArgb(76, 90, 112)
        _chkRolActivo.ForeColor = Color.FromArgb(76, 90, 112)
        _chkUsuarioActivo.Text = "Usuario activo"
        _chkRolActivo.Text = "Rol activo"

        FormatearBoton(_btnCrearUsuario)
        FormatearBoton(_btnActualizarUsuario)
        FormatearBoton(_btnCambiarClave)
        FormatearBoton(_btnAsignarRolUsuario)
        FormatearBoton(_btnRevocarRolUsuario, True)
        FormatearBoton(_btnEliminarUsuario, True)
        FormatearBoton(_btnCrearRol)
        FormatearBoton(_btnActualizarRol)
        FormatearBoton(_btnEliminarRol, True)
        FormatearBoton(_btnAsignarPermisoRol)
        FormatearBoton(_btnRevocarPermisoRol, True)
        FormatearBoton(_btnSalir)

        ApplyActionTextByTab()
        ConfigureActionRowButtons(FlowUsuariosBotones, _btnCrearUsuario, _btnActualizarUsuario, _btnCambiarClave, _btnAsignarRolUsuario, _btnRevocarRolUsuario, _btnEliminarUsuario)
        ConfigureActionRowButtons(FlowRolesBotones, _btnCrearRol, _btnActualizarRol, _btnEliminarRol, _btnAsignarPermisoRol, _btnRevocarPermisoRol)
        _btnSalir.Width = 148
        _btnSalir.Height = 32
        _btnSalir.MinimumSize = New Size(148, 32)
        _btnSalir.MaximumSize = New Size(148, 32)
        PanelFooter.Height = 1
        PanelFooter.Padding = New Padding(0)
        PanelFooter.BackColor = Me.BackColor
        _btnSalir.BackColor = Color.FromArgb(76, 90, 112)
        _btnSalir.ForeColor = Color.White
        _btnSalir.TextAlign = ContentAlignment.MiddleCenter
        _btnSalir.Padding = New Padding(8, 0, 8, 0)
        _btnSalir.Font = New Font("Segoe UI Semibold", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
        _btnSalir.Cursor = Cursors.Hand
        _btnSalir.FlatAppearance.MouseOverBackColor = Color.FromArgb(63, 76, 96)
        _btnSalir.FlatAppearance.MouseDownBackColor = Color.FromArgb(53, 65, 82)
    End Sub

    Private Sub ApplyVisualGuards()
        ' Modo designer-first estricto:
        ' evitar ajustes de geometría en runtime para no desalinear el diseño.
    End Sub

    Private Sub EnsureCrudRuntimeLayout()
        If IsInDesignMode() Then
            Exit Sub
        End If

        RestoreTopLevelDesignerLayout()
        RestoreUsersTabDesignerLayout()
        RestoreRolesTabDesignerLayout()
        EnsurePermissionsTabHidden()
        NormalizeLegacyDesignerMetrics()
        ConfigureCrudLayoutTables()
        ConfigureCrudFlow(FlowUsuariosBotones)
        ConfigureCrudFlow(FlowRolesBotones)
        ConfigureFeedbackLabel(TabUsuarios, "SplitUsuarios")
        ConfigureFeedbackLabel(TabRoles, "SplitRoles")
        RefreshStableFooterLayout()
        AdjustCrudGridHeights()

        EnsureActionRowVisible(FlowUsuariosBotones)
        EnsureActionRowVisible(FlowRolesBotones)

        FlowUsuariosBotones.BringToFront()
        FlowRolesBotones.BringToFront()

        EnsureButtonsVisible(
            _btnCrearUsuario, _btnActualizarUsuario, _btnCambiarClave, _btnAsignarRolUsuario, _btnRevocarRolUsuario, _btnEliminarUsuario,
            _btnCrearRol, _btnActualizarRol, _btnEliminarRol, _btnAsignarPermisoRol, _btnRevocarPermisoRol)

        _layoutReady = True
    End Sub

    Private Sub RestoreTopLevelDesignerLayout()
        If Not Me.Controls.Contains(TabsPrincipal) Then
            Me.Controls.Add(TabsPrincipal)
        End If

        If Not Me.Controls.Contains(PanelFooter) Then
            Me.Controls.Add(PanelFooter)
        End If

        TabsPrincipal.Visible = True
        TabsPrincipal.Enabled = True
        TabsPrincipal.Dock = DockStyle.Fill
        TabsPrincipal.Margin = New Padding(0)
        EnsurePermissionsTabHidden()

        PanelFooter.Visible = False
        PanelFooter.Enabled = False
        PanelFooter.Dock = DockStyle.Bottom
        PanelFooter.Height = 1
        PanelFooter.Padding = New Padding(0)

        If Me.Controls.Contains(TabsPrincipal) Then
            Me.Controls.SetChildIndex(TabsPrincipal, 0)
        End If
        If Me.Controls.Contains(PanelFooter) Then
            Me.Controls.SetChildIndex(PanelFooter, 1)
        End If

        TabsPrincipal.BringToFront()
        PanelFooter.SendToBack()
        Me.PerformLayout()
    End Sub

    Private Sub RestoreUsersTabDesignerLayout()
        ConfigureSimpleCrudTab(TabUsuarios, _gridUsuarios, PanelUsuariosBottom)
    End Sub

    Private Sub RestoreRolesTabDesignerLayout()
        ConfigureSimpleCrudTab(TabRoles, _gridRoles, PanelRolesBottom)
    End Sub

    Private Sub ConfigureSimpleCrudTab(ByVal host As TabPage, ByVal grid As DataGridView, ByVal editorPanel As Panel)
        If host Is Nothing OrElse grid Is Nothing Then
            Exit Sub
        End If

        Dim tabHost As TableLayoutPanel = GetCrudTabHost(host)
        Dim toolbarPanel As Panel = GetCrudToolbarPanel(host)

        host.SuspendLayout()
        tabHost.SuspendLayout()
        Try
            If Not editorPanel Is Nothing Then
                If editorPanel.Parent Is host Then
                    host.Controls.Remove(editorPanel)
                End If
                editorPanel.Visible = False
                editorPanel.Dock = DockStyle.None
            End If

            If grid.Parent IsNot tabHost Then
                If grid.Parent IsNot Nothing Then
                    grid.Parent.Controls.Remove(grid)
                End If
                tabHost.Controls.Add(grid, 0, 1)
            End If

            If toolbarPanel.Parent IsNot tabHost Then
                If toolbarPanel.Parent IsNot Nothing Then
                    toolbarPanel.Parent.Controls.Remove(toolbarPanel)
                End If
                tabHost.Controls.Add(toolbarPanel, 0, 0)
            End If

            If tabHost.Parent IsNot host Then
                If tabHost.Parent IsNot Nothing Then
                    tabHost.Parent.Controls.Remove(tabHost)
                End If
                host.Controls.Add(tabHost)
            End If

            tabHost.Dock = DockStyle.Fill
            toolbarPanel.Dock = DockStyle.Fill
            toolbarPanel.Visible = True
            toolbarPanel.Margin = New Padding(0)
            toolbarPanel.Padding = New Padding(0)
            toolbarPanel.BackColor = host.BackColor

            grid.Dock = DockStyle.Fill
            grid.Margin = New Padding(0)
            grid.Visible = True
            grid.ScrollBars = ScrollBars.Vertical
            grid.BringToFront()
            tabHost.Controls.SetChildIndex(toolbarPanel, 0)
            tabHost.Controls.SetChildIndex(grid, 1)
        Finally
            tabHost.ResumeLayout()
            host.ResumeLayout()
        End Try
    End Sub

    Private Function GetCrudTabHost(ByVal host As TabPage) As TableLayoutPanel
        Dim tabHost As TableLayoutPanel = Nothing

        If host Is TabUsuarios Then
            tabHost = _usuariosHost
        ElseIf host Is TabRoles Then
            tabHost = _rolesPageHost
        End If

        If tabHost Is Nothing Then
            tabHost = New TableLayoutPanel()
            tabHost.ColumnCount = 1
            tabHost.RowCount = 2
            tabHost.Dock = DockStyle.Fill
            tabHost.Margin = New Padding(0)
            tabHost.Padding = New Padding(0)
            tabHost.BackColor = host.BackColor
            tabHost.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
            tabHost.RowStyles.Add(New RowStyle(SizeType.Absolute, 46.0!))
            tabHost.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))

            If host Is TabUsuarios Then
                tabHost.Name = "UsuariosSimpleHost"
                _usuariosHost = tabHost
            ElseIf host Is TabRoles Then
                tabHost.Name = "RolesSimpleHost"
                _rolesPageHost = tabHost
            End If
        End If

        Return tabHost
    End Function

    Private Function GetCrudToolbarPanel(ByVal host As TabPage) As Panel
        Dim toolbarPanel As Panel = Nothing

        If host Is TabUsuarios Then
            toolbarPanel = _usuariosActionsPanel
        ElseIf host Is TabRoles Then
            toolbarPanel = _rolesActionsPanel
        End If

        If toolbarPanel Is Nothing Then
            toolbarPanel = New Panel()
            toolbarPanel.Dock = DockStyle.Fill
            toolbarPanel.Margin = New Padding(0)
            toolbarPanel.Padding = New Padding(0)
            toolbarPanel.BackColor = host.BackColor

            If host Is TabUsuarios Then
                toolbarPanel.Name = "UsuariosToolbarPanel"
                _usuariosActionsPanel = toolbarPanel
            ElseIf host Is TabRoles Then
                toolbarPanel.Name = "RolesToolbarPanel"
                _rolesActionsPanel = toolbarPanel
            End If
        End If

        Return toolbarPanel
    End Function

    Private Sub RestoreTabDesignerLayout(ByVal host As TabPage,
                                         ByVal grid As DataGridView,
                                         ByVal editorPanel As Panel,
                                         ByVal layout As TableLayoutPanel,
                                         ByVal flow As FlowLayoutPanel,
                                         ByVal buttons As IEnumerable(Of Button),
                                         ByVal gridHeight As Integer)
        If host Is Nothing OrElse grid Is Nothing OrElse editorPanel Is Nothing OrElse layout Is Nothing OrElse flow Is Nothing Then
            Exit Sub
        End If

        host.SuspendLayout()
        editorPanel.SuspendLayout()
        layout.SuspendLayout()
        flow.SuspendLayout()
        Try
            For Each child As Control In host.Controls.Cast(Of Control).ToArray()
                If child Is grid OrElse child Is editorPanel Then
                    Continue For
                End If
                host.Controls.Remove(child)
            Next

            If grid.Parent IsNot host Then
                If grid.Parent IsNot Nothing Then
                    grid.Parent.Controls.Remove(grid)
                End If
                host.Controls.Add(grid)
            End If

            If editorPanel.Parent IsNot host Then
                If editorPanel.Parent IsNot Nothing Then
                    editorPanel.Parent.Controls.Remove(editorPanel)
                End If
                host.Controls.Add(editorPanel)
            End If

            If layout.Parent IsNot editorPanel Then
                If layout.Parent IsNot Nothing Then
                    layout.Parent.Controls.Remove(layout)
                End If
                editorPanel.Controls.Add(layout)
            End If

            For Each child As Control In editorPanel.Controls.Cast(Of Control).ToArray()
                If child Is layout OrElse child.Name.EndsWith("_Feedback", StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If
                editorPanel.Controls.Remove(child)
            Next

            For Each child As Control In flow.Controls.Cast(Of Control).ToArray()
                Dim currentButton As Button = TryCast(child, Button)
                If Not currentButton Is Nothing AndAlso buttons.Contains(currentButton) Then
                    Continue For
                End If
                flow.Controls.Remove(child)
            Next

            grid.Dock = DockStyle.Top
            grid.Height = gridHeight
            grid.Visible = True
            grid.Margin = New Padding(0)

            editorPanel.Dock = DockStyle.Fill
            editorPanel.Visible = True
            editorPanel.AutoScroll = True
            editorPanel.Margin = New Padding(0)

            layout.Dock = DockStyle.Fill
            layout.Visible = True
            layout.Margin = New Padding(0)

            For Each child As Control In layout.Controls
                If child Is Nothing Then
                    Continue For
                End If
                child.Visible = True
            Next

            For Each btn As Button In buttons
                If btn Is Nothing Then
                    Continue For
                End If
                btn.Visible = True
                If btn.Parent IsNot flow Then
                    If btn.Parent IsNot Nothing Then
                        btn.Parent.Controls.Remove(btn)
                    End If
                    flow.Controls.Add(btn)
                End If
            Next

            Dim buttonIndex As Integer = 0
            For Each btn As Button In buttons
                If btn Is Nothing OrElse Not flow.Controls.Contains(btn) Then
                    Continue For
                End If
                flow.Controls.SetChildIndex(btn, buttonIndex)
                buttonIndex += 1
            Next

            flow.Dock = DockStyle.Fill
            flow.Visible = True

            SyncEditorPanelDockOrder(editorPanel, layout)
            editorPanel.PerformLayout()
            host.PerformLayout()
        Finally
            flow.ResumeLayout()
            layout.ResumeLayout()
            editorPanel.ResumeLayout()
            host.ResumeLayout()
        End Try
    End Sub

    Private Sub SyncEditorPanelDockOrder(ByVal editorPanel As Panel, ByVal layout As TableLayoutPanel)
        If editorPanel Is Nothing Then
            Exit Sub
        End If

        For Each child As Control In editorPanel.Controls
            If child Is Nothing Then
                Continue For
            End If

            If child Is layout Then
                child.Dock = DockStyle.Fill
                child.SendToBack()
                Continue For
            End If

            If child.Name.EndsWith("_Feedback", StringComparison.OrdinalIgnoreCase) Then
                child.Dock = DockStyle.Bottom
                child.BringToFront()
            End If
        Next
    End Sub

    Private Sub RefreshStableFooterLayout()
        If _btnSalir Is Nothing OrElse TabsPrincipal Is Nothing OrElse TabsPrincipal.SelectedTab Is Nothing Then
            Exit Sub
        End If

        EnsurePermissionsTabHidden()
        EnsureToolbarHost()
        SyncFooterPrimaryButtonState()
        RefreshFooterPagerState()

        If PanelFooter IsNot Nothing Then
            PanelFooter.Visible = False
            PanelFooter.Enabled = False
            PanelFooter.Height = 1
            PanelFooter.Padding = New Padding(0)
            PanelFooter.Controls.Clear()
        End If

        Dim selectedTab As TabPage = TabsPrincipal.SelectedTab
        Dim targetToolbarPanel As Panel = GetCrudToolbarPanel(selectedTab)
        If targetToolbarPanel Is Nothing Then
            Exit Sub
        End If

        targetToolbarPanel.SuspendLayout()
        Try
            If _footerHost.Parent IsNot Nothing AndAlso _footerHost.Parent IsNot targetToolbarPanel Then
                _footerHost.Parent.Controls.Remove(_footerHost)
            End If
            If _footerHost.Parent Is Nothing Then
                targetToolbarPanel.Controls.Clear()
            End If
            If _footerHost.Parent IsNot targetToolbarPanel Then
                targetToolbarPanel.Controls.Add(_footerHost)
            End If

            _footerHost.Dock = DockStyle.Fill
            _footerHost.Margin = New Padding(0)
            _footerHost.Visible = True
            targetToolbarPanel.Controls.SetChildIndex(_footerHost, 0)
            _footerHost.BringToFront()
        Finally
            targetToolbarPanel.ResumeLayout()
        End Try
    End Sub

    Private Sub EnsureToolbarHost()
        If _footerHost Is Nothing Then
            _footerHost = New TableLayoutPanel()
            _footerHost.Name = "CrudTopToolbar"
            _footerHost.ColumnCount = 3
            _footerHost.RowCount = 1
            _footerHost.Dock = DockStyle.Top
            _footerHost.Height = 44
            _footerHost.Margin = New Padding(0, 0, 0, 10)
            _footerHost.Padding = New Padding(0, 0, 0, 6)
            _footerHost.BackColor = Color.FromArgb(246, 248, 252)
            _footerHost.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 244.0!))
            _footerHost.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
            _footerHost.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 274.0!))
            _footerHost.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))
        Else
            _footerHost.Controls.Clear()
        End If

        If _lnkPaginaAnterior Is Nothing Then
            _lnkPaginaAnterior = BuildPagerLink("Anterior")
            AddHandler _lnkPaginaAnterior.Click, AddressOf FooterPaginaAnterior_Click
        End If

        If _lblFooterPageInfo Is Nothing Then
            _lblFooterPageInfo = New Label()
            _lblFooterPageInfo.Name = "FooterPageInfo"
            _lblFooterPageInfo.AutoSize = False
            _lblFooterPageInfo.Width = 110
            _lblFooterPageInfo.Height = 28
            _lblFooterPageInfo.TextAlign = ContentAlignment.MiddleCenter
            _lblFooterPageInfo.Font = New Font("Segoe UI Semibold", 8.5!, FontStyle.Bold)
            _lblFooterPageInfo.ForeColor = Color.FromArgb(78, 92, 113)
            _lblFooterPageInfo.Margin = New Padding(4, 0, 4, 0)
        End If

        If _lnkPaginaSiguiente Is Nothing Then
            _lnkPaginaSiguiente = BuildPagerLink("Siguiente")
            AddHandler _lnkPaginaSiguiente.Click, AddressOf FooterPaginaSiguiente_Click
        End If

        If _btnNuevoRegistro Is Nothing Then
            _btnNuevoRegistro = New Button()
            _btnNuevoRegistro.Name = "_btnNuevoRegistro"
            AddHandler _btnNuevoRegistro.Click, AddressOf BtnNuevoRegistro_Click
        End If

        ConfigureToolbarButton(_btnNuevoRegistro, Color.FromArgb(49, 90, 140), Color.White, 144)
        _btnNuevoRegistro.FlatAppearance.MouseOverBackColor = Color.FromArgb(41, 76, 119)
        _btnNuevoRegistro.FlatAppearance.MouseDownBackColor = Color.FromArgb(34, 64, 101)

        ConfigureToolbarButton(_btnSalir, Color.FromArgb(96, 108, 126), Color.White, 108)
        _btnSalir.FlatAppearance.MouseOverBackColor = Color.FromArgb(82, 93, 109)
        _btnSalir.FlatAppearance.MouseDownBackColor = Color.FromArgb(70, 80, 95)

        Dim pagerPanel As New FlowLayoutPanel()
        pagerPanel.Name = "PagerPanel"
        pagerPanel.Dock = DockStyle.Fill
        pagerPanel.Margin = New Padding(0)
        pagerPanel.Padding = New Padding(0, 6, 0, 0)
        pagerPanel.WrapContents = False
        pagerPanel.AutoScroll = False
        pagerPanel.FlowDirection = FlowDirection.LeftToRight
        pagerPanel.BackColor = _footerHost.BackColor
        pagerPanel.Controls.Add(_lnkPaginaAnterior)
        pagerPanel.Controls.Add(_lblFooterPageInfo)
        pagerPanel.Controls.Add(_lnkPaginaSiguiente)

        Dim spacer As New Panel()
        spacer.Dock = DockStyle.Fill
        spacer.Margin = New Padding(0)
        spacer.BackColor = _footerHost.BackColor

        Dim actionsPanel As New FlowLayoutPanel()
        actionsPanel.Name = "ToolbarActions"
        actionsPanel.Dock = DockStyle.Fill
        actionsPanel.Margin = New Padding(0)
        actionsPanel.Padding = New Padding(0, 4, 0, 0)
        actionsPanel.WrapContents = False
        actionsPanel.AutoScroll = False
        actionsPanel.FlowDirection = FlowDirection.RightToLeft
        actionsPanel.BackColor = _footerHost.BackColor

        If _btnSalir.Parent IsNot Nothing Then
            _btnSalir.Parent.Controls.Remove(_btnSalir)
        End If
        If _btnNuevoRegistro.Parent IsNot Nothing Then
            _btnNuevoRegistro.Parent.Controls.Remove(_btnNuevoRegistro)
        End If

        actionsPanel.Controls.Add(_btnSalir)
        actionsPanel.Controls.Add(_btnNuevoRegistro)

        _footerHost.Controls.Add(pagerPanel, 0, 0)
        _footerHost.Controls.Add(spacer, 1, 0)
        _footerHost.Controls.Add(actionsPanel, 2, 0)
    End Sub

    Private Function BuildPagerLink(ByVal text As String) As LinkLabel
        Dim link As New LinkLabel()
        link.AutoSize = False
        link.Width = 76
        link.Height = 28
        link.Text = text
        link.TextAlign = ContentAlignment.MiddleCenter
        link.LinkBehavior = LinkBehavior.NeverUnderline
        link.Font = New Font("Segoe UI Semibold", 8.5!, FontStyle.Bold)
        link.LinkColor = Color.FromArgb(84, 98, 121)
        link.ActiveLinkColor = Color.FromArgb(49, 90, 140)
        link.VisitedLinkColor = Color.FromArgb(84, 98, 121)
        link.DisabledLinkColor = Color.FromArgb(174, 182, 194)
        link.Margin = New Padding(0)
        Return link
    End Function

    Private Sub ConfigureToolbarButton(ByVal btn As Button, ByVal backColor As Color, ByVal foreColor As Color, ByVal width As Integer)
        btn.AutoSize = False
        btn.AutoEllipsis = True
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.BackColor = backColor
        btn.ForeColor = foreColor
        btn.Font = New Font("Segoe UI Semibold", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
        btn.Height = 32
        btn.Width = width
        btn.MinimumSize = New Size(width, 32)
        btn.MaximumSize = New Size(width, 32)
        btn.Margin = New Padding(8, 0, 0, 0)
        btn.Padding = New Padding(10, 0, 10, 0)
        btn.TextAlign = ContentAlignment.MiddleCenter
        btn.UseVisualStyleBackColor = False
        btn.Visible = True
        btn.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btn.Cursor = Cursors.Hand
    End Sub

    Private Sub RefreshFooterPagerState()
        If _lblFooterPageInfo Is Nothing OrElse _lnkPaginaAnterior Is Nothing OrElse _lnkPaginaSiguiente Is Nothing Then
            Exit Sub
        End If

        Dim totalRows As Integer
        Dim currentPageIndex As Integer

        If TabsPrincipal.SelectedTab Is TabUsuarios Then
            totalRows = If(_usuariosFilteredData Is Nothing, 0, _usuariosFilteredData.Rows.Count)
            currentPageIndex = _usuariosPageIndex
        ElseIf TabsPrincipal.SelectedTab Is TabRoles Then
            totalRows = If(_rolesFilteredData Is Nothing, 0, _rolesFilteredData.Rows.Count)
            currentPageIndex = _rolesPageIndex
        Else
            totalRows = 0
            currentPageIndex = 0
        End If

        Dim totalPages As Integer = GetTotalPages(totalRows)
        Dim currentPage As Integer = If(totalRows = 0, 0, currentPageIndex + 1)

        If totalRows <= 0 Then
            _lblFooterPageInfo.Text = "Sin registros"
        Else
            _lblFooterPageInfo.Text = String.Format("Pagina {0} de {1}", currentPage, totalPages)
        End If
        _lnkPaginaAnterior.Enabled = currentPageIndex > 0
        _lnkPaginaSiguiente.Enabled = currentPageIndex < totalPages - 1
        _lnkPaginaAnterior.Visible = True
        _lnkPaginaSiguiente.Visible = True
        _lblFooterPageInfo.Visible = True
    End Sub

    Private Sub FooterPaginaAnterior_Click(sender As Object, e As EventArgs)
        If TabsPrincipal.SelectedTab Is TabUsuarios Then
            If _usuariosPageIndex <= 0 Then
                Exit Sub
            End If
            _usuariosPageIndex -= 1
            BindUsuariosPage()
        ElseIf TabsPrincipal.SelectedTab Is TabRoles Then
            If _rolesPageIndex <= 0 Then
                Exit Sub
            End If
            _rolesPageIndex -= 1
            BindRolesPage()
        End If
    End Sub

    Private Sub FooterPaginaSiguiente_Click(sender As Object, e As EventArgs)
        If TabsPrincipal.SelectedTab Is TabUsuarios Then
            Dim totalPages As Integer = GetTotalPages(If(_usuariosFilteredData Is Nothing, 0, _usuariosFilteredData.Rows.Count))
            If _usuariosPageIndex >= totalPages - 1 Then
                Exit Sub
            End If
            _usuariosPageIndex += 1
            BindUsuariosPage()
        ElseIf TabsPrincipal.SelectedTab Is TabRoles Then
            Dim totalPages As Integer = GetTotalPages(If(_rolesFilteredData Is Nothing, 0, _rolesFilteredData.Rows.Count))
            If _rolesPageIndex >= totalPages - 1 Then
                Exit Sub
            End If
            _rolesPageIndex += 1
            BindRolesPage()
        End If
    End Sub

    Private Sub SyncFooterHintPresentation()
        ' El formulario simplificado no deja mensajes persistentes en el pie.
    End Sub

    Private Sub SyncFooterPrimaryButtonState()
        If _btnNuevoRegistro Is Nothing Then
            Exit Sub
        End If

        Dim selectedTab As TabPage = TabsPrincipal.SelectedTab
        Dim canCreate As Boolean = False

        If selectedTab Is TabUsuarios Then
            canCreate = TienePermisoSeguridad("Usuarios.Crear")
        ElseIf selectedTab Is TabRoles Then
            canCreate = TienePermisoSeguridad("Roles.Crear")
        End If

        If selectedTab Is TabUsuarios Then
            _btnNuevoRegistro.Text = "+ Nuevo usuario"
            _btnNuevoRegistro.Width = 150
        ElseIf selectedTab Is TabRoles Then
            _btnNuevoRegistro.Text = "+ Nuevo rol"
            _btnNuevoRegistro.Width = 132
        Else
            _btnNuevoRegistro.Text = "Nuevo"
            _btnNuevoRegistro.Width = 120
        End If
        _btnNuevoRegistro.MinimumSize = New Size(_btnNuevoRegistro.Width, 32)
        _btnNuevoRegistro.MaximumSize = New Size(_btnNuevoRegistro.Width, 32)
        _btnNuevoRegistro.Enabled = canCreate
        _btnNuevoRegistro.Visible = Not selectedTab Is Nothing
        _btnSalir.Text = "Cerrar"
        _btnSalir.Visible = True
    End Sub

    Private Sub EnsureRootLayoutVisible()
        RestoreTopLevelDesignerLayout()
        RefreshStableFooterLayout()
    End Sub

    Private Sub NormalizeLegacyDesignerMetrics()
        TabsPrincipal.Margin = New Padding(0)
        TabsPrincipal.Padding = New Point(18, 8)

        For Each tab As TabPage In New TabPage() {TabUsuarios, TabRoles}
            tab.Margin = New Padding(0)
            tab.Padding = New Padding(12)
        Next

        For Each panel As Panel In New Panel() {PanelUsuariosBottom, PanelRolesBottom}
            panel.Margin = New Padding(0)
        Next

        PanelUsuariosBottom.Padding = New Padding(12, 10, 12, 10)
        PanelRolesBottom.Padding = New Padding(8)

        LayoutUsuarios.Margin = New Padding(0)
        LayoutRoles.Margin = New Padding(0)

        _gridUsuarios.Margin = New Padding(0)
        _gridRoles.Margin = New Padding(0)
    End Sub

    Private Sub EnsureDesignerTabLayout(ByVal host As TabPage, ByVal grid As DataGridView, ByVal editorPanel As Panel)
        If host Is Nothing OrElse grid Is Nothing OrElse editorPanel Is Nothing Then
            Exit Sub
        End If

        host.SuspendLayout()
        Try
            host.Controls.Remove(editorPanel)
            host.Controls.Remove(grid)

            editorPanel.Dock = DockStyle.Fill
            grid.Dock = DockStyle.Top

            host.Controls.Add(grid)
            host.Controls.Add(editorPanel)
        Finally
            host.ResumeLayout()
        End Try
    End Sub

    Private Sub AdjustCrudGridHeights()
        ApplyPagedGridHeight(_gridUsuarios)
        ApplyPagedGridHeight(_gridRoles)
    End Sub

    Private Sub ApplyPagedGridHeight(ByVal grid As DataGridView)
        If grid Is Nothing Then
            Exit Sub
        End If

        grid.Height = grid.ColumnHeadersHeight + (grid.RowTemplate.Height * GridPageSize) + 6
    End Sub

    Private Sub AjustarAlturaGridEnTab(ByVal tab As TabPage, ByVal grid As DataGridView, ByVal ratio As Double, ByVal minimo As Integer, ByVal maximo As Integer)
        If tab Is Nothing OrElse grid Is Nothing Then
            Exit Sub
        End If

        Dim available As Integer = Math.Max(200, tab.ClientSize.Height - 16)
        Dim objetivo As Integer = CInt(Math.Round(available * ratio))
        objetivo = Math.Max(minimo, objetivo)
        objetivo = Math.Min(maximo, objetivo)
        grid.Height = objetivo
    End Sub

    Private Sub EnsureButtonsVisible(ByVal ParamArray buttons() As Button)
        If buttons Is Nothing Then
            Exit Sub
        End If

        For Each btn As Button In buttons
            If btn Is Nothing Then
                Continue For
            End If
            btn.Visible = True
            btn.BringToFront()
        Next
    End Sub

    Private Sub EnsureActionRowVisible(ByVal flow As FlowLayoutPanel)
        If flow Is Nothing Then
            Exit Sub
        End If
        flow.Dock = DockStyle.Fill
        flow.Height = 46
        flow.WrapContents = False
        flow.AutoScroll = True
        flow.Padding = New Padding(0, 4, 0, 2)
        flow.Margin = New Padding(0, 4, 0, 0)
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
        _btnCrearUsuario.Text = "Nuevo usuario"
        _btnActualizarUsuario.Text = "Editar usuario"
        _btnCambiarClave.Text = "Cambiar clave"
        _btnAsignarRolUsuario.Text = "Asignar rol"
        _btnRevocarRolUsuario.Text = "Revocar rol"
        _btnEliminarUsuario.Text = "Eliminar usuario"
        _btnSalir.Text = "Cerrar"

        _btnCrearRol.Text = "Nuevo rol"
        _btnActualizarRol.Text = "Editar rol"
        _btnEliminarRol.Text = "Eliminar rol"
        _btnAsignarPermisoRol.Text = "Asignar permiso"
        _btnRevocarPermisoRol.Text = "Revocar permiso"

        _btnCrearUsuario.Width = 126
        _btnActualizarUsuario.Width = 126
        _btnCambiarClave.Width = 132
        _btnAsignarRolUsuario.Width = 120
        _btnRevocarRolUsuario.Width = 122
        _btnEliminarUsuario.Width = 128

        _btnCrearRol.Width = 110
        _btnActualizarRol.Width = 110
        _btnEliminarRol.Width = 116
        _btnAsignarPermisoRol.Width = 132
        _btnRevocarPermisoRol.Width = 136
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
        CargarCombos()
        EnsureGridTopVisible(_gridUsuarios)
        EnsureGridTopVisible(_gridRoles)

        If _gridUsuarios.CurrentRow Is Nothing Then
            ClearUsuarioDetail()
        Else
            GridUsuarios_SelectionChanged(Nothing, EventArgs.Empty)
        End If

        If _gridRoles.CurrentRow Is Nothing Then
            ClearRolDetail()
        Else
            GridRoles_SelectionChanged(Nothing, EventArgs.Empty)
        End If
    End Sub

    Private Sub CargarUsuarios()
        _usuariosData = _service.ListarUsuarios()
        ApplyUserFilter()
        UpdateUsersSummary()
    End Sub

    Private Sub CargarRoles()
        _rolesData = _service.ListarRoles()
        ApplyRoleFilter()
        UpdateRolesSummary()
    End Sub

    Private Sub ApplyUserFilter()
        If _usuariosData Is Nothing Then
            Exit Sub
        End If
        Dim termino As String = GetSearchValue(_txtBuscarUsuarios)
        Dim filtroEstado As String = If(_cmbEstadoUsuarios Is Nothing OrElse _cmbEstadoUsuarios.SelectedItem Is Nothing, "Todos", _cmbEstadoUsuarios.SelectedItem.ToString())

        Dim filas As IEnumerable(Of DataRow) = _usuariosData.AsEnumerable().Where(
            Function(row)
                Dim nombreUsuario As String = row.Field(Of String)("NombreUsuario")
                Dim nombreCompleto As String = row.Field(Of String)("NombreCompleto")
                Dim roles As String = row.Field(Of String)("Roles")
                Dim coincideBusqueda As Boolean = termino.Length = 0 OrElse
                    ContieneTexto(nombreUsuario, termino) OrElse
                    ContieneTexto(nombreCompleto, termino) OrElse
                    ContieneTexto(roles, termino)

                If Not coincideBusqueda Then
                    Return False
                End If

                Dim esActivo As Boolean = False
                If Not row.IsNull("EsActivo") Then
                    esActivo = Convert.ToBoolean(row("EsActivo"))
                End If

                Dim bloqueadoHasta As DateTime? = Nothing
                If Not row.IsNull("BloqueadoHasta") Then
                    bloqueadoHasta = Convert.ToDateTime(row("BloqueadoHasta"))
                End If

                Select Case filtroEstado
                    Case "Activos"
                        Return esActivo
                    Case "Inactivos"
                        Return Not esActivo
                    Case "Bloqueados"
                        Return bloqueadoHasta.HasValue AndAlso bloqueadoHasta.Value > DateTime.UtcNow
                    Case Else
                        Return True
                End Select
            End Function)

        _usuariosFilteredData = CopyRowsOrSchema(_usuariosData, filas)
        NormalizePageIndex(_usuariosFilteredData, _usuariosPageIndex)
        BindUsuariosPage()
        UpdateFilterFeedback("usuarios", GetVisibleRowCount(_gridUsuarios), "No hay usuarios que coincidan con los filtros.", "Seleccione un usuario para editar o eliminar.")
    End Sub

    Private Sub ApplyRoleFilter()
        If _rolesData Is Nothing Then
            Exit Sub
        End If
        Dim termino As String = GetSearchValue(_txtBuscarRoles)
        Dim filtroEstado As String = If(_cmbEstadoRoles Is Nothing OrElse _cmbEstadoRoles.SelectedItem Is Nothing, "Todos", _cmbEstadoRoles.SelectedItem.ToString())

        Dim filas As IEnumerable(Of DataRow) = _rolesData.AsEnumerable().Where(
            Function(row)
                Dim nombreRol As String = row.Field(Of String)("NombreRol")
                Dim descripcion As String = row.Field(Of String)("Descripcion")
                Dim coincideBusqueda As Boolean = termino.Length = 0 OrElse
                    ContieneTexto(nombreRol, termino) OrElse
                    ContieneTexto(descripcion, termino)

                If Not coincideBusqueda Then
                    Return False
                End If

                Dim esActivo As Boolean = False
                If Not row.IsNull("EsActivo") Then
                    esActivo = Convert.ToBoolean(row("EsActivo"))
                End If

                Select Case filtroEstado
                    Case "Activos"
                        Return esActivo
                    Case "Inactivos"
                        Return Not esActivo
                    Case Else
                        Return True
                End Select
            End Function)

        _rolesFilteredData = CopyRowsOrSchema(_rolesData, filas)
        NormalizePageIndex(_rolesFilteredData, _rolesPageIndex)
        BindRolesPage()
        UpdateFilterFeedback("roles", GetVisibleRowCount(_gridRoles), "No hay roles que coincidan con los filtros.", "Seleccione un rol para editar o eliminar.")
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
            ClearUsuarioDetail()
            UpdateActionStates()
            Return
        End If

        If Not PanelUsuariosBottom.Visible Then
            SetFeedback("usuarios", "Usuario listo para editar o eliminar.", False)
            UpdateActionStates()
            Return
        End If

        _txtNombreUsuario.Text = ValorCelda(_gridUsuarios.CurrentRow, "NombreUsuario")
        _txtNombreCompletoUsuario.Text = ValorCelda(_gridUsuarios.CurrentRow, "NombreCompleto")
        _txtContrasenaUsuario.Text = "Gestionada por el boton Cambiar clave"
        _txtContrasenaUsuario.ReadOnly = True
        _chkUsuarioActivo.Checked = ValorCelda(_gridUsuarios.CurrentRow, "EsActivo") = "True"

        Dim idUsuario As Integer
        If Integer.TryParse(ValorCelda(_gridUsuarios.CurrentRow, "IdUsuario"), idUsuario) Then
            Dim roles As DataTable = _service.ListarRolesDeUsuario(idUsuario)
            _lstRolesUsuario.DataSource = roles
            _lstRolesUsuario.DisplayMember = "NombreRol"
            _lstRolesUsuario.ValueMember = "IdRol"
            SetFeedback("usuarios", "Usuario listo para editar o eliminar.", False)
        Else
            _lstRolesUsuario.DataSource = Nothing
        End If

        UpdateActionStates()
    End Sub

    Private Sub GridUsuarios_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles _gridUsuarios.CellDoubleClick
        If e.RowIndex >= 0 AndAlso _btnActualizarUsuario.Enabled Then
            BtnActualizarUsuario_Click(_btnActualizarUsuario, EventArgs.Empty)
        End If
    End Sub

    Private Sub GridRoles_SelectionChanged(sender As Object, e As EventArgs) Handles _gridRoles.SelectionChanged
        If _gridRoles.CurrentRow Is Nothing Then
            ClearRolDetail()
            UpdateActionStates()
            Return
        End If

        If Not PanelRolesBottom.Visible Then
            SetFeedback("roles", "Rol listo para editar o eliminar.", False)
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
            SetFeedback("roles", "Rol listo para editar o eliminar.", False)
        Else
            _lstPermisosRol.DataSource = Nothing
        End If
        UpdateActionStates()
    End Sub

    Private Sub TabsPrincipal_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabsPrincipal.SelectedIndexChanged
        AdjustCrudGridHeights()
        UpdateActionStates()
        RefreshStableFooterLayout()
    End Sub

    Private Sub CamposUsuarios_TextChanged(sender As Object, e As EventArgs) Handles _txtNombreUsuario.TextChanged, _txtNombreCompletoUsuario.TextChanged, _txtContrasenaUsuario.TextChanged
        UpdateActionStates()
    End Sub

    Private Sub CamposRoles_TextChanged(sender As Object, e As EventArgs) Handles _txtNombreRol.TextChanged, _txtDescripcionRol.TextChanged
        UpdateActionStates()
    End Sub

    Private Sub ListasSeleccionChanged(sender As Object, e As EventArgs) Handles _cmbRolUsuario.SelectedIndexChanged, _lstRolesUsuario.SelectedIndexChanged, _cmbPermisoRol.SelectedIndexChanged, _lstPermisosRol.SelectedIndexChanged
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
            If AbrirEditorUsuarioModal() Then
                CargarUsuarios()
                CargarCombos()
                UpdateActionStates()
                SetFeedback("usuarios", "Usuario creado correctamente.", False)
            End If
        Catch ex As Exception
            SetFeedback("usuarios", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnActualizarUsuario_Click(sender As Object, e As EventArgs) Handles _btnActualizarUsuario.Click
        Try
            If AbrirEditorUsuarioModal(ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")) Then
                CargarUsuarios()
                CargarCombos()
                UpdateActionStates()
                SetFeedback("usuarios", "Usuario actualizado correctamente.", False)
            End If
        Catch ex As Exception
            SetFeedback("usuarios", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnEliminarUsuario_Click(sender As Object, e As EventArgs) Handles _btnEliminarUsuario.Click
        Try
            Dim idUsuario As Integer = ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")
            If Not CrudOperationHelper.ConfirmarEliminacion("el usuario seleccionado") Then
                Exit Sub
            End If
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

    Private Sub BtnCambiarClave_Click(sender As Object, e As EventArgs) Handles _btnCambiarClave.Click
        Try
            Dim idUsuario As Integer = ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")
            Dim nombreUsuario As String = ValorCelda(_gridUsuarios.CurrentRow, "NombreUsuario")

            Using frm As New FrmCambiarContrasenaUsuarioModal(_service, idUsuario, nombreUsuario)
                If frm.ShowDialog(Me) = DialogResult.OK Then
                    SetFeedback("usuarios", "Contrasena actualizada correctamente.", False)
                End If
            End Using
        Catch ex As Exception
            SetFeedback("usuarios", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnAsignarRolUsuario_Click(sender As Object, e As EventArgs) Handles _btnAsignarRolUsuario.Click
        Try
            Dim idUsuario As Integer = ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")
            If _cmbRolUsuario.SelectedValue Is Nothing Then
                Throw New Exception("No hay rol seleccionado para asignar.")
            End If

            Dim idRol As Integer = Convert.ToInt32(_cmbRolUsuario.SelectedValue)
            _service.AsignarRolAUsuario(idUsuario, idRol)
            GridUsuarios_SelectionChanged(Nothing, EventArgs.Empty)
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
                Throw New Exception("Seleccione el rol que desea revocar del usuario.")
            End If
            If Not CrudOperationHelper.ConfirmarEliminacion("la asignacion de rol del usuario") Then
                Exit Sub
            End If

            Dim idRol As Integer = Convert.ToInt32(_lstRolesUsuario.SelectedValue)
            _service.RevocarRolAUsuario(idUsuario, idRol)
            GridUsuarios_SelectionChanged(Nothing, EventArgs.Empty)
            UpdateActionStates()
            SetFeedback("usuarios", "Rol revocado del usuario.", False)
        Catch ex As Exception
            SetFeedback("usuarios", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnCrearRol_Click(sender As Object, e As EventArgs) Handles _btnCrearRol.Click
        Try
            If AbrirEditorRolModal() Then
                CargarRoles()
                CargarCombos()
                UpdateActionStates()
                SetFeedback("roles", "Rol creado correctamente.", False)
            End If
        Catch ex As Exception
            SetFeedback("roles", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnActualizarRol_Click(sender As Object, e As EventArgs) Handles _btnActualizarRol.Click
        Try
            If AbrirEditorRolModal(ObtenerIdSeleccionado(_gridRoles, "IdRol")) Then
                CargarRoles()
                CargarCombos()
                UpdateActionStates()
                SetFeedback("roles", "Rol actualizado correctamente.", False)
            End If
        Catch ex As Exception
            SetFeedback("roles", ex.Message, True)
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnEliminarRol_Click(sender As Object, e As EventArgs) Handles _btnEliminarRol.Click
        Try
            Dim idRol As Integer = ObtenerIdSeleccionado(_gridRoles, "IdRol")
            If Not CrudOperationHelper.ConfirmarEliminacion("el rol seleccionado") Then
                Exit Sub
            End If
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
            If Not CrudOperationHelper.ConfirmarEliminacion("la asignacion de permiso del rol") Then
                Exit Sub
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

    Private Sub BtnSalir_Click(sender As Object, e As EventArgs) Handles _btnSalir.Click
        Me.Close()
    End Sub

    Private Sub BtnNuevoRegistro_Click(sender As Object, e As EventArgs)
        If TabsPrincipal.SelectedTab Is TabUsuarios Then
            If _btnCrearUsuario.Enabled Then
                BtnCrearUsuario_Click(_btnCrearUsuario, EventArgs.Empty)
            End If
        ElseIf TabsPrincipal.SelectedTab Is TabRoles Then
            If _btnCrearRol.Enabled Then
                BtnCrearRol_Click(_btnCrearRol, EventArgs.Empty)
            End If
        End If
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
                If _btnActualizarUsuario.Enabled Then
                    BtnActualizarUsuario_Click(_btnActualizarUsuario, EventArgs.Empty)
                ElseIf _btnCrearUsuario.Enabled Then
                    BtnCrearUsuario_Click(_btnCrearUsuario, EventArgs.Empty)
                End If
            Case TabRoles.Name
                If _btnActualizarRol.Enabled Then
                    BtnActualizarRol_Click(_btnActualizarRol, EventArgs.Empty)
                ElseIf _btnCrearRol.Enabled Then
                    BtnCrearRol_Click(_btnCrearRol, EventArgs.Empty)
                End If
        End Select
    End Sub

    Private Sub UpdateActionStates()
        If IsInDesignMode() Then
            Return
        End If

        Dim usuarioSeleccionado As Boolean = Not _gridUsuarios.CurrentRow Is Nothing
        Dim rolSeleccionado As Boolean = Not _gridRoles.CurrentRow Is Nothing
        Dim canGestionarRolesUsuario As Boolean = TienePermisoSeguridad("Usuarios.Editar")

        Dim canCrearUsuario As Boolean = TienePermisoSeguridad("Usuarios.Crear")
        Dim canEditarUsuario As Boolean = TienePermisoSeguridad("Usuarios.Editar")
        Dim canCambiarClave As Boolean = TienePermisoSeguridad("Usuarios.CambiarClave", "Usuarios.Editar")
        Dim canEliminarUsuario As Boolean = TienePermisoSeguridad("Usuarios.Eliminar")

        Dim canCrearRol As Boolean = TienePermisoSeguridad("Roles.Crear")
        Dim canEditarRol As Boolean = TienePermisoSeguridad("Roles.Editar")
        Dim canEliminarRol As Boolean = TienePermisoSeguridad("Roles.Eliminar")
        Dim canGestionarPermisosRol As Boolean = TienePermisoSeguridad("Roles.Permisos.Gestionar")

        _btnCrearUsuario.Enabled = canCrearUsuario
        _btnActualizarUsuario.Enabled = canEditarUsuario AndAlso usuarioSeleccionado
        _btnCambiarClave.Enabled = canCambiarClave AndAlso usuarioSeleccionado
        _btnAsignarRolUsuario.Enabled = canGestionarRolesUsuario AndAlso usuarioSeleccionado AndAlso Not _cmbRolUsuario.SelectedValue Is Nothing
        _btnRevocarRolUsuario.Enabled = canGestionarRolesUsuario AndAlso usuarioSeleccionado AndAlso Not _lstRolesUsuario.SelectedValue Is Nothing
        _btnEliminarUsuario.Enabled = canEliminarUsuario AndAlso usuarioSeleccionado

        _btnCrearRol.Enabled = canCrearRol
        _btnActualizarRol.Enabled = canEditarRol AndAlso rolSeleccionado
        _btnEliminarRol.Enabled = canEliminarRol AndAlso rolSeleccionado
        _btnAsignarPermisoRol.Enabled = canGestionarPermisosRol AndAlso rolSeleccionado AndAlso Not _cmbPermisoRol.SelectedValue Is Nothing
        _btnRevocarPermisoRol.Enabled = canGestionarPermisosRol AndAlso rolSeleccionado AndAlso Not _lstPermisosRol.SelectedValue Is Nothing
        _cmbRolUsuario.Enabled = canGestionarRolesUsuario AndAlso usuarioSeleccionado AndAlso _cmbRolUsuario.Items.Count > 0
        _lstRolesUsuario.Enabled = usuarioSeleccionado
        _cmbPermisoRol.Enabled = canGestionarPermisosRol AndAlso rolSeleccionado AndAlso _cmbPermisoRol.Items.Count > 0
        _lstPermisosRol.Enabled = rolSeleccionado
        SyncFooterPrimaryButtonState()
    End Sub

    Private Sub SetFeedback(ByVal scope As String, ByVal text As String, ByVal isError As Boolean)
        Dim lbl As Label = Nothing
        Select Case scope
            Case "usuarios"
                lbl = _lblFeedbackUsuarios
            Case "roles"
                lbl = _lblFeedbackRoles
        End Select

        If lbl Is Nothing Then
            _footerStatusText = text
            _footerStatusIsError = isError
            SyncFooterHintPresentation()
            Exit Sub
        End If

        lbl.ForeColor = If(isError, Color.FromArgb(166, 47, 63), Color.FromArgb(47, 104, 54))
        lbl.Text = text
        _footerStatusText = text
        _footerStatusIsError = isError
        SyncFooterHintPresentation()
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
            If String.Equals(col.Name, GridActionEditColumnName, StringComparison.Ordinal) OrElse
               String.Equals(col.Name, GridActionDeleteColumnName, StringComparison.Ordinal) Then
                col.SortMode = DataGridViewColumnSortMode.NotSortable
                Continue For
            End If
            If col.Name.StartsWith("Id", StringComparison.OrdinalIgnoreCase) Then
                col.Visible = False
                Continue For
            End If

            If headers.ContainsKey(col.Name) Then
                col.HeaderText = headers(col.Name)
            End If

            col.SortMode = DataGridViewColumnSortMode.Automatic

            If String.Equals(col.Name, "NombreUsuario", StringComparison.OrdinalIgnoreCase) Then
                col.FillWeight = 95
                col.MinimumWidth = 110
            ElseIf String.Equals(col.Name, "NombreCompleto", StringComparison.OrdinalIgnoreCase) Then
                col.FillWeight = 155
                col.MinimumWidth = 180
            ElseIf String.Equals(col.Name, "EsActivo", StringComparison.OrdinalIgnoreCase) Then
                col.FillWeight = 60
                col.MinimumWidth = 72
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            ElseIf String.Equals(col.Name, "IntentosFallidos", StringComparison.OrdinalIgnoreCase) Then
                col.FillWeight = 92
                col.MinimumWidth = 130
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            ElseIf String.Equals(col.Name, "BloqueadoHasta", StringComparison.OrdinalIgnoreCase) Then
                col.FillWeight = 108
                col.MinimumWidth = 120
            ElseIf String.Equals(col.Name, "Descripcion", StringComparison.OrdinalIgnoreCase) Then
                col.FillWeight = 170
            ElseIf String.Equals(col.Name, "FechaCreacion", StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(col.Name, "FechaUltimoIngreso", StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(col.Name, "FechaUltimoIn", StringComparison.OrdinalIgnoreCase) Then
                col.FillWeight = 115
                col.MinimumWidth = 132
            ElseIf String.Equals(col.Name, "Roles", StringComparison.OrdinalIgnoreCase) Then
                col.FillWeight = 125
                col.MinimumWidth = 150
            End If
        Next

        EnsureGridActionColumns(
            grid,
            If(grid Is _gridUsuarios, TienePermisoSeguridad("Usuarios.Editar"), TienePermisoSeguridad("Roles.Editar")),
            If(grid Is _gridUsuarios, TienePermisoSeguridad("Usuarios.Eliminar"), TienePermisoSeguridad("Roles.Eliminar")))

        If grid.Rows.Count > 0 Then
            Try
                grid.FirstDisplayedScrollingRowIndex = 0
                If grid.CurrentCell Is Nothing AndAlso grid.Columns.Count > 0 Then
                    grid.CurrentCell = grid.Rows(0).Cells(grid.Columns.Cast(Of DataGridViewColumn)().First(Function(c) c.Visible).Index)
                End If
            Catch
            End Try
        Else
            grid.ClearSelection()
        End If
    End Sub

    Private Sub EnsureGridActionColumns(ByVal grid As DataGridView, ByVal canEdit As Boolean, ByVal canDelete As Boolean)
        If grid Is Nothing Then
            Exit Sub
        End If

        Dim editColumn As DataGridViewButtonColumn = TryCast(grid.Columns(GridActionEditColumnName), DataGridViewButtonColumn)
        If editColumn Is Nothing Then
            editColumn = New DataGridViewButtonColumn()
            editColumn.Name = GridActionEditColumnName
        editColumn.HeaderText = "Editar"
        editColumn.Text = "✎ Editar"
        editColumn.UseColumnTextForButtonValue = True
        editColumn.FlatStyle = FlatStyle.Standard
        editColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        editColumn.Width = 94
        editColumn.ReadOnly = True
        editColumn.SortMode = DataGridViewColumnSortMode.NotSortable
        editColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        grid.Columns.Add(editColumn)
    End If

        editColumn.HeaderText = "Editar"
        editColumn.Text = "✎ Editar"
        editColumn.UseColumnTextForButtonValue = True
        editColumn.FlatStyle = FlatStyle.Standard
        editColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        editColumn.Width = 94
        editColumn.ReadOnly = True
        editColumn.SortMode = DataGridViewColumnSortMode.NotSortable
        editColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        editColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter

        Dim deleteColumn As DataGridViewButtonColumn = TryCast(grid.Columns(GridActionDeleteColumnName), DataGridViewButtonColumn)
        If deleteColumn Is Nothing Then
            deleteColumn = New DataGridViewButtonColumn()
            deleteColumn.Name = GridActionDeleteColumnName
            grid.Columns.Add(deleteColumn)
        End If

        deleteColumn.HeaderText = "Borrar"
        deleteColumn.Text = "✖ Borrar"
        deleteColumn.UseColumnTextForButtonValue = True
        deleteColumn.FlatStyle = FlatStyle.Standard
        deleteColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        deleteColumn.Width = 92
        deleteColumn.ReadOnly = True
        deleteColumn.SortMode = DataGridViewColumnSortMode.NotSortable
        deleteColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        deleteColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter

        editColumn.Visible = canEdit
        deleteColumn.Visible = canDelete
        If grid.Columns.Count >= 2 Then
            editColumn.DisplayIndex = grid.Columns.Count - 2
            deleteColumn.DisplayIndex = grid.Columns.Count - 1
        End If
    End Sub

    Private Sub Grid_CellPainting(sender As Object, e As DataGridViewCellPaintingEventArgs)
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then
            Exit Sub
        End If

        Dim grid As DataGridView = DirectCast(sender, DataGridView)
        Dim columnName As String = grid.Columns(e.ColumnIndex).Name
        If Not String.Equals(columnName, GridActionEditColumnName, StringComparison.Ordinal) AndAlso
           Not String.Equals(columnName, GridActionDeleteColumnName, StringComparison.Ordinal) Then
            Exit Sub
        End If

        e.Handled = True
        e.PaintBackground(e.CellBounds, True)
        e.Paint(e.CellBounds, DataGridViewPaintParts.Border)

        Dim isEdit As Boolean = String.Equals(columnName, GridActionEditColumnName, StringComparison.Ordinal)
        Dim fillColor As Color = If(isEdit, Color.FromArgb(70, 97, 132), Color.FromArgb(157, 89, 89))
        Dim borderColor As Color = If(isEdit, Color.FromArgb(56, 78, 107), Color.FromArgb(130, 71, 71))
        Dim buttonText As String = If(isEdit, "✎ Editar", "✖ Borrar")
        Dim textColor As Color = Color.White

        If grid.Rows(e.RowIndex).Selected Then
            fillColor = ControlPaint.Dark(fillColor, 0.1F)
            borderColor = ControlPaint.Dark(borderColor, 0.15F)
        End If

        Dim buttonBounds As New Rectangle(
            e.CellBounds.X + 8,
            e.CellBounds.Y + 5,
            Math.Max(20, e.CellBounds.Width - 16),
            Math.Max(18, e.CellBounds.Height - 10))

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Using path As GraphicsPath = BuildRoundedRectanglePath(buttonBounds, 7)
            Using brush As New SolidBrush(fillColor)
                e.Graphics.FillPath(brush, path)
            End Using
            Using pen As New Pen(borderColor)
                e.Graphics.DrawPath(pen, path)
            End Using
        End Using

        Using textFont As New Font("Segoe UI Semibold", 8.5!, FontStyle.Bold)
            TextRenderer.DrawText(
                e.Graphics,
                buttonText,
                textFont,
                buttonBounds,
                textColor,
                TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
        End Using
    End Sub

    Private Function BuildRoundedRectanglePath(ByVal bounds As Rectangle, ByVal radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim diameter As Integer = Math.Max(2, radius * 2)

        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90)
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90)
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90)
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90)
        path.CloseFigure()

        Return path
    End Function

    Private Sub CrudGrid_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles _gridUsuarios.CellContentClick, _gridRoles.CellContentClick
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then
            Exit Sub
        End If

        Dim grid As DataGridView = DirectCast(sender, DataGridView)
        Dim columnName As String = grid.Columns(e.ColumnIndex).Name
        If Not String.Equals(columnName, GridActionEditColumnName, StringComparison.Ordinal) AndAlso
           Not String.Equals(columnName, GridActionDeleteColumnName, StringComparison.Ordinal) Then
            Exit Sub
        End If

        grid.ClearSelection()
        grid.Rows(e.RowIndex).Selected = True
        grid.CurrentCell = grid.Rows(e.RowIndex).Cells(e.ColumnIndex)

        If grid Is _gridUsuarios Then
            If String.Equals(columnName, GridActionEditColumnName, StringComparison.Ordinal) Then
                If _btnActualizarUsuario.Enabled Then
                    BtnActualizarUsuario_Click(_btnActualizarUsuario, EventArgs.Empty)
                End If
            ElseIf _btnEliminarUsuario.Enabled Then
                BtnEliminarUsuario_Click(_btnEliminarUsuario, EventArgs.Empty)
            End If
            Exit Sub
        End If

        If grid Is _gridRoles Then
            If String.Equals(columnName, GridActionEditColumnName, StringComparison.Ordinal) Then
                If _btnActualizarRol.Enabled Then
                    BtnActualizarRol_Click(_btnActualizarRol, EventArgs.Empty)
                End If
            ElseIf _btnEliminarRol.Enabled Then
                BtnEliminarRol_Click(_btnEliminarRol, EventArgs.Empty)
            End If
        End If
    End Sub

    Private Sub ConfigureFieldTooltips()
        If _tooltips Is Nothing Then
            Exit Sub
        End If
        _tooltips.SetToolTip(_txtNombreRol, "Nombre único del rol.")
        _tooltips.SetToolTip(_txtDescripcionRol, "Descripción funcional del rol.")
        _tooltips.SetToolTip(_txtClavePermiso, "Clave técnica del permiso (única).")
        _tooltips.SetToolTip(_txtDescripcionPermiso, "Descripción funcional del permiso.")
        If Not _txtBuscarUsuarios Is Nothing Then
            _tooltips.SetToolTip(_txtBuscarUsuarios, "Buscar por usuario, nombre completo o rol.")
        End If
        If Not _txtBuscarRoles Is Nothing Then
            _tooltips.SetToolTip(_txtBuscarRoles, "Buscar por nombre o descripción del rol.")
        End If
    End Sub

    Private Sub ConfigureUsersListOnlyLayout()
        RestoreUsersTabDesignerLayout()
    End Sub

    Private Sub ConfigureRolesWorkspace()
        RestoreRolesTabDesignerLayout()
    End Sub

    Private Sub ConfigureFooterBar()
        RefreshStableFooterLayout()
    End Sub

    Private Sub EnsureFloatingCloseHost()
        ' Footer flotante deshabilitado: se usa siempre el footer estable del diseñador.
    End Sub

    Private Sub LayoutFloatingCloseButton()
        RefreshStableFooterLayout()
    End Sub

    Private Function AbrirEditorUsuarioModal(Optional ByVal idUsuario As Integer? = Nothing) As Boolean
        Using frm As New FrmUsuarioRbacModal(_service, idUsuario)
            Return frm.ShowDialog(Me) = DialogResult.OK
        End Using
    End Function

    Private Function AbrirEditorRolModal(Optional ByVal idRol As Integer? = Nothing) As Boolean
        Using frm As New FrmRolRbacModal(_service, idRol)
            Return frm.ShowDialog(Me) = DialogResult.OK
        End Using
    End Function

    Private Function CopyRowsOrSchema(ByVal source As DataTable, ByVal rows As IEnumerable(Of DataRow)) As DataTable
        Dim result As DataTable = source.Clone()
        For Each row As DataRow In rows
            result.ImportRow(row)
        Next
        Return result
    End Function

    Private Sub BindUsuariosPage()
        If _usuariosFilteredData Is Nothing Then
            _gridUsuarios.DataSource = Nothing
            RefreshFooterPagerState()
            Exit Sub
        End If

        NormalizePageIndex(_usuariosFilteredData, _usuariosPageIndex)
        _gridUsuarios.DataSource = BuildPageTable(_usuariosFilteredData, _usuariosPageIndex)
        ApplyPagedGridHeight(_gridUsuarios)
        RefreshFooterPagerState()
    End Sub

    Private Sub BindRolesPage()
        If _rolesFilteredData Is Nothing Then
            _gridRoles.DataSource = Nothing
            RefreshFooterPagerState()
            Exit Sub
        End If

        NormalizePageIndex(_rolesFilteredData, _rolesPageIndex)
        _gridRoles.DataSource = BuildPageTable(_rolesFilteredData, _rolesPageIndex)
        ApplyPagedGridHeight(_gridRoles)
        RefreshFooterPagerState()
    End Sub

    Private Function BuildPageTable(ByVal source As DataTable, ByVal pageIndex As Integer) As DataTable
        If source Is Nothing Then
            Return Nothing
        End If

        Dim result As DataTable = source.Clone()
        For Each row As DataRow In source.AsEnumerable().Skip(pageIndex * GridPageSize).Take(GridPageSize)
            result.ImportRow(row)
        Next
        Return result
    End Function

    Private Sub NormalizePageIndex(ByVal source As DataTable, ByRef pageIndex As Integer)
        Dim totalPages As Integer = GetTotalPages(If(source Is Nothing, 0, source.Rows.Count))
        pageIndex = Math.Max(0, Math.Min(pageIndex, totalPages - 1))
    End Sub

    Private Function GetTotalPages(ByVal totalRows As Integer) As Integer
        Return Math.Max(1, CInt(Math.Ceiling(totalRows / CDbl(GridPageSize))))
    End Function

    Private Function ContieneTexto(ByVal value As String, ByVal termino As String) As Boolean
        If String.IsNullOrWhiteSpace(termino) Then
            Return True
        End If
        If String.IsNullOrWhiteSpace(value) Then
            Return False
        End If
        Return value.IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    Private Sub EnsureUsersHeaderPanel()
        If Not _usuariosHeader Is Nothing Then
            Exit Sub
        End If

        _txtBuscarUsuarios = BuildSearchTextBox("Buscar usuario, nombre o rol")
        _cmbEstadoUsuarios = BuildFilterComboBox()
        _cmbEstadoUsuarios.Items.AddRange(New Object() {"Todos", "Activos", "Inactivos", "Bloqueados"})
        _cmbEstadoUsuarios.SelectedIndex = 0
        _lblResumenUsuarios = BuildSummaryLabel("0 usuarios")

        AddHandler _txtBuscarUsuarios.TextChanged, AddressOf UsuariosFiltroChanged
        AddHandler _cmbEstadoUsuarios.SelectedIndexChanged, AddressOf UsuariosFiltroChanged

        _usuariosHeader = BuildHeaderPanel(
            "Usuarios y accesos",
            "Administra cuentas, estado operativo y trazabilidad de ingreso desde una vista mas clara.",
            _txtBuscarUsuarios,
            _cmbEstadoUsuarios,
            _lblResumenUsuarios)
    End Sub

    Private Sub EnsureRolesHeaderPanel()
        If Not _rolesHeader Is Nothing Then
            Exit Sub
        End If

        _txtBuscarRoles = BuildSearchTextBox("Buscar rol o descripcion")
        _cmbEstadoRoles = BuildFilterComboBox()
        _cmbEstadoRoles.Items.AddRange(New Object() {"Todos", "Activos", "Inactivos"})
        _cmbEstadoRoles.SelectedIndex = 0
        _lblResumenRoles = BuildSummaryLabel("0 roles")

        AddHandler _txtBuscarRoles.TextChanged, AddressOf RolesFiltroChanged
        AddHandler _cmbEstadoRoles.SelectedIndexChanged, AddressOf RolesFiltroChanged

        _rolesHeader = BuildHeaderPanel(
            "Roles y permisos",
            "Selecciona un rol para revisar su descripcion y administrar los permisos asignados.",
            _txtBuscarRoles,
            _cmbEstadoRoles,
            _lblResumenRoles)
    End Sub

    Private Function BuildHeaderPanel(ByVal title As String, ByVal subtitle As String, ByVal searchBox As TextBox, ByVal filterCombo As ComboBox, ByVal summaryLabel As Label) As Panel
        Dim panel As New Panel()
        panel.Dock = DockStyle.Fill
        panel.Margin = New Padding(0, 0, 0, 10)
        panel.Padding = New Padding(18, 12, 18, 10)
        panel.BackColor = Color.White

        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.Margin = New Padding(0)
        layout.Padding = New Padding(0)
        layout.ColumnCount = 2
        layout.RowCount = 2
        layout.BackColor = Color.White
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 430.0!))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0!))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))

        Dim lblTitle As New Label()
        lblTitle.Text = title
        lblTitle.Dock = DockStyle.Fill
        lblTitle.Font = New Font("Segoe UI Semibold", 13.0!, FontStyle.Bold)
        lblTitle.ForeColor = Color.FromArgb(27, 42, 65)
        lblTitle.TextAlign = ContentAlignment.MiddleLeft

        Dim lblSubtitle As New Label()
        lblSubtitle.Text = subtitle
        lblSubtitle.Dock = DockStyle.Fill
        lblSubtitle.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular)
        lblSubtitle.ForeColor = Color.FromArgb(92, 103, 122)
        lblSubtitle.TextAlign = ContentAlignment.TopLeft

        Dim actionsLayout As New TableLayoutPanel()
        actionsLayout.Dock = DockStyle.Fill
        actionsLayout.Margin = New Padding(0)
        actionsLayout.Padding = New Padding(0)
        actionsLayout.ColumnCount = 1
        actionsLayout.RowCount = 3
        actionsLayout.BackColor = Color.White
        actionsLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 18.0!))
        actionsLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 32.0!))
        actionsLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 20.0!))

        Dim lblResumenTitulo As New Label()
        lblResumenTitulo.Text = "Filtros"
        lblResumenTitulo.Dock = DockStyle.Fill
        lblResumenTitulo.Font = New Font("Segoe UI Semibold", 9.0!, FontStyle.Bold)
        lblResumenTitulo.ForeColor = Color.FromArgb(76, 90, 112)
        lblResumenTitulo.TextAlign = ContentAlignment.MiddleLeft

        Dim filtersRow As New TableLayoutPanel()
        filtersRow.Dock = DockStyle.Fill
        filtersRow.Margin = New Padding(0)
        filtersRow.Padding = New Padding(0)
        filtersRow.ColumnCount = 3
        filtersRow.RowCount = 1
        filtersRow.BackColor = Color.White
        filtersRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
        filtersRow.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 108.0!))
        filtersRow.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 10.0!))

        filterCombo.Width = 108
        filterCombo.Margin = New Padding(10, 0, 0, 0)
        summaryLabel.Margin = New Padding(0)
        summaryLabel.Dock = DockStyle.Right
        summaryLabel.AutoSize = False
        summaryLabel.Height = 22
        summaryLabel.Width = 240
        summaryLabel.TextAlign = ContentAlignment.MiddleRight
        summaryLabel.BackColor = Color.Transparent
        summaryLabel.ForeColor = Color.FromArgb(63, 81, 108)
        summaryLabel.Font = New Font("Segoe UI", 8.75!, FontStyle.Regular)
        summaryLabel.Padding = New Padding(0)

        filtersRow.Controls.Add(searchBox, 0, 0)
        filtersRow.Controls.Add(filterCombo, 1, 0)
        filtersRow.Controls.Add(New Panel(), 2, 0)

        actionsLayout.Controls.Add(lblResumenTitulo, 0, 0)
        actionsLayout.Controls.Add(filtersRow, 0, 1)
        actionsLayout.Controls.Add(summaryLabel, 0, 2)

        layout.Controls.Add(lblTitle, 0, 0)
        layout.Controls.Add(lblSubtitle, 0, 1)
        layout.Controls.Add(actionsLayout, 1, 0)
        layout.SetRowSpan(actionsLayout, 2)

        panel.Controls.Add(layout)
        Return panel
    End Function

    Private Function BuildSearchTextBox(ByVal placeholder As String) As TextBox
        Dim tb As New TextBox()
        tb.Dock = DockStyle.Fill
        tb.BorderStyle = BorderStyle.FixedSingle
        tb.BackColor = Color.White
        tb.ForeColor = Color.FromArgb(29, 42, 61)
        tb.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular)
        tb.Margin = New Padding(0)
        tb.Tag = placeholder
        AddHandler tb.Enter, AddressOf SearchBox_Enter
        AddHandler tb.Leave, AddressOf SearchBox_Leave
        Return tb
    End Function

    Private Function BuildFilterComboBox() As ComboBox
        Dim cb As New ComboBox()
        cb.DropDownStyle = ComboBoxStyle.DropDownList
        cb.FlatStyle = FlatStyle.Flat
        cb.Dock = DockStyle.Fill
        cb.BackColor = Color.White
        cb.ForeColor = Color.FromArgb(29, 42, 61)
        cb.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular)
        cb.Margin = New Padding(0)
        Return cb
    End Function

    Private Function BuildSummaryLabel(ByVal text As String) As Label
        Dim lbl As New Label()
        lbl.Text = text
        lbl.Dock = DockStyle.Fill
        lbl.TextAlign = ContentAlignment.MiddleCenter
        lbl.BackColor = Color.Transparent
        lbl.ForeColor = Color.FromArgb(40, 59, 86)
        lbl.Font = New Font("Segoe UI Semibold", 8.5!, FontStyle.Bold)
        lbl.Padding = New Padding(4, 0, 4, 0)
        lbl.AutoEllipsis = True
        Return lbl
    End Function

    Private Function BuildTopActionsPanel() As Panel
        Dim panel As New Panel()
        panel.Dock = DockStyle.Fill
        panel.Margin = New Padding(0, 0, 0, 10)
        panel.Padding = New Padding(12, 6, 12, 6)
        panel.BackColor = Color.White
        Return panel
    End Function

    Private Sub UsuariosFiltroChanged(sender As Object, e As EventArgs)
        ApplyUserFilter()
        UpdateUsersSummary()
    End Sub

    Private Sub RolesFiltroChanged(sender As Object, e As EventArgs)
        ApplyRoleFilter()
        UpdateRolesSummary()
    End Sub

    Private Sub UpdateUsersSummary()
        If _lblResumenUsuarios Is Nothing OrElse _usuariosData Is Nothing Then
            Exit Sub
        End If

        Dim total As Integer = _usuariosData.Rows.Count
        Dim visibles As Integer = GetVisibleRowCount(_gridUsuarios)
        Dim activos As Integer = _usuariosData.AsEnumerable().Count(Function(row) Not row.IsNull("EsActivo") AndAlso Convert.ToBoolean(row("EsActivo")))
        Dim bloqueados As Integer = _usuariosData.AsEnumerable().Count(
            Function(row) Not row.IsNull("BloqueadoHasta") AndAlso Convert.ToDateTime(row("BloqueadoHasta")) > DateTime.UtcNow)

        _lblResumenUsuarios.Text = String.Format("{0}/{1} visibles | {2} activos | {3} bloqueados", visibles, total, activos, bloqueados)
    End Sub

    Private Sub UpdateRolesSummary()
        If _lblResumenRoles Is Nothing OrElse _rolesData Is Nothing Then
            Exit Sub
        End If

        Dim total As Integer = _rolesData.Rows.Count
        Dim visibles As Integer = GetVisibleRowCount(_gridRoles)
        Dim activos As Integer = _rolesData.AsEnumerable().Count(Function(row) Not row.IsNull("EsActivo") AndAlso Convert.ToBoolean(row("EsActivo")))
        _lblResumenRoles.Text = String.Format("{0}/{1} visibles | {2} activos", visibles, total, activos)
    End Sub

    Private Function GetVisibleRowCount(ByVal grid As DataGridView) As Integer
        If grid Is Nothing OrElse grid.DataSource Is Nothing Then
            Return 0
        End If

        Dim table As DataTable = TryCast(grid.DataSource, DataTable)
        If Not table Is Nothing Then
            Return table.Rows.Count
        End If

        Dim view As DataView = TryCast(grid.DataSource, DataView)
        If Not view Is Nothing Then
            Return view.Count
        End If

        Return grid.Rows.Count
    End Function

    Private Function GetSearchValue(ByVal tb As TextBox) As String
        If tb Is Nothing Then
            Return String.Empty
        End If

        Dim placeholder As String = TryCast(tb.Tag, String)
        If Not String.IsNullOrWhiteSpace(placeholder) AndAlso String.Equals(tb.Text, placeholder, StringComparison.Ordinal) Then
            Return String.Empty
        End If

        Return tb.Text.Trim()
    End Function

    Private Sub ApplySearchPlaceholder(ByVal tb As TextBox)
        If tb Is Nothing Then
            Exit Sub
        End If

        Dim placeholder As String = TryCast(tb.Tag, String)
        If String.IsNullOrWhiteSpace(placeholder) Then
            Exit Sub
        End If

        If String.IsNullOrWhiteSpace(tb.Text) Then
            tb.Text = placeholder
            tb.ForeColor = Color.FromArgb(138, 149, 166)
        End If
    End Sub

    Private Sub SearchBox_Enter(sender As Object, e As EventArgs)
        Dim tb As TextBox = TryCast(sender, TextBox)
        If tb Is Nothing Then
            Exit Sub
        End If

        Dim placeholder As String = TryCast(tb.Tag, String)
        If Not String.IsNullOrWhiteSpace(placeholder) AndAlso String.Equals(tb.Text, placeholder, StringComparison.Ordinal) Then
            tb.Text = String.Empty
            tb.ForeColor = Color.FromArgb(29, 42, 61)
        End If
    End Sub

    Private Sub SearchBox_Leave(sender As Object, e As EventArgs)
        ApplySearchPlaceholder(TryCast(sender, TextBox))
    End Sub

    Private Sub UpdateFilterFeedback(ByVal scope As String, ByVal visibleCount As Integer, ByVal emptyMessage As String, ByVal readyMessage As String)
        If visibleCount <= 0 Then
            SetFeedback(scope, emptyMessage, False)
        Else
            SetFeedback(scope, readyMessage, False)
        End If
    End Sub

    Private Sub SetInitialFocus(sender As Object, e As EventArgs)
        If TabsPrincipal.SelectedTab Is TabUsuarios AndAlso Not _txtBuscarUsuarios Is Nothing Then
            _txtBuscarUsuarios.Select()
            _txtBuscarUsuarios.SelectionStart = 0
            _txtBuscarUsuarios.SelectionLength = 0
        End If
    End Sub

    Private Sub Grid_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs)
        Dim grid As DataGridView = DirectCast(sender, DataGridView)
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then
            Exit Sub
        End If

        Dim colName As String = grid.Columns(e.ColumnIndex).Name
        If String.Equals(colName, "EsActivo", StringComparison.OrdinalIgnoreCase) Then
            Exit Sub
        End If

        If String.Equals(colName, "BloqueadoHasta", StringComparison.OrdinalIgnoreCase) Then
            If e.Value Is Nothing OrElse e.Value Is DBNull.Value OrElse String.IsNullOrWhiteSpace(e.Value.ToString()) Then
                e.Value = "Disponible"
            Else
                Dim fecha As DateTime = Convert.ToDateTime(e.Value)
                If fecha <= DateTime.UtcNow Then
                    e.Value = "Disponible"
                Else
                    e.Value = fecha.ToLocalTime().ToString("yyyy/MM/dd HH:mm")
                End If
            End If
            e.FormattingApplied = True
            Exit Sub
        End If

        If String.Equals(colName, "FechaCreacion", StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(colName, "FechaUltimoIngreso", StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(colName, "FechaUltimoIn", StringComparison.OrdinalIgnoreCase) Then
            If e.Value Is Nothing OrElse e.Value Is DBNull.Value OrElse String.IsNullOrWhiteSpace(e.Value.ToString()) Then
                e.Value = "Sin registro"
            Else
                e.Value = Convert.ToDateTime(e.Value).ToLocalTime().ToString("yyyy/MM/dd HH:mm")
            End If
            e.FormattingApplied = True
        End If
    End Sub

    Private Sub Grid_RowPrePaint(sender As Object, e As DataGridViewRowPrePaintEventArgs)
        Dim grid As DataGridView = DirectCast(sender, DataGridView)
        If e.RowIndex < 0 OrElse e.RowIndex >= grid.Rows.Count Then
            Exit Sub
        End If

        Dim row As DataGridViewRow = grid.Rows(e.RowIndex)
        row.DefaultCellStyle.ForeColor = Color.FromArgb(36, 51, 77)
        row.DefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 33, 59)
        row.DefaultCellStyle.BackColor = If((e.RowIndex Mod 2) = 0, Color.White, Color.FromArgb(248, 250, 254))
        row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(228, 236, 246)

        If grid.Columns.Contains("EsActivo") Then
            Dim valorActivo As Object = row.Cells("EsActivo").Value
            If valorActivo IsNot Nothing AndAlso valorActivo IsNot DBNull.Value AndAlso Not Convert.ToBoolean(valorActivo) Then
                row.DefaultCellStyle.ForeColor = Color.FromArgb(131, 142, 158)
            End If
        End If

        If grid.Columns.Contains("BloqueadoHasta") Then
            Dim valorBloqueo As Object = row.Cells("BloqueadoHasta").Value
            If valorBloqueo IsNot Nothing AndAlso valorBloqueo IsNot DBNull.Value Then
                Dim fechaBloqueo As DateTime = Convert.ToDateTime(valorBloqueo)
                If fechaBloqueo > DateTime.UtcNow Then
                    row.DefaultCellStyle.BackColor = Color.FromArgb(250, 243, 243)
                    row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 223, 223)
                End If
            End If
        End If
    End Sub

    Private Sub Grid_DataError(sender As Object, e As DataGridViewDataErrorEventArgs)
        e.ThrowException = False
        e.Cancel = False

        Dim grid As DataGridView = TryCast(sender, DataGridView)
        If grid Is Nothing Then
            Exit Sub
        End If

        If grid Is _gridUsuarios Then
            SetFeedback("usuarios", "Se detecto un problema de formato en la grilla. Revise los tipos de datos cargados.", True)
        ElseIf grid Is _gridRoles Then
            SetFeedback("roles", "Se detecto un problema de formato en la grilla de roles.", True)
        End If
    End Sub

    Private Sub ClearUsuarioDetail()
        _txtNombreUsuario.Text = String.Empty
        _txtNombreCompletoUsuario.Text = String.Empty
        _txtContrasenaUsuario.Text = String.Empty
        _chkUsuarioActivo.Checked = False
        _lstRolesUsuario.DataSource = Nothing
        SetFeedback("usuarios", "Seleccione un usuario para editar o eliminar.", False)
    End Sub

    Private Sub ClearRolDetail()
        _txtNombreRol.Text = String.Empty
        _txtDescripcionRol.Text = String.Empty
        _chkRolActivo.Checked = False
        _lstPermisosRol.DataSource = Nothing
        SetFeedback("roles", "Seleccione un rol para editar o eliminar.", False)
    End Sub
End Class
