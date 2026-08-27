Option Strict On
Option Explicit On

Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmRolRbacModal
    Inherits Form

    Private ReadOnly _service As SeguridadRbacService
    Private ReadOnly _idRol As Integer?
    Private ReadOnly _esEdicion As Boolean

    Private ReadOnly _layoutRoot As New TableLayoutPanel()
    Private ReadOnly _txtNombreRol As New TextBox()
    Private ReadOnly _txtDescripcionRol As New TextBox()
    Private ReadOnly _chkActivo As New CheckBox()
    Private ReadOnly _btnGuardar As New Button()
    Private ReadOnly _btnCancelar As New Button()

    Public Sub New(ByVal service As SeguridadRbacService, Optional ByVal idRol As Integer? = Nothing)
        _service = service
        _idRol = idRol
        _esEdicion = idRol.HasValue

        InicializarLayout()

        If _esEdicion Then
            CargarRol(idRol.Value)
        Else
            Me.Text = "Nuevo rol"
            _chkActivo.Checked = True
        End If
    End Sub

    Private Sub InicializarLayout()
        Me.Text = "Rol"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MinimizeBox = False
        Me.MaximizeBox = False
        Me.ShowInTaskbar = False
        Me.ClientSize = New Size(640, 250)
        Me.MinimumSize = New Size(640, 250)
        Me.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular)
        Me.BackColor = Color.White

        _layoutRoot.Dock = DockStyle.Fill
        _layoutRoot.Padding = New Padding(16)
        _layoutRoot.ColumnCount = 2
        _layoutRoot.RowCount = 4
        _layoutRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150.0!))
        _layoutRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
        _layoutRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 42.0!))
        _layoutRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 42.0!))
        _layoutRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 38.0!))
        _layoutRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 54.0!))

        Dim lblNombre As New Label()
        lblNombre.Text = "Nombre del rol"
        lblNombre.Anchor = AnchorStyles.Left
        lblNombre.AutoSize = True

        _txtNombreRol.Dock = DockStyle.Fill

        Dim lblDescripcion As New Label()
        lblDescripcion.Text = "Descripcion"
        lblDescripcion.Anchor = AnchorStyles.Left
        lblDescripcion.AutoSize = True

        _txtDescripcionRol.Dock = DockStyle.Fill

        _chkActivo.Text = "Rol activo"
        _chkActivo.Anchor = AnchorStyles.Left
        _chkActivo.Checked = True

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

        _layoutRoot.Controls.Add(lblNombre, 0, 0)
        _layoutRoot.Controls.Add(_txtNombreRol, 1, 0)
        _layoutRoot.Controls.Add(lblDescripcion, 0, 1)
        _layoutRoot.Controls.Add(_txtDescripcionRol, 1, 1)
        _layoutRoot.Controls.Add(_chkActivo, 1, 2)
        _layoutRoot.Controls.Add(footer, 0, 3)
        _layoutRoot.SetColumnSpan(footer, 2)

        Me.Controls.Add(_layoutRoot)
        Me.AcceptButton = _btnGuardar
        Me.CancelButton = _btnCancelar
    End Sub

    Private Sub CargarRol(ByVal idRol As Integer)
        Dim rol As DataRow = _service.ObtenerRolPorId(idRol)
        If rol Is Nothing Then
            Throw New Exception("No se encontro el rol seleccionado.")
        End If

        Me.Text = "Editar rol"
        _txtNombreRol.Text = Convert.ToString(rol("NombreRol"))
        _txtNombreRol.ReadOnly = True
        _txtDescripcionRol.Text = Convert.ToString(rol("Descripcion"))
        _chkActivo.Checked = If(rol("EsActivo") Is DBNull.Value, False, CBool(rol("EsActivo")))
    End Sub

    Private Sub BtnGuardar_Click(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If String.IsNullOrWhiteSpace(_txtNombreRol.Text) Then
                Throw New Exception("Nombre del rol es obligatorio.")
            End If

            If _esEdicion Then
                _service.ActualizarRol(_idRol.Value, _txtDescripcionRol.Text, _chkActivo.Checked)
            Else
                _service.CrearRol(_txtNombreRol.Text, _txtDescripcionRol.Text, _chkActivo.Checked)
            End If

            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
End Class
