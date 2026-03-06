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
        Me.ParametrosSistemaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
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
        Me.PanelMenuLateral = New System.Windows.Forms.Panel()
        Me.BtnMenuDashboard = New System.Windows.Forms.Button()
        Me.BtnMenuReportes = New System.Windows.Forms.Button()
        Me.BtnMenuUtilitarios = New System.Windows.Forms.Button()
        Me.BtnMenuMantenimientos = New System.Windows.Forms.Button()
        Me.PanelSidebarHeader = New System.Windows.Forms.Panel()
        Me.PicSidebarLogo = New System.Windows.Forms.PictureBox()
        Me.LblSidebarTitulo = New System.Windows.Forms.Label()
        Me.PanelCabeceraModulo = New System.Windows.Forms.Panel()
        Me.PicDashboardIcon = New System.Windows.Forms.PictureBox()
        Me.LblDashboardTitulo = New System.Windows.Forms.Label()
        Me.BtnCerrar = New System.Windows.Forms.PictureBox()
        Me.MenuStrip1.SuspendLayout()
        Me.PanelMenuLateral.SuspendLayout()
        Me.PanelSidebarHeader.SuspendLayout()
        CType(Me.PicSidebarLogo, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PanelCabeceraModulo.SuspendLayout()
        CType(Me.PicDashboardIcon, System.ComponentModel.ISupportInitialize).BeginInit()
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
        Me.MenuStrip1.Size = New System.Drawing.Size(1440, 32)
        Me.MenuStrip1.TabIndex = 1
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'MantenimientoToolStripMenuItem
        '
        Me.MantenimientoToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.UsuariosToolStripMenuItem, Me.GestiónRutasToolStripMenuItem, Me.GestiónBecasToolStripMenuItem, Me.ParametrosSistemaToolStripMenuItem})
        Me.MantenimientoToolStripMenuItem.ForeColor = System.Drawing.SystemColors.Desktop
        Me.MantenimientoToolStripMenuItem.Name = "MantenimientoToolStripMenuItem"
        Me.MantenimientoToolStripMenuItem.Size = New System.Drawing.Size(155, 28)
        Me.MantenimientoToolStripMenuItem.Text = "Mantenimiento"
        '
        'UsuariosToolStripMenuItem
        '
        Me.UsuariosToolStripMenuItem.Name = "UsuariosToolStripMenuItem"
        Me.UsuariosToolStripMenuItem.Size = New System.Drawing.Size(260, 28)
        Me.UsuariosToolStripMenuItem.Text = "Estudiantes"
        '
        'GestiónRutasToolStripMenuItem
        '
        Me.GestiónRutasToolStripMenuItem.Name = "GestiónRutasToolStripMenuItem"
        Me.GestiónRutasToolStripMenuItem.Size = New System.Drawing.Size(260, 28)
        Me.GestiónRutasToolStripMenuItem.Text = "Gestión Rutas"
        '
        'GestiónBecasToolStripMenuItem
        '
        Me.GestiónBecasToolStripMenuItem.Name = "GestiónBecasToolStripMenuItem"
        Me.GestiónBecasToolStripMenuItem.Size = New System.Drawing.Size(260, 28)
        Me.GestiónBecasToolStripMenuItem.Text = "Gestión Becas"
        '
        'ParametrosSistemaToolStripMenuItem
        '
        Me.ParametrosSistemaToolStripMenuItem.Name = "ParametrosSistemaToolStripMenuItem"
        Me.ParametrosSistemaToolStripMenuItem.Size = New System.Drawing.Size(260, 28)
        Me.ParametrosSistemaToolStripMenuItem.Text = "Parámetros Sistema"
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
        'PanelMenuLateral
        '
        Me.PanelMenuLateral.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(51, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.PanelMenuLateral.Controls.Add(Me.BtnMenuDashboard)
        Me.PanelMenuLateral.Controls.Add(Me.BtnMenuReportes)
        Me.PanelMenuLateral.Controls.Add(Me.BtnMenuUtilitarios)
        Me.PanelMenuLateral.Controls.Add(Me.BtnMenuMantenimientos)
        Me.PanelMenuLateral.Controls.Add(Me.PanelSidebarHeader)
        Me.PanelMenuLateral.Dock = System.Windows.Forms.DockStyle.Left
        Me.PanelMenuLateral.Location = New System.Drawing.Point(0, 32)
        Me.PanelMenuLateral.Margin = New System.Windows.Forms.Padding(4)
        Me.PanelMenuLateral.Name = "PanelMenuLateral"
        Me.PanelMenuLateral.Size = New System.Drawing.Size(301, 868)
        Me.PanelMenuLateral.TabIndex = 2
        '
        'BtnMenuDashboard
        '
        Me.BtnMenuDashboard.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(53, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BtnMenuDashboard.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.BtnMenuDashboard.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BtnMenuDashboard.Location = New System.Drawing.Point(0, 70)
        Me.BtnMenuDashboard.Margin = New System.Windows.Forms.Padding(5)
        Me.BtnMenuDashboard.Name = "BtnMenuDashboard"
        Me.BtnMenuDashboard.Size = New System.Drawing.Size(301, 59)
        Me.BtnMenuDashboard.TabIndex = 4
        Me.BtnMenuDashboard.Text = "Dashboard"
        Me.BtnMenuDashboard.UseVisualStyleBackColor = False
        '
        'BtnMenuReportes
        '
        Me.BtnMenuReportes.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(53, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BtnMenuReportes.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.BtnMenuReportes.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BtnMenuReportes.Location = New System.Drawing.Point(0, 244)
        Me.BtnMenuReportes.Margin = New System.Windows.Forms.Padding(5)
        Me.BtnMenuReportes.Name = "BtnMenuReportes"
        Me.BtnMenuReportes.Size = New System.Drawing.Size(301, 59)
        Me.BtnMenuReportes.TabIndex = 3
        Me.BtnMenuReportes.Text = "Reportes"
        Me.BtnMenuReportes.UseVisualStyleBackColor = False
        '
        'BtnMenuUtilitarios
        '
        Me.BtnMenuUtilitarios.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(53, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BtnMenuUtilitarios.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.BtnMenuUtilitarios.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BtnMenuUtilitarios.Location = New System.Drawing.Point(0, 186)
        Me.BtnMenuUtilitarios.Margin = New System.Windows.Forms.Padding(5)
        Me.BtnMenuUtilitarios.Name = "BtnMenuUtilitarios"
        Me.BtnMenuUtilitarios.Size = New System.Drawing.Size(301, 59)
        Me.BtnMenuUtilitarios.TabIndex = 2
        Me.BtnMenuUtilitarios.Text = "Utilitarios"
        Me.BtnMenuUtilitarios.UseVisualStyleBackColor = False
        '
        'BtnMenuMantenimientos
        '
        Me.BtnMenuMantenimientos.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(53, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BtnMenuMantenimientos.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.BtnMenuMantenimientos.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BtnMenuMantenimientos.Location = New System.Drawing.Point(0, 128)
        Me.BtnMenuMantenimientos.Margin = New System.Windows.Forms.Padding(5)
        Me.BtnMenuMantenimientos.Name = "BtnMenuMantenimientos"
        Me.BtnMenuMantenimientos.Size = New System.Drawing.Size(301, 59)
        Me.BtnMenuMantenimientos.TabIndex = 1
        Me.BtnMenuMantenimientos.Text = "Mantenimientos"
        Me.BtnMenuMantenimientos.UseVisualStyleBackColor = False
        '
        'PanelSidebarHeader
        '
        Me.PanelSidebarHeader.BackColor = System.Drawing.Color.FromArgb(CType(CType(214, Byte), Integer), CType(CType(219, Byte), Integer), CType(CType(233, Byte), Integer))
        Me.PanelSidebarHeader.Controls.Add(Me.PicSidebarLogo)
        Me.PanelSidebarHeader.Controls.Add(Me.LblSidebarTitulo)
        Me.PanelSidebarHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.PanelSidebarHeader.Location = New System.Drawing.Point(0, 0)
        Me.PanelSidebarHeader.Margin = New System.Windows.Forms.Padding(4)
        Me.PanelSidebarHeader.Name = "PanelSidebarHeader"
        Me.PanelSidebarHeader.Size = New System.Drawing.Size(301, 70)
        Me.PanelSidebarHeader.TabIndex = 0
        '
        'PicSidebarLogo
        '
        Me.PicSidebarLogo.BackColor = System.Drawing.Color.Transparent
        Me.PicSidebarLogo.BackgroundImage = CType(resources.GetObject("PicSidebarLogo.BackgroundImage"), System.Drawing.Image)
        Me.PicSidebarLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PicSidebarLogo.Location = New System.Drawing.Point(29, 15)
        Me.PicSidebarLogo.Margin = New System.Windows.Forms.Padding(4)
        Me.PicSidebarLogo.Name = "PicSidebarLogo"
        Me.PicSidebarLogo.Size = New System.Drawing.Size(63, 48)
        Me.PicSidebarLogo.TabIndex = 1
        Me.PicSidebarLogo.TabStop = False
        '
        'LblSidebarTitulo
        '
        Me.LblSidebarTitulo.AutoSize = True
        Me.LblSidebarTitulo.BackColor = System.Drawing.Color.Transparent
        Me.LblSidebarTitulo.Font = New System.Drawing.Font("Century Gothic", 15.75!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblSidebarTitulo.ForeColor = System.Drawing.Color.White
        Me.LblSidebarTitulo.Location = New System.Drawing.Point(117, 21)
        Me.LblSidebarTitulo.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LblSidebarTitulo.Name = "LblSidebarTitulo"
        Me.LblSidebarTitulo.Size = New System.Drawing.Size(143, 33)
        Me.LblSidebarTitulo.TabIndex = 0
        Me.LblSidebarTitulo.Text = "SCEscolar"
        '
        'PanelCabeceraModulo
        '
        Me.PanelCabeceraModulo.BackColor = System.Drawing.Color.FromArgb(CType(CType(214, Byte), Integer), CType(CType(219, Byte), Integer), CType(CType(233, Byte), Integer))
        Me.PanelCabeceraModulo.Controls.Add(Me.PicDashboardIcon)
        Me.PanelCabeceraModulo.Controls.Add(Me.LblDashboardTitulo)
        Me.PanelCabeceraModulo.Dock = System.Windows.Forms.DockStyle.Top
        Me.PanelCabeceraModulo.Location = New System.Drawing.Point(301, 32)
        Me.PanelCabeceraModulo.Margin = New System.Windows.Forms.Padding(4)
        Me.PanelCabeceraModulo.Name = "PanelCabeceraModulo"
        Me.PanelCabeceraModulo.Size = New System.Drawing.Size(1139, 70)
        Me.PanelCabeceraModulo.TabIndex = 3
        '
        'PicDashboardIcon
        '
        Me.PicDashboardIcon.BackColor = System.Drawing.Color.Transparent
        Me.PicDashboardIcon.BackgroundImage = CType(resources.GetObject("PicDashboardIcon.BackgroundImage"), System.Drawing.Image)
        Me.PicDashboardIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PicDashboardIcon.Location = New System.Drawing.Point(8, 21)
        Me.PicDashboardIcon.Margin = New System.Windows.Forms.Padding(4)
        Me.PicDashboardIcon.Name = "PicDashboardIcon"
        Me.PicDashboardIcon.Size = New System.Drawing.Size(33, 31)
        Me.PicDashboardIcon.TabIndex = 3
        Me.PicDashboardIcon.TabStop = False
        Me.PicDashboardIcon.Visible = False
        '
        'LblDashboardTitulo
        '
        Me.LblDashboardTitulo.AutoSize = True
        Me.LblDashboardTitulo.Font = New System.Drawing.Font("Century Gothic", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblDashboardTitulo.ForeColor = System.Drawing.Color.DimGray
        Me.LblDashboardTitulo.Location = New System.Drawing.Point(49, 22)
        Me.LblDashboardTitulo.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LblDashboardTitulo.Name = "LblDashboardTitulo"
        Me.LblDashboardTitulo.Size = New System.Drawing.Size(160, 33)
        Me.LblDashboardTitulo.TabIndex = 1
        Me.LblDashboardTitulo.Text = "Dashboard"
        Me.LblDashboardTitulo.Visible = False
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
        Me.Controls.Add(Me.PanelCabeceraModulo)
        Me.Controls.Add(Me.BtnCerrar)
        Me.Controls.Add(Me.PanelMenuLateral)
        Me.Controls.Add(Me.MenuStrip1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.Name = "FrmPrincipal"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "procedamos "
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.PanelMenuLateral.ResumeLayout(False)
        Me.PanelSidebarHeader.ResumeLayout(False)
        Me.PanelSidebarHeader.PerformLayout()
        CType(Me.PicSidebarLogo, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PanelCabeceraModulo.ResumeLayout(False)
        Me.PanelCabeceraModulo.PerformLayout()
        CType(Me.PicDashboardIcon, System.ComponentModel.ISupportInitialize).EndInit()
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
    Friend WithEvents PanelMenuLateral As Panel
    Friend WithEvents PicSidebarLogo As PictureBox
    Friend WithEvents LblSidebarTitulo As Label
    Friend WithEvents PanelCabeceraModulo As Panel
    Friend WithEvents BtnMenuMantenimientos As System.Windows.Forms.Button
    Friend WithEvents BtnMenuDashboard As System.Windows.Forms.Button
    Friend WithEvents BtnMenuReportes As System.Windows.Forms.Button
    Friend WithEvents BtnMenuUtilitarios As System.Windows.Forms.Button
    Friend WithEvents LblDashboardTitulo As Label
    Friend WithEvents PanelSidebarHeader As Panel
    Friend WithEvents PicDashboardIcon As PictureBox
    Friend WithEvents BtnCerrar As PictureBox
    Friend WithEvents ReporteDeServicioTransporteToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GestiónRutasToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GestiónBecasToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents RecargasToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents ReporteProyecciónComedorToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ReporteEstudiantesBecadosToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AgregarEstudianteManualToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ParametrosSistemaToolStripMenuItem As ToolStripMenuItem
End Class
