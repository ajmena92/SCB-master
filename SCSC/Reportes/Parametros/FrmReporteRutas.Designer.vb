<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmReporteRutas
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
        Me.LblTituloReporte = New System.Windows.Forms.Label()
        Me.PanelAcciones = New System.Windows.Forms.Panel()
        Me.BtnRegresar = New System.Windows.Forms.Button()
        Me.BtnCancelar = New System.Windows.Forms.Button()
        Me.BtnGuardar = New System.Windows.Forms.Button()
        Me.GroupParametros = New System.Windows.Forms.GroupBox()
        Me.CbHorario = New System.Windows.Forms.ComboBox()
        Me.LblHorarioCaption = New System.Windows.Forms.Label()
        Me.CbTipoUsuario = New System.Windows.Forms.ComboBox()
        Me.LblRutaCaption = New System.Windows.Forms.Label()
        Me.LblFechaFinalCaption = New System.Windows.Forms.Label()
        Me.LblFechaInicialCaption = New System.Windows.Forms.Label()
        Me.RbDetallo = New System.Windows.Forms.RadioButton()
        Me.RbGeneral = New System.Windows.Forms.RadioButton()
        Me.FecFinal = New System.Windows.Forms.DateTimePicker()
        Me.FecIni = New System.Windows.Forms.DateTimePicker()
        Me.PanelAcciones.SuspendLayout()
        Me.GroupParametros.SuspendLayout()
        Me.SuspendLayout()
        '
        'LblTituloReporte
        '
        Me.LblTituloReporte.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblTituloReporte.AutoSize = True
        Me.LblTituloReporte.BackColor = System.Drawing.Color.Transparent
        Me.LblTituloReporte.Font = New System.Drawing.Font("Calibri", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTituloReporte.ForeColor = System.Drawing.Color.Black
        Me.LblTituloReporte.Location = New System.Drawing.Point(121, 20)
        Me.LblTituloReporte.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LblTituloReporte.Name = "LblTituloReporte"
        Me.LblTituloReporte.Size = New System.Drawing.Size(533, 37)
        Me.LblTituloReporte.TabIndex = 21
        Me.LblTituloReporte.Text = "Reporte de Transporte por Rango de Fecha"
        Me.LblTituloReporte.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'PanelAcciones
        '
        Me.PanelAcciones.BackColor = System.Drawing.Color.Transparent
        Me.PanelAcciones.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PanelAcciones.Controls.Add(Me.BtnRegresar)
        Me.PanelAcciones.Controls.Add(Me.BtnCancelar)
        Me.PanelAcciones.Controls.Add(Me.BtnGuardar)
        Me.PanelAcciones.Location = New System.Drawing.Point(436, 330)
        Me.PanelAcciones.Margin = New System.Windows.Forms.Padding(4)
        Me.PanelAcciones.Name = "PanelAcciones"
        Me.PanelAcciones.Size = New System.Drawing.Size(300, 81)
        Me.PanelAcciones.TabIndex = 22
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
        'GroupParametros
        '
        Me.GroupParametros.Controls.Add(Me.CbHorario)
        Me.GroupParametros.Controls.Add(Me.LblHorarioCaption)
        Me.GroupParametros.Controls.Add(Me.CbTipoUsuario)
        Me.GroupParametros.Controls.Add(Me.LblRutaCaption)
        Me.GroupParametros.Controls.Add(Me.LblFechaFinalCaption)
        Me.GroupParametros.Controls.Add(Me.LblFechaInicialCaption)
        Me.GroupParametros.Controls.Add(Me.RbDetallo)
        Me.GroupParametros.Controls.Add(Me.RbGeneral)
        Me.GroupParametros.Controls.Add(Me.FecFinal)
        Me.GroupParametros.Controls.Add(Me.FecIni)
        Me.GroupParametros.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupParametros.Location = New System.Drawing.Point(28, 85)
        Me.GroupParametros.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupParametros.Name = "GroupParametros"
        Me.GroupParametros.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupParametros.Size = New System.Drawing.Size(708, 237)
        Me.GroupParametros.TabIndex = 23
        Me.GroupParametros.TabStop = False
        Me.GroupParametros.Text = "Parametros"
        '
        'CbHorario
        '
        Me.CbHorario.FormattingEnabled = True
        Me.CbHorario.Location = New System.Drawing.Point(147, 185)
        Me.CbHorario.Margin = New System.Windows.Forms.Padding(4)
        Me.CbHorario.Name = "CbHorario"
        Me.CbHorario.Size = New System.Drawing.Size(213, 32)
        Me.CbHorario.TabIndex = 14
        '
        'LblHorarioCaption
        '
        Me.LblHorarioCaption.AutoSize = True
        Me.LblHorarioCaption.Location = New System.Drawing.Point(56, 193)
        Me.LblHorarioCaption.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LblHorarioCaption.Name = "LblHorarioCaption"
        Me.LblHorarioCaption.Size = New System.Drawing.Size(83, 24)
        Me.LblHorarioCaption.TabIndex = 13
        Me.LblHorarioCaption.Text = "Horario: "
        '
        'CbTipoUsuario
        '
        Me.CbTipoUsuario.FormattingEnabled = True
        Me.CbTipoUsuario.Location = New System.Drawing.Point(147, 134)
        Me.CbTipoUsuario.Margin = New System.Windows.Forms.Padding(4)
        Me.CbTipoUsuario.Name = "CbTipoUsuario"
        Me.CbTipoUsuario.Size = New System.Drawing.Size(160, 32)
        Me.CbTipoUsuario.TabIndex = 8
        '
        'LblRutaCaption
        '
        Me.LblRutaCaption.AutoSize = True
        Me.LblRutaCaption.Location = New System.Drawing.Point(80, 142)
        Me.LblRutaCaption.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LblRutaCaption.Name = "LblRutaCaption"
        Me.LblRutaCaption.Size = New System.Drawing.Size(59, 24)
        Me.LblRutaCaption.TabIndex = 7
        Me.LblRutaCaption.Text = "Ruta: "
        '
        'LblFechaFinalCaption
        '
        Me.LblFechaFinalCaption.AutoSize = True
        Me.LblFechaFinalCaption.Location = New System.Drawing.Point(335, 72)
        Me.LblFechaFinalCaption.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LblFechaFinalCaption.Name = "LblFechaFinalCaption"
        Me.LblFechaFinalCaption.Size = New System.Drawing.Size(108, 24)
        Me.LblFechaFinalCaption.TabIndex = 5
        Me.LblFechaFinalCaption.Text = "Fecha Final:"
        '
        'LblFechaInicialCaption
        '
        Me.LblFechaInicialCaption.AutoSize = True
        Me.LblFechaInicialCaption.Location = New System.Drawing.Point(22, 74)
        Me.LblFechaInicialCaption.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LblFechaInicialCaption.Name = "LblFechaInicialCaption"
        Me.LblFechaInicialCaption.Size = New System.Drawing.Size(117, 24)
        Me.LblFechaInicialCaption.TabIndex = 4
        Me.LblFechaInicialCaption.Text = "Fecha Inicial:"
        '
        'RbDetallo
        '
        Me.RbDetallo.AutoSize = True
        Me.RbDetallo.Location = New System.Drawing.Point(451, 172)
        Me.RbDetallo.Margin = New System.Windows.Forms.Padding(4)
        Me.RbDetallo.Name = "RbDetallo"
        Me.RbDetallo.Size = New System.Drawing.Size(112, 28)
        Me.RbDetallo.TabIndex = 3
        Me.RbDetallo.Text = "Detallado"
        Me.RbDetallo.UseVisualStyleBackColor = True
        '
        'RbGeneral
        '
        Me.RbGeneral.AutoSize = True
        Me.RbGeneral.Checked = True
        Me.RbGeneral.Location = New System.Drawing.Point(451, 135)
        Me.RbGeneral.Margin = New System.Windows.Forms.Padding(4)
        Me.RbGeneral.Name = "RbGeneral"
        Me.RbGeneral.Size = New System.Drawing.Size(97, 28)
        Me.RbGeneral.TabIndex = 2
        Me.RbGeneral.TabStop = True
        Me.RbGeneral.Text = "General"
        Me.RbGeneral.UseVisualStyleBackColor = True
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
        Me.FecIni.Location = New System.Drawing.Point(147, 66)
        Me.FecIni.Margin = New System.Windows.Forms.Padding(4)
        Me.FecIni.Name = "FecIni"
        Me.FecIni.Size = New System.Drawing.Size(160, 32)
        Me.FecIni.TabIndex = 0
        '
        'FrmReporteRutas
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(749, 446)
        Me.ControlBox = False
        Me.Controls.Add(Me.GroupParametros)
        Me.Controls.Add(Me.PanelAcciones)
        Me.Controls.Add(Me.LblTituloReporte)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "FrmReporteRutas"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Reporte de Transporte por Rango de Fecha"
        Me.PanelAcciones.ResumeLayout(False)
        Me.GroupParametros.ResumeLayout(False)
        Me.GroupParametros.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents LblTituloReporte As Label
    Friend WithEvents PanelAcciones As Panel
    Friend WithEvents BtnRegresar As Button
    Friend WithEvents BtnCancelar As Button
    Friend WithEvents BtnGuardar As Button
    Friend WithEvents GroupParametros As GroupBox
    Friend WithEvents LblFechaFinalCaption As Label
    Friend WithEvents LblFechaInicialCaption As Label
    Friend WithEvents RbDetallo As RadioButton
    Friend WithEvents RbGeneral As RadioButton
    Friend WithEvents FecFinal As DateTimePicker
    Friend WithEvents FecIni As DateTimePicker
    Friend WithEvents LblRutaCaption As Label
    Friend WithEvents CbTipoUsuario As ComboBox
    Friend WithEvents CbHorario As ComboBox
    Friend WithEvents LblHorarioCaption As Label
End Class
