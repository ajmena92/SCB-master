<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ControlTransporte
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
        Me.components = New System.ComponentModel.Container()
        Dim PromptLabel As System.Windows.Forms.Label
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ControlTransporte))
        Me.LblTitulo = New System.Windows.Forms.Label()
        Me.StatusLine = New System.Windows.Forms.Label()
        Me.Prompt = New System.Windows.Forms.TextBox()
        Me.LblRuta = New System.Windows.Forms.Label()
        Me.TxtSeccion = New System.Windows.Forms.TextBox()
        Me.StatusText = New System.Windows.Forms.TextBox()
        Me.lblProcesando = New System.Windows.Forms.Label()
        Me.BunifuGradientPanel1 = New System.Windows.Forms.Panel()
        Me.LblFecha = New System.Windows.Forms.Label()
        Me.GbDatos = New System.Windows.Forms.GroupBox()
        Me.TxtPermisoSalida = New System.Windows.Forms.TextBox()
        Me.LblPermisoSalidaCaption = New System.Windows.Forms.Label()
        Me.TxtTipo = New System.Windows.Forms.TextBox()
        Me.TxtRuta = New System.Windows.Forms.TextBox()
        Me.LblRutaCaption = New System.Windows.Forms.Label()
        Me.LblTipoCaption = New System.Windows.Forms.Label()
        Me.LblCedula = New System.Windows.Forms.TextBox()
        Me.LblCedulaCaption = New System.Windows.Forms.Label()
        Me.LblSeccionCaption = New System.Windows.Forms.Label()
        Me.TxtUsuario = New System.Windows.Forms.TextBox()
        Me.LblUsuarioCaption = New System.Windows.Forms.Label()
        Me.Picture = New System.Windows.Forms.PictureBox()
        Me.TxtCedula = New System.Windows.Forms.TextBox()
        Me.Imgprocess = New System.Windows.Forms.PictureBox()
        Me.PanelResult = New System.Windows.Forms.Panel()
        Me.BtnCerrar = New System.Windows.Forms.PictureBox()
        Me.PanelTopBar = New System.Windows.Forms.Panel()
        PromptLabel = New System.Windows.Forms.Label()
        Me.BunifuGradientPanel1.SuspendLayout()
        Me.GbDatos.SuspendLayout()
        CType(Me.Picture, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Imgprocess, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PanelResult.SuspendLayout()
        CType(Me.BtnCerrar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PanelTopBar.SuspendLayout()
        Me.SuspendLayout()
        '
        'PromptLabel
        '
        PromptLabel.AutoSize = True
        PromptLabel.Location = New System.Drawing.Point(767, 464)
        PromptLabel.Name = "PromptLabel"
        PromptLabel.Size = New System.Drawing.Size(77, 24)
        PromptLabel.TabIndex = 15
        PromptLabel.Text = "Prompt:"
        PromptLabel.Visible = False
        '
        'LblTitulo
        '
        Me.LblTitulo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblTitulo.BackColor = System.Drawing.Color.Transparent
        Me.LblTitulo.Font = New System.Drawing.Font("Arial Narrow", 61.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTitulo.ForeColor = System.Drawing.Color.Black
        Me.LblTitulo.Location = New System.Drawing.Point(3, 3)
        Me.LblTitulo.Name = "LblTitulo"
        Me.LblTitulo.Size = New System.Drawing.Size(590, 284)
        Me.LblTitulo.TabIndex = 1
        Me.LblTitulo.Text = "Sistema de Control de Marcas"
        Me.LblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'StatusLine
        '
        Me.StatusLine.AutoSize = True
        Me.StatusLine.Location = New System.Drawing.Point(859, 433)
        Me.StatusLine.Name = "StatusLine"
        Me.StatusLine.Size = New System.Drawing.Size(110, 24)
        Me.StatusLine.TabIndex = 19
        Me.StatusLine.Text = "[Status line]"
        Me.StatusLine.Visible = False
        '
        'Prompt
        '
        Me.Prompt.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Prompt.Location = New System.Drawing.Point(6, 252)
        Me.Prompt.Name = "Prompt"
        Me.Prompt.ReadOnly = True
        Me.Prompt.Size = New System.Drawing.Size(269, 32)
        Me.Prompt.TabIndex = 16
        Me.Prompt.Visible = False
        '
        'LblRuta
        '
        Me.LblRuta.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblRuta.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblRuta.Font = New System.Drawing.Font("Calibri", 36.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblRuta.ForeColor = System.Drawing.Color.White
        Me.LblRuta.Location = New System.Drawing.Point(0, 3)
        Me.LblRuta.Name = "LblRuta"
        Me.LblRuta.Padding = New System.Windows.Forms.Padding(20, 15, 15, 15)
        Me.LblRuta.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.LblRuta.Size = New System.Drawing.Size(593, 138)
        Me.LblRuta.TabIndex = 55
        Me.LblRuta.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'TxtSeccion
        '
        Me.TxtSeccion.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TxtSeccion.BackColor = System.Drawing.Color.White
        Me.TxtSeccion.Font = New System.Drawing.Font("Calibri", 18.0!)
        Me.TxtSeccion.Location = New System.Drawing.Point(24, 260)
        Me.TxtSeccion.Name = "TxtSeccion"
        Me.TxtSeccion.ReadOnly = True
        Me.TxtSeccion.Size = New System.Drawing.Size(236, 44)
        Me.TxtSeccion.TabIndex = 8
        '
        'StatusText
        '
        Me.StatusText.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.StatusText.BackColor = System.Drawing.SystemColors.Window
        Me.StatusText.Location = New System.Drawing.Point(321, 0)
        Me.StatusText.Multiline = True
        Me.StatusText.Name = "StatusText"
        Me.StatusText.ReadOnly = True
        Me.StatusText.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.StatusText.Size = New System.Drawing.Size(269, 203)
        Me.StatusText.TabIndex = 18
        Me.StatusText.Visible = False
        '
        'lblProcesando
        '
        Me.lblProcesando.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblProcesando.Font = New System.Drawing.Font("Arial Narrow", 48.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblProcesando.ForeColor = System.Drawing.Color.Black
        Me.lblProcesando.Location = New System.Drawing.Point(0, 287)
        Me.lblProcesando.Name = "lblProcesando"
        Me.lblProcesando.Size = New System.Drawing.Size(590, 286)
        Me.lblProcesando.TabIndex = 27
        Me.lblProcesando.Text = "Por favor, coloque su identificador digital en el dispositivo"
        Me.lblProcesando.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        'BunifuGradientPanel1
        '
        Me.BunifuGradientPanel1.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(51, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BunifuGradientPanel1.BackgroundImage = CType(resources.GetObject("BunifuGradientPanel1.BackgroundImage"), System.Drawing.Image)
        Me.BunifuGradientPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.BunifuGradientPanel1.Controls.Add(Me.LblFecha)
        Me.BunifuGradientPanel1.Controls.Add(Me.GbDatos)
        Me.BunifuGradientPanel1.Controls.Add(Me.Picture)
        Me.BunifuGradientPanel1.Controls.Add(Me.TxtCedula)
        Me.BunifuGradientPanel1.Dock = System.Windows.Forms.DockStyle.Left
        Me.BunifuGradientPanel1.Location = New System.Drawing.Point(0, 0)
        Me.BunifuGradientPanel1.Name = "BunifuGradientPanel1"
        Me.BunifuGradientPanel1.Size = New System.Drawing.Size(447, 784)
        Me.BunifuGradientPanel1.TabIndex = 30
        '
        'LblFecha
        '
        Me.LblFecha.BackColor = System.Drawing.Color.Transparent
        Me.LblFecha.Font = New System.Drawing.Font("Calibri", 36.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblFecha.ForeColor = System.Drawing.Color.White
        Me.LblFecha.Location = New System.Drawing.Point(0, 0)
        Me.LblFecha.Name = "LblFecha"
        Me.LblFecha.Size = New System.Drawing.Size(447, 68)
        Me.LblFecha.TabIndex = 21
        Me.LblFecha.Text = "Fecha"
        Me.LblFecha.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'GbDatos
        '
        Me.GbDatos.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GbDatos.BackColor = System.Drawing.Color.Transparent
        Me.GbDatos.Controls.Add(Me.TxtPermisoSalida)
        Me.GbDatos.Controls.Add(Me.LblPermisoSalidaCaption)
        Me.GbDatos.Controls.Add(Me.TxtTipo)
        Me.GbDatos.Controls.Add(Me.TxtRuta)
        Me.GbDatos.Controls.Add(Me.LblRutaCaption)
        Me.GbDatos.Controls.Add(Me.LblTipoCaption)
        Me.GbDatos.Controls.Add(Me.LblCedula)
        Me.GbDatos.Controls.Add(Me.LblCedulaCaption)
        Me.GbDatos.Controls.Add(Me.LblSeccionCaption)
        Me.GbDatos.Controls.Add(Me.TxtSeccion)
        Me.GbDatos.Controls.Add(Me.TxtUsuario)
        Me.GbDatos.Controls.Add(Me.LblUsuarioCaption)
        Me.GbDatos.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GbDatos.ForeColor = System.Drawing.Color.White
        Me.GbDatos.Location = New System.Drawing.Point(12, 306)
        Me.GbDatos.Name = "GbDatos"
        Me.GbDatos.Size = New System.Drawing.Size(420, 466)
        Me.GbDatos.TabIndex = 20
        Me.GbDatos.TabStop = False
        '
        'TxtPermisoSalida
        '
        Me.TxtPermisoSalida.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TxtPermisoSalida.BackColor = System.Drawing.Color.White
        Me.TxtPermisoSalida.Font = New System.Drawing.Font("Calibri", 18.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtPermisoSalida.ForeColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.TxtPermisoSalida.Location = New System.Drawing.Point(19, 416)
        Me.TxtPermisoSalida.Name = "TxtPermisoSalida"
        Me.TxtPermisoSalida.ReadOnly = True
        Me.TxtPermisoSalida.Size = New System.Drawing.Size(241, 44)
        Me.TxtPermisoSalida.TabIndex = 59
        '
        'LblPermisoSalidaCaption
        '
        Me.LblPermisoSalidaCaption.AutoSize = True
        Me.LblPermisoSalidaCaption.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblPermisoSalidaCaption.ForeColor = System.Drawing.Color.White
        Me.LblPermisoSalidaCaption.Location = New System.Drawing.Point(20, 394)
        Me.LblPermisoSalidaCaption.Name = "LblPermisoSalidaCaption"
        Me.LblPermisoSalidaCaption.Size = New System.Drawing.Size(137, 24)
        Me.LblPermisoSalidaCaption.TabIndex = 58
        Me.LblPermisoSalidaCaption.Text = "Permiso Salida:"
        '
        'TxtTipo
        '
        Me.TxtTipo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TxtTipo.BackColor = System.Drawing.Color.White
        Me.TxtTipo.Font = New System.Drawing.Font("Calibri", 18.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtTipo.Location = New System.Drawing.Point(24, 187)
        Me.TxtTipo.Name = "TxtTipo"
        Me.TxtTipo.ReadOnly = True
        Me.TxtTipo.Size = New System.Drawing.Size(236, 44)
        Me.TxtTipo.TabIndex = 7
        '
        'TxtRuta
        '
        Me.TxtRuta.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TxtRuta.BackColor = System.Drawing.Color.White
        Me.TxtRuta.Font = New System.Drawing.Font("Calibri", 18.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtRuta.ForeColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.TxtRuta.Location = New System.Drawing.Point(19, 332)
        Me.TxtRuta.Name = "TxtRuta"
        Me.TxtRuta.ReadOnly = True
        Me.TxtRuta.Size = New System.Drawing.Size(241, 44)
        Me.TxtRuta.TabIndex = 57
        '
        'LblRutaCaption
        '
        Me.LblRutaCaption.AutoSize = True
        Me.LblRutaCaption.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblRutaCaption.ForeColor = System.Drawing.Color.White
        Me.LblRutaCaption.Location = New System.Drawing.Point(20, 310)
        Me.LblRutaCaption.Name = "LblRutaCaption"
        Me.LblRutaCaption.Size = New System.Drawing.Size(54, 24)
        Me.LblRutaCaption.TabIndex = 56
        Me.LblRutaCaption.Text = "Ruta:"
        '
        'LblTipoCaption
        '
        Me.LblTipoCaption.AutoSize = True
        Me.LblTipoCaption.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTipoCaption.ForeColor = System.Drawing.Color.White
        Me.LblTipoCaption.Location = New System.Drawing.Point(20, 165)
        Me.LblTipoCaption.Name = "LblTipoCaption"
        Me.LblTipoCaption.Size = New System.Drawing.Size(52, 24)
        Me.LblTipoCaption.TabIndex = 6
        Me.LblTipoCaption.Text = "Tipo:"
        '
        'LblCedula
        '
        Me.LblCedula.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblCedula.BackColor = System.Drawing.Color.White
        Me.LblCedula.Font = New System.Drawing.Font("Calibri", 18.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblCedula.Location = New System.Drawing.Point(19, 45)
        Me.LblCedula.Name = "LblCedula"
        Me.LblCedula.ReadOnly = True
        Me.LblCedula.Size = New System.Drawing.Size(241, 44)
        Me.LblCedula.TabIndex = 5
        '
        'LblCedulaCaption
        '
        Me.LblCedulaCaption.AutoSize = True
        Me.LblCedulaCaption.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblCedulaCaption.ForeColor = System.Drawing.Color.White
        Me.LblCedulaCaption.Location = New System.Drawing.Point(20, 23)
        Me.LblCedulaCaption.Name = "LblCedulaCaption"
        Me.LblCedulaCaption.Size = New System.Drawing.Size(73, 24)
        Me.LblCedulaCaption.TabIndex = 4
        Me.LblCedulaCaption.Text = "Cedula:"
        '
        'LblSeccionCaption
        '
        Me.LblSeccionCaption.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblSeccionCaption.AutoSize = True
        Me.LblSeccionCaption.Font = New System.Drawing.Font("Calibri", 12.0!)
        Me.LblSeccionCaption.ForeColor = System.Drawing.Color.White
        Me.LblSeccionCaption.Location = New System.Drawing.Point(20, 238)
        Me.LblSeccionCaption.Name = "LblSeccionCaption"
        Me.LblSeccionCaption.Size = New System.Drawing.Size(77, 24)
        Me.LblSeccionCaption.TabIndex = 2
        Me.LblSeccionCaption.Text = "Sección:"
        '
        'TxtUsuario
        '
        Me.TxtUsuario.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TxtUsuario.BackColor = System.Drawing.Color.White
        Me.TxtUsuario.Font = New System.Drawing.Font("Calibri", 18.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtUsuario.Location = New System.Drawing.Point(19, 116)
        Me.TxtUsuario.Name = "TxtUsuario"
        Me.TxtUsuario.ReadOnly = True
        Me.TxtUsuario.Size = New System.Drawing.Size(395, 44)
        Me.TxtUsuario.TabIndex = 1
        '
        'LblUsuarioCaption
        '
        Me.LblUsuarioCaption.AutoSize = True
        Me.LblUsuarioCaption.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblUsuarioCaption.ForeColor = System.Drawing.Color.White
        Me.LblUsuarioCaption.Location = New System.Drawing.Point(20, 94)
        Me.LblUsuarioCaption.Name = "LblUsuarioCaption"
        Me.LblUsuarioCaption.Size = New System.Drawing.Size(80, 24)
        Me.LblUsuarioCaption.TabIndex = 0
        Me.LblUsuarioCaption.Text = "Usuario:"
        '
        'Picture
        '
        Me.Picture.BackColor = System.Drawing.SystemColors.Window
        Me.Picture.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Picture.Location = New System.Drawing.Point(83, 81)
        Me.Picture.Name = "Picture"
        Me.Picture.Size = New System.Drawing.Size(263, 219)
        Me.Picture.TabIndex = 14
        Me.Picture.TabStop = False
        '
        'TxtCedula
        '
        Me.TxtCedula.Location = New System.Drawing.Point(116, 81)
        Me.TxtCedula.MaxLength = 50
        Me.TxtCedula.Name = "TxtCedula"
        Me.TxtCedula.Size = New System.Drawing.Size(195, 32)
        Me.TxtCedula.TabIndex = 29
        '
        'Imgprocess
        '
        Me.Imgprocess.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.Imgprocess.BackColor = System.Drawing.Color.Transparent
        Me.Imgprocess.Image = Global.SCSC.My.Resources.Resources.Ingreso
        Me.Imgprocess.Location = New System.Drawing.Point(249, 576)
        Me.Imgprocess.Name = "Imgprocess"
        Me.Imgprocess.Size = New System.Drawing.Size(123, 118)
        Me.Imgprocess.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.Imgprocess.TabIndex = 24
        Me.Imgprocess.TabStop = False
        'PanelResult
        '
        Me.PanelResult.BackColor = System.Drawing.Color.Gainsboro
        Me.PanelResult.Controls.Add(Me.Imgprocess)
        Me.PanelResult.Controls.Add(Me.lblProcesando)
        Me.PanelResult.Controls.Add(Me.LblTitulo)
        Me.PanelResult.Controls.Add(Me.StatusText)
        Me.PanelResult.Controls.Add(Me.LblRuta)
        Me.PanelResult.Controls.Add(Me.Prompt)
        Me.PanelResult.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PanelResult.Location = New System.Drawing.Point(447, 52)
        Me.PanelResult.Name = "PanelResult"
        Me.PanelResult.Size = New System.Drawing.Size(593, 732)
        Me.PanelResult.TabIndex = 57
        '
        'BtnCerrar
        '
        Me.BtnCerrar.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtnCerrar.BackColor = System.Drawing.Color.Transparent
        Me.BtnCerrar.BackgroundImage = CType(resources.GetObject("BtnCerrar.BackgroundImage"), System.Drawing.Image)
        Me.BtnCerrar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.BtnCerrar.Location = New System.Drawing.Point(546, 8)
        Me.BtnCerrar.Name = "BtnCerrar"
        Me.BtnCerrar.Size = New System.Drawing.Size(36, 36)
        Me.BtnCerrar.TabIndex = 58
        Me.BtnCerrar.TabStop = False
        '
        'PanelTopBar
        '
        Me.PanelTopBar.Controls.Add(Me.BtnCerrar)
        Me.PanelTopBar.Dock = System.Windows.Forms.DockStyle.Top
        Me.PanelTopBar.Location = New System.Drawing.Point(447, 0)
        Me.PanelTopBar.Name = "PanelTopBar"
        Me.PanelTopBar.Size = New System.Drawing.Size(593, 52)
        Me.PanelTopBar.TabIndex = 56
        '
        'ControlTransporte
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(10.0!, 24.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(1040, 784)
        Me.ControlBox = False
        Me.Controls.Add(Me.PanelResult)
        Me.Controls.Add(Me.PanelTopBar)
        Me.Controls.Add(Me.BunifuGradientPanel1)
        Me.Controls.Add(Me.StatusLine)
        Me.Controls.Add(PromptLabel)
        Me.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ForeColor = System.Drawing.Color.White
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "ControlTransporte"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "ControlMarcas"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.BunifuGradientPanel1.ResumeLayout(False)
        Me.BunifuGradientPanel1.PerformLayout()
        Me.GbDatos.ResumeLayout(False)
        Me.GbDatos.PerformLayout()
        CType(Me.Picture, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Imgprocess, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PanelResult.ResumeLayout(False)
        Me.PanelResult.PerformLayout()
        CType(Me.BtnCerrar, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PanelTopBar.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents LblTitulo As System.Windows.Forms.Label
    Private WithEvents StatusLine As System.Windows.Forms.Label
    Private WithEvents Prompt As System.Windows.Forms.TextBox
    Private WithEvents Picture As System.Windows.Forms.PictureBox
    Friend WithEvents GbDatos As System.Windows.Forms.GroupBox
    Friend WithEvents TxtUsuario As System.Windows.Forms.TextBox
    Friend WithEvents LblUsuarioCaption As System.Windows.Forms.Label
    Friend WithEvents TxtTipo As System.Windows.Forms.TextBox
    Friend WithEvents LblTipoCaption As System.Windows.Forms.Label
    Friend WithEvents LblCedula As System.Windows.Forms.TextBox
    Friend WithEvents LblCedulaCaption As System.Windows.Forms.Label
    Friend WithEvents LblSeccionCaption As System.Windows.Forms.Label
    Friend WithEvents LblFecha As System.Windows.Forms.Label
    Private WithEvents StatusText As System.Windows.Forms.TextBox
    Friend WithEvents Imgprocess As PictureBox
    Friend WithEvents lblProcesando As System.Windows.Forms.Label
    Friend WithEvents TxtCedula As TextBox
    Friend WithEvents TxtSeccion As TextBox
    Friend WithEvents TxtRuta As TextBox
    Friend WithEvents LblRutaCaption As Label
    Friend WithEvents LblRuta As Label
    Friend WithEvents BunifuGradientPanel1 As System.Windows.Forms.Panel
    Friend WithEvents PanelResult As Panel
    Friend WithEvents PanelTopBar As Panel
    Friend WithEvents BtnCerrar As PictureBox
    Friend WithEvents TxtPermisoSalida As TextBox
    Friend WithEvents LblPermisoSalidaCaption As Label
End Class
