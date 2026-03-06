<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmAgregarEstudiante
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmAgregarEstudiante))
        Me.GroupDatosEstudiante = New System.Windows.Forms.GroupBox()
        Me.CBEspecialidad = New System.Windows.Forms.ComboBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.CBHorario = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TxtFecNac = New System.Windows.Forms.DateTimePicker()
        Me.PanelAcciones = New System.Windows.Forms.Panel()
        Me.BtnEliminar = New System.Windows.Forms.Button()
        Me.BtnRegresar = New System.Windows.Forms.Button()
        Me.BtnCancelar = New System.Windows.Forms.Button()
        Me.BtnGuardar = New System.Windows.Forms.Button()
        Me.TxtSeccion = New System.Windows.Forms.TextBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.TxtTelefono = New System.Windows.Forms.TextBox()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.TxtApe1 = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.TxtApe2 = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.TxtNombre = New System.Windows.Forms.TextBox()
        Me.LblNombre = New System.Windows.Forms.Label()
        Me.LblTituloModulo = New System.Windows.Forms.Label()
        Me.GroupBusquedaEstudiante = New System.Windows.Forms.GroupBox()
        Me.TxtCedula = New System.Windows.Forms.TextBox()
        Me.Buscar = New System.Windows.Forms.Button()
        Me.LblCedulaBusqueda = New System.Windows.Forms.Label()
        Me.GroupDatosEstudiante.SuspendLayout()
        Me.PanelAcciones.SuspendLayout()
        Me.GroupBusquedaEstudiante.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupDatosEstudiante
        '
        Me.GroupDatosEstudiante.BackColor = System.Drawing.Color.Transparent
        Me.GroupDatosEstudiante.Controls.Add(Me.CBEspecialidad)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label8)
        Me.GroupDatosEstudiante.Controls.Add(Me.CBHorario)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label2)
        Me.GroupDatosEstudiante.Controls.Add(Me.TxtFecNac)
        Me.GroupDatosEstudiante.Controls.Add(Me.PanelAcciones)
        Me.GroupDatosEstudiante.Controls.Add(Me.TxtSeccion)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label10)
        Me.GroupDatosEstudiante.Controls.Add(Me.TxtTelefono)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label12)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label7)
        Me.GroupDatosEstudiante.Controls.Add(Me.TxtApe1)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label6)
        Me.GroupDatosEstudiante.Controls.Add(Me.TxtApe2)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label5)
        Me.GroupDatosEstudiante.Controls.Add(Me.TxtNombre)
        Me.GroupDatosEstudiante.Controls.Add(Me.LblNombre)
        Me.GroupDatosEstudiante.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupDatosEstudiante.ForeColor = System.Drawing.Color.Black
        Me.GroupDatosEstudiante.Location = New System.Drawing.Point(12, 202)
        Me.GroupDatosEstudiante.Name = "GroupDatosEstudiante"
        Me.GroupDatosEstudiante.Size = New System.Drawing.Size(620, 372)
        Me.GroupDatosEstudiante.TabIndex = 21
        Me.GroupDatosEstudiante.TabStop = False
        Me.GroupDatosEstudiante.Text = "Datos del Estudiante"
        '
        'CBEspecialidad
        '
        Me.CBEspecialidad.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.CBEspecialidad.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CBEspecialidad.FormattingEnabled = True
        Me.CBEspecialidad.Location = New System.Drawing.Point(33, 217)
        Me.CBEspecialidad.Name = "CBEspecialidad"
        Me.CBEspecialidad.Size = New System.Drawing.Size(171, 31)
        Me.CBEspecialidad.TabIndex = 65
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.BackColor = System.Drawing.Color.Transparent
        Me.Label8.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label8.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.Location = New System.Drawing.Point(216, 190)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(88, 24)
        Me.Label8.TabIndex = 64
        Me.Label8.Text = "Horario : "
        '
        'CBHorario
        '
        Me.CBHorario.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.CBHorario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CBHorario.FormattingEnabled = True
        Me.CBHorario.Location = New System.Drawing.Point(220, 217)
        Me.CBHorario.Name = "CBHorario"
        Me.CBHorario.Size = New System.Drawing.Size(171, 31)
        Me.CBHorario.TabIndex = 63
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label2.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(29, 191)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(172, 24)
        Me.Label2.TabIndex = 59
        Me.Label2.Text = "Especilidad o Nivel:"
        '
        'TxtFecNac
        '
        Me.TxtFecNac.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.TxtFecNac.Location = New System.Drawing.Point(418, 149)
        Me.TxtFecNac.Name = "TxtFecNac"
        Me.TxtFecNac.Size = New System.Drawing.Size(158, 30)
        Me.TxtFecNac.TabIndex = 61
        '
        'PanelAcciones
        '
        Me.PanelAcciones.BackColor = System.Drawing.Color.Transparent
        Me.PanelAcciones.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PanelAcciones.Controls.Add(Me.BtnEliminar)
        Me.PanelAcciones.Controls.Add(Me.BtnRegresar)
        Me.PanelAcciones.Controls.Add(Me.BtnCancelar)
        Me.PanelAcciones.Controls.Add(Me.BtnGuardar)
        Me.PanelAcciones.Location = New System.Drawing.Point(297, 291)
        Me.PanelAcciones.Name = "PanelAcciones"
        Me.PanelAcciones.Size = New System.Drawing.Size(281, 66)
        Me.PanelAcciones.TabIndex = 19
        '
        'BtnEliminar
        '
        Me.BtnEliminar.BackgroundImage = Global.SCSC.My.Resources.Resources.Eliminar
        Me.BtnEliminar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnEliminar.Location = New System.Drawing.Point(74, -1)
        Me.BtnEliminar.Name = "BtnEliminar"
        Me.BtnEliminar.Size = New System.Drawing.Size(62, 63)
        Me.BtnEliminar.TabIndex = 62
        Me.BtnEliminar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText
        Me.BtnEliminar.UseVisualStyleBackColor = True
        '
        'BtnRegresar
        '
        Me.BtnRegresar.BackgroundImage = Global.SCSC.My.Resources.Resources.Regresar
        Me.BtnRegresar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnRegresar.Location = New System.Drawing.Point(210, -1)
        Me.BtnRegresar.Name = "BtnRegresar"
        Me.BtnRegresar.Size = New System.Drawing.Size(63, 63)
        Me.BtnRegresar.TabIndex = 7
        Me.BtnRegresar.UseVisualStyleBackColor = True
        '
        'BtnCancelar
        '
        Me.BtnCancelar.BackgroundImage = Global.SCSC.My.Resources.Resources.Cancelar
        Me.BtnCancelar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnCancelar.Location = New System.Drawing.Point(142, -1)
        Me.BtnCancelar.Name = "BtnCancelar"
        Me.BtnCancelar.Size = New System.Drawing.Size(63, 63)
        Me.BtnCancelar.TabIndex = 5
        Me.BtnCancelar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText
        Me.BtnCancelar.UseVisualStyleBackColor = True
        '
        'BtnGuardar
        '
        Me.BtnGuardar.BackgroundImage = Global.SCSC.My.Resources.Resources.Aceptar
        Me.BtnGuardar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnGuardar.Location = New System.Drawing.Point(7, 0)
        Me.BtnGuardar.Name = "BtnGuardar"
        Me.BtnGuardar.Size = New System.Drawing.Size(63, 63)
        Me.BtnGuardar.TabIndex = 0
        Me.BtnGuardar.UseVisualStyleBackColor = True
        '
        'TxtSeccion
        '
        Me.TxtSeccion.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtSeccion.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtSeccion.Location = New System.Drawing.Point(220, 149)
        Me.TxtSeccion.MaxLength = 30
        Me.TxtSeccion.Name = "TxtSeccion"
        Me.TxtSeccion.Size = New System.Drawing.Size(158, 30)
        Me.TxtSeccion.TabIndex = 59
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.BackColor = System.Drawing.Color.Transparent
        Me.Label10.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label10.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label10.Location = New System.Drawing.Point(216, 122)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(82, 24)
        Me.Label10.TabIndex = 60
        Me.Label10.Text = "Sección :"
        '
        'TxtTelefono
        '
        Me.TxtTelefono.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtTelefono.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtTelefono.Location = New System.Drawing.Point(31, 149)
        Me.TxtTelefono.MaxLength = 50
        Me.TxtTelefono.Name = "TxtTelefono"
        Me.TxtTelefono.Size = New System.Drawing.Size(158, 30)
        Me.TxtTelefono.TabIndex = 57
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.BackColor = System.Drawing.Color.Transparent
        Me.Label12.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label12.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label12.Location = New System.Drawing.Point(29, 122)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(92, 24)
        Me.Label12.TabIndex = 58
        Me.Label12.Text = "Telefono :"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.BackColor = System.Drawing.Color.Transparent
        Me.Label7.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label7.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.Location = New System.Drawing.Point(414, 122)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(164, 24)
        Me.Label7.TabIndex = 48
        Me.Label7.Text = "Fecha Nacimiento:"
        '
        'TxtApe1
        '
        Me.TxtApe1.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtApe1.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtApe1.Location = New System.Drawing.Point(220, 72)
        Me.TxtApe1.MaxLength = 100
        Me.TxtApe1.Name = "TxtApe1"
        Me.TxtApe1.Size = New System.Drawing.Size(158, 30)
        Me.TxtApe1.TabIndex = 45
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.BackColor = System.Drawing.Color.Transparent
        Me.Label6.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label6.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.Location = New System.Drawing.Point(216, 43)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(145, 24)
        Me.Label6.TabIndex = 46
        Me.Label6.Text = "Primer Apellido:"
        '
        'TxtApe2
        '
        Me.TxtApe2.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtApe2.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtApe2.Location = New System.Drawing.Point(418, 72)
        Me.TxtApe2.MaxLength = 100
        Me.TxtApe2.Name = "TxtApe2"
        Me.TxtApe2.Size = New System.Drawing.Size(158, 30)
        Me.TxtApe2.TabIndex = 46
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.BackColor = System.Drawing.Color.Transparent
        Me.Label5.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label5.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.Location = New System.Drawing.Point(414, 43)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(162, 24)
        Me.Label5.TabIndex = 44
        Me.Label5.Text = "Segundo Apellido:"
        '
        'TxtNombre
        '
        Me.TxtNombre.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtNombre.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtNombre.Location = New System.Drawing.Point(31, 72)
        Me.TxtNombre.MaxLength = 100
        Me.TxtNombre.Name = "TxtNombre"
        Me.TxtNombre.Size = New System.Drawing.Size(158, 30)
        Me.TxtNombre.TabIndex = 41
        '
        'LblNombre
        '
        Me.LblNombre.AutoSize = True
        Me.LblNombre.BackColor = System.Drawing.Color.Transparent
        Me.LblNombre.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.LblNombre.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblNombre.Location = New System.Drawing.Point(27, 45)
        Me.LblNombre.Name = "LblNombre"
        Me.LblNombre.Size = New System.Drawing.Size(83, 24)
        Me.LblNombre.TabIndex = 42
        Me.LblNombre.Text = "Nombre:"
        '
        'LblTituloModulo
        '
        Me.LblTituloModulo.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblTituloModulo.AutoSize = True
        Me.LblTituloModulo.BackColor = System.Drawing.Color.Transparent
        Me.LblTituloModulo.Font = New System.Drawing.Font("Arial Narrow", 21.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTituloModulo.ForeColor = System.Drawing.Color.Black
        Me.LblTituloModulo.Location = New System.Drawing.Point(136, 20)
        Me.LblTituloModulo.Name = "LblTituloModulo"
        Me.LblTituloModulo.Size = New System.Drawing.Size(386, 43)
        Me.LblTituloModulo.TabIndex = 20
        Me.LblTituloModulo.Text = "Agregar Estudiante Manual"
        Me.LblTituloModulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'GroupBusquedaEstudiante
        '
        Me.GroupBusquedaEstudiante.BackColor = System.Drawing.Color.Transparent
        Me.GroupBusquedaEstudiante.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.GroupBusquedaEstudiante.Controls.Add(Me.TxtCedula)
        Me.GroupBusquedaEstudiante.Controls.Add(Me.Buscar)
        Me.GroupBusquedaEstudiante.Controls.Add(Me.LblCedulaBusqueda)
        Me.GroupBusquedaEstudiante.Location = New System.Drawing.Point(18, 79)
        Me.GroupBusquedaEstudiante.Name = "GroupBusquedaEstudiante"
        Me.GroupBusquedaEstudiante.Size = New System.Drawing.Size(614, 108)
        Me.GroupBusquedaEstudiante.TabIndex = 0
        Me.GroupBusquedaEstudiante.TabStop = False
        Me.GroupBusquedaEstudiante.Text = "Estudiante"
        '
        'TxtCedula
        '
        Me.TxtCedula.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtCedula.Font = New System.Drawing.Font("Calibri", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtCedula.Location = New System.Drawing.Point(106, 51)
        Me.TxtCedula.MaxLength = 14
        Me.TxtCedula.Name = "TxtCedula"
        Me.TxtCedula.Size = New System.Drawing.Size(163, 36)
        Me.TxtCedula.TabIndex = 0
        '
        'Buscar
        '
        Me.Buscar.AccessibleRole = System.Windows.Forms.AccessibleRole.ButtonMenu
        Me.Buscar.BackColor = System.Drawing.Color.Transparent
        Me.Buscar.BackgroundImage = Global.SCSC.My.Resources.Resources.Buscar_30x30
        Me.Buscar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.Buscar.FlatAppearance.BorderColor = System.Drawing.Color.WhiteSmoke
        Me.Buscar.FlatAppearance.BorderSize = 0
        Me.Buscar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.Buscar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.Buscar.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Buscar.ForeColor = System.Drawing.Color.White
        Me.Buscar.Location = New System.Drawing.Point(275, 53)
        Me.Buscar.Name = "Buscar"
        Me.Buscar.Size = New System.Drawing.Size(27, 27)
        Me.Buscar.TabIndex = 41
        Me.Buscar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.Buscar.UseVisualStyleBackColor = False
        '
        'LblCedulaBusqueda
        '
        Me.LblCedulaBusqueda.AutoSize = True
        Me.LblCedulaBusqueda.BackColor = System.Drawing.Color.Transparent
        Me.LblCedulaBusqueda.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.LblCedulaBusqueda.Font = New System.Drawing.Font("Arial Narrow", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblCedulaBusqueda.Location = New System.Drawing.Point(23, 57)
        Me.LblCedulaBusqueda.Name = "LblCedulaBusqueda"
        Me.LblCedulaBusqueda.Size = New System.Drawing.Size(66, 24)
        Me.LblCedulaBusqueda.TabIndex = 40
        Me.LblCedulaBusqueda.Text = "Cédula:"
        '
        'FrmAgregarEstudiante
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(10.0!, 24.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(647, 586)
        Me.Controls.Add(Me.GroupBusquedaEstudiante)
        Me.Controls.Add(Me.GroupDatosEstudiante)
        Me.Controls.Add(Me.LblTituloModulo)
        Me.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmAgregarEstudiante"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Mantenimiento de Estudiantes"
        Me.GroupDatosEstudiante.ResumeLayout(False)
        Me.GroupDatosEstudiante.PerformLayout()
        Me.PanelAcciones.ResumeLayout(False)
        Me.GroupBusquedaEstudiante.ResumeLayout(False)
        Me.GroupBusquedaEstudiante.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents GroupDatosEstudiante As System.Windows.Forms.GroupBox
    Friend WithEvents PanelAcciones As System.Windows.Forms.Panel
    Friend WithEvents BtnRegresar As System.Windows.Forms.Button
    Friend WithEvents BtnCancelar As System.Windows.Forms.Button
    Friend WithEvents BtnGuardar As System.Windows.Forms.Button
    Friend WithEvents LblTituloModulo As System.Windows.Forms.Label
    Friend WithEvents GroupBusquedaEstudiante As System.Windows.Forms.GroupBox
    Friend WithEvents TxtCedula As System.Windows.Forms.TextBox
    Friend WithEvents Buscar As System.Windows.Forms.Button
    Friend WithEvents LblCedulaBusqueda As System.Windows.Forms.Label
    Friend WithEvents TxtApe1 As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents TxtApe2 As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents TxtNombre As System.Windows.Forms.TextBox
    Friend WithEvents LblNombre As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents TxtSeccion As TextBox
    Friend WithEvents Label10 As Label
    Friend WithEvents TxtTelefono As TextBox
    Friend WithEvents Label12 As Label
    Friend WithEvents TxtFecNac As DateTimePicker
    Friend WithEvents BtnEliminar As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents CBHorario As ComboBox
    Friend WithEvents CBEspecialidad As ComboBox
End Class
