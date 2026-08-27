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
        Me.LblTituloReporte = New System.Windows.Forms.Label()
        Me.PanelAcciones = New System.Windows.Forms.Panel()
        Me.BtnRegresar = New System.Windows.Forms.Button()
        Me.BtnCancelar = New System.Windows.Forms.Button()
        Me.BtnGuardar = New System.Windows.Forms.Button()
        Me.GroupParametros = New System.Windows.Forms.GroupBox()
        Me.RbGeneral = New System.Windows.Forms.RadioButton()
        Me.RbDetallo = New System.Windows.Forms.RadioButton()
        Me.PanelTiposReporte = New System.Windows.Forms.Panel()
        Me.RbPermisoSalida = New System.Windows.Forms.RadioButton()
        Me.RbBecaTransporte = New System.Windows.Forms.RadioButton()
        Me.RbBecaComedor = New System.Windows.Forms.RadioButton()
        Me.CbHorario = New System.Windows.Forms.ComboBox()
        Me.LblHorarioCaption = New System.Windows.Forms.Label()
        Me.PanelAcciones.SuspendLayout()
        Me.GroupParametros.SuspendLayout()
        Me.PanelTiposReporte.SuspendLayout()
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
        Me.LblTituloReporte.Location = New System.Drawing.Point(153, 18)
        Me.LblTituloReporte.Name = "LblTituloReporte"
        Me.LblTituloReporte.Size = New System.Drawing.Size(374, 35)
        Me.LblTituloReporte.TabIndex = 21
        Me.LblTituloReporte.Text = "Reporte Estudiantes Becados "
        Me.LblTituloReporte.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'PanelAcciones
        '
        Me.PanelAcciones.BackColor = System.Drawing.Color.Transparent
        Me.PanelAcciones.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PanelAcciones.Controls.Add(Me.BtnRegresar)
        Me.PanelAcciones.Controls.Add(Me.BtnCancelar)
        Me.PanelAcciones.Controls.Add(Me.BtnGuardar)
        Me.PanelAcciones.Location = New System.Drawing.Point(294, 225)
        Me.PanelAcciones.Name = "PanelAcciones"
        Me.PanelAcciones.Size = New System.Drawing.Size(258, 66)
        Me.PanelAcciones.TabIndex = 22
        '
        'BtnRegresar
        '
        Me.BtnRegresar.BackgroundImage = Global.SCSC.My.Resources.Resources.Regresar
        Me.BtnRegresar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnRegresar.Location = New System.Drawing.Point(143, 0)
        Me.BtnRegresar.Name = "BtnRegresar"
        Me.BtnRegresar.Size = New System.Drawing.Size(63, 63)
        Me.BtnRegresar.TabIndex = 7
        Me.BtnRegresar.UseVisualStyleBackColor = True
        '
        'BtnCancelar
        '
        Me.BtnCancelar.BackgroundImage = Global.SCSC.My.Resources.Resources.Cancelar
        Me.BtnCancelar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnCancelar.Location = New System.Drawing.Point(75, 0)
        Me.BtnCancelar.Name = "BtnCancelar"
        Me.BtnCancelar.Size = New System.Drawing.Size(63, 63)
        Me.BtnCancelar.TabIndex = 5
        Me.BtnCancelar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText
        Me.BtnCancelar.UseVisualStyleBackColor = True
        '
        'BtnGuardar
        '
        Me.BtnGuardar.BackgroundImage = Global.SCSC.My.Resources.Resources.Pantalla
        Me.BtnGuardar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnGuardar.Location = New System.Drawing.Point(7, 0)
        Me.BtnGuardar.Name = "BtnGuardar"
        Me.BtnGuardar.Size = New System.Drawing.Size(63, 63)
        Me.BtnGuardar.TabIndex = 0
        Me.BtnGuardar.UseVisualStyleBackColor = True
        '
        'GroupParametros
        '
        Me.GroupParametros.Controls.Add(Me.RbGeneral)
        Me.GroupParametros.Controls.Add(Me.RbDetallo)
        Me.GroupParametros.Controls.Add(Me.PanelTiposReporte)
        Me.GroupParametros.Controls.Add(Me.CbHorario)
        Me.GroupParametros.Controls.Add(Me.LblHorarioCaption)
        Me.GroupParametros.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupParametros.Location = New System.Drawing.Point(10, 62)
        Me.GroupParametros.Name = "GroupParametros"
        Me.GroupParametros.Size = New System.Drawing.Size(542, 148)
        Me.GroupParametros.TabIndex = 23
        Me.GroupParametros.TabStop = False
        Me.GroupParametros.Text = "Parametros"
        '
        'RbGeneral
        '
        Me.RbGeneral.AutoSize = True
        Me.RbGeneral.Checked = True
        Me.RbGeneral.Location = New System.Drawing.Point(306, 63)
        Me.RbGeneral.Name = "RbGeneral"
        Me.RbGeneral.Size = New System.Drawing.Size(199, 28)
        Me.RbGeneral.TabIndex = 24
        Me.RbGeneral.TabStop = True
        Me.RbGeneral.Text = "Reporte por Sección"
        Me.RbGeneral.UseVisualStyleBackColor = True
        '
        'RbDetallo
        '
        Me.RbDetallo.AutoSize = True
        Me.RbDetallo.Location = New System.Drawing.Point(306, 93)
        Me.RbDetallo.Name = "RbDetallo"
        Me.RbDetallo.Size = New System.Drawing.Size(214, 28)
        Me.RbDetallo.TabIndex = 25
        Me.RbDetallo.Text = "Detallado por servicio"
        Me.RbDetallo.UseVisualStyleBackColor = True
        '
        'PanelTiposReporte
        '
        Me.PanelTiposReporte.Controls.Add(Me.RbPermisoSalida)
        Me.PanelTiposReporte.Controls.Add(Me.RbBecaTransporte)
        Me.PanelTiposReporte.Controls.Add(Me.RbBecaComedor)
        Me.PanelTiposReporte.Location = New System.Drawing.Point(17, 26)
        Me.PanelTiposReporte.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.PanelTiposReporte.Name = "PanelTiposReporte"
        Me.PanelTiposReporte.Size = New System.Drawing.Size(228, 98)
        Me.PanelTiposReporte.TabIndex = 13
        '
        'RbPermisoSalida
        '
        Me.RbPermisoSalida.AutoSize = True
        Me.RbPermisoSalida.Location = New System.Drawing.Point(17, 65)
        Me.RbPermisoSalida.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
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
        Me.RbBecaTransporte.Location = New System.Drawing.Point(17, 37)
        Me.RbBecaTransporte.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
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
        Me.RbBecaComedor.Location = New System.Drawing.Point(17, 10)
        Me.RbBecaComedor.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
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
        Me.CbHorario.Location = New System.Drawing.Point(375, 31)
        Me.CbHorario.Name = "CbHorario"
        Me.CbHorario.Size = New System.Drawing.Size(161, 32)
        Me.CbHorario.TabIndex = 12
        '
        'LblHorarioCaption
        '
        Me.LblHorarioCaption.AutoSize = True
        Me.LblHorarioCaption.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblHorarioCaption.Location = New System.Drawing.Point(307, 34)
        Me.LblHorarioCaption.Name = "LblHorarioCaption"
        Me.LblHorarioCaption.Size = New System.Drawing.Size(83, 24)
        Me.LblHorarioCaption.TabIndex = 11
        Me.LblHorarioCaption.Text = "Horario: "
        '
        'FrmBecados
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(562, 310)
        Me.ControlBox = False
        Me.Controls.Add(Me.GroupParametros)
        Me.Controls.Add(Me.PanelAcciones)
        Me.Controls.Add(Me.LblTituloReporte)
        Me.Name = "FrmBecados"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Reporte Proyeción Comedor"
        Me.PanelAcciones.ResumeLayout(False)
        Me.GroupParametros.ResumeLayout(False)
        Me.GroupParametros.PerformLayout()
        Me.PanelTiposReporte.ResumeLayout(False)
        Me.PanelTiposReporte.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents LblTituloReporte As Label
    Friend WithEvents PanelAcciones As Panel
    Friend WithEvents BtnRegresar As Button
    Friend WithEvents BtnCancelar As Button
    Friend WithEvents BtnGuardar As Button
    Friend WithEvents GroupParametros As GroupBox
    Friend WithEvents CbHorario As ComboBox
    Friend WithEvents LblHorarioCaption As Label
    Friend WithEvents PanelTiposReporte As Panel
    Friend WithEvents RbPermisoSalida As RadioButton
    Friend WithEvents RbBecaTransporte As RadioButton
    Friend WithEvents RbBecaComedor As RadioButton
    Friend WithEvents RbDetallo As RadioButton
    Friend WithEvents RbGeneral As RadioButton
End Class
