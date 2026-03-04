<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> Partial Class Login
    Inherits System.Windows.Forms.Form
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Login))
        Me.LbFecha = New System.Windows.Forms.Label()
        Me.ClavePaso = New System.Windows.Forms.TextBox()
        Me.CodUsuario = New System.Windows.Forms.TextBox()
        Me.LblUsuarioCaption = New System.Windows.Forms.Label()
        Me.LblClaveCaption = New System.Windows.Forms.Label()
        Me.BtnTogglePassword = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.SeparatorUsuario = New System.Windows.Forms.Panel()
        Me.SeparatorClave = New System.Windows.Forms.Panel()
        Me.BtnLogin = New System.Windows.Forms.Button()
        Me.BtnCerrar = New System.Windows.Forms.PictureBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.LblInstitucion = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.BtnComedor = New System.Windows.Forms.Button()
        Me.BtnTransporte = New System.Windows.Forms.Button()
        Me.Version = New System.Windows.Forms.Label()
        Me.ChkOpen = New System.Windows.Forms.CheckBox()
        CType(Me.BtnCerrar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'LbFecha
        '
        Me.LbFecha.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LbFecha.AutoSize = True
        Me.LbFecha.BackColor = System.Drawing.Color.Transparent
        Me.LbFecha.Font = New System.Drawing.Font("Segoe UI Semibold", 36.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LbFecha.ForeColor = System.Drawing.Color.FromArgb(CType(CType(109, Byte), Integer), CType(CType(122, Byte), Integer), CType(CType(136, Byte), Integer))
        Me.LbFecha.Location = New System.Drawing.Point(560, 600)
        Me.LbFecha.Name = "LbFecha"
        Me.LbFecha.Size = New System.Drawing.Size(203, 81)
        Me.LbFecha.TabIndex = 37
        Me.LbFecha.Text = "Label1"
        '
        'ClavePaso
        '
        Me.ClavePaso.AcceptsReturn = True
        Me.ClavePaso.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ClavePaso.BackColor = System.Drawing.Color.White
        Me.ClavePaso.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.ClavePaso.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.ClavePaso.Font = New System.Drawing.Font("Segoe UI", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ClavePaso.ForeColor = System.Drawing.SystemColors.WindowText
        Me.ClavePaso.ImeMode = System.Windows.Forms.ImeMode.Disable
        Me.ClavePaso.Location = New System.Drawing.Point(610, 374)
        Me.ClavePaso.Margin = New System.Windows.Forms.Padding(4)
        Me.ClavePaso.MaxLength = 0
        Me.ClavePaso.Name = "ClavePaso"
        Me.ClavePaso.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.ClavePaso.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.ClavePaso.Size = New System.Drawing.Size(520, 32)
        Me.ClavePaso.TabIndex = 13
        '
        'CodUsuario
        '
        Me.CodUsuario.AcceptsReturn = True
        Me.CodUsuario.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CodUsuario.BackColor = System.Drawing.Color.White
        Me.CodUsuario.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.CodUsuario.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.CodUsuario.Font = New System.Drawing.Font("Segoe UI", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CodUsuario.ForeColor = System.Drawing.SystemColors.WindowText
        Me.CodUsuario.Location = New System.Drawing.Point(610, 314)
        Me.CodUsuario.Margin = New System.Windows.Forms.Padding(4)
        Me.CodUsuario.MaxLength = 100
        Me.CodUsuario.Name = "CodUsuario"
        Me.CodUsuario.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.CodUsuario.Size = New System.Drawing.Size(560, 32)
        Me.CodUsuario.TabIndex = 12
        '
        'LblUsuarioCaption
        '
        Me.LblUsuarioCaption.AutoSize = True
        Me.LblUsuarioCaption.BackColor = System.Drawing.Color.Transparent
        Me.LblUsuarioCaption.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblUsuarioCaption.ForeColor = System.Drawing.Color.FromArgb(CType(CType(76, Byte), Integer), CType(CType(89, Byte), Integer), CType(CType(104, Byte), Integer))
        Me.LblUsuarioCaption.Location = New System.Drawing.Point(610, 286)
        Me.LblUsuarioCaption.Name = "LblUsuarioCaption"
        Me.LblUsuarioCaption.Size = New System.Drawing.Size(70, 23)
        Me.LblUsuarioCaption.TabIndex = 10
        Me.LblUsuarioCaption.Text = "Usuario"
        '
        'LblClaveCaption
        '
        Me.LblClaveCaption.AutoSize = True
        Me.LblClaveCaption.BackColor = System.Drawing.Color.Transparent
        Me.LblClaveCaption.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblClaveCaption.ForeColor = System.Drawing.Color.FromArgb(CType(CType(76, Byte), Integer), CType(CType(89, Byte), Integer), CType(CType(104, Byte), Integer))
        Me.LblClaveCaption.Location = New System.Drawing.Point(610, 345)
        Me.LblClaveCaption.Name = "LblClaveCaption"
        Me.LblClaveCaption.Size = New System.Drawing.Size(99, 23)
        Me.LblClaveCaption.TabIndex = 11
        Me.LblClaveCaption.Text = "Contraseña"
        '
        'BtnTogglePassword
        '
        Me.BtnTogglePassword.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtnTogglePassword.BackColor = System.Drawing.Color.White
        Me.BtnTogglePassword.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BtnTogglePassword.FlatAppearance.BorderSize = 0
        Me.BtnTogglePassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BtnTogglePassword.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BtnTogglePassword.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(82, Byte), Integer), CType(CType(99, Byte), Integer))
        Me.BtnTogglePassword.Location = New System.Drawing.Point(1140, 374)
        Me.BtnTogglePassword.Name = "BtnTogglePassword"
        Me.BtnTogglePassword.Size = New System.Drawing.Size(76, 38)
        Me.BtnTogglePassword.TabIndex = 14
        Me.BtnTogglePassword.UseVisualStyleBackColor = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label1.Font = New System.Drawing.Font("Segoe UI", 36.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.Label1.Location = New System.Drawing.Point(560, 108)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label1.Size = New System.Drawing.Size(524, 81)
        Me.Label1.TabIndex = 42
        Me.Label1.Text = "Acceso al sistema"
        '
        'Label3
        '
        Me.Label3.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label3.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(59, Byte), Integer), CType(CType(73, Byte), Integer))
        Me.Label3.Location = New System.Drawing.Point(560, 206)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label3.Size = New System.Drawing.Size(620, 56)
        Me.Label3.TabIndex = 43
        Me.Label3.Text = "Gestione accesos y modulos de control escolar en una interfaz moderna y segura."
        '
        'SeparatorUsuario
        '
        Me.SeparatorUsuario.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.SeparatorUsuario.BackColor = System.Drawing.Color.FromArgb(CType(CType(205, Byte), Integer), CType(CType(216, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.SeparatorUsuario.Location = New System.Drawing.Point(610, 352)
        Me.SeparatorUsuario.Margin = New System.Windows.Forms.Padding(4)
        Me.SeparatorUsuario.Name = "SeparatorUsuario"
        Me.SeparatorUsuario.Size = New System.Drawing.Size(560, 1)
        Me.SeparatorUsuario.TabIndex = 44
        '
        'SeparatorClave
        '
        Me.SeparatorClave.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.SeparatorClave.BackColor = System.Drawing.Color.FromArgb(CType(CType(205, Byte), Integer), CType(CType(216, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.SeparatorClave.Location = New System.Drawing.Point(610, 412)
        Me.SeparatorClave.Margin = New System.Windows.Forms.Padding(4)
        Me.SeparatorClave.Name = "SeparatorClave"
        Me.SeparatorClave.Size = New System.Drawing.Size(606, 1)
        Me.SeparatorClave.TabIndex = 45
        '
        'BtnLogin
        '
        Me.BtnLogin.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtnLogin.BackColor = System.Drawing.Color.FromArgb(CType(CType(24, Byte), Integer), CType(CType(119, Byte), Integer), CType(CType(242, Byte), Integer))
        Me.BtnLogin.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BtnLogin.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(24, Byte), Integer), CType(CType(119, Byte), Integer), CType(CType(242, Byte), Integer))
        Me.BtnLogin.FlatAppearance.BorderSize = 0
        Me.BtnLogin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(14, Byte), Integer), CType(CType(93, Byte), Integer), CType(CType(196, Byte), Integer))
        Me.BtnLogin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(132, Byte), Integer), CType(CType(251, Byte), Integer))
        Me.BtnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BtnLogin.Font = New System.Drawing.Font("Segoe UI Semibold", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BtnLogin.ForeColor = System.Drawing.Color.White
        Me.BtnLogin.Location = New System.Drawing.Point(870, 426)
        Me.BtnLogin.Margin = New System.Windows.Forms.Padding(5)
        Me.BtnLogin.Name = "BtnLogin"
        Me.BtnLogin.Size = New System.Drawing.Size(260, 52)
        Me.BtnLogin.TabIndex = 48
        Me.BtnLogin.Text = "Iniciar sesión"
        Me.BtnLogin.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.BtnLogin.UseVisualStyleBackColor = False
        '
        'BtnCerrar
        '
        Me.BtnCerrar.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtnCerrar.BackColor = System.Drawing.Color.Transparent
        Me.BtnCerrar.BackgroundImage = CType(resources.GetObject("BtnCerrar.BackgroundImage"), System.Drawing.Image)
        Me.BtnCerrar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.BtnCerrar.Location = New System.Drawing.Point(1202, 18)
        Me.BtnCerrar.Name = "BtnCerrar"
        Me.BtnCerrar.Size = New System.Drawing.Size(33, 32)
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
        Me.Panel1.Size = New System.Drawing.Size(500, 760)
        Me.Panel1.TabIndex = 37
        '
        'LblInstitucion
        '
        Me.LblInstitucion.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LblInstitucion.AutoSize = True
        Me.LblInstitucion.BackColor = System.Drawing.Color.Transparent
        Me.LblInstitucion.Cursor = System.Windows.Forms.Cursors.Default
        Me.LblInstitucion.Font = New System.Drawing.Font("Segoe UI Black", 14.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblInstitucion.ForeColor = System.Drawing.Color.White
        Me.LblInstitucion.Location = New System.Drawing.Point(15, 692)
        Me.LblInstitucion.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LblInstitucion.Name = "LblInstitucion"
        Me.LblInstitucion.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.LblInstitucion.Size = New System.Drawing.Size(227, 32)
        Me.LblInstitucion.TabIndex = 51
        Me.LblInstitucion.Text = "Version de Prueba"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.Font = New System.Drawing.Font("Segoe UI", 34.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.ForeColor = System.Drawing.Color.White
        Me.Label2.Location = New System.Drawing.Point(30, 28)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(306, 76)
        Me.Label2.TabIndex = 50
        Me.Label2.Text = "SICEscolar"
        '
        'BtnComedor
        '
        Me.BtnComedor.BackColor = System.Drawing.Color.FromArgb(CType(CType(246, Byte), Integer), CType(CType(250, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.BtnComedor.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.BtnComedor.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BtnComedor.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(153, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(214, Byte), Integer))
        Me.BtnComedor.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(208, Byte), Integer), CType(CType(226, Byte), Integer), CType(CType(246, Byte), Integer))
        Me.BtnComedor.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(221, Byte), Integer), CType(CType(235, Byte), Integer), CType(CType(250, Byte), Integer))
        Me.BtnComedor.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BtnComedor.Font = New System.Drawing.Font("Segoe UI Semibold", 13.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BtnComedor.ForeColor = System.Drawing.Color.FromArgb(CType(CType(16, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.BtnComedor.Image = CType(resources.GetObject("BtnComedor.Image"), System.Drawing.Image)
        Me.BtnComedor.ImageAlign = System.Drawing.ContentAlignment.TopCenter
        Me.BtnComedor.Location = New System.Drawing.Point(86, 158)
        Me.BtnComedor.Margin = New System.Windows.Forms.Padding(6)
        Me.BtnComedor.Name = "BtnComedor"
        Me.BtnComedor.Size = New System.Drawing.Size(328, 196)
        Me.BtnComedor.TabIndex = 49
        Me.BtnComedor.Text = "Control de comedor"
        Me.BtnComedor.TextAlign = System.Drawing.ContentAlignment.BottomCenter
        Me.BtnComedor.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText
        Me.BtnComedor.UseVisualStyleBackColor = False
        '
        'BtnTransporte
        '
        Me.BtnTransporte.BackColor = System.Drawing.Color.FromArgb(CType(CType(246, Byte), Integer), CType(CType(250, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.BtnTransporte.Cursor = System.Windows.Forms.Cursors.Hand
        Me.BtnTransporte.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(153, Byte), Integer), CType(CType(182, Byte), Integer), CType(CType(214, Byte), Integer))
        Me.BtnTransporte.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(208, Byte), Integer), CType(CType(226, Byte), Integer), CType(CType(246, Byte), Integer))
        Me.BtnTransporte.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(221, Byte), Integer), CType(CType(235, Byte), Integer), CType(CType(250, Byte), Integer))
        Me.BtnTransporte.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BtnTransporte.Font = New System.Drawing.Font("Segoe UI Semibold", 13.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BtnTransporte.ForeColor = System.Drawing.Color.FromArgb(CType(CType(16, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.BtnTransporte.Image = CType(resources.GetObject("BtnTransporte.Image"), System.Drawing.Image)
        Me.BtnTransporte.ImageAlign = System.Drawing.ContentAlignment.TopCenter
        Me.BtnTransporte.Location = New System.Drawing.Point(86, 378)
        Me.BtnTransporte.Margin = New System.Windows.Forms.Padding(6)
        Me.BtnTransporte.Name = "BtnTransporte"
        Me.BtnTransporte.Size = New System.Drawing.Size(328, 196)
        Me.BtnTransporte.TabIndex = 48
        Me.BtnTransporte.Text = "Control de transporte"
        Me.BtnTransporte.TextAlign = System.Drawing.ContentAlignment.BottomCenter
        Me.BtnTransporte.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText
        Me.BtnTransporte.UseVisualStyleBackColor = False
        '
        'Version
        '
        Me.Version.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Version.AutoSize = True
        Me.Version.BackColor = System.Drawing.Color.Transparent
        Me.Version.Cursor = System.Windows.Forms.Cursors.Default
        Me.Version.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Version.ForeColor = System.Drawing.Color.White
        Me.Version.Location = New System.Drawing.Point(18, 731)
        Me.Version.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Version.Name = "Version"
        Me.Version.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Version.Size = New System.Drawing.Size(149, 20)
        Me.Version.TabIndex = 41
        Me.Version.Text = "Version del Sistema "
        Me.Version.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'ChkOpen
        '
        Me.ChkOpen.AutoSize = True
        Me.ChkOpen.BackColor = System.Drawing.Color.Transparent
        Me.ChkOpen.Location = New System.Drawing.Point(30, 74)
        Me.ChkOpen.Margin = New System.Windows.Forms.Padding(4)
        Me.ChkOpen.Name = "ChkOpen"
        Me.ChkOpen.Size = New System.Drawing.Size(137, 27)
        Me.ChkOpen.TabIndex = 40
        Me.ChkOpen.Text = "Modo técnico"
        Me.ChkOpen.UseVisualStyleBackColor = False
        '
        'Login
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 23.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(245, Byte), Integer), CType(CType(248, Byte), Integer), CType(CType(252, Byte), Integer))
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(1248, 760)
        Me.ControlBox = False
        Me.Controls.Add(Me.BtnLogin)
        Me.Controls.Add(Me.BtnCerrar)
        Me.Controls.Add(Me.LbFecha)
        Me.Controls.Add(Me.BtnTogglePassword)
        Me.Controls.Add(Me.LblClaveCaption)
        Me.Controls.Add(Me.LblUsuarioCaption)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.ClavePaso)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.CodUsuario)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.SeparatorUsuario)
        Me.Controls.Add(Me.SeparatorClave)
        Me.Cursor = System.Windows.Forms.Cursors.Default
        Me.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.Location = New System.Drawing.Point(143, 113)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Login"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "SCSC"
        CType(Me.BtnCerrar, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Panel1 As Panel
    Friend WithEvents LbFecha As Label
    Public WithEvents Label3 As Label
    Public WithEvents ClavePaso As TextBox
    Public WithEvents Label1 As Label
    Public WithEvents CodUsuario As TextBox
    Friend WithEvents LblUsuarioCaption As Label
    Friend WithEvents LblClaveCaption As Label
    Friend WithEvents BtnTogglePassword As Button
    Friend WithEvents ChkOpen As CheckBox
    Public WithEvents Version As Label
    Friend WithEvents SeparatorUsuario As Panel
    Friend WithEvents SeparatorClave As Panel
    Friend WithEvents BtnCerrar As PictureBox
    Friend WithEvents BtnComedor As Button
    Friend WithEvents BtnTransporte As Button
    Friend WithEvents BtnLogin As Button
    Friend WithEvents Label2 As Label
    Public WithEvents LblInstitucion As Label
#End Region
End Class
