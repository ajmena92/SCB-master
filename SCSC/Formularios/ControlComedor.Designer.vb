<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ControlComedor
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
        Dim PromptLabel As System.Windows.Forms.Label
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ControlComedor))
        Me.BtnSalir = New System.Windows.Forms.Button()
        Me.StatusLine = New System.Windows.Forms.Label()
        Me.Prompt = New System.Windows.Forms.TextBox()
        Me.Picture = New System.Windows.Forms.PictureBox()
        Me.GbDatos = New System.Windows.Forms.GroupBox()
        Me.TxtTipo = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.LblCedula = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.TxtTiquetes = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.TxtUsuario = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.LblFecha = New System.Windows.Forms.Label()
        Me.StatusText = New System.Windows.Forms.TextBox()
        Me.Imgprocess = New System.Windows.Forms.PictureBox()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.PanelResult = New System.Windows.Forms.Panel()
        Me.LblTitulo = New System.Windows.Forms.Label()
        Me.BunifuGradientPanel1 = New Bunifu.Framework.UI.BunifuGradientPanel()
        Me.TxtCedula = New System.Windows.Forms.TextBox()
        Me.lblProcesando = New System.Windows.Forms.Label()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.LblRuta = New System.Windows.Forms.Label()
        PromptLabel = New System.Windows.Forms.Label()
        CType(Me.Picture, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GbDatos.SuspendLayout()
        CType(Me.Imgprocess, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PanelResult.SuspendLayout()
        Me.BunifuGradientPanel1.SuspendLayout()
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
        'BtnSalir
        '
        Me.BtnSalir.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtnSalir.BackColor = System.Drawing.Color.Silver
        Me.BtnSalir.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.BtnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.BtnSalir.Font = New System.Drawing.Font("Calibri", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BtnSalir.ForeColor = System.Drawing.Color.Black
        Me.BtnSalir.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.BtnSalir.Location = New System.Drawing.Point(1280, 713)
        Me.BtnSalir.Margin = New System.Windows.Forms.Padding(4)
        Me.BtnSalir.Name = "BtnSalir"
        Me.BtnSalir.Size = New System.Drawing.Size(70, 38)
        Me.BtnSalir.TabIndex = 0
        Me.BtnSalir.Text = "Salir "
        Me.BtnSalir.UseVisualStyleBackColor = False
        '
        'StatusLine
        '
        Me.StatusLine.AutoSize = True
        Me.StatusLine.Location = New System.Drawing.Point(1164, 362)
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
        Me.Prompt.Location = New System.Drawing.Point(1056, 378)
        Me.Prompt.Name = "Prompt"
        Me.Prompt.ReadOnly = True
        Me.Prompt.Size = New System.Drawing.Size(294, 32)
        Me.Prompt.TabIndex = 16
        Me.Prompt.Visible = False
        '
        'Picture
        '
        Me.Picture.BackColor = System.Drawing.SystemColors.Window
        Me.Picture.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Picture.Location = New System.Drawing.Point(81, 79)
        Me.Picture.Name = "Picture"
        Me.Picture.Size = New System.Drawing.Size(263, 219)
        Me.Picture.TabIndex = 14
        Me.Picture.TabStop = False
        '
        'GbDatos
        '
        Me.GbDatos.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GbDatos.BackColor = System.Drawing.Color.Transparent
        Me.GbDatos.Controls.Add(Me.TxtTipo)
        Me.GbDatos.Controls.Add(Me.Label5)
        Me.GbDatos.Controls.Add(Me.LblCedula)
        Me.GbDatos.Controls.Add(Me.Label4)
        Me.GbDatos.Controls.Add(Me.TxtTiquetes)
        Me.GbDatos.Controls.Add(Me.Label3)
        Me.GbDatos.Controls.Add(Me.TxtUsuario)
        Me.GbDatos.Controls.Add(Me.Label2)
        Me.GbDatos.Font = New System.Drawing.Font("Calibri", 20.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GbDatos.Location = New System.Drawing.Point(12, 318)
        Me.GbDatos.Name = "GbDatos"
        Me.GbDatos.Size = New System.Drawing.Size(415, 442)
        Me.GbDatos.TabIndex = 20
        Me.GbDatos.TabStop = False
        '
        'TxtTipo
        '
        Me.TxtTipo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TxtTipo.BackColor = System.Drawing.Color.White
        Me.TxtTipo.Location = New System.Drawing.Point(22, 264)
        Me.TxtTipo.Name = "TxtTipo"
        Me.TxtTipo.ReadOnly = True
        Me.TxtTipo.Size = New System.Drawing.Size(370, 49)
        Me.TxtTipo.TabIndex = 7
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Arial Narrow", 16.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.ForeColor = System.Drawing.Color.White
        Me.Label5.Location = New System.Drawing.Point(26, 228)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(65, 33)
        Me.Label5.TabIndex = 6
        Me.Label5.Text = "Tipo:"
        '
        'LblCedula
        '
        Me.LblCedula.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblCedula.BackColor = System.Drawing.Color.White
        Me.LblCedula.Location = New System.Drawing.Point(32, 74)
        Me.LblCedula.Name = "LblCedula"
        Me.LblCedula.ReadOnly = True
        Me.LblCedula.Size = New System.Drawing.Size(237, 49)
        Me.LblCedula.TabIndex = 5
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Arial Narrow", 16.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.ForeColor = System.Drawing.Color.White
        Me.Label4.Location = New System.Drawing.Point(25, 42)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(93, 33)
        Me.Label4.TabIndex = 4
        Me.Label4.Text = "Cedula:"
        '
        'TxtTiquetes
        '
        Me.TxtTiquetes.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TxtTiquetes.BackColor = System.Drawing.Color.White
        Me.TxtTiquetes.Font = New System.Drawing.Font("Calibri", 26.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtTiquetes.ForeColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.TxtTiquetes.Location = New System.Drawing.Point(22, 359)
        Me.TxtTiquetes.Name = "TxtTiquetes"
        Me.TxtTiquetes.ReadOnly = True
        Me.TxtTiquetes.Size = New System.Drawing.Size(374, 61)
        Me.TxtTiquetes.TabIndex = 3
        Me.TxtTiquetes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Arial Narrow", 16.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.ForeColor = System.Drawing.Color.White
        Me.Label3.Location = New System.Drawing.Point(26, 323)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(108, 33)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "Tiquetes:"
        '
        'TxtUsuario
        '
        Me.TxtUsuario.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TxtUsuario.BackColor = System.Drawing.Color.White
        Me.TxtUsuario.Location = New System.Drawing.Point(22, 169)
        Me.TxtUsuario.Name = "TxtUsuario"
        Me.TxtUsuario.ReadOnly = True
        Me.TxtUsuario.Size = New System.Drawing.Size(374, 49)
        Me.TxtUsuario.TabIndex = 1
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Arial Narrow", 16.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.ForeColor = System.Drawing.Color.White
        Me.Label2.Location = New System.Drawing.Point(26, 133)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(99, 33)
        Me.Label2.TabIndex = 0
        Me.Label2.Text = "Usuario:"
        '
        'LblFecha
        '
        Me.LblFecha.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblFecha.BackColor = System.Drawing.Color.Transparent
        Me.LblFecha.Font = New System.Drawing.Font("Calibri", 36.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblFecha.ForeColor = System.Drawing.Color.White
        Me.LblFecha.Location = New System.Drawing.Point(-13, 0)
        Me.LblFecha.Name = "LblFecha"
        Me.LblFecha.Size = New System.Drawing.Size(460, 59)
        Me.LblFecha.TabIndex = 21
        Me.LblFecha.Text = "FECHA SERVER"
        Me.LblFecha.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'StatusText
        '
        Me.StatusText.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.StatusText.BackColor = System.Drawing.SystemColors.Window
        Me.StatusText.Location = New System.Drawing.Point(996, 484)
        Me.StatusText.Multiline = True
        Me.StatusText.Name = "StatusText"
        Me.StatusText.ReadOnly = True
        Me.StatusText.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.StatusText.Size = New System.Drawing.Size(366, 206)
        Me.StatusText.TabIndex = 18
        Me.StatusText.Visible = False
        '
        'Imgprocess
        '
        Me.Imgprocess.Anchor = System.Windows.Forms.AnchorStyles.Top
        Me.Imgprocess.BackColor = System.Drawing.Color.Transparent
        Me.Imgprocess.Image = Global.SCSC.My.Resources.Resources.Info
        Me.Imgprocess.Location = New System.Drawing.Point(520, 573)
        Me.Imgprocess.Name = "Imgprocess"
        Me.Imgprocess.Size = New System.Drawing.Size(675, 165)
        Me.Imgprocess.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.Imgprocess.TabIndex = 24
        Me.Imgprocess.TabStop = False
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.Transparent
        Me.PictureBox1.Image = Global.SCSC.My.Resources.Resources.Ingreso
        Me.PictureBox1.Location = New System.Drawing.Point(1220, 510)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(130, 151)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.PictureBox1.TabIndex = 26
        Me.PictureBox1.TabStop = False
        '
        'PanelResult
        '
        Me.PanelResult.BackColor = System.Drawing.Color.Gainsboro
        Me.PanelResult.Controls.Add(Me.PictureBox1)
        Me.PanelResult.Controls.Add(Me.LblTitulo)
        Me.PanelResult.Controls.Add(Me.BunifuGradientPanel1)
        Me.PanelResult.Controls.Add(Me.Imgprocess)
        Me.PanelResult.Controls.Add(Me.lblProcesando)
        Me.PanelResult.Controls.Add(Me.TextBox1)
        Me.PanelResult.Controls.Add(Me.LblRuta)
        Me.PanelResult.Controls.Add(Me.StatusLine)
        Me.PanelResult.Controls.Add(Me.Prompt)
        Me.PanelResult.Controls.Add(Me.StatusText)
        Me.PanelResult.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PanelResult.Location = New System.Drawing.Point(0, 0)
        Me.PanelResult.Name = "PanelResult"
        Me.PanelResult.Size = New System.Drawing.Size(1362, 763)
        Me.PanelResult.TabIndex = 58
        '
        'LblTitulo
        '
        Me.LblTitulo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblTitulo.BackColor = System.Drawing.Color.Transparent
        Me.LblTitulo.Font = New System.Drawing.Font("Arial Narrow", 61.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTitulo.ForeColor = System.Drawing.Color.Black
        Me.LblTitulo.Location = New System.Drawing.Point(447, 0)
        Me.LblTitulo.Name = "LblTitulo"
        Me.LblTitulo.Size = New System.Drawing.Size(915, 298)
        Me.LblTitulo.TabIndex = 22
        Me.LblTitulo.Text = "Sistema de Control de Marcas  Comedor"
        Me.LblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'BunifuGradientPanel1
        '
        Me.BunifuGradientPanel1.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(51, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BunifuGradientPanel1.BackgroundImage = CType(resources.GetObject("BunifuGradientPanel1.BackgroundImage"), System.Drawing.Image)
        Me.BunifuGradientPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.BunifuGradientPanel1.Controls.Add(Me.Picture)
        Me.BunifuGradientPanel1.Controls.Add(Me.TxtCedula)
        Me.BunifuGradientPanel1.Controls.Add(Me.LblFecha)
        Me.BunifuGradientPanel1.Controls.Add(Me.GbDatos)
        Me.BunifuGradientPanel1.Dock = System.Windows.Forms.DockStyle.Left
        Me.BunifuGradientPanel1.GradientBottomLeft = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(51, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BunifuGradientPanel1.GradientBottomRight = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(51, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BunifuGradientPanel1.GradientTopLeft = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(51, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BunifuGradientPanel1.GradientTopRight = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(51, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.BunifuGradientPanel1.Location = New System.Drawing.Point(0, 0)
        Me.BunifuGradientPanel1.Name = "BunifuGradientPanel1"
        Me.BunifuGradientPanel1.Quality = 10
        Me.BunifuGradientPanel1.Size = New System.Drawing.Size(447, 763)
        Me.BunifuGradientPanel1.TabIndex = 56
        '
        'TxtCedula
        '
        Me.TxtCedula.Location = New System.Drawing.Point(120, 79)
        Me.TxtCedula.MaxLength = 50
        Me.TxtCedula.Name = "TxtCedula"
        Me.TxtCedula.Size = New System.Drawing.Size(195, 32)
        Me.TxtCedula.TabIndex = 30
        '
        'lblProcesando
        '
        Me.lblProcesando.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblProcesando.Font = New System.Drawing.Font("Arial Narrow", 48.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblProcesando.ForeColor = System.Drawing.Color.Black
        Me.lblProcesando.Location = New System.Drawing.Point(433, 284)
        Me.lblProcesando.Name = "lblProcesando"
        Me.lblProcesando.Size = New System.Drawing.Size(926, 286)
        Me.lblProcesando.TabIndex = 27
        Me.lblProcesando.Text = "Por favor, coloque su identificador digital en el dispositivo"
        Me.lblProcesando.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'TextBox1
        '
        Me.TextBox1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBox1.BackColor = System.Drawing.SystemColors.Window
        Me.TextBox1.Location = New System.Drawing.Point(321, 0)
        Me.TextBox1.Multiline = True
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.ReadOnly = True
        Me.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.TextBox1.Size = New System.Drawing.Size(1038, 234)
        Me.TextBox1.TabIndex = 18
        Me.TextBox1.Visible = False
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
        Me.LblRuta.Size = New System.Drawing.Size(1362, 138)
        Me.LblRuta.TabIndex = 55
        Me.LblRuta.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'ControlComedor
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(10.0!, 24.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(1362, 763)
        Me.ControlBox = False
        Me.Controls.Add(PromptLabel)
        Me.Controls.Add(Me.BtnSalir)
        Me.Controls.Add(Me.PanelResult)
        Me.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ForeColor = System.Drawing.Color.White
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "ControlComedor"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "ControlMarcas"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        CType(Me.Picture, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GbDatos.ResumeLayout(False)
        Me.GbDatos.PerformLayout()
        CType(Me.Imgprocess, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PanelResult.ResumeLayout(False)
        Me.PanelResult.PerformLayout()
        Me.BunifuGradientPanel1.ResumeLayout(False)
        Me.BunifuGradientPanel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents BtnSalir As System.Windows.Forms.Button
    Private WithEvents StatusLine As System.Windows.Forms.Label
    Private WithEvents Prompt As System.Windows.Forms.TextBox
    Private WithEvents Picture As System.Windows.Forms.PictureBox
    Friend WithEvents GbDatos As System.Windows.Forms.GroupBox
    Friend WithEvents TxtUsuario As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents TxtTipo As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents LblCedula As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents TxtTiquetes As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents LblFecha As System.Windows.Forms.Label
    Private WithEvents StatusText As System.Windows.Forms.TextBox
    Friend WithEvents Imgprocess As PictureBox
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents PanelResult As Panel
    Friend WithEvents lblProcesando As Label
    Private WithEvents TextBox1 As TextBox
    Friend WithEvents LblRuta As Label
    Friend WithEvents BunifuGradientPanel1 As Bunifu.Framework.UI.BunifuGradientPanel
    Friend WithEvents LblTitulo As Label
    Friend WithEvents TxtCedula As TextBox
End Class
