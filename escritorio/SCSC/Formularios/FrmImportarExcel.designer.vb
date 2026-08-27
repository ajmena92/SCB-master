<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmImportarExcel
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmImportarExcel))
        Me.PanelAcciones = New System.Windows.Forms.Panel()
        Me.BtnRegresar = New System.Windows.Forms.Button()
        Me.BtnCancelar = New System.Windows.Forms.Button()
        Me.BtnGuardar = New System.Windows.Forms.Button()
        Me.LblTituloModulo = New System.Windows.Forms.Label()
        Me.GroupConfiguracionImportacion = New System.Windows.Forms.GroupBox()
        Me.CBHorario = New System.Windows.Forms.ComboBox()
        Me.RbDocentes = New System.Windows.Forms.RadioButton()
        Me.RdEst = New System.Windows.Forms.RadioButton()
        Me.Progreso = New System.Windows.Forms.ProgressBar()
        Me.LblEstado = New System.Windows.Forms.Label()
        Me.DgvVistaPrevia = New System.Windows.Forms.DataGridView()
        Me.PanelAcciones.SuspendLayout()
        Me.GroupConfiguracionImportacion.SuspendLayout()
        CType(Me.DgvVistaPrevia, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PanelAcciones
        '
        Me.PanelAcciones.BackColor = System.Drawing.Color.Transparent
        Me.PanelAcciones.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PanelAcciones.Controls.Add(Me.BtnRegresar)
        Me.PanelAcciones.Controls.Add(Me.BtnCancelar)
        Me.PanelAcciones.Controls.Add(Me.BtnGuardar)
        Me.PanelAcciones.Location = New System.Drawing.Point(301, 292)
        Me.PanelAcciones.Name = "PanelAcciones"
        Me.PanelAcciones.Size = New System.Drawing.Size(198, 66)
        Me.PanelAcciones.TabIndex = 19
        '
        'BtnRegresar
        '
        Me.BtnRegresar.BackgroundImage = Global.SCSC.My.Resources.Resources.Regresar
        Me.BtnRegresar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnRegresar.Location = New System.Drawing.Point(129, 2)
        Me.BtnRegresar.Name = "BtnRegresar"
        Me.BtnRegresar.Size = New System.Drawing.Size(62, 63)
        Me.BtnRegresar.TabIndex = 8
        Me.BtnRegresar.UseVisualStyleBackColor = True
        '
        'BtnCancelar
        '
        Me.BtnCancelar.BackgroundImage = Global.SCSC.My.Resources.Resources.Cancelar
        Me.BtnCancelar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.BtnCancelar.Location = New System.Drawing.Point(68, 2)
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
        Me.BtnGuardar.Location = New System.Drawing.Point(5, 2)
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
        Me.LblTituloModulo.Font = New System.Drawing.Font("Calibri", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTituloModulo.ForeColor = System.Drawing.Color.Black
        Me.LblTituloModulo.Location = New System.Drawing.Point(148, 9)
        Me.LblTituloModulo.Name = "LblTituloModulo"
        Me.LblTituloModulo.Size = New System.Drawing.Size(220, 29)
        Me.LblTituloModulo.TabIndex = 20
        Me.LblTituloModulo.Text = "Importar Datos PIAD"
        Me.LblTituloModulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'GroupConfiguracionImportacion
        '
        Me.GroupConfiguracionImportacion.BackColor = System.Drawing.Color.Transparent
        Me.GroupConfiguracionImportacion.Controls.Add(Me.CBHorario)
        Me.GroupConfiguracionImportacion.Controls.Add(Me.RbDocentes)
        Me.GroupConfiguracionImportacion.Controls.Add(Me.RdEst)
        Me.GroupConfiguracionImportacion.Location = New System.Drawing.Point(31, 67)
        Me.GroupConfiguracionImportacion.Name = "GroupConfiguracionImportacion"
        Me.GroupConfiguracionImportacion.Size = New System.Drawing.Size(468, 127)
        Me.GroupConfiguracionImportacion.TabIndex = 21
        Me.GroupConfiguracionImportacion.TabStop = False
        Me.GroupConfiguracionImportacion.Text = "Tipo de Datos "
        '
        'CBHorario
        '
        Me.CBHorario.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.CBHorario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CBHorario.FormattingEnabled = True
        Me.CBHorario.Location = New System.Drawing.Point(255, 52)
        Me.CBHorario.Name = "CBHorario"
        Me.CBHorario.Size = New System.Drawing.Size(171, 27)
        Me.CBHorario.TabIndex = 2
        '
        'RbDocentes
        '
        Me.RbDocentes.AutoSize = True
        Me.RbDocentes.Location = New System.Drawing.Point(30, 70)
        Me.RbDocentes.Name = "RbDocentes"
        Me.RbDocentes.Size = New System.Drawing.Size(148, 23)
        Me.RbDocentes.TabIndex = 1
        Me.RbDocentes.Text = "Actulizar Docentes"
        Me.RbDocentes.UseVisualStyleBackColor = True
        '
        'RdEst
        '
        Me.RdEst.AutoSize = True
        Me.RdEst.Checked = True
        Me.RdEst.Location = New System.Drawing.Point(30, 31)
        Me.RdEst.Name = "RdEst"
        Me.RdEst.Size = New System.Drawing.Size(163, 23)
        Me.RdEst.TabIndex = 0
        Me.RdEst.TabStop = True
        Me.RdEst.Text = "Actulizar Estudiantes"
        Me.RdEst.UseVisualStyleBackColor = True
        '
        'Progreso
        '
        Me.Progreso.Location = New System.Drawing.Point(47, 254)
        Me.Progreso.Name = "Progreso"
        Me.Progreso.Size = New System.Drawing.Size(434, 23)
        Me.Progreso.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        Me.Progreso.TabIndex = 22
        '
        'LblEstado
        '
        Me.LblEstado.BackColor = System.Drawing.Color.Transparent
        Me.LblEstado.Font = New System.Drawing.Font("Calibri", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblEstado.Location = New System.Drawing.Point(43, 209)
        Me.LblEstado.Name = "LblEstado"
        Me.LblEstado.Size = New System.Drawing.Size(438, 42)
        Me.LblEstado.TabIndex = 23
        Me.LblEstado.Text = "Iniciar Proceso"
        Me.LblEstado.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'DgvVistaPrevia
        '
        Me.DgvVistaPrevia.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DgvVistaPrevia.Location = New System.Drawing.Point(12, 373)
        Me.DgvVistaPrevia.Name = "DgvVistaPrevia"
        Me.DgvVistaPrevia.Size = New System.Drawing.Size(487, 180)
        Me.DgvVistaPrevia.TabIndex = 24
        '
        'FrmImportarExcel
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 19.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(515, 580)
        Me.Controls.Add(Me.DgvVistaPrevia)
        Me.Controls.Add(Me.LblEstado)
        Me.Controls.Add(Me.Progreso)
        Me.Controls.Add(Me.GroupConfiguracionImportacion)
        Me.Controls.Add(Me.PanelAcciones)
        Me.Controls.Add(Me.LblTituloModulo)
        Me.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmImportarExcel"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Conexion Inteligente"
        Me.PanelAcciones.ResumeLayout(False)
        Me.GroupConfiguracionImportacion.ResumeLayout(False)
        Me.GroupConfiguracionImportacion.PerformLayout()
        CType(Me.DgvVistaPrevia, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents PanelAcciones As System.Windows.Forms.Panel
    Friend WithEvents BtnCancelar As System.Windows.Forms.Button
    Friend WithEvents BtnGuardar As System.Windows.Forms.Button
    Friend WithEvents LblTituloModulo As System.Windows.Forms.Label
    Friend WithEvents BtnRegresar As System.Windows.Forms.Button
    Friend WithEvents GroupConfiguracionImportacion As System.Windows.Forms.GroupBox
    Friend WithEvents RbDocentes As System.Windows.Forms.RadioButton
    Friend WithEvents RdEst As System.Windows.Forms.RadioButton
    Friend WithEvents Progreso As System.Windows.Forms.ProgressBar
    Friend WithEvents LblEstado As System.Windows.Forms.Label
    Friend WithEvents DgvVistaPrevia As DataGridView
    Friend WithEvents CBHorario As ComboBox
End Class
