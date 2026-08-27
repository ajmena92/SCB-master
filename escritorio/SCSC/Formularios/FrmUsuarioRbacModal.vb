Option Strict On
Option Explicit On

Imports System.Data
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

Public Class FrmUsuarioRbacModal
    Inherits Form

    Private NotInheritable Class RoleOption
        Public Property Id As Integer
        Public Property Nombre As String
        Public Property EsActivo As Boolean

        Public Overrides Function ToString() As String
            If EsActivo Then
                Return Nombre
            End If
            Return Nombre & " (inactivo)"
        End Function
    End Class

    Private ReadOnly _service As SeguridadRbacService
    Private ReadOnly _idUsuario As Integer?
    Private ReadOnly _esEdicion As Boolean
    Private _estaBloqueado As Boolean

    Private ReadOnly _layoutRoot As New TableLayoutPanel()
    Private ReadOnly _lblContrasena As New Label()
    Private ReadOnly _txtNombreUsuario As New TextBox()
    Private ReadOnly _txtNombreCompleto As New TextBox()
    Private ReadOnly _txtContrasena As New TextBox()
    Private ReadOnly _chkActivo As New CheckBox()
    Private ReadOnly _chkRoles As New CheckedListBox()
    Private ReadOnly _btnGuardar As New Button()
    Private ReadOnly _btnCancelar As New Button()
    Private ReadOnly _btnCambiarClave As New Button()
    Private ReadOnly _btnDesbloquear As New Button()

    Public Sub New(ByVal service As SeguridadRbacService, Optional ByVal idUsuario As Integer? = Nothing)
        _service = service
        _idUsuario = idUsuario
        _esEdicion = idUsuario.HasValue

        InicializarLayout()
        CargarRoles()

        If _esEdicion Then
            CargarUsuario(idUsuario.Value)
        Else
            Me.Text = "Nuevo usuario"
            _txtContrasena.Enabled = True
            _btnCambiarClave.Visible = False
            _btnDesbloquear.Visible = False
        End If
    End Sub

    Private Sub InicializarLayout()
        Me.Text = "Usuario"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MinimizeBox = False
        Me.MaximizeBox = False
        Me.ShowInTaskbar = False
        Me.ClientSize = New Size(700, 500)
        Me.MinimumSize = New Size(700, 500)
        Me.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular)
        Me.BackColor = Color.White

        _layoutRoot.Dock = DockStyle.Fill
        _layoutRoot.Padding = New Padding(16)
        _layoutRoot.ColumnCount = 2
        _layoutRoot.RowCount = 6
        _layoutRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170.0!))
        _layoutRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
        _layoutRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 40.0!))
        _layoutRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 40.0!))
        _layoutRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 40.0!))
        _layoutRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 40.0!))
        _layoutRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))
        _layoutRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 54.0!))

        Dim lblNombreUsuario As New Label()
        lblNombreUsuario.Text = "Nombre usuario"
        lblNombreUsuario.Anchor = AnchorStyles.Left
        lblNombreUsuario.AutoSize = True

        _txtNombreUsuario.Dock = DockStyle.Fill

        Dim lblNombreCompleto As New Label()
        lblNombreCompleto.Text = "Nombre completo"
        lblNombreCompleto.Anchor = AnchorStyles.Left
        lblNombreCompleto.AutoSize = True

        _txtNombreCompleto.Dock = DockStyle.Fill

        _lblContrasena.Text = "Contraseña inicial"
        _lblContrasena.Anchor = AnchorStyles.Left
        _lblContrasena.AutoSize = True

        _txtContrasena.Dock = DockStyle.Fill
        _txtContrasena.PasswordChar = "*"c

        Dim lblRoles As New Label()
        lblRoles.Text = "Roles obligatorios"
        lblRoles.Anchor = AnchorStyles.Left
        lblRoles.AutoSize = True

        _chkActivo.Text = "Usuario activo"
        _chkActivo.Checked = True
        _chkActivo.Anchor = AnchorStyles.Left

        _chkRoles.Dock = DockStyle.Fill
        _chkRoles.CheckOnClick = True
        _chkRoles.BorderStyle = BorderStyle.FixedSingle
        _chkRoles.IntegralHeight = False

        Dim headerActions As New FlowLayoutPanel()
        headerActions.Dock = DockStyle.Fill
        headerActions.FlowDirection = FlowDirection.LeftToRight
        headerActions.WrapContents = False

        _btnCambiarClave.Text = "Cambiar clave..."
        _btnCambiarClave.AutoSize = False
        _btnCambiarClave.Width = 130
        _btnCambiarClave.Height = 32
        AddHandler _btnCambiarClave.Click, AddressOf BtnCambiarClave_Click

        _btnDesbloquear.Text = "Desbloquear"
        _btnDesbloquear.AutoSize = False
        _btnDesbloquear.Width = 120
        _btnDesbloquear.Height = 32
        AddHandler _btnDesbloquear.Click, AddressOf BtnDesbloquear_Click

        headerActions.Controls.Add(_btnCambiarClave)
        headerActions.Controls.Add(_btnDesbloquear)

        Dim footer As New FlowLayoutPanel()
        footer.Dock = DockStyle.Fill
        footer.FlowDirection = FlowDirection.RightToLeft
        footer.WrapContents = False

        _btnGuardar.Text = "Guardar"
        _btnGuardar.AutoSize = False
        _btnGuardar.Width = 110
        _btnGuardar.Height = 34
        AddHandler _btnGuardar.Click, AddressOf BtnGuardar_Click

        _btnCancelar.Text = "Cancelar"
        _btnCancelar.AutoSize = False
        _btnCancelar.Width = 110
        _btnCancelar.Height = 34
        _btnCancelar.DialogResult = DialogResult.Cancel

        footer.Controls.Add(_btnCancelar)
        footer.Controls.Add(_btnGuardar)

        _layoutRoot.Controls.Add(lblNombreUsuario, 0, 0)
        _layoutRoot.Controls.Add(_txtNombreUsuario, 1, 0)
        _layoutRoot.Controls.Add(lblNombreCompleto, 0, 1)
        _layoutRoot.Controls.Add(_txtNombreCompleto, 1, 1)
        _layoutRoot.Controls.Add(_lblContrasena, 0, 2)
        _layoutRoot.Controls.Add(_txtContrasena, 1, 2)
        _layoutRoot.Controls.Add(_chkActivo, 0, 3)
        _layoutRoot.Controls.Add(headerActions, 1, 3)
        _layoutRoot.Controls.Add(lblRoles, 0, 4)
        _layoutRoot.Controls.Add(_chkRoles, 1, 4)
        _layoutRoot.Controls.Add(footer, 0, 5)
        _layoutRoot.SetColumnSpan(_chkActivo, 1)
        _layoutRoot.SetColumnSpan(footer, 2)

        Me.Controls.Add(_layoutRoot)
        Me.AcceptButton = _btnGuardar
        Me.CancelButton = _btnCancelar
    End Sub

    Private Sub CargarRoles()
        _chkRoles.Items.Clear()

        Dim roles As DataTable = _service.ListarRoles()
        For Each row As DataRow In roles.Rows
            Dim opcion As New RoleOption()
            opcion.Id = CInt(row("IdRol"))
            opcion.Nombre = Convert.ToString(row("NombreRol"))
            opcion.EsActivo = If(row("EsActivo") Is DBNull.Value, True, CBool(row("EsActivo")))
            _chkRoles.Items.Add(opcion, False)
        Next
    End Sub

    Private Sub CargarUsuario(ByVal idUsuario As Integer)
        Dim usuario As DataRow = _service.ObtenerUsuarioPorId(idUsuario)
        If usuario Is Nothing Then
            Throw New Exception("No se encontró el usuario seleccionado.")
        End If

        Me.Text = "Editar usuario"
        _txtNombreUsuario.Text = Convert.ToString(usuario("NombreUsuario"))
        _txtNombreUsuario.ReadOnly = True
        _txtNombreCompleto.Text = Convert.ToString(usuario("NombreCompleto"))
        _chkActivo.Checked = If(usuario("EsActivo") Is DBNull.Value, False, CBool(usuario("EsActivo")))
        _lblContrasena.Visible = False
        _txtContrasena.Visible = False
        _layoutRoot.RowStyles(2).Height = 0
        _estaBloqueado = Not usuario("BloqueadoHasta") Is DBNull.Value AndAlso CDate(usuario("BloqueadoHasta")) > DateTime.UtcNow
        _btnDesbloquear.Visible = _estaBloqueado

        Dim rolesAsignados As HashSet(Of Integer) = New HashSet(Of Integer)(
            _service.ListarRolesDeUsuario(idUsuario).AsEnumerable().Select(Function(row) CInt(row("IdRol"))))

        For i As Integer = 0 To _chkRoles.Items.Count - 1
            Dim opcion As RoleOption = TryCast(_chkRoles.Items(i), RoleOption)
            If opcion IsNot Nothing Then
                _chkRoles.SetItemChecked(i, rolesAsignados.Contains(opcion.Id))
            End If
        Next
    End Sub

    Private Function ObtenerRolesSeleccionados() As Integer()
        Dim seleccionados As New List(Of Integer)()
        For Each item As Object In _chkRoles.CheckedItems
            Dim opcion As RoleOption = TryCast(item, RoleOption)
            If opcion IsNot Nothing Then
                seleccionados.Add(opcion.Id)
            End If
        Next
        Return seleccionados.Distinct().ToArray()
    End Function

    Private Sub BtnGuardar_Click(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim rolesSeleccionados As Integer() = ObtenerRolesSeleccionados()
            If String.IsNullOrWhiteSpace(_txtNombreUsuario.Text) Then
                Throw New Exception("NombreUsuario es obligatorio.")
            End If
            If String.IsNullOrWhiteSpace(_txtNombreCompleto.Text) Then
                Throw New Exception("NombreCompleto es obligatorio.")
            End If
            If rolesSeleccionados.Length = 0 Then
                Throw New Exception("Debe asignar al menos un rol al usuario.")
            End If

            If _esEdicion Then
                _service.ActualizarUsuarioConRoles(_idUsuario.Value, _txtNombreCompleto.Text, _chkActivo.Checked, rolesSeleccionados)
            Else
                If String.IsNullOrWhiteSpace(_txtContrasena.Text) Then
                    Throw New Exception("Contraseña es obligatoria.")
                End If
                _service.CrearUsuarioConRoles(_txtNombreUsuario.Text, _txtNombreCompleto.Text, _txtContrasena.Text, _chkActivo.Checked, rolesSeleccionados)
            End If

            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnCambiarClave_Click(ByVal sender As Object, ByVal e As EventArgs)
        If Not _esEdicion Then
            Exit Sub
        End If

        Using frm As New FrmCambiarContrasenaUsuarioModal(_service, _idUsuario.Value, _txtNombreUsuario.Text)
            frm.ShowDialog(Me)
        End Using
    End Sub

    Private Sub BtnDesbloquear_Click(ByVal sender As Object, ByVal e As EventArgs)
        If Not _esEdicion OrElse Not _estaBloqueado Then
            Exit Sub
        End If

        Try
            _service.DesbloquearUsuario(_idUsuario.Value, "Desbloqueo manual desde edición RBAC.", If(String.IsNullOrWhiteSpace(CodigoUsuario), "LOCAL", CodigoUsuario))
            _estaBloqueado = False
            _btnDesbloquear.Visible = False
            MsgBox("Usuario desbloqueado correctamente.", MsgBoxStyle.Information)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
End Class
