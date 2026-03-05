<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ControlComedor
    Inherits System.Windows.Forms.Form

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.PanelResult = New System.Windows.Forms.Panel()
        Me.PanelMainStatus = New System.Windows.Forms.Panel()
        Me.lblProcesando = New System.Windows.Forms.Label()
        Me.Imgprocess = New System.Windows.Forms.PictureBox()
        Me.LblTitulo = New System.Windows.Forms.Label()
        Me.PicBrandHeader = New System.Windows.Forms.PictureBox()
        Me.BunifuGradientPanel1 = New System.Windows.Forms.Panel()
        Me.LblScanHint = New System.Windows.Forms.Label()
        Me.TxtCedula = New System.Windows.Forms.TextBox()
        Me.GbDatos = New System.Windows.Forms.GroupBox()
        Me.TxtTiquetes = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.TxtTipo = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.TxtUsuario = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.LblCedula = New System.Windows.Forms.TextBox()
        Me.LblRegistroError = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Picture = New System.Windows.Forms.PictureBox()
        Me.LblFecha = New System.Windows.Forms.Label()
        Me.BtnSalir = New System.Windows.Forms.Button()
        Me.PanelResult.SuspendLayout()
        Me.PanelMainStatus.SuspendLayout()
        CType(Me.Imgprocess, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PicBrandHeader, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.BunifuGradientPanel1.SuspendLayout()
        Me.GbDatos.SuspendLayout()
        CType(Me.Picture, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PanelResult
        '
        Me.PanelResult.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(246, Byte), Integer), CType(CType(251, Byte), Integer))
        Me.PanelResult.Controls.Add(Me.PanelMainStatus)
        Me.PanelResult.Controls.Add(Me.LblTitulo)
        Me.PanelResult.Controls.Add(Me.PicBrandHeader)
        Me.PanelResult.Controls.Add(Me.BunifuGradientPanel1)
        Me.PanelResult.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PanelResult.Location = New System.Drawing.Point(0, 0)
        Me.PanelResult.Name = "PanelResult"
        Me.PanelResult.Size = New System.Drawing.Size(1600, 900)
        Me.PanelResult.TabIndex = 0
        '
        'PanelMainStatus
        '
        Me.PanelMainStatus.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.PanelMainStatus.BackColor = System.Drawing.Color.White
        Me.PanelMainStatus.Controls.Add(Me.lblProcesando)
        Me.PanelMainStatus.Controls.Add(Me.Imgprocess)
        Me.PanelMainStatus.Location = New System.Drawing.Point(520, 116)
        Me.PanelMainStatus.Name = "PanelMainStatus"
        Me.PanelMainStatus.Size = New System.Drawing.Size(1056, 754)
        Me.PanelMainStatus.TabIndex = 2
        '
        'lblProcesando
        '
        Me.lblProcesando.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblProcesando.Font = New System.Drawing.Font("Segoe UI Semibold", 24.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblProcesando.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(32, Byte), Integer), CType(CType(51, Byte), Integer))
        Me.lblProcesando.Location = New System.Drawing.Point(52, 68)
        Me.lblProcesando.Name = "lblProcesando"
        Me.lblProcesando.Size = New System.Drawing.Size(947, 170)
        Me.lblProcesando.TabIndex = 1
        Me.lblProcesando.Text = "Esperando lectura de carnet"
        Me.lblProcesando.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Imgprocess
        '
        Me.Imgprocess.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Imgprocess.BackColor = System.Drawing.Color.Transparent
        Me.Imgprocess.Image = Global.SCSC.My.Resources.Resources.Info
        Me.Imgprocess.Location = New System.Drawing.Point(218, 249)
        Me.Imgprocess.Name = "Imgprocess"
        Me.Imgprocess.Size = New System.Drawing.Size(220, 220)
        Me.Imgprocess.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.Imgprocess.TabIndex = 0
        Me.Imgprocess.TabStop = False
        '
        'LblTitulo
        '
        Me.LblTitulo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblTitulo.Font = New System.Drawing.Font("Segoe UI Semibold", 28.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTitulo.ForeColor = System.Drawing.Color.FromArgb(CType(CType(17, Byte), Integer), CType(CType(33, Byte), Integer), CType(CType(59, Byte), Integer))
        Me.LblTitulo.Location = New System.Drawing.Point(520, 20)
        Me.LblTitulo.Name = "LblTitulo"
        Me.LblTitulo.Size = New System.Drawing.Size(1056, 85)
        Me.LblTitulo.TabIndex = 1
        Me.LblTitulo.Text = "Control de Marcas - Comedor"
        Me.LblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'PicBrandHeader
        '
        Me.PicBrandHeader.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.PicBrandHeader.BackColor = System.Drawing.Color.Transparent
        Me.PicBrandHeader.Image = Nothing
        Me.PicBrandHeader.Location = New System.Drawing.Point(1285, 23)
        Me.PicBrandHeader.Name = "PicBrandHeader"
        Me.PicBrandHeader.Size = New System.Drawing.Size(290, 72)
        Me.PicBrandHeader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PicBrandHeader.TabIndex = 3
        Me.PicBrandHeader.TabStop = False
        '
        'BunifuGradientPanel1
        '
        Me.BunifuGradientPanel1.BackColor = System.Drawing.Color.FromArgb(CType(CType(16, Byte), Integer), CType(CType(26, Byte), Integer), CType(CType(46, Byte), Integer))
        Me.BunifuGradientPanel1.Controls.Add(Me.LblScanHint)
        Me.BunifuGradientPanel1.Controls.Add(Me.TxtCedula)
        Me.BunifuGradientPanel1.Controls.Add(Me.GbDatos)
        Me.BunifuGradientPanel1.Controls.Add(Me.Picture)
        Me.BunifuGradientPanel1.Controls.Add(Me.LblFecha)
        Me.BunifuGradientPanel1.Controls.Add(Me.BtnSalir)
        Me.BunifuGradientPanel1.Dock = System.Windows.Forms.DockStyle.Left
        Me.BunifuGradientPanel1.Location = New System.Drawing.Point(0, 0)
        Me.BunifuGradientPanel1.Name = "BunifuGradientPanel1"
        Me.BunifuGradientPanel1.Size = New System.Drawing.Size(420, 900)
        Me.BunifuGradientPanel1.TabIndex = 0
        '
        'LblScanHint
        '
        Me.LblScanHint.AutoSize = True
        Me.LblScanHint.Font = New System.Drawing.Font("Segoe UI", 14.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblScanHint.ForeColor = System.Drawing.Color.White
        Me.LblScanHint.Location = New System.Drawing.Point(30, 271)
        Me.LblScanHint.Name = "LblScanHint"
        Me.LblScanHint.Size = New System.Drawing.Size(204, 32)
        Me.LblScanHint.TabIndex = 5
        Me.LblScanHint.Text = "Escanear carnet"
        '
        'TxtCedula
        '
        Me.TxtCedula.BackColor = System.Drawing.Color.White
        Me.TxtCedula.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.TxtCedula.Font = New System.Drawing.Font("Segoe UI Semibold", 22.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtCedula.Location = New System.Drawing.Point(32, 307)
        Me.TxtCedula.MaxLength = 50
        Me.TxtCedula.Name = "TxtCedula"
        Me.TxtCedula.Size = New System.Drawing.Size(436, 56)
        Me.TxtCedula.TabIndex = 0
        '
        'GbDatos
        '
        Me.GbDatos.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GbDatos.BackColor = System.Drawing.Color.FromArgb(CType(CType(28, Byte), Integer), CType(CType(42, Byte), Integer), CType(CType(68, Byte), Integer))
        Me.GbDatos.Controls.Add(Me.TxtTiquetes)
        Me.GbDatos.Controls.Add(Me.Label3)
        Me.GbDatos.Controls.Add(Me.TxtTipo)
        Me.GbDatos.Controls.Add(Me.Label5)
        Me.GbDatos.Controls.Add(Me.TxtUsuario)
        Me.GbDatos.Controls.Add(Me.Label2)
        Me.GbDatos.Controls.Add(Me.LblCedula)
        Me.GbDatos.Controls.Add(Me.LblRegistroError)
        Me.GbDatos.Controls.Add(Me.Label4)
        Me.GbDatos.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GbDatos.ForeColor = System.Drawing.Color.FromArgb(CType(CType(188, Byte), Integer), CType(CType(202, Byte), Integer), CType(CType(225, Byte), Integer))
        Me.GbDatos.Location = New System.Drawing.Point(22, 381)
        Me.GbDatos.Name = "GbDatos"
        Me.GbDatos.Size = New System.Drawing.Size(454, 445)
        Me.GbDatos.TabIndex = 1
        Me.GbDatos.TabStop = False
        Me.GbDatos.Text = "Datos del registro"
        '
        'TxtTiquetes
        '
        Me.TxtTiquetes.BackColor = System.Drawing.Color.White
        Me.TxtTiquetes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.TxtTiquetes.Font = New System.Drawing.Font("Segoe UI", 28.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtTiquetes.ForeColor = System.Drawing.Color.FromArgb(CType(CType(141, Byte), Integer), CType(CType(36, Byte), Integer), CType(CType(36, Byte), Integer))
        Me.TxtTiquetes.Location = New System.Drawing.Point(17, 365)
        Me.TxtTiquetes.Name = "TxtTiquetes"
        Me.TxtTiquetes.ReadOnly = True
        Me.TxtTiquetes.Size = New System.Drawing.Size(417, 70)
        Me.TxtTiquetes.TabIndex = 7
        Me.TxtTiquetes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Segoe UI", 14.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.ForeColor = System.Drawing.Color.White
        Me.Label3.Location = New System.Drawing.Point(13, 330)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(105, 32)
        Me.Label3.TabIndex = 6
        Me.Label3.Text = "Tiquetes"
        '
        'TxtTipo
        '
        Me.TxtTipo.BackColor = System.Drawing.Color.White
        Me.TxtTipo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.TxtTipo.Font = New System.Drawing.Font("Segoe UI", 16.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtTipo.Location = New System.Drawing.Point(17, 273)
        Me.TxtTipo.Name = "TxtTipo"
        Me.TxtTipo.ReadOnly = True
        Me.TxtTipo.Size = New System.Drawing.Size(417, 43)
        Me.TxtTipo.TabIndex = 5
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Segoe UI", 13.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.ForeColor = System.Drawing.Color.White
        Me.Label5.Location = New System.Drawing.Point(13, 239)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(56, 30)
        Me.Label5.TabIndex = 4
        Me.Label5.Text = "Tipo"
        '
        'TxtUsuario
        '
        Me.TxtUsuario.BackColor = System.Drawing.Color.White
        Me.TxtUsuario.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.TxtUsuario.Font = New System.Drawing.Font("Segoe UI", 15.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtUsuario.Location = New System.Drawing.Point(17, 176)
        Me.TxtUsuario.Name = "TxtUsuario"
        Me.TxtUsuario.ReadOnly = True
        Me.TxtUsuario.Size = New System.Drawing.Size(417, 41)
        Me.TxtUsuario.TabIndex = 3
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Segoe UI", 13.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.ForeColor = System.Drawing.Color.White
        Me.Label2.Location = New System.Drawing.Point(13, 142)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(88, 30)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Usuario"
        '
        'LblCedula
        '
        Me.LblCedula.BackColor = System.Drawing.Color.White
        Me.LblCedula.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.LblCedula.Font = New System.Drawing.Font("Segoe UI", 15.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblCedula.Location = New System.Drawing.Point(17, 81)
        Me.LblCedula.Name = "LblCedula"
        Me.LblCedula.ReadOnly = True
        Me.LblCedula.Size = New System.Drawing.Size(417, 41)
        Me.LblCedula.TabIndex = 1
        '
        'LblRegistroError
        '
        Me.LblRegistroError.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(47, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.LblRegistroError.Location = New System.Drawing.Point(17, 392)
        Me.LblRegistroError.Name = "LblRegistroError"
        Me.LblRegistroError.Size = New System.Drawing.Size(417, 46)
        Me.LblRegistroError.TabIndex = 8
        Me.LblRegistroError.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Segoe UI", 13.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.ForeColor = System.Drawing.Color.White
        Me.Label4.Location = New System.Drawing.Point(13, 47)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(80, 30)
        Me.Label4.TabIndex = 0
        Me.Label4.Text = "Cédula"
        '
        'Picture
        '
        Me.Picture.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(236, Byte), Integer), CType(CType(246, Byte), Integer))
        Me.Picture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Picture.Image = Global.SCSC.My.Resources.Resources.Login
        Me.Picture.Location = New System.Drawing.Point(127, 68)
        Me.Picture.Name = "Picture"
        Me.Picture.Size = New System.Drawing.Size(246, 185)
        Me.Picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.Picture.TabIndex = 4
        Me.Picture.TabStop = False
        '
        'LblFecha
        '
        Me.LblFecha.Dock = System.Windows.Forms.DockStyle.Top
        Me.LblFecha.Font = New System.Drawing.Font("Segoe UI", 16.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblFecha.ForeColor = System.Drawing.Color.White
        Me.LblFecha.Location = New System.Drawing.Point(0, 0)
        Me.LblFecha.Name = "LblFecha"
        Me.LblFecha.Size = New System.Drawing.Size(500, 56)
        Me.LblFecha.TabIndex = 3
        Me.LblFecha.Text = "Fecha"
        Me.LblFecha.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'BtnSalir
        '
        Me.BtnSalir.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtnSalir.BackColor = System.Drawing.Color.FromArgb(CType(CType(160, Byte), Integer), CType(CType(46, Byte), Integer), CType(CType(63, Byte), Integer))
        Me.BtnSalir.FlatAppearance.BorderSize = 0
        Me.BtnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BtnSalir.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BtnSalir.ForeColor = System.Drawing.Color.White
        Me.BtnSalir.Location = New System.Drawing.Point(0, 0)
        Me.BtnSalir.Name = "BtnSalir"
        Me.BtnSalir.Size = New System.Drawing.Size(0, 0)
        Me.BtnSalir.TabIndex = 2
        Me.BtnSalir.Text = ""
        Me.BtnSalir.UseVisualStyleBackColor = False
        Me.BtnSalir.Visible = False
        '
        'ControlComedor
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1600, 900)
        Me.ControlBox = False
        Me.Controls.Add(Me.PanelResult)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "ControlComedor"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "ControlComedor"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.PanelResult.ResumeLayout(False)
        Me.PanelMainStatus.ResumeLayout(False)
        CType(Me.Imgprocess, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PicBrandHeader, System.ComponentModel.ISupportInitialize).EndInit()
        Me.BunifuGradientPanel1.ResumeLayout(False)
        Me.BunifuGradientPanel1.PerformLayout()
        Me.GbDatos.ResumeLayout(False)
        Me.GbDatos.PerformLayout()
        CType(Me.Picture, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents PanelResult As Panel
    Friend WithEvents BunifuGradientPanel1 As Panel
    Friend WithEvents LblFecha As Label
    Friend WithEvents Picture As PictureBox
    Friend WithEvents GbDatos As GroupBox
    Friend WithEvents TxtTiquetes As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents TxtTipo As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents TxtUsuario As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents LblCedula As TextBox
    Friend WithEvents LblRegistroError As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents TxtCedula As TextBox
    Friend WithEvents BtnSalir As Button
    Friend WithEvents LblTitulo As Label
    Friend WithEvents PicBrandHeader As PictureBox
    Friend WithEvents PanelMainStatus As Panel
    Friend WithEvents lblProcesando As Label
    Friend WithEvents Imgprocess As PictureBox
    Friend WithEvents LblScanHint As Label
End Class
