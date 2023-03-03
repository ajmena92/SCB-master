<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmReporteComedor

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
        Me.CbHorario = New System.Windows.Forms.ComboBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.CbBeca = New System.Windows.Forms.ComboBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.CbTipoUsuario = New System.Windows.Forms.ComboBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.RbDetallo = New System.Windows.Forms.RadioButton()
        Me.RbGeneral = New System.Windows.Forms.RadioButton()
        Me.FecFinal = New System.Windows.Forms.DateTimePicker()
        Me.FecIni = New System.Windows.Forms.DateTimePicker()
        Me.Panel4.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
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
        Me.Label1.Location = New System.Drawing.Point(128, 22)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(507, 35)
        Me.Label1.TabIndex = 21
        Me.Label1.Text = "Reporte de Comedor por Rango de Fecha"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Panel4
        '
        Me.Panel4.BackColor = System.Drawing.Color.Transparent
        Me.Panel4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.Panel4.Controls.Add(Me.BtnRegresar)
        Me.Panel4.Controls.Add(Me.BtnCancelar)
        Me.Panel4.Controls.Add(Me.BtnGuardar)
        Me.Panel4.Location = New System.Drawing.Point(436, 360)
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
        Me.GroupBox1.Controls.Add(Me.CbHorario)
        Me.GroupBox1.Controls.Add(Me.Label6)
        Me.GroupBox1.Controls.Add(Me.CbBeca)
        Me.GroupBox1.Controls.Add(Me.Label5)
        Me.GroupBox1.Controls.Add(Me.CbTipoUsuario)
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.RbDetallo)
        Me.GroupBox1.Controls.Add(Me.RbGeneral)
        Me.GroupBox1.Controls.Add(Me.FecFinal)
        Me.GroupBox1.Controls.Add(Me.FecIni)
        Me.GroupBox1.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox1.Location = New System.Drawing.Point(13, 76)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(723, 276)
        Me.GroupBox1.TabIndex = 23
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Parametros"
        '
        'CbHorario
        '
        Me.CbHorario.FormattingEnabled = True
        Me.CbHorario.Location = New System.Drawing.Point(147, 195)
        Me.CbHorario.Margin = New System.Windows.Forms.Padding(4)
        Me.CbHorario.Name = "CbHorario"
        Me.CbHorario.Size = New System.Drawing.Size(213, 32)
        Me.CbHorario.TabIndex = 12
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.Location = New System.Drawing.Point(56, 199)
        Me.Label6.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(83, 24)
        Me.Label6.TabIndex = 11
        Me.Label6.Text = "Horario: "
        '
        'CbBeca
        '
        Me.CbBeca.FormattingEnabled = True
        Me.CbBeca.Location = New System.Drawing.Point(451, 134)
        Me.CbBeca.Margin = New System.Windows.Forms.Padding(4)
        Me.CbBeca.Name = "CbBeca"
        Me.CbBeca.Size = New System.Drawing.Size(175, 32)
        Me.CbBeca.TabIndex = 10
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.Location = New System.Drawing.Point(342, 140)
        Me.Label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(101, 24)
        Me.Label5.TabIndex = 9
        Me.Label5.Text = "Tipo Beca: "
        '
        'CbTipoUsuario
        '
        Me.CbTipoUsuario.FormattingEnabled = True
        Me.CbTipoUsuario.Location = New System.Drawing.Point(147, 132)
        Me.CbTipoUsuario.Margin = New System.Windows.Forms.Padding(4)
        Me.CbTipoUsuario.Name = "CbTipoUsuario"
        Me.CbTipoUsuario.Size = New System.Drawing.Size(160, 32)
        Me.CbTipoUsuario.TabIndex = 8
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.Location = New System.Drawing.Point(12, 137)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(127, 24)
        Me.Label4.TabIndex = 7
        Me.Label4.Text = "Tipo Usuario: "
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(335, 72)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(108, 24)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Fecha Final:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(22, 72)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(117, 24)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Fecha Inicial:"
        '
        'RbDetallo
        '
        Me.RbDetallo.AutoSize = True
        Me.RbDetallo.Checked = True
        Me.RbDetallo.Location = New System.Drawing.Point(527, 199)
        Me.RbDetallo.Margin = New System.Windows.Forms.Padding(4)
        Me.RbDetallo.Name = "RbDetallo"
        Me.RbDetallo.Size = New System.Drawing.Size(112, 28)
        Me.RbDetallo.TabIndex = 3
        Me.RbDetallo.TabStop = True
        Me.RbDetallo.Text = "Detallado"
        Me.RbDetallo.UseVisualStyleBackColor = True
        Me.RbDetallo.Visible = False
        '
        'RbGeneral
        '
        Me.RbGeneral.AutoSize = True
        Me.RbGeneral.Location = New System.Drawing.Point(527, 235)
        Me.RbGeneral.Margin = New System.Windows.Forms.Padding(4)
        Me.RbGeneral.Name = "RbGeneral"
        Me.RbGeneral.Size = New System.Drawing.Size(97, 28)
        Me.RbGeneral.TabIndex = 2
        Me.RbGeneral.Text = "General"
        Me.RbGeneral.UseVisualStyleBackColor = True
        Me.RbGeneral.Visible = False
        '
        'FecFinal
        '
        Me.FecFinal.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.FecFinal.Location = New System.Drawing.Point(451, 66)
        Me.FecFinal.Margin = New System.Windows.Forms.Padding(4)
        Me.FecFinal.Name = "FecFinal"
        Me.FecFinal.Size = New System.Drawing.Size(175, 32)
        Me.FecFinal.TabIndex = 1
        '
        'FecIni
        '
        Me.FecIni.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.FecIni.Location = New System.Drawing.Point(147, 67)
        Me.FecIni.Margin = New System.Windows.Forms.Padding(4)
        Me.FecIni.Name = "FecIni"
        Me.FecIni.Size = New System.Drawing.Size(160, 32)
        Me.FecIni.TabIndex = 0
        '
        'FrmReporteComedor
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(749, 446)
        Me.ControlBox = False
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Panel4)
        Me.Controls.Add(Me.Label1)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "FrmReporteComedor"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Reporte de Marcas por Rango de Fecha"
        Me.Panel4.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents Panel4 As Panel
    Friend WithEvents BtnRegresar As Button
    Friend WithEvents BtnCancelar As Button
    Friend WithEvents BtnGuardar As Button
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents RbDetallo As RadioButton
    Friend WithEvents RbGeneral As RadioButton
    Friend WithEvents FecFinal As DateTimePicker
    Friend WithEvents FecIni As DateTimePicker
    Friend WithEvents Label4 As Label
    Friend WithEvents CbTipoUsuario As ComboBox
    Friend WithEvents CbHorario As ComboBox
    Friend WithEvents Label6 As Label
    Friend WithEvents CbBeca As ComboBox
    Friend WithEvents Label5 As Label
End Class
