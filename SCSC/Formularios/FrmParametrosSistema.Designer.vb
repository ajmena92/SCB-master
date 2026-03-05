<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmParametrosSistema
    Inherits System.Windows.Forms.Form

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.PanelAcciones = New System.Windows.Forms.Panel()
        Me.BtnCerrar = New System.Windows.Forms.Button()
        Me.BtnEliminar = New System.Windows.Forms.Button()
        Me.BtnGuardar = New System.Windows.Forms.Button()
        Me.BtnCrear = New System.Windows.Forms.Button()
        Me.BtnRecargar = New System.Windows.Forms.Button()
        Me.LabelTitulo = New System.Windows.Forms.Label()
        Me.LblEstado = New System.Windows.Forms.Label()
        Me.DgvParametros = New System.Windows.Forms.DataGridView()
        Me.ColClave = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ColValor = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.PanelAcciones.SuspendLayout()
        CType(Me.DgvParametros, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PanelAcciones
        '
        Me.PanelAcciones.Controls.Add(Me.BtnCerrar)
        Me.PanelAcciones.Controls.Add(Me.BtnEliminar)
        Me.PanelAcciones.Controls.Add(Me.BtnGuardar)
        Me.PanelAcciones.Controls.Add(Me.BtnCrear)
        Me.PanelAcciones.Controls.Add(Me.BtnRecargar)
        Me.PanelAcciones.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.PanelAcciones.Location = New System.Drawing.Point(0, 510)
        Me.PanelAcciones.Name = "PanelAcciones"
        Me.PanelAcciones.Size = New System.Drawing.Size(1024, 70)
        Me.PanelAcciones.TabIndex = 0
        '
        'BtnCerrar
        '
        Me.BtnCerrar.Location = New System.Drawing.Point(873, 16)
        Me.BtnCerrar.Name = "BtnCerrar"
        Me.BtnCerrar.Size = New System.Drawing.Size(130, 40)
        Me.BtnCerrar.TabIndex = 4
        Me.BtnCerrar.Text = "Cerrar"
        Me.BtnCerrar.UseVisualStyleBackColor = True
        '
        'BtnEliminar
        '
        Me.BtnEliminar.Location = New System.Drawing.Point(707, 16)
        Me.BtnEliminar.Name = "BtnEliminar"
        Me.BtnEliminar.Size = New System.Drawing.Size(150, 40)
        Me.BtnEliminar.TabIndex = 3
        Me.BtnEliminar.Text = "Restablecer"
        Me.BtnEliminar.UseVisualStyleBackColor = True
        '
        'BtnGuardar
        '
        Me.BtnGuardar.Location = New System.Drawing.Point(551, 16)
        Me.BtnGuardar.Name = "BtnGuardar"
        Me.BtnGuardar.Size = New System.Drawing.Size(140, 40)
        Me.BtnGuardar.TabIndex = 2
        Me.BtnGuardar.Text = "Guardar"
        Me.BtnGuardar.UseVisualStyleBackColor = True
        '
        'BtnCrear
        '
        Me.BtnCrear.Location = New System.Drawing.Point(180, 16)
        Me.BtnCrear.Name = "BtnCrear"
        Me.BtnCrear.Size = New System.Drawing.Size(160, 40)
        Me.BtnCrear.TabIndex = 1
        Me.BtnCrear.Text = "Crear Fila 0"
        Me.BtnCrear.UseVisualStyleBackColor = True
        '
        'BtnRecargar
        '
        Me.BtnRecargar.Location = New System.Drawing.Point(16, 16)
        Me.BtnRecargar.Name = "BtnRecargar"
        Me.BtnRecargar.Size = New System.Drawing.Size(150, 40)
        Me.BtnRecargar.TabIndex = 0
        Me.BtnRecargar.Text = "Recargar"
        Me.BtnRecargar.UseVisualStyleBackColor = True
        '
        'LabelTitulo
        '
        Me.LabelTitulo.AutoSize = True
        Me.LabelTitulo.Location = New System.Drawing.Point(18, 15)
        Me.LabelTitulo.Name = "LabelTitulo"
        Me.LabelTitulo.Size = New System.Drawing.Size(302, 21)
        Me.LabelTitulo.TabIndex = 1
        Me.LabelTitulo.Text = "Mantenimiento de Parametros del Sistema"
        '
        'LblEstado
        '
        Me.LblEstado.Location = New System.Drawing.Point(18, 45)
        Me.LblEstado.Name = "LblEstado"
        Me.LblEstado.Size = New System.Drawing.Size(980, 24)
        Me.LblEstado.TabIndex = 2
        Me.LblEstado.Text = "Listo."
        '
        'DgvParametros
        '
        Me.DgvParametros.AllowUserToAddRows = False
        Me.DgvParametros.AllowUserToDeleteRows = False
        Me.DgvParametros.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DgvParametros.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.ColClave, Me.ColValor})
        Me.DgvParametros.Location = New System.Drawing.Point(22, 80)
        Me.DgvParametros.Name = "DgvParametros"
        Me.DgvParametros.RowHeadersVisible = False
        Me.DgvParametros.RowTemplate.Height = 24
        Me.DgvParametros.Size = New System.Drawing.Size(980, 418)
        Me.DgvParametros.TabIndex = 3
        '
        'ColClave
        '
        Me.ColClave.HeaderText = "Parametro"
        Me.ColClave.Name = "ColClave"
        Me.ColClave.ReadOnly = True
        Me.ColClave.Width = 340
        '
        'ColValor
        '
        Me.ColValor.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.ColValor.HeaderText = "Valor"
        Me.ColValor.Name = "ColValor"
        '
        'FrmParametrosSistema
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 21.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1024, 580)
        Me.Controls.Add(Me.DgvParametros)
        Me.Controls.Add(Me.LblEstado)
        Me.Controls.Add(Me.LabelTitulo)
        Me.Controls.Add(Me.PanelAcciones)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmParametrosSistema"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Parametros del Sistema"
        Me.PanelAcciones.ResumeLayout(False)
        CType(Me.DgvParametros, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents PanelAcciones As Panel
    Friend WithEvents BtnCerrar As Button
    Friend WithEvents BtnEliminar As Button
    Friend WithEvents BtnGuardar As Button
    Friend WithEvents BtnCrear As Button
    Friend WithEvents BtnRecargar As Button
    Friend WithEvents LabelTitulo As Label
    Friend WithEvents LblEstado As Label
    Friend WithEvents DgvParametros As DataGridView
    Friend WithEvents ColClave As DataGridViewTextBoxColumn
    Friend WithEvents ColValor As DataGridViewTextBoxColumn
End Class
