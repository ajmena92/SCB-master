<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmRecarga
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmRecarga))
        Me.GroupDatosCompra = New System.Windows.Forms.GroupBox()
        Me.LblTipoUsuario = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.LblTipoBeca = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.LblTotal = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.LblCantidadRecarga = New System.Windows.Forms.Label()
        Me.TxtRecarga = New System.Windows.Forms.TextBox()
        Me.LblCantTiques = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.TxtPrimerApellido = New System.Windows.Forms.TextBox()
        Me.TxtSegundoApellido = New System.Windows.Forms.TextBox()
        Me.TxtNombre = New System.Windows.Forms.TextBox()
        Me.LblNombreBusqueda = New System.Windows.Forms.Label()
        Me.PanelAcciones = New System.Windows.Forms.Panel()
        Me.BtnRegresar = New System.Windows.Forms.Button()
        Me.BtnCancelar = New System.Windows.Forms.Button()
        Me.BtnGuardar = New System.Windows.Forms.Button()
        Me.LblTituloModulo = New System.Windows.Forms.Label()
        Me.GroupDatosUsuario = New System.Windows.Forms.GroupBox()
        Me.txtCedula = New System.Windows.Forms.TextBox()
        Me.PicUsuario = New System.Windows.Forms.PictureBox()
        Me.Buscar = New System.Windows.Forms.Button()
        Me.LblCedulaBusqueda = New System.Windows.Forms.Label()
        Me.Imprimir = New System.Windows.Forms.Button()
        Me.GroupDatosCompra.SuspendLayout()
        Me.PanelAcciones.SuspendLayout()
        Me.GroupDatosUsuario.SuspendLayout()
        CType(Me.PicUsuario, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'GroupDatosCompra
        '
        Me.GroupDatosCompra.BackColor = System.Drawing.Color.Transparent
        Me.GroupDatosCompra.Controls.Add(Me.LblTipoUsuario)
        Me.GroupDatosCompra.Controls.Add(Me.Label9)
        Me.GroupDatosCompra.Controls.Add(Me.LblTipoBeca)
        Me.GroupDatosCompra.Controls.Add(Me.Label5)
        Me.GroupDatosCompra.Controls.Add(Me.LblTotal)
        Me.GroupDatosCompra.Controls.Add(Me.Label7)
        Me.GroupDatosCompra.Controls.Add(Me.LblCantidadRecarga)
        Me.GroupDatosCompra.Controls.Add(Me.TxtRecarga)
        Me.GroupDatosCompra.Controls.Add(Me.LblCantTiques)
        Me.GroupDatosCompra.Controls.Add(Me.Label8)
        Me.GroupDatosCompra.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupDatosCompra.ForeColor = System.Drawing.Color.Black
        Me.GroupDatosCompra.Location = New System.Drawing.Point(18, 170)
        Me.GroupDatosCompra.Name = "GroupDatosCompra"
        Me.GroupDatosCompra.Size = New System.Drawing.Size(903, 168)
        Me.GroupDatosCompra.TabIndex = 21
        Me.GroupDatosCompra.TabStop = False
        Me.GroupDatosCompra.Text = "Datos Compra"
        '
        'LblTipoUsuario
        '
        Me.LblTipoUsuario.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblTipoUsuario.Font = New System.Drawing.Font("Calibri", 16.2!)
        Me.LblTipoUsuario.ForeColor = System.Drawing.Color.Black
        Me.LblTipoUsuario.ImageAlign = System.Drawing.ContentAlignment.TopCenter
        Me.LblTipoUsuario.Location = New System.Drawing.Point(494, 26)
        Me.LblTipoUsuario.Name = "LblTipoUsuario"
        Me.LblTipoUsuario.Padding = New System.Windows.Forms.Padding(2)
        Me.LblTipoUsuario.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.LblTipoUsuario.Size = New System.Drawing.Size(167, 52)
        Me.LblTipoUsuario.TabIndex = 58
        Me.LblTipoUsuario.Text = " "
        Me.LblTipoUsuario.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.BackColor = System.Drawing.Color.Transparent
        Me.Label9.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label9.Font = New System.Drawing.Font("Arial Narrow", 12.0!)
        Me.Label9.Location = New System.Drawing.Point(431, 103)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(57, 24)
        Me.Label9.TabIndex = 57
        Me.Label9.Text = "Beca: "
        '
        'LblTipoBeca
        '
        Me.LblTipoBeca.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblTipoBeca.Font = New System.Drawing.Font("Calibri", 16.2!)
        Me.LblTipoBeca.ForeColor = System.Drawing.Color.Black
        Me.LblTipoBeca.ImageAlign = System.Drawing.ContentAlignment.TopCenter
        Me.LblTipoBeca.Location = New System.Drawing.Point(494, 86)
        Me.LblTipoBeca.Name = "LblTipoBeca"
        Me.LblTipoBeca.Padding = New System.Windows.Forms.Padding(2)
        Me.LblTipoBeca.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.LblTipoBeca.Size = New System.Drawing.Size(167, 52)
        Me.LblTipoBeca.TabIndex = 56
        Me.LblTipoBeca.Text = " "
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.BackColor = System.Drawing.Color.Transparent
        Me.Label5.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label5.Font = New System.Drawing.Font("Arial Narrow", 12.0!)
        Me.Label5.Location = New System.Drawing.Point(382, 39)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(106, 24)
        Me.Label5.TabIndex = 55
        Me.Label5.Text = "Tipo Usuario:"
        '
        'LblTotal
        '
        Me.LblTotal.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblTotal.Font = New System.Drawing.Font("Calibri", 16.2!, System.Drawing.FontStyle.Bold)
        Me.LblTotal.ForeColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.LblTotal.Location = New System.Drawing.Point(168, 95)
        Me.LblTotal.Name = "LblTotal"
        Me.LblTotal.Padding = New System.Windows.Forms.Padding(2)
        Me.LblTotal.Size = New System.Drawing.Size(177, 58)
        Me.LblTotal.TabIndex = 54
        Me.LblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.BackColor = System.Drawing.Color.Transparent
        Me.Label7.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label7.Font = New System.Drawing.Font("Arial Narrow", 12.0!)
        Me.Label7.Location = New System.Drawing.Point(49, 114)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(113, 24)
        Me.Label7.TabIndex = 53
        Me.Label7.Text = "Total a Pagar:"
        '
        'LblCantidadRecarga
        '
        Me.LblCantidadRecarga.AutoSize = True
        Me.LblCantidadRecarga.BackColor = System.Drawing.Color.Transparent
        Me.LblCantidadRecarga.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.LblCantidadRecarga.Font = New System.Drawing.Font("Arial Narrow", 12.0!)
        Me.LblCantidadRecarga.Location = New System.Drawing.Point(4, 39)
        Me.LblCantidadRecarga.Name = "LblCantidadRecarga"
        Me.LblCantidadRecarga.Size = New System.Drawing.Size(167, 24)
        Me.LblCantidadRecarga.TabIndex = 52
        Me.LblCantidadRecarga.Text = "Tiquetes a Recargar :"
        '
        'TxtRecarga
        '
        Me.TxtRecarga.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtRecarga.Font = New System.Drawing.Font("Calibri", 16.2!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtRecarga.Location = New System.Drawing.Point(168, 38)
        Me.TxtRecarga.MaxLength = 14
        Me.TxtRecarga.Name = "TxtRecarga"
        Me.TxtRecarga.Size = New System.Drawing.Size(158, 40)
        Me.TxtRecarga.TabIndex = 51
        '
        'LblCantTiques
        '
        Me.LblCantTiques.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblCantTiques.Font = New System.Drawing.Font("Arial", 13.75!, System.Drawing.FontStyle.Bold)
        Me.LblCantTiques.ForeColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.LblCantTiques.Location = New System.Drawing.Point(745, 69)
        Me.LblCantTiques.Name = "LblCantTiques"
        Me.LblCantTiques.Padding = New System.Windows.Forms.Padding(20, 20, 15, 15)
        Me.LblCantTiques.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.LblCantTiques.Size = New System.Drawing.Size(138, 69)
        Me.LblCantTiques.TabIndex = 50
        Me.LblCantTiques.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.BackColor = System.Drawing.Color.Transparent
        Me.Label8.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label8.Font = New System.Drawing.Font("Arial Narrow", 12.0!)
        Me.Label8.Location = New System.Drawing.Point(742, 26)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(141, 24)
        Me.Label8.TabIndex = 49
        Me.Label8.Text = "Total de  Tiquetes"
        '
        'TxtPrimerApellido
        '
        Me.TxtPrimerApellido.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtPrimerApellido.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtPrimerApellido.Location = New System.Drawing.Point(569, 31)
        Me.TxtPrimerApellido.MaxLength = 14
        Me.TxtPrimerApellido.Name = "TxtPrimerApellido"
        Me.TxtPrimerApellido.ReadOnly = True
        Me.TxtPrimerApellido.Size = New System.Drawing.Size(149, 30)
        Me.TxtPrimerApellido.TabIndex = 45
        '
        'TxtSegundoApellido
        '
        Me.TxtSegundoApellido.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtSegundoApellido.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtSegundoApellido.Location = New System.Drawing.Point(740, 30)
        Me.TxtSegundoApellido.MaxLength = 14
        Me.TxtSegundoApellido.Name = "TxtSegundoApellido"
        Me.TxtSegundoApellido.ReadOnly = True
        Me.TxtSegundoApellido.Size = New System.Drawing.Size(149, 30)
        Me.TxtSegundoApellido.TabIndex = 43
        '
        'TxtNombre
        '
        Me.TxtNombre.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtNombre.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtNombre.Location = New System.Drawing.Point(401, 31)
        Me.TxtNombre.MaxLength = 14
        Me.TxtNombre.Name = "TxtNombre"
        Me.TxtNombre.ReadOnly = True
        Me.TxtNombre.Size = New System.Drawing.Size(149, 30)
        Me.TxtNombre.TabIndex = 41
        '
        'LblNombreBusqueda
        '
        Me.LblNombreBusqueda.AutoSize = True
        Me.LblNombreBusqueda.BackColor = System.Drawing.Color.Transparent
        Me.LblNombreBusqueda.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.LblNombreBusqueda.Font = New System.Drawing.Font("Arial Narrow", 12.0!)
        Me.LblNombreBusqueda.Location = New System.Drawing.Point(320, 33)
        Me.LblNombreBusqueda.Name = "LblNombreBusqueda"
        Me.LblNombreBusqueda.Size = New System.Drawing.Size(74, 24)
        Me.LblNombreBusqueda.TabIndex = 42
        Me.LblNombreBusqueda.Text = "Nombre:"
        '
        'PanelAcciones
        '
        Me.PanelAcciones.BackColor = System.Drawing.Color.Transparent
        Me.PanelAcciones.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PanelAcciones.Controls.Add(Me.BtnRegresar)
        Me.PanelAcciones.Controls.Add(Me.BtnCancelar)
        Me.PanelAcciones.Controls.Add(Me.BtnGuardar)
        Me.PanelAcciones.Location = New System.Drawing.Point(724, 344)
        Me.PanelAcciones.Name = "PanelAcciones"
        Me.PanelAcciones.Size = New System.Drawing.Size(197, 72)
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
        Me.BtnCancelar.Location = New System.Drawing.Point(67, 2)
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
        Me.LblTituloModulo.Font = New System.Drawing.Font("Microsoft Sans Serif", 21.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTituloModulo.ForeColor = System.Drawing.Color.Black
        Me.LblTituloModulo.Location = New System.Drawing.Point(331, 9)
        Me.LblTituloModulo.Name = "LblTituloModulo"
        Me.LblTituloModulo.Size = New System.Drawing.Size(312, 42)
        Me.LblTituloModulo.TabIndex = 20
        Me.LblTituloModulo.Text = "Modulo Recargas"
        Me.LblTituloModulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'GroupDatosUsuario
        '
        Me.GroupDatosUsuario.BackColor = System.Drawing.Color.Transparent
        Me.GroupDatosUsuario.Controls.Add(Me.txtCedula)
        Me.GroupDatosUsuario.Controls.Add(Me.PicUsuario)
        Me.GroupDatosUsuario.Controls.Add(Me.Buscar)
        Me.GroupDatosUsuario.Controls.Add(Me.LblCedulaBusqueda)
        Me.GroupDatosUsuario.Controls.Add(Me.TxtPrimerApellido)
        Me.GroupDatosUsuario.Controls.Add(Me.LblNombreBusqueda)
        Me.GroupDatosUsuario.Controls.Add(Me.TxtNombre)
        Me.GroupDatosUsuario.Controls.Add(Me.TxtSegundoApellido)
        Me.GroupDatosUsuario.Location = New System.Drawing.Point(18, 50)
        Me.GroupDatosUsuario.Name = "GroupDatosUsuario"
        Me.GroupDatosUsuario.Size = New System.Drawing.Size(903, 86)
        Me.GroupDatosUsuario.TabIndex = 22
        Me.GroupDatosUsuario.TabStop = False
        Me.GroupDatosUsuario.Text = "Datos del Usuario"
        '
        'txtCedula
        '
        Me.txtCedula.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.txtCedula.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtCedula.Location = New System.Drawing.Point(133, 38)
        Me.txtCedula.MaxLength = 50
        Me.txtCedula.Name = "txtCedula"
        Me.txtCedula.Size = New System.Drawing.Size(120, 30)
        Me.txtCedula.TabIndex = 39
        '
        'PicUsuario
        '
        Me.PicUsuario.BackColor = System.Drawing.Color.Transparent
        Me.PicUsuario.Image = Global.SCSC.My.Resources.Resources.Users
        Me.PicUsuario.Location = New System.Drawing.Point(18, 33)
        Me.PicUsuario.Name = "PicUsuario"
        Me.PicUsuario.Size = New System.Drawing.Size(37, 37)
        Me.PicUsuario.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PicUsuario.TabIndex = 47
        Me.PicUsuario.TabStop = False
        '
        'Buscar
        '
        Me.Buscar.AccessibleRole = System.Windows.Forms.AccessibleRole.ButtonMenu
        Me.Buscar.BackColor = System.Drawing.Color.Transparent
        Me.Buscar.BackgroundImage = Global.SCSC.My.Resources.Resources.Buscar_30x30
        Me.Buscar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.Buscar.FlatAppearance.BorderColor = System.Drawing.Color.WhiteSmoke
        Me.Buscar.FlatAppearance.BorderSize = 0
        Me.Buscar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.Buscar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.Buscar.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Buscar.ForeColor = System.Drawing.Color.White
        Me.Buscar.Location = New System.Drawing.Point(259, 40)
        Me.Buscar.Name = "Buscar"
        Me.Buscar.Size = New System.Drawing.Size(27, 27)
        Me.Buscar.TabIndex = 41
        Me.Buscar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.Buscar.UseVisualStyleBackColor = False
        '
        'LblCedulaBusqueda
        '
        Me.LblCedulaBusqueda.AutoSize = True
        Me.LblCedulaBusqueda.BackColor = System.Drawing.Color.Transparent
        Me.LblCedulaBusqueda.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.LblCedulaBusqueda.Font = New System.Drawing.Font("Arial Narrow", 12.0!)
        Me.LblCedulaBusqueda.Location = New System.Drawing.Point(61, 40)
        Me.LblCedulaBusqueda.Name = "LblCedulaBusqueda"
        Me.LblCedulaBusqueda.Size = New System.Drawing.Size(66, 24)
        Me.LblCedulaBusqueda.TabIndex = 40
        Me.LblCedulaBusqueda.Text = "Cédula:"
        '
        'Imprimir
        '
        Me.Imprimir.Location = New System.Drawing.Point(101, 360)
        Me.Imprimir.Name = "Imprimir"
        Me.Imprimir.Size = New System.Drawing.Size(140, 35)
        Me.Imprimir.TabIndex = 48
        Me.Imprimir.Text = "Imprimir Recibo"
        Me.Imprimir.UseVisualStyleBackColor = True
        Me.Imprimir.Visible = False
        '
        'FrmRecarga
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(10.0!, 24.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(933, 432)
        Me.Controls.Add(Me.Imprimir)
        Me.Controls.Add(Me.GroupDatosUsuario)
        Me.Controls.Add(Me.GroupDatosCompra)
        Me.Controls.Add(Me.PanelAcciones)
        Me.Controls.Add(Me.LblTituloModulo)
        Me.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmRecarga"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Mantenimiento de estudiantes "
        Me.GroupDatosCompra.ResumeLayout(False)
        Me.GroupDatosCompra.PerformLayout()
        Me.PanelAcciones.ResumeLayout(False)
        Me.GroupDatosUsuario.ResumeLayout(False)
        Me.GroupDatosUsuario.PerformLayout()
        CType(Me.PicUsuario, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents GroupDatosCompra As System.Windows.Forms.GroupBox
    Friend WithEvents PanelAcciones As System.Windows.Forms.Panel
    Friend WithEvents BtnCancelar As System.Windows.Forms.Button
    Friend WithEvents BtnGuardar As System.Windows.Forms.Button
    Friend WithEvents LblTituloModulo As System.Windows.Forms.Label
    Friend WithEvents GroupDatosUsuario As System.Windows.Forms.GroupBox
    Friend WithEvents txtCedula As System.Windows.Forms.TextBox
    Friend WithEvents Buscar As System.Windows.Forms.Button
    Friend WithEvents LblCedulaBusqueda As System.Windows.Forms.Label
    Friend WithEvents TxtPrimerApellido As System.Windows.Forms.TextBox
    Friend WithEvents TxtSegundoApellido As System.Windows.Forms.TextBox
    Friend WithEvents TxtNombre As System.Windows.Forms.TextBox
    Friend WithEvents LblNombreBusqueda As System.Windows.Forms.Label
    Friend WithEvents LblCantTiques As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents LblCantidadRecarga As System.Windows.Forms.Label
    Friend WithEvents TxtRecarga As System.Windows.Forms.TextBox
    Friend WithEvents LblTotal As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents BtnRegresar As System.Windows.Forms.Button
    Friend WithEvents PicUsuario As System.Windows.Forms.PictureBox
    Friend WithEvents Imprimir As System.Windows.Forms.Button
    Friend WithEvents LblTipoUsuario As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents LblTipoBeca As Label
    Friend WithEvents Label5 As Label
End Class
