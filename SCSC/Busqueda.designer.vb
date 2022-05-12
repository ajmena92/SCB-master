<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Busqueda
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
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

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Busqueda))
        Me.LblTitulo = New System.Windows.Forms.Label()
        Me.LblFiltrado = New System.Windows.Forms.Label()
        Me.TxtFiltro = New System.Windows.Forms.TextBox()
        Me.BtnFiltro = New System.Windows.Forms.Button()
        Me.GridConsulta = New System.Windows.Forms.DataGridView()
        Me.Cancelar = New System.Windows.Forms.Button()
        Me.Guardar = New System.Windows.Forms.Button()
        Me.LblMensajes = New System.Windows.Forms.Label()
        Me.LblSeleccion = New System.Windows.Forms.Label()
        CType(Me.GridConsulta, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'LblTitulo
        '
        Me.LblTitulo.AutoSize = True
        Me.LblTitulo.BackColor = System.Drawing.Color.Transparent
        Me.LblTitulo.Font = New System.Drawing.Font("Calibri", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTitulo.ForeColor = System.Drawing.Color.Transparent
        Me.LblTitulo.Location = New System.Drawing.Point(12, 18)
        Me.LblTitulo.Name = "LblTitulo"
        Me.LblTitulo.Size = New System.Drawing.Size(189, 26)
        Me.LblTitulo.TabIndex = 26
        Me.LblTitulo.Text = "Titulo del formulario"
        '
        'LblFiltrado
        '
        Me.LblFiltrado.AutoSize = True
        Me.LblFiltrado.BackColor = System.Drawing.Color.Transparent
        Me.LblFiltrado.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblFiltrado.Location = New System.Drawing.Point(21, 76)
        Me.LblFiltrado.Name = "LblFiltrado"
        Me.LblFiltrado.Size = New System.Drawing.Size(90, 19)
        Me.LblFiltrado.TabIndex = 31
        Me.LblFiltrado.Text = "Filtrado por"
        '
        'TxtFiltro
        '
        Me.TxtFiltro.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtFiltro.Location = New System.Drawing.Point(19, 99)
        Me.TxtFiltro.Name = "TxtFiltro"
        Me.TxtFiltro.Size = New System.Drawing.Size(393, 27)
        Me.TxtFiltro.TabIndex = 29
        '
        'BtnFiltro
        '
        Me.BtnFiltro.BackColor = System.Drawing.Color.White
        Me.BtnFiltro.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.BtnFiltro.FlatAppearance.BorderSize = 0
        Me.BtnFiltro.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.BtnFiltro.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.BtnFiltro.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BtnFiltro.Image = Global.SCSC.My.Resources.Resources.Buscar_30x30
        Me.BtnFiltro.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.BtnFiltro.Location = New System.Drawing.Point(414, 97)
        Me.BtnFiltro.Name = "BtnFiltro"
        Me.BtnFiltro.Size = New System.Drawing.Size(41, 30)
        Me.BtnFiltro.TabIndex = 30
        Me.BtnFiltro.UseVisualStyleBackColor = False
        '
        'GridConsulta
        '
        Me.GridConsulta.AllowUserToAddRows = False
        Me.GridConsulta.AllowUserToDeleteRows = False
        Me.GridConsulta.AllowUserToOrderColumns = True
        Me.GridConsulta.AllowUserToResizeRows = False
        Me.GridConsulta.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GridConsulta.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.GridConsulta.BackgroundColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.GridConsulta.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.GridConsulta.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.GridConsulta.GridColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.GridConsulta.Location = New System.Drawing.Point(18, 140)
        Me.GridConsulta.MultiSelect = False
        Me.GridConsulta.Name = "GridConsulta"
        Me.GridConsulta.ReadOnly = True
        Me.GridConsulta.RowHeadersVisible = False
        Me.GridConsulta.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.GridConsulta.Size = New System.Drawing.Size(535, 209)
        Me.GridConsulta.TabIndex = 32
        '
        'Cancelar
        '
        Me.Cancelar.BackgroundImage = Global.SCSC.My.Resources.Resources.Cancelar
        Me.Cancelar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.Cancelar.Location = New System.Drawing.Point(492, 384)
        Me.Cancelar.Name = "Cancelar"
        Me.Cancelar.Size = New System.Drawing.Size(62, 63)
        Me.Cancelar.TabIndex = 34
        Me.Cancelar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText
        Me.Cancelar.UseVisualStyleBackColor = True
        '
        'Guardar
        '
        Me.Guardar.BackgroundImage = Global.SCSC.My.Resources.Resources.Aceptar
        Me.Guardar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.Guardar.Location = New System.Drawing.Point(428, 384)
        Me.Guardar.Name = "Guardar"
        Me.Guardar.Size = New System.Drawing.Size(62, 63)
        Me.Guardar.TabIndex = 33
        Me.Guardar.UseVisualStyleBackColor = True
        '
        'LblMensajes
        '
        Me.LblMensajes.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LblMensajes.AutoSize = True
        Me.LblMensajes.Font = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblMensajes.Location = New System.Drawing.Point(15, 428)
        Me.LblMensajes.Name = "LblMensajes"
        Me.LblMensajes.Size = New System.Drawing.Size(13, 14)
        Me.LblMensajes.TabIndex = 35
        Me.LblMensajes.Text = ".."
        '
        'LblSeleccion
        '
        Me.LblSeleccion.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LblSeleccion.AutoSize = True
        Me.LblSeleccion.Font = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblSeleccion.ForeColor = System.Drawing.Color.DimGray
        Me.LblSeleccion.Location = New System.Drawing.Point(15, 406)
        Me.LblSeleccion.Name = "LblSeleccion"
        Me.LblSeleccion.Size = New System.Drawing.Size(13, 14)
        Me.LblSeleccion.TabIndex = 36
        Me.LblSeleccion.Text = ".."
        '
        'Busqueda
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 19.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(160, Byte), Integer), CType(CType(160, Byte), Integer), CType(CType(160, Byte), Integer))
        Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(571, 464)
        Me.ControlBox = False
        Me.Controls.Add(Me.LblSeleccion)
        Me.Controls.Add(Me.LblMensajes)
        Me.Controls.Add(Me.Cancelar)
        Me.Controls.Add(Me.Guardar)
        Me.Controls.Add(Me.GridConsulta)
        Me.Controls.Add(Me.LblFiltrado)
        Me.Controls.Add(Me.TxtFiltro)
        Me.Controls.Add(Me.BtnFiltro)
        Me.Controls.Add(Me.LblTitulo)
        Me.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "Busqueda"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Busqueda"
        CType(Me.GridConsulta, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents LblTitulo As System.Windows.Forms.Label
    Friend WithEvents LblFiltrado As System.Windows.Forms.Label
    Friend WithEvents TxtFiltro As System.Windows.Forms.TextBox
    Friend WithEvents BtnFiltro As System.Windows.Forms.Button
    Friend WithEvents GridConsulta As System.Windows.Forms.DataGridView
    Friend WithEvents Cancelar As System.Windows.Forms.Button
    Friend WithEvents Guardar As System.Windows.Forms.Button
    Friend WithEvents LblMensajes As System.Windows.Forms.Label
    Friend WithEvents LblSeleccion As System.Windows.Forms.Label
End Class
