<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmRutas
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmRutas))
        Me.TxtDescripcion = New System.Windows.Forms.TextBox()
        Me.LblDescripcionRuta = New System.Windows.Forms.Label()
        Me.PanelAcciones = New System.Windows.Forms.Panel()
        Me.BtnEliminar = New System.Windows.Forms.Button()
        Me.BtnRegresar = New System.Windows.Forms.Button()
        Me.BtnCancelar = New System.Windows.Forms.Button()
        Me.BtnGuardar = New System.Windows.Forms.Button()
        Me.LblTituloModulo = New System.Windows.Forms.Label()
        Me.GroupDatosRuta = New System.Windows.Forms.GroupBox()
        Me.CkActivo = New System.Windows.Forms.CheckBox()
        Me.txtCodRuta = New System.Windows.Forms.TextBox()
        Me.Buscar = New System.Windows.Forms.Button()
        Me.LblBuscarRuta = New System.Windows.Forms.Label()
        Me.PanelAcciones.SuspendLayout()
        Me.GroupDatosRuta.SuspendLayout()
        Me.SuspendLayout()
        '
        'TxtDescripcion
        '
        Me.TxtDescripcion.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtDescripcion.Font = New System.Drawing.Font("Calibri", 13.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtDescripcion.Location = New System.Drawing.Point(160, 99)
        Me.TxtDescripcion.MaxLength = 100
        Me.TxtDescripcion.Multiline = True
        Me.TxtDescripcion.Name = "TxtDescripcion"
        Me.TxtDescripcion.Size = New System.Drawing.Size(301, 68)
        Me.TxtDescripcion.TabIndex = 41
        '
        'LblDescripcionRuta
        '
        Me.LblDescripcionRuta.AutoSize = True
        Me.LblDescripcionRuta.BackColor = System.Drawing.Color.Transparent
        Me.LblDescripcionRuta.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.LblDescripcionRuta.Font = New System.Drawing.Font("Arial Narrow", 13.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblDescripcionRuta.Location = New System.Drawing.Point(28, 113)
        Me.LblDescripcionRuta.Name = "LblDescripcionRuta"
        Me.LblDescripcionRuta.Size = New System.Drawing.Size(126, 29)
        Me.LblDescripcionRuta.TabIndex = 42
        Me.LblDescripcionRuta.Text = "Descripción: "
        '
        'PanelAcciones
        '
        Me.PanelAcciones.BackColor = System.Drawing.Color.Transparent
        Me.PanelAcciones.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PanelAcciones.Controls.Add(Me.BtnEliminar)
        Me.PanelAcciones.Controls.Add(Me.BtnRegresar)
        Me.PanelAcciones.Controls.Add(Me.BtnCancelar)
        Me.PanelAcciones.Controls.Add(Me.BtnGuardar)
        Me.PanelAcciones.Location = New System.Drawing.Point(288, 316)
        Me.PanelAcciones.Name = "PanelAcciones"
        Me.PanelAcciones.Size = New System.Drawing.Size(257, 72)
        Me.PanelAcciones.TabIndex = 19
        '
        'BtnEliminar
        '
        Me.BtnEliminar.BackgroundImage = Global.SCSC.My.Resources.Resources.Eliminar
        Me.BtnEliminar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnEliminar.Location = New System.Drawing.Point(66, 0)
        Me.BtnEliminar.Name = "BtnEliminar"
        Me.BtnEliminar.Size = New System.Drawing.Size(62, 63)
        Me.BtnEliminar.TabIndex = 9
        Me.BtnEliminar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText
        Me.BtnEliminar.UseVisualStyleBackColor = True
        '
        'BtnRegresar
        '
        Me.BtnRegresar.BackgroundImage = Global.SCSC.My.Resources.Resources.Regresar
        Me.BtnRegresar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnRegresar.Location = New System.Drawing.Point(190, 0)
        Me.BtnRegresar.Name = "BtnRegresar"
        Me.BtnRegresar.Size = New System.Drawing.Size(62, 63)
        Me.BtnRegresar.TabIndex = 8
        Me.BtnRegresar.UseVisualStyleBackColor = True
        '
        'BtnCancelar
        '
        Me.BtnCancelar.BackgroundImage = Global.SCSC.My.Resources.Resources.Cancelar
        Me.BtnCancelar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnCancelar.Location = New System.Drawing.Point(128, 0)
        Me.BtnCancelar.Name = "BtnCancelar"
        Me.BtnCancelar.Size = New System.Drawing.Size(62, 63)
        Me.BtnCancelar.TabIndex = 5
        Me.BtnCancelar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText
        Me.BtnCancelar.UseVisualStyleBackColor = True
        '
        'BtnGuardar
        '
        Me.BtnGuardar.BackgroundImage = Global.SCSC.My.Resources.Resources.Aceptar
        Me.BtnGuardar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnGuardar.Location = New System.Drawing.Point(3, 0)
        Me.BtnGuardar.Name = "BtnGuardar"
        Me.BtnGuardar.Size = New System.Drawing.Size(62, 63)
        Me.BtnGuardar.TabIndex = 0
        Me.BtnGuardar.UseVisualStyleBackColor = True
        '
        'LblTituloModulo
        '
        Me.LblTituloModulo.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblTituloModulo.AutoSize = True
        Me.LblTituloModulo.BackColor = System.Drawing.Color.Transparent
        Me.LblTituloModulo.Font = New System.Drawing.Font("Microsoft Sans Serif", 25.0!, System.Drawing.FontStyle.Bold)
        Me.LblTituloModulo.ForeColor = System.Drawing.Color.Black
        Me.LblTituloModulo.Location = New System.Drawing.Point(157, 9)
        Me.LblTituloModulo.Name = "LblTituloModulo"
        Me.LblTituloModulo.Size = New System.Drawing.Size(289, 48)
        Me.LblTituloModulo.TabIndex = 20
        Me.LblTituloModulo.Text = "Modulo Rutas"
        Me.LblTituloModulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'GroupDatosRuta
        '
        Me.GroupDatosRuta.BackColor = System.Drawing.Color.Transparent
        Me.GroupDatosRuta.Controls.Add(Me.CkActivo)
        Me.GroupDatosRuta.Controls.Add(Me.txtCodRuta)
        Me.GroupDatosRuta.Controls.Add(Me.Buscar)
        Me.GroupDatosRuta.Controls.Add(Me.LblBuscarRuta)
        Me.GroupDatosRuta.Controls.Add(Me.LblDescripcionRuta)
        Me.GroupDatosRuta.Controls.Add(Me.TxtDescripcion)
        Me.GroupDatosRuta.Font = New System.Drawing.Font("Arial Narrow", 12.0!, System.Drawing.FontStyle.Bold)
        Me.GroupDatosRuta.Location = New System.Drawing.Point(48, 68)
        Me.GroupDatosRuta.Name = "GroupDatosRuta"
        Me.GroupDatosRuta.Size = New System.Drawing.Size(492, 242)
        Me.GroupDatosRuta.TabIndex = 22
        Me.GroupDatosRuta.TabStop = False
        Me.GroupDatosRuta.Text = "Datos de Rutas"
        '
        'CkActivo
        '
        Me.CkActivo.AutoSize = True
        Me.CkActivo.Checked = True
        Me.CkActivo.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CkActivo.Font = New System.Drawing.Font("Arial Narrow", 13.8!)
        Me.CkActivo.Location = New System.Drawing.Point(160, 173)
        Me.CkActivo.Name = "CkActivo"
        Me.CkActivo.Size = New System.Drawing.Size(133, 33)
        Me.CkActivo.TabIndex = 43
        Me.CkActivo.Text = "Ruta Activa"
        Me.CkActivo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.CkActivo.UseVisualStyleBackColor = True
        '
        'txtCodRuta
        '
        Me.txtCodRuta.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.txtCodRuta.Font = New System.Drawing.Font("Calibri", 13.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtCodRuta.Location = New System.Drawing.Point(160, 43)
        Me.txtCodRuta.MaxLength = 10
        Me.txtCodRuta.Name = "txtCodRuta"
        Me.txtCodRuta.Size = New System.Drawing.Size(149, 36)
        Me.txtCodRuta.TabIndex = 39
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
        Me.Buscar.Location = New System.Drawing.Point(327, 48)
        Me.Buscar.Name = "Buscar"
        Me.Buscar.Size = New System.Drawing.Size(27, 27)
        Me.Buscar.TabIndex = 41
        Me.Buscar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.Buscar.UseVisualStyleBackColor = False
        '
        'LblBuscarRuta
        '
        Me.LblBuscarRuta.AutoSize = True
        Me.LblBuscarRuta.BackColor = System.Drawing.Color.Transparent
        Me.LblBuscarRuta.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.LblBuscarRuta.Font = New System.Drawing.Font("Arial Narrow", 13.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblBuscarRuta.Location = New System.Drawing.Point(74, 50)
        Me.LblBuscarRuta.Name = "LblBuscarRuta"
        Me.LblBuscarRuta.Size = New System.Drawing.Size(80, 29)
        Me.LblBuscarRuta.TabIndex = 40
        Me.LblBuscarRuta.Text = "Código:"
        '
        'FrmRutas
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(10.0!, 24.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(582, 409)
        Me.Controls.Add(Me.GroupDatosRuta)
        Me.Controls.Add(Me.PanelAcciones)
        Me.Controls.Add(Me.LblTituloModulo)
        Me.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmRutas"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Mantenimiento de estudiantes "
        Me.PanelAcciones.ResumeLayout(False)
        Me.GroupDatosRuta.ResumeLayout(False)
        Me.GroupDatosRuta.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents PanelAcciones As System.Windows.Forms.Panel
    Friend WithEvents BtnCancelar As System.Windows.Forms.Button
    Friend WithEvents BtnGuardar As System.Windows.Forms.Button
    Friend WithEvents LblTituloModulo As System.Windows.Forms.Label
    Friend WithEvents GroupDatosRuta As System.Windows.Forms.GroupBox
    Friend WithEvents txtCodRuta As System.Windows.Forms.TextBox
    Friend WithEvents Buscar As System.Windows.Forms.Button
    Friend WithEvents LblBuscarRuta As System.Windows.Forms.Label
    Friend WithEvents TxtDescripcion As System.Windows.Forms.TextBox
    Friend WithEvents LblDescripcionRuta As System.Windows.Forms.Label
    Friend WithEvents BtnRegresar As System.Windows.Forms.Button
    Friend WithEvents CkActivo As CheckBox
    Friend WithEvents BtnEliminar As Button
End Class
