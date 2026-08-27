<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmEstudiantes
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmEstudiantes))
        Me.GroupDatosEstudiante = New System.Windows.Forms.GroupBox()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.CBPermisoSalida = New System.Windows.Forms.ComboBox()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.CBRutaPendiente = New System.Windows.Forms.CheckBox()
        Me.CBBeca = New System.Windows.Forms.ComboBox()
        Me.PanelAcciones = New System.Windows.Forms.Panel()
        Me.BtnRegresar = New System.Windows.Forms.Button()
        Me.BtnCancelar = New System.Windows.Forms.Button()
        Me.BtnGuardar = New System.Windows.Forms.Button()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.CBGenero = New System.Windows.Forms.ComboBox()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.TxtSeccion = New System.Windows.Forms.TextBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.TxtTelefono = New System.Windows.Forms.TextBox()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.CBRuta = New System.Windows.Forms.ComboBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.LblRuta = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.TxtTipoUsuario = New System.Windows.Forms.TextBox()
        Me.LblCantTiques = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TxtFecNac = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.TxtApe1 = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.TxtApe2 = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.TxtNombre = New System.Windows.Forms.TextBox()
        Me.LblNombre = New System.Windows.Forms.Label()
        Me.LblTituloModulo = New System.Windows.Forms.Label()
        Me.GroupBusquedaEstudiante = New System.Windows.Forms.GroupBox()
        Me.StatusLine = New System.Windows.Forms.Label()
        Me.Picture = New System.Windows.Forms.PictureBox()
        Me.TxtCedula = New System.Windows.Forms.TextBox()
        Me.Buscar = New System.Windows.Forms.Button()
        Me.LblCedulaBusqueda = New System.Windows.Forms.Label()
        Me.StatusText = New System.Windows.Forms.TextBox()
        Me.Prompt = New System.Windows.Forms.TextBox()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.GroupDatosEstudiante.SuspendLayout()
        Me.PanelAcciones.SuspendLayout()
        Me.GroupBusquedaEstudiante.SuspendLayout()
        CType(Me.Picture, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'GroupDatosEstudiante
        '
        Me.GroupDatosEstudiante.BackColor = System.Drawing.Color.Transparent
        Me.GroupDatosEstudiante.Controls.Add(Me.Label16)
        Me.GroupDatosEstudiante.Controls.Add(Me.CBPermisoSalida)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label15)
        Me.GroupDatosEstudiante.Controls.Add(Me.CBRutaPendiente)
        Me.GroupDatosEstudiante.Controls.Add(Me.CBBeca)
        Me.GroupDatosEstudiante.Controls.Add(Me.PanelAcciones)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label14)
        Me.GroupDatosEstudiante.Controls.Add(Me.CBGenero)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label13)
        Me.GroupDatosEstudiante.Controls.Add(Me.TxtSeccion)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label10)
        Me.GroupDatosEstudiante.Controls.Add(Me.TxtTelefono)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label12)
        Me.GroupDatosEstudiante.Controls.Add(Me.CBRuta)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label11)
        Me.GroupDatosEstudiante.Controls.Add(Me.LblRuta)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label9)
        Me.GroupDatosEstudiante.Controls.Add(Me.TxtTipoUsuario)
        Me.GroupDatosEstudiante.Controls.Add(Me.LblCantTiques)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label8)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label2)
        Me.GroupDatosEstudiante.Controls.Add(Me.TxtFecNac)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label7)
        Me.GroupDatosEstudiante.Controls.Add(Me.TxtApe1)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label6)
        Me.GroupDatosEstudiante.Controls.Add(Me.TxtApe2)
        Me.GroupDatosEstudiante.Controls.Add(Me.Label5)
        Me.GroupDatosEstudiante.Controls.Add(Me.TxtNombre)
        Me.GroupDatosEstudiante.Controls.Add(Me.LblNombre)
        Me.GroupDatosEstudiante.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupDatosEstudiante.ForeColor = System.Drawing.Color.Black
        Me.GroupDatosEstudiante.Location = New System.Drawing.Point(20, 250)
        Me.GroupDatosEstudiante.Name = "GroupDatosEstudiante"
        Me.GroupDatosEstudiante.Size = New System.Drawing.Size(1130, 450)
        Me.GroupDatosEstudiante.TabIndex = 21
        Me.GroupDatosEstudiante.TabStop = False
        Me.GroupDatosEstudiante.Text = "Datos del Estudiante"
        '
        'Label16
        '
        Me.Label16.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label16.ForeColor = System.Drawing.Color.FromArgb(CType(CType(70, Byte), Integer), CType(CType(84, Byte), Integer), CType(CType(109, Byte), Integer))
        Me.Label16.Location = New System.Drawing.Point(30, 372)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(760, 55)
        Me.Label16.TabIndex = 70
        Me.Label16.Text = "NOTA: Al activar ruta pendiente en el reporte de transporte el estudiante se marc" &
    "ará de color rojo."
        '
        'CBPermisoSalida
        '
        Me.CBPermisoSalida.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.CBPermisoSalida.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CBPermisoSalida.Font = New System.Drawing.Font("Calibri", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CBPermisoSalida.FormattingEnabled = True
        Me.CBPermisoSalida.Location = New System.Drawing.Point(491, 177)
        Me.CBPermisoSalida.Name = "CBPermisoSalida"
        Me.CBPermisoSalida.Size = New System.Drawing.Size(185, 36)
        Me.CBPermisoSalida.TabIndex = 69
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.BackColor = System.Drawing.Color.Transparent
        Me.Label15.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label15.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label15.Location = New System.Drawing.Point(348, 183)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(137, 24)
        Me.Label15.TabIndex = 68
        Me.Label15.Text = "Permiso Salida:"
        '
        'CBRutaPendiente
        '
        Me.CBRutaPendiente.AutoSize = True
        Me.CBRutaPendiente.Location = New System.Drawing.Point(491, 144)
        Me.CBRutaPendiente.Name = "CBRutaPendiente"
        Me.CBRutaPendiente.Size = New System.Drawing.Size(148, 27)
        Me.CBRutaPendiente.TabIndex = 67
        Me.CBRutaPendiente.Text = "Ruta Pendiente"
        Me.CBRutaPendiente.UseVisualStyleBackColor = True
        '
        'CBBeca
        '
        Me.CBBeca.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.CBBeca.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CBBeca.Font = New System.Drawing.Font("Calibri", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CBBeca.FormattingEnabled = True
        Me.CBBeca.Location = New System.Drawing.Point(491, 51)
        Me.CBBeca.Name = "CBBeca"
        Me.CBBeca.Size = New System.Drawing.Size(185, 36)
        Me.CBBeca.TabIndex = 65
        '
        'PanelAcciones
        '
        Me.PanelAcciones.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.PanelAcciones.BackColor = System.Drawing.Color.Transparent
        Me.PanelAcciones.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PanelAcciones.Controls.Add(Me.BtnRegresar)
        Me.PanelAcciones.Controls.Add(Me.BtnCancelar)
        Me.PanelAcciones.Controls.Add(Me.BtnGuardar)
        Me.PanelAcciones.Location = New System.Drawing.Point(800, 340)
        Me.PanelAcciones.Name = "PanelAcciones"
        Me.PanelAcciones.Size = New System.Drawing.Size(315, 88)
        Me.PanelAcciones.TabIndex = 19
        '
        'BtnRegresar
        '
        Me.BtnRegresar.BackColor = System.Drawing.Color.FromArgb(CType(CType(51, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(85, Byte), Integer))
        Me.BtnRegresar.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BtnRegresar.Font = New System.Drawing.Font("Segoe UI Semibold", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BtnRegresar.ForeColor = System.Drawing.Color.White
        Me.BtnRegresar.Location = New System.Drawing.Point(212, 16)
        Me.BtnRegresar.Name = "BtnRegresar"
        Me.BtnRegresar.Size = New System.Drawing.Size(96, 56)
        Me.BtnRegresar.TabIndex = 7
        Me.BtnRegresar.Text = "Cerrar"
        Me.BtnRegresar.UseVisualStyleBackColor = False
        '
        'BtnCancelar
        '
        Me.BtnCancelar.BackColor = System.Drawing.Color.FromArgb(CType(CType(245, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(11, Byte), Integer))
        Me.BtnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BtnCancelar.Font = New System.Drawing.Font("Segoe UI Semibold", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BtnCancelar.ForeColor = System.Drawing.Color.White
        Me.BtnCancelar.Location = New System.Drawing.Point(110, 16)
        Me.BtnCancelar.Name = "BtnCancelar"
        Me.BtnCancelar.Size = New System.Drawing.Size(96, 56)
        Me.BtnCancelar.TabIndex = 5
        Me.BtnCancelar.Text = "Limpiar"
        Me.BtnCancelar.UseVisualStyleBackColor = False
        '
        'BtnGuardar
        '
        Me.BtnGuardar.BackColor = System.Drawing.Color.FromArgb(CType(CType(16, Byte), Integer), CType(CType(124, Byte), Integer), CType(CType(16, Byte), Integer))
        Me.BtnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BtnGuardar.Font = New System.Drawing.Font("Segoe UI Semibold", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BtnGuardar.ForeColor = System.Drawing.Color.White
        Me.BtnGuardar.Location = New System.Drawing.Point(8, 16)
        Me.BtnGuardar.Name = "BtnGuardar"
        Me.BtnGuardar.Size = New System.Drawing.Size(96, 56)
        Me.BtnGuardar.TabIndex = 0
        Me.BtnGuardar.Text = "Guardar"
        Me.BtnGuardar.UseVisualStyleBackColor = False
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.BackColor = System.Drawing.Color.Transparent
        Me.Label14.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label14.Location = New System.Drawing.Point(412, 51)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(54, 24)
        Me.Label14.TabIndex = 64
        Me.Label14.Text = "Beca:"
        '
        'CBGenero
        '
        Me.CBGenero.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.CBGenero.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CBGenero.Font = New System.Drawing.Font("Calibri", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CBGenero.FormattingEnabled = True
        Me.CBGenero.Location = New System.Drawing.Point(491, 219)
        Me.CBGenero.Name = "CBGenero"
        Me.CBGenero.Size = New System.Drawing.Size(185, 36)
        Me.CBGenero.TabIndex = 63
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.BackColor = System.Drawing.Color.Transparent
        Me.Label13.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label13.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label13.Location = New System.Drawing.Point(432, 227)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(53, 24)
        Me.Label13.TabIndex = 62
        Me.Label13.Text = "Sexo:"
        '
        'TxtSeccion
        '
        Me.TxtSeccion.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtSeccion.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtSeccion.Location = New System.Drawing.Point(179, 261)
        Me.TxtSeccion.MaxLength = 100
        Me.TxtSeccion.Name = "TxtSeccion"
        Me.TxtSeccion.ReadOnly = True
        Me.TxtSeccion.Size = New System.Drawing.Size(158, 30)
        Me.TxtSeccion.TabIndex = 59
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.BackColor = System.Drawing.Color.Transparent
        Me.Label10.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label10.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label10.Location = New System.Drawing.Point(81, 263)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(82, 24)
        Me.Label10.TabIndex = 60
        Me.Label10.Text = "Sección :"
        '
        'TxtTelefono
        '
        Me.TxtTelefono.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtTelefono.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtTelefono.Location = New System.Drawing.Point(179, 225)
        Me.TxtTelefono.MaxLength = 100
        Me.TxtTelefono.Name = "TxtTelefono"
        Me.TxtTelefono.ReadOnly = True
        Me.TxtTelefono.Size = New System.Drawing.Size(158, 30)
        Me.TxtTelefono.TabIndex = 57
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.BackColor = System.Drawing.Color.Transparent
        Me.Label12.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label12.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label12.Location = New System.Drawing.Point(76, 227)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(87, 24)
        Me.Label12.TabIndex = 58
        Me.Label12.Text = "Telefono:"
        '
        'CBRuta
        '
        Me.CBRuta.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.CBRuta.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CBRuta.Font = New System.Drawing.Font("Calibri", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CBRuta.FormattingEnabled = True
        Me.CBRuta.Location = New System.Drawing.Point(491, 99)
        Me.CBRuta.Name = "CBRuta"
        Me.CBRuta.Size = New System.Drawing.Size(185, 36)
        Me.CBRuta.TabIndex = 56
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.BackColor = System.Drawing.Color.Transparent
        Me.Label11.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label11.Font = New System.Drawing.Font("Arial Narrow", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label11.Location = New System.Drawing.Point(800, 40)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(101, 24)
        Me.Label11.TabIndex = 55
        Me.Label11.Text = "Codigo Ruta"
        '
        'LblRuta
        '
        Me.LblRuta.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblRuta.Font = New System.Drawing.Font("Calibri", 21.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblRuta.ForeColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.LblRuta.Location = New System.Drawing.Point(803, 68)
        Me.LblRuta.Name = "LblRuta"
        Me.LblRuta.Padding = New System.Windows.Forms.Padding(20, 15, 15, 15)
        Me.LblRuta.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.LblRuta.Size = New System.Drawing.Size(289, 78)
        Me.LblRuta.TabIndex = 54
        Me.LblRuta.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.BackColor = System.Drawing.Color.Transparent
        Me.Label9.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label9.Location = New System.Drawing.Point(414, 98)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(54, 24)
        Me.Label9.TabIndex = 52
        Me.Label9.Text = "Ruta:"
        '
        'TxtTipoUsuario
        '
        Me.TxtTipoUsuario.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtTipoUsuario.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtTipoUsuario.Location = New System.Drawing.Point(177, 40)
        Me.TxtTipoUsuario.MaxLength = 100
        Me.TxtTipoUsuario.Name = "TxtTipoUsuario"
        Me.TxtTipoUsuario.ReadOnly = True
        Me.TxtTipoUsuario.Size = New System.Drawing.Size(120, 30)
        Me.TxtTipoUsuario.TabIndex = 51
        '
        'LblCantTiques
        '
        Me.LblCantTiques.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LblCantTiques.Font = New System.Drawing.Font("Calibri", 21.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblCantTiques.ForeColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.LblCantTiques.Location = New System.Drawing.Point(803, 200)
        Me.LblCantTiques.Name = "LblCantTiques"
        Me.LblCantTiques.Padding = New System.Windows.Forms.Padding(20, 15, 15, 15)
        Me.LblCantTiques.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.LblCantTiques.Size = New System.Drawing.Size(289, 86)
        Me.LblCantTiques.TabIndex = 50
        Me.LblCantTiques.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.BackColor = System.Drawing.Color.Transparent
        Me.Label8.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label8.Font = New System.Drawing.Font("Arial Narrow", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.Location = New System.Drawing.Point(800, 170)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(165, 24)
        Me.Label8.TabIndex = 49
        Me.Label8.Text = "Cantidad de Tiquetes"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(30, 40)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(148, 24)
        Me.Label2.TabIndex = 39
        Me.Label2.Text = "Tipo de Usuario:"
        '
        'TxtFecNac
        '
        Me.TxtFecNac.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtFecNac.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtFecNac.Location = New System.Drawing.Point(179, 189)
        Me.TxtFecNac.MaxLength = 100
        Me.TxtFecNac.Name = "TxtFecNac"
        Me.TxtFecNac.ReadOnly = True
        Me.TxtFecNac.Size = New System.Drawing.Size(158, 30)
        Me.TxtFecNac.TabIndex = 47
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.BackColor = System.Drawing.Color.Transparent
        Me.Label7.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label7.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.Location = New System.Drawing.Point(8, 189)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(164, 24)
        Me.Label7.TabIndex = 48
        Me.Label7.Text = "Fecha Nacimiento:"
        '
        'TxtApe1
        '
        Me.TxtApe1.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtApe1.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtApe1.Location = New System.Drawing.Point(177, 76)
        Me.TxtApe1.MaxLength = 100
        Me.TxtApe1.Name = "TxtApe1"
        Me.TxtApe1.ReadOnly = True
        Me.TxtApe1.Size = New System.Drawing.Size(158, 30)
        Me.TxtApe1.TabIndex = 45
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.BackColor = System.Drawing.Color.Transparent
        Me.Label6.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label6.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.Location = New System.Drawing.Point(29, 81)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(145, 24)
        Me.Label6.TabIndex = 46
        Me.Label6.Text = "Primer Apellido:"
        '
        'TxtApe2
        '
        Me.TxtApe2.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtApe2.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtApe2.Location = New System.Drawing.Point(177, 112)
        Me.TxtApe2.MaxLength = 100
        Me.TxtApe2.Name = "TxtApe2"
        Me.TxtApe2.ReadOnly = True
        Me.TxtApe2.Size = New System.Drawing.Size(158, 30)
        Me.TxtApe2.TabIndex = 43
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.BackColor = System.Drawing.Color.Transparent
        Me.Label5.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label5.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.Location = New System.Drawing.Point(12, 114)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(162, 24)
        Me.Label5.TabIndex = 44
        Me.Label5.Text = "Segundo Apellido:"
        '
        'TxtNombre
        '
        Me.TxtNombre.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtNombre.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtNombre.Location = New System.Drawing.Point(179, 148)
        Me.TxtNombre.MaxLength = 100
        Me.TxtNombre.Name = "TxtNombre"
        Me.TxtNombre.ReadOnly = True
        Me.TxtNombre.Size = New System.Drawing.Size(158, 30)
        Me.TxtNombre.TabIndex = 41
        '
        'LblNombre
        '
        Me.LblNombre.AutoSize = True
        Me.LblNombre.BackColor = System.Drawing.Color.Transparent
        Me.LblNombre.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.LblNombre.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblNombre.Location = New System.Drawing.Point(89, 150)
        Me.LblNombre.Name = "LblNombre"
        Me.LblNombre.Size = New System.Drawing.Size(83, 24)
        Me.LblNombre.TabIndex = 42
        Me.LblNombre.Text = "Nombre:"
        '
        'LblTituloModulo
        '
        Me.LblTituloModulo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LblTituloModulo.AutoSize = True
        Me.LblTituloModulo.BackColor = System.Drawing.Color.Transparent
        Me.LblTituloModulo.Font = New System.Drawing.Font("Segoe UI Semibold", 21.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblTituloModulo.ForeColor = System.Drawing.Color.FromArgb(CType(CType(17, Byte), Integer), CType(CType(33, Byte), Integer), CType(CType(59, Byte), Integer))
        Me.LblTituloModulo.Location = New System.Drawing.Point(394, 24)
        Me.LblTituloModulo.Name = "LblTituloModulo"
        Me.LblTituloModulo.Size = New System.Drawing.Size(414, 47)
        Me.LblTituloModulo.TabIndex = 20
        Me.LblTituloModulo.Text = "Mantenimiento de Estudiantes"
        Me.LblTituloModulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'GroupBusquedaEstudiante
        '
        Me.GroupBusquedaEstudiante.BackColor = System.Drawing.Color.Transparent
        Me.GroupBusquedaEstudiante.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.GroupBusquedaEstudiante.Controls.Add(Me.StatusLine)
        Me.GroupBusquedaEstudiante.Controls.Add(Me.Picture)
        Me.GroupBusquedaEstudiante.Controls.Add(Me.TxtCedula)
        Me.GroupBusquedaEstudiante.Controls.Add(Me.Buscar)
        Me.GroupBusquedaEstudiante.Controls.Add(Me.LblCedulaBusqueda)
        Me.GroupBusquedaEstudiante.Location = New System.Drawing.Point(20, 82)
        Me.GroupBusquedaEstudiante.Name = "GroupBusquedaEstudiante"
        Me.GroupBusquedaEstudiante.Size = New System.Drawing.Size(1130, 160)
        Me.GroupBusquedaEstudiante.TabIndex = 0
        Me.GroupBusquedaEstudiante.TabStop = False
        Me.GroupBusquedaEstudiante.Text = "Estudiante"
        '
        'StatusLine
        '
        Me.StatusLine.AutoSize = False
        Me.StatusLine.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.StatusLine.ForeColor = System.Drawing.Color.FromArgb(CType(CType(70, Byte), Integer), CType(CType(84, Byte), Integer), CType(CType(109, Byte), Integer))
        Me.StatusLine.Location = New System.Drawing.Point(20, 112)
        Me.StatusLine.Name = "StatusLine"
        Me.StatusLine.Size = New System.Drawing.Size(760, 29)
        Me.StatusLine.TabIndex = 25
        Me.StatusLine.Text = "Estado: listo para búsqueda."
        '
        'Picture
        '
        Me.Picture.BackColor = System.Drawing.SystemColors.Window
        Me.Picture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.Picture.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Picture.Location = New System.Drawing.Point(946, 26)
        Me.Picture.Name = "Picture"
        Me.Picture.Size = New System.Drawing.Size(160, 118)
        Me.Picture.TabIndex = 43
        Me.Picture.TabStop = False
        '
        'TxtCedula
        '
        Me.TxtCedula.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.TxtCedula.Font = New System.Drawing.Font("Calibri", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtCedula.Location = New System.Drawing.Point(114, 46)
        Me.TxtCedula.MaxLength = 14
        Me.TxtCedula.Name = "TxtCedula"
        Me.TxtCedula.Size = New System.Drawing.Size(298, 36)
        Me.TxtCedula.TabIndex = 0
        '
        'Buscar
        '
        Me.Buscar.AccessibleRole = System.Windows.Forms.AccessibleRole.ButtonMenu
        Me.Buscar.BackColor = System.Drawing.Color.Transparent
        Me.Buscar.BackgroundImage = Nothing
        Me.Buscar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.Buscar.FlatAppearance.BorderColor = System.Drawing.Color.WhiteSmoke
        Me.Buscar.FlatAppearance.BorderSize = 1
        Me.Buscar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.Buscar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.Buscar.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Buscar.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Buscar.ForeColor = System.Drawing.Color.FromArgb(CType(CType(17, Byte), Integer), CType(CType(33, Byte), Integer), CType(CType(59, Byte), Integer))
        Me.Buscar.Location = New System.Drawing.Point(420, 46)
        Me.Buscar.Name = "Buscar"
        Me.Buscar.Size = New System.Drawing.Size(94, 36)
        Me.Buscar.TabIndex = 41
        Me.Buscar.Text = "Buscar"
        Me.Buscar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.Buscar.UseVisualStyleBackColor = False
        '
        'LblCedulaBusqueda
        '
        Me.LblCedulaBusqueda.AutoSize = True
        Me.LblCedulaBusqueda.BackColor = System.Drawing.Color.Transparent
        Me.LblCedulaBusqueda.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.LblCedulaBusqueda.Font = New System.Drawing.Font("Arial Narrow", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LblCedulaBusqueda.Location = New System.Drawing.Point(45, 57)
        Me.LblCedulaBusqueda.Name = "LblCedulaBusqueda"
        Me.LblCedulaBusqueda.Size = New System.Drawing.Size(66, 24)
        Me.LblCedulaBusqueda.TabIndex = 40
        Me.LblCedulaBusqueda.Text = "Cédula:"
        '
        'StatusText
        '
        Me.StatusText.BackColor = System.Drawing.SystemColors.Window
        Me.StatusText.Location = New System.Drawing.Point(18, 21)
        Me.StatusText.Multiline = True
        Me.StatusText.Name = "StatusText"
        Me.StatusText.ReadOnly = True
        Me.StatusText.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.StatusText.Size = New System.Drawing.Size(71, 19)
        Me.StatusText.TabIndex = 23
        Me.StatusText.Visible = False
        '
        'Prompt
        '
        Me.Prompt.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Prompt.Location = New System.Drawing.Point(106, 16)
        Me.Prompt.Name = "Prompt"
        Me.Prompt.ReadOnly = True
        Me.Prompt.Size = New System.Drawing.Size(200, 32)
        Me.Prompt.TabIndex = 24
        Me.Prompt.Visible = False
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.Transparent
        Me.PictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PictureBox1.Image = Global.SCSC.My.Resources.Resources.Users
        Me.PictureBox1.Location = New System.Drawing.Point(1068, 4)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(70, 70)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.PictureBox1.TabIndex = 44
        Me.PictureBox1.TabStop = False
        '
        'FrmEstudiantes
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(10.0!, 24.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(244, Byte), Integer), CType(CType(250, Byte), Integer))
        Me.BackgroundImage = Nothing
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.ClientSize = New System.Drawing.Size(1170, 720)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.Prompt)
        Me.Controls.Add(Me.StatusText)
        Me.Controls.Add(Me.GroupBusquedaEstudiante)
        Me.Controls.Add(Me.GroupDatosEstudiante)
        Me.Controls.Add(Me.LblTituloModulo)
        Me.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = True
        Me.MinimizeBox = False
        Me.Name = "FrmEstudiantes"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Mantenimiento de Estudiantes"
        Me.GroupDatosEstudiante.ResumeLayout(False)
        Me.GroupDatosEstudiante.PerformLayout()
        Me.PanelAcciones.ResumeLayout(False)
        Me.GroupBusquedaEstudiante.ResumeLayout(False)
        Me.GroupBusquedaEstudiante.PerformLayout()
        CType(Me.Picture, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents GroupDatosEstudiante As System.Windows.Forms.GroupBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents PanelAcciones As System.Windows.Forms.Panel
    Friend WithEvents BtnRegresar As System.Windows.Forms.Button
    Friend WithEvents BtnCancelar As System.Windows.Forms.Button
    Friend WithEvents BtnGuardar As System.Windows.Forms.Button
    Friend WithEvents LblTituloModulo As System.Windows.Forms.Label
    Friend WithEvents GroupBusquedaEstudiante As System.Windows.Forms.GroupBox
    Friend WithEvents TxtCedula As System.Windows.Forms.TextBox
    Friend WithEvents Buscar As System.Windows.Forms.Button
    Friend WithEvents LblCedulaBusqueda As System.Windows.Forms.Label
    Friend WithEvents TxtApe1 As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents TxtApe2 As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents TxtNombre As System.Windows.Forms.TextBox
    Friend WithEvents LblNombre As System.Windows.Forms.Label
    Friend WithEvents LblCantTiques As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents TxtTipoUsuario As System.Windows.Forms.TextBox
    Private WithEvents Picture As System.Windows.Forms.PictureBox
    Private WithEvents StatusText As System.Windows.Forms.TextBox
    Private WithEvents Prompt As System.Windows.Forms.TextBox
    Friend WithEvents TxtFecNac As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Private WithEvents StatusLine As System.Windows.Forms.Label
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents Label9 As Label
    Friend WithEvents Label11 As Label
    Friend WithEvents LblRuta As Label
    Friend WithEvents CBGenero As ComboBox
    Friend WithEvents Label13 As Label
    Friend WithEvents TxtSeccion As TextBox
    Friend WithEvents Label10 As Label
    Friend WithEvents TxtTelefono As TextBox
    Friend WithEvents Label12 As Label
    Friend WithEvents CBRuta As ComboBox
    Friend WithEvents CBBeca As ComboBox
    Friend WithEvents Label14 As Label
    Friend WithEvents Label16 As Label
    Friend WithEvents CBPermisoSalida As ComboBox
    Friend WithEvents Label15 As Label
    Friend WithEvents CBRutaPendiente As CheckBox
End Class
