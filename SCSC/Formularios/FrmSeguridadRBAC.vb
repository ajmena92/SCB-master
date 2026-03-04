Option Explicit On
Option Strict On

Imports System.Data
Imports System.Windows.Forms

Partial Public Class FrmSeguridadRBAC

    Private ReadOnly _service As New SeguridadRbacService()

    Public Sub New()
        InitializeComponent()
        UIThemeManagerV2.Apply(Me, "dialogo")
        Me.StartPosition = FormStartPosition.CenterParent

        ConfigurarGrid(_gridUsuarios)
        ConfigurarGrid(_gridRoles)
        ConfigurarGrid(_gridPermisos)

        _txtContrasenaUsuario.PasswordChar = "*"c
        _cmbRolUsuario.DropDownStyle = ComboBoxStyle.DropDownList
        _cmbPermisoRol.DropDownStyle = ComboBoxStyle.DropDownList

        FormatearBoton(_btnCrearUsuario)
        FormatearBoton(_btnActualizarUsuario)
        FormatearBoton(_btnEliminarUsuario)
        FormatearBoton(_btnCambiarClave)
        FormatearBoton(_btnAsignarRolUsuario)
        FormatearBoton(_btnRevocarRolUsuario)

        FormatearBoton(_btnCrearRol)
        FormatearBoton(_btnActualizarRol)
        FormatearBoton(_btnEliminarRol)
        FormatearBoton(_btnAsignarPermisoRol)
        FormatearBoton(_btnRevocarPermisoRol)

        FormatearBoton(_btnCrearPermiso)
        FormatearBoton(_btnActualizarPermiso)
        FormatearBoton(_btnEliminarPermiso)

        AddHandler _gridUsuarios.SelectionChanged, AddressOf GridUsuarios_SelectionChanged
        AddHandler _gridRoles.SelectionChanged, AddressOf GridRoles_SelectionChanged
        AddHandler _gridPermisos.SelectionChanged, AddressOf GridPermisos_SelectionChanged

        AddHandler _btnCrearUsuario.Click, AddressOf BtnCrearUsuario_Click
        AddHandler _btnActualizarUsuario.Click, AddressOf BtnActualizarUsuario_Click
        AddHandler _btnEliminarUsuario.Click, AddressOf BtnEliminarUsuario_Click
        AddHandler _btnCambiarClave.Click, AddressOf BtnCambiarClave_Click
        AddHandler _btnAsignarRolUsuario.Click, AddressOf BtnAsignarRolUsuario_Click
        AddHandler _btnRevocarRolUsuario.Click, AddressOf BtnRevocarRolUsuario_Click

        AddHandler _btnCrearRol.Click, AddressOf BtnCrearRol_Click
        AddHandler _btnActualizarRol.Click, AddressOf BtnActualizarRol_Click
        AddHandler _btnEliminarRol.Click, AddressOf BtnEliminarRol_Click
        AddHandler _btnAsignarPermisoRol.Click, AddressOf BtnAsignarPermisoRol_Click
        AddHandler _btnRevocarPermisoRol.Click, AddressOf BtnRevocarPermisoRol_Click

        AddHandler _btnCrearPermiso.Click, AddressOf BtnCrearPermiso_Click
        AddHandler _btnActualizarPermiso.Click, AddressOf BtnActualizarPermiso_Click
        AddHandler _btnEliminarPermiso.Click, AddressOf BtnEliminarPermiso_Click
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        RecargarTodo()
    End Sub

    Private Sub FormatearBoton(ByVal btn As Button)
        btn.FlatStyle = FlatStyle.Flat
        btn.BackColor = UIConstants.Accent
        btn.ForeColor = Color.White
    End Sub

    Private Sub ConfigurarGrid(ByVal grid As DataGridView)
        grid.ReadOnly = True
        grid.MultiSelect = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
    End Sub

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

    Private Sub GridUsuarios_SelectionChanged(sender As Object, e As EventArgs)
        If _gridUsuarios.CurrentRow Is Nothing Then
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
    End Sub

    Private Sub GridRoles_SelectionChanged(sender As Object, e As EventArgs)
        If _gridRoles.CurrentRow Is Nothing Then
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
    End Sub

    Private Sub GridPermisos_SelectionChanged(sender As Object, e As EventArgs)
        If _gridPermisos.CurrentRow Is Nothing Then
            Return
        End If

        _txtClavePermiso.Text = ValorCelda(_gridPermisos.CurrentRow, "ClavePermiso")
        _txtDescripcionPermiso.Text = ValorCelda(_gridPermisos.CurrentRow, "Descripcion")
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

    Private Sub BtnCrearUsuario_Click(sender As Object, e As EventArgs)
        Try
            If String.IsNullOrWhiteSpace(_txtNombreUsuario.Text) OrElse String.IsNullOrWhiteSpace(_txtNombreCompletoUsuario.Text) OrElse String.IsNullOrWhiteSpace(_txtContrasenaUsuario.Text) Then
                Throw New Exception("NombreUsuario, NombreCompleto y Contraseña son obligatorios.")
            End If

            _service.CrearUsuario(_txtNombreUsuario.Text, _txtNombreCompletoUsuario.Text, _txtContrasenaUsuario.Text, _chkUsuarioActivo.Checked)
            _txtContrasenaUsuario.Text = ""
            CargarUsuarios()
            CargarCombos()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnActualizarUsuario_Click(sender As Object, e As EventArgs)
        Try
            Dim idUsuario As Integer = ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")
            _service.ActualizarUsuario(idUsuario, _txtNombreCompletoUsuario.Text, _chkUsuarioActivo.Checked)
            CargarUsuarios()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnCambiarClave_Click(sender As Object, e As EventArgs)
        Try
            Dim idUsuario As Integer = ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")
            If String.IsNullOrWhiteSpace(_txtContrasenaUsuario.Text) Then
                Throw New Exception("Debe indicar la nueva contraseña.")
            End If
            _service.CambiarContrasenaUsuario(idUsuario, _txtContrasenaUsuario.Text)
            _txtContrasenaUsuario.Text = ""
            MsgBox("Contraseña actualizada.", MsgBoxStyle.Information)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnEliminarUsuario_Click(sender As Object, e As EventArgs)
        Try
            Dim idUsuario As Integer = ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")
            _service.EliminarUsuario(idUsuario)
            CargarUsuarios()
            CargarCombos()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnAsignarRolUsuario_Click(sender As Object, e As EventArgs)
        Try
            Dim idUsuario As Integer = ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")
            If _cmbRolUsuario.SelectedValue Is Nothing Then
                Throw New Exception("No hay rol seleccionado.")
            End If
            Dim idRol As Integer = Convert.ToInt32(_cmbRolUsuario.SelectedValue)
            _service.AsignarRolAUsuario(idUsuario, idRol)
            GridUsuarios_SelectionChanged(Nothing, EventArgs.Empty)
            CargarUsuarios()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnRevocarRolUsuario_Click(sender As Object, e As EventArgs)
        Try
            Dim idUsuario As Integer = ObtenerIdSeleccionado(_gridUsuarios, "IdUsuario")
            If _lstRolesUsuario.SelectedValue Is Nothing Then
                Throw New Exception("Seleccione el rol a revocar del listado de roles del usuario.")
            End If
            Dim idRol As Integer = Convert.ToInt32(_lstRolesUsuario.SelectedValue)
            _service.RevocarRolAUsuario(idUsuario, idRol)
            GridUsuarios_SelectionChanged(Nothing, EventArgs.Empty)
            CargarUsuarios()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnCrearRol_Click(sender As Object, e As EventArgs)
        Try
            If String.IsNullOrWhiteSpace(_txtNombreRol.Text) Then
                Throw New Exception("NombreRol es obligatorio.")
            End If
            _service.CrearRol(_txtNombreRol.Text, _txtDescripcionRol.Text, _chkRolActivo.Checked)
            CargarRoles()
            CargarCombos()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnActualizarRol_Click(sender As Object, e As EventArgs)
        Try
            Dim idRol As Integer = ObtenerIdSeleccionado(_gridRoles, "IdRol")
            _service.ActualizarRol(idRol, _txtDescripcionRol.Text, _chkRolActivo.Checked)
            CargarRoles()
            CargarCombos()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnEliminarRol_Click(sender As Object, e As EventArgs)
        Try
            Dim idRol As Integer = ObtenerIdSeleccionado(_gridRoles, "IdRol")
            _service.EliminarRol(idRol)
            CargarRoles()
            CargarCombos()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnAsignarPermisoRol_Click(sender As Object, e As EventArgs)
        Try
            Dim idRol As Integer = ObtenerIdSeleccionado(_gridRoles, "IdRol")
            If _cmbPermisoRol.SelectedValue Is Nothing Then
                Throw New Exception("No hay permiso seleccionado.")
            End If
            Dim idPermiso As Integer = Convert.ToInt32(_cmbPermisoRol.SelectedValue)
            _service.AsignarPermisoARol(idRol, idPermiso)
            GridRoles_SelectionChanged(Nothing, EventArgs.Empty)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnRevocarPermisoRol_Click(sender As Object, e As EventArgs)
        Try
            Dim idRol As Integer = ObtenerIdSeleccionado(_gridRoles, "IdRol")
            If _lstPermisosRol.SelectedValue Is Nothing Then
                Throw New Exception("Seleccione el permiso a revocar del listado de permisos del rol.")
            End If
            Dim idPermiso As Integer = Convert.ToInt32(_lstPermisosRol.SelectedValue)
            _service.RevocarPermisoARol(idRol, idPermiso)
            GridRoles_SelectionChanged(Nothing, EventArgs.Empty)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnCrearPermiso_Click(sender As Object, e As EventArgs)
        Try
            If String.IsNullOrWhiteSpace(_txtClavePermiso.Text) Then
                Throw New Exception("ClavePermiso es obligatoria.")
            End If
            _service.CrearPermiso(_txtClavePermiso.Text, _txtDescripcionPermiso.Text)
            CargarPermisos()
            CargarCombos()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnActualizarPermiso_Click(sender As Object, e As EventArgs)
        Try
            Dim idPermiso As Integer = ObtenerIdSeleccionado(_gridPermisos, "IdPermiso")
            _service.ActualizarPermiso(idPermiso, _txtDescripcionPermiso.Text)
            CargarPermisos()
            CargarCombos()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnEliminarPermiso_Click(sender As Object, e As EventArgs)
        Try
            Dim idPermiso As Integer = ObtenerIdSeleccionado(_gridPermisos, "IdPermiso")
            _service.EliminarPermiso(idPermiso)
            CargarPermisos()
            CargarCombos()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
End Class
