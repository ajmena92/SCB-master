<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class IMPRIMIR
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(IMPRIMIR))
        Me.prdImpresoras = New System.Windows.Forms.PrintDialog()
        Me.btnSeleccionarImpresora = New System.Windows.Forms.Button()
        Me.btnCrear = New System.Windows.Forms.Button()
        Me.btnVer = New System.Windows.Forms.Button()
        Me.btncerrar = New System.Windows.Forms.Button()
        Me.lblImpresoraActual = New System.Windows.Forms.Label()
        Me.txtAlto = New System.Windows.Forms.TextBox()
        Me.txtAncho = New System.Windows.Forms.TextBox()
        Me.txtNombre = New System.Windows.Forms.TextBox()
        Me.LblNombreCaption = New System.Windows.Forms.Label()
        Me.LblAltoCaption = New System.Windows.Forms.Label()
        Me.LblAnchoCaption = New System.Windows.Forms.Label()
        Me.prdoDocumento = New System.Drawing.Printing.PrintDocument()
        Me.ppdDocumento = New System.Windows.Forms.PrintPreviewDialog()
        Me.SuspendLayout()
        '
        'prdImpresoras
        '
        Me.prdImpresoras.UseEXDialog = True
        '
        'btnSeleccionarImpresora
        '
        Me.btnSeleccionarImpresora.Location = New System.Drawing.Point(33, 17)
        Me.btnSeleccionarImpresora.Name = "btnSeleccionarImpresora"
        Me.btnSeleccionarImpresora.Size = New System.Drawing.Size(122, 23)
        Me.btnSeleccionarImpresora.TabIndex = 0
        Me.btnSeleccionarImpresora.Text = "Selecionar Impresora"
        Me.btnSeleccionarImpresora.UseVisualStyleBackColor = True
        '
        'btnCrear
        '
        Me.btnCrear.Location = New System.Drawing.Point(211, 172)
        Me.btnCrear.Name = "btnCrear"
        Me.btnCrear.Size = New System.Drawing.Size(75, 23)
        Me.btnCrear.TabIndex = 1
        Me.btnCrear.Text = "Crear"
        Me.btnCrear.UseVisualStyleBackColor = True
        '
        'btnVer
        '
        Me.btnVer.Location = New System.Drawing.Point(292, 172)
        Me.btnVer.Name = "btnVer"
        Me.btnVer.Size = New System.Drawing.Size(75, 23)
        Me.btnVer.TabIndex = 2
        Me.btnVer.Text = "Ver"
        Me.btnVer.UseVisualStyleBackColor = True
        '
        'btncerrar
        '
        Me.btncerrar.Location = New System.Drawing.Point(292, 226)
        Me.btncerrar.Name = "btncerrar"
        Me.btncerrar.Size = New System.Drawing.Size(75, 23)
        Me.btncerrar.TabIndex = 3
        Me.btncerrar.Text = "Cerrar"
        Me.btncerrar.UseVisualStyleBackColor = True
        '
        'lblImpresoraActual
        '
        Me.lblImpresoraActual.AutoSize = True
        Me.lblImpresoraActual.Location = New System.Drawing.Point(228, 17)
        Me.lblImpresoraActual.Name = "lblImpresoraActual"
        Me.lblImpresoraActual.Size = New System.Drawing.Size(63, 13)
        Me.lblImpresoraActual.TabIndex = 4
        Me.lblImpresoraActual.Text = "(NINGUNA)"
        '
        'txtAlto
        '
        Me.txtAlto.Location = New System.Drawing.Point(94, 121)
        Me.txtAlto.Name = "txtAlto"
        Me.txtAlto.Size = New System.Drawing.Size(100, 20)
        Me.txtAlto.TabIndex = 5
        '
        'txtAncho
        '
        Me.txtAncho.Location = New System.Drawing.Point(94, 95)
        Me.txtAncho.Name = "txtAncho"
        Me.txtAncho.Size = New System.Drawing.Size(100, 20)
        Me.txtAncho.TabIndex = 6
        '
        'txtNombre
        '
        Me.txtNombre.Location = New System.Drawing.Point(94, 69)
        Me.txtNombre.Name = "txtNombre"
        Me.txtNombre.Size = New System.Drawing.Size(100, 20)
        Me.txtNombre.TabIndex = 7
        '
        'LblNombreCaption
        '
        Me.LblNombreCaption.AutoSize = True
        Me.LblNombreCaption.Location = New System.Drawing.Point(49, 69)
        Me.LblNombreCaption.Name = "LblNombreCaption"
        Me.LblNombreCaption.Size = New System.Drawing.Size(44, 13)
        Me.LblNombreCaption.TabIndex = 8
        Me.LblNombreCaption.Text = "Nombre"
        '
        'LblAltoCaption
        '
        Me.LblAltoCaption.AutoSize = True
        Me.LblAltoCaption.Location = New System.Drawing.Point(49, 124)
        Me.LblAltoCaption.Name = "LblAltoCaption"
        Me.LblAltoCaption.Size = New System.Drawing.Size(25, 13)
        Me.LblAltoCaption.TabIndex = 9
        Me.LblAltoCaption.Text = "Alto"
        '
        'LblAnchoCaption
        '
        Me.LblAnchoCaption.AutoSize = True
        Me.LblAnchoCaption.Location = New System.Drawing.Point(49, 95)
        Me.LblAnchoCaption.Name = "LblAnchoCaption"
        Me.LblAnchoCaption.Size = New System.Drawing.Size(38, 13)
        Me.LblAnchoCaption.TabIndex = 10
        Me.LblAnchoCaption.Text = "Ancho"
        '
        'prdoDocumento
        '
        '
        'ppdDocumento
        '
        Me.ppdDocumento.AutoScrollMargin = New System.Drawing.Size(0, 0)
        Me.ppdDocumento.AutoScrollMinSize = New System.Drawing.Size(0, 0)
        Me.ppdDocumento.ClientSize = New System.Drawing.Size(400, 300)
        Me.ppdDocumento.Enabled = True
        Me.ppdDocumento.Icon = CType(resources.GetObject("ppdDocumento.Icon"), System.Drawing.Icon)
        Me.ppdDocumento.Name = "ppdDocumento"
        Me.ppdDocumento.Visible = False
        '
        'IMPRIMIR
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(429, 261)
        Me.Controls.Add(Me.LblAnchoCaption)
        Me.Controls.Add(Me.LblAltoCaption)
        Me.Controls.Add(Me.LblNombreCaption)
        Me.Controls.Add(Me.txtNombre)
        Me.Controls.Add(Me.txtAncho)
        Me.Controls.Add(Me.txtAlto)
        Me.Controls.Add(Me.lblImpresoraActual)
        Me.Controls.Add(Me.btncerrar)
        Me.Controls.Add(Me.btnVer)
        Me.Controls.Add(Me.btnCrear)
        Me.Controls.Add(Me.btnSeleccionarImpresora)
        Me.Name = "IMPRIMIR"
        Me.Text = "IMPRIMIR"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents prdImpresoras As System.Windows.Forms.PrintDialog
    Friend WithEvents btnSeleccionarImpresora As System.Windows.Forms.Button
    Friend WithEvents btnCrear As System.Windows.Forms.Button
    Friend WithEvents btnVer As System.Windows.Forms.Button
    Friend WithEvents btncerrar As System.Windows.Forms.Button
    Friend WithEvents lblImpresoraActual As System.Windows.Forms.Label
    Friend WithEvents txtAlto As System.Windows.Forms.TextBox
    Friend WithEvents txtAncho As System.Windows.Forms.TextBox
    Friend WithEvents txtNombre As System.Windows.Forms.TextBox
    Friend WithEvents LblNombreCaption As System.Windows.Forms.Label
    Friend WithEvents LblAltoCaption As System.Windows.Forms.Label
    Friend WithEvents LblAnchoCaption As System.Windows.Forms.Label
    Friend WithEvents prdoDocumento As System.Drawing.Printing.PrintDocument
    Friend WithEvents ppdDocumento As System.Windows.Forms.PrintPreviewDialog
End Class
