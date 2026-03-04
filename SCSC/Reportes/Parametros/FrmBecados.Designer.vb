<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmBecados
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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Panel4 = New System.Windows.Forms.Panel()
        Me.BtnRegresar = New System.Windows.Forms.Button()
        Me.BtnCancelar = New System.Windows.Forms.Button()
        Me.BtnGuardar = New System.Windows.Forms.Button()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.RbPermisoSalida = New System.Windows.Forms.RadioButton()
        Me.RbBecaTransporte = New System.Windows.Forms.RadioButton()
        Me.RbBecaComedor = New System.Windows.Forms.RadioButton()
        Me.CbHorario = New System.Windows.Forms.ComboBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.RbDetallo = New System.Windows.Forms.RadioButton()
        Me.RbGeneral = New System.Windows.Forms.RadioButton()
        Me.Panel4.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Font = New System.Drawing.Font("Arial Narrow", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(204, 22)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(374, 35)
        Me.Label1.TabIndex = 21
        Me.Label1.Text = "Reporte Estudiantes Becados "
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Panel4
        '
        Me.Panel4.BackColor = System.Drawing.Color.Transparent
        Me.Panel4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.Panel4.Controls.Add(Me.BtnRegresar)
        Me.Panel4.Controls.Add(Me.BtnCancelar)
        Me.Panel4.Controls.Add(Me.BtnGuardar)
        Me.Panel4.Location = New System.Drawing.Point(436, 277)
        Me.Panel4.Margin = New System.Windows.Forms.Padding(4)
        Me.Panel4.Name = "Panel4"
        Me.Panel4.Size = New System.Drawing.Size(300, 81)
        Me.Panel4.TabIndex = 22
        '
        'BtnRegresar
        '
        Me.BtnRegresar.BackgroundImage = Global.SCSC.My.Resources.Resources.Regresar
        Me.BtnRegresar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnRegresar.Location = New System.Drawing.Point(191, 0)
        Me.BtnRegresar.Margin = New System.Windows.Forms.Padding(4)
        Me.BtnRegresar.Name = "BtnRegresar"
        Me.BtnRegresar.Size = New System.Drawing.Size(84, 78)
        Me.BtnRegresar.TabIndex = 7
        Me.BtnRegresar.UseVisualStyleBackColor = True
        '
        'BtnCancelar
        '
        Me.BtnCancelar.BackgroundImage = Global.SCSC.My.Resources.Resources.Cancelar
        Me.BtnCancelar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnCancelar.Location = New System.Drawing.Point(100, 0)
        Me.BtnCancelar.Margin = New System.Windows.Forms.Padding(4)
        Me.BtnCancelar.Name = "BtnCancelar"
        Me.BtnCancelar.Size = New System.Drawing.Size(84, 78)
        Me.BtnCancelar.TabIndex = 5
        Me.BtnCancelar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText
        Me.BtnCancelar.UseVisualStyleBackColor = True
        '
        'BtnGuardar
        '
        Me.BtnGuardar.BackgroundImage = Global.SCSC.My.Resources.Resources.Pantalla
        Me.BtnGuardar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnGuardar.Location = New System.Drawing.Point(9, 0)
        Me.BtnGuardar.Margin = New System.Windows.Forms.Padding(4)
        Me.BtnGuardar.Name = "BtnGuardar"
        Me.BtnGuardar.Size = New System.Drawing.Size(84, 78)
        Me.BtnGuardar.TabIndex = 0
        Me.BtnGuardar.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.RbGeneral)
        Me.GroupBox1.Controls.Add(Me.RbDetallo)
        Me.GroupBox1.Controls.Add(Me.Panel1)
        Me.GroupBox1.Controls.Add(Me.CbHorario)
        Me.GroupBox1.Controls.Add(Me.Label6)
        Me.GroupBox1.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox1.Location = New System.Drawing.Point(13, 76)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(723, 182)
        Me.GroupBox1.TabIndex = 23
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Parametros"
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.RbPermisoSalida)
        Me.Panel1.Controls.Add(Me.RbBecaTransporte)
        Me.Panel1.Controls.Add(Me.RbBecaComedor)
        Me.Panel1.Location = New System.Drawing.Point(23, 32)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(304, 120)
        Me.Panel1.TabIndex = 13
        '
        'RbPermisoSalida
        '
        Me.RbPermisoSalida.AutoSize = True
        Me.RbPermisoSalida.Location = New System.Drawing.Point(23, 80)
        Me.RbPermisoSalida.Name = "RbPermisoSalida"
        Me.RbPermisoSalida.Size = New System.Drawing.Size(259, 28)
        Me.RbPermisoSalida.TabIndex = 2
        Me.RbPermisoSalida.TabStop = True
        Me.RbPermisoSalida.Text = "Reporte Permisos de Salida"
        Me.RbPermisoSalida.UseVisualStyleBackColor = True
        '
        'RbBecaTransporte
        '
        Me.RbBecaTransporte.AutoSize = True
        Me.RbBecaTransporte.Location = New System.Drawing.Point(23, 46)
        Me.RbBecaTransporte.Name = "RbBecaTransporte"
        Me.RbBecaTransporte.Size = New System.Drawing.Size(246, 28)
        Me.RbBecaTransporte.TabIndex = 1
        Me.RbBecaTransporte.TabStop = True
        Me.RbBecaTransporte.Text = "Reporte Becas Transporte"
        Me.RbBecaTransporte.UseVisualStyleBackColor = True
        '
        'RbBecaComedor
        '
        Me.RbBecaComedor.AutoSize = True
        Me.RbBecaComedor.Location = New System.Drawing.Point(23, 12)
        Me.RbBecaComedor.Name = "RbBecaComedor"
        Me.RbBecaComedor.Size = New System.Drawing.Size(232, 28)
        Me.RbBecaComedor.TabIndex = 0
        Me.RbBecaComedor.TabStop = True
        Me.RbBecaComedor.Text = "Reporte Becas Comedor"
        Me.RbBecaComedor.UseVisualStyleBackColor = True
        '
        'CbHorario
        '
        Me.CbHorario.FormattingEnabled = True
        Me.CbHorario.Location = New System.Drawing.Point(500, 38)
        Me.CbHorario.Margin = New System.Windows.Forms.Padding(4)
        Me.CbHorario.Name = "CbHorario"
        Me.CbHorario.Size = New System.Drawing.Size(213, 32)
        Me.CbHorario.TabIndex = 12
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.Location = New System.Drawing.Point(409, 42)
        Me.Label6.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(83, 24)
        Me.Label6.TabIndex = 11
        Me.Label6.Text = "Horario: "
        '
        'RbDetallo
        '
        Me.RbDetallo.AutoSize = True
        Me.RbDetallo.Location = New System.Drawing.Point(408, 114)
        Me.RbDetallo.Margin = New System.Windows.Forms.Padding(4)
        Me.RbDetallo.Name = "RbDetallo"
        Me.RbDetallo.Size = New System.Drawing.Size(214, 28)
        Me.RbDetallo.TabIndex = 25
        Me.RbDetallo.Text = "Detallado por servicio"
        Me.RbDetallo.UseVisualStyleBackColor = True
        '
        'RbGeneral
        '
        Me.RbGeneral.AutoSize = True
        Me.RbGeneral.Checked = True
        Me.RbGeneral.Location = New System.Drawing.Point(408, 78)
        Me.RbGeneral.Margin = New System.Windows.Forms.Padding(4)
        Me.RbGeneral.Name = "RbGeneral"
        Me.RbGeneral.Size = New System.Drawing.Size(199, 28)
        Me.RbGeneral.TabIndex = 24
        Me.RbGeneral.TabStop = True
        Me.RbGeneral.Text = "Reporte por Sección"
        Me.RbGeneral.UseVisualStyleBackColor = True
        '
        'FrmBecados
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(749, 381)
        Me.ControlBox = False
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Panel4)
        Me.Controls.Add(Me.Label1)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "FrmBecados"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Reporte Proyeción Comedor"
        Me.Panel4.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents Panel4 As Panel
    Friend WithEvents BtnRegresar As Button
    Friend WithEvents BtnCancelar As Button
    Friend WithEvents BtnGuardar As Button
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents CbHorario As ComboBox
    Friend WithEvents Label6 As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents RbPermisoSalida As RadioButton
    Friend WithEvents RbBecaTransporte As RadioButton
    Friend WithEvents RbBecaComedor As RadioButton
    Friend WithEvents RbDetallo As RadioButton
    Friend WithEvents RbGeneral As RadioButton
End Class
