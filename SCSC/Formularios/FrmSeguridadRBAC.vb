Option Explicit On
Option Strict On

Imports System.Data
Imports System.ComponentModel
Imports System.Windows.Forms

Partial Public Class FrmSeguridadRBAC

    Private ReadOnly _service As New SeguridadRbacService()

    Public Sub New()
        InitializeComponent()
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MinimumSize = New Size(900, 620)
        Me.Size = New Size(980, 640)
    End Sub

    Private Sub FrmSeguridadRBAC_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If IsInDesignMode() Then
            Return
        End If

        Try
            Me.KeyPreview = True
            UIThemeManagerV2.Apply(Me, "dialogo")
            ApplyVisualStandard2026()
            EnsureCrudAreaVisible()

            ConfigurarGrid(_gridUsuarios)
            ConfigurarGrid(_gridRoles)
            ConfigurarGrid(_gridPermisos)

            _txtContrasenaUsuario.PasswordChar = "*"c
            _cmbRolUsuario.DropDownStyle = ComboBoxStyle.DropDownList
            _cmbPermisoRol.DropDownStyle = ComboBoxStyle.DropDownList
            RecargarTodo()
            UpdateActionStates()
        Catch ex As Exception
            MsgBox("Error cargando formulario de seguridad RBAC: " & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub FormatearBoton(ByVal btn As Button, Optional ByVal isDanger As Boolean = False)
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.BackColor = If(isDanger, UIConstants.Danger, UIConstants.Accent)
        btn.ForeColor = Color.White
        btn.Font = UIConstants.FontBodyStrong()
        btn.Height = 34
    End Sub

    Private Sub ConfigurarGrid(ByVal grid As DataGridView)
        grid.ReadOnly = True
        grid.MultiSelect = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
    End Sub

    Private Sub EnsureCrudAreaVisible()
        ' Evita que la botonera se colapse por filas porcentuales en distintos DPI.
        If LayoutUsuarios.RowStyles.Count >= 4 Then
            LayoutUsuarios.RowStyles(0).SizeType = SizeType.Absolute
            LayoutUsuarios.RowStyles(0).Height = 34
            LayoutUsuarios.RowStyles(1).SizeType = SizeType.Absolute
            LayoutUsuarios.RowStyles(1).Height = 34
            LayoutUsuarios.RowStyles(2).SizeType = SizeType.Absolute
            LayoutUsuarios.RowStyles(2).Height = 120
            LayoutUsuarios.RowStyles(3).SizeType = SizeType.Absolute
            LayoutUsuarios.RowStyles(3).Height = 72
        End If

        If LayoutRoles.RowStyles.Count >= 4 Then
            LayoutRoles.RowStyles(0).SizeType = SizeType.Absolute
            LayoutRoles.RowStyles(0).Height = 34
            LayoutRoles.RowStyles(1).SizeType = SizeType.Absolute
            LayoutRoles.RowStyles(1).Height = 34
            LayoutRoles.RowStyles(2).SizeType = SizeType.Absolute
            LayoutRoles.RowStyles(2).Height = 120
            LayoutRoles.RowStyles(3).SizeType = SizeType.Absolute
            LayoutRoles.RowStyles(3).Height = 72
        End If

        If LayoutPermisos.RowStyles.Count >= 3 Then
            LayoutPermisos.RowStyles(0).SizeType = SizeType.Absolute
            LayoutPermisos.RowStyles(0).Height = 34
            LayoutPermisos.RowStyles(1).SizeType = SizeType.Absolute
            LayoutPermisos.RowStyles(1).Height = 34
            LayoutPermisos.RowStyles(2).SizeType = SizeType.Absolute
            LayoutPermisos.RowStyles(2).Height = 72
        End If

        FlowUsuariosBotones.AutoSize = False
        FlowUsuariosBotones.Height = 64
        FlowUsuariosBotones.WrapContents = True

        FlowRolesBotones.AutoSize = False
        FlowRolesBotones.Height = 64
        FlowRolesBotones.WrapContents = True

        FlowPermisosBotones.AutoSize = False
        FlowPermisosBotones.Height = 64
        FlowPermisosBotones.WrapContents = True

        PanelUsuariosBottom.AutoScroll = True
        PanelRolesBottom.AutoScroll = True
        PanelPermisosBottom.AutoScroll = True

        ' Fallback defensivo: evita colapso de layouts en ciertas combinaciones DPI/tema.
        LayoutUsuarios.Dock = DockStyle.Top
        LayoutUsuarios.Height = Math.Max(260, LayoutUsuarios.Height)
        LayoutRoles.Dock = DockStyle.Top
        LayoutRoles.Height = Math.Max(260, LayoutRoles.Height)
        LayoutPermisos.Dock = DockStyle.Top
        LayoutPermisos.Height = Math.Max(180, LayoutPermisos.Height)

        PanelUsuariosBottom.MinimumSize = New Size(0, 260)
        PanelRolesBottom.MinimumSize = New Size(0, 260)
        PanelPermisosBottom.MinimumSize = New Size(0, 180)

        LabelUsuarioNombre.Visible = True
        _txtNombreUsuario.Visible = True
        LabelUsuarioCompleto.Visible = True
        _txtNombreCompletoUsuario.Visible = True
        LabelUsuarioContrasena.Visible = True
        _txtContrasenaUsuario.Visible = True
        LabelRolAsignar.Visible = True
        _cmbRolUsuario.Visible = True
        LabelRolesUsuario.Visible = True
        _lstRolesUsuario.Visible = True
        FlowUsuariosBotones.Visible = True

        LabelNombreRol.Visible = True
        _txtNombreRol.Visible = True
        LabelDescripcionRol.Visible = True
        _txtDescripcionRol.Visible = True
        LabelPermisoAsignar.Visible = True
        _cmbPermisoRol.Visible = True
        LabelPermisosRol.Visible = True
        _lstPermisosRol.Visible = True
        FlowRolesBotones.Visible = True

        LabelClavePermiso.Visible = True
        _txtClavePermiso.Visible = True
        LabelDescripcionPermiso.Visible = True
        _txtDescripcionPermiso.Visible = True
        FlowPermisosBotones.Visible = True
    End Sub

    Private Sub ApplyVisualStandard2026()
        Me.BackColor = UIConstants.AppBackground
        TabsPrincipal.Font = UIConstants.FontBodyStrong()
        TabUsuarios.BackColor = UIConstants.Surface
        TabRoles.BackColor = UIConstants.Surface
        TabPermisos.BackColor = UIConstants.Surface

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
        _btnSalir.Width = 110
        _btnSalir.Height = 32
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
        _gridUsuarios.DataSource = _service.ListarUsuarios()
    End Sub

    Private Sub CargarRoles()
        _gridRoles.DataSource = _service.ListarRoles()
    End Sub

    Private Sub CargarPermisos()
        _gridPermisos.DataSource = _service.ListarPermisos()
    End Sub

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
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnActualizarUsuario_Click(sender As Object, e As EventArgs) Handles _btnActualizarUsuario.Click
        Try
            Dim idUsuario As Integer = ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")
            _service.ActualizarUsuario(idUsuario, _txtNombreCompletoUsuario.Text, _chkUsuarioActivo.Checked)
            CargarUsuarios()
            UpdateActionStates()
        Catch ex As Exception
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
        Catch ex As Exception
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
        Catch ex As Exception
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
        Catch ex As Exception
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
        Catch ex As Exception
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
        Catch ex As Exception
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
        Catch ex As Exception
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
        Catch ex As Exception
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
        Catch ex As Exception
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
        Catch ex As Exception
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
        Catch ex As Exception
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
        Catch ex As Exception
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
        Catch ex As Exception
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
End Class
