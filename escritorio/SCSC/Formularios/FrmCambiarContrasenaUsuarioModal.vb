Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmCambiarContrasenaUsuarioModal
    Inherits Form

    Private ReadOnly _service As SeguridadRbacService
    Private ReadOnly _idUsuario As Integer

    Private ReadOnly _txtNueva As New TextBox()
    Private ReadOnly _txtConfirmacion As New TextBox()
    Private ReadOnly _btnGuardar As New Button()
    Private ReadOnly _btnCancelar As New Button()

    Public Sub New(ByVal service As SeguridadRbacService, ByVal idUsuario As Integer, ByVal nombreUsuario As String)
        _service = service
        _idUsuario = idUsuario

        Me.Text = "Cambiar contraseña"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MinimizeBox = False
        Me.MaximizeBox = False
        Me.ShowInTaskbar = False
        Me.ClientSize = New Size(460, 220)
        Me.Font = BrandTypography.CreateFont(10.0!, FontStyle.Regular)
        Me.BackColor = Color.FromArgb(248, 250, 253)

        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.Padding = New Padding(20)
        layout.ColumnCount = 2
        layout.RowCount = 4
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 158.0!))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 38.0!))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 44.0!))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 44.0!))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))

        Dim lblUsuario As New Label()
        lblUsuario.Text = "Usuario"
        lblUsuario.AutoSize = True
        lblUsuario.Anchor = AnchorStyles.Left
        lblUsuario.Font = BrandTypography.CreateFont(9.5!, FontStyle.Bold)

        Dim txtUsuario As New TextBox()
        txtUsuario.ReadOnly = True
        txtUsuario.Text = nombreUsuario
        txtUsuario.Dock = DockStyle.Fill
        txtUsuario.TabStop = False
        txtUsuario.BackColor = Color.FromArgb(238, 242, 247)
        txtUsuario.BorderStyle = BorderStyle.FixedSingle

        Dim lblNueva As New Label()
        lblNueva.Text = "Nueva contraseña"
        lblNueva.AutoSize = True
        lblNueva.Anchor = AnchorStyles.Left
        lblNueva.Font = BrandTypography.CreateFont(9.5!, FontStyle.Bold)

        _txtNueva.Dock = DockStyle.Fill
        _txtNueva.PasswordChar = "*"c
        _txtNueva.BorderStyle = BorderStyle.FixedSingle
        _txtNueva.Font = BrandTypography.CreateFont(10.0!, FontStyle.Regular)
        AddHandler _txtNueva.KeyDown, AddressOf NuevaContrasena_KeyDown

        Dim lblConfirmacion As New Label()
        lblConfirmacion.Text = "Confirmar contraseña"
        lblConfirmacion.AutoSize = True
        lblConfirmacion.Anchor = AnchorStyles.Left
        lblConfirmacion.Font = BrandTypography.CreateFont(9.5!, FontStyle.Bold)

        _txtConfirmacion.Dock = DockStyle.Fill
        _txtConfirmacion.PasswordChar = "*"c
        _txtConfirmacion.BorderStyle = BorderStyle.FixedSingle
        _txtConfirmacion.Font = BrandTypography.CreateFont(10.0!, FontStyle.Regular)
        AddHandler _txtConfirmacion.KeyDown, AddressOf Confirmacion_KeyDown

        Dim footer As New FlowLayoutPanel()
        footer.Dock = DockStyle.Fill
        footer.FlowDirection = FlowDirection.RightToLeft
        footer.WrapContents = False

        _btnGuardar.Text = "Guardar"
        _btnGuardar.AutoSize = False
        _btnGuardar.Width = 110
        _btnGuardar.Height = 34
        _btnGuardar.FlatStyle = FlatStyle.Flat
        _btnGuardar.FlatAppearance.BorderSize = 0
        _btnGuardar.BackColor = Color.FromArgb(24, 103, 184)
        _btnGuardar.ForeColor = Color.White
        _btnGuardar.Font = BrandTypography.CreateFont(9.5!, FontStyle.Bold)
        AddHandler _btnGuardar.Click, AddressOf BtnGuardar_Click

        _btnCancelar.Text = "Cancelar"
        _btnCancelar.AutoSize = False
        _btnCancelar.Width = 110
        _btnCancelar.Height = 34
        _btnCancelar.FlatStyle = FlatStyle.Flat
        _btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(178, 190, 204)
        _btnCancelar.BackColor = Color.White
        _btnCancelar.Font = BrandTypography.CreateFont(9.5!, FontStyle.Bold)
        _btnCancelar.DialogResult = DialogResult.Cancel

        footer.Controls.Add(_btnCancelar)
        footer.Controls.Add(_btnGuardar)

        layout.Controls.Add(lblUsuario, 0, 0)
        layout.Controls.Add(txtUsuario, 1, 0)
        layout.Controls.Add(lblNueva, 0, 1)
        layout.Controls.Add(_txtNueva, 1, 1)
        layout.Controls.Add(lblConfirmacion, 0, 2)
        layout.Controls.Add(_txtConfirmacion, 1, 2)
        layout.Controls.Add(footer, 0, 3)
        layout.SetColumnSpan(footer, 2)

        Me.Controls.Add(layout)
        Me.AcceptButton = _btnGuardar
        Me.CancelButton = _btnCancelar
        AddHandler Me.Shown, AddressOf Modal_Shown
    End Sub

    Private Sub Modal_Shown(ByVal sender As Object, ByVal e As EventArgs)
        BeginInvoke(New Action(Sub()
                                   _txtNueva.Focus()
                                   _txtNueva.SelectAll()
                               End Sub))
    End Sub

    Private Sub NuevaContrasena_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        If e.KeyCode <> Keys.Enter Then
            Exit Sub
        End If

        e.SuppressKeyPress = True
        _txtConfirmacion.Focus()
        _txtConfirmacion.SelectAll()
    End Sub

    Private Sub Confirmacion_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        If e.KeyCode <> Keys.Enter Then
            Exit Sub
        End If

        e.SuppressKeyPress = True
        _btnGuardar.PerformClick()
    End Sub

    Private Sub BtnGuardar_Click(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If String.IsNullOrWhiteSpace(_txtNueva.Text) Then
                MsgBox("Debe indicar la nueva contraseña.", MsgBoxStyle.Exclamation)
                _txtNueva.Focus()
                Exit Sub
            End If
            If _txtNueva.Text <> _txtConfirmacion.Text Then
                MsgBox("La confirmación de contraseña no coincide.", MsgBoxStyle.Exclamation)
                _txtConfirmacion.Focus()
                _txtConfirmacion.SelectAll()
                Exit Sub
            End If

            _service.CambiarContrasenaUsuario(_idUsuario, _txtNueva.Text)
            MsgBox("Contraseña actualizada.", MsgBoxStyle.Information)
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
End Class
