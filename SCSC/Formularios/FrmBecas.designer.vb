<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmBecas
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmBecas))
        Me.TxtDescripcion = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.CkActivo = New System.Windows.Forms.CheckBox()
        Me.Buscar = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtCodBeca = New System.Windows.Forms.TextBox()
        Me.Panel4 = New System.Windows.Forms.Panel()
        Me.BtnEliminar = New System.Windows.Forms.Button()
        Me.BtnRegresar = New System.Windows.Forms.Button()
        Me.BtnCancelar = New System.Windows.Forms.Button()
        Me.BtnGuardar = New System.Windows.Forms.Button()
        Me.GpDiasBeca = New System.Windows.Forms.GroupBox()
        Me.Ck6 = New System.Windows.Forms.CheckBox()
        Me.Ck5 = New System.Windows.Forms.CheckBox()
        Me.Ck4 = New System.Windows.Forms.CheckBox()
        Me.Ck3 = New System.Windows.Forms.CheckBox()
        Me.Ck2 = New System.Windows.Forms.CheckBox()
        Me.GroupBox2.SuspendLayout()
        Me.Panel4.SuspendLayout()
        Me.GpDiasBeca.SuspendLayout()
        Me.SuspendLayout()
        '
        'TxtDescripcion
        '
        Me.TxtDescripcion.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtDescripcion.Font = New System.Drawing.Font("Calibri", 13.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtDescripcion.Location = New System.Drawing.Point(160, 113)
        Me.TxtDescripcion.MaxLength = 50
        Me.TxtDescripcion.Name = "TxtDescripcion"
        Me.TxtDescripcion.Size = New System.Drawing.Size(422, 36)
        Me.TxtDescripcion.TabIndex = 41
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label3.Font = New System.Drawing.Font("Arial Narrow", 13.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(28, 113)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(126, 29)
        Me.Label3.TabIndex = 42
        Me.Label3.Text = "Descripción: "
        '
        'Label1
        '
        Me.Label1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 21.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(233, 14)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(256, 42)
        Me.Label1.TabIndex = 20
        Me.Label1.Text = "Modulo Becas"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'GroupBox2
        '
        Me.GroupBox2.BackColor = System.Drawing.Color.Transparent
        Me.GroupBox2.Controls.Add(Me.CkActivo)
        Me.GroupBox2.Controls.Add(Me.Buscar)
        Me.GroupBox2.Controls.Add(Me.Label4)
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Controls.Add(Me.TxtDescripcion)
        Me.GroupBox2.Location = New System.Drawing.Point(31, 59)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(598, 242)
        Me.GroupBox2.TabIndex = 22
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Datos de Beca"
        '
        'CkActivo
        '
        Me.CkActivo.Checked = True
        Me.CkActivo.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CkActivo.Font = New System.Drawing.Font("Arial Narrow", 13.8!, System.Drawing.FontStyle.Bold)
        Me.CkActivo.Location = New System.Drawing.Point(160, 180)
        Me.CkActivo.Name = "CkActivo"
        Me.CkActivo.Size = New System.Drawing.Size(133, 33)
        Me.CkActivo.TabIndex = 43
        Me.CkActivo.Text = "Activa"
        Me.CkActivo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.CkActivo.UseVisualStyleBackColor = True
        '
        'Buscar
        '
        Me.Buscar.AccessibleRole = System.Windows.Forms.AccessibleRole.ButtonMenu
        Me.Buscar.BackColor = System.Drawing.Color.Transparent
        Me.Buscar.BackgroundImage = Global.SCSC.My.Resources.Resources.Buscar_30x30
        Me.Buscar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.Buscar.FlatAppearance.BorderColor = System.Drawing.Color.WhiteSmoke
        Me.Buscar.FlatAppearance.BorderSize = 0
        Me.Buscar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.Buscar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.Buscar.ForeColor = System.Drawing.Color.White
        Me.Buscar.Location = New System.Drawing.Point(242, 53)
        Me.Buscar.Name = "Buscar"
        Me.Buscar.Size = New System.Drawing.Size(43, 46)
        Me.Buscar.TabIndex = 41
        Me.Buscar.TextAlign = System.Drawing.ContentAlignment.BottomCenter
        Me.Buscar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.Buscar.UseVisualStyleBackColor = False
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.BackColor = System.Drawing.Color.Transparent
        Me.Label4.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label4.Font = New System.Drawing.Font("Arial Narrow", 13.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.Location = New System.Drawing.Point(28, 58)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(216, 29)
        Me.Label4.TabIndex = 40
        Me.Label4.Text = "Buscar Beca Comedor:"
        '
        'txtCodBeca
        '
        Me.txtCodBeca.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.txtCodBeca.Font = New System.Drawing.Font("Calibri", 13.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtCodBeca.Location = New System.Drawing.Point(495, 17)
        Me.txtCodBeca.MaxLength = 14
        Me.txtCodBeca.Name = "txtCodBeca"
        Me.txtCodBeca.ReadOnly = True
        Me.txtCodBeca.Size = New System.Drawing.Size(149, 36)
        Me.txtCodBeca.TabIndex = 39
        Me.txtCodBeca.Visible = False
        '
        'Panel4
        '
        Me.Panel4.BackColor = System.Drawing.Color.Transparent
        Me.Panel4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.Panel4.Controls.Add(Me.BtnEliminar)
        Me.Panel4.Controls.Add(Me.BtnRegresar)
        Me.Panel4.Controls.Add(Me.BtnCancelar)
        Me.Panel4.Controls.Add(Me.BtnGuardar)
        Me.Panel4.Location = New System.Drawing.Point(372, 451)
        Me.Panel4.Name = "Panel4"
        Me.Panel4.Size = New System.Drawing.Size(257, 72)
        Me.Panel4.TabIndex = 23
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
        'GpDiasBeca
        '
        Me.GpDiasBeca.BackColor = System.Drawing.Color.Transparent
        Me.GpDiasBeca.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.GpDiasBeca.Controls.Add(Me.Ck6)
        Me.GpDiasBeca.Controls.Add(Me.Ck5)
        Me.GpDiasBeca.Controls.Add(Me.Ck4)
        Me.GpDiasBeca.Controls.Add(Me.Ck3)
        Me.GpDiasBeca.Controls.Add(Me.Ck2)
        Me.GpDiasBeca.Location = New System.Drawing.Point(31, 320)
        Me.GpDiasBeca.Name = "GpDiasBeca"
        Me.GpDiasBeca.Size = New System.Drawing.Size(601, 114)
        Me.GpDiasBeca.TabIndex = 24
        Me.GpDiasBeca.TabStop = False
        Me.GpDiasBeca.Text = "Días Becados"
        '
        'Ck6
        '
        Me.Ck6.Checked = True
        Me.Ck6.CheckState = System.Windows.Forms.CheckState.Checked
        Me.Ck6.Font = New System.Drawing.Font("Arial Narrow", 13.8!)
        Me.Ck6.Location = New System.Drawing.Point(463, 49)
        Me.Ck6.Name = "Ck6"
        Me.Ck6.Size = New System.Drawing.Size(133, 33)
        Me.Ck6.TabIndex = 49
        Me.Ck6.Tag = "6"
        Me.Ck6.Text = "Viernes"
        Me.Ck6.UseVisualStyleBackColor = True
        '
        'Ck5
        '
        Me.Ck5.Checked = True
        Me.Ck5.CheckState = System.Windows.Forms.CheckState.Checked
        Me.Ck5.Font = New System.Drawing.Font("Arial Narrow", 13.8!)
        Me.Ck5.Location = New System.Drawing.Point(356, 49)
        Me.Ck5.Name = "Ck5"
        Me.Ck5.Size = New System.Drawing.Size(133, 33)
        Me.Ck5.TabIndex = 48
        Me.Ck5.Tag = "5"
        Me.Ck5.Text = "Jueves"
        Me.Ck5.UseVisualStyleBackColor = True
        '
        'Ck4
        '
        Me.Ck4.Checked = True
        Me.Ck4.CheckState = System.Windows.Forms.CheckState.Checked
        Me.Ck4.Font = New System.Drawing.Font("Arial Narrow", 13.8!)
        Me.Ck4.Location = New System.Drawing.Point(226, 49)
        Me.Ck4.Name = "Ck4"
        Me.Ck4.Size = New System.Drawing.Size(133, 33)
        Me.Ck4.TabIndex = 46
        Me.Ck4.Tag = "4"
        Me.Ck4.Text = "Miercoles"
        Me.Ck4.UseVisualStyleBackColor = True
        '
        'Ck3
        '
        Me.Ck3.Checked = True
        Me.Ck3.CheckState = System.Windows.Forms.CheckState.Checked
        Me.Ck3.Font = New System.Drawing.Font("Arial Narrow", 13.8!)
        Me.Ck3.Location = New System.Drawing.Point(115, 49)
        Me.Ck3.Name = "Ck3"
        Me.Ck3.Size = New System.Drawing.Size(133, 33)
        Me.Ck3.TabIndex = 45
        Me.Ck3.Tag = "3"
        Me.Ck3.Text = "Martes"
        Me.Ck3.UseVisualStyleBackColor = True
        '
        'Ck2
        '
        Me.Ck2.Checked = True
        Me.Ck2.CheckState = System.Windows.Forms.CheckState.Checked
        Me.Ck2.Font = New System.Drawing.Font("Arial Narrow", 13.8!)
        Me.Ck2.Location = New System.Drawing.Point(15, 49)
        Me.Ck2.Name = "Ck2"
        Me.Ck2.Size = New System.Drawing.Size(133, 33)
        Me.Ck2.TabIndex = 44
        Me.Ck2.Tag = "2"
        Me.Ck2.Text = "Lunes"
        Me.Ck2.UseVisualStyleBackColor = True
        '
        'FrmBecas
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(10.0!, 24.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(667, 539)
        Me.Controls.Add(Me.GpDiasBeca)
        Me.Controls.Add(Me.txtCodBeca)
        Me.Controls.Add(Me.Panel4)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.Label1)
        Me.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmBecas"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Mantenimiento de estudiantes "
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.Panel4.ResumeLayout(False)
        Me.GpDiasBeca.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents txtCodBeca As System.Windows.Forms.TextBox
    Friend WithEvents Buscar As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents TxtDescripcion As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents CkActivo As CheckBox
    Friend WithEvents Panel4 As Panel
    Friend WithEvents BtnEliminar As Button
    Friend WithEvents BtnRegresar As Button
    Friend WithEvents BtnCancelar As Button
    Friend WithEvents BtnGuardar As Button
    Friend WithEvents GpDiasBeca As GroupBox
    Friend WithEvents Ck6 As CheckBox
    Friend WithEvents Ck5 As CheckBox
    Friend WithEvents Ck4 As CheckBox
    Friend WithEvents Ck3 As CheckBox
    Friend WithEvents Ck2 As CheckBox
End Class
