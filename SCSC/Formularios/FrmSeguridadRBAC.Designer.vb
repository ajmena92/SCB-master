<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmSeguridadRBAC
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
        Me.TabsPrincipal = New System.Windows.Forms.TabControl()
        Me.TabUsuarios = New System.Windows.Forms.TabPage()
        Me.PanelUsuariosBottom = New System.Windows.Forms.Panel()
        Me.LayoutUsuarios = New System.Windows.Forms.TableLayoutPanel()
        Me.LabelUsuarioNombre = New System.Windows.Forms.Label()
        Me._txtNombreUsuario = New System.Windows.Forms.TextBox()
        Me.LabelUsuarioCompleto = New System.Windows.Forms.Label()
        Me._txtNombreCompletoUsuario = New System.Windows.Forms.TextBox()
        Me.LabelUsuarioContrasena = New System.Windows.Forms.Label()
        Me._txtContrasenaUsuario = New System.Windows.Forms.TextBox()
        Me._chkUsuarioActivo = New System.Windows.Forms.CheckBox()
        Me.LabelRolAsignar = New System.Windows.Forms.Label()
        Me._cmbRolUsuario = New System.Windows.Forms.ComboBox()
        Me.LabelRolesUsuario = New System.Windows.Forms.Label()
        Me._lstRolesUsuario = New System.Windows.Forms.ListBox()
        Me.FlowUsuariosBotones = New System.Windows.Forms.FlowLayoutPanel()
        Me._btnCrearUsuario = New System.Windows.Forms.Button()
        Me._btnActualizarUsuario = New System.Windows.Forms.Button()
        Me._btnEliminarUsuario = New System.Windows.Forms.Button()
        Me._btnCambiarClave = New System.Windows.Forms.Button()
        Me._btnAsignarRolUsuario = New System.Windows.Forms.Button()
        Me._btnRevocarRolUsuario = New System.Windows.Forms.Button()
        Me._gridUsuarios = New System.Windows.Forms.DataGridView()
        Me.TabRoles = New System.Windows.Forms.TabPage()
        Me.PanelRolesBottom = New System.Windows.Forms.Panel()
        Me.LayoutRoles = New System.Windows.Forms.TableLayoutPanel()
        Me.LabelNombreRol = New System.Windows.Forms.Label()
        Me._txtNombreRol = New System.Windows.Forms.TextBox()
        Me.LabelDescripcionRol = New System.Windows.Forms.Label()
        Me._txtDescripcionRol = New System.Windows.Forms.TextBox()
        Me._chkRolActivo = New System.Windows.Forms.CheckBox()
        Me.LabelPermisoAsignar = New System.Windows.Forms.Label()
        Me._cmbPermisoRol = New System.Windows.Forms.ComboBox()
        Me.LabelPermisosRol = New System.Windows.Forms.Label()
        Me._lstPermisosRol = New System.Windows.Forms.ListBox()
        Me.FlowRolesBotones = New System.Windows.Forms.FlowLayoutPanel()
        Me._btnCrearRol = New System.Windows.Forms.Button()
        Me._btnActualizarRol = New System.Windows.Forms.Button()
        Me._btnEliminarRol = New System.Windows.Forms.Button()
        Me._btnAsignarPermisoRol = New System.Windows.Forms.Button()
        Me._btnRevocarPermisoRol = New System.Windows.Forms.Button()
        Me._gridRoles = New System.Windows.Forms.DataGridView()
        Me.TabPermisos = New System.Windows.Forms.TabPage()
        Me.PanelPermisosBottom = New System.Windows.Forms.Panel()
        Me.LayoutPermisos = New System.Windows.Forms.TableLayoutPanel()
        Me.LabelClavePermiso = New System.Windows.Forms.Label()
        Me._txtClavePermiso = New System.Windows.Forms.TextBox()
        Me.LabelDescripcionPermiso = New System.Windows.Forms.Label()
        Me._txtDescripcionPermiso = New System.Windows.Forms.TextBox()
        Me.FlowPermisosBotones = New System.Windows.Forms.FlowLayoutPanel()
        Me._btnCrearPermiso = New System.Windows.Forms.Button()
        Me._btnActualizarPermiso = New System.Windows.Forms.Button()
        Me._btnEliminarPermiso = New System.Windows.Forms.Button()
        Me._gridPermisos = New System.Windows.Forms.DataGridView()
        Me.PanelFooter = New System.Windows.Forms.Panel()
        Me._btnSalir = New System.Windows.Forms.Button()
        Me.TabsPrincipal.SuspendLayout()
        Me.TabUsuarios.SuspendLayout()
        Me.PanelUsuariosBottom.SuspendLayout()
        Me.LayoutUsuarios.SuspendLayout()
        Me.FlowUsuariosBotones.SuspendLayout()
        CType(Me._gridUsuarios, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabRoles.SuspendLayout()
        Me.PanelRolesBottom.SuspendLayout()
        Me.LayoutRoles.SuspendLayout()
        Me.FlowRolesBotones.SuspendLayout()
        CType(Me._gridRoles, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPermisos.SuspendLayout()
        Me.PanelPermisosBottom.SuspendLayout()
        Me.LayoutPermisos.SuspendLayout()
        Me.FlowPermisosBotones.SuspendLayout()
        CType(Me._gridPermisos, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PanelFooter.SuspendLayout()
        Me.SuspendLayout()
        '
        'TabsPrincipal
        '
        Me.TabsPrincipal.Controls.Add(Me.TabUsuarios)
        Me.TabsPrincipal.Controls.Add(Me.TabRoles)
        Me.TabsPrincipal.Controls.Add(Me.TabPermisos)
        Me.TabsPrincipal.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TabsPrincipal.Location = New System.Drawing.Point(0, 0)
        Me.TabsPrincipal.Name = "TabsPrincipal"
        Me.TabsPrincipal.SelectedIndex = 0
        Me.TabsPrincipal.Size = New System.Drawing.Size(940, 532)
        Me.TabsPrincipal.TabIndex = 0
        '
        'TabUsuarios
        '
        Me.TabUsuarios.Controls.Add(Me.PanelUsuariosBottom)
        Me.TabUsuarios.Controls.Add(Me._gridUsuarios)
        Me.TabUsuarios.Location = New System.Drawing.Point(4, 25)
        Me.TabUsuarios.Name = "TabUsuarios"
        Me.TabUsuarios.Padding = New System.Windows.Forms.Padding(3)
        Me.TabUsuarios.Size = New System.Drawing.Size(932, 551)
        Me.TabUsuarios.TabIndex = 0
        Me.TabUsuarios.Text = "Usuarios"
        Me.TabUsuarios.UseVisualStyleBackColor = True
        '
        'PanelUsuariosBottom
        '
        Me.PanelUsuariosBottom.AutoScroll = True
        Me.PanelUsuariosBottom.Controls.Add(Me.LayoutUsuarios)
        Me.PanelUsuariosBottom.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PanelUsuariosBottom.Location = New System.Drawing.Point(3, 223)
        Me.PanelUsuariosBottom.Name = "PanelUsuariosBottom"
        Me.PanelUsuariosBottom.Padding = New System.Windows.Forms.Padding(8)
        Me.PanelUsuariosBottom.Size = New System.Drawing.Size(926, 325)
        Me.PanelUsuariosBottom.TabIndex = 1
        '
        'LayoutUsuarios
        '
        Me.LayoutUsuarios.ColumnCount = 4
        Me.LayoutUsuarios.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.0!))
        Me.LayoutUsuarios.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.LayoutUsuarios.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.0!))
        Me.LayoutUsuarios.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.LayoutUsuarios.Controls.Add(Me.LabelUsuarioNombre, 0, 0)
        Me.LayoutUsuarios.Controls.Add(Me._txtNombreUsuario, 1, 0)
        Me.LayoutUsuarios.Controls.Add(Me.LabelUsuarioCompleto, 2, 0)
        Me.LayoutUsuarios.Controls.Add(Me._txtNombreCompletoUsuario, 3, 0)
        Me.LayoutUsuarios.Controls.Add(Me.LabelUsuarioContrasena, 0, 1)
        Me.LayoutUsuarios.Controls.Add(Me._txtContrasenaUsuario, 1, 1)
        Me.LayoutUsuarios.Controls.Add(Me._chkUsuarioActivo, 3, 1)
        Me.LayoutUsuarios.Controls.Add(Me.LabelRolAsignar, 0, 2)
        Me.LayoutUsuarios.Controls.Add(Me._cmbRolUsuario, 1, 2)
        Me.LayoutUsuarios.Controls.Add(Me.LabelRolesUsuario, 2, 2)
        Me.LayoutUsuarios.Controls.Add(Me._lstRolesUsuario, 3, 2)
        Me.LayoutUsuarios.Controls.Add(Me.FlowUsuariosBotones, 0, 3)
        Me.LayoutUsuarios.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LayoutUsuarios.Location = New System.Drawing.Point(8, 8)
        Me.LayoutUsuarios.Name = "LayoutUsuarios"
        Me.LayoutUsuarios.RowCount = 4
        Me.LayoutUsuarios.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
        Me.LayoutUsuarios.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
        Me.LayoutUsuarios.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110.0!))
        Me.LayoutUsuarios.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.LayoutUsuarios.Size = New System.Drawing.Size(910, 309)
        Me.LayoutUsuarios.TabIndex = 0
        Me.LayoutUsuarios.SetColumnSpan(Me.FlowUsuariosBotones, 4)
        '
        'Usuario controls
        '
        Me.LabelUsuarioNombre.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.LabelUsuarioNombre.AutoSize = True
        Me.LabelUsuarioNombre.Text = "NombreUsuario"
        Me._txtNombreUsuario.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LabelUsuarioCompleto.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.LabelUsuarioCompleto.AutoSize = True
        Me.LabelUsuarioCompleto.Text = "NombreCompleto"
        Me._txtNombreCompletoUsuario.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LabelUsuarioContrasena.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.LabelUsuarioContrasena.AutoSize = True
        Me.LabelUsuarioContrasena.Text = "Contraseña"
        Me._txtContrasenaUsuario.Dock = System.Windows.Forms.DockStyle.Fill
        Me._chkUsuarioActivo.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me._chkUsuarioActivo.Text = "EsActivo"
        Me._chkUsuarioActivo.Checked = True
        Me.LabelRolAsignar.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.LabelRolAsignar.AutoSize = True
        Me.LabelRolAsignar.Text = "Rol para asignar"
        Me._cmbRolUsuario.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LabelRolesUsuario.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.LabelRolesUsuario.AutoSize = True
        Me.LabelRolesUsuario.Text = "Roles del usuario"
        Me._lstRolesUsuario.Dock = System.Windows.Forms.DockStyle.Fill

        Me.FlowUsuariosBotones.AutoSize = True
        Me.FlowUsuariosBotones.WrapContents = True
        Me.FlowUsuariosBotones.Dock = System.Windows.Forms.DockStyle.Top
        Me.FlowUsuariosBotones.Controls.Add(Me._btnCrearUsuario)
        Me.FlowUsuariosBotones.Controls.Add(Me._btnActualizarUsuario)
        Me.FlowUsuariosBotones.Controls.Add(Me._btnEliminarUsuario)
        Me.FlowUsuariosBotones.Controls.Add(Me._btnCambiarClave)
        Me.FlowUsuariosBotones.Controls.Add(Me._btnAsignarRolUsuario)
        Me.FlowUsuariosBotones.Controls.Add(Me._btnRevocarRolUsuario)

        Me._btnCrearUsuario.Text = "Crear"
        Me._btnActualizarUsuario.Text = "Actualizar"
        Me._btnEliminarUsuario.Text = "Eliminar"
        Me._btnCambiarClave.Text = "Cambiar Clave"
        Me._btnAsignarRolUsuario.Text = "Asignar Rol"
        Me._btnRevocarRolUsuario.Text = "Revocar Rol"

        Me._gridUsuarios.Dock = System.Windows.Forms.DockStyle.Top
        Me._gridUsuarios.Location = New System.Drawing.Point(3, 3)
        Me._gridUsuarios.Name = "_gridUsuarios"
        Me._gridUsuarios.Size = New System.Drawing.Size(926, 220)
        Me._gridUsuarios.TabIndex = 0

        'TabRoles
        Me.TabRoles.Controls.Add(Me.PanelRolesBottom)
        Me.TabRoles.Controls.Add(Me._gridRoles)
        Me.TabRoles.Location = New System.Drawing.Point(4, 25)
        Me.TabRoles.Name = "TabRoles"
        Me.TabRoles.Padding = New System.Windows.Forms.Padding(3)
        Me.TabRoles.Size = New System.Drawing.Size(932, 551)
        Me.TabRoles.TabIndex = 1
        Me.TabRoles.Text = "Roles"
        Me.TabRoles.UseVisualStyleBackColor = True

        Me.PanelRolesBottom.AutoScroll = True
        Me.PanelRolesBottom.Controls.Add(Me.LayoutRoles)
        Me.PanelRolesBottom.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PanelRolesBottom.Location = New System.Drawing.Point(3, 223)
        Me.PanelRolesBottom.Name = "PanelRolesBottom"
        Me.PanelRolesBottom.Padding = New System.Windows.Forms.Padding(8)
        Me.PanelRolesBottom.Size = New System.Drawing.Size(926, 325)

        Me.LayoutRoles.ColumnCount = 4
        Me.LayoutRoles.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.0!))
        Me.LayoutRoles.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.LayoutRoles.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.0!))
        Me.LayoutRoles.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.LayoutRoles.Controls.Add(Me.LabelNombreRol, 0, 0)
        Me.LayoutRoles.Controls.Add(Me._txtNombreRol, 1, 0)
        Me.LayoutRoles.Controls.Add(Me.LabelDescripcionRol, 2, 0)
        Me.LayoutRoles.Controls.Add(Me._txtDescripcionRol, 3, 0)
        Me.LayoutRoles.Controls.Add(Me._chkRolActivo, 3, 1)
        Me.LayoutRoles.Controls.Add(Me.LabelPermisoAsignar, 0, 2)
        Me.LayoutRoles.Controls.Add(Me._cmbPermisoRol, 1, 2)
        Me.LayoutRoles.Controls.Add(Me.LabelPermisosRol, 2, 2)
        Me.LayoutRoles.Controls.Add(Me._lstPermisosRol, 3, 2)
        Me.LayoutRoles.Controls.Add(Me.FlowRolesBotones, 0, 3)
        Me.LayoutRoles.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LayoutRoles.Location = New System.Drawing.Point(8, 8)
        Me.LayoutRoles.Name = "LayoutRoles"
        Me.LayoutRoles.RowCount = 4
        Me.LayoutRoles.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
        Me.LayoutRoles.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
        Me.LayoutRoles.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110.0!))
        Me.LayoutRoles.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.LayoutRoles.Size = New System.Drawing.Size(910, 309)
        Me.LayoutRoles.SetColumnSpan(Me.FlowRolesBotones, 4)

        Me.LabelNombreRol.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.LabelNombreRol.AutoSize = True
        Me.LabelNombreRol.Text = "NombreRol"
        Me._txtNombreRol.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LabelDescripcionRol.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.LabelDescripcionRol.AutoSize = True
        Me.LabelDescripcionRol.Text = "Descripcion"
        Me._txtDescripcionRol.Dock = System.Windows.Forms.DockStyle.Fill
        Me._chkRolActivo.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me._chkRolActivo.Text = "EsActivo"
        Me._chkRolActivo.Checked = True
        Me.LabelPermisoAsignar.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.LabelPermisoAsignar.AutoSize = True
        Me.LabelPermisoAsignar.Text = "Permiso para asignar"
        Me._cmbPermisoRol.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LabelPermisosRol.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.LabelPermisosRol.AutoSize = True
        Me.LabelPermisosRol.Text = "Permisos del rol"
        Me._lstPermisosRol.Dock = System.Windows.Forms.DockStyle.Fill

        Me.FlowRolesBotones.AutoSize = True
        Me.FlowRolesBotones.WrapContents = True
        Me.FlowRolesBotones.Dock = System.Windows.Forms.DockStyle.Top
        Me.FlowRolesBotones.Controls.Add(Me._btnCrearRol)
        Me.FlowRolesBotones.Controls.Add(Me._btnActualizarRol)
        Me.FlowRolesBotones.Controls.Add(Me._btnEliminarRol)
        Me.FlowRolesBotones.Controls.Add(Me._btnAsignarPermisoRol)
        Me.FlowRolesBotones.Controls.Add(Me._btnRevocarPermisoRol)

        Me._btnCrearRol.Text = "Crear"
        Me._btnActualizarRol.Text = "Actualizar"
        Me._btnEliminarRol.Text = "Eliminar"
        Me._btnAsignarPermisoRol.Text = "Asignar Permiso"
        Me._btnRevocarPermisoRol.Text = "Revocar Permiso"

        Me._gridRoles.Dock = System.Windows.Forms.DockStyle.Top
        Me._gridRoles.Location = New System.Drawing.Point(3, 3)
        Me._gridRoles.Name = "_gridRoles"
        Me._gridRoles.Size = New System.Drawing.Size(926, 220)

        'TabPermisos
        Me.TabPermisos.Controls.Add(Me.PanelPermisosBottom)
        Me.TabPermisos.Controls.Add(Me._gridPermisos)
        Me.TabPermisos.Location = New System.Drawing.Point(4, 25)
        Me.TabPermisos.Name = "TabPermisos"
        Me.TabPermisos.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPermisos.Size = New System.Drawing.Size(932, 551)
        Me.TabPermisos.TabIndex = 2
        Me.TabPermisos.Text = "Permisos"
        Me.TabPermisos.UseVisualStyleBackColor = True

        Me.PanelPermisosBottom.AutoScroll = True
        Me.PanelPermisosBottom.Controls.Add(Me.LayoutPermisos)
        Me.PanelPermisosBottom.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PanelPermisosBottom.Location = New System.Drawing.Point(3, 223)
        Me.PanelPermisosBottom.Name = "PanelPermisosBottom"
        Me.PanelPermisosBottom.Padding = New System.Windows.Forms.Padding(8)
        Me.PanelPermisosBottom.Size = New System.Drawing.Size(926, 325)

        Me.LayoutPermisos.ColumnCount = 2
        Me.LayoutPermisos.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.LayoutPermisos.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75.0!))
        Me.LayoutPermisos.Controls.Add(Me.LabelClavePermiso, 0, 0)
        Me.LayoutPermisos.Controls.Add(Me._txtClavePermiso, 1, 0)
        Me.LayoutPermisos.Controls.Add(Me.LabelDescripcionPermiso, 0, 1)
        Me.LayoutPermisos.Controls.Add(Me._txtDescripcionPermiso, 1, 1)
        Me.LayoutPermisos.Controls.Add(Me.FlowPermisosBotones, 0, 2)
        Me.LayoutPermisos.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LayoutPermisos.Location = New System.Drawing.Point(8, 8)
        Me.LayoutPermisos.Name = "LayoutPermisos"
        Me.LayoutPermisos.RowCount = 3
        Me.LayoutPermisos.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
        Me.LayoutPermisos.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
        Me.LayoutPermisos.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.LayoutPermisos.Size = New System.Drawing.Size(910, 309)
        Me.LayoutPermisos.SetColumnSpan(Me.FlowPermisosBotones, 2)

        Me.LabelClavePermiso.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.LabelClavePermiso.AutoSize = True
        Me.LabelClavePermiso.Text = "ClavePermiso"
        Me._txtClavePermiso.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LabelDescripcionPermiso.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.LabelDescripcionPermiso.AutoSize = True
        Me.LabelDescripcionPermiso.Text = "Descripcion"
        Me._txtDescripcionPermiso.Dock = System.Windows.Forms.DockStyle.Fill

        Me.FlowPermisosBotones.AutoSize = True
        Me.FlowPermisosBotones.WrapContents = True
        Me.FlowPermisosBotones.Dock = System.Windows.Forms.DockStyle.Top
        Me.FlowPermisosBotones.Controls.Add(Me._btnCrearPermiso)
        Me.FlowPermisosBotones.Controls.Add(Me._btnActualizarPermiso)
        Me.FlowPermisosBotones.Controls.Add(Me._btnEliminarPermiso)

        Me._btnCrearPermiso.Text = "Crear"
        Me._btnActualizarPermiso.Text = "Actualizar"
        Me._btnEliminarPermiso.Text = "Eliminar"

        Me._gridPermisos.Dock = System.Windows.Forms.DockStyle.Top
        Me._gridPermisos.Location = New System.Drawing.Point(3, 3)
        Me._gridPermisos.Name = "_gridPermisos"
        Me._gridPermisos.Size = New System.Drawing.Size(926, 220)

        'PanelFooter
        Me.PanelFooter.Controls.Add(Me._btnSalir)
        Me.PanelFooter.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.PanelFooter.Location = New System.Drawing.Point(0, 532)
        Me.PanelFooter.Name = "PanelFooter"
        Me.PanelFooter.Padding = New System.Windows.Forms.Padding(8)
        Me.PanelFooter.Size = New System.Drawing.Size(940, 48)
        Me.PanelFooter.TabIndex = 1

        '_btnSalir
        Me._btnSalir.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me._btnSalir.Location = New System.Drawing.Point(828, 8)
        Me._btnSalir.Name = "_btnSalir"
        Me._btnSalir.Size = New System.Drawing.Size(100, 32)
        Me._btnSalir.TabIndex = 0
        Me._btnSalir.Text = "Salir"
        Me._btnSalir.UseVisualStyleBackColor = True

        'Form
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.ClientSize = New System.Drawing.Size(940, 580)
        Me.Controls.Add(Me.TabsPrincipal)
        Me.Controls.Add(Me.PanelFooter)
        Me.ControlBox = False
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmSeguridadRBAC"
        Me.Text = "Gestión de Seguridad RBAC"
        Me.TabsPrincipal.ResumeLayout(False)
        Me.TabUsuarios.ResumeLayout(False)
        Me.PanelUsuariosBottom.ResumeLayout(False)
        Me.LayoutUsuarios.ResumeLayout(False)
        Me.LayoutUsuarios.PerformLayout()
        Me.FlowUsuariosBotones.ResumeLayout(False)
        Me.FlowUsuariosBotones.PerformLayout()
        CType(Me._gridUsuarios, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabRoles.ResumeLayout(False)
        Me.PanelRolesBottom.ResumeLayout(False)
        Me.LayoutRoles.ResumeLayout(False)
        Me.LayoutRoles.PerformLayout()
        Me.FlowRolesBotones.ResumeLayout(False)
        Me.FlowRolesBotones.PerformLayout()
        CType(Me._gridRoles, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPermisos.ResumeLayout(False)
        Me.PanelPermisosBottom.ResumeLayout(False)
        Me.LayoutPermisos.ResumeLayout(False)
        Me.LayoutPermisos.PerformLayout()
        Me.FlowPermisosBotones.ResumeLayout(False)
        Me.FlowPermisosBotones.PerformLayout()
        CType(Me._gridPermisos, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PanelFooter.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabsPrincipal As TabControl
    Friend WithEvents TabUsuarios As TabPage
    Friend WithEvents TabRoles As TabPage
    Friend WithEvents TabPermisos As TabPage

    Friend WithEvents PanelUsuariosBottom As Panel
    Friend WithEvents LayoutUsuarios As TableLayoutPanel
    Friend WithEvents LabelUsuarioNombre As Label
    Friend WithEvents LabelUsuarioCompleto As Label
    Friend WithEvents LabelUsuarioContrasena As Label
    Friend WithEvents LabelRolAsignar As Label
    Friend WithEvents LabelRolesUsuario As Label
    Friend WithEvents _gridUsuarios As DataGridView
    Friend WithEvents _txtNombreUsuario As TextBox
    Friend WithEvents _txtNombreCompletoUsuario As TextBox
    Friend WithEvents _txtContrasenaUsuario As TextBox
    Friend WithEvents _chkUsuarioActivo As CheckBox
    Friend WithEvents _cmbRolUsuario As ComboBox
    Friend WithEvents _lstRolesUsuario As ListBox
    Friend WithEvents FlowUsuariosBotones As FlowLayoutPanel
    Friend WithEvents _btnCrearUsuario As Button
    Friend WithEvents _btnActualizarUsuario As Button
    Friend WithEvents _btnEliminarUsuario As Button
    Friend WithEvents _btnCambiarClave As Button
    Friend WithEvents _btnAsignarRolUsuario As Button
    Friend WithEvents _btnRevocarRolUsuario As Button

    Friend WithEvents PanelRolesBottom As Panel
    Friend WithEvents LayoutRoles As TableLayoutPanel
    Friend WithEvents LabelNombreRol As Label
    Friend WithEvents LabelDescripcionRol As Label
    Friend WithEvents LabelPermisoAsignar As Label
    Friend WithEvents LabelPermisosRol As Label
    Friend WithEvents _gridRoles As DataGridView
    Friend WithEvents _txtNombreRol As TextBox
    Friend WithEvents _txtDescripcionRol As TextBox
    Friend WithEvents _chkRolActivo As CheckBox
    Friend WithEvents _cmbPermisoRol As ComboBox
    Friend WithEvents _lstPermisosRol As ListBox
    Friend WithEvents FlowRolesBotones As FlowLayoutPanel
    Friend WithEvents _btnCrearRol As Button
    Friend WithEvents _btnActualizarRol As Button
    Friend WithEvents _btnEliminarRol As Button
    Friend WithEvents _btnAsignarPermisoRol As Button
    Friend WithEvents _btnRevocarPermisoRol As Button

    Friend WithEvents PanelPermisosBottom As Panel
    Friend WithEvents LayoutPermisos As TableLayoutPanel
    Friend WithEvents LabelClavePermiso As Label
    Friend WithEvents LabelDescripcionPermiso As Label
    Friend WithEvents _gridPermisos As DataGridView
    Friend WithEvents _txtClavePermiso As TextBox
    Friend WithEvents _txtDescripcionPermiso As TextBox
    Friend WithEvents FlowPermisosBotones As FlowLayoutPanel
    Friend WithEvents _btnCrearPermiso As Button
    Friend WithEvents _btnActualizarPermiso As Button
    Friend WithEvents _btnEliminarPermiso As Button
    Friend WithEvents PanelFooter As Panel
    Friend WithEvents _btnSalir As Button
End Class
