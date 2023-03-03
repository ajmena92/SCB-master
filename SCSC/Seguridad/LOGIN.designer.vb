<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> Partial Class Login
#Region "Windows Form Designer generated code "
	<System.Diagnostics.DebuggerNonUserCode()> Public Sub New()
		MyBase.New()
		'This call is required by the Windows Form Designer.
		InitializeComponent()
	End Sub
	'Form overrides dispose to clean up the component list.
	<System.Diagnostics.DebuggerNonUserCode()> Protected Overloads Overrides Sub Dispose(ByVal Disposing As Boolean)
		If Disposing Then
			If Not components Is Nothing Then
				components.Dispose()
			End If
		End If
		MyBase.Dispose(Disposing)
	End Sub
	'Required by the Windows Form Designer
	Private components As System.ComponentModel.IContainer
    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Login))
        Me.BunifuElipse1 = New Bunifu.Framework.UI.BunifuElipse(Me.components)
        Me.LbFecha = New System.Windows.Forms.Label()
        Me.ClavePaso = New System.Windows.Forms.TextBox()
        Me.CodUsuario = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.BunifuSeparator1 = New Bunifu.Framework.UI.BunifuSeparator()
        Me.BunifuSeparator2 = New Bunifu.Framework.UI.BunifuSeparator()
        Me.BtnLogin = New Bunifu.Framework.UI.BunifuThinButton2()
        Me.BtnCerrar = New System.Windows.Forms.PictureBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.LblInstitucion = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.BtnComedor = New Bunifu.Framework.UI.BunifuTileButton()
        Me.BtnTransporte = New Bunifu.Framework.UI.BunifuTileButton()
        Me.Version = New System.Windows.Forms.Label()
        Me.ChkOpen = New System.Windows.Forms.CheckBox()
        CType(Me.BtnCerrar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'BunifuElipse1
        '
        Me.BunifuElipse1.ElipseRadius = 5
        Me.BunifuElipse1.TargetControl = Me
        '
        'LbFecha
        '
        Me.LbFecha.AutoSize = True
        Me.LbFecha.BackColor = System.Drawing.Color.Transparent
        Me.LbFecha.Font = New System.Drawing.Font("Arial Narrow", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LbFecha.ForeColor = System.Drawing.Color.DarkGray
        Me.LbFecha.Location = New System.Drawing.Point(598, 440)
        Me.LbFecha.Name = "LbFecha"
        Me.LbFecha.Size = New System.Drawing.Size(79, 31)
        Me.LbFecha.TabIndex = 37
        Me.LbFecha.Text = "Label1"
        '
        'ClavePaso
        '
        Me.ClavePaso.AcceptsReturn = True
        Me.ClavePaso.BackColor = System.Drawing.Color.White
        Me.ClavePaso.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.ClavePaso.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.ClavePaso.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ClavePaso.ForeColor = System.Drawing.SystemColors.WindowText
        Me.ClavePaso.ImeMode = System.Windows.Forms.ImeMode.Disable
        Me.ClavePaso.Location = New System.Drawing.Point(616, 296)
        Me.ClavePaso.Margin = New System.Windows.Forms.Padding(4)
        Me.ClavePaso.MaxLength = 0
        Me.ClavePaso.Name = "ClavePaso"
        Me.ClavePaso.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.ClavePaso.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.ClavePaso.Size = New System.Drawing.Size(260, 25)
        Me.ClavePaso.TabIndex = 13
        '
        'CodUsuario
        '
        Me.CodUsuario.AcceptsReturn = True
        Me.CodUsuario.BackColor = System.Drawing.Color.White
        Me.CodUsuario.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.CodUsuario.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.CodUsuario.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CodUsuario.ForeColor = System.Drawing.SystemColors.WindowText
        Me.CodUsuario.Location = New System.Drawing.Point(616, 252)
        Me.CodUsuario.Margin = New System.Windows.Forms.Padding(4)
        Me.CodUsuario.MaxLength = 100
        Me.CodUsuario.Name = "CodUsuario"
        Me.CodUsuario.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.CodUsuario.Size = New System.Drawing.Size(260, 25)
        Me.CodUsuario.TabIndex = 12
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label1.Font = New System.Drawing.Font("Calibri", 56.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.Label1.Location = New System.Drawing.Point(581, 73)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label1.Size = New System.Drawing.Size(154, 69)
        Me.Label1.TabIndex = 42
        Me.Label1.Text = "Login"
        '
        'Label3
        '
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label3.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.Label3.Location = New System.Drawing.Point(590, 146)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label3.Size = New System.Drawing.Size(300, 86)
        Me.Label3.TabIndex = 43
        Me.Label3.Text = "Sistema de Control Escolar ""SICE"". Acceso funciones administrativas."
        '
        'BunifuSeparator1
        '
        Me.BunifuSeparator1.BackColor = System.Drawing.Color.Transparent
        Me.BunifuSeparator1.LineColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(51, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BunifuSeparator1.LineThickness = 1
        Me.BunifuSeparator1.Location = New System.Drawing.Point(593, 263)
        Me.BunifuSeparator1.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.BunifuSeparator1.Name = "BunifuSeparator1"
        Me.BunifuSeparator1.Size = New System.Drawing.Size(305, 24)
        Me.BunifuSeparator1.TabIndex = 44
        Me.BunifuSeparator1.Transparency = 255
        Me.BunifuSeparator1.Vertical = False
        '
        'BunifuSeparator2
        '
        Me.BunifuSeparator2.BackColor = System.Drawing.Color.Transparent
        Me.BunifuSeparator2.LineColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(51, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BunifuSeparator2.LineThickness = 1
        Me.BunifuSeparator2.Location = New System.Drawing.Point(594, 307)
        Me.BunifuSeparator2.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.BunifuSeparator2.Name = "BunifuSeparator2"
        Me.BunifuSeparator2.Size = New System.Drawing.Size(304, 24)
        Me.BunifuSeparator2.TabIndex = 45
        Me.BunifuSeparator2.Transparency = 255
        Me.BunifuSeparator2.Vertical = False
        '
        'BtnLogin
        '
        Me.BtnLogin.ActiveBorderThickness = 1
        Me.BtnLogin.ActiveCornerRadius = 20
        Me.BtnLogin.ActiveFillColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(53, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BtnLogin.ActiveForecolor = System.Drawing.Color.White
        Me.BtnLogin.ActiveLineColor = System.Drawing.Color.SeaShell
        Me.BtnLogin.BackColor = System.Drawing.Color.White
        Me.BtnLogin.BackgroundImage = CType(resources.GetObject("BtnLogin.BackgroundImage"), System.Drawing.Image)
        Me.BtnLogin.ButtonText = "Ingresar"
        Me.BtnLogin.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BtnLogin.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BtnLogin.ForeColor = System.Drawing.Color.SeaGreen
        Me.BtnLogin.IdleBorderThickness = 1
        Me.BtnLogin.IdleCornerRadius = 20
        Me.BtnLogin.IdleFillColor = System.Drawing.Color.White
        Me.BtnLogin.IdleForecolor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(53, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BtnLogin.IdleLineColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(43, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.BtnLogin.Location = New System.Drawing.Point(717, 358)
        Me.BtnLogin.Margin = New System.Windows.Forms.Padding(5)
        Me.BtnLogin.Name = "BtnLogin"
        Me.BtnLogin.Size = New System.Drawing.Size(181, 41)
        Me.BtnLogin.TabIndex = 48
        Me.BtnLogin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'BtnCerrar
        '
        Me.BtnCerrar.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtnCerrar.BackColor = System.Drawing.Color.Transparent
        Me.BtnCerrar.BackgroundImage = CType(resources.GetObject("BtnCerrar.BackgroundImage"), System.Drawing.Image)
        Me.BtnCerrar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.BtnCerrar.Location = New System.Drawing.Point(891, 9)
        Me.BtnCerrar.Name = "BtnCerrar"
        Me.BtnCerrar.Size = New System.Drawing.Size(25, 25)
        Me.BtnCerrar.TabIndex = 47
        Me.BtnCerrar.TabStop = False
        '
        'Panel1
        '
        Me.Panel1.BackgroundImage = CType(resources.GetObject("Panel1.BackgroundImage"), System.Drawing.Image)
        Me.Panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.Panel1.Controls.Add(Me.LblInstitucion)
        Me.Panel1.Controls.Add(Me.Label2)
        Me.Panel1.Controls.Add(Me.BtnComedor)
        Me.Panel1.Controls.Add(Me.BtnTransporte)
        Me.Panel1.Controls.Add(Me.Version)
        Me.Panel1.Controls.Add(Me.ChkOpen)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Left
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(574, 480)
        Me.Panel1.TabIndex = 37
        '
        'LblInstitucion
        '
        Me.LblInstitucion.AutoSize = True
        Me.LblInstitucion.BackColor = System.Drawing.Color.Transparent
        Me.LblInstitucion.Cursor = System.Windows.Forms.Cursors.Default
        Me.LblInstitucion.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.LblInstitucion.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblInstitucion.ForeColor = System.Drawing.Color.White
        Me.LblInstitucion.Location = New System.Drawing.Point(0, 457)
        Me.LblInstitucion.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LblInstitucion.Name = "LblInstitucion"
        Me.LblInstitucion.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.LblInstitucion.Size = New System.Drawing.Size(154, 23)
        Me.LblInstitucion.TabIndex = 51
        Me.LblInstitucion.Text = "Version de Prueba"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.Font = New System.Drawing.Font("Calibri", 36.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.ForeColor = System.Drawing.Color.White
        Me.Label2.Location = New System.Drawing.Point(290, 9)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(284, 73)
        Me.Label2.TabIndex = 50
        Me.Label2.Text = "SICEscolar"
        '
        'BtnComedor
        '
        Me.BtnComedor.BackColor = System.Drawing.Color.Transparent
        Me.BtnComedor.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.BtnComedor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.BtnComedor.color = System.Drawing.Color.Transparent
        Me.BtnComedor.colorActive = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(53, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BtnComedor.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BtnComedor.Font = New System.Drawing.Font("Calibri", 16.2!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BtnComedor.ForeColor = System.Drawing.Color.White
        Me.BtnComedor.Image = CType(resources.GetObject("BtnComedor.Image"), System.Drawing.Image)
        Me.BtnComedor.ImagePosition = 21
        Me.BtnComedor.ImageZoom = 40
        Me.BtnComedor.LabelPosition = 42
        Me.BtnComedor.LabelText = "Comedor"
        Me.BtnComedor.Location = New System.Drawing.Point(29, 99)
        Me.BtnComedor.Margin = New System.Windows.Forms.Padding(6)
        Me.BtnComedor.Name = "BtnComedor"
        Me.BtnComedor.Size = New System.Drawing.Size(166, 163)
        Me.BtnComedor.TabIndex = 49
        '
        'BtnTransporte
        '
        Me.BtnTransporte.BackColor = System.Drawing.Color.Transparent
        Me.BtnTransporte.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.BtnTransporte.color = System.Drawing.Color.Transparent
        Me.BtnTransporte.colorActive = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(53, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BtnTransporte.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BtnTransporte.Font = New System.Drawing.Font("Calibri", 16.2!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BtnTransporte.ForeColor = System.Drawing.Color.White
        Me.BtnTransporte.Image = CType(resources.GetObject("BtnTransporte.Image"), System.Drawing.Image)
        Me.BtnTransporte.ImagePosition = 21
        Me.BtnTransporte.ImageZoom = 40
        Me.BtnTransporte.LabelPosition = 42
        Me.BtnTransporte.LabelText = "Transporte"
        Me.BtnTransporte.Location = New System.Drawing.Point(29, 274)
        Me.BtnTransporte.Margin = New System.Windows.Forms.Padding(6)
        Me.BtnTransporte.Name = "BtnTransporte"
        Me.BtnTransporte.Size = New System.Drawing.Size(166, 163)
        Me.BtnTransporte.TabIndex = 48
        '
        'Version
        '
        Me.Version.AutoSize = True
        Me.Version.BackColor = System.Drawing.Color.Transparent
        Me.Version.Cursor = System.Windows.Forms.Cursors.Default
        Me.Version.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Version.Font = New System.Drawing.Font("Calibri", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Version.ForeColor = System.Drawing.Color.White
        Me.Version.Location = New System.Drawing.Point(0, 0)
        Me.Version.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Version.Name = "Version"
        Me.Version.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Version.Size = New System.Drawing.Size(153, 21)
        Me.Version.TabIndex = 41
        Me.Version.Text = "Version del Sistema "
        Me.Version.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'ChkOpen
        '
        Me.ChkOpen.AutoSize = True
        Me.ChkOpen.BackColor = System.Drawing.Color.Transparent
        Me.ChkOpen.Location = New System.Drawing.Point(7, 4)
        Me.ChkOpen.Margin = New System.Windows.Forms.Padding(4)
        Me.ChkOpen.Name = "ChkOpen"
        Me.ChkOpen.Size = New System.Drawing.Size(121, 27)
        Me.ChkOpen.TabIndex = 40
        Me.ChkOpen.Text = "Force Open"
        Me.ChkOpen.UseVisualStyleBackColor = False
        '
        'Login
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(10.0!, 23.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(924, 480)
        Me.ControlBox = False
        Me.Controls.Add(Me.BtnLogin)
        Me.Controls.Add(Me.BtnCerrar)
        Me.Controls.Add(Me.LbFecha)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.ClavePaso)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.CodUsuario)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.BunifuSeparator1)
        Me.Controls.Add(Me.BunifuSeparator2)
        Me.Cursor = System.Windows.Forms.Cursors.Default
        Me.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Location = New System.Drawing.Point(143, 113)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Login"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "                                        S.C.S.C"
        CType(Me.BtnCerrar, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents BunifuElipse1 As Bunifu.Framework.UI.BunifuElipse
    Friend WithEvents Panel1 As Panel
    Friend WithEvents LbFecha As Label
    Public WithEvents Label3 As Label
    Public WithEvents ClavePaso As TextBox
    Public WithEvents Label1 As Label
    Public WithEvents CodUsuario As TextBox
    Friend WithEvents ChkOpen As CheckBox
    Public WithEvents Version As Label
    Friend WithEvents BunifuSeparator1 As Bunifu.Framework.UI.BunifuSeparator
    Friend WithEvents BunifuSeparator2 As Bunifu.Framework.UI.BunifuSeparator
    Friend WithEvents BtnCerrar As PictureBox
    Friend WithEvents BtnComedor As Bunifu.Framework.UI.BunifuTileButton
    Friend WithEvents BtnTransporte As Bunifu.Framework.UI.BunifuTileButton
    Friend WithEvents BtnLogin As Bunifu.Framework.UI.BunifuThinButton2
    Friend WithEvents Label2 As Label
    Public WithEvents LblInstitucion As Label
#End Region
End Class