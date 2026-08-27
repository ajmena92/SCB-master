<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmProyeccionComedor
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
        Me.CbBeca = New System.Windows.Forms.ComboBox()
        Me.LblTipoBecaCaption = New System.Windows.Forms.Label()
        Me.LblFechaReporteCaption = New System.Windows.Forms.Label()
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
        Me.LblTituloReporte.Font = New System.Drawing.Font("Arial Narrow", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTituloReporte.ForeColor = System.Drawing.Color.Black
        Me.LblTituloReporte.Location = New System.Drawing.Point(128, 22)
        Me.LblTituloReporte.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LblTituloReporte.Name = "LblTituloReporte"
        Me.LblTituloReporte.Size = New System.Drawing.Size(484, 35)
        Me.LblTituloReporte.TabIndex = 21
        Me.LblTituloReporte.Text = "Reporte Proyeción Asistencia Comedor"
        Me.LblTituloReporte.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'PanelAcciones
        '
        Me.PanelAcciones.BackColor = System.Drawing.Color.Transparent
        Me.PanelAcciones.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PanelAcciones.Controls.Add(Me.BtnRegresar)
        Me.PanelAcciones.Controls.Add(Me.BtnCancelar)
        Me.PanelAcciones.Controls.Add(Me.BtnGuardar)
        Me.PanelAcciones.Location = New System.Drawing.Point(436, 277)
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
        Me.GroupParametros.Controls.Add(Me.CbBeca)
        Me.GroupParametros.Controls.Add(Me.LblTipoBecaCaption)
        Me.GroupParametros.Controls.Add(Me.LblFechaReporteCaption)
        Me.GroupParametros.Controls.Add(Me.FecIni)
        Me.GroupParametros.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupParametros.Location = New System.Drawing.Point(13, 76)
        Me.GroupParametros.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupParametros.Name = "GroupParametros"
        Me.GroupParametros.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupParametros.Size = New System.Drawing.Size(723, 182)
        Me.GroupParametros.TabIndex = 23
        Me.GroupParametros.TabStop = False
        Me.GroupParametros.Text = "Parametros"
        '
        'CbHorario
        '
        Me.CbHorario.FormattingEnabled = True
        Me.CbHorario.Location = New System.Drawing.Point(496, 67)
        Me.CbHorario.Margin = New System.Windows.Forms.Padding(4)
        Me.CbHorario.Name = "CbHorario"
        Me.CbHorario.Size = New System.Drawing.Size(213, 32)
        Me.CbHorario.TabIndex = 12
        '
        'LblHorarioCaption
        '
        Me.LblHorarioCaption.AutoSize = True
        Me.LblHorarioCaption.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblHorarioCaption.Location = New System.Drawing.Point(405, 71)
        Me.LblHorarioCaption.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LblHorarioCaption.Name = "LblHorarioCaption"
        Me.LblHorarioCaption.Size = New System.Drawing.Size(83, 24)
        Me.LblHorarioCaption.TabIndex = 11
        Me.LblHorarioCaption.Text = "Horario: "
        '
        'CbBeca
        '
        Me.CbBeca.FormattingEnabled = True
        Me.CbBeca.Location = New System.Drawing.Point(155, 129)
        Me.CbBeca.Margin = New System.Windows.Forms.Padding(4)
        Me.CbBeca.Name = "CbBeca"
        Me.CbBeca.Size = New System.Drawing.Size(175, 32)
        Me.CbBeca.TabIndex = 10
        '
        'LblTipoBecaCaption
        '
        Me.LblTipoBecaCaption.AutoSize = True
        Me.LblTipoBecaCaption.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTipoBecaCaption.Location = New System.Drawing.Point(46, 132)
        Me.LblTipoBecaCaption.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LblTipoBecaCaption.Name = "LblTipoBecaCaption"
        Me.LblTipoBecaCaption.Size = New System.Drawing.Size(101, 24)
        Me.LblTipoBecaCaption.TabIndex = 9
        Me.LblTipoBecaCaption.Text = "Tipo Beca: "
        '
        'LblFechaReporteCaption
        '
        Me.LblFechaReporteCaption.AutoSize = True
        Me.LblFechaReporteCaption.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblFechaReporteCaption.Location = New System.Drawing.Point(16, 72)
        Me.LblFechaReporteCaption.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LblFechaReporteCaption.Name = "LblFechaReporteCaption"
        Me.LblFechaReporteCaption.Size = New System.Drawing.Size(135, 24)
        Me.LblFechaReporteCaption.TabIndex = 4
        Me.LblFechaReporteCaption.Text = "Fecha Reporte:"
        '
        'FecIni
        '
        Me.FecIni.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.FecIni.Location = New System.Drawing.Point(159, 67)
        Me.FecIni.Margin = New System.Windows.Forms.Padding(4)
        Me.FecIni.Name = "FecIni"
        Me.FecIni.Size = New System.Drawing.Size(160, 32)
        Me.FecIni.TabIndex = 0
        '
        'FrmProyeccionComedor
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(749, 381)
        Me.ControlBox = False
        Me.Controls.Add(Me.GroupParametros)
        Me.Controls.Add(Me.PanelAcciones)
        Me.Controls.Add(Me.LblTituloReporte)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "FrmProyeccionComedor"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Reporte Proyeción Comedor"
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
    Friend WithEvents LblFechaReporteCaption As Label
    Friend WithEvents FecIni As DateTimePicker
    Friend WithEvents CbHorario As ComboBox
    Friend WithEvents LblHorarioCaption As Label
    Friend WithEvents CbBeca As ComboBox
    Friend WithEvents LblTipoBecaCaption As Label
End Class
