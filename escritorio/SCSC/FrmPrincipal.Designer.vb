<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Public Partial Class FrmPrincipal
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
        Me.LblLegacySidebarBody = New System.Windows.Forms.Label()
        Me.LblLegacySidebarTitle = New System.Windows.Forms.Label()
        Me.PanelCabeceraModulo = New System.Windows.Forms.Panel()
        Me.LblLegacyHeader = New System.Windows.Forms.Label()
        Me.BtnCerrar = New System.Windows.Forms.Button()
        Me.PanelDesignSurface = New System.Windows.Forms.Panel()
        Me.PanelDesignCard = New System.Windows.Forms.Panel()
        Me.LblDesignHint = New System.Windows.Forms.Label()
        Me.LblDesignBody = New System.Windows.Forms.Label()
        Me.LblDesignTitle = New System.Windows.Forms.Label()
        Me.LblDesignCaption = New System.Windows.Forms.Label()
        Me.MenuStrip1.SuspendLayout()
        Me.PanelMenuLateral.SuspendLayout()
        Me.PanelCabeceraModulo.SuspendLayout()
        Me.PanelDesignSurface.SuspendLayout()
        Me.PanelDesignCard.SuspendLayout()
        Me.SuspendLayout()
        '
        'MenuStrip1
        '
        Me.MenuStrip1.BackColor = System.Drawing.Color.White
        Me.MenuStrip1.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.MenuStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MantenimientoToolStripMenuItem, Me.UtilitariosToolStripMenuItem, Me.ReportesToolStripMenuItem, Me.AyudaToolStripMenuItem, Me.ImprimirToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(1440, 31)
        Me.MenuStrip1.TabIndex = 0
        Me.MenuStrip1.Text = "MenuStrip1"
        Me.MenuStrip1.Visible = False
        '
        'MantenimientoToolStripMenuItem
        '
        Me.MantenimientoToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.UsuariosToolStripMenuItem, Me.GestiónRutasToolStripMenuItem, Me.GestiónBecasToolStripMenuItem, Me.ParametrosSistemaToolStripMenuItem})
        Me.MantenimientoToolStripMenuItem.Name = "MantenimientoToolStripMenuItem"
        Me.MantenimientoToolStripMenuItem.Size = New System.Drawing.Size(141, 27)
        Me.MantenimientoToolStripMenuItem.Text = "Mantenimiento"
        '
        'UsuariosToolStripMenuItem
        '
        Me.UsuariosToolStripMenuItem.Name = "UsuariosToolStripMenuItem"
        Me.UsuariosToolStripMenuItem.Size = New System.Drawing.Size(244, 28)
        Me.UsuariosToolStripMenuItem.Text = "Estudiantes"
        '
        'GestiónRutasToolStripMenuItem
        '
        Me.GestiónRutasToolStripMenuItem.Name = "GestiónRutasToolStripMenuItem"
        Me.GestiónRutasToolStripMenuItem.Size = New System.Drawing.Size(244, 28)
        Me.GestiónRutasToolStripMenuItem.Text = "Gestión Rutas"
        '
        'GestiónBecasToolStripMenuItem
        '
        Me.GestiónBecasToolStripMenuItem.Name = "GestiónBecasToolStripMenuItem"
        Me.GestiónBecasToolStripMenuItem.Size = New System.Drawing.Size(244, 28)
        Me.GestiónBecasToolStripMenuItem.Text = "Gestión Becas"
        '
        'ParametrosSistemaToolStripMenuItem
        '
        Me.ParametrosSistemaToolStripMenuItem.Name = "ParametrosSistemaToolStripMenuItem"
        Me.ParametrosSistemaToolStripMenuItem.Size = New System.Drawing.Size(244, 28)
        Me.ParametrosSistemaToolStripMenuItem.Text = "Parámetros Sistema"
        '
        'UtilitariosToolStripMenuItem
        '
        Me.UtilitariosToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ControlDeMarcasToolStripMenuItem, Me.ImportarDatosListaPIADToolStripMenuItem, Me.ImportarDatosPIADToolStripMenuItem, Me.AgregarEstudianteManualToolStripMenuItem, Me.RecargasToolStripMenuItem1})
        Me.UtilitariosToolStripMenuItem.Name = "UtilitariosToolStripMenuItem"
        Me.UtilitariosToolStripMenuItem.Size = New System.Drawing.Size(96, 27)
        Me.UtilitariosToolStripMenuItem.Text = "Utilitarios"
        '
        'ControlDeMarcasToolStripMenuItem
        '
        Me.ControlDeMarcasToolStripMenuItem.Name = "ControlDeMarcasToolStripMenuItem"
        Me.ControlDeMarcasToolStripMenuItem.Size = New System.Drawing.Size(320, 28)
        Me.ControlDeMarcasToolStripMenuItem.Text = "Control de Marcas Comedor"
        '
        'ImportarDatosListaPIADToolStripMenuItem
        '
        Me.ImportarDatosListaPIADToolStripMenuItem.Name = "ImportarDatosListaPIADToolStripMenuItem"
        Me.ImportarDatosListaPIADToolStripMenuItem.Size = New System.Drawing.Size(320, 28)
        Me.ImportarDatosListaPIADToolStripMenuItem.Text = "Control de Marcas Transporte"
        '
        'ImportarDatosPIADToolStripMenuItem
        '
        Me.ImportarDatosPIADToolStripMenuItem.Name = "ImportarDatosPIADToolStripMenuItem"
        Me.ImportarDatosPIADToolStripMenuItem.Size = New System.Drawing.Size(320, 28)
        Me.ImportarDatosPIADToolStripMenuItem.Text = "Importar Datos PIAD"
        '
        'AgregarEstudianteManualToolStripMenuItem
        '
        Me.AgregarEstudianteManualToolStripMenuItem.Name = "AgregarEstudianteManualToolStripMenuItem"
        Me.AgregarEstudianteManualToolStripMenuItem.Size = New System.Drawing.Size(320, 28)
        Me.AgregarEstudianteManualToolStripMenuItem.Text = "Agregar Estudiante Manual"
        '
        'RecargasToolStripMenuItem1
        '
        Me.RecargasToolStripMenuItem1.Name = "RecargasToolStripMenuItem1"
        Me.RecargasToolStripMenuItem1.Size = New System.Drawing.Size(320, 28)
        Me.RecargasToolStripMenuItem1.Text = "Recargas"
        '
        'ReportesToolStripMenuItem
        '
        Me.ReportesToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ReporteEstudiantesBecadosToolStripMenuItem, Me.ReporteDiariosToolStripMenuItem, Me.ReporteDeServicioTransporteToolStripMenuItem, Me.ReporteProyecciónComedorToolStripMenuItem})
        Me.ReportesToolStripMenuItem.Name = "ReportesToolStripMenuItem"
        Me.ReportesToolStripMenuItem.Size = New System.Drawing.Size(91, 27)
        Me.ReportesToolStripMenuItem.Text = "Reportes"
        '
        'ReporteEstudiantesBecadosToolStripMenuItem
        '
        Me.ReporteEstudiantesBecadosToolStripMenuItem.Name = "ReporteEstudiantesBecadosToolStripMenuItem"
        Me.ReporteEstudiantesBecadosToolStripMenuItem.Size = New System.Drawing.Size(327, 28)
        Me.ReporteEstudiantesBecadosToolStripMenuItem.Text = "Reporte Estudiantes Becados"
        '
        'ReporteDiariosToolStripMenuItem
        '
        Me.ReporteDiariosToolStripMenuItem.Name = "ReporteDiariosToolStripMenuItem"
        Me.ReporteDiariosToolStripMenuItem.Size = New System.Drawing.Size(327, 28)
        Me.ReporteDiariosToolStripMenuItem.Text = "Reporte Servicio Comedor"
        '
        'ReporteDeServicioTransporteToolStripMenuItem
        '
        Me.ReporteDeServicioTransporteToolStripMenuItem.Name = "ReporteDeServicioTransporteToolStripMenuItem"
        Me.ReporteDeServicioTransporteToolStripMenuItem.Size = New System.Drawing.Size(327, 28)
        Me.ReporteDeServicioTransporteToolStripMenuItem.Text = "Reporte de Servicio Transporte"
        '
        'ReporteProyecciónComedorToolStripMenuItem
        '
        Me.ReporteProyecciónComedorToolStripMenuItem.Name = "ReporteProyecciónComedorToolStripMenuItem"
        Me.ReporteProyecciónComedorToolStripMenuItem.Size = New System.Drawing.Size(327, 28)
        Me.ReporteProyecciónComedorToolStripMenuItem.Text = "Reporte Proyección Comedor"
        '
        'AyudaToolStripMenuItem
        '
        Me.AyudaToolStripMenuItem.Name = "AyudaToolStripMenuItem"
        Me.AyudaToolStripMenuItem.Size = New System.Drawing.Size(72, 27)
        Me.AyudaToolStripMenuItem.Text = "Ayuda"
        '
        'ImprimirToolStripMenuItem
        '
        Me.ImprimirToolStripMenuItem.Name = "ImprimirToolStripMenuItem"
        Me.ImprimirToolStripMenuItem.Size = New System.Drawing.Size(89, 27)
        Me.ImprimirToolStripMenuItem.Text = "Imprimir"
        '
        'PanelMenuLateral
        '
        Me.PanelMenuLateral.BackColor = System.Drawing.Color.FromArgb(CType(CType(16, Byte), Integer), CType(CType(26, Byte), Integer), CType(CType(46, Byte), Integer))
        Me.PanelMenuLateral.Controls.Add(Me.LblLegacySidebarBody)
        Me.PanelMenuLateral.Controls.Add(Me.LblLegacySidebarTitle)
        Me.PanelMenuLateral.Dock = System.Windows.Forms.DockStyle.Left
        Me.PanelMenuLateral.Location = New System.Drawing.Point(0, 0)
        Me.PanelMenuLateral.Name = "PanelMenuLateral"
        Me.PanelMenuLateral.Size = New System.Drawing.Size(280, 900)
        Me.PanelMenuLateral.TabIndex = 1
        Me.PanelMenuLateral.Visible = False
        '
        'LblLegacySidebarBody
        '
        Me.LblLegacySidebarBody.ForeColor = System.Drawing.Color.FromArgb(CType(CType(191, Byte), Integer), CType(CType(204, Byte), Integer), CType(CType(223, Byte), Integer))
        Me.LblLegacySidebarBody.Location = New System.Drawing.Point(24, 72)
        Me.LblLegacySidebarBody.Name = "LblLegacySidebarBody"
        Me.LblLegacySidebarBody.Size = New System.Drawing.Size(220, 72)
        Me.LblLegacySidebarBody.TabIndex = 1
        Me.LblLegacySidebarBody.Text = "Fallback clásico disponible si el shell moderno no logra inicializar."
        '
        'LblLegacySidebarTitle
        '
        Me.LblLegacySidebarTitle.AutoSize = True
        Me.LblLegacySidebarTitle.Font = New System.Drawing.Font("Segoe UI Semibold", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblLegacySidebarTitle.ForeColor = System.Drawing.Color.White
        Me.LblLegacySidebarTitle.Location = New System.Drawing.Point(24, 24)
        Me.LblLegacySidebarTitle.Name = "LblLegacySidebarTitle"
        Me.LblLegacySidebarTitle.Size = New System.Drawing.Size(164, 41)
        Me.LblLegacySidebarTitle.TabIndex = 0
        Me.LblLegacySidebarTitle.Text = "SCSC 2026"
        '
        'PanelCabeceraModulo
        '
        Me.PanelCabeceraModulo.BackColor = System.Drawing.Color.White
        Me.PanelCabeceraModulo.Controls.Add(Me.LblLegacyHeader)
        Me.PanelCabeceraModulo.Dock = System.Windows.Forms.DockStyle.Top
        Me.PanelCabeceraModulo.Location = New System.Drawing.Point(280, 0)
        Me.PanelCabeceraModulo.Name = "PanelCabeceraModulo"
        Me.PanelCabeceraModulo.Size = New System.Drawing.Size(1160, 80)
        Me.PanelCabeceraModulo.TabIndex = 2
        Me.PanelCabeceraModulo.Visible = False
        '
        'LblLegacyHeader
        '
        Me.LblLegacyHeader.AutoSize = True
        Me.LblLegacyHeader.Font = New System.Drawing.Font("Segoe UI Semibold", 16.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblLegacyHeader.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(32, Byte), Integer), CType(CType(51, Byte), Integer))
        Me.LblLegacyHeader.Location = New System.Drawing.Point(24, 22)
        Me.LblLegacyHeader.Name = "LblLegacyHeader"
        Me.LblLegacyHeader.Size = New System.Drawing.Size(196, 37)
        Me.LblLegacyHeader.TabIndex = 0
        Me.LblLegacyHeader.Text = "Panel principal"
        '
        'BtnCerrar
        '
        Me.BtnCerrar.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtnCerrar.BackColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(32, Byte), Integer), CType(CType(51, Byte), Integer))
        Me.BtnCerrar.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BtnCerrar.FlatAppearance.BorderSize = 0
        Me.BtnCerrar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(12, Byte), Integer), CType(CType(18, Byte), Integer), CType(CType(31, Byte), Integer))
        Me.BtnCerrar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(34, Byte), Integer), CType(CType(45, Byte), Integer), CType(CType(69, Byte), Integer))
        Me.BtnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BtnCerrar.Font = New System.Drawing.Font("Segoe UI Semibold", 14.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BtnCerrar.ForeColor = System.Drawing.Color.White
        Me.BtnCerrar.Location = New System.Drawing.Point(1388, 12)
        Me.BtnCerrar.Name = "BtnCerrar"
        Me.BtnCerrar.Size = New System.Drawing.Size(40, 36)
        Me.BtnCerrar.TabIndex = 3
        Me.BtnCerrar.Text = "×"
        Me.BtnCerrar.UseVisualStyleBackColor = False
        Me.BtnCerrar.Visible = False
        '
        'PanelDesignSurface
        '
        Me.PanelDesignSurface.BackColor = System.Drawing.Color.FromArgb(CType(CType(239, Byte), Integer), CType(CType(244, Byte), Integer), CType(CType(251, Byte), Integer))
        Me.PanelDesignSurface.Controls.Add(Me.PanelDesignCard)
        Me.PanelDesignSurface.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PanelDesignSurface.Location = New System.Drawing.Point(280, 80)
        Me.PanelDesignSurface.Name = "PanelDesignSurface"
        Me.PanelDesignSurface.Padding = New System.Windows.Forms.Padding(32)
        Me.PanelDesignSurface.Size = New System.Drawing.Size(1160, 820)
        Me.PanelDesignSurface.TabIndex = 4
        '
        'PanelDesignCard
        '
        Me.PanelDesignCard.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.PanelDesignCard.BackColor = System.Drawing.Color.White
        Me.PanelDesignCard.Controls.Add(Me.LblDesignHint)
        Me.PanelDesignCard.Controls.Add(Me.LblDesignBody)
        Me.PanelDesignCard.Controls.Add(Me.LblDesignTitle)
        Me.PanelDesignCard.Controls.Add(Me.LblDesignCaption)
        Me.PanelDesignCard.Location = New System.Drawing.Point(216, 178)
        Me.PanelDesignCard.Name = "PanelDesignCard"
        Me.PanelDesignCard.Size = New System.Drawing.Size(728, 360)
        Me.PanelDesignCard.TabIndex = 0
        '
        'LblDesignHint
        '
        Me.LblDesignHint.ForeColor = System.Drawing.Color.FromArgb(CType(CType(98, Byte), Integer), CType(CType(111, Byte), Integer), CType(CType(129, Byte), Integer))
        Me.LblDesignHint.Location = New System.Drawing.Point(48, 246)
        Me.LblDesignHint.Name = "LblDesignHint"
        Me.LblDesignHint.Size = New System.Drawing.Size(632, 50)
        Me.LblDesignHint.TabIndex = 3
        Me.LblDesignHint.Text = "El shell moderno se construye en runtime con UIShellHost. Este diseñador deja una" &
    " base limpia y mantiene fallback clásico oculto."
        '
        'LblDesignBody
        '
        Me.LblDesignBody.ForeColor = System.Drawing.Color.FromArgb(CType(CType(76, Byte), Integer), CType(CType(90, Byte), Integer), CType(CType(112, Byte), Integer))
        Me.LblDesignBody.Location = New System.Drawing.Point(48, 150)
        Me.LblDesignBody.Name = "LblDesignBody"
        Me.LblDesignBody.Size = New System.Drawing.Size(632, 68)
        Me.LblDesignBody.TabIndex = 2
        Me.LblDesignBody.Text = "Navegación, dashboard, permisos y estado de refresh se renderizan dinámicamente. " &
    "Usa esta vista para ajustar tamaño base, icono y comportamiento general del form" &
    "ulario."
        '
        'LblDesignTitle
        '
        Me.LblDesignTitle.AutoSize = True
        Me.LblDesignTitle.Font = New System.Drawing.Font("Segoe UI Semibold", 24.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblDesignTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(32, Byte), Integer), CType(CType(51, Byte), Integer))
        Me.LblDesignTitle.Location = New System.Drawing.Point(43, 78)
        Me.LblDesignTitle.Name = "LblDesignTitle"
        Me.LblDesignTitle.Size = New System.Drawing.Size(405, 54)
        Me.LblDesignTitle.TabIndex = 1
        Me.LblDesignTitle.Text = "Shell moderno activo"
        '
        'LblDesignCaption
        '
        Me.LblDesignCaption.AutoSize = True
        Me.LblDesignCaption.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblDesignCaption.ForeColor = System.Drawing.Color.FromArgb(CType(CType(39, Byte), Integer), CType(CType(99, Byte), Integer), CType(CType(235, Byte), Integer))
        Me.LblDesignCaption.Location = New System.Drawing.Point(48, 40)
        Me.LblDesignCaption.Name = "LblDesignCaption"
        Me.LblDesignCaption.Size = New System.Drawing.Size(176, 23)
        Me.LblDesignCaption.TabIndex = 0
        Me.LblDesignCaption.Text = "FRMPRINCIPAL 2026"
        '
        'FrmPrincipal
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(239, Byte), Integer), CType(CType(244, Byte), Integer), CType(CType(251, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(1440, 900)
        Me.Controls.Add(Me.PanelDesignSurface)
        Me.Controls.Add(Me.BtnCerrar)
        Me.Controls.Add(Me.PanelCabeceraModulo)
        Me.Controls.Add(Me.PanelMenuLateral)
        Me.Controls.Add(Me.MenuStrip1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.Name = "FrmPrincipal"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "SCSC - Panel Principal"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.PanelMenuLateral.ResumeLayout(False)
        Me.PanelMenuLateral.PerformLayout()
        Me.PanelCabeceraModulo.ResumeLayout(False)
        Me.PanelCabeceraModulo.PerformLayout()
        Me.PanelDesignSurface.ResumeLayout(False)
        Me.PanelDesignCard.ResumeLayout(False)
        Me.PanelDesignCard.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents MantenimientoToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UsuariosToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GestiónRutasToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GestiónBecasToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ParametrosSistemaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UtilitariosToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ControlDeMarcasToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ImportarDatosListaPIADToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ImportarDatosPIADToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AgregarEstudianteManualToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RecargasToolStripMenuItem1 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ReportesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ReporteEstudiantesBecadosToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ReporteDiariosToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ReporteDeServicioTransporteToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ReporteProyecciónComedorToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AyudaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ImprimirToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PanelMenuLateral As System.Windows.Forms.Panel
    Friend WithEvents PanelCabeceraModulo As System.Windows.Forms.Panel
    Friend WithEvents BtnCerrar As System.Windows.Forms.Button
    Friend WithEvents PanelDesignSurface As System.Windows.Forms.Panel
    Friend WithEvents PanelDesignCard As System.Windows.Forms.Panel
    Friend WithEvents LblDesignHint As System.Windows.Forms.Label
    Friend WithEvents LblDesignBody As System.Windows.Forms.Label
    Friend WithEvents LblDesignTitle As System.Windows.Forms.Label
    Friend WithEvents LblDesignCaption As System.Windows.Forms.Label
    Friend WithEvents LblLegacySidebarBody As System.Windows.Forms.Label
    Friend WithEvents LblLegacySidebarTitle As System.Windows.Forms.Label
    Friend WithEvents LblLegacyHeader As System.Windows.Forms.Label
End Class
