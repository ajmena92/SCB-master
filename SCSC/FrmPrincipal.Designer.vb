<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmPrincipal
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmPrincipal))
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.MantenimientoToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UsuariosToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.GestiónRutasToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.GestiónBecasToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UtilitariosToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ControlDeMarcasToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ImportarDatosListaPIADToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ImportarDatosPIADToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AgregarEstudianteManualToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RecargasToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ReportesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ReporteEstudiantesBecadosToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ReporteDiariosToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ReporteDeServicioTransporteToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ReporteProyecciónComedorToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AyudaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ImprimirToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.BunifuFlatButton3 = New System.Windows.Forms.Button()
        Me.BunifuFlatButton2 = New System.Windows.Forms.Button()
        Me.BunifuFlatButton1 = New System.Windows.Forms.Button()
        Me.BtnMantenimiento = New System.Windows.Forms.Button()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.PictureBox3 = New System.Windows.Forms.PictureBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.BtnCerrar = New System.Windows.Forms.PictureBox()
        Me.MenuStrip1.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.Panel3.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel2.SuspendLayout()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.BtnCerrar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'MenuStrip1
        '
        Me.MenuStrip1.BackColor = System.Drawing.Color.White
        Me.MenuStrip1.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.MenuStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MantenimientoToolStripMenuItem, Me.UtilitariosToolStripMenuItem, Me.ReportesToolStripMenuItem, Me.AyudaToolStripMenuItem, Me.ImprimirToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(1278, 32)
        Me.MenuStrip1.TabIndex = 1
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'MantenimientoToolStripMenuItem
        '
        Me.MantenimientoToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.UsuariosToolStripMenuItem, Me.GestiónRutasToolStripMenuItem, Me.GestiónBecasToolStripMenuItem})
        Me.MantenimientoToolStripMenuItem.ForeColor = System.Drawing.SystemColors.Desktop
        Me.MantenimientoToolStripMenuItem.Name = "MantenimientoToolStripMenuItem"
        Me.MantenimientoToolStripMenuItem.Size = New System.Drawing.Size(155, 28)
        Me.MantenimientoToolStripMenuItem.Text = "Mantenimiento"
        '
        'UsuariosToolStripMenuItem
        '
        Me.UsuariosToolStripMenuItem.Name = "UsuariosToolStripMenuItem"
        Me.UsuariosToolStripMenuItem.Size = New System.Drawing.Size(210, 28)
        Me.UsuariosToolStripMenuItem.Text = "Estudiantes"
        '
        'GestiónRutasToolStripMenuItem
        '
        Me.GestiónRutasToolStripMenuItem.Name = "GestiónRutasToolStripMenuItem"
        Me.GestiónRutasToolStripMenuItem.Size = New System.Drawing.Size(210, 28)
        Me.GestiónRutasToolStripMenuItem.Text = "Gestión Rutas"
        '
        'GestiónBecasToolStripMenuItem
        '
        Me.GestiónBecasToolStripMenuItem.Name = "GestiónBecasToolStripMenuItem"
        Me.GestiónBecasToolStripMenuItem.Size = New System.Drawing.Size(210, 28)
        Me.GestiónBecasToolStripMenuItem.Text = "Gestión Becas"
        '
        'UtilitariosToolStripMenuItem
        '
        Me.UtilitariosToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ControlDeMarcasToolStripMenuItem, Me.ImportarDatosListaPIADToolStripMenuItem, Me.ImportarDatosPIADToolStripMenuItem, Me.AgregarEstudianteManualToolStripMenuItem, Me.RecargasToolStripMenuItem1})
        Me.UtilitariosToolStripMenuItem.Name = "UtilitariosToolStripMenuItem"
        Me.UtilitariosToolStripMenuItem.Size = New System.Drawing.Size(106, 28)
        Me.UtilitariosToolStripMenuItem.Text = "Utilitarios"
        '
        'ControlDeMarcasToolStripMenuItem
        '
        Me.ControlDeMarcasToolStripMenuItem.Name = "ControlDeMarcasToolStripMenuItem"
        Me.ControlDeMarcasToolStripMenuItem.Size = New System.Drawing.Size(344, 28)
        Me.ControlDeMarcasToolStripMenuItem.Text = "Control de Marcas Comedor"
        '
        'ImportarDatosListaPIADToolStripMenuItem
        '
        Me.ImportarDatosListaPIADToolStripMenuItem.Name = "ImportarDatosListaPIADToolStripMenuItem"
        Me.ImportarDatosListaPIADToolStripMenuItem.Size = New System.Drawing.Size(344, 28)
        Me.ImportarDatosListaPIADToolStripMenuItem.Text = "Control de Marcas Transporte"
        '
        'ImportarDatosPIADToolStripMenuItem
        '
        Me.ImportarDatosPIADToolStripMenuItem.Name = "ImportarDatosPIADToolStripMenuItem"
        Me.ImportarDatosPIADToolStripMenuItem.Size = New System.Drawing.Size(344, 28)
        Me.ImportarDatosPIADToolStripMenuItem.Text = "Importar Datos PIAD"
        '
        'AgregarEstudianteManualToolStripMenuItem
        '
        Me.AgregarEstudianteManualToolStripMenuItem.Name = "AgregarEstudianteManualToolStripMenuItem"
        Me.AgregarEstudianteManualToolStripMenuItem.Size = New System.Drawing.Size(344, 28)
        Me.AgregarEstudianteManualToolStripMenuItem.Text = "Agregar Estudiante Manual"
        '
        'RecargasToolStripMenuItem1
        '
        Me.RecargasToolStripMenuItem1.Name = "RecargasToolStripMenuItem1"
        Me.RecargasToolStripMenuItem1.Size = New System.Drawing.Size(344, 28)
        Me.RecargasToolStripMenuItem1.Text = "Recargas"
        '
        'ReportesToolStripMenuItem
        '
        Me.ReportesToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ReporteEstudiantesBecadosToolStripMenuItem, Me.ReporteDiariosToolStripMenuItem, Me.ReporteDeServicioTransporteToolStripMenuItem, Me.ReporteProyecciónComedorToolStripMenuItem})
        Me.ReportesToolStripMenuItem.Name = "ReportesToolStripMenuItem"
        Me.ReportesToolStripMenuItem.Size = New System.Drawing.Size(99, 28)
        Me.ReportesToolStripMenuItem.Text = "Reportes"
        '
        'ReporteEstudiantesBecadosToolStripMenuItem
        '
        Me.ReporteEstudiantesBecadosToolStripMenuItem.Name = "ReporteEstudiantesBecadosToolStripMenuItem"
        Me.ReporteEstudiantesBecadosToolStripMenuItem.Size = New System.Drawing.Size(352, 28)
        Me.ReporteEstudiantesBecadosToolStripMenuItem.Text = "Reporte Estudiantes Becados"
        '
        'ReporteDiariosToolStripMenuItem
        '
        Me.ReporteDiariosToolStripMenuItem.Name = "ReporteDiariosToolStripMenuItem"
        Me.ReporteDiariosToolStripMenuItem.Size = New System.Drawing.Size(352, 28)
        Me.ReporteDiariosToolStripMenuItem.Text = "Reporte Servicio Comedor"
        '
        'ReporteDeServicioTransporteToolStripMenuItem
        '
        Me.ReporteDeServicioTransporteToolStripMenuItem.Name = "ReporteDeServicioTransporteToolStripMenuItem"
        Me.ReporteDeServicioTransporteToolStripMenuItem.Size = New System.Drawing.Size(352, 28)
        Me.ReporteDeServicioTransporteToolStripMenuItem.Text = "Reporte de Servicio Transporte"
        '
        'ReporteProyecciónComedorToolStripMenuItem
        '
        Me.ReporteProyecciónComedorToolStripMenuItem.Name = "ReporteProyecciónComedorToolStripMenuItem"
        Me.ReporteProyecciónComedorToolStripMenuItem.Size = New System.Drawing.Size(352, 28)
        Me.ReporteProyecciónComedorToolStripMenuItem.Text = "Reporte Proyección Comedor"
        '
        'AyudaToolStripMenuItem
        '
        Me.AyudaToolStripMenuItem.ForeColor = System.Drawing.SystemColors.Desktop
        Me.AyudaToolStripMenuItem.Name = "AyudaToolStripMenuItem"
        Me.AyudaToolStripMenuItem.Size = New System.Drawing.Size(77, 28)
        Me.AyudaToolStripMenuItem.Text = "Ayuda"
        '
        'ImprimirToolStripMenuItem
        '
        Me.ImprimirToolStripMenuItem.Name = "ImprimirToolStripMenuItem"
        Me.ImprimirToolStripMenuItem.Size = New System.Drawing.Size(96, 28)
        Me.ImprimirToolStripMenuItem.Text = "Imprimir"
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(51, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.Panel1.Controls.Add(Me.BunifuFlatButton3)
        Me.Panel1.Controls.Add(Me.BunifuFlatButton2)
        Me.Panel1.Controls.Add(Me.BunifuFlatButton1)
        Me.Panel1.Controls.Add(Me.BtnMantenimiento)
        Me.Panel1.Controls.Add(Me.Panel3)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Left
        Me.Panel1.Location = New System.Drawing.Point(0, 32)
        Me.Panel1.Margin = New System.Windows.Forms.Padding(4)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(301, 621)
        Me.Panel1.TabIndex = 2
        '
        'BunifuFlatButton3
        '
        Me.BunifuFlatButton3.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(53, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BunifuFlatButton3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.BunifuFlatButton3.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BunifuFlatButton3.Location = New System.Drawing.Point(0, 70)
        Me.BunifuFlatButton3.Margin = New System.Windows.Forms.Padding(5)
        Me.BunifuFlatButton3.Name = "BunifuFlatButton3"
        Me.BunifuFlatButton3.Size = New System.Drawing.Size(301, 59)
        Me.BunifuFlatButton3.TabIndex = 4
        Me.BunifuFlatButton3.Text = "Dashboard"
        Me.BunifuFlatButton3.UseVisualStyleBackColor = False
        '
        'BunifuFlatButton2
        '
        Me.BunifuFlatButton2.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(53, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BunifuFlatButton2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.BunifuFlatButton2.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BunifuFlatButton2.Location = New System.Drawing.Point(0, 244)
        Me.BunifuFlatButton2.Margin = New System.Windows.Forms.Padding(5)
        Me.BunifuFlatButton2.Name = "BunifuFlatButton2"
        Me.BunifuFlatButton2.Size = New System.Drawing.Size(301, 59)
        Me.BunifuFlatButton2.TabIndex = 3
        Me.BunifuFlatButton2.Text = "Reportes"
        Me.BunifuFlatButton2.UseVisualStyleBackColor = False
        '
        'BunifuFlatButton1
        '
        Me.BunifuFlatButton1.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(53, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BunifuFlatButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.BunifuFlatButton1.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BunifuFlatButton1.Location = New System.Drawing.Point(0, 186)
        Me.BunifuFlatButton1.Margin = New System.Windows.Forms.Padding(5)
        Me.BunifuFlatButton1.Name = "BunifuFlatButton1"
        Me.BunifuFlatButton1.Size = New System.Drawing.Size(301, 59)
        Me.BunifuFlatButton1.TabIndex = 2
        Me.BunifuFlatButton1.Text = "Utilitarios"
        Me.BunifuFlatButton1.UseVisualStyleBackColor = False
        '
        'BtnMantenimiento
        '
        Me.BtnMantenimiento.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(53, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BtnMantenimiento.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.BtnMantenimiento.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BtnMantenimiento.Location = New System.Drawing.Point(0, 128)
        Me.BtnMantenimiento.Margin = New System.Windows.Forms.Padding(5)
        Me.BtnMantenimiento.Name = "BtnMantenimiento"
        Me.BtnMantenimiento.Size = New System.Drawing.Size(301, 59)
        Me.BtnMantenimiento.TabIndex = 1
        Me.BtnMantenimiento.Text = "Mantenimientos"
        Me.BtnMantenimiento.UseVisualStyleBackColor = False
        '
        'Panel3
        '
        Me.Panel3.BackColor = System.Drawing.Color.FromArgb(CType(CType(214, Byte), Integer), CType(CType(219, Byte), Integer), CType(CType(233, Byte), Integer))
        Me.Panel3.Controls.Add(Me.PictureBox1)
        Me.Panel3.Controls.Add(Me.Label1)
        Me.Panel3.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel3.Location = New System.Drawing.Point(0, 0)
        Me.Panel3.Margin = New System.Windows.Forms.Padding(4)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(301, 70)
        Me.Panel3.TabIndex = 0
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.Transparent
        Me.PictureBox1.BackgroundImage = CType(resources.GetObject("PictureBox1.BackgroundImage"), System.Drawing.Image)
        Me.PictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PictureBox1.Location = New System.Drawing.Point(29, 15)
        Me.PictureBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(63, 48)
        Me.PictureBox1.TabIndex = 1
        Me.PictureBox1.TabStop = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Font = New System.Drawing.Font("Century Gothic", 15.75!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.White
        Me.Label1.Location = New System.Drawing.Point(117, 21)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(143, 33)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "SCEscolar"
        '
        'Panel2
        '
        Me.Panel2.BackColor = System.Drawing.Color.FromArgb(CType(CType(214, Byte), Integer), CType(CType(219, Byte), Integer), CType(CType(233, Byte), Integer))
        Me.Panel2.Controls.Add(Me.PictureBox3)
        Me.Panel2.Controls.Add(Me.Label2)
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel2.Location = New System.Drawing.Point(301, 32)
        Me.Panel2.Margin = New System.Windows.Forms.Padding(4)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(977, 70)
        Me.Panel2.TabIndex = 3
        '
        'PictureBox3
        '
        Me.PictureBox3.BackColor = System.Drawing.Color.Transparent
        Me.PictureBox3.BackgroundImage = CType(resources.GetObject("PictureBox3.BackgroundImage"), System.Drawing.Image)
        Me.PictureBox3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PictureBox3.Location = New System.Drawing.Point(8, 21)
        Me.PictureBox3.Margin = New System.Windows.Forms.Padding(4)
        Me.PictureBox3.Name = "PictureBox3"
        Me.PictureBox3.Size = New System.Drawing.Size(33, 31)
        Me.PictureBox3.TabIndex = 3
        Me.PictureBox3.TabStop = False
        Me.PictureBox3.Visible = False
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Century Gothic", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.ForeColor = System.Drawing.Color.DimGray
        Me.Label2.Location = New System.Drawing.Point(49, 22)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(160, 33)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Dashboard"
        Me.Label2.Visible = False
        '
        'BtnCerrar
        '
        Me.BtnCerrar.BackColor = System.Drawing.Color.Transparent
        Me.BtnCerrar.BackgroundImage = CType(resources.GetObject("BtnCerrar.BackgroundImage"), System.Drawing.Image)
        Me.BtnCerrar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.BtnCerrar.Location = New System.Drawing.Point(1241, 1)
        Me.BtnCerrar.Margin = New System.Windows.Forms.Padding(4)
        Me.BtnCerrar.Name = "BtnCerrar"
        Me.BtnCerrar.Size = New System.Drawing.Size(33, 31)
        Me.BtnCerrar.TabIndex = 2
        Me.BtnCerrar.TabStop = False
        '
        'FrmPrincipal
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(1440, 900)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.BtnCerrar)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.Name = "FrmPrincipal"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Sistema Control Servicio al Comedor"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.BtnCerrar, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents MantenimientoToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AyudaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UtilitariosToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UsuariosToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ControlDeMarcasToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ReportesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ReporteDiariosToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ImportarDatosPIADToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ImprimirToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ImportarDatosListaPIADToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents Panel1 As Panel
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Panel2 As Panel
    Friend WithEvents BtnMantenimiento As System.Windows.Forms.Button
    Friend WithEvents BunifuFlatButton3 As System.Windows.Forms.Button
    Friend WithEvents BunifuFlatButton2 As System.Windows.Forms.Button
    Friend WithEvents BunifuFlatButton1 As System.Windows.Forms.Button
    Friend WithEvents Label2 As Label
    Friend WithEvents Panel3 As Panel
    Friend WithEvents PictureBox3 As PictureBox
    Friend WithEvents BtnCerrar As PictureBox
    Friend WithEvents ReporteDeServicioTransporteToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GestiónRutasToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GestiónBecasToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents RecargasToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents ReporteProyecciónComedorToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ReporteEstudiantesBecadosToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AgregarEstudianteManualToolStripMenuItem As ToolStripMenuItem
End Class
